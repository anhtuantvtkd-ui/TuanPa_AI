using System;
using System.Collections.Generic;
using System.Linq;
using IOE=System.InvalidOperationException;

namespace Tvxd.Meps {
 // TVXD-MEPS-ROOMQTY-037 HIERARCHY S1 PILOT: PURE project > floor > room identity and conservative multi-snapshot aggregation.
 // Standalone: System + collections + Linq only; no CAD, Forms, JSON or IO; C# 5; ASCII. NOT connected to any UI, store or export yet.
 //  - Stable IDs "PJ_"/"FL_"/"RM_" + Guid "N" (32 hex) from the caller's Func<Guid> (injectable for tests). IDs never derive from or change with names.
 //    An empty Guid, or a Guid part already used anywhere in the project (any prefix), is refused before any mutation.
 //  - Names: display text kept exactly as typed. Refused: null/blank, control characters, invalid Unicode, more than MaxText chars.
 //    Duplicates are checked only within the immediate parent (floors in a project, rooms in a floor) on the key Trim + NFC + upper-invariant,
 //    so "WC" may exist on two floors but "WC"/"wc "/composed-vs-decomposed forms are one name on the same floor.
 //  - Every mutating call validates the whole project graph and the request first; an invalid request throws InvalidOperationException
 //    (ArgumentNullException for a null id factory) and leaves the graph unchanged. No height or other engineering value is ever defaulted.
 //  - AssignLegacy: the caller must name the floor (never inferred). It only records the import identity marker (legacyKey + legacyHash, stored
 //    verbatim); it does NOT migrate geometry or declarations. Same key + hash + floor + name again = the same room (idempotent); any other
 //    combination for an already-imported key throws.
 //  - Aggregate never mutates its inputs and never subtracts or allocates. It refuses (as issues) foreign drawings, unknown floor/room
 //    references, duplicate snapshot IDs, stale or incomplete snapshots, unowned crossing sources, invalid quantity rows, and any source or
 //    device identity (drawing + handle, case-insensitive) claimed by two snapshots. Valid quantity rows still appear as PROVISIONAL totals,
 //    summed separately per level (PROJECT / FLOOR / ROOM) and exact (kind, code, unit); different materials or units are never added together.
 //    official = no issue at all and at least one snapshot. Pilot rule: every total carries the project-wide official flag (one issue anywhere
 //    makes every level non-official), which is conservative by design. pending = snapshots in that scope that are not clean.
 //    Snapshot identities, fresh and complete are supplied by the caller from an actual verified scan; this file cannot prove them.
 //    Aggregate official is NOT a product FINAL PASS.
 public sealed class HierProject037 {public string id,name,drawingId;public List<HierFloor037> floors=new List<HierFloor037>();}
 // S2 additive fields (old data without them stays valid and unchanged):
 //  - HierFloor037.mountingCeilingHeight / HierRoom037.mountingCeilingOverride: optional DEVICE-MOUNTING ceiling reference, finite > 0 when entered.
 //    It is a separate concept from storeyHeight and wireCeilingHeight/wireCeilingOverride; none of them substitutes for another. Never defaulted.
 //  - HierRoom037.code: optional human-facing room CODE (null for old data). Every non-null code must be acceptable text and unique across the
 //    WHOLE project (one DWG) on NameKey. The name stays a free description. Code changes never change the stable room id.
 // S4: on one floor, two CODED rooms may carry the same description (the code identifies them); a room WITHOUT code (legacy AddRoom/AssignLegacy) is
 //    identified by its name, so its name must stay unique on the floor against every room. AddRoom/AssignLegacy (uncoded) keep their checks;
 //    AddCodedRoom creates a coded room; RenameRoom of a coded room only collides with uncoded rooms. Names are never suffixed or changed.
 public sealed class HierFloor037 {public string id,name;public int order;public double? storeyHeight,wireCeilingHeight,mountingCeilingHeight;public List<HierRoom037> rooms=new List<HierRoom037>();}
 public sealed class HierRoom037 {public string id,name,code;public double? wireCeilingOverride,mountingCeilingOverride;public string legacyKey,legacyHash;}
 public sealed class HierQuantity037 {public string kind,code,unit;public double value;}
 // acceptedSources: whole native source entity handles within drawingId (never scan-local segment IDs). usedDevices: device handles used by rows.
 public sealed class HierZoneSnapshot037 {
  public string id,drawingId,floorId,roomId;public bool fresh,complete;
  public List<string> acceptedSources=new List<string>(),usedDevices=new List<string>(),unownedCrossingSources=new List<string>();
  public List<HierQuantity037> quantities=new List<HierQuantity037>();
 }
 public sealed class HierTotal037 {public string level,scopeId,kind,code,unit;public double value;public bool official;public int pending;}
 public sealed class HierAggregate037 {public bool official;public List<string> issues=new List<string>();public List<HierTotal037> totals=new List<HierTotal037>();}

 public static class RoomHierarchy037 {
  public const string ProjectPrefix="PJ_",FloorPrefix="FL_",RoomPrefix="RM_";
  public const string Wire="WIRE",Conduit="CONDUIT",LevelProject="PROJECT",LevelFloor="FLOOR",LevelRoom="ROOM";
  public const int MaxText=200;
  const char US='\u001f';
  static readonly StringComparer O=StringComparer.Ordinal,OI=StringComparer.OrdinalIgnoreCase;

  static bool Blank(string s){return string.IsNullOrWhiteSpace(s);}
  static string Show(string s){return s==null?"(null)":"\""+s+"\"";}

  // null = acceptable text; otherwise the reason.
  public static string TextProblem(string s,string what){
   if(Blank(s))return what+" is empty.";
   if(s.Length>MaxText)return what+" is longer than "+MaxText+" characters.";
   foreach(var c in s)if(char.IsControl(c))return what+" contains a control character.";
   try{s.Normalize(System.Text.NormalizationForm.FormC);}catch(ArgumentException){return what+" is not valid Unicode.";}
   return null;}
  // Duplicate-comparison key only; the display text is never changed.
  public static string NameKey(string s){return s.Trim().Normalize(System.Text.NormalizationForm.FormC).ToUpperInvariant();}
  static bool IsHex(char c){return (c>='0'&&c<='9')||(c>='a'&&c<='f')||(c>='A'&&c<='F');}
  static string IdProblem(string id,string prefix){
   if(id==null||!id.StartsWith(prefix,StringComparison.Ordinal)||id.Length!=prefix.Length+32||!id.Skip(prefix.Length).All(IsHex))
    return "ID "+Show(id)+" is not "+prefix+"<32 hex>.";
   if(id.Skip(prefix.Length).All(c=>c=='0'))return "ID "+id+" uses the empty Guid.";
   return null;}
  static string GuidPart(string id){return id.Substring(3).ToLowerInvariant();}
  static string HeightProblem(double? v,string what){
   if(!v.HasValue)return null;
   if(double.IsNaN(v.Value)||double.IsInfinity(v.Value)||v.Value<=0)return what+" must be a finite number > 0 when entered.";
   return null;}

  // Whole-graph structural validation. null = valid.
  public static string Problem(HierProject037 p){
   if(p==null)return "Project is null.";
   var t=IdProblem(p.id,ProjectPrefix);if(t!=null)return "Project "+t;
   t=TextProblem(p.name,"Project name");if(t!=null)return t;
   t=TextProblem(p.drawingId,"Drawing identity");if(t!=null)return t;
   if(p.floors==null)return "Project floor list is null.";
   var guids=new HashSet<string>(O){GuidPart(p.id)};var floorNames=new HashSet<string>(O);var legacy=new HashSet<string>(O);var codes=new HashSet<string>(O);
   foreach(var f in p.floors){
    if(f==null)return "A floor entry is null.";
    t=IdProblem(f.id,FloorPrefix);if(t!=null)return "Floor "+t;
    if(!guids.Add(GuidPart(f.id)))return "Identity "+f.id+" is used twice.";
    t=TextProblem(f.name,"Floor name");if(t!=null)return t+" (floor "+f.id+")";
    if(!floorNames.Add(NameKey(f.name)))return "Floor name "+Show(f.name)+" is used twice in the project.";
    t=HeightProblem(f.storeyHeight,"Floor storeyHeight");if(t!=null)return t+" (floor "+f.id+")";
    t=HeightProblem(f.wireCeilingHeight,"Floor wireCeilingHeight");if(t!=null)return t+" (floor "+f.id+")";
    t=HeightProblem(f.mountingCeilingHeight,"Floor mountingCeilingHeight");if(t!=null)return t+" (floor "+f.id+")";
    if(f.rooms==null)return "Room list of floor "+f.id+" is null.";
    var roomNames=new Dictionary<string,bool>(O);   // name key -> some room with this name has no code
    foreach(var r in f.rooms){
     if(r==null)return "A room entry of floor "+f.id+" is null.";
     t=IdProblem(r.id,RoomPrefix);if(t!=null)return "Room "+t;
     if(!guids.Add(GuidPart(r.id)))return "Identity "+r.id+" is used twice.";
     t=TextProblem(r.name,"Room name");if(t!=null)return t+" (room "+r.id+")";
     var nk=NameKey(r.name);bool uncoded;
     if(roomNames.TryGetValue(nk,out uncoded)&&(uncoded||r.code==null))return "Room name "+Show(r.name)+" is used twice on floor "+Show(f.name)+".";
     roomNames[nk]=r.code==null;   // reached only when the key is new or every room so far with it (and this one) is coded
     t=HeightProblem(r.wireCeilingOverride,"Room wireCeilingOverride");if(t!=null)return t+" (room "+r.id+")";
     t=HeightProblem(r.mountingCeilingOverride,"Room mountingCeilingOverride");if(t!=null)return t+" (room "+r.id+")";
     if(r.code!=null){t=TextProblem(r.code,"Room code");if(t!=null)return t+" (room "+r.id+")";
      if(!codes.Add(NameKey(r.code)))return "Room code "+Show(r.code)+" is used twice in the project.";}
     if(r.legacyKey!=null||r.legacyHash!=null){
      t=TextProblem(r.legacyKey,"Legacy key");if(t==null)t=TextProblem(r.legacyHash,"Legacy hash");if(t!=null)return t+" (room "+r.id+")";
      if(!legacy.Add(r.legacyKey))return "Legacy key "+Show(r.legacyKey)+" is imported twice.";}}}
   return null;}
  static void Check(HierProject037 p){var e=Problem(p);if(e!=null)throw new IOE("Hierarchy is invalid: "+e+" Nothing changed.");}
  static void Name(string name,string what){var t=TextProblem(name,what);if(t!=null)throw new IOE(t+" Nothing changed.");}
  static IEnumerable<string> Ids(HierProject037 p){
   if(p==null)yield break;yield return p.id;
   foreach(var f in p.floors){yield return f.id;foreach(var r in f.rooms)yield return r.id;}}
  // New ID; the factory is called once and its result checked before the caller mutates anything.
  static string NewId(Func<Guid> ids,string prefix,HierProject037 p){
   if(ids==null)throw new ArgumentNullException("ids");
   var g=ids();if(g==Guid.Empty)throw new IOE("The ID factory returned the empty Guid. Nothing changed.");
   var id=prefix+g.ToString("N");var part=GuidPart(id);
   if(Ids(p).Any(x=>GuidPart(x)==part))throw new IOE("The ID factory returned Guid "+part+" that is already used in the project. Nothing changed.");
   return id;}
  static HierFloor037 Floor(HierProject037 p,string floorId){
   if(Blank(floorId))throw new IOE("A floor must be chosen explicitly; MEPS never picks a floor. Nothing changed.");
   var f=p.floors.FirstOrDefault(x=>O.Equals(x.id,floorId));
   if(f==null)throw new IOE("Floor "+Show(floorId)+" does not exist in the project. Nothing changed.");
   return f;}

  public static HierProject037 Create(string projectName,string drawingId,Func<Guid> ids){
   Name(projectName,"Project name");Name(drawingId,"Drawing identity");
   var p=new HierProject037{id=NewId(ids,ProjectPrefix,null),name=projectName,drawingId=drawingId};
   Check(p);return p;}

  public static HierFloor037 AddFloor(HierProject037 p,string name,Func<Guid> ids){
   Check(p);Name(name,"Floor name");var k=NameKey(name);
   if(p.floors.Any(f=>NameKey(f.name)==k))throw new IOE("Floor name "+Show(name)+" already exists in the project. Nothing changed.");
   int order=1;if(p.floors.Count>0){int max=p.floors.Max(f=>f.order);if(max==int.MaxValue)throw new IOE("Floor order is exhausted. Nothing changed.");order=Math.Max(1,max+1);}
   var fl=new HierFloor037{id=NewId(ids,FloorPrefix,p),name=name,order=order};
   p.floors.Add(fl);return fl;}

  public static HierRoom037 AddRoom(HierProject037 p,string floorId,string name,Func<Guid> ids){
   Check(p);var f=Floor(p,floorId);Name(name,"Room name");var k=NameKey(name);
   if(f.rooms.Any(r=>NameKey(r.name)==k))throw new IOE("Room name "+Show(name)+" already exists on floor "+Show(f.name)+". Nothing changed.");
   var rm=new HierRoom037{id=NewId(ids,RoomPrefix,p),name=name};
   f.rooms.Add(rm);return rm;}

  public static HierFloor037 RenameFloor(HierProject037 p,string id,string name){
   Check(p);var f=Floor(p,id);Name(name,"Floor name");var k=NameKey(name);
   if(p.floors.Any(x=>x!=f&&NameKey(x.name)==k))throw new IOE("Floor name "+Show(name)+" already exists in the project. Nothing changed.");
   f.name=name;return f;}

  public static HierRoom037 RenameRoom(HierProject037 p,string floorId,string id,string name){
   Check(p);var f=Floor(p,floorId);
   var r=Blank(id)?null:f.rooms.FirstOrDefault(x=>O.Equals(x.id,id));
   if(r==null)throw new IOE("Room "+Show(id)+" does not exist on floor "+Show(f.name)+". Nothing changed.");
   Name(name,"Room name");var k=NameKey(name);
   // S4: a coded room may share its description with other CODED rooms of the floor; an uncoded (legacy) room is identified by its name.
   if(f.rooms.Any(x=>x!=r&&NameKey(x.name)==k&&(r.code==null||x.code==null)))throw new IOE("Room name "+Show(name)+" already exists on floor "+Show(f.name)+". Nothing changed.");
   r.name=name;return r;}

  // S4 coded room: code must be acceptable text and unique across the WHOLE project (NameKey); the description may repeat among coded rooms of the
  // same floor but not equal an uncoded (legacy, name-identified) room of that floor. Validates everything first; nothing changes on refusal.
  public static HierRoom037 AddCodedRoom(HierProject037 p,string floorId,string code,string name,Func<Guid> ids){
   Check(p);var f=Floor(p,floorId);Name(name,"Room name");Name(code,"Room code");var k=NameKey(name);var ck=NameKey(code);
   var o=p.floors.SelectMany(x=>x.rooms).FirstOrDefault(x=>x.code!=null&&NameKey(x.code)==ck);
   if(o!=null)throw new IOE("Room code "+Show(code)+" is already used by room "+Show(o.name)+" ("+o.id+"). Nothing changed.");
   if(f.rooms.Any(x=>x.code==null&&NameKey(x.name)==k))throw new IOE("Room name "+Show(name)+" is used by a room without code on floor "+Show(f.name)+". Nothing changed.");
   var rm=new HierRoom037{id=NewId(ids,RoomPrefix,p),name=name,code=code};
   f.rooms.Add(rm);return rm;}

  // Explicit placement of one legacy single-room draft (identity marker only).
  public static HierRoom037 AssignLegacy(HierProject037 p,string floorId,string roomName,string legacyKey,string legacyHash,Func<Guid> ids){
   Check(p);var f=Floor(p,floorId);Name(roomName,"Room name");Name(legacyKey,"Legacy key");Name(legacyHash,"Legacy hash");
   var k=NameKey(roomName);
   foreach(var pf in p.floors)foreach(var r in pf.rooms){if(!O.Equals(r.legacyKey,legacyKey))continue;
    if(pf==f&&O.Equals(r.legacyHash,legacyHash)&&NameKey(r.name)==k)return r;
    if(!O.Equals(r.legacyHash,legacyHash))throw new IOE("Legacy draft "+legacyKey+" was already imported with a different content hash (room "+r.id+"); the old draft changed. Nothing changed.");
    throw new IOE("Legacy draft "+legacyKey+" is already imported as room "+Show(r.name)+" on floor "+Show(pf.name)+" ("+r.id+"); a different target is refused. Nothing changed.");}
   if(f.rooms.Any(x=>NameKey(x.name)==k))throw new IOE("Room name "+Show(roomName)+" already exists on floor "+Show(f.name)+"; choose another name. Nothing changed.");
   var rm=new HierRoom037{id=NewId(ids,RoomPrefix,p),name=roomName,legacyKey=legacyKey,legacyHash=legacyHash};
   f.rooms.Add(rm);return rm;}

  // ---- Aggregation ----
  sealed class Z {public HierZoneSnapshot037 s;public string label,floorId,roomId;public bool valid,clean;public readonly List<HierQuantity037> good=new List<HierQuantity037>();}
  sealed class Use {public Z z;public string kind,handle;}
  static string QuantityProblem(HierQuantity037 q){
   if(q==null)return "row is null";
   if(q.kind!=Wire&&q.kind!=Conduit)return "kind "+Show(q.kind)+" is not WIRE or CONDUIT";
   var t=TextProblem(q.code,"code");if(t!=null)return t;
   t=TextProblem(q.unit,"unit");if(t!=null)return t;
   if(double.IsNaN(q.value)||double.IsInfinity(q.value)||q.value<0)return "value is not a finite number >= 0";
   return null;}

  public static HierAggregate037 Aggregate(HierProject037 p,IList<HierZoneSnapshot037> zones){
   var a=new HierAggregate037();var iss=a.issues;
   var hp=Problem(p);
   if(hp!=null)iss.Add("HIERARCHY_INVALID: "+hp+" No snapshot contributes.");
   if(zones==null){iss.Add("SNAPSHOTS_NULL: no snapshot list; nothing is official.");return a;}
   if(zones.Count==0)iss.Add("NO_SNAPSHOTS: an empty selection is never official.");
   if(hp!=null)return a;
   var floors=new Dictionary<string,HierFloor037>(O);var roomFloor=new Dictionary<string,string>(O);
   foreach(var f in p.floors){floors[f.id]=f;foreach(var r in f.rooms)roomFloor[r.id]=f.id;}
   var idCount=new Dictionary<string,int>(OI);
   foreach(var z in zones)if(z!=null&&!Blank(z.id)){int n;idCount.TryGetValue(z.id,out n);idCount[z.id]=n+1;}
   var states=new List<Z>();int nulls=0;
   for(int i=0;i<zones.Count;i++){var z=zones[i];
    if(z==null){nulls++;iss.Add("SNAPSHOT_NULL: entry #"+i+" is null.");continue;}
    var st=new Z{s=z,label=Blank(z.id)?"snapshot #"+i:"snapshot "+Show(z.id)};states.Add(st);bool ok=true;
    if(Blank(z.id)){iss.Add("SNAPSHOT_ID_MISSING: "+st.label+" has no ID; not counted.");ok=false;}
    else if(idCount[z.id]>1){iss.Add("DUPLICATE_SNAPSHOT_ID: "+st.label+" occurs "+idCount[z.id]+" times; none of them is counted.");ok=false;}
    if(Blank(z.drawingId)){iss.Add("DRAWING_MISSING: "+st.label+" has no drawing identity; not counted.");ok=false;}
    else if(!OI.Equals(z.drawingId,p.drawingId)){iss.Add("FOREIGN_DRAWING: "+st.label+" belongs to drawing "+Show(z.drawingId)+", not "+Show(p.drawingId)+"; not counted (multi-drawing aggregation is not supported).");ok=false;}
    string rf;
    if(Blank(z.floorId)||!floors.ContainsKey(z.floorId)){iss.Add("FLOOR_UNKNOWN: "+st.label+" floor "+Show(z.floorId)+" is not in the project; not counted.");ok=false;}
    else if(Blank(z.roomId)||!roomFloor.TryGetValue(z.roomId,out rf)){st.floorId=z.floorId;iss.Add("ROOM_UNKNOWN: "+st.label+" room "+Show(z.roomId)+" is not in the project; not counted.");ok=false;}
    else if(rf!=z.floorId){iss.Add("ROOM_NOT_ON_FLOOR: "+st.label+" room "+z.roomId+" belongs to floor "+rf+", not "+z.floorId+"; not counted.");ok=false;}
    else{st.floorId=z.floorId;st.roomId=z.roomId;}
    bool lists=z.acceptedSources!=null&&z.usedDevices!=null&&z.unownedCrossingSources!=null&&z.quantities!=null;
    if(!lists){iss.Add("SNAPSHOT_LIST_NULL: "+st.label+" has a null list; not counted.");ok=false;}
    else if(z.acceptedSources.Concat(z.usedDevices).Concat(z.unownedCrossingSources).Any(Blank)){iss.Add("IDENTITY_BLANK: "+st.label+" lists a blank handle; not counted.");ok=false;}
    st.valid=ok;st.clean=ok;
    if(!z.fresh){iss.Add("STALE: "+st.label+" is not a fresh scan of the current drawing.");st.clean=false;}
    if(!z.complete){iss.Add("INCOMPLETE: "+st.label+" is not complete (missing data, CHECK or blockers).");st.clean=false;}
    if(lists&&z.unownedCrossingSources.Count>0){iss.Add("UNOWNED_CROSSING: "+st.label+" has "+z.unownedCrossingSources.Count+" crossing source(s) owned by no snapshot: "+
     string.Join(",",z.unownedCrossingSources.Where(x=>!Blank(x)).OrderBy(x=>x,O))+".");st.clean=false;}
    if(!ok)continue;
    for(int j=0;j<z.quantities.Count;j++){var q=z.quantities[j];var qp=QuantityProblem(q);
     if(qp!=null){iss.Add("QUANTITY_INVALID: "+st.label+" row #"+j+": "+qp+"; row not counted.");st.clean=false;}else st.good.Add(q);}}
   // Identity ownership across snapshots (drawing + handle, case-insensitive). Never subtracted or allocated: a shared identity only blocks.
   var uses=new SortedDictionary<string,List<Use>>(O);
   foreach(var st in states){if(!st.valid)continue;
    foreach(var pair in st.s.acceptedSources.Select(h=>new{k="SOURCE",h}).Concat(st.s.usedDevices.Select(h=>new{k="DEVICE",h}))){
     var key=st.s.drawingId.ToUpperInvariant()+US+pair.h.ToUpperInvariant();List<Use> l;
     if(!uses.TryGetValue(key,out l))uses[key]=l=new List<Use>();
     if(!l.Any(u=>u.z==st&&u.kind==pair.k))l.Add(new Use{z=st,kind=pair.k,handle=pair.h});}}
   foreach(var kv in uses){var l=kv.Value;var zs=l.Select(u=>u.z).Distinct().ToList();var kinds=l.Select(u=>u.kind).Distinct().OrderBy(x=>x,O).ToList();
    var who=string.Join(", ",zs.Select(x=>x.label));
    if(kinds.Count>1){iss.Add("IDENTITY_KIND_CONFLICT: handle "+l[0].handle+" is listed as both source and device ("+who+").");foreach(var zk in zs){zk.clean=false;}}
    if(zs.Count>1){iss.Add("SHARED_"+(kinds.Count>1?"SOURCE_OR_DEVICE":kinds[0])+": handle "+l[0].handle+" is claimed by "+who+
      "; ownership is not proven, provisional totals may double count.");foreach(var zk in zs){zk.clean=false;}}}
   // Provisional totals per level and exact (kind, code, unit).
   var map=new Dictionary<string,HierTotal037>(O);
   Action<string,string,HierQuantity037> add=(lv,sc,qq)=>{var mk=lv+US+sc+US+qq.kind+US+qq.code+US+qq.unit;HierTotal037 tt;
    if(!map.TryGetValue(mk,out tt))map[mk]=tt=new HierTotal037{level=lv,scopeId=sc,kind=qq.kind,code=qq.code,unit=qq.unit};tt.value+=qq.value;};
   foreach(var st in states){if(!st.valid)continue;foreach(var q in st.good){add(LevelRoom,st.roomId,q);add(LevelFloor,st.floorId,q);add(LevelProject,p.id,q);}}
   foreach(var tv in map.Values){if(double.IsInfinity(tv.value)||double.IsNaN(tv.value))iss.Add("TOTAL_OVERFLOW: "+tv.level+" "+tv.scopeId+" "+tv.kind+" "+tv.code+" "+tv.unit+" is not finite.");}
   a.official=iss.Count==0&&states.Count>0;
   var floorRank=new Dictionary<string,int>(O);var roomRank=new Dictionary<string,int>(O);int fi=0,ri=0;
   foreach(var f in p.floors.OrderBy(x=>x.order).ThenBy(x=>x.id,O)){floorRank[f.id]=fi++;foreach(var r in f.rooms)roomRank[r.id]=ri++;}
   foreach(var tp in map.Values){tp.official=a.official;var sid=tp.scopeId;
    tp.pending=tp.level==LevelProject?states.Count(s=>!s.clean)+nulls:tp.level==LevelFloor?states.Count(s=>!s.clean&&s.floorId==sid):states.Count(s=>!s.clean&&s.roomId==sid);}
   Func<HierTotal037,int> lr=x=>x.level==LevelProject?0:x.level==LevelFloor?1:2;
   Func<HierTotal037,int> sr=x=>x.level==LevelFloor?floorRank[x.scopeId]:x.level==LevelRoom?roomRank[x.scopeId]:0;
   a.totals=map.Values.OrderBy(lr).ThenBy(sr).ThenBy(x=>x.kind,O).ThenBy(x=>x.code,O).ThenBy(x=>x.unit,O).ToList();
   return a;}
 }
}
