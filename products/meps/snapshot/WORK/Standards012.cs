using System;using System.Linq;using System.Collections.Generic;using System.Globalization;
#if !STANDARDS_PURE
using Autodesk.AutoCAD.DatabaseServices;using Autodesk.AutoCAD.Geometry;using Autodesk.AutoCAD.Colors;using Ex=System.Exception;
#endif
namespace Tvxd.Meps {
 // TVXD-MEPS-LAYERS-012 drafting profile for CLEAN new drawings (user decision 2026-10-09). The pure part (no CAD types; TESTS/BUILD012.ps1 also compiles it
 // alone with /define:STANDARDS_PURE) holds the 20 layer rows of INPUT/T_CHUAN_20261009.lsp exactly as written there (name, ACI, linetype, lineweight in
 // 1/100 mm, plot flag, description, source line), read as DATA only; the LSP is never executed and is not read at runtime. Two recorded overlays:
 //  1. ACI 3 for every device layer (user decision); ACI 2 (WireOverlayAci) for the six wire-panel preset layers of PresetMap (TVXD-MEPS-WIREMENU-013,
 //     user decision: wires yellow). E-DAY wire symbols map to E-DAY-CS and are therefore yellow too. E-GHICHU, E-BANG, E-SDNL-GC and E-PHUTRO keep their
 //     source annotation/helper colours. The source ACI stays in sourceAci. Existing drawing layers are never recoloured (a same-name green layer conflicts).
 //  2. E-DH: an explicit MEPS category alias of E-TB's properties, so air-conditioning symbols get their own layer (drafting classification, no new
 //     engineering value). It is not a source row; its provenance says so.
 // Library categories map to exactly one profile layer (CategoryMap, visible in the guide and QA). An unknown category is refused with a "mapping required"
 // message; nothing is guessed. Wire presets are profile layers, drawn ByLayer; they are snapshots of the current drawing, never saved as user samples.
 public sealed class StdSourceRow012 {public int line,aci,weight,plot;public string name,linetype,description;}
 public sealed class StdLayer012 {public string name,linetype,description,provenance,aliasOf;public int sourceAci,aci,weight;public bool plot;}
 public sealed class WirePreset012 {public string layer,linetype,name,kind;public int aci,weight;public bool metric;}
 public static class Standards012 {
  public const string SourceFile="T_CHUAN_20261009.lsp",SourceSha256="E8384803DFDC6DF08C1BE12B1AB3B683F32FDD91F07A22E99D398E7AB09A3458",SourceVersion="1.0";
  public const string Profile="TVXD_T_CHUAN_DIEN_1.0+ACI3+WIRE-ACI2+E-DH_013",Continuous="Continuous",AliasName="E-DH",AliasOf="E-TB";
  // Seventh marker value of a definition normalised to a category layer (LibraryCad06.LayerStyle012); with the profile it never matches a 0.6 frozen definition.
  public const string BlockSignature="L012_CATEGORY_BYLAYER_V1";
  public const int OverlayAci=3,WireOverlayAci=2;
  static StdSourceRow012 R(int line,string name,int aci,string linetype,int weight,int plot,string description){return new StdSourceRow012{line=line,name=name,aci=aci,linetype=linetype,weight=weight,plot=plot,description=description};}
  // *T-LAYER-DIEN* of the source, lines 44–63, verbatim (TESTS/Test012.cs re-parses the LSP text and compares every field).
  public static readonly StdSourceRow012[] Source={
   R(44,"E-DEN",2,"Continuous",25,1,"Den chieu sang"),R(45,"E-QUAT",3,"Continuous",25,1,"Quat tran, quat tuong"),R(46,"E-CT",4,"Continuous",25,1,"Cong tac"),
   R(47,"E-OC",5,"Continuous",25,1,"O cam"),R(48,"E-TU",1,"Continuous",30,1,"Tu dien"),R(49,"E-TB",6,"Continuous",25,1,"Thiet bi dien khac"),
   R(50,"E-DAY-CS",2,"Continuous",25,1,"Day nhom chieu sang"),R(51,"E-DAY-OC",5,"Continuous",25,1,"Day nhom o cam"),R(52,"E-DAY-DL",1,"Continuous",30,1,"Day dieu hoa, dong luc"),
   R(53,"E-DAY-TRUC",1,"Continuous",35,1,"Cap truc, cap cap nguon"),R(54,"E-ONG",8,"HIDDEN",13,1,"Ong luon"),R(55,"E-TIEPDIA",3,"DASHED",25,1,"Tiep dia"),
   R(56,"E-CHONGSET",30,"Continuous",25,1,"Chong set"),R(57,"E-NHE",4,"Continuous",18,1,"Dien nhe: mang, camera, dien thoai"),R(58,"E-BAOCHAY",1,"Continuous",25,1,"Bao chay"),
   R(59,"E-GHICHU",7,"Continuous",13,1,"Chu ghi chu, nhan, ma mach"),R(60,"E-BANG",7,"Continuous",18,1,"Bang bieu, khung bang"),R(61,"E-SDNL-TB",2,"Continuous",30,1,"So do nguyen ly, net thiet bi"),
   R(62,"E-SDNL-GC",8,"Continuous",13,1,"So do nguyen ly, net ke bang"),R(63,"E-PHUTRO",8,"Continuous",9,0,"Net giong, net dung - KHONG IN")};
  public static readonly string[] KeepSourceColor={"E-GHICHU","E-BANG","E-SDNL-GC","E-PHUTRO"};
  // Library category (Library.Category: name prefix) → profile layer. E-AT and E-X have no own source layer: generic E-TB ("Thiet bi dien khac").
  public static readonly string[][] CategoryMap={new[]{"E-DEN","E-DEN"},new[]{"E-QUAT","E-QUAT"},new[]{"E-CT","E-CT"},new[]{"E-OC","E-OC"},new[]{"E-TU","E-TU"},new[]{"E-NHE","E-NHE"},
   new[]{"E-DH","E-DH"},new[]{"E-DAY","E-DAY-CS"},new[]{"E-SDNL","E-SDNL-TB"},new[]{"E-AT","E-TB"},new[]{"E-X","E-TB"}};
  // Wire / pipe / earth / trunk presets of the compact wire panel, in display order.
  public static readonly string[][] PresetMap={new[]{"E-DAY-CS","Dây chiếu sáng"},new[]{"E-DAY-OC","Dây ổ cắm"},new[]{"E-DAY-DL","Dây điều hoà, động lực"},new[]{"E-DAY-TRUC","Cáp trục, cấp nguồn"},
   new[]{"E-ONG","Ống luồn"},new[]{"E-TIEPDIA","Tiếp địa"}};
  static bool Same(string a,string b){return string.Equals(a,b,StringComparison.OrdinalIgnoreCase);}
  static List<StdLayer012> layers;
  // The 21 profile layers: 20 source rows (ACI overlay applied) + the E-DH alias. Validated once.
  public static List<StdLayer012> Layers(){if(layers!=null)return layers;var l=new List<StdLayer012>();
   foreach(var s in Source){bool keep=KeepSourceColor.Any(k=>Same(k,s.name)),wire=PresetMap.Any(p=>Same(p[0],s.name));int aci=keep?s.aci:wire?WireOverlayAci:OverlayAci;
    l.Add(new StdLayer012{name=s.name,linetype=s.linetype,description=s.description,sourceAci=s.aci,aci=aci,weight=s.weight,plot=s.plot==1,
     provenance=SourceFile+" dòng "+s.line+" (sha256 "+SourceSha256.Substring(0,12)+"…, T_CHUAN "+SourceVersion+")"+(keep?"; màu nguồn giữ nguyên":"; màu ACI "+aci+(wire?" (nét dây vàng)":"")+" theo quyết định người dùng (nguồn ACI "+s.aci+")")});}
   var tb=l.Single(x=>Same(x.name,AliasOf));
   l.Add(new StdLayer012{name=AliasName,linetype=tb.linetype,description="Dieu hoa (MEPS: alias thuoc tinh cua E-TB)",sourceAci=tb.sourceAci,aci=tb.aci,weight=tb.weight,plot=tb.plot,aliasOf=AliasOf,
    provenance="Không có trong "+SourceFile+": nhóm MEPS E-DH dùng đúng thuộc tính của E-TB (dòng 49) để tách điều hoà; phân loại bản vẽ, không phải giá trị kỹ thuật mới"});
#if CABLES016
   // TVXD-MEPS-CABLES-016: + the 19 user cable layers (Cable016, provenance USER_CABLE_CATALOG_016), drafting aliases of E-DAY-CS; the 21 rows above unchanged.
   var cs=l.Single(x=>Same(x.name,Cable016.AliasOf));
   foreach(var c in Cable016.Layers()){if(!Same(c.linetype,cs.linetype)||c.aci!=cs.aci||c.weight!=cs.weight||c.plot!=cs.plot||c.sourceAci!=cs.sourceAci)throw new InvalidOperationException("Layer cáp "+c.name+" không còn khớp thuộc tính "+cs.name+".");l.Add(c);}
   // TVXD-MEPS-CABLES-024: count follows the catalogue (21 + 26 = 47); a fixed 40 would refuse the 7 new XLPE 4-core rows at runtime.
   if(l.Count!=21+Cable016.Count)throw new InvalidOperationException("Hồ sơ layer phải có "+(21+Cable016.Count)+" layer (21 + "+Cable016.Count+" cáp).");
#endif
   if(l.Select(x=>x.name.ToUpperInvariant()).Distinct().Count()!=l.Count)throw new InvalidOperationException("Hồ sơ layer trùng tên.");
   int[] weights={0,5,9,13,15,18,20,25,30,35,40,50,53,60,70,80,90,100,106,120,140,158,200,211};
   foreach(var x in l)if(!weights.Contains(x.weight)||x.aci<1||x.aci>255||string.IsNullOrEmpty(x.linetype))throw new InvalidOperationException("Hồ sơ layer "+x.name+" không hợp lệ.");
   foreach(var target in CategoryMap.Select(m=>m[1]).Concat(PresetMap.Select(m=>m[0])))if(!l.Any(x=>Same(x.name,target)))throw new InvalidOperationException("Ánh xạ trỏ tới layer ngoài hồ sơ: "+target+".");
   layers=l;return l;}
  public static StdLayer012 Layer(string name){var s=Layers().FirstOrDefault(x=>Same(x.name,name));if(s==null)throw new InvalidOperationException("Layer "+name+" không thuộc hồ sơ "+Profile+".");return s;}
  public static bool NeedsMetric(StdLayer012 s){return !Same(s.linetype,Continuous);}
  // Exact category → layer; no fallback for a category that is not listed.
  public static string LayerFor(string category){var m=CategoryMap.FirstOrDefault(x=>Same(x[0],category));
   if(m==null)throw new InvalidOperationException("Nhóm ký hiệu '"+(category??"(trống)")+"' chưa có ánh xạ layer trong hồ sơ "+Profile+"; cần bổ sung ánh xạ (MEPS không tự đoán layer). Chưa đặt gì.");return Layer(m[1]).name;}
  public static string MappingText(){return string.Join("; ",CategoryMap.Select(m=>m[0]+" → "+m[1]));}
#if CABLES016
  // 016: the selector offers the 19 user cables only (exact labels, grouped by kind). PresetMap keeps its six rows for identity/legacy classification.
  public static List<WirePreset012> Presets(){var list=Cable016.Presets();foreach(var p in list){var s=Layer(p.layer);if(!Same(s.linetype,p.linetype)||s.aci!=p.aci||s.weight!=p.weight)throw new InvalidOperationException("Nét cáp "+p.layer+" khác hồ sơ.");}return list;}
  public static List<WirePreset012> LegacyPresets(){return PresetMap.Select(p=>{var s=Layer(p[0]);return new WirePreset012{layer=s.name,linetype=s.linetype,kind=p[1],name=p[1]+" · "+s.name,aci=s.aci,weight=s.weight,metric=NeedsMetric(s)};}).ToList();}
#else
  public static List<WirePreset012> Presets(){return PresetMap.Select(p=>{var s=Layer(p[0]);return new WirePreset012{layer=s.name,linetype=s.linetype,kind=p[1],name=p[1]+" · "+s.name,aci=s.aci,weight=s.weight,metric=NeedsMetric(s)};}).ToList();}
#endif
 }
#if !STANDARDS_PURE
 // Result of one Ensure: created / matched layers, conflicts (same name, other properties: left untouched, the affected action is refused), skipped (needs
 // a metric dashed linetype in a drawing that is not mm), errors (could not load/validate). Complete only when all 21 layers are present and equal.
 public sealed class StdReport012 {public string units;public bool metric;public List<string> created=new List<string>(),matched=new List<string>(),conflicts=new List<string>(),skipped=new List<string>(),errors=new List<string>(),linetypes=new List<string>();
  public bool Complete{get{return conflicts.Count==0&&skipped.Count==0&&errors.Count==0;}}
  public string Summary(){return (Complete?"HOÀN TẤT":"CHƯA ĐỦ")+": tạo "+created.Count+", có sẵn đúng "+matched.Count+", xung đột "+conflicts.Count+", bỏ qua "+skipped.Count+", lỗi "+errors.Count+" / "+Standards012.Layers().Count+" layer (INSUNITS "+units+")"
   +(linetypes.Count>0?"; kiểu nét: "+string.Join("; ",linetypes):"")+(conflicts.Count>0?"; XUNG ĐỘT: "+string.Join("; ",conflicts.Take(4)):"")+(skipped.Count>0?"; BỎ QUA: "+string.Join("; ",skipped.Take(2)):"")+(errors.Count>0?"; LỖI: "+string.Join("; ",errors.Take(3)):"");}}
 // Preset intent at the click: profile layer and the drawing's units / LTSCALE / MSLTSCALE / annotation scale. Drawing is refused when any of these changed.
 public sealed class PresetSnapshot012 {public string layer,linetype,profile,document;public int units;public double ltscale;public WireSettings010 settings;}
 // AutoCAD side (database only, no UI: the same code runs in AutoCAD and in CoreConsole QA). Writes only through a transaction of the given database and
 // never changes CLAYER, INSUNITS, LTSCALE, text styles, plot settings or an existing layer / linetype / block definition.
 public static class StandardsCad012 {
  public const string MetricFile="acadiso.lin";
  static bool Same(string a,string b){return string.Equals(a,b,StringComparison.OrdinalIgnoreCase);}
  static Color ByLayer{get{return Color.FromColorIndex(ColorMethod.ByLayer,256);}}
  public static bool Metric(Database db){return db.Insunits==UnitsValue.Millimeters;}
  sealed class TypeState {public string kind,text;}
  // Simple dash pattern [length, dash…]; null when the record has shapes/text or is scaled to fit (never equal to a profile linetype).
  public static double[] Pattern(LinetypeTableRecord r){if(r.IsScaledToFit)return null;var d=new double[r.NumDashes+1];d[0]=r.PatternLength;
   for(int i=0;i<r.NumDashes;i++){if(!r.ShapeStyleAt(i).IsNull||r.ShapeNumberAt(i)!=0)return null;d[i+1]=r.DashLengthAt(i);}return d;}
  public static bool SamePattern(double[] a,double[] b){return a!=null&&b!=null&&a.Length==b.Length&&Enumerable.Range(0,a.Length).All(i=>Library.GeomEqual(a[i],b[i]));}
  static string Text(double[] p){return p==null?"(có shape/chữ)":string.Join(",",p.Select(x=>Library.F(x)));}
  // Reference pattern: AutoCAD's own loader reads acadiso.lin into an owned side database. A failure is reported; acad.lin and Continuous are never used instead.
  public static double[] MetricReference(string name){using(var side=new Database(true,true)){
    try{side.LoadLineTypeFile(name,MetricFile);}catch(Ex ex){throw new InvalidOperationException("Không nạp được kiểu nét "+name+" từ "+MetricFile+" ("+ex.Message+"); MEPS không thay bằng acad.lin hay Continuous.",ex);}
    double[] p;using(var tr=side.TransactionManager.StartTransaction()){var id=WireCad010.Live(tr,(LinetypeTable)tr.GetObject(side.LinetypeTableId,OpenMode.ForRead),name);
     if(id.IsNull)throw new InvalidOperationException("Đã gọi nạp "+name+" từ "+MetricFile+" nhưng không thấy bản ghi; MEPS không thay nét khác.");p=Pattern((LinetypeTableRecord)tr.GetObject(id,OpenMode.ForRead));tr.Commit();}
    if(p==null)throw new InvalidOperationException("Kiểu nét "+name+" trong "+MetricFile+" có shape/chữ; hồ sơ chỉ nhận nét gạch đơn.");return p;}}
  static string Diff(Transaction tr,LayerTableRecord l,StdLayer012 s){var d=new List<string>();
   if(!(l.Color.ColorMethod==ColorMethod.ByAci&&l.Color.ColorIndex==s.aci))d.Add("màu "+(l.Color.ColorMethod==ColorMethod.ByAci?"ACI "+l.Color.ColorIndex:l.Color.ToString())+" ≠ ACI "+s.aci);
   string lt=((LinetypeTableRecord)tr.GetObject(l.LinetypeObjectId,OpenMode.ForRead)).Name;if(!Same(lt,s.linetype))d.Add("kiểu nét "+lt+" ≠ "+s.linetype);
   if((int)l.LineWeight!=s.weight)d.Add("độ đậm "+(int)l.LineWeight+" ≠ "+s.weight);if(l.IsPlottable!=s.plot)d.Add("in "+(l.IsPlottable?"có":"không")+" ≠ "+(s.plot?"có":"không"));return string.Join(", ",d);}
  // Profile layers into db, inside ONE transaction (nested in the caller's transaction when there is one, so the caller's abort also undoes it):
  //  existing equal layer → kept; existing different layer → conflict, left untouched; missing → created with the profile values and description.
  //  Dashed linetypes: only in a mm drawing; an existing record must equal acadiso.lin (else conflict), a missing one is loaded from acadiso.lin with
  //  Database.LoadLineTypeFile and read back. No table is open while it loads. Other drawings: skipped with the reason, units unchanged.
  // required (a layer the caller is about to use): when it is not present-and-equal afterwards the whole Ensure is undone and the reason is thrown.
  public static StdReport012 Ensure(Database db,string required){var rep=new StdReport012{units=db.Insunits.ToString(),metric=Metric(db)};var profile=Standards012.Layers();
   if(required!=null)Standards012.Layer(required);
   var all=db.TransactionManager.StartTransaction();
   try{var missing=new List<StdLayer012>();var types=new Dictionary<string,TypeState>(StringComparer.OrdinalIgnoreCase);var load=new List<string>();var why=new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);
    // 1. read only
    using(var tr=db.TransactionManager.StartTransaction()){var lt=(LayerTable)tr.GetObject(db.LayerTableId,OpenMode.ForRead);var tt=(LinetypeTable)tr.GetObject(db.LinetypeTableId,OpenMode.ForRead);
     Func<string,TypeState> type=name=>{TypeState t;if(types.TryGetValue(name,out t))return t;
      if(!rep.metric)t=new TypeState{kind="skip",text="bản vẽ INSUNITS="+db.Insunits+" (không phải mm): không chuẩn bị được nét "+name+" theo "+MetricFile+"; MEPS không đổi đơn vị"};
      else{double[] reference=null;try{reference=MetricReference(name);}catch(Ex ex){t=new TypeState{kind="error",text=ex.Message};}
       if(t==null){var id=WireCad010.Live(tr,tt,name);if(id.IsNull){t=new TypeState{kind="load"};load.Add(name);}
        else{var p=Pattern((LinetypeTableRecord)tr.GetObject(id,OpenMode.ForRead));t=SamePattern(p,reference)?new TypeState{kind="ok"}:new TypeState{kind="conflict",text="kiểu nét "+name+" có sẵn khác "+MetricFile+" ("+Text(p)+" ≠ "+Text(reference)+")"};
         if(t.kind=="ok")rep.linetypes.Add(name+" có sẵn, đúng "+MetricFile);}}}
      types[name]=t;return t;};
     foreach(var s in profile){var id=WireCad010.Live(tr,lt,s.name);TypeState t=Standards012.NeedsMetric(s)?type(s.linetype):null;
      if(!id.IsNull){var l=(LayerTableRecord)tr.GetObject(id,OpenMode.ForRead);string d=Diff(tr,l,s);
       if(d.Length==0&&t!=null&&t.kind!="ok")d=t.kind=="load"?"kiểu nét "+s.linetype+" của layer không còn bản ghi sống":t.text;
       if(d.Length==0)rep.matched.Add(s.name);else{string w=s.name+": "+d+" (giữ nguyên layer có sẵn)";why[s.name]=w;if(t!=null&&t.kind=="skip"&&Diff(tr,l,s).Length==0)rep.skipped.Add(w);else rep.conflicts.Add(w);}continue;}
      if(t==null||t.kind=="ok"||t.kind=="load"){missing.Add(s);continue;}
      string m=s.name+": "+t.text;why[s.name]=m;if(t.kind=="skip")rep.skipped.Add(m);else if(t.kind=="conflict")rep.conflicts.Add(m+" — layer chưa tạo");else rep.errors.Add(m);}
     tr.Commit();}
    // 2. missing dashed linetypes (no table open in this transaction tree)
    foreach(var name in load.Where(n=>missing.Any(q=>Same(q.linetype,n)))){string err=null;try{db.LoadLineTypeFile(name,MetricFile);}catch(Ex ex){err=ex.Message;}
     if(err==null){using(var tr=db.TransactionManager.StartTransaction()){var id=WireCad010.Live(tr,(LinetypeTable)tr.GetObject(db.LinetypeTableId,OpenMode.ForRead),name);
       if(id.IsNull)err="không thấy bản ghi sau khi nạp";else if(!SamePattern(Pattern((LinetypeTableRecord)tr.GetObject(id,OpenMode.ForRead)),MetricReference(name)))err="bản ghi nạp được khác tham chiếu";tr.Commit();}}
     if(err==null){rep.linetypes.Add(name+" nạp từ "+MetricFile);continue;}
     foreach(var s in missing.Where(x=>Same(x.linetype,name)).ToList()){string m=s.name+": không nạp được kiểu nét "+name+" từ "+MetricFile+" ("+err+"); MEPS không thay bằng acad.lin hay Continuous";why[s.name]=m;rep.errors.Add(m);missing.Remove(s);}}
    // 3. create, then read back
    if(missing.Count>0)using(var tr=db.TransactionManager.StartTransaction()){var lt=(LayerTable)tr.GetObject(db.LayerTableId,OpenMode.ForWrite);var tt=(LinetypeTable)tr.GetObject(db.LinetypeTableId,OpenMode.ForRead);var made=new List<KeyValuePair<ObjectId,StdLayer012>>();
     foreach(var s in missing){var type=WireCad010.Live(tr,tt,s.linetype);if(type.IsNull)throw new InvalidOperationException("Thiếu kiểu nét "+s.linetype+" cho layer "+s.name+".");
      var rec=new LayerTableRecord{Name=s.name,Color=Color.FromColorIndex(ColorMethod.ByAci,(short)s.aci),LinetypeObjectId=type,LineWeight=(LineWeight)s.weight,IsPlottable=s.plot};var id=lt.Add(rec);tr.AddNewlyCreatedDBObject(rec,true);rec.Description=s.description;made.Add(new KeyValuePair<ObjectId,StdLayer012>(id,s));}
     foreach(var kv in made){var l=(LayerTableRecord)tr.GetObject(kv.Key,OpenMode.ForRead);string d=Diff(tr,l,kv.Value);if(d.Length>0||l.Description!=kv.Value.description)throw new InvalidOperationException("Layer "+kv.Value.name+" tạo ra không đúng hồ sơ ("+d+"); không tạo layer nào.");rep.created.Add(kv.Value.name);}
     tr.Commit();}
    if(required!=null&&!rep.created.Concat(rep.matched).Any(n=>Same(n,required))){string w;if(!why.TryGetValue(required,out w))w=required+": chưa sẵn sàng";throw new InvalidOperationException("Layer chuẩn "+w+". Chưa thay đổi bản vẽ.");}
    all.Commit();return rep;}
   finally{all.Dispose();}}
  // A profile layer the caller will draw on: present, live, not xref-dependent, on, thawed and unlocked (MEPS never switches a layer on or unlocks it).
  public static ObjectId Usable(Transaction tr,Database db,string name){var id=WireCad010.Live(tr,(LayerTable)tr.GetObject(db.LayerTableId,OpenMode.ForRead),name);if(id.IsNull)throw new InvalidOperationException("Chưa có layer chuẩn "+name+".");
   var l=(LayerTableRecord)tr.GetObject(id,OpenMode.ForRead);if(l.IsOff||l.IsFrozen||l.IsLocked||l.IsDependent)throw new InvalidOperationException("Layer chuẩn "+name+" đang "+(l.IsLocked?"khóa":l.IsFrozen?"đóng băng":l.IsOff?"tắt":"phụ thuộc xref")+"; MEPS không tự bật/mở khóa. Chưa thay đổi bản vẽ.");return id;}
  static string EntityIssue(Database db,Entity e,ObjectId layer){if(e.LayerId!=layer)return e.GetType().Name+" "+e.Handle+" trên layer "+e.Layer;if(e.Color.ColorMethod!=ColorMethod.ByLayer)return e.GetType().Name+" "+e.Handle+" màu "+e.Color;
   if(e.LinetypeId!=db.ByLayerLinetype)return e.GetType().Name+" "+e.Handle+" kiểu nét "+e.Linetype;if(e.LineWeight!=LineWeight.ByLayer)return e.GetType().Name+" "+e.Handle+" độ đậm "+(int)e.LineWeight;return null;}
  static IEnumerable<ObjectId> Children(Entity e){var br=e as BlockReference;if(br!=null)foreach(ObjectId a in br.AttributeCollection)yield return a;
   if(e is Polyline2d||e is Polyline3d||e is PolyFaceMesh||e is PolygonMesh)foreach(ObjectId v in (System.Collections.IEnumerable)e)yield return v;}
  // First object of a definition closure (entities, attribute definitions, nested attribute references, old-polyline vertices) that is not on layer with
  // Color/Linetype/Lineweight ByLayer; null when the whole closure is normalised.
  public static string ClosureIssue(Transaction tr,Database db,ObjectId root,ObjectId layer){foreach(var bid in LibraryCad.Closure(tr,root))foreach(ObjectId id in (BlockTableRecord)tr.GetObject(bid,OpenMode.ForRead)){
    var e=tr.GetObject(id,OpenMode.ForRead) as Entity;if(e==null)continue;string w=EntityIssue(db,e,layer);if(w!=null)return w;foreach(var c in Children(e)){var x=tr.GetObject(c,OpenMode.ForRead) as Entity;if(x==null)continue;w=EntityIssue(db,x,layer);if(w!=null)return w;}}
   return null;}
  static void Paint(Transaction tr,Database db,ObjectId id,ObjectId layer){var e=tr.GetObject(id,OpenMode.ForWrite,false,true) as Entity;if(e==null)return;if(e is ProxyEntity)throw new InvalidOperationException("Khối có proxy entity, không chuẩn hoá layer được; không đặt.");
   e.LayerId=layer;e.Color=ByLayer;e.LinetypeId=db.ByLayerLinetype;e.LineWeight=LineWeight.ByLayer;
   var dim=e as Dimension;if(dim!=null){dim.Dimclrd=ByLayer;dim.Dimclre=ByLayer;dim.Dimclrt=ByLayer;dim.Dimltype=db.ByLayerLinetype;dim.Dimltex1=db.ByLayerLinetype;dim.Dimltex2=db.ByLayerLinetype;dim.Dimlwd=LineWeight.ByLayer;dim.Dimlwe=LineWeight.ByLayer;}
   foreach(var c in Children(e).ToList())Paint(tr,db,c,layer);}
  static Database Read(string path){var db=new Database(false,true);try{db.ReadDwgFile(path,FileOpenMode.OpenForReadAndAllShare,true,"");db.CloseInput(true);return db;}catch{db.Dispose();throw;}}
  public static ObjectId FindDef(Database db,string name){using(var tr=db.TransactionManager.StartTransaction()){var bt=(BlockTable)tr.GetObject(db.BlockTableId,OpenMode.ForRead);if(!bt.Has(name))throw new InvalidOperationException("File thư viện không chứa khối "+name+".");var id=bt[name];tr.Commit();return id;}}
  // In the side copy (never saved): every object of the closure goes to the category layer with Color/Linetype/Lineweight ByLayer. Geometry, texts, fonts,
  // attribute data, widths, transforms and extents are then compared with the untouched library file (LibraryCad.VerifyShape: layer and definition names
  // left out) and the closure is read back; any difference stops the insertion before anything reaches the drawing.
  public static void NormalizeSide(Database side,ObjectId root,string file,string entryName,StdLayer012 s,List<string> notes){if(Standards012.NeedsMetric(s))throw new InvalidOperationException("Layer nhóm ký hiệu "+s.name+" không phải nét liền; hồ sơ 012 chỉ đặt khối trên layer nét liền.");
   ObjectId lid;
   using(var tr=side.TransactionManager.StartTransaction()){var lt=(LayerTable)tr.GetObject(side.LayerTableId,OpenMode.ForRead);lid=WireCad010.Live(tr,lt,s.name);
    if(lid.IsNull){var cont=WireCad010.Live(tr,(LinetypeTable)tr.GetObject(side.LinetypeTableId,OpenMode.ForRead),Standards012.Continuous);lt.UpgradeOpen();var rec=new LayerTableRecord{Name=s.name,Color=Color.FromColorIndex(ColorMethod.ByAci,(short)s.aci),LinetypeObjectId=cont,LineWeight=(LineWeight)s.weight,IsPlottable=s.plot};lid=lt.Add(rec);tr.AddNewlyCreatedDBObject(rec,true);}
    foreach(var bid in LibraryCad.Closure(tr,root))foreach(ObjectId id in (BlockTableRecord)tr.GetObject(bid,OpenMode.ForRead))Paint(tr,side,id,lid);tr.Commit();}
   using(var tr=side.TransactionManager.StartTransaction()){string w=ClosureIssue(tr,side,root,lid);if(w!=null)throw new InvalidOperationException("Chuẩn hoá layer chưa đủ ("+w+"); không đặt.");tr.Commit();}
   using(var pristine=Read(file)){var diff=LibraryCad.VerifyShape(pristine,FindDef(pristine,entryName),side,root);if(diff.Count>0)throw new InvalidOperationException("Chuẩn hoá layer làm khác hình/chữ của bản chép ("+diff[0]+"); không đặt.");}
   notes.Add("Khối "+entryName+": mọi đối tượng trong khối trên layer "+s.name+", màu/nét/độ đậm ByLayer; hình, chữ, font, bề rộng giữ nguyên (đã đối chiếu file thư viện).");}
  static string Rollback(Database db,HashSet<string> before){HashSet<string> after;try{after=LegendPlace011.Snapshot(db);}catch(Ex a){return "CHƯA xác nhận được việc hoàn tác ("+a.Message+"); kiểm tra bản vẽ và dùng UNDO nếu cần";}
   var left=after.Where(x=>!before.Contains(x)).ToList();var lost=before.Where(x=>!after.Contains(x)).ToList();
   if(left.Count==0&&lost.Count==0)return "đã hoàn tác toàn bộ (layer, kiểu nét, khối, kiểu chữ, Model, NOD); bản vẽ không đổi (đã đối chiếu)";
   return "CHƯA hoàn tác hết: còn "+(left.Count==0?"(không)":string.Join(", ",left.Take(8)))+(lost.Count>0?"; khác trước: "+string.Join(", ",lost.Take(8)):"")+"; kiểm tra bản vẽ và dùng UNDO nếu cần";}
  static void Undoable(Database db){if(!db.UndoRecording)throw new InvalidOperationException("Bản vẽ đang tắt ghi UNDO (UndoRecording=false) nên không hoàn tác được nếu lỗi giữa chừng; chưa thay đổi gì. Bật lại UNDO (UNDO → All) rồi thử lại.");}
  // Product insertion (TVXD-MEPS-LAYERS-012): category → profile layer (unknown category refused), profile layers ensured, target layer usable, then
  // LibraryCad06.Insert with a private normalised definition (marker value 7 = BlockSignature|layer|profile; reused only when its closure is still
  // normalised to that layer), the reference on the category layer ByLayer; metadata (e.g. the ratio record) last. ONE outer transaction: any failure
  // aborts everything (layers, loaded linetypes, definitions, styles, reference, metadata) and the live state is compared with the state before.
  public static ObjectId InsertAtomic(Database db,string root,LibEntry e,Point3d position,double scale,double rotation,List<string> notes,Action<Transaction,Database> metadata,out StdReport012 report){
   if(notes==null)notes=new List<string>();report=null;var spec=Standards012.Layer(Standards012.LayerFor(e.category));
#if IDENTITY015
   // 015: readable root name = the entry's stable code (refused before any write when the catalog no longer matches the frozen codebook).
   string code=Identity015.BlockCode(e);
#endif
   Undoable(db);
   var before=LegendPlace011.Snapshot(db);
   if(metadata!=null)try{using(var dry=db.TransactionManager.StartTransaction()){metadata(dry,db);dry.Abort();}}catch(Ex ex){throw new InvalidOperationException(ex.Message+" — lỗi dữ liệu bản vẽ, chưa chèn gì; "+Rollback(db,before)+".",ex);}
   var outer=db.TransactionManager.StartTransaction();
   try{report=Ensure(db,spec.name);ObjectId lid;using(var tr=db.TransactionManager.StartTransaction()){lid=Usable(tr,db,spec.name);tr.Commit();}
    var style=new LibraryCad06.LayerStyle012{layer=lid,signature=Standards012.BlockSignature+"|"+spec.name+"|"+Standards012.Profile,normalize=(side,sideRoot,file,n)=>NormalizeSide(side,sideRoot,file,e.name,spec,n),reuseIssue=(tr,def)=>ClosureIssue(tr,db,def,lid)};
#if IDENTITY015
    // The identity schema is part of the signature, so only a 015 definition of this entry is ever reused under its readable name.
    style.rootName=code;style.signature+="|"+Identity015.Schema+"_"+Identity015.Version;
#endif
    var id=LibraryCad06.Insert(db,root,e,position,scale,rotation,notes,style);
    using(var tr=db.TransactionManager.StartTransaction()){var br=(BlockReference)tr.GetObject(id,OpenMode.ForRead);string w=EntityIssue(db,br,lid);if(w==null)foreach(var c in Children(br)){w=EntityIssue(db,(Entity)tr.GetObject(c,OpenMode.ForRead),lid);if(w!=null)break;}
     if(w==null)w=ClosureIssue(tr,db,br.BlockTableRecord,lid);if(w!=null)throw new InvalidOperationException("Khối vừa đặt chưa đúng layer "+spec.name+"/ByLayer ("+w+"); huỷ đặt.");tr.Commit();}
#if IDENTITY015
    // Type code on the reference, in the same outer transaction (an abort or UNDO removes it with the insertion).
    IdentityCad015.MarkBlock(outer,(BlockReference)outer.GetObject(id,OpenMode.ForWrite),e);notes.Add("Mã ký hiệu "+code+" (mã loại, không phải số thứ tự thiết bị).");
#endif
    if(metadata!=null)metadata(outer,db);outer.Commit();return id;}
   catch(Ex ex){string undo;try{if(!outer.IsDisposed)outer.Abort();undo=Rollback(db,before);}catch(Ex a){undo="CHƯA xác nhận được việc hoàn tác ("+a.Message+"); kiểm tra bản vẽ và dùng UNDO nếu cần";}
    throw new InvalidOperationException(ex.Message+" — "+undo+".",ex);}
   finally{if(!outer.IsDisposed)outer.Dispose();}}
  // ---- wire presets ----
  public static PresetSnapshot012 Snapshot(Database db,string document,string layer,WireSettings010 settings){WireModel010.Validate(settings);
   bool known=Standards012.PresetMap.Any(p=>Same(p[0],layer));
#if CABLES016
   // 016: one of the six legacy presets OR one of the 19 cables; nothing else (no reinterpretation of other drawing layers).
   known=known||Cable016.Find(layer)!=null;
#endif
   if(!known)throw new InvalidOperationException("Layer "+layer+" không phải nét chuẩn của bảng nét dây.");var s=Standards012.Layer(layer);
   if(Standards012.NeedsMetric(s)&&!Metric(db))throw new InvalidOperationException("Bản vẽ INSUNITS="+db.Insunits+" (không phải mm): không chuẩn bị được nét "+s.linetype+" của "+s.name+" theo "+MetricFile+"; MEPS không đổi đơn vị. Chưa vẽ.");
   return new PresetSnapshot012{layer=s.name,linetype=s.linetype,profile=Standards012.Profile,document=document,units=(int)db.Insunits,ltscale=db.Ltscale,settings=new WireSettings010{msltscale=settings.msltscale,annoScale=settings.annoScale,annoName=settings.annoName}};}
  public static void CheckSnapshot(Database db,PresetSnapshot012 p,WireSettings010 now){if(p==null||p.profile!=Standards012.Profile)throw new InvalidOperationException("Yêu cầu nét chuẩn không thuộc hồ sơ hiện hành; bấm lại nét.");
   if((int)db.Insunits!=p.units)throw new InvalidOperationException("INSUNITS đã đổi sau khi bấm nét chuẩn; bấm lại. Chưa vẽ.");if(!Library.GeomEqual(db.Ltscale,p.ltscale))throw new InvalidOperationException("LTSCALE đã đổi sau khi bấm nét chuẩn; bấm lại. Chưa vẽ.");
   if(!WireModel010.SameSettings(p.settings,now))throw new InvalidOperationException("MSLTSCALE/tỷ lệ chú thích đã đổi sau khi bấm nét chuẩn; bấm lại. Chưa vẽ.");}
  // One LWPOLYLINE on the preset's profile layer, Color/Linetype/Lineweight ByLayer, entity linetype scale 1 (appearance follows the layer and the
  // drawing's LTSCALE/MSLTSCALE captured in the snapshot). Same transaction rules as InsertAtomic.
  public static ObjectId DrawPreset(Database db,PresetSnapshot012 p,IList<Point3d> points,WireSettings010 now){CheckSnapshot(db,p,now);
   if(points==null||points.Count<2)throw new InvalidOperationException("Cần ít nhất hai điểm.");
   for(int i=0;i<points.Count;i++){var q=points[i];if(!WireModel010.Finite(q.X)||!WireModel010.Finite(q.Y)||!WireModel010.Finite(q.Z)||Math.Abs(q.Z)>1e-8)throw new InvalidOperationException("Chỉ hỗ trợ điểm trên XY, cao độ 0.");if(i>0&&q.DistanceTo(points[i-1])<1e-8)throw new InvalidOperationException("Hai điểm liên tiếp trùng nhau.");}
   Undoable(db);var before=LegendPlace011.Snapshot(db);var outer=db.TransactionManager.StartTransaction();
   try{Ensure(db,p.layer);ObjectId id;using(var tr=db.TransactionManager.StartTransaction()){var lid=Usable(tr,db,p.layer);var ms=(BlockTableRecord)tr.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db),OpenMode.ForWrite);
     var pl=new Polyline();pl.SetDatabaseDefaults(db);pl.LayerId=lid;pl.Color=ByLayer;pl.LinetypeId=db.ByLayerLinetype;pl.LineWeight=LineWeight.ByLayer;pl.LinetypeScale=1;
     for(int i=0;i<points.Count;i++)pl.AddVertexAt(i,new Point2d(points[i].X,points[i].Y),0,0,0);id=ms.AppendEntity(pl);tr.AddNewlyCreatedDBObject(pl,true);
#if IDENTITY015
     IdentityCad015.MarkPreset(tr,pl,p);
#endif
#if CABLES016
     // 016: cable type attributes right after the identity, same transaction (legacy six presets: none).
     CableCad016.MarkPreset(tr,pl,p.layer);
#endif
     tr.Commit();}
    outer.Commit();return id;}
   catch(Ex ex){string undo;try{if(!outer.IsDisposed)outer.Abort();undo=Rollback(db,before);}catch(Ex a){undo="CHƯA xác nhận được việc hoàn tác ("+a.Message+")";}throw new InvalidOperationException(ex.Message+" — "+undo+".",ex);}
   finally{if(!outer.IsDisposed)outer.Dispose();}}
 }
#endif
}
