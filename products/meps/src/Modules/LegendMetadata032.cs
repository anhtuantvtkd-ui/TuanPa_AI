using System;using System.IO;using System.Linq;using System.Collections.Generic;using System.Globalization;using System.Text;using System.Text.RegularExpressions;using System.Security.Cryptography;
#if METADATA032 && !MODELLIB019
#error METADATA032 needs MODELLIB019 (Naming019 cell cleaning, MText escaping and template text format; LibraryDelete021 lock and identity check).
#endif
#if METADATA032
namespace Tvxd.Meps {
 // TVXD-MEPS-METADATA-032: the user edits DIỄN GIẢI / QUY CÁCH / GHI CHÚ of an EXISTING library entry for legends exported later, without recapture or
 // rename. Pure .NET (Core, Library, Legend, Naming019, LibraryDelete021; no CAD or WinForms types), so it is testable without AutoCAD.
 //  - Storage: metadata032.json in the library root (UTF-8, atomic write), separate from legend.json, catalog.json and block files, which are never written
 //    here. One record per entry: entry ID + source hash + name (fingerprint), three cells, revision and time. A cell null = no override (the source text
 //    stays, also after a source re-read); "" = blank on purpose; other = literal plain text. An entry without a record changes nothing.
 //  - Legend.Load reads and validates the file on every fresh read: unsupported schema/version, duplicate ID, bad fingerprint or cell = clear error before any
 //    export or save (no silent loss). An absent file = empty overlay (old libraries unchanged).
 //  - Legend.Decide: only after a non-blocking source decision, Apply replaces the overridden cells of that entry. Other entries are untouched; a source row
 //    shared with other symbols is split by PlanForExport (different CellKey). Text format (style, layer, height) = the source cell's, else Naming019.Format.
 //  - Save: under LibraryLock021 the entry identity (LibraryDelete021.IdentityIssue) and the file hash must still be those of the snapshot the dialog showed;
 //    backup copy, atomic write, read-back of the file and of the effective decision. Any failure after the write restores the previous bytes (or removes the
 //    new file) and says whether that worked. Deleting an entry from the tool keeps its record, so legends of symbols already placed still use it.
 public sealed class MetadataEntry032 {public string entryId,sourceSha256,name,description,spec,note,updatedUtc;public int revision;}
 public sealed class MetadataFile032 {public string schema=LegendMetadata032.Schema;public int version=LegendMetadata032.Version,revision;public List<MetadataEntry032> entries=new List<MetadataEntry032>();}
 // Runtime overlay (LegendMap.metadata032). sha256 = hash of the bytes read; null (present = false) when the file is absent.
 public sealed class MetadataOverlay032 {public MetadataFile032 file=new MetadataFile032();public string sha256;public bool present;
  public MetadataEntry032 Find(string entryId){return file.entries.FirstOrDefault(x=>string.Equals(x.entryId,entryId,StringComparison.Ordinal));}}
 // What the dialog shows for one entry: source[k] = cell of the unchanged source decision, effective[k] = cell exported now, overridden[k] = the record sets it.
 public sealed class MetadataSnapshot032 {public string root,fileSha256,sourceStatus,sourceReason,effectiveStatus,effectiveReason,layout;public int fileRevision;public LibEntry entry;public MetadataEntry032 current;
  public string[] source=new string[3],effective=new string[3];public bool[] overridden=new bool[3];}
 public sealed class MetadataResult032 {public string action,entryId,name,status,backup;public int revision,fileRevision;public string[] effective=new string[3];
  public string Summary(){
   if(action==LegendMetadata032.NoChange)return "Không có thay đổi thông tin bảng ký hiệu của "+name+"; chưa ghi gì.";
   string tail=" Chỉ bảng ký hiệu TẠO MỚI từ thư viện này dùng nội dung này; bảng đã đặt trong bản vẽ không đổi. Đây là ghi file thư viện: UNDO của CAD không hoàn tác."+(backup!=null?" Sao lưu: "+backup+".":"");
   if(action==LegendMetadata032.Reset)return "Đã dùng lại nội dung gốc cho "+name+" (mục "+entryId+"): bỏ phần sửa tay, bảng ký hiệu lấy chữ từ bảng nguồn."+tail;
   return "Đã lưu thông tin bảng ký hiệu của "+name+" (mục "+entryId+", lần sửa "+revision+"; trạng thái khi tạo bảng: "+status+")."+tail;}}
 public static class LegendMetadata032 {
  public const string FileName="metadata032.json",Schema="TVXD_MEPS_METADATA_032",Invalid="METADATA032_INVALID",Saved="SAVED",Reset="RESET",NoChange="NO_CHANGE";
  public const int Version=1;
  // QA failure injection: "before-write" (nothing written), "after-write" (written, then rolled back), "after-readback" (read back, then rolled back).
  public static string FailAt;
  static void Fail(string stage){if(FailAt!=null&&string.Equals(FailAt,stage,StringComparison.Ordinal))throw new InvalidOperationException("QA: lỗi giả lập tại "+stage+".");}
  static readonly Regex Id=new Regex("^[0-9a-f]{12}$"),Sha=new Regex("^[0-9a-fA-F]{64}$");
  static readonly UTF8Encoding Strict=new UTF8Encoding(false,true);
  static bool Same(string a,string b){return string.Equals(a,b,StringComparison.OrdinalIgnoreCase);}
  static string Hash(byte[] b){using(var h=SHA256.Create())return string.Concat(h.ComputeHash(b).Select(x=>x.ToString("x2",CultureInfo.InvariantCulture)));}
  static T Copy<T>(T x){var j=Library.Json();return j.Deserialize<T>(j.Serialize(x));}
  static string Short(string s){return s==null?"null":s.Length>24?s.Substring(0,24)+"…":s;}
  static UsedSymbol Used(LibEntry e){return new UsedSymbol{entryId=e.id,name=e.name,sourceSha256=e.sourceSha256,count=1};}
  // Cell k of a record (0 DIỄN GIẢI, 1 QUY CÁCH, 2 GHI CHÚ); null = not overridden.
  public static string Field(MetadataEntry032 e,int k){return e==null?null:k==0?e.description:k==1?e.spec:e.note;}
  // One user cell: Naming019.CleanCell (CRLF/CR → LF, tab → space, trimmed, ≤ 500 characters, no control character except LF) plus well-formed UTF-16 (no
  // lone surrogate, no U+FFFE/U+FFFF). Unicode (Vietnamese) text is kept literally: nothing normalised, translated or formatted.
  public static string Clean(string s,string what){string c=Naming019.CleanCell(s,what);
   for(int i=0;i<c.Length;i++){char ch=c[i];if(char.IsHighSurrogate(ch)&&i+1<c.Length&&char.IsLowSurrogate(c[i+1])){i++;continue;}
    if(char.IsSurrogate(ch)||ch==(char)0xFFFE||ch==(char)0xFFFF)throw new InvalidOperationException("Ô "+what+" có ký tự Unicode không hợp lệ (vị trí "+(i+1)+").");}
   return c;}
  // F032-01: MText contents that DISPLAY the user's text literally. Naming019.MTextContents escapes \ { } and LF; MText still turns %%d %%c %%p into ° ∅ ±
  // (native probe TESTS/METADATA032/R3/PERCENT_PROBE.txt), so every '%' becomes the MText Unicode escape \U+0025. Metadata cells only; shared Naming019 and
  // source-table formatting are unchanged.
  public static string LiteralContents(string text){return Naming019.MTextContents(text).Replace("%",@"\U+0025");}
  static Exception Bad(string why){return new InvalidOperationException(FileName+" không hợp lệ: "+why+". Không dùng nội dung đã sửa, chưa ghi gì; khôi phục file từ bản sao "+FileName+".032bak_… (hoặc .bak) trong thư mục thư viện.");}
  public static void Validate(MetadataFile032 f){
   if(f==null||f.schema!=Schema)throw Bad("không đúng schema "+Schema);
   if(f.version!=Version)throw Bad("phiên bản "+f.version+" chưa được hỗ trợ (bản này đọc phiên bản "+Version+")");
   if(f.revision<0||f.entries==null)throw Bad("thiếu danh sách mục hoặc số lần sửa âm");
   var seen=new HashSet<string>(StringComparer.Ordinal);
   foreach(var e in f.entries){if(e==null)throw Bad("có mục rỗng");
    if(e.entryId==null||!Id.IsMatch(e.entryId))throw Bad("ID mục không hợp lệ ("+Short(e.entryId)+")");
    if(!seen.Add(e.entryId))throw Bad("trùng ID mục "+e.entryId);
    if(e.sourceSha256==null||!Sha.IsMatch(e.sourceSha256))throw Bad("mục "+e.entryId+": hash nguồn không hợp lệ");
    try{Library.ValidName(e.name);}catch(Exception ex){throw Bad("mục "+e.entryId+": "+ex.Message);}
    if(e.revision<1||string.IsNullOrEmpty(e.updatedUtc)||e.updatedUtc.Length>40)throw Bad("mục "+e.entryId+": thiếu số lần sửa hoặc thời điểm sửa");
    if(Enumerable.Range(0,3).All(k=>Field(e,k)==null))throw Bad("mục "+e.entryId+" không có ô nào");
    for(int k=0;k<3;k++){string v=Field(e,k);if(v==null)continue;string c;
     try{c=Clean(v,Legend.CellNames[k]);}catch(Exception ex){throw Bad("mục "+e.entryId+": "+ex.Message);}
     if(!string.Equals(c,v,StringComparison.Ordinal))throw Bad("mục "+e.entryId+", ô "+Legend.CellNames[k]+" chưa chuẩn hoá (khoảng trắng đầu/cuối, tab hoặc CR)");}}}
  // Fresh read of the sidecar (Legend.Load calls it every time). Strict UTF-8; a leading BOM is accepted.
  public static MetadataOverlay032 Read(string root){string p=Library.SafePath(root,FileName);var o=new MetadataOverlay032();if(!File.Exists(p))return o;
   byte[] bytes=File.ReadAllBytes(p);MetadataFile032 f;
   try{string text=Strict.GetString(bytes);if(text.Length>0&&text[0]==(char)0xFEFF)text=text.Substring(1);f=Library.Json().Deserialize<MetadataFile032>(text);}
   catch(Exception ex){throw Bad("không đọc được UTF-8/JSON ("+ex.Message+")");}
   Validate(f);o.file=f;o.sha256=Hash(bytes);o.present=true;return o;}
  // A source cell whose own text format can carry the user's text: a style recorded for that source and a positive finite height.
  static bool Usable(LegendCell c,LegendSource s){return c!=null&&!string.IsNullOrEmpty(c.style)&&s.styles!=null&&s.styles.Any(x=>x!=null&&Same(x.name,c.style))&&c.height>0&&Core.Finite(c.height);}
  static void Block(ExportDecision d,string why){d.blocking=true;d.status=Invalid;d.reason=why+"; không tạo bảng với nội dung chưa chắc chắn (chuột phải ký hiệu → Sửa thông tin bảng ký hiệu… để xem/sửa).";}
  // Called by Legend.Decide after a NON-blocking source decision. Never touches the source cells (new LegendCell objects); never writes text for a cell the
  // user did not set. Status: all three cells set, or DIỄN GIẢI set on an OK / MISSING_DESCRIPTION / NOT_IN_TABLE decision → OK when DIỄN GIẢI has text,
  // else MISSING_DESCRIPTION (NOT_IN_TABLE kept when only other cells are set); an AMBIGUOUS decision with uncertain cells left stays AMBIGUOUS.
  public static void Apply(ExportDecision d,LegendMap map){if(d==null||d.blocking||d.entry==null||map==null||map.metadata032==null)return;
   var m=map.metadata032.Find(d.entry.id);if(m==null)return;
   if(!Same(m.sourceSha256,d.entry.sourceSha256)||!Same(m.name,d.entry.name)){Block(d,FileName+": bản ghi mục "+m.entryId+" mang tên/nguồn "+m.name+" khác mục thư viện "+d.entry.name);return;}
   var src=map.sources.FirstOrDefault(x=>Same(x.sourceSha256,d.entry.sourceSha256));if(src==null){Block(d,"thiếu bảng nguồn của "+d.entry.name);return;}
   var v=new[]{m.description,m.spec,m.note};var cells=new LegendCell[3];LegendCell fmt=null;bool fmtRead=false;var manual=new List<string>();var userBlank=new List<string>();
   for(int k=0;k<3;k++){if(v[k]==null){cells[k]=d.cells[k];continue;}manual.Add(Legend.CellNames[k]);
    if(v[k].Length==0){cells[k]=new LegendCell();userBlank.Add(Legend.CellNames[k]);continue;}
    var f=Usable(d.cells[k],src)?d.cells[k]:null;if(f==null){if(!fmtRead){fmt=Naming019.Format(map,src);fmtRead=true;}f=fmt;}
    if(f==null){Block(d,"bảng nguồn "+src.sourceFile+" không có kiểu chữ ô dùng được cho nội dung đã sửa ở ô "+Legend.CellNames[k]);return;}
    cells[k]=new LegendCell{text=v[k],contents=LiteralContents(v[k]),style=f.style,layer=f.layer,height=f.height};}
   string before=d.status;bool all=v.All(x=>x!=null),descNow=cells[0].contents.Length>0;
   if(all||(v[0]!=null&&(before==Legend.Ok||before==Legend.MissingDescription||before==Legend.NotInTable)))d.status=descNow?Legend.Ok:(all||before==Legend.Ok?Legend.MissingDescription:before);
   d.cells=cells;d.blanks=new List<string>();for(int k=0;k<3;k++)if(cells[k].contents.Length==0)d.blanks.Add(Legend.CellNames[k]);
   string where=d.layout==Legend.SourceRow?"hàng nguồn "+(d.row+1):"dòng riêng sau các dòng nguồn, hướng "+(d.pose!=null&&d.pose.origin==Legend.OriginLibrary?"theo định nghĩa thư viện":"theo bảng nguồn");
   d.reason=where+"; ô do người dùng sửa trong thư viện ("+FileName+", lần "+m.revision+", "+m.updatedUtc+"): "+string.Join(", ",manual)+(userBlank.Count>0?" (để trống có chủ ý: "+string.Join(", ",userBlank)+")":"")+
    "; ô để trống: "+(d.blanks.Count==0?"(không)":string.Join(", ",d.blanks))+" (MEPS không tự viết); trạng thái nguồn "+before;}
  // Active entry the user picked, unchanged since (same check as LibraryDelete021); a deleted or changed entry is refused with nothing written.
  static LibEntry Active(LibCatalog c,LibEntry want){var now=c.entries.FirstOrDefault(x=>x!=null&&x.id==want.id);
   if(now==null&&(c.deleted??new List<LibEntry>()).Any(x=>x!=null&&x.id==want.id))throw new InvalidOperationException(want.name+" đã bị xoá khỏi công cụ; không sửa thông tin của mục đã xoá (nội dung đã lưu trước đó vẫn được giữ cho bảng ký hiệu của ký hiệu đã đặt). Chưa thay đổi gì.");
   string issue=LibraryDelete021.IdentityIssue(want,now);if(issue!=null)throw new InvalidOperationException("Không sửa "+want.name+" (mục "+want.id+"): "+issue+" sau khi bạn chọn. Tải lại thư viện rồi chọn lại; chưa thay đổi gì.");return now;}
  static ExportDecision SourceDecision(LibEntry now,LegendMap map,LibCatalog catalog){var b=Legend.DecideSource(Used(now),map,x=>Library.Reference(catalog,x));
   if(b.blocking)throw new InvalidOperationException("Chưa sửa được thông tin bảng ký hiệu của "+now.name+": ký hiệu đang bị chặn khi tạo bảng ("+b.status+" — "+b.reason+"). Xử lý lý do đó trước; chưa thay đổi gì.");return b;}
  // Read-only state for the dialog (no lock, nothing written). Save re-checks everything under the lock.
  public static MetadataSnapshot032 Snapshot(string root,LibEntry target){
   if(target==null||string.IsNullOrEmpty(target.id))throw new InvalidOperationException("Chuột phải đúng ký hiệu cần sửa thông tin trong danh sách; chưa thay đổi gì.");
   if(string.IsNullOrEmpty(root))throw new InvalidOperationException("Chưa có thư mục thư viện.");
   var catalog=Library.Load(root);var now=Active(catalog,target);var map=Legend.Load(root);var b=SourceDecision(now,map,catalog);
   var d=Legend.Decide(Used(now),map,x=>Library.Reference(catalog,x));var cur=map.metadata032.Find(now.id);
   var s=new MetadataSnapshot032{root=root,entry=Copy(now),fileSha256=map.metadata032.sha256,fileRevision=map.metadata032.file.revision,current=cur==null?null:Copy(cur),
    sourceStatus=b.status,sourceReason=b.reason,effectiveStatus=d.status,effectiveReason=d.reason,layout=b.layout};
   for(int k=0;k<3;k++){s.source[k]=b.cells[k].text??"";s.overridden[k]=Field(cur,k)!=null;s.effective[k]=d.blocking?(Field(cur,k)??s.source[k]):d.cells[k].text??"";}
   return s;}
  static bool SameCells(MetadataEntry032 a,MetadataEntry032 b){if(a==null||b==null)return a==null&&b==null;
   return Same(a.sourceSha256,b.sourceSha256)&&Same(a.name,b.name)&&Enumerable.Range(0,3).All(k=>string.Equals(Field(a,k),Field(b,k),StringComparison.Ordinal));}
  static string Text(MetadataEntry032 e){return Library.Json().Serialize(new object[]{e.entryId,e.sourceSha256,e.name,e.description,e.spec,e.note,e.revision,e.updatedUtc});}
  // fields[k]: null = no override (source text), "" = blank on purpose, other = literal text (cleaned here; invalid → exception, nothing written).
  // All three null = remove this entry's record ("Dùng lại nội dung gốc"); other entries' records are never changed.
  public static MetadataResult032 Save(MetadataSnapshot032 snap,string[] fields){
   if(snap==null||snap.entry==null||string.IsNullOrEmpty(snap.root))throw new InvalidOperationException("Thiếu ký hiệu cần sửa; chưa ghi gì.");
   if(fields==null||fields.Length!=3)throw new InvalidOperationException("Thiếu ô DIỄN GIẢI / QUY CÁCH / GHI CHÚ; chưa ghi gì.");
   var want=new string[3];for(int k=0;k<3;k++)want[k]=fields[k]==null?null:Clean(fields[k],Legend.CellNames[k]);
   string root=snap.root;var res=new MetadataResult032{entryId=snap.entry.id,name=snap.entry.name};
   using(LibraryLock021.Acquire(root)){
    var catalog=Library.Load(root);var now=Active(catalog,snap.entry);
    string path=Library.SafePath(root,FileName);byte[] before=File.Exists(path)?File.ReadAllBytes(path):null;string sha=before==null?null:Hash(before);
    if(!string.Equals(sha,snap.fileSha256,StringComparison.Ordinal))throw new InvalidOperationException(FileName+" đã thay đổi sau khi mở hộp thoại (một lần sửa khác đã lưu). Bấm Hủy rồi mở lại để thấy nội dung mới; chưa ghi gì.");
    var map=Legend.Load(root);if(!string.Equals(map.metadata032.sha256,sha,StringComparison.Ordinal))throw new InvalidOperationException(FileName+" đổi trong lúc đọc; chưa ghi gì, thử lại.");
    var b=SourceDecision(now,map,catalog);
    var file=Copy(map.metadata032.file);var old=file.entries.FirstOrDefault(x=>x.entryId==now.id);MetadataEntry032 next=null;
    if(want.Any(x=>x!=null))next=new MetadataEntry032{entryId=now.id,sourceSha256=now.sourceSha256,name=now.name,description=want[0],spec=want[1],note=want[2],revision=old==null?1:old.revision+1,updatedUtc=DateTime.UtcNow.ToString("o",CultureInfo.InvariantCulture)};
    if(SameCells(old,next)){res.action=NoChange;res.fileRevision=file.revision;res.revision=old==null?0:old.revision;return res;}
    if(old!=null)file.entries.Remove(old);if(next!=null)file.entries.Add(next);file.entries=file.entries.OrderBy(x=>x.entryId,StringComparer.Ordinal).ToList();file.revision++;
    Validate(file);string text=Library.Json().Serialize(file);Validate(Library.Json().Deserialize<MetadataFile032>(text));
    // Trial in memory: the new record must give a non-blocking decision (e.g. a text format exists) before anything is written.
    var kept=map.metadata032;ExportDecision trial;map.metadata032=new MetadataOverlay032{file=file,present=true};try{trial=Legend.Decide(Used(now),map,x=>Library.Reference(catalog,x));}finally{map.metadata032=kept;}
    if(trial.blocking)throw new InvalidOperationException("Chưa lưu: "+trial.reason+" Chưa ghi gì.");
    var others=file.entries.Where(x=>x.entryId!=now.id).Select(Text).ToList();
    if(before!=null){string bk=path+".032bak_"+DateTime.UtcNow.ToString("yyyyMMddHHmmssfff",CultureInfo.InvariantCulture)+"_"+Guid.NewGuid().ToString("N").Substring(0,6);using(var fs=new FileStream(bk,FileMode.CreateNew,FileAccess.Write))fs.Write(before,0,before.Length);res.backup=Path.GetFileName(bk);}
    Fail("before-write");
    try{Library.WriteAtomic(path,text);Fail("after-write");
     // read-back: exact bytes, this entry's record, every other record, then the effective decision of this entry.
     var again=Read(root);if(!string.Equals(again.sha256,Hash(Strict.GetBytes(text)),StringComparison.Ordinal))throw new InvalidOperationException("Đọc lại: nội dung file khác bản vừa ghi.");
     var got=again.Find(now.id);if(next==null?got!=null:got==null||Text(got)!=Text(next))throw new InvalidOperationException("Đọc lại: bản ghi của mục không khớp.");
     if(!again.file.entries.Where(x=>x.entryId!=now.id).Select(Text).SequenceEqual(others))throw new InvalidOperationException("Đọc lại: bản ghi của mục khác bị thay đổi.");
     var map2=Legend.Load(root);var d2=Legend.Decide(Used(now),map2,x=>Library.Reference(catalog,x));if(d2.blocking)throw new InvalidOperationException("Đọc lại: "+d2.reason);
     for(int k=0;k<3;k++){bool ok=want[k]==null?string.Equals(d2.cells[k].contents,b.cells[k].contents,StringComparison.Ordinal):
       string.Equals(d2.cells[k].text,want[k],StringComparison.Ordinal)&&string.Equals(d2.cells[k].contents,want[k].Length==0?"":LiteralContents(want[k]),StringComparison.Ordinal);
      if(!ok)throw new InvalidOperationException("Đọc lại: ô "+Legend.CellNames[k]+" khi tạo bảng không khớp nội dung đã lưu.");}
     Fail("after-readback");
     res.action=next==null?Reset:Saved;res.revision=next==null?0:next.revision;res.fileRevision=file.revision;res.status=d2.status;res.effective=d2.cells.Select(c=>c.text??"").ToArray();return res;}
    catch(Exception ex){string undo;
     try{if(before!=null){Library.WriteAtomic(path,Strict.GetString(before));undo=File.ReadAllBytes(path).SequenceEqual(before)?FileName+" đã khôi phục và đọc lại khớp bản trước":"CẢNH BÁO: "+FileName+" khôi phục nhưng đọc lại KHÁC bản trước";}
      else{if(File.Exists(path))File.Delete(path);undo=File.Exists(path)?"CẢNH BÁO: CHƯA xoá được "+FileName+" vừa tạo":FileName+" vừa tạo đã được bỏ (trước đó chưa có file)";}}
     catch(Exception r){undo="CẢNH BÁO: CHƯA khôi phục được "+FileName+" ("+r.Message+")";}
     throw new InvalidOperationException("Chưa lưu thông tin "+res.name+": "+ex.Message+" ("+undo+(res.backup!=null?"; sao lưu: "+res.backup:"")+").",ex);}}}
  // "Dùng lại nội dung gốc": removes only this entry's record.
  public static MetadataResult032 ResetEntry(MetadataSnapshot032 snap){return Save(snap,new string[3]);}
 }
}
#endif
