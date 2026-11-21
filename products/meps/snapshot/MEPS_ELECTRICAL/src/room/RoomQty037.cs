using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Tvxd.Meps {
 // TVXD-MEPS-ROOMQTY-037 CORE PILOT: PURE conservative device / TAG / wire-segment association for ONE selected room.
 // No CAD reference, no IO, no preprocessor symbol; C# 5 / .NET 4 + System.Core. ASCII only (no /codepage needed).
 // The native adapter (later task) owns: device classification (type), stable drawing-scoped IDs, geometric node IDs (two segment ends / a device
 // connection point share a node ID only when the adapter proved they coincide), room boundary status per segment and evidence text.
 // This core NEVER infers nearest neighbours, crossings, clipping, elevations, conduit size, conductor count or any engineering value.
 //
 // Graph: undirected; every ACCEPTED segment (valid, unique) is an edge, boundary-excluded ones included for topology only.
 //  - A device node terminates a chain. A non-device node of degree 2 continues the chain (physical segments are summed).
 //  - Component with a non-device node of degree >= 3            -> AMBIGUOUS_BRANCH (NEEDS_INPUT), no metres allocated.
 //  - Component with a closed loop (edges >= nodes)               -> COMPONENT_CYCLE (BLOCKED).
 //  - Component with a node carrying 2+ distinct devices         -> COMPONENT_MULTI_DEVICE_NODE (BLOCKED).
 //  - Component touching a rejected segment / bad device mapping -> COMPONENT_INVALID_SEGMENT (BLOCKED).
 //  - Chain that does not end at a device (open node / boundary)  -> CHAIN_NO_DEVICE_END (BLOCKED); same device at both ends -> CHAIN_SAME_DEVICE.
 //  One bad component never removes rows of other components.
 // Association per device-device chain (row): common TAG token keys of the two endpoint devices:
 //  exactly one -> control channel (basis TAG_TOKEN); several -> CHECK MULTIPLE_COMMON_TOKENS; none while both have tokens -> CHECK TAG_NO_COMMON_TOKEN;
 //  none but same complete structured panel/circuit -> NEEDS_INPUT STRUCTURED_PROPOSED; otherwise NEEDS_INPUT TAG_MISSING.
 //  Different structured panel/circuit -> CHECK STRUCTURED_CONFLICT and never resolved, whatever the tags say.
 //  Visible TAG = control-channel label; structured panel/circuit = circuit grouping. They are never equated.
 //  Another room switch carrying the row's channel -> CHECK MULTI_SWITCH_SAME_CHANNEL, except (NF-037-CHAIN-002) a row that is a whole nonbranch component
 //  with exactly two SWITCH ends, no other device, no excluded segment, one common token and no other in-scope switch on it: ASSOCIATED, basis
 //  PHYSICAL_ENDPOINT_TAG_TOKEN (measured point-to-point takeoff, not an inferred two-way design; assembly still declared downstream).
 // Row status: ASSOCIATED / NEEDS_INPUT / CHECK. This is association only, NOT a quantity PASS.
 // Coverage invariant: every accepted unique segment ID is owned exactly once by a row or by a diagnostic's segmentIds; checked, else COVERAGE_VIOLATION.
 // Output is sorted (ordinal) and independent of input order.

 public sealed class RoomDevice037 {
  public string id,type,name,tagText,panel,circuit;   // type: SWITCH|LAMP|SOCKET|FAN|PANEL|OTHER (exact). panel+circuit: structured grouping, both or neither
  public List<string> allowedTokens;                  // null = no restriction; else every parsed tag token must be in this list (TAG_TOKEN_NOT_ALLOWED)
 }
 public sealed class RoomSegment037 {
  public string id,nodeA,nodeB,cableCode,boundary,evidence;   // boundary: FULLY_INSIDE | CROSSING | UNKNOWN (exact)
  public double length;                                        // measured length in declared drawing units, > 0 and finite
 }
 public sealed class RoomNodeDevice037 {public string nodeId,deviceId;}
 public sealed class RoomInput037 {
  public string roomId,units;   // units: exactly "MM" or "M"; no default
  public List<RoomDevice037> devices=new List<RoomDevice037>();
  public List<RoomSegment037> segments=new List<RoomSegment037>();
  public List<RoomNodeDevice037> nodeDevices=new List<RoomNodeDevice037>();
 }
 // tokens: trimmed original display text; keys: matching keys (Unicode NFC + invariant upper case), same order; issues: TAG_* codes.
 public sealed class RoomTag037 {public string original;public readonly List<string> tokens=new List<string>(),keys=new List<string>(),issues=new List<string>();}
 public sealed class RoomSegmentUse037 {public string id,nodeA,nodeB,cableCode,evidence;public double length;}
 // severity: INFO | NEEDS_INPUT | CHECK | BLOCKED. segmentIds: accepted segments OWNED by this diagnostic (coverage). relatedIds: references only.
 public sealed class RoomDiag037 {public string severity,code,subject,message;public readonly List<string> segmentIds=new List<string>(),relatedIds=new List<string>();
  public override string ToString(){return severity+" "+code+" ["+subject+"] "+message;}}
 public sealed class RoomRow037 {
  public string rowKey,status,basis,roomId,units;     // basis: TAG_TOKEN | PHYSICAL_ENDPOINT_TAG_TOKEN (measured switch->switch chain) | STRUCTURED_PROPOSED | NONE
  public string fromId,fromType,fromName,fromTag,fromLabel,fromPanel,fromCircuit;
  public string toId,toType,toName,toTag,toLabel,toPanel,toCircuit;
  public string controlKey,controlLabel,structuredKey,controllerStatus;   // controllerStatus: IN_ROW|SWITCH_PAIR_PHYSICAL|ROOM_CANDIDATE|AMBIGUOUS|NONE|NOT_APPLICABLE|null
  public double drawingLength;public double? planMetres;                  // planMetres null when units invalid
  public readonly List<string> candidateKeys=new List<string>(),controllerIds=new List<string>(),missing=new List<string>(),diagnostics=new List<string>(),
   nodeIds=new List<string>(),cableCodes=new List<string>();
  public readonly List<RoomSegmentUse037> segments=new List<RoomSegmentUse037>();   // traversal order from -> to
  public List<string> SegmentIds(){return segments.Select(s=>s.id).ToList();}
 }
 public sealed class RoomResult037 {
  public string roomId,units,status;   // status: ASSOCIATED | NEEDS_INPUT | CHECK (never PASS)
  public readonly List<RoomRow037> rows=new List<RoomRow037>();
  public readonly List<RoomDiag037> diagnostics=new List<RoomDiag037>();
  public readonly List<string> acceptedSegmentIds=new List<string>(),rejectedSegmentIds=new List<string>(),
   unaccountedSegmentIds=new List<string>(),doubleCountedSegmentIds=new List<string>(),foreignSegmentIds=new List<string>();
  public bool coverageOk;
  public int CountRows(string st){return rows.Count(r=>r.status==st);}
  public List<RoomDiag037> Diags(string code){return diagnostics.Where(d=>d.code==code).ToList();}
 }

 // ---- PATCH F037-02: logical control paths over a junction tree + separate physical segment inventory (ResolveControlPaths) ----
 // One logical path = one controller SWITCH -> one LAMP/FAN target on one control channel. Paths may share physical segments.
 // planMetres = full plan length of that path (shared segments included per path). It is NOT a conduit or wire quantity.
 // S6: a logical path is the controller->load ASSOCIATION (incl. every intermediate device it passes). The measured quantity row is a separate
 // projection made by RoomCompletion037 (successive device-to-device sections of a daisy chain); this class keeps the full logical path unchanged.
 public sealed class RoomLogicalPath037 {
  public string pathId,status,basis,fromId,toId,fromLabel,toLabel,controlKey,controlLabel,structuredKey;   // status: ASSOCIATED|NEEDS_INPUT|CHECK
  public double drawingLength;public double? planMetres;
  public readonly List<string> segmentIds=new List<string>(),nodeIds=new List<string>(),intermediateDeviceIds=new List<string>(),missing=new List<string>(),diagnostics=new List<string>();
 }
 // Each accepted segment appears exactly once. pathIds = logical paths using it (a shared trunk lists several).
 // status: ASSIGNED (>=1 ASSOCIATED path) | CANDIDATE (only non-associated paths) | UNASSIGNED (no path; owned by a diagnostic) | EXCLUDED (boundary).
 public sealed class RoomPhysicalSegment037 {
  public string id,nodeA,nodeB,cableCode,boundary,evidence,componentId,status;
  public double length;public double? lengthMetres;
  public readonly List<string> pathIds=new List<string>();
 }
 // physicalLengthMetres: each physical segment with >=1 path membership counted ONCE (null if units invalid or no membership).
 // Not a conduit quantity (no shared-conduit declaration) and not a wire quantity (no declared assemblies). status never PASS.
 public sealed class RoomNetwork037 {
  public string roomId,units,status;
  public double? physicalLengthMetres;
  public bool coverageOk;
  public readonly List<RoomLogicalPath037> logicalPaths=new List<RoomLogicalPath037>();
  public readonly List<RoomPhysicalSegment037> physicalSegments=new List<RoomPhysicalSegment037>();
  public readonly List<RoomDiag037> diagnostics=new List<RoomDiag037>();
  public readonly List<string> acceptedSegmentIds=new List<string>(),rejectedSegmentIds=new List<string>(),unassignedSegmentIds=new List<string>();
  public List<RoomDiag037> Diags(string code){return diagnostics.Where(d=>d.code==code).ToList();}
  public RoomPhysicalSegment037 Segment(string id){return physicalSegments.FirstOrDefault(s=>s.id==id);}
 }

 public static class RoomQty037 {
  public const string Associated="ASSOCIATED",NeedsInput="NEEDS_INPUT",Check="CHECK",Info="INFO",Blocked="BLOCKED";
  public const string Inside="FULLY_INSIDE",Crossing="CROSSING",Unknown="UNKNOWN",MM="MM",M="M";
  public const string Switch="SWITCH",Lamp="LAMP",Socket="SOCKET",Fan="FAN",Panel="PANEL",Other="OTHER";
  static readonly string[] Types={Switch,Lamp,Socket,Fan,Panel,Other};
  static readonly StringComparer O=StringComparer.Ordinal;

  sealed class Dev {public string id,type,panel,circuit;public RoomDevice037 src;public RoomTag037 tag;public bool typeOk,partial,tagBad;
   public string Struct{get{return panel==null?null:panel+"\u001f"+circuit;}}
   public string StructText{get{return panel==null?null:panel+"/"+circuit;}}}
  sealed class Seg {public RoomSegment037 s;public bool excluded;}
  sealed class Ctx {public RoomInput037 input;public RoomResult037 res;public bool unitsOk;public double upm;
   public readonly Dictionary<string,Dev> devs=new Dictionary<string,Dev>(StringComparer.Ordinal);
   public readonly HashSet<string> badDev=new HashSet<string>(StringComparer.Ordinal),taint=new HashSet<string>(StringComparer.Ordinal);
   public readonly Dictionary<string,SortedSet<string>> nodeDevs=new Dictionary<string,SortedSet<string>>(StringComparer.Ordinal);
   public readonly Dictionary<string,string> parent=new Dictionary<string,string>(StringComparer.Ordinal);
   public readonly Dictionary<string,List<Seg>> adj=new Dictionary<string,List<Seg>>(StringComparer.Ordinal);
   public readonly List<Seg> segs=new List<Seg>();}

  static bool Blank(string s){return string.IsNullOrWhiteSpace(s);}
  static string Show(string s){return s==null?"(null)":"\""+s+"\"";}
  static string N(double v){return v.ToString("R",CultureInfo.InvariantCulture);}
  static string J(IEnumerable<string> l){return string.Join(",",l);}
  static void AddOnce(List<string> l,string v){if(!l.Contains(v))l.Add(v);}
  static int Rank(string s){switch(s){case Blocked:return 3;case Check:return 2;case NeedsInput:return 1;default:return 0;}}
  static RoomDiag037 Diag(RoomResult037 r,string sev,string code,string subject,string msg){var d=new RoomDiag037{severity=sev,code=code,subject=subject,message=msg};r.diagnostics.Add(d);return d;}
  static string Key(string s){string n;try{n=s.Normalize(NormalizationForm.FormC);}catch(ArgumentException){n=s;}return n.ToUpperInvariant();}

  // TAG text -> channel tokens. Split on ',' and ';' only; each token trimmed; text kept as typed (leading zeros, Unicode). "1-3" stays ONE literal token.
  // Issues (never silent): TAG_MISSING (null/blank), TAG_EMPTY_TOKEN ("1,,2", "1,"), TAG_DUPLICATE_TOKEN ("1,1" / "a,A"; kept once).
  public static RoomTag037 ParseTag(string text){var t=new RoomTag037{original=text};
   if(Blank(text)){t.issues.Add("TAG_MISSING");return t;}
   foreach(var raw in text.Split(',',';')){var tok=raw.Trim();
    if(tok.Length==0){AddOnce(t.issues,"TAG_EMPTY_TOKEN");continue;}
    var k=Key(tok);if(t.keys.Contains(k)){AddOnce(t.issues,"TAG_DUPLICATE_TOKEN");continue;}
    t.tokens.Add(tok);t.keys.Add(k);}
   return t;}

  public static RoomResult037 Associate(RoomInput037 input){var res=new RoomResult037();
   if(input==null){Diag(res,Blocked,"INPUT_NULL","input","No room input supplied.");Finish(res);return res;}
   res.roomId=input.roomId;res.units=input.units;
   var c=Prepare(input,res);
   Components(c);
   Tail(c);
   Finish(res);return res;}

  // Shared strict validation (room, units, devices, tags, segments, node map) used by Associate and ResolveControlPaths.
  static Ctx Prepare(RoomInput037 input,RoomResult037 res){var c=new Ctx{input=input,res=res};
   if(Blank(input.roomId))Diag(res,NeedsInput,"ROOM_ID_MISSING","room","Room ID is required; every row stays NEEDS_INPUT.");
   if(input.units==MM){c.unitsOk=true;c.upm=1000;}else if(input.units==M){c.unitsOk=true;c.upm=1;}
   else Diag(res,NeedsInput,"UNITS_INVALID","units","Drawing units must be declared exactly MM or M (got "+Show(input.units)+"); no default, plan metres not computed.");
   ReadDevices(c);ReadSegments(c);ReadNodes(c);
   foreach(var l in c.adj.Values)l.Sort((x,y)=>string.CompareOrdinal(x.s.id,y.s.id));
   return c;}
  static void Tail(Ctx c){
   foreach(var d in c.devs.Values.OrderBy(x=>x.id,O)){var id=d.id;
    if(!c.nodeDevs.Any(kv=>kv.Value.Contains(id)&&c.adj.ContainsKey(kv.Key)))
     Diag(c.res,Info,"DEVICE_NO_SEGMENT","device:"+id,"Device "+id+" is not at any node of an accepted wire segment; no row.").relatedIds.Add(id);}
   if(c.res.acceptedSegmentIds.Count==0)Diag(c.res,NeedsInput,"NO_ACCEPTED_SEGMENTS","room","No valid wire segment was accepted for this room.");}

  static bool SameList(List<string> a,List<string> b){if(a==null||b==null)return a==null&&b==null;return a.SequenceEqual(b,O);}
  static bool SameDevice(RoomDevice037 a,RoomDevice037 b){return O.Equals(a.id,b.id)&&O.Equals(a.type,b.type)&&O.Equals(a.name,b.name)&&O.Equals(a.tagText,b.tagText)&&
   O.Equals(a.panel,b.panel)&&O.Equals(a.circuit,b.circuit)&&SameList(a.allowedTokens,b.allowedTokens);}
  static string DevText(RoomDevice037 d){return "type "+Show(d.type)+", name "+Show(d.name)+", tag "+Show(d.tagText);}

  static void ReadDevices(Ctx c){var by=new Dictionary<string,List<RoomDevice037>>(O);
   foreach(var d in c.input.devices??new List<RoomDevice037>()){
    if(d==null){Diag(c.res,Check,"DEVICE_NULL","device(null)","Null device entry ignored.");continue;}
    if(Blank(d.id)){Diag(c.res,Check,"DEVICE_ID_MISSING","device(no id)","Device without a stable drawing-scoped ID ("+DevText(d)+") cannot be associated.");continue;}
    List<RoomDevice037> l;if(!by.TryGetValue(d.id,out l))by[d.id]=l=new List<RoomDevice037>();l.Add(d);}
   foreach(var id in by.Keys.OrderBy(k=>k,O)){var l=by[id];
    if(l.Skip(1).Any(x=>!SameDevice(x,l[0]))){c.badDev.Add(id);
     Diag(c.res,Blocked,"DEVICE_DUPLICATE_CONFLICT","device:"+id,"Device ID "+id+" supplied "+l.Count+" times with different data; not used, wire components at its nodes are blocked.").relatedIds.Add(id);continue;}
    if(l.Count>1)Diag(c.res,Info,"DEVICE_DUPLICATE_SELECTION","device:"+id,"Device "+id+" selected "+l.Count+" times with identical data; used once.").relatedIds.Add(id);
    var d=l[0];var v=new Dev{id=id,src=d,type=d.type};
    v.typeOk=d.type!=null&&Array.IndexOf(Types,d.type)>=0;
    if(!v.typeOk)Diag(c.res,Check,"DEVICE_TYPE_INVALID","device:"+id,"Device "+id+" type "+Show(d.type)+" is not SWITCH/LAMP/SOCKET/FAN/PANEL/OTHER; rows need its type.").relatedIds.Add(id);
    bool hp=!Blank(d.panel),hc=!Blank(d.circuit);
    if(hp&&hc){v.panel=d.panel;v.circuit=d.circuit;}
    else if(hp||hc){v.partial=true;Diag(c.res,Check,"DEVICE_STRUCTURED_PARTIAL","device:"+id,"Device "+id+" has only one of panel/circuit (panel "+Show(d.panel)+", circuit "+Show(d.circuit)+"); structured group not used.").relatedIds.Add(id);}
    v.tag=ParseTag(d.tagText);
    if(d.allowedTokens!=null){var allow=new HashSet<string>(d.allowedTokens.Where(t=>!Blank(t)).Select(t=>Key(t.Trim())),O);
     if(v.tag.keys.Any(k=>!allow.Contains(k)))AddOnce(v.tag.issues,"TAG_TOKEN_NOT_ALLOWED");}
    foreach(var iss in v.tag.issues){
     if(iss=="TAG_MISSING"){if(d.type!=Panel)Diag(c.res,NeedsInput,iss,"device:"+id,"Device "+id+" has no TAG text; channel cannot be matched.").relatedIds.Add(id);continue;}
     v.tagBad=true;Diag(c.res,Check,iss,"device:"+id,"Device "+id+" TAG "+Show(d.tagText)+" is malformed ("+iss+"); rows using it stay CHECK.").relatedIds.Add(id);}
    c.devs[id]=v;}}

  static string SegText(RoomSegment037 s){return "nodes "+Show(s.nodeA)+"-"+Show(s.nodeB)+", length "+N(s.length)+", boundary "+Show(s.boundary);}
  static bool SameSegment(RoomSegment037 a,RoomSegment037 b){
   bool nodes=(O.Equals(a.nodeA,b.nodeA)&&O.Equals(a.nodeB,b.nodeB))||(O.Equals(a.nodeA,b.nodeB)&&O.Equals(a.nodeB,b.nodeA));
   return nodes&&a.length.Equals(b.length)&&O.Equals(a.cableCode,b.cableCode)&&O.Equals(a.boundary,b.boundary)&&O.Equals(a.evidence,b.evidence);}
  static void Node(Ctx c,string n){if(!c.parent.ContainsKey(n))c.parent[n]=n;}
  static string Find(Ctx c,string n){var r=n;while(!O.Equals(c.parent[r],r))r=c.parent[r];
   while(!O.Equals(c.parent[n],r)){var nx=c.parent[n];c.parent[n]=r;n=nx;}return r;}
  static void Union(Ctx c,string a,string b){Node(c,a);Node(c,b);var ra=Find(c,a);var rb=Find(c,b);if(O.Equals(ra,rb))return;
   if(string.CompareOrdinal(ra,rb)<0)c.parent[rb]=ra;else c.parent[ra]=rb;}
  static void TaintNode(Ctx c,string n){Node(c,n);c.taint.Add(n);}
  // A rejected segment still marks its known nodes: any component reaching them is blocked instead of being re-read with a hole in its topology.
  static void Taint(Ctx c,RoomSegment037 s){bool a=!Blank(s.nodeA),b=!Blank(s.nodeB);if(a)TaintNode(c,s.nodeA);if(b)TaintNode(c,s.nodeB);if(a&&b)Union(c,s.nodeA,s.nodeB);}

  static void ReadSegments(Ctx c){var by=new Dictionary<string,List<RoomSegment037>>(O);
   foreach(var s in c.input.segments??new List<RoomSegment037>()){
    if(s==null){Diag(c.res,Blocked,"SEGMENT_NULL","segment(null)","Null segment entry ignored.");continue;}
    if(Blank(s.id)){Diag(c.res,Blocked,"SEGMENT_ID_MISSING","segment(no id)","Segment without a stable source ID ("+SegText(s)+") rejected; components at its nodes are blocked.");Taint(c,s);continue;}
    List<RoomSegment037> l;if(!by.TryGetValue(s.id,out l))by[s.id]=l=new List<RoomSegment037>();l.Add(s);}
   foreach(var id in by.Keys.OrderBy(k=>k,O)){var l=by[id];
    if(l.Skip(1).Any(x=>!SameSegment(x,l[0]))){c.res.rejectedSegmentIds.Add(id);
     Diag(c.res,Blocked,"SEGMENT_DUPLICATE_CONFLICT","segment:"+id,"Segment ID "+id+" supplied "+l.Count+" times with different data; rejected, components at its nodes are blocked.").relatedIds.Add(id);
     foreach(var x in l)Taint(c,x);continue;}
    var s=l.OrderBy(x=>(x.nodeA??"")+"\u001f"+(x.nodeB??""),O).First();
    if(l.Count>1)Diag(c.res,Info,"SEGMENT_DUPLICATE_SELECTION","segment:"+id,"Segment "+id+" selected "+l.Count+" times with identical data; counted once.").relatedIds.Add(id);
    var bad=new List<string>();
    if(Blank(s.nodeA)||Blank(s.nodeB))bad.Add("SEGMENT_NODE_MISSING");else if(O.Equals(s.nodeA,s.nodeB))bad.Add("SEGMENT_SELF_EDGE");
    if(double.IsNaN(s.length)||double.IsInfinity(s.length)||!(s.length>0))bad.Add("SEGMENT_LENGTH_INVALID");
    if(s.boundary!=Inside&&s.boundary!=Crossing&&s.boundary!=Unknown)bad.Add("SEGMENT_BOUNDARY_INVALID");
    if(bad.Count>0){c.res.rejectedSegmentIds.Add(id);
     Diag(c.res,Blocked,bad[0],"segment:"+id,"Segment "+id+" rejected ("+J(bad)+"; "+SegText(s)+"); not counted, components at its nodes are blocked.").relatedIds.Add(id);
     Taint(c,s);continue;}
    var g=new Seg{s=s,excluded=s.boundary!=Inside};c.segs.Add(g);c.res.acceptedSegmentIds.Add(id);Union(c,s.nodeA,s.nodeB);
    foreach(var n in new[]{s.nodeA,s.nodeB}){List<Seg> a;if(!c.adj.TryGetValue(n,out a))c.adj[n]=a=new List<Seg>();a.Add(g);}
    if(g.excluded)Diag(c.res,NeedsInput,"BOUNDARY_EXCLUDED","segment:"+id,"Segment "+id+" boundary "+s.boundary+": excluded from room quantities (not clipped, not counted in full). Evidence "+Show(s.evidence)+".").segmentIds.Add(id);}}

  static void ReadNodes(Ctx c){var seen=new HashSet<string>(O);
   foreach(var m in c.input.nodeDevices??new List<RoomNodeDevice037>()){
    if(m==null){Diag(c.res,Check,"NODE_MAP_INVALID","node(null)","Null node-device entry ignored.");continue;}
    if(Blank(m.nodeId)||Blank(m.deviceId)){
     Diag(c.res,Check,"NODE_MAP_INVALID","node:"+(m.nodeId??""),"Node-device entry needs both IDs (node "+Show(m.nodeId)+", device "+Show(m.deviceId)+")"+(Blank(m.nodeId)?".":"; components at this node are blocked."));
     if(!Blank(m.nodeId))TaintNode(c,m.nodeId);continue;}
    if(!seen.Add(m.nodeId+"\u001f"+m.deviceId))continue;   // exact repeat of the same mapping
    if(c.badDev.Contains(m.deviceId)){TaintNode(c,m.nodeId);continue;}
    if(!c.devs.ContainsKey(m.deviceId)){
     Diag(c.res,Blocked,"NODE_MAP_UNKNOWN_DEVICE","node:"+m.nodeId,"Node "+m.nodeId+" maps to device "+m.deviceId+" which is not a valid listed device; components at this node are blocked.").relatedIds.Add(m.deviceId);
     TaintNode(c,m.nodeId);continue;}
    SortedSet<string> set;if(!c.nodeDevs.TryGetValue(m.nodeId,out set))c.nodeDevs[m.nodeId]=set=new SortedSet<string>(O);set.Add(m.deviceId);Node(c,m.nodeId);}}

  static int DevCount(Ctx c,string n){SortedSet<string> s;return c.nodeDevs.TryGetValue(n,out s)?s.Count:0;}
  static string OtherNode(Seg e,string n){return O.Equals(e.s.nodeA,n)?e.s.nodeB:e.s.nodeA;}

  static void Components(Ctx c){var comp=new Dictionary<string,List<Seg>>(O);var nodesOf=new Dictionary<string,List<string>>(O);
   foreach(var g in c.segs){var r=Find(c,g.s.nodeA);List<Seg> l;if(!comp.TryGetValue(r,out l))comp[r]=l=new List<Seg>();l.Add(g);}
   foreach(var n in c.parent.Keys.ToList()){var r=Find(c,n);List<string> l;if(!nodesOf.TryGetValue(r,out l))nodesOf[r]=l=new List<string>();l.Add(n);}
   foreach(var r in comp.Keys.OrderBy(k=>k,O)){
    var edges=comp[r].OrderBy(e=>e.s.id,O).ToList();var nodes=nodesOf[r].OrderBy(x=>x,O).ToList();
    var acc=new SortedSet<string>(O);foreach(var e in edges){acc.Add(e.s.nodeA);acc.Add(e.s.nodeB);}
    var reasons=new List<string>();string code=null;var related=new SortedSet<string>(O);
    var tainted=nodes.Where(n=>c.taint.Contains(n)).ToList();
    if(tainted.Count>0){code="COMPONENT_INVALID_SEGMENT";reasons.Add("touches rejected segment/device data at node(s) "+J(tainted));foreach(var n in tainted)related.Add(n);}
    var multi=acc.Where(n=>DevCount(c,n)>1).ToList();
    if(multi.Count>0){code=code??"COMPONENT_MULTI_DEVICE_NODE";
     reasons.Add("node(s) "+J(multi.Select(n=>n+"{"+J(c.nodeDevs[n])+"}"))+" carry more than one distinct device");foreach(var n in multi)related.Add(n);}
    if(edges.Count>=acc.Count){code=code??"COMPONENT_CYCLE";reasons.Add("contains a closed loop ("+edges.Count+" segments, "+acc.Count+" nodes)");}
    var branch=acc.Where(n=>DevCount(c,n)==0&&c.adj[n].Count>=3).ToList();
    if(branch.Count>0){code=code??"AMBIGUOUS_BRANCH";reasons.Add("junction node(s) "+J(branch)+" have 3 or more segments; shared-trunk allocation needs explicit mapping");foreach(var n in branch)related.Add(n);}
    if(code!=null){
     foreach(var n in acc){SortedSet<string> ds;if(c.nodeDevs.TryGetValue(n,out ds))foreach(var d in ds)related.Add(d);}
     var inside=edges.Where(e=>!e.excluded).ToList();
     var dg=Diag(c.res,code=="AMBIGUOUS_BRANCH"?NeedsInput:Blocked,code,"component:"+nodes[0],
      "Wire component at nodes "+J(acc)+": "+string.Join("; ",reasons)+". Its "+inside.Count+" inside segment(s) are not associated; no metres allocated.");
     foreach(var e in inside)dg.segmentIds.Add(e.s.id);dg.relatedIds.AddRange(related);continue;}
    Walk(c,edges,acc);}}

  static void Walk(Ctx c,List<Seg> edges,SortedSet<string> nodes){var seen=new HashSet<Seg>();
   foreach(var e in edges)if(e.excluded)seen.Add(e);   // owned by BOUNDARY_EXCLUDED
   // NF-037-CHAIN-002: the component is one measured two-device chain only when it has exactly two devices, no boundary-excluded segment and the row path
   // uses every segment of the component (checked per row below). Components reaching Walk are already free of taint, cycles, branches, multi-device nodes.
   var compDevs=new SortedSet<string>(O);foreach(var n in nodes){SortedSet<string> ds;if(c.nodeDevs.TryGetValue(n,out ds))compDevs.UnionWith(ds);}
   bool twoDevicesInside=compDevs.Count==2&&!edges.Any(e=>e.excluded);
   foreach(var n in nodes){if(!c.nodeDevs.ContainsKey(n))continue;var d1=c.nodeDevs[n].Min;
    foreach(var e in c.adj[n]){if(seen.Contains(e))continue;
     var path=new List<Seg>{e};var pn=new List<string>{n};string cur=OtherNode(e,n);Seg came=e,stop=null;string end;
     while(true){pn.Add(cur);if(c.nodeDevs.ContainsKey(cur)){end="DEVICE";break;}
      Seg nx=null;foreach(var x in c.adj[cur])if(x!=came){nx=x;break;}
      if(nx==null){end="OPEN";break;}
      if(nx.excluded){end="BOUNDARY";stop=nx;break;}
      path.Add(nx);came=nx;cur=OtherNode(nx,cur);}
     foreach(var p in path)seen.Add(p);
     if(end!="DEVICE"){
      var dg=Diag(c.res,Blocked,"CHAIN_NO_DEVICE_END","device:"+d1+"@"+n,"Wire chain from device "+d1+" ends at "+(end=="BOUNDARY"?"excluded boundary segment "+stop.s.id:"open node "+cur+" without a device")+"; not associated.");
      foreach(var p in path)dg.segmentIds.Add(p.s.id);dg.relatedIds.Add(d1);if(stop!=null)dg.relatedIds.Add(stop.s.id);continue;}
     var d2=c.nodeDevs[cur].Min;
     if(O.Equals(d1,d2)){var dg=Diag(c.res,Blocked,"CHAIN_SAME_DEVICE","device:"+d1+"@"+n,"Wire chain starts and ends at the same device "+d1+" (nodes "+n+", "+cur+"); not associated.");
      foreach(var p in path)dg.segmentIds.Add(p.s.id);dg.relatedIds.Add(d1);continue;}
     c.res.rows.Add(Row(c,c.devs[d1],c.devs[d2],path,pn,twoDevicesInside&&path.Count==edges.Count));}}
   foreach(var e in edges){if(seen.Contains(e))continue;   // chains with no device at either end
    var chain=new List<Seg>{e};seen.Add(e);var ends=new List<string>();
    foreach(var start in new[]{e.s.nodeA,e.s.nodeB}){string cur=start;Seg came=e;
     while(true){if(c.nodeDevs.ContainsKey(cur)){ends.Add("device node "+cur);break;}
      Seg nx=null;foreach(var x in c.adj[cur])if(x!=came){nx=x;break;}
      if(nx==null){ends.Add("open node "+cur);break;}
      if(nx.excluded){ends.Add("boundary segment "+nx.s.id);break;}
      if(seen.Contains(nx)){ends.Add("node "+cur);break;}
      chain.Add(nx);seen.Add(nx);came=nx;cur=OtherNode(nx,cur);}}
    var ids=chain.Select(x=>x.s.id).OrderBy(x=>x,O).ToList();
    var dg=Diag(c.res,Blocked,"CHAIN_NO_DEVICE_END","chain:"+ids[0],"Wire chain "+J(ids)+" has no device at either end ("+J(ends.OrderBy(x=>x,O))+"); not associated.");
    dg.segmentIds.AddRange(ids);}}

  // Orientation: PANEL -> other; SWITCH -> LAMP; otherwise ordinal device ID order.
  static bool AFirst(Dev a,Dev b){bool ap=a.type==Panel,bp=b.type==Panel;if(ap!=bp)return ap;
   if(a.type==Switch&&b.type==Lamp)return true;if(b.type==Switch&&a.type==Lamp)return false;return string.CompareOrdinal(a.id,b.id)<0;}
  static string Label(Dev d){return (d.src.type??"?")+(Blank(d.src.name)?"":" "+d.src.name)+(Blank(d.src.tagText)?"":" ["+d.src.tagText+"]");}
  static void Flag(RoomRow037 r,string sev,string code){if(Rank(sev)>Rank(r.status))r.status=sev;AddOnce(r.diagnostics,code);}

  // simpleChain: the row path is the whole wire component, which carries exactly these two devices and no boundary-excluded segment (see Walk).
  static RoomRow037 Row(Ctx c,Dev a,Dev b,List<Seg> path,List<string> pn,bool simpleChain){
   if(!AFirst(a,b)){var t=a;a=b;b=t;path=Enumerable.Reverse(path).ToList();pn=Enumerable.Reverse(pn).ToList();}
   var r=new RoomRow037{status=Associated,basis="NONE",roomId=c.input.roomId,units=c.input.units,
    fromId=a.id,fromType=a.src.type,fromName=a.src.name,fromTag=a.src.tagText,fromLabel=Label(a),fromPanel=a.src.panel,fromCircuit=a.src.circuit,
    toId=b.id,toType=b.src.type,toName=b.src.name,toTag=b.src.tagText,toLabel=Label(b),toPanel=b.src.panel,toCircuit=b.src.circuit};
   r.nodeIds.AddRange(pn);double sum=0;
   foreach(var g in path){sum+=g.s.length;r.segments.Add(new RoomSegmentUse037{id=g.s.id,nodeA=g.s.nodeA,nodeB=g.s.nodeB,cableCode=g.s.cableCode,evidence=g.s.evidence,length=g.s.length});}
   r.drawingLength=sum;if(c.unitsOk)r.planMetres=sum/c.upm;
   r.rowKey=a.id+"->"+b.id+"|"+J(path.Select(g=>g.s.id).OrderBy(x=>x,O));
   r.cableCodes.AddRange(path.Where(g=>!Blank(g.s.cableCode)).Select(g=>g.s.cableCode).Distinct(O).OrderBy(x=>x,O));
   if(r.cableCodes.Count>1)Flag(r,Check,"CABLE_CODE_MIXED");
   if(Blank(c.input.roomId)){Flag(r,NeedsInput,"ROOM_ID_MISSING");AddOnce(r.missing,"roomId");}
   if(!c.unitsOk){Flag(r,NeedsInput,"UNITS_INVALID");AddOnce(r.missing,"units");}
   bool typed=true;
   foreach(var d in new[]{a,b}){
    if(!d.typeOk){Flag(r,NeedsInput,"TYPE_INVALID");AddOnce(r.missing,"type:"+d.id);typed=false;}
    else if(d.type==Other){Flag(r,NeedsInput,"OTHER_TYPE_NEEDS_MAPPING");AddOnce(r.missing,"mapping:"+d.id);typed=false;}
    if(d.partial){Flag(r,Check,"STRUCTURED_PARTIAL");AddOnce(r.missing,"panelCircuit:"+d.id);}
    if(d.tagBad)Flag(r,Check,"TAG_MALFORMED");}
   string sa=a.Struct,sb=b.Struct;bool conflict=sa!=null&&sb!=null&&!O.Equals(sa,sb);
   if(conflict)Flag(r,Check,"STRUCTURED_CONFLICT");else if(sa!=null&&sb!=null)r.structuredKey=a.StructText;
   string scope=conflict?null:(sa??sb);
   foreach(var k in a.tag.keys)if(b.tag.keys.Contains(k))r.candidateKeys.Add(k);
   if(!typed||conflict){}
   else if(r.candidateKeys.Count==1){r.controlKey=r.candidateKeys[0];r.controlLabel=a.tag.tokens[a.tag.keys.IndexOf(r.controlKey)];r.basis="TAG_TOKEN";}
   else if(r.candidateKeys.Count>1){Flag(r,Check,"MULTIPLE_COMMON_TOKENS");AddOnce(r.missing,"controlKey");}
   else if(a.tag.keys.Count>0&&b.tag.keys.Count>0){Flag(r,Check,"TAG_NO_COMMON_TOKEN");AddOnce(r.missing,"controlKey");}
   else if(r.structuredKey!=null){r.basis="STRUCTURED_PROPOSED";Flag(r,NeedsInput,"STRUCTURED_PROPOSED");AddOnce(r.missing,"controlKey");}
   else{Flag(r,NeedsInput,"TAG_MISSING");
    foreach(var d in new[]{a,b})if(d.tag.keys.Count==0){if(d.type!=Panel)AddOnce(r.missing,"tag:"+d.id);else if(d.Struct==null)AddOnce(r.missing,"panelCircuit:"+d.id);}}
   if(r.controlKey!=null){
    bool rel=new[]{a,b}.Any(d=>d.type==Lamp||d.type==Fan||d.type==Switch);
    if(!rel)r.controllerStatus="NOT_APPLICABLE";
    else{
     // Switches of this room carrying the channel within the row's structured scope (switches without structured data always count).
     var sw=c.devs.Values.Where(d=>d.type==Switch&&d.tag.keys.Contains(r.controlKey)&&(scope==null||d.Struct==null||O.Equals(d.Struct,scope))).Select(d=>d.id).OrderBy(x=>x,O).ToList();
     r.controllerIds.AddRange(sw);
     // NF-037-CHAIN-002: measured SWITCH -> SWITCH physical endpoint association (not an inferred two-way/multi-way electrical design). Allowed only when the
     // row is the whole nonbranch component with exactly these two switches and no other device, no rejected/crossing segment (simpleChain), exactly one
     // common token (controlKey), no structured conflict/partial data, no malformed tag, and the two row switches are the ONLY room switches carrying the
     // token in scope. Anything else keeps CHECK MULTI_SWITCH_SAME_CHANNEL. Wire assembly / conductors are never inferred: the completion model's declaration
     // stays required.
     bool switchPair=simpleChain&&a.type==Switch&&b.type==Switch&&sw.Count==2&&sw.Contains(a.id)&&sw.Contains(b.id)&&!a.tagBad&&!b.tagBad&&!a.partial&&!b.partial;
     if(switchPair){r.controllerStatus="SWITCH_PAIR_PHYSICAL";r.basis="PHYSICAL_ENDPOINT_TAG_TOKEN";AddOnce(r.diagnostics,"SWITCH_TO_SWITCH_MEASURED_CHAIN");}
     else if(sw.Count>1){r.controllerStatus="AMBIGUOUS";Flag(r,Check,"MULTI_SWITCH_SAME_CHANNEL");}
     else if(a.type==Switch||b.type==Switch)r.controllerStatus="IN_ROW";
     else if(sw.Count==1)r.controllerStatus="ROOM_CANDIDATE";
     else{r.controllerStatus="NONE";AddOnce(r.diagnostics,"CONTROLLER_NOT_FOUND");}}}
   return r;}

  static string DiagKey(RoomDiag037 d){return d.code+"\u001f"+d.subject+"\u001f"+J(d.segmentIds)+"\u001f"+J(d.relatedIds)+"\u001f"+d.message;}
  static void Finish(RoomResult037 res){
   var count=new Dictionary<string,int>(O);
   foreach(var id in res.rows.SelectMany(r=>r.segments.Select(s=>s.id)).Concat(res.diagnostics.SelectMany(d=>d.segmentIds))){int k;count.TryGetValue(id,out k);count[id]=k+1;}
   var accepted=new HashSet<string>(res.acceptedSegmentIds,O);
   foreach(var id in res.acceptedSegmentIds){int k;count.TryGetValue(id,out k);if(k==0)res.unaccountedSegmentIds.Add(id);else if(k>1)res.doubleCountedSegmentIds.Add(id);}
   foreach(var id in count.Keys)if(!accepted.Contains(id))res.foreignSegmentIds.Add(id);
   res.coverageOk=res.unaccountedSegmentIds.Count==0&&res.doubleCountedSegmentIds.Count==0&&res.foreignSegmentIds.Count==0;
   if(!res.coverageOk)Diag(res,Blocked,"COVERAGE_VIOLATION","room","Segment ownership broken: unaccounted "+J(res.unaccountedSegmentIds)+"; double "+J(res.doubleCountedSegmentIds)+"; foreign "+J(res.foreignSegmentIds)+".");
   foreach(var l in new[]{res.acceptedSegmentIds,res.rejectedSegmentIds,res.unaccountedSegmentIds,res.doubleCountedSegmentIds,res.foreignSegmentIds})l.Sort(string.CompareOrdinal);
   foreach(var d in res.diagnostics){d.segmentIds.Sort(string.CompareOrdinal);var rel=d.relatedIds.Distinct(O).OrderBy(x=>x,O).ToList();d.relatedIds.Clear();d.relatedIds.AddRange(rel);}
   res.rows.Sort((x,y)=>string.CompareOrdinal(x.rowKey,y.rowKey));
   res.diagnostics.Sort((x,y)=>string.CompareOrdinal(DiagKey(x),DiagKey(y)));
   int top=res.rows.Select(r=>Rank(r.status)).Concat(res.diagnostics.Select(d=>Rank(d.severity))).DefaultIfEmpty(0).Max();
   res.status=top>=2?Check:top==1?NeedsInput:Associated;}

  // ---- PATCH F037-02: ResolveControlPaths ----
  static void Raise(ref string status,List<string> codes,string sev,string code){if(Rank(sev)>Rank(status))status=sev;AddOnce(codes,code);}
  static void Reason(List<string> reasons,ref string code,ref string sev,string c2,string s2,string text){if(code==null)code=c2;if(Rank(s2)>Rank(sev))sev=s2;reasons.Add(text);}

  // Same strict validation as Associate. Each connected wire tree is resolved independently and only when ALL hold:
  // no rejected segment/device data, no node with 2+ devices, acyclic, fully inside the room (no CROSSING/UNKNOWN segment), every device on exactly one node,
  // exactly one SWITCH, and every other device a LAMP or FAN. Junctions of any degree are allowed. Otherwise one diagnostic owns the tree's inside segments.
  // In a resolved tree every LAMP/FAN target gets one logical path from the switch (unique tree path). The path is ASSOCIATED only when the switch and target
  // tags share exactly one token, the structured panel/circuit values do not differ, every intermediate device on the path carries that token without a
  // malformed tag or a differing panel/circuit, and no other switch in the room carries the token in the same scope. Missing target tags still yield a path
  // (NEEDS_INPUT), never a silent omission. Segments on no path -> NETWORK_UNASSIGNED_SEGMENTS. Association only; no conductor, conduit or quantity PASS.
  public static RoomNetwork037 ResolveControlPaths(RoomInput037 input){var net=new RoomNetwork037();var res=new RoomResult037();
   if(input==null){Diag(res,Blocked,"INPUT_NULL","input","No room input supplied.");FinishNet(net,res,null);return net;}
   net.roomId=input.roomId;net.units=input.units;
   var c=Prepare(input,res);
   var phys=new Dictionary<string,RoomPhysicalSegment037>(O);
   foreach(var g in c.segs){var p=new RoomPhysicalSegment037{id=g.s.id,nodeA=g.s.nodeA,nodeB=g.s.nodeB,cableCode=g.s.cableCode,boundary=g.s.boundary,evidence=g.s.evidence,length=g.s.length};
    if(c.unitsOk)p.lengthMetres=g.s.length/c.upm;phys[p.id]=p;net.physicalSegments.Add(p);}
   var devNodeCount=new Dictionary<string,int>(O);
   foreach(var ds in c.nodeDevs.Values)foreach(var d in ds){int k;devNodeCount.TryGetValue(d,out k);devNodeCount[d]=k+1;}
   var comp=new Dictionary<string,List<Seg>>(O);var nodesOf=new Dictionary<string,List<string>>(O);
   foreach(var g in c.segs){var r=Find(c,g.s.nodeA);List<Seg> l;if(!comp.TryGetValue(r,out l))comp[r]=l=new List<Seg>();l.Add(g);}
   foreach(var n in c.parent.Keys.ToList()){var r=Find(c,n);List<string> l;if(!nodesOf.TryGetValue(r,out l))nodesOf[r]=l=new List<string>();l.Add(n);}
   foreach(var r in comp.Keys.OrderBy(x=>x,O))
    NetComponent(c,net,phys,devNodeCount,comp[r].OrderBy(e=>e.s.id,O).ToList(),nodesOf[r].OrderBy(x=>x,O).ToList());
   Tail(c);FinishNet(net,res,c);return net;}

  static void NetComponent(Ctx c,RoomNetwork037 net,Dictionary<string,RoomPhysicalSegment037> phys,Dictionary<string,int> devNodeCount,List<Seg> edges,List<string> nodes){
   string cid="component:"+nodes[0];
   var acc=new SortedSet<string>(O);foreach(var e in edges){acc.Add(e.s.nodeA);acc.Add(e.s.nodeB);phys[e.s.id].componentId=cid;}
   var inside=edges.Where(e=>!e.excluded).ToList();
   var devIds=new SortedSet<string>(O);var devNode=new Dictionary<string,string>(O);
   foreach(var n in acc){SortedSet<string> ds;if(!c.nodeDevs.TryGetValue(n,out ds))continue;foreach(var d in ds){devIds.Add(d);devNode[d]=n;}}
   var reasons=new List<string>();string code=null,sev=Info;
   var tainted=nodes.Where(n=>c.taint.Contains(n)).ToList();
   if(tainted.Count>0)Reason(reasons,ref code,ref sev,"COMPONENT_INVALID_SEGMENT",Blocked,"touches rejected segment/device data at node(s) "+J(tainted));
   var multi=acc.Where(n=>DevCount(c,n)>1).ToList();
   if(multi.Count>0)Reason(reasons,ref code,ref sev,"COMPONENT_MULTI_DEVICE_NODE",Blocked,"node(s) "+J(multi)+" carry more than one distinct device");
   if(edges.Count>=acc.Count)Reason(reasons,ref code,ref sev,"COMPONENT_CYCLE",Blocked,"contains a closed loop ("+edges.Count+" segments, "+acc.Count+" nodes)");
   var crossing=edges.Where(e=>e.excluded).Select(e=>e.s.id).ToList();
   if(crossing.Count>0)Reason(reasons,ref code,ref sev,"NETWORK_BOUNDARY_CROSSING",NeedsInput,"reaches boundary-excluded segment(s) "+J(crossing)+"; the tree is not complete inside the room");
   var multiNode=devIds.Where(d=>devNodeCount[d]>1).ToList();
   if(multiNode.Count>0)Reason(reasons,ref code,ref sev,"NETWORK_DEVICE_MULTI_NODE",Check,"device(s) "+J(multiNode)+" mapped to more than one node; path ends are ambiguous");
   var switches=devIds.Where(d=>c.devs[d].type==Switch).ToList();
   if(code==null){
    if(switches.Count==0)Reason(reasons,ref code,ref sev,"NETWORK_NO_CONTROLLER",NeedsInput,"no SWITCH in this wire tree; no control path resolved (Associate() chain rows remain the candidate view)");
    else if(switches.Count>1)Reason(reasons,ref code,ref sev,"NETWORK_COMPETING_CONTROLLERS",Check,"switches "+J(switches)+" share one wire tree; competing/two-way control needs manual review");
    else{var bad=devIds.Where(d=>d!=switches[0]&&!(c.devs[d].typeOk&&(c.devs[d].type==Lamp||c.devs[d].type==Fan))).ToList();
     if(bad.Count>0)Reason(reasons,ref code,ref sev,"NETWORK_UNSUPPORTED_PATTERN",NeedsInput,"device(s) "+J(bad)+" are not LAMP/FAN targets (socket/panel/other/invalid type); switch semantics are not fabricated");}}
   if(code!=null){var dg=Diag(c.res,sev,code,cid,"Wire tree at nodes "+J(acc)+": "+string.Join("; ",reasons)+". Its "+inside.Count+" inside segment(s) get no logical path.");
    foreach(var e in inside)dg.segmentIds.Add(e.s.id);dg.relatedIds.AddRange(devIds);return;}
   var S=c.devs[switches[0]];string sNode=devNode[S.id];
   var prev=new Dictionary<string,Seg>(O);prev[sNode]=null;var q=new Queue<string>();q.Enqueue(sNode);
   while(q.Count>0){var at=q.Dequeue();foreach(var e in c.adj[at]){var m=OtherNode(e,at);if(prev.ContainsKey(m))continue;prev[m]=e;q.Enqueue(m);}}
   foreach(var tid in devIds){if(O.Equals(tid,S.id))continue;var T=c.devs[tid];
    var p=new RoomLogicalPath037{pathId="P:"+S.id+"->"+tid,status=Associated,basis="NONE",fromId=S.id,toId=tid,fromLabel=Label(S),toLabel=Label(T)};
    var segs=new List<Seg>();var pn=new List<string>();string cur=devNode[tid];pn.Add(cur);
    while(!O.Equals(cur,sNode)){var e=prev[cur];segs.Add(e);cur=OtherNode(e,cur);pn.Add(cur);}
    segs.Reverse();pn.Reverse();
    double sum=0;foreach(var g in segs){sum+=g.s.length;p.segmentIds.Add(g.s.id);}
    p.nodeIds.AddRange(pn);p.drawingLength=sum;if(c.unitsOk)p.planMetres=sum/c.upm;
    if(Blank(c.input.roomId)){Raise(ref p.status,p.diagnostics,NeedsInput,"ROOM_ID_MISSING");AddOnce(p.missing,"roomId");}
    if(!c.unitsOk){Raise(ref p.status,p.diagnostics,NeedsInput,"UNITS_INVALID");AddOnce(p.missing,"units");}
    foreach(var d in new[]{S,T}){if(d.partial){Raise(ref p.status,p.diagnostics,Check,"STRUCTURED_PARTIAL");AddOnce(p.missing,"panelCircuit:"+d.id);}
     if(d.tagBad)Raise(ref p.status,p.diagnostics,Check,"TAG_MALFORMED");}
    bool conflict=S.Struct!=null&&T.Struct!=null&&!O.Equals(S.Struct,T.Struct);
    if(conflict)Raise(ref p.status,p.diagnostics,Check,"STRUCTURED_CONFLICT");else if(S.Struct!=null&&T.Struct!=null)p.structuredKey=S.StructText;
    var common=S.tag.keys.Where(k=>T.tag.keys.Contains(k)).ToList();
    if(conflict){}
    else if(S.tag.keys.Count==0||T.tag.keys.Count==0){Raise(ref p.status,p.diagnostics,NeedsInput,"TAG_MISSING");
     foreach(var d in new[]{S,T})if(d.tag.keys.Count==0)AddOnce(p.missing,"tag:"+d.id);}
    else if(common.Count==1){p.controlKey=common[0];p.controlLabel=S.tag.tokens[S.tag.keys.IndexOf(p.controlKey)];p.basis="TAG_TOKEN";}
    else if(common.Count>1){Raise(ref p.status,p.diagnostics,Check,"MULTIPLE_COMMON_TOKENS");AddOnce(p.missing,"controlKey");}
    else{Raise(ref p.status,p.diagnostics,Check,"TAG_NO_COMMON_TOKEN");AddOnce(p.missing,"controlKey");}
    // Passing through another lamp/fan is allowed only when it carries the same channel (and is otherwise consistent).
    for(int i=1;i<pn.Count-1;i++){SortedSet<string> ds;if(!c.nodeDevs.TryGetValue(pn[i],out ds))continue;var D=c.devs[ds.Min];p.intermediateDeviceIds.Add(D.id);
     bool ok=!D.tagBad&&!(D.Struct!=null&&S.Struct!=null&&!O.Equals(D.Struct,S.Struct))&&(p.controlKey==null||D.tag.keys.Contains(p.controlKey));
     if(!ok)Raise(ref p.status,p.diagnostics,Check,"INTERMEDIATE_DEVICE_CONFLICT");}
    if(p.controlKey!=null){string scope=S.Struct??T.Struct;string key=p.controlKey;
     bool rival=c.devs.Values.Any(d=>d.type==Switch&&!O.Equals(d.id,S.id)&&d.tag.keys.Contains(key)&&(scope==null||d.Struct==null||O.Equals(d.Struct,scope)));
     if(rival)Raise(ref p.status,p.diagnostics,Check,"MULTI_SWITCH_SAME_CHANNEL");}
    foreach(var g in segs)phys[g.s.id].pathIds.Add(p.pathId);
    net.logicalPaths.Add(p);}
   var left=inside.Where(e=>phys[e.s.id].pathIds.Count==0).Select(e=>e.s.id).ToList();
   if(left.Count>0){var ud=Diag(c.res,NeedsInput,"NETWORK_UNASSIGNED_SEGMENTS",cid,"Segment(s) "+J(left)+" of the wire tree controlled by "+S.id+" lie on no controller->target path (open/dangling ends); not associated.");
    ud.segmentIds.AddRange(left);ud.relatedIds.Add(S.id);}}

  static void FinishNet(RoomNetwork037 net,RoomResult037 res,Ctx c){
   var st=new Dictionary<string,string>(O);foreach(var p in net.logicalPaths)st[p.pathId]=p.status;
   var own=new Dictionary<string,int>(O);foreach(var d in res.diagnostics)foreach(var id in d.segmentIds){int k;own.TryGetValue(id,out k);own[id]=k+1;}
   var accepted=new HashSet<string>(res.acceptedSegmentIds,O);
   bool ok=net.physicalSegments.Count==accepted.Count&&net.physicalSegments.All(s=>accepted.Contains(s.id));
   double sum=0;bool any=false;
   foreach(var s in net.physicalSegments.OrderBy(x=>x.id,O)){s.pathIds.Sort(string.CompareOrdinal);int k;own.TryGetValue(s.id,out k);
    if(s.pathIds.Count>0){any=true;sum+=s.length;if(k!=0)ok=false;s.status=s.pathIds.Any(pid=>st[pid]==Associated)?"ASSIGNED":"CANDIDATE";}
    else{if(k!=1)ok=false;if(s.boundary==Inside){s.status="UNASSIGNED";net.unassignedSegmentIds.Add(s.id);}else s.status="EXCLUDED";}}
   foreach(var id in own.Keys)if(!accepted.Contains(id))ok=false;
   net.coverageOk=ok;
   if(!ok)Diag(res,Blocked,"COVERAGE_VIOLATION","room","Physical segment ownership broken: every accepted segment needs path membership or exactly one owning diagnostic.");
   net.physicalLengthMetres=c!=null&&c.unitsOk&&any?(double?)(sum/c.upm):null;
   net.acceptedSegmentIds.AddRange(res.acceptedSegmentIds.OrderBy(x=>x,O));net.rejectedSegmentIds.AddRange(res.rejectedSegmentIds.OrderBy(x=>x,O));
   foreach(var d in res.diagnostics){d.segmentIds.Sort(string.CompareOrdinal);var rel=d.relatedIds.Distinct(O).OrderBy(x=>x,O).ToList();d.relatedIds.Clear();d.relatedIds.AddRange(rel);}
   res.diagnostics.Sort((x,y)=>string.CompareOrdinal(DiagKey(x),DiagKey(y)));net.diagnostics.AddRange(res.diagnostics);
   net.physicalSegments.Sort((x,y)=>string.CompareOrdinal(x.id,y.id));net.logicalPaths.Sort((x,y)=>string.CompareOrdinal(x.pathId,y.pathId));
   int top=net.logicalPaths.Select(p=>Rank(p.status)).Concat(net.diagnostics.Select(d=>Rank(d.severity))).DefaultIfEmpty(0).Max();
   if(net.unassignedSegmentIds.Count>0&&top<1)top=1;
   net.status=top>=2?Check:top==1?NeedsInput:Associated;}

  // ---- S6 (additive, display only): compact room-local device labels. Never persisted, never an identity, never written back to CAD. ----
  // Label = short Vietnamese type code + the device's own visible TAG text: SWITCH "CT", LAMP "D-stroke" (U+0110), SOCKET "OC", FAN "Q",
  // PANEL "T"+U+0110, OTHER "TB", invalid/unknown type "?"; e.g. CT1,2 / (U+0110)1 / OC3 / Q2. No TAG -> "<code>(kh(U+00F4)ng TAG)": the TAG stays reported
  // missing by the existing TAG_MISSING reasons and is never invented. Display only: whitespace/control runs in the TAG become one space, unusable
  // characters "?", and a TAG longer than MaxTagShown is shortened with "...". Devices of the room (input.devices, distinct non-blank IDs) sharing one
  // base label get "-01", "-02", ... in ordinal device-ID order (display ordinal from the room's device set, independent of input order). A label
  // that still collides (e.g. TAG "1" twice plus TAG "1-01") gets " #<device id>" on every colliding device; any remaining collision falls back to
  // "M(U+00E3) <device id>" (no type code starts with "M"). Different device IDs therefore never share a label. Result: device ID -> label.
  // Non-ASCII letters are built from char codes so that this file stays ASCII.
  public const int MaxTagShown=40;
  static readonly string DStroke=((char)0x0110).ToString(),NoTag="(kh"+(char)0x00F4+"ng TAG)",IdWord="M"+(char)0x00E3+" ";
  public static string TypeCode(string type){switch(type){case Switch:return "CT";case Lamp:return DStroke;case Socket:return "OC";case Fan:return "Q";
   case Panel:return "T"+DStroke;case Other:return "TB";default:return "?";}}
  static string TagShown(string text){if(Blank(text))return null;var b=new StringBuilder();bool gap=false;
   foreach(char ch in text.Trim()){
    switch(char.GetUnicodeCategory(ch)){
     case UnicodeCategory.Control:case UnicodeCategory.Format:case UnicodeCategory.LineSeparator:case UnicodeCategory.ParagraphSeparator:case UnicodeCategory.SpaceSeparator:
      gap=true;continue;
     case UnicodeCategory.PrivateUse:case UnicodeCategory.OtherNotAssigned:case UnicodeCategory.Surrogate:
      if(gap&&b.Length>0)b.Append(' ');gap=false;b.Append('?');continue;}
    if(gap&&b.Length>0)b.Append(' ');gap=false;b.Append(ch);}
   var s=b.ToString();if(s.Length==0)return null;return s.Length>MaxTagShown?s.Substring(0,MaxTagShown)+"...":s;}
  static string DevKey(RoomDevice037 d){return (d.type??"")+"\u001f"+(d.name??"")+"\u001f"+(d.tagText??"")+"\u001f"+(d.panel??"")+"\u001f"+(d.circuit??"");}
  public static Dictionary<string,string> DisplayLabels(RoomInput037 input){var by=new SortedDictionary<string,RoomDevice037>(O);
   if(input!=null&&input.devices!=null)foreach(var d in input.devices){if(d==null||Blank(d.id))continue;RoomDevice037 o;
    if(!by.TryGetValue(d.id,out o)||string.CompareOrdinal(DevKey(d),DevKey(o))<0)by[d.id]=d;}   // conflicting duplicates: deterministic pick
   var res=new Dictionary<string,string>(O);
   foreach(var g in by.GroupBy(kv=>TypeCode(kv.Value.type)+(TagShown(kv.Value.tagText)??NoTag),O)){
    var ids=g.Select(kv=>kv.Key).OrderBy(x=>x,O).ToList();if(ids.Count==1){res[ids[0]]=g.Key;continue;}
    string fmt="D"+Math.Max(2,ids.Count.ToString(CultureInfo.InvariantCulture).Length).ToString(CultureInfo.InvariantCulture);
    for(int i=0;i<ids.Count;i++)res[ids[i]]=g.Key+"-"+(i+1).ToString(fmt,CultureInfo.InvariantCulture);}
   foreach(var g in res.ToList().GroupBy(kv=>kv.Value,O).Where(x=>x.Count()>1).ToList())foreach(var kv in g)res[kv.Key]=kv.Value+" #"+kv.Key;
   foreach(var g in res.ToList().GroupBy(kv=>kv.Value,O).Where(x=>x.Count()>1).ToList())foreach(var kv in g)res[kv.Key]=IdWord+kv.Key;
   return res;}

  // Canonical text of a network (QA determinism / review).
  public static string DescribeNetwork(RoomNetwork037 n){var s=new StringBuilder();
   s.Append("NETWORK|").Append(n.status).Append('|').Append(n.roomId).Append('|').Append(n.units).Append("|coverage=").Append(n.coverageOk?"OK":"FAIL")
    .Append("|physicalM=").Append(n.physicalLengthMetres.HasValue?N(n.physicalLengthMetres.Value):"").Append("|unassigned=").Append(J(n.unassignedSegmentIds))
    .Append("|rejected=").Append(J(n.rejectedSegmentIds)).Append('\n');
   foreach(var p in n.logicalPaths)s.Append("PATH|").Append(p.pathId).Append('|').Append(p.status).Append('|').Append(p.basis).Append('|').Append(p.fromLabel).Append(" -> ").Append(p.toLabel)
    .Append("|ctl=").Append(p.controlKey).Append("|struct=").Append(p.structuredKey).Append("|len=").Append(N(p.drawingLength)).Append("|m=").Append(p.planMetres.HasValue?N(p.planMetres.Value):"")
    .Append("|segs=").Append(J(p.segmentIds)).Append("|nodes=").Append(J(p.nodeIds)).Append("|via=").Append(J(p.intermediateDeviceIds)).Append("|missing=").Append(J(p.missing)).Append("|diag=").Append(J(p.diagnostics)).Append('\n');
   foreach(var g in n.physicalSegments)s.Append("SEG|").Append(g.id).Append('|').Append(g.status).Append('|').Append(g.componentId).Append("|len=").Append(N(g.length))
    .Append("|m=").Append(g.lengthMetres.HasValue?N(g.lengthMetres.Value):"").Append("|paths=").Append(J(g.pathIds)).Append('\n');
   foreach(var d in n.diagnostics)s.Append("DIAG|").Append(d.severity).Append('|').Append(d.code).Append('|').Append(d.subject).Append("|segs=").Append(J(d.segmentIds))
    .Append("|rel=").Append(J(d.relatedIds)).Append('|').Append(d.message).Append('\n');
   return s.ToString();}

  // Canonical text of a result (QA determinism / review). Same input set in any order -> identical text.
  public static string Describe(RoomResult037 r){var s=new StringBuilder();
   s.Append("RESULT|").Append(r.status).Append('|').Append(r.roomId).Append('|').Append(r.units).Append("|coverage=").Append(r.coverageOk?"OK":"FAIL")
    .Append("|accepted=").Append(J(r.acceptedSegmentIds)).Append("|rejected=").Append(J(r.rejectedSegmentIds)).Append('\n');
   foreach(var w in r.rows)s.Append("ROW|").Append(w.rowKey).Append('|').Append(w.status).Append('|').Append(w.basis).Append('|').Append(w.fromLabel).Append(" -> ").Append(w.toLabel)
    .Append("|ctl=").Append(w.controlKey).Append("|cand=").Append(J(w.candidateKeys)).Append("|struct=").Append(w.structuredKey).Append("|controller=").Append(w.controllerStatus).Append(':').Append(J(w.controllerIds))
    .Append("|len=").Append(N(w.drawingLength)).Append("|m=").Append(w.planMetres.HasValue?N(w.planMetres.Value):"").Append("|segs=").Append(J(w.SegmentIds())).Append("|nodes=").Append(J(w.nodeIds))
    .Append("|cable=").Append(J(w.cableCodes)).Append("|missing=").Append(J(w.missing)).Append("|diag=").Append(J(w.diagnostics)).Append('\n');
   foreach(var d in r.diagnostics)s.Append("DIAG|").Append(d.severity).Append('|').Append(d.code).Append('|').Append(d.subject).Append("|segs=").Append(J(d.segmentIds))
    .Append("|rel=").Append(J(d.relatedIds)).Append('|').Append(d.message).Append('\n');
   return s.ToString();}
 }
}
