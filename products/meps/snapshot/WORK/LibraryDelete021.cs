using System;using System.IO;using System.Linq;using System.Collections.Generic;using System.Globalization;using System.Text;using System.Threading;
namespace Tvxd.Meps {
 // TVXD-MEPS-LIBDELETE-021: "Xóa khỏi công cụ" = actual removal of ONE entry from the active catalog (not the hidden flag). Pure .NET (Library + Core only,
 // no CAD or WinForms types), so it is testable without AutoCAD.
 //  - The entry leaves catalog.entries and is kept as an exact copy in catalog.deleted (deletedUtc set, hidden cleared): it is then absent from the list,
 //    Search (also with hidden), the group menu, "Hiện mục đã ẩn"/restore and placement (Library.Find). Only Library.Reference / WithHistory (legend and
 //    identity paths of symbols ALREADY placed in a drawing) still see it.
 //  - Nothing else is written or removed: blocks/<id>.dwg, previews/<id>.json, legend.json rows, the source file, captures019 and placed blocks stay as
 //    they are (historical drawings and rollback need them).
 //  - The request names the exact entry the user confirmed (ID + name + source/file hash + file + import time). The catalog is re-read under the shared
 //    library lock; a missing or changed entry is refused with nothing written (stale request).
 //  - Write: backup copy catalog.json.021bak_<time>, atomic replace (Library.Save → catalog.json.bak), read-back; any failure after the write restores the
 //    previous text and reads it back, and the message says whether that succeeded.
 public sealed class DeleteResult021 {public string id,name,backup;public int activeBefore,activeAfter,deletedAfter;
  public string Summary(){return "Đã xoá "+name+" (mục "+id+") khỏi công cụ: không còn trong danh sách, tìm kiếm, nhóm, 'Hiện mục đã ẩn' hay khôi phục ("+activeBefore+" → "+activeAfter+" mục). "+
   "Khối "+name+" đã đặt trong bản vẽ giữ nguyên tên/mã và bảng ký hiệu của bản vẽ đó vẫn nhận ra (bản ghi lịch sử nội bộ). File nguồn, file thư viện và hình xem trước không bị xoá."+
   (backup!=null?" Sao lưu catalog: "+backup+".":"");}}
 // One writer per library folder: the SAME named mutex as ModelLibraryCad019 (capture/publish) used since 019, so a delete never interleaves with a capture
 // in this AutoCAD session or another one on the machine.
 public static class LibraryLock021 {
  sealed class Held:IDisposable {readonly Mutex m;public Held(Mutex m){this.m=m;}public void Dispose(){try{m.ReleaseMutex();}catch{}m.Dispose();}}
  public static string Name(string root){return "Local\\TVXD_MEPS_LIB019_"+Library.Sha256Text(Path.GetFullPath(root).TrimEnd('\\','/').ToUpperInvariant()).Substring(0,24);}
  public static IDisposable Acquire(string root){var m=new Mutex(false,Name(root));bool got;
   try{got=m.WaitOne(TimeSpan.FromSeconds(10));}catch(AbandonedMutexException){got=true;}
   if(!got){m.Dispose();throw new InvalidOperationException("Thư viện đang được ghi bởi thao tác MEPS khác; thử lại sau. Chưa thay đổi gì.");}return new Held(m);}
 }
 public static class LibraryDelete021 {
  // QA failure injection: "before-catalog" (nothing written), "after-catalog" (written, then rolled back), "after-readback" (read back, then rolled back).
  public static string FailAt;
  static void Fail(string stage){if(FailAt!=null&&string.Equals(FailAt,stage,StringComparison.Ordinal))throw new InvalidOperationException("QA: lỗi giả lập tại "+stage+".");}
  static bool Eq(string a,string b){return string.Equals(a,b,StringComparison.Ordinal);}
  // Exact identity of a delete request: what the user saw and confirmed must still be the entry on disk.
  public static string IdentityIssue(LibEntry want,LibEntry now){if(now==null)return "mục không còn trong thư viện";
   if(!Eq(want.id,now.id)||!Eq(want.name,now.name))return "tên/ID đã đổi ("+want.name+" → "+now.name+")";
   if(!Eq(want.sourceSha256,now.sourceSha256)||!Eq(want.sourceFile,now.sourceFile))return "nguồn của mục đã đổi";
   if(!Eq(want.fileSha256,now.fileSha256)||!Eq(want.file,now.file))return "file thư viện của mục đã đổi";
   if(!Eq(want.importedUtc,now.importedUtc))return "mục đã được nhập lại";return null;}
  static T Copy<T>(T x){var j=Library.Json();return j.Deserialize<T>(j.Serialize(x));}
  // PATCH01 F023-01: stable semantic snapshot (field order independent), not raw serializer output.
  static string Text(LibEntry e){return Library.EntrySnapshot(e);}
  // Deletes exactly 'expected' (a snapshot of the entry the user right-clicked/confirmed). Throws with nothing written for a missing/stale request.
  public static DeleteResult021 Delete(string root,LibEntry expected){
   if(expected==null||string.IsNullOrEmpty(expected.id))throw new InvalidOperationException("Chưa chọn ký hiệu cần xoá (chuột phải đúng ký hiệu trong danh sách); chưa thay đổi gì.");
   var want=Copy(expected);var res=new DeleteResult021{id=want.id,name=want.name};
   using(LibraryLock021.Acquire(root)){
    string path=Library.SafePath(root,Library.CatalogFile);string before=File.Exists(path)?File.ReadAllText(path,Encoding.UTF8):null;
    var catalog=Library.Load(root);var now=catalog.entries.FirstOrDefault(x=>x.id==want.id);
    if(now==null&&catalog.deleted.Any(x=>x.id==want.id))throw new InvalidOperationException(want.name+" đã được xoá khỏi công cụ trước đó; chưa thay đổi gì.");
    string issue=IdentityIssue(want,now);if(issue!=null)throw new InvalidOperationException("Không xoá "+want.name+" (mục "+want.id+"): "+issue+" sau khi bạn chọn. Tải lại thư viện rồi chọn lại; chưa thay đổi gì.");
    res.activeBefore=catalog.entries.Count;
    var others=catalog.entries.Where(x=>x.id!=want.id).Select(Text).ToList();var history=catalog.deleted.Select(Text).ToList();
    var rec=Copy(now);rec.hidden=false;rec.deletedUtc=DateTime.UtcNow.ToString("o",CultureInfo.InvariantCulture);
    catalog.entries.Remove(now);catalog.deleted.Add(rec);
    string stamp=DateTime.UtcNow.ToString("yyyyMMddHHmmssfff",CultureInfo.InvariantCulture);
    if(before!=null){string b=path+".021bak_"+stamp;using(var f=new FileStream(b,FileMode.CreateNew,FileAccess.Write))using(var w=new StreamWriter(f,new UTF8Encoding(false)))w.Write(before);res.backup=Path.GetFileName(b);}
    Fail("before-catalog");
    try{Library.Save(root,catalog);Fail("after-catalog");
     // read-back: the entry is gone from every active view, its record is the exact entry + deletedUtc, every other active entry and old record is unchanged.
     var again=Library.Load(root);
     if(again.entries.Any(x=>x.id==want.id)||Library.Search(again,"",Library.All,true).Any(x=>x.id==want.id))throw new InvalidOperationException("Đọc lại: mục vẫn còn trong danh sách.");
     if(again.entries.Count!=res.activeBefore-1||!again.entries.Select(Text).SequenceEqual(others))throw new InvalidOperationException("Đọc lại: các mục khác bị thay đổi.");
     var got=again.deleted.SingleOrDefault(x=>x.id==want.id);var exp=Copy(now);exp.hidden=false;exp.deletedUtc=rec.deletedUtc;
     if(got==null||!Eq(Text(got),Text(exp)))throw new InvalidOperationException("Đọc lại: bản ghi lịch sử không khớp mục đã xoá.");
     if(!again.deleted.Where(x=>x.id!=want.id).Select(Text).SequenceEqual(history))throw new InvalidOperationException("Đọc lại: lịch sử cũ bị thay đổi.");
     Fail("after-readback");res.activeAfter=again.entries.Count;res.deletedAfter=again.deleted.Count;return res;}
    catch(Exception ex){string undo;
     try{if(before!=null){Library.WriteAtomic(path,before);string back=File.ReadAllText(path,Encoding.UTF8);undo=Eq(back,before)?"catalog đã khôi phục và đọc lại khớp bản trước":"CẢNH BÁO: catalog khôi phục nhưng đọc lại KHÁC bản trước";}
      else undo="CẢNH BÁO: không có catalog trước đó để khôi phục";}
     catch(Exception r){undo="CẢNH BÁO: CHƯA khôi phục được catalog ("+r.Message+")";}
     throw new InvalidOperationException("Chưa xoá "+want.name+": "+ex.Message+" ("+undo+(res.backup!=null?"; sao lưu: "+res.backup:"")+").",ex);}}}
 }
}
