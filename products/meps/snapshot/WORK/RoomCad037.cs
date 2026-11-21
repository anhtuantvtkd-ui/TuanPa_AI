using System;using System.Linq;using System.Collections.Generic;using System.Globalization;using System.Text;using System.Security.Cryptography;using IOE=System.InvalidOperationException;
#if ROOMCAD037
#if !TAG033 || TAG033_PURE || !CIRCUIT032 || CIRCUIT032_PURE || !IDENTITY015 || !CABLES016
#error ROOMCAD037 needs the native TAG033 part (TagCad033.Read), the native CIRCUIT032 part (CircuitCad032), IDENTITY015 and CABLES016 (Identity015.OriginCable).
#endif
using Autodesk.AutoCAD.DatabaseServices;using Autodesk.AutoCAD.Geometry;
namespace Tvxd.Meps {
 // TVXD-MEPS-ROOMQTY-037 NATIVE ADAPTER (read only): one WCS XY rectangle of top-level Model -> RoomInput037 -> RoomQty037.Associate + ResolveControlPaths.
 // ASCII only. Never writes: one OpenClose transaction opened ForRead (TagCad033.Read / CircuitCad032.Scan open their own read transactions); no marking,
 // no Undo, no Save, no layer/style/geometry/record change. Foreign documents, Paper space, xrefs, MINSERT and nested entities are not read as wires/devices.
 //  - Wires: a Model Curve is a CANDIDATE when its layer is a Cable016 layer, or it carries a Cable016 record, or a Cable016 Identity015 record, or a malformed
 //    Identity015 record. Cable016 record / layer / identity codes must agree (Route034 rule); a non-cable identity on a candidate = conflict. Only LINE (start Z =
 //    end Z) and open LWPOLYLINE with normal +Z are measured: native GetDistanceAtParameter at every vertex (bulge arcs included), interval length = difference
 //    of native distances, so the intervals of one source sum to its native length (telescoping). Anything else on a candidate (closed/nonplanar/zero length,
 //    ARC, SPLINE, old 2D/3D polyline, malformed/conflicting metadata) is NOT measured: BLOCKED diagnostic + every node it touches is blocked. Non-candidate
 //    LINE/LWPOLYLINE touching the room are only counted (UNTYPED_CURVES_EXCLUDED); they are never interpreted as walls or wires. Text/dimension ignored.
 //  - Scope: exact bounding box of each measured source (arc extrema included, not endpoint-only). Box inside the rectangle (numerical epsilon) = FULLY_INSIDE;
 //    box touching the rectangle grown by the tolerance = CROSSING (whole source excluded by the core, never clipped). Devices: insertion point inside the rectangle.
 //  - Devices (top-level Model BlockReference, legends and xrefs skipped): (1) valid Identity015 record: must pass CircuitCad032.Scan validation (identity <->
 //    library marker); FROZEN_015 category E-CT SWITCH, E-DEN LAMP, E-OC SOCKET, E-QUAT FAN, E-TU PANEL, any other category OTHER; IMPORTED identity OTHER
 //    (no type inferred). (2) no identity: library marker block name, else effective block name, by exact prefix "E-CT-" SWITCH, "E-DEN-" LAMP, "E-OC-" SOCKET,
 //    "E-QUAT-" FAN, "E-TU-" PANEL (existing MEPS aliases, basis recorded); a marker / circuit tag without such a name = OTHER. Nothing else is classified (no
 //    substring rule); unmarked TVXD_* names are reported, not adopted. TAG = TagCad033.Read only (never the nearest TEXT). panel/circuit = Circuit032 tag only.
 //    Malformed identity/circuit/marker/TAG = CHECK diagnostic and the device node is blocked.
 //  - Ports = source endpoints + device insertion points. Ports within T = max(tolerance, Eps) are one node (XY), Eps = numerical roundoff only =
 //    max(1e-9, 32 * machine epsilon * max |coordinate|) (NF-037-GEOM-001; ~7.1e-6 at 1e9, never a geometric gap bridge); a node whose
 //    ports are farther apart than T (transitive cluster) is flagged and blocked. Node ID = "N:x,y" of its smallest port (x, then y): independent of iteration.
 //    A port within T of the interior of a measured source splits it there (T junction / device on wire / endpoint at a polyline vertex). Interior/interior
 //    crossings are never connected. A port near a source end but not coincident with that end, or near one source at two places = flagged and blocked.
 //    Two measured sources sharing an interval (1/4, 1/2, 3/4 samples all on the other) = WIRE_OVERLAP, not merged, both blocked.
 //  - Blocked nodes reach the core as node-device entries mapping to TaintDevice (an ID that is never a device): the core blocks every component reaching them
 //    (NODE_MAP_UNKNOWN_DEVICE) while other components are still resolved. No connection by bounding box or nearest label; no heights, conductor counts,
 //    conduit sizes or quantities are guessed. physicalLengthMetres of the network is NOT a wire or conduit quantity.
 //  - Limits MaxCurves candidate wires / MaxDevices devices / MaxSegments intervals: exceeding one = BLOCKED, no input/result/network/signature (no partial result).
 //  - signature = SHA-256 of the options, FingerprintGuid, INSUNITS and every collected source/device/untyped/unclassified entity (handle, type, layer, geometry,
 //    raw Cable016/Identity015/Circuit032/library records, TAG state, block definition digest). IsCurrent re-scans with the same options and compares it.
 public sealed class RoomCadDiag037 {public string severity,code,message;public readonly List<string> handles=new List<string>();
  public override string ToString(){return severity+" "+code+" ["+string.Join(",",handles)+"] "+message;}}
 // status: BOUND (node on a measured interval) | UNBOUND. tagState: NONE | TAG:<handle>:<len>:<text> | INVALID:<reason>. basis: why this type was assigned.
 public sealed class RoomCadDevice037 {public string handle,type,name,tagText,tagHandle,tagState,panel,circuit,basis,identityCode,nodeId,status;public double x,y,z;public bool bound,blocked;public ObjectId id;}
 // One measured interval of one source: [fromDistance, toDistance] native distances along the source; length = toDistance - fromDistance (drawing units).
 public sealed class RoomCadSegment037 {public string id,sourceHandle,sourceType,nodeA,nodeB,boundary,cableCode,evidence;public int index;public double fromDistance,toDistance,length,sourceLength;public ObjectId sourceId;}
 // One collected candidate wire. status: ACCEPTED (measured, segments fed to the core) | REJECTED (reason; not measured, touching nodes blocked).
 public sealed class RoomCadSource037 {public string handle,type,layer,cableCode,boundary,status,reason;public double length,minX,minY,maxX,maxY;public bool hasBox;public ObjectId id;
  public readonly List<string> segmentIds=new List<string>();}
 // status: ASSOCIATED | NEEDS_INPUT | CHECK | BLOCKED (never PASS). input/result/network null when options are invalid or a limit was exceeded.
 public sealed class RoomCadScan037 {
  public string roomId,declaredUnits,status,fingerprint,signature;public double connectionTolerance,effectiveTolerance,minX,minY,maxX,maxY;public int insunits,untypedExcluded,unclassifiedBlocks;
  public bool optionsOk,limitExceeded;
  public RoomInput037 input;public RoomResult037 result;public RoomNetwork037 network;
  public readonly List<RoomCadDiag037> diagnostics=new List<RoomCadDiag037>();
  public readonly Dictionary<string,RoomCadDevice037> devices=new Dictionary<string,RoomCadDevice037>(StringComparer.Ordinal);
  public readonly Dictionary<string,RoomCadSegment037> segments=new Dictionary<string,RoomCadSegment037>(StringComparer.Ordinal);
  public readonly List<RoomCadSource037> sources=new List<RoomCadSource037>();
  public readonly SortedDictionary<string,SortedSet<string>> blockedNodes=new SortedDictionary<string,SortedSet<string>>(StringComparer.Ordinal);   // node -> reason codes
  public List<RoomCadDiag037> Diags(string code){return diagnostics.Where(d=>d.code==code).ToList();}
 }
 public static class RoomCad037 {
  public const int MaxCurves=1000,MaxDevices=2000,MaxSegments=10000,MaxDefEntities=2000,MaxDefDepth=4,MaxListed=200;
  public const string TaintDevice="#ADAPTER_BLOCKED_SEE_SCAN_DIAGNOSTICS",Accepted="ACCEPTED",Rejected="REJECTED",Bound="BOUND",Unbound="UNBOUND";
  const string Bl=RoomQty037.Blocked,Ck=RoomQty037.Check,Ni=RoomQty037.NeedsInput,In=RoomQty037.Info;
  static readonly Tolerance Tol=new Tolerance(1e-9,1e-9);
  static readonly CultureInfo Inv=CultureInfo.InvariantCulture;
  static readonly string[][] Aliases={new[]{"E-CT-",RoomQty037.Switch},new[]{"E-DEN-",RoomQty037.Lamp},new[]{"E-OC-",RoomQty037.Socket},new[]{"E-QUAT-",RoomQty037.Fan},new[]{"E-TU-",RoomQty037.Panel}};
  static readonly string[][] Categories={new[]{"E-CT",RoomQty037.Switch},new[]{"E-DEN",RoomQty037.Lamp},new[]{"E-OC",RoomQty037.Socket},new[]{"E-QUAT",RoomQty037.Fan},new[]{"E-TU",RoomQty037.Panel}};

  sealed class Hit {public double along,perp;public string node;}
  sealed class Src {public string handle,type,layer,code,boundary,geom,why;public ObjectId id;public bool bad;public int n;public double z,len,nativeLen,minX,minY,maxX,maxY;
   public double[] vx,vy,bu,d;public string startNode,endNode;public RoomCadSource037 dto;public readonly List<Hit> splits=new List<Hit>();public readonly List<RoomCadSegment037> segs=new List<RoomCadSegment037>();}
  sealed class Foreign {public string handle;public ObjectId id;public bool hasEnds,hasBox;public double sx,sy,sz,ex,ey,ez,minX,minY,maxX,maxY;}
  sealed class Dev {public RoomCadDevice037 dto;public bool bad;}
  sealed class Port {public double x,y,z;public int kind,end;public string owner,node;public Src src;public Foreign fo;public Dev dev;}
  sealed class Ctx {public RoomCadScan037 s;public double tol,T,epsR,scaleR;public bool limit;   // scaleR: max |rectangle coordinate|; epsR = Eps(scaleR)
   public readonly List<Src> srcs=new List<Src>();public readonly List<Foreign> foreign=new List<Foreign>();public readonly List<Dev> devs=new List<Dev>();
   public readonly List<string> sig=new List<string>(),untyped=new List<string>(),unclassified=new List<string>();
   public readonly HashSet<string> reported=new HashSet<string>(StringComparer.Ordinal);
   public Dictionary<string,CircuitItem032> circuit=new Dictionary<string,CircuitItem032>(StringComparer.Ordinal);
   public Dictionary<string,string> category=new Dictionary<string,string>(StringComparer.Ordinal);}

  static bool Fin(double v){return !double.IsNaN(v)&&!double.IsInfinity(v);}
  static string N(double v){return v.ToString("R",Inv);}
  static string P(Point3d p){return N(p.X)+","+N(p.Y)+","+N(p.Z);}
  static double Hyp(double x,double y){return Math.Sqrt(x*x+y*y);}
  // NF-037-GEOM-001: numerical roundoff only, never a geometric tolerance. Bound = max(AbsFloor, RoundOff * |scale|) with RoundOff = 32 x double machine
  // epsilon (2^-52) ~ 7.1e-15: at |coordinate| 1e9 -> ~7.1e-6 drawing units (~60 ulp of 1e9), at 1e6 -> ~7.1e-9, below ~1.4e5 the 1e-9 floor applies.
  // Exactly coincident stored coordinates have distance 0; projections/arc distances carry a few ulp of the coordinate magnitude, well inside the bound.
  // Any larger gap is bridged only by the caller's declared connectionTolerance (T = max(tolerance, Eps)). The former 1e-9 * scale bridged ~1 unit at 1e9.
  const double RoundOff=32*2.220446049250313E-16,AbsFloor=1e-9;
  static double Eps(double scale){return Math.Max(AbsFloor,RoundOff*Math.Abs(scale));}
  // Consistency of two native AutoCAD values of one polyline (last-vertex distance vs Length): unchanged relative 1e-9 check, never stricter than roundoff
  // at the polyline's coordinate magnitude. Not used for connection matching.
  static bool LenAgree(double a,double b,double coordScale){return Math.Abs(a-b)<=Math.Max(1e-9*Math.Max(1.0,Math.Abs(b)),Eps(coordScale));}
  static bool SameZ(double a,double b){return Math.Abs(a-b)<=Eps(Math.Max(Math.Abs(a),Math.Abs(b)));}
  static int Rank(string s){switch(s){case Bl:return 3;case Ck:return 2;case Ni:return 1;default:return 0;}}
  static string Hash(string s){using(var h=SHA256.Create()){var b=h.ComputeHash(Encoding.UTF8.GetBytes(s??""));var sb=new StringBuilder(64);foreach(var x in b)sb.Append(x.ToString("X2",Inv));return sb.ToString();}}
  static string Join(string[] v){if(v==null)return "-";var s=new StringBuilder();s.Append(v.Length);foreach(var x in v)s.Append('|').Append(x==null?-1:x.Length).Append(':').Append(x);return s.ToString();}
  static string Tx(string s){return (s??"").Length+":"+(s??"");}
  static string Listed(List<string> l){return string.Join(",",l.Take(MaxListed))+(l.Count>MaxListed?",... ("+l.Count+" total)":"");}
  static RoomCadDiag037 D(RoomCadScan037 s,string sev,string code,string msg,params string[] handles){var d=new RoomCadDiag037{severity=sev,code=code,message=msg};
   foreach(var h in handles??new string[0])if(h!=null&&!d.handles.Contains(h))d.handles.Add(h);s.diagnostics.Add(d);return d;}
  // Blocks node for the core; report = one diagnostic per (code, node).
  static void Taint(Ctx c,string node,string code,string sev,string msg,params string[] handles){if(node==null)return;SortedSet<string> set;
   if(!c.s.blockedNodes.TryGetValue(node,out set))c.s.blockedNodes[node]=set=new SortedSet<string>(StringComparer.Ordinal);set.Add(code);
   if(msg!=null&&c.reported.Add(code+"\u001f"+node))D(c.s,sev,code,msg+" (node "+node+"); every wire component reaching this node is blocked.",handles);}
  static string[] ReadRaw(Transaction tr,DBObject o,string key){if(o.ExtensionDictionary.IsNull)return null;var d=tr.GetObject(o.ExtensionDictionary,OpenMode.ForRead) as DBDictionary;
   if(d==null||!d.Contains(key))return null;var x=tr.GetObject(d.GetAt(key),OpenMode.ForRead) as Xrecord;if(x==null)throw new IOE("key "+key+" of "+o.Handle+" is not an Xrecord");
   using(var rb=x.Data){return rb==null?new string[0]:rb.AsArray().Select(t=>t.TypeCode==(short)DxfCode.Text?Convert.ToString(t.Value,Inv):null).ToArray();}}
  static bool Ext(Entity e,out double x0,out double y0,out double x1,out double y1){x0=y0=x1=y1=0;
   try{var x=e.GeometricExtents;x0=x.MinPoint.X;y0=x.MinPoint.Y;x1=x.MaxPoint.X;y1=x.MaxPoint.Y;return Fin(x0)&&Fin(y0)&&Fin(x1)&&Fin(y1)&&x1>=x0&&y1>=y0;}
   catch(Autodesk.AutoCAD.Runtime.Exception){return false;}}
  static bool InsideBox(RoomCadScan037 s,double e,double x0,double y0,double x1,double y1){return x0>=s.minX-e&&x1<=s.maxX+e&&y0>=s.minY-e&&y1<=s.maxY+e;}
  static bool TouchBox(RoomCadScan037 s,double g,double x0,double y0,double x1,double y1){return x1>=s.minX-g&&x0<=s.maxX+g&&y1>=s.minY-g&&y0<=s.maxY+g;}

  // ---- public API ----
  // Read-only scan of one room. corner1/corner2: opposite WCS corners (XY, nonzero area). declaredUnits: exactly "MM" or "M", confirmed by the caller (no
  // default; anything else = NEEDS_INPUT UNITS_INVALID in the core). connectionTolerance: finite >= 0 drawing units, geometry matching only (0 = exact within
  // floating-point roundoff Eps, see Eps); it is not a design length and a bridged gap is never added to any length. Never throws for drawing content; invalid options = BLOCKED scan with diagnostics.
  public static RoomCadScan037 Scan(Database db,Point3d corner1,Point3d corner2,string roomId,string declaredUnits,double connectionTolerance){
   var s=new RoomCadScan037{roomId=roomId,declaredUnits=declaredUnits,connectionTolerance=connectionTolerance};
   s.minX=Math.Min(corner1.X,corner2.X);s.maxX=Math.Max(corner1.X,corner2.X);s.minY=Math.Min(corner1.Y,corner2.Y);s.maxY=Math.Max(corner1.Y,corner2.Y);
   bool ok=true;
   if(db==null){D(s,Bl,"DB_NULL","No drawing database; nothing scanned.");ok=false;}
   if(!Fin(corner1.X)||!Fin(corner1.Y)||!Fin(corner2.X)||!Fin(corner2.Y)){D(s,Bl,"RECT_INVALID","Room rectangle corners must be finite WCS coordinates.");ok=false;}
   else if(!(s.maxX-s.minX>0)||!(s.maxY-s.minY>0)){D(s,Bl,"RECT_ZERO_AREA","Room rectangle has zero area ("+N(s.maxX-s.minX)+" x "+N(s.maxY-s.minY)+"); pick two opposite corners.");ok=false;}
   if(!Fin(connectionTolerance)||connectionTolerance<0){D(s,Bl,"TOLERANCE_INVALID","Connection tolerance "+N(connectionTolerance)+" must be a finite value >= 0 in drawing units.");ok=false;}
   if(declaredUnits!=RoomQty037.MM&&declaredUnits!=RoomQty037.M)D(s,Ni,"UNITS_INVALID","Declared unit "+(declaredUnits==null?"(null)":"\""+declaredUnits+"\"")+" is not exactly MM or M; plan metres are not computed.");
   if(db!=null){s.fingerprint=db.FingerprintGuid;s.insunits=(int)db.Insunits;}
   s.optionsOk=ok;if(!ok){Finish(s);return s;}
   UnitCheck(s);
   var c=new Ctx{s=s,tol=connectionTolerance};
   c.scaleR=new[]{s.minX,s.maxX,s.minY,s.maxY}.Max(v=>Math.Abs(v));c.epsR=Eps(c.scaleR);c.T=Math.Max(c.tol,c.epsR);
   try{foreach(var it in CircuitCad032.Scan(db).items)if(it.handle!=null)c.circuit[it.handle]=it;}
   catch(Exception ex){D(s,Bl,"CIRCUIT_SCAN_FAILED","CircuitCad032 validation scan failed ("+ex.Message+"); device identities cannot be validated.");s.optionsOk=false;Finish(s);return s;}
   foreach(var e in Identity015.Codebook)c.category[e.id]=e.category;
   using(var tr=db.TransactionManager.StartOpenCloseTransaction()){
    Collect(tr,db,c);
    if(!c.limit)Topology(tr,c);
    tr.Commit();}
   s.effectiveTolerance=c.T;
   if(c.limit){s.limitExceeded=true;Finish(s);return s;}
   Feed(c);
   s.signature=Signature(c);
   Finish(s);return s;}
  // true only when a fresh scan of the same rectangle/options gives the same signature (same drawing fingerprint, entities, geometry and metadata). Read only.
  public static bool IsCurrent(Database db,RoomCadScan037 scan){if(db==null||scan==null||scan.signature==null)return false;
   var again=Scan(db,new Point3d(scan.minX,scan.minY,0),new Point3d(scan.maxX,scan.maxY,0),scan.roomId,scan.declaredUnits,scan.connectionTolerance);
   return again.signature!=null&&string.Equals(again.signature,scan.signature,StringComparison.Ordinal);}

  static void UnitCheck(RoomCadScan037 s){int want=s.declaredUnits==RoomQty037.MM?4:s.declaredUnits==RoomQty037.M?6:-1;if(want<0)return;
   if(s.insunits==0)D(s,In,"INSUNITS_UNDEFINED","Drawing INSUNITS is 0 (unitless); the declared unit "+s.declaredUnits+" is the caller's confirmation.");
   else if(s.insunits!=want)D(s,Ck,"UNIT_INSUNITS_MISMATCH","Declared unit "+s.declaredUnits+" differs from drawing INSUNITS "+s.insunits+"; the caller must confirm the unit.");}

  // ---- collection ----
  static void Collect(Transaction tr,Database db,Ctx c){var ms=(BlockTableRecord)tr.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db),OpenMode.ForRead);
   var ids=new List<ObjectId>();foreach(ObjectId id in ms)if(!id.IsErased)ids.Add(id);
   foreach(var id in ids){if(c.limit)return;
    try{var e=tr.GetObject(id,OpenMode.ForRead) as Entity;if(e==null)continue;
     var br=e as BlockReference;
     if(br!=null)Block(tr,db,c,br);
     else if(e is DBText||e is MText||e is Dimension||e is Leader||e is MLeader||e is Table)continue;   // annotations (Leader is a Curve): ignored
     else if(e is Curve)Wire(tr,c,(Curve)e);
     else Other(tr,c,e);}
    catch(Exception ex){D(c.s,Bl,"ENTITY_READ_FAILED","Model object "+id.Handle+" could not be read ("+ex.Message+"); the room cannot be proven complete.",id.Handle.ToString());}}
   c.srcs.Sort((a,b)=>string.CompareOrdinal(a.handle,b.handle));c.foreign.Sort((a,b)=>string.CompareOrdinal(a.handle,b.handle));
   c.devs.Sort((a,b)=>string.CompareOrdinal(a.dto.handle,b.dto.handle));c.untyped.Sort(string.CompareOrdinal);c.unclassified.Sort(string.CompareOrdinal);
   foreach(var g in c.srcs)c.s.sources.Add(g.dto);foreach(var d in c.devs)c.s.devices[d.dto.handle]=d.dto;
   c.s.sources.Sort((a,b)=>string.CompareOrdinal(a.handle,b.handle));
   c.s.untypedExcluded=c.untyped.Count;c.s.unclassifiedBlocks=c.unclassified.Count;
   if(c.untyped.Count>0)D(c.s,In,"UNTYPED_CURVES_EXCLUDED",c.untyped.Count+" LINE/LWPOLYLINE object(s) touching the room are on no Cable016 layer and carry no cable record; excluded, not interpreted as walls or wires: "+Listed(c.untyped)+".",c.untyped.Take(MaxListed).ToArray());
   if(c.unclassified.Count>0)D(c.s,In,"BLOCKS_UNCLASSIFIED",c.unclassified.Count+" block reference(s) inside the room are not recognised MEPS devices (no identity, no library marker, no E-CT-/E-DEN-/E-OC-/E-QUAT-/E-TU- name); not used: "+Listed(c.unclassified)+".",c.unclassified.Take(MaxListed).ToArray());}

  static void Limit(Ctx c,string what,int max){if(c.limit)return;c.limit=true;
   D(c.s,Bl,"SCAN_LIMIT_EXCEEDED","More than "+max+" "+what+" in the room; scan stopped, no partial result. Select a smaller room.");}

  static string AliasType(string name){if(name==null)return null;foreach(var a in Aliases)if(name.Length>a[0].Length&&name.StartsWith(a[0],StringComparison.OrdinalIgnoreCase))return a[1];return null;}
  static string CategoryType(string cat){foreach(var a in Categories)if(string.Equals(cat,a[0],StringComparison.Ordinal))return a[1];return RoomQty037.Other;}

  static void Block(Transaction tr,Database db,Ctx c,BlockReference br){
   var b=(BlockTableRecord)tr.GetObject(br.BlockTableRecord,OpenMode.ForRead);
   if(b.IsLayout||b.IsFromExternalReference||b.IsFromOverlayReference||b.IsDependent||LegendPlace011.IsLegend(tr,b))return;
   BlockTableRecord eff=b;
   if(br.IsDynamicBlock&&!br.DynamicBlockTableRecord.IsNull){eff=(BlockTableRecord)tr.GetObject(br.DynamicBlockTableRecord,OpenMode.ForRead);if(LegendPlace011.IsLegend(tr,eff))return;}
   var p=br.Position;string h=br.Handle.ToString(),name=eff.Name;
   if(!Fin(p.X)||!Fin(p.Y)||!Fin(p.Z)){D(c.s,Ck,"BLOCK_POSITION_INVALID","Block "+h+" ("+name+") has a non-finite insertion point; not used.",h);return;}
   if(!InsideBox(c.s,c.epsR,p.X,p.Y,p.X,p.Y))return;
   string[] idRaw=null,cRaw=null,marker=null;IdentityRecord015 ident=null;CircuitTag032 ctag=null;var errs=new List<string>();
   try{idRaw=ReadRaw(tr,br,Identity015.Key);if(idRaw!=null)ident=Identity015.Decode(idRaw);}catch(IOE ex){errs.Add("Identity015 record malformed ("+ex.Message+")");}
   try{cRaw=ReadRaw(tr,br,Circuit032.Key);if(cRaw!=null)ctag=Circuit032.Decode(cRaw);}catch(IOE ex){errs.Add("Circuit032 record malformed ("+ex.Message+")");}
   try{marker=LibraryCad.ReadMarker(tr,b);if(marker==null&&eff.ObjectId!=b.ObjectId)marker=LibraryCad.ReadMarker(tr,eff);}catch(Exception ex){errs.Add("library marker unreadable ("+ex.Message+")");}
   string type=null,basis=null,code=null;
   if(errs.Count>0){type=RoomQty037.Other;basis="INVALID_METADATA";}
   else if(ident!=null){code=ident.code;CircuitItem032 it;c.circuit.TryGetValue(h,out it);
    if(it==null||it.status==Circuit032.Invalid){errs.Add("identity "+ident.code+" fails CircuitCad032 validation ("+(it==null?"not in its scan":it.issue)+")");type=RoomQty037.Other;basis="INVALID_METADATA";}
    else if(ident.origin==Identity015.OriginFrozen){string cat;c.category.TryGetValue(it.entryId??"",out cat);type=CategoryType(cat);basis="IDENTITY015_FROZEN:"+ident.code+"/"+(cat??"?");}
    else{type=RoomQty037.Other;basis="IDENTITY015_IMPORTED:"+ident.code;}}
   else{string alias=marker!=null&&marker.Length>2&&!string.IsNullOrEmpty(marker[2])?marker[2]:name;string at=AliasType(alias);
    if(at!=null){type=at;basis=(marker!=null?"ALIAS_LIBRARY_MARKER:":"ALIAS_BLOCK_NAME:")+alias;}
    else if(ctag!=null||marker!=null){type=RoomQty037.Other;basis=(marker!=null?"LIBRARY_MARKER_UNMAPPED:":"CIRCUIT_TAG_ONLY:")+alias;}}
   if(type==null){c.unclassified.Add(h);c.sig.Add("B|"+h+"|"+Tx(name)+"|"+P(p));
    if(name.StartsWith("TVXD_",StringComparison.OrdinalIgnoreCase))D(c.s,Ck,"BLOCK_CANONICAL_UNMARKED","Block "+h+" ("+name+") looks like a MEPS block but has no identity/library marker; not adopted as a device.",h);
    return;}
   if(br is MInsertBlock){D(c.s,Ck,"DEVICE_MINSERT_UNSUPPORTED","Device candidate "+h+" ("+name+") is a MINSERT; not supported, not used.",h);c.sig.Add("B|"+h+"|MINSERT|"+Tx(name)+"|"+P(p));return;}
   if(c.devs.Count>=MaxDevices){Limit(c,"devices",MaxDevices);return;}
   string tagText=null,tagHandle=null,tagState;
   try{var t=TagCad033.Read(db,br.ObjectId);if(t==null)tagState="NONE";else{tagText=t.text;tagHandle=t.tagHandle;tagState="TAG:"+t.tagHandle+":"+Tx(t.text);}}
   catch(Exception ex){tagState="INVALID:"+ex.Message;errs.Add("TAG033 label unreadable ("+ex.Message+")");}
   var dto=new RoomCadDevice037{handle=h,id=br.ObjectId,type=type,name=name,tagText=tagText,tagHandle=tagHandle,tagState=tagState,basis=basis,identityCode=code,
    panel=ctag==null?null:ctag.panel,circuit=ctag==null?null:ctag.circuit,x=p.X,y=p.Y,z=p.Z};
   var dev=new Dev{dto=dto,bad=errs.Count>0};c.devs.Add(dev);
   if(dev.bad)D(c.s,Ck,"DEVICE_METADATA_INVALID","Device "+h+" ("+name+"): "+string.Join("; ",errs)+"; its wire component is blocked.",h);
   if(type==RoomQty037.Other)D(c.s,Ni,"DEVICE_TYPE_OTHER","Device "+h+" ("+name+", basis "+basis+") has no SWITCH/LAMP/SOCKET/FAN/PANEL type; needs a mapping.",h);
   var def=new StringBuilder();int budget=MaxDefEntities;DefDigest(tr,b,def,0,ref budget);
   c.sig.Add("D|"+h+"|B:"+b.Handle+":"+Tx(b.Name)+"|E:"+eff.Handle+":"+Tx(name)+"|P:"+P(p)+"|M:"+string.Join(",",br.BlockTransform.ToArray().Select(v=>N(v)))+"|LY:"+Tx(br.Layer)+
    "|I:"+Join(idRaw)+"|C:"+Join(cRaw)+"|K:"+Join(marker)+"|TAG:"+Tx(tagState)+"|DEF:"+Hash(def.ToString())+"|TY:"+type+"|"+Tx(basis));}
  // Definition digest (change detection only, not a completeness proof like BN01): entity handle/type/extents, curve ends, nested references recursively.
  static void DefDigest(Transaction tr,BlockTableRecord b,StringBuilder sb,int depth,ref int budget){
   sb.Append('{').Append(b.Handle).Append(':').Append(Tx(b.Name)).Append('@').Append(P(b.Origin));
   if(depth>MaxDefDepth){sb.Append("!DEPTH}");return;}
   foreach(ObjectId eid in b){if(eid.IsErased)continue;if(--budget<0){sb.Append("!BUDGET");break;}
    var e=tr.GetObject(eid,OpenMode.ForRead) as Entity;if(e==null)continue;
    sb.Append('|').Append(e.Handle).Append(':').Append(e.GetType().Name).Append(':').Append(e.Visible?'V':'H');
    double x0,y0,x1,y1;if(Ext(e,out x0,out y0,out x1,out y1))sb.Append(':').Append(N(x0)).Append(',').Append(N(y0)).Append(';').Append(N(x1)).Append(',').Append(N(y1));else sb.Append(":noext");
    var cv=e as Curve;if(cv!=null){try{sb.Append(":S").Append(P(cv.StartPoint)).Append(":E").Append(P(cv.EndPoint));}catch(Autodesk.AutoCAD.Runtime.Exception){sb.Append(":noends");}}
    var tx=e as DBText;if(tx!=null)sb.Append(":T").Append(Tx(tx.TextString));
    var nb=e as BlockReference;if(nb!=null){sb.Append(":M").Append(string.Join(",",nb.BlockTransform.ToArray().Select(v=>N(v))));
     var nr=tr.GetObject(nb.BlockTableRecord,OpenMode.ForRead) as BlockTableRecord;if(nr!=null)DefDigest(tr,nr,sb,depth+1,ref budget);}}
   sb.Append('}');}

  static void Wire(Transaction tr,Ctx c,Curve e){string h=e.Handle.ToString();
   string[] cableRaw=null,idRaw=null;CableSpec016 cable=null;IdentityRecord015 ident=null;var meta=new List<string>();
   try{cableRaw=ReadRaw(tr,e,Cable016.Key);if(cableRaw!=null)cable=Cable016.Decode(cableRaw);}catch(IOE ex){meta.Add("Cable016 record malformed ("+ex.Message+")");if(cableRaw==null)cableRaw=new[]{"\u0000UNREADABLE"};}
   try{idRaw=ReadRaw(tr,e,Identity015.Key);if(idRaw!=null)ident=Identity015.Decode(idRaw);}catch(IOE ex){meta.Add("Identity015 record malformed ("+ex.Message+")");if(idRaw==null)idRaw=new[]{"\u0000UNREADABLE"};}
   var layerSpec=Cable016.Find(e.Layer);
   bool candidate=layerSpec!=null||cableRaw!=null||(ident!=null&&ident.origin==Identity015.OriginCable)||(idRaw!=null&&ident==null);
   var g=Geo(e);double x0,y0,x1,y1;bool box;
   if(g.why==null){x0=g.minX;y0=g.minY;x1=g.maxX;y1=g.maxY;box=true;}else box=Ext(e,out x0,out y0,out x1,out y1);
   double grow=Math.Max(c.tol,c.epsR);
   if(!candidate){if((e is Line||e is Polyline)&&box&&TouchBox(c.s,grow,x0,y0,x1,y1)){c.untyped.Add(h);c.sig.Add("U|"+h);}return;}
   if(box&&!TouchBox(c.s,grow,x0,y0,x1,y1))return;
   if(c.srcs.Count+c.foreign.Count>=MaxCurves){Limit(c,"candidate wires",MaxCurves);return;}
   // Route034 binding rule: cable record, cable layer alias and a Cable016 identity must name one code; any other identity on a candidate = conflict.
   var codes=new List<string>();
   if(cable!=null)codes.Add(cable.code);if(layerSpec!=null)codes.Add(layerSpec.code);
   if(ident!=null){if(ident.origin==Identity015.OriginCable)codes.Add(ident.code);else meta.Add("identity "+ident.code+" ("+ident.kind+"/"+ident.origin+") is not a Cable016 code");}
   if(codes.Distinct(StringComparer.Ordinal).Count()>1)meta.Add("cable codes disagree ("+string.Join(", ",codes)+")");
   string known=codes.Count>0&&codes.Distinct(StringComparer.Ordinal).Count()==1?codes[0]:null;
   string boundary=box&&InsideBox(c.s,c.epsR,x0,y0,x1,y1)?RoomQty037.Inside:RoomQty037.Crossing;
   var dto=new RoomCadSource037{handle=h,id=e.ObjectId,type=g.type,layer=e.Layer,cableCode=known,boundary=boundary,hasBox=box,minX=x0,minY=y0,maxX=x1,maxY=y1,length=g.why==null?g.nativeLen:0};
   c.sig.Add("W|"+h+"|"+g.type+"|"+Tx(e.Layer)+"|"+(g.why==null?g.geom:"X:"+Tx(g.why)+(box?":"+N(x0)+","+N(y0)+";"+N(x1)+","+N(y1):":nobox"))+"|C:"+Join(cableRaw)+"|I:"+Join(idRaw));
   if(g.why==null){g.handle=h;g.id=e.ObjectId;g.layer=e.Layer;g.code=known;g.boundary=boundary;g.dto=dto;
    if(meta.Count>0){g.bad=true;dto.status=Rejected;dto.reason=string.Join("; ",meta);
     D(c.s,Bl,"WIRE_METADATA_INVALID","Candidate wire "+h+" (layer "+e.Layer+"): "+dto.reason+"; not measured, components touching it are blocked.",h);}
    else dto.status=Accepted;
    c.srcs.Add(g);return;}
   dto.status=Rejected;dto.reason=g.why+(meta.Count>0?"; "+string.Join("; ",meta):"");
   var f=new Foreign{handle=h,id=e.ObjectId,hasBox=box,minX=x0,minY=y0,maxX=x1,maxY=y1};
   try{Point3d a=e.StartPoint,b=e.EndPoint;if(Fin(a.X)&&Fin(a.Y)&&Fin(b.X)&&Fin(b.Y)){f.sx=a.X;f.sy=a.Y;f.sz=a.Z;f.ex=b.X;f.ey=b.Y;f.ez=b.Z;f.hasEnds=true;}}catch(Autodesk.AutoCAD.Runtime.Exception){}
   c.foreign.Add(f);c.s.sources.Add(dto);
   D(c.s,Bl,"WIRE_UNSUPPORTED","Candidate wire "+h+" ("+g.type+", layer "+e.Layer+"): "+dto.reason+"; not measured, components touching it are blocked.",h);}
  static void Other(Transaction tr,Ctx c,Entity e){bool cab=Cable016.IsCable(e.Layer);
   if(!cab){try{cab=ReadRaw(tr,e,Cable016.Key)!=null;}catch(IOE){cab=true;}}
   if(!cab)return;double x0,y0,x1,y1;bool box=Ext(e,out x0,out y0,out x1,out y1);
   if(box&&!TouchBox(c.s,Math.Max(c.tol,c.epsR),x0,y0,x1,y1))return;string h=e.Handle.ToString();
   c.sig.Add("O|"+h+"|"+e.GetType().Name+"|"+Tx(e.Layer)+(box?"|"+N(x0)+","+N(y0)+";"+N(x1)+","+N(y1):"|nobox"));
   D(c.s,Ck,"CABLE_LAYER_UNSUPPORTED_ENTITY","Object "+h+" ("+e.GetType().Name+") on cable layer "+e.Layer+" touches the room but is not a wire curve; not measured.",h);}

  // ---- geometry (LINE / open planar LWPOLYLINE) ----
  static Src Why(Src g,string why){g.why=why;return g;}
  static Src Geo(Curve e){var g=new Src{type=e.GetType().Name};var ln=e as Line;var pl=e as Polyline;var sb=new StringBuilder();
   try{
    if(ln!=null){g.type="LINE";Point3d a=ln.StartPoint,b=ln.EndPoint;
     if(!Fin(a.X)||!Fin(a.Y)||!Fin(a.Z)||!Fin(b.X)||!Fin(b.Y)||!Fin(b.Z))return Why(g,"LINE has non-finite coordinates");
     if(!SameZ(a.Z,b.Z))return Why(g,"LINE is not planar in XY (start Z "+N(a.Z)+" != end Z "+N(b.Z)+")");
     g.n=2;g.vx=new[]{a.X,b.X};g.vy=new[]{a.Y,b.Y};g.bu=new[]{0.0,0.0};g.z=a.Z;g.nativeLen=ln.Length;g.d=new[]{0.0,g.nativeLen};
     sb.Append("LINE|").Append(P(a)).Append('|').Append(P(b));}
    else if(pl!=null){g.type="LWPOLYLINE";int n=pl.NumberOfVertices;
     if(pl.Closed)return Why(g,"closed LWPOLYLINE is not an open wire route");
     if(n<2)return Why(g,"LWPOLYLINE has "+n+" vertex(es)");
     if(!pl.Normal.IsEqualTo(Vector3d.ZAxis,Tol))return Why(g,"LWPOLYLINE is not planar in WCS XY (normal "+N(pl.Normal.X)+","+N(pl.Normal.Y)+","+N(pl.Normal.Z)+")");
     if(!Fin(pl.Elevation))return Why(g,"LWPOLYLINE elevation is not finite");
     g.n=n;g.vx=new double[n];g.vy=new double[n];g.bu=new double[n];g.d=new double[n];g.z=pl.Elevation;
     sb.Append("LWPOLYLINE|").Append(n).Append('|').Append(N(pl.Elevation));
     for(int i=0;i<n;i++){var q=pl.GetPoint2dAt(i);double bu=pl.GetBulgeAt(i),d=pl.GetDistanceAtParameter(i);
      if(!Fin(q.X)||!Fin(q.Y)||!Fin(bu)||!Fin(d))return Why(g,"LWPOLYLINE vertex "+i+" has a non-finite value");
      if(i>0&&d<g.d[i-1])return Why(g,"LWPOLYLINE native distance decreases at vertex "+i);
      g.vx[i]=q.X;g.vy[i]=q.Y;g.bu[i]=bu;g.d[i]=d;sb.Append("|V").Append(i).Append(':').Append(N(q.X)).Append(',').Append(N(q.Y)).Append(",b").Append(N(bu));}
     g.nativeLen=pl.Length;
     double vs=0;for(int i=0;i<n;i++)vs=Math.Max(vs,Math.Max(Math.Abs(g.vx[i]),Math.Abs(g.vy[i])));
     if(!LenAgree(g.d[n-1],g.nativeLen,vs))return Why(g,"LWPOLYLINE native distance at the last vertex "+N(g.d[n-1])+" != Length "+N(g.nativeLen));}
    else return Why(g,"entity type "+e.GetType().Name+" is not LINE or LWPOLYLINE (ARC, SPLINE, old 2D/3D polyline ... are not measured)");}
   catch(Autodesk.AutoCAD.Runtime.Exception ex){return Why(g,"geometry unreadable ("+ex.Message+")");}
   g.len=g.d[g.n-1];
   if(!Fin(g.len)||!(g.len>0)||!Fin(g.nativeLen)||!(g.nativeLen>0))return Why(g,"length "+N(g.nativeLen)+" is not a positive finite value");
   Box(g);g.geom=sb.ToString();return g;}
  // Exact arc of sub-segment i (bulge b, CCW for b > 0): centre, radius, start angle a0, signed sweep th = 4 atan(b). false = straight.
  static bool Arc(Src g,int i,out double cx,out double cy,out double r,out double a0,out double th){cx=cy=r=a0=th=0;double b=g.bu[i];if(Math.Abs(b)<1e-9)return false;
   double ax=g.vx[i],ay=g.vy[i],bx=g.vx[i+1],by=g.vy[i+1],dx=bx-ax,dy=by-ay,ch=Hyp(dx,dy);if(!(ch>0))return false;
   double h=ch*(1-b*b)/(4*b);cx=(ax+bx)/2-dy/ch*h;cy=(ay+by)/2+dx/ch*h;r=Math.Abs(ch*(1+b*b)/(4*b));th=4*Math.Atan(b);a0=Math.Atan2(ay-cy,ax-cx);return true;}
  static double Mod2Pi(double a){double t=2*Math.PI;a%=t;if(a<0)a+=t;return a;}
  static bool InSweep(double a0,double th,double ang,out double delta){delta=th>0?Mod2Pi(ang-a0):Mod2Pi(a0-ang);return delta<=Math.Abs(th)+1e-12;}
  static void Box(Src g){g.minX=g.maxX=g.vx[0];g.minY=g.maxY=g.vy[0];
   for(int i=0;i<g.n;i++){Grow(g,g.vx[i],g.vy[i]);if(i==g.n-1)break;double cx,cy,r,a0,th,dl;
    if(!Arc(g,i,out cx,out cy,out r,out a0,out th))continue;
    for(int k=0;k<4;k++){double ang=k*Math.PI/2;if(InSweep(a0,th,ang,out dl))Grow(g,cx+r*Math.Cos(ang),cy+r*Math.Sin(ang));}}}
  static void Grow(Src g,double x,double y){if(x<g.minX)g.minX=x;if(x>g.maxX)g.maxX=x;if(y<g.minY)g.minY=y;if(y>g.maxY)g.maxY=y;}
  // Nearest point of sub-segment i to (px,py): native distance along the source and perpendicular distance.
  static void Sub(Src g,int i,double px,double py,out double along,out double perp){double L=g.d[i+1]-g.d[i],ax=g.vx[i],ay=g.vy[i],bx=g.vx[i+1],by=g.vy[i+1];double cx,cy,r,a0,th,delta;
   if(!Arc(g,i,out cx,out cy,out r,out a0,out th)){double dx=bx-ax,dy=by-ay,c2=dx*dx+dy*dy,t=c2>0?((px-ax)*dx+(py-ay)*dy)/c2:0;t=t<0?0:t>1?1:t;
    perp=Hyp(px-(ax+t*dx),py-(ay+t*dy));along=g.d[i]+t*L;return;}
   if(InSweep(a0,th,Math.Atan2(py-cy,px-cx),out delta)){perp=Math.Abs(Hyp(px-cx,py-cy)-r);along=g.d[i]+Math.Min(1.0,delta/Math.Abs(th))*L;return;}
   double da=Hyp(px-ax,py-ay),db=Hyp(px-bx,py-by);if(da<=db){perp=da;along=g.d[i];}else{perp=db;along=g.d[i+1];}}
  static List<Hit> Project(Src g,double px,double py,double tol){var l=new List<Hit>();
   for(int i=0;i<g.n-1;i++){if(!(g.d[i+1]-g.d[i]>0))continue;double a,p;Sub(g,i,px,py,out a,out p);if(p<=tol)l.Add(new Hit{along=a,perp=p});}return l;}
  static Hit Nearest(Src g,double px,double py){Hit best=null;
   for(int i=0;i<g.n-1;i++){if(!(g.d[i+1]-g.d[i]>0))continue;double a,p;Sub(g,i,px,py,out a,out p);if(best==null||p<best.perp||(p==best.perp&&a<best.along))best=new Hit{along=a,perp=p};}return best;}
  static void PointAt(Src g,double along,out double x,out double y){x=g.vx[0];y=g.vy[0];
   for(int i=0;i<g.n-1;i++){double L=g.d[i+1]-g.d[i];if(!(L>0)||along>g.d[i+1])continue;double f=(along-g.d[i])/L;f=f<0?0:f>1?1:f;double cx,cy,r,a0,th;
    if(!Arc(g,i,out cx,out cy,out r,out a0,out th)){x=g.vx[i]+f*(g.vx[i+1]-g.vx[i]);y=g.vy[i]+f*(g.vy[i+1]-g.vy[i]);return;}
    double ang=a0+th*f;x=cx+r*Math.Cos(ang);y=cy+r*Math.Sin(ang);return;}
   x=g.vx[g.n-1];y=g.vy[g.n-1];}

  // ---- topology ----
  static void Topology(Transaction tr,Ctx c){var ports=new List<Port>();
   foreach(var g in c.srcs){ports.Add(new Port{x=g.vx[0],y=g.vy[0],z=g.z,kind=0,end=0,owner=g.handle,src=g});ports.Add(new Port{x=g.vx[g.n-1],y=g.vy[g.n-1],z=g.z,kind=0,end=1,owner=g.handle,src=g});}
   foreach(var f in c.foreign)if(f.hasEnds){ports.Add(new Port{x=f.sx,y=f.sy,z=f.sz,kind=1,end=0,owner=f.handle,fo=f});ports.Add(new Port{x=f.ex,y=f.ey,z=f.ez,kind=1,end=1,owner=f.handle,fo=f});}
   foreach(var d in c.devs)ports.Add(new Port{x=d.dto.x,y=d.dto.y,z=d.dto.z,kind=2,end=0,owner=d.dto.handle,dev=d});
   double scale=Math.Max(c.scaleR,ports.Select(p=>Math.Max(Math.Abs(p.x),Math.Abs(p.y))).DefaultIfEmpty(0).Max());   // explicit scale (no epsR/1e-9 recovery)
   c.T=Math.Max(c.tol,Eps(scale));double T=c.T;
   ports.Sort((a,b)=>{int k=a.x.CompareTo(b.x);if(k!=0)return k;k=a.y.CompareTo(b.y);if(k!=0)return k;k=a.kind.CompareTo(b.kind);if(k!=0)return k;k=string.CompareOrdinal(a.owner,b.owner);return k!=0?k:a.end.CompareTo(b.end);});
   int n=ports.Count;var par=new int[n];for(int i=0;i<n;i++)par[i]=i;
   Func<int,int> find=null;find=i=>{while(par[i]!=i){par[i]=par[par[i]];i=par[i];}return i;};
   for(int i=0;i<n;i++)for(int j=i+1;j<n&&ports[j].x-ports[i].x<=T;j++)if(Hyp(ports[j].x-ports[i].x,ports[j].y-ports[i].y)<=T){int a=find(i),b=find(j);if(a!=b)par[Math.Max(a,b)]=Math.Min(a,b);}
   var clusters=new Dictionary<int,List<Port>>();var order=new List<int>();
   for(int i=0;i<n;i++){int r=find(i);List<Port> l;if(!clusters.TryGetValue(r,out l)){clusters[r]=l=new List<Port>();order.Add(r);}l.Add(ports[i]);}
   var nodes=new List<KeyValuePair<string,List<Port>>>();
   foreach(var r in order){var l=clusters[r];var rep=l.OrderBy(p=>p.x).ThenBy(p=>p.y).First();string id="N:"+N(rep.x)+","+N(rep.y);
    foreach(var p in l)p.node=id;nodes.Add(new KeyValuePair<string,List<Port>>(id,l));
    var owners=l.Select(p=>p.owner).Distinct(StringComparer.Ordinal).OrderBy(x=>x,StringComparer.Ordinal).ToArray();
    bool wide=false;for(int i=0;i<l.Count&&!wide;i++)for(int j=i+1;j<l.Count;j++)if(Hyp(l[i].x-l[j].x,l[i].y-l[j].y)>T){wide=true;break;}
    if(wide)Taint(c,id,"PORT_CLUSTER_AMBIGUOUS",Ck,"Connection points of "+string.Join(",",owners)+" chain together within tolerance "+N(T)+" but some are farther apart than it; no node is chosen",owners);
    if(l.Max(p=>p.z)-l.Min(p=>p.z)>Eps(l.Max(p=>Math.Abs(p.z))))D(c.s,In,"ELEVATION_MIXED","Connection points at node "+id+" differ in Z ("+N(l.Min(p=>p.z))+" .. "+N(l.Max(p=>p.z))+"); connected in plan (XY) only.",owners);
    foreach(var p in l){if(p.fo!=null)Taint(c,id,"ON_UNSUPPORTED_WIRE",Bl,null);if(p.src!=null&&p.src.bad)Taint(c,id,"WIRE_METADATA_INVALID",Bl,null);
     if(p.src!=null){if(p.end==0)p.src.startNode=id;else p.src.endNode=id;}}}
   // Interior junctions: a node within T of the interior of a measured source splits it there.
   foreach(var kv in nodes){string id=kv.Key;var l=kv.Value;double bx0=l.Min(p=>p.x)-T,by0=l.Min(p=>p.y)-T,bx1=l.Max(p=>p.x)+T,by1=l.Max(p=>p.y)+T;
    foreach(var g in c.srcs){if(g.maxX<bx0||g.minX>bx1||g.maxY<by0||g.minY>by1)continue;
     bool ownStart=l.Any(p=>p.src==g&&p.end==0),ownEnd=l.Any(p=>p.src==g&&p.end==1);var inner=new List<Hit>();
     foreach(var p in l)foreach(var hh in Project(g,p.x,p.y,T)){
      if(hh.along<=T){if(!ownStart)Taint(c,id,"PORT_NEAR_WIRE_END",Ck,"A connection point lies within tolerance of wire "+g.handle+" near its start but not on its endpoint",g.handle,p.owner);}
      else if(hh.along>=g.len-T){if(!ownEnd)Taint(c,id,"PORT_NEAR_WIRE_END",Ck,"A connection point lies within tolerance of wire "+g.handle+" near its end but not on its endpoint",g.handle,p.owner);}
      else inner.Add(hh);}
     if(inner.Count==0)continue;
     inner.Sort((a,b)=>a.along.CompareTo(b.along));int groups=1;double start=inner[0].along;
     foreach(var hh in inner)if(hh.along-start>T){groups++;start=hh.along;}
     if(groups>1)Taint(c,id,"PORT_MULTIPLE_POSITIONS",Ck,"Node lies within tolerance of wire "+g.handle+" at "+groups+" separate places; position not chosen",g.handle);
     var best=inner.OrderBy(hh=>hh.perp).ThenBy(hh=>hh.along).First();g.splits.Add(new Hit{along=best.along,perp=best.perp,node=id});}
    foreach(var f in c.foreign){if(f.hasBox&&(f.maxX<bx0||f.minX>bx1||f.maxY<by0||f.minY>by1))continue;
     try{var cv=tr.GetObject(f.id,OpenMode.ForRead) as Curve;if(cv==null)continue;
      foreach(var p in l){var q=cv.GetClosestPointTo(new Point3d(p.x,p.y,p.z),false);if(Hyp(q.X-p.x,q.Y-p.y)<=T){Taint(c,id,"ON_UNSUPPORTED_WIRE",Bl,null);break;}}}
     catch(Autodesk.AutoCAD.Runtime.Exception){}}}
   // Intervals between consecutive positions along each source.
   int segCount=0;
   foreach(var g in c.srcs){var pos=new List<Hit>{new Hit{along=0,node=g.startNode}};pos.AddRange(g.splits.OrderBy(hh=>hh.along).ThenBy(hh=>hh.node,StringComparer.Ordinal));pos.Add(new Hit{along=g.len,node=g.endNode});
    for(int k=1;k<pos.Count;k++)if(pos[k].along-pos[k-1].along<=T){Taint(c,pos[k-1].node,"SPLIT_TOO_CLOSE",Ck,"Two connection positions on wire "+g.handle+" are within tolerance of each other along it",g.handle);Taint(c,pos[k].node,"SPLIT_TOO_CLOSE",Ck,"Two connection positions on wire "+g.handle+" are within tolerance of each other along it",g.handle);}
    if(g.bad){foreach(var hh in pos)Taint(c,hh.node,"WIRE_METADATA_INVALID",Bl,null);continue;}
    int idx=0;
    for(int k=1;k<pos.Count;k++){double a=pos[k-1].along,b=pos[k].along;if(!(b>a))continue;
     if(++segCount>MaxSegments){Limit(c,"wire intervals",MaxSegments);return;}
     var sg=new RoomCadSegment037{id="S:"+g.handle+":"+idx.ToString("D4",Inv),index=idx,sourceHandle=g.handle,sourceType=g.type,sourceId=g.id,nodeA=pos[k-1].node,nodeB=pos[k].node,
      boundary=g.boundary,cableCode=g.code,fromDistance=a,toDistance=b,length=b-a,sourceLength=g.nativeLen};
     sg.evidence="src="+g.handle+";type="+g.type+";layer="+g.layer+";from="+N(a)+";to="+N(b)+";srcLen="+N(g.nativeLen)+";boundary="+g.boundary;
     idx++;g.segs.Add(sg);g.dto.segmentIds.Add(sg.id);c.s.segments[sg.id]=sg;}}
   // Overlap: an interval whose 1/4, 1/2 and 3/4 points all lie on another measured source shares geometry with it (never merged).
   var pairs=new HashSet<string>(StringComparer.Ordinal);
   foreach(var g in c.srcs){if(g.bad)continue;
    foreach(var sg in g.segs){var pts=new double[3,2];for(int k=0;k<3;k++){double x,y;PointAt(g,sg.fromDistance+(k+1)*0.25*sg.length,out x,out y);pts[k,0]=x;pts[k,1]=y;}
     foreach(var o in c.srcs){if(o==g||o.bad)continue;
      if(o.maxX<Math.Min(Math.Min(pts[0,0],pts[1,0]),pts[2,0])-T||o.minX>Math.Max(Math.Max(pts[0,0],pts[1,0]),pts[2,0])+T||o.maxY<Math.Min(Math.Min(pts[0,1],pts[1,1]),pts[2,1])-T||o.minY>Math.Max(Math.Max(pts[0,1],pts[1,1]),pts[2,1])+T)continue;
      Hit mid=null;bool all=true;for(int k=0;k<3&&all;k++){var hh=Nearest(o,pts[k,0],pts[k,1]);if(hh==null||hh.perp>T)all=false;else if(k==1)mid=hh;}
      if(!all)continue;
      Taint(c,sg.nodeA,"WIRE_OVERLAP",Ck,null);Taint(c,sg.nodeB,"WIRE_OVERLAP",Ck,null);
      foreach(var os in o.segs)if(mid.along>=os.fromDistance&&mid.along<=os.toDistance){Taint(c,os.nodeA,"WIRE_OVERLAP",Ck,null);Taint(c,os.nodeB,"WIRE_OVERLAP",Ck,null);}
      string key=string.CompareOrdinal(g.handle,o.handle)<0?g.handle+"|"+o.handle:o.handle+"|"+g.handle;
      if(pairs.Add(key))D(c.s,Ck,"WIRE_OVERLAP","Wires "+key.Replace("|"," and ")+" overlap (shared geometry); not merged, their components are blocked.",g.handle,o.handle);}}}
   // Devices: bound only when their node is an end of a measured interval.
   var segNodes=new HashSet<string>(c.s.segments.Values.SelectMany(x=>new[]{x.nodeA,x.nodeB}),StringComparer.Ordinal);
   foreach(var p in ports){if(p.dev==null)continue;var d=p.dev.dto;d.nodeId=p.node;d.bound=segNodes.Contains(p.node);d.status=d.bound?Bound:Unbound;
    if(p.dev.bad)Taint(c,p.node,"DEVICE_METADATA_INVALID",Ck,null);
    if(!d.bound)D(c.s,Ni,"DEVICE_UNBOUND","Device "+d.handle+" ("+d.name+") insertion point is not on a measured wire end/interior within tolerance "+N(T)+"; no connection invented.",d.handle);}
   foreach(var d in c.devs)d.dto.blocked=d.dto.nodeId!=null&&c.s.blockedNodes.ContainsKey(d.dto.nodeId);}

  // ---- core feed ----
  static void Feed(Ctx c){var s=c.s;var inp=new RoomInput037{roomId=s.roomId,units=s.declaredUnits};
   foreach(var d in c.devs)inp.devices.Add(new RoomDevice037{id=d.dto.handle,type=d.dto.type,name=d.dto.name,tagText=d.dto.tagText,panel=d.dto.panel,circuit=d.dto.circuit});
   foreach(var sg in s.segments.Values.OrderBy(x=>x.id,StringComparer.Ordinal))
    inp.segments.Add(new RoomSegment037{id=sg.id,nodeA=sg.nodeA,nodeB=sg.nodeB,cableCode=sg.cableCode,boundary=sg.boundary,evidence=sg.evidence,length=sg.length});
   foreach(var d in c.devs)if(d.dto.nodeId!=null)inp.nodeDevices.Add(new RoomNodeDevice037{nodeId=d.dto.nodeId,deviceId=d.dto.handle});
   foreach(var n in s.blockedNodes.Keys)inp.nodeDevices.Add(new RoomNodeDevice037{nodeId=n,deviceId=TaintDevice});
   if(inp.devices.Count==0&&inp.segments.Count==0)D(s,Ni,"SCAN_EMPTY","No device and no measured wire in the room; nothing to associate (an empty scan is not a PASS).");
   s.input=inp;s.result=RoomQty037.Associate(inp);s.network=RoomQty037.ResolveControlPaths(inp);}
  static string Signature(Ctx c){var s=c.s;var l=new List<string>(c.sig);l.Sort(string.CompareOrdinal);
   var head="ROOMCAD037|1|room:"+Tx(s.roomId)+"|units:"+Tx(s.declaredUnits)+"|tol:"+N(s.connectionTolerance)+"|rect:"+N(s.minX)+","+N(s.minY)+";"+N(s.maxX)+","+N(s.maxY)+"|fp:"+Tx(s.fingerprint)+"|iu:"+s.insunits;
   return Hash(head+"\n"+string.Join("\n",l));}
  static void Finish(RoomCadScan037 s){
   var seen=new HashSet<string>(StringComparer.Ordinal);var keep=new List<RoomCadDiag037>();
   foreach(var d in s.diagnostics){d.handles.Sort(string.CompareOrdinal);if(seen.Add(d.severity+"\u001f"+d.code+"\u001f"+string.Join(",",d.handles)+"\u001f"+d.message))keep.Add(d);}
   keep.Sort((a,b)=>{int k=Rank(b.severity).CompareTo(Rank(a.severity));if(k!=0)return k;k=string.CompareOrdinal(a.code,b.code);if(k!=0)return k;k=string.CompareOrdinal(string.Join(",",a.handles),string.Join(",",b.handles));return k!=0?k:string.CompareOrdinal(a.message,b.message);});
   s.diagnostics.Clear();s.diagnostics.AddRange(keep);
   int top=s.diagnostics.Select(d=>Rank(d.severity)).DefaultIfEmpty(0).Max();
   if(s.result!=null)top=Math.Max(top,Rank(s.result.status));if(s.network!=null)top=Math.Max(top,Rank(s.network.status));
   if(s.input==null)top=3;
   s.status=top>=3?Bl:top==2?Ck:top==1?Ni:RoomQty037.Associated;}
  // Canonical text of a scan (QA determinism / review); core outputs via RoomQty037.Describe / DescribeNetwork.
  public static string Describe(RoomCadScan037 s){var b=new StringBuilder();
   b.Append("SCAN|").Append(s.status).Append('|').Append(s.roomId).Append('|').Append(s.declaredUnits).Append("|tol=").Append(N(s.connectionTolerance)).Append("|T=").Append(N(s.effectiveTolerance))
    .Append("|rect=").Append(N(s.minX)).Append(',').Append(N(s.minY)).Append(';').Append(N(s.maxX)).Append(',').Append(N(s.maxY)).Append("|sig=").Append(s.signature).Append("|untyped=").Append(s.untypedExcluded)
    .Append("|unclassified=").Append(s.unclassifiedBlocks).Append("|limit=").Append(s.limitExceeded?1:0).Append('\n');
   foreach(var d in s.devices.Values.OrderBy(x=>x.handle,StringComparer.Ordinal))b.Append("DEV|").Append(d.handle).Append('|').Append(d.type).Append('|').Append(d.name).Append("|tag=").Append(d.tagText)
    .Append("|pc=").Append(d.panel).Append('/').Append(d.circuit).Append("|node=").Append(d.nodeId).Append('|').Append(d.status).Append(d.blocked?"|BLOCKED":"").Append("|basis=").Append(d.basis).Append('\n');
   foreach(var x in s.sources)b.Append("SRC|").Append(x.handle).Append('|').Append(x.type).Append('|').Append(x.status).Append('|').Append(x.boundary).Append("|code=").Append(x.cableCode)
    .Append("|len=").Append(N(x.length)).Append("|segs=").Append(string.Join(",",x.segmentIds)).Append("|why=").Append(x.reason).Append('\n');
   foreach(var x in s.segments.Values.OrderBy(v=>v.id,StringComparer.Ordinal))b.Append("SEG|").Append(x.id).Append('|').Append(x.nodeA).Append('-').Append(x.nodeB).Append("|d=").Append(N(x.fromDistance))
    .Append("..").Append(N(x.toDistance)).Append("|len=").Append(N(x.length)).Append('|').Append(x.boundary).Append('\n');
   foreach(var kv in s.blockedNodes)b.Append("BLOCKEDNODE|").Append(kv.Key).Append('|').Append(string.Join(",",kv.Value)).Append('\n');
   foreach(var d in s.diagnostics)b.Append("DIAG|").Append(d.severity).Append('|').Append(d.code).Append('|').Append(string.Join(",",d.handles)).Append('|').Append(d.message).Append('\n');
   return b.ToString();}
 }
}
#endif
