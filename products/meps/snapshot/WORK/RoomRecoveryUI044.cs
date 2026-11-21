using System;using System.IO;using System.Linq;using System.Drawing;using System.Globalization;using System.Collections.Generic;using System.Runtime.CompilerServices;using System.Windows.Forms;using IOE=System.InvalidOperationException;
#if ROOMCAD037 && BDGKL034
using Autodesk.AutoCAD.Geometry;using Document=Autodesk.AutoCAD.ApplicationServices.Document;using Database=Autodesk.AutoCAD.DatabaseServices.Database;
using ObjectEventArgs=Autodesk.AutoCAD.DatabaseServices.ObjectEventArgs;using ObjectErasedEventArgs=Autodesk.AutoCAD.DatabaseServices.ObjectErasedEventArgs;
namespace Tvxd.Meps {
 // TVXD-MEPS-RAW-DRAFT-044 native adapter of the raw-draft recovery (Vietnamese text: /codepage:65001). Owned by ONE RoomUi037.Run (bound command).
 // Pure parts: RoomRawDraft044.cs (payload TVXD_MEPS_RAWDRAFT044, overlay, signatures, keeper); file store: RoomRecovery043.cs (unchanged).
 //  - Root: <directory of the executing plugin assembly>\RECOVERY_DATA (own sub-folder; plugin folder and sub-folder must be plain, non-reparse
 //    directories before creation/use). QA only: explicit RootOverride + BoundaryOverride (existing absolute folders; the store re-proves the boundary).
 //    Root unusable or unwritable = recovery OFF with an explicit status; the quantity form works exactly as before.
 //  - Identity: fingerprint = RoomProjectStore037.DrawingId; saved DWG = Path.GetFullPath(db.Filename); unsaved = per-Document session token
 //    (ConditionalWeakTable, never a global "Drawing1"). baseRevision = hash of the freshly read NOD raw (or the explicit MISSING sentinel).
 //    geometrySignature = RoomCad037 signatures of every recovered room (changed drafts, open room, rooms with raw cells) on their recorded bases.
 //  - Start (after the fresh project read in RoomUi037.Run): Load; a candidate is only inspected, its room bases are rescanned read-only, and it is
 //    restored only on an exact match. Restore = recovered project document (in memory, projectDirty), selection, raw cells and the open room rebuilt
 //    from the CURRENT scan with RoomUi037.Rebuild (quantities recomputed; no cached total/PASS). No NOD write, no QSAVE, no DWG change.
 //  - Checkpoints: every accepted mutation (form hook Accepted, every Run action round, after SaveProject) and every raw text input (editing control
 //    TextChanged, explicit flush in CommitEdits and before the form's FormClosing CancelEdit). Raw input reuses the cached accepted part (no CAD scan,
 //    no project re-encode per keystroke). A failed write keeps every input in the form and shows CHƯA LƯU ĐƯỢC NHÁP.
 //  - Raw cells survive refused commits and close; they are shown red in the grid, listed in VƯỚNG MẮC and block Excel export until corrected or
 //    explicitly discarded (BỎ Ô NHÁP). Accepted declarations alone drive Recalc.
 //  - PATCH1 (R044-QA-F01): before every write or "kept" claim (also an unchanged payload) Live() runs the stored readGuard, re-proves the bound drawing
 //    identity + fingerprint and compares a FRESH NOD read with the keeper base. Mismatch = keeper suspended for the session (no write, previous files
 //    untouched, no rebase, held status); read failure = nothing written this time. (R044-QA-F02): a raw cell is removed only together with a fresh
 //    accepted capture, so no durable snapshot ever holds neither the raw text nor the accepted value.
 //  - PATCH2 (R044-QA-F03): this controller watches ObjectAppended/Modified/Erased of its own Database (count only). After any event (or an INSUNITS
 //    change) the next checkpoint rescans the recorded room bases once and compares them with the last proven signatures; a change = keeper suspended
 //    (LIVE_SOURCE_CHANGED, sticky). No events = no scan. Subscription released by Close/End (also on failure).
 //  - After a successful explicit SaveProject the NOD is read again; only if it equals the saved raw (and the identity is proven) does the base advance.
 //  - Conflict/corrupt/stale candidates are HELD (kept, never auto-restored or overwritten); recovery UI for them is a later gate (no merge here).
 public sealed class RoomRecoveryUi044 {
  public const string Folder="RECOVERY_DATA";public const int MaxPayloadChars=8000000;
  // QA hooks (both or none; absolute existing folders, root equal to or inside boundary). Reset by RoomUi037.ResetHooks.
  public static string RootOverride,BoundaryOverride;
  // QA inspection: controller of the latest RoomUi037.Run (null after ResetHooks).
  public static RoomRecoveryUi044 Last;
  public static void ResetHooks(){RootOverride=null;BoundaryOverride=null;Last=null;}
  static readonly ConditionalWeakTable<Document,string> Tokens=new ConditionalWeakTable<Document,string>();
  static readonly StringComparer O=StringComparer.Ordinal;
  static readonly CultureInfo Inv=CultureInfo.InvariantCulture;
  const string RowKind=RawDraftCodec044.Row,SupKind=RawDraftCodec044.Supplement;
  public static string SessionToken(Document d){if(d==null)throw new ArgumentNullException("d");return Tokens.GetValue(d,k=>"S044-"+Guid.NewGuid().ToString("N"));}

  readonly Database db;readonly RoomSession037 s;RoomRecovery043 store;string fingerprint;
  public RawDraftKeeper044 Keeper;public readonly RawOverlay044 Overlay=new RawOverlay044();
  public string OffReason;            // recovery disabled (Vietnamese reason); null = enabled
  public string StartNote;public bool StartWarn;public int OtherCandidates;public string StatusText="";public bool StatusError;
  public string Root,Boundary;
  Dictionary<string,string> baseDrafts=new Dictionary<string,string>(O);       // room ID -> draft JSON of the DWG base
  readonly Dictionary<string,RawScan044> knownBasis=new Dictionary<string,RawScan044>(O);
  Dictionary<string,RoomCadScan037> loadScans=new Dictionary<string,RoomCadScan037>(O);
  Part cache;RoomReport037 form;bool closing,busy;Control editor;string edKind,edKey,edField;
  string captureError;
  // PATCH1 R044-QA-F01: stored readGuard of the Run + provider of the live drawing identity; boundIdentity = its value at Bind.
  Action<string> liveGuard;Func<string> identityNow;string boundIdentity;
  // PATCH2 R044-QA-F03: source-change invalidation of THIS controller and THIS Database only (instance handlers, no static revision, no cross-document
  // state). The callbacks only count (no transaction, no scan, no object access). Live() re-proves the recorded room signatures with RoomCad037 rescans
  // only when events happened (or INSUNITS changed) since the last proof; an unchanged context costs no scan. proven = room ID -> basis + signature
  // last proven current (load rescans, then every successful checkpoint).
  bool watching;long sourceEvents,provenEvents;int provenInsunits;readonly Dictionary<string,RawScan044> proven=new Dictionary<string,RawScan044>(O);
  public bool SourceWatching{get{return watching;}}public long SourceEvents{get{return sourceEvents;}}   // QA: lifecycle / no-leak inspection
  void OnObject(object o,ObjectEventArgs e){sourceEvents++;}
  void OnErased(object o,ObjectErasedEventArgs e){sourceEvents++;}
  // watching is set first so a partial subscription failure is still fully released by End (-= of a handler never added is harmless).
  void Watch(){watching=true;db.ObjectAppended+=OnObject;db.ObjectModified+=OnObject;db.ObjectErased+=OnErased;
   provenEvents=sourceEvents;provenInsunits=(int)db.Insunits;}
  // Releases the subscription (idempotent). Called by Close (finally), by Run on failure and on a failed Start. After End no checkpoint can write
  // (Live refuses without a watch), so protection never silently turns off while the controller is still used.
  public void End(){if(!watching)return;watching=false;
   try{db.ObjectAppended-=OnObject;}catch(Exception){}try{db.ObjectModified-=OnObject;}catch(Exception){}try{db.ObjectErased-=OnErased;}catch(Exception){}}
  static string BasisKey(RawScan044 b){return b.units+"|"+RawDraftCodec044.R(b.tolerance)+"|"+RawDraftCodec044.R(b.minX)+"|"+RawDraftCodec044.R(b.minY)+"|"+RawDraftCodec044.R(b.maxX)+"|"+RawDraftCodec044.R(b.maxY);}
  string ScanNow(RawScan044 b){return RoomCad037.Scan(db,new Point3d(b.minX,b.minY,0),new Point3d(b.maxX,b.maxY,0),b.roomId,b.units,b.tolerance).signature;}
  // null = every recorded room of p is still the source it was proven on; otherwise the reason. Throws on a scan error (caller: transient failure).
  string SourceChanged(RawDraftPayload044 p){
   foreach(var x in p.scans){if(x.units==RawDraftCodec044.NoUnits||x.signature==null)continue;   // unverifiable rooms are never restorable anyway
    var now=ScanNow(x);
    if(now==null||!O.Equals(now,x.signature))return "room "+x.roomId+": recorded source signature is no longer current";
    RawScan044 was;if(!proven.TryGetValue(x.roomId,out was))continue;
    if(O.Equals(BasisKey(was),BasisKey(x))){if(!O.Equals(was.signature,now))return "room "+x.roomId+": source changed since it was proven";}
    else{var old=ScanNow(was);if(old==null||!O.Equals(old,was.signature))return "room "+x.roomId+": source of the previously proven basis changed";}}   // re-picked room
   return null;}
  void Prove(RawDraftPayload044 p){foreach(var x in p.scans)if(x.units!=RawDraftCodec044.NoUnits&&x.signature!=null)proven[x.roomId]=Copy(x);}

  RoomRecoveryUi044(Database db,RoomSession037 s){this.db=db;this.s=s;}
  // Live identity of the bound drawing (no CAD scan): same Database object, saved document path or unsaved session token. Saving an unsaved drawing,
  // Save As and a different database all change it. Document.Name is used (not db.Filename) so a background autosave file name can never be taken
  // for a Save As (unverified hypothesis about Filename during autosave; Name is the document's own path either way).
  static string DocumentKey(Document d,Database db){if(!ReferenceEquals(d.Database,db))return "OTHER_DATABASE";
   return d.IsNamedDrawing&&!string.IsNullOrEmpty(d.Name)?"SAVED|"+Path.GetFullPath(d.Name):"UNSAVED|"+SessionToken(d);}
  static string DatabaseKey(Database db){var f=db.Filename;return string.IsNullOrEmpty(f)?"DB_UNSAVED|":"DB_SAVED|"+f;}

  // ---- start ----
  public static RoomRecoveryUi044 Start(Document d,RoomSession037 s,Action<string> readGuard){if(d==null)throw new ArgumentNullException("d");
   string path=null,token=null,why=null;var db=d.Database;
   try{if(d.IsNamedDrawing&&!string.IsNullOrEmpty(db.Filename))path=Path.GetFullPath(db.Filename);else token=SessionToken(d);}
   catch(Exception ex){why="không xác định được bản vẽ ("+ex.GetType().Name+": "+ex.Message+")";}
   return Start(db,path,token,s,readGuard,why,()=>DocumentKey(d,db));}
  // QA entry (deterministic identity): exactly one of canonicalPath / sessionToken. The live identity check compares the ACTUAL database (Filename =
  // saved path or empty) against its own value at Start, never against the supplied fixture key.
  public static RoomRecoveryUi044 Start(Database db,string canonicalPath,string sessionToken,RoomSession037 s,Action<string> readGuard){
   return Start(db,canonicalPath,sessionToken,s,readGuard,null,db==null?null:(Func<string>)(()=>DatabaseKey(db)));}
  static RoomRecoveryUi044 Start(Database db,string canonicalPath,string sessionToken,RoomSession037 s,Action<string> readGuard,string why,Func<string> identityNow){
   if(s==null)throw new ArgumentNullException("s");
   var r=new RoomRecoveryUi044(db,s);Last=r;s.rawOverlay=r.Overlay;r.liveGuard=readGuard;r.identityNow=identityNow;
   if(why==null){try{why=r.Bind(canonicalPath,sessionToken,readGuard);}catch(Exception ex){why="lỗi khi mở nháp khôi phục ("+ex.GetType().Name+": "+ex.Message+")";}}
   if(why!=null){r.End();r.OffReason=why;r.Overlay.Clear();if(r.Keeper!=null&&r.Keeper.State==RawDraftKeeper044.Ready)r.Keeper.Disable("OFF",why);
    r.StartNote="NHÁP KHÔI PHỤC: TẮT — "+why+". Thay đổi chưa lưu sẽ mất khi đóng bảng (như trước).";r.StartWarn=true;}
   r.Status();return r;}

  string Bind(string path,string token,Action<string> readGuard){
   if(!s.projectMode)return "chỉ dùng trong chế độ dự án";
   if(s.projectError!=null)return "dữ liệu dự án trong bản vẽ không đọc được";
   if(db==null)return "không có bản vẽ";
   if(!RawDraftCodec044.RowFields.SequenceEqual(RoomAdjustmentsUi037.CellFields,O)||!RawDraftCodec044.SupplementFields.SequenceEqual(RoomAdjustmentsUi037.SupplementFields,O))
    return "danh sách cột nhập của MEPS khác lược đồ nháp 044 (lỗi phiên bản)";
   if((path==null)==(token==null))return "cần đúng một nhận dạng bản vẽ (đường dẫn hoặc phiên)";
   try{fingerprint=RoomProjectStore037.DrawingId(db);}catch(IOE ex){return ex.Message;}
   if(identityNow==null)return "không có cách kiểm tra lại nhận dạng bản vẽ";
   boundIdentity=identityNow();if(string.IsNullOrEmpty(boundIdentity))return "không xác định được bản vẽ";
   string root,boundary;var e=ResolveRoot(out root,out boundary);if(e!=null)return e;Root=root;Boundary=boundary;
   store=new RoomRecovery043(root,boundary,MaxPayloadChars);if(store.RootError!=null)return "thư mục nháp không dùng được ("+store.RootError+")";
   e=Probe(root);if(e!=null)return e;
   baseDrafts=Drafts(s.projectDocument);
   Keeper=new RawDraftKeeper044(store,fingerprint,path,token,RawDraftSign044.BaseRevision(s.projectRaw));
   // PATCH2 F03: watch this Database BEFORE any candidate is inspected/applied (a subscription failure = recovery OFF, nothing applied).
   try{Watch();}catch(Exception ex){End();return "không theo dõi được thay đổi hình CAD ("+ex.GetType().Name+": "+ex.Message+")";}
   if(readGuard!=null)readGuard("trước khi kiểm tra nháp khôi phục");
   RoomProjectDocument037 candidateDoc=null;
   var p=Keeper.Open(x=>{candidateDoc=Check(x);loadScans=new Dictionary<string,RoomCadScan037>(O);return RawDraftSign044.Current(x.scans,Rescan);});
   if(p!=null)Apply(p,candidateDoc);
   // PATCH2 F03: baseline = the read-only load rescans just made (exact current signatures). A held/disabled keeper never writes: no watch needed.
   foreach(var b in knownBasis.Values)proven[b.roomId]=Copy(b);
   if(Keeper.State!=RawDraftKeeper044.Ready)End();
   if(Keeper.Outcome==RawDraftKeeper044.Fresh)OtherCandidates=CountCandidates();
   StartNote=Note(p);return null;}

  // Default <plugin dir>\RECOVERY_DATA, else the explicit QA pair. null = ok.
  public static string ResolveRoot(out string root,out string boundary){root=null;boundary=null;
   try{
    if(RootOverride!=null||BoundaryOverride!=null){
     if(string.IsNullOrWhiteSpace(RootOverride)||string.IsNullOrWhiteSpace(BoundaryOverride))return "cấu hình nháp QA cần cả thư mục và ranh giới";
     if(!Path.IsPathRooted(RootOverride)||!Path.IsPathRooted(BoundaryOverride))return "thư mục nháp QA phải là đường dẫn tuyệt đối";
     root=Path.GetFullPath(RootOverride);boundary=Path.GetFullPath(BoundaryOverride);
     return PlainDir(boundary,"ranh giới nháp QA")??PlainDir(root,"thư mục nháp QA");}   // QA folders are never created here
    var loc=typeof(RoomRecoveryUi044).Assembly.Location;
    if(string.IsNullOrEmpty(loc))return "không xác định được thư mục của plugin";
    boundary=Path.GetDirectoryName(Path.GetFullPath(loc));
    var err=PlainDir(boundary,"thư mục plugin");if(err!=null)return err;
    root=Path.Combine(boundary,Folder);
    if(!Directory.Exists(root)&&!File.Exists(root))Directory.CreateDirectory(root);   // parent proven plain above: creates only the own sub-folder
    return PlainDir(root,"thư mục nháp "+Folder);}
   catch(Exception ex){root=null;return "thư mục nháp không tạo/đọc được ("+ex.GetType().Name+": "+ex.Message+")";}}
  static string PlainDir(string p,string what){FileAttributes a;
   try{a=File.GetAttributes(p);}catch(Exception ex){return what+" không truy cập được ("+ex.Message+")";}
   if((a&FileAttributes.ReparsePoint)!=0)return what+" là liên kết/junction; MEPS không dùng";
   if((a&FileAttributes.Directory)==0)return what+" không phải thư mục";
   return null;}
  static string Probe(string root){
   try{var p=Path.Combine(root,"W044_probe_"+Guid.NewGuid().ToString("N")+".tmp");
    using(var fs=new FileStream(p,FileMode.CreateNew,FileAccess.Write,FileShare.None,1,FileOptions.DeleteOnClose)){fs.WriteByte(0);fs.Flush(true);}
    return null;}
   catch(Exception ex){return "thư mục nháp không ghi được ("+ex.GetType().Name+": "+ex.Message+")";}}
  int CountCandidates(){try{var own=Keeper.PrimaryPath;
    return store.Candidates(fingerprint).Count(x=>x.candidate!=null&&!string.Equals(x.primaryPath,own,StringComparison.OrdinalIgnoreCase));}catch(Exception){return 0;}}
  static Dictionary<string,string> Drafts(RoomProjectDocument037 d){var m=new Dictionary<string,string>(O);
   if(d!=null&&d.drafts!=null)foreach(var x in d.drafts)if(x!=null&&x.roomId!=null&&!m.ContainsKey(x.roomId))m[x.roomId]=x.draftJson;
   return m;}

  // Candidate inspection (nothing applied). Invalid content = RawDraftFormat044 (keeper: HELD_CORRUPT, candidate kept).
  RoomProjectDocument037 Check(RawDraftPayload044 p){RoomProjectDocument037 doc;
   if(p.projectJson==null)doc=RoomProjectUi037.CopyDocument(s.projectDocument);
   else{try{doc=RoomProjectStore037.Decode(p.projectJson);}catch(IOE ex){throw new RawDraftFormat044("PROJECT_INVALID",ex.Message);}
    if(!string.Equals(doc.model.hierarchy.drawingId,fingerprint,StringComparison.OrdinalIgnoreCase))throw new RawDraftFormat044("PROJECT_OTHER_DRAWING","recovered project belongs to another drawing.");}
   if(doc==null&&(p.cells.Count>0||p.openRoom!=null))throw new RawDraftFormat044("NO_PROJECT","raw cells or an open room without a project document.");
   if(p.openRoom!=null){HierFloor037 fl;
    if(p.openRoom.units!=RoomQty037.MM&&p.openRoom.units!=RoomQty037.M)throw new RawDraftFormat044("UNITS","open room units are not MM or M.");
    if(RoomProjectUi037.FindRoom(doc.model,p.openRoom.roomId,out fl)==null)throw new RawDraftFormat044("OPEN_ROOM_UNKNOWN","open room is not in the recovered project.");
    if(!p.scans.Any(x=>O.Equals(x.roomId,p.openRoom.roomId)))throw new RawDraftFormat044("OPEN_ROOM_UNSCANNED","open room has no recorded scan basis.");}
   foreach(var x in p.scans)if(x.units!=RawDraftCodec044.NoUnits&&x.units!=RoomQty037.MM&&x.units!=RoomQty037.M)throw new RawDraftFormat044("UNITS","scan units are not MM or M.");
   return doc;}
  // Read-only RoomCad037 scan of one recorded basis (load time only). null = incomplete.
  string Rescan(RawScan044 b){var sc=RoomCad037.Scan(db,new Point3d(b.minX,b.minY,0),new Point3d(b.maxX,b.maxY,0),b.roomId,b.units,b.tolerance);loadScans[b.roomId]=sc;
   if(sc.signature!=null)knownBasis[b.roomId]=new RawScan044{roomId=b.roomId,units=b.units,tolerance=b.tolerance,minX=b.minX,minY=b.minY,maxX=b.maxX,maxY=b.maxY,signature=sc.signature};
   return sc.signature;}

  // Exact match only (keeper). The session changes here and nowhere else at start.
  void Apply(RawDraftPayload044 p,RoomProjectDocument037 doc){
   var keepDoc=s.projectDocument;bool keepDirty=s.projectDirty;string keepRoom=s.selectedRoomId,keepFloor=s.selectedFloorId;
   try{
    s.projectDocument=doc;s.projectDirty=p.projectDirty;Overlay.Replace(p.cells);HierFloor037 fl;
    if(p.openRoom!=null){var o=p.openRoom;RoomCadScan037 scan;
     if(RoomProjectUi037.FindRoom(doc.model,o.roomId,out fl)!=null&&loadScans.TryGetValue(o.roomId,out scan)&&scan.signature!=null)OpenRoom(o,fl,scan,doc);}
    else if(p.selectedRoomId!=null&&doc!=null&&RoomProjectUi037.FindRoom(doc.model,p.selectedRoomId,out fl)!=null){s.selectedRoomId=p.selectedRoomId;s.selectedFloorId=fl.id;}
    cache=null;}
   catch(Exception ex){
    s.projectDocument=keepDoc;s.projectDirty=keepDirty;s.selectedRoomId=keepRoom;s.selectedFloorId=keepFloor;Overlay.Clear();
    s.batch=null;s.scan=null;s.roomId=null;s.hasRegion=false;s.saved=null;s.adjustments=null;s.dirty=false;s.unmatched=new List<RoomDraftRow037>();
    Keeper.Disable("APPLY_FAILED",ex.Message);   // candidate stays on disk untouched
    throw new IOE("không áp dụng được nháp ("+ex.Message+"); nháp trên máy giữ nguyên");}}
  // Same session state as RoomUi037.RegionProject after a scan of a project room with a saved rectangle; the draft comes from the recovered document.
  void OpenRoom(RawOpenRoom044 o,HierFloor037 fl,RoomCadScan037 scan,RoomProjectDocument037 doc){
   s.roomId=o.roomId;s.selectedRoomId=o.roomId;s.selectedFloorId=fl.id;s.units=o.units;s.tolerance=o.tolerance;s.unitConfirmed=o.unitConfirmed;
   s.minX=scan.minX;s.minY=scan.minY;s.maxX=scan.maxX;s.maxY=scan.maxY;s.hasRegion=true;
   s.saved=null;s.savedRaw=null;s.loadError=null;s.devices.Clear();s.unmatched=new List<RoomDraftRow037>();s.dirty=false;s.heights=null;s.adjustments=null;
   IList<RoomDraftRow037> decl=null;string mode=null;bool indep=false;string err;var draft=RoomProjectUi037.DraftOf(doc,o.roomId,out err);
   if(err!=null)s.loadError="Bản nháp của phòng không đọc được: "+err+" (MEPS không ghi đè).";
   else if(draft!=null){s.saved=draft;decl=draft.rows;mode=draft.conduitMode;indep=draft.independentAssembliesConfirmed;s.adjustments=RoomAdjustments037.Copy(draft.adjustments);}
   RoomUi037.Rebuild(s,scan,decl,mode,indep);}

  string Note(RawDraftPayload044 p){var k=Keeper;
   if(k.Outcome==RawDraftKeeper044.Restored&&p!=null){
    var b=new List<string>();int here=s.roomId==null?0:Overlay.Room(s.roomId).Count;
    if(p.projectDirty)b.Add("có thay đổi dự án / phòng chưa lưu");
    if(Overlay.Count>0)b.Add(Overlay.Count.ToString(Inv)+" ô nhập chưa áp dụng (tô đỏ, chặn xuất Excel)"+(here<Overlay.Count?" — "+(Overlay.Count-here).ToString(Inv)+" ô ở phòng khác":""));
    b.Add(s.batch!=null?"đã quét lại phòng "+RoomProjectUi037.RoomLabel(s)+" từ bản vẽ hiện tại":s.selectedRoomId!=null?"bấm 'Chọn vùng CAD' để mở phòng đã chọn":"");
    StartWarn=true;
    return "ĐÃ KHÔI PHỤC NHÁP lưu lúc "+When(k.LastSavedUtc)+" (nháp trên máy, CHƯA ghi vào bản vẽ): "+string.Join("; ",b.Where(x=>x.Length>0))+". Bấm 'Lưu vào bản vẽ' để ghi.";}
   if(k.State!=RawDraftKeeper044.Ready){StartWarn=true;return HeldText();}
   if(OtherCandidates>0)return "Có "+OtherCandidates.ToString(Inv)+" nháp khôi phục khác của cùng mã bản vẽ (đường dẫn/phiên khác, ví dụ trước khi Save As): MEPS KHÔNG tự chuyển sang bản vẽ này.";
   return null;}

  // ---- form hooks ----
  public void Attach(RoomReport037 f){if(f==null)return;form=f;closing=false;editor=null;
   f.RawCellText=(key,field)=>OffReason!=null||s.roomId==null?null:Overlay.Get(s.roomId,RowKind,key,field);
   f.RawSupplementText=(id,field)=>OffReason!=null||s.roomId==null?null:Overlay.Get(s.roomId,SupKind,id,field);
   f.RawDone=Done;f.RawDoneMany=DoneMany;f.RawFlush=Flush;f.BeforeClose=BeforeClose;f.CloseCancelled=()=>{closing=false;};f.Accepted=Changed;f.DiscardRaw=Discard;
   f.RouteGrid.EditingControlShowing+=(o,e)=>Showing(f.RouteGrid,RowKind,RoomReport037.RouteFields,e);
   f.SupplementGrid.EditingControlShowing+=(o,e)=>Showing(f.SupplementGrid,SupKind,RoomReport037.SupplementFields,e);
   // The grid reuses its editing control: detach at every end of edit (after the form's own CellEndEdit commit), so the next cell's initial text
   // is never recorded under this cell's key.
   f.RouteGrid.CellEndEdit+=(o,e)=>Detach();f.SupplementGrid.CellEndEdit+=(o,e)=>Detach();
   f.Disposed+=(o,e)=>{if(form==f){Detach();form=null;}};
   Status();}
  void Showing(DataGridView g,string kind,string[] fields,DataGridViewEditingControlShowingEventArgs e){Detach();if(closing||OffReason!=null||e.Control==null)return;
   var c=g.CurrentCell;if(c==null||c.RowIndex<0||c.ColumnIndex<0||c.ColumnIndex>=fields.Length||fields[c.ColumnIndex]==null)return;
   var tag=g.Rows[c.RowIndex].Tag;var row=tag as RoomCompletionRow037;string key=row!=null?row.rowKey:tag as string;if(key==null)return;
   editor=e.Control;edKind=kind;edKey=key;edField=fields[c.ColumnIndex];editor.TextChanged+=OnText;}
  void Detach(){if(editor!=null){editor.TextChanged-=OnText;editor=null;}}
  void OnText(object sender,EventArgs e){if(busy||closing||editor==null||!ReferenceEquals(sender,editor))return;busy=true;
   try{RawInput(edKind,edKey,edField,editor.Text??"");}catch(Exception ex){Fail(ex);}finally{busy=false;}}
  // Explicit read of the open editor (CommitEdits / before close): covers input that raised no TextChanged.
  void Flush(){if(busy||closing||editor==null)return;busy=true;try{RawInput(edKind,edKey,edField,editor.Text??"");}catch(Exception ex){Fail(ex);}finally{busy=false;}}
  void BeforeClose(){Flush();closing=true;Detach();Checkpoint();}
  // The cell now shows its accepted value (commit succeeded or the edit ended unchanged): its raw text is resolved. Ignored while closing.
  // PATCH1 R044-QA-F02: the form calls this right after the model was changed (CommitCell/CommitSupplement) and BEFORE its Accepted hook. The raw text
  // is dropped only together with a fresh accepted capture (cache invalidated), so the one write that removes the raw cell carries the new accepted
  // value: every durable snapshot holds either the raw text or the accepted value, never neither. The later Accepted/Changed finds the same payload.
  void Done(string kind,string key,string field){if(closing)return;Detach();if(s.roomId!=null&&Overlay.Remove(s.roomId,kind,key,field)){cache=null;Checkpoint();}}
  // 045 bulk apply (paste / fill / undo): called once AFTER the whole accepted change; every listed raw cell {kind,key,field} of the open room is
  // dropped together with ONE fresh accepted capture (same F02 rule as Done: never a snapshot without both).
  void DoneMany(List<string[]> cells){if(closing)return;Detach();if(s.roomId==null||cells==null)return;bool any=false;
   foreach(var c in cells)if(c!=null&&c.Length==3&&Overlay.Remove(s.roomId,c[0],c[1],c[2]))any=true;
   if(any){cache=null;Checkpoint();}}
  public void RawInput(string kind,string key,string field,string text){if(OffReason!=null||s.roomId==null||text==null)return;
   var acc=AcceptedText(kind,key,field);if(acc==null)return;
   bool same=string.Equals(text,acc,StringComparison.Ordinal);
   bool ch=same?Overlay.Remove(s.roomId,kind,key,field):Overlay.Set(s.roomId,kind,key,field,text);
   if(ch){if(same)cache=null;Checkpoint();}}   // F02: a raw cell resolved by the accepted value is removed only with a fresh accepted capture
  string AcceptedText(string kind,string key,string field){if(s.batch==null)return null;
   if(kind==RowKind){var r=s.batch.rows.FirstOrDefault(q=>q.rowKey!=null&&O.Equals(q.rowKey,key));return r==null||r.input==null?null:RoomAdjustmentsUi037.CellText(r.input,field);}
   var x=RoomAdjustmentsUi037.Supplement(s,key);return x==null?null:RoomAdjustmentsUi037.SupplementText(x,field);}
  // Accepted mutation (form action, Run round): accepted part recomputed, then checkpoint.
  public void Changed(){cache=null;Checkpoint();}
  // Explicit user action: drop every raw cell (all rooms); accepted values stay as they are.
  public void Discard(){if(form!=null){closing=true;try{foreach(var g in new[]{form.RouteGrid,form.SupplementGrid})if(g.IsCurrentCellInEditMode){g.CancelEdit();g.EndEdit();}}finally{closing=false;}Detach();}
   int n=Overlay.Clear();Checkpoint();
   if(form!=null){form.ShowSession();form.SetStatus(n==0?"Không có ô nhập nháp nào.":"Đã bỏ "+n.ToString(Inv)+" ô nhập nháp chưa áp dụng; bảng hiện giá trị đã áp dụng.",false);}}

  // ---- capture / checkpoint ----
  sealed class Part {public string projectJson,selectedRoomId,selectedFloorId;public bool projectDirty;public RawOpenRoom044 openRoom;public RawScan044 openBasis;
   public RoomProjectDocument037 doc;public readonly List<string> changedRooms=new List<string>();public readonly Dictionary<string,RawScan044> draftBasis=new Dictionary<string,RawScan044>(StringComparer.Ordinal);}
  Part Accepted(){var a=new Part();
   bool open=s.batch!=null&&s.scan!=null&&s.scan.signature!=null&&s.roomId!=null&&(s.units==RoomQty037.MM||s.units==RoomQty037.M);
   a.selectedRoomId=s.batch!=null?s.roomId:s.selectedRoomId;a.selectedFloorId=s.selectedFloorId;
   var doc=RoomProjectUi037.CopyDocument(s.projectDocument);HierFloor037 fl;
   // Pre-scan project edits are in projectDocument already; the open room's accepted declarations join a CLONE (no NOD write) like a room switch.
   bool roomDraft=open&&s.dirty&&s.loadError==null&&doc!=null&&RoomProjectUi037.FindRoom(doc.model,s.roomId,out fl)!=null;
   if(roomDraft){RoomProjectUi037.PutDraft(doc,RoomUi037.ToDraft(s,false));RoomFastEntryUi045.PutRoom(doc,s);}   // 045: row presets / RIÊNG blanks with the draft
   a.projectDirty=(s.projectDirty||roomDraft)&&doc!=null;
   if(a.projectDirty)a.projectJson=RoomProjectStore037.Encode(doc);
   if(open){a.openRoom=new RawOpenRoom044{roomId=s.roomId,units=s.units,unitConfirmed=s.unitConfirmed,tolerance=s.tolerance,minX=s.minX,minY=s.minY,maxX=s.maxX,maxY=s.maxY};
    a.openBasis=new RawScan044{roomId=s.roomId,units=s.units,tolerance=s.tolerance,minX=s.minX,minY=s.minY,maxX=s.maxX,maxY=s.maxY,signature=s.scan.signature};
    knownBasis[s.roomId]=a.openBasis;}
   a.doc=doc;
   if(a.projectDirty&&doc.drafts!=null)foreach(var x in doc.drafts){if(x==null||x.roomId==null)continue;string b;
    if(!baseDrafts.TryGetValue(x.roomId,out b)||!string.Equals(b,x.draftJson,StringComparison.Ordinal))a.changedRooms.Add(x.roomId);}
   return a;}
  RawScan044 Basis(Part a,string roomId){RawScan044 b;
   if(a.openBasis!=null&&O.Equals(a.openBasis.roomId,roomId))return Copy(a.openBasis);
   if(knownBasis.TryGetValue(roomId,out b))return Copy(b);
   if(!a.draftBasis.TryGetValue(roomId,out b)){string err;var d=a.doc==null?null:RoomProjectUi037.DraftOf(a.doc,roomId,out err);
    b=d==null||(d.units!=RoomQty037.MM&&d.units!=RoomQty037.M)?new RawScan044{roomId=roomId,units=RawDraftCodec044.NoUnits}:
     new RawScan044{roomId=roomId,units=d.units,tolerance=d.tolerance,minX=d.minX,minY=d.minY,maxX=d.maxX,maxY=d.maxY,signature=d.signature};
    a.draftBasis[roomId]=b;}
   return Copy(b);}
  static RawScan044 Copy(RawScan044 b){return new RawScan044{roomId=b.roomId,units=b.units,tolerance=b.tolerance,minX=b.minX,minY=b.minY,maxX=b.maxX,maxY=b.maxY,signature=b.signature};}
  public RawDraftPayload044 Capture(){if(cache==null)cache=Accepted();var a=cache;
   var p=new RawDraftPayload044{projectJson=a.projectJson,projectDirty=a.projectDirty,selectedRoomId=a.selectedRoomId,selectedFloorId=a.selectedFloorId};
   if(a.openRoom!=null)p.openRoom=new RawOpenRoom044{roomId=a.openRoom.roomId,units=a.openRoom.units,unitConfirmed=a.openRoom.unitConfirmed,tolerance=a.openRoom.tolerance,
    minX=a.openRoom.minX,minY=a.openRoom.minY,maxX=a.openRoom.maxX,maxY=a.openRoom.maxY};
   p.cells=Overlay.All();
   var rooms=new SortedSet<string>(a.changedRooms,O);if(a.openRoom!=null)rooms.Add(a.openRoom.roomId);foreach(var c in p.cells)rooms.Add(c.roomId);
   foreach(var id in rooms)p.scans.Add(Basis(a,id));
   return p;}
  // Raw texts of the open room that now equal the accepted value are resolved. true = something removed (F02: caller recaptures the accepted part).
  bool Reconcile(){if(s.roomId==null||s.batch==null)return false;bool any=false;
   foreach(var c in Overlay.Room(s.roomId)){var acc=AcceptedText(c.kind,c.key,c.field);
    if(acc!=null&&string.Equals(acc,c.text,StringComparison.Ordinal)&&Overlay.Remove(c.roomId,c.kind,c.key,c.field))any=true;}
   return any;}
  // true = the current capture is on disk (written and read back now or earlier) AND the live drawing still is the bound context (PATCH1 F01: proven
  // by Live() right before the keeper may write or report Holds, also when the payload is unchanged).
  public bool Checkpoint(){if(OffReason!=null||Keeper==null){Status();return false;}
   RawDraftPayload044 p;
   try{if(Reconcile())cache=null;p=Capture();captureError=null;}
   catch(Exception ex){captureError=ex.Message;Keeper.Failed("CAPTURE",ex.Message);Status();return false;}
   if(Keeper.State!=RawDraftKeeper044.Ready||!Live("trước khi lưu nháp khôi phục",p)){Status();return false;}
   Keeper.Checkpoint(p);Status();return Keeper.Holds(p);}
  // PATCH1 R044-QA-F01. Proves that the bound drawing is still the context of this draft: stored readGuard, same identity (database object, saved
  // path / unsaved token or, for the QA entry, the actual database Filename at Start), same fingerprint, and the FRESHLY read project NOD gives the
  // keeper's base revision. No CAD scan for these (PATCH2 source check below rescans only after DB events). Read/scan problem = transient failure (nothing written, LastFailed, next checkpoint proves again). Mismatch =
  // keeper suspended for the session (sticky, previous recovery files untouched, never rebased automatically).
  // PATCH2: + source context of p (see SourceChanged); success proves p's room signatures as the new baseline.
  bool Live(string when,RawDraftPayload044 p){string code=null,why=null;
   if(!watching){Keeper.Failed("SOURCE_WATCH_OFF","không còn theo dõi thay đổi hình CAD; nháp trên máy giữ nguyên");return false;}
   try{if(liveGuard!=null)liveGuard(when);
    var id=identityNow==null?null:identityNow();
    if(id==null||!string.Equals(id,boundIdentity,StringComparison.OrdinalIgnoreCase)){code=RawDraftKeeper044.LiveIdentityChanged;why="identity "+boundIdentity+" -> "+(id??"?");}
    else if(!string.Equals(RoomProjectStore037.DrawingId(db),fingerprint,StringComparison.OrdinalIgnoreCase)){code=RawDraftKeeper044.LiveIdentityChanged;why="drawing fingerprint changed";}
    else{var r=RoomProjectStore037.Read(db);var rev=RawDraftSign044.BaseRevision(r.raw);
     if(!string.Equals(rev,Keeper.BaseRevision,StringComparison.Ordinal)){code=RawDraftKeeper044.LiveBaseChanged;why="base "+Keeper.BaseRevision+" -> "+rev;}
     // PATCH2 F03: source context. Rescans only when this Database raised events (or INSUNITS changed) since the last proof.
     else{long seen=sourceEvents;int ins=(int)db.Insunits;
      if(seen!=provenEvents||ins!=provenInsunits){why=SourceChanged(p);
       if(why!=null)code=RawDraftKeeper044.LiveSourceChanged;else{provenEvents=seen;provenInsunits=ins;}}}}}
   catch(Exception ex){Keeper.Failed("LIVE_CHECK_FAILED","không kiểm tra lại được bản vẽ / dữ liệu dự án / hình CAD ("+ex.GetType().Name+": "+ex.Message+"); nháp trên máy giữ nguyên");return false;}
   if(code==null){Prove(p);return true;}
   Keeper.Suspend(code,why);return false;}
  void Fail(Exception ex){captureError=ex.Message;if(Keeper!=null)Keeper.Failed("CAPTURE",ex.Message);Status();}

  // After a successful explicit SaveProject: re-read the NOD; the base advances only when it equals the raw just saved AND the bound drawing identity
  // and fingerprint are proven unchanged (PATCH1 F01). A suspended keeper is never rebased (Keeper.Rebase refuses unless READY).
  public void AfterSave(Action<string> readGuard){if(OffReason!=null||Keeper==null)return;
   string why=null;RoomProjectRead037 r=null;
   try{if(readGuard!=null)readGuard("sau khi lưu, trước khi cập nhật nháp khôi phục");
    var id=identityNow==null?null:identityNow();
    if(id==null||!string.Equals(id,boundIdentity,StringComparison.OrdinalIgnoreCase)||!string.Equals(RoomProjectStore037.DrawingId(db),fingerprint,StringComparison.OrdinalIgnoreCase))
     why="bản vẽ đang gắn với bảng đã đổi (đường dẫn, đã lưu/chưa lưu hoặc mã bản vẽ)";
    else r=RoomProjectStore037.Read(db);}
   catch(Exception ex){why=ex.Message;}
   if(why==null&&!string.Equals(r.raw,s.projectRaw,StringComparison.Ordinal))why="dữ liệu dự án đọc lại khác dữ liệu vừa lưu";
   if(why!=null){OffReason="không xác nhận được dữ liệu dự án sau khi lưu ("+why+"); nháp không cập nhật nữa";Keeper.Disable(RawDraftKeeper044.RebaseUnproven,why);End();Status();return;}
   if(Keeper.Rebase(RawDraftSign044.BaseRevision(r.raw)))baseDrafts=Drafts(r.document);
   cache=null;Checkpoint();}

  // End of Run. Text replacing the "đã bỏ" sentence when the current unsaved state (accepted changes and/or raw cells) is on disk; null otherwise.
  // PATCH1 F01: Close claims "kept" only through the guarded Checkpoint. When nothing could be kept and the state is raw cells only (no accepted change,
  // so Run prints no 'đã bỏ' sentence), the loss is stated here instead of being silent.
  // PATCH2 F03: the source-change subscription is always released here (final checkpoint first, then End), also when the checkpoint throws.
  public string Close(){try{return CloseCore();}finally{End();}}
  string CloseCore(){if(OffReason!=null||Keeper==null)return null;closing=true;Detach();form=null;
   if(!Checkpoint()){
    if(!s.dirty&&!s.projectDirty&&Overlay.Count>0)return " "+Overlay.Count.ToString(Inv)+" ô nhập chưa áp dụng đã bỏ."+NotKeptSuffix();
    return null;}
   var p=Capture();if(p.IsClean)return null;
   int n=Overlay.Count;
   return " Thay đổi chưa lưu"+(p.projectDirty?" (khai báo phòng / thiết lập dự án)":"")+(n>0?(p.projectDirty?" và ":" (")+n.ToString(Inv)+" ô nhập chưa áp dụng"+(p.projectDirty?"":")"):"")+
    " CHƯA ghi vào bản vẽ; đã giữ trong NHÁP KHÔI PHỤC trên máy lúc "+When(Keeper.LastSavedUtc)+" — mở lại bảng trên cùng bản vẽ để tiếp tục, rồi 'Lưu vào bản vẽ'.";}
  // Appended to the unchanged 'đã bỏ' sentence only when recovery was active but could not keep the state (disabled recovery = message as before).
  public string NotKeptSuffix(){if(OffReason!=null||Keeper==null)return "";
   if(Keeper.State!=RawDraftKeeper044.Ready)return " "+HeldText();
   if(!Keeper.LastFailed&&captureError==null)return " (Nháp khôi phục không giữ được phần này: lần quét phòng chưa hoàn tất hoặc bản nháp phòng không đọc được.)";
   return " CHƯA LƯU ĐƯỢC NHÁP khôi phục ("+(captureError??(Keeper==null?"":Keeper.Message))+").";}

  // ---- status / export block ----
  static string When(string utc){DateTime t;
   return utc!=null&&DateTime.TryParseExact(utc,"o",Inv,DateTimeStyles.RoundtripKind,out t)?t.ToLocalTime().ToString("HH:mm:ss dd/MM/yyyy",Inv):"?";}
  string HeldText(){var k=Keeper;string at=k.HeldCandidate==null?"":" lưu lúc "+When(k.HeldCandidate.savedUtc);
   if(k.State==RawDraftKeeper044.HeldCorrupt)return "CHƯA LƯU NHÁP MỚI: tệp nháp khôi phục của bản vẽ này bị hỏng hoặc chỉ còn bản dự phòng ("+k.Reason+"); MEPS giữ nguyên làm bằng chứng, không tự sửa/ghi đè/khôi phục. Thay đổi phiên này chưa được giữ khi đóng.";
   if(k.State==RawDraftKeeper044.HeldStale)return "CHƯA LƯU NHÁP MỚI: một phiên khác vừa lưu nháp của bản vẽ này; MEPS không ghi đè. Đóng bảng rồi mở lại.";
   if(k.State==RawDraftKeeper044.HeldConflict&&(k.Reason==RawDraftKeeper044.LiveBaseChanged||k.Reason==RawDraftKeeper044.LiveIdentityChanged||
     k.Reason==RawDraftKeeper044.LiveSourceChanged)){   // PATCH1 F01 / PATCH2 F03
    string kept=k.LastSavedUtc==null?"":" (nháp lưu lúc "+When(k.LastSavedUtc)+" giữ nguyên)";
    return "CHƯA LƯU NHÁP MỚI: "+(k.Reason==RawDraftKeeper044.LiveBaseChanged?"dữ liệu dự án trong bản vẽ đã đổi ngoài bảng này (lệnh khác, U/Undo…)":
     k.Reason==RawDraftKeeper044.LiveSourceChanged?"hình CAD của phòng đã quét (thiết bị, tuyến dây, khối, thuộc tính hoặc lớp) đã đổi sau khi mở bảng":
     "bản vẽ đang gắn với bảng đã đổi (Lưu thành tên khác, lưu lần đầu hoặc bản vẽ khác)")+"; MEPS không ghi đè nháp khôi phục trên máy"+kept+
     " và không tự chuyển nháp. Dữ liệu đang nhập vẫn ở trong bảng nhưng KHÔNG được giữ khi đóng; đóng bảng rồi mở lại.";}
   if(k.State==RawDraftKeeper044.HeldConflict){string why=k.Reason=="BASE_REVISION_MISMATCH"?"dữ liệu dự án trong bản vẽ đã khác lúc lưu nháp (đã Lưu, U/Undo hoặc lệnh khác)":
     k.Reason=="GEOMETRY_MISMATCH"?"hình CAD của phòng trong nháp đã đổi":k.Reason=="IDENTITY_MISMATCH"?"nháp thuộc bản vẽ khác":"không kiểm tra được nháp ("+k.Reason+")";
    return "CÓ NHÁP KHÔI PHỤC"+at+" NHƯNG KHÔNG KHỚP: "+why+". MEPS không tự khôi phục và không ghi đè nháp đó (khôi phục thủ công chưa có ở bản này). Thay đổi phiên này chưa được giữ khi đóng.";}
   return "NHÁP KHÔI PHỤC: TẮT"+(k.Message==null?"":" ("+k.Message+")")+".";}
  void Status(){
   if(OffReason!=null){StatusText="NHÁP KHÔI PHỤC: TẮT — "+OffReason+".";StatusError=true;}
   else if(Keeper==null){StatusText="NHÁP KHÔI PHỤC: chưa sẵn sàng.";StatusError=true;}
   else if(Keeper.State!=RawDraftKeeper044.Ready){StatusText=HeldText();StatusError=true;}
   else if(Keeper.LastFailed){StatusText="CHƯA LƯU ĐƯỢC NHÁP: "+(captureError??Keeper.Message??Keeper.Reason)+". Dữ liệu đang nhập vẫn ở trong bảng; MEPS thử lại ở lần nhập/thao tác sau.";StatusError=true;}
   else if(Keeper.LastWritten!=null&&Keeper.LastSavedUtc!=null){StatusText="ĐÃ LƯU NHÁP KHÔI PHỤC lúc "+When(Keeper.LastSavedUtc)+" (trên máy, CHƯA ghi vào bản vẽ).";StatusError=false;}
   else{StatusText="NHÁP KHÔI PHỤC: sẵn sàng — tự lưu trên máy khi bạn nhập (không ghi vào bản vẽ).";StatusError=false;}
   var f=form;if(f!=null&&!f.IsDisposed){f.RecoveryLabel.Text=StatusText;f.RecoveryLabel.ForeColor=StatusError?BdgklLook034.Error:BdgklLook034.Muted;f.RawFooter();}}
  // Blocking text while raw cells exist (FillRest, Excel export); null = none. Open-room cells whose row/supplement is not in the current scan are
  // counted separately (never reassigned by position).
  public static string BlockText(RoomSession037 s){var o=s==null?null:s.rawOverlay;if(o==null||o.Count==0)return null;
   int here=0,unmatched=0;
   if(s.roomId!=null)foreach(var c in o.Room(s.roomId)){here++;
    bool hit=c.kind==RowKind?s.batch!=null&&s.batch.rows.Any(r=>r.rowKey!=null&&O.Equals(r.rowKey,c.key)):RoomAdjustmentsUi037.Supplement(s,c.key)!=null;if(!hit)unmatched++;}
   int other=o.Count-here;
   return "Còn "+o.Count.ToString(Inv)+" ô nhập nháp CHƯA ÁP DỤNG"+(here-unmatched>0?" ("+(here-unmatched).ToString(Inv)+" ô tô đỏ ở phòng đang mở)":"")+
    (unmatched>0?" ("+unmatched.ToString(Inv)+" ô không khớp dòng hiện tại, giữ riêng)":"")+(other>0?" ("+other.ToString(Inv)+" ô ở phòng khác)":"")+
    ": sửa thành giá trị hợp lệ rồi Enter, hoặc bấm 'BỎ Ô NHÁP'. Tổng chưa chính thức; chưa xuất Excel được.";}
 }
}
#endif
