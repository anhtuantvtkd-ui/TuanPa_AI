using System;using System.Collections;using System.Collections.Generic;using System.Globalization;using System.Linq;using System.Security.Cryptography;using System.Text;using System.Web.Script.Serialization;
namespace Tvxd.Meps {
 // TVXD-MEPS-RAW-DRAFT-044: pure raw-draft payload, raw cell overlay, signatures and the keeper state machine over RoomRecovery043 (store unchanged).
 // No AutoCAD API, no Forms, no NOD access, no QSAVE, no network. ASCII-only source; reason codes are English, RoomRecoveryUi044 maps them to UI text.
 // Contract: TESTS/RAW_DRAFT044/EXECUTOR/README.md.
 //  - The payload (opaque to RoomRecovery043) is a DIFFERENT schema from envelope043: TVXD_MEPS_RAWDRAFT044 v1, canonical JavaScriptSerializer JSON of
 //    fixed fields. Accepted state (projectJson = RoomProjectStore037 document incl. room drafts, or null = identical to the DWG base) and the raw overlay
 //    (cells keyed roomId + kind ROW/SUPPLEMENT + rowKey/supplementId + field; never a visual index) are stored separately. No computed total or status.
 //  - Decode is strict: unknown/missing field, wrong type, schema/version, bounds, unpaired surrogate or any non-canonical text (re-encode must give the
 //    exact same string) = RawDraftFormat044; nothing is repaired. Numbers are round-trip "R" strings (exact doubles).
 //  - Keeper: Load with a pending geometry, inspect the candidate payload WITHOUT applying it, let the caller rescan the recorded room bases, Load again
 //    with the computed geometry. Only an exact match (identity, base revision, geometry, same generation and payload) is restored. Base/geometry
 //    conflict, corrupt primary (also backup-only), invalid payload or stale generation = HELD: the candidate is kept and never overwritten by an
 //    automatic checkpoint. A candidate without user data (IsClean) may be superseded.
 public sealed class RawCell044 {public string roomId,kind,key,field,text;}
 // Scan basis of one recovered room (stable room ID, declared units, connection tolerance, WCS rectangle) + the RoomCad037 signature it was captured on
 // (null = incomplete scan, never verifiable).
 public sealed class RawScan044 {public string roomId,units,signature;public double tolerance,minX,minY,maxX,maxY;}
 public sealed class RawOpenRoom044 {public string roomId,units;public bool unitConfirmed;public double tolerance,minX,minY,maxX,maxY;}
 public sealed class RawDraftPayload044 {
  public string projectJson,selectedRoomId,selectedFloorId;public bool projectDirty;public RawOpenRoom044 openRoom;
  public List<RawScan044> scans=new List<RawScan044>();public List<RawCell044> cells=new List<RawCell044>();
  // Nothing the user could lose: no accepted change against the base and no raw cell (selection / open room alone are never restored).
  public bool IsClean{get{return projectJson==null&&!projectDirty&&(cells==null||cells.Count==0);}}
 }
 public sealed class RawDraftFormat044:Exception {public readonly string Reason;public RawDraftFormat044(string reason,string message):base(message){Reason=reason;}}

 public static class RawDraftCodec044 {
  public const string Schema="TVXD_MEPS_RAWDRAFT044",Row="ROW",Supplement="SUPPLEMENT",NoUnits="NONE";
  public const int Version=1,MaxCells=20000,MaxCellText=32767,MaxId=400,MaxUnits=16,MaxScans=2000,MaxSignature=4096,MaxProjectChars=2000000;
  // Must equal RoomAdjustmentsUi037.CellFields / SupplementFields (RoomRecoveryUi044 checks before use; a mismatch disables recovery).
  public static readonly string[] RowFields={"circuitId","routingMode","tailCount","tailEachLength","factor","assemblyCode","conduitCode"};
  public static readonly string[] SupplementFields={"kind","materialCode","metres","circuitId","description","reason"};
  static readonly string[] RootKeys={"schema","version","projectJson","projectDirty","selectedRoomId","selectedFloorId","openRoom","scans","cells"};
  static readonly string[] OpenKeys={"roomId","units","unitConfirmed","tolerance","minX","minY","maxX","maxY"};
  static readonly string[] ScanKeys={"roomId","units","tolerance","minX","minY","maxX","maxY","signature"};
  static readonly string[] CellKeys={"roomId","kind","key","field","text"};
  static readonly CultureInfo Inv=CultureInfo.InvariantCulture;
  const StringComparison ORD=StringComparison.Ordinal;
  static JavaScriptSerializer Json(){return new JavaScriptSerializer{MaxJsonLength=int.MaxValue,RecursionLimit=8};}
  static RawDraftFormat044 F(string reason,string message){return new RawDraftFormat044(reason,message);}
  public static string R(double v){return v.ToString("R",Inv);}

  public static string Encode(RawDraftPayload044 p){Validate(p);return Json().Serialize(ToDict(p));}
  public static RawDraftPayload044 TryDecode(string text,out string reason,out string message){reason=null;message=null;
   try{return Decode(text);}catch(RawDraftFormat044 ex){reason=ex.Reason;message=ex.Message;return null;}}
  public static RawDraftPayload044 Decode(string text){
   if(text==null)throw F("PAYLOAD_NULL","raw draft payload is null.");
   Dictionary<string,object> d;
   try{d=Json().DeserializeObject(text) as Dictionary<string,object>;}catch(Exception ex){throw F("JSON","raw draft JSON parse failed: "+ex.Message);}
   if(d==null)throw F("SCHEMA","raw draft root is not a JSON object.");
   Keys(d,RootKeys,"payload");
   if(!Schema.Equals(d["schema"] as string,ORD))throw F("SCHEMA","schema is not "+Schema+".");
   if(!(d["version"] is int))throw F("SCHEMA","version is not an integer.");
   if((int)d["version"]!=Version)throw F("UNSUPPORTED_VERSION","raw draft version "+((int)d["version"]).ToString(Inv)+" is not supported.");
   var p=new RawDraftPayload044{projectJson=Str(d,"projectJson",true),projectDirty=Bool(d,"projectDirty"),selectedRoomId=Str(d,"selectedRoomId",true),selectedFloorId=Str(d,"selectedFloorId",true)};
   object o=d["openRoom"];
   if(o!=null){var od=o as Dictionary<string,object>;if(od==null)throw F("SCHEMA","openRoom is not an object.");Keys(od,OpenKeys,"openRoom");
    p.openRoom=new RawOpenRoom044{roomId=Str(od,"roomId",false),units=Str(od,"units",false),unitConfirmed=Bool(od,"unitConfirmed"),tolerance=Num(od,"tolerance"),
     minX=Num(od,"minX"),minY=Num(od,"minY"),maxX=Num(od,"maxX"),maxY=Num(od,"maxY")};}
   foreach(var x in Items(d,"scans",MaxScans)){var sd=x as Dictionary<string,object>;if(sd==null)throw F("SCHEMA","scan entry is not an object.");Keys(sd,ScanKeys,"scan");
    p.scans.Add(new RawScan044{roomId=Str(sd,"roomId",false),units=Str(sd,"units",false),tolerance=Num(sd,"tolerance"),minX=Num(sd,"minX"),minY=Num(sd,"minY"),
     maxX=Num(sd,"maxX"),maxY=Num(sd,"maxY"),signature=Str(sd,"signature",true)});}
   foreach(var x in Items(d,"cells",MaxCells)){var cd=x as Dictionary<string,object>;if(cd==null)throw F("SCHEMA","cell entry is not an object.");Keys(cd,CellKeys,"cell");
    p.cells.Add(new RawCell044{roomId=Str(cd,"roomId",false),kind=Str(cd,"kind",false),key=Str(cd,"key",false),field=Str(cd,"field",false),text=Str(cd,"text",false)});}
   Validate(p);
   if(!string.Equals(Json().Serialize(ToDict(p)),text,ORD))throw F("NON_CANONICAL","raw draft is not in canonical form (duplicate/reordered fields, whitespace or a foreign writer).");
   return p;}

  // Structural rules shared by Encode and Decode (engineering values are never interpreted here; cell text is kept ordinal-exactly).
  public static void Validate(RawDraftPayload044 p){
   if(p==null)throw F("PAYLOAD_NULL","raw draft payload is null.");
   if(p.projectJson!=null&&(p.projectJson.Length==0||p.projectJson.Length>MaxProjectChars))throw F("BOUNDS","projectJson is empty or longer than "+MaxProjectChars.ToString(Inv)+" chars.");
   if(p.projectJson!=null&&!WellFormed(p.projectJson))throw F("SCHEMA","projectJson contains unpaired surrogates.");
   if((p.projectJson!=null)!=p.projectDirty)throw F("INCONSISTENT","projectDirty must be true exactly when projectJson is present.");
   Id(p.selectedRoomId,"selectedRoomId",true);Id(p.selectedFloorId,"selectedFloorId",true);
   if(p.openRoom!=null){var o=p.openRoom;Id(o.roomId,"openRoom.roomId",false);Units(o.units,"openRoom.units");
    if(o.units==NoUnits)throw F("BOUNDS","openRoom.units must be a declared unit.");
    Fin(o.tolerance,"openRoom.tolerance",true);Fin(o.minX,"openRoom.minX",false);Fin(o.minY,"openRoom.minY",false);Fin(o.maxX,"openRoom.maxX",false);Fin(o.maxY,"openRoom.maxY",false);
    if(!(o.maxX>o.minX)||!(o.maxY>o.minY))throw F("BOUNDS","openRoom rectangle has no area.");
    if(!string.Equals(o.roomId,p.selectedRoomId,ORD))throw F("INCONSISTENT","openRoom.roomId must equal selectedRoomId.");}
   if(p.scans==null||p.cells==null)throw F("SCHEMA","scans and cells are required lists.");
   if(p.scans.Count>MaxScans)throw F("BOUNDS","more than "+MaxScans.ToString(Inv)+" scans.");
   if(p.cells.Count>MaxCells)throw F("BOUNDS","more than "+MaxCells.ToString(Inv)+" raw cells.");
   var rooms=new HashSet<string>(StringComparer.Ordinal);
   foreach(var x in p.scans){if(x==null)throw F("SCHEMA","null scan entry.");Id(x.roomId,"scan.roomId",false);Units(x.units,"scan.units");
    Fin(x.tolerance,"scan.tolerance",true);Fin(x.minX,"scan.minX",false);Fin(x.minY,"scan.minY",false);Fin(x.maxX,"scan.maxX",false);Fin(x.maxY,"scan.maxY",false);
    if(x.signature!=null&&(x.signature.Length==0||x.signature.Length>MaxSignature||!WellFormed(x.signature)||x.signature.Any(ch=>char.IsControl(ch))))throw F("BOUNDS","scan.signature is invalid.");
    if(x.units==NoUnits&&x.signature!=null)throw F("INCONSISTENT","a scan without a basis cannot carry a signature.");
    if(!rooms.Add(x.roomId))throw F("DUPLICATE","two scans for room "+x.roomId+".");}
   var cells=new HashSet<string>(StringComparer.Ordinal);
   foreach(var c in p.cells){if(c==null)throw F("SCHEMA","null cell entry.");Id(c.roomId,"cell.roomId",false);Id(c.key,"cell.key",false);
    string[] allowed=c.kind==Row?RowFields:c.kind==Supplement?SupplementFields:null;
    if(allowed==null)throw F("UNKNOWN_KIND","cell kind '"+c.kind+"' is not "+Row+" or "+Supplement+".");
    if(c.field==null||Array.IndexOf(allowed,c.field)<0)throw F("UNKNOWN_FIELD","cell field '"+c.field+"' is not editable for "+c.kind+".");
    if(c.text==null)throw F("SCHEMA","cell text is null.");
    if(c.text.Length>MaxCellText)throw F("BOUNDS","cell text longer than "+MaxCellText.ToString(Inv)+" chars.");
    if(!WellFormed(c.text))throw F("SCHEMA","cell text contains unpaired surrogates.");
    if(!cells.Add(RawDraftSign044.Lp(c.roomId)+RawDraftSign044.Lp(c.kind)+RawDraftSign044.Lp(c.key)+RawDraftSign044.Lp(c.field)))throw F("DUPLICATE","two raw cells for the same room/row/field.");}}

  static Dictionary<string,object> ToDict(RawDraftPayload044 p){var d=new Dictionary<string,object>();
   d["schema"]=Schema;d["version"]=Version;d["projectJson"]=p.projectJson;d["projectDirty"]=p.projectDirty;d["selectedRoomId"]=p.selectedRoomId;d["selectedFloorId"]=p.selectedFloorId;
   if(p.openRoom==null)d["openRoom"]=null;
   else{var o=p.openRoom;var od=new Dictionary<string,object>();od["roomId"]=o.roomId;od["units"]=o.units;od["unitConfirmed"]=o.unitConfirmed;od["tolerance"]=R(o.tolerance);
    od["minX"]=R(o.minX);od["minY"]=R(o.minY);od["maxX"]=R(o.maxX);od["maxY"]=R(o.maxY);d["openRoom"]=od;}
   var scans=new List<object>();
   foreach(var x in p.scans){var sd=new Dictionary<string,object>();sd["roomId"]=x.roomId;sd["units"]=x.units;sd["tolerance"]=R(x.tolerance);sd["minX"]=R(x.minX);sd["minY"]=R(x.minY);
    sd["maxX"]=R(x.maxX);sd["maxY"]=R(x.maxY);sd["signature"]=x.signature;scans.Add(sd);}
   d["scans"]=scans;
   var cells=new List<object>();
   foreach(var c in p.cells){var cd=new Dictionary<string,object>();cd["roomId"]=c.roomId;cd["kind"]=c.kind;cd["key"]=c.key;cd["field"]=c.field;cd["text"]=c.text;cells.Add(cd);}
   d["cells"]=cells;return d;}

  static void Keys(Dictionary<string,object> d,string[] keys,string what){
   foreach(var k in d.Keys)if(Array.IndexOf(keys,k)<0)throw F("UNKNOWN_FIELD","unknown field '"+k+"' in "+what+".");
   foreach(var k in keys)if(!d.ContainsKey(k))throw F("SCHEMA","missing field '"+k+"' in "+what+".");}
  static string Str(Dictionary<string,object> d,string k,bool nullable){object v=d[k];if(v==null){if(nullable)return null;throw F("SCHEMA","field '"+k+"' must be a string.");}
   var s=v as string;if(s==null)throw F("SCHEMA","field '"+k+"' must be a string"+(nullable?" or null.":"."));return s;}
  static bool Bool(Dictionary<string,object> d,string k){object v=d[k];if(!(v is bool))throw F("SCHEMA","field '"+k+"' must be true or false.");return (bool)v;}
  static double Num(Dictionary<string,object> d,string k){var s=d[k] as string;if(s==null)throw F("SCHEMA","field '"+k+"' must be a round-trip number string.");double v;
   if(!double.TryParse(s,NumberStyles.Float,Inv,out v)||double.IsNaN(v)||double.IsInfinity(v)||!string.Equals(R(v),s,ORD))throw F("SCHEMA","field '"+k+"' is not a canonical finite number.");
   return v;}
  static IEnumerable<object> Items(Dictionary<string,object> d,string k,int max){var l=d[k] as IList;if(l==null||d[k] is string)throw F("SCHEMA","field '"+k+"' must be a list.");
   if(l.Count>max)throw F("BOUNDS","field '"+k+"' has more than "+max.ToString(Inv)+" entries.");return l.Cast<object>();}
  static void Id(string v,string what,bool nullable){if(v==null){if(nullable)return;throw F("SCHEMA",what+" is required.");}
   if(v.Length==0||v.Length>MaxId||!WellFormed(v)||v.Any(ch=>char.IsControl(ch)))throw F("BOUNDS",what+" is empty, longer than "+MaxId.ToString(Inv)+" chars or has control characters.");}
  static void Units(string v,string what){if(v==null||v.Length==0||v.Length>MaxUnits||v.Any(ch=>char.IsControl(ch)))throw F("BOUNDS",what+" is invalid.");}
  static void Fin(double v,string what,bool nonNegative){if(double.IsNaN(v)||double.IsInfinity(v)||(nonNegative&&v<0))throw F("BOUNDS",what+" must be finite"+(nonNegative?" and >= 0.":"."));}
  public static bool WellFormed(string s){
   for(int i=0;i<s.Length;i++){char c=s[i];
    if(char.IsHighSurrogate(c)){if(i+1>=s.Length||!char.IsLowSurrogate(s[i+1]))return false;i++;}
    else if(char.IsLowSurrogate(c))return false;}
   return true;}
 }

 // Base revision of the DWG-stored project data and geometry signature of the recovered rooms (both opaque identity tokens of RoomRecovery043).
 public static class RawDraftSign044 {
  public const string Missing="NOD037:MISSING",NoScans="G044:NONE",Pending="G044:PENDING";
  // Never equal to each other: an incomplete capture can never be proven current, a failed rescan never matches anything.
  public const string Incomplete="\u0001INCOMPLETE",Unverifiable="\u0001UNVERIFIABLE";
  static readonly CultureInfo Inv=CultureInfo.InvariantCulture;
  internal static string Lp(string s){return s==null?"-1:\n":s.Length.ToString(Inv)+":"+s+"\n";}
  // Exact UTF-16 code units (no encoder replacement), so every distinct raw text gives a distinct hash input.
  static string Hash(string s){var b=new byte[s.Length*2];for(int i=0;i<s.Length;i++){b[2*i]=(byte)s[i];b[2*i+1]=(byte)(s[i]>>8);}
   using(var h=SHA256.Create())return string.Concat(h.ComputeHash(b).Select(x=>x.ToString("x2",Inv)));}
  // nodRaw = RoomProjectStore037 raw of the freshly read NOD record; null = no project record (explicit sentinel).
  public static string BaseRevision(string nodRaw){return nodRaw==null?Missing:"NOD037:SHA256:"+Hash(nodRaw);}
  // Order-independent (sorted by room ID). signatureOf returns the signature used for this side of the comparison.
  public static string Geometry(IEnumerable<RawScan044> scans,Func<RawScan044,string> signatureOf){
   var l=(scans??Enumerable.Empty<RawScan044>()).Where(x=>x!=null).OrderBy(x=>x.roomId,StringComparer.Ordinal).ToList();
   if(l.Count==0)return NoScans;
   var sb=new StringBuilder(RawDraftCodec044.Schema).Append("|GEO|1\n");
   foreach(var x in l)sb.Append(Lp(x.roomId)).Append(Lp(x.units)).Append(Lp(RawDraftCodec044.R(x.tolerance))).Append(Lp(RawDraftCodec044.R(x.minX))).Append(Lp(RawDraftCodec044.R(x.minY)))
    .Append(Lp(RawDraftCodec044.R(x.maxX))).Append(Lp(RawDraftCodec044.R(x.maxY))).Append(Lp(signatureOf(x)));
   return "G044:"+Hash(sb.ToString());}
  // Capture side: the signatures recorded in the payload.
  public static string Captured(IEnumerable<RawScan044> scans){return Geometry(scans,x=>x.signature??Incomplete);}
  // Load side: a fresh read-only rescan of each recorded basis (rescan returns null for an incomplete scan or an unusable basis).
  public static string Current(IEnumerable<RawScan044> scans,Func<RawScan044,string> rescan){return Geometry(scans,x=>(x.units==RawDraftCodec044.NoUnits?null:rescan(x))??Unverifiable);}
 }

 // Raw (not accepted) cell texts of the session, keyed by stable room ID + kind + row key / supplement ID + exact field name. Never by grid position.
 public sealed class RawOverlay044 {
  readonly Dictionary<string,RawCell044> map=new Dictionary<string,RawCell044>(StringComparer.Ordinal);
  static string K(string roomId,string kind,string key,string field){return RawDraftSign044.Lp(roomId)+RawDraftSign044.Lp(kind)+RawDraftSign044.Lp(key)+RawDraftSign044.Lp(field);}
  static RawCell044 Copy(RawCell044 c){return new RawCell044{roomId=c.roomId,kind=c.kind,key=c.key,field=c.field,text=c.text};}
  public int Count{get{return map.Count;}}
  public string Get(string roomId,string kind,string key,string field){if(roomId==null||kind==null||key==null||field==null)return null;RawCell044 c;return map.TryGetValue(K(roomId,kind,key,field),out c)?c.text:null;}
  // true = changed. Text is stored exactly (also empty); null text is refused.
  public bool Set(string roomId,string kind,string key,string field,string text){
   if(roomId==null||kind==null||key==null||field==null||text==null)throw new ArgumentNullException("raw cell");
   var k=K(roomId,kind,key,field);RawCell044 c;if(map.TryGetValue(k,out c)&&string.Equals(c.text,text,StringComparison.Ordinal))return false;
   map[k]=new RawCell044{roomId=roomId,kind=kind,key=key,field=field,text=text};return true;}
  public bool Remove(string roomId,string kind,string key,string field){if(roomId==null||kind==null||key==null||field==null)return false;return map.Remove(K(roomId,kind,key,field));}
  public int Clear(){int n=map.Count;map.Clear();return n;}
  public List<RawCell044> All(){return map.Values.OrderBy(c=>c.roomId,StringComparer.Ordinal).ThenBy(c=>c.kind,StringComparer.Ordinal).ThenBy(c=>c.key,StringComparer.Ordinal)
   .ThenBy(c=>c.field,StringComparer.Ordinal).Select(Copy).ToList();}
  public List<RawCell044> Room(string roomId){return All().Where(c=>string.Equals(c.roomId,roomId,StringComparison.Ordinal)).ToList();}
  public List<string> Rooms(){return map.Values.Select(c=>c.roomId).Distinct(StringComparer.Ordinal).OrderBy(x=>x,StringComparer.Ordinal).ToList();}
  public void Replace(IEnumerable<RawCell044> cells){map.Clear();if(cells!=null)foreach(var c in cells)if(c!=null)Set(c.roomId,c.kind,c.key,c.field,c.text);}
 }

 // State machine of one drawing identity over RoomRecovery043. Never throws for store/payload problems; State/Reason/Message say what happened.
 public sealed class RawDraftKeeper044 {
  public const string Disabled="DISABLED",Ready="READY",HeldConflict="HELD_CONFLICT",HeldCorrupt="HELD_CORRUPT",HeldStale="HELD_STALE";
  public const string Off="OFF",Fresh="FRESH",Restored="RESTORED",Clean="CLEAN",Superseded="CLEAN_SUPERSEDED",Held="HELD";
  readonly RoomRecovery043 store;readonly string fingerprint,canonicalPath,sessionToken;
  public string BaseRevision;
  public string State=Disabled,Outcome=Off,Reason,Message,LastSavedUtc,LastWritten,PrimaryPath;
  public long Generation=-1;public bool LastFailed;public int Writes;
  public RecoveryEnvelope043 HeldCandidate;public RawDraftPayload044 RestoredPayload;public RecoveryResult043 LastResult;
  public RawDraftKeeper044(RoomRecovery043 store,string fingerprint,string canonicalPath,string sessionToken,string baseRevision){
   this.store=store;this.fingerprint=fingerprint;this.canonicalPath=canonicalPath;this.sessionToken=sessionToken;BaseRevision=baseRevision;}
  public RecoveryIdentity043 Identity(string geometry){
   return new RecoveryIdentity043{fingerprint=fingerprint,canonicalPath=canonicalPath,sessionToken=sessionToken,baseRevision=BaseRevision,geometrySignature=geometry};}
  public void Disable(string reason,string message){State=Disabled;Outcome=Off;Reason=reason;Message=message;}
  // PATCH1 R044-QA-F01: the caller proved (fresh read) that the live drawing no longer matches this keeper: project NOD base revision or bound drawing
  // identity changed. From now on no write, no Holds claim and no Rebase for the rest of the session; the disk (primary/backup/generation) is untouched.
  // Sticky: nothing in the keeper clears it (not Close, not an unchanged payload, not Rebase). Only a READY keeper is suspended (an earlier hold stays).
  public const string LiveBaseChanged="LIVE_BASE_CHANGED",LiveIdentityChanged="LIVE_IDENTITY_CHANGED",RebaseUnproven="REBASE_UNPROVEN";
  // PATCH2 R044-QA-F03: a recovered/open room's CAD source changed after it was proven (same Suspend semantics).
  public const string LiveSourceChanged="LIVE_SOURCE_CHANGED";
  public void Suspend(string reason,string message){if(State!=Ready)return;State=HeldConflict;Outcome=Held;Reason=reason;Message=message;LastFailed=true;}
  // Capture problem in the caller (payload could not be built): nothing written, previous file unchanged, next checkpoint retries.
  public void Failed(string reason,string message){LastFailed=true;Reason=reason;Message=message;}
  static string First(RecoveryResult043 r){return r.reasons.Count>0?r.reasons[0]:r.status;}
  void Hold(string state,RecoveryResult043 r,string reason){State=state;Outcome=Held;Reason=reason;Message=r.message;HeldCandidate=r.candidate;Generation=r.diskGeneration;}
  // Returns the payload to restore (exact match only) or null; State tells whether checkpoints may write. currentGeometry is called only for a
  // non-clean candidate whose identity and base revision match: it must rescan the recorded bases read-only (RawDraftSign044.Current). It may throw
  // RawDraftFormat044 for a payload the caller cannot accept (HELD_CORRUPT); any other exception = HELD_CONFLICT.
  public RawDraftPayload044 Open(Func<RawDraftPayload044,string> currentGeometry){
   RestoredPayload=null;HeldCandidate=null;LastFailed=false;
   if(store==null){Disable("STORE_NULL","no recovery store.");return null;}
   if(store.RootError!=null){Disable("ROOT_INVALID",store.RootError);return null;}
   var r=store.Load(Identity(RawDraftSign044.Pending));LastResult=r;PrimaryPath=r.primaryPath;
   if(r.status==RecoveryStatus043.Missing){Generation=0;State=Ready;Outcome=Fresh;Reason=null;Message=r.message;return null;}
   if(r.status==RecoveryStatus043.Rejected||r.status==RecoveryStatus043.ReadFailed){Disable(First(r),r.message);return null;}
   if(r.status==RecoveryStatus043.Corrupt||r.candidate==null){Hold(HeldCorrupt,r,First(r));return null;}
   if(r.fromBackup||r.reasons.Contains("PRIMARY_CORRUPT")||r.reasons.Contains("PRIMARY_MISSING")){Hold(HeldCorrupt,r,"PRIMARY_CORRUPT_BACKUP_ONLY");return null;}
   string why,msg;var p=RawDraftCodec044.TryDecode(r.candidate.payload,out why,out msg);
   if(p==null){Hold(HeldCorrupt,r,"PAYLOAD_"+why);Message=msg;return null;}
   if(r.reasons.Contains("IDENTITY_MISMATCH")){Hold(HeldConflict,r,"IDENTITY_MISMATCH");return null;}
   if(p.IsClean){   // nothing to lose: no restore, the next checkpoint may replace it (also with a new base revision)
    State=Ready;Generation=r.diskGeneration;HeldCandidate=r.candidate;Reason=null;Message=r.message;
    if(r.reasons.Contains("BASE_REVISION_MISMATCH")){Outcome=Superseded;LastWritten=null;}else{Outcome=Clean;LastWritten=r.candidate.payload;LastSavedUtc=r.candidate.savedUtc;}
    return null;}
   if(r.reasons.Contains("BASE_REVISION_MISMATCH")){Hold(HeldConflict,r,"BASE_REVISION_MISMATCH");return null;}
   string g;
   try{g=currentGeometry(p);}
   catch(RawDraftFormat044 ex){Hold(HeldCorrupt,r,"PAYLOAD_"+ex.Reason);Message=ex.Message;return null;}
   catch(Exception ex){Hold(HeldConflict,r,"GEOMETRY_CHECK_FAILED");Message=ex.GetType().Name+": "+ex.Message;return null;}
   var r2=store.Load(Identity(g));LastResult=r2;
   if(r2.status==RecoveryStatus043.Ok&&r2.candidate!=null&&string.Equals(r2.payload,r.candidate.payload,StringComparison.Ordinal)&&r2.diskGeneration==r.diskGeneration){
    State=Ready;Outcome=Restored;Generation=r2.diskGeneration;LastWritten=r2.payload;LastSavedUtc=r2.candidate.savedUtc;Reason=null;Message=r2.message;RestoredPayload=p;return p;}
   if(r2.status==RecoveryStatus043.Conflict&&r2.reasons.Contains("GEOMETRY_MISMATCH")&&r2.reasons.Count==1){Hold(HeldConflict,r2,"GEOMETRY_MISMATCH");return null;}
   Hold(HeldConflict,r2.candidate!=null?r2:r,"RELOAD_CHANGED");return null;}
  // Writes p when READY and it differs from the last verified write (a clean payload is not written while nothing exists on disk).
  // null = nothing attempted (not READY / unchanged / nothing to keep / payload invalid -> LastFailed). Otherwise the store result.
  public RecoveryResult043 Checkpoint(RawDraftPayload044 p){
   if(State!=Ready)return null;
   string text;
   try{text=RawDraftCodec044.Encode(p);}catch(RawDraftFormat044 ex){Failed("PAYLOAD_"+ex.Reason,ex.Message);return null;}
   if(string.Equals(text,LastWritten,StringComparison.Ordinal)){LastFailed=false;return null;}
   if(Generation==0&&p.IsClean){LastFailed=false;return null;}
   var r=store.Save(Identity(RawDraftSign044.Captured(p.scans)),text,Generation);LastResult=r;Writes++;
   if(r.status==RecoveryStatus043.Ok){Generation=r.diskGeneration;LastWritten=text;LastSavedUtc=r.candidate.savedUtc;LastFailed=false;Reason=null;Message=r.message;PrimaryPath=r.primaryPath;return r;}
   if(r.status==RecoveryStatus043.Conflict){Hold(HeldStale,r,First(r));return r;}
   if(r.status==RecoveryStatus043.Corrupt){Hold(HeldCorrupt,r,First(r));return r;}
   Failed(First(r),r.message);return r;}
  // After a proven explicit DWG save: later checkpoints carry the new base (forced rewrite). Refused unless READY.
  public bool Rebase(string newBaseRevision){
   if(State!=Ready||string.IsNullOrWhiteSpace(newBaseRevision))return false;
   BaseRevision=newBaseRevision;LastWritten=null;return true;}
  // True when the last verified write equals p (what is on disk is exactly the current capture).
  public bool Holds(RawDraftPayload044 p){
   if(State!=Ready||LastFailed||LastWritten==null)return false;
   try{return string.Equals(RawDraftCodec044.Encode(p),LastWritten,StringComparison.Ordinal);}catch(RawDraftFormat044){return false;}}
 }
}
