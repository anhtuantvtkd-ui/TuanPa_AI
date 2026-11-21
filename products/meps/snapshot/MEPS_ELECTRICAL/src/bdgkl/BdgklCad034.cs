using System;using System.Linq;using System.Collections.Generic;using System.Globalization;using System.Text;using System.Text.RegularExpressions;using System.Web.Script.Serialization;using IOE=System.InvalidOperationException;
#if BDGKL034
#if !ROUTE034 || ROUTE034_PURE || !TAG033 || TAG033_PURE
#error BDGKL034 needs the native ROUTE034 part (RouteCad034.Snapshot) and the native TAG033 part (TagCad033.Read).
#endif
using Autodesk.AutoCAD.DatabaseServices;using Autodesk.AutoCAD.Geometry;
namespace Tvxd.Meps {
 // TVXD-MEPS-WIREQTY-034 BDGKL NATIVE PILOT (UI: BdgklUI034.cs; formulas: the pure core Bdgkl034.cs, unchanged). One route = one Model LINE/open planar
 // LWPOLYLINE between two DIFFERENT top-level Model block references the user picked, on the same floor; everything else is declared by the user.
 //  - Own record: Xrecord key Key on the ROUTE entity's extension dictionary = [Schema, Version, chunk count, JSON chunks (each 1..Chunk chars)]. The JSON
 //    (JavaScriptSerializer) is a flat object of strings/nulls in the fixed order Fields, type "BDGKL_ROUTE"; numbers are invariant "R" text. A record is read
 //    only when it is EXACTLY canonical (re-encoding gives the same values); unknown schema/version/type, malformed or non-Xrecord = INVALID, never repaired
 //    or overwritten. Route034 / Cable016 / Identity015 / Tag033 records are never written or migrated; they may coexist on the same entity.
 //  - Stored: declared config + identity stamps only (own route GUID, route handle, FingerprintGuid, INSUNITS, route geometry hash from RouteCad034.Snapshot,
 //    each endpoint's handle, block name, display label, position and SHA-256 signature). NEVER a length or quantity: every read measures the live route and
 //    recalculates with Bdgkl034.Calculate / Aggregate.
 //  - Endpoint signature: handle, Model owner, block record handle/name (dynamic: stable definition name too), position, full BlockTransform, normal, the
 //    block definition signature (BN01: complete geometry of supported primitives, nested definitions recursively; unsupported entity/budget/depth/cycle
 //    = unresolved: refused by Snapshot, CHECK on a saved row; see SignDef) and the TAG033 label state (handle + text, NONE or INVALID+reason). MOVE/ROTATE/SCALE/rename/definition edit/TAG edit/deletion → the row is
 //    RECONFIRM_REQUIRED and excluded until the user saves it again. Labels are display text only (TAG033 text, else block name + handle); never identity,
 //    never a device type or circuit.
 //  - COPY of a configured route: the copy carries the record but its handle differs → RECONFIRM_REQUIRED; saving it again gives it a NEW route GUID.
 //  - Pilot limits: shared conduit is NOT supported: the user must tick 'Không dùng chung ống' (U = V = 0); a stored non-zero U/V is CHECK (outside pilot) and
 //    is refused for overwrite. No region, no XLSX, no floor-type defaults, no cable→assembly mapping, no nearest-device association.
 //  - Apply: ONE transaction (one native UNDO step of the caller's command), exact snapshot recheck (route signature, endpoint signatures, the exact BDGKL raw
 //    values), readback, guard right before Commit. No Save, no layer/colour/geometry/INSUNITS/TAG/library change. QA: FailAt = "after-write".
 public sealed class BdgklConfig034 {
  public string floorId,areaId,circuitId,declaredUnit,assemblyCode,conduitCode,routingMode,startHeightMode,endHeightMode;
  public double? ceilingHeight,startFixedHeight,startCeilingOffset,endFixedHeight,endCeilingOffset,tailEachLength,factor;public int? tailCount;
  // Explicit 'Không dùng chung ống' (false until the user ticks it). false = U/V not entered → MISSING_DATA (no shared-conduit entry in this pilot).
  public bool noSharedConduit;
  public BdgklConfig034 Copy(){return (BdgklConfig034)MemberwiseClone();}
 }
 // Live state of one endpoint block. defResolved false = definition too large to sign (warning; no live-geometry proof of the definition).
 // BN01: defResolved false (defIssue = why) = the definition holds an unsupported entity or exceeds the budget/depth: Snapshot refuses it, a saved row is CHECK.
 public sealed class BdgklEndpoint034 {public string handle,blockName,label,position,tagText,tagState,defSignature,defIssue,signature;public bool defResolved,dynamic;public ObjectId id;}
 // Decoded stored record (identity stamps + declared config). sharedHorizontal/sharedVertical are the stored U/V (pilot writes only 0).
 public sealed class BdgklRecord034 {public string routeGuid,routeHandle,fingerprint,geometryHash;public int insunits;
  public string startHandle,startBlock,startLabel,startPosition,startSignature,endHandle,endBlock,endLabel,endPosition,endSignature;
  public BdgklConfig034 config;public double sharedHorizontal,sharedVertical;
  public bool OutsidePilot{get{return sharedHorizontal!=0||sharedVertical!=0;}}}
 // Exact context for one save: the route (RouteCad034.Snapshot item incl. signature), both endpoints, the exact BDGKL raw values of the route (null = none).
 // existingError non-null = the stored record is unreadable/outside the pilot: Apply refuses (never overwritten). routeGuid = GUID a save will write.
 public sealed class BdgklSnapshot034 {public ObjectId routeId;public RouteItem034 route;public BdgklEndpoint034 start,end;public int insunits;public string fingerprint,routeGuid;
  public string[] existingRaw;public BdgklRecord034 existing;public string existingError,existingStatus,existingReason;
  public double Length{get{return route.length;}}
  public string UnitWarning(string unit){return unit==null?null:Route034.UnitWarning(insunits,unit);}}
 // One configured route of a scan/read. status: OK (counted) | MISSING_DATA | CHECK (core) | RECONFIRM_REQUIRED (stamps) | INVALID (record unreadable).
 // result = Bdgkl034.Calculate with the LIVE length (informational unless OK); input/result null for INVALID.
 public sealed class BdgklRow034 {public ObjectId routeId,startId,endId;public string routeHandle,routeGuid,status,reason,warning,startLabel,endLabel;public double? liveLength;
  public BdgklRecord034 record;public BdgklInput034 input;public BdgklResult034 result;
  public bool Countable{get{return status==BdgklCad034.Ok;}}}
 public sealed class BdgklScan034 {public readonly List<BdgklRow034> rows=new List<BdgklRow034>();public BdgklBatch034 batch;public int insunits;public string fingerprint;
  public int Count(string status){return rows.Count(r=>r.status==status);}
  public int Excluded{get{return rows.Count(r=>r.status!=BdgklCad034.Ok);}}
  public string Summary(){return "Toàn Model — pilot cùng tầng: "+rows.Count+" tuyến BDGKL, "+Count(BdgklCad034.Ok)+" được tính, "+Excluded+" KHÔNG tính ("+
   Count(Bdgkl034.MissingData)+" thiếu dữ liệu, "+Count(Bdgkl034.Check)+" cần kiểm tra, "+Count(BdgklCad034.Reconfirm)+" cần xác nhận lại, "+Count(BdgklCad034.Invalid)+" bản ghi không đọc được).";}}
 public sealed class BdgklApply034 {public bool changed,warn;public string message,routeGuid;public BdgklResult034 result;public string[] written;}
 public static class BdgklCad034 {
  public const string Key="TVXD_MEPS_BDGKL_034",Schema="TVXD_MEPS_BDGKL",Version="034.1",Type="BDGKL_ROUTE",SharedNone="NONE";
  public const string Ok=Bdgkl034.Ok,Reconfirm="RECONFIRM_REQUIRED",Invalid="INVALID";
  public const int Chunk=240,MaxPayload=16000,MaxDefEntities=5000;
  public const string Scope="Toàn Model — pilot cùng tầng";
  // QA failure injection: "after-write" throws after the record is written, before readback/Commit (the transaction aborts). null in production.
  public static string FailAt;
  static void Fail(string stage){if(FailAt!=null&&string.Equals(FailAt,stage,StringComparison.Ordinal))throw new IOE("QA: lỗi giả lập tại "+stage+".");}
  static readonly CultureInfo Inv=CultureInfo.InvariantCulture;
  static readonly Regex HandleRx=new Regex(@"\A[0-9A-Fa-f]{1,16}\z"),HashRx=new Regex(@"\A[0-9A-F]{64}\z"),GuidRx=new Regex(@"\A[0-9a-f]{32}\z");
  // Fixed JSON order (after "type"). Every value is a string or null.
  static readonly string[] Fields={"routeGuid","routeHandle","fingerprint","insunits","geometryHash",
   "startHandle","startBlock","startLabel","startPosition","startSignature","endHandle","endBlock","endLabel","endPosition","endSignature",
   "floorId","areaId","circuitId","declaredUnit","assemblyCode","conduitCode","routingMode","ceilingHeight",
   "startHeightMode","startFixedHeight","startCeilingOffset","endHeightMode","endFixedHeight","endCeilingOffset","tailCount","tailEachLength","factor",
   "sharedMode","sharedHorizontal","sharedVertical"};
  static readonly HashSet<string> Nullable=new HashSet<string>(StringComparer.Ordinal){"startFixedHeight","startCeilingOffset","endFixedHeight","endCeilingOffset"};
  static string N(double v){return v.ToString("R",Inv);}
  static string N(double? v){return v.HasValue?N(v.Value):null;}
  static string P(Point3d p){return N(p.X)+","+N(p.Y)+","+N(p.Z);}
  static string Short(string s){return s==null?"null":s.Length>40?s.Substring(0,40)+"…":s;}
  static IOE Bad(string why){return new IOE("Bản ghi "+Key+" không hợp lệ ("+why+"); MEPS không sửa, không ghi đè.");}

  // ---- record codec (pure .NET; public for the native harness) ----
  public static string[] Encode(BdgklRecord034 r){if(r==null||r.config==null)throw new IOE("Thiếu bản ghi BDGKL.");var c=r.config;
   var o=new Dictionary<string,object>();o["type"]=Type;
   string[] vals={r.routeGuid,r.routeHandle,r.fingerprint,r.insunits.ToString(Inv),r.geometryHash,
    r.startHandle,r.startBlock,r.startLabel,r.startPosition,r.startSignature,r.endHandle,r.endBlock,r.endLabel,r.endPosition,r.endSignature,
    c.floorId,c.areaId,c.circuitId,c.declaredUnit,c.assemblyCode,c.conduitCode,c.routingMode,N(c.ceilingHeight),
    c.startHeightMode,N(c.startFixedHeight),N(c.startCeilingOffset),c.endHeightMode,N(c.endFixedHeight),N(c.endCeilingOffset),
    c.tailCount.HasValue?c.tailCount.Value.ToString(Inv):null,N(c.tailEachLength),N(c.factor),SharedNone,N(r.sharedHorizontal),N(r.sharedVertical)};
   for(int i=0;i<Fields.Length;i++){if(vals[i]==null&&!Nullable.Contains(Fields[i]))throw new IOE("Bản ghi BDGKL thiếu "+Fields[i]+"; không ghi.");o[Fields[i]]=vals[i];}
   CheckStamps(r);
   string json=new JavaScriptSerializer().Serialize(o);
   if(json.Length>MaxPayload)throw new IOE("Bản ghi BDGKL dài "+json.Length+" ký tự (tối đa "+MaxPayload+"); không ghi.");
   var parts=new List<string>();
   for(int i=0;i<json.Length;){int n=Math.Min(Chunk,json.Length-i);if(n<json.Length-i&&char.IsHighSurrogate(json[i+n-1]))n--;parts.Add(json.Substring(i,n));i+=n;}
   var res=new List<string>{Schema,Version,parts.Count.ToString(Inv)};res.AddRange(parts);return res.ToArray();}
  static void CheckStamps(BdgklRecord034 r){Guid g;
   if(r.routeGuid==null||!GuidRx.IsMatch(r.routeGuid))throw Bad("GUID tuyến");
   if(r.routeHandle==null||!HandleRx.IsMatch(r.routeHandle)||r.startHandle==null||!HandleRx.IsMatch(r.startHandle)||r.endHandle==null||!HandleRx.IsMatch(r.endHandle))throw Bad("handle");
   if(r.fingerprint==null||r.fingerprint.Length>64||!Guid.TryParse(r.fingerprint,out g))throw Bad("FingerprintGuid");
   if(r.insunits<0)throw Bad("INSUNITS");
   if(r.geometryHash==null||!HashRx.IsMatch(r.geometryHash)||r.startSignature==null||!HashRx.IsMatch(r.startSignature)||r.endSignature==null||!HashRx.IsMatch(r.endSignature))throw Bad("dấu hình học/điểm đầu cuối");
   if(string.Equals(r.startHandle,r.endHandle,StringComparison.Ordinal))throw Bad("điểm đầu trùng điểm cuối");
   double[] nums={r.sharedHorizontal,r.sharedVertical};foreach(var x in nums)if(double.IsNaN(x)||double.IsInfinity(x)||x<0)throw Bad("U/V");}
  static double? Dbl(string s,string f){if(s==null)return null;double v;
   if(!double.TryParse(s,NumberStyles.Float,Inv,out v)||double.IsNaN(v)||double.IsInfinity(v))throw Bad("số "+f+" = "+Short(s));return v;}
  // Strict read. Unknown schema/version/type, wrong count/kind, non-canonical text = exception (the caller reports INVALID and never overwrites).
  public static BdgklRecord034 Decode(string[] v){
   if(v==null||v.Length<4||v.Any(x=>x==null))throw Bad("số trường hoặc kiểu dữ liệu sai");
   if(v[0]!=Schema)throw Bad("schema lạ "+Short(v[0]));if(v[1]!=Version)throw Bad("phiên bản "+Short(v[1])+" chưa hỗ trợ; bản này đọc "+Version);
   int count;if(!int.TryParse(v[2],NumberStyles.None,Inv,out count)||count<1||v.Length!=3+count||count.ToString(Inv)!=v[2])throw Bad("số đoạn dữ liệu");
   var sb=new StringBuilder();for(int i=3;i<v.Length;i++){if(v[i].Length==0||v[i].Length>Chunk)throw Bad("độ dài đoạn dữ liệu");sb.Append(v[i]);}
   if(sb.Length>MaxPayload)throw Bad("dữ liệu quá dài");
   Dictionary<string,object> o;try{o=new JavaScriptSerializer().DeserializeObject(sb.ToString()) as Dictionary<string,object>;}catch(Exception ex){throw Bad("JSON: "+ex.Message);}
   if(o==null||o.Count!=Fields.Length+1)throw Bad("số khoá JSON");
   object t;if(!o.TryGetValue("type",out t)||!(t is string)||(string)t!=Type)throw Bad("loại bản ghi");
   var s=new Dictionary<string,string>(StringComparer.Ordinal);
   foreach(var f in Fields){object x;if(!o.TryGetValue(f,out x))throw Bad("thiếu khoá "+f);
    if(x==null){if(!Nullable.Contains(f))throw Bad("khoá "+f+" rỗng");s[f]=null;}else if(x is string)s[f]=(string)x;else throw Bad("khoá "+f+" không phải chữ");}
   if(s["sharedMode"]!=SharedNone)throw Bad("cách trừ trùng ống "+Short(s["sharedMode"])+" chưa hỗ trợ");
   int iu,tc;
   if(!int.TryParse(s["insunits"],NumberStyles.None,Inv,out iu))throw Bad("INSUNITS");
   if(!int.TryParse(s["tailCount"],NumberStyles.None,Inv,out tc))throw Bad("số đầu chờ");
   var c=new BdgklConfig034{floorId=s["floorId"],areaId=s["areaId"],circuitId=s["circuitId"],declaredUnit=s["declaredUnit"],assemblyCode=s["assemblyCode"],conduitCode=s["conduitCode"],
    routingMode=s["routingMode"],ceilingHeight=Dbl(s["ceilingHeight"],"ceilingHeight"),startHeightMode=s["startHeightMode"],startFixedHeight=Dbl(s["startFixedHeight"],"startFixedHeight"),
    startCeilingOffset=Dbl(s["startCeilingOffset"],"startCeilingOffset"),endHeightMode=s["endHeightMode"],endFixedHeight=Dbl(s["endFixedHeight"],"endFixedHeight"),
    endCeilingOffset=Dbl(s["endCeilingOffset"],"endCeilingOffset"),tailCount=tc,tailEachLength=Dbl(s["tailEachLength"],"tailEachLength"),factor=Dbl(s["factor"],"factor")};
   double? u=Dbl(s["sharedHorizontal"],"sharedHorizontal"),w=Dbl(s["sharedVertical"],"sharedVertical");
   c.noSharedConduit=u.Value==0&&w.Value==0;
   var r=new BdgklRecord034{routeGuid=s["routeGuid"],routeHandle=s["routeHandle"],fingerprint=s["fingerprint"],insunits=iu,geometryHash=s["geometryHash"],
    startHandle=s["startHandle"],startBlock=s["startBlock"],startLabel=s["startLabel"],startPosition=s["startPosition"],startSignature=s["startSignature"],
    endHandle=s["endHandle"],endBlock=s["endBlock"],endLabel=s["endLabel"],endPosition=s["endPosition"],endSignature=s["endSignature"],config=c,sharedHorizontal=u.Value,sharedVertical=w.Value};
   string[] again;try{again=Encode(r);}catch(IOE ex){throw Bad(ex.Message);}
   if(!again.SequenceEqual(v,StringComparer.Ordinal))throw Bad("giá trị không ở dạng chuẩn");
   return r;}
  // Core input of a stored/declared config on a live length (null = not measurable now). U = V = 0 only after the explicit 'Không dùng chung ống'.
  public static BdgklInput034 Input(string routeKey,BdgklConfig034 c,string startKey,string endKey,string startLabel,string endLabel,double? nativeLength){
   if(c==null)throw new IOE("Thiếu thông số BDGKL.");
   return new BdgklInput034{routeKey=routeKey,floorId=c.floorId,areaId=c.areaId,circuitId=c.circuitId,startKey=startKey,endKey=endKey,startLabel=startLabel,endLabel=endLabel,
    assemblyCode=c.assemblyCode,conduitCode=c.conduitCode,nativeLength=nativeLength,declaredUnit=c.declaredUnit,routingMode=c.routingMode,ceilingHeight=c.ceilingHeight,
    floorFinishedElevation=null,startHeightMode=c.startHeightMode,startFixedHeight=c.startFixedHeight,startCeilingOffset=c.startCeilingOffset,endHeightMode=c.endHeightMode,
    endFixedHeight=c.endFixedHeight,endCeilingOffset=c.endCeilingOffset,tailCount=c.tailCount,tailEachLength=c.tailEachLength,
    sharedHorizontal=c.noSharedConduit?(double?)0.0:null,sharedVertical=c.noSharedConduit?(double?)0.0:null,sharedOwnerKey=null,factor=c.factor};}

  // ---- AutoCAD reads ----
  static string[] Values(Xrecord x){using(var d=x.Data){return d==null?new string[0]:d.AsArray().Select(t=>t.TypeCode==(short)DxfCode.Text?Convert.ToString(t.Value,Inv):null).ToArray();}}
  // Raw values under Key; null = no key; non-Xrecord entry = exception.
  static string[] ReadRaw(Transaction tr,DBObject o){if(o.ExtensionDictionary.IsNull)return null;var d=(DBDictionary)tr.GetObject(o.ExtensionDictionary,OpenMode.ForRead);
   if(!d.Contains(Key))return null;var x=tr.GetObject(d.GetAt(Key),OpenMode.ForRead) as Xrecord;if(x==null)throw new IOE("khoá "+Key+" của "+o.Handle+" không phải Xrecord; MEPS không ghi đè");return Values(x);}
  static bool SameRaw(string[] a,string[] b){if(a==null||b==null)return a==null&&b==null;return a.SequenceEqual(b,StringComparer.Ordinal);}
  static ObjectId FromHandle(Database db,string h){long v;
   if(h==null||!HandleRx.IsMatch(h)||!long.TryParse(h,NumberStyles.AllowHexSpecifier,Inv,out v)||v<=0)return ObjectId.Null;
   try{return db.GetObjectId(false,new Handle(v),0);}catch(Autodesk.AutoCAD.Runtime.Exception){return ObjectId.Null;}}
  // ---- BN01 block definition signature: complete geometry values of supported primitives ----
  // Supported: Line; LWPOLYLINE (closed, elevation, normal, thickness, every vertex x/y/bulge/start+end width); Circle; Arc; Ellipse; Point; DBText and
  // AttributeDefinition (text, tag/prompt/flags, position, alignment point, height, rotation, width factor, oblique, modes, mirror, style handle, normal);
  // MText (contents, location, height, width, rotation, attachment, direction, line spacing, style handle, normal); Solid (4 corners, normal, thickness);
  // Hatch (pattern, style, elevation, normal, loops of bulge polylines or line/circular-arc edges); nested BlockReference (record handle, full transform,
  // attributes, and its definition recursively). Everything else (2D/3D polyline, spline, region, solid3d, proxy, custom, MINSERT, xref…) is UNSUPPORTED:
  // no completeness is claimed. Limits: MaxDefEntities entities in total, MaxDefDepth nesting levels, cycles refused. Values are invariant "R" text and
  // handles, so the signature is stable across save/reopen. Display-only properties (layer, colour, linetype) are not geometry and are not signed.
  public const int MaxDefDepth=8;
  sealed class DefSig {public readonly StringBuilder sb=new StringBuilder();public int budget=MaxDefEntities;public bool overBudget;public readonly List<string> unsupported=new List<string>();}
  static string P2(Point2d p){return N(p.X)+","+N(p.Y);}
  static string V3(Vector3d v){return N(v.X)+","+N(v.Y)+","+N(v.Z);}
  static void SignDef(Transaction tr,BlockTableRecord btr,DefSig s,int depth,HashSet<ObjectId> path){
   if(depth>MaxDefDepth){s.unsupported.Add("block lồng quá "+MaxDefDepth+" cấp");return;}
   if(!path.Add(btr.ObjectId)){s.unsupported.Add("block lồng vòng "+btr.Name);return;}
   s.sb.Append("{BTR:").Append(btr.Handle).Append(':').Append(btr.Name.Length).Append(':').Append(btr.Name).Append('@').Append(P(btr.Origin));
   foreach(ObjectId eid in btr){if(eid.IsErased)continue;if(--s.budget<0){s.overBudget=true;break;}
    var e=tr.GetObject(eid,OpenMode.ForRead) as Entity;if(e==null)continue;
    s.sb.Append('|').Append(e.Handle).Append(':').Append(e.Visible?'V':'H');
    bool ok;try{ok=Prim(tr,e,s,depth,path);}catch(Autodesk.AutoCAD.Runtime.Exception){ok=false;}
    if(!ok){s.unsupported.Add(e.GetType().Name);s.sb.Append(":UNSUPPORTED:").Append(e.GetType().Name);}}
   s.sb.Append('}');path.Remove(btr.ObjectId);}
  static void Text(StringBuilder b,string t){b.Append((t??"").Length).Append(':').Append(t??"");}
  static bool Prim(Transaction tr,Entity e,DefSig s,int depth,HashSet<ObjectId> path){var b=s.sb;
   var ln=e as Line;if(ln!=null){b.Append(":L:").Append(P(ln.StartPoint)).Append(';').Append(P(ln.EndPoint)).Append(';').Append(V3(ln.Normal)).Append(';').Append(N(ln.Thickness));return true;}
   var pl=e as Polyline;if(pl!=null){b.Append(":PL:").Append(pl.Closed?1:0).Append(';').Append(N(pl.Elevation)).Append(';').Append(V3(pl.Normal)).Append(';').Append(N(pl.Thickness)).Append(';').Append(pl.NumberOfVertices);
    for(int i=0;i<pl.NumberOfVertices;i++)b.Append(";v").Append(P2(pl.GetPoint2dAt(i))).Append(",b").Append(N(pl.GetBulgeAt(i))).Append(",w").Append(N(pl.GetStartWidthAt(i))).Append(',').Append(N(pl.GetEndWidthAt(i)));
    return true;}
   var arc=e as Arc;if(arc!=null){b.Append(":A:").Append(P(arc.Center)).Append(';').Append(N(arc.Radius)).Append(';').Append(N(arc.StartAngle)).Append(';').Append(N(arc.EndAngle)).Append(';').Append(V3(arc.Normal)).Append(';').Append(N(arc.Thickness));return true;}
   var ci=e as Circle;if(ci!=null){b.Append(":C:").Append(P(ci.Center)).Append(';').Append(N(ci.Radius)).Append(';').Append(V3(ci.Normal)).Append(';').Append(N(ci.Thickness));return true;}
   var el=e as Ellipse;if(el!=null){b.Append(":E:").Append(P(el.Center)).Append(';').Append(V3(el.MajorAxis)).Append(';').Append(N(el.RadiusRatio)).Append(';').Append(N(el.StartParam)).Append(';').Append(N(el.EndParam)).Append(';').Append(V3(el.Normal));return true;}
   var pt=e as DBPoint;if(pt!=null){b.Append(":PT:").Append(P(pt.Position)).Append(';').Append(V3(pt.Normal)).Append(';').Append(N(pt.Thickness));return true;}
   var tx=e as DBText;if(tx!=null){var ad=tx as AttributeDefinition;
    if(ad!=null){b.Append(":AD:");Text(b,ad.Tag);b.Append(';');Text(b,ad.Prompt);b.Append(';').Append(ad.Constant?1:0).Append(ad.Invisible?1:0).Append(ad.Verifiable?1:0).Append(ad.Preset?1:0).Append(ad.IsMTextAttributeDefinition?1:0);}
    b.Append(":T:");Text(b,tx.TextString);b.Append(';').Append(P(tx.Position)).Append(';').Append(P(tx.AlignmentPoint)).Append(';').Append(N(tx.Height)).Append(';').Append(N(tx.Rotation)).Append(';')
     .Append(N(tx.WidthFactor)).Append(';').Append(N(tx.Oblique)).Append(';').Append((int)tx.HorizontalMode).Append(',').Append((int)tx.VerticalMode).Append(';').Append(tx.IsMirroredInX?1:0).Append(tx.IsMirroredInY?1:0)
     .Append(';').Append(tx.TextStyleId.Handle).Append(';').Append(V3(tx.Normal)).Append(';').Append(N(tx.Thickness));return true;}
   var mt=e as MText;if(mt!=null){b.Append(":MT:");Text(b,mt.Contents);b.Append(';').Append(P(mt.Location)).Append(';').Append(N(mt.TextHeight)).Append(';').Append(N(mt.Width)).Append(';').Append(N(mt.Rotation))
     .Append(';').Append((int)mt.Attachment).Append(';').Append(V3(mt.Direction)).Append(';').Append(N(mt.LineSpacingFactor)).Append(';').Append(mt.TextStyleId.Handle).Append(';').Append(V3(mt.Normal));return true;}
   var so=e as Solid;if(so!=null){b.Append(":S:");for(short i=0;i<4;i++)b.Append(P(so.GetPointAt(i))).Append(';');b.Append(V3(so.Normal)).Append(';').Append(N(so.Thickness));return true;}
   var ha=e as Hatch;if(ha!=null){b.Append(":H:").Append((int)ha.PatternType).Append(';');Text(b,ha.PatternName);b.Append(';').Append(N(ha.PatternScale)).Append(';').Append(N(ha.PatternAngle)).Append(';')
     .Append((int)ha.HatchStyle).Append(';').Append(N(ha.Elevation)).Append(';').Append(V3(ha.Normal)).Append(';').Append(ha.IsGradient?1:0).Append(';').Append(ha.NumberOfLoops);
    for(int i=0;i<ha.NumberOfLoops;i++){var lp=ha.GetLoopAt(i);b.Append(";loop").Append((int)lp.LoopType);
     if(lp.IsPolyline){foreach(BulgeVertex bv in lp.Polyline)b.Append(",v").Append(P2(bv.Vertex)).Append(",b").Append(N(bv.Bulge));}
     else foreach(Curve2d c in lp.Curves){var ls=c as LineSegment2d;var ca=c as CircularArc2d;
      if(ls!=null)b.Append(",l").Append(P2(ls.StartPoint)).Append(';').Append(P2(ls.EndPoint));
      else if(ca!=null)b.Append(",a").Append(P2(ca.Center)).Append(';').Append(N(ca.Radius)).Append(';').Append(N(ca.StartAngle)).Append(';').Append(N(ca.EndAngle)).Append(';').Append(ca.IsClockWise?1:0);
      else return false;}}
    return true;}
   var nb=e as BlockReference;if(nb!=null&&!(nb is MInsertBlock)){var nr=(BlockTableRecord)tr.GetObject(nb.BlockTableRecord,OpenMode.ForRead);
    if(nr.IsFromExternalReference||nr.IsFromOverlayReference||nr.IsDependent||nr.IsLayout)return false;
    b.Append(":BR:").Append(nr.Handle).Append(';').Append(string.Join(",",nb.BlockTransform.ToArray().Select(q=>N(q))));
    foreach(ObjectId aid in nb.AttributeCollection){if(aid.IsErased)continue;var ar=tr.GetObject(aid,OpenMode.ForRead) as AttributeReference;if(ar==null)continue;
     b.Append(";att");Text(b,ar.Tag);b.Append(',');Text(b,ar.TextString);b.Append(',').Append(P(ar.Position)).Append(',').Append(N(ar.Height)).Append(',').Append(N(ar.Rotation)).Append(',').Append(ar.Invisible?1:0);}
    SignDef(tr,nr,s,depth+1,path);return true;}
   return false;}
  // One endpoint block → live state + signature, or exception naming why it cannot be used. Read only (TagCad033.Read opens its own read transaction).
  static BdgklEndpoint034 Endpoint(Transaction tr,Database db,ObjectId id,string what){
   if(id.IsNull||id.IsErased||id.Database!=db)throw new IOE(what+": block đã bị xoá hoặc không thuộc bản vẽ hiện tại");
   var br=tr.GetObject(id,OpenMode.ForRead) as BlockReference;string h=what+" "+id.Handle+": ";
   if(br==null)throw new IOE(h+"không phải block (BlockReference)");
   if(br is MInsertBlock)throw new IOE(h+"MINSERT chưa hỗ trợ");
   if(br.OwnerId!=SymbolUtilityServices.GetBlockModelSpaceId(db))throw new IOE(h+"không nằm trực tiếp trong Model (Layout, bên trong block hoặc xref)");
   var btr=(BlockTableRecord)tr.GetObject(br.BlockTableRecord,OpenMode.ForRead);
   if(btr.IsFromExternalReference||btr.IsFromOverlayReference||btr.IsDependent)throw new IOE(h+"là xref/thuộc xref; chưa hỗ trợ");
   if(btr.IsLayout)throw new IOE(h+"là bản ghi Layout; chưa hỗ trợ");
   bool dyn=br.IsDynamicBlock;BlockTableRecord eff=btr;
   if(dyn&&!br.DynamicBlockTableRecord.IsNull)eff=(BlockTableRecord)tr.GetObject(br.DynamicBlockTableRecord,OpenMode.ForRead);
   if(btr.IsAnonymous&&!dyn)throw new IOE(h+"là block vô danh "+btr.Name+" (ARRAY/nhóm…); chưa hỗ trợ");
   // BN01: definition signature from the COMPLETE geometry values of the record actually drawn (anonymous record of a dynamic block included), nested
   // block definitions recursively. Any unsupported entity / budget / depth / cycle = unresolved (defIssue says why); the signature stays deterministic.
   var ds=new DefSig();SignDef(tr,btr,ds,0,new HashSet<ObjectId>());
   bool resolved=!ds.overBudget&&ds.unsupported.Count==0;string defSig=Route034.Hash(ds.sb.ToString());
   string defIssue=resolved?null:"định nghĩa block "+btr.Name+" có "+(ds.overBudget?"hơn "+MaxDefEntities+" đối tượng":"")+(ds.overBudget&&ds.unsupported.Count>0?" và ":"")+
    (ds.unsupported.Count>0?"đối tượng chưa hỗ trợ kiểm hình học ("+string.Join(", ",ds.unsupported.Distinct(StringComparer.Ordinal).Take(5))+(ds.unsupported.Count>5?", …":"")+")":"")+
    ": MEPS không xác minh được thay đổi bên trong block";
   string tagText=null,tagState;
   try{var t=TagCad033.Read(db,id);if(t==null)tagState="NONE";else{tagText=t.text;tagState="TAG:"+t.tagHandle+":"+(t.text??"").Length+":"+t.text;}}
   catch(IOE ex){tagState="INVALID:"+ex.Message;}
   string name=eff.Name,handle=br.Handle.ToString();
   string label=tagText!=null&&Bdgkl034.TextProblem(tagText)==null?tagText:name+" #"+handle;
   if(label.Length>Bdgkl034.MaxText)label=name.Substring(0,Math.Max(1,Bdgkl034.MaxText-handle.Length-3))+"… #"+handle;
   if(Bdgkl034.TextProblem(label)!=null)label="Block #"+handle;
   var m=br.BlockTransform.ToArray();
   var sig=new StringBuilder().Append(handle).Append("|O:").Append(br.OwnerId.Handle).Append("|B:").Append(btr.Handle).Append(':').Append(btr.Name.Length).Append(':').Append(btr.Name)
    .Append("|E:").Append(eff.Handle).Append(':').Append(name.Length).Append(':').Append(name).Append("|D:").Append(dyn?1:0).Append("|P:").Append(P(br.Position))
    .Append("|M:").Append(string.Join(",",m.Select(q=>N(q)))).Append("|N:").Append(N(br.Normal.X)).Append(',').Append(N(br.Normal.Y)).Append(',').Append(N(br.Normal.Z))
    .Append("|DEF:").Append(defSig).Append("|TAG:").Append(tagState.Length).Append(':').Append(tagState);
   return new BdgklEndpoint034{id=id,handle=handle,blockName=name,label=label,position=P(br.Position),tagText=tagText,tagState=tagState,defSignature=defSig,defResolved=resolved,defIssue=defIssue,dynamic=dyn,
    signature=Route034.Hash(sig.ToString())};}
  // Why a stored record no longer matches this route (empty = still valid). geometryHash null = route not measurable now (reason added by the caller).
  static List<string> RouteIssues(BdgklRecord034 r,Database db,string handle,string geometryHash){var l=new List<string>();
   if(!string.Equals(r.routeHandle,handle,StringComparison.Ordinal))l.Add("handle lúc lưu "+r.routeHandle+" ≠ "+handle+" (bản sao COPY/dán/chèn)");
   if(!string.Equals(r.fingerprint,db.FingerprintGuid,StringComparison.OrdinalIgnoreCase))l.Add("FingerprintGuid khác bản vẽ lúc lưu (WBLOCK/chép sang bản vẽ khác)");
   if(r.insunits!=(int)db.Insunits)l.Add("INSUNITS lúc lưu "+r.insunits+" ≠ hiện tại "+(int)db.Insunits);
   if(geometryHash!=null&&!string.Equals(r.geometryHash,geometryHash,StringComparison.Ordinal))l.Add("hình học tuyến đã đổi sau khi lưu (MOVE/STRETCH/PEDIT/grip…)");
   return l;}
  static string Warn(BdgklEndpoint034 s,BdgklEndpoint034 e,int insunits,string unit){var w=new List<string>();
   var u=unit==null?null:Route034.UnitWarning(insunits,unit);if(u!=null)w.Add(u);
   return w.Count==0?null:string.Join("; ",w);}

  // ---- public API ----
  // Context for one save: route via RouteCad034.Snapshot (supported Model LINE/open planar LWPOLYLINE, refuses locked layer/conflicting route metadata),
  // two DIFFERENT live top-level Model block references, and the exact BDGKL raw values already on the route. Exception = why (nothing written).
  public static BdgklSnapshot034 Snapshot(Database db,ObjectId routeId,ObjectId startId,ObjectId endId){if(db==null)throw new IOE("Không có bản vẽ.");
   if(startId==endId)throw new IOE("Điểm đầu và điểm cuối là cùng một block; chọn hai thiết bị khác nhau. Bản vẽ không đổi.");
   if(startId==routeId||endId==routeId)throw new IOE("Điểm đầu/cuối phải là block, không phải chính tuyến. Bản vẽ không đổi.");
   RouteSnapshot034 rs;try{rs=RouteCad034.Snapshot(db,new[]{routeId});}catch(IOE ex){throw new IOE("Tuyến: "+ex.Message);}
   var snap=new BdgklSnapshot034{routeId=routeId,route=rs.items[0],insunits=(int)db.Insunits,fingerprint=db.FingerprintGuid};
   using(var tr=db.TransactionManager.StartOpenCloseTransaction()){
    var e=(Entity)tr.GetObject(routeId,OpenMode.ForRead);
    try{snap.existingRaw=ReadRaw(tr,e);}catch(IOE ex){snap.existingError=ex.Message;snap.existingRaw=new[]{"\u0000NOT_XRECORD"};}
    try{snap.start=Endpoint(tr,db,startId,"Điểm đầu");snap.end=Endpoint(tr,db,endId,"Điểm cuối");}catch(IOE ex){throw new IOE(ex.Message+". Bản vẽ không đổi.");}
    // BN01: a block whose definition cannot be fully signed is not accepted in the pilot (its interior edits could not be detected).
    foreach(var p in new[]{snap.start,snap.end})if(!p.defResolved)throw new IOE("Block "+p.label+" (handle "+p.handle+"): "+p.defIssue+"; pilot chưa nhận block này. Bản vẽ không đổi.");
    tr.Commit();}
   snap.existingStatus="NONE";
   if(snap.existingError==null&&snap.existingRaw!=null){
    try{snap.existing=Decode(snap.existingRaw);}catch(IOE ex){snap.existingError=ex.Message;}
    if(snap.existing!=null){
     if(snap.existing.OutsidePilot)snap.existingError="bản ghi đang có trừ trùng ống U = "+N(snap.existing.sharedHorizontal)+", V = "+N(snap.existing.sharedVertical)+" — ngoài phạm vi pilot; MEPS không ghi đè, không xoá";
     var iss=RouteIssues(snap.existing,db,snap.route.handle,snap.route.geometryHash);
     if(!string.Equals(snap.existing.startHandle,snap.start.handle,StringComparison.Ordinal)||!string.Equals(snap.existing.startSignature,snap.start.signature,StringComparison.Ordinal))iss.Add("điểm đầu khác/đã đổi so với lúc lưu");
     if(!string.Equals(snap.existing.endHandle,snap.end.handle,StringComparison.Ordinal)||!string.Equals(snap.existing.endSignature,snap.end.signature,StringComparison.Ordinal))iss.Add("điểm cuối khác/đã đổi so với lúc lưu");
     snap.existingStatus=iss.Count==0?Ok:Reconfirm;snap.existingReason=iss.Count==0?null:string.Join("; ",iss);}}
   if(snap.existingError!=null)snap.existingStatus=Invalid;
   // Keep the route's own GUID only when the stored record belongs to THIS entity in THIS drawing; a COPY/other drawing gets a new GUID.
   snap.routeGuid=snap.existing!=null&&string.Equals(snap.existing.routeHandle,snap.route.handle,StringComparison.Ordinal)&&
    string.Equals(snap.existing.fingerprint,db.FingerprintGuid,StringComparison.OrdinalIgnoreCase)?snap.existing.routeGuid:Guid.NewGuid().ToString("N");
   return snap;}
  // Snapshot of a stored row with its saved endpoint handles (Sửa tuyến). Exception = an endpoint is gone/unusable (the UI then asks to pick again).
  public static BdgklSnapshot034 SnapshotSaved(Database db,ObjectId routeId){if(db==null)throw new IOE("Không có bản vẽ.");BdgklRecord034 rec;
   using(var tr=db.TransactionManager.StartOpenCloseTransaction()){var e=tr.GetObject(routeId,OpenMode.ForRead);rec=Decode(ReadRaw(tr,e));tr.Commit();}
   ObjectId s=FromHandle(db,rec.startHandle),en=FromHandle(db,rec.endHandle);
   if(s.IsNull||s.IsErased)throw new IOE("Điểm đầu đã lưu (handle "+rec.startHandle+") không còn trong bản vẽ; chọn lại điểm đầu/cuối.");
   if(en.IsNull||en.IsErased)throw new IOE("Điểm cuối đã lưu (handle "+rec.endHandle+") không còn trong bản vẽ; chọn lại điểm đầu/cuối.");
   return Snapshot(db,routeId,s,en);}
  // Calculation of a declared config on the snapshot's live route length (nothing written).
  public static BdgklResult034 Preview(BdgklSnapshot034 snap,BdgklConfig034 config){if(snap==null)throw new IOE("Chưa có tuyến đã chọn.");
   return Bdgkl034.Calculate(Input(snap.routeGuid,config,snap.start.handle,snap.end.handle,snap.start.label,snap.end.label,snap.Length));}
  // Saves ONE record: full OK calculation required; a declared unit that differs from INSUNITS needs unitConfirmed; same exact context (route signature,
  // endpoint signatures, BDGKL raw values) inside the write transaction; readback; guard right before Commit. Identical record = nothing written.
  public static BdgklApply034 Apply(Database db,BdgklSnapshot034 snap,BdgklConfig034 config,bool unitConfirmed,Action<string> guard){
   if(db==null||snap==null||snap.route==null||snap.start==null||snap.end==null)throw new IOE("Chưa có tuyến/điểm đầu cuối hợp lệ; bản vẽ không đổi.");
   if(config==null)throw new IOE("Thiếu thông số BDGKL; bản vẽ không đổi.");
   if(snap.existingError!=null)throw new IOE("Bản ghi BDGKL đang có không dùng được ("+snap.existingError+"); MEPS không ghi đè. Bản vẽ không đổi.");
   if(!config.noSharedConduit)throw new IOE("Chưa tích 'Không dùng chung ống' (pilot chưa hỗ trợ trừ trùng ống); bản vẽ không đổi.");
   if(snap.insunits!=(int)db.Insunits||!string.Equals(snap.fingerprint,db.FingerprintGuid,StringComparison.Ordinal))throw new IOE("Bản vẽ/INSUNITS đã đổi sau khi chọn; chọn lại. Bản vẽ không đổi.");
   var c=config.Copy();var res=Preview(snap,c);
   if(res.status!=Ok)throw new IOE("Chưa lưu: dữ liệu chưa đủ/chưa hợp lệ ("+res.status+"): "+string.Join("; ",res.issues.Take(6).Select(i=>i.message))+" Bản vẽ không đổi.");
   var uw=snap.UnitWarning(c.declaredUnit);if(uw!=null&&!unitConfirmed)throw new IOE("Chưa lưu: "+uw+" Cần xác nhận đơn vị. Bản vẽ không đổi.");
   var rec=new BdgklRecord034{routeGuid=snap.routeGuid,routeHandle=snap.route.handle,fingerprint=db.FingerprintGuid,insunits=(int)db.Insunits,geometryHash=snap.route.geometryHash,
    startHandle=snap.start.handle,startBlock=snap.start.blockName,startLabel=snap.start.label,startPosition=snap.start.position,startSignature=snap.start.signature,
    endHandle=snap.end.handle,endBlock=snap.end.blockName,endLabel=snap.end.label,endPosition=snap.end.position,endSignature=snap.end.signature,config=c,sharedHorizontal=0,sharedVertical=0};
   var want=Encode(rec);var ap=new BdgklApply034{routeGuid=rec.routeGuid,result=res,written=want};
   using(var tr=db.TransactionManager.StartTransaction()){
    RouteSnapshot034 now;try{now=RouteCad034.Snapshot(db,new[]{snap.routeId});}catch(IOE ex){throw new IOE("Tuyến đã đổi sau khi chọn ("+ex.Message+"); chọn lại. Bản vẽ không đổi.");}
    if(!string.Equals(now.items[0].signature,snap.route.signature,StringComparison.Ordinal))throw new IOE("Tuyến "+snap.route.handle+" đã đổi (hình học/layer/dữ liệu tuyến) sau khi chọn; chọn lại. Bản vẽ không đổi.");
    BdgklEndpoint034 s2,e2;try{s2=Endpoint(tr,db,snap.start.id,"Điểm đầu");e2=Endpoint(tr,db,snap.end.id,"Điểm cuối");}catch(IOE ex){throw new IOE(ex.Message+"; chọn lại. Bản vẽ không đổi.");}
    if(s2.signature!=snap.start.signature||e2.signature!=snap.end.signature)throw new IOE("Block điểm đầu/cuối đã đổi (vị trí/xoay/tỉ lệ/tên/định nghĩa/TAG) sau khi chọn; chọn lại. Bản vẽ không đổi.");
    var e=(Entity)tr.GetObject(snap.routeId,OpenMode.ForRead);string[] had;
    try{had=ReadRaw(tr,e);}catch(IOE ex){throw new IOE(ex.Message+". Bản vẽ không đổi.");}
    if(!SameRaw(had,snap.existingRaw))throw new IOE("Dữ liệu BDGKL của tuyến "+snap.route.handle+" đã đổi sau khi chọn (lệnh khác/Undo); mở lại bảng. Bản vẽ không đổi.");
    if(had!=null&&had.SequenceEqual(want,StringComparer.Ordinal)){ap.message="Tuyến "+snap.route.handle+" đã có đúng thông số BDGKL này; không ghi gì.";return ap;}
    if(e.ExtensionDictionary.IsNull){e.UpgradeOpen();e.CreateExtensionDictionary();}
    var d=(DBDictionary)tr.GetObject(e.ExtensionDictionary,OpenMode.ForWrite);
    using(var b=new ResultBuffer(want.Select(x=>new TypedValue((int)DxfCode.Text,x)).ToArray())){
     if(d.Contains(Key)){var x=tr.GetObject(d.GetAt(Key),OpenMode.ForWrite) as Xrecord;if(x==null)throw new IOE("Khoá "+Key+" không phải Xrecord; MEPS không ghi đè. Bản vẽ không đổi.");x.Data=b;}
     else{var x=new Xrecord();x.Data=b;d.SetAt(Key,x);tr.AddNewlyCreatedDBObject(x,true);}}
    Fail("after-write");
    var back=ReadRaw(tr,e);if(back==null||!back.SequenceEqual(want,StringComparer.Ordinal))throw new IOE("Đọc lại bản ghi BDGKL không khớp; bản vẽ không đổi.");
    Decode(back);
    if(guard!=null)guard("ngay trước khi lưu bản ghi BDGKL");
    tr.Commit();}
   ap.changed=true;
   // After Commit (same command): fresh read. A mismatch is reported (U reverts the step); nothing is repaired here.
   BdgklRow034 r=null;try{r=Read(db,snap.routeId);}catch(Exception){}
   ap.warn=r==null||r.status!=Ok;
   ap.message=(ap.warn?"CẢNH BÁO: đọc lại sau khi lưu không ra trạng thái OK ("+(r==null?"không đọc được":r.status+": "+r.reason)+"); dùng U để hoàn tác. ":"")+
    "Đã lưu BDGKL cho tuyến "+snap.route.handle+" ("+snap.start.label+" → "+snap.end.label+"): M = "+Route034.Meters(res.routeM.Value)+" m, sau hệ số "+Route034.Meters(res.routeAfterFactorM.Value)+
    " m. U/UNDO hoàn tác; chưa tự lưu DWG. Khối lượng luôn tính lại từ chiều dài hiện tại; COPY, sửa tuyến/block/TAG hoặc đổi INSUNITS cần xác nhận lại.";
   return ap;}
  // Row of one Model entity carrying Key (null = no BDGKL record). Never throws for bad data; status/reason say why.
  static BdgklRow034 Evaluate(Transaction tr,Database db,Entity e){string[] raw;var row=new BdgklRow034{routeId=e.ObjectId,routeHandle=e.Handle.ToString()};
   try{raw=ReadRaw(tr,e);}catch(IOE ex){row.status=Invalid;row.reason=ex.Message;return row;}
   if(raw==null)return null;
   BdgklRecord034 rec;try{rec=Decode(raw);}catch(IOE ex){row.status=Invalid;row.reason=ex.Message;return row;}
   row.record=rec;row.routeGuid=rec.routeGuid;row.startLabel=rec.startLabel;row.endLabel=rec.endLabel;
   var iss=new List<string>();string hash=null;
   if(e.OwnerId!=SymbolUtilityServices.GetBlockModelSpaceId(db))iss.Add("tuyến không nằm trực tiếp trong Model");
   else{try{var it=RouteCad034.SnapshotRead(db,new[]{e.ObjectId}).items[0]; /* BN02: read-only (locked layer still measured) */row.liveLength=it.length;hash=it.geometryHash;}catch(IOE ex){iss.Add("không đo được tuyến: "+ex.Message);}}
   iss.AddRange(RouteIssues(rec,db,row.routeHandle,hash));
   BdgklEndpoint034 s=null,en=null;
   foreach(var side in new[]{0,1}){string h=side==0?rec.startHandle:rec.endHandle,sig=side==0?rec.startSignature:rec.endSignature,what=side==0?"Điểm đầu":"Điểm cuối",lbl=side==0?rec.startLabel:rec.endLabel;
    var id=FromHandle(db,h);if(id.IsNull||id.IsErased){iss.Add(what+" "+lbl+" (handle "+h+") đã bị xoá");continue;}
    BdgklEndpoint034 p;try{p=Endpoint(tr,db,id,what);}catch(IOE ex){iss.Add(ex.Message);continue;}
    if(side==0){s=p;row.startId=id;}else{en=p;row.endId=id;}
    if(!string.Equals(p.signature,sig,StringComparison.Ordinal))iss.Add(what+" "+lbl+" đã đổi sau khi lưu (MOVE/xoay/tỉ lệ/đổi tên/định nghĩa block/TAG)");}
   row.input=Input(rec.routeGuid,rec.config,rec.startHandle,rec.endHandle,rec.startLabel,rec.endLabel,row.liveLength);
   row.result=Bdgkl034.Calculate(row.input);row.warning=Warn(s,en,(int)db.Insunits,rec.config.declaredUnit);
   var unresolved=new[]{s,en}.Where(q=>q!=null&&!q.defResolved).Select(q=>q.defIssue).ToList();
   if(unresolved.Count>0){row.status=Bdgkl034.Check;row.reason="không tính: "+string.Join("; ",unresolved)+(iss.Count>0?"; "+string.Join("; ",iss):"");}
   else if(rec.OutsidePilot){row.status=Bdgkl034.Check;row.reason="bản ghi có trừ trùng ống U/V ≠ 0: ngoài phạm vi pilot (không tính, MEPS không xoá)"+(iss.Count>0?"; "+string.Join("; ",iss):"");}
   else if(iss.Count>0){row.status=Reconfirm;row.reason="cần mở 'Sửa tuyến' và lưu lại: "+string.Join("; ",iss);}
   else{row.status=row.result.status;row.reason=row.result.issues.Count==0?null:string.Join("; ",row.result.issues.Select(i=>i.message));}
   return row;}
  // Read helper (QA/UI): current row of one entity; null = no BDGKL record. Read only.
  public static BdgklRow034 Read(Database db,ObjectId routeId){if(db==null)throw new IOE("Không có bản vẽ.");
   if(routeId.IsNull||routeId.IsErased||routeId.Database!=db)throw new IOE("đối tượng đã bị xoá hoặc không thuộc bản vẽ hiện tại");
   using(var tr=db.TransactionManager.StartOpenCloseTransaction()){var e=tr.GetObject(routeId,OpenMode.ForRead) as Entity;var r=e==null?null:Evaluate(tr,db,e);tr.Commit();return r;}}
  // Every Model entity carrying Key (INVALID included), then Bdgkl034.Aggregate. Non-OK rows enter the batch with no length under a per-handle key so they
  // are excluded and counted in their group (never summed, no duplicate-key clash with the original of a COPY); INVALID rows have no group. Read only.
  public static BdgklScan034 ScanModel(Database db){if(db==null)throw new IOE("Không có bản vẽ.");var s=new BdgklScan034{insunits=(int)db.Insunits,fingerprint=db.FingerprintGuid};
   using(var tr=db.TransactionManager.StartOpenCloseTransaction()){
    foreach(ObjectId id in (BlockTableRecord)tr.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db),OpenMode.ForRead)){if(id.IsErased)continue;var e=tr.GetObject(id,OpenMode.ForRead) as Entity;if(e==null)continue;
     BdgklRow034 r;try{r=Evaluate(tr,db,e);}catch(Autodesk.AutoCAD.Runtime.Exception ex){r=new BdgklRow034{routeId=id,routeHandle=e.Handle.ToString(),status=Invalid,reason="lỗi đọc ("+ex.Message+")"};}
     if(r!=null)s.rows.Add(r);}
    tr.Commit();}
   var inputs=new List<BdgklInput034>();var map=new List<BdgklRow034>();
   foreach(var r in s.rows){if(r.input==null)continue;BdgklInput034 i;
    if(r.status==Ok)i=r.input;else{i=r.input.Copy();i.routeKey="EXCLUDED#"+r.routeHandle;i.nativeLength=null;}
    inputs.Add(i);map.Add(r);}
   s.batch=Bdgkl034.Aggregate(inputs);
   // Batch-level refusals (e.g. duplicate route GUID) also exclude the row on screen.
   for(int k=0;k<map.Count;k++){var br=s.batch.rows[k].result;if(map[k].status==Ok&&br.status!=Ok){map[k].status=br.status;map[k].reason=string.Join("; ",br.issues.Select(x=>x.message));}}
   return s;}
 }
}
#endif
