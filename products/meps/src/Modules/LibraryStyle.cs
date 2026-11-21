using System;using System.IO;using System.Linq;using System.Collections.Generic;using System.Globalization;using Autodesk.AutoCAD.DatabaseServices;using Ex=System.Exception;
namespace Tvxd.Meps {
 // Runtime value used only where the source itself has none (Default lineweight, ByBlock / layer 0 without a source reference). Captured by the plugin
 // from LWDEFAULT at import/upgrade time and written into each entry's styleNotes as provenance; it is an AutoCAD setting, not an engineering standard.
 public sealed class LwOptions {public int runtimeDefault;public string provenance;}
 public sealed class FreezeResult {public bool ok;public int entities;public List<string> notes=new List<string>(),ambiguous=new List<string>();}
 public sealed class UpgradeReport {public int frozen,already,ambiguous;public List<string> errors=new List<string>(),ambiguousNames=new List<string>(),noSource=new List<string>();
  public string Summary(){return "cố định nét "+frozen+", đã cố định "+already+", mơ hồ (giữ nguyên) "+ambiguous+", lỗi "+errors.Count+(errors.Count>0?" ("+string.Join("; ",errors.Take(3))+")":"");}}
 // 0.5: freezes the effective source lineweight of every entity of a library COPY as an explicit value, on top of 0.3.1's ACI 3 + Continuous.
 // Resolution (Lineweight.Resolve): explicit value; ByLayer = the entity's layer in the copy (layer records were cloned from the source at import);
 // layer 0 / ByBlock take the SOURCE layer weight / the lineweight of the source table reference of that definition (nested references pass theirs down).
 // The source layer weight travels in LwContext.layerLw because the reference's layer is not cloned into a copy whose entities are all on layer 0 (F01).
 // Several source references or nesting paths giving different values = ambiguous: the copy is not modified and the entry is flagged, nothing is guessed.
 // Layer names, layer records and geometry are never changed, so a target drawing's same-named layers keep their own settings and no longer affect the symbol.
 public static class LibraryStyle {
  public const string Frozen=Meps05.StyleFrozen,Ambiguous=Meps05.StyleAmbiguous;
  static string K(ObjectId id,Transaction tr){return ((DBObject)tr.GetObject(id,OpenMode.ForRead)).Handle.ToString();}
  static Dictionary<string,int> Layers(Transaction tr,Database db){var d=new Dictionary<string,int>(StringComparer.OrdinalIgnoreCase);foreach(ObjectId id in (LayerTable)tr.GetObject(db.LayerTableId,OpenMode.ForRead)){var l=(LayerTableRecord)tr.GetObject(id,OpenMode.ForRead);d[l.Name]=(int)l.LineWeight;}return d;}
  static LwEnt Ent(Entity e){return new LwEnt{key=e.Handle.ToString(),layer=e.Layer,lw=(int)e.LineWeight};}
  // Pure model of the definition closure: definitions keyed by record handle, entities by handle; nested references carry their attribute references.
  static Dictionary<string,List<LwEnt>> Model(Transaction tr,ObjectId root,Dictionary<string,ObjectId> ids){var defs=new Dictionary<string,List<LwEnt>>();
   foreach(var bid in LibraryCad.Closure(tr,root)){var list=new List<LwEnt>();defs[K(bid,tr)]=list;
    foreach(ObjectId id in (BlockTableRecord)tr.GetObject(bid,OpenMode.ForRead)){var e=tr.GetObject(id,OpenMode.ForRead) as Entity;if(e==null)continue;var x=Ent(e);ids[x.key]=id;list.Add(x);
     var br=e as BlockReference;if(br!=null){x.def=K(br.BlockTableRecord,tr);if(!br.DynamicBlockTableRecord.IsNull&&br.DynamicBlockTableRecord!=br.BlockTableRecord)x.alt=K(br.DynamicBlockTableRecord,tr);foreach(ObjectId a in br.AttributeCollection){var ar=tr.GetObject(a,OpenMode.ForRead) as AttributeReference;if(ar==null)continue;var y=Ent(ar);ids[y.key]=a;if(x.attrs==null)x.attrs=new List<LwEnt>();x.attrs.Add(y);}}}}
   return defs;}
  static int Effective(Transaction tr,Database db,Entity e,int runtime,out string why){int v=(int)e.LineWeight;why="tham chiếu nguồn "+e.Handle+" trên layer "+e.Layer;
   if(v>=0)return v;if(v==Lineweight.ByLayer){var l=(LayerTableRecord)tr.GetObject(e.LayerId,OpenMode.ForRead);if((int)l.LineWeight>=0)return (int)l.LineWeight;why+=" (layer Default → LWDEFAULT runtime)";return runtime;}
   why+=(v==Lineweight.ByBlock?" (ByBlock ở model space → LWDEFAULT runtime)":" (Default → LWDEFAULT runtime)");return runtime;}
  // Every model-space reference of the named definition in the source (directly, or as the original of an anonymous dynamic-block representation).
  public static List<LwContext> SourceContexts(Database src,string name,LwOptions o){var list=new List<LwContext>();
   using(var tr=src.TransactionManager.StartTransaction()){foreach(ObjectId id in (BlockTableRecord)tr.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(src),OpenMode.ForRead)){var br=tr.GetObject(id,OpenMode.ForRead) as BlockReference;if(br==null)continue;
     string n=((BlockTableRecord)tr.GetObject(br.BlockTableRecord,OpenMode.ForRead)).Name;if(br.IsDynamicBlock&&!br.DynamicBlockTableRecord.IsNull)n=((BlockTableRecord)tr.GetObject(br.DynamicBlockTableRecord,OpenMode.ForRead)).Name;
     if(!string.Equals(n,name,StringComparison.OrdinalIgnoreCase))continue;string why;int v=Effective(tr,src,br,o.runtimeDefault,out why);int layerLw=(int)((LayerTableRecord)tr.GetObject(br.LayerId,OpenMode.ForRead)).LineWeight;
     list.Add(new LwContext{layer=br.Layer,layerLw=layerLw,lw=v,provenance=why});}tr.Commit();}
   return list;}
  // Resolves and, when unambiguous, writes explicit lineweights into the copy database (in memory; the caller saves it). Afterwards every entity of the
  // closure is re-read and must carry exactly the resolved explicit value.
  public static FreezeResult Freeze(Database lib,ObjectId def,List<LwContext> contexts,LwOptions o){var res=new FreezeResult();var ids=new Dictionary<string,ObjectId>();LwResult r;
   using(var tr=lib.TransactionManager.StartTransaction()){var model=Model(tr,def,ids);r=Lineweight.Resolve(model,K(def,tr),Layers(tr,lib),contexts,o.runtimeDefault,o.provenance);
    // Safety net: an entity of the closure that no reference path reached has no known context, so the entry is not frozen at all.
    if(r.Ok){var lost=ids.Keys.Where(k=>!r.values.ContainsKey(k)).ToList();if(lost.Count>0){r.ambiguous.Add(lost.Count+" đối tượng không có ngữ cảnh tham chiếu (ví dụ "+lost[0]+")");r.values.Clear();}}
    res.ambiguous.AddRange(r.ambiguous.Take(20));if(r.Ok){foreach(var kv in r.values){var e=(Entity)tr.GetObject(ids[kv.Key],OpenMode.ForWrite,false,true);e.LineWeight=(LineWeight)kv.Value;}res.entities=r.values.Count;}tr.Commit();}
   if(!r.Ok){res.notes.Add("MƠ HỒ: "+string.Join("; ",r.ambiguous.Take(5))+" — giữ nguyên bản chép, không đoán.");return res;}
   using(var tr=lib.TransactionManager.StartTransaction()){foreach(var kv in r.values){var e=(Entity)tr.GetObject(ids[kv.Key],OpenMode.ForRead);if((int)e.LineWeight!=kv.Value)throw new Ex("Nét của "+e.GetType().Name+" "+e.Handle+" không ghi được.");}tr.Commit();}
   res.ok=true;res.notes.Add("Nét cố định cho "+r.values.Count+" đối tượng (màu ACI 3, Continuous giữ như 0.3.1).");res.notes.AddRange(r.notes);
   if(r.runtimeUses>0)res.notes.Add("INFO "+r.runtimeUses+" giá trị lấy từ "+o.provenance+"; đây là thiết lập AutoCAD lúc nhập, không phải tiêu chuẩn kỹ thuật.");return res;}
  static Database Read(string path){var db=new Database(false,true);db.ReadDwgFile(path,FileOpenMode.OpenForReadAndAllShare,true,"");db.CloseInput(true);return db;}
  static ObjectId Find(Database db,string name){using(var tr=db.TransactionManager.StartTransaction()){var bt=(BlockTable)tr.GetObject(db.BlockTableId,OpenMode.ForRead);if(!bt.Has(name))throw new Ex("File thư viện không chứa khối "+name+".");var id=bt[name];tr.Commit();return id;}}
  // Upgrades an OWNED library copy (LIBRARY_0_5, seeded by copying LIBRARY_0_4) in place: each not-yet-frozen entry's blocks/<id>.dwg is read, frozen,
  // verified against the unfrozen file (same geometry/layers/text, still ACI 3 + Continuous) and replaced; the previous file is kept beside it as
  // .orphan_<time> (never deleted). IDs, hidden flags, previews and user additions are kept. sourcePath (optional) gives the table references for entries
  // of that exact source hash; other entries resolve from their own copy only and record that no source reference was available.
  public static UpgradeReport Upgrade(string root,string sourcePath,LwOptions o){return Upgrade(root,sourcePath,o,false);}
  // onlySource: only entries of that exact source hash (the '+ Thêm block' path, F02: re-picking the source freezes its old copies; other sources untouched).
  public static UpgradeReport Upgrade(string root,string sourcePath,LwOptions o,bool onlySource){var rep=new UpgradeReport();var cat=Library.Load(root);bool changed=false;
   string sha=sourcePath!=null&&File.Exists(sourcePath)?Library.Sha256File(sourcePath):null;if(onlySource&&(sha==null||!cat.entries.Any(e=>e.style!=Frozen&&string.Equals(e.sourceSha256,sha,StringComparison.OrdinalIgnoreCase))))return rep;
   Database src=sha!=null?LibraryCad.OpenSource(sourcePath,Library.SafePath(root,"logs")):null;
   try{foreach(var e in cat.entries){if(onlySource&&!string.Equals(e.sourceSha256,sha,StringComparison.OrdinalIgnoreCase))continue;if(e.style==Frozen){rep.already++;continue;}
     string file=Library.SafePath(root,e.file),temp=Path.Combine(Path.GetDirectoryName(file),e.id+"."+Guid.NewGuid().ToString("N")+".tmp.dwg");
     try{if(!File.Exists(file)||Library.Sha256File(file)!=e.fileSha256){rep.errors.Add(e.name+": file thư viện khác hash catalog, không nâng cấp");continue;}
      bool own=src!=null&&string.Equals(sha,e.sourceSha256,StringComparison.OrdinalIgnoreCase);var ctx=own?SourceContexts(src,e.name,o):new List<LwContext>();if(!own)rep.noSource.Add(e.name);
      FreezeResult fr;
      using(var before=Read(file))using(var lib=Read(file)){var def=Find(lib,e.name);fr=Freeze(lib,def,ctx,o);
       if(!fr.ok){e.style=Ambiguous;e.styleNotes=fr.notes.Concat(fr.ambiguous).Take(30).ToList();rep.ambiguous++;rep.ambiguousNames.Add(e.name);changed=true;continue;}
       var issues=LibraryCad.Verify(before,Find(before,e.name),lib,def);if(issues.Count>0){rep.errors.Add(e.name+": sau khi cố định nét khác bản chép: "+issues[0]);continue;}
       lib.SaveAs(temp,DwgVersion.Current);}
      // Atomic swap; the unfrozen 0.4 copy stays beside it (never deleted).
      File.Replace(temp,file,file+".unfrozen_"+DateTime.UtcNow.ToString("yyyyMMddHHmmssfff",CultureInfo.InvariantCulture));e.fileSha256=Library.Sha256File(file);e.style=Frozen;e.styleNotes=fr.notes.Take(30).ToList();
      if(!own)e.styleNotes.Add("INFO không có tham chiếu trong file nguồn khi nâng cấp ("+(sha==null?"không có file nguồn":"nguồn khác")+"); ByBlock/layer 0 dùng "+o.provenance+".");rep.frozen++;changed=true;}
     catch(Ex ex){rep.errors.Add(e.name+": "+ex.Message);}finally{if(File.Exists(temp))File.Delete(temp);}}}
   finally{if(src!=null)src.Dispose();}
   if(sha!=null&&Library.Sha256File(sourcePath)!=sha)throw new Ex("File nguồn đã thay đổi trong lúc nâng cấp; catalog không được ghi.");
   if(changed)Library.Save(root,cat);return rep;}
 }
}
