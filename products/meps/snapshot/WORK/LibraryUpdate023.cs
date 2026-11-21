using System;using System.IO;using System.Linq;using System.Collections.Generic;using System.Globalization;using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
#if LIBUPDATE023 && !MODELLIB019
#error LIBUPDATE023 needs MODELLIB019 (ModelLibraryCad019.Snapshot, LibraryCad.BuildSelected019).
#endif
#if LIBUPDATE023
namespace Tvxd.Meps {
 // TVXD-MEPS-LIBUPDATE-023 (user report: a symbol edited with BEDIT resets in the next drawing). Explicit update of the GEOMETRY of ONE existing library entry
 // from an edited Model block. Identity is kept: id, name, category, sourceFile, sourceSha256 (the ORIGINAL source), importedUtc, hidden, legend rows and
 // type code. Geometry resource, fileSha256, preview, bounds, attributes, nested names, style and warnings come from the new snapshot.
 // Pipeline (existing public APIs, nothing re-implemented):
 //  1. read-only checks: exact expected entry (LibraryDelete021.IdentityIssue), drawing not Unitless (PATCH02: unit metadata follows the drawing, no conversion), selected block not linked to ANOTHER entry;
 //  2. ModelLibraryCad019.Snapshot under a temporary valid name T into an owned side database (the user's drawing is only read); a nested definition named
 //     like the entry is renamed <name>_SRC inside the side copy; saved as a NEW file updates023/<id>_<stamp>_capture.dwg;
 //  3. LibraryCad.BuildSelected019 in a NEW staging root updates023/stage_<stamp> (its own empty catalog: the entry's name is not re-validated as a new name);
 //  4. the built definition T is renamed to the entry name in an owned copy (rename only; root/closure checked);
 //  5. publish under LibraryLock021: re-read + exact identity + current file hash; the current blocks/<id>.dwg and previews/<id>.json are first COPIED to
 //     *.023rev_<stamp>, catalog backed up; then blocks/<id>.dwg / previews/<id>.json replaced (the catalog schema fixes these paths), catalog
 //     saved, read back (identity, hashes, other entries, deleted history, type code, legend decision), audit sidecar updates023/<id>_<stamp>.json written.
 //     Any failure after the first replace restores block/preview/catalog from the temporary copies and reads them back (copies then kept for recovery).
 //  6. PATCH04 (user: "xóa hình cũ đi"): only AFTER step 5 is verified, this run's own copies are deleted when their content is still exactly what this run
 //     wrote (*.023rev_<stamp>, catalog .023bak_<stamp>, adjacent .bak of this run's atomic writes, capture) plus this run's stage_<stamp> tree (no reparse
 //     points). Errors there are cleanup warnings only (never rollback); the audit JSON is kept and records what was removed/remains.
 // Library files are not part of CAD UNDO: UNDO never reverts an update. Placed Model objects are not affected.
 // Placed symbols keep their old definition (marker id + old file hash): nothing in any drawing is rewritten. New insertions see the new file hash, so
 // LibraryCad06.Insert never reuses an old private definition for them.
 public sealed class UpdateResult023 {
  public string id,name,code,oldFileSha256,newFileSha256,capture,captureSha256,blockBackup,previewBackup,catalogBackup,audit,stage;
  public List<string> notes=new List<string>(),artifacts=new List<string>();
  // PATCH04: cleanup after the verified update (removed/remaining relative paths; clean = nothing left and no warning).
  public List<string> removed=new List<string>(),remaining=new List<string>(),cleanupWarnings=new List<string>();public bool clean;
  public string Summary(){return "Đã cập nhật hình của "+name+" (mục "+id+", mã "+code+" giữ nguyên; tên, nhóm, dòng bảng ký hiệu giữ nguyên). Lần đặt mới dùng hình mới; ký hiệu đã đặt trong các bản vẽ giữ hình cũ. "+
   (clean?"Đã xoá bản sao hình cũ và tệp tạm của lần cập nhật này ("+removed.Count+" mục). ":"Cập nhật vẫn hợp lệ, nhưng CHƯA dọn hết tệp tạm/bản sao hình cũ — còn lại: "+(remaining.Count>0?string.Join(", ",remaining):"(xem cảnh báo)")+(cleanupWarnings.Count>0?" ("+string.Join("; ",cleanupWarnings.Take(3))+")":"")+". ")+
   "Nhật ký "+audit+". UNDO của CAD không hoàn tác cập nhật thư viện.";}}
 public static class LibraryUpdate023 {
  public const string Dir="updates023",AuditSchema="TVXD_MEPS_LIBUPDATE023";
  // QA failure injection: "after-snapshot", "after-build" (nothing published), "before-catalog" (backups only), "after-catalog", "after-readback" (rolled back),
  // "cleanup" (PATCH04: after the verified update; caught as a cleanup warning, update kept, remaining files reported).
  public static string FailAt;
  static void Fail(string stage){if(FailAt!=null&&string.Equals(FailAt,stage,StringComparison.Ordinal))throw new InvalidOperationException("QA: lỗi giả lập tại "+stage+".");}
  static bool Eq(string a,string b){return string.Equals(a,b,StringComparison.Ordinal);}
  static T Copy<T>(T x){var j=Library.Json();return j.Deserialize<T>(j.Serialize(x));}
  // PATCH01 F023-01: stable semantic snapshot (field order independent), not raw serializer output.
  static string Text(LibEntry e){return Library.EntrySnapshot(e);}
  static string Read(string p){return File.Exists(p)?File.ReadAllText(p,Encoding.UTF8):null;}
  static void WriteNew(string path,string text){using(var f=new FileStream(path,FileMode.CreateNew,FileAccess.Write))using(var w=new StreamWriter(f,new UTF8Encoding(false)))w.Write(text);}
  static string Rel(string root,string full){string b=Path.GetFullPath(root).TrimEnd('\\','/')+Path.DirectorySeparatorChar;return full.StartsWith(b,StringComparison.OrdinalIgnoreCase)?full.Substring(b.Length).Replace('\\','/'):full;}
  static string Free(BlockTable bt,string name){if(!bt.Has(name))return name;for(int k=2;k<1000;k++){string n=name+k.ToString(CultureInfo.InvariantCulture);if(!bt.Has(n))return n;}throw new InvalidOperationException("Không tìm được tên tạm.");}
  static ObjectId Def(Database db,string name){using(var tr=db.TransactionManager.StartOpenCloseTransaction()){var bt=(BlockTable)tr.GetObject(db.BlockTableId,OpenMode.ForRead);if(!bt.Has(name))throw new InvalidOperationException("Bản dựng không chứa khối "+name+".");return bt[name];}}
  // Read-only checks shared by the UI (before any prompt) and Update. Returns the current catalog entry.
  public static LibEntry Precheck(string root,LibEntry expected,Database db){
   if(expected==null||string.IsNullOrEmpty(expected.id))throw new InvalidOperationException("Chưa có ký hiệu đích (chuột phải đúng ký hiệu trong danh sách); chưa thay đổi gì.");
   var catalog=Library.Load(root);var now=catalog.entries.FirstOrDefault(x=>x.id==expected.id);
   if(now==null&&catalog.deleted.Any(x=>x.id==expected.id))throw new InvalidOperationException(expected.name+" đã bị xoá khỏi công cụ; không cập nhật. Chưa thay đổi gì.");
   string issue=LibraryDelete021.IdentityIssue(expected,now);if(issue!=null)throw new InvalidOperationException("Không cập nhật "+expected.name+": "+issue+" sau khi bạn chọn. Tải lại thư viện rồi chọn lại; chưa thay đổi gì.");
   // PATCH02 C (revised GPT spec): the update records the VISIBLE geometry with the CURRENT drawing's unit metadata, coordinates unchanged (no conversion).
   // Only Unitless is refused (no guess); a different unit than the old entry is allowed and becomes the entry's new unit metadata.
   if(db!=null&&db.Insunits==UnitsValue.Undefined)throw new InvalidOperationException("Bản vẽ chưa đặt đơn vị (INSUNITS = 0, Unitless); MEPS không đoán đơn vị cho hình cập nhật. Đặt INSUNITS rồi thử lại. Chưa thay đổi gì.");
   return now;}
  // The selected block may be the entry's own placed symbol (marker/identity = expected.id) or an unlinked block; a block linked to ANOTHER entry is refused.
  public static string LinkIssue(Selected019 s,LibEntry expected){string other=s.markerId??s.identityId;
   if(s.markerId!=null&&s.identityId!=null&&s.markerId!=s.identityId)return "dấu định nghĩa ("+s.markerId+") khác mã trên tham chiếu ("+s.identityId+")";
   if(other!=null&&other!=expected.id)return "khối đã chọn là ký hiệu của mục khác ("+(s.alias??other)+", mục "+other+")";return null;}
  public static UpdateResult023 Update(Database db,ObjectId refId,LibEntry expected,string root,LwOptions lw,string drawing){
   if(lw==null)throw new InvalidOperationException("Thiếu LWDEFAULT; chưa thay đổi gì.");
   var want=Copy(expected);var before=Precheck(root,want,db);var info=ModelLibraryCad019.Inspect(db,refId);
   string link=LinkIssue(info,want);if(link!=null)throw new InvalidOperationException("Không cập nhật "+want.name+": "+link+". Chưa thay đổi gì.");
   var res=new UpdateResult023{id=before.id,name=before.name,oldFileSha256=before.fileSha256};
   string stamp=DateTime.UtcNow.ToString("yyyyMMddHHmmssfff",CultureInfo.InvariantCulture)+"_"+Guid.NewGuid().ToString("N").Substring(0,8);
   string temp="E-X-MEPSUPD-"+Guid.NewGuid().ToString("N").Substring(0,12).ToUpperInvariant();
   Directory.CreateDirectory(Library.SafePath(root,Dir));
   string capRel=Dir+"/"+before.id+"_"+stamp+"_capture.dwg",cap=Library.SafePath(root,capRel),capTmp=Library.SafePath(root,Dir+"/~"+stamp+".tmp.dwg");
   string stageRel=Dir+"/stage_"+stamp,stage=Library.SafePath(root,stageRel);
   if(File.Exists(cap)||File.Exists(capTmp)||Directory.Exists(stage))throw new InvalidOperationException("Tệp tạm cập nhật đã tồn tại; không ghi đè. Chưa thay đổi gì.");
   // 2. snapshot (drawing only read) under T; a nested definition with the entry's name is renamed in the OWNED copy so the final rename cannot clash.
   try{using(var side=new Database(true,true)){ModelLibraryCad019.Snapshot(db,refId,side,temp,res.notes);
     using(var tr=side.TransactionManager.StartTransaction()){var bt=(BlockTable)tr.GetObject(side.BlockTableId,OpenMode.ForRead);
      if(bt.Has(before.name)){var clash=(BlockTableRecord)tr.GetObject(bt[before.name],OpenMode.ForWrite);string alt=Free(bt,before.name+"_SRC");clash.Name=alt;res.notes.Add("INFO định nghĩa lồng "+before.name+" đổi thành "+alt+" chỉ trong bản chụp (bản vẽ gốc giữ nguyên).");}
      tr.Commit();}
     side.SaveAs(capTmp,DwgVersion.Current);}
    File.Move(capTmp,cap);}
   finally{if(File.Exists(capTmp))File.Delete(capTmp);}
   res.capture=capRel;res.artifacts.Add(capRel);res.captureSha256=Library.Sha256File(cap);Fail("after-snapshot");
   // 3. build in a fresh staging root (own empty catalog), unchanged LibraryCad pipeline.
   Directory.CreateDirectory(stage);res.stage=stageRel;res.artifacts.Add(stageRel+"/");LibEntry built;
   using(var src=LibraryCad.OpenSource(cap,Library.SafePath(root,"logs")))built=LibraryCad.BuildSelected019(stage,src,Def(src,temp),temp,res.captureSha256,Path.GetFileName(cap),lw);
   if(Library.Sha256File(cap)!=res.captureSha256)throw new InvalidOperationException("Tệp chụp đã thay đổi trong lúc dựng; chưa cập nhật.");
   if(built.status!="OK")throw new InvalidOperationException("Hình mới chưa được hỗ trợ: "+string.Join("; ",built.warnings.Where(w=>w.StartsWith("UNSUPPORTED")).Take(3))+". Chưa cập nhật.");
   var pi=Meps05.PlaceIssue(built);if(pi!=null)throw new InvalidOperationException(pi+" Chưa cập nhật.");
   if(built.insunits!=(int)db.Insunits||built.insunits==(int)UnitsValue.Undefined)throw new InvalidOperationException("Đơn vị bản dựng ("+built.insunits+") khác đơn vị bản vẽ nguồn ("+(int)db.Insunits+"); chưa cập nhật.");
   // 4. rename T → entry name in an owned copy of the built file (rename only).
   string builtFile=Path.Combine(stage,built.file.Replace('/','\\')),builtPreview=Path.Combine(stage,built.preview.Replace('/','\\'));
   string newBlock=Library.SafePath(root,Library.BlocksDir+"/"+before.id+".u023_"+stamp+".tmp.dwg");var nested=new List<string>();int rootCount,closure;
   try{using(var lib=new Database(false,true)){lib.ReadDwgFile(builtFile,FileShare.Read,true,"");lib.CloseInput(true);
     using(var tr=lib.TransactionManager.StartTransaction()){var bt=(BlockTable)tr.GetObject(lib.BlockTableId,OpenMode.ForRead);if(!bt.Has(temp)||bt.Has(before.name))throw new InvalidOperationException("Bản dựng không đúng cấu trúc (thiếu khối tạm hoặc đã có "+before.name+"); chưa cập nhật.");
      var b=(BlockTableRecord)tr.GetObject(bt[temp],OpenMode.ForWrite);b.Name=before.name;rootCount=b.Cast<ObjectId>().Count();var ids=LibraryCad.Closure(tr,b.ObjectId).ToList();closure=ids.Count;
      foreach(var c in ids.Skip(1)){var n=(BlockTableRecord)tr.GetObject(c,OpenMode.ForRead);if(!n.IsAnonymous)nested.Add(n.Name);}tr.Commit();}
     lib.SaveAs(newBlock,DwgVersion.Current);}
    using(var chk=new Database(false,true)){chk.ReadDwgFile(newBlock,FileShare.Read,true,"");chk.CloseInput(true);
     using(var tr=chk.TransactionManager.StartOpenCloseTransaction()){var bt=(BlockTable)tr.GetObject(chk.BlockTableId,OpenMode.ForRead);if(!bt.Has(before.name)||bt.Has(temp))throw new InvalidOperationException("Đọc lại bản đổi tên: thiếu khối "+before.name+".");
      var b=(BlockTableRecord)tr.GetObject(bt[before.name],OpenMode.ForRead);if(b.Cast<ObjectId>().Count()!=rootCount||LibraryCad.Closure(tr,b.ObjectId).Count()!=closure)throw new InvalidOperationException("Đọc lại bản đổi tên: nội dung khác bản dựng.");}}
    Fail("after-build");}
   catch{if(File.Exists(newBlock))File.Move(newBlock,newBlock+".failed");throw;}
   string newSha=Library.Sha256File(newBlock),newPreviewText=File.ReadAllText(builtPreview,Encoding.UTF8);res.artifacts.Add(Rel(root,newBlock));
   var upd=Copy(before);upd.fileSha256=newSha;upd.dynamic=built.dynamic;upd.attributes=built.attributes;upd.nested=nested;upd.style=built.style;upd.styleNotes=built.styleNotes;upd.status=built.status;upd.previewStatus=built.previewStatus;
   upd.minX=built.minX;upd.minY=built.minY;upd.maxX=built.maxX;upd.maxY=built.maxY;upd.insunits=built.insunits;upd.units=built.units;
   upd.warnings=built.warnings.Where(w=>w.Length<=2000).ToList();upd.warnings.Add("INFO LIBUPDATE023: hình cập nhật "+stamp+" từ khối "+info.effectiveName+" (handle "+info.handle+") trong Model của "+(drawing??"bản vẽ hiện tại")+"; nguồn gốc của mục ("+before.sourceFile+") giữ nguyên; hash bản chụp tạm "+capRel+" ghi trong nhật ký "+Dir+" (tệp chụp được xoá sau khi cập nhật thành công).");
   // 5. publish
   using(LibraryLock021.Acquire(root)){
    var catalog=Library.Load(root);var now=catalog.entries.FirstOrDefault(x=>x.id==want.id);string issue=LibraryDelete021.IdentityIssue(want,now);
    if(issue!=null)throw new InvalidOperationException("Không cập nhật "+want.name+": "+issue+" trong lúc dựng hình. Chưa thay đổi gì (tệp chụp/dựng giữ lại: "+string.Join(", ",res.artifacts)+").");
    string blockPath=Library.SafePath(root,now.file),prevPath=Library.SafePath(root,now.preview),catPath=Library.SafePath(root,Library.CatalogFile);
    if(!File.Exists(blockPath)||Library.Sha256File(blockPath)!=now.fileSha256)throw new InvalidOperationException("File thư viện hiện tại của "+now.name+" khác hash trong catalog; không cập nhật. Chưa thay đổi gì.");
    string code=Identity015.BlockCode(now);res.code=code;
    var legendBefore=Legend.Decide(new UsedSymbol{entryId=now.id,name=now.name,sourceSha256=now.sourceSha256,count=1},Legend.Load(root),x=>catalog.entries.FirstOrDefault(q=>q.id==x));
    string legendText=Read(Library.SafePath(root,Legend.File));
    var others=catalog.entries.Where(x=>x.id!=now.id).Select(Text).ToList();var history=catalog.deleted.Select(Text).ToList();
    string catText=Read(catPath),prevText=Read(prevPath);
    // Temporary copies of the current revision (CreateNew) for rollback; deleted by PATCH04 cleanup only after a verified update.
    string revBlock=blockPath+".023rev_"+stamp,revPrev=prevPath+".023rev_"+stamp,catBak=catPath+".023bak_"+stamp;
    File.Copy(blockPath,revBlock,false);if(Library.Sha256File(revBlock)!=now.fileSha256)throw new InvalidOperationException("Sao lưu hình cũ đọc lại khác hash; chưa cập nhật.");res.blockBackup=Rel(root,revBlock);
    if(prevText!=null){WriteNew(revPrev,prevText);res.previewBackup=Rel(root,revPrev);}
    WriteNew(catBak,catText);res.catalogBackup=Rel(root,catBak);
    Fail("before-catalog");
    bool replaced=false;Dictionary<string,object> audit=null;string auditPath=null;
    try{File.Replace(newBlock,blockPath,null);replaced=true;res.artifacts.Remove(Rel(root,newBlock));Library.WriteAtomic(prevPath,newPreviewText);
     int idx=catalog.entries.IndexOf(now);catalog.entries[idx]=upd;Library.Save(root,catalog);Fail("after-catalog");
     // read-back
     var again=Library.Load(root);var got=again.entries.SingleOrDefault(x=>x.id==want.id);if(got==null)throw new InvalidOperationException("Đọc lại: mục không còn trong catalog.");
     if(!Eq(got.name,now.name)||!Eq(got.category,now.category)||!Eq(got.sourceFile,now.sourceFile)||!Eq(got.sourceSha256,now.sourceSha256)||!Eq(got.importedUtc,now.importedUtc)||got.hidden!=now.hidden||!Eq(got.file,now.file)||!Eq(got.preview,now.preview))
      throw new InvalidOperationException("Đọc lại: định danh của mục bị thay đổi.");
     if(!Eq(got.fileSha256,newSha)||Library.Sha256File(blockPath)!=newSha)throw new InvalidOperationException("Đọc lại: hash file hình mới không khớp.");
     if(got.insunits!=upd.insunits||!Eq(got.units,upd.units))throw new InvalidOperationException("Đọc lại: đơn vị của mục không khớp bản cập nhật.");
     if(!Eq(Read(prevPath),newPreviewText))throw new InvalidOperationException("Đọc lại: hình xem trước không khớp.");
     if(!again.entries.Where(x=>x.id!=want.id).Select(Text).SequenceEqual(others)||!again.deleted.Select(Text).SequenceEqual(history))throw new InvalidOperationException("Đọc lại: mục khác hoặc lịch sử xoá bị thay đổi.");
     if(!Eq(Identity015.BlockCode(got),code))throw new InvalidOperationException("Đọc lại: mã loại đã đổi.");
     if(!Eq(Read(Library.SafePath(root,Legend.File)),legendText))throw new InvalidOperationException("Đọc lại: legend.json bị thay đổi.");
     var legendAfter=Legend.Decide(new UsedSymbol{entryId=got.id,name=got.name,sourceSha256=got.sourceSha256,count=1},Legend.Load(root),x=>again.entries.FirstOrDefault(q=>q.id==x));
     if(legendAfter.blocking!=legendBefore.blocking||!Eq(legendAfter.layout,legendBefore.layout)||!Eq(legendAfter.status,legendBefore.status))throw new InvalidOperationException("Đọc lại: quyết định bảng ký hiệu đổi ("+legendBefore.status+" → "+legendAfter.status+").");
     Fail("after-readback");
     string auditRel=Dir+"/"+now.id+"_"+stamp+".json";audit=new Dictionary<string,object>{{"schema",AuditSchema},{"revision",stamp},{"utc",DateTime.UtcNow.ToString("o",CultureInfo.InvariantCulture)},
      {"target",new Dictionary<string,object>{{"id",now.id},{"name",now.name},{"code",code},{"sourceFile",now.sourceFile},{"sourceSha256",now.sourceSha256},{"importedUtc",now.importedUtc}}},
      {"previousFileSha256",now.fileSha256},{"newFileSha256",newSha},{"blockBackup",res.blockBackup},{"previewBackup",res.previewBackup},{"catalogBackup",res.catalogBackup},
      {"capture",new Dictionary<string,object>{{"file",capRel},{"sha256",res.captureSha256},{"drawing",drawing},{"handle",info.handle},{"effectiveName",info.effectiveName},{"insunits",(int)db.Insunits},{"linkedEntry",info.markerId??info.identityId}}},
      {"units",new Dictionary<string,object>{{"previousInsunits",now.insunits},{"previousUnits",now.units},{"newInsunits",upd.insunits},{"newUnits",upd.units},{"conversion","none: coordinates captured as visible in the source drawing"}}},
      {"stage",stageRel},{"note","sourceSha256 of the entry is the ORIGINAL source; the capture hash is recorded only here. Placed symbols in drawings are not rewritten."}};
     // F023-04: the audit is part of the commit — written and read back exactly INSIDE the pre-commit try (mismatch → rollback like any pre-commit failure).
     auditPath=Library.SafePath(root,auditRel);string auditText=Library.Json().Serialize(audit);WriteNew(auditPath,auditText);
     if(!Eq(Read(auditPath),auditText))throw new InvalidOperationException("Đọc lại: nhật ký cập nhật "+auditRel+" khác nội dung vừa ghi.");res.audit=auditRel;res.newFileSha256=newSha;}
    catch(Exception ex){var undo=new List<string>();bool ok=true;
     if(replaced){try{string back=blockPath+".u023_restore_"+stamp+".tmp";File.Copy(revBlock,back,false);File.Replace(back,blockPath,null);bool same=Library.Sha256File(blockPath)==now.fileSha256;ok&=same;undo.Add(same?"hình cũ đã khôi phục (hash khớp)":"CẢNH BÁO hình khôi phục khác hash");}
      catch(Exception r){ok=false;undo.Add("CẢNH BÁO chưa khôi phục hình ("+r.Message+")");}}
     try{if(prevText!=null){Library.WriteAtomic(prevPath,prevText);bool same=Eq(Read(prevPath),prevText);ok&=same;undo.Add(same?"hình xem trước đã khôi phục":"CẢNH BÁO hình xem trước khôi phục khác");}}catch(Exception r){ok=false;undo.Add("CẢNH BÁO chưa khôi phục hình xem trước ("+r.Message+")");}
     try{Library.WriteAtomic(catPath,catText);bool same=Eq(Read(catPath),catText);ok&=same;undo.Add(same?"catalog đã khôi phục và đọc lại khớp":"CẢNH BÁO catalog khôi phục khác bản trước");}catch(Exception r){ok=false;undo.Add("CẢNH BÁO chưa khôi phục catalog ("+r.Message+")");}
     throw new InvalidOperationException("Chưa cập nhật "+want.name+": "+ex.Message+" ("+(ok?"thư viện như trước: ":"KIỂM TRA thư viện: ")+string.Join("; ",undo)+"; bản giữ lại: "+res.blockBackup+", "+res.catalogBackup+"; tệp chụp/dựng: "+string.Join(", ",res.artifacts)+").",ex);}
    // ---- COMMITTED (publish + read-back + audit verified). PATCH04 cleanup runs OUTSIDE the rollback catch: any problem here is a cleanup warning only,
    // the new geometry/catalog stay valid and nothing is rolled back (the old copies may already be gone).
    Cleanup(root,res,new[]{
      new Own(Rel(root,revBlock),p=>Library.Sha256File(p)==now.fileSha256),
      new Own(res.previewBackup,p=>Eq(Read(p),prevText)),
      new Own(Rel(root,catBak),p=>Eq(Read(p),catText)),
      // adjacent .bak written by Library.WriteAtomic in THIS run (catalog save, preview replace): deleted only when it still holds exactly what this run put there.
      new Own(Rel(root,catPath+".bak"),p=>Eq(Read(p),catText)),
      new Own(prevText==null?null:Rel(root,prevPath+".bak"),p=>Eq(Read(p),prevText)),
      new Own(capRel,p=>Library.Sha256File(p)==res.captureSha256)},stageRel);
    try{audit["cleanup"]=new Dictionary<string,object>{{"clean",res.clean},{"removed",res.removed},{"remaining",res.remaining},{"warnings",res.cleanupWarnings}};
     string text=Library.Json().Serialize(audit);File.WriteAllText(auditPath,text,new UTF8Encoding(false));if(!Eq(Read(auditPath),text)){res.clean=false;res.cleanupWarnings.Add("nhật ký "+res.audit+" đọc lại khác (trạng thái dọn chưa ghi chắc chắn)");}}
    // F023-04: a failed audit persistence/read-back makes the result NOT clean (explicit warning; the committed update is kept, no rollback).
    catch(Exception ex){res.clean=false;res.cleanupWarnings.Add("chưa ghi trạng thái dọn vào nhật ký "+res.audit+": "+ex.Message);}
    return res;}}
  // ---- PATCH04: removal of THIS run's own generated copies after a verified update ----
  sealed class Own {public readonly string rel;public readonly Func<string,bool> expected;public Own(string rel,Func<string,bool> expected){this.rel=rel;this.expected=expected;}}
  static bool Reparse(string p){return (File.GetAttributes(p)&FileAttributes.ReparsePoint)!=0;}
  // Collects files/dirs of an owned tree without following links; false when any reparse point is found (the tree is then left untouched).
  static bool Walk(string dir,List<string> files,List<string> dirs){if(Reparse(dir))return false;
   foreach(var f in Directory.GetFiles(dir)){if(Reparse(f))return false;files.Add(f);}
   foreach(var d in Directory.GetDirectories(dir)){if(!Walk(d,files,dirs))return false;}dirs.Add(dir);return true;}
  static void Cleanup(string root,UpdateResult023 res,Own[] own,string stageRel){
   try{Fail("cleanup");
    foreach(var o in own){if(o.rel==null)continue;string full;
     try{full=Library.SafePath(root,o.rel);}catch(Exception ex){res.remaining.Add(o.rel);res.cleanupWarnings.Add(o.rel+": đường dẫn không an toàn ("+ex.Message+"), giữ lại");continue;}
     if(!File.Exists(full))continue;
     if(Reparse(full)){res.remaining.Add(o.rel);res.cleanupWarnings.Add(o.rel+": là liên kết (reparse), giữ lại");continue;}
     bool match;try{match=o.expected(full);}catch(Exception ex){match=false;res.cleanupWarnings.Add(o.rel+": không đọc được ("+ex.Message+")");}
     if(!match){res.remaining.Add(o.rel);res.cleanupWarnings.Add(o.rel+": nội dung khác bản do lần cập nhật này tạo, giữ lại");continue;}
     try{File.Delete(full);}catch(Exception ex){res.cleanupWarnings.Add(o.rel+": chưa xoá được ("+ex.Message+")");}
     if(File.Exists(full))res.remaining.Add(o.rel);else res.removed.Add(o.rel);}
    // staging root of THIS run only: exact updates023/stage_<stamp>, no reparse point anywhere inside, files then directories deepest first.
    string stage;try{stage=Library.SafePath(root,stageRel);}catch(Exception ex){res.remaining.Add(stageRel+"/");res.cleanupWarnings.Add(stageRel+": đường dẫn không an toàn ("+ex.Message+"), giữ lại");stage=null;}
    if(stage!=null&&Directory.Exists(stage)){var files=new List<string>();var dirs=new List<string>();
     if(!Walk(stage,files,dirs)){res.remaining.Add(stageRel+"/");res.cleanupWarnings.Add(stageRel+": có liên kết (reparse) bên trong, giữ nguyên cả thư mục");}
     else{try{foreach(var f in files){File.SetAttributes(f,FileAttributes.Normal);File.Delete(f);}foreach(var d in dirs)Directory.Delete(d,false);}catch(Exception ex){res.cleanupWarnings.Add(stageRel+": chưa xoá hết ("+ex.Message+")");}
      if(Directory.Exists(stage))res.remaining.Add(stageRel+"/");else res.removed.Add(stageRel+"/");}}}
   catch(Exception ex){res.cleanupWarnings.Add("dọn tệp tạm dừng: "+ex.Message);
    // Report what is really still there (no false "clean").
    foreach(var o in own){if(o.rel==null||res.removed.Contains(o.rel)||res.remaining.Contains(o.rel))continue;try{if(File.Exists(Library.SafePath(root,o.rel)))res.remaining.Add(o.rel);}catch{res.remaining.Add(o.rel);}}
    try{if(!res.remaining.Contains(stageRel+"/")&&!res.removed.Contains(stageRel+"/")&&Directory.Exists(Library.SafePath(root,stageRel)))res.remaining.Add(stageRel+"/");}catch{res.remaining.Add(stageRel+"/");}}
   res.clean=res.remaining.Count==0&&res.cleanupWarnings.Count==0;
   res.artifacts.Clear();res.artifacts.AddRange(res.remaining);}
 }
}
#endif
