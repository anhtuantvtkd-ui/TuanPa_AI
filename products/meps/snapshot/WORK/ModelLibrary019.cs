using System;using System.IO;using System.Linq;using System.Collections.Generic;using System.Globalization;using System.Text;using System.Text.RegularExpressions;
#if MODELLIB019 && !MODELLIB019_PURE
using System.Threading;using Autodesk.AutoCAD.DatabaseServices;using Autodesk.AutoCAD.Geometry;
#endif
#if MODELLIB019 && !MODELLIB019_PURE && !(STANDARDS012 && IDENTITY015)
#error MODELLIB019 needs STANDARDS012 and IDENTITY015 (category layer + type code of every new entry).
#endif
#if MODELLIB019
namespace Tvxd.Meps {
 // TVXD-MEPS-MODELLIB-019: the user authors library entries from Model (capture one selected block, make an editable copy, remove an entry by selecting a
 // placed symbol — 021: an actual tool delete, see LibraryDelete021) under a MANDATORY naming rule for NEW entries. Pure part (no CAD types; compiles with /define:MODELLIB019;MODELLIB019_PURE together with Core,
 // Library, Legend, Meps06, Standards012 /define:STANDARDS_PURE and Identity015 /define:IDENTITY_PURE):
 //  - Naming019.Check: E-<GROUP>-<TYPE>[-<VARIANT>…], ASCII upper case, ≤ 120 characters, Library.ValidName, a Standards012.CategoryMap group, a valid
 //    Identity015 import code; an alias already in the catalog (hidden too), in the 67-name codebook or (copy) in the drawing is refused with a distinct
 //    revision suggestion (E-OC-1 → E-OC-1-R02). Nothing is renamed automatically; existing entries are never re-validated or migrated.
 //  - Naming019.AddCaptureLegend: one generated LegendSource (parser MODEL_CAPTURE019) per captured entry. Column/heading layout, styles, layers and grid
 //    come from an existing source table used as TEMPLATE (no row text of the template is copied); exactly one LegendRow with the cells the user typed
 //    (blank when absent), pose 1 / rotation 0. No engineering value is inferred.
 public sealed class NameCheck019 {public string name,reason,suggestion,category,layer;public bool ok;
  public string Message(){return ok?"Tên "+name+" hợp lệ (nhóm "+category+" → layer "+layer+").":"Tên '"+(name??"")+"' không dùng được: "+reason+(suggestion!=null?" Gợi ý: "+suggestion+".":"")+" Ví dụ đúng: "+Naming019.Example+".";}}
 // DIỄN GIẢI / QUY CÁCH / GHI CHÚ typed by the user when adding from Model; blank is allowed (the legend cell then stays blank).
 public sealed class CaptureCells019 {public string description="",spec="",note="";}
 public static class Naming019 {
  public const string Pattern=@"^E-(DEN|CT|OC|NHE|DH|QUAT|TU|AT|X|SDNL|DAY)-[A-Z0-9]+(?:-[A-Z0-9]+)*$";
  public const int MaxLength=120,MaxCell=500;
  public const string Parser="MODEL_CAPTURE019",CaptureDir="captures019",Example="E-OC-USB-1",RowHandle="MODEL_CAPTURE019";
  public static readonly string[] Groups={"DEN","CT","OC","NHE","DH","QUAT","TU","AT","X","SDNL","DAY"};
  public const string RuleText="Quy tắc: E-<NHÓM>-<LOẠI>[-<BIẾN THỂ>], chữ IN HOA không dấu A–Z, số 0–9 và '-', không khoảng trắng, tối đa 120 ký tự; NHÓM: DEN, CT, OC, NHE, DH, QUAT, TU, AT, X, SDNL, DAY.";
  static readonly Regex Rx=new Regex(Pattern,RegexOptions.CultureInvariant),Revision=new Regex("-R[0-9]{2,3}$",RegexOptions.CultureInvariant);
  static bool Same(string a,string b){return string.Equals(a,b,StringComparison.OrdinalIgnoreCase);}
  // First rule a name breaks (exact reason), or null when it has the required shape. Order: empty, length, accents, spaces, lower case, other characters,
  // prefix, parts, group, empty part.
  public static string Shape(string name){
   if(string.IsNullOrEmpty(name)||name.Trim().Length==0)return "tên trống.";
   if(name.Length>MaxLength)return "tên dài "+name.Length+" ký tự, tối đa "+MaxLength+".";
   var accent=name.FirstOrDefault(c=>c>127);if(accent!=default(char))return "có chữ có dấu hoặc ký tự ngoài ASCII ('"+accent+"'); chỉ dùng A–Z, 0–9 và '-' không dấu.";
   if(name.Any(char.IsWhiteSpace))return "có khoảng trắng; dùng '-' để ngăn các phần.";
   if(name.Any(c=>c>='a'&&c<='z'))return "có chữ thường; dùng chữ IN HOA (ví dụ "+name.ToUpperInvariant()+").";
   var bad=name.FirstOrDefault(c=>!((c>='A'&&c<='Z')||(c>='0'&&c<='9')||c=='-'));if(bad!=default(char))return "có ký tự '"+bad+"' không được dùng; chỉ A–Z, 0–9 và '-'.";
   if(!name.StartsWith("E-",StringComparison.Ordinal))return "phải bắt đầu bằng E-<NHÓM>-.";
   var parts=name.Split('-');if(parts.Any(p=>p.Length==0))return "có hai dấu '-' liền nhau hoặc '-' ở cuối.";
   if(parts.Length<3)return "cần ít nhất 3 phần E-<NHÓM>-<LOẠI> (tên 2 phần như E-OC không thuộc nhóm nào).";
   if(!Groups.Contains(parts[1]))return "nhóm '"+parts[1]+"' không thuộc danh sách "+string.Join(", ",Groups)+" (nhóm mới cần bổ sung ánh xạ layer trước).";
   if(!Rx.IsMatch(name))return "không đúng mẫu E-<NHÓM>-<LOẠI>[-<BIẾN THỂ>].";
   return null;}
  static bool Book(string name){return Identity015.Codebook.Any(f=>Same(f.name,name));}
  // 021: a name deleted from the tool stays taken (catalog.deleted): placed symbols of old drawings still carry it, so a NEW entry never reuses it.
  static IEnumerable<LibEntry> Deleted(LibCatalog c){return c==null||c.deleted==null?Enumerable.Empty<LibEntry>():c.deleted.Where(e=>e!=null);}
  static bool Taken(string name,LibCatalog catalog,ICollection<string> alsoTaken){return Book(name)||(catalog!=null&&catalog.entries.Any(e=>e!=null&&Same(e.name,name)))||Deleted(catalog).Any(e=>Same(e.name,name))||(alsoTaken!=null&&alsoTaken.Any(n=>Same(n,name)));}
  // A revision alias not used anywhere: <base>-R02, -R03 … (an existing -Rnn suffix is replaced). null when the name itself has no valid shape.
  public static string Suggest(string name,LibCatalog catalog,ICollection<string> alsoTaken){if(name==null)return null;string b=Revision.Replace(name,"");if(Shape(b)!=null)return null;
   for(int k=2;k<=999;k++){string c=b+"-R"+(k<100?k.ToString("00",CultureInfo.InvariantCulture):k.ToString(CultureInfo.InvariantCulture));if(Shape(c)==null&&!Taken(c,catalog,alsoTaken))return c;}return null;}
  // Mandatory check of a NEW root name. catalog = the library re-read by the caller (hidden entries count); alsoTaken = block names of the drawing (copy).
  public static NameCheck019 Check(string name,LibCatalog catalog,ICollection<string> alsoTaken){var r=new NameCheck019{name=name};
   string why=Shape(name);if(why!=null){r.reason=why;r.suggestion=null;return r;}
   try{Library.ValidName(name);}catch(Exception ex){r.reason=ex.Message;return r;}
   string cat=Library.Category(name);var map=Standards012.CategoryMap.FirstOrDefault(m=>Same(m[0],cat));
   if(map==null){r.reason="nhóm "+cat+" chưa có ánh xạ layer trong hồ sơ "+Standards012.Profile+".";return r;}
   string code=Identity015.Prefix+Identity015.Normalize(name)+"_"+new string('0',12);if(!Identity015.ValidCode(code)){r.reason="không tạo được mã loại hợp lệ ("+code+").";return r;}
   if(Book(name)){r.reason="trùng tên một trong 67 ký hiệu gốc (sổ mã 015); tên/mã gốc giữ nguyên.";r.suggestion=Suggest(name,catalog,alsoTaken);return r;}
   var hit=catalog==null?null:catalog.entries.FirstOrDefault(e=>e!=null&&Same(e.name,name));
   if(hit!=null){r.reason="đã có trong thư viện"+(hit.hidden?" (mục đang ẩn)":"")+" — mục "+hit.id+"; MEPS không ghi đè hình hay gộp mã.";r.suggestion=Suggest(name,catalog,alsoTaken);return r;}
   var gone=Deleted(catalog).FirstOrDefault(e=>Same(e.name,name));
   if(gone!=null){r.reason="là tên mục "+gone.id+" đã xoá khỏi công cụ; tên được giữ cho các ký hiệu đã đặt trong bản vẽ cũ (mã loại cũ), nên mục mới cần tên khác.";r.suggestion=Suggest(name,catalog,alsoTaken);return r;}
   if(alsoTaken!=null&&alsoTaken.Any(n=>Same(n,name))){r.reason="đã là tên một định nghĩa khối trong bản vẽ hiện tại.";r.suggestion=Suggest(name,catalog,alsoTaken);return r;}
   r.ok=true;r.category=cat;r.layer=map[1];return r;}
  // One user cell: trimmed, ≤ 500 characters, no control characters except line breaks (tab → space). Line breaks become MText \P in contents.
  public static string CleanCell(string s,string what){s=(s??"").Replace("\r\n","\n").Replace('\r','\n').Replace('\t',' ').Trim();
   if(s.Length>MaxCell)throw new InvalidOperationException("Ô "+what+" dài "+s.Length+" ký tự, tối đa "+MaxCell+".");
   if(s.Any(c=>char.IsControl(c)&&c!='\n'))throw new InvalidOperationException("Ô "+what+" có ký tự điều khiển.");return s;}
  public static CaptureCells019 Clean(CaptureCells019 c){if(c==null)c=new CaptureCells019();
   return new CaptureCells019{description=CleanCell(c.description,Legend.CellNames[0]),spec=CleanCell(c.spec,Legend.CellNames[1]),note=CleanCell(c.note,Legend.CellNames[2])};}
  // Plain text → raw MText contents (backslash and braces escaped, line break → \P); nothing else is added.
  public static string MTextContents(string text){var b=new StringBuilder();foreach(char ch in text??""){if(ch=='\\')b.Append("\\\\");else if(ch=='{')b.Append("\\{");else if(ch=='}')b.Append("\\}");else if(ch=='\n')b.Append("\\P");else b.Append(ch);}return b.ToString();}
  // ---- legend ----
  static bool Usable(LegendSource s){return s!=null&&s.status==Legend.Table&&s.parser!=Parser&&s.columns!=null&&s.columns.Count==5&&s.headers!=null&&s.headers.Count==4&&s.dividers!=null&&s.dividers.Count>=2&&s.styles!=null&&s.styles.Count>0;}
  // Template table: the codebook source when present and complete, else the first complete source table read by the parser (never a generated one).
  public static LegendSource FindTemplate(LegendMap map){if(map==null)return null;var book=map.sources.FirstOrDefault(s=>Usable(s)&&Same(s.sourceSha256,Identity015.CodebookSha256));return book??map.sources.FirstOrDefault(Usable);}
  // Text format (style, layer, height; NO text) of a body cell of the template: the most frequent among its rows, else the DIỄN GIẢI heading.
  public static LegendCell Format(LegendMap map,LegendSource t){var styles=new HashSet<string>(t.styles.Select(x=>x.name),StringComparer.OrdinalIgnoreCase);
   var cells=map.rows.Where(r=>Same(r.sourceSha256,t.sourceSha256)).SelectMany(r=>new[]{r.description,r.spec,r.note}).Where(c=>c!=null&&!string.IsNullOrEmpty(c.style)&&styles.Contains(c.style)&&c.height>0&&Core.Finite(c.height)).ToList();
   var c0=cells.GroupBy(c=>c.style.ToUpperInvariant()+"\u0001"+(c.layer??"")+"\u0001"+c.height.ToString("R",CultureInfo.InvariantCulture)).OrderByDescending(g=>g.Count()).Select(g=>g.First()).FirstOrDefault();
   if(c0==null)c0=new[]{t.headers[1]}.Concat(t.headers).FirstOrDefault(h=>h!=null&&!string.IsNullOrEmpty(h.style)&&styles.Contains(h.style));
   return c0==null?null:new LegendCell{style=c0.style,layer=c0.layer,height=c0.height};}
  static LegendCell UserCell(string text,LegendCell fmt){if(text.Length==0)return new LegendCell();return new LegendCell{text=text,contents=MTextContents(text),style=fmt.style,layer=fmt.layer,height=fmt.height};}
  static T Copy<T>(T x){var j=Library.Json();return j.Deserialize<T>(j.Serialize(x));}
  // Adds the generated source + one row of a captured entry to map (in memory) and validates the map. The entry's own source hash/file is the capture DWG;
  // insunits/units stay the template table's layout marker (the numbers ARE the template's); the capture drawing's INSUNITS is recorded as provenance.
  public static void AddCaptureLegend(LegendMap map,LegendSource t,LibEntry e,CaptureCells019 cells,string captureUnits){
   if(map==null||e==null||!Usable(t))throw new InvalidOperationException("Thiếu bảng ký hiệu mẫu hợp lệ trong legend.json; chưa thêm gì.");
   if(map.sources.Any(s=>Same(s.sourceSha256,e.sourceSha256))||map.rows.Any(r=>Same(r.sourceSha256,e.sourceSha256)))throw new InvalidOperationException("legend.json đã có nguồn "+e.sourceSha256.Substring(0,12)+"; không ghi đè.");
   var c=Clean(cells);var fmt=Format(map,t);if(fmt==null)throw new InvalidOperationException("Bảng mẫu "+t.sourceFile+" không có kiểu chữ ô dùng được; chưa thêm gì.");
   double h=Legend.TypicalRowHeight(t);if(!Core.Finite(h)||!(h>0))throw new InvalidOperationException("Bảng mẫu "+t.sourceFile+" không có chiều cao hàng hợp lệ; chưa thêm gì.");
   // (gen, not s: C# 5 refuses a local named like the lambda parameter s above)
   var gen=Copy(t);gen.sourceSha256=e.sourceSha256;gen.sourceFile=e.sourceFile;gen.parser=Parser;gen.status=Legend.Table;gen.extractedUtc=DateTime.UtcNow.ToString("o",CultureInfo.InvariantCulture);
   gen.title=null;gen.titleTop=0;gen.dividers=new List<double>{t.headerBottom,t.headerBottom-h};gen.bottom=t.headerBottom-h;
   gen.issues=new List<string>{"INFO "+Parser+": nguồn là bản chụp từ Model "+e.sourceFile+" (sha256 "+e.sourceSha256+"); không phải bảng đọc từ DXF/DWG.",
    "INFO bố cục cột, tiêu đề cột, kiểu chữ, layer và đường kẻ lấy từ bảng mẫu "+t.sourceFile+" (sha256 "+t.sourceSha256+", parser "+(t.parser??"?")+"); không chép chữ mô tả của hàng nào trong mẫu.",
    "INFO INSUNITS của bảng = dấu bố cục của bảng mẫu ("+t.units+"); bản vẽ chụp có INSUNITS "+(captureUnits??"?")+" (ghi trong catalog). Không quy đổi đơn vị."};
   var row=new LegendRow{name=e.name,blockName=e.name,entryId=e.id,sourceSha256=e.sourceSha256,sourceFile=e.sourceFile,insertHandle=RowHandle,row=0,top=gen.dividers[0],bottom=gen.dividers[1],
    x=(gen.columns[0]+gen.columns[1])/2,y=(gen.dividers[0]+gen.dividers[1])/2,z=0,rotation=0,sx=1,sy=1,sz=1,nx=0,ny=0,nz=1,description=UserCell(c.description,fmt),spec=UserCell(c.spec,fmt),note=UserCell(c.note,fmt)};
   row.status=c.description.Length>0?Legend.Ok:Legend.MissingDescription;
   row.issues.Add("INFO "+Parser+": DIỄN GIẢI/QUY CÁCH/GHI CHÚ do người dùng nhập khi thêm từ Model"+(new[]{c.description,c.spec,c.note}.All(x=>x.Length==0)?" (để trống cả ba)":"")+"; MEPS không tự viết, không lấy từ DXF gốc.");
   if(row.status==Legend.MissingDescription)row.issues.Add("Ô DIỄN GIẢI trống.");
   var copy=Copy(map);copy.sources.Add(gen);copy.rows.Add(row);Legend.Validate(copy);map.sources.Add(gen);map.rows.Add(row);}
 }
#if !MODELLIB019_PURE
 // Selected reference as read (never modified). alias/markerId/identityId: MEPS identity of a placed library symbol, else null.
 public sealed class Selected019 {public string effectiveName,alias,markerId,markerName,markerSource,markerFile,identityId,identitySource,identityIssue,handle;public bool dynamic,anonymous;}
 // sourceId: MEPS entry ID the selected original already carried (marker/identity), null for a foreign/unlinked block.
 public sealed class CaptureResult019 {public LibEntry entry;public string capture,code,layer,sourceId;public bool blank;public List<string> notes=new List<string>(),artifacts=new List<string>(),backups=new List<string>();
  // F09: the original is never converted, adopted or erased; the wording depends on whether it was already a MEPS symbol.
  public string Summary(){return "Đã thêm "+entry.name+" vào thư viện (mục "+entry.id+", mã "+code+", khi đặt nằm trên layer "+layer+"). Bản chụp: "+capture+". Bảng ký hiệu: "+(blank?"DIỄN GIẢI để trống":"dùng chữ bạn nhập")+
   ". Khối gốc và bản vẽ không bị đổi tên/sửa/lưu/xoá; khối gốc KHÔNG tự chuyển sang mục mới "+entry.id+" — "+(sourceId!=null?"nó giữ mã MEPS cũ (mục "+sourceId+")":"nó vẫn là khối ngoài, MEPS không nhận")+
   "; muốn dùng mục mới thì đặt ký hiệu "+entry.name+" từ bảng.";}}
 public sealed class CopyResult019 {public ObjectId reference,definition;public string name;public List<string> notes=new List<string>();}
 // AutoCAD database side (AcDbMgd/AcCoreMgd only: also usable by CoreConsole QA). The user's drawing is only READ by Capture/Inspect/ResolveForRemove;
 // EditableCopy adds one definition closure + one reference in the caller's command (UNDO removes all of it); a failure removes only its own new definitions.
 public static class ModelLibraryCad019 {
  // QA failure injection: a stage name ("after-snapshot", "after-build", "before-legend", "before-catalog", "after-catalog", "copy-after-clone",
  // "copy-after-reference") makes that stage throw. null in production.
  public static string FailAt;
  static void Fail(string stage){if(FailAt!=null&&string.Equals(FailAt,stage,StringComparison.Ordinal))throw new InvalidOperationException("QA: lỗi giả lập tại "+stage+".");}
  static bool Same(string a,string b){return string.Equals(a,b,StringComparison.OrdinalIgnoreCase);}
  // ---- reading the selection ----
  public static Selected019 Inspect(Database db,ObjectId id){return Inspect(db,id,true);}
  static Selected019 Inspect(Database db,ObjectId id,bool content){if(id.IsNull||id.IsErased||id.Database!=db)throw new InvalidOperationException("Đối tượng đã bị xoá hoặc không thuộc bản vẽ hiện tại; chưa thay đổi gì.");
   var s=new Selected019();
   using(var tr=db.TransactionManager.StartOpenCloseTransaction()){var br=tr.GetObject(id,OpenMode.ForRead) as BlockReference;
    if(br==null)throw new InvalidOperationException("Chỉ chọn MỘT block (BlockReference); MEPS không nhận hình rời.");
    if(br is MInsertBlock)throw new InvalidOperationException("MINSERT (khối mảng) chưa hỗ trợ; chưa thay đổi gì.");
    if(br.OwnerId!=SymbolUtilityServices.GetBlockModelSpaceId(db))throw new InvalidOperationException("Block không nằm trực tiếp trong Model (Layout hoặc bên trong khối khác); chưa thay đổi gì.");
    var b=(BlockTableRecord)tr.GetObject(br.BlockTableRecord,OpenMode.ForRead);s.handle=br.Handle.ToString();s.dynamic=br.IsDynamicBlock;s.anonymous=b.IsAnonymous;
    if(b.IsLayout)throw new InvalidOperationException("Đối tượng là layout, không phải block.");
    if(b.IsFromExternalReference||b.IsFromOverlayReference||b.IsDependent)throw new InvalidOperationException("Block "+b.Name+" là xref/phụ thuộc xref; MEPS không resolve xref. Chưa thay đổi gì.");
    s.effectiveName=b.Name;BlockTableRecord dyn=null;if(br.IsDynamicBlock&&!br.DynamicBlockTableRecord.IsNull){dyn=(BlockTableRecord)tr.GetObject(br.DynamicBlockTableRecord,OpenMode.ForRead);s.effectiveName=dyn.Name;}
    if(content)foreach(var c in LibraryCad.Closure(tr,br.BlockTableRecord)){var n=(BlockTableRecord)tr.GetObject(c,OpenMode.ForRead);
     if(n.IsFromExternalReference||n.IsFromOverlayReference||n.IsDependent)throw new InvalidOperationException("Block chứa xref lồng "+n.Name+"; MEPS không resolve xref. Chưa thay đổi gì.");
     foreach(ObjectId eid in n){var o=tr.GetObject(eid,OpenMode.ForRead,false,true);if(o is ProxyEntity)throw new InvalidOperationException("Block chứa proxy entity (định nghĩa "+n.Name+"); MEPS không làm phẳng proxy. Chưa thay đổi gì.");}}
    try{var rec=IdentityCad015.Read(tr,br);if(rec!=null&&rec.kind==Identity015.KindBlock){s.alias=rec.alias;s.identityId=rec.sourceId;s.identitySource=rec.sourceSha256;}}catch(InvalidOperationException ex){s.identityIssue=ex.Message;}
    var mark=LibraryCad.ReadMarker(tr,b);if(mark==null&&dyn!=null)mark=LibraryCad.ReadMarker(tr,dyn);
    if(mark!=null&&mark.Length>=3){s.markerId=mark[0];s.markerFile=mark[1];s.markerName=mark[2];s.markerSource=mark.Length>=4?mark[3]:null;if(s.alias==null)s.alias=s.markerName;}
    tr.Commit();}
   return s;}
  // Readable default alias: the MEPS entry name of a placed library symbol, else the effective block name (shown to the user, validated, never applied silently).
  public static string DefaultAlias(Selected019 s){return s==null?"":!string.IsNullOrEmpty(s.alias)?s.alias:s.effectiveName??"";}
  public static List<string> BlockNames(Database db){var list=new List<string>();using(var tr=db.TransactionManager.StartOpenCloseTransaction()){foreach(ObjectId id in (BlockTable)tr.GetObject(db.BlockTableId,OpenMode.ForRead)){if(id.IsErased)continue;var b=(BlockTableRecord)tr.GetObject(id,OpenMode.ForRead);if(!b.IsAnonymous&&!b.IsLayout)list.Add(b.Name);}tr.Commit();}return list;}
  // Removes every extension-dictionary key of a MEPS record (TVXD_MEPS…: library marker, identity, cable, legend, room/device metadata) from an object of an
  // OWNED database, so a capture/copy never carries the identity of the block it was made from. Returns the number of keys removed.
  static int Strip(Transaction tr,DBObject o){if(o.ExtensionDictionary.IsNull)return 0;var d=(DBDictionary)tr.GetObject(o.ExtensionDictionary,OpenMode.ForWrite);var keys=new List<string>();
   foreach(DBDictionaryEntry de in d)if(de.Key.StartsWith("TVXD_MEPS",StringComparison.OrdinalIgnoreCase))keys.Add(de.Key);
   foreach(var k in keys){var id=d.GetAt(k);d.Remove(k);var x=tr.GetObject(id,OpenMode.ForWrite,true);if(!x.IsErased)x.Erase();}return keys.Count;}
  static int StripClosure(Transaction tr,ObjectId root,ObjectId skip){int n=0;foreach(var bid in LibraryCad.Closure(tr,root)){if(bid==skip)continue;var b=(BlockTableRecord)tr.GetObject(bid,OpenMode.ForWrite);n+=Strip(tr,b);
    foreach(ObjectId eid in b){var o=tr.GetObject(eid,OpenMode.ForWrite,false,true);n+=Strip(tr,o);var br=o as BlockReference;if(br!=null)foreach(ObjectId a in br.AttributeCollection)n+=Strip(tr,tr.GetObject(a,OpenMode.ForWrite,false,true));}}return n;}
  static ObjectId Def(Database db,string name){using(var tr=db.TransactionManager.StartTransaction()){var bt=(BlockTable)tr.GetObject(db.BlockTableId,OpenMode.ForRead);if(!bt.Has(name))throw new InvalidOperationException("Bản chụp không chứa khối "+name+".");var id=bt[name];tr.Commit();return id;}}
  // '+ → Thêm từ DWG/DXF' naming gate (LibraryCad.Import, before any entry is built): definitions already in the catalog (same ID) keep the old behaviour;
  // a NEW definition must pass Naming019.Check. A new root definition (not nested in another definition of the file) with an invalid or already used
  // name refuses the whole import (no entry written). A new definition that only occurs nested inside another one and fails the check is a dependency, not a
  // standalone symbol: it is skipped (reported) and stays inside the copy of the block that contains it.
  public static List<KeyValuePair<string,ObjectId>> FilePlan(Database src,List<KeyValuePair<string,ObjectId>> defs,LibCatalog catalog,string sha,List<string> skipped){
   var nested=new HashSet<ObjectId>();using(var tr=src.TransactionManager.StartTransaction()){foreach(var d in defs)foreach(var c in LibraryCad.Closure(tr,d.Value).Skip(1))nested.Add(c);tr.Commit();}
   var keep=new List<KeyValuePair<string,ObjectId>>();var errors=new List<string>();
   foreach(var d in defs){string id=null;try{id=Library.EntryId(sha,d.Key);}catch(Exception){}
    if(id!=null&&catalog.entries.Any(x=>x.id==id)){keep.Add(d);continue;}
    // 021: the same source + same name as a deleted record = the user explicitly re-adds that deleted entry (same ID; LibraryCad.Import resolves the record).
    if(id!=null&&catalog.deleted!=null&&catalog.deleted.Any(x=>x!=null&&x.id==id)){keep.Add(d);continue;}
    var chk=Naming019.Check(d.Key,catalog,null);if(chk.ok){keep.Add(d);continue;}
    if(nested.Contains(d.Value)){skipped.Add(d.Key+": khối lồng/phụ trợ, "+chk.reason+" — không nhập thành mục riêng (vẫn nằm trong khối chứa nó)");continue;}
    errors.Add(d.Key+": "+chk.reason+(chk.suggestion!=null?" Gợi ý: "+chk.suggestion+".":""));}
   if(errors.Count>0)throw new InvalidOperationException("Không thêm mục nào: "+errors.Count+" block gốc có tên sai quy tắc hoặc đã có trong thư viện — "+string.Join("; ",errors.Take(8))+(errors.Count>8?" …":"")+". Sửa tên trong BẢN SAO file nguồn rồi thêm lại. "+Naming019.RuleText);
   return keep;}
  // ---- source text styles (F02) ----
  // WblockCloneObjects(Ignore) keeps a same-named style of the side database (the default Standard of new Database(true,true)), so a modified source
  // Standard would silently become the default. Wanted = styles used by the definition closure (LibraryCad06.SourceStyles) plus styles used ONLY by the
  // selected reference's own attribute references (refId; ObjectId.Null = none). The source is only read.
  static Dictionary<string,LegendStyle> UsedStyles(Database db,ObjectId def,ObjectId refId){var want=LibraryCad06.SourceStyles(db,def);if(refId.IsNull)return want;
   using(var tr=db.TransactionManager.StartOpenCloseTransaction()){var br=(BlockReference)tr.GetObject(refId,OpenMode.ForRead);
    foreach(ObjectId a in br.AttributeCollection){if(a.IsErased)continue;var ar=tr.GetObject(a,OpenMode.ForRead,false,true) as AttributeReference;if(ar==null||ar.IsErased||ar.TextStyleId.IsNull)continue;
#if TAG033
     // G004: the project's own TAG label is not captured (Snapshot erases it from the copy), so its style is not wanted; foreign attributes still count.
     if(TagCad033.IsOwnedAttribute(tr,ar))continue;
#endif
     var r=(TextStyleTableRecord)tr.GetObject(ar.TextStyleId,OpenMode.ForRead);if(!want.ContainsKey(r.Name))want[r.Name]=LibraryCad06.StyleInfo(r);}tr.Commit();}
   return want;}
  // Before db.WblockCloneObjects(..., side, Ignore): every same-named style of the OWNED side database that differs from the source gets the source record.
  static void PreserveInto(Dictionary<string,LegendStyle> want,Database side,List<string> notes){if(want.Count==0)return;
   using(var tr=side.TransactionManager.StartTransaction()){var st=(TextStyleTable)tr.GetObject(side.TextStyleTableId,OpenMode.ForRead);
    foreach(var kv in want){if(!st.Has(kv.Key))continue;var r=(TextStyleTableRecord)tr.GetObject(st[kv.Key],OpenMode.ForRead);var had=LibraryCad06.StyleInfo(r);if(LibraryCad06.SourceStyleKept(kv.Value,had))continue;
     r.UpgradeOpen();LibraryCad06.Overwrite(r,kv.Value);notes.Add("INFO kiểu chữ "+r.Name+" của bản sao lấy theo nguồn: "+LibraryCad06.Describe(had)+" → "+LibraryCad06.Describe(kv.Value)+".");}tr.Commit();}}
  // Read-back after the clone: the source records are unchanged (exact Meps06.SameStyle) and every style used by the copy keeps its source record.
  static void CheckStyles(Dictionary<string,LegendStyle> want,Database db,ObjectId def,ObjectId refId,Database copy,ObjectId copyDef,string what){
   var now=UsedStyles(db,def,refId);foreach(var kv in want){LegendStyle n;if(!now.TryGetValue(kv.Key,out n)||!Meps06.SameStyle(kv.Value,n))throw new InvalidOperationException("Kiểu chữ nguồn "+kv.Key+" thay đổi trong lúc chép ("+LibraryCad06.Describe(kv.Value)+" → "+LibraryCad06.Describe(n)+"); "+what+".");}
   var diff=LibraryCad06.StyleDiffs(want,copy,copyDef);if(diff.Count>0)throw new InvalidOperationException("Kiểu chữ của bản sao khác nguồn ("+diff[0]+(diff.Count>1?"; +"+(diff.Count-1)+" khác":"")+"); "+what+".");}
  static string Free(BlockTable bt,string name){if(!bt.Has(name))return name;for(int k=2;k<1000;k++){string n=name+k.ToString(CultureInfo.InvariantCulture);if(!bt.Has(n))return n;}throw new InvalidOperationException("Không tìm được tên tạm trong bản chụp.");}
  // ---- capture ----
  // Snapshot of ONE selected reference into an owned side database: a new outer definition (base point = the reference's insertion point) holding a clone
  // of the reference (scale, rotation, attributes, dependencies), plus one reference of the outer definition in the side Model at the origin (lineweight
  // context for LibraryStyle). The user's drawing is only read (WblockCloneObjects from it). MEPS records copied with the clone are removed in the side copy.
  public static void Snapshot(Database db,ObjectId refId,Database side,string name,List<string> notes){string bad=Naming019.Shape(name);if(bad!=null)throw new InvalidOperationException("Tên "+name+": "+bad);
   Point3d at;ObjectId def;using(var tr=db.TransactionManager.StartOpenCloseTransaction()){var src=(BlockReference)tr.GetObject(refId,OpenMode.ForRead);at=src.Position;def=src.BlockTableRecord;tr.Commit();}
   // F02: source text styles (closure + reference-only attribute styles) onto the side database BEFORE the clone; checked again after it.
   var styles=UsedStyles(db,def,refId);PreserveInto(styles,side,notes);
   // PATCH03: layer 0 is never cloned under Ignore (the side database keeps its own), so the OWNED side layer 0 takes the source layer 0 lineweight
   // before the clone; the wrapper reference sits on it. Other layers are cloned normally. The source is only read.
   LineWeight lw0;using(var tr=db.TransactionManager.StartOpenCloseTransaction()){lw0=((LayerTableRecord)tr.GetObject(db.LayerZero,OpenMode.ForRead)).LineWeight;tr.Commit();}
   using(var tr=side.TransactionManager.StartTransaction()){var z=(LayerTableRecord)tr.GetObject(side.LayerZero,OpenMode.ForRead);
    if(z.LineWeight!=lw0){var was=z.LineWeight;z.UpgradeOpen();z.LineWeight=lw0;notes.Add("INFO lớp 0 của bản chụp lấy nét theo nguồn: "+was+" → "+lw0+".");}tr.Commit();}
   side.Insunits=db.Insunits;ObjectId outer;string temp="MEPS019_CAPTURE_"+Guid.NewGuid().ToString("N").Substring(0,12).ToUpperInvariant();
   using(var tr=side.TransactionManager.StartTransaction()){var bt=(BlockTable)tr.GetObject(side.BlockTableId,OpenMode.ForWrite);var b=new BlockTableRecord{Name=temp,Origin=Point3d.Origin};outer=bt.Add(b);tr.AddNewlyCreatedDBObject(b,true);tr.Commit();}
   var map=new IdMapping();db.WblockCloneObjects(new ObjectIdCollection(new[]{refId}),outer,map,DuplicateRecordCloning.Ignore,false);
   if(!map.Contains(refId)||!map[refId].IsCloned)throw new InvalidOperationException("Không chép được block đã chọn vào bản chụp; chưa thêm gì.");var nested=map[refId].Value;
   using(var tr=side.TransactionManager.StartTransaction()){var nb=(BlockReference)tr.GetObject(nested,OpenMode.ForWrite);nb.TransformBy(Matrix3d.Displacement(Point3d.Origin-at));
    // PATCH03: in the source Model a ByBlock reference resolves to LWDEFAULT; inside the wrapper it would inherit the wrapper instead. The OWNED copy
    // gets Default (resolved by LibraryStyle.Freeze with the supplied runtime LWDEFAULT); the source reference is untouched.
    if(nb.LineWeight==LineWeight.ByBlock){nb.LineWeight=LineWeight.ByLineWeightDefault;notes.Add("INFO nét ByBlock của block đã chọn đổi thành Default chỉ trong bản chụp (giữ hình thức như trong Model nguồn).");}
#if TAG033
    // G004: the selected (root) reference's own TAG labels are identified by their marker BEFORE Strip removes it, and erased from the OWNED copy only AFTER
    // Strip/StripClosure, so a project label is never baked into the library symbol. Foreign attributes (even named TVXD_LABEL_033) stay; the source is untouched.
    var labels=new List<ObjectId>();foreach(ObjectId a in nb.AttributeCollection){if(a.IsErased)continue;var ar=tr.GetObject(a,OpenMode.ForRead,false,true) as AttributeReference;
     if(ar!=null&&!ar.IsErased&&TagCad033.IsOwnedAttribute(tr,ar))labels.Add(a);}
#endif
    int stripped=Strip(tr,nb);foreach(ObjectId a in nb.AttributeCollection)stripped+=Strip(tr,tr.GetObject(a,OpenMode.ForWrite,false,true));stripped+=StripClosure(tr,outer,outer);
    if(stripped>0)notes.Add("INFO bỏ "+stripped+" dữ liệu MEPS cũ (dấu thư viện/mã) khỏi bản chụp; bản vẽ gốc giữ nguyên.");
#if TAG033
    foreach(var a in labels){var ar=tr.GetObject(a,OpenMode.ForWrite,false,true);if(!ar.IsErased)ar.Erase();}
    if(labels.Count>0)notes.Add("INFO nhãn TAG hiển thị của dự án ("+labels.Count+") không được chép vào hình ký hiệu thư viện; nhãn trên bản vẽ gốc giữ nguyên.");
#endif
    var bt=(BlockTable)tr.GetObject(side.BlockTableId,OpenMode.ForRead);
    if(bt.Has(name)){var clash=(BlockTableRecord)tr.GetObject(bt[name],OpenMode.ForWrite);string alt=Free(bt,name+"_SRC");clash.Name=alt;notes.Add("INFO định nghĩa lồng cùng tên "+name+" đổi thành "+alt+" chỉ trong bản chụp (bản vẽ gốc giữ nguyên).");}
    ((BlockTableRecord)tr.GetObject(outer,OpenMode.ForWrite)).Name=name;
    var ms=(BlockTableRecord)tr.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(side),OpenMode.ForWrite);var r=new BlockReference(Point3d.Origin,outer);r.SetDatabaseDefaults(side);r.LayerId=side.LayerZero;ms.AppendEntity(r);tr.AddNewlyCreatedDBObject(r,true);
    tr.Commit();}
   CheckStyles(styles,db,def,refId,side,outer,"bản chụp không dùng được, chưa thêm gì");}
  // One writer per library folder (this AutoCAD session and others on the machine); the catalog is re-read inside. 021: the mutex (same name as 019) lives in
  // LibraryLock021 so the tool delete (LibraryDelete021) shares it.
  static IDisposable Lock(string root){return LibraryLock021.Acquire(root);}
  static string Rel(string root,string full){string b=Path.GetFullPath(root).TrimEnd('\\','/')+Path.DirectorySeparatorChar;return full.StartsWith(b,StringComparison.OrdinalIgnoreCase)?full.Substring(b.Length).Replace('\\','/'):full;}
  // Capture → stage → publish. Every name/collision/template/selection check runs before the first file is written. Files: captures019/<name>_<time>_<id>.dwg,
  // then blocks/<id>.dwg + previews/<id>.json (LibraryCad.Build pipeline). Publication (legend.json then catalog.json) under the library lock with backups,
  // read-back and rollback; on any failure the previous catalog/legend are kept and the files already written are reported as unregistered (not shown in
  // the palette, not deleted).
  public static CaptureResult019 Capture(Database db,ObjectId refId,string name,CaptureCells019 cells,string root,LwOptions lw,string drawing){
   if(lw==null)throw new InvalidOperationException("Thiếu LWDEFAULT; chưa thêm gì.");var res=new CaptureResult019();var c=Naming019.Clean(cells);res.blank=c.description.Length==0;
   var catalog=Library.Load(root);var check=Naming019.Check(name,catalog,null);if(!check.ok)throw new InvalidOperationException(check.Message());
   var template=Naming019.FindTemplate(Legend.Load(root));if(template==null)throw new InvalidOperationException("legend.json chưa có bảng ký hiệu mẫu (nguồn có bảng 4 cột đã đọc); thêm nguồn gốc bằng '+ → Thêm từ DWG/DXF' trước. Chưa thêm gì.");
   if(Naming019.Format(Legend.Load(root),template)==null)throw new InvalidOperationException("Bảng mẫu "+template.sourceFile+" không có kiểu chữ ô dùng được; chưa thêm gì.");
   var info=Inspect(db,refId);res.layer=check.layer;res.sourceId=info.markerId??info.identityId;
   string dirRel=Naming019.CaptureDir;Directory.CreateDirectory(Library.SafePath(root,dirRel));
   string stamp=DateTime.UtcNow.ToString("yyyyMMddHHmmssfff",CultureInfo.InvariantCulture)+"_"+Guid.NewGuid().ToString("N").Substring(0,8);
   string rel=dirRel+"/"+name+"_"+stamp+".dwg",file=Library.SafePath(root,rel),temp=Library.SafePath(root,dirRel+"/~"+stamp+".tmp.dwg");
   if(File.Exists(file)||File.Exists(temp))throw new InvalidOperationException("Tệp chụp đã tồn tại; không ghi đè. Chưa thêm gì.");
   try{using(var side=new Database(true,true)){Snapshot(db,refId,side,name,res.notes);side.SaveAs(temp,DwgVersion.Current);}
    File.Move(temp,file);}
   finally{if(File.Exists(temp))File.Delete(temp);}
   res.capture=rel;res.artifacts.Add(rel);
   try{Fail("after-snapshot");string sha=Library.Sha256File(file);string id=Library.EntryId(sha,name);
    if(Library.Reference(catalog,id)!=null||Identity015.IsFrozen(id))throw new InvalidOperationException("ID "+id+" đã dùng (mục đang dùng hoặc đã xoá); không ghi đè.");
    string blocks=Library.BlocksDir+"/"+id+".dwg",previews=Library.PreviewsDir+"/"+id+".json";LibEntry e;
    try{using(var src=LibraryCad.OpenSource(file,Library.SafePath(root,"logs")))e=LibraryCad.BuildSelected019(root,src,Def(src,name),name,sha,Path.GetFileName(file),lw);}
    finally{foreach(var p in new[]{blocks,previews})if(File.Exists(Library.SafePath(root,p)))res.artifacts.Add(p);}
    if(Library.Sha256File(file)!=sha)throw new InvalidOperationException("Tệp chụp đã thay đổi trong lúc dựng mục; chưa đăng ký.");
    if(e.status!="OK")throw new InvalidOperationException("Mục chưa được hỗ trợ: "+string.Join("; ",e.warnings.Where(w=>w.StartsWith("UNSUPPORTED")).Take(3))+". Chưa đăng ký.");
    var issue=Meps05.PlaceIssue(e);if(issue!=null)throw new InvalidOperationException(issue+" Chưa đăng ký.");
    e.warnings.Add("INFO "+Naming019.Parser+": chụp 1 tham chiếu block "+info.effectiveName+" (handle "+info.handle+") từ Model của "+(drawing??"bản vẽ hiện tại")+", INSUNITS "+db.Insunits+"; tỉ lệ, góc xoay, thuộc tính và khối lồng giữ như lúc chụp; bản vẽ gốc không bị sửa.");
    e.warnings.AddRange(res.notes.Where(n=>n.Length<=2000));res.code=Identity015.BlockCode(e);Fail("after-build");
    Publish(root,e,c,db.Insunits.ToString(),res);res.entry=e;res.artifacts.Clear();return res;}
   catch(Exception ex){var rb=ex as Rollback019;
    // F03: only a failure before the first catalog/legend write, or a rollback that read both files back equal, may say the library is unchanged.
    string lib=rb==null?"catalog/legend chưa bị ghi (lỗi trước khi đăng ký).":rb.restored?"catalog/legend đã khôi phục và đọc lại khớp bản trước.":"CẢNH BÁO: catalog/legend CÓ THỂ KHÔNG còn như trước — xem kết quả khôi phục và bản sao lưu ở trên; kiểm tra thư viện trước khi dùng tiếp.";
    throw new InvalidOperationException(ex.Message+" "+lib+" Tệp đã tạo nhưng KHÔNG đăng ký, không hiện trong bảng: "+string.Join(", ",res.artifacts)+".",ex);}}
  static string Read(string path){return File.Exists(path)?File.ReadAllText(path,Encoding.UTF8):null;}
  static string Backup(string path,string text,string stamp){if(text==null)return null;string b=path+".019bak_"+stamp;using(var f=new FileStream(b,FileMode.CreateNew,FileAccess.Write))using(var w=new StreamWriter(f,new UTF8Encoding(false)))w.Write(text);return b;}
  // Restores a file to its text before publication; a file that did not exist before is moved aside (kept, not deleted).
  static string Restore(string path,string text,string stamp){if(text!=null){Library.WriteAtomic(path,text);return Path.GetFileName(path)+" khôi phục";}if(File.Exists(path)){File.Move(path,path+".019failed_"+stamp);return Path.GetFileName(path)+" mới tạo được đổi tên .019failed_"+stamp;}return Path.GetFileName(path)+" không đổi";}
  // F03: one file restored independently of the other (its own try), then read back: ok only when the text equals the text before publication (or the file
  // is absent again when it did not exist before).
  static string RestoreChecked(string path,string text,string stamp,out bool ok){ok=false;string done;
   try{done=Restore(path,text,stamp);}catch(Exception r){return Path.GetFileName(path)+" CHƯA khôi phục được ("+r.Message+")";}
   try{string now=Read(path);ok=text==null?now==null:string.Equals(now,text,StringComparison.Ordinal);}catch(Exception r){return done+", nhưng KHÔNG đọc lại được ("+r.Message+")";}
   return ok?done+" (đọc lại khớp bản trước)":done+", nhưng đọc lại KHÁC bản trước";}
  // Publication failed after a write; restored = both files read back equal to their previous text.
  sealed class Rollback019:InvalidOperationException {public readonly bool restored;public Rollback019(string message,bool restored,Exception inner):base(message,inner){this.restored=restored;}}
  static void Publish(string root,LibEntry e,CaptureCells019 cells,string captureUnits,CaptureResult019 res){
   using(Lock(root)){var catalog=Library.Load(root);var check=Naming019.Check(e.name,catalog,null);if(!check.ok)throw new InvalidOperationException(check.Message());
    if(Library.Reference(catalog,e.id)!=null)throw new InvalidOperationException("ID "+e.id+" đã có trong catalog (mục đang dùng hoặc đã xoá); không ghi đè.");
    var map=Legend.Load(root);var template=Naming019.FindTemplate(map);Naming019.AddCaptureLegend(map,template,e,cells,captureUnits);
    catalog.entries.Add(e);Library.Validate(catalog);var j=Library.Json();Library.Validate(j.Deserialize<LibCatalog>(j.Serialize(catalog)));
    string catPath=Library.SafePath(root,Library.CatalogFile),legPath=Library.SafePath(root,Legend.File),stamp=DateTime.UtcNow.ToString("yyyyMMddHHmmssfff",CultureInfo.InvariantCulture);
    string catText=Read(catPath),legText=Read(legPath);foreach(var b in new[]{Backup(catPath,catText,stamp),Backup(legPath,legText,stamp)})if(b!=null)res.backups.Add(Rel(root,b));
    try{Fail("before-legend");Legend.Save(root,map);Fail("before-catalog");Library.Save(root,catalog);Fail("after-catalog");
     // read-back: same file hash, a valid non-frozen type code, a non-blocking source-backed legend decision
     var again=Library.Load(root);var got=again.entries.Single(x=>x.id==e.id);if(Library.Sha256File(Library.SafePath(root,got.file))!=got.fileSha256)throw new InvalidOperationException("Đọc lại: hash file thư viện khác catalog.");
     if(Identity015.IsFrozen(got.id)||Identity015.BlockCode(got)!=res.code)throw new InvalidOperationException("Đọc lại: mã loại không khớp.");
     var d=Legend.Decide(new UsedSymbol{entryId=got.id,name=got.name,sourceSha256=got.sourceSha256,count=1},Legend.Load(root),x=>again.entries.FirstOrDefault(q=>q.id==x));
     if(d.blocking||d.layout!=Legend.SourceRow)throw new InvalidOperationException("Đọc lại: bảng ký hiệu chưa xuất được cho mục mới ("+d.status+" — "+d.reason+").");}
    catch(Exception ex){bool legOk,catOk;string undo=RestoreChecked(legPath,legText,stamp,out legOk)+"; "+RestoreChecked(catPath,catText,stamp,out catOk);bool ok=legOk&&catOk;
     undo=(ok?"đã khôi phục legend/catalog: ":"CHƯA khôi phục đầy đủ legend/catalog: ")+undo+"; bản sao lưu: "+(res.backups.Count>0?string.Join(", ",res.backups):"không có (tệp chưa tồn tại trước đó)");
     throw new Rollback019(ex.Message+" ("+undo+")",ok,ex);}}}
  // ---- remove from Model (021: resolve here, then the actual tool delete LibraryDelete021.Delete in ModelLibraryUI019.RunRemove) ----
  // The entry of a placed symbol, resolved ONLY from its MEPS marker (definition) and/or identity record (reference): both must agree, name and source hash
  // must match the current catalog entry. Foreign/unlinked blocks and blocks of another library are refused (nothing changes).
  public static LibEntry ResolveForRemove(Database db,ObjectId refId,LibCatalog catalog,List<string> notes){var s=Inspect(db,refId,false);
   if(s.identityIssue!=null&&s.markerId==null)throw new InvalidOperationException("Dữ liệu mã trên khối lỗi ("+s.identityIssue+"); không gỡ gì.");
   if(s.markerId==null&&s.identityId==null)throw new InvalidOperationException("Khối "+s.effectiveName+" không mang dấu thư viện MEPS (khối ngoài hoặc chưa liên kết); MEPS không đoán theo tên, không gỡ gì.");
   if(s.markerId!=null&&s.identityId!=null&&s.markerId!=s.identityId)throw new InvalidOperationException("Dấu định nghĩa ("+s.markerId+") khác mã trên tham chiếu ("+s.identityId+"); không gỡ gì.");
   string id=s.markerId??s.identityId;var e=catalog.entries.FirstOrDefault(x=>x.id==id);
   if(e==null&&catalog.deleted!=null&&catalog.deleted.Any(x=>x!=null&&x.id==id))throw new InvalidOperationException("Khối thuộc mục "+id+" đã được xoá khỏi công cụ trước đó; không thay đổi gì.");
   if(e==null)throw new InvalidOperationException("Khối thuộc mục "+id+" không có trong thư viện hiện hành "+Meps05.LibraryDir+" (thư viện khác/không rõ); không thay đổi gì.");
   if(s.markerName!=null&&!Same(s.markerName,e.name))throw new InvalidOperationException("Tên trong dấu khối ("+s.markerName+") khác mục "+e.name+"; không gỡ gì.");
   if(!string.IsNullOrEmpty(s.markerSource)&&!Same(s.markerSource,e.sourceSha256))throw new InvalidOperationException("Nguồn trong dấu khối khác nguồn của mục "+e.name+" (thư viện khác); không gỡ gì.");
   if(s.identityId!=null&&(!Same(s.alias,e.name)||(!string.IsNullOrEmpty(s.identitySource)&&!Same(s.identitySource,e.sourceSha256))))throw new InvalidOperationException("Mã trên khối không khớp mục "+e.name+" của thư viện hiện hành; không gỡ gì.");
   if(notes!=null&&s.markerFile!=null&&s.markerFile!=e.fileSha256)notes.Add("INFO khối được đặt từ bản chép cũ hơn của cùng mục (hash file khác); vẫn là mục "+e.id+".");
   return e;}
  // Old 019/020 hide (catalog hidden flag only, under the library lock). Since 021 the remove route deletes instead; kept only for callers of the old API.
  public static void Hide(string root,string id){using(Lock(root))Library.SetHidden(root,id,true);}
  // ---- editable copy ----
  // One independent, valid, unused definition newName (copy of the selected reference's definition; every non-anonymous nested definition copied under
  // newName$N<k>, so nothing mutable is shared with the original) and one reference at 'at' with the original's transform, properties and attribute values.
  // The original definition and reference are only read. Refused (nothing written): invalid/used name, dynamic/anonymous/xref/proxy/MINSERT, multiline
  // attribute, an attribute without definition, a shape difference. The reference/attributes are written in one transaction that any failure aborts; the
  // definitions written by WblockCloneObjects survive that abort (native QA 019), so a failure also runs Uncopy and reports its verified result.
  static HashSet<ObjectId> BlockIds(Database db){var s=new HashSet<ObjectId>();using(var tr=db.TransactionManager.StartOpenCloseTransaction()){foreach(ObjectId id in (BlockTable)tr.GetObject(db.BlockTableId,OpenMode.ForRead))s.Add(id);tr.Commit();}return s;}
  // F07: rollback of a failed copy, after its transaction is aborted and disposed. Erases ONLY block definitions that did not exist before the copy (id not in
  // beforeIds) AND were made by it: mapped from the side closure in 'back' (also a partial mapping) or named newName / newName$N<k> (names checked unused
  // before). Nothing is purged; pre-existing records and layers/styles/linetypes the clone may have added are never erased. If a candidate is referenced from
  // outside the candidates nothing is erased. The text says "unchanged" only when LegendPlace011.Snapshot (live names, entity counts, origins, tables,
  // Model, NOD) equals the state before the copy; otherwise it is an explicit incomplete-rollback warning.
  static string Uncopy(Database db,HashSet<string> before,HashSet<ObjectId> beforeIds,List<ObjectId> closure,IdMapping back,HashSet<string> given){
   var gone=new List<string>();var shared=new List<string>();
   try{using(var tr=db.TransactionManager.StartTransaction()){var mapped=new HashSet<ObjectId>();
     foreach(var bid in closure){if(!back.Contains(bid))continue;var p=back[bid];if(p.IsCloned&&!p.Value.IsNull)mapped.Add(p.Value);}
     var own=new List<ObjectId>();
     foreach(ObjectId id in (BlockTable)tr.GetObject(db.BlockTableId,OpenMode.ForRead)){if(id.IsErased||beforeIds.Contains(id))continue;var b=(BlockTableRecord)tr.GetObject(id,OpenMode.ForRead);
      if(b.IsLayout||b.IsFromExternalReference||b.IsFromOverlayReference||b.IsDependent)continue;if(mapped.Contains(id)||(!b.IsAnonymous&&given.Contains(b.Name)))own.Add(id);}
     var set=new HashSet<ObjectId>(own);
     foreach(var id in own){var b=(BlockTableRecord)tr.GetObject(id,OpenMode.ForRead);foreach(ObjectId r in b.GetBlockReferenceIds(true,false)){if(r.IsErased)continue;var o=tr.GetObject(r,OpenMode.ForRead);if(!set.Contains(o.OwnerId)){shared.Add(b.Name);break;}}}
     if(shared.Count==0){var ents=new List<ObjectId>();foreach(var id in own)foreach(ObjectId e in (BlockTableRecord)tr.GetObject(id,OpenMode.ForRead))if(!e.IsErased)ents.Add(e);
      foreach(var e in ents)tr.GetObject(e,OpenMode.ForWrite,false,true).Erase();
      foreach(var id in own){var b=(BlockTableRecord)tr.GetObject(id,OpenMode.ForWrite);gone.Add(b.Name);b.Erase();}}
     tr.Commit();}}
   catch(Exception ce){return "CẢNH BÁO: CHƯA hoàn tác được định nghĩa vừa chép ("+ce.Message+"); bản vẽ CÓ THỂ còn khối "+string.Join(", ",given.Take(8))+" — kiểm tra bản vẽ và dùng UNDO nếu cần.";}
   HashSet<string> after;try{after=LegendPlace011.Snapshot(db);}catch(Exception ce){return "CẢNH BÁO: CHƯA xác nhận được việc hoàn tác ("+ce.Message+"); kiểm tra bản vẽ và dùng UNDO nếu cần.";}
   var left=after.Where(x=>!before.Contains(x)).ToList();var lost=before.Where(x=>!after.Contains(x)).ToList();
   string did=shared.Count>0?"không gỡ định nghĩa vừa chép vì còn được tham chiếu từ ngoài bản sao ("+string.Join(", ",shared.Take(8))+")":gone.Count>0?"đã gỡ "+gone.Count+" định nghĩa vừa chép ("+string.Join(", ",gone.Take(8))+(gone.Count>8?" …":"")+")":"không có định nghĩa nào đã chép";
   if(shared.Count==0&&left.Count==0&&lost.Count==0)return "Bản vẽ giữ nguyên: "+did+" và đọc lại khớp trạng thái trước khi chép (khối, số đối tượng, layer, kiểu nét, kiểu chữ, Model, NOD).";
   return "CẢNH BÁO: CHƯA hoàn tác hết bản sao — "+did+"; còn "+(left.Count==0?"(không)":string.Join(", ",left.Take(8)))+(lost.Count>0?"; khác trước: "+string.Join(", ",lost.Take(8)):"")+"; kiểm tra bản vẽ và dùng UNDO nếu cần.";}
  public static CopyResult019 EditableCopy(Database db,ObjectId refId,string newName,Point3d at,LibCatalog catalog){
   if(!db.UndoRecording)throw new InvalidOperationException("Bản vẽ đang tắt ghi UNDO; chưa tạo bản sao.");if(!Core.Finite(at.X)||!Core.Finite(at.Y)||!Core.Finite(at.Z))throw new InvalidOperationException("Điểm đặt không hợp lệ.");
   var names=BlockNames(db);var check=Naming019.Check(newName,catalog,names);if(!check.ok)throw new InvalidOperationException(check.Message());
   var s=Inspect(db,refId);if(s.dynamic)throw new InvalidOperationException("Khối động chưa tạo được bản sao sửa (trạng thái động không chép an toàn); dùng '+ → Thêm block từ Model' để chụp trạng thái đang thấy. Chưa thay đổi gì.");
   if(s.anonymous)throw new InvalidOperationException("Khối ẩn danh chưa tạo được bản sao sửa; chưa thay đổi gì.");
   ObjectId rootDef;using(var tr=db.TransactionManager.StartOpenCloseTransaction()){var br=(BlockReference)tr.GetObject(refId,OpenMode.ForRead);rootDef=br.BlockTableRecord;
    foreach(ObjectId a in br.AttributeCollection){var ar=tr.GetObject(a,OpenMode.ForRead,false,true) as AttributeReference;if(ar!=null&&ar.IsMTextAttribute)throw new InvalidOperationException("Thuộc tính nhiều dòng "+ar.Tag+" chưa hỗ trợ trong bản sao sửa; MEPS không bỏ thuộc tính. Chưa thay đổi gì.");}tr.Commit();}
   var res=new CopyResult019{name=newName};
   // F02: both clones (drawing → side → drawing) use Ignore; source text styles are put into the side database first and read back after each clone.
   var styles=UsedStyles(db,rootDef,ObjectId.Null);
   using(var side=new Database(true,true)){PreserveInto(styles,side,res.notes);var map=new IdMapping();db.WblockCloneObjects(new ObjectIdCollection(new[]{rootDef}),side.BlockTableId,map,DuplicateRecordCloning.Ignore,false);var sideRoot=map[rootDef].Value;var closure=new List<ObjectId>();
    var given=new HashSet<string>(StringComparer.OrdinalIgnoreCase){newName};
    using(var tr=side.TransactionManager.StartTransaction()){closure=LibraryCad.Closure(tr,sideRoot);int stripped=StripClosure(tr,sideRoot,ObjectId.Null);if(stripped>0)res.notes.Add("Bỏ "+stripped+" dữ liệu MEPS cũ khỏi bản sao.");
     var taken=new HashSet<string>(names,StringComparer.OrdinalIgnoreCase);taken.Add(newName);int k=0;
     foreach(var bid in closure){var b=(BlockTableRecord)tr.GetObject(bid,OpenMode.ForWrite);if(b.IsAnonymous)continue;if(bid==sideRoot){b.Name=newName;continue;}string n;do{k++;n=newName+"$N"+k.ToString(CultureInfo.InvariantCulture);}while(taken.Contains(n));taken.Add(n);given.Add(n);b.Name=n;}
     tr.Commit();}
    CheckStyles(styles,db,rootDef,ObjectId.Null,side,sideRoot,"chưa thay đổi gì");
    // F07: state before the copy (live records + ids of every block definition) for the owned-clone rollback in the catch below.
    var before=LegendPlace011.Snapshot(db);var beforeIds=BlockIds(db);var back=new IdMapping();
    var outer=db.TransactionManager.StartTransaction();
    try{side.WblockCloneObjects(new ObjectIdCollection(new[]{sideRoot}),db.BlockTableId,back,DuplicateRecordCloning.Ignore,false);
     foreach(var bid in closure)if(!back.Contains(bid)||!back[bid].IsCloned)throw new InvalidOperationException("Một định nghĩa lồng không được chép riêng (sẽ dùng chung với bản gốc); huỷ.");
     var def=back[sideRoot].Value;Fail("copy-after-clone");
     var src=(BlockReference)outer.GetObject(refId,OpenMode.ForRead);var ms=(BlockTableRecord)outer.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db),OpenMode.ForWrite);
     var move=at-src.Position;var nb=new BlockReference(src.Position,def);nb.SetDatabaseDefaults(db);nb.SetPropertiesFrom(src);nb.BlockTransform=Matrix3d.Displacement(move)*src.BlockTransform;
     var id=ms.AppendEntity(nb);outer.AddNewlyCreatedDBObject(nb,true);
     var olds=new List<AttributeReference>();foreach(ObjectId a in src.AttributeCollection){var ar=outer.GetObject(a,OpenMode.ForRead,false,true) as AttributeReference;if(ar!=null&&!ar.IsErased)olds.Add(ar);}var used=new bool[olds.Count];
     var rec=(BlockTableRecord)outer.GetObject(def,OpenMode.ForRead);
     foreach(ObjectId aid in rec){var ad=outer.GetObject(aid,OpenMode.ForRead) as AttributeDefinition;if(ad==null||ad.Constant)continue;var ar=new AttributeReference();ar.SetAttributeFromBlock(ad,nb.BlockTransform);
      int i=-1;for(int q=0;q<olds.Count&&i<0;q++)if(!used[q]&&Same(olds[q].Tag,ad.Tag))i=q;
      if(i>=0){used[i]=true;var o=olds[i];ar.SetPropertiesFrom(o);ar.TextStyleId=o.TextStyleId;ar.Height=o.Height;ar.WidthFactor=o.WidthFactor;ar.Oblique=o.Oblique;ar.Rotation=o.Rotation;ar.Normal=o.Normal;ar.IsMirroredInX=o.IsMirroredInX;ar.IsMirroredInY=o.IsMirroredInY;
       ar.HorizontalMode=o.HorizontalMode;ar.VerticalMode=o.VerticalMode;ar.Position=o.Position+move;if(!o.IsDefaultAlignment)ar.AlignmentPoint=o.AlignmentPoint+move;ar.TextString=o.TextString;ar.Invisible=o.Invisible;}
      else ar.TextString=ad.TextString;
      nb.AttributeCollection.AppendAttribute(ar);outer.AddNewlyCreatedDBObject(ar,true);}
     for(int i=0;i<olds.Count;i++)if(!used[i])throw new InvalidOperationException("Thuộc tính "+olds[i].Tag+" trên khối gốc không có định nghĩa tương ứng; MEPS không bỏ thuộc tính. Chưa tạo bản sao.");
     Fail("copy-after-reference");
     var diff=LibraryCad.VerifyShape(db,rootDef,db,def);if(diff.Count>0)throw new InvalidOperationException("Bản sao khác hình định nghĩa gốc ("+diff[0]+"); chưa tạo bản sao.");
     CheckStyles(styles,db,rootDef,ObjectId.Null,db,def,"chưa tạo bản sao");
     outer.Commit();res.reference=id;res.definition=def;}
    catch(Exception ex){if(!outer.IsDisposed){outer.Abort();outer.Dispose();}
     throw new InvalidOperationException(ex.Message+" "+Uncopy(db,before,beforeIds,closure,back,given),ex);}
    finally{if(!outer.IsDisposed)outer.Dispose();}}
   res.notes.Add("Định nghĩa mới "+newName+" (khối lồng chép riêng tên "+newName+"$N…); khối gốc và định nghĩa gốc giữ nguyên.");return res;}
 }
#endif
}
#endif
