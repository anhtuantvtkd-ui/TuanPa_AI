using System;using System.Linq;using System.Collections.Generic;using System.Globalization;
using Autodesk.AutoCAD.Runtime;using Autodesk.AutoCAD.ApplicationServices;using Autodesk.AutoCAD.DatabaseServices;using Autodesk.AutoCAD.EditorInput;using Autodesk.AutoCAD.Geometry;
using App=Autodesk.AutoCAD.ApplicationServices.Application;using CadColor=Autodesk.AutoCAD.Colors.Color;using CadColorMethod=Autodesk.AutoCAD.Colors.ColorMethod;
using DbPolyline=Autodesk.AutoCAD.DatabaseServices.Polyline;
[assembly:CommandClass(typeof(Tvxd.Meps.FlexCommands022))]
// FLEX022_STANDALONE = pilot DLL; FLEX022 = full product DLL (023 integration). Either registers the grip overrule once at NETLOAD (no database access),
// so bodies of drawings opened later in the session get their grips. 029: this file now needs FlexCurve029.cs (and FlexGeometry022.cs) in every build.
#if FLEX022_STANDALONE||FLEX022
[assembly:ExtensionApplication(typeof(Tvxd.Meps.FlexApp022))]
#endif
namespace Tvxd.Meps {
 // TVXD-MEPS-FLEXDUCT-022 native PILOT (not integrated into the palette). One flex duct = two database entities linked by XData:
 //  BODY  = LWPOLYLINE on E-X-ONGGIOMEM, ACI 3, LW 0.05 mm (explicit on the entity and on the layer), vertices from FlexGeometry022;
 //  SPINE = the user's original LINE (same object/handle), moved to E-X-ONGGIOMEM-TAM (ACI 8, LW 0.05, Continuous), color/LW/linetype ByLayer.
 // Both carry one XData record (RegApp TVXD_MEPS_FLEX022, 25 text values, schema TVXD_MEPS_FLEXDUCT 022.1): role, assembly GUID, own handle, peer
 // handle, diameter (drawing units), INSUNITS, unit, direction flag, cell count, spine ends at the last MEPS write, spine ends at creation, type, source.
 // A GripOverrule (filtered by that XData) replaces the body's vertex grips by two spine-end grips; moving one projects the offset on the spine axis,
 // rewrites the spine end and regenerates the body at the same diameter, inside the grip command (no Idle/reactor writes). Spine LINEs show no grips.
 // Verification never trusts stored lengths: a body is VERIFIED only when its vertices equal FlexGeometry022.Build(current spine, D) and both
 // records point at each other with their own current handles; anything else is UNVERIFIED with a reason and excluded from totals.
 // TVXD-MEPS-FLEXUI-029 general centerlines (same 25 fields, record version field 2):
 //  - 022.1 = LINE spine, algorithm FlexGeometry022, coupled end grips: UNCHANGED (a LINE still creates 022.1).
 //  - 029.1 = LWPOLYLINE / ARC / SPLINE spine (the original object, same handle/type/geometry; only layer/color/LW/linetype ByLayer and its own XData
 //    change), body = FlexCurve029.Build(current spine, D, fromEnd). Field 25 (source) = "MEPS029|<TYPE>|provenance"; ends = spine Start/EndPoint at the
 //    last MEPS write (metadata only, never used to rebuild), origin = at creation (kept by updates).
 //  - Verification rebuilds the body from the CURRENT curve every time (link, style, owner, units, source type, cell count, elevation, every vertex/bulge).
 //    BOQ length of a verified 029.1 pair = native curve length, never the chord.
 //  - Curved body: no grips. Curved spine: native grips (the LWPOLYLINE overrule defers to base for a 029.1 SPINE; Arc/Spline are not overruled). Editing
 //    the spine makes the pair STALE (UNVERIFIED, excluded) until the explicit update (UpdateCurve), started by the SAME palette button on the owned body or
 //    spine. No reactors. An update rewrites the existing body and both records in one transaction (same handles, no new entities).
 public sealed class FlexRecord022 {
  public string role,assembly,self,peer,unit;public double diameter;public int insunits,cells;public bool fromEnd;
  public double[] ends=new double[6],origin=new double[6];
  // 029: record version (field 2) and source/provenance (field 25). 022.1 records keep FlexDuct022.Provenance.
  public string version=FlexDuct022.Version,source=FlexDuct022.Provenance;
  public string SourceType{get{return version==FlexDuct022.Version?"LINE":FlexDuct022.Type029(source);}}
 }
 public sealed class FlexBoqItem022 {
  public string assembly,body,spine,status,reason,unit,typeCode=FlexDuct022.TypeCode,sourceType;public double diameter,length,metres;public int insunits;
  public string Key{get{return typeCode+"|D="+FlexGeometry022.R(diameter)+"|"+unit;}}
 }
 public sealed class FlexBoqTotal022 {public string key,unit;public double diameter,length,metres;public int count;}
 public sealed class FlexBoqReport022 {public readonly List<FlexBoqItem022> items=new List<FlexBoqItem022>();public readonly List<FlexBoqTotal022> totals=new List<FlexBoqTotal022>();
  public int Unverified{get{return items.Count(i=>i.status!=FlexDuct022.Verified);}}}
 public sealed class FlexResult022 {public ObjectId body,spine;public string assembly;public int cells;public double length;}
 public static class FlexDuct022 {
  public const string AppName="TVXD_MEPS_FLEX022",Schema="TVXD_MEPS_FLEXDUCT",Version="022.1",Body="BODY",Spine="SPINE",TypeCode="ONG_GIO_MEM";
  public const string BodyLayer="E-X-ONGGIOMEM",CenterLayer="E-X-ONGGIOMEM-TAM",Verified="VERIFIED",Unverified="UNVERIFIED";
  public const string Provenance="MEPS022 straight-LINE port of Ong Noi Mem.lsp (ssg 2008, cadviet.com, free LISP)";
  public const string Version029="029.1",SourcePrefix029="MEPS029|",Provenance029="MEPS029 general-curve port of Ong Noi Mem.lsp (ssg 2008, cadviet.com, free LISP)";
  public const short BodyAci=3,CenterAci=8;public const int Fields=25;
  public const string StraightAdvice="Đây là ống thẳng theo LINE (bản ghi 022.1): chọn thân ống và kéo grip đầu mút để đổi chiều dài theo phương tuyến; không cần cập nhật, MEPS không tạo ống thứ hai.";
  static readonly CultureInfo Inv=CultureInfo.InvariantCulture;
  // ---- record ----
  public static string Source029(string type){return SourcePrefix029+type+"|"+Provenance029;}
  // Source type of a 029.1 source field; null when the field is not a valid 029 source.
  public static string Type029(string source){if(source==null||!source.StartsWith(SourcePrefix029,StringComparison.Ordinal))return null;var p=source.Split('|');
   return p.Length>=3&&(p[1]=="LWPOLYLINE"||p[1]=="ARC"||p[1]=="SPLINE")?p[1]:null;}
  // LINE | LWPOLYLINE | ARC | SPLINE; null = not an accepted centerline type (Polyline2d/3d, Circle, Ellipse, …).
  public static string SourceTypeOf(Curve c){return c is Line?"LINE":c is DbPolyline?"LWPOLYLINE":c is Arc?"ARC":c is Spline?"SPLINE":null;}
  static ResultBuffer Encode(FlexRecord022 r){var v=new List<string>{Schema,r.version,r.role,r.assembly,r.self,r.peer,FlexGeometry022.R(r.diameter),r.insunits.ToString(Inv),r.unit,r.fromEnd?"1":"0",r.cells.ToString(Inv)};
   v.AddRange(r.ends.Select(x=>FlexGeometry022.R(x)));v.AddRange(r.origin.Select(x=>FlexGeometry022.R(x)));v.Add(TypeCode);v.Add(r.source);
   var tv=new List<TypedValue>{new TypedValue((int)DxfCode.ExtendedDataRegAppName,AppName)};tv.AddRange(v.Select(s=>new TypedValue((int)DxfCode.ExtendedDataAsciiString,s)));return new ResultBuffer(tv.ToArray());}
  // Null when the object has no FLEX022 XData; malformed/other version → exception (callers report UNVERIFIED / refuse; never overwrite).
  public static FlexRecord022 Read(DBObject o){TypedValue[] a;using(var rb=o.GetXDataForApplication(AppName)){if(rb==null)return null;a=rb.AsArray();}
   if(a.Length!=Fields+1||a.Skip(1).Any(t=>t.TypeCode!=(short)DxfCode.ExtendedDataAsciiString))throw new InvalidOperationException("XData "+AppName+" của "+o.Handle+" sai cấu trúc.");
   var v=a.Skip(1).Select(t=>Convert.ToString(t.Value,Inv)).ToArray();
   if(v[0]!=Schema||(v[1]!=Version&&v[1]!=Version029))throw new InvalidOperationException("XData "+AppName+" của "+o.Handle+" khác schema "+Schema+" "+Version+"/"+Version029+".");
   var r=new FlexRecord022{role=v[2],assembly=v[3],self=v[4],peer=v[5],diameter=FlexGeometry022.P(v[6]),unit=v[8],version=v[1],source=v[24]};int k;
   if(r.role!=Body&&r.role!=Spine)throw new InvalidOperationException("Vai trò lạ "+r.role+".");Guid g;if(!Guid.TryParseExact(r.assembly,"N",out g))throw new InvalidOperationException("Mã ống không hợp lệ.");
   long h;if(!long.TryParse(r.self,NumberStyles.HexNumber,Inv,out h)||!long.TryParse(r.peer,NumberStyles.HexNumber,Inv,out h))throw new InvalidOperationException("Handle không hợp lệ.");
   if(!int.TryParse(v[7],NumberStyles.Integer,Inv,out k))throw new InvalidOperationException("INSUNITS không hợp lệ.");r.insunits=k;string un;double m;
   if(!FlexGeometry022.Unit(k,out un,out m)||un!=r.unit||r.diameter<=0)throw new InvalidOperationException("Đơn vị/đường kính không hợp lệ.");
   if(v[9]!="0"&&v[9]!="1")throw new InvalidOperationException("Cờ hướng không hợp lệ.");r.fromEnd=v[9]=="1";
   if(!int.TryParse(v[10],NumberStyles.Integer,Inv,out k)||k<FlexGeometry022.MinCells||k>FlexGeometry022.MaxCells)throw new InvalidOperationException("Số nếp không hợp lệ.");r.cells=k;
   for(int i=0;i<6;i++){r.ends[i]=FlexGeometry022.P(v[11+i]);r.origin[i]=FlexGeometry022.P(v[17+i]);}
   if(v[23]!=TypeCode)throw new InvalidOperationException("Loại ống lạ "+v[23]+".");
   if(r.version==Version029&&Type029(r.source)==null)throw new InvalidOperationException("Nguồn đường tâm '"+r.source+"' không hợp lệ cho "+Version029+".");return r;}
  static void Write(Entity e,FlexRecord022 r){using(var rb=Encode(r))e.XData=rb;}
  static void SetEnds(double[] a,Point3d s,Point3d e){a[0]=s.X;a[1]=s.Y;a[2]=s.Z;a[3]=e.X;a[4]=e.Y;a[5]=e.Z;}
  static ObjectId ByHandle(Database db,string hex){try{var id=db.GetObjectId(false,new Handle(Convert.ToInt64(hex,16)),0);return id;}catch{return ObjectId.Null;}}
  // ---- body vertices ----
  static bool Verts(DbPolyline pl,out double[] xs,out double[] ys,out double[] bs){int m=pl.NumberOfVertices;xs=new double[m];ys=new double[m];bs=new double[m];
   for(int i=0;i<m;i++){var p=pl.GetPoint2dAt(i);xs[i]=p.X;ys[i]=p.Y;bs[i]=pl.GetBulgeAt(i);}return m>=5*FlexGeometry022.MinCells+3&&(m-3)%5==0;}
  static void Apply(DbPolyline pl,double[] xs,double[] ys,double[] bs){int m=xs.Length;while(pl.NumberOfVertices>m)pl.RemoveVertexAt(pl.NumberOfVertices-1);
   for(int i=0;i<m;i++){var p=new Point2d(xs[i],ys[i]);if(i<pl.NumberOfVertices){pl.SetPointAt(i,p);pl.SetBulgeAt(i,bs[i]);pl.SetStartWidthAt(i,0);pl.SetEndWidthAt(i,0);}else pl.AddVertexAt(i,p,bs[i],0,0);}}
  static void Orient(bool fromEnd,double sx,double sy,double ex,double ey,out double ox,out double oy,out double tx,out double ty){if(fromEnd){ox=ex;oy=ey;tx=sx;ty=sy;}else{ox=sx;oy=sy;tx=ex;ty=ey;}}
  // Spine start/end recovered from the body outline (grip positions); false when the body is not a well-formed owned STRAIGHT (022.1) body.
  // 029.1 (curved) bodies return false: they get no grips at all.
  internal static bool GripEnds(DbPolyline pl,out FlexRecord022 r,out Point3d s,out Point3d e){r=null;s=e=Point3d.Origin;
   try{r=Read(pl);}catch{return false;}if(r==null||r.role!=Body||r.version!=Version||!pl.Normal.IsEqualTo(Vector3d.ZAxis))return false;
   // Resident bodies: own handle and stored cell count must match. Drag clones (no ObjectId) carry the original's record and are only shape-checked.
   bool resident=!pl.ObjectId.IsNull;if(resident&&r.self!=pl.Handle.ToString())return false;
   double[] xs,ys,bs;if(!Verts(pl,out xs,out ys,out bs)||(resident&&xs.Length!=5*r.cells+3))return false;double ox,oy,tx,ty;FlexGeometry022.Ends(xs,ys,out ox,out oy,out tx,out ty);
   var o=new Point3d(ox,oy,pl.Elevation);var t=new Point3d(tx,ty,pl.Elevation);if(r.fromEnd){s=t;e=o;}else{s=o;e=t;}return true;}
  // ---- verification (read-only) ----
  static bool BodyStyle(DbPolyline b){return string.Equals(b.Layer,BodyLayer,StringComparison.OrdinalIgnoreCase)&&b.LineWeight==LineWeight.LineWeight005&&b.Color.ColorMethod==CadColorMethod.ByAci&&b.Color.ColorIndex==BodyAci&&!b.Closed&&b.Normal.IsEqualTo(Vector3d.ZAxis);}
  // Linkage only (no geometry): null = body and spine are a consistent, uniquely-owned pair of the same version, source type, units, styles and space.
  // spine is returned whenever it was found (even when a later check fails).
  static string Link(Transaction tr,DbPolyline body,FlexRecord022 r,out Curve spine){spine=null;var db=body.Database;
   if(r.role!=Body)return "NOT_BODY";if(r.self!=body.Handle.ToString())return "CLONED: bản sao/được chèn (handle "+body.Handle+" ≠ "+r.self+")";
   if((int)db.Insunits!=r.insunits)return "UNITS_CHANGED: INSUNITS "+(int)db.Insunits+" ≠ "+r.insunits;
   if(!BodyStyle(body))return "BODY_STYLE_ALTERED: thân không còn trên "+BodyLayer+"/ACI "+BodyAci+"/0.05 mm";
   var sid=ByHandle(db,r.peer);if(sid.IsNull)return "SPINE_MISSING: không tìm thấy đường tâm "+r.peer;if(sid.IsErased)return "SPINE_ERASED: đường tâm "+r.peer+" đã xoá";
   spine=tr.GetObject(sid,OpenMode.ForRead) as Curve;if(spine==null)return "SPINE_NOT_CURVE: đối tượng "+r.peer+" không phải đường tâm";
   string st=SourceTypeOf(spine);
   if(r.version==Version&&!(spine is Line))return "SPINE_NOT_LINE";
   if(r.version==Version029&&(st==null||st=="LINE"||st!=r.SourceType))return "SOURCE_TYPE_CHANGED: đường tâm là "+(st??spine.GetType().Name)+", bản ghi "+r.SourceType;
   FlexRecord022 s;try{s=Read(spine);}catch(System.Exception ex){return "SPINE_MALFORMED: "+ex.Message;}
   if(s==null)return "SPINE_UNLINKED";if(s.role!=Spine||s.assembly!=r.assembly||s.peer!=r.self||s.self!=spine.Handle.ToString()||s.diameter!=r.diameter||s.insunits!=r.insunits||s.fromEnd!=r.fromEnd)return "LINK_MISMATCH: hai bản ghi không trỏ về nhau";
   if(s.version!=r.version||(r.version==Version029&&(s.source!=r.source||s.cells!=r.cells)))return "LINK_MISMATCH: phiên bản/nguồn/số nếp của hai bản ghi khác nhau";
   if(!string.Equals(spine.Layer,CenterLayer,StringComparison.OrdinalIgnoreCase))return "SPINE_STYLE_ALTERED: đường tâm không còn trên "+CenterLayer;
   if(spine.OwnerId!=body.OwnerId)return "OWNER_MISMATCH: thân và đường tâm không cùng không gian";return null;}
  // Geometry only: null = the body equals the body rebuilt from the CURRENT spine (022.1: FlexGeometry022; 029.1: FlexCurve029). Otherwise STALE_GEOMETRY.
  static string Geometry(DbPolyline body,FlexRecord022 r,Curve spine){double[] xs,ys,bs;
   if(r.version==Version){var line=spine as Line;if(line==null)return "SPINE_NOT_LINE";
    Point3d a=line.StartPoint,b=line.EndPoint;double L=Math.Sqrt((b.X-a.X)*(b.X-a.X)+(b.Y-a.Y)*(b.Y-a.Y)),tol=FlexGeometry022.Tolerance(L,r.diameter);
    if(Math.Abs(a.Z-b.Z)>tol||Math.Abs(a.Z-body.Elevation)>tol)return "STALE_GEOMETRY: cao độ đường tâm/thân khác nhau";
    double ox,oy,tx,ty;Orient(r.fromEnd,a.X,a.Y,b.X,b.Y,out ox,out oy,out tx,out ty);double[] gx,gy,gb;
    try{FlexGeometry022.Build(ox,oy,tx,ty,r.diameter,out gx,out gy,out gb);}catch(ArgumentException ex){return "STALE_GEOMETRY: "+ex.Message;}
    if(!Verts(body,out xs,out ys,out bs)||!FlexGeometry022.Same(xs,ys,bs,gx,gy,gb,tol))return "STALE_GEOMETRY: thân không khớp đường tâm hiện tại (sửa riêng một phần?)";return null;}
   const string Hint=" (đường tâm đã sửa? bấm nút Ống gió mềm, chọn ống để cập nhật)";
   FlexShape029 g;try{g=FlexCurve029.Build(spine,r.diameter,r.fromEnd);}catch(FlexCurveException029 ex){return "STALE_GEOMETRY: "+ex.Message;}
   double t=FlexGeometry022.Tolerance(g.Length,r.diameter);
   if(g.Cells!=r.cells)return "STALE_GEOMETRY: số nếp "+g.Cells+" ≠ "+r.cells+Hint;
   if(Math.Abs(g.Elevation-body.Elevation)>t)return "STALE_GEOMETRY: cao độ đường tâm/thân khác nhau"+Hint;
   if(!Verts(body,out xs,out ys,out bs)||!FlexGeometry022.Same(xs,ys,bs,g.X,g.Y,g.Bulges,t))return "STALE_GEOMETRY: thân không khớp đường tâm hiện tại"+Hint;return null;}
  // Null = body and spine are a consistent, uniquely-owned pair matching the current spine geometry. Otherwise a reason code + text.
  static string Verify(Transaction tr,DbPolyline body,FlexRecord022 r,out Curve spine){return Link(tr,body,r,out spine)??Geometry(body,r,spine);}
  public static string Validate(Database db,ObjectId bodyId){using(var tr=db.TransactionManager.StartOpenCloseTransaction()){var b=tr.GetObject(bodyId,OpenMode.ForRead) as DbPolyline;if(b==null)return "NOT_BODY";
    FlexRecord022 r;try{r=Read(b);}catch(System.Exception ex){return "MALFORMED: "+ex.Message;}if(r==null)return "NOT_BODY";Curve s;return Verify(tr,b,r,out s);}}
  // BOQ length: 022.1 LINE = XY length (unchanged); 029.1 = native curve length (horizontal curve: equals its XY length). Never the chord of a curve.
  static double SpineLength(Curve c,FlexRecord022 r){var l=c as Line;
   if(r.version==Version&&l!=null)return Math.Sqrt(Math.Pow(l.EndPoint.X-l.StartPoint.X,2)+Math.Pow(l.EndPoint.Y-l.StartPoint.Y,2));
   try{return c.GetDistanceAtParameter(c.EndParam)-c.GetDistanceAtParameter(c.StartParam);}catch{return 0;}}
  // ---- layers: an existing layer is used only when it already equals the standard; it is never modified ----
  static string LayerIssue(Database db,LayerTableRecord l,short aci){
   if(l.IsErased)return "Layer "+l.Name+" đã bị xoá.";if(l.IsLocked)return "Layer "+l.Name+" đang khoá; mở khoá rồi thử lại. Chưa sửa bản vẽ.";
   if(l.IsFrozen||l.IsOff)return "Layer "+l.Name+" đang tắt/đóng băng; bật lại rồi thử lại. Chưa sửa bản vẽ.";
   if(l.Color.ColorMethod!=CadColorMethod.ByAci||l.Color.ColorIndex!=aci||l.LineWeight!=LineWeight.LineWeight005||l.LinetypeObjectId!=db.ContinuousLinetype)
    return "Layer "+l.Name+" có sẵn nhưng khác chuẩn (ACI "+aci+", 0.05 mm, Continuous); MEPS không sửa layer có sẵn. Chưa sửa bản vẽ.";return null;}
  static string CheckLayer(Transaction tr,Database db,string name,short aci){var lt=(LayerTable)tr.GetObject(db.LayerTableId,OpenMode.ForRead);if(!lt.Has(name))return null;
   return LayerIssue(db,(LayerTableRecord)tr.GetObject(lt[name],OpenMode.ForRead),aci);}
  static ObjectId EnsureLayer(Transaction tr,Database db,string name,short aci,string description){var lt=(LayerTable)tr.GetObject(db.LayerTableId,OpenMode.ForRead);
   if(lt.Has(name)){var id=lt[name];string why=LayerIssue(db,(LayerTableRecord)tr.GetObject(id,OpenMode.ForRead),aci);if(why!=null)throw new InvalidOperationException(why);return id;}
   lt.UpgradeOpen();var l=new LayerTableRecord{Name=name,Color=CadColor.FromColorIndex(CadColorMethod.ByAci,aci),LineWeight=LineWeight.LineWeight005,Description=description,IsPlottable=true};
   l.LinetypeObjectId=db.ContinuousLinetype;var nid=lt.Add(l);tr.AddNewlyCreatedDBObject(l,true);return nid;}
  static void EnsureRegApp(Transaction tr,Database db){var rat=(RegAppTable)tr.GetObject(db.RegAppTableId,OpenMode.ForRead);if(!rat.Has(AppName)){rat.UpgradeOpen();var ra=new RegAppTableRecord{Name=AppName};rat.Add(ra);tr.AddNewlyCreatedDBObject(ra,true);}}
  // The original centerline becomes the faint BOQ centerline: layer -TAM, color/LW/linetype ByLayer. Geometry, handle and type are not touched.
  static void Restyle(Database db,Entity src,ObjectId centerLayer){src.UpgradeOpen();src.LayerId=centerLayer;src.Color=CadColor.FromColorIndex(CadColorMethod.ByLayer,256);src.LineWeight=LineWeight.ByLayer;src.LinetypeId=db.ByLayerLinetype;}
  static DbPolyline NewBody(Database db,double[] xs,double[] ys,double[] bs,double elevation,ObjectId bodyLayer){var pl=new DbPolyline(xs.Length);pl.SetDatabaseDefaults(db);
   for(int i=0;i<xs.Length;i++)pl.AddVertexAt(i,new Point2d(xs[i],ys[i]),bs[i],0,0);pl.Normal=Vector3d.ZAxis;pl.Elevation=elevation;
   pl.LayerId=bodyLayer;pl.Color=CadColor.FromColorIndex(CadColorMethod.ByAci,BodyAci);pl.LineWeight=LineWeight.LineWeight005;pl.LinetypeId=db.ByLayerLinetype;return pl;}
  // FlexCurve029 refusals become ArgumentException (same class as FlexGeometry022.Check refusals of the straight path).
  static FlexShape029 Shape(Curve c,double diameter,bool fromEnd){try{return FlexCurve029.Build(c,diameter,fromEnd);}catch(FlexCurveException029 ex){throw new ArgumentException(ex.Message);}}
  // ---- create ----
  // Read-only check of a candidate centerline (LINE, LWPOLYLINE, ARC, SPLINE); null when Create can proceed (diameter/curve geometry still to be checked).
  // length = XY length (LINE) or native curve length (others). Used before prompting the diameter.
  public static string Inspect(Database db,ObjectId lineId,out double length){length=0;string un;double m;
   if(!FlexGeometry022.Unit((int)db.Insunits,out un,out m))return (int)db.Insunits==0?"Bản vẽ chưa đặt đơn vị (INSUNITS = 0, Unitless). Đặt INSUNITS (vd. 4 = mm) rồi thử lại; MEPS không tự giả định mm.":"Đơn vị bản vẽ INSUNITS = "+(int)db.Insunits+" chưa hỗ trợ (chỉ mm, cm, dm, m, in, ft).";
   using(var tr=db.TransactionManager.StartOpenCloseTransaction()){var cv=tr.GetObject(lineId,OpenMode.ForRead) as Curve;if(cv==null||cv.IsErased)return "Chỉ chọn LINE, PLINE (LWPOLYLINE), ARC hoặc SPLINE làm đường tâm.";
    string type=SourceTypeOf(cv);if(type==null)return "Chỉ chọn LINE, PLINE (LWPOLYLINE), ARC hoặc SPLINE làm đường tâm ("+cv.GetType().Name+" chưa hỗ trợ). Chưa sửa bản vẽ.";
    var owner=tr.GetObject(cv.OwnerId,OpenMode.ForRead) as BlockTableRecord;if(owner==null||!owner.IsLayout)return type+" phải nằm trực tiếp trong Model/Layout (không trong block).";
    FlexRecord022 had;try{had=Read(cv);}catch(System.Exception ex){return type+" có dữ liệu "+AppName+" hỏng ("+ex.Message+"); MEPS không ghi đè.";}
    if(had!=null)return type+" này đã thuộc một ống gió mềm MEPS; bấm nút Ống gió mềm và chọn nó để cập nhật (MEPS không tạo ống thứ hai).";
    var ll=(LayerTableRecord)tr.GetObject(cv.LayerId,OpenMode.ForRead);if(ll.IsLocked)return "Layer "+ll.Name+" của "+type+" đang khoá. Chưa sửa bản vẽ.";
    var line=cv as Line;
    if(line!=null){Point3d a=line.StartPoint,b=line.EndPoint;if(!FlexGeometry022.Finite(a.X+a.Y+a.Z+b.X+b.Y+b.Z))return "Toạ độ LINE không hữu hạn.";
     length=Math.Sqrt((b.X-a.X)*(b.X-a.X)+(b.Y-a.Y)*(b.Y-a.Y));if(!(length>0))return "LINE có chiều dài 0 trong mặt phẳng XY.";
     if(Math.Abs(a.Z-b.Z)>FlexGeometry022.Tolerance(length,0))return "LINE không nằm ngang (Z hai đầu khác nhau); chỉ nhận LINE song song mặt XY.";
     return CheckLayer(tr,db,BodyLayer,BodyAci)??CheckLayer(tr,db,CenterLayer,CenterAci);}
    // A PLINE with a width would not be a faint centerline; MEPS never changes the source's widths.
    var pl=cv as DbPolyline;if(pl!=null)for(int i=0;i<pl.NumberOfVertices;i++)if(pl.GetStartWidthAt(i)!=0||pl.GetEndWidthAt(i)!=0)return "PLINE có độ rộng nét (width) khác 0 tại đỉnh "+(i+1)+"; MEPS không sửa đường tâm gốc. Đặt width = 0 rồi thử lại. Chưa sửa bản vẽ.";
    try{length=cv.GetDistanceAtParameter(cv.EndParam)-cv.GetDistanceAtParameter(cv.StartParam);}catch(System.Exception ex){return "Không đo được chiều dài "+type+" ("+ex.Message+"). Chưa sửa bản vẽ.";}
    if(!FlexGeometry022.Finite(length)||!(length>0))return type+" có chiều dài 0. Chưa sửa bản vẽ.";
    return CheckLayer(tr,db,BodyLayer,BodyAci)??CheckLayer(tr,db,CenterLayer,CenterAci);}}
  // Deterministic create (QA and the interactive command). fromEnd = the corrugation starts at the centerline's end point (source: start nearest the pick).
  // One transaction: on any refusal nothing is written. The centerline keeps its handle, type and geometry; only its layer/color/LW/linetype and own XData
  // change. LINE → 022.1 (unchanged); LWPOLYLINE/ARC/SPLINE → 029.1 (CreateCurve).
  public static FlexResult022 Create(Database db,ObjectId lineId,double diameter,bool fromEnd){double L;string why=Inspect(db,lineId,out L);if(why!=null)throw new InvalidOperationException(why);
   if(!lineId.ObjectClass.IsDerivedFrom(RXObject.GetClass(typeof(Line))))return CreateCurve(db,lineId,diameter,fromEnd);
   why=FlexGeometry022.Check(L,diameter);if(why!=null)throw new ArgumentException(why);string unit;double metres;FlexGeometry022.Unit((int)db.Insunits,out unit,out metres);
   using(var tr=db.TransactionManager.StartTransaction()){var line=(Line)tr.GetObject(lineId,OpenMode.ForRead);Point3d a=line.StartPoint,b=line.EndPoint;
    double ox,oy,tx,ty;double[] xs,ys,bs;Orient(fromEnd,a.X,a.Y,b.X,b.Y,out ox,out oy,out tx,out ty);int n=FlexGeometry022.Build(ox,oy,tx,ty,diameter,out xs,out ys,out bs);
    EnsureRegApp(tr,db);
    var bodyLayer=EnsureLayer(tr,db,BodyLayer,BodyAci,"Ong gio mem - than (MEPS 022)");var centerLayer=EnsureLayer(tr,db,CenterLayer,CenterAci,"Ong gio mem - duong tam BOQ (MEPS 022)");
    var owner=(BlockTableRecord)tr.GetObject(line.OwnerId,OpenMode.ForWrite);var pl=NewBody(db,xs,ys,bs,a.Z,bodyLayer);
    var bodyId=owner.AppendEntity(pl);tr.AddNewlyCreatedDBObject(pl,true);
    Restyle(db,line,centerLayer);
    string asm=Guid.NewGuid().ToString("N");
    var rb=new FlexRecord022{role=Body,assembly=asm,self=pl.Handle.ToString(),peer=line.Handle.ToString(),diameter=diameter,insunits=(int)db.Insunits,unit=unit,fromEnd=fromEnd,cells=n};
    SetEnds(rb.ends,a,b);SetEnds(rb.origin,a,b);
    var rs=new FlexRecord022{role=Spine,assembly=asm,self=rb.peer,peer=rb.self,diameter=diameter,insunits=rb.insunits,unit=unit,fromEnd=fromEnd,cells=n};
    SetEnds(rs.ends,a,b);SetEnds(rs.origin,a,b);Write(pl,rb);Write(line,rs);
    Curve chk;string bad=Verify(tr,pl,rb,out chk);if(bad!=null)throw new InvalidOperationException("Tự kiểm sau khi tạo thất bại ("+bad+"); huỷ, chưa sửa bản vẽ.");
    tr.Commit();return new FlexResult022{body=bodyId,spine=lineId,assembly=asm,cells=n,length=L};}}
  // 029: LWPOLYLINE/ARC/SPLINE. The body is computed (read-only FlexCurve029.Build) BEFORE anything is written; any refusal → nothing written.
  static FlexResult022 CreateCurve(Database db,ObjectId id,double diameter,bool fromEnd){string unit;double metres;FlexGeometry022.Unit((int)db.Insunits,out unit,out metres);
   if(!FlexGeometry022.Finite(diameter)||diameter<=0)throw new ArgumentException("Đường kính phải là số dương hữu hạn.");
   using(var tr=db.TransactionManager.StartTransaction()){var src=(Curve)tr.GetObject(id,OpenMode.ForRead);string type=SourceTypeOf(src);
    var g=Shape(src,diameter,fromEnd);Point3d a=src.StartPoint,b=src.EndPoint;
    EnsureRegApp(tr,db);
    var bodyLayer=EnsureLayer(tr,db,BodyLayer,BodyAci,"Ong gio mem - than (MEPS 022)");var centerLayer=EnsureLayer(tr,db,CenterLayer,CenterAci,"Ong gio mem - duong tam BOQ (MEPS 022)");
    var owner=(BlockTableRecord)tr.GetObject(src.OwnerId,OpenMode.ForWrite);var pl=NewBody(db,g.X,g.Y,g.Bulges,g.Elevation,bodyLayer);
    var bodyId=owner.AppendEntity(pl);tr.AddNewlyCreatedDBObject(pl,true);
    Restyle(db,src,centerLayer);
    string asm=Guid.NewGuid().ToString("N"),source=Source029(type);
    var rb=new FlexRecord022{role=Body,assembly=asm,self=pl.Handle.ToString(),peer=src.Handle.ToString(),diameter=diameter,insunits=(int)db.Insunits,unit=unit,fromEnd=fromEnd,cells=g.Cells,version=Version029,source=source};
    SetEnds(rb.ends,a,b);SetEnds(rb.origin,a,b);
    var rs=new FlexRecord022{role=Spine,assembly=asm,self=rb.peer,peer=rb.self,diameter=diameter,insunits=rb.insunits,unit=unit,fromEnd=fromEnd,cells=g.Cells,version=Version029,source=source};
    SetEnds(rs.ends,a,b);SetEnds(rs.origin,a,b);Write(pl,rb);Write(src,rs);
    Curve chk;string bad=Verify(tr,pl,rb,out chk);if(bad!=null)throw new InvalidOperationException("Tự kiểm sau khi tạo thất bại ("+bad+"); huỷ, chưa sửa bản vẽ.");
    tr.Commit();return new FlexResult022{body=bodyId,spine=id,assembly=asm,cells=g.Cells,length=g.Length};}}
  // ---- 029 explicit update of a curved duct (after its spine was edited, or to change D) ----
  // Resolves an owned 029.1 duct from the selected body OR spine. Null = mutually linked pair (its geometry may be stale); otherwise the refusal text.
  // Refused: no/malformed record, 022.1 (StraightAdvice), clone/forged copy (self ≠ handle), missing/erased peer, any Link failure (units, styles, version,
  // source type, assembly/peer/self, owner), not in Model/Layout, non-standard or locked/off/frozen duct layers.
  static string Resolve(Transaction tr,Database db,ObjectId selectedId,out DbPolyline body,out FlexRecord022 br,out Curve spine){body=null;br=null;spine=null;
   var en=tr.GetObject(selectedId,OpenMode.ForRead) as Entity;if(en==null||en.IsErased)return "Không tìm thấy đối tượng đã chọn.";
   FlexRecord022 r;try{r=Read(en);}catch(System.Exception ex){return "Dữ liệu "+AppName+" hỏng ("+ex.Message+"); MEPS không ghi đè.";}
   if(r==null)return "Đối tượng này không phải ống gió mềm MEPS.";if(r.version==Version)return StraightAdvice;
   if(r.self!=en.Handle.ToString())return "Đối tượng là bản sao/được chèn của một ống (handle "+en.Handle+" ≠ "+r.self+"); MEPS không cập nhật bản sao. Chưa sửa bản vẽ.";
   if(r.role==Body)body=en as DbPolyline;
   else{var bid=ByHandle(db,r.peer);if(bid.IsNull||bid.IsErased)return "Thân ống "+r.peer+" đã mất; không cập nhật. Chưa sửa bản vẽ.";body=tr.GetObject(bid,OpenMode.ForRead) as DbPolyline;}
   if(body==null)return "Thân ống không hợp lệ; không cập nhật. Chưa sửa bản vẽ.";
   try{br=Read(body);}catch(System.Exception ex){return "Dữ liệu thân ống hỏng ("+ex.Message+"); không cập nhật.";}
   if(br==null||br.version!=Version029)return "Thân ống không mang bản ghi "+Version029+"; không cập nhật.";
   string why=Link(tr,body,br,out spine);if(why!=null)return "Liên kết ống không hợp lệ ("+why+"); MEPS không cập nhật. Chưa sửa bản vẽ.";
   if(r.role==Spine&&spine.ObjectId!=en.ObjectId)return "Đường tâm và thân ống không trỏ về nhau; không cập nhật. Chưa sửa bản vẽ.";
   var owner=tr.GetObject(body.OwnerId,OpenMode.ForRead) as BlockTableRecord;if(owner==null||!owner.IsLayout)return "Ống phải nằm trực tiếp trong Model/Layout.";
   return CheckLayer(tr,db,BodyLayer,BodyAci)??CheckLayer(tr,db,CenterLayer,CenterAci);}
  // Rebuilds the body of an owned 029.1 duct from its CURRENT spine with diameter D: the same body object (handle) gets the new vertices/elevation, both
  // records get D, cell count and current ends (assembly, handles, direction, source, origin kept). One transaction, self-verified before commit; any refusal
  // or failure → nothing written. Repeating it creates no entity.
  public static FlexResult022 UpdateCurve(Database db,ObjectId selectedId,double diameter){
   if(!FlexGeometry022.Finite(diameter)||diameter<=0)throw new ArgumentException("Đường kính phải là số dương hữu hạn. Chưa sửa bản vẽ.");
   string un;double m;if(!FlexGeometry022.Unit((int)db.Insunits,out un,out m))throw new InvalidOperationException("Đơn vị bản vẽ INSUNITS = "+(int)db.Insunits+" chưa hỗ trợ; không cập nhật.");
   using(var tr=db.TransactionManager.StartTransaction()){DbPolyline body;FlexRecord022 br;Curve spine;string why=Resolve(tr,db,selectedId,out body,out br,out spine);if(why!=null)throw new InvalidOperationException(why);
    var g=Shape(spine,diameter,br.fromEnd);FlexRecord022 sr=Read(spine);
    body.UpgradeOpen();Apply(body,g.X,g.Y,g.Bulges);body.Elevation=g.Elevation;
    br.diameter=diameter;br.cells=g.Cells;SetEnds(br.ends,spine.StartPoint,spine.EndPoint);
    sr.diameter=diameter;sr.cells=g.Cells;SetEnds(sr.ends,spine.StartPoint,spine.EndPoint);
    Write(body,br);spine.UpgradeOpen();Write(spine,sr);
    Curve chk;why=Verify(tr,body,br,out chk);if(why!=null)throw new InvalidOperationException("Tự kiểm sau khi cập nhật thất bại ("+why+"); huỷ, chưa sửa bản vẽ.");
    tr.Commit();return new FlexResult022{body=body.ObjectId,spine=spine.ObjectId,assembly=br.assembly,cells=g.Cells,length=g.Length};}}
  // ---- resize: the ONLY path, used by the grip overrule (FlexGripOverrule022.MoveGripPointsAt) and by GripResize below ----
  // preview (non-resident drag clone): only the clone's outline changes. Resident: full Verify, then spine end(s) and body rewritten together.
  // The offset is projected on the spine axis (lengthen/shorten only; direction and the other end stay). Refusal → nothing written. 022.1 (LINE) only.
  internal static string MoveEnds(DbPolyline body,bool moveStart,bool moveEnd,Vector3d offset,bool resident){
   if(!FlexGeometry022.Finite(offset.X)||!FlexGeometry022.Finite(offset.Y)||!FlexGeometry022.Finite(offset.Z))return "Độ dời không hữu hạn; không đổi.";
   if(!moveStart&&!moveEnd)return "Không có grip đầu mút.";FlexRecord022 r;Point3d s,e;if(!GripEnds(body,out r,out s,out e))return "Thân ống không hợp lệ hoặc là bản sao; không kéo.";
   if(!resident)return Regenerate(body,r,s,e,moveStart,moveEnd,offset,null);
   var db=body.Database;if((int)db.Insunits!=r.insunits)return "INSUNITS đã đổi sau khi tạo ống; không kéo.";
   using(var tr=db.TransactionManager.StartOpenCloseTransaction()){Curve cs;string why=Verify(tr,body,r,out cs);var spine=cs as Line;if(why==null&&spine==null)why="SPINE_NOT_LINE";
    if(why!=null)return "Ống không còn liên kết hợp lệ ("+why+"); không kéo.";
    var sl=(LayerTableRecord)tr.GetObject(spine.LayerId,OpenMode.ForRead);if(sl.IsLocked)return "Layer "+sl.Name+" của đường tâm đang khoá; không kéo.";
    why=Regenerate(body,r,spine.StartPoint,spine.EndPoint,moveStart,moveEnd,offset,spine);if(why!=null)return why;tr.Commit();return null;}}
  static string Regenerate(DbPolyline body,FlexRecord022 r,Point3d s,Point3d e,bool moveStart,bool moveEnd,Vector3d offset,Line spine){
   Vector3d axis=new Vector3d(e.X-s.X,e.Y-s.Y,0);double L=axis.Length;if(!(L>0))return "Đường tâm suy biến; không kéo.";Vector3d u=axis/L;double d=offset.X*u.X+offset.Y*u.Y;
   Point3d ns=moveStart?s+u*d:s,ne=moveEnd?e+u*d:e;double nl=(ne.X-ns.X)*u.X+(ne.Y-ns.Y)*u.Y;
   if(!(nl>0))return "Kéo quá đầu mút còn lại; giữ nguyên.";string why=FlexGeometry022.Check(nl,r.diameter);if(why!=null)return why+" Giữ nguyên.";
   double ox,oy,tx,ty;double[] xs,ys,bs;Orient(r.fromEnd,ns.X,ns.Y,ne.X,ne.Y,out ox,out oy,out tx,out ty);int n=FlexGeometry022.Build(ox,oy,tx,ty,r.diameter,out xs,out ys,out bs);
   if(spine!=null){spine.UpgradeOpen();spine.StartPoint=new Point3d(ns.X,ns.Y,s.Z);spine.EndPoint=new Point3d(ne.X,ne.Y,e.Z);
    FlexRecord022 sr=Read(spine);sr.cells=n;SetEnds(sr.ends,spine.StartPoint,spine.EndPoint);Write(spine,sr);r.cells=n;SetEnds(r.ends,spine.StartPoint,spine.EndPoint);}
   Apply(body,xs,ys,bs);
   // XData is rewritten only on the resident body (with its spine); the drag clone gets the outline only.
   if(spine!=null)Write(body,r);return null;}
  // QA entry: runs the overrule's own GetGripPoints + MoveGripPointsAt on the resident body (same code the CAD grip edit calls). end 0 = LINE start,
  // 1 = LINE end, 2 = both (moves the duct along its axis). Returns null on success or the refusal text. Undo evidence must be taken separately.
  public static string GripResize(Database db,ObjectId bodyId,int end,Vector3d offset){Initialize();
   using(var tr=db.TransactionManager.StartTransaction()){var b=tr.GetObject(bodyId,OpenMode.ForWrite) as DbPolyline;if(b==null)return "NOT_BODY";
    var grips=new GripDataCollection();FlexGripOverrule022.Instance.GetGripPoints(b,grips,1,5,Vector3d.ZAxis,GetGripPointsFlags.GripPointsOnly);
    if(grips.Count!=2)return "Thân ống không hợp lệ hoặc là bản sao; không có grip đầu mút.";var hot=new GripDataCollection();
    for(int i=0;i<grips.Count;i++)if(end==2||end==i)hot.Add(grips[i]);if(hot.Count==0)return "Grip không hợp lệ.";
    LastResult="NO_CALL: MoveGripPointsAt did not reach MoveEnds";FlexGripOverrule022.Instance.MoveGripPointsAt(b,hot,offset,(MoveGripPointsFlags)0);string why=LastResult;if(why==null)tr.Commit();return why;}}
  // ---- grip diagnostics (pilot evidence) ----
  public static int PreviewMoves,ResidentMoves,ResidentRefusals;public static string LastResult;
  internal static void Note(bool resident,string why){if(resident){ResidentMoves++;LastResult=why;if(why!=null){ResidentRefusals++;try{var d=App.DocumentManager.MdiActiveDocument;if(d!=null)d.Editor.WriteMessage("\nMEPS Ống gió mềm: "+why);}catch{}}}else PreviewMoves++;}
  // ---- BOQ read (read-only; counts each spine at most once, through its verified body) ----
  public static FlexBoqReport022 ReadBoq(Database db){var rep=new FlexBoqReport022();var bodies=new List<Tuple<DbPolyline,FlexRecord022>>();var spines=new List<Tuple<Entity,FlexRecord022>>();
   using(var tr=db.TransactionManager.StartOpenCloseTransaction()){var bt=(BlockTable)tr.GetObject(db.BlockTableId,OpenMode.ForRead);
    foreach(ObjectId bid in bt){var btr=(BlockTableRecord)tr.GetObject(bid,OpenMode.ForRead);if(!btr.IsLayout)continue;
     foreach(ObjectId id in btr){var en=tr.GetObject(id,OpenMode.ForRead) as Entity;if(en==null)continue;FlexRecord022 r;
      try{r=Read(en);}catch(System.Exception ex){rep.items.Add(new FlexBoqItem022{body=en.Handle.ToString(),status=Unverified,reason="MALFORMED: "+ex.Message});continue;}if(r==null)continue;
      var pl=en as DbPolyline;if(r.role==Body&&pl!=null)bodies.Add(Tuple.Create(pl,r));else if(r.role==Spine)spines.Add(Tuple.Create(en,r));
      else rep.items.Add(new FlexBoqItem022{assembly=r.assembly,body=en.Handle.ToString(),status=Unverified,reason="BODY_NOT_POLYLINE"});}}
    // Duplicate identity among self-consistent members (clones are reported CLONED by Verify and not counted here).
    var own=bodies.Where(x=>x.Item2.self==x.Item1.Handle.ToString()).GroupBy(x=>x.Item2.assembly).ToDictionary(g=>g.Key,g=>g.Count());
    var ownSp=spines.Where(x=>x.Item2.self==x.Item1.Handle.ToString()).GroupBy(x=>x.Item2.assembly).ToDictionary(g=>g.Key,g=>g.Count());
    var usedSpines=new HashSet<string>();
    foreach(var x in bodies){var r=x.Item2;var it=new FlexBoqItem022{assembly=r.assembly,body=x.Item1.Handle.ToString(),spine=r.peer,diameter=r.diameter,unit=r.unit,insunits=r.insunits,sourceType=r.SourceType};Curve sp;
     string why=Verify(tr,x.Item1,r,out sp);int c;if(why==null&&((own.TryGetValue(r.assembly,out c)&&c>1)||(ownSp.TryGetValue(r.assembly,out c)&&c>1)))why="DUPLICATE_IDENTITY: mã ống "+r.assembly+" xuất hiện nhiều lần";
     if(why==null&&!usedSpines.Add(sp.Handle.ToString()))why="SPINE_SHARED: đường tâm đã được tính cho ống khác";
     if(sp!=null&&!sp.IsErased){it.length=SpineLength(sp,r);string un;double m;if(FlexGeometry022.Unit(r.insunits,out un,out m))it.metres=it.length*m;}
     it.status=why==null?Verified:Unverified;it.reason=why;rep.items.Add(it);}
    var bodyAsm=new HashSet<string>(bodies.Select(x=>x.Item2.assembly));
    foreach(var x in spines){if(bodyAsm.Contains(x.Item2.assembly))continue;rep.items.Add(new FlexBoqItem022{assembly=x.Item2.assembly,spine=x.Item1.Handle.ToString(),body=x.Item2.peer,diameter=x.Item2.diameter,unit=x.Item2.unit,insunits=x.Item2.insunits,sourceType=x.Item2.SourceType,status=Unverified,reason="ORPHAN_SPINE: thân ống đã mất/không hợp lệ"});}}
   foreach(var g in rep.items.Where(i=>i.status==Verified).GroupBy(i=>i.Key))
    rep.totals.Add(new FlexBoqTotal022{key=g.Key,diameter=g.First().diameter,unit=g.First().unit,count=g.Count(),length=g.Sum(i=>i.length),metres=g.Sum(i=>i.metres)});
   return rep;}
  // ---- registration (no database access; safe at load and for every reopened drawing) ----
  static bool registered;
  public static void Initialize(){if(registered)return;Overrule.AddOverrule(RXObject.GetClass(typeof(DbPolyline)),FlexGripOverrule022.Instance,false);
   Overrule.AddOverrule(RXObject.GetClass(typeof(Line)),FlexGripOverrule022.Instance,false);Overrule.Overruling=true;registered=true;}
  public static void Terminate(){if(!registered)return;try{Overrule.RemoveOverrule(RXObject.GetClass(typeof(DbPolyline)),FlexGripOverrule022.Instance);Overrule.RemoveOverrule(RXObject.GetClass(typeof(Line)),FlexGripOverrule022.Instance);}catch{}registered=false;}
  // ---- palette entry (future button) and interactive command ----
  sealed class Request {public Document doc;public int units;public Matrix3d ucs;}
  static Request pending;static Document watched;static bool hooked,running;static double lastDiameter;
  // Palette integration (023): pending or prompting; OtherBusy = the host's placement/wire/model jobs (set by Plugin), checked by Queue.
  public static bool Busy{get{return pending!=null||running;}}
  public static Func<bool> OtherBusy;
  static void Context(Request r){var d=App.DocumentManager.MdiActiveDocument;if(r.doc!=d||(int)d.Database.Insunits!=r.units||!d.Editor.CurrentUserCoordinateSystem.IsEqualTo(r.ucs))throw new InvalidOperationException("Bản vẽ/UCS/đơn vị đã đổi sau khi bấm; bấm lại. Chưa sửa bản vẽ.");
   if(!d.Database.TileMode)throw new InvalidOperationException("Bản thí điểm chỉ chạy trong Model. Chưa sửa bản vẽ.");if((Convert.ToInt32(App.GetSystemVariable("UNDOCTL"))&1)==0)throw new InvalidOperationException("UNDO đang tắt; chưa sửa bản vẽ.");}
  static Request NewRequest(){var d=App.DocumentManager.MdiActiveDocument;if(d==null)throw new InvalidOperationException("Mở bản vẽ trước.");return new Request{doc=d,units=(int)d.Database.Insunits,ucs=d.Editor.CurrentUserCoordinateSystem};}
  static void Drop(){pending=null;if(watched!=null){try{watched.CommandWillStart-=OnCommand;}catch{}watched=null;}}
  static void OnCommand(object s,CommandEventArgs e){if(pending!=null&&!string.Equals(e.GlobalCommandName,"TVXDFLEX022RUN",StringComparison.OrdinalIgnoreCase))Drop();}
  // Binds the active document, INSUNITS and UCS to one request and dispatches TVXDFLEX022RUN; another command first, a document switch or close drops it.
  public static void Queue(){var d=App.DocumentManager.MdiActiveDocument;if(d==null)throw new InvalidOperationException("Mở bản vẽ trước.");if(!string.IsNullOrEmpty(d.CommandInProgress)||Busy||(OtherBusy!=null&&OtherBusy()))throw new InvalidOperationException("Hoàn tất lệnh đang chạy hoặc nhấn Esc trước.");
   Initialize();var r=NewRequest();Context(r);
   if(!hooked){hooked=true;App.DocumentManager.DocumentActivated+=(s,e)=>{if(pending!=null&&pending.doc!=e.Document)Drop();};App.DocumentManager.DocumentToBeDestroyed+=(s,e)=>{if(pending!=null&&pending.doc==e.Document)Drop();};}
   pending=r;watched=d;d.CommandWillStart+=OnCommand;try{d.SendStringToExecute("TVXDFLEX022RUN ",true,false,false);}catch{Drop();throw;}}
  internal static void RunPending(){var r=pending;Drop();if(r==null){Say("Không có yêu cầu Ống gió mềm đang chờ.");return;}Interactive(r);}
  internal static void RunQa(){Interactive(null);}
  static string F(double x){return x.ToString("0.###",Inv);}
  // Read-only: the record of the picked entity (null = not owned), or a refusal text for malformed XData.
  static string Owned(Database db,ObjectId id,out FlexRecord022 own){own=null;using(var tr=db.TransactionManager.StartOpenCloseTransaction()){var en=tr.GetObject(id,OpenMode.ForRead) as Entity;if(en==null)return "Không tìm thấy đối tượng đã chọn.";
    try{own=Read(en);}catch(System.Exception ex){return "Đối tượng có dữ liệu "+AppName+" hỏng ("+ex.Message+"); MEPS không ghi đè.";}return null;}}
  // One palette button, one prompt: a free LINE/PLINE/ARC/SPLINE → create; an owned 029.1 body or spine → update (default D = current); an owned 022.1 →
  // StraightAdvice (no second duct). Every write goes through Create/UpdateCurve (one transaction each).
  static void Interactive(Request r){if(running){Say("Đang chạy Ống gió mềm; hoàn tất hoặc Esc trước.");return;}running=true;try{Initialize();if(r==null)r=NewRequest();Context(r);var ed=r.doc.Editor;var db=r.doc.Database;
    var po=new PromptEntityOptions("\nChọn đường tâm LINE/PLINE/ARC/SPLINE để tạo ống gió mềm, hoặc chọn ống đã tạo để cập nhật (Esc huỷ): ");po.SetRejectMessage("\nChỉ chọn LINE, PLINE (LWPOLYLINE), ARC hoặc SPLINE.");
    po.AddAllowedClass(typeof(Line),true);po.AddAllowedClass(typeof(DbPolyline),true);po.AddAllowedClass(typeof(Arc),true);po.AddAllowedClass(typeof(Spline),true);
    // 023: focus this bound document's drawing window (public Document.Window.Focus) after the context check; a failure does not cancel the prompt.
    try{r.doc.Window.Focus();}catch{}
    var picked=ed.GetEntity(po);if(picked.Status!=PromptStatus.OK){Say("Đã huỷ; bản vẽ giữ nguyên.");return;}Context(r);
    FlexRecord022 own;string why=Owned(db,picked.ObjectId,out own);if(why!=null){Say(why);return;}
    if(own!=null){UpdateInteractive(r,picked.ObjectId,own);return;}
    double L;why=Inspect(db,picked.ObjectId,out L);if(why!=null){Say(why);return;}
    string unit;double m;FlexGeometry022.Unit(r.units,out unit,out m);string type;bool fromEnd;
    using(var tr=db.TransactionManager.StartOpenCloseTransaction()){var cv=(Curve)tr.GetObject(picked.ObjectId,OpenMode.ForRead);type=SourceTypeOf(cv);var p=cv.GetClosestPointTo(picked.PickedPoint.TransformBy(r.ucs),false);
     var line=cv as Line;if(line!=null)fromEnd=p.DistanceTo(line.EndPoint)<p.DistanceTo(line.StartPoint);
     else{try{fromEnd=cv.GetDistAtPoint(p)-cv.GetDistanceAtParameter(cv.StartParam)>L/2;}catch{fromEnd=false;}}}
    var pd=new PromptDoubleOptions("\nĐường kính ống ("+unit+" — đơn vị bản vẽ hiện hành, MEPS không quy đổi; "+type+" dài "+F(L)+" "+unit+"): "){AllowNegative=false,AllowZero=false,AllowNone=lastDiameter>0};
    if(lastDiameter>0){pd.DefaultValue=lastDiameter;pd.UseDefaultValue=true;}var got=ed.GetDouble(pd);if(got.Status!=PromptStatus.OK){Say("Đã huỷ; bản vẽ giữ nguyên.");return;}Context(r);
    if(type=="LINE"){why=FlexGeometry022.Check(L,got.Value);if(why!=null){Say(why+" Chưa sửa bản vẽ.");return;}}
    var res=Create(db,picked.ObjectId,got.Value,fromEnd);lastDiameter=got.Value;
    if(type=="LINE")Say("Đã tạo ống gió mềm D="+F(got.Value)+" "+unit+", dài "+F(L)+" "+unit+" ("+res.cells+" nếp). Chọn thân ống, kéo grip đầu mút để dài/ngắn THEO PHƯƠNG TUYẾN (thẳng hàng; kéo lệch hướng chỉ tính phần dọc tuyến). UNDO hoàn tác.");
    else Say("Đã tạo ống gió mềm theo "+type+" D="+F(got.Value)+" "+unit+", dài "+F(res.length)+" "+unit+" theo đường tâm ("+res.cells+" nếp). Đường tâm gốc giữ nguyên hình, thành nét tâm mờ. Đổi tuyến: sửa đường tâm bằng grip của nó, rồi bấm lại nút Ống gió mềm và chọn ống để cập nhật; trước khi cập nhật, ống không được cộng khối lượng. UNDO hoàn tác.");}
   catch(System.Exception ex){Say(ex.Message);}finally{running=false;}}
  static void UpdateInteractive(Request r,ObjectId id,FlexRecord022 own){if(own.version==Version){Say(StraightAdvice);return;}
   var db=r.doc.Database;var ed=r.doc.Editor;double cur,L;string type,state;
   using(var tr=db.TransactionManager.StartOpenCloseTransaction()){DbPolyline b;FlexRecord022 br;Curve sp;string why=Resolve(tr,db,id,out b,out br,out sp);if(why!=null){Say(why);return;}
    cur=br.diameter;type=br.SourceType;L=SpineLength(sp,br);string stale=Geometry(b,br,sp);state=stale==null?"thân đang khớp đường tâm":"cần cập nhật — "+stale;}
   string unit;double m;FlexGeometry022.Unit(r.units,out unit,out m);
   var pd=new PromptDoubleOptions("\nCập nhật ống gió mềm theo "+type+" (đường tâm dài "+F(L)+" "+unit+"; "+state+"). Đường kính ("+unit+" — đơn vị bản vẽ hiện hành): "){AllowNegative=false,AllowZero=false,AllowNone=true,DefaultValue=cur,UseDefaultValue=true};
   var got=ed.GetDouble(pd);if(got.Status!=PromptStatus.OK){Say("Đã huỷ cập nhật; bản vẽ giữ nguyên.");return;}Context(r);
   var res=UpdateCurve(db,id,got.Value);lastDiameter=got.Value;
   Say("Đã cập nhật ống gió mềm theo "+type+": D="+F(got.Value)+" "+unit+", dài "+F(res.length)+" "+unit+" theo đường tâm hiện tại ("+res.cells+" nếp). Thân và đường tâm giữ nguyên handle, không tạo đối tượng mới. UNDO hoàn tác.");}
  internal static void Say(string s){try{var d=App.DocumentManager.MdiActiveDocument;if(d!=null)d.Editor.WriteMessage("\nMEPS Ống gió mềm: "+s);}catch{}}
 }
 // Grip overrule restricted by XData: owned straight (022.1) bodies get two spine-end grips only; owned spine LINEs get no grips (they move with the body).
 // 029: curved (029.1) bodies get no grips; a 029.1 LWPOLYLINE SPINE keeps its native grips/stretch (every call defers to base). Arc/Spline are not overruled.
 public sealed class FlexGripOverrule022:GripOverrule {
  public static readonly FlexGripOverrule022 Instance=new FlexGripOverrule022();
  sealed class EndGrip:GripData {public readonly int End;public EndGrip(int end,Point3d p){End=end;GripPoint=p;}}
  FlexGripOverrule022(){SetCustomFilter();}
  public override bool IsApplicable(RXObject subject){try{var en=subject as Entity;if(!(en is DbPolyline)&&!(en is Line))return false;using(var rb=en.GetXDataForApplication(FlexDuct022.AppName))return rb!=null;}catch{return false;}}
  static bool CurvedSpine(Entity en){try{var r=FlexDuct022.Read(en);return r!=null&&r.role==FlexDuct022.Spine&&r.version==FlexDuct022.Version029;}catch{return false;}}
  public override void GetGripPoints(Entity entity,GripDataCollection grips,double curViewUnitSize,int gripSize,Vector3d curViewDir,GetGripPointsFlags bitFlags){
   if(CurvedSpine(entity)){base.GetGripPoints(entity,grips,curViewUnitSize,gripSize,curViewDir,bitFlags);return;}
   try{var pl=entity as DbPolyline;FlexRecord022 r;Point3d s,e;if(pl!=null&&FlexDuct022.GripEnds(pl,out r,out s,out e)){grips.Add(new EndGrip(0,s));grips.Add(new EndGrip(1,e));}}catch{}
   // Straight spines, curved bodies and malformed/cloned bodies: no grips (native vertex grips would break the corrugation or the link).
  }
  public override void MoveGripPointsAt(Entity entity,GripDataCollection grips,Vector3d offset,MoveGripPointsFlags bitFlags){
   if(CurvedSpine(entity)){base.MoveGripPointsAt(entity,grips,offset,bitFlags);return;}
   var pl=entity as DbPolyline;if(pl==null)return;bool s=false,e=false;
   foreach(GripData g in grips){var eg=g as EndGrip;if(eg==null)continue;if(eg.End==0)s=true;else e=true;}if(!s&&!e)return;
   bool resident=pl.Database!=null&&!pl.ObjectId.IsNull;string why;try{why=FlexDuct022.MoveEnds(pl,s,e,offset,resident);}catch(System.Exception ex){why=ex.Message;}FlexDuct022.Note(resident,why);}
  // STRETCH: no stretch points (a crossing window leaves the duct unchanged; a full window still moves it as a whole). A curved spine stretches natively.
  public override void GetStretchPoints(Entity entity,Point3dCollection stretchPoints){if(CurvedSpine(entity))base.GetStretchPoints(entity,stretchPoints);}
  public override void MoveStretchPointsAt(Entity entity,IntegerCollection indices,Vector3d offset){if(CurvedSpine(entity))base.MoveStretchPointsAt(entity,indices,offset);}
 }
 public sealed class FlexCommands022 {
  // QA-only standalone command (prompts the centerline or an owned duct, then diameter). The palette calls FlexDuct022.Queue() (→ TVXDFLEX022RUN).
  [CommandMethod("TVXDFLEX022",CommandFlags.Modal)]public void Qa(){FlexDuct022.RunQa();}
  [CommandMethod("TVXDFLEX022RUN",CommandFlags.Modal)]public void Run(){FlexDuct022.RunPending();}
  [CommandMethod("TVXDFLEX022BOQ",CommandFlags.Modal)]public void Boq(){var d=App.DocumentManager.MdiActiveDocument;if(d==null)return;FlexDuct022.Initialize();var rep=FlexDuct022.ReadBoq(d.Database);
   foreach(var i in rep.items)d.Editor.WriteMessage("\n"+i.status+" "+i.Key+" "+(i.sourceType??"?")+" L="+FlexGeometry022.R(i.length)+" ("+FlexGeometry022.R(i.metres)+" m) body="+i.body+" spine="+i.spine+(i.reason==null?"":" — "+i.reason));
   foreach(var t in rep.totals)d.Editor.WriteMessage("\nTỔNG "+t.key+": "+t.count+" ống, "+FlexGeometry022.R(t.length)+" "+t.unit+" = "+FlexGeometry022.R(t.metres)+" m");
   d.Editor.WriteMessage("\nUNVERIFIED: "+rep.Unverified+" (không cộng vào tổng).");}
 }
#if FLEX022_STANDALONE||FLEX022
 public sealed class FlexApp022:IExtensionApplication {public void Initialize(){FlexDuct022.Initialize();}public void Terminate(){FlexDuct022.Terminate();}}
#endif
}
