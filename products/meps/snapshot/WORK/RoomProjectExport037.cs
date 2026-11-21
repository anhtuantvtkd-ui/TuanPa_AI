using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml;
using IOE=System.InvalidOperationException;

namespace Tvxd.Meps {
 // TVXD-MEPS-ROOMQTY-037 S4 GROUPED PROJECT WORKBOOK (pure: System.IO.Compression + System.Xml + System.Core; no CAD, Forms, Office, network or file IO).
 // Vietnamese literals: compile with /codepage:65001 (shared MEPS build). C# 5 / .NET 4.5.
 // Build(request) composes ONE workbook in memory from the single-room packages of RoomXlsx037.BuildPackage (the proven per-room BDGKL / DM.DIEM / THKL
 // sheets, formulas, styles and layout are reused as produced; nothing is recomputed differently):
 //  - Sheets: TONG_HOP (summary), CAO_DO (height provenance snapshot), then per room in tree order "<nn>_<code>_BDGKL", "<nn>_<code>_DM.DIEM",
 //    "<nn>_<code>_THKL". <nn> = 1-based position (2 digits, 3 when more than 99 rooms); <code> = room code with every character other than ASCII
 //    letter/digit/'-' replaced by '_' and cut to MaxCodeChars. Names are <= 31 characters, unique case-insensitively and always quoted in formulas.
 //  - Every formula of a room package (<f>, conditional-formatting <formula>, data-validation <formula1/2>) and every defined name (filter database,
 //    print area/titles) is rewritten by a formula tokenizer: only sheet prefixes OUTSIDE string literals are mapped (BDGKL / 'DM.DIEM' / THKL of the
 //    same package -> that room's new names); an unknown sheet, an external '[..]' reference or a bare '!' refuses the export. Cell text and cached
 //    values are never searched or replaced (no regex over user text). localSheetId is renumbered; relationship targets are rebuilt from the package
 //    relationships (unexpected parts, external or non-worksheet relationships refuse the export). Styles: the packages must carry identical styles.xml;
 //    four cell formats are appended (existing indexes unchanged). tabSelected is removed from room sheets (no grouped-sheet editing on open).
 //  - DM.DIEM of each room: ONLY the legacy diagnostic RoomXlsx037.LegacyHeightNote (found by exact text in column A directly below the exact height
 //    section title) is replaced by ProjectHeightNote. Single-room exports (RoomXlsx037.Write) are unchanged.
 //  - TONG_HOP: project name, explicit scope and room/floor counts; floor > room (code + description) rows in tree order with a floor subtotal after each
 //    floor and one scope total. Material columns = exact (kind, code, unit) declared in at least one exported room; kinds: wire metres after factor,
 //    hidden conduit after factor, exposed conduit after factor (the official THKL columns D / F / G). Room cell = formula on that room's official THKL
 //    cell (numeric only when the room THKL status is PASS, else "CHECK"); room status = PASS only when THKL is PASS AND the room cells of each kind sum
 //    to that room's PASS-only BDGKL helper total (BK / BN / BO) within ReconcileTolerance, so a code missing from the summary columns cannot vanish.
 //    Floor / scope totals are "CHECK" unless every contributing room is PASS; unlike kinds, codes or units are never added. Cached values = model values.
 //  - CAO_DO: snapshot (labelled as such) of project defaults and, per room and actual scanned device, storey / wiring-ceiling / mounting-ceiling
 //    references with their source, effective height, source CHUNG / PHÒNG / RIÊNG, mode, declared value and own (RIÊNG) declaration. Not referenced by
 //    any formula; editing it changes nothing. A VALID effective height must equal the FIXED height every row of that device carries, else refused.
 //  - Problem(request) refuses (Vietnamese reason): missing data, more than MaxRooms rooms, duplicate room IDs/codes, a floor split into two runs,
 //    scope ROOM with more than one room / FLOOR with more than one floor, and the SAME whole physical source handle (case-insensitive) owned by two
 //    exported rooms (whole-source identity: scan-local segment sub-IDs are not comparable across scans; an accepted source lies fully inside its room
 //    rectangle, so a shared handle means overlapping rooms = double count). A device in two rooms is allowed (boundary device; no allocation invented)
 //    and listed as information.
 // S5 adjustments (BO DONG exclusions / BO SUNG manual supplements, RoomAdjustments037). Rooms use RoomXlsx037.BuildMeasuredPackage: the per-room
 //  sheets contain the ACTIVE measured rows only (sharing already allocated by Calculate on the complete source; nothing is recomputed here).
 //  When NO room carries an exclusion or supplement record, the workbook is composed exactly as before (no new sheet, note or formula).
 //  When at least one room does, three sheets are added before CAO_DO and the combined sheet leads (active tab):
 //   TONG_CAD_BO_SUNG (combined): floor > room > material rows (exact kind / code / unit) with CAD | BO SUNG | TONG columns. CAD = formula on the
 //    room's TONG_HOP material cell, numeric only while that room's TONG_HOP status is PASS (so the official THKL / reconciliation guards still
 //    apply: an edited unlisted code gives CHECK, never 0). BO SUNG = SUM of the room's contiguous BO_SUNG metres range, CHECK when any of those
 //    rows is not PASS or its room code / kind / code / unit text no longer matches. TONG = CAD + BO SUNG. Floor and scope rows sum only the room
 //    rows of the same exact kind / code / unit (SUMPRODUCT + EXACT on numeric helper columns N:Q) and are CHECK when a room row is CHECK or the
 //    number of matching room rows differs from the export. Cached values are the core combinedTotals (checked against the package and the
 //    supplements before composing; any difference refuses the export).
 //   TONG_HOP keeps its measured formulas and is labelled CAD (measured only). BO_SUNG: one row per supplement; metres of active rows are the
 //    editable input (blank / text / negative -> CHECK, 0 valid); inactive rows are listed separately and never counted. BO_DONG: audit of every
 //    exclusion record (active and restored) with room, row key, reason, captured / current source signature and measured inclusion.
 //   All user text is written as inline strings (never as a formula).
 public sealed class RoomExportHeight037 {public string deviceId,deviceType,deviceName,status,source,mode,ownMode,message;public double? effective,declaredValue,ownValue;public int rowsUsing;}
 // batch = recalculated PASS export copy of the room (readable labels); sourceHandles = whole native source handles of the room's rows; deviceHandles =
 // devices used by rows; heights = one entry per actual scanned device. Ceiling sources: "PHÒNG" | "TẦNG" | null (not declared).
 public sealed class RoomExportRoom037 {
  public string roomId,code,description,floorId,floorName,wireCeilingSource,mountingCeilingSource;
  public double? storeyHeight,wireCeiling,mountingCeiling;
  public RoomCompletionBatch037 batch;
  public List<string> sourceHandles=new List<string>(),deviceHandles=new List<string>();
  public List<RoomExportHeight037> heights=new List<RoomExportHeight037>();
 }
 // scope: ROOM | FLOOR | PROJECT. rooms in tree order (floors contiguous). notes: extra caller notes shown on TONG_HOP (e.g. unsaved project settings).
 public sealed class RoomExportRequest037 {
  public string projectName,scope,scopeLabel,exportedAt;
  public List<RoomHeightRule037> defaults=new List<RoomHeightRule037>();
  public List<RoomExportRoom037> rooms=new List<RoomExportRoom037>();
  public List<string> notes=new List<string>();
 }

 public static class RoomProjectExport037 {
  public const string SheetSummary="TONG_HOP",SheetHeights="CAO_DO",SuffixMain="_BDGKL",SuffixInput="_DM.DIEM",SuffixTotals="_THKL";
  // S5: sheets added only when at least one room carries adjustments.
  public const string SheetCombined="TONG_CAD_BO_SUNG",SheetManual="BO_SUNG",SheetAudit="BO_DONG";
  // S5: bound of exclusion + supplement records over the whole request.
  public const int MaxAdjustRows=100000;
  public const string ScopeRoom="ROOM",ScopeFloor="FLOOR",ScopeProject="PROJECT";
  public const string LevelRoom="PHÒNG",LevelFloor="CỘNG TẦNG",LevelTotal="TỔNG CỘNG",Pass="PASS",Check="CHECK";
  // MaxRooms = RoomStore037.MaxRooms (existing room limit); MaxHeightRows bounds CAO_DO; MaxNotes bounds caller notes.
  public const int MaxRooms=200,MaxHeightRows=100000,MaxCodeChars=12,MaxNotes=50,MaxSheetName=31;
  public const double ReconcileTolerance=0.000001;
  public const string ProjectHeightNote="Chế độ DỰ ÁN: cao độ thiết bị lấy từ hồ sơ cao độ của dự án lúc xuất (ưu tiên RIÊNG > PHÒNG > CHUNG) và đã ghi vào bảng thiết bị phía trên "+
   "(cột E:G, cách khai FIXED); công thức dùng các ô đó. Nguồn gốc từng thiết bị: sheet CAO_DO (ảnh chụp lúc xuất, chỉ để truy vết; sửa CAO_DO không đổi công thức).";
  const string Ns="http://schemas.openxmlformats.org/spreadsheetml/2006/main",NsR="http://schemas.openxmlformats.org/officeDocument/2006/relationships";
  const string NsPkg="http://schemas.openxmlformats.org/package/2006/relationships";
  // Cell formats of RoomXlsx037.Styles() (indexes 0..14, checked by count) + four appended by this file.
  const int BaseXfs=15,SHead=1,SText=2,SInNum=5,SInt=6,STitle=8,SHText=9,SHNum=10,SHInt=11,SStatus=12,SNote=13,SCode=14;
  const int SNum=BaseXfs,STotNum=BaseXfs+1,STotText=BaseXfs+2,STotCode=BaseXfs+3;
  const int MaxCellText=32767,MaxFormula=8192,PaperA3=8;
  static readonly CultureInfo Inv=CultureInfo.InvariantCulture;
  static readonly StringComparer O=StringComparer.Ordinal,OI=StringComparer.OrdinalIgnoreCase;
  static readonly string[] Kinds={RoomXlsx037.KindWire,RoomXlsx037.KindHidden,RoomXlsx037.KindExposed};

  // ---- pure helpers (public for the independent harness) ----
  public static string KindLabel(string k){return k==RoomXlsx037.KindWire?"DÂY DẪN – M TUYẾN SAU HỆ SỐ":k==RoomXlsx037.KindHidden?"ỐNG ĐI CHÌM – M SAU HỆ SỐ":
   k==RoomXlsx037.KindExposed?"ỐNG ĐI NỔI – M SAU HỆ SỐ":k;}
  public static string ScopeName(string s){return s==ScopeRoom?"PHÒNG ĐANG MỞ":s==ScopeFloor?"TẦNG ĐANG CHỌN":s==ScopeProject?"TOÀN DỰ ÁN":s;}
  public static string SourceLabel(string s){return s==RoomProject037.Chung?"CHUNG":s==RoomProject037.Phong?"PHÒNG":s==RoomProject037.Rieng?"RIÊNG":"—";}
  public static string ModeLabel(string m){return m==RoomProject037.Fixed?"CỐ ĐỊNH TỪ SÀN HOÀN THIỆN":m==RoomProject037.MountingOffset?"HẠ DƯỚI TRẦN LẮP THIẾT BỊ":"—";}
  public static string StatusLabel(string s){return s==RoomProject037.Valid?"HỢP LỆ":s==RoomProject037.Invalid?"KHÔNG HỢP LỆ":"CHƯA KHAI";}
  // "<nn>_<code>" (see header). index 0-based; count = rooms in the workbook.
  public static string SheetPrefix(int index,int count,string code){
   var b=new StringBuilder();foreach(char c in code??""){if(b.Length>=MaxCodeChars)break;b.Append((c>='A'&&c<='Z')||(c>='a'&&c<='z')||(c>='0'&&c<='9')||c=='-'?c:'_');}
   if(b.Length==0)b.Append('P');
   return (index+1).ToString(count>99?"D3":"D2",Inv)+"_"+b;}
  // Handles (case-insensitive) owned by two or more rooms: "<handle>: <code>, <code>". Sources block; devices are information only.
  public static List<string> SharedSources(IList<RoomExportRoom037> rooms){return Shared(rooms,r=>r.sourceHandles);}
  public static List<string> SharedDevices(IList<RoomExportRoom037> rooms){return Shared(rooms,r=>r.deviceHandles);}
  static List<string> Shared(IList<RoomExportRoom037> rooms,Func<RoomExportRoom037,List<string>> pick){var m=new SortedDictionary<string,List<string>>(O);var show=new Dictionary<string,string>(O);
   if(rooms!=null)foreach(var r in rooms){if(r==null)continue;var l=pick(r);if(l==null)continue;
    foreach(var h in l.Where(x=>!string.IsNullOrWhiteSpace(x)).Select(x=>x.ToUpperInvariant()).Distinct(O)){List<string> who;
     if(!m.TryGetValue(h,out who)){m[h]=who=new List<string>();show[h]=l.First(x=>x!=null&&x.ToUpperInvariant()==h);}if(!who.Contains(r.code))who.Add(r.code);}}
   return m.Where(kv=>kv.Value.Count>1).Select(kv=>show[kv.Key]+": "+string.Join(", ",kv.Value)).ToList();}

  static bool Fin(double? v){return !v.HasValue||(!double.IsNaN(v.Value)&&!double.IsInfinity(v.Value));}
  static string TextBad(string s,string what,bool required){
   if(s==null)return required?what+" trống.":null;if(required&&string.IsNullOrWhiteSpace(s))return what+" trống.";
   if(s.Length>MaxCellText)return what+" dài quá "+MaxCellText.ToString(Inv)+" ký tự.";
   for(int i=0;i<s.Length;i++){char c=s[i];if(char.IsHighSurrogate(c)&&i+1<s.Length&&char.IsLowSurrogate(s[i+1])){i++;continue;}
    if(!XmlConvert.IsXmlChar(c))return what+" có ký tự U+"+((int)c).ToString("X4",Inv)+" mà Excel không lưu được.";}
   return null;}
  // null = the request can be composed; otherwise the first reason (Vietnamese). Never mutates the request.
  public static string Problem(RoomExportRequest037 r){
   if(r==null)return "Thiếu yêu cầu xuất.";
   var t=TextBad(r.projectName,"Tên dự án",true)??TextBad(r.scopeLabel,"Phạm vi xuất",true)??TextBad(r.exportedAt,"Thời điểm xuất",true);if(t!=null)return t;
   if(r.scope!=ScopeRoom&&r.scope!=ScopeFloor&&r.scope!=ScopeProject)return "Phạm vi \""+r.scope+"\" không phải ROOM/FLOOR/PROJECT.";
   if(r.defaults==null||r.rooms==null||r.notes==null)return "Yêu cầu xuất thiếu danh sách bắt buộc.";
   foreach(var d in r.defaults){var p=RoomProject037.RuleProblem(d);if(p!=null)return "Mặc định chung: "+p;}
   if(r.notes.Count>MaxNotes)return "Quá "+MaxNotes.ToString(Inv)+" ghi chú.";foreach(var n in r.notes){t=TextBad(n,"Ghi chú",true);if(t!=null)return t;}
   if(r.rooms.Count==0)return "Không có phòng nào trong phạm vi xuất; không xuất tổng bằng 0.";
   if(r.rooms.Count>MaxRooms)return "Phạm vi có "+r.rooms.Count.ToString(Inv)+" phòng, quá giới hạn "+MaxRooms.ToString(Inv)+" phòng một tệp; chọn phạm vi nhỏ hơn (không xuất một phần).";
   var ids=new HashSet<string>(O);var codes=new HashSet<string>(O);var floorsDone=new HashSet<string>(O);string floor=null;int heights=0,adjust=0;
   foreach(var x in r.rooms){
    if(x==null)return "Có phòng rỗng (null) trong yêu cầu.";
    t=TextBad(x.roomId,"Mã nhận dạng phòng",true)??TextBad(x.code,"Mã phòng",true)??TextBad(x.description,"Mô tả phòng",true)??TextBad(x.floorId,"Mã nhận dạng tầng",true)??
     TextBad(x.floorName,"Tên tầng",true)??TextBad(x.wireCeilingSource,"Nguồn trần đi dây",false)??TextBad(x.mountingCeilingSource,"Nguồn trần lắp",false);if(t!=null)return t+" (phòng "+x.code+")";
    if(!ids.Add(x.roomId))return "Phòng "+x.roomId+" có hai lần trong yêu cầu.";
    if(!codes.Add(RoomHierarchy037.NameKey(x.code)))return "Mã phòng \""+x.code+"\" có hai lần; mã phòng phải duy nhất.";
    if(!O.Equals(x.floorId,floor)){if(floor!=null)floorsDone.Add(floor);if(floorsDone.Contains(x.floorId))return "Các phòng của tầng \""+x.floorName+"\" không liền nhau theo thứ tự cây.";floor=x.floorId;}
    if(!Fin(x.storeyHeight)||!Fin(x.wireCeiling)||!Fin(x.mountingCeiling))return "Phòng "+x.code+" có cao độ tham chiếu không hữu hạn.";
    if(x.batch==null||x.batch.rows==null||x.batch.rows.Count==0)return "Phòng "+x.code+" không có dòng khối lượng; không tính là 0.";
    if(x.sourceHandles==null||x.deviceHandles==null||x.heights==null)return "Phòng "+x.code+" thiếu danh sách nguồn/thiết bị/cao độ.";
    if(x.sourceHandles.Any(string.IsNullOrWhiteSpace)||x.deviceHandles.Any(string.IsNullOrWhiteSpace))return "Phòng "+x.code+" có mã nguồn/thiết bị trống.";
    if(x.sourceHandles.Count==0)return "Phòng "+x.code+" không chứng minh được nguồn CAD của các dòng.";
    foreach(var h in x.heights){if(h==null)return "Phòng "+x.code+" có mục cao độ rỗng.";
     t=TextBad(h.deviceId,"Mã thiết bị",true)??TextBad(h.deviceType,"Loại thiết bị",false)??TextBad(h.deviceName,"Tên thiết bị",false)??TextBad(h.status,"Trạng thái cao độ",false)??
      TextBad(h.source,"Nguồn cao độ",false)??TextBad(h.mode,"Cách khai",false)??TextBad(h.ownMode,"Cách khai riêng",false)??TextBad(h.message,"Ghi chú cao độ",false);if(t!=null)return t+" (phòng "+x.code+")";
     if(!Fin(h.effective)||!Fin(h.declaredValue)||!Fin(h.ownValue))return "Phòng "+x.code+" thiết bị "+h.deviceId+" có số không hữu hạn.";}
    heights+=Math.Max(1,x.heights.Count);
    // S5: adjustments are validated (structure + XLSX-storable text) before anything is composed; null = none (unchanged behaviour).
    var adj=x.batch.adjustments;
    if(adj!=null){var ap=RoomAdjustments037.Problem(adj);if(ap!=null)return "Phòng "+x.code+": khai báo BỎ DÒNG / BỔ SUNG không hợp lệ ("+ap+").";
     adjust+=adj.exclusions.Count+adj.supplements.Count;
     t=TextBad(x.batch.sourceSignature,"Chữ ký nguồn hiện tại",false);if(t!=null)return t+" (phòng "+x.code+")";
     foreach(var e in adj.exclusions){t=TextBad(e.rowKey,"Khóa dòng bỏ",true)??TextBad(e.sourceSignature,"Chữ ký nguồn lúc bỏ",true)??TextBad(e.reason,"Lý do bỏ dòng",true);
      if(t!=null)return t+" (phòng "+x.code+")";}
     foreach(var m in adj.supplements){t=TextBad(m.id,"Mã bổ sung",true)??TextBad(m.roomId,"Phòng của mục bổ sung",true)??TextBad(m.kind,"Loại bổ sung",false)??
      TextBad(m.materialCode,"Mã vật tư bổ sung",false)??TextBad(m.unit,"Đơn vị bổ sung",false)??TextBad(m.circuitId,"Lộ bổ sung",false)??
      TextBad(m.description,"Mô tả bổ sung",false)??TextBad(m.reason,"Lý do bổ sung",false);
      if(t!=null)return t+" (phòng "+x.code+")";}}}
   if(heights>MaxHeightRows)return "Bảng CAO_DO vượt "+MaxHeightRows.ToString(Inv)+" dòng; chọn phạm vi nhỏ hơn.";
   if(adjust>MaxAdjustRows)return "Có "+adjust.ToString(Inv)+" bản ghi BỎ DÒNG / BỔ SUNG, quá giới hạn "+MaxAdjustRows.ToString(Inv)+"; chọn phạm vi nhỏ hơn.";
   if(r.scope==ScopeRoom&&r.rooms.Count!=1)return "Phạm vi PHÒNG ĐANG MỞ phải có đúng một phòng.";
   if(r.scope==ScopeFloor&&r.rooms.Select(q=>q.floorId).Distinct(O).Count()!=1)return "Phạm vi TẦNG ĐANG CHỌN phải thuộc đúng một tầng.";
   var shared=SharedSources(r.rooms);
   if(shared.Count>0)return "Cùng một đoạn dây CAD (nguồn nguyên vẹn) nằm trong vùng của hai phòng – sẽ bị tính hai lần; sửa vùng phòng rồi quét lại: "+
    string.Join("; ",shared.Take(10))+(shared.Count>10?"; … ("+shared.Count.ToString(Inv)+" nguồn)":"")+".";
   return null;}

  // ---- formula tokenizer (sheet prefixes only; string literals copied verbatim) ----
  // map: old sheet name (case-insensitive) -> new sheet name. Throws InvalidOperationException for an unknown sheet, external reference or bare '!'.
  public static string RewriteFormula(string formula,IDictionary<string,string> map){if(formula==null)return null;if(map==null)throw new ArgumentNullException("map");
   var b=new StringBuilder(formula.Length+32);int i=0,n=formula.Length;
   while(i<n){char c=formula[i];
    if(c=='"'){int j=i+1;while(true){if(j>=n)throw new IOE("Công thức có chuỗi không đóng: "+formula);if(formula[j]=='"'){if(j+1<n&&formula[j+1]=='"'){j+=2;continue;}break;}j++;}
     b.Append(formula,i,j-i+1);i=j+1;continue;}
    if(c=='\''){int j=i+1;var name=new StringBuilder();
     while(true){if(j>=n)throw new IOE("Công thức có tên sheet không đóng: "+formula);if(formula[j]=='\''){if(j+1<n&&formula[j+1]=='\''){name.Append('\'');j+=2;continue;}break;}name.Append(formula[j]);j++;}
     if(j+1>=n||formula[j+1]!='!')throw new IOE("Công thức có dấu ' không phải tên sheet: "+formula);
     b.Append(SheetRef(map,name.ToString(),formula));i=j+2;continue;}
    if(c=='[')throw new IOE("Công thức có tham chiếu ngoài tệp ([..]); MEPS không gộp liên kết ngoài: "+formula);
    if(c=='!')throw new IOE("Công thức có tham chiếu sheet không rõ tên: "+formula);
    if(char.IsLetter(c)||c=='_'||c=='\\'){int j=i;while(j<n&&(char.IsLetterOrDigit(formula[j])||formula[j]=='_'||formula[j]=='.'||formula[j]=='\\'))j++;
     if(j<n&&formula[j]=='!'){b.Append(SheetRef(map,formula.Substring(i,j-i),formula));i=j+1;}else{b.Append(formula,i,j-i);i=j;}continue;}
    b.Append(c);i++;}
   return b.ToString();}
  static string SheetRef(IDictionary<string,string> map,string name,string formula){string to;
   if(!map.TryGetValue(name,out to)||string.IsNullOrEmpty(to))throw new IOE("Công thức tham chiếu sheet lạ \""+name+"\"; không gộp: "+formula);
   return "'"+to.Replace("'","''")+"'!";}

  // ---- composition ----
  sealed class Def {public string name,hidden,text;public int role;}
  sealed class Pkg {public RoomExportRoom037 room;public RoomXlsxPackage037 p;public string prefix;public string[] names=new string[3];
   public XmlDocument[] sheets=new XmlDocument[3];public readonly List<Def> defs=new List<Def>();public string styles;
   public readonly Dictionary<string,RoomXlsxTotal037> totals=new Dictionary<string,RoomXlsxTotal037>(O);}
  sealed class Mat {public string kind,code,unit;public int col;}
  static string MatKey(string kind,string code,string unit){return kind+"\u001f"+code+"\u001f"+unit;}

  // Composed workbook bytes. Throws InvalidOperationException (Vietnamese reason) when Problem() refuses or a package cannot be composed safely.
  public static byte[] Build(RoomExportRequest037 r){
   var pr=Problem(r);if(pr!=null)throw new IOE("Chưa xuất: "+pr);
   var pk=new List<Pkg>();int count=r.rooms.Count;
   // S5: adjusted = at least one room carries exclusion / supplement records (declarations only, checked before recalculation).
   bool adjusted=r.rooms.Any(x=>RoomXlsx037.HasAdjustments(x.batch));
   var lead=adjusted?new List<string>{SheetCombined,SheetSummary,SheetManual,SheetAudit,SheetHeights}:new List<string>{SheetSummary,SheetHeights};
   var allNames=new HashSet<string>(lead,OI);
   for(int i=0;i<count;i++){var x=r.rooms[i];var g=new Pkg{room=x,prefix=SheetPrefix(i,count,x.code)};
    try{g.p=RoomXlsx037.BuildMeasuredPackage(x.batch,null);}
    catch(Exception ex){if(ex is OutOfMemoryException)throw;throw new IOE("Phòng "+x.code+" chưa xuất được: "+ex.Message);}
    g.names[0]=g.prefix+SuffixMain;g.names[1]=g.prefix+SuffixInput;g.names[2]=g.prefix+SuffixTotals;
    foreach(var nm in g.names){if(nm.Length>MaxSheetName||nm.IndexOfAny(new[]{'\'','[',']',':','*','?','/','\\'})>=0)throw new IOE("Tên sheet \""+nm+"\" không hợp lệ.");
     if(!allNames.Add(nm))throw new IOE("Tên sheet \""+nm+"\" bị trùng.");}
    foreach(var t in g.p.totals){var k=MatKey(t.kind,t.code,t.unit);if(g.totals.ContainsKey(k))throw new IOE("Phòng "+x.code+": tổng THKL trùng mã "+t.code+".");g.totals[k]=t;}
    Read(g);pk.Add(g);}
   var styles=pk[0].styles;foreach(var g in pk)if(!O.Equals(g.styles,styles))throw new IOE("Các phòng có bảng định dạng khác nhau; không gộp.");
   foreach(var g in pk)Consistent(g);
   // material columns: (kind, code, unit) declared in at least one exported room; kind order fixed, codes ordinal.
   var mats=new List<Mat>();int col=8;
   foreach(var k in Kinds){string kind=k;
    foreach(var cu in pk.SelectMany(q=>q.p.totals).Where(q=>q.kind==kind&&q.used).Select(q=>new{q.code,q.unit}).Distinct().OrderBy(q=>q.code,O).ThenBy(q=>q.unit,O))
     mats.Add(new Mat{kind=kind,code=cu.code,unit=cu.unit,col=col++});}
   if(mats.Count==0)throw new IOE("Không có vật tư nào trong các phòng; không xuất tổng bằng 0.");
   // sheet order: lead sheets (TONG_HOP, CAO_DO; S5 adjusted: TONG_CAD_BO_SUNG, TONG_HOP, BO_SUNG, BO_DONG, CAO_DO), then room i -> L+3i, L+1+3i, L+2+3i
   // with L = lead.Count
   for(int i=0;i<pk.Count;i++){var g=pk[i];var map=new Dictionary<string,string>(OI){{RoomXlsx037.SheetMain,g.names[0]},{RoomXlsx037.SheetInput,g.names[1]},{RoomXlsx037.SheetTotals,g.names[2]}};
    foreach(var d in g.sheets)Transform(d,map);
    ReplaceLegacyNote(g.sheets[1],g.room.code);
    foreach(var d in g.defs)d.text=RewriteFormula(d.text,map);}
   var roomRows=new int[pk.Count];
   string summary=Summary(r,pk,mats,adjusted,roomRows),heights=Heights(r,pk);
   // Lead sheets in workbook order (same order as the names in lead).
   var parts=new List<string>();
   if(adjusted){var ranges=new Dictionary<string,int[]>(O);string manual=Manual(r,pk,ranges);
    parts.Add(Combined(r,pk,mats,roomRows,ranges));parts.Add(summary);parts.Add(manual);parts.Add(Audit(r,pk));parts.Add(heights);}
   else{parts.Add(summary);parts.Add(heights);}
   if(parts.Count!=lead.Count)throw new IOE("Chưa xuất: số sheet đầu tệp không khớp.");
   using(var ms=new MemoryStream()){
    using(var z=new ZipArchive(ms,ZipArchiveMode.Create,true)){
     int sheets=lead.Count+3*pk.Count;
     Entry(z,"[Content_Types].xml",Utf(ContentTypes(sheets)));
     Entry(z,"_rels/.rels",Utf("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><Relationships xmlns=\""+NsPkg+"\"><Relationship Id=\"rId1\" Type=\""+NsR+"/officeDocument\" Target=\"xl/workbook.xml\"/></Relationships>"));
     Entry(z,"xl/workbook.xml",Utf(Workbook(lead,pk)));
     var rels=new StringBuilder("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><Relationships xmlns=\""+NsPkg+"\">");
     for(int i=1;i<=sheets;i++)rels.Append("<Relationship Id=\"rId"+I(i)+"\" Type=\""+NsR+"/worksheet\" Target=\"worksheets/sheet"+I(i)+".xml\"/>");
     rels.Append("<Relationship Id=\"rId"+I(sheets+1)+"\" Type=\""+NsR+"/styles\" Target=\"styles.xml\"/></Relationships>");
     Entry(z,"xl/_rels/workbook.xml.rels",Utf(rels.ToString()));
     Entry(z,"xl/styles.xml",Save(ExtendStyles(styles)));
     int n=1;foreach(var p in parts)Entry(z,"xl/worksheets/sheet"+I(n++)+".xml",Utf(p));
     foreach(var g in pk)foreach(var d in g.sheets)Entry(z,"xl/worksheets/sheet"+I(n++)+".xml",Save(d));}
    return ms.ToArray();}}
  // Build + RoomXlsx037.WriteNew (new file only, never overwrites).
  public static void Write(string path,RoomExportRequest037 r){var bytes=Build(r);RoomXlsx037.WriteNew(path,bytes);}

  // ---- package reading ----
  static readonly string[] SheetChildren={"sheetPr","dimension","sheetViews","sheetFormatPr","cols","sheetData","autoFilter","mergeCells","conditionalFormatting",
   "dataValidations","printOptions","pageMargins","pageSetup","headerFooter"};
  static XmlDocument Load(Stream s){var d=new XmlDocument{PreserveWhitespace=true,XmlResolver=null};
   using(var rd=XmlReader.Create(s,new XmlReaderSettings{DtdProcessing=DtdProcessing.Prohibit,XmlResolver=null}))d.Load(rd);return d;}
  static XmlDocument Load(Dictionary<string,ZipArchiveEntry> e,string name){ZipArchiveEntry z;if(!e.TryGetValue(name,out z))throw new IOE("Gói phòng thiếu phần "+name+".");
   using(var s=z.Open())return Load(s);}
  static string Text(Dictionary<string,ZipArchiveEntry> e,string name){ZipArchiveEntry z;if(!e.TryGetValue(name,out z))throw new IOE("Gói phòng thiếu phần "+name+".");
   using(var s=z.Open())using(var rd=new StreamReader(s,new UTF8Encoding(false,true)))return rd.ReadToEnd();}
  static XmlNamespaceManager Nm(XmlDocument d){var m=new XmlNamespaceManager(d.NameTable);m.AddNamespace("m",Ns);m.AddNamespace("r",NsR);m.AddNamespace("p",NsPkg);return m;}
  static void Read(Pkg g){string who="Phòng "+g.room.code+": ";
   using(var z=new ZipArchive(new MemoryStream(g.p.bytes),ZipArchiveMode.Read)){
    var e=new Dictionary<string,ZipArchiveEntry>(O);foreach(var x in z.Entries){if(e.ContainsKey(x.FullName))throw new IOE(who+"gói có phần trùng "+x.FullName+".");e[x.FullName]=x;}
    var wb=Load(e,"xl/workbook.xml");var rl=Load(e,"xl/_rels/workbook.xml.rels");var nw=Nm(wb);var nr=Nm(rl);
    var targets=new Dictionary<string,string>(O);string stylesTarget=null;
    foreach(XmlElement x in rl.SelectNodes("/p:Relationships/p:Relationship",nr)){
     if(x.GetAttribute("TargetMode")=="External")throw new IOE(who+"gói có liên kết ngoài; không gộp.");
     string type=x.GetAttribute("Type"),target=x.GetAttribute("Target").TrimStart('/');if(target.StartsWith("xl/",StringComparison.Ordinal))target=target.Substring(3);
     if(type==NsR+"/worksheet")targets[x.GetAttribute("Id")]="xl/"+target;else if(type==NsR+"/styles")stylesTarget="xl/"+target;
     else throw new IOE(who+"gói có quan hệ lạ "+type+"; không gộp.");}
    if(stylesTarget==null)throw new IOE(who+"gói thiếu bảng định dạng.");
    var expected=new HashSet<string>(O){"[Content_Types].xml","_rels/.rels","xl/workbook.xml","xl/_rels/workbook.xml.rels",stylesTarget};
    var order=new List<string>();
    foreach(XmlElement x in wb.SelectNodes("/m:workbook/m:sheets/m:sheet",nw)){string name=x.GetAttribute("name"),rid=x.GetAttributeNode("id",NsR)==null?null:x.GetAttributeNode("id",NsR).Value,t;
     if(rid==null||!targets.TryGetValue(rid,out t))throw new IOE(who+"sheet "+name+" không có quan hệ hợp lệ.");
     int role=O.Equals(name,RoomXlsx037.SheetMain)?0:O.Equals(name,RoomXlsx037.SheetInput)?1:O.Equals(name,RoomXlsx037.SheetTotals)?2:-1;
     if(role<0||g.sheets[role]!=null)throw new IOE(who+"gói có sheet lạ/trùng \""+name+"\".");
     g.sheets[role]=Load(e,t);expected.Add(t);order.Add(name);
     CheckSheet(g.sheets[role],who+name);}
    if(g.sheets.Any(s=>s==null))throw new IOE(who+"gói thiếu sheet BDGKL/DM.DIEM/THKL.");
    foreach(var k in e.Keys)if(!expected.Contains(k))throw new IOE(who+"gói có phần lạ "+k+"; không gộp.");
    foreach(XmlElement x in wb.SelectNodes("/m:workbook/m:definedNames/m:definedName",nw)){var ls=x.GetAttribute("localSheetId");int li;
     if(string.IsNullOrEmpty(ls)||!int.TryParse(ls,NumberStyles.None,Inv,out li)||li<0||li>=order.Count)throw new IOE(who+"tên vùng "+x.GetAttribute("name")+" không gắn với một sheet; không gộp.");
     string sn=order[li];int role=O.Equals(sn,RoomXlsx037.SheetMain)?0:O.Equals(sn,RoomXlsx037.SheetInput)?1:2;
     g.defs.Add(new Def{name=x.GetAttribute("name"),hidden=x.GetAttribute("hidden"),text=x.InnerText,role=role});}
    if(wb.SelectSingleNode("/m:workbook/m:externalReferences",nw)!=null)throw new IOE(who+"gói có tham chiếu ngoài; không gộp.");
    g.styles=Text(e,stylesTarget);}}
  static void CheckSheet(XmlDocument d,string who){var root=d.DocumentElement;
   if(root==null||root.LocalName!="worksheet"||root.NamespaceURI!=Ns)throw new IOE(who+": không phải worksheet.");
   foreach(XmlNode c in root.ChildNodes){if(c.NodeType!=XmlNodeType.Element)continue;
    if(c.NamespaceURI!=Ns||Array.IndexOf(SheetChildren,c.LocalName)<0)throw new IOE(who+": có phần tử chưa hỗ trợ <"+c.Name+">; không gộp.");}}
  static IEnumerable<XmlElement> Elements(XmlDocument d,string local){return d.GetElementsByTagName(local,Ns).Cast<XmlElement>().ToList();}
  // Formulas + views of one room sheet.
  static void Transform(XmlDocument d,IDictionary<string,string> map){
   foreach(var local in new[]{"f","formula","formula1","formula2"})foreach(var e in Elements(d,local)){var t=e.InnerText;var n=RewriteFormula(t,map);
    if(n.Length>MaxFormula)throw new IOE("Công thức sau khi đổi tên sheet quá dài.");if(!O.Equals(n,t))e.InnerText=n;}
   foreach(var v in Elements(d,"sheetView"))v.RemoveAttribute("tabSelected");}
  static string CellRef(XmlElement t){var isEl=t.ParentNode as XmlElement;var c=isEl==null?null:isEl.ParentNode as XmlElement;
   return c==null||c.LocalName!="c"?null:c.GetAttribute("r");}
  // DM.DIEM: only the legacy note, exactly once, in column A directly below the exact height-section title.
  static void ReplaceLegacyNote(XmlDocument d,string code){string who="Phòng "+code+" DM.DIEM: ";
   var notes=Elements(d,"t").Where(t=>O.Equals(t.InnerText,RoomXlsx037.LegacyHeightNote)).ToList();
   if(notes.Count!=1)throw new IOE(who+"không tìm đúng một ghi chú cao độ kiểu cũ ("+notes.Count.ToString(Inv)+"); không gộp.");
   var t0=notes[0];var at=CellRef(t0);int row;
   if(at==null||!at.StartsWith("A",StringComparison.Ordinal)||!int.TryParse(at.Substring(1),NumberStyles.None,Inv,out row)||row<2)throw new IOE(who+"ghi chú cao độ không ở cột A; không gộp.");
   string above="A"+I(row-1);
   if(!Elements(d,"t").Any(t=>O.Equals(t.InnerText,RoomXlsx037.HeightSectionTitle)&&O.Equals(CellRef(t),above)))throw new IOE(who+"không thấy tiêu đề mục cao độ ngay trên ghi chú; không gộp.");
   t0.InnerText=ProjectHeightNote;
   var rowEl=t0.ParentNode.ParentNode.ParentNode as XmlElement;if(rowEl!=null&&rowEl.LocalName=="row"){rowEl.SetAttribute("ht",Num(48));rowEl.SetAttribute("customHeight","1");}}
  // Every VALID effective height of CAO_DO must equal the FIXED height carried by every row of that device (start/end). Same snapshot, else refused.
  // S5: excluded rows are not in the measured workbook, so their declarations are not required to match.
  static void Consistent(Pkg g){var x=g.room;var dev=new Dictionary<string,List<double?>>(O);
   foreach(var r in x.batch.rows){var i=r.input;if(i==null||r.excluded)continue;
    Action<string,string,double?> add=(id,mode,h)=>{if(id==null)return;List<double?> l;if(!dev.TryGetValue(id,out l))dev[id]=l=new List<double?>();l.Add(mode==Bdgkl034.Fixed?h:null);};
    add(r.startId,i.startHeightMode,i.startFixedHeight);add(r.endId,i.endHeightMode,i.endFixedHeight);}
   foreach(var h in x.heights){List<double?> l;if(!dev.TryGetValue(h.deviceId,out l))continue;
    if(h.status!=RoomProject037.Valid||l.Any(v=>!Nullable.Equals(v,h.effective)))
     throw new IOE("Phòng "+x.code+": cao độ thiết bị "+h.deviceId+" trong CAO_DO khác cao độ dùng để tính; không xuất (tính lại rồi xuất).");}}

  // ---- writing helpers ----
  sealed class Sh {public readonly SortedDictionary<int,SortedDictionary<int,string>> cells=new SortedDictionary<int,SortedDictionary<int,string>>();
   public readonly Dictionary<int,double> heights=new Dictionary<int,double>();public readonly List<string> merges=new List<string>();
   public void Put(int r,int c,string xml){SortedDictionary<int,string> row;if(!cells.TryGetValue(r,out row))cells[r]=row=new SortedDictionary<int,string>();row[c]=xml;}
   public void Height(int r,double h){double old;if(!heights.TryGetValue(r,out old)||h>old)heights[r]=h;}
   public void Merge(int r1,int c1,int r2,int c2,int st){merges.Add(Ref(r1,c1)+":"+Ref(r2,c2));
    for(int r=r1;r<=r2;r++)for(int c=c1;c<=c2;c++){if(r==r1&&c==c1)continue;SortedDictionary<int,string> row;if(cells.TryGetValue(r,out row)&&row.ContainsKey(c))continue;Blank(this,r,c,st);}}}
  static string I(int v){return v.ToString(Inv);}
  static string Col(int c){var s="";while(c>0){int m=(c-1)%26;s=(char)('A'+m)+s;c=(c-1)/26;}return s;}
  static string Ref(int r,int c){return Col(c)+I(r);}
  static string Abs(string cell){int k=0;while(k<cell.Length&&char.IsLetter(cell[k]))k++;return "$"+cell.Substring(0,k)+"$"+cell.Substring(k);}
  static string Num(double v){if(double.IsNaN(v)||double.IsInfinity(v))throw new IOE("Chưa xuất: số không hữu hạn.");return v.ToString("R",Inv);}
  static string X(string s){var b=new StringBuilder(s.Length+8);
   for(int i=0;i<s.Length;i++){char c=s[i];
    if(char.IsHighSurrogate(c)&&i+1<s.Length&&char.IsLowSurrogate(s[i+1])){b.Append(c).Append(s[i+1]);i++;continue;}
    if(!XmlConvert.IsXmlChar(c))throw new IOE("Chưa xuất: văn bản có ký tự U+"+((int)c).ToString("X4",Inv)+" mà XLSX không lưu được.");
    switch(c){case '&':b.Append("&amp;");break;case '<':b.Append("&lt;");break;case '>':b.Append("&gt;");break;case '"':b.Append("&quot;");break;default:b.Append(c);break;}}
   return b.ToString();}
  static void Blank(Sh s,int r,int c,int st){s.Put(r,c,"<c r=\""+Ref(r,c)+"\" s=\""+I(st)+"\"/>");}
  static void Txt(Sh s,int r,int c,string t,int st){if(string.IsNullOrEmpty(t)){Blank(s,r,c,st);return;}
   if(t.Length>MaxCellText)throw new IOE("Chưa xuất: văn bản ô "+Ref(r,c)+" quá dài.");
   s.Put(r,c,"<c r=\""+Ref(r,c)+"\" s=\""+I(st)+"\" t=\"inlineStr\"><is><t xml:space=\"preserve\">"+X(t)+"</t></is></c>");}
  static void Val(Sh s,int r,int c,double? v,int st){if(!v.HasValue){Blank(s,r,c,st);return;}s.Put(r,c,"<c r=\""+Ref(r,c)+"\" s=\""+I(st)+"\"><v>"+Num(v.Value)+"</v></c>");}
  static void Fx(Sh s,int r,int c,string f,int st,double? num,string text){
   if(f.Length>MaxFormula)throw new IOE("Chưa xuất: công thức ô "+Ref(r,c)+" quá dài.");string head="<c r=\""+Ref(r,c)+"\" s=\""+I(st)+"\"";
   if(num.HasValue)s.Put(r,c,head+"><f>"+X(f)+"</f><v>"+Num(num.Value)+"</v></c>");else s.Put(r,c,head+" t=\"str\"><f>"+X(f)+"</f><v>"+X(text??"")+"</v></c>");}
  static double TextHeight(string t,double width){if(string.IsNullOrEmpty(t))return 18;int lines=0;double per=Math.Max(1,width*0.95);
   foreach(var part in t.Split('\n'))lines+=Math.Max(1,(int)Math.Ceiling(part.Length*1.1/per));return Math.Min(409,Math.Max(18,lines*15.0+4));}
  static string Cols(IList<double> w){return Cols(w,0);}
  // hiddenFrom: first 1-based column hidden (0 = none); hidden columns keep their formulas.
  static string Cols(IList<double> w,int hiddenFrom){var b=new StringBuilder("<cols>");
   for(int i=0;i<w.Count;i++)b.Append("<col min=\""+I(i+1)+"\" max=\""+I(i+1)+"\" width=\""+Num(w[i])+"\" customWidth=\"1\""+(hiddenFrom>0&&i+1>=hiddenFrom?" hidden=\"1\"":"")+"/>");
   return b.Append("</cols>").ToString();}
  // Element order of CT_Worksheet: sheetData, autoFilter, mergeCells, conditionalFormatting, pageMargins, pageSetup.
  static string SheetXml(Sh s,bool selected,string pane,string cols,string filter,string tail){var b=new StringBuilder();
   b.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><worksheet xmlns=\""+Ns+"\" xmlns:r=\""+NsR+"\">");
   b.Append("<sheetPr><pageSetUpPr fitToPage=\"1\"/></sheetPr><sheetViews><sheetView workbookViewId=\"0\""+(selected?" tabSelected=\"1\"":"")+">"+(pane??"")+"</sheetView></sheetViews>");
   b.Append("<sheetFormatPr defaultRowHeight=\"15\"/>").Append(cols).Append("<sheetData>");
   foreach(var row in s.cells){double h;b.Append("<row r=\""+I(row.Key)+"\""+(s.heights.TryGetValue(row.Key,out h)?" ht=\""+Num(h)+"\" customHeight=\"1\"":"")+">");
    foreach(var c in row.Value)b.Append(c.Value);b.Append("</row>");}
   b.Append("</sheetData>").Append(filter??"");
   if(s.merges.Count>0){b.Append("<mergeCells count=\""+I(s.merges.Count)+"\">");foreach(var m in s.merges)b.Append("<mergeCell ref=\""+m+"\"/>");b.Append("</mergeCells>");}
   b.Append(tail).Append("</worksheet>");return b.ToString();}
  static string Page(){return "<pageMargins left=\"0.4\" right=\"0.4\" top=\"0.5\" bottom=\"0.5\" header=\"0.3\" footer=\"0.3\"/><pageSetup paperSize=\""+I(PaperA3)+
   "\" orientation=\"landscape\" pageOrder=\"overThenDown\" fitToWidth=\"1\" fitToHeight=\"0\"/>";}
  static string CheckRule(string sqref){return "<conditionalFormatting sqref=\""+sqref+"\"><cfRule type=\"cellIs\" dxfId=\"0\" priority=\"1\" operator=\"equal\"><formula>&quot;"+Check+"&quot;</formula></cfRule></conditionalFormatting>";}
  static void Note(Sh s,int r,int last,string t,int st,double width){Txt(s,r,1,t,st);s.Merge(r,1,r,last,st);s.Height(r,TextHeight(t,width));}

  // ---- TONG_HOP ----
  // adjusted (S5): title / one extra note label this sheet as CAD measured only and it is not the active tab; roomRows[i] = TONG_HOP row of pk[i].
  // Without adjustments the sheet is unchanged.
  static string Summary(RoomExportRequest037 r,List<Pkg> pk,List<Mat> mats,bool adjusted,int[] roomRows){var s=new Sh();
   int last=mats[mats.Count-1].col;var widths=new List<double>{6,12,16,12,32,18,12};foreach(var m in mats)widths.Add(14);double all=widths.Sum();
   int floors=pk.Select(q=>q.room.floorId).Distinct(O).Count();
   Txt(s,1,1,(adjusted?"TỔNG HỢP KHỐI LƯỢNG ĐO CAD (CHƯA GỒM BỔ SUNG THỦ CÔNG) – DỰ ÁN ":"TỔNG HỢP KHỐI LƯỢNG DÂY DẪN VÀ ỐNG LUỒN – DỰ ÁN ")+r.projectName,STitle);
   s.Merge(1,1,1,last,STitle);s.Height(1,30);
   var notes=new List<string>{
    "PHẠM VI XUẤT: "+ScopeName(r.scope)+" – "+r.scopeLabel+" · "+I(pk.Count)+" PHÒNG · "+I(floors)+" TẦNG · XUẤT LÚC "+r.exportedAt+
     (r.scope==ScopeProject?"":" (KHÔNG phải toàn dự án)"),
    "Số của từng phòng là CÔNG THỨC lấy ô CHÍNH THỨC (chỉ các dòng PASS) ở sheet <phòng>_THKL; phòng chưa PASS hiện CHECK và tổng tầng / tổng cộng hiện CHECK. "+
     "Trạng thái phòng PASS chỉ khi THKL của phòng PASS và tổng các cột cùng loại khớp tổng PASS ở sheet <phòng>_BDGKL. Không cộng khác loại, khác mã hoặc khác đơn vị.",
    "Mỗi phòng có 3 sheet <số>_<mã phòng>_BDGKL / _DM.DIEM / _THKL (cột BỘ SHEET); trong ghi chú của các sheet đó, tên BDGKL / DM.DIEM / THKL chỉ các sheet cùng phòng. "+
     "Sheet CAO_DO là ảnh chụp nguồn gốc cao độ lúc xuất (chỉ để truy vết). PASS = đủ dữ liệu, KHÔNG phải phê duyệt kỹ thuật."};
   if(adjusted)notes.Insert(1,"SHEET NÀY CHỈ CÓ KHỐI LƯỢNG ĐO TRÊN CAD (dòng đang tính; dòng nguồn đã BỎ không có ở đây). CAD + BỔ SUNG + TỔNG: sheet "+SheetCombined+
    "; mục bổ sung thủ công: sheet "+SheetManual+"; dòng đã bỏ / khôi phục: sheet "+SheetAudit+".");
   notes.AddRange(r.notes);
   var sharedDev=SharedDevices(r.rooms);
   if(sharedDev.Count>0)notes.Add("Thiết bị nằm trong vùng của nhiều phòng (không phân bổ; mỗi phòng chỉ tính các tuyến của mình): "+string.Join("; ",sharedDev.Take(20))+(sharedDev.Count>20?"; …":"")+".");
   int row=2;foreach(var n in notes)Note(s,row++,last,n,SNote,all);
   int h1=row+1,h2=h1+1,h3=h1+2;
   string[] head={"STT","CẤP","TẦNG","MÃ PHÒNG","MÔ TẢ PHÒNG","BỘ SHEET PHÒNG","TRẠNG THÁI"};
   for(int c=1;c<=head.Length;c++){Txt(s,h1,c,head[c-1],SHead);s.Merge(h1,c,h3,c,SHead);}
   foreach(var grp in mats.GroupBy(q=>q.kind)){var l=grp.ToList();Txt(s,h1,l[0].col,KindLabel(grp.Key),SHead);s.Merge(h1,l[0].col,h1,l[l.Count-1].col,SHead);
    foreach(var m in l){Txt(s,h2,m.col,m.code,SHead);Txt(s,h3,m.col,m.unit,SHead);}}
   s.Height(h1,48);s.Height(h2,30);s.Height(h3,20);
   int first=h3+1;row=first;int stt=0;var totals=new double[last+1];
   foreach(var fg in pk.GroupBy(q=>q.room.floorId)){var fl=fg.ToList();int a=row;var fsum=new double[last+1];
    foreach(var g in fl){var x=g.room;string T="'"+g.names[2]+"'!",M="'"+g.names[0]+"'!";stt++;roomRows[pk.IndexOf(g)]=row;
     Val(s,row,1,stt,SCode);Txt(s,row,2,LevelRoom,SCode);Txt(s,row,3,x.floorName,SText);Txt(s,row,4,x.code,SCode);Txt(s,row,5,x.description,SText);Txt(s,row,6,g.prefix,SCode);
     foreach(var m in mats){RoomCell(s,row,m,g,T);RoomXlsxTotal037 t;double v=g.totals.TryGetValue(MatKey(m.kind,m.code,m.unit),out t)?t.value:0;fsum[m.col]+=v;}
     var parts=new List<string>{T+"$B$4=\""+Pass+"\""};
     foreach(var k in Kinds){var l=mats.Where(m=>m.kind==k).ToList();string helper=k==RoomXlsx037.KindWire?"BK":k==RoomXlsx037.KindHidden?"BN":"BO";
      string sum=l.Count==0?"0":"SUM("+Ref(row,l[0].col)+":"+Ref(row,l[l.Count-1].col)+")";
      parts.Add("ABS("+sum+"-SUM("+M+"$"+helper+"$12:$"+helper+"$"+I(g.p.lastRow)+"))<="+Num(ReconcileTolerance));}
     Fx(s,row,7,"IFERROR(IF(AND("+string.Join(",",parts)+"),\""+Pass+"\",\""+Check+"\"),\""+Check+"\")",SStatus,null,Pass);
     s.Height(row,Math.Max(TextHeight(x.description,32),TextHeight(x.floorName,16)));row++;}
    int b=row-1;string st="$G$"+I(a)+":$G$"+I(b);
    Blank(s,row,1,STotCode);Txt(s,row,2,LevelFloor,STotCode);Txt(s,row,3,fl[0].room.floorName,STotText);Blank(s,row,4,STotCode);
    Txt(s,row,5,"Cộng "+I(fl.Count)+" phòng của tầng "+fl[0].room.floorName,STotText);Blank(s,row,6,STotCode);
    Fx(s,row,7,"IF(COUNTIF("+st+",\""+Pass+"\")="+I(fl.Count)+",\""+Pass+"\",\""+Check+"\")",STotCode,null,Pass);
    foreach(var m in mats){string c=Col(m.col);
     Fx(s,row,m.col,"IF(COUNTIF("+st+",\""+Pass+"\")<>"+I(fl.Count)+",\""+Check+"\",SUM("+c+I(a)+":"+c+I(b)+"))",STotNum,fsum[m.col],null);totals[m.col]+=fsum[m.col];}
    s.Height(row,Math.Max(22,TextHeight("Cộng "+I(fl.Count)+" phòng của tầng "+fl[0].room.floorName,32)));row++;}
   int lastData=row-1;string lv="$B$"+I(first)+":$B$"+I(lastData),sv="$G$"+I(first)+":$G$"+I(lastData);
   string passRooms="SUMPRODUCT(--("+lv+"=\""+LevelRoom+"\"),--("+sv+"=\""+Pass+"\"))";
   Blank(s,row,1,STotCode);Txt(s,row,2,LevelTotal,STotCode);Blank(s,row,3,STotText);Blank(s,row,4,STotCode);
   string what=r.scope==ScopeProject?"Toàn dự án: "+I(pk.Count)+" phòng, "+I(floors)+" tầng":"Phạm vi "+ScopeName(r.scope)+": "+I(pk.Count)+" phòng (không phải toàn dự án)";
   Txt(s,row,5,what,STotText);Blank(s,row,6,STotCode);
   Fx(s,row,7,"IF("+passRooms+"="+I(pk.Count)+",\""+Pass+"\",\""+Check+"\")",STotCode,null,Pass);
   foreach(var m in mats){string c=Col(m.col);
    Fx(s,row,m.col,"IF("+passRooms+"<>"+I(pk.Count)+",\""+Check+"\",SUMIF("+lv+",\""+LevelRoom+"\","+c+"$"+I(first)+":"+c+"$"+I(lastData)+"))",STotNum,totals[m.col],null);}
   s.Height(row,Math.Max(26,TextHeight(what,32)));
   string pane="<pane xSplit=\"7\" ySplit=\""+I(h3)+"\" topLeftCell=\""+Ref(first,8)+"\" activePane=\"bottomRight\" state=\"frozen\"/><selection pane=\"bottomRight\" activeCell=\""+Ref(first,8)+"\" sqref=\""+Ref(first,8)+"\"/>";
   return SheetXml(s,!adjusted,pane,Cols(widths),null,CheckRule(Ref(first,7)+":"+Ref(row,last))+Page());}
  // One room material cell: formula on the room's official THKL cell; a code not listed in that room's THKL is 0 only while the room THKL is PASS.
  static void RoomCell(Sh s,int row,Mat m,Pkg g,string T){RoomXlsxTotal037 t;
   if(g.totals.TryGetValue(MatKey(m.kind,m.code,m.unit),out t)){string a=T+Abs(t.cell);Fx(s,row,m.col,"IF(ISNUMBER("+a+"),"+a+",\""+Check+"\")",SNum,t.value,null);}
   else Fx(s,row,m.col,"IF("+T+"$B$4=\""+Pass+"\",0,\""+Check+"\")",SNum,0,null);}

  // ---- S5 adjusted sheets (only when at least one room carries adjustments) ----
  sealed class Sup {public int room;public RoomSupplement037 s;public RoomSupplementState037 st;}
  sealed class Line {public string kind,code,unit;public double cad,manual,total;public int count;}
  static int KindIndex(string k){int i=Array.IndexOf(Kinds,k);return i<0?Kinds.Length:i;}
  static bool Near(double a,double b){return Math.Abs(a-b)<=ReconcileTolerance*Math.Max(1.0,Math.Max(Math.Abs(a),Math.Abs(b)));}
  static string SupKey(int room,string kind,string code,string unit){return room.ToString(Inv)+"\u001f"+MatKey(kind,code,unit);}
  static string Box(string col,int a,int b){return "$"+col+"$"+I(a)+":$"+col+"$"+I(b);}
  static string Frozen(int h,int col){return "<pane ySplit=\""+I(h)+"\" topLeftCell=\""+Ref(h+1,1)+"\" activePane=\"bottomLeft\" state=\"frozen\"/><selection pane=\"bottomLeft\" activeCell=\""+
   Ref(h+1,col)+"\" sqref=\""+Ref(h+1,col)+"\"/>";}
  static string ScopeLine(RoomExportRequest037 r,int rooms,int floors){
   return "PHẠM VI XUẤT: "+ScopeName(r.scope)+" – "+r.scopeLabel+" · "+I(rooms)+" PHÒNG · "+I(floors)+" TẦNG · XUẤT LÚC "+r.exportedAt+(r.scope==ScopeProject?"":" (KHÔNG phải toàn dự án)");}

  // Core combined totals of one room (exact kind / code / unit, kind order WIRE, CONDUIT_HIDDEN, CONDUIT_EXPOSED), verified against the measured
  // package (CAD) and the active supplements (BO SUNG). Any difference refuses the export (no numeric guess).
  static List<RoomCombinedTotal037> RoomLines(Pkg g){var x=g.room;var b=x.batch;string who="Phòng "+x.code+": ";
   if(b.combinedTotals==null||b.combinedTotals.Count==0)throw new IOE(who+"không có tổng CAD + BỔ SUNG; không xuất.");
   var seen=new HashSet<string>(O);
   foreach(var c in b.combinedTotals){
    if(c==null||KindIndex(c.kind)>=Kinds.Length||!O.Equals(c.unit,RoomXlsx037.UnitMetre)||string.IsNullOrEmpty(c.code))throw new IOE(who+"tổng CAD + BỔ SUNG có vật tư không hợp lệ; không xuất.");
    var k=MatKey(c.kind,c.code,c.unit);if(!seen.Add(k))throw new IOE(who+"tổng CAD + BỔ SUNG trùng vật tư "+c.kind+" "+c.code+"; không xuất.");
    RoomXlsxTotal037 t;double cad=g.totals.TryGetValue(k,out t)?t.value:0;double man=0;int items=0;
    if(b.adjustments!=null)foreach(var q in b.adjustments.supplements){
     if(!q.active||!O.Equals(q.kind,c.kind)||!O.Equals(q.materialCode,c.code)||!O.Equals(q.unit,c.unit))continue;
     if(!q.metres.HasValue)throw new IOE(who+"mục bổ sung "+q.id+" chưa có số mét; không xuất.");
     man+=q.metres.Value;items++;}
    if(!Near(cad,c.cadM)||!Near(man,c.manualM)||items!=c.manualItems||!Near(c.cadM+c.manualM,c.combinedM))
     throw new IOE(who+"tổng "+c.kind+" "+c.code+" của lõi tính khác gói phòng / mục bổ sung; không xuất (tính lại rồi xuất).");}
   foreach(var t in g.totals.Values)if(t.used&&!seen.Contains(MatKey(t.kind,t.code,t.unit)))
    throw new IOE(who+"vật tư đo "+t.kind+" "+t.code+" thiếu trong tổng CAD + BỔ SUNG; không xuất.");
   return b.combinedTotals.OrderBy(c=>KindIndex(c.kind)).ThenBy(c=>c.code,O).ThenBy(c=>c.unit,O).ToList();}

  // TONG_CAD_BO_SUNG: A STT, B CẤP, C TẦNG, D MÃ PHÒNG, E MÔ TẢ, F LOẠI (exact kind), G MÃ VẬT TƯ, H ĐƠN VỊ, I CAD, J BỔ SUNG, K TỔNG, L TRẠNG THÁI,
  // M NGUỒN; hidden helpers N CAD number, O CAD error (1/0), P BỔ SUNG number, Q BỔ SUNG error (room rows only).
  static readonly string[] CombinedHead={"STT","CẤP","TẦNG","MÃ PHÒNG","MÔ TẢ PHÒNG","LOẠI (WIRE = dây dẫn; CONDUIT_HIDDEN = ống chìm; CONDUIT_EXPOSED = ống nổi)",
   "MÃ VẬT TƯ","ĐƠN VỊ","CAD (m) – ĐO TRÊN BẢN VẼ, SAU HỆ SỐ","BỔ SUNG (m) – NHẬP THỦ CÔNG","TỔNG (m) = CAD + BỔ SUNG","TRẠNG THÁI","NGUỒN SỐ LIỆU",
   "Kiểm tra: CAD số","Kiểm tra: CAD lỗi","Kiểm tra: BỔ SUNG số","Kiểm tra: BỔ SUNG lỗi"};
  static string Combined(RoomExportRequest037 r,List<Pkg> pk,List<Mat> mats,int[] roomRows,Dictionary<string,int[]> ranges){var s=new Sh();
   double[] w={6,12,14,12,30,22,14,8,16,16,16,12,46,10,8,10,8};const int last=13;double all=w.Take(last).Sum();
   int floors=pk.Select(q=>q.room.floorId).Distinct(O).Count();
   Txt(s,1,1,"TỔNG HỢP KHỐI LƯỢNG CAD + BỔ SUNG – DỰ ÁN "+r.projectName,STitle);s.Merge(1,1,1,last,STitle);s.Height(1,30);
   var notes=new List<string>{ScopeLine(r,pk.Count,floors),
    "CAD = khối lượng đo trên bản vẽ sau hệ số: công thức lấy ô của phòng ở sheet "+SheetSummary+" (ô CHÍNH THỨC của <phòng>_THKL, chỉ dòng PASS, không gồm dòng đã BỎ). "+
     "Phòng chưa PASS ở "+SheetSummary+" (kể cả mã vật tư bị sửa ngoài danh sách) → CHECK, không bao giờ hiểu là 0.",
    "BỔ SUNG = tổng SỐ MÉT ở sheet "+SheetManual+" của đúng phòng / loại / mã / đơn vị (sửa ô xanh ở đó; không áp hệ số, không cộng đầu chờ, không phải tuyến CAD). "+
     "TỔNG = CAD + BỔ SUNG. CỘNG TẦNG / TỔNG CỘNG chỉ cộng đúng cùng loại, mã và đơn vị. Ô CHECK = không có số chính thức.",
    "Dòng nguồn đã BỎ / KHÔI PHỤC và lý do: sheet "+SheetAudit+". PASS = đủ dữ liệu, KHÔNG phải phê duyệt kỹ thuật."};
   int row=2;foreach(var n in notes)Note(s,row++,last,n,SNote,all);
   int h=row+1;for(int c=1;c<=CombinedHead.Length;c++)Txt(s,h,c,CombinedHead[c-1],SHead);s.Height(h,62);
   int first=h+1;row=first;int stt=0;var grand=new Dictionary<string,Line>(O);
   foreach(var fg in Enumerable.Range(0,pk.Count).GroupBy(j=>pk[j].room.floorId)){int a=row;var fl=new Dictionary<string,Line>(O);string floorName=pk[fg.First()].room.floorName;
    foreach(int i in fg){var g=pk[i];var x=g.room;string st="'"+SheetSummary+"'!$G$"+I(roomRows[i]);
     foreach(var c in RoomLines(g)){stt++;string R=I(row);var k=MatKey(c.kind,c.code,c.unit);
      Val(s,row,1,stt,SCode);Txt(s,row,2,LevelRoom,SCode);Txt(s,row,3,x.floorName,SText);Txt(s,row,4,x.code,SCode);Txt(s,row,5,x.description,SText);
      Txt(s,row,6,c.kind,SCode);Txt(s,row,7,c.code,SCode);Txt(s,row,8,c.unit,SCode);
      var m=mats.FirstOrDefault(q=>q.kind==c.kind&&q.code==c.code&&q.unit==c.unit);string cadSrc;
      if(m!=null){string cell="'"+SheetSummary+"'!$"+Col(m.col)+"$"+I(roomRows[i]);
       Fx(s,row,9,"IFERROR(IF(AND("+st+"=\""+Pass+"\",ISNUMBER("+cell+")),"+cell+",\""+Check+"\"),\""+Check+"\")",SNum,c.cadM,null);
       cadSrc=SheetSummary+"!"+Col(m.col)+I(roomRows[i])+" (ô CHÍNH THỨC của "+g.names[2]+")";}
      else{
       // No exported room measures this material: CAD is 0 only while the room is PASS in TONG_HOP.
       if(c.cadM!=0)throw new IOE("Phòng "+x.code+": vật tư đo "+c.kind+" "+c.code+" không có cột ở "+SheetSummary+"; không xuất.");
       Fx(s,row,9,"IFERROR(IF("+st+"=\""+Pass+"\",0,\""+Check+"\"),\""+Check+"\")",SNum,0,null);
       cadSrc="phòng không đo mã này (0 khi phòng PASS ở "+SheetSummary+")";}
      int[] rg;string manSrc;
      if(ranges.TryGetValue(SupKey(i,c.kind,c.code,c.unit),out rg)){string B="'"+SheetManual+"'!";
       if(rg[1]-rg[0]+1!=c.manualItems)throw new IOE("Phòng "+x.code+": số mục bổ sung "+c.kind+" "+c.code+" không khớp; không xuất.");
       // bad = rows not PASS + rows whose room code / kind / code / unit text differs from this combined row (each term = count - exact matches).
       string bad="SUMPRODUCT(--("+B+Box("P",rg[0],rg[1])+"<>\""+Pass+"\"))";
       foreach(var col in new[]{"D","F","G","H"})bad+="+"+I(c.manualItems)+"-SUMPRODUCT(--EXACT("+B+Box(col,rg[0],rg[1])+",$"+col+R+"))";
       Fx(s,row,10,"IFERROR(IF("+bad+">0,\""+Check+"\",SUM("+B+Box("I",rg[0],rg[1])+")),\""+Check+"\")",SNum,c.manualM,null);
       manSrc=I(c.manualItems)+" mục ở "+SheetManual+" dòng "+I(rg[0])+"–"+I(rg[1]);}
      else{if(c.manualItems!=0)throw new IOE("Phòng "+x.code+": không tìm thấy dòng bổ sung "+c.kind+" "+c.code+"; không xuất.");
       Val(s,row,10,0.0,SNum);manSrc="không có mục bổ sung";}
      Fx(s,row,11,"IFERROR(IF(AND(ISNUMBER($I"+R+"),ISNUMBER($J"+R+")),$I"+R+"+$J"+R+",\""+Check+"\"),\""+Check+"\")",SNum,c.combinedM,null);
      Fx(s,row,12,"IF(ISNUMBER($K"+R+"),\""+Pass+"\",\""+Check+"\")",SStatus,null,Pass);
      Txt(s,row,13,"CAD: "+cadSrc+" · BỔ SUNG: "+manSrc,SText);
      Fx(s,row,14,"IF(ISNUMBER($I"+R+"),$I"+R+",0)",SHNum,c.cadM,null);Fx(s,row,15,"IF(ISNUMBER($I"+R+"),0,1)",SHInt,0,null);
      Fx(s,row,16,"IF(ISNUMBER($J"+R+"),$J"+R+",0)",SHNum,c.manualM,null);Fx(s,row,17,"IF(ISNUMBER($J"+R+"),0,1)",SHInt,0,null);
      s.Height(row,Math.Max(Math.Max(TextHeight(x.description,30),TextHeight("CAD: "+cadSrc+" · BỔ SUNG: "+manSrc,46)),TextHeight(x.floorName,14)));
      foreach(var d in new[]{fl,grand}){Line l;if(!d.TryGetValue(k,out l))d[k]=l=new Line{kind=c.kind,code=c.code,unit=c.unit};
       l.cad+=c.cadM;l.manual+=c.manualM;l.total+=c.combinedM;l.count++;}
      row++;}}
    int b=row-1;
    foreach(var l in Ordered(fl.Values)){AggRow(s,row,LevelFloor,floorName,"Cộng tầng "+floorName,l,a,b);row++;}}
   int lastData=row-1;
   string what=r.scope==ScopeProject?"Toàn dự án: "+I(pk.Count)+" phòng, "+I(floors)+" tầng":"Phạm vi "+ScopeName(r.scope)+": "+I(pk.Count)+" phòng (không phải toàn dự án)";
   foreach(var l in Ordered(grand.Values)){AggRow(s,row,LevelTotal,"",what,l,first,lastData);row++;}
   return SheetXml(s,true,Frozen(h,9),Cols(w,14),null,CheckRule(Ref(first,9)+":"+Ref(row-1,12))+Page());}
  static IEnumerable<Line> Ordered(IEnumerable<Line> l){return l.OrderBy(q=>KindIndex(q.kind)).ThenBy(q=>q.code,O).ThenBy(q=>q.unit,O);}
  // Floor / scope row of one exact material: sums the PHÒNG rows a..b of the same kind / code / unit; CHECK when a matching room row is CHECK or
  // when the number of matching room rows differs from the export (a renamed / edited room row cannot vanish silently).
  static void AggRow(Sh s,int row,string level,string floor,string what,Line l,int a,int b){string R=I(row);
   Blank(s,row,1,STotCode);Txt(s,row,2,level,STotCode);Txt(s,row,3,floor,STotText);Blank(s,row,4,STotCode);Txt(s,row,5,what,STotText);
   Txt(s,row,6,l.kind,STotCode);Txt(s,row,7,l.code,STotCode);Txt(s,row,8,l.unit,STotCode);
   string mask="--("+Box("B",a,b)+"=\""+LevelRoom+"\"),--EXACT("+Box("F",a,b)+",$F"+R+"),--EXACT("+Box("G",a,b)+",$G"+R+"),--EXACT("+Box("H",a,b)+",$H"+R+")";
   Func<string,string,string> agg=(num,err)=>"IFERROR(IF(OR(SUMPRODUCT("+mask+")<>"+I(l.count)+",SUMPRODUCT("+mask+","+Box(err,a,b)+")>0),\""+Check+"\",SUMPRODUCT("+mask+","+Box(num,a,b)+")),\""+Check+"\")";
   Fx(s,row,9,agg("N","O"),STotNum,l.cad,null);Fx(s,row,10,agg("P","Q"),STotNum,l.manual,null);
   Fx(s,row,11,"IFERROR(IF(AND(ISNUMBER($I"+R+"),ISNUMBER($J"+R+")),$I"+R+"+$J"+R+",\""+Check+"\"),\""+Check+"\")",STotNum,l.total,null);
   Fx(s,row,12,"IF(ISNUMBER($K"+R+"),\""+Pass+"\",\""+Check+"\")",STotCode,null,Pass);
   Txt(s,row,13,"Cộng "+I(l.count)+" dòng phòng cùng loại / mã / đơn vị",STotText);
   s.Height(row,Math.Max(22,TextHeight(what,30)));}

  // BO_SUNG: A STT, B ID, C room stable id, D room code, E floor, F kind, G code, H unit, I METRES (active rows: editable input), J circuit,
  // K description, L reason, M active, N overlap decision, O status at export, P current status (active rows: formula), Q metres at export.
  // Active rows are sorted room > kind > code > unit > id, so the rows of one room material are contiguous (ranges[SupKey] = {first,last}).
  static readonly string[] ManualHead={"STT","MÃ BỔ SUNG (ID)","MÃ NHẬN DẠNG PHÒNG","MÃ PHÒNG","TẦNG","LOẠI (WIRE / CONDUIT_HIDDEN / CONDUIT_EXPOSED)","MÃ VẬT TƯ",
   "ĐƠN VỊ","SỐ MÉT (sửa ô xanh)","LỘ / MẠCH","MÔ TẢ","LÝ DO","HIỆU LỰC","TRÙNG VẬT TƯ ĐO CAD CỦA PHÒNG","TRẠNG THÁI LÚC XUẤT","TRẠNG THÁI HIỆN TẠI","SỐ MÉT LÚC XUẤT"};
  static string Manual(RoomExportRequest037 r,List<Pkg> pk,Dictionary<string,int[]> ranges){var s=new Sh();
   double[] w={6,38,22,12,14,20,14,8,14,14,30,30,10,28,14,14,12};int last=w.Length;double all=w.Sum();
   Txt(s,1,1,"BO_SUNG – KHỐI LƯỢNG BỔ SUNG THỦ CÔNG (KHÔNG PHẢI TUYẾN ĐO CAD) – DỰ ÁN "+r.projectName,STitle);s.Merge(1,1,1,last,STitle);s.Height(1,30);
   Note(s,2,last,"Chỉ sửa cột SỐ MÉT (chữ xanh) của mục đang tính; tổng phòng / tầng / dự án ở sheet "+SheetCombined+" tự tính lại. Ô trống, chữ hoặc số âm → CHECK và các tổng "+
    "liên quan hiện CHECK (không có số chính thức). 0 là giá trị hợp lệ.",SNote,all);
   Note(s,3,last,"Số mét nhập là khối lượng bổ sung cuối cùng: không áp hệ số uốn cong, không cộng đầu chờ, không nhân số lõi, không phân bổ ống dùng chung, không có điểm đầu/cuối CAD. "+
    "Các cột khác chỉ để truy vết (sửa phòng / loại / mã / đơn vị → CHECK). Mục KHÔNG HIỆU LỰC (đã xoá, có thể khôi phục) không được tính.",SNote,all);
   const int h=5;for(int c=1;c<=last;c++)Txt(s,h,c,ManualHead[c-1],SHead);s.Height(h,48);
   var act=new List<Sup>();var off=new List<Sup>();
   for(int i=0;i<pk.Count;i++){var x=pk[i].room;var b=x.batch;if(b.adjustments==null)continue;
    var states=new Dictionary<string,RoomSupplementState037>(O);
    if(b.supplementStates!=null)foreach(var st in b.supplementStates)if(st!=null&&st.id!=null&&!states.ContainsKey(st.id))states[st.id]=st;
    foreach(var q in b.adjustments.supplements){RoomSupplementState037 st;
     if(!states.TryGetValue(q.id,out st))throw new IOE("Phòng "+x.code+": mục bổ sung "+q.id+" không có trạng thái tính; không xuất.");
     if(!q.active){off.Add(new Sup{room=i,s=q,st=st});continue;}
     if(st.status!=RoomCompletion037.Pass||!q.metres.HasValue)throw new IOE("Phòng "+x.code+": mục bổ sung "+q.id+" chưa PASS ("+st.status+"); không xuất.");
     act.Add(new Sup{room=i,s=q,st=st});}}
   act=act.OrderBy(z=>z.room).ThenBy(z=>KindIndex(z.s.kind)).ThenBy(z=>z.s.materialCode,O).ThenBy(z=>z.s.unit,O).ThenBy(z=>z.s.id,O).ToList();
   int row=h+1,stt=0;
   if(act.Count==0){Note(s,row,last,"Không có mục bổ sung đang tính.",SNote,all);row++;}
   int a0=row;
   foreach(var q in act){stt++;string R=I(row);ManualRow(s,row,stt,pk[q.room],q,true);
    Fx(s,row,16,"IFERROR(IF($I"+R+"=\"\",\""+Check+"\",IF(NOT(ISNUMBER($I"+R+")),\""+Check+"\",IF($I"+R+"<0,\""+Check+"\",\""+Pass+"\"))),\""+Check+"\")",SStatus,null,Pass);
    var key=SupKey(q.room,q.s.kind,q.s.materialCode,q.s.unit);int[] rg;
    if(!ranges.TryGetValue(key,out rg))ranges[key]=new[]{row,row};else if(rg[1]==row-1)rg[1]=row;else throw new IOE("Chưa xuất: các dòng bổ sung cùng vật tư không liền nhau.");
    row++;}
   int a1=row-1;row++;
   Txt(s,row,1,"MỤC KHÔNG HIỆU LỰC (đã xoá, có thể khôi phục) – KHÔNG TÍNH, CHỈ ĐỂ TRUY VẾT",SHead);s.Merge(row,1,row,last,SHead);s.Height(row,26);row++;
   if(off.Count==0){Note(s,row,last,"Không có mục bổ sung không hiệu lực.",SNote,all);row++;}
   foreach(var q in off.OrderBy(z=>z.room).ThenBy(z=>z.s.id,O)){stt++;ManualRow(s,row,stt,pk[q.room],q,false);Txt(s,row,16,"KHÔNG TÍNH",SCode);row++;}
   return SheetXml(s,false,Frozen(h,9),Cols(w),null,(act.Count>0?CheckRule("P"+I(a0)+":P"+I(a1)):"")+Page());}
  static void ManualRow(Sh s,int row,int stt,Pkg g,Sup q,bool active){var x=g.room;var m=q.s;
   string overlap=!active?"—":!q.st.overlap?"KHÔNG TRÙNG":m.overlapConfirmed?"CÓ TRÙNG – ĐÃ XÁC NHẬN LÀ KHỐI LƯỢNG CỘNG THÊM":"CÓ TRÙNG – CHƯA XÁC NHẬN";
   Val(s,row,1,stt,SCode);Txt(s,row,2,m.id,SCode);Txt(s,row,3,x.roomId,SText);Txt(s,row,4,x.code,SCode);Txt(s,row,5,x.floorName,SText);
   Txt(s,row,6,m.kind,SCode);Txt(s,row,7,m.materialCode,SCode);Txt(s,row,8,m.unit,SCode);Val(s,row,9,m.metres,active?SInNum:SHNum);
   Txt(s,row,10,m.circuitId,SText);Txt(s,row,11,m.description,SText);Txt(s,row,12,m.reason,SText);Txt(s,row,13,active?"CÓ":"KHÔNG",SCode);
   Txt(s,row,14,overlap,SText);Txt(s,row,15,q.st.status,SCode);Val(s,row,17,m.metres,SHNum);
   s.Height(row,Math.Max(Math.Max(TextHeight(m.description,30),TextHeight(m.reason,30)),Math.Max(TextHeight(x.roomId,22),TextHeight(overlap,28))));}

  // BO_DONG: every exclusion record of every exported room (active = ĐANG BỎ, restored = ĐÃ KHÔI PHỤC), in room order then declaration order.
  static readonly string[] AuditHead={"STT","MÃ NHẬN DẠNG PHÒNG","MÃ PHÒNG","TẦNG","KHÓA DÒNG (rowKey)","TUYẾN: ĐIỂM ĐẦU → ĐIỂM CUỐI","LỘ / NHÃN","LOẠI DÂY","CỠ ỐNG",
   "KC MẶT BẰNG (m)","LÝ DO BỎ","CHỮ KÝ NGUỒN LÚC BỎ","CHỮ KÝ NGUỒN HIỆN TẠI","CHỮ KÝ","BẢN GHI","TRẠNG THÁI TÍNH","TRONG BẢNG ĐO CAD","GHI CHÚ"};
  static string Audit(RoomExportRequest037 r,List<Pkg> pk){var s=new Sh();
   double[] w={6,22,12,14,30,34,14,14,10,12,30,26,26,10,16,20,20,44};int last=w.Length;double all=w.Sum();
   Txt(s,1,1,"BO_DONG – DÒNG NGUỒN CAD ĐÃ BỎ / KHÔI PHỤC (TRUY VẾT) – DỰ ÁN "+r.projectName,STitle);s.Merge(1,1,1,last,STitle);s.Height(1,30);
   Note(s,2,last,"Dòng ĐANG BỎ không có khối lượng ở sheet <phòng>_BDGKL và không tham gia phân bổ ống dùng chung (phân bổ đã tính lại trên các dòng còn lại của nguồn đầy đủ). "+
    "Hình học CAD gốc và khai báo của dòng không bị xoá; KHÔI PHỤC giữ bản ghi để truy vết và dòng được tính lại.",SNote,all);
   Note(s,3,last,"Bỏ dòng chỉ áp dụng khi đúng khóa dòng và đúng chữ ký nguồn của lần quét hiện tại; không ghép theo nhãn. Sheet này chỉ để truy vết: sửa ở đây không đổi khối lượng.",SNote,all);
   const int h=5;for(int c=1;c<=last;c++)Txt(s,h,c,AuditHead[c-1],SHead);s.Height(h,48);
   int row=h+1,stt=0;
   for(int i=0;i<pk.Count;i++){var x=pk[i].room;var b=x.batch;if(b.adjustments==null)continue;
    var states=new Dictionary<string,RoomExclusionState037>(O);
    if(b.exclusionStates!=null)foreach(var st in b.exclusionStates)if(st!=null&&st.rowKey!=null&&!states.ContainsKey(st.rowKey))states[st.rowKey]=st;
    var rows=new Dictionary<string,RoomCompletionRow037>(O);foreach(var z in b.rows)if(z!=null&&z.rowKey!=null&&!rows.ContainsKey(z.rowKey))rows[z.rowKey]=z;
    foreach(var e in b.adjustments.exclusions){RoomExclusionState037 st;RoomCompletionRow037 q;rows.TryGetValue(e.rowKey,out q);
     if(!states.TryGetValue(e.rowKey,out st))throw new IOE("Phòng "+x.code+": dòng bỏ "+e.rowKey+" không có trạng thái tính; không xuất.");
     if(e.active&&(st.status!=RoomAdjustments037.Applied||q==null||!q.excluded))throw new IOE("Phòng "+x.code+": dòng bỏ "+e.rowKey+" chưa được áp dụng ("+st.status+"); không xuất.");
     stt++;string route=q==null?null:(q.startLabel??"—")+" → "+(q.endLabel??"—");var d=q==null?null:q.input;
     Val(s,row,1,stt,SCode);Txt(s,row,2,x.roomId,SText);Txt(s,row,3,x.code,SCode);Txt(s,row,4,x.floorName,SText);Txt(s,row,5,e.rowKey,SText);Txt(s,row,6,route,SText);
     Txt(s,row,7,d==null?null:d.circuitId,SText);Txt(s,row,8,d==null?null:d.assemblyCode,SCode);Txt(s,row,9,d==null?null:d.conduitCode,SCode);Val(s,row,10,q==null?null:q.planMetres,SNum);
     Txt(s,row,11,e.reason,SText);Txt(s,row,12,e.sourceSignature,SText);Txt(s,row,13,b.sourceSignature,SText);
     Txt(s,row,14,O.Equals(e.sourceSignature,b.sourceSignature)?"KHỚP":"KHÁC",SCode);Txt(s,row,15,e.active?"ĐANG BỎ":"ĐÃ KHÔI PHỤC",SCode);Txt(s,row,16,st.status,SCode);
     Txt(s,row,17,q==null?"KHÔNG CÓ TRONG NGUỒN HIỆN TẠI":q.excluded?"KHÔNG (đã bỏ)":"CÓ (đang tính)",SCode);Txt(s,row,18,st.message,SText);
     s.Height(row,Math.Max(Math.Max(TextHeight(e.reason,30),TextHeight(st.message,44)),Math.Max(TextHeight(e.rowKey,30),TextHeight(route,34))));row++;}}
   if(stt==0){Note(s,row,last,"Không có dòng nguồn nào bị bỏ hoặc khôi phục.",SNote,all);row++;}
   return SheetXml(s,false,Frozen(h,5),Cols(w),null,Page());}

  // ---- CAO_DO ----
  static string Heights(RoomExportRequest037 r,List<Pkg> pk){var s=new Sh();
   string[] head={"STT","TẦNG","MÃ PHÒNG","MÔ TẢ PHÒNG","BỘ SHEET PHÒNG","CHIỀU CAO TẦNG (m) – CHỈ THÔNG TIN","TRẦN ĐI DÂY (m)","NGUỒN TRẦN ĐI DÂY","TRẦN LẮP THIẾT BỊ (m)",
    "NGUỒN TRẦN LẮP","MÃ THIẾT BỊ (CAD)","LOẠI THIẾT BỊ","TÊN THIẾT BỊ","SỐ TUYẾN DÙNG","CAO ĐỘ HIỆU LỰC (m)","NGUỒN CAO ĐỘ","CÁCH KHAI","GIÁ TRỊ KHAI (m)","TRẠNG THÁI",
    "KHAI RIÊNG: CÁCH KHAI","KHAI RIÊNG: GIÁ TRỊ (m)","GHI CHÚ"};
   double[] w={6,14,12,26,16,14,12,12,12,12,14,14,20,10,12,12,22,12,14,22,12,48};int last=head.Length;double all=w.Sum();
   Txt(s,1,1,"CAO_DO – ẢNH CHỤP NGUỒN GỐC CAO ĐỘ THIẾT BỊ LÚC XUẤT – DỰ ÁN "+r.projectName,STitle);s.Merge(1,1,1,last,STitle);s.Height(1,30);
   Note(s,2,last,"ẢNH CHỤP lúc xuất ("+r.exportedAt+"), chỉ để truy vết: sửa sheet này KHÔNG làm thay đổi công thức hay khối lượng. Cao độ dùng để tính nằm ở bảng thiết bị "+
    "của sheet <phòng>_DM.DIEM (cột E:G, cách khai FIXED). Thứ tự ưu tiên: RIÊNG (một thiết bị) > PHÒNG (loại thiết bị của phòng) > CHUNG (mặc định dự án).",SNote,all);
   Note(s,3,last,"Trần đi dây và trần lắp thiết bị là hai mốc khác nhau, không thay thế cho nhau; chiều cao tầng chỉ là thông tin. Nguồn trần: PHÒNG = khai riêng của phòng, "+
    "TẦNG = giá trị của tầng. Ô trống = chưa khai (không bao giờ hiểu là 0).",SNote,all);
   int row=5;Txt(s,row,1,"MẶC ĐỊNH CHUNG CỦA DỰ ÁN",SHead);s.Merge(row,1,row,3,SHead);Txt(s,row,4,"CÁCH KHAI",SHead);Txt(s,row,5,"GIÁ TRỊ (m)",SHead);s.Height(row,30);row++;
   foreach(var t in RoomProject037.Types){var d=r.defaults.FirstOrDefault(x=>x.deviceType==t);
    Txt(s,row,1,RoomHeight037.TypeName(t).ToUpperInvariant(),SText);s.Merge(row,1,row,3,SText);Txt(s,row,4,d==null?"CHƯA KHAI":ModeLabel(d.mode),SText);
    Val(s,row,5,d==null?(double?)null:d.value,SNum);s.Height(row,TextHeight(d==null?"":ModeLabel(d.mode),26));row++;}
   row++;int h=row;for(int c=1;c<=last;c++)Txt(s,h,c,head[c-1],SHead);s.Height(h,48);row++;int first=row,stt=0;
   foreach(var g in pk){var x=g.room;
    Action<int> roomCols=rr=>{Txt(s,rr,2,x.floorName,SText);Txt(s,rr,3,x.code,SCode);Txt(s,rr,4,x.description,SText);Txt(s,rr,5,g.prefix,SCode);
     Val(s,rr,6,x.storeyHeight,SNum);Val(s,rr,7,x.wireCeiling,SNum);Txt(s,rr,8,x.wireCeilingSource??"CHƯA KHAI",SCode);Val(s,rr,9,x.mountingCeiling,SNum);
     Txt(s,rr,10,x.mountingCeilingSource??"CHƯA KHAI",SCode);};
    if(x.heights.Count==0){stt++;Val(s,row,1,stt,SCode);roomCols(row);Txt(s,row,11,"(không có thiết bị trong vùng quét)",SText);s.Merge(row,11,row,last,SText);s.Height(row,22);row++;continue;}
    foreach(var d in x.heights.OrderBy(q=>q.deviceType??"",O).ThenBy(q=>q.deviceName??"",O).ThenBy(q=>q.deviceId,O)){stt++;Val(s,row,1,stt,SCode);roomCols(row);
     Txt(s,row,11,d.deviceId,SCode);Txt(s,row,12,RoomHeight037.TypeName(d.deviceType).ToUpperInvariant(),SText);Txt(s,row,13,d.deviceName,SText);Val(s,row,14,d.rowsUsing,SInt);
     Val(s,row,15,d.effective,SNum);Txt(s,row,16,SourceLabel(d.source),SCode);Txt(s,row,17,ModeLabel(d.mode),SText);Val(s,row,18,d.declaredValue,SNum);
     Txt(s,row,19,StatusLabel(d.status),SCode);Txt(s,row,20,d.ownMode==null?"":ModeLabel(d.ownMode),SText);Val(s,row,21,d.ownValue,SNum);Txt(s,row,22,d.message,SText);
     s.Height(row,Math.Max(Math.Max(TextHeight(d.message,48),TextHeight(x.description,26)),Math.Max(TextHeight(ModeLabel(d.mode),22),TextHeight(d.deviceName,20))));row++;}}
   string pane="<pane ySplit=\""+I(h)+"\" topLeftCell=\""+Ref(first,1)+"\" activePane=\"bottomLeft\" state=\"frozen\"/><selection pane=\"bottomLeft\" activeCell=\""+Ref(first,1)+"\" sqref=\""+Ref(first,1)+"\"/>";
   return SheetXml(s,false,pane,Cols(w),"<autoFilter ref=\""+Ref(h,1)+":"+Ref(row-1,last)+"\"/>",Page());}

  // ---- package parts ----
  static byte[] Utf(string s){return new UTF8Encoding(false).GetBytes(s);}
  static byte[] Save(XmlDocument d){var ws=new XmlWriterSettings{Encoding=new UTF8Encoding(false),Indent=false,NewLineHandling=NewLineHandling.None};
   using(var ms=new MemoryStream()){using(var w=XmlWriter.Create(ms,ws))d.Save(w);return ms.ToArray();}}
  static void Entry(ZipArchive z,string name,byte[] bytes){using(var s=z.CreateEntry(name,CompressionLevel.Optimal).Open())s.Write(bytes,0,bytes.Length);}
  static string ContentTypes(int sheets){const string M="application/vnd.openxmlformats-officedocument.spreadsheetml.";
   var b=new StringBuilder("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">");
   b.Append("<Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/><Default Extension=\"xml\" ContentType=\"application/xml\"/>");
   b.Append("<Override PartName=\"/xl/workbook.xml\" ContentType=\""+M+"sheet.main+xml\"/>");
   for(int i=1;i<=sheets;i++)b.Append("<Override PartName=\"/xl/worksheets/sheet"+I(i)+".xml\" ContentType=\""+M+"worksheet+xml\"/>");
   b.Append("<Override PartName=\"/xl/styles.xml\" ContentType=\""+M+"styles+xml\"/></Types>");return b.ToString();}
  // lead: sheets before the room sheets (TONG_HOP, CAO_DO; S5 adjusted: TONG_CAD_BO_SUNG, TONG_HOP, BO_SUNG, BO_DONG, CAO_DO). The first one is active.
  static string Workbook(List<string> lead,List<Pkg> pk){var b=new StringBuilder("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><workbook xmlns=\""+Ns+"\" xmlns:r=\""+NsR+"\">");
   b.Append("<bookViews><workbookView activeTab=\"0\"/></bookViews><sheets>");
   int n=1;foreach(var nm in lead){b.Append("<sheet name=\""+X(nm)+"\" sheetId=\""+I(n)+"\" r:id=\"rId"+I(n)+"\"/>");n++;}
   foreach(var g in pk)foreach(var nm in g.names){b.Append("<sheet name=\""+X(nm)+"\" sheetId=\""+I(n)+"\" r:id=\"rId"+I(n)+"\"/>");n++;}
   b.Append("</sheets>");
   var defs=new StringBuilder();
   for(int i=0;i<pk.Count;i++)foreach(var d in pk[i].defs)
    defs.Append("<definedName name=\""+X(d.name)+"\" localSheetId=\""+I(lead.Count+3*i+d.role)+"\""+(string.IsNullOrEmpty(d.hidden)?"":" hidden=\""+X(d.hidden)+"\"")+">"+X(d.text)+"</definedName>");
   if(defs.Length>0)b.Append("<definedNames>").Append(defs).Append("</definedNames>");
   b.Append("<calcPr calcId=\"191029\" fullCalcOnLoad=\"1\"/></workbook>");return b.ToString();}
  // Room styles + 4 appended cell formats (existing indexes unchanged): 15 number 0.0## right, 16 bold total number on grey, 17 bold total text, 18 bold total centre.
  static XmlDocument ExtendStyles(string styles){XmlDocument d;using(var sr=new StringReader(styles))using(var rd=XmlReader.Create(sr,new XmlReaderSettings{DtdProcessing=DtdProcessing.Prohibit,XmlResolver=null})){
    d=new XmlDocument{PreserveWhitespace=true,XmlResolver=null};d.Load(rd);}
   var xfs=d.GetElementsByTagName("cellXfs",Ns).Cast<XmlElement>().SingleOrDefault();
   if(xfs==null||xfs.GetElementsByTagName("xf",Ns).Count!=BaseXfs)throw new IOE("Bảng định dạng của phòng khác dự kiến ("+BaseXfs.ToString(Inv)+" kiểu ô); không gộp.");
   Action<string,string,string,string,string> add=(fmt,font,fill,h,wrap)=>{var x=d.CreateElement("xf",Ns);
    foreach(var a in new[]{Tuple.Create("numFmtId",fmt),Tuple.Create("fontId",font),Tuple.Create("fillId",fill),Tuple.Create("borderId","1"),Tuple.Create("xfId","0"),
     Tuple.Create("applyNumberFormat","1"),Tuple.Create("applyFont","1"),Tuple.Create("applyFill","1"),Tuple.Create("applyBorder","1"),Tuple.Create("applyAlignment","1")})x.SetAttribute(a.Item1,a.Item2);
    var al=d.CreateElement("alignment",Ns);al.SetAttribute("horizontal",h);al.SetAttribute("vertical","center");if(wrap!=null)al.SetAttribute("wrapText","1");x.AppendChild(al);xfs.AppendChild(x);};
   add("165","0","0","right",null);add("165","1","3","right",null);add("0","1","3","left","1");add("0","1","3","center","1");
   xfs.SetAttribute("count",I(BaseXfs+4));return d;}
 }
}
