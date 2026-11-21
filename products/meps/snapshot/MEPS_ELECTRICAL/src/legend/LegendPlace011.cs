using System;using System.Linq;using System.Collections.Generic;using System.Globalization;using Autodesk.AutoCAD.DatabaseServices;using Autodesk.AutoCAD.Geometry;using Autodesk.AutoCAD.Colors;using Ex=System.Exception;
namespace Tvxd.Meps {
 // reference/definition/blockName/handle: the one new model-space reference and its new block definition. renamed = staging records given a private name
 // because the target already had a different record of that name; restoredFonts = cloned text styles whose font descriptor had to be re-applied.
 public sealed class LegendPlacement {public ObjectId reference,definition;public string blockName,handle;public LegendResult legend;public List<string> notes=new List<string>();public int renamed,restoredFonts,entities;}
 // TVXD-MEPS-LEGEND-011: the centred legend placed into the CURRENT drawing's Model as ONE block reference, top-left corner at the picked point.
 //  1. LegendCad.Stage draws the centred legend into an owned in-memory side database (never saved, no new document).
 //  2. Mangle: every staging layer / linetype / text style / dimension style / named block whose name already exists in the target with different properties
 //     (or exists only as an erased record) is renamed IN THE STAGING COPY to a free private name (<name>_L011[_k]). Equal records (same properties; for
 //     blocks the same six-value MEPS 0.6 marker) are shared; 012: so is an equal live <name>_L011[_k] left by an earlier placement (a repeat placement
 //     adds no new private records, only its own container). No existing target record is ever modified.
 //  3. One outer transaction on the target: a new block definition TVXD_LEGEND011_<time>_<id> carrying the Marker xrecord, the staging model space cloned
 //     into it (WblockCloneObjects, Ignore), font descriptors of NEWLY cloned text styles re-applied when CAD dropped them and read back against the staging
 //     records (which LegendCad/LibraryCad06 already verified against legend.json / the library files), one reference on layer 0 (ByLayer), then a check
 //     that every pre-existing record is unchanged. Any failure aborts the outer transaction; the live state is compared with the state before.
 //  The target must record undo (as an active drawing does) so the abort removes nested work; 012: INSUNITS may differ from the source marker (TablePolicy:
 //  numeric layout 1:1, unitless staging and container, no implicit factor); layer 0 must be on, thawed and unlocked. The container has no LibraryCad marker and a non-MEPS03_ name, so LegendCad.ScanUsed / LibraryCad06.ScanForeign never count the nested symbol
 //  copies as used devices. The drawing is not saved.
 public static class LegendPlace011 {
  public const string Marker="TVXD_MEPS_LEGEND011",Prefix="TVXD_LEGEND011_",Suffix="_L011";
  static readonly HashSet<string> Fixed=new HashSet<string>(new[]{"0","Defpoints","Standard","Continuous","ByLayer","ByBlock","Annotative"},StringComparer.OrdinalIgnoreCase);
  static bool Same(string a,string b){return string.Equals(a,b,StringComparison.OrdinalIgnoreCase);}
  public static int SourceUnits(LegendMap map,LegendPlan plan){var u=plan.rows.Select(r=>map.sources.First(s=>Same(s.sourceSha256,r.sourceSha256)).insunits).Distinct().ToList();
   if(u.Count!=1)throw new Ex("Ký hiệu đến từ các nguồn khác đơn vị; V1 chưa ghép chung một bảng.");return u[0];}
  // TVXD-MEPS-LAYERS-012 table-layout policy (replaces the 011 refusal of a different INSUNITS): the source library's INSUNITS (1 = inches) is a legacy
  // marker of the annotation table, NOT a physical unit of its raw geometry. The table keeps the source numeric layout at 1× in a unitless staging
  // database and a unitless container (BlockTableRecord.Units = Undefined, reference scale 1); no implicit inch→mm or other factor is applied. The target's
  // INSUNITS is only bound for stale-context checks (Plugin.LegendContext). Physical symbol placement (raw / ratio) is unchanged and not covered here.
  public const string TablePolicy="Bảng ký hiệu giữ bố cục số của bảng nguồn 1:1 (INSUNITS nguồn chỉ là dấu cũ, không quy đổi inch→mm); khối bảng không đơn vị, hệ số 1.";
  // Checks that the plan's source tables share one unit marker (they are laid out together). Called before the point prompt and again inside Place.
  public static void CheckUnits(Database target,LegendMap map,LegendPlan plan){SourceUnits(map,plan);}
  public static bool IsLegend(Transaction tr,BlockTableRecord b){if(b.ExtensionDictionary.IsNull)return false;return ((DBDictionary)tr.GetObject(b.ExtensionDictionary,OpenMode.ForRead)).Contains(Marker);}
  // Model-space references of legend containers (QA and diagnostics).
  public static List<ObjectId> Containers(Database db){var l=new List<ObjectId>();using(var tr=db.TransactionManager.StartTransaction()){
    foreach(ObjectId id in (BlockTableRecord)tr.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db),OpenMode.ForRead)){if(id.IsErased)continue;var br=tr.GetObject(id,OpenMode.ForRead) as BlockReference;if(br!=null&&IsLegend(tr,(BlockTableRecord)tr.GetObject(br.BlockTableRecord,OpenMode.ForRead)))l.Add(id);}tr.Commit();}return l;}
  // inject (QA only, null in the product): runs inside the outer transaction after the clone and the reference, before the final checks.
  public static LegendPlacement Place(Database target,string root,LegendMap map,LegendPlan plan,Point3d at,double rotation,Action<Transaction,Database> inject){
   if(!Core.Finite(at.X)||!Core.Finite(at.Y)||!Core.Finite(at.Z)||!Core.Finite(rotation))throw new Ex("Điểm đặt/góc không hợp lệ.");
   CheckUnits(target,map,plan);
   if(!target.UndoRecording)throw new Ex("Bản vẽ đang tắt ghi UNDO (UndoRecording=false) nên không hoàn tác được nếu lỗi giữa chừng; chưa đặt gì. Bật lại UNDO (UNDO → All) rồi đặt lại.");
   using(var tr=target.TransactionManager.StartTransaction()){var z=(LayerTableRecord)tr.GetObject(target.LayerZero,OpenMode.ForRead);if(z.IsFrozen||z.IsOff||z.IsLocked)throw new Ex("Layer 0 đang "+(z.IsLocked?"khóa":"tắt/đóng băng")+"; bảng ký hiệu là một khối trên layer 0. Mở layer 0 rồi đặt lại; MEPS không tự đổi. Chưa đặt gì.");tr.Commit();}
   var res=new LegendPlacement();res.notes.Add(TablePolicy);
   using(var staging=new Database(true,true)){
    res.legend=LegendCad.Stage(root,map,plan,staging);if(staging.Insunits!=UnitsValue.Undefined)throw new Ex("Bảng tạm phải không đơn vị (chính sách bố cục 1:1); chưa đặt gì.");
    var ids=new ObjectIdCollection();using(var tr=staging.TransactionManager.StartTransaction()){foreach(ObjectId id in (BlockTableRecord)tr.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(staging),OpenMode.ForRead))if(!id.IsErased)ids.Add(id);tr.Commit();}
    if(ids.Count==0)throw new Ex("Bảng ký hiệu trống; chưa đặt gì.");
    var before=Snapshot(target);var outer=target.TransactionManager.StartTransaction();
    try{res.renamed=Mangle(staging,target,res.notes);var styles=Styles(staging);string name=FreeName(target);ObjectId def;
     using(var tr=target.TransactionManager.StartTransaction()){var bt=(BlockTable)tr.GetObject(target.BlockTableId,OpenMode.ForWrite);var b=new BlockTableRecord{Name=name,Origin=Point3d.Origin,Comments="TVXD MEPS bang ky hieu (TVXD-MEPS-LEGEND-011)",Units=UnitsValue.Undefined};def=bt.Add(b);tr.AddNewlyCreatedDBObject(b,true);
      b.CreateExtensionDictionary();var dict=(DBDictionary)tr.GetObject(b.ExtensionDictionary,OpenMode.ForWrite);var x=new Xrecord();
      using(var data=new ResultBuffer(new TypedValue((int)DxfCode.Text,"LEGEND"),new TypedValue((int)DxfCode.Text,Meps06.Version),new TypedValue((int)DxfCode.Text,DateTime.UtcNow.ToString("o",CultureInfo.InvariantCulture)),new TypedValue((int)DxfCode.Int32,res.legend.rows),new TypedValue((int)DxfCode.Int32,res.legend.symbols)))x.Data=data;
      dict.SetAt(Marker,x);tr.AddNewlyCreatedDBObject(x,true);tr.Commit();}
     var idmap=new IdMapping();staging.WblockCloneObjects(ids,def,idmap,DuplicateRecordCloning.Ignore,false);
     res.restoredFonts=Fonts(target,idmap,styles,res.notes);
     using(var tr=target.TransactionManager.StartTransaction()){var ms=(BlockTableRecord)tr.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(target),OpenMode.ForWrite);var br=new BlockReference(at,def);br.SetDatabaseDefaults(target);
      br.Rotation=rotation;br.ScaleFactors=new Scale3d(1);br.LayerId=target.LayerZero;br.Color=Color.FromColorIndex(ColorMethod.ByLayer,256);br.LinetypeId=target.ByLayerLinetype;br.LineWeight=LineWeight.ByLayer;br.LinetypeScale=1;
      res.reference=ms.AppendEntity(br);tr.AddNewlyCreatedDBObject(br,true);res.handle=br.Handle.ToString();tr.Commit();}
     if(inject!=null)inject(outer,target);
     using(var tr=target.TransactionManager.StartTransaction()){int c=((BlockTableRecord)tr.GetObject(def,OpenMode.ForRead)).Cast<ObjectId>().Count(i=>!i.IsErased);if(c!=ids.Count)throw new Ex("Khối bảng có "+c+" đối tượng, cần "+ids.Count+"; huỷ đặt.");tr.Commit();}
     var after=Snapshot(target);var changed=before.Where(s=>!s.StartsWith("Model ",StringComparison.Ordinal)&&!after.Contains(s)).ToList();
     if(changed.Count>0)throw new Ex("Bản ghi có sẵn của bản vẽ bị thay đổi ("+string.Join(", ",changed.Take(4))+"); huỷ đặt.");
     outer.Commit();res.definition=def;res.blockName=name;res.entities=ids.Count;res.notes=res.notes.Distinct().ToList();return res;}
    catch(Ex ex){string undo;try{if(!outer.IsDisposed)outer.Abort();undo=Rollback(target,before);}
     catch(Ex a){undo="CHƯA xác nhận được việc hoàn tác ("+a.Message+"); kiểm tra bản vẽ và dùng UNDO nếu cần";}
     throw new Ex(ex.Message+" — "+undo+".",ex);}
    finally{if(!outer.IsDisposed)outer.Dispose();}}}
  static string FreeName(Database db){using(var tr=db.TransactionManager.StartTransaction()){var bt=(BlockTable)tr.GetObject(db.BlockTableId,OpenMode.ForRead);
    for(int k=0;k<100;k++){string n=Prefix+DateTime.Now.ToString("yyyyMMdd_HHmmss",CultureInfo.InvariantCulture)+"_"+Guid.NewGuid().ToString("N").Substring(0,6).ToUpperInvariant();if(!bt.Has(n)){SymbolUtilityServices.ValidateSymbolName(n,false);tr.Commit();return n;}}}
   throw new Ex("Không tìm được tên khối bảng còn trống.");}
  // Live record of a symbol table by name (erased records are skipped: F1 of TVXD-MEPS-WIRES-010 applies to every table).
  static ObjectId LiveRecord(Transaction tr,SymbolTable t,string name){foreach(ObjectId id in t){if(id.IsErased)continue;if(Same(((SymbolTableRecord)tr.GetObject(id,OpenMode.ForRead)).Name,name))return id;}return ObjectId.Null;}
  static bool SameColor(Color a,Color b){if(a.ColorMethod!=b.ColorMethod)return false;return a.ColorMethod==ColorMethod.ByColor?a.Red==b.Red&&a.Green==b.Green&&a.Blue==b.Blue:a.ColorIndex==b.ColorIndex;}
  // 012 fix (native010: TextAt threw eNotApplicable on a simple dash): text/shape elements always carry a shape style, so a dash with a style on either side
  // makes the pair different (renamed, never shared) and TextAt is never read. Simple dashes compare length and shape number only.
  public static bool SameLinetype(LinetypeTableRecord a,LinetypeTableRecord b){if(a.NumDashes!=b.NumDashes||!Library.GeomEqual(a.PatternLength,b.PatternLength)||a.IsScaledToFit!=b.IsScaledToFit)return false;
   for(int i=0;i<a.NumDashes;i++){if(!a.ShapeStyleAt(i).IsNull||!b.ShapeStyleAt(i).IsNull)return false;if(!Library.GeomEqual(a.DashLengthAt(i),b.DashLengthAt(i))||a.ShapeNumberAt(i)!=b.ShapeNumberAt(i))return false;}return true;}
  // A target layer that is off, frozen or locked would hide or fade the legend: never shared.
  static bool SameLayer(Transaction ts,Transaction tt,LayerTableRecord a,LayerTableRecord b){if(b.IsOff||b.IsFrozen||b.IsLocked)return false;
   if(!SameColor(a.Color,b.Color)||a.LineWeight!=b.LineWeight||a.IsPlottable!=b.IsPlottable||!a.Transparency.Equals(b.Transparency))return false;
   return Same(((LinetypeTableRecord)ts.GetObject(a.LinetypeObjectId,OpenMode.ForRead)).Name,((LinetypeTableRecord)tt.GetObject(b.LinetypeObjectId,OpenMode.ForRead)).Name);}
  static bool SameBlock(Transaction ts,Transaction tt,BlockTableRecord a,BlockTableRecord b){var ma=LibraryCad.ReadMarker(ts,a);var mb=LibraryCad.ReadMarker(tt,b);
   return ma!=null&&mb!=null&&ma.Length>=6&&mb.Length>=6&&ma[5]==LibraryCad06.StyleMarker&&Enumerable.Range(0,6).All(i=>ma[i]==mb[i]);}
  // Linetypes first, so a renamed linetype makes the layers that use it differ too.
  static int Mangle(Database staging,Database target,List<string> notes){int n=0;
   using(var tt=target.TransactionManager.StartTransaction())using(var ts=staging.TransactionManager.StartTransaction()){
    n+=Pass(ts,tt,staging.LinetypeTableId,target.LinetypeTableId,(a,b)=>SameLinetype((LinetypeTableRecord)a,(LinetypeTableRecord)b),"Kiểu nét",notes);
    // 012: text styles are shared by LibraryCad06.SourceStyleKept(staging, target), the exact criterion Fonts verifies for a shared record after the clone
    // (CAD may resolve an empty source file name of a cloned style; Meps06.SameStyle alone then never matched the earlier _L011 copy).
    n+=Pass(ts,tt,staging.TextStyleTableId,target.TextStyleTableId,(a,b)=>LibraryCad06.SourceStyleKept(LibraryCad06.StyleInfo((TextStyleTableRecord)a),LibraryCad06.StyleInfo((TextStyleTableRecord)b)),"Kiểu chữ",notes);
    n+=Pass(ts,tt,staging.LayerTableId,target.LayerTableId,(a,b)=>SameLayer(ts,tt,(LayerTableRecord)a,(LayerTableRecord)b),"Layer",notes);
    n+=Pass(ts,tt,staging.DimStyleTableId,target.DimStyleTableId,(a,b)=>false,"Kiểu kích thước",notes);
    n+=Pass(ts,tt,staging.BlockTableId,target.BlockTableId,(a,b)=>SameBlock(ts,tt,(BlockTableRecord)a,(BlockTableRecord)b),"Khối",notes);
    ts.Commit();tt.Commit();}
   return n;}
  static int Pass(Transaction ts,Transaction tt,ObjectId stagingTable,ObjectId targetTable,Func<SymbolTableRecord,SymbolTableRecord,bool> same,string kind,List<string> notes){
   var st=(SymbolTable)ts.GetObject(stagingTable,OpenMode.ForRead);var tg=(SymbolTable)tt.GetObject(targetTable,OpenMode.ForRead);int n=0;var list=new List<ObjectId>();var taken=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
   foreach(ObjectId id in st){if(id.IsErased)continue;list.Add(id);taken.Add(((SymbolTableRecord)ts.GetObject(id,OpenMode.ForRead)).Name);}
   foreach(var id in list){var r=(SymbolTableRecord)ts.GetObject(id,OpenMode.ForRead);string name=r.Name;if(string.IsNullOrEmpty(name)||name.StartsWith("*",StringComparison.Ordinal)||Fixed.Contains(name)||r.IsDependent)continue;
    var b=r as BlockTableRecord;if(b!=null&&(b.IsLayout||b.IsAnonymous||b.IsFromExternalReference||b.IsFromOverlayReference))continue;
    if(!tg.Has(name))continue;var live=LiveRecord(tt,tg,name);if(!live.IsNull&&same(r,(SymbolTableRecord)tt.GetObject(live,OpenMode.ForRead)))continue;
    // 012 F012-REPEAT-STYLE: a live private variant (<name>_L011[_k]) left by an earlier placement is reused when the same full property comparison as for
    // the original name says equal (never by name alone); the staging record takes its name and the Ignore clone shares it. Only then a free name.
    string reuse=null,fresh=null;
    for(int k=1;k<1000&&reuse==null;k++){string c=Variant(name,k);if(c.Length>255||taken.Contains(c)||!tg.Has(c))continue;var v=LiveRecord(tt,tg,c);if(!v.IsNull&&same(r,(SymbolTableRecord)tt.GetObject(v,OpenMode.ForRead)))reuse=c;}
    if(reuse!=null){r.UpgradeOpen();r.Name=reuse;taken.Add(reuse);n++;notes.Add(kind+" "+name+" đã có trong bản vẽ với thuộc tính khác → bảng dùng lại tên riêng "+reuse+" đã có cùng thuộc tính (bản ghi có sẵn giữ nguyên).");continue;}
    for(int k=1;k<1000&&fresh==null;k++){string c=Variant(name,k);if(c.Length<=255&&!tg.Has(c)&&!taken.Contains(c))fresh=c;}
    if(fresh==null)throw new Ex(kind+" "+name+": không tìm được tên riêng còn trống; chưa đặt gì.");SymbolUtilityServices.ValidateSymbolName(fresh,false);
    r.UpgradeOpen();r.Name=fresh;taken.Add(fresh);n++;notes.Add(kind+" "+name+(live.IsNull?" chỉ còn bản ghi đã xoá trong bản vẽ":" đã có trong bản vẽ với thuộc tính khác")+" → bảng dùng tên riêng "+fresh+" (bản ghi có sẵn giữ nguyên).");}
   return n;}
  static string Variant(string name,int k){return name+Suffix+(k>1?"_"+k.ToString(CultureInfo.InvariantCulture):"");}
  static Dictionary<ObjectId,LegendStyle> Styles(Database db){var d=new Dictionary<ObjectId,LegendStyle>();using(var tr=db.TransactionManager.StartTransaction()){
    foreach(ObjectId id in (TextStyleTable)tr.GetObject(db.TextStyleTableId,OpenMode.ForRead))if(!id.IsErased)d[id]=LibraryCad06.StyleInfo((TextStyleTableRecord)tr.GetObject(id,OpenMode.ForRead));tr.Commit();}return d;}
  // Text styles reached by the clone: a NEWLY cloned record whose descriptor CAD dropped gets it back (NATIVE009); a shared (pre-existing) record is never
  // touched and must already be equal (Mangle). Every record must then keep its staging properties (LibraryCad06.SourceStyleKept).
  static int Fonts(Database target,IdMapping map,Dictionary<ObjectId,LegendStyle> styles,List<string> notes){int restored=0;var issues=new List<string>();
   using(var tr=target.TransactionManager.StartTransaction()){
    foreach(IdPair p in map){LegendStyle want;if(p.Value.IsNull||!styles.TryGetValue(p.Key,out want))continue;var r=(TextStyleTableRecord)tr.GetObject(p.Value,OpenMode.ForRead);if(LibraryCad06.SourceStyleKept(want,LibraryCad06.StyleInfo(r)))continue;
     if(!p.IsCloned){issues.Add("kiểu chữ có sẵn "+r.Name+" khác kiểu của bảng "+LibraryCad06.Describe(want));continue;}
     r.UpgradeOpen();LibraryCad06.RestoreFont(r,want);restored++;var got=LibraryCad06.StyleInfo(r);
     if(LibraryCad06.SourceStyleKept(want,got))notes.Add("Kiểu chữ "+r.Name+": đặt lại mô tả font sau khi chép (font nguồn giữ đúng).");else issues.Add("kiểu chữ "+r.Name+": cần "+LibraryCad06.Describe(want)+", sau khi chép "+LibraryCad06.Describe(got));}
    if(issues.Count>0)throw new Ex("Font nguồn không giữ đúng khi chép vào bản vẽ: "+string.Join("; ",issues.Take(4))+"; không đặt bảng.");
    tr.Commit();}
   return restored;}
  static string ColorText(Color c){return c.ColorMethod==ColorMethod.ByColor?"rgb"+c.Red+","+c.Green+","+c.Blue:c.ColorMethod+":"+c.ColorIndex;}
  // Live (non-erased) records with their properties, model-space object count and named-object-dictionary entries.
  public static HashSet<string> Snapshot(Database db){var s=new HashSet<string>(StringComparer.Ordinal);using(var tr=db.TransactionManager.StartTransaction()){
    foreach(ObjectId id in (LayerTable)tr.GetObject(db.LayerTableId,OpenMode.ForRead)){if(id.IsErased)continue;var l=(LayerTableRecord)tr.GetObject(id,OpenMode.ForRead);
     s.Add("layer "+l.Name+"|"+ColorText(l.Color)+"|"+(int)l.LineWeight+"|"+((LinetypeTableRecord)tr.GetObject(l.LinetypeObjectId,OpenMode.ForRead)).Name+"|"+l.IsPlottable+"|"+l.IsOff+"|"+l.IsFrozen+"|"+l.IsLocked);}
    foreach(ObjectId id in (LinetypeTable)tr.GetObject(db.LinetypeTableId,OpenMode.ForRead)){if(id.IsErased)continue;var t=(LinetypeTableRecord)tr.GetObject(id,OpenMode.ForRead);
     s.Add("linetype "+t.Name+"|"+Library.F(t.PatternLength)+"|"+string.Join(",",Enumerable.Range(0,t.NumDashes).Select(i=>Library.F(t.DashLengthAt(i)))));}
    foreach(ObjectId id in (TextStyleTable)tr.GetObject(db.TextStyleTableId,OpenMode.ForRead))if(!id.IsErased)s.Add("style "+LibraryCad06.Describe(LibraryCad06.StyleInfo((TextStyleTableRecord)tr.GetObject(id,OpenMode.ForRead))));
    foreach(ObjectId id in (DimStyleTable)tr.GetObject(db.DimStyleTableId,OpenMode.ForRead))if(!id.IsErased)s.Add("dimstyle "+((DimStyleTableRecord)tr.GetObject(id,OpenMode.ForRead)).Name);
    foreach(ObjectId id in (BlockTable)tr.GetObject(db.BlockTableId,OpenMode.ForRead)){if(id.IsErased)continue;var b=(BlockTableRecord)tr.GetObject(id,OpenMode.ForRead);
     s.Add(b.IsLayout?"block "+b.Name:"block "+b.Name+"|"+b.Cast<ObjectId>().Count(i=>!i.IsErased)+"|"+Library.F(b.Origin.X)+","+Library.F(b.Origin.Y)+","+Library.F(b.Origin.Z));}
    int n=0;foreach(ObjectId id in (BlockTableRecord)tr.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db),OpenMode.ForRead))if(!id.IsErased)n++;s.Add("Model "+n+" đối tượng");
    foreach(DBDictionaryEntry de in (DBDictionary)tr.GetObject(db.NamedObjectsDictionaryId,OpenMode.ForRead))if(!de.Value.IsErased)s.Add("NOD "+de.Key+" "+de.Value.Handle);tr.Commit();}
   return s;}
  // Verified rollback text: full rollback only when the live state equals the state before the call.
  static string Rollback(Database db,HashSet<string> before){HashSet<string> after;try{after=Snapshot(db);}catch(Ex a){return "CHƯA xác nhận được việc hoàn tác ("+a.Message+"); kiểm tra bản vẽ và dùng UNDO nếu cần";}
   var left=after.Where(x=>!before.Contains(x)).ToList();var lost=before.Where(x=>!after.Contains(x)).ToList();
   if(left.Count==0&&lost.Count==0)return "đã hoàn tác toàn bộ; bản vẽ không đổi (đã đối chiếu layer, kiểu nét, kiểu chữ, khối, Model, NOD)";
   return "CHƯA hoàn tác hết: còn "+(left.Count==0?"(không)":string.Join(", ",left.Take(8)))+(lost.Count>0?"; khác trước: "+string.Join(", ",lost.Take(8)):"")+"; kiểm tra bản vẽ và dùng UNDO nếu cần";}
 }
}
