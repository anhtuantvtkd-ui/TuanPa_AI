using System;using System.IO;using System.Linq;using System.Collections.Generic;using System.Globalization;using System.Text.RegularExpressions;using Autodesk.AutoCAD.DatabaseServices;using Autodesk.AutoCAD.Geometry;using Autodesk.AutoCAD.Colors;using Ex=System.Exception;
namespace Tvxd.Meps {
 // restored (021): names of entries deleted from the tool earlier and re-added by this explicit import (also counted in imported/added).
 public sealed class ImportReport {public string source,sourceSha256;public int imported,existing;public List<string> added=new List<string>(),skipped=new List<string>(),errors=new List<string>(),restored=new List<string>();
  public string Summary(){return source+": thêm "+imported+(restored.Count>0?" (trong đó thêm lại mục đã xoá: "+string.Join(", ",restored.Take(5))+(restored.Count>5?" …":"")+")":"")+", đã có "+existing+", bỏ qua "+skipped.Count+", lỗi "+errors.Count+(errors.Count>0?" ("+string.Join("; ",errors.Take(3))+")":"");}}
 // AutoCAD side of the 0.3 library. Every source is read into a separate side Database and never saved back.
 // Each named definition is cloned into its own new DWG under the library root, then that COPY is normalized:
 // every entity (nested definitions, attribute definitions, attribute references, old-polyline vertices) gets ACI 3 and linetype Continuous.
 // Layer names, layer table records, geometry, base points and attribute defaults are kept. No xref resolution, no proxy flattening.
 // R02 (009): text styles used by the definition keep their source records (LibraryCad06.PreserveStyles before the clone, StyleDiffs read-back after).
 // Colours outside Entity.Color (hatch background/gradient, MText inline/background, Table/MLeader/Mline style colours) are not edited: the entry becomes UNSUPPORTED.
 // 0.5: Import(root,source,LwOptions) additionally freezes effective lineweights (LibraryStyle); the marker gains the entry's style as a fifth value.
 public static class LibraryCad {
  public const string Marker="TVXD_MEPS03_LIB";
  // AutoCAD built-in dimension arrowhead definitions; they are support blocks, not symbols, and are reported as skipped.
  static readonly string[] Arrowheads={"_ARCHTICK","_DOT","_DOTSMALL","_DOTBLANK","_ORIGIN","_ORIGIN2","_OPEN","_OPEN90","_OPEN30","_CLOSED","_CLOSEDBLANK","_SMALL","_NONE","_OBLIQUE","_BOXFILLED","_BOXBLANK","_DATUMFILLED","_DATUMBLANK","_INTEGRAL"};
  static Color Green{get{return Color.FromColorIndex(ColorMethod.ByAci,3);}}
  public static bool IsNormalized(Entity e,ObjectId continuous){return e.Color.ColorMethod==ColorMethod.ByAci&&e.Color.ColorIndex==3&&e.LinetypeId==continuous;}
  static ObjectId Continuous(Transaction tr,Database db){var lt=(LinetypeTable)tr.GetObject(db.LinetypeTableId,OpenMode.ForRead);if(!lt.Has("Continuous"))throw new Ex("Bản vẽ thiếu linetype Continuous.");return lt["Continuous"];}
  public static Database OpenSource(string path,string logDir){string ext=Path.GetExtension(path).ToLowerInvariant();if(ext!=".dwg"&&ext!=".dxf")throw new Ex("Chỉ nhận file .dwg hoặc .dxf.");if(!File.Exists(path))throw new Ex("Không thấy file nguồn.");var db=new Database(false,true);try{if(ext==".dxf"){Directory.CreateDirectory(logDir);db.DxfIn(path,Path.Combine(logDir,"dxfin_"+DateTime.UtcNow.ToString("yyyyMMddHHmmssfff")+"_"+Guid.NewGuid().ToString("N").Substring(0,6)+".log"));}else{db.ReadDwgFile(path,FileOpenMode.OpenForReadAndAllShare,true,"");db.CloseInput(true);}return db;}catch{db.Dispose();throw;}}
  // Named, non-layout, non-anonymous, non-xref definitions; arrowheads and xrefs are listed in skipped with the reason.
  public static List<KeyValuePair<string,ObjectId>> Definitions(Database db,List<string> skipped){var list=new List<KeyValuePair<string,ObjectId>>();using(var tr=db.TransactionManager.StartTransaction()){var bt=(BlockTable)tr.GetObject(db.BlockTableId,OpenMode.ForRead);foreach(ObjectId id in bt){var b=(BlockTableRecord)tr.GetObject(id,OpenMode.ForRead);if(b.IsLayout||b.IsAnonymous)continue;if(b.IsFromExternalReference||b.IsFromOverlayReference||b.IsDependent){skipped.Add(b.Name+": xref/phụ thuộc xref, không resolve");continue;}if(Arrowheads.Contains(b.Name.ToUpperInvariant())){skipped.Add(b.Name+": khối mũi tên kích thước của CAD");continue;}list.Add(new KeyValuePair<string,ObjectId>(b.Name,id));}tr.Commit();}return list;}
  public static List<ObjectId> Closure(Transaction tr,ObjectId root){var list=new List<ObjectId>{root};for(int i=0;i<list.Count;i++){var b=(BlockTableRecord)tr.GetObject(list[i],OpenMode.ForRead);foreach(ObjectId id in b){var br=tr.GetObject(id,OpenMode.ForRead) as BlockReference;if(br==null)continue;foreach(var child in new[]{br.BlockTableRecord,br.DynamicBlockTableRecord})if(!child.IsNull&&!list.Contains(child))list.Add(child);}if(list.Count>500)throw new Ex("Khối lồng quá nhiều cấp/định nghĩa.");}return list;}
  public static ImportReport Import(string root,string sourcePath){return Import(root,sourcePath,null);}
  // 0.5: with lineweight options every new copy is also frozen (LibraryStyle) using this source's own table references; without them it is the 0.3.1 import.
  public static ImportReport Import(string root,string sourcePath,LwOptions lw){if(!File.Exists(sourcePath))throw new Ex("Không thấy file nguồn.");string sha=Library.Sha256File(sourcePath);var report=new ImportReport{source=Path.GetFileName(sourcePath),sourceSha256=sha};
   Directory.CreateDirectory(Library.SafePath(root,Library.BlocksDir));Directory.CreateDirectory(Library.SafePath(root,Library.PreviewsDir));var catalog=Library.Load(root);
   using(var src=OpenSource(sourcePath,Library.SafePath(root,"logs"))){var defs=Definitions(src,report.skipped);
#if MODELLIB019
    // TVXD-MEPS-MODELLIB-019: naming gate for NEW root names before any entry is built (invalid/used → exception, nothing added; see ModelLibraryCad019.FilePlan).
    defs=ModelLibraryCad019.FilePlan(src,defs,catalog,sha,report.skipped);
#endif
    foreach(var def in defs){string id=Library.EntryId(sha,def.Key);if(catalog.entries.Any(x=>x.id==id)){report.existing++;continue;}
     // TVXD-MEPS-LIBDELETE-021: an ID of catalog.deleted is the user explicitly re-adding that deleted entry (same source hash + same name). Its record must
     // match exactly (else refused, never overwritten); the rebuilt entry becomes active and the record leaves the history in the SAME catalog save (one ID
     // is never active and deleted at once). Build moves the old copy/preview aside (.orphan_), nothing is erased.
     var old=catalog.deleted.FirstOrDefault(x=>x.id==id);
     if(old!=null&&(!string.Equals(old.sourceSha256,sha,StringComparison.OrdinalIgnoreCase)||!string.Equals(old.name,def.Key,StringComparison.OrdinalIgnoreCase))){report.errors.Add(def.Key+": ID "+id+" trùng bản ghi đã xoá khác nguồn/tên ("+old.name+"); không ghi đè.");continue;}
     try{catalog.entries.Add(Build(root,src,def.Value,def.Key,id,sha,report.source,lw));report.imported++;report.added.Add(def.Key);
      if(old!=null){catalog.deleted.Remove(old);report.restored.Add(def.Key);}}catch(Ex ex){report.errors.Add(def.Key+": "+ex.Message);}}}
   if(Library.Sha256File(sourcePath)!=sha)throw new Ex("File nguồn đã thay đổi trong lúc nhập; catalog không được ghi.");if(report.imported>0)Library.Save(root,catalog);return report;}
#if MODELLIB019
  // TVXD-MEPS-MODELLIB-019 selected-only facade: builds exactly ONE entry for the validated root definition of an owned capture database (its nested
  // definitions are never separate entries), through the unchanged Build pipeline (normalisation, lineweight freeze, verification, preview, extents, file hash).
  public static LibEntry BuildSelected019(string root,Database src,ObjectId rootDef,string name,string sourceSha256,string sourceFile,LwOptions lw){if(lw==null)throw new Ex("Thiếu LWDEFAULT; chưa thêm gì.");
   var chk=Naming019.Check(name,Library.Load(root),null);if(!chk.ok)throw new Ex(chk.Message());
   using(var tr=src.TransactionManager.StartTransaction()){var b=(BlockTableRecord)tr.GetObject(rootDef,OpenMode.ForRead);if(!string.Equals(b.Name,name,StringComparison.OrdinalIgnoreCase)||b.IsLayout||b.IsAnonymous||b.IsFromExternalReference||b.IsFromOverlayReference||b.IsDependent)throw new Ex("Định nghĩa gốc của bản chụp không hợp lệ; chưa thêm gì.");tr.Commit();}
   Directory.CreateDirectory(Library.SafePath(root,Library.BlocksDir));Directory.CreateDirectory(Library.SafePath(root,Library.PreviewsDir));
   return Build(root,src,rootDef,name,Library.EntryId(sourceSha256,name),sourceSha256,sourceFile,lw);}
#endif
  // Moves an unreferenced leftover (e.g. from an interrupted import) aside instead of overwriting or deleting it.
  static void Aside(string path){if(File.Exists(path))File.Move(path,path+".orphan_"+DateTime.UtcNow.ToString("yyyyMMddHHmmssfff"));}
  static LibEntry Build(string root,Database src,ObjectId defId,string name,string id,string sha,string sourceFile,LwOptions lw){
   var e=new LibEntry{id=id,name=name,category=Library.Category(name),file=Library.BlocksDir+"/"+id+".dwg",preview=Library.PreviewsDir+"/"+id+".json",sourceFile=sourceFile,sourceSha256=sha,insunits=(int)src.Insunits,units=src.Insunits.ToString(),importedUtc=DateTime.UtcNow.ToString("o",CultureInfo.InvariantCulture),status="OK"};
   string target=Library.SafePath(root,e.file),previewPath=Library.SafePath(root,e.preview),temp=Path.Combine(Path.GetDirectoryName(target),id+"."+Guid.NewGuid().ToString("N")+".tmp.dwg");
   // Text extents need the owning database as working database; it is always restored before the side database is disposed.
   var previous=HostApplicationServices.WorkingDatabase;
   try{using(var lib=new Database(true,true)){try{lib.Insunits=src.Insunits;
    // R02: same-named styles of the new database (default Standard) take the source records first, because Ignore cloning would keep them.
    var styles=LibraryCad06.PreserveStyles(src,defId,lib,null);var map=new IdMapping();src.WblockCloneObjects(new ObjectIdCollection(new[]{defId}),lib.BlockTableId,map,DuplicateRecordCloning.Ignore,false);ObjectId libDef=map[defId].Value;
    var notes=Normalize(lib);e.warnings.AddRange(notes);if(notes.Any(q=>q.StartsWith("UNSUPPORTED")))e.status="UNSUPPORTED";
    var diff=Verify(src,defId,lib,libDef);if(diff.Count>0){e.status="UNSUPPORTED";e.warnings.AddRange(diff.Take(20).Select(q=>"UNSUPPORTED kiểm tra bản sao: "+q));}
    var sd=LibraryCad06.StyleDiffs(styles,lib,libDef);if(sd.Count>0){e.status="UNSUPPORTED";e.warnings.AddRange(sd.Take(10).Select(q=>"UNSUPPORTED kiểu chữ nguồn: "+q));}
    if(lw!=null&&e.status=="OK"){var fr=LibraryStyle.Freeze(lib,libDef,LibraryStyle.SourceContexts(src,name,lw),lw);e.style=fr.ok?LibraryStyle.Frozen:LibraryStyle.Ambiguous;e.styleNotes.AddRange(fr.notes.Concat(fr.ambiguous).Take(30));
     if(fr.ok){var again=Verify(src,defId,lib,libDef);if(again.Count>0){e.status="UNSUPPORTED";e.warnings.AddRange(again.Take(5).Select(q=>"UNSUPPORTED sau khi cố định nét: "+q));}}}
    HostApplicationServices.WorkingDatabase=lib;
    using(var tr=lib.TransactionManager.StartTransaction()){var b=(BlockTableRecord)tr.GetObject(libDef,OpenMode.ForRead);e.dynamic=b.IsDynamicBlock;if(e.dynamic)e.warnings.Add("INFO khối động: tham số/hành động giữ nguyên trong định nghĩa, chưa kiểm thử thao tác động.");
     foreach(var c in Closure(tr,libDef).Skip(1)){var n=(BlockTableRecord)tr.GetObject(c,OpenMode.ForRead);if(!n.IsAnonymous)e.nested.Add(n.Name);}
     foreach(ObjectId aid in b){var ad=tr.GetObject(aid,OpenMode.ForRead) as AttributeDefinition;if(ad!=null)e.attributes.Add(new LibAttr{tag=ad.Tag,value=ad.TextString,prompt=ad.Prompt,constant=ad.Constant,invisible=ad.Invisible});}
     bool sized=false;try{using(var probe=new BlockReference(Point3d.Origin,libDef)){var x=probe.GeometricExtents;e.minX=x.MinPoint.X;e.minY=x.MinPoint.Y;e.maxX=x.MaxPoint.X;e.maxY=x.MaxPoint.Y;sized=new[]{e.minX,e.minY,e.maxX,e.maxY}.All(Core.Finite);}}catch{}
     var preview=Preview(tr,libDef);e.previewStatus=preview.status+(preview.reason!=null?": "+preview.reason:"");
     if(!sized&&preview.prims.Count>0){e.minX=preview.minX;e.minY=preview.minY;e.maxX=preview.maxX;e.maxY=preview.maxY;sized=true;e.warnings.Add("INFO kích thước lấy từ hình xem trước (CAD không trả extents).");}
     if(!sized){e.minX=e.minY=e.maxX=e.maxY=0;e.status="UNSUPPORTED";e.warnings.Add("UNSUPPORTED không xác định được kích thước khối.");}
     Aside(previewPath);Library.WriteAtomic(previewPath,Library.Json().Serialize(preview));tr.Commit();}
    HostApplicationServices.WorkingDatabase=previous;lib.SaveAs(temp,DwgVersion.Current);}finally{HostApplicationServices.WorkingDatabase=previous;}}
    Aside(target);File.Move(temp,target);e.fileSha256=Library.Sha256File(target);return e;}
   finally{if(File.Exists(temp))File.Delete(temp);}}
  // Normalizes every block definition of an owned COPY database. Layer table records are not touched.
  public static List<string> Normalize(Database db){var notes=new List<string>();using(var tr=db.TransactionManager.StartTransaction()){var cont=Continuous(tr,db);var bt=(BlockTable)tr.GetObject(db.BlockTableId,OpenMode.ForRead);foreach(ObjectId bid in bt){var b=(BlockTableRecord)tr.GetObject(bid,OpenMode.ForRead);if(b.IsFromExternalReference||b.IsFromOverlayReference||b.IsDependent){notes.Add("UNSUPPORTED định nghĩa xref "+b.Name+" không resolve");continue;}foreach(ObjectId id in b)Paint(tr,id,cont,notes);}tr.Commit();}return notes.Distinct().ToList();}
  static void Paint(Transaction tr,ObjectId id,ObjectId cont,List<string> notes){var e=tr.GetObject(id,OpenMode.ForWrite,false,true) as Entity;if(e==null)return;if(e is ProxyEntity){notes.Add("UNSUPPORTED proxy entity (không làm phẳng)");return;}
   try{e.Color=Green;e.LinetypeId=cont;}catch(Ex ex){notes.Add("UNSUPPORTED "+e.GetType().Name+": "+ex.Message);return;}
   var br=e as BlockReference;if(br!=null)foreach(ObjectId a in br.AttributeCollection)Paint(tr,a,cont,notes);
   if(e is Polyline2d||e is Polyline3d||e is PolyFaceMesh||e is PolygonMesh)foreach(ObjectId v in (System.Collections.IEnumerable)e)Paint(tr,v,cont,notes);
   var dim=e as Dimension;if(dim!=null){dim.Dimclrd=Green;dim.Dimclre=Green;dim.Dimclrt=Green;dim.Dimltype=cont;dim.Dimltex1=cont;dim.Dimltex2=cont;}
   // Colours outside Entity.Color are left untouched (no content/geometry edits) and make the entry UNSUPPORTED, so it is never labelled normalized.
   var h=e as Hatch;if(h!=null){if(h.BackgroundColor!=null&&h.BackgroundColor.ColorMethod!=ColorMethod.None)notes.Add("UNSUPPORTED Hatch có màu nền riêng, chưa chuẩn hóa");if(h.IsGradient)notes.Add("UNSUPPORTED Hatch gradient: màu gradient riêng, chưa chuẩn hóa");}
   var mt=e as MText;if(mt!=null){if(Library.HasInlineColor(mt.Contents))notes.Add("UNSUPPORTED MText có mã màu nội dòng, chưa chuẩn hóa");if(mt.BackgroundFill&&!mt.UseBackgroundColor)notes.Add("UNSUPPORTED MText có màu nền riêng, chưa chuẩn hóa");}
   var at=e as AttributeDefinition;if(at!=null&&at.IsMTextAttributeDefinition)using(var m=at.MTextAttributeDefinition)if(Library.HasInlineColor(m.Contents))notes.Add("UNSUPPORTED thuộc tính MText có mã màu nội dòng, chưa chuẩn hóa");
   var ar=e as AttributeReference;if(ar!=null&&ar.IsMTextAttribute)using(var m=ar.MTextAttribute)if(Library.HasInlineColor(m.Contents))notes.Add("UNSUPPORTED thuộc tính MText có mã màu nội dòng, chưa chuẩn hóa");
   var style=Library.StyleColorIssue(e.GetType().Name);if(style!=null)notes.Add(style);}
  static string I(int v){return v.ToString(CultureInfo.InvariantCulture);}
  static void Pt(ShapeRecord r,Point3d p){r.N(p.X,p.Y,p.Z);}
  static void Vec(ShapeRecord r,Vector3d v){r.N(v.X,v.Y,v.Z);}
  // Reads one property group into r; a group CAD cannot read is recorded as an exact text marker (same on both sides), never half-added.
  static void Part(ShapeRecord r,string what,Action<ShapeRecord> read){var t=new ShapeRecord();try{read(t);}catch{r.T(what+":không đọc được");return;}r.text.AddRange(t.text);r.num.AddRange(t.num);}
  // Typed structural signature per definition in closure order: name + base point, then per entity its type, layer, type-specific geometry
  // (curve ends, circle/arc/ellipse centre/radius/axes/angles, polyline vertices/bulges/widths, spline control points, text/attribute data,
  // nested reference name/transform, hatch pattern/loops) and extents; attribute references follow their block reference.
  // names=false (TVXD-MEPS-LAYERS-012, VerifyShape): layer and definition/reference names are written as "*"; everything else is identical.
  static List<ShapeRecord> Signature(Transaction tr,ObjectId root,ObjectId cont,List<string> paint){return Signature(tr,root,cont,paint,true);}
  static List<ShapeRecord> Signature(Transaction tr,ObjectId root,ObjectId cont,List<string> paint,bool names){var sig=new List<ShapeRecord>();foreach(var bid in Closure(tr,root)){var b=(BlockTableRecord)tr.GetObject(bid,OpenMode.ForRead);var def=new ShapeRecord().T("DEF",!names||b.IsAnonymous?"*":b.Name);Pt(def,b.Origin);sig.Add(def);
   foreach(ObjectId id in b){var e=tr.GetObject(id,OpenMode.ForRead) as Entity;if(e==null)continue;var r=new ShapeRecord().T(e.GetType().Name,names?e.Layer:"*");
    var c=e as Curve;if(c!=null)Part(r,"ends",q=>{var s=c.StartPoint;var z=c.EndPoint;Pt(q,s);Pt(q,z);});
    var ci=e as Circle;if(ci!=null)Part(r,"circle",q=>{Pt(q,ci.Center);q.N(ci.Radius);Vec(q,ci.Normal);});
    var arc=e as Arc;if(arc!=null)Part(r,"arc",q=>{Pt(q,arc.Center);q.N(arc.Radius,arc.StartAngle,arc.EndAngle);Vec(q,arc.Normal);});
    var el=e as Ellipse;if(el!=null)Part(r,"ellipse",q=>{Pt(q,el.Center);Vec(q,el.MajorAxis);q.N(el.RadiusRatio,el.StartParam,el.EndParam);Vec(q,el.Normal);});
    var pl=e as Polyline;if(pl!=null)Part(r,"polyline",q=>{q.T(I(pl.NumberOfVertices),pl.Closed?"closed":"open");q.N(pl.Elevation);for(int i=0;i<pl.NumberOfVertices;i++){var v=pl.GetPoint2dAt(i);q.N(v.X,v.Y,pl.GetBulgeAt(i),pl.GetStartWidthAt(i),pl.GetEndWidthAt(i));}});
    var sp=e as Spline;if(sp!=null)Part(r,"spline",q=>{q.T(I(sp.Degree),I(sp.NumControlPoints),I(sp.NumFitPoints));for(int i=0;i<sp.NumControlPoints;i++)Pt(q,sp.GetControlPointAt(i));});
    var t=e as DBText;if(t!=null)Part(r,"text",q=>{q.T(t.TextString,t.HorizontalMode.ToString(),t.VerticalMode.ToString());Pt(q,t.Position);q.N(t.Height,t.Rotation,t.WidthFactor,t.Oblique);});
    var mt=e as MText;if(mt!=null)Part(r,"mtext",q=>{q.T(mt.Contents,mt.Attachment.ToString());Pt(q,mt.Location);q.N(mt.TextHeight,mt.Width,mt.Rotation);});
    var ad=e as AttributeDefinition;if(ad!=null)r.T(ad.Tag,ad.Prompt,ad.Constant?"constant":"variable",ad.Invisible?"invisible":"visible");
    var br=e as BlockReference;if(br!=null){var n=(BlockTableRecord)tr.GetObject(br.BlockTableRecord,OpenMode.ForRead);r.T("ref",!names||n.IsAnonymous?"*":n.Name);Pt(r,br.Position);r.N(br.ScaleFactors.X,br.ScaleFactors.Y,br.ScaleFactors.Z,br.Rotation);Vec(r,br.Normal);}
    var h=e as Hatch;if(h!=null)Part(r,"hatch",q=>{q.T(h.PatternName,I(h.NumberOfLoops),h.IsSolidFill?"solid":"pattern");q.N(h.PatternScale,h.PatternAngle);});
    try{var x=e.GeometricExtents;Pt(r,x.MinPoint);Pt(r,x.MaxPoint);}catch{r.T("noext");}
    sig.Add(r);
    if(br!=null)foreach(ObjectId a in br.AttributeCollection){var ar=tr.GetObject(a,OpenMode.ForRead) as AttributeReference;if(ar==null)continue;var ra=new ShapeRecord().T("ATTREF",names?ar.Layer:"*",ar.Tag,ar.TextString,ar.Invisible?"invisible":"visible");Pt(ra,ar.Position);ra.N(ar.Height,ar.Rotation);sig.Add(ra);}
    if(paint!=null){if(!IsNormalized(e,cont))paint.Add(e.GetType().Name+" "+e.Handle+" chưa ACI3/Continuous");if(br!=null)foreach(ObjectId a in br.AttributeCollection){var ar=(Entity)tr.GetObject(a,OpenMode.ForRead);if(!IsNormalized(ar,cont))paint.Add("AttributeReference "+ar.Handle+" chưa ACI3/Continuous");}}}}return sig;}
  // Strings/types/layers/names/attributes compare exactly; geometric doubles within Library.GeomEqual (1e-8 abs + 1e-12 rel). The copy is not modified.
  public static List<string> Verify(Database src,ObjectId srcDef,Database lib,ObjectId libDef){List<ShapeRecord> a,b;var paint=new List<string>();var previous=HostApplicationServices.WorkingDatabase;
   try{HostApplicationServices.WorkingDatabase=src;using(var tr=src.TransactionManager.StartTransaction()){a=Signature(tr,srcDef,ObjectId.Null,null);tr.Commit();}
    HostApplicationServices.WorkingDatabase=lib;using(var tr=lib.TransactionManager.StartTransaction()){b=Signature(tr,libDef,Continuous(tr,lib),paint);tr.Commit();}}
   finally{HostApplicationServices.WorkingDatabase=previous;}
   var issues=Library.CompareShapes(a,b,20);issues.AddRange(paint);return issues;}
  // TVXD-MEPS-LAYERS-012: geometry, texts, attributes, transforms and extents only (layer and definition names left out, no colour/linetype check), for a
  // copy whose closure was moved to a category layer ByLayer or whose definitions were renamed. Neither database is modified.
  public static List<string> VerifyShape(Database src,ObjectId srcDef,Database lib,ObjectId libDef){List<ShapeRecord> a,b;var previous=HostApplicationServices.WorkingDatabase;
   try{HostApplicationServices.WorkingDatabase=src;using(var tr=src.TransactionManager.StartTransaction()){a=Signature(tr,srcDef,ObjectId.Null,null,false);tr.Commit();}
    HostApplicationServices.WorkingDatabase=lib;using(var tr=lib.TransactionManager.StartTransaction()){b=Signature(tr,libDef,ObjectId.Null,null,false);tr.Commit();}}
   finally{HostApplicationServices.WorkingDatabase=previous;}
   return Library.CompareShapes(a,b,20);}
  // ---- Preview extraction: real geometry with nested transforms, sampled curves, hatch loops and text. ----
  sealed class Builder {public Transaction tr;public PreviewData data=new PreviewData();public SortedSet<string> skipped=new SortedSet<string>();public int points;public bool truncated;public double minX=double.MaxValue,minY=double.MaxValue,maxX=double.MinValue,maxY=double.MinValue;
   public void Grow(double x,double y){minX=Math.Min(minX,x);minY=Math.Min(minY,y);maxX=Math.Max(maxX,x);maxY=Math.Max(maxY,y);}}
  public static PreviewData Preview(Transaction tr,ObjectId def){var b=new Builder{tr=tr};Walk(b,def,Matrix3d.Identity,0,new List<ObjectId>());var d=b.data;
   if(d.prims.Count==0){d.status="UNSUPPORTED";d.reason="Không có hình vẽ hỗ trợ"+(b.skipped.Count>0?": "+string.Join(", ",b.skipped):"");return d;}
   d.minX=b.minX;d.minY=b.minY;d.maxX=b.maxX;d.maxY=b.maxY;
   if(b.skipped.Count>0||b.truncated){d.status="PARTIAL";d.reason=(b.skipped.Count>0?"Chưa vẽ: "+string.Join(", ",b.skipped):"")+(b.truncated?(b.skipped.Count>0?"; ":"")+"cắt bớt do quá "+Library.MaxPreviewPoints+" điểm":"");}return d;}
  static void Walk(Builder b,ObjectId btrId,Matrix3d m,int depth,List<ObjectId> stack){if(depth>16||stack.Contains(btrId)){b.skipped.Add("khối lồng quá sâu/vòng lặp");return;}stack.Add(btrId);var btr=(BlockTableRecord)b.tr.GetObject(btrId,OpenMode.ForRead);
   foreach(ObjectId id in btr){var e=b.tr.GetObject(id,OpenMode.ForRead) as Entity;if(e==null||!e.Visible)continue;try{Draw(b,e,m,depth,stack);}catch(Ex){b.skipped.Add(e.GetType().Name+" (lỗi đọc hình)");}}stack.RemoveAt(stack.Count-1);}
  static void Add(Builder b,string k,List<double[]> rings){int n=rings.Sum(r=>r.Length/2);if(n==0)return;if(b.points+n>Library.MaxPreviewPoints){b.truncated=true;return;}b.points+=n;foreach(var r in rings)for(int i=0;i<r.Length;i+=2)b.Grow(r[i],r[i+1]);b.data.prims.Add(new PreviewPrim{k=k,rings=rings});}
  static double[] Flat(List<Point3d> pts){var r=new double[pts.Count*2];for(int i=0;i<pts.Count;i++){r[2*i]=Math.Round(pts[i].X,6);r[2*i+1]=Math.Round(pts[i].Y,6);}return r;}
  static void Draw(Builder b,Entity e,Matrix3d m,int depth,List<ObjectId> stack){
   var br=e as BlockReference;if(br!=null){if(e is MInsertBlock)b.skipped.Add("MINSERT (chỉ vẽ ô đầu)");Walk(b,br.BlockTableRecord,m*br.BlockTransform,depth+1,stack);foreach(ObjectId a in br.AttributeCollection){var ar=b.tr.GetObject(a,OpenMode.ForRead) as AttributeReference;if(ar!=null&&ar.Visible&&!ar.Invisible)Text(b,ar,m);}return;}
   // Attribute definitions show their default value, as an inserted reference will; inside nested definitions only constant ones are visible.
   var ad=e as AttributeDefinition;if(ad!=null){if(ad.Invisible||(depth>0&&!ad.Constant))return;Text(b,ad,m);return;}
   var t=e as DBText;if(t!=null){Text(b,t,m);return;}
   var mt=e as MText;if(mt!=null){int ap=(int)mt.Attachment;if(ap<1||ap>9)ap=1;AddText(b,mt.Text,mt.Location,mt.TextHeight,mt.Rotation,(ap-1)%3,ap<=3?2:ap<=6?1:0,m);return;}
   var h=e as Hatch;if(h!=null){HatchPrim(b,h,m);return;}
   var s=e as Solid;if(s!=null){var pts=new List<Point3d>{s.GetPointAt(0),s.GetPointAt(1),s.GetPointAt(3),s.GetPointAt(2)}.Select(p=>p.TransformBy(m)).ToList();Add(b,"F",new List<double[]>{Flat(pts)});return;}
   var dp=e as DBPoint;if(dp!=null){Add(b,"D",new List<double[]>{Flat(new List<Point3d>{dp.Position.TransformBy(m)})});return;}
   var dim=e as Dimension;if(dim!=null&&!dim.DimBlockId.IsNull){Walk(b,dim.DimBlockId,m,depth+1,stack);return;}
   var c=e as Curve;if(c!=null){CurvePrim(b,c,m);return;}
   b.skipped.Add(e.GetType().Name);}
  static void CurvePrim(Builder b,Curve c,Matrix3d m){var pts=new List<Point3d>();var line=c as Line;var pl=c as Polyline;
   if(line!=null){pts.Add(line.StartPoint);pts.Add(line.EndPoint);}
   else if(pl!=null){int n=pl.NumberOfVertices,segs=pl.Closed?n:n-1;pts.Add(pl.GetPoint3dAt(0));for(int i=0;i<segs;i++){if(Math.Abs(pl.GetBulgeAt(i))>1e-12)for(int k=1;k<16;k++)pts.Add(pl.GetPointAtParameter(i+k/16.0));pts.Add(pl.GetPoint3dAt((i+1)%n));}}
   else{double s0=c.StartParam,s1=c.EndParam;if(!Core.Finite(s0)||!Core.Finite(s1)||s1<=s0){b.skipped.Add(c.GetType().Name+" (tham số lỗi)");return;}int n=c is Polyline2d||c is Polyline3d?Math.Max(1,(int)Math.Ceiling(s1-s0)*8):c is Spline?96:64;for(int i=0;i<=n;i++)pts.Add(c.GetPointAtParameter(s0+(s1-s0)*i/n));}
   Add(b,c.Closed?"C":"L",new List<double[]>{Flat(pts.Select(p=>p.TransformBy(m)).ToList())});}
  static void HatchPrim(Builder b,Hatch h,Matrix3d m){var ocs=m*Matrix3d.Displacement(h.Normal.GetNormal()*h.Elevation)*Matrix3d.PlaneToWorld(h.Normal);var rings=new List<double[]>();
   for(int i=0;i<h.NumberOfLoops;i++){var loop=h.GetLoopAt(i);var pts=new List<Point2d>();
    if(loop.IsPolyline){var v=loop.Polyline;for(int j=0;j<v.Count;j++){var a=v[j].Vertex;var z=v[(j+1)%v.Count].Vertex;pts.Add(a);if(Math.Abs(v[j].Bulge)>1e-12)BulgeArc(pts,a,z,v[j].Bulge);}}
    else foreach(Curve2d c in loop.Curves){var sp=c is LineSegment2d?c.GetSamplePoints(2):c.GetSamplePoints(24);pts.AddRange(sp);}
    if(pts.Count>=3)rings.Add(Flat(pts.Select(p=>new Point3d(p.X,p.Y,0).TransformBy(ocs)).ToList()));}
   if(rings.Count==0){b.skipped.Add("Hatch (không đọc được biên)");return;}Add(b,h.IsSolidFill||h.IsGradient?"F":"P",rings);}
  // Interior points of a bulge arc from a to z (positive bulge = counter-clockwise).
  static void BulgeArc(List<Point2d> pts,Point2d a,Point2d z,double bulge){double theta=4*Math.Atan(bulge),dx=z.X-a.X,dy=z.Y-a.Y,d=Math.Sqrt(dx*dx+dy*dy);if(d<1e-12)return;double off=d/2/Math.Tan(theta/2);var c=new Point2d((a.X+z.X)/2-dy/d*off,(a.Y+z.Y)/2+dx/d*off);double r=a.GetDistanceTo(c),a0=Math.Atan2(a.Y-c.Y,a.X-c.X);for(int k=1;k<16;k++){double t=a0+theta*k/16;pts.Add(new Point2d(c.X+r*Math.Cos(t),c.Y+r*Math.Sin(t)));}}
  static void Text(Builder b,DBText t,Matrix3d m){bool left=t.HorizontalMode==TextHorizontalMode.TextLeft&&t.VerticalMode==TextVerticalMode.TextBase,fitted=t.HorizontalMode==TextHorizontalMode.TextAlign||t.HorizontalMode==TextHorizontalMode.TextFit;int ha=0,va=0;
   if(!left&&!fitted){if(t.HorizontalMode==TextHorizontalMode.TextCenter||t.HorizontalMode==TextHorizontalMode.TextMid)ha=1;else if(t.HorizontalMode==TextHorizontalMode.TextRight)ha=2;if(t.HorizontalMode==TextHorizontalMode.TextMid||t.VerticalMode==TextVerticalMode.TextVerticalMid)va=1;else if(t.VerticalMode==TextVerticalMode.TextTop)va=2;}
   AddText(b,t.TextString,left||fitted?t.Position:t.AlignmentPoint,t.Height,t.Rotation,ha,va,m);}
  static void AddText(Builder b,string text,Point3d anchor,double height,double rotation,int ha,int va,Matrix3d m){if(string.IsNullOrEmpty(text)||!(height>0))return;var p=anchor.TransformBy(m);var dir=new Vector3d(Math.Cos(rotation),Math.Sin(rotation),0).TransformBy(m);var up=new Vector3d(-Math.Sin(rotation),Math.Cos(rotation),0).TransformBy(m);double h=height*up.Length,rot=Math.Atan2(dir.Y,dir.X);
   if(b.points+1>Library.MaxPreviewPoints){b.truncated=true;return;}b.points++;double len=h*0.7*text.Split('\n').Max(x=>x.Length),lines=text.Split('\n').Length;
   b.Grow(p.X,p.Y);foreach(var k in new[]{-1.0,1.0}){b.Grow(p.X+k*len*Math.Cos(rot),p.Y+k*len*Math.Sin(rot));b.Grow(p.X+k*h*lines*-Math.Sin(rot),p.Y+k*h*lines*Math.Cos(rot));}
   b.data.prims.Add(new PreviewPrim{k="T",text=text,x=Math.Round(p.X,6),y=Math.Round(p.Y,6),h=h,rot=rot,ha=ha,va=va});}
  // ---- Insertion into the current drawing ----
  static string TargetName(LibEntry e,string name,string suffix){string n=Library.NamePrefix+e.id+"_"+name+suffix;SymbolUtilityServices.ValidateSymbolName(n,false);return n;}
  // Marker values: [0] entry ID (stable across 0.3.1–0.5 for the same source/name), [1] library file hash, [2] block name, [3] source hash, [4] style (0.5+).
  public static string[] ReadMarker(Transaction tr,BlockTableRecord b){return MarkerOf(tr,b);}
  static string[] MarkerOf(Transaction tr,BlockTableRecord b){if(b.ExtensionDictionary.IsNull)return null;var d=(DBDictionary)tr.GetObject(b.ExtensionDictionary,OpenMode.ForRead);if(!d.Contains(Marker))return null;var x=(Xrecord)tr.GetObject(d.GetAt(Marker),OpenMode.ForRead);using(var data=x.Data){return data==null?null:data.AsArray().Select(v=>Convert.ToString(v.Value,CultureInfo.InvariantCulture)).ToArray();}}
  // Inserts one library entry into db's model space. Definitions (root and nested) are cloned under namespaced names
  // MEPS03_<id>_<name>[_k]; an existing definition is reused only when it carries this entry's marker and file hash, otherwise
  // the next free suffix is used. Existing definitions, layers and styles in the drawing are never overwritten.
  public static ObjectId Insert(Database db,string root,LibEntry e,Point3d position,double scale,double rotation){Library.CheckScale(scale);if(!Core.Finite(rotation)||!Core.Finite(position.X)||!Core.Finite(position.Y)||!Core.Finite(position.Z))throw new Ex("Điểm chèn/góc không hợp lệ.");
   if(e.hidden)throw new Ex("Mục đang ẩn; khôi phục trước khi đặt.");if(e.status!="OK")throw new Ex("Mục "+e.name+" chưa được hỗ trợ: "+string.Join("; ",e.warnings.Where(w=>w.StartsWith("UNSUPPORTED")).Take(3)));
   string file=Library.SafePath(root,e.file);if(!File.Exists(file))throw new Ex("Thiếu file thư viện "+e.file+".");if(Library.Sha256File(file)!=e.fileSha256)throw new Ex("File thư viện khác hash trong catalog; không đặt.");
   using(var tr=db.TransactionManager.StartTransaction()){var layer=(LayerTableRecord)tr.GetObject(db.Clayer,OpenMode.ForRead);if(layer.IsLocked||layer.IsFrozen||layer.IsOff)throw new Ex("Layer hiện hành "+layer.Name+" đang khóa/đóng băng/tắt.");tr.Commit();}
   ObjectId def=ObjectId.Null;
   using(var side=new Database(false,true)){side.ReadDwgFile(file,FileOpenMode.OpenForReadAndAllShare,true,"");side.CloseInput(true);ObjectId sideRoot;var names=new List<string>();
    using(var tr=side.TransactionManager.StartTransaction()){var bt=(BlockTable)tr.GetObject(side.BlockTableId,OpenMode.ForRead);if(!bt.Has(e.name))throw new Ex("File thư viện không chứa khối "+e.name+".");sideRoot=bt[e.name];foreach(var id in Closure(tr,sideRoot)){var b=(BlockTableRecord)tr.GetObject(id,OpenMode.ForRead);if(!b.IsAnonymous)names.Add(b.Name);}tr.Commit();}
    string suffix=null;
    using(var tr=db.TransactionManager.StartTransaction()){var bt=(BlockTable)tr.GetObject(db.BlockTableId,OpenMode.ForRead);for(int k=1;k<1000&&def.IsNull&&suffix==null;k++){string sfx=k==1?"":"_"+k,rootName=TargetName(e,e.name,sfx);
      // 0.5: the style marker must match too, so an older unfrozen definition of the same entry is never reused for a frozen one (and vice versa).
      if(bt.Has(rootName)){var mark=MarkerOf(tr,(BlockTableRecord)tr.GetObject(bt[rootName],OpenMode.ForRead));if(mark!=null&&mark.Length>=2&&mark[0]==e.id&&mark[1]==e.fileSha256&&(mark.Length>=5?mark[4]:"")==(e.style??""))def=bt[rootName];continue;}
      if(names.All(n=>!bt.Has(TargetName(e,n,sfx))))suffix=sfx;}tr.Commit();}
    if(def.IsNull){if(suffix==null)throw new Ex("Không tìm được tên khối riêng còn trống.");
     using(var tr=side.TransactionManager.StartTransaction()){foreach(var id in Closure(tr,sideRoot)){var b=(BlockTableRecord)tr.GetObject(id,OpenMode.ForWrite);if(!b.IsAnonymous)b.Name=TargetName(e,b.Name,suffix);}
      var rootRec=(BlockTableRecord)tr.GetObject(sideRoot,OpenMode.ForWrite);if(rootRec.ExtensionDictionary.IsNull)rootRec.CreateExtensionDictionary();var dict=(DBDictionary)tr.GetObject(rootRec.ExtensionDictionary,OpenMode.ForWrite);var x=new Xrecord();
      using(var data=new ResultBuffer(new TypedValue((int)DxfCode.Text,e.id),new TypedValue((int)DxfCode.Text,e.fileSha256),new TypedValue((int)DxfCode.Text,e.name),new TypedValue((int)DxfCode.Text,e.sourceSha256),new TypedValue((int)DxfCode.Text,e.style??"")))x.Data=data;dict.SetAt(Marker,x);tr.AddNewlyCreatedDBObject(x,true);tr.Commit();}
     var map=new IdMapping();side.WblockCloneObjects(new ObjectIdCollection(new[]{sideRoot}),db.BlockTableId,map,DuplicateRecordCloning.Ignore,false);def=map[sideRoot].Value;}}
   using(var tr=db.TransactionManager.StartTransaction()){var cont=Continuous(tr,db);var space=(BlockTableRecord)tr.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db),OpenMode.ForWrite);
    var br=new BlockReference(position,def){ScaleFactors=new Scale3d(scale),Rotation=rotation,LayerId=db.Clayer};br.Color=Green;br.LinetypeId=cont;var id=space.AppendEntity(br);tr.AddNewlyCreatedDBObject(br,true);
    var rec=(BlockTableRecord)tr.GetObject(def,OpenMode.ForRead);if(rec.HasAttributeDefinitions)foreach(ObjectId aid in rec){var ad=tr.GetObject(aid,OpenMode.ForRead) as AttributeDefinition;if(ad==null||ad.Constant)continue;var ar=new AttributeReference();ar.SetAttributeFromBlock(ad,br.BlockTransform);ar.TextString=ad.TextString;if(ad.IsMTextAttributeDefinition)ar.UpdateMTextAttribute();ar.Color=Green;ar.LinetypeId=cont;ar.LineWeight=ad.LineWeight;br.AttributeCollection.AppendAttribute(ar);tr.AddNewlyCreatedDBObject(ar,true);}
    tr.Commit();return id;}}
 }
}
