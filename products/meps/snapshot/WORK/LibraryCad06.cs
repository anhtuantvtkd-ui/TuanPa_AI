using System;using System.IO;using System.Linq;using System.Collections.Generic;using Autodesk.AutoCAD.DatabaseServices;using Autodesk.AutoCAD.Geometry;using Autodesk.AutoCAD.Colors;using Ex=System.Exception;
namespace Tvxd.Meps {
 public sealed class RemapResult {public string source,sourceSha256,parserBefore,parserAfter,logPath;public Dictionary<string,int> before,after;public MapReport report;
  public string Summary(){return "Đã đọc lại bảng nguồn "+source+": parser "+(parserBefore??"-")+" → "+(parserAfter??"-")+"; trước "+Meps06.CountText(before)+"; sau "+Meps06.CountText(after)+". Nguồn chỉ đọc; bản ghi của nguồn khác giữ nguyên.";}}
 // 0.6.0 AutoCAD side (TVXD-MEPS-IMPLEMENT-008):
 //  - Insert: LibraryCad.Insert with source text styles namespaced. In the side copy of the library file (never saved) every text-bearing object of the
 //    closure is re-pointed to a copy of its style named Meps06.StyleName(source hash, style); the copy must keep every property of the source style and
 //    the re-pointed closure must match the untouched file (LibraryCad.Verify) before anything is cloned. The cloned definition therefore never picks
 //    up a same-named style of the target drawing, and no existing target style is modified (Ignore cloning; a namespaced name already present with other
 //    properties gets the next suffix). The marker gains a sixth value StyleMarker, so older definitions of the same entry (0.3.1–0.5.2) are never reused.
 //  - ScanForeign: model-space references that look like library symbols but carry no MEPS marker, and xrefs (reported, never adopted by name).
 //  - Remap: re-reads the legend table of exactly one library source (hash checked) into the library's legend.json with the current parser.
 public static class LibraryCad06 {
  public const string StyleMarker="TS_NS_V1",StylePrefix="MEPS06_";
  static Color Green{get{return Color.FromColorIndex(ColorMethod.ByAci,3);}}
  static ObjectId Continuous(Transaction tr,Database db){var lt=(LinetypeTable)tr.GetObject(db.LinetypeTableId,OpenMode.ForRead);if(!lt.Has("Continuous"))throw new Ex("Bản vẽ thiếu linetype Continuous.");return lt["Continuous"];}
  static string TargetName(LibEntry e,string name,string suffix){string n=Library.NamePrefix+e.id+"_"+name+suffix;SymbolUtilityServices.ValidateSymbolName(n,false);return n;}
  static Database Read(string path){var db=new Database(false,true);try{db.ReadDwgFile(path,FileOpenMode.OpenForReadAndAllShare,true,"");db.CloseInput(true);return db;}catch{db.Dispose();throw;}}
  static ObjectId FindDef(Database db,string name){using(var tr=db.TransactionManager.StartTransaction()){var bt=(BlockTable)tr.GetObject(db.BlockTableId,OpenMode.ForRead);if(!bt.Has(name))throw new Ex("File thư viện không chứa khối "+name+".");var id=bt[name];tr.Commit();return id;}}
  public static LegendStyle StyleInfo(TextStyleTableRecord r){var f=r.Font;return new LegendStyle{name=r.Name,fileName=r.FileName,bigFontFileName=r.BigFontFileName,typeface=f.TypeFace,bold=f.Bold,italic=f.Italic,charset=f.CharacterSet,pitch=f.PitchAndFamily,xScale=r.XScale,oblique=r.ObliquingAngle,textSize=r.TextSize,flagBits=r.FlagBits,vertical=r.IsVertical};}
  public static ObjectId StyleOf(Entity e){var t=e as DBText;if(t!=null)return t.TextStyleId;var m=e as MText;if(m!=null)return m.TextStyleId;var d=e as Dimension;if(d!=null)return d.TextStyleId;return ObjectId.Null;}
  static void SetStyle(Entity e,ObjectId id){var t=e as DBText;if(t!=null){t.TextStyleId=id;return;}var m=e as MText;if(m!=null){m.TextStyleId=id;return;}var d=e as Dimension;if(d!=null)d.TextStyleId=id;}
  // Text-bearing objects of a definition closure: its texts, MTexts, attribute definitions, dimensions, and the attribute references of nested references.
  public static List<ObjectId> TextObjects(Transaction tr,ObjectId root){var list=new List<ObjectId>();
   foreach(var bid in LibraryCad.Closure(tr,root))foreach(ObjectId id in (BlockTableRecord)tr.GetObject(bid,OpenMode.ForRead)){var e=tr.GetObject(id,OpenMode.ForRead) as Entity;if(e==null)continue;if(!StyleOf(e).IsNull)list.Add(id);var br=e as BlockReference;if(br!=null)foreach(ObjectId a in br.AttributeCollection)if(!a.IsErased)list.Add(a);}
   return list;}
  // Exact copy of a style record under a new name (DBObject.Clone); property-by-property only if CAD refuses the clone. The caller checks the result.
  // The font descriptor is not set here: CAD drops it when FileName is assigned or the clone is added (NATIVE009), so the caller applies RestoreFont after the Add.
  static TextStyleTableRecord Copy(TextStyleTableRecord src,string name){TextStyleTableRecord c=null;try{c=(TextStyleTableRecord)src.Clone();c.Name=name;return c;}catch(Ex){if(c!=null)c.Dispose();}
   var info=StyleInfo(src);c=new TextStyleTableRecord{Name=name};
   c.FileName=info.fileName??"";c.BigFontFileName=info.bigFontFileName??"";c.XScale=info.xScale;c.ObliquingAngle=info.oblique;c.TextSize=info.textSize;c.IsVertical=info.vertical;c.FlagBits=(byte)info.flagBits;return c;}
  // Re-applies the recorded font descriptor last (after file names and table insertion) on a record open for write; the read-back still decides.
  public static void RestoreFont(TextStyleTableRecord r,LegendStyle info){if(!string.IsNullOrEmpty(info.typeface))r.Font=new Autodesk.AutoCAD.GraphicsInterface.FontDescriptor(info.typeface,info.bold,info.italic,info.charset,info.pitch);}
  // ---- R02 (TVXD-MEPS-LEGEND-009): source text styles in an OWNED library database ----
  // WblockCloneObjects(Ignore) keeps a style already present under the same name (the default Standard of new Database(true,true)), so source texts would
  // silently use that style. PreserveStyles overwrites such same-named records of the owned database from the source records before the clone (source
  // records only, no guessed font); StyleDiffs reads every style used by the owned closure back. The user's drawing is never involved.
  // Text styles used by the texts of a definition closure, by name, as recorded in db.
  public static Dictionary<string,LegendStyle> SourceStyles(Database db,ObjectId root){var d=new Dictionary<string,LegendStyle>(StringComparer.OrdinalIgnoreCase);
   using(var tr=db.TransactionManager.StartTransaction()){foreach(var id in TextObjects(tr,root)){var sid=StyleOf((Entity)tr.GetObject(id,OpenMode.ForRead));if(sid.IsNull)continue;var r=(TextStyleTableRecord)tr.GetObject(sid,OpenMode.ForRead);if(!d.ContainsKey(r.Name))d[r.Name]=StyleInfo(r);}tr.Commit();}return d;}
  // Every recorded property of a source style onto a record (open for write) of an owned database; the whole font descriptor last (NATIVE009).
  public static void Overwrite(TextStyleTableRecord r,LegendStyle info){if(!(info.xScale>0))throw new Ex("Kiểu chữ nguồn "+info.name+" có hệ số rộng không hợp lệ "+info.xScale+".");
   r.FileName=info.fileName??"";r.BigFontFileName=info.bigFontFileName??"";r.XScale=info.xScale;r.ObliquingAngle=info.oblique;r.TextSize=info.textSize;r.IsVertical=info.vertical;r.FlagBits=(byte)info.flagBits;
   r.Font=new Autodesk.AutoCAD.GraphicsInterface.FontDescriptor(info.typeface??"",info.bold,info.italic,info.charset,info.pitch);}
  // Source style kept: Meps06.SameStyle, with one exception only: a file name EMPTY in the source may be resolved by CAD to a non-empty one when the source
  // typeface is non-empty and identical (ordinal) and every other property, the whole descriptor included, is equal. Nothing else is relaxed.
  public static bool SourceStyleKept(LegendStyle src,LegendStyle got){if(src==null||got==null)return false;if(Meps06.SameStyle(src,got))return true;
   if(!string.IsNullOrEmpty(src.fileName)||string.IsNullOrEmpty(got.fileName)||string.IsNullOrEmpty(src.typeface)||!string.Equals(src.typeface,got.typeface,StringComparison.Ordinal))return false;
   return Meps06.SameStyle(src,new LegendStyle{name=got.name,fileName=src.fileName,bigFontFileName=got.bigFontFileName,typeface=got.typeface,bold=got.bold,italic=got.italic,charset=got.charset,pitch=got.pitch,flagBits=got.flagBits,vertical=got.vertical,xScale=got.xScale,oblique=got.oblique,textSize=got.textSize});}
  public static string Describe(LegendStyle s){return s==null?"(không có)":s.name+" [file '"+s.fileName+"', bigfont '"+s.bigFontFileName+"', typeface '"+s.typeface+"', bold "+s.bold+", italic "+s.italic+", charset "+s.charset+", pitch "+s.pitch+", xScale "+Library.F(s.xScale)+", oblique "+Library.F(s.oblique)+", size "+Library.F(s.textSize)+", flags "+s.flagBits+", vertical "+s.vertical+"]";}
  // Before src.WblockCloneObjects(..., lib, Ignore): same-named styles of the owned lib that differ from the source get the source properties. Returns the
  // source styles of the closure for StyleDiffs; notes (optional) receives one line per overwritten record.
  public static Dictionary<string,LegendStyle> PreserveStyles(Database src,ObjectId srcDef,Database lib,List<string> notes){var want=SourceStyles(src,srcDef);if(want.Count==0)return want;
   using(var tr=lib.TransactionManager.StartTransaction()){var st=(TextStyleTable)tr.GetObject(lib.TextStyleTableId,OpenMode.ForRead);
    foreach(var kv in want){if(!st.Has(kv.Key))continue;var r=(TextStyleTableRecord)tr.GetObject(st[kv.Key],OpenMode.ForRead);var had=StyleInfo(r);if(SourceStyleKept(kv.Value,had))continue;
     r.UpgradeOpen();Overwrite(r,kv.Value);if(notes!=null)notes.Add("kiểu chữ "+r.Name+": "+Describe(had)+" → nguồn "+Describe(kv.Value));}tr.Commit();}
   return want;}
  // Read-back of an owned closure against the source styles (want): empty when every used style keeps its source record (SourceStyleKept).
  public static List<string> StyleDiffs(Dictionary<string,LegendStyle> want,Database lib,ObjectId libDef){var issues=new List<string>();var seen=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
   using(var tr=lib.TransactionManager.StartTransaction()){foreach(var id in TextObjects(tr,libDef)){var sid=StyleOf((Entity)tr.GetObject(id,OpenMode.ForRead));if(sid.IsNull)continue;var r=(TextStyleTableRecord)tr.GetObject(sid,OpenMode.ForRead);if(!seen.Add(r.Name))continue;
     LegendStyle s;var got=StyleInfo(r);if(!want.TryGetValue(r.Name,out s))issues.Add("kiểu chữ "+r.Name+" không có trong nguồn");else if(!SourceStyleKept(s,got))issues.Add("kiểu chữ "+r.Name+": nguồn "+Describe(s)+" ≠ bản chép "+Describe(got));}tr.Commit();}
   foreach(var n in want.Keys)if(!seen.Contains(n))issues.Add("kiểu chữ nguồn "+n+" không còn được dùng trong bản chép");return issues;}
  // TVXD-MEPS-LAYERS-012: optional layer normalisation of the private definition (StandardsCad012 supplies it). layer = target layer of the reference;
  // signature = seventh marker value; normalize(side, sideRoot, libraryFile, notes) runs on the side copy after the style re-pointing was verified;
  // reuseIssue(tr, definition) = null when an existing definition with this marker is still normalised (else it is not reused).
  // rootName (TVXD-MEPS-IDENTITY-015, optional): readable name of the ROOT definition (e.g. TVXD_E_CT_1C_01). A same-named definition is reused only with
  // this entry's marker, the signature and a clean closure; otherwise the root gets the first free rootName_k (k ≥ 2) and the existing block is left as is.
  // Nested definitions keep their private MEPS03_<id>_<name>[_k] names, with their own free suffix. null = the earlier private root name (unchanged).
  public sealed class LayerStyle012 {public ObjectId layer;public string signature,rootName;public Action<Database,ObjectId,string,List<string>> normalize;public Func<Transaction,ObjectId,string> reuseIssue;}
  static string RootName(string code,int k){string n=k==1?code:code+"_"+k;SymbolUtilityServices.ValidateSymbolName(n,false);return n;}
  // Inserts one library entry into db's model space (same checks, naming and reference properties as LibraryCad.Insert), with namespaced text styles.
  // notes receives the style mapping and read-back results (QA evidence). Nothing is written to db before the definition is ready to clone.
  // Legacy 0.6 behaviour (frozen ACI 3 on the current layer); it reuses only six-value markers, never a 012 normalised definition.
  public static ObjectId Insert(Database db,string root,LibEntry e,Point3d position,double scale,double rotation,List<string> notes){return Insert(db,root,e,position,scale,rotation,notes,null);}
  // style != null (012): the closure is moved to style.layer with Color/Linetype/Lineweight ByLayer in the side copy, the marker gets the seventh value,
  // and the reference goes on style.layer ByLayer. Text-style namespacing, file-hash checks and the clone are the same as the legacy path.
  public static ObjectId Insert(Database db,string root,LibEntry e,Point3d position,double scale,double rotation,List<string> notes,LayerStyle012 style){if(notes==null)notes=new List<string>();Library.CheckScale(scale);
   if(!Core.Finite(rotation)||!Core.Finite(position.X)||!Core.Finite(position.Y)||!Core.Finite(position.Z))throw new Ex("Điểm chèn/góc không hợp lệ.");
   if(e.hidden)throw new Ex("Mục đang ẩn; khôi phục trước khi đặt.");if(e.status!="OK")throw new Ex("Mục "+e.name+" chưa được hỗ trợ: "+string.Join("; ",e.warnings.Where(w=>w.StartsWith("UNSUPPORTED")).Take(3)));
   string file=Library.SafePath(root,e.file);if(!File.Exists(file))throw new Ex("Thiếu file thư viện "+e.file+".");if(Library.Sha256File(file)!=e.fileSha256)throw new Ex("File thư viện khác hash trong catalog; không đặt.");
   using(var tr=db.TransactionManager.StartTransaction()){var layer=(LayerTableRecord)tr.GetObject(style==null?db.Clayer:style.layer,OpenMode.ForRead);if(layer.IsLocked||layer.IsFrozen||layer.IsOff)throw new Ex((style==null?"Layer hiện hành ":"Layer chuẩn ")+layer.Name+" đang khóa/đóng băng/tắt.");tr.Commit();}
   ObjectId def=ObjectId.Null;
   using(var side=Read(file)){ObjectId sideRoot;var names=new List<string>();var styles=new Dictionary<string,LegendStyle>(StringComparer.OrdinalIgnoreCase);List<ObjectId> texts;
    using(var tr=side.TransactionManager.StartTransaction()){var bt=(BlockTable)tr.GetObject(side.BlockTableId,OpenMode.ForRead);if(!bt.Has(e.name))throw new Ex("File thư viện không chứa khối "+e.name+".");sideRoot=bt[e.name];
     foreach(var id in LibraryCad.Closure(tr,sideRoot)){var b=(BlockTableRecord)tr.GetObject(id,OpenMode.ForRead);if(!b.IsAnonymous)names.Add(b.Name);}
     texts=TextObjects(tr,sideRoot);foreach(var id in texts){var sid=StyleOf((Entity)tr.GetObject(id,OpenMode.ForRead));if(sid.IsNull)continue;var rec=(TextStyleTableRecord)tr.GetObject(sid,OpenMode.ForRead);if(!styles.ContainsKey(rec.Name))styles[rec.Name]=StyleInfo(rec);}tr.Commit();}
    string suffix=null,rootPick=null;bool canonical=style!=null&&!string.IsNullOrEmpty(style.rootName);var chosen=new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);
    using(var tr=db.TransactionManager.StartTransaction()){var bt=(BlockTable)tr.GetObject(db.BlockTableId,OpenMode.ForRead);
     for(int k=1;k<1000&&def.IsNull&&suffix==null&&rootPick==null;k++){string sfx=k==1?"":"_"+k,rootName=canonical?RootName(style.rootName,k):TargetName(e,e.name,sfx);
      // Reused only with this entry's marker, file hash, style AND the 0.6 style namespace marker.
      if(bt.Has(rootName)){var mark=LibraryCad.ReadMarker(tr,(BlockTableRecord)tr.GetObject(bt[rootName],OpenMode.ForRead));bool entry=mark!=null&&mark.Length>=6&&mark[0]==e.id&&mark[1]==e.fileSha256&&mark[4]==(e.style??"")&&mark[5]==StyleMarker;
       if(style==null){if(entry&&mark.Length==6)def=bt[rootName];}
       else if(entry&&mark.Length==7&&mark[6]==style.signature){string why=style.reuseIssue(tr,bt[rootName]);if(why==null)def=bt[rootName];else notes.Add("Không dùng lại "+rootName+": khối đã bị sửa khỏi hồ sơ layer ("+why+").");}
       if(canonical&&def.IsNull)notes.Add("Tên "+rootName+" đã có trong bản vẽ nhưng không phải khối MEPS hợp lệ của "+e.name+"; không nhận theo tên, khối có sẵn giữ nguyên.");
       continue;}
      if(canonical){rootPick=rootName;break;}
      if(names.All(n=>!bt.Has(TargetName(e,n,sfx))))suffix=sfx;}
     // Canonical root: nested definitions take their own first free private suffix (the root name is not part of that choice).
     if(canonical&&def.IsNull&&rootPick!=null){var nested=names.Where(n=>!string.Equals(n,e.name,StringComparison.OrdinalIgnoreCase)).ToList();
      for(int k=1;k<1000&&suffix==null;k++){string sfx=k==1?"":"_"+k;if(nested.All(n=>!bt.Has(TargetName(e,n,sfx))))suffix=sfx;}
      if(suffix==null)rootPick=null;else notes.Add("Tên khối gốc: "+rootPick+".");}
     if(def.IsNull){var st=(TextStyleTable)tr.GetObject(db.TextStyleTableId,OpenMode.ForRead);
      foreach(var kv in styles){string pick=null;for(int k=1;k<1000&&pick==null;k++){string n=Meps06.StyleName(e.sourceSha256,kv.Key,k);SymbolUtilityServices.ValidateSymbolName(n,false);
        if(!st.Has(n))pick=n;else if(Meps06.SameStyle(StyleInfo((TextStyleTableRecord)tr.GetObject(st[n],OpenMode.ForRead)),kv.Value)){pick=n;notes.Add("Kiểu chữ "+n+" đã có trong bản vẽ với cùng thuộc tính; dùng lại.");}}
       if(pick==null)throw new Ex("Không tìm được tên kiểu chữ riêng cho "+kv.Key+".");chosen[kv.Key]=pick;}}
     tr.Commit();}
    if(def.IsNull){if(suffix==null)throw new Ex("Không tìm được tên khối riêng còn trống.");
     // 1. Re-point the closure's texts to exact namespaced copies of their styles (side copy only).
     using(var tr=side.TransactionManager.StartTransaction()){var st=(TextStyleTable)tr.GetObject(side.TextStyleTableId,OpenMode.ForWrite);var ids=new Dictionary<string,ObjectId>(StringComparer.OrdinalIgnoreCase);
      foreach(var kv in chosen){var src=(TextStyleTableRecord)tr.GetObject(st[kv.Key],OpenMode.ForRead);ObjectId nid;if(st.Has(kv.Value))nid=st[kv.Value];else{var copy=Copy(src,kv.Value);nid=st.Add(copy);tr.AddNewlyCreatedDBObject(copy,true);RestoreFont(copy,StyleInfo(src));}
       if(!Meps06.SameStyle(StyleInfo((TextStyleTableRecord)tr.GetObject(nid,OpenMode.ForRead)),styles[kv.Key]))throw new Ex("Kiểu chữ "+kv.Value+" không giữ đúng thuộc tính của kiểu nguồn "+kv.Key+"; không đặt.");ids[kv.Key]=nid;notes.Add("Kiểu chữ nguồn "+kv.Key+" → "+kv.Value+".");}
      foreach(var id in texts){var en=(Entity)tr.GetObject(id,OpenMode.ForWrite,false,true);var sid=StyleOf(en);if(sid.IsNull)continue;ObjectId nid;if(ids.TryGetValue(((TextStyleTableRecord)tr.GetObject(sid,OpenMode.ForRead)).Name,out nid)&&nid!=sid)SetStyle(en,nid);}
      tr.Commit();}
     // 2. The re-pointed closure must equal the untouched library file (geometry, text, extents; style names are not part of the signature).
     if(texts.Count>0)using(var pristine=Read(file)){var diff=LibraryCad.Verify(pristine,FindDef(pristine,e.name),side,sideRoot);if(diff.Count>0)throw new Ex("Đổi tên kiểu chữ làm khác bản chép ("+diff[0]+"); không đặt.");}
     // 2b. (012) category layer + ByLayer on the side copy, verified against the untouched file by the caller's normalize.
     if(style!=null)style.normalize(side,sideRoot,file,notes);
     // 3. Namespaced definition names and the marker, then one clone into the target.
     using(var tr=side.TransactionManager.StartTransaction()){foreach(var id in LibraryCad.Closure(tr,sideRoot)){var b=(BlockTableRecord)tr.GetObject(id,OpenMode.ForWrite);if(!b.IsAnonymous)b.Name=canonical&&id==sideRoot?rootPick:TargetName(e,b.Name,suffix);}
      var rootRec=(BlockTableRecord)tr.GetObject(sideRoot,OpenMode.ForWrite);if(rootRec.ExtensionDictionary.IsNull)rootRec.CreateExtensionDictionary();var dict=(DBDictionary)tr.GetObject(rootRec.ExtensionDictionary,OpenMode.ForWrite);var x=new Xrecord();
      var values=new List<TypedValue>{new TypedValue((int)DxfCode.Text,e.id),new TypedValue((int)DxfCode.Text,e.fileSha256),new TypedValue((int)DxfCode.Text,e.name),new TypedValue((int)DxfCode.Text,e.sourceSha256),new TypedValue((int)DxfCode.Text,e.style??""),new TypedValue((int)DxfCode.Text,StyleMarker)};
      if(style!=null)values.Add(new TypedValue((int)DxfCode.Text,style.signature));
      using(var data=new ResultBuffer(values.ToArray()))x.Data=data;
      dict.SetAt(LibraryCad.Marker,x);tr.AddNewlyCreatedDBObject(x,true);tr.Commit();}
     var map=new IdMapping();side.WblockCloneObjects(new ObjectIdCollection(new[]{sideRoot}),db.BlockTableId,map,DuplicateRecordCloning.Ignore,false);def=map[sideRoot].Value;}
    else notes.Add("Dùng lại định nghĩa đã có của "+e.name+" (cùng dấu MEPS 0.6).");}
   ObjectId refId;
   using(var tr=db.TransactionManager.StartTransaction()){var cont=Continuous(tr,db);var space=(BlockTableRecord)tr.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db),OpenMode.ForWrite);
    var br=new BlockReference(position,def){ScaleFactors=new Scale3d(scale),Rotation=rotation,LayerId=style==null?db.Clayer:style.layer};
    if(style==null){br.Color=Green;br.LinetypeId=cont;}else{br.Color=Color.FromColorIndex(ColorMethod.ByLayer,256);br.LinetypeId=db.ByLayerLinetype;br.LineWeight=LineWeight.ByLayer;}refId=space.AppendEntity(br);tr.AddNewlyCreatedDBObject(br,true);
    var rec=(BlockTableRecord)tr.GetObject(def,OpenMode.ForRead);if(rec.HasAttributeDefinitions)foreach(ObjectId aid in rec){var ad=tr.GetObject(aid,OpenMode.ForRead) as AttributeDefinition;if(ad==null||ad.Constant)continue;var ar=new AttributeReference();ar.SetAttributeFromBlock(ad,br.BlockTransform);ar.TextString=ad.TextString;if(ad.IsMTextAttributeDefinition)ar.UpdateMTextAttribute();
     if(style==null){ar.Color=Green;ar.LinetypeId=cont;ar.LineWeight=ad.LineWeight;}else{ar.LayerId=style.layer;ar.Color=Color.FromColorIndex(ColorMethod.ByLayer,256);ar.LinetypeId=db.ByLayerLinetype;ar.LineWeight=LineWeight.ByLayer;}br.AttributeCollection.AppendAttribute(ar);tr.AddNewlyCreatedDBObject(ar,true);}
    // Read back: every text of the inserted closure (and the new attribute references) uses a namespaced style.
    var used=new SortedSet<string>(StringComparer.OrdinalIgnoreCase);foreach(var id in TextObjects(tr,def).Concat(br.AttributeCollection.Cast<ObjectId>())){var sid=StyleOf((Entity)tr.GetObject(id,OpenMode.ForRead));if(!sid.IsNull)used.Add(((TextStyleTableRecord)tr.GetObject(sid,OpenMode.ForRead)).Name);}
    var outside=used.Where(n=>!n.StartsWith(StylePrefix,StringComparison.OrdinalIgnoreCase)).ToList();if(outside.Count>0)notes.Add("CẢNH BÁO kiểu chữ ngoài vùng tên MEPS06_: "+string.Join(", ",outside));
    notes.Add("Kiểu chữ dùng trong khối đã đặt: "+(used.Count==0?"(không có chữ)":string.Join(", ",used)));
    tr.Commit();}
   return refId;}
  // Insert and the drawing metadata (e.g. the ratio record) as ONE change: Insert's transactions and its definition clone run nested inside an outer
  // transaction of db, then metadata(outer, db) writes into it, then the outer commits. Any failure aborts the outer, which undoes the nested work
  // (reference, cloned definitions, namespaced styles, metadata).
  // metadata is first run once in a transaction that is always aborted (dry run), so a metadata error such as a malformed existing ratio key stops
  // BEFORE any symbol is imported; metadata must therefore write only through the given transaction. After a failure the live block/style/model/NOD
  // state is compared with the state before the call: the message claims a full rollback only when they match, otherwise it names what is left.
  // metadata may be null (insert only). LibraryCad.Insert and Insert above are unchanged.
  // The abort only undoes the nested work while db records undo (native diagnostic R3: with UndoRecording false the reference and the ratio record stayed),
  // so a database that does not record undo is refused before the dry run or any write; the user's UNDO setting is never changed here.
  public static ObjectId InsertAtomic(Database db,string root,LibEntry e,Point3d position,double scale,double rotation,List<string> notes,Action<Transaction,Database> metadata){
   if(!db.UndoRecording)throw new Ex("Bản vẽ đang tắt ghi UNDO (UndoRecording=false, ví dụ UNDO → Control → None) nên không hoàn tác được nếu lỗi giữa chừng; chưa chèn gì, bản vẽ không đổi. Bật lại UNDO (UNDO → All) rồi đặt lại.");
   var before=Live(db);
   if(metadata!=null)try{using(var dry=db.TransactionManager.StartTransaction()){metadata(dry,db);dry.Abort();}}catch(Ex ex){throw new Ex(ex.Message+" — lỗi dữ liệu bản vẽ, chưa chèn gì; "+Rollback(db,before)+".",ex);}
   var outer=db.TransactionManager.StartTransaction();
   try{var id=Insert(db,root,e,position,scale,rotation,notes);if(metadata!=null)metadata(outer,db);outer.Commit();return id;}
   catch(Ex ex){string undo;try{if(!outer.IsDisposed)outer.Abort();undo=Rollback(db,before);}
    catch(Ex a){undo="CHƯA xác nhận được việc hoàn tác ("+a.Message+"); kiểm tra bản vẽ và dùng UNDO nếu cần";}
    throw new Ex(ex.Message+" — "+undo+".",ex);}
   finally{if(!outer.IsDisposed)outer.Dispose();}}
  // Non-erased block and text style names, model-space object count and named-object-dictionary entries (key + handle) of db.
  static HashSet<string> Live(Database db){var s=new HashSet<string>(StringComparer.Ordinal);using(var tr=db.TransactionManager.StartOpenCloseTransaction()){
    foreach(ObjectId id in (BlockTable)tr.GetObject(db.BlockTableId,OpenMode.ForRead))if(!id.IsErased)s.Add("khối "+((BlockTableRecord)tr.GetObject(id,OpenMode.ForRead)).Name);
    foreach(ObjectId id in (TextStyleTable)tr.GetObject(db.TextStyleTableId,OpenMode.ForRead))if(!id.IsErased)s.Add("kiểu chữ "+((TextStyleTableRecord)tr.GetObject(id,OpenMode.ForRead)).Name);
    int n=0;foreach(ObjectId id in (BlockTableRecord)tr.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db),OpenMode.ForRead))if(!id.IsErased)n++;s.Add("Model "+n+" đối tượng");
    foreach(DBDictionaryEntry de in (DBDictionary)tr.GetObject(db.NamedObjectsDictionaryId,OpenMode.ForRead))if(!de.Value.IsErased)s.Add("NOD "+de.Key+" "+de.Value.Handle);tr.Commit();}return s;}
  // Verified rollback text: full rollback only when the live state equals the state before the call.
  static string Rollback(Database db,HashSet<string> before){HashSet<string> after;try{after=Live(db);}catch(Ex a){return "CHƯA xác nhận được việc hoàn tác ("+a.Message+"); kiểm tra bản vẽ và dùng UNDO nếu cần";}
   var left=after.Where(x=>!before.Contains(x)).ToList();var lost=before.Where(x=>!after.Contains(x)).ToList();
   if(left.Count==0&&lost.Count==0)return "đã hoàn tác toàn bộ (khối, định nghĩa, kiểu chữ, dữ liệu tỷ lệ); bản vẽ không đổi (đã đối chiếu)";
   return "CHƯA hoàn tác hết: còn "+(left.Count==0?"(không)":string.Join(", ",left.Take(8)))+(lost.Count>0?"; khác trước: "+string.Join(", ",lost.Take(8)):"")+"; kiểm tra bản vẽ và dùng UNDO nếu cần";}
  // Model-space references without a MEPS marker whose effective name is a library entry name, or that use a MEPS03_ definition name; and xrefs.
  public static List<ForeignRef> ScanForeign(Database db,LibCatalog cat){var names=new HashSet<string>(cat.entries.Select(x=>x.name),StringComparer.OrdinalIgnoreCase);var found=new Dictionary<string,ForeignRef>(StringComparer.OrdinalIgnoreCase);
   Action<string,string> add=(name,kind)=>{ForeignRef f;string key=kind+"|"+name;if(!found.TryGetValue(key,out f))found[key]=f=new ForeignRef{name=name,kind=kind};f.count++;};
   using(var tr=db.TransactionManager.StartOpenCloseTransaction()){foreach(ObjectId id in (BlockTableRecord)tr.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db),OpenMode.ForRead)){if(id.IsErased)continue;var br=tr.GetObject(id,OpenMode.ForRead) as BlockReference;if(br==null)continue;
     var b=(BlockTableRecord)tr.GetObject(br.BlockTableRecord,OpenMode.ForRead);if(b.IsFromExternalReference||b.IsFromOverlayReference||b.IsDependent){add(b.Name,Meps06.KindXref);continue;}
     var mark=LibraryCad.ReadMarker(tr,b);BlockTableRecord dyn=null;if(br.IsDynamicBlock&&!br.DynamicBlockTableRecord.IsNull){dyn=(BlockTableRecord)tr.GetObject(br.DynamicBlockTableRecord,OpenMode.ForRead);if(mark==null)mark=LibraryCad.ReadMarker(tr,dyn);}
     if(mark!=null&&mark.Length>=3)continue;string name=dyn!=null?dyn.Name:b.Name;
     if(names.Contains(name))add(name,Meps06.KindName);else if(name.StartsWith(Library.NamePrefix,StringComparison.OrdinalIgnoreCase))add(name,Meps06.KindMepsName);
#if IDENTITY015
     // 015: an unmarked reference named like a readable code (TVXD_E_CT_1C_01[_k]) is a look-alike: reported, never adopted by its name.
     else if(Identity015.LooksCanonical(name,cat))add(name,Meps06.KindName);
#endif
     }tr.Commit();}
   return found.Values.OrderBy(f=>f.kind,StringComparer.Ordinal).ThenBy(f=>f.name,StringComparer.OrdinalIgnoreCase).ToList();}
  // Re-reads the legend table of the selected source (picked file must have exactly that hash and belong to this library) with the current parser.
  // Only that source's records change (checked); the previous legend.json stays as legend.json.bak; one provenance line goes to logs/legend_remap.log.
  public static RemapResult Remap(string root,string selectedSha,string path){if(string.IsNullOrEmpty(path)||!File.Exists(path))throw new Ex("Không thấy file nguồn.");string sha=Library.Sha256File(path);var cat=Library.Load(root);Meps06.CheckRemap(cat,selectedSha,sha);
   var before=Legend.Load(root);string others=Meps06.OthersSignature(before,sha);var res=new RemapResult{source=Path.GetFileName(path),sourceSha256=sha,before=Meps06.Counts(before,sha),parserBefore=Meps06.ParserOf(before,sha)};
#if MODELLIB019
   // 019 (F10): same refusal as the Plugin guard, here before any write so a direct API call cannot erase the user's cells of a generated table.
   if(Meps06.ParserOf(before,selectedSha)=="MODEL_CAPTURE019"||Meps06.ParserOf(before,sha)=="MODEL_CAPTURE019")throw new Ex("Nguồn là bảng do MEPS tự tạo khi thêm block từ Model (MODEL_CAPTURE019; chữ do người dùng nhập), không phải bảng đọc từ DWG/DXF. Không đọc lại: bản chụp không có bảng, đọc lại sẽ xoá chữ đã nhập. Chưa thay đổi gì.");
#endif
   res.report=LegendCad.MapSource(root,path);
   if(Library.Sha256File(path)!=sha)throw new Ex("File nguồn đã thay đổi trong lúc đọc bảng.");
   var after=Legend.Load(root);if(Meps06.OthersSignature(after,sha)!=others)throw new Ex("Bản ghi của nguồn khác thay đổi ngoài dự kiến; legend.json trước đó còn ở legend.json.bak.");
   res.after=Meps06.Counts(after,sha);res.parserAfter=Meps06.ParserOf(after,sha);
   res.logPath=Library.SafePath(root,"logs/legend_remap.log");Directory.CreateDirectory(Path.GetDirectoryName(res.logPath));
   File.AppendAllText(res.logPath,Meps06.RemapLine(DateTime.UtcNow,sha,res.source,res.parserBefore,res.parserAfter,res.before,res.after)+"\r\n",new System.Text.UTF8Encoding(false));
   return res;}
 }
}
