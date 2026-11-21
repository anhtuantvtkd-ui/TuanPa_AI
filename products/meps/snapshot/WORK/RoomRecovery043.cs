using System;using System.Collections.Generic;using System.Globalization;using System.IO;using System.Linq;using System.Security.Cryptography;using System.Text;using System.Web.Script.Serialization;
namespace Tvxd.Meps {
 // TVXD-MEPS-RECOVERY-043 pilot: pure-file durable recovery store for raw multi-room drafts. No AutoCAD API, no QSAVE, no network.
 // ASCII-only source; messages are English diagnostics, the UI maps the stable reason codes. Contract: TESTS/RECOVERY043/EXECUTOR/README.md.
 //  - One envelope per drawing identity in a bound root: R043_<sha256(identity)>.rcv.json (+ .rcv.bak last-good, .rcv.tmp, .rcv.lock).
 //    File names never contain room/user text. Identity = fingerprint + canonical DWG path (saved) OR fingerprint + session token (unsaved).
 //  - Payload is an opaque caller string stored/returned ordinal-exactly (cell text is never parsed). Envelope fields are fixed: unknown
 //    field/version/schema, bad UTF-8/JSON/checksum or oversize = CORRUPT (fail closed); never repaired or overwritten automatically.
 //  - Save: per-key exclusive lock file, state re-read under the lock, expectedGeneration must equal the disk generation (lost-update guard),
 //    temp write + Flush(true) + readback, then File.Replace(temp,primary,backup) if the primary exists else File.Move. A corrupt primary blocks
 //    Save until the caller explicitly calls Quarantine (renames corrupt files and keeps them as evidence; a valid backup is never touched).
 //  - Load never restores a draft whose identity/base revision/geometry signature differs: CONFLICT, payload null, candidate returned apart.
 //    Save is a fresh caller capture, not a restore: it may carry a new baseRevision/geometry at the expected generation (caller's duty).
 //  - R043-F01: every public call re-proves root-inside-boundary (no reparse point root..boundary), and every slot path (primary, backup,
 //    temp, lock, scanned candidate, quarantine target) must be absent or a plain non-reparse file directly in root right before it is
 //    read/written/moved/deleted. Links are refused, never followed. Residual check-then-use window documented in the README.
 //  - Native facts (FingerprintGuid on SaveAs/copy, Undo revision token) belong to the caller and are NOT proven by this file.
 public static class RecoveryStatus043 {
  public const string Ok="OK",RecoveredFromBackup="RECOVERED_FROM_BACKUP",Missing="MISSING",Conflict="CONFLICT",Corrupt="CORRUPT",
   WriteFailed="WRITE_FAILED",ReadFailed="READ_FAILED",Rejected="REJECTED";}
 public sealed class RecoveryIdentity043 {
  public string fingerprint;       // required, opaque, compared ordinal (e.g. FingerprintGuid "N" form)
  public string canonicalPath;     // saved DWG: absolute path equal to Path.GetFullPath(path); null when unsaved
  public string sessionToken;      // unsaved DWG (Drawing1): caller-generated token; null when saved
  public string baseRevision;      // required: token of the DWG-stored data the draft is based on
  public string geometrySignature; // required: source geometry/topology signature
 }
 public sealed class RecoveryEnvelope043 {
  public int version;public long generation;
  public string savedUtc,fingerprint,canonicalPath,sessionToken,baseRevision,geometrySignature,payload;}
 public sealed class RecoveryResult043 {
  public string status,message;
  public List<string> reasons=new List<string>();
  public string payload;                // set only for OK / RECOVERED_FROM_BACKUP when identity, base revision and geometry all match
  public RecoveryEnvelope043 candidate; // envelope found on disk (also on CONFLICT): explicit recovery only
  public long diskGeneration=-1;        // pass as expectedGeneration to Save; 0 = nothing on disk; -1 = unknown
  public bool fromBackup;
  public string primaryPath,backupPath;
  public List<string> quarantined=new List<string>();}

 public sealed class RoomRecovery043 {
  public const string Schema="TVXD_MEPS_RECOVERY043",Prefix="R043_",PrimaryExt=".rcv.json",BackupExt=".rcv.bak",TempExt=".rcv.tmp",LockExt=".rcv.lock";
  public const int Version=1,MaxIdentityChars=4096,DefaultMaxPayloadChars=2000000,MaxPayloadLimit=50000000,MaxScanFiles=1000;
  public static readonly string[] Phases={"LOCK_ACQUIRED","TEMP_WRITTEN","TEMP_VERIFIED","BEFORE_COMMIT"};
  static readonly string[] Fields={"schema","version","generation","savedUtc","fingerprint","canonicalPath","sessionToken","baseRevision","geometrySignature","payload","sha256"};
  static readonly UTF8Encoding Utf8=new UTF8Encoding(false,true);
  static readonly CultureInfo Inv=CultureInfo.InvariantCulture;
  const StringComparison OIC=StringComparison.OrdinalIgnoreCase,ORD=StringComparison.Ordinal;
  const string Ok=RecoveryStatus043.Ok,Recovered=RecoveryStatus043.RecoveredFromBackup,Missing=RecoveryStatus043.Missing,Conflict=RecoveryStatus043.Conflict,
   Corrupt=RecoveryStatus043.Corrupt,WriteFailed=RecoveryStatus043.WriteFailed,ReadFailed=RecoveryStatus043.ReadFailed,Rejected=RecoveryStatus043.Rejected;
  readonly string root,bound,rootError;readonly int maxPayload;readonly long maxFileBytes;
  // Test-only failure injection, called with (phase, tempPath) for each of Phases. An exception aborts Save before commit (WRITE_FAILED,
  // reason INJECTED_FAULT_<phase>, last-good kept). A hook may also tamper with the temp file at TEMP_WRITTEN to exercise readback.
  public Action<string,string> FaultHook;
  public string Root{get{return root;}}
  public string RootError{get{return rootError;}}
  public int MaxPayloadChars{get{return maxPayload;}}
  public long MaxFileBytes{get{return maxFileBytes;}}

  public RoomRecovery043(string rootDir,string boundaryDir):this(rootDir,boundaryDir,DefaultMaxPayloadChars){}
  // rootDir must exist and lie inside boundaryDir (or equal it); no reparse point between them. Invalid root = every call returns REJECTED.
  public RoomRecovery043(string rootDir,string boundaryDir,int maxPayloadChars){
   maxPayload=maxPayloadChars;maxFileBytes=6L*maxPayloadChars+6L*6*MaxIdentityChars+65536;
   if(maxPayloadChars<1||maxPayloadChars>MaxPayloadLimit){rootError="CONFIG_INVALID: maxPayloadChars must be 1.."+MaxPayloadLimit.ToString(Inv)+".";return;}
   try{rootError=CheckRoot(rootDir,boundaryDir,out root);if(rootError==null)bound=Norm(boundaryDir);}catch(Exception ex){root=null;rootError="ROOT_INVALID: "+ex.Message;}}

  // ---------- public API ----------
  public RecoveryResult043 Load(RecoveryIdentity043 id){
   var pre=Pre(id);if(pre!=null)return pre;
   try{var r=Current(Key(id));Judge(r,id);return r;}
   catch(Exception ex){return Fail(new RecoveryResult043(),ReadFailed,"READ_IO",ex.GetType().Name+": "+ex.Message);}}

  public RecoveryResult043 Save(RecoveryIdentity043 id,string payload,long expectedGeneration){
   var pre=Pre(id);if(pre!=null)return pre;
   if(payload==null)return Fail(new RecoveryResult043(),Rejected,"PAYLOAD_INVALID","payload is null.");
   if(payload.Length>maxPayload)return Fail(new RecoveryResult043(),Rejected,"OVERSIZE","payload has "+payload.Length.ToString(Inv)+" chars; limit "+maxPayload.ToString(Inv)+".");
   if(!WellFormed(payload))return Fail(new RecoveryResult043(),Rejected,"PAYLOAD_INVALID","payload contains unpaired UTF-16 surrogates; UTF-8 storage would alter it.");
   if(expectedGeneration<0)return Fail(new RecoveryResult043(),Rejected,"GENERATION_INVALID","expectedGeneration must be >= 0 (use diskGeneration from Load).");
   string key=Key(id),tmp=FilePath(key,TempExt),err;var r=New(key);bool committed=false;
   try{err=CheckEntry(FilePath(key,LockExt));}catch(Exception ex){return Fail(r,WriteFailed,"WRITE_IO",ex.GetType().Name+": "+ex.Message);}
   if(err!=null)return Refuse(r,err);
   var lk=TryLock(key,out err);if(lk==null)return Fail(r,WriteFailed,"LOCK_BUSY","recovery file is locked by another session: "+err);
   try{
    Hook("LOCK_ACQUIRED",tmp);
    var cur=Current(key);
    if(cur.status==ReadFailed||cur.status==Rejected){cur.reasons.Add("SAVE_REFUSED");cur.message="Save refused: "+cur.message;return cur;}
    if(cur.status==Corrupt||cur.reasons.Contains("PRIMARY_CORRUPT")){
     cur.status=Corrupt;cur.payload=null;cur.reasons.Add("QUARANTINE_REQUIRED");
     cur.message="Save refused: corrupt recovery file kept as evidence; call Quarantine explicitly first. "+cur.message;return cur;}
    if(cur.candidate!=null&&!SameDrawing(cur.candidate,id)){
     cur.status=Conflict;cur.payload=null;cur.reasons.Add("IDENTITY_MISMATCH");cur.message="Save refused: the file at this key belongs to another identity.";return cur;}
    if(expectedGeneration!=cur.diskGeneration){
     cur.status=Conflict;cur.payload=null;cur.reasons.Add("STALE_GENERATION");
     cur.message="Save refused: expected generation "+expectedGeneration.ToString(Inv)+" but disk has "+cur.diskGeneration.ToString(Inv)+"; another session saved, reload first.";return cur;}
    var env=new RecoveryEnvelope043{version=Version,generation=cur.diskGeneration+1,savedUtc=DateTime.UtcNow.ToString("o",Inv),fingerprint=id.fingerprint,
     canonicalPath=id.canonicalPath,sessionToken=id.sessionToken,baseRevision=id.baseRevision,geometrySignature=id.geometrySignature,payload=payload};
    var bytes=Utf8.GetBytes(Serialize(env));
    if(bytes.Length>maxFileBytes)return Fail(r,Rejected,"OVERSIZE","encoded envelope exceeds "+maxFileBytes.ToString(Inv)+" bytes.");
    err=Guard()??CheckEntry(tmp);if(err!=null)return Unchanged(Refuse(r,err));
    // An orphan temp (plain file, checked above) is unlinked, then CreateNew: never truncate through an existing name (e.g. a hard link).
    if(File.Exists(tmp))File.Delete(tmp);
    using(var fs=new FileStream(tmp,FileMode.CreateNew,FileAccess.Write,FileShare.None,4096,FileOptions.WriteThrough)){fs.Write(bytes,0,bytes.Length);fs.Flush(true);}
    Hook("TEMP_WRITTEN",tmp);
    var t=ReadSlot(tmp);
    if(t.state!=Ok||!Same(t.env,env))return Fail(r,WriteFailed,"TEMP_READBACK_MISMATCH","temp readback failed ("+(t.reason??t.state)+"); previous recovery data unchanged.");
    Hook("TEMP_VERIFIED",tmp);Hook("BEFORE_COMMIT",tmp);
    err=Guard()??CheckEntry(tmp)??CheckEntry(r.primaryPath)??CheckEntry(r.backupPath);if(err!=null)return Unchanged(Refuse(r,err));
    if(cur.status==Ok)File.Replace(tmp,r.primaryPath,r.backupPath,true);else File.Move(tmp,r.primaryPath);
    committed=true;
    var v=ReadSlot(r.primaryPath);
    if(v.state!=Ok||!Same(v.env,env))return Fail(r,WriteFailed,"POST_COMMIT_READBACK","primary readback after commit failed ("+(v.reason??v.state)+"); backup holds the previous last-good.");
    r.status=Ok;r.candidate=env;r.payload=payload;r.diskGeneration=env.generation;
    r.message="Saved generation "+env.generation.ToString(Inv)+" at "+env.savedUtc+".";return r;}
   catch(InjectedFault ex){return Fail(r,WriteFailed,"INJECTED_FAULT_"+ex.Phase,ex.Message+" Previous recovery data unchanged.");}
   catch(Exception ex){return Fail(r,WriteFailed,committed?"POST_COMMIT_ERROR":"WRITE_IO",ex.GetType().Name+": "+ex.Message);}
   finally{if(!committed)TryDelete(tmp);lk.Dispose();}}

  // Explicit user/caller action only: renames CORRUPT primary/backup of this identity to R043_<key>.<primary|backup>.corrupt-<utc>.rcv.
  // Valid, missing or unreadable (READ_FAILED) files are left untouched. Returns the new Load state plus the quarantined paths.
  public RecoveryResult043 Quarantine(RecoveryIdentity043 id){
   var pre=Pre(id);if(pre!=null)return pre;
   string key=Key(id),err;
   try{err=CheckEntry(FilePath(key,LockExt));}catch(Exception ex){return Fail(New(key),WriteFailed,"QUARANTINE_IO",ex.GetType().Name+": "+ex.Message);}
   if(err!=null)return Refuse(New(key),err);
   var lk=TryLock(key,out err);
   if(lk==null)return Fail(New(key),WriteFailed,"LOCK_BUSY","recovery file is locked by another session: "+err);
   var moved=new List<string>();
   try{
    foreach(var kind in new[]{"primary","backup"}){
     var p=FilePath(key,kind=="primary"?PrimaryExt:BackupExt);var s=ReadSlot(p);if(s.state!=Corrupt)continue;
     var q=FilePath(key,"."+kind+".corrupt-"+DateTime.UtcNow.ToString("yyyyMMdd'T'HHmmssfffffff'Z'",Inv)+".rcv");
     err=Guard()??CheckEntry(p)??CheckEntry(q);
     if(err!=null){var f=Refuse(New(key),err);f.quarantined.AddRange(moved);f.message+=" Quarantine stopped; remaining files untouched.";return f;}
     File.Move(p,q);moved.Add(q);}
    var r=Current(key);r.quarantined.AddRange(moved);Judge(r,id);
    r.message=(moved.Count==0?"Nothing to quarantine. ":"Quarantined "+moved.Count.ToString(Inv)+" corrupt file(s) as evidence. ")+r.message;return r;}
   catch(Exception ex){var f=Fail(New(key),WriteFailed,"QUARANTINE_IO",ex.GetType().Name+": "+ex.Message);f.quarantined.AddRange(moved);return f;}
   finally{lk.Dispose();}}

  // SaveAs/copy helper: valid primaries in the root with this fingerprint (any path/session), newest first. Each item is CONFLICT with
  // reason CANDIDATE_ONLY: the caller must show it and let the user recover explicitly (then Save under the new identity).
  public List<RecoveryResult043> Candidates(string fingerprint){
   var list=new List<RecoveryResult043>();
   if(rootError!=null){list.Add(RootFail());return list;}
   var fe=Field("fingerprint",fingerprint);if(fe!=null){list.Add(Fail(new RecoveryResult043(),Rejected,"IDENTITY_INVALID",fe));return list;}
   try{int n=0;
    var g=Guard();if(g!=null){list.Add(Refuse(new RecoveryResult043(),g));return list;}
    foreach(var p in Directory.EnumerateFiles(root,Prefix+"*"+PrimaryExt,SearchOption.TopDirectoryOnly)){
     if(++n>MaxScanFiles){list.Add(Fail(new RecoveryResult043(),ReadFailed,"SCAN_LIMIT","more than "+MaxScanFiles.ToString(Inv)+" recovery files; scan truncated."));break;}
     var s=ReadSlot(p);
     if(s.state==Rejected){list.Add(Fail(new RecoveryResult043{primaryPath=p},Rejected,s.reason,s.message));continue;}
     if(s.state!=Ok||!string.Equals(s.env.fingerprint,fingerprint,ORD))continue;
     list.Add(new RecoveryResult043{status=Conflict,candidate=s.env,diskGeneration=s.env.generation,primaryPath=p,reasons={"CANDIDATE_ONLY"},
      message="Candidate for explicit recovery only ("+(s.env.canonicalPath??"unsaved session")+", generation "+s.env.generation.ToString(Inv)+", saved "+s.env.savedUtc+")."});}}
   catch(Exception ex){list.Add(Fail(new RecoveryResult043(),ReadFailed,"READ_IO",ex.GetType().Name+": "+ex.Message));}
   return list.OrderByDescending(x=>x.candidate==null?"":x.candidate.savedUtc,StringComparer.Ordinal).ToList();}

  // ---------- state ----------
  sealed class Slot {public string state,reason,message;public RecoveryEnvelope043 env;}
  sealed class BadEnvelope:Exception {public readonly string Reason;public BadEnvelope(string r,string m):base(m){Reason=r;}}
  sealed class InjectedFault:Exception {public readonly string Phase;public InjectedFault(string p,Exception inner):base("Injected failure at "+p+": "+inner.Message,inner){Phase=p;}}

  Slot ReadSlot(string p){var s=new Slot();
   try{
    var e=Guard()??CheckEntry(p);if(e!=null){s.state=Rejected;s.reason=Code(e);s.message=e;return s;}
    if(!File.Exists(p)){s.state=Missing;return s;}
    byte[] b;
    using(var fs=new FileStream(p,FileMode.Open,FileAccess.Read,FileShare.ReadWrite|FileShare.Delete)){
     if(fs.Length>maxFileBytes){s.state=Corrupt;s.reason="OVERSIZE";s.message="file has "+fs.Length.ToString(Inv)+" bytes; limit "+maxFileBytes.ToString(Inv)+".";return s;}
     b=new byte[fs.Length];int n=0;while(n<b.Length){int k=fs.Read(b,n,b.Length-n);if(k<=0)break;n+=k;}
     if(n!=b.Length)throw new IOException("short read.");}
    s.env=Parse(b);s.state=Ok;}
   catch(BadEnvelope ex){s.state=Corrupt;s.reason=ex.Reason;s.message=ex.Message;}
   catch(FileNotFoundException){s.state=Missing;}
   catch(Exception ex){s.state=ReadFailed;s.reason="READ_IO";s.message=ex.GetType().Name+": "+ex.Message;}
   return s;}

  // Disk state of one key: primary if valid, else the last-good backup (reported, corrupt primary untouched), else MISSING/CORRUPT.
  RecoveryResult043 Current(string key){
   var r=New(key);var p=ReadSlot(r.primaryPath);
   if(p.state==Rejected)return Fail(r,Rejected,p.reason,"primary refused, not read: "+p.message);
   if(p.state==ReadFailed)return Fail(r,ReadFailed,"PRIMARY_READ_IO","cannot read primary: "+p.message);
   if(p.state==Ok){r.status=Ok;r.candidate=p.env;r.diskGeneration=p.env.generation;return r;}
   var b=ReadSlot(r.backupPath);
   if(b.state==Rejected)return Fail(r,Rejected,b.reason,"backup refused, not read: "+b.message);
   if(b.state==ReadFailed)return Fail(r,ReadFailed,"BACKUP_READ_IO","cannot read backup: "+b.message);
   if(p.state==Missing&&b.state==Missing){r.status=Missing;r.diskGeneration=0;r.message="No recovery draft for this identity.";return r;}
   if(b.state==Ok){
    r.status=Recovered;r.fromBackup=true;r.candidate=b.env;r.diskGeneration=b.env.generation;
    if(p.state==Corrupt){r.reasons.Add("PRIMARY_CORRUPT");r.reasons.Add("PRIMARY_"+p.reason);
     r.message="Primary recovery file is corrupt ("+p.message+") and was left untouched as evidence; using last-good backup generation "+b.env.generation.ToString(Inv)+" saved "+b.env.savedUtc+".";}
    else{r.reasons.Add("PRIMARY_MISSING");r.message="Primary recovery file is missing; using last-good backup generation "+b.env.generation.ToString(Inv)+" saved "+b.env.savedUtc+".";}
    return r;}
   r.status=Corrupt;
   if(p.state==Corrupt)r.reasons.Add("PRIMARY_"+p.reason);
   if(b.state==Corrupt)r.reasons.Add("BACKUP_"+b.reason);
   r.message="No valid recovery file: primary "+(p.state==Corrupt?"corrupt ("+p.message+")":"missing")+", backup "+(b.state==Corrupt?"corrupt ("+b.message+")":"missing")+". Files left untouched.";
   return r;}

  static void Judge(RecoveryResult043 r,RecoveryIdentity043 id){
   if(r.candidate==null||(r.status!=Ok&&r.status!=Recovered))return;
   var e=r.candidate;int n=r.reasons.Count;
   if(!SameDrawing(e,id))r.reasons.Add("IDENTITY_MISMATCH");
   if(!string.Equals(e.baseRevision,id.baseRevision,ORD))r.reasons.Add("BASE_REVISION_MISMATCH");
   if(!string.Equals(e.geometrySignature,id.geometrySignature,ORD))r.reasons.Add("GEOMETRY_MISMATCH");
   if(r.reasons.Count>n){r.status=Conflict;r.payload=null;
    r.message=(r.message==null?"":r.message+" ")+"Draft NOT restored: "+string.Join(", ",r.reasons.Skip(n))+". Candidate returned separately for explicit recovery only.";}
   else{r.payload=e.payload;if(r.message==null)r.message="Draft generation "+e.generation.ToString(Inv)+" saved "+e.savedUtc+" restored.";}}

  // ---------- envelope ----------
  JavaScriptSerializer Json(){return new JavaScriptSerializer{MaxJsonLength=(int)maxFileBytes,RecursionLimit=4};}
  static string Lp(string s){return s==null?"-1:\n":s.Length.ToString(Inv)+":"+s+"\n";}
  static string Sha(string s){using(var h=SHA256.Create())return string.Concat(h.ComputeHash(Utf8.GetBytes(s)).Select(x=>x.ToString("x2",Inv)));}
  static string Checksum(RecoveryEnvelope043 e){
   var sb=new StringBuilder(Schema).Append("|SUM\n");
   foreach(var s in new[]{e.version.ToString(Inv),e.generation.ToString(Inv),e.savedUtc,e.fingerprint,e.canonicalPath,e.sessionToken,e.baseRevision,e.geometrySignature,e.payload})sb.Append(Lp(s));
   return Sha(sb.ToString());}
  string Serialize(RecoveryEnvelope043 e){
   var d=new Dictionary<string,object>();
   d["schema"]=Schema;d["version"]=e.version;d["generation"]=e.generation.ToString(Inv);d["savedUtc"]=e.savedUtc;d["fingerprint"]=e.fingerprint;
   d["canonicalPath"]=e.canonicalPath;d["sessionToken"]=e.sessionToken;d["baseRevision"]=e.baseRevision;d["geometrySignature"]=e.geometrySignature;
   d["payload"]=e.payload;d["sha256"]=Checksum(e);
   return Json().Serialize(d);}
  static string Str(Dictionary<string,object> d,string k,bool nullable){
   object v=d[k];if(v==null&&nullable)return null;
   var s=v as string;if(s==null)throw new BadEnvelope("SCHEMA","field '"+k+"' must be "+(nullable?"a string or null.":"a string."));
   if(!WellFormed(s))throw new BadEnvelope("SCHEMA","field '"+k+"' contains unpaired surrogates.");
   return s;}
  RecoveryEnvelope043 Parse(byte[] bytes){
   string text;try{text=Utf8.GetString(bytes);}catch(ArgumentException){throw new BadEnvelope("ENCODING","file is not valid UTF-8.");}
   Dictionary<string,object> d;
   try{d=Json().DeserializeObject(text) as Dictionary<string,object>;}catch(Exception ex){throw new BadEnvelope("JSON","JSON parse failed: "+ex.Message);}
   if(d==null)throw new BadEnvelope("SCHEMA","JSON root is not an object.");
   object v;
   if(!d.TryGetValue("schema",out v)||!Schema.Equals(v as string,ORD))throw new BadEnvelope("SCHEMA","schema is not "+Schema+".");
   if(!d.TryGetValue("version",out v)||!(v is int))throw new BadEnvelope("SCHEMA","version is missing or not an integer.");
   if((int)v!=Version)throw new BadEnvelope("UNSUPPORTED_VERSION","envelope version "+((int)v).ToString(Inv)+" is not supported (expected "+Version.ToString(Inv)+").");
   foreach(var k in d.Keys)if(Array.IndexOf(Fields,k)<0)throw new BadEnvelope("UNKNOWN_FIELD","unknown field '"+k+"'.");
   foreach(var k in Fields)if(!d.ContainsKey(k))throw new BadEnvelope("SCHEMA","missing field '"+k+"'.");
   var e=new RecoveryEnvelope043{version=Version,savedUtc=Str(d,"savedUtc",false),fingerprint=Str(d,"fingerprint",false),canonicalPath=Str(d,"canonicalPath",true),
    sessionToken=Str(d,"sessionToken",true),baseRevision=Str(d,"baseRevision",false),geometrySignature=Str(d,"geometrySignature",false),payload=Str(d,"payload",false)};
   long g;if(!long.TryParse(Str(d,"generation",false),NumberStyles.None,Inv,out g)||g<1)throw new BadEnvelope("SCHEMA","generation must be a positive integer string.");
   e.generation=g;
   DateTime t;
   if(!DateTime.TryParseExact(e.savedUtc,"o",Inv,DateTimeStyles.RoundtripKind,out t)||t.Kind!=DateTimeKind.Utc)throw new BadEnvelope("SCHEMA","savedUtc is not a UTC round-trip timestamp.");
   if(e.payload.Length>maxPayload)throw new BadEnvelope("OVERSIZE","payload exceeds "+maxPayload.ToString(Inv)+" chars.");
   var ie=CheckIdentity(Identity(e));if(ie!=null)throw new BadEnvelope("SCHEMA","stored identity invalid: "+ie);
   if(!string.Equals(Str(d,"sha256",false),Checksum(e),ORD))throw new BadEnvelope("CHECKSUM","checksum mismatch.");
   return e;}
  static bool Same(RecoveryEnvelope043 a,RecoveryEnvelope043 b){
   return a!=null&&b!=null&&a.version==b.version&&a.generation==b.generation&&string.Equals(a.savedUtc,b.savedUtc,ORD)&&SameDrawing(a,Identity(b))
    &&string.Equals(a.baseRevision,b.baseRevision,ORD)&&string.Equals(a.geometrySignature,b.geometrySignature,ORD)&&string.Equals(a.payload,b.payload,ORD);}
  static RecoveryIdentity043 Identity(RecoveryEnvelope043 e){
   return new RecoveryIdentity043{fingerprint=e.fingerprint,canonicalPath=e.canonicalPath,sessionToken=e.sessionToken,baseRevision=e.baseRevision,geometrySignature=e.geometrySignature};}
  static bool SameDrawing(RecoveryEnvelope043 e,RecoveryIdentity043 id){
   return string.Equals(e.fingerprint,id.fingerprint,ORD)&&string.Equals(e.sessionToken,id.sessionToken,ORD)
    &&(e.canonicalPath==null?id.canonicalPath==null:id.canonicalPath!=null&&string.Equals(e.canonicalPath,id.canonicalPath,OIC));}

  // ---------- identity, root and file helpers ----------
  static bool WellFormed(string s){
   for(int i=0;i<s.Length;i++){char c=s[i];
    if(char.IsHighSurrogate(c)){if(i+1>=s.Length||!char.IsLowSurrogate(s[i+1]))return false;i++;}
    else if(char.IsLowSurrogate(c))return false;}
   return true;}
  static string Field(string name,string v){
   if(string.IsNullOrWhiteSpace(v))return name+" is empty.";
   if(v.Length>MaxIdentityChars)return name+" is longer than "+MaxIdentityChars.ToString(Inv)+" chars.";
   if(!WellFormed(v)||v.Any(c=>char.IsControl(c)))return name+" contains control characters or unpaired surrogates.";
   return null;}
  static string CheckIdentity(RecoveryIdentity043 id){
   if(id==null)return "identity is null.";
   string e=Field("fingerprint",id.fingerprint)??Field("baseRevision",id.baseRevision)??Field("geometrySignature",id.geometrySignature);
   if(e!=null)return e;
   if((id.canonicalPath==null)==(id.sessionToken==null))return "exactly one of canonicalPath (saved DWG) or sessionToken (unsaved DWG) is required.";
   if(id.sessionToken!=null)return Field("sessionToken",id.sessionToken);
   e=Field("canonicalPath",id.canonicalPath);if(e!=null)return e;
   if(!IsAbs(id.canonicalPath))return "canonicalPath must be an absolute drive or UNC path.";
   string f;try{f=Path.GetFullPath(id.canonicalPath);}catch(Exception ex){return "canonicalPath is invalid: "+ex.Message;}
   return string.Equals(f,id.canonicalPath,ORD)?null:"canonicalPath is not canonical (Path.GetFullPath gives "+f+").";}
  static string Key(RecoveryIdentity043 id){
   bool saved=id.canonicalPath!=null;
   return Sha(Schema+"|KEY\n"+(saved?"PATH\n":"SESSION\n")+Lp(id.fingerprint)+Lp(saved?id.canonicalPath.ToUpperInvariant():id.sessionToken));}
  static bool IsAbs(string p){
   bool drive=p.Length>=3&&((p[0]>='A'&&p[0]<='Z')||(p[0]>='a'&&p[0]<='z'))&&p[1]==':'&&(p[2]=='\\'||p[2]=='/');
   bool unc=p.StartsWith(@"\\",ORD)&&!p.StartsWith(@"\\?\",ORD)&&!p.StartsWith(@"\\.\",ORD);
   return drive||unc;}
  static string Norm(string p){var f=Path.GetFullPath(p);return f.Length>3?f.TrimEnd('\\'):f;}
  static string CheckRoot(string r,string b,out string full){
   full=null;
   if(string.IsNullOrWhiteSpace(r)||string.IsNullOrWhiteSpace(b))return "ROOT_UNBOUND: root and boundary directories are both required.";
   if(!IsAbs(r)||!IsAbs(b))return "ROOT_INVALID: root and boundary must be absolute drive or UNC paths.";
   string fr=Norm(r),fb=Norm(b),pre=fb.EndsWith("\\",ORD)?fb:fb+"\\";
   if(!fr.Equals(fb,OIC)&&!fr.StartsWith(pre,OIC))return "ROOT_ESCAPE: root "+fr+" is outside boundary "+fb+".";
   if(!Directory.Exists(fb))return "ROOT_MISSING: boundary does not exist: "+fb;
   if(!Directory.Exists(fr))return "ROOT_MISSING: root does not exist: "+fr;
   for(var d=new DirectoryInfo(fr);d!=null;d=d.Parent){
    if((d.Attributes&FileAttributes.ReparsePoint)!=0)return "ROOT_REPARSE: reparse point/junction not allowed: "+d.FullName;
    if(Norm(d.FullName).Equals(fb,OIC)){full=fr;return null;}}
   return "ROOT_ESCAPE: boundary not reached from root.";}
  string FilePath(string key,string ext){return Path.Combine(root,Prefix+key+ext);}
  RecoveryResult043 New(string key){return new RecoveryResult043{primaryPath=FilePath(key,PrimaryExt),backupPath=FilePath(key,BackupExt)};}
  static string Code(string e){int i=e.IndexOf(':');return i>0?e.Substring(0,i):"PATH_INVALID";}
  static RecoveryResult043 Refuse(RecoveryResult043 r,string e){return Fail(r,Rejected,Code(e),e);}
  static RecoveryResult043 Unchanged(RecoveryResult043 r){r.message+=" Nothing committed; previous recovery data unchanged.";return r;}
  RecoveryResult043 RootFail(){return Refuse(new RecoveryResult043(),rootError);}
  // R043-F01: re-prove the construction-time root binding at operation time (root replaced/redirected/removed after construction).
  string Guard(){
   string full,e;try{e=CheckRoot(root,bound,out full);}catch(Exception ex){return "ROOT_INVALID: "+ex.GetType().Name+": "+ex.Message;}
   return e??(string.Equals(full,root,OIC)?null:"ROOT_ESCAPE: root no longer resolves to "+root+".");}
  // A slot must be directly in root and either absent or a plain file; reparse points (symlink/junction/mount) and directories are
  // refused and never opened, written, moved or deleted. GetAttributes does not follow a final-component link. An absent slot is
  // accepted only while root itself is still a non-reparse directory (checked here again, after Guard's full root..boundary walk).
  string CheckEntry(string p){
   if(!string.Equals(Path.GetDirectoryName(p),root,OIC))return "PATH_ESCAPE: "+p+" is not directly in root.";
   FileAttributes a;
   try{a=File.GetAttributes(root);}catch(Exception ex){return "ROOT_MISSING: root is not accessible: "+ex.Message;}
   if((a&FileAttributes.ReparsePoint)!=0)return "ROOT_REPARSE: reparse point/junction not allowed: "+root;
   if((a&FileAttributes.Directory)==0)return "ROOT_MISSING: root is not a directory: "+root;
   try{a=File.GetAttributes(p);}catch(FileNotFoundException){return null;}catch(DirectoryNotFoundException){return "ROOT_MISSING: root does not exist: "+root;}
   if((a&FileAttributes.ReparsePoint)!=0)return "PATH_REPARSE: reparse point not allowed, not followed: "+p;
   if((a&FileAttributes.Directory)!=0)return "PATH_NOT_FILE: a directory occupies "+p;
   return null;}
  RecoveryResult043 Pre(RecoveryIdentity043 id){
   if(rootError!=null)return RootFail();
   var g=Guard();if(g!=null)return Refuse(new RecoveryResult043(),g);
   var e=CheckIdentity(id);return e==null?null:Fail(new RecoveryResult043(),Rejected,"IDENTITY_INVALID",e);}
  static RecoveryResult043 Fail(RecoveryResult043 r,string status,string reason,string message){r.status=status;r.reasons.Add(reason);r.message=message;r.payload=null;return r;}
  FileStream TryLock(string key,out string err){
   err=null;
   try{return new FileStream(FilePath(key,LockExt),FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.None,1,FileOptions.DeleteOnClose);}
   catch(IOException ex){err=ex.Message;return null;}
   catch(UnauthorizedAccessException ex){err=ex.Message;return null;}}
  void Hook(string phase,string tmp){var h=FaultHook;if(h==null)return;try{h(phase,tmp);}catch(Exception ex){throw new InjectedFault(phase,ex);}}
  void TryDelete(string p){try{if(Guard()==null&&CheckEntry(p)==null&&File.Exists(p))File.Delete(p);}catch(Exception){}}
 }
}
