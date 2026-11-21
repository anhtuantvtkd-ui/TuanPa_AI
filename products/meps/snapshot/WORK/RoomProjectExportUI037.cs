using System;using System.IO;using System.Drawing;using System.Linq;using System.Text;using System.Globalization;using System.Collections.Generic;using System.Windows.Forms;using IOE=System.InvalidOperationException;
#if ROOMCAD037 && BDGKL034
using Autodesk.AutoCAD.Geometry;using Database=Autodesk.AutoCAD.DatabaseServices.Database;
namespace Tvxd.Meps {
 // TVXD-MEPS-ROOMQTY-037 S4 grouped Excel export, CAD side (Vietnamese text: /codepage:65001). Reached only from the existing 'Xuất Excel' button of
 // RoomReport037 in project mode (RoomUi037.Export → Export below); no new command, menu, queue or palette.
 //  - Scope chooser RoomExportScopeDialog037: PHÒNG ĐANG MỞ (the scanned open room) / TẦNG ĐANG CHỌN (one floor, preselected = floor of the open room or the
 //    last selected floor; changeable) / TOÀN DỰ ÁN. Nothing is preselected among the three; Xuất is enabled only for a valid choice.
 //  - ScopeRooms (pure): rooms in tree order (floors by order then stable ID, rooms in list order), stable IDs, every room coded (RoomProject037.Problem).
 //  - Collect: ONE copy of the in-memory project document (the session and the drawing are never changed). Each selected room is rebuilt fresh:
 //      open room (scanned in this session) = its in-memory declarations over a COPY of its scan input, current profile (RoomUi037.Recalc →
 //      RoomProjectUi037.ApplyProfile), scan must still be current (RoomCad037.IsCurrent);
 //      other rooms = their draft in the project document (saved, or kept in this session) → new RoomCad037.Scan of the saved rectangle/units/tolerance →
 //      RoomUi037.Rebuild (declarations by row key + current profile). Unreadable/foreign drafts (other drawing fingerprint, legacy per-device heights or
 //      room height box, no confirmed signature) are refused, never migrated. No draft / empty draft = CHƯA QUÉT / CHƯA KHAI, never counted as 0.
 //    Every room must pass RoomUi037.ExportReason (PASS, no NeedsReview) and RoomProjectUi037.ExportCopy. Any refused room blocks the whole scope with
 //    its code and reason (no partial export). The same whole source handle in two rooms blocks (RoomProjectExport037.SharedSources).
 //  - Export: persisted project raw checked (unchanged since read/save) before collecting; workbook bytes built before the path chooser; after the chooser
 //    and immediately before writing: raw again + RoomCad037.IsCurrent of EVERY exported room; then RoomXlsx037.WriteNew (new file only).
 public sealed class RoomExportScopeRoom037 {public int index;public string floorId,floorName,roomId,code,description;}
 public sealed class RoomExportScopeChoice037 {public string scope,floorId;}
 // One selected room. session = temporary export session (never the open session); data = composed input; problem = why it blocks (null = ready).
 public sealed class RoomExportBuilt037 {public RoomExportScopeRoom037 room;public RoomSession037 session;public RoomExportRoom037 data;public string problem;public bool current;}
 public sealed class RoomExportSet037 {public string scope,floorId,label;public RoomProjectDocument037 document;
  public List<RoomExportBuilt037> rooms=new List<RoomExportBuilt037>();public List<string> problems=new List<string>();}

 public static class RoomProjectExportUi037 {
  static readonly StringComparer O=StringComparer.Ordinal,OI=StringComparer.OrdinalIgnoreCase;
  static readonly CultureInfo Inv=CultureInfo.InvariantCulture;
  public const int MaxListedProblems=30;
  // QA hook (synchronous): shows the scope chooser and returns its result. Collection, checks and writing always run. RoomUi037.ResetHooks resets it.
  public static Func<RoomExportScopeDialog037,IWin32Window,DialogResult> ShowScope=DefaultShowScope;
  public static DialogResult DefaultShowScope(RoomExportScopeDialog037 f,IWin32Window owner){return f.ShowDialog(owner);}
  public static void ResetHooks(){ShowScope=DefaultShowScope;}

  // ---- pure scope helpers ----
  public static List<RoomExportScopeRoom037> TreeRooms(RoomProjectData037 m){var l=new List<RoomExportScopeRoom037>();
   if(m==null)throw new IOE("Chưa có dự án: bấm 'DỰ ÁN / TẦNG / PHÒNG' trước. Chưa xuất.");
   var p=RoomProject037.Problem(m);if(p!=null)throw new IOE("Dữ liệu dự án không hợp lệ: "+p+" Chưa xuất.");
   foreach(var f in m.hierarchy.floors.OrderBy(x=>x.order).ThenBy(x=>x.id,O))foreach(var r in f.rooms)
    l.Add(new RoomExportScopeRoom037{index=l.Count,floorId=f.id,floorName=f.name,roomId=r.id,code=r.code,description=r.name});
   return l;}
  // Rooms of one scope in tree order. currentRoomId = scanned open room (ROOM); floorId = chosen floor (FLOOR). Throws with the reason when empty/invalid.
  public static List<RoomExportScopeRoom037> ScopeRooms(RoomProjectData037 m,string scope,string currentRoomId,string floorId){var all=TreeRooms(m);List<RoomExportScopeRoom037> l;
   if(scope==RoomProjectExport037.ScopeRoom){
    if(string.IsNullOrEmpty(currentRoomId))throw new IOE("Chưa có phòng đang mở (đã quét): bấm 'Chọn vùng CAD', hoặc chọn TẦNG / TOÀN DỰ ÁN.");
    l=all.Where(x=>O.Equals(x.roomId,currentRoomId)).ToList();if(l.Count!=1)throw new IOE("Phòng đang mở không có trong dự án; chọn lại phòng bằng 'Chọn vùng CAD'.");}
   else if(scope==RoomProjectExport037.ScopeFloor){
    if(string.IsNullOrEmpty(floorId))throw new IOE("Chọn một tầng để xuất.");
    if(RoomProjectUi037.FindFloor(m,floorId)==null)throw new IOE("Tầng đã chọn không có trong dự án.");
    l=all.Where(x=>O.Equals(x.floorId,floorId)).ToList();if(l.Count==0)throw new IOE("Tầng đã chọn chưa có phòng nào; không xuất tổng bằng 0.");}
   else if(scope==RoomProjectExport037.ScopeProject){l=all;if(l.Count==0)throw new IOE("Dự án chưa có phòng nào; không xuất tổng bằng 0.");}
   else throw new IOE("Chưa chọn phạm vi xuất (PHÒNG ĐANG MỞ / TẦNG ĐANG CHỌN / TOÀN DỰ ÁN).");
   if(l.Count>RoomProjectExport037.MaxRooms)throw new IOE("Phạm vi có "+l.Count.ToString(Inv)+" phòng, quá giới hạn "+RoomProjectExport037.MaxRooms.ToString(Inv)+" phòng một tệp; chọn từng tầng.");
   return l;}
  public static string ScopeLabel(RoomProjectData037 m,string scope,IList<RoomExportScopeRoom037> rooms){if(rooms==null||rooms.Count==0)return "";
   int floors=rooms.Select(x=>x.floorId).Distinct(O).Count();
   if(scope==RoomProjectExport037.ScopeRoom)return "phòng "+rooms[0].code+" — "+rooms[0].description+" · tầng "+rooms[0].floorName;
   if(scope==RoomProjectExport037.ScopeFloor)return "tầng "+rooms[0].floorName+" ("+rooms.Count.ToString(Inv)+" phòng)";
   return "dự án "+m.hierarchy.name+" ("+floors.ToString(Inv)+" tầng, "+rooms.Count.ToString(Inv)+" phòng)";}
  // Open room = the room scanned in this session (null when none); its floor, else the last selected floor.
  public static string CurrentRoomId(RoomSession037 s){return s!=null&&s.batch!=null&&s.scan!=null?s.roomId:null;}
  public static string CurrentFloorId(RoomSession037 s){if(s==null)return null;HierFloor037 f;
   var id=CurrentRoomId(s);if(id!=null&&RoomProjectUi037.FindRoom(RoomProjectUi037.Model(s),id,out f)!=null)return f.id;return s.selectedFloorId;}

  // ---- CAD: collection ----
  // The persisted project raw must equal the one the session read/saved (same rule as RoomUi037 Save/Export).
  public static void CheckRaw(Database db,RoomSession037 s,string when){RoomProjectRead037 r;
   try{r=RoomProjectStore037.Read(db);}catch(IOE ex){throw new IOE("Dữ liệu dự án trong bản vẽ "+when+" không đọc được: "+ex.Message+" Chưa xuất.");}
   if(!string.Equals(r.raw,s.projectRaw,StringComparison.Ordinal))
    throw new IOE("Dữ liệu dự án trong bản vẽ đã đổi sau khi mở bảng (lệnh khác hoặc U) — phát hiện "+when+"; đóng bảng và mở lại. Chưa xuất.");}
  public static RoomExportSet037 Collect(Database db,RoomSession037 s,RoomExportScopeChoice037 choice,Action<string> readGuard){
   if(db==null)throw new ArgumentNullException("db");if(s==null)throw new ArgumentNullException("s");if(choice==null)throw new ArgumentNullException("choice");
   if(s.projectError!=null||s.projectDocument==null)throw new IOE("Chưa có dữ liệu dự án đọc được; chưa xuất.");
   if(readGuard!=null)readGuard("trước khi xuất Excel");
   CheckRaw(db,s,"trước khi xuất Excel");
   var doc=RoomProjectUi037.CopyDocument(s.projectDocument);
   var list=ScopeRooms(doc.model,choice.scope,CurrentRoomId(s),choice.floorId);
   var set=new RoomExportSet037{scope=choice.scope,floorId=choice.floorId,label=ScopeLabel(doc.model,choice.scope,list),document=doc};
   string fp=db.FingerprintGuid;
   foreach(var it in list){var b=new RoomExportBuilt037{room=it};set.rooms.Add(b);
    try{
     if(readGuard!=null)readGuard("trước khi quét phòng "+it.code);
     RoomSession037 t;
     if(O.Equals(CurrentRoomId(s),it.roomId)){b.current=true;t=FromSession(db,s,doc);}else t=FromDraft(db,doc,it,fp);
     var why=RoomUi037.ExportReason(t);if(why!=null)throw new IOE(why);
     b.session=t;b.data=Data(doc.model,it,t);}
    catch(IOE ex){b.problem=ex.Message;b.data=null;set.problems.Add("Phòng "+it.code+" (tầng "+it.floorName+"): "+ex.Message);}}
   foreach(var c in RoomProjectExport037.SharedSources(set.rooms.Where(x=>x.data!=null).Select(x=>x.data).ToList()))
    set.problems.Add("Cùng một đoạn dây CAD nằm trong vùng của nhiều phòng (sẽ bị tính hai lần; sửa vùng phòng rồi quét lại) – nguồn "+c);
   return set;}
  static RoomInput037 CopyInput(RoomInput037 src){return new RoomInput037{roomId=src.roomId,units=src.units,devices=new List<RoomDevice037>(src.devices),
   segments=new List<RoomSegment037>(src.segments),nodeDevices=new List<RoomNodeDevice037>(src.nodeDevices)};}
  // Open room: in-memory declarations of the session over a copy of its scan input; profile re-resolved; the session itself is untouched.
  static RoomSession037 FromSession(Database db,RoomSession037 s,RoomProjectDocument037 doc){
   if(s.scan.signature==null)throw new IOE("lần quét phòng đang mở chưa hoàn tất (vướng mắc CAD chặn việc đọc phòng).");
   if(s.batch.source==null)throw new IOE("phòng đang mở chưa có dữ liệu quét.");
   if(!RoomCad037.IsCurrent(db,s.scan))throw new IOE("hình trong phòng đang mở đã đổi sau lần quét; bấm 'Cập nhật' rồi xem lại.");
   var t=new RoomSession037{projectMode=true,projectDocument=doc,roomId=s.roomId,selectedRoomId=s.roomId,selectedFloorId=s.selectedFloorId,units=s.units,tolerance=s.tolerance,
    unitConfirmed=s.unitConfirmed,minX=s.minX,minY=s.minY,maxX=s.maxX,maxY=s.maxY,hasRegion=true,scan=s.scan,scannedAt=s.scannedAt,saved=s.saved};
   t.batch=RoomCompletion037.Create(CopyInput(s.batch.source));t.batch.conduitMode=s.batch.conduitMode;t.batch.independentAssembliesConfirmed=s.batch.independentAssembliesConfirmed;
   var by=new Dictionary<string,RoomCompletionRow037>(O);foreach(var r in s.batch.rows)if(r.rowKey!=null&&r.input!=null&&!by.ContainsKey(r.rowKey))by[r.rowKey]=r;
   foreach(var r in t.batch.rows){RoomCompletionRow037 o;if(r.input==null||r.rowKey==null||!by.TryGetValue(r.rowKey,out o))continue;var i=r.input;var q=o.input;
    i.circuitId=q.circuitId;i.routingMode=q.routingMode;i.tailCount=q.tailCount;i.tailEachLength=q.tailEachLength;i.factor=q.factor;i.assemblyCode=q.assemblyCode;i.conduitCode=q.conduitCode;}
   // S5: the session's current BỎ DÒNG / BỔ SUNG (deep copy); Recalc sets the batch signature from t.scan (= the open room's current scan).
   t.adjustments=RoomAdjustments037.Copy(s.adjustments);
   // 045 PATCH1 F01: the CURRENT (also unsaved) row state of the open room — RIÊNG sets, explicit blanks, row presets — deep-copied into the export
   // session and into the export document copy, so Recalc → Resolve never re-binds from older persisted row state: an explicit blank stays missing
   // (refused like the table shows it), inherited fields are re-resolved from the current presets/defaults. Live session and stored NOD untouched.
   if(s.fast!=null&&O.Equals(s.fast.roomId,s.roomId)){t.fast=FastEntry045.Copy(s.fast);RoomFastEntryUi045.PutRoom(doc,s);}
   RoomUi037.Recalc(t);return t;}
  // Other room: its project draft → fresh scan of the saved rectangle → declarations by row key + current profile. Foreign/legacy drafts refused.
  static RoomSession037 FromDraft(Database db,RoomProjectDocument037 doc,RoomExportScopeRoom037 it,string fingerprint){string err;
   var d=RoomProjectUi037.DraftOf(doc,it.roomId,out err);
   if(err!=null)throw new IOE("bản nháp của phòng không đọc được ("+err+"); MEPS không sửa/chuyển đổi. Mở phòng bằng 'Chọn vùng CAD', khai báo lại và Lưu.");
   if(d==null)throw new IOE("CHƯA QUÉT / CHƯA LƯU: phòng chưa có vùng CAD và khai báo trong dự án; mở phòng bằng 'Chọn vùng CAD', khai báo và 'Lưu vào bản vẽ'. Không tính là 0.");
   if(!string.Equals(d.fingerprint,fingerprint,StringComparison.OrdinalIgnoreCase))throw new IOE("bản nháp ghi dấu bản vẽ khác ("+d.fingerprint+"); MEPS không dùng bản nháp của bản vẽ khác.");
   if(d.devices.Count>0||d.heights!=null)throw new IOE("bản nháp còn cao độ kiểu cũ (từng thiết bị / hộp cao độ phòng), không phải bản nháp dự án; MEPS không tự chuyển đổi. Mở phòng, xem lại và Lưu.");
   if(d.rows.Count==0)throw new IOE("CHƯA KHAI: bản nháp chưa có khai báo nào; không tính là 0.");
   if(d.signature==null||d.confirmedSignature==null)throw new IOE("bản nháp chưa gắn với một lần quét hoàn chỉnh; mở phòng, quét và Lưu.");
   var scan=RoomCad037.Scan(db,new Point3d(d.minX,d.minY,0),new Point3d(d.maxX,d.maxY,0),it.roomId,d.units,d.tolerance);
   if(scan.signature==null)throw new IOE("quét lại vùng đã lưu không hoàn tất: "+string.Join("; ",RoomUi037.NativeBlockers(scan,d.unitConfirmed).Take(3)));
   var t=new RoomSession037{projectMode=true,projectDocument=doc,roomId=it.roomId,selectedRoomId=it.roomId,selectedFloorId=it.floorId,units=d.units,tolerance=d.tolerance,
    unitConfirmed=d.unitConfirmed,minX=scan.minX,minY=scan.minY,maxX=scan.maxX,maxY=scan.maxY,hasRegion=true,saved=d};
   // S5: the draft's BỎ DÒNG / BỔ SUNG (deep copy); Rebuild → Recalc compares them with the signature of THIS fresh scan (stale = CHECK, never remapped).
   t.adjustments=RoomAdjustments037.Copy(d.adjustments);
   RoomUi037.Rebuild(t,scan,d.rows,d.conduitMode,d.independentAssembliesConfirmed);
   return t;}
  static RoomHeightRule037 Own(RoomProjectData037 m,string deviceId,out string otherRoom){otherRoom=null;if(m==null||m.devices==null)return null;
   var e=m.devices.FirstOrDefault(x=>x!=null&&OI.Equals(x.deviceId,deviceId));if(e==null)return null;otherRoom=e.roomId;return e.rule;}
  // Pure composition input of one built room (readable export copy, source/device identities, height provenance from RoomProject037.Resolve).
  public static RoomExportRoom037 Data(RoomProjectData037 m,RoomExportScopeRoom037 it,RoomSession037 t){
   var b=RoomProjectUi037.ExportCopy(t);
   HierFloor037 f;var room=RoomProjectUi037.FindRoom(m,it.roomId,out f);if(room==null)throw new IOE("phòng không còn trong dự án.");
   var x=new RoomExportRoom037{roomId=room.id,code=room.code,description=room.name,floorId=f.id,floorName=f.name,storeyHeight=f.storeyHeight,batch=b,
    wireCeiling=room.wireCeilingOverride.HasValue?room.wireCeilingOverride:f.wireCeilingHeight,wireCeilingSource=room.wireCeilingOverride.HasValue?"PHÒNG":f.wireCeilingHeight.HasValue?"TẦNG":null,
    mountingCeiling=room.mountingCeilingOverride.HasValue?room.mountingCeilingOverride:f.mountingCeilingHeight,
    mountingCeilingSource=room.mountingCeilingOverride.HasValue?"PHÒNG":f.mountingCeilingHeight.HasValue?"TẦNG":null};
   var src=new SortedSet<string>(O);var devs=new SortedSet<string>(O);
   // S5: every row (also an excluded one) must still prove its CAD source; only ACTIVE counted rows own source handles / devices for the cross-room overlap.
   foreach(var r in t.batch.rows){
    foreach(var g in r.segments){RoomCadSegment037 sg;
     if(g==null||g.id==null||t.scan==null||!t.scan.segments.TryGetValue(g.id,out sg)||string.IsNullOrWhiteSpace(sg.sourceHandle))
      throw new IOE("không chứng minh được nguồn CAD của đoạn "+(g==null?"?":g.id)+"; quét lại phòng.");
     if(!r.excluded)src.Add(sg.sourceHandle);}
    if(r.excluded)continue;
    if(r.startId!=null)devs.Add(r.startId);if(r.endId!=null)devs.Add(r.endId);}
   x.sourceHandles.AddRange(src);x.deviceHandles.AddRange(devs);
   foreach(var d in RoomProjectUi037.Devices(t)){var e=RoomProject037.Resolve(m,room.id,d.id,d.type);string other;var own=Own(m,d.id,out other);
    var win=e.source==RoomProject037.Rieng?own:e.source==RoomProject037.Phong?RoomProjectUi037.RoomRule(m,room.id,d.type):e.source==RoomProject037.Chung?RoomProjectUi037.DefaultRule(m,d.type):null;
    string msg=e.message;
    if(own!=null&&!O.Equals(other,room.id)){HierFloor037 of;var orm=RoomProjectUi037.FindRoom(m,other,out of);msg="Khai riêng thuộc phòng khác ("+RoomProjectUi037.RoomLabel(orm)+"). "+msg;}
    string id=d.id;
    x.heights.Add(new RoomExportHeight037{deviceId=d.id,deviceType=d.type,deviceName=d.name,status=e.status,source=e.source,mode=e.mode,message=msg,
     effective=e.status==RoomProject037.Valid?e.height:null,declaredValue=win==null?(double?)null:win.value,
     ownMode=own==null?null:own.mode,ownValue=own==null?(double?)null:own.value,rowsUsing=t.batch.rows.Count(r=>!r.excluded&&(O.Equals(r.startId,id)||O.Equals(r.endId,id)))});}
   return x;}
  // Pure request of a fully collected set (throws when the set has problems).
  public static RoomExportRequest037 Request(RoomExportSet037 set,RoomSession037 s,string exportedAt){
   if(set==null)throw new ArgumentNullException("set");
   if(set.problems.Count>0||set.rooms.Any(x=>x.data==null))throw new IOE("Còn phòng chưa sẵn sàng; chưa xuất.");
   var m=set.document.model;
   var r=new RoomExportRequest037{projectName=m.hierarchy.name,scope=set.scope,scopeLabel=set.label,exportedAt=exportedAt,
    defaults=m.defaults.Select(d=>new RoomHeightRule037{deviceType=d.deviceType,mode=d.mode,value=d.value}).ToList(),rooms=set.rooms.Select(x=>x.data).ToList()};
   if(s!=null&&s.projectDirty)r.notes.Add("THIẾT LẬP DỰ ÁN ĐANG MỞ CÓ THAY ĐỔI CHƯA LƯU vào bản vẽ: số liệu xuất theo thiết lập trong phiên (bấm 'Lưu vào bản vẽ' để lưu).");
   if(s!=null&&s.dirty&&set.rooms.Any(x=>x.current))r.notes.Add("Phòng đang mở có khai báo CHƯA LƯU vào bản vẽ: số liệu của phòng này theo khai báo trong phiên.");
   return r;}
  // After the path chooser / immediately before writing: persisted raw and EVERY exported room scan still current.
  public static void Recheck(Database db,RoomSession037 s,RoomExportSet037 set,string when){CheckRaw(db,s,when);
   foreach(var b in set.rooms)if(b.session==null||!RoomCad037.IsCurrent(db,b.session.scan))
    throw new IOE("Hình của phòng "+b.room.code+" đã đổi "+when+"; bấm 'Cập nhật' / quét lại rồi xuất. Chưa xuất.");}
  static string SafeName(string s){var bad=Path.GetInvalidFileNameChars();var b=new StringBuilder();foreach(char c in s??"")b.Append(Array.IndexOf(bad,c)>=0||char.IsWhiteSpace(c)?'_':c);
   var t=b.ToString();return t.Length>40?t.Substring(0,40):t;}

  // ---- CAD: bound export flow (RoomUi037.Export in project mode) ----
  public static string Export(Database db,RoomSession037 s,Action<string> readGuard,IWin32Window owner,float dpi){
   if(s.projectError!=null||s.projectDocument==null)throw new IOE("Chưa có dữ liệu dự án đọc được; chưa xuất.");
   RoomExportScopeChoice037 choice;
   using(var f=new RoomExportScopeDialog037(s.projectDocument.model,CurrentRoomId(s),CurrentFloorId(s),dpi)){
    var r=ShowScope(f,owner);if(r!=DialogResult.OK||f.Result==null)return "Đã huỷ xuất Excel; chưa ghi tệp nào.";choice=f.Result;}
   var set=Collect(db,s,choice,readGuard);
   if(set.problems.Count>0)throw new IOE("Chưa xuất Excel ("+RoomProjectExport037.ScopeName(set.scope)+": "+set.label+") – "+set.problems.Count.ToString(Inv)+" vướng mắc: "+
    string.Join(" | ",set.problems.Take(MaxListedProblems))+(set.problems.Count>MaxListedProblems?" | …":"")+". Không xuất một phần; bản vẽ không đổi.");
   var req=Request(set,s,DateTime.Now.ToString("yyyy-MM-dd HH:mm",Inv));
   byte[] bytes=RoomProjectExport037.Build(req);   // composed before the path chooser: composition problems are reported first
   string tag=set.scope==RoomProjectExport037.ScopeRoom?set.rooms[0].room.code:set.scope==RoomProjectExport037.ScopeFloor?"TANG_"+set.rooms[0].room.floorName:"DU_AN_"+req.projectName;
   var path=RoomUi037.ChooseExportPath(owner,"MEPS_BDGKL_"+SafeName(tag)+"_"+DateTime.Now.ToString("yyyyMMdd_HHmm",Inv)+".xlsx");
   if(string.IsNullOrEmpty(path))return "Đã huỷ xuất Excel; chưa ghi tệp nào.";
   if(File.Exists(path)||Directory.Exists(path))throw new IOE("Tệp \""+path+"\" đã tồn tại; MEPS không ghi đè. Chọn tên khác.");
   if(readGuard!=null)readGuard("ngay trước khi xuất Excel");
   Recheck(db,s,set,"trong lúc chọn tệp");
   RoomXlsx037.WriteNew(path,bytes);
   int floors=set.rooms.Select(x=>x.room.floorId).Distinct(O).Count();
   return "Đã xuất Excel "+RoomProjectExport037.ScopeName(set.scope)+" ("+set.label+"): "+path+" – "+set.rooms.Count.ToString(Inv)+" phòng, "+floors.ToString(Inv)+
    " tầng; sheet TONG_HOP, CAO_DO và 3 sheet mỗi phòng. Mọi phòng đã quét lại từ CAD và kiểm tra lại sau khi chọn tệp"+
    (s.projectDirty||s.dirty?" — có thiết lập/khai báo CHƯA LƯU vào bản vẽ (ghi chú trong TONG_HOP)":"")+". MEPS không tự mở tệp; bản vẽ không đổi.";}
 }

 // ---- Xuất Excel: chọn phạm vi ----
 public sealed class RoomExportScopeDialog037:Form {
  public readonly RadioButton RoomRadio=new RadioButton(),FloorRadio=new RadioButton(),ProjectRadio=new RadioButton();
  public readonly ComboBox FloorBox=new ComboBox();
  public readonly TextBox InfoBox=new TextBox();
  public readonly Button OkBtn=new Button(),CancelBtn=new Button();
  public RoomExportScopeChoice037 Result;
  readonly RoomProjectData037 model;readonly string currentRoomId;readonly float dpi;readonly Font face,bold;bool loading;
  int S(int px){return Math.Max(1,(int)Math.Round(px*dpi));}
  // currentRoomId: scanned open room (null = none; PHÒNG ĐANG MỞ disabled). floorId: floor to preselect in the list (null = none).
  public RoomExportScopeDialog037(RoomProjectData037 model,string currentRoomId,string floorId,float dpi){
   this.model=model;this.currentRoomId=currentRoomId;this.dpi=BdgklLook034.Dpi(dpi);loading=true;
   Text="MEPS — Xuất Excel: chọn phạm vi";Name="room037_export_scope";FormBorderStyle=FormBorderStyle.Sizable;MinimizeBox=false;MaximizeBox=false;ShowInTaskbar=false;
   StartPosition=FormStartPosition.CenterParent;AutoScaleMode=AutoScaleMode.None;face=RoomLook037.Face(this.dpi,false);bold=RoomLook037.Face(this.dpi,true);Font=face;
   BackColor=BdgklLook034.Back;ForeColor=BdgklLook034.Ink;ClientSize=new Size(S(680),S(480));MinimumSize=new Size(S(540),S(400));
   var root=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=2,RowCount=6,Padding=new Padding(S(10)),BackColor=BdgklLook034.Back};
   root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,S(220)));root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
   foreach(int h in new[]{S(52),S(36),S(36),S(36),-100,S(50)})root.RowStyles.Add(h<0?new RowStyle(SizeType.Percent,-h):new RowStyle(SizeType.Absolute,h));
   var title=new Label{Text="PHẠM VI XUẤT – một tệp Excel: TONG_HOP (tổng hợp), CAO_DO (nguồn gốc cao độ) và 3 sheet cho mỗi phòng. Mọi phòng được quét lại từ CAD trước khi xuất.",
    Dock=DockStyle.Fill,Font=bold,ForeColor=BdgklLook034.Teal,TextAlign=ContentAlignment.MiddleLeft};
   root.Controls.Add(title,0,0);root.SetColumnSpan(title,2);
   HierFloor037 cf=null;var cur=currentRoomId==null?null:RoomProjectUi037.FindRoom(model,currentRoomId,out cf);
   Action<RadioButton,string,string,int> radio=(rb,text,name,at)=>{rb.Text=text;rb.Name=name;rb.AccessibleName=text;rb.Dock=DockStyle.Fill;rb.Font=bold;rb.ForeColor=BdgklLook034.Ink;
    rb.Checked=false;rb.CheckedChanged+=(s,e)=>Changed();root.Controls.Add(rb,0,at);};
   radio(RoomRadio,"PHÒNG ĐANG MỞ","room037_scope_room",1);radio(FloorRadio,"TẦNG ĐANG CHỌN","room037_scope_floor",2);radio(ProjectRadio,"TOÀN DỰ ÁN","room037_scope_project",3);
   RoomRadio.Enabled=cur!=null;
   root.Controls.Add(new Label{Text=cur!=null?RoomProjectUi037.RoomLabel(cur)+" · tầng "+cf.name:"(chưa có phòng đang mở – bấm 'Chọn vùng CAD' để mở một phòng)",Dock=DockStyle.Fill,
    AutoEllipsis=true,ForeColor=cur!=null?BdgklLook034.Ink:BdgklLook034.Muted,TextAlign=ContentAlignment.MiddleLeft},1,1);
   FloorBox.Name="room037_scope_floor_list";FloorBox.AccessibleName="Tầng xuất";FloorBox.DropDownStyle=ComboBoxStyle.DropDownList;FloorBox.Dock=DockStyle.Fill;FloorBox.Margin=new Padding(0,S(4),0,S(4));
   int sel=-1;
   if(model!=null&&model.hierarchy!=null&&model.hierarchy.floors!=null)
    foreach(var f in model.hierarchy.floors.Where(x=>x!=null).OrderBy(x=>x.order).ThenBy(x=>x.id,StringComparer.Ordinal)){
     FloorBox.Items.Add(new RouteDialog034.Choice(f.id,f.name+" ("+(f.rooms==null?0:f.rooms.Count).ToString(CultureInfo.InvariantCulture)+" phòng)"));if(f.id==floorId)sel=FloorBox.Items.Count-1;}
   FloorBox.SelectedIndex=sel;FloorBox.SelectedIndexChanged+=(s,e)=>{if(!loading&&FloorBox.SelectedIndex>=0&&!FloorRadio.Checked)FloorRadio.Checked=true;Changed();};
   root.Controls.Add(FloorBox,1,2);
   int rooms=model==null||model.hierarchy==null||model.hierarchy.floors==null?0:model.hierarchy.floors.Where(x=>x!=null&&x.rooms!=null).Sum(x=>x.rooms.Count);
   root.Controls.Add(new Label{Text=model==null?"(chưa có dự án)":model.hierarchy.name+" – "+(model.hierarchy.floors==null?0:model.hierarchy.floors.Count).ToString(CultureInfo.InvariantCulture)+
    " tầng, "+rooms.ToString(CultureInfo.InvariantCulture)+" phòng",Dock=DockStyle.Fill,AutoEllipsis=true,ForeColor=BdgklLook034.Ink,TextAlign=ContentAlignment.MiddleLeft},1,3);
   InfoBox.Name="room037_scope_info";InfoBox.AccessibleName="Danh sách phòng sẽ xuất";InfoBox.Multiline=true;InfoBox.ReadOnly=true;InfoBox.ScrollBars=ScrollBars.Vertical;
   InfoBox.Dock=DockStyle.Fill;InfoBox.BorderStyle=BorderStyle.FixedSingle;InfoBox.BackColor=BdgklLook034.Paper;InfoBox.ForeColor=BdgklLook034.Ink;
   root.Controls.Add(InfoBox,0,4);root.SetColumnSpan(InfoBox,2);
   var buttons=new FlowLayoutPanel{Dock=DockStyle.Fill,FlowDirection=FlowDirection.RightToLeft,WrapContents=false,BackColor=BdgklLook034.Back,Margin=new Padding(0,S(6),0,0)};
   CancelBtn.Text="Hủy";CancelBtn.Name="room037_scope_cancel";CancelBtn.AccessibleName="Hủy";CancelBtn.DialogResult=DialogResult.Cancel;BdgklLook034.Style(CancelBtn,false,face,S(90),S(30));
   OkBtn.Text="Xuất Excel…";OkBtn.Name="room037_scope_ok";OkBtn.AccessibleName="Xuất Excel";BdgklLook034.Style(OkBtn,true,face,S(120),S(30));OkBtn.Click+=(s,e)=>TryAccept();
   buttons.Controls.Add(CancelBtn);buttons.Controls.Add(OkBtn);root.Controls.Add(buttons,0,5);root.SetColumnSpan(buttons,2);
   CancelButton=CancelBtn;Controls.Add(root);loading=false;Changed();}
  public string Scope(){return RoomRadio.Checked?RoomProjectExport037.ScopeRoom:FloorRadio.Checked?RoomProjectExport037.ScopeFloor:ProjectRadio.Checked?RoomProjectExport037.ScopeProject:null;}
  public string FloorId(){var c=FloorBox.SelectedItem as RouteDialog034.Choice;return c==null?null:c.value;}
  // QA helper: select a scope (and for FLOOR the floor by stable ID). true = valid choice (Xuất enabled); nothing is exported here.
  public bool Choose(string scope,string floorId){loading=true;
   try{if(scope==RoomProjectExport037.ScopeFloor){FloorBox.SelectedIndex=-1;for(int i=0;i<FloorBox.Items.Count;i++)if(((RouteDialog034.Choice)FloorBox.Items[i]).value==floorId)FloorBox.SelectedIndex=i;}
    RoomRadio.Checked=scope==RoomProjectExport037.ScopeRoom&&RoomRadio.Enabled;FloorRadio.Checked=scope==RoomProjectExport037.ScopeFloor;ProjectRadio.Checked=scope==RoomProjectExport037.ScopeProject;}
   finally{loading=false;}
   Changed();return OkBtn.Enabled;}
  List<RoomExportScopeRoom037> Rooms(){var sc=Scope();if(sc==null)throw new IOE("Chọn một phạm vi: PHÒNG ĐANG MỞ, TẦNG ĐANG CHỌN hoặc TOÀN DỰ ÁN.");
   return RoomProjectExportUi037.ScopeRooms(model,sc,currentRoomId,sc==RoomProjectExport037.ScopeFloor?FloorId():null);}
  void Changed(){if(loading)return;
   try{var l=Rooms();var sb=new StringBuilder("Sẽ xuất "+RoomProjectExport037.ScopeName(Scope())+": "+RoomProjectExportUi037.ScopeLabel(model,Scope(),l)+"\r\n");
    foreach(var r in l.Take(300))sb.Append("  • tầng ").Append(r.floorName).Append(" · ").Append(r.code).Append(" — ").Append(r.description).Append("\r\n");
    if(l.Count>300)sb.Append("  … (").Append(l.Count.ToString(CultureInfo.InvariantCulture)).Append(" phòng)\r\n");
    sb.Append("Phòng chưa quét / chưa đủ dữ liệu / hình đã đổi sẽ CHẶN toàn bộ lần xuất (không xuất một phần).");
    InfoBox.Text=sb.ToString();OkBtn.Enabled=true;}
   catch(IOE ex){InfoBox.Text=ex.Message;OkBtn.Enabled=false;}}
  public bool TryAccept(){try{var l=Rooms();var sc=Scope();Result=new RoomExportScopeChoice037{scope=sc,floorId=sc==RoomProjectExport037.ScopeFloor?l[0].floorId:null};}
   catch(IOE ex){Result=null;InfoBox.Text=ex.Message;OkBtn.Enabled=false;return false;}
   DialogResult=DialogResult.OK;return true;}
  protected override void Dispose(bool disposing){base.Dispose(disposing);if(disposing){face.Dispose();bold.Dispose();}}
 }
}
#endif
