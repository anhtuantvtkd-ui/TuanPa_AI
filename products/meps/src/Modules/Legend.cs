using System;using System.Collections.Generic;using System.Globalization;using System.IO;using System.Linq;using System.Text;using System.Text.RegularExpressions;
namespace Tvxd.Meps {
 // 0.5 pure model (TVXD-MEPS-PROJECT-005): drawing-ratio scale, effective lineweight resolution, source legend-table parsing, LIBRARY_0_5/legend.json,
 // used-symbol planning and legend row layout. No CAD types: LibraryStyle.cs and LegendCad.cs read native data into these records and apply the results,
 // so every rule here is testable offline (TESTS/Test05.cs).
 public static class Meps05 {
  // 0.5.1: side-by-side build (0.5 is already loaded and cannot be overwritten); same LIBRARY_0_5 and legend.json schema.
  // 0.5.2 (TVXD-MEPS-UI-006): side-by-side palette build beside 0.5.1; same LIBRARY_0_5 and legend.json schema.
  // 0.6.0 (TVXD-MEPS-IMPLEMENT-008): own LIBRARY_0_6 (a copy of LIBRARY_0_5 made before tests; LIBRARY_0_5 is never written); same schemas.
  public const string Version="0.6.0",LibraryDir="LIBRARY_0_6";
  // LibEntry.style values (also the fifth marker value of an inserted definition): frozen effective lineweights / source did not allow one value.
  public const string StyleFrozen="FROZEN_LW_V1",StyleAmbiguous="LW_AMBIGUOUS";
  // Tile preview: one third of 0.4.2's 76 px (76/3 = 25.3, rounded up). Display only; insertion size always comes from the catalog extents and the chosen scale.
  public const int PreviewSize=26;
  public const double MaxDenominator=1e6;
  public static double CheckDenominator(double v,string what){if(!Core.Finite(v)||v<=0||v>MaxDenominator)throw new Exception(what+" phải là số dương hữu hạn (tối đa 1000000).");return v;}
  // "Theo tỷ lệ bản vẽ": factor = baseScale × N / N0. N0 (reference denominator the symbol was drawn for) and N (current drawing denominator) are both
  // typed by the user; nothing is inferred from DIMSCALE/CANNOSCALE/INSUNITS and there is no default. The result goes through the usual scale range check.
  public static double RatioScale(double baseScale,double refDenominator,double drawingDenominator){Library.CheckScale(baseScale);CheckDenominator(refDenominator,"Mẫu số tỷ lệ gốc N0");CheckDenominator(drawingDenominator,"Mẫu số tỷ lệ bản vẽ N");return Library.CheckScale(baseScale*drawingDenominator/refDenominator);}
  public static string RatioText(double baseScale,double refDenominator,double drawingDenominator){return Library.F(baseScale)+" × "+Library.F(drawingDenominator)+" / "+Library.F(refDenominator)+" = "+Library.F(RatioScale(baseScale,refDenominator,drawingDenominator));}
  // F02: 0.5 places and exports only copies whose effective lineweights are frozen. Anything else gets an actionable reason (never treated as standardized).
  // LibraryCad.Insert itself is unchanged, so 0.3.1/0.4 code paths and older unfrozen placements keep working.
  public const string StyleUnfrozen="UNFROZEN";
  public static string PlaceIssue(LibEntry e){if(e==null)return "không có trong thư viện "+LibraryDir+".";
   if(e.status!="OK")return "mục chưa được hỗ trợ ("+string.Join("; ",(e.warnings??new List<string>()).Where(w=>w.StartsWith("UNSUPPORTED")).Take(2))+").";
   if(e.style==StyleFrozen)return null;
   if(e.style==StyleAmbiguous)return "nét nguồn MƠ HỒ, bản chép chưa cố định ("+string.Join("; ",(e.styleNotes??new List<string>()).Take(2))+"). Cách xử lý: trong một bản sao của file nguồn "+e.sourceFile+", cho mọi tham chiếu của khối "+e.name+" cùng layer/nét rồi bấm '+ Thêm block' với bản sao đó; hoặc đặt bằng MEPS042 (0.4.2, nét theo layer đích).";
   return "bản chép cũ chưa cố định nét. Cách xử lý: bấm '+ Thêm block' và chọn lại đúng file nguồn "+e.sourceFile+" để cố định nét (giữ nguyên ID); hoặc đặt bằng MEPS042 (0.4.2, nét theo layer đích).";}
  public static string StyleCode(LibEntry e){return e.status!="OK"?"UNSUPPORTED":e.style??StyleUnfrozen;}
 }
 // ---- Effective lineweight resolution (freezing the source appearance) ----
 // One entity of a definition: lw is the raw AutoCAD value (>=0 explicit 1/100 mm, -1 ByLayer, -2 ByBlock, -3 Default). def/attrs are set for a nested
 // block reference: its definition and its own attribute references, which take the reference as their block context.
 // alt = the original definition of a nested dynamic-block reference (its anonymous representation is def); both are resolved in the same context.
 public sealed class LwEnt {public string key,layer,def,alt;public int lw;public List<LwEnt> attrs;}
 // Context of a top-level reference in the source: its layer and that SOURCE layer's own lineweight (for layer-0 ByLayer inheritance), and the reference's
 // resolved lineweight (for ByBlock). The two are separate: a reference on E-TU (0.30) with an explicit 0.50 gives layer-0 ByLayer 0.30 and ByBlock 0.50.
 // The source layer weight is carried here because a layer used only by the outer reference is not cloned into the library copy (F01). null = unknown.
 public sealed class LwContext {public string layer,provenance;public int? lw,layerLw;}
 public sealed class LwResult {public Dictionary<string,int> values=new Dictionary<string,int>();public Dictionary<string,string> provenance=new Dictionary<string,string>();public List<string> ambiguous=new List<string>(),notes=new List<string>();public int runtimeUses;public bool Ok{get{return ambiguous.Count==0;}}}
 public static class Lineweight {
  public const int ByLayer=-1,ByBlock=-2,Default=-3;
  static readonly int[] Valid={0,5,9,13,15,18,20,25,30,35,40,50,53,60,70,80,90,100,106,120,140,158,200,211};
  public static bool IsValid(int v){return Array.IndexOf(Valid,v)>=0;}
  public static string Mm(int v){return (v/100.0).ToString("0.00",CultureInfo.InvariantCulture)+" mm";}
  static bool Zero(string layer){return string.Equals(layer,"0",StringComparison.OrdinalIgnoreCase);}
  // Every entity of the closure gets exactly one explicit value, or the result is ambiguous (two contexts / two nesting paths give different values,
  // a layer is missing, or the nesting is cyclic). Values that only the runtime can supply (Default, ByBlock or layer 0 without a source reference)
  // use runtimeDefault and are counted with runtimeProvenance: a captured AutoCAD setting, not an engineering standard.
  public static LwResult Resolve(Dictionary<string,List<LwEnt>> defs,string root,Dictionary<string,int> layerLw,List<LwContext> contexts,int runtimeDefault,string runtimeProvenance){
   if(!IsValid(runtimeDefault))throw new Exception("LWDEFAULT runtime không hợp lệ: "+runtimeDefault);
   var r=new LwResult();var seen=new Dictionary<string,HashSet<int>>();var layers=new Dictionary<string,int>(layerLw??new Dictionary<string,int>(),StringComparer.OrdinalIgnoreCase);
   var list=contexts!=null&&contexts.Count>0?contexts:new List<LwContext>{new LwContext{provenance="không có tham chiếu nguồn"}};
   var counts=new Dictionary<string,int>();
   Action<string,int,string> put=(key,v,why)=>{HashSet<int> s;if(!seen.TryGetValue(key,out s))seen[key]=s=new HashSet<int>();s.Add(v);if(!r.provenance.ContainsKey(key))r.provenance[key]=why;string c=Mm(v)+" ← "+why;int n;counts.TryGetValue(c,out n);counts[c]=n+1;};
   // A layer's own value: explicit, or Default → runtime. L/LL = inherited layer name and its source weight (from the context, or from the copy's layer table
   // once a nested reference sits on a named layer, whose record was cloned with the definition).
   Func<string,string,int,Tuple<int,string>> layerValue=(key,layer,lv)=>{if(lv>=0){if(!IsValid(lv)){r.ambiguous.Add(key+": layer "+layer+" có nét lạ "+lv);return null;}return Tuple.Create(lv,"ByLayer "+layer);}
    if(lv==Default){r.runtimeUses++;return Tuple.Create(runtimeDefault,"layer "+layer+" để Default → "+runtimeProvenance);}r.ambiguous.Add(key+": layer "+layer+" có nét lạ "+lv);return null;};
   Func<LwEnt,string,int?,int?,string,Tuple<int,string>> one=null;
   one=(e,L,LL,W,why)=>{
    if(e.lw>=0){if(!IsValid(e.lw)){r.ambiguous.Add(e.key+": giá trị nét lạ "+e.lw);return null;}return Tuple.Create(e.lw,"giá trị riêng của đối tượng");}
    if(e.lw==ByLayer){if(Zero(e.layer)){if(L==null){r.runtimeUses++;return Tuple.Create(runtimeDefault,"layer 0 trong khối, "+why+" → "+runtimeProvenance);}
      if(!LL.HasValue){r.ambiguous.Add(e.key+": thiếu nét của layer nguồn "+L+" (layer 0 kế thừa)");return null;}var t=layerValue(e.key,L,LL.Value);return t==null?null:Tuple.Create(t.Item1,"layer 0 ← "+t.Item2+" ("+why+")");}
     int lv;if(!layers.TryGetValue(e.layer,out lv)){r.ambiguous.Add(e.key+": thiếu layer "+e.layer);return null;}return layerValue(e.key,e.layer,lv);}
    if(e.lw==ByBlock){if(W.HasValue)return Tuple.Create(W.Value,"ByBlock ← "+why);r.runtimeUses++;return Tuple.Create(runtimeDefault,"ByBlock, "+why+" → "+runtimeProvenance);}
    if(e.lw==Default){r.runtimeUses++;return Tuple.Create(runtimeDefault,"Default → "+runtimeProvenance);}
    r.ambiguous.Add(e.key+": giá trị nét lạ "+e.lw);return null;};
   Action<string,string,int?,int?,string,int,List<string>> walk=null;
   walk=(def,L,LL,W,why,depth,stack)=>{List<LwEnt> ents;if(depth>16||stack.Contains(def)){r.ambiguous.Add("khối lồng quá sâu/vòng lặp: "+def);return;}if(!defs.TryGetValue(def,out ents)){r.ambiguous.Add("thiếu định nghĩa "+def);return;}stack.Add(def);
    foreach(var e in ents){var v=one(e,L,LL,W,why);if(v==null)continue;put(e.key,v.Item1,v.Item2);
     if(e.def!=null||e.alt!=null||e.attrs!=null){string L2=L;int? LL2=LL;if(!Zero(e.layer)){int lv;L2=e.layer;LL2=layers.TryGetValue(e.layer,out lv)?(int?)lv:null;}string why2="tham chiếu lồng "+e.key+" ("+v.Item2+")";
      if(e.attrs!=null)foreach(var a in e.attrs){var av=one(a,L2,LL2,v.Item1,why2);if(av!=null)put(a.key,av.Item1,av.Item2);}
      if(e.def!=null)walk(e.def,L2,LL2,v.Item1,why2,depth+1,stack);if(e.alt!=null&&e.alt!=e.def)walk(e.alt,L2,LL2,v.Item1,why2,depth+1,stack);}}
    stack.RemoveAt(stack.Count-1);};
   foreach(var c in list)walk(root,c.layer,c.layerLw,c.lw,c.provenance??"tham chiếu nguồn",0,new List<string>());
   foreach(var kv in seen){if(kv.Value.Count>1)r.ambiguous.Add(kv.Key+": nhiều giá trị "+string.Join("/",kv.Value.OrderBy(x=>x).Select(Mm)));else r.values[kv.Key]=kv.Value.First();}
   if(!r.Ok)r.values.Clear();
   foreach(var kv in counts.OrderByDescending(x=>x.Value).ThenBy(x=>x.Key,StringComparer.Ordinal).Take(24))r.notes.Add(kv.Value+" × "+kv.Key);
   if(counts.Count>24)r.notes.Add("… còn "+(counts.Count-24)+" nhóm khác");
   return r;}
 }
 // ---- Source legend table ----
 // Plain records of model-space content. Boxes are WCS extents (texts: real text extents; inserts: definition geometry without attributes, transformed).
 public sealed class TLine {public string handle,layer;public double x1,y1,x2,y2;}
 // 0.5.1 (F07) anchor metadata, filled natively by LegendCad.Extract: anchored = values present; (ax,ay) = MText.Location / TEXT insertion or alignment
 // point; rotation (radians) about the normal (nx,ny,nz); align = horizontal edge the anchor fixes (0 left, 1 centre, 2 right, -1 none: Aligned/Fit).
 public sealed class TText {public string handle,layer,text,contents,style;public double height,minX,minY,maxX,maxY;public bool anchored;public double ax,ay,rotation,nx,ny,nz=1;public int align=-1;}
 // rotation in radians about the insert normal (nx,ny,nz); scales as stored on the source reference.
 public sealed class TInsert {public string handle,layer,name,blockName;public bool dynamic;public double x,y,z,rotation,sx=1,sy=1,sz=1,nx,ny,nz=1,minX,minY,maxX,maxY;}
 public sealed class TableInput {public string sourceSha256,sourceFile;public List<TLine> lines=new List<TLine>();public List<TText> texts=new List<TText>();public List<TInsert> inserts=new List<TInsert>();}
 // text = plain text (MText.Text, or TEXT string); contents = raw MText.Contents (formatting codes kept). Several texts in one cell are kept in reading order.
 // anchored = handles of this cell assigned by their source anchor although their box crosses a column border (F07, Legend.AnchorColumn).
 public sealed class LegendCell {public string text="",contents="",style,layer;public double height;public List<string> handles=new List<string>(),anchored=new List<string>();}
 // V1_2 (TVXD-MEPS-LEGEND-009): ambiguity = why an AMBIGUOUS row is ambiguous (TEXT_CROSSING / BLOCK_CROSSING; null for other statuses and for maps read by an
 // older parser). certain = for a TEXT_CROSSING row, its DIỄN GIẢI / QUY CÁCH / GHI CHÚ cells that no crossing text touches (null element = uncertain cell).
 // description/spec/note of an AMBIGUOUS row stay empty as before, so strict Resolve/Plan are unchanged.
 public sealed class LegendRow {public string name,blockName,entryId,status,sourceSha256,sourceFile,layer,insertHandle,ambiguity;public bool dynamic;public int row;public double top,bottom,x,y,z,rotation,sx=1,sy=1,sz=1,nx,ny,nz=1;public LegendCell description=new LegendCell(),spec=new LegendCell(),note=new LegendCell();public List<LegendCell> certain;public List<string> issues=new List<string>();}
 public sealed class LegendLayer {public string name,linetype,color;public int aci=-1,red,green,blue,lineweight;public bool trueColor;}
 // Every property of the source text style record that affects glyphs (F03); the legend DWG recreates it under Legend.StyleName, never reusing a same-named style.
 public sealed class LegendStyle {public string name,fileName,bigFontFileName,typeface;public bool bold,italic,vertical;public int charset,pitch,flagBits;public double xScale=1,oblique,textSize;}
 public sealed class LegendSource {public string sourceSha256,sourceFile,parser=Legend.Parser,status,units,gridLayer,extractedUtc;public int insunits;public double headerTop,headerBottom,bottom,titleTop;
  public List<double> columns=new List<double>(),dividers=new List<double>();public List<LegendCell> headers=new List<LegendCell>();public LegendCell title;public List<string> issues=new List<string>();public List<LegendLayer> layers=new List<LegendLayer>();public List<LegendStyle> styles=new List<LegendStyle>();}
 public sealed class LegendMap {public string schema=Legend.Schema;public int version=1;public List<LegendSource> sources=new List<LegendSource>();public List<LegendRow> rows=new List<LegendRow>();
#if METADATA032
  // TVXD-MEPS-METADATA-032: runtime-only overlay of user-edited cells read by Load from metadata032.json; never written to legend.json (null = map built in
  // memory, no overlay).
  [System.Web.Script.Serialization.ScriptIgnore] public MetadataOverlay032 metadata032;
#endif
 }
 public sealed class LegendResolution {public string status,reason;public LegendRow row;}
 // Used symbols: one record per model-space reference that carries the LibraryCad marker (any MEPS version; the entry ID is stable).
 public sealed class UsedRef {public string handle,entryId,name,sourceSha256,space="Model";public bool erased,xref;}
 // referenceOnly (TVXD-MEPS-IDENTITY-015): added by Identity015.Augment for a legend symbol that is NOT placed in Model (count 0, no handles); default false.
 public sealed class UsedSymbol {public string entryId,name,sourceSha256;public int count;public bool referenceOnly;public List<string> handles=new List<string>();}
 // Orientation of a symbol in the legend, taken from its source table insert (F04): rotation about +Z, cap = source uniform scale (never shown larger).
 // origin (0.6.0 export policy): SOURCE_TABLE, or LIBRARY_DEFINITION (rotation 0, cap 1) when no single source orientation exists for an appended row.
 public sealed class LegendPose {public double rotation,cap=1,x;public string layer,issue,origin=Legend.OriginSource;}
 // symbols, entries and poses are parallel lists. key/layout/status are set by Legend.PlanForExport (layout SOURCE_ROW or APPENDED_ROW; status of the row's texts).
 public sealed class PlanRow {public string sourceSha256,key,layout=Legend.SourceRow,status;public int row;public LegendRow text;public List<UsedSymbol> symbols=new List<UsedSymbol>();public List<LibEntry> entries=new List<LibEntry>();public List<LegendPose> poses=new List<LegendPose>();}
 // missing = blockers (strict Plan: also missing texts; PlanForExport: real blockers only). warnings = exported symbols with empty/uncertain cells (export policy).
 public sealed class LegendPlan {public string policy=Legend.PolicyStrict;public List<PlanRow> rows=new List<PlanRow>();public List<string> missing=new List<string>(),warnings=new List<string>();public int symbols;public bool CanExport{get{return missing.Count==0&&rows.Count>0;}}}
 // One used symbol under the 0.6.0 export policy. cells = DIỄN GIẢI / QUY CÁCH / GHI CHÚ to write (empty = blank cell, never invented); blanks = names of the
 // empty cells; source = the source row the symbol is drawn in (layout SOURCE_ROW), null for APPENDED_ROW (own row after the source rows).
 public sealed class ExportDecision {public string name,entryId,status,reason,layout;public bool blocking=true;public int row=-1;public LibEntry entry;public LegendRow source;public LegendPose pose;public LegendCell[] cells={new LegendCell(),new LegendCell(),new LegendCell()};public List<string> blanks=new List<string>();}
 public static class Legend {
  // V1_2 (TVXD-MEPS-LEGEND-009): same cells and statuses as V1_1, plus LegendRow.ambiguity/certain evidence for AMBIGUOUS rows.
  public const string Schema="TVXD_MEPS_LEGEND_0_5",File="legend.json",Parser="TVXD_SOURCE_TABLE_V1_2";
  public const string Table="TABLE",NoTable="NO_TABLE",Ambiguous="AMBIGUOUS",Ok="OK",MissingDescription="MISSING_DESCRIPTION",UnresolvedDynamic="UNRESOLVED_DYNAMIC",NotMapped="NOT_MAPPED",NotInTable="NOT_IN_TABLE",UnsupportedTransform="UNSUPPORTED_TRANSFORM";
  // 0.6.0 export policy (TVXD-MEPS-LEGEND-009). NotInLibrary has the same value as Meps06.NotInLibrary.
  public const string NotInLibrary="NOT_IN_LIBRARY",TextCrossing="TEXT_CROSSING",BlockCrossing="BLOCK_CROSSING",SourceRow="SOURCE_ROW",AppendedRow="APPENDED_ROW",OriginSource="SOURCE_TABLE",OriginLibrary="LIBRARY_DEFINITION",PolicyStrict="STRICT_ALL_TEXT",PolicyBlank="BLANK_MISSING_TEXT";
  public static readonly string[] CellNames={"DIỄN GIẢI","QUY CÁCH","GHI CHÚ"};
  // Column headings of the source table (TVXD layout, V1). Matched after Unicode NFC, trimming, single spaces and upper case; cell texts are kept verbatim.
  public static readonly string[] Headers={"KÝ HIỆU","DIỄN GIẢI","QUY CÁCH","GHI CHÚ"};
  static readonly Regex Spaces=new Regex(@"\s+");
  public static string Norm(string s){return Spaces.Replace((s??"").Normalize(NormalizationForm.FormC).Trim()," ").ToUpperInvariant();}
  static double Cx(TText t){return (t.minX+t.maxX)/2;}static double Cy(TText t){return (t.minY+t.maxY)/2;}
  static double Cx(TInsert t){return (t.minX+t.maxX)/2;}static double Cy(TInsert t){return (t.minY+t.maxY)/2;}
  // Parser V1 for the TVXD source layout: four column headings, vertical LINE column borders crossing the heading row and full-width horizontal LINE row
  // dividers. A symbol belongs to the row between the two dividers that contain its whole box; texts are assigned to the cell that contains their whole box.
  // Anything crossing a divider/border is AMBIGUOUS and gets no text; nothing is matched by nearest Y. No headings at all = NO_TABLE (descriptions missing).
  // V1_1 (F07): a text whose box stays inside one row but crosses a COLUMN border may still be assigned by its source anchor (AnchorColumn); the
  // assignment is recorded as an INFO diagnostic and in LegendCell.anchored. Texts without anchor metadata keep the strict V1 behaviour.
  public static LegendSource Parse(TableInput t,out List<LegendRow> rows){var s=new LegendSource{sourceSha256=t.sourceSha256,sourceFile=t.sourceFile};rows=new List<LegendRow>();
   var heads=new TText[4];for(int i=0;i<4;i++){var m=t.texts.Where(x=>Norm(x.text)==Headers[i]).ToList();if(m.Count==1)heads[i]=m[0];else if(m.Count>1)s.issues.Add("Tiêu đề '"+Headers[i]+"' xuất hiện "+m.Count+" lần ("+string.Join(",",m.Select(x=>x.handle))+")");}
   if(heads.All(h=>h==null)&&s.issues.Count==0){s.status=NoTable;s.issues.Add("Không có tiêu đề bảng KÝ HIỆU/DIỄN GIẢI/QUY CÁCH/GHI CHÚ; mô tả để trống, không đoán.");return s;}
   if(heads.Any(h=>h==null)){s.status=Ambiguous;s.issues.Add("Thiếu hoặc trùng tiêu đề cột: "+string.Join(", ",Headers.Where((h,i)=>heads[i]==null)));return s;}
   double hh=heads.Max(h=>Math.Max(h.maxY-h.minY,h.height));double big=t.lines.SelectMany(l=>new[]{Math.Abs(l.x1),Math.Abs(l.y1),Math.Abs(l.x2),Math.Abs(l.y2)}).DefaultIfEmpty(1).Max();double tol=Math.Max(Math.Max(hh,1e-9)*1e-3,big*1e-9);
   double hy=heads.Average(h=>Cy(h));if(!(Cx(heads[0])<Cx(heads[1])&&Cx(heads[1])<Cx(heads[2])&&Cx(heads[2])<Cx(heads[3]))||heads.Any(h=>Math.Abs(Cy(h)-hy)>hh)){s.status=Ambiguous;s.issues.Add("Tiêu đề cột không cùng hàng hoặc sai thứ tự.");return s;}
   foreach(var h in heads)s.headers.Add(Cell(new[]{h}));
   var verticals=t.lines.Where(l=>Math.Abs(l.x1-l.x2)<=tol&&Math.Abs(l.y1-l.y2)>tol&&Math.Min(l.y1,l.y2)-tol<=hy&&Math.Max(l.y1,l.y2)+tol>=hy).OrderBy(l=>l.x1).ToList();
   var groups=new List<List<TLine>>();foreach(var l in verticals){if(groups.Count>0&&Math.Abs(groups.Last()[0].x1-l.x1)<=tol)groups.Last().Add(l);else groups.Add(new List<TLine>{l});}
   Func<List<TLine>,double> gx=g=>g.Average(l=>(l.x1+l.x2)/2);var c=heads.Select(h=>Cx(h)).ToArray();var cols=new List<List<TLine>>();
   var left=groups.Where(g=>gx(g)<c[0]).ToList();if(left.Count==0){s.status=Ambiguous;s.issues.Add("Không có đường biên trái của bảng.");return s;}if(left.Count>1)s.issues.Add("INFO "+left.Count+" đường dọc bên trái; dùng đường gần tiêu đề nhất.");cols.Add(left.Last());
   for(int i=0;i<3;i++){var mid=groups.Where(g=>gx(g)>c[i]&&gx(g)<c[i+1]).ToList();if(mid.Count!=1){s.status=Ambiguous;s.issues.Add("Giữa cột '"+Headers[i]+"' và '"+Headers[i+1]+"' có "+mid.Count+" đường dọc (cần đúng 1).");return s;}cols.Add(mid[0]);}
   var right=groups.Where(g=>gx(g)>c[3]).ToList();if(right.Count==0){s.status=Ambiguous;s.issues.Add("Không có đường biên phải của bảng.");return s;}if(right.Count>1)s.issues.Add("INFO "+right.Count+" đường dọc bên phải; dùng đường gần tiêu đề nhất.");cols.Add(right.First());
   foreach(var g in cols)s.columns.Add(gx(g));var X=s.columns;
   for(int i=0;i<4;i++)if(heads[i].minX<X[i]-tol||heads[i].maxX>X[i+1]+tol){s.status=Ambiguous;s.issues.Add("Tiêu đề '"+Headers[i]+"' vượt đường biên cột.");return s;}
   s.bottom=cols.Max(g=>g.Min(l=>Math.Min(l.y1,l.y2)));if(cols.Select(g=>g.Min(l=>Math.Min(l.y1,l.y2))).Any(v=>Math.Abs(v-s.bottom)>tol))s.issues.Add("INFO đường dọc kết thúc ở các cao độ khác nhau; đáy bảng lấy theo đường cao nhất "+Library.F(s.bottom)+".");
   var ys=new List<double>();var horizontals=t.lines.Where(q=>Math.Abs(q.y1-q.y2)<=tol&&Math.Min(q.x1,q.x2)<=X[0]+tol&&Math.Max(q.x1,q.x2)>=X[4]-tol).OrderByDescending(q=>(q.y1+q.y2)/2).ToList();
   foreach(var l in horizontals){double y=(l.y1+l.y2)/2;if(ys.Count==0||Math.Abs(ys.Last()-y)>tol)ys.Add(y);if(s.gridLayer==null)s.gridLayer=l.layer;}
   var above=ys.Where(y=>y>hy).ToList();var below=ys.Where(y=>y<hy).ToList();if(above.Count==0||below.Count==0){s.status=Ambiguous;s.issues.Add("Không tìm thấy đường kẻ trên/dưới hàng tiêu đề.");return s;}
   s.headerTop=above.Min();s.headerBottom=below.Max();var bounds=below.Where(y=>y>=s.bottom-tol).ToList();if(bounds.Count==0||Math.Abs(bounds.Last()-s.bottom)>tol){bounds.Add(s.bottom);s.issues.Add("INFO đáy bảng không có đường ngang toàn chiều rộng; dùng đáy đường dọc.");}
   s.dividers.AddRange(bounds);if(bounds.Count<2){s.status=Ambiguous;s.issues.Add("Bảng không có hàng dữ liệu.");return s;}
   foreach(var h in heads)if(h.maxY>s.headerTop+tol||h.minY<s.headerBottom-tol){s.status=Ambiguous;s.issues.Add("Tiêu đề '"+h.text+"' vượt đường kẻ hàng tiêu đề.");return s;}
   var top2=above.Where(y=>y>s.headerTop+tol).DefaultIfEmpty(double.NaN).Min();if(!double.IsNaN(top2)){var tt=t.texts.Where(x=>x.minY>=s.headerTop-tol&&x.maxY<=top2+tol&&x.minX>=X[0]-tol&&x.maxX<=X[4]+tol).ToList();if(tt.Count>0){s.title=Cell(tt);s.titleTop=top2;}}
   int n=bounds.Count-1;Func<double,double,int> rowOf=(lo,hi)=>{for(int i=0;i<n;i++)if(lo>=bounds[i+1]-tol&&hi<=bounds[i]+tol)return i;return -1;};
   var cells=new List<TText>[n,3];var amb=new bool[n];for(int i=0;i<n;i++)for(int k=0;k<3;k++)cells[i,k]=new List<TText>();var byAnchor=new HashSet<string>();var crossing=new List<TText>();
   foreach(var x in t.texts){double cx=Cx(x),cy=Cy(x);if(cx<X[0]||cx>X[4]||cy>s.headerBottom||cy<s.bottom)continue;int col=-1;for(int k=0;k<4;k++)if(cx>=X[k]&&cx<=X[k+1])col=k;
    int r=rowOf(x.minY,x.maxY);bool inCol=x.minX>=X[col]-tol&&x.maxX<=X[col+1]+tol;string why=null;
    if(r>=0&&!inCol){int k=AnchorColumn(x,t.texts,X,bounds,r,tol,out why);if(k>=0){col=k;inCol=true;byAnchor.Add(x.handle);
      s.issues.Add("INFO chữ "+x.handle+" '"+x.text+"' có khung vượt đường kẻ cột ("+Library.F(x.minX)+".."+Library.F(x.maxX)+"); gán theo điểm neo nguồn ("+Library.F(x.ax)+", "+Library.F(x.ay)+") vào hàng "+(r+1)+", cột '"+Headers[k]+"'.");}}
    if(r<0||!inCol){crossing.Add(x);int cr=-1;for(int i=0;i<n;i++)if(cy<=bounds[i]&&cy>=bounds[i+1])cr=i;if(cr>=0)amb[cr]=true;s.issues.Add("Chữ "+x.handle+" '"+x.text+"' vượt đường kẻ hàng/cột; hàng "+(cr+1)+" bị đánh dấu AMBIGUOUS."+(why!=null?" Điểm neo không dùng được: "+why+".":""));continue;}
    if(col==0){s.issues.Add("INFO chữ "+x.handle+" '"+x.text+"' nằm trong cột ký hiệu; không dùng làm mô tả.");continue;}cells[r,col-1].Add(x);}
   foreach(var b in t.inserts){double cx=Cx(b),cy=Cy(b);if(cx<X[0]||cx>X[4]||cy>s.headerBottom||cy<s.bottom)continue;
    if(cx>X[1]){s.issues.Add("INFO khối "+b.handle+" ("+(b.name??b.blockName)+") nằm ngoài cột ký hiệu; bỏ qua.");continue;}
    int r=rowOf(b.minY,b.maxY);bool inCol=b.minX>=X[0]-tol&&b.maxX<=X[1]+tol;
    var row=new LegendRow{name=b.name,blockName=b.blockName,dynamic=b.dynamic,sourceSha256=t.sourceSha256,sourceFile=t.sourceFile,layer=b.layer,insertHandle=b.handle,x=b.x,y=b.y,z=b.z,rotation=b.rotation,sx=b.sx,sy=b.sy,sz=b.sz,nx=b.nx,ny=b.ny,nz=b.nz};
    if(r<0||!inCol){int cr=-1;for(int i=0;i<n;i++)if(cy<=bounds[i]&&cy>=bounds[i+1])cr=i;row.row=cr;row.status=Ambiguous;row.ambiguity=BlockCrossing;row.issues.Add("Khối vượt đường kẻ hàng/cột; không gán mô tả.");if(cr>=0){row.top=bounds[cr];row.bottom=bounds[cr+1];}rows.Add(row);continue;}
    row.row=r;row.top=bounds[r];row.bottom=bounds[r+1];rows.Add(row);}
   foreach(var row in rows.Where(x=>x.status==null)){int r=row.row;row.description=Cell(cells[r,0]);row.spec=Cell(cells[r,1]);row.note=Cell(cells[r,2]);
    foreach(var cell in new[]{row.description,row.spec,row.note})cell.anchored.AddRange(cell.handles.Where(hd=>byAnchor.Contains(hd)));
    if(string.IsNullOrEmpty(row.name)||row.name.StartsWith("*")){row.status=UnresolvedDynamic;row.issues.Add("Khối ẩn danh "+row.blockName+" chưa xác định được tên định nghĩa gốc (cần DynamicBlockTableRecord bản gốc).");}
    // V1_2: a cell of this row that no crossing text touches keeps its own texts as evidence in row.certain (used only by the export policy).
    else if(amb[r]){row.status=Ambiguous;row.ambiguity=TextCrossing;row.issues.Add("Hàng có chữ vượt đường kẻ; không gán mô tả.");var own=new[]{row.description,row.spec,row.note};row.certain=new List<LegendCell>();
     for(int k=1;k<4;k++){int kk=k;bool touched=crossing.Any(q=>q.minX<X[kk+1]-tol&&q.maxX>X[kk]+tol&&q.minY<bounds[r]-tol&&q.maxY>bounds[r+1]+tol);row.certain.Add(touched?null:own[k-1]);}
     row.description=new LegendCell();row.spec=new LegendCell();row.note=new LegendCell();}
    else if(row.description.text.Trim().Length==0){row.status=MissingDescription;row.issues.Add("Ô DIỄN GIẢI trống.");}else row.status=Ok;}
   s.status=Table;return s;}
  // F07: column of a text whose box stays inside row r but crosses a column border, decided only by unique source evidence; otherwise -1 and the reason.
  // Required: anchor metadata; horizontal text on +Z; the anchor strictly inside row r and strictly inside one column (on a divider/border = rejected);
  // the anchored edge (left/centre/right) and the box centre in that same column; no text of another column in this row overlapping the box.
  // So the box may only run past the column border on the side away from its anchor (MText reference width or glyph overflow).
  public static int AnchorColumn(TText x,List<TText> all,List<double> X,List<double> bounds,int r,double tol,out string why){why=null;
   if(!x.anchored){why="không có dữ liệu điểm neo";return -1;}
   if(!new[]{x.ax,x.ay,x.rotation,x.nx,x.ny,x.nz,x.minX,x.maxX}.All(Core.Finite)){why="điểm neo không hữu hạn";return -1;}
   double a=Angle(x.rotation);if(Math.Abs(x.nx)>1e-9||Math.Abs(x.ny)>1e-9||!(x.nz>0)||(a>1e-9&&2*Math.PI-a>1e-9)){why="chữ không nằm ngang (góc "+Library.F(x.rotation)+")";return -1;}
   if(x.align<0||x.align>2){why="chữ căn Aligned/Fit, không có cạnh neo";return -1;}
   if(!(x.ay<bounds[r]-tol&&x.ay>bounds[r+1]+tol)){why="điểm neo nằm trên hoặc ngoài đường kẻ của hàng "+(r+1);return -1;}
   int k=-1;for(int i=0;i<4;i++)if(x.ax>X[i]+tol&&x.ax<X[i+1]-tol)k=i;if(k<0){why="điểm neo nằm trên đường kẻ cột";return -1;}
   double cx=(x.minX+x.maxX)/2,edge=x.align==0?x.minX:x.align==2?x.maxX:cx;
   if(edge<X[k]-tol||edge>X[k+1]+tol||cx<X[k]||cx>X[k+1]){why="khung chữ và điểm neo chỉ vào các cột khác nhau";return -1;}
   foreach(var o in all){if(ReferenceEquals(o,x))continue;double ox=(o.minX+o.maxX)/2;if(ox>=X[k]&&ox<=X[k+1])continue;
    if(o.minY<x.maxY-tol&&o.maxY>x.minY+tol&&o.minX<x.maxX-tol&&o.maxX>x.minX+tol){why="khung chữ chồng lên chữ "+o.handle+" của cột khác";return -1;}}
   return k;}
  // Reading order inside one cell: top to bottom, then left to right. Plain texts join with a line break, raw MText contents with \P.
  static LegendCell Cell(IEnumerable<TText> items){var list=items.OrderByDescending(x=>x.maxY).ThenBy(x=>x.minX).ToList();var c=new LegendCell();if(list.Count==0)return c;
   c.text=string.Join("\n",list.Select(x=>x.text??""));c.contents=string.Join("\\P",list.Select(x=>x.contents??x.text??""));c.handles.AddRange(list.Select(x=>x.handle));c.height=list[0].height;c.style=list[0].style;c.layer=list[0].layer;return c;}
  // Replaces only this source's previous records (re-reading the same source is idempotent); other sources are untouched. Entry IDs follow Library.EntryId.
  public static void Merge(LegendMap m,LegendSource s,List<LegendRow> rows){m.sources.RemoveAll(x=>string.Equals(x.sourceSha256,s.sourceSha256,StringComparison.OrdinalIgnoreCase));m.rows.RemoveAll(x=>string.Equals(x.sourceSha256,s.sourceSha256,StringComparison.OrdinalIgnoreCase));
   foreach(var r in rows){r.entryId=null;if(!string.IsNullOrEmpty(r.name)&&!r.name.StartsWith("*"))try{r.entryId=Library.EntryId(s.sourceSha256,r.name);}catch(Exception ex){r.issues.Add("Tên khối không hợp lệ: "+ex.Message);}}
   m.sources.Add(s);m.rows.AddRange(rows);}
  static bool Same(string a,string b){return string.Equals(a,b,StringComparison.OrdinalIgnoreCase);}
  static LegendResolution Res(string status,string reason,LegendRow row){return new LegendResolution{status=status,reason=reason,row=row};}
  // Exact lookup by (source hash, block name). OK only when every row for the symbol has a description and they all agree; otherwise the reason is explicit.
  public static LegendResolution Resolve(LegendMap m,string sourceSha256,string name){var src=m.sources.FirstOrDefault(x=>Same(x.sourceSha256,sourceSha256));
   if(src==null)return Res(NotMapped,"nguồn chưa được đọc bảng ký hiệu",null);
   var rows=m.rows.Where(r=>Same(r.sourceSha256,sourceSha256)&&Same(r.name,name)).OrderBy(r=>r.row).ToList();
   if(rows.Count==0)return src.status==Table?Res(NotInTable,"khối không có trong bảng ký hiệu của nguồn",null):Res(src.status==NoTable?NoTable:Ambiguous,src.status==NoTable?"nguồn không có bảng ký hiệu":"bảng nguồn không đọc được chắc chắn: "+string.Join("; ",src.issues.Take(2)),null);
   var bad=rows.FirstOrDefault(r=>r.status!=Ok);if(bad!=null)return Res(bad.status,string.Join("; ",bad.issues.DefaultIfEmpty(bad.status)),bad);
   if(rows.Select(r=>r.description.contents+"\u0001"+r.spec.contents+"\u0001"+r.note.contents).Distinct().Count()>1)return Res(Ambiguous,"khối có trong nhiều hàng với mô tả khác nhau",rows[0]);
   return Res(Ok,null,rows[0]);}
  // Model space only, not erased, not xref, valid stable entry ID; copies of one entry count once (with every handle kept).
  public static List<UsedSymbol> Dedupe(IEnumerable<UsedRef> refs){var map=new Dictionary<string,UsedSymbol>();
   foreach(var r in refs){if(r==null||r.erased||r.xref||r.space!="Model"||r.entryId==null||!Regex.IsMatch(r.entryId,"^[0-9a-f]{12}$"))continue;UsedSymbol u;if(!map.TryGetValue(r.entryId,out u))map[r.entryId]=u=new UsedSymbol{entryId=r.entryId,name=r.name,sourceSha256=r.sourceSha256};u.count++;u.handles.Add(r.handle);}
   return map.Values.OrderBy(u=>u.name,StringComparer.OrdinalIgnoreCase).ThenBy(u=>u.entryId,StringComparer.Ordinal).ToList();}
  // Groups used symbols by their source row (symbols sharing a source row share its texts). Any symbol without a library entry, without frozen lineweights
  // (F02), without an OK mapping or with an unsupported source transform (F04) is listed by name in missing, and then the plan cannot be exported:
  // no partial, unstandardized or invented legend.
  public static LegendPlan Plan(List<UsedSymbol> used,LegendMap map,Func<string,LibEntry> entry){var p=new LegendPlan();var groups=new Dictionary<string,PlanRow>();
   foreach(var u in used){var e=entry(u.entryId);if(e==null){p.missing.Add((u.name??u.entryId)+": không có trong thư viện "+Meps05.LibraryDir);continue;}
    var why=Meps05.PlaceIssue(e);if(why!=null){p.missing.Add(e.name+": "+Meps05.StyleCode(e)+" — "+why);continue;}
    var r=Resolve(map,e.sourceSha256,e.name);if(r.status!=Ok){p.missing.Add(e.name+": "+r.status+" — "+r.reason);continue;}
    var pose=Pose(map.rows.Where(x=>Same(x.sourceSha256,r.row.sourceSha256)&&x.row==r.row.row&&Same(x.name,e.name)).ToList());
    if(pose.issue!=null){p.missing.Add(e.name+": "+UnsupportedTransform+" — "+pose.issue+" (hàng nguồn "+(r.row.row+1)+"); không đoán hướng đặt.");continue;}
    string key=r.row.sourceSha256.ToLowerInvariant()+"|"+r.row.row.ToString(CultureInfo.InvariantCulture);PlanRow g;if(!groups.TryGetValue(key,out g))groups[key]=g=new PlanRow{sourceSha256=r.row.sourceSha256,row=r.row.row,text=r.row};g.symbols.Add(u);g.entries.Add(e);g.poses.Add(pose);p.symbols++;}
   var order=map.sources.Select(x=>x.sourceSha256.ToLowerInvariant()).ToList();
   foreach(var g in groups.Values.OrderBy(x=>order.IndexOf(x.sourceSha256.ToLowerInvariant())).ThenBy(x=>x.row)){
    var xs=g.symbols.Select((u,i)=>new{u,e=g.entries[i],o=g.poses[i]}).OrderBy(q=>q.o.x).ThenBy(q=>q.e.name,StringComparer.OrdinalIgnoreCase).ToList();
    g.symbols=xs.Select(q=>q.u).ToList();g.entries=xs.Select(q=>q.e).ToList();g.poses=xs.Select(q=>q.o).ToList();p.rows.Add(g);}
   return p;}
  static bool SameNum(double a,double b){return Math.Abs(a-b)<=1e-9*Math.Max(1,Math.Max(Math.Abs(a),Math.Abs(b)));}
  static double Angle(double a){double t=2*Math.PI,v=a%t;if(v<0)v+=t;if(SameNum(v,t))v=0;return v;}
  // F04: the legend shows a symbol in its source table orientation. Supported: normal +Z, equal positive X/Y/Z scales (any rotation). Mirrored, non-uniform
  // or tilted inserts, and copies of one symbol in one row with different transforms, return an issue instead of a guessed orientation.
  public static LegendPose Pose(List<LegendRow> placements){var p=new LegendPose();if(placements==null||placements.Count==0){p.issue="không có tham chiếu nguồn trong hàng";return p;}
   foreach(var r in placements){p.issue=TransformIssue(r);if(p.issue!=null)return p;}
   var a=placements[0];if(placements.Any(r=>!SameNum(r.sx,a.sx)||!SameNum(Angle(r.rotation),Angle(a.rotation)))){p.issue="cùng khối có "+placements.Count+" tham chiếu với hướng/tỉ lệ khác nhau trong hàng";return p;}
   p.rotation=Angle(a.rotation);p.cap=Library.CheckScale(a.sx);p.layer=a.layer;p.x=placements.Min(r=>r.x);return p;}
  // Incompatible transform of one source insert (non-finite, not +Z, mirrored, non-uniform), or null. Shared by Pose and the export policy.
  public static string TransformIssue(LegendRow r){string at=" (insert "+r.insertHandle+")";
   if(!new[]{r.rotation,r.sx,r.sy,r.sz,r.nx,r.ny,r.nz}.All(Core.Finite))return "phép biến đổi không hữu hạn"+at;
   if(Math.Abs(r.nx)>1e-9||Math.Abs(r.ny)>1e-9||!(r.nz>0))return "mặt phẳng chèn không phải +Z (normal "+Library.F(r.nx)+","+Library.F(r.ny)+","+Library.F(r.nz)+")"+at;
   if(!(r.sx>0)||!(r.sy>0)||!(r.sz>0))return "khối bị lật/gương (tỉ lệ "+Library.F(r.sx)+","+Library.F(r.sy)+","+Library.F(r.sz)+")"+at;
   if(!SameNum(r.sx,r.sy)||!SameNum(r.sx,r.sz))return "tỉ lệ không đều ("+Library.F(r.sx)+","+Library.F(r.sy)+","+Library.F(r.sz)+")"+at;
   return null;}
  // ---- 0.6.0 export policy (TVXD-MEPS-LEGEND-009; user rule 2026-10-09, supersedes the all-text gate for production export) ----
  // A used symbol is exported in the verified four-column source table layout even when its source text is missing or uncertain; that cell stays EMPTY.
  // MEPS never writes a description itself (no block name used as text). Real blockers stay: not in the library or not frozen (F02), source table never read /
  // absent / unreadable / incomplete schema, incompatible source transform (F04). Plan above is unchanged (historical strict rule).
  //  - OK (strict Resolve): the source row, exactly as Plan.
  //  - MISSING_DESCRIPTION in one source row: that row; DIỄN GIẢI empty, QUY CÁCH / GHI CHÚ verbatim.
  //  - AMBIGUOUS by a crossing text (V1_2 evidence): that row; only cells no crossing text touches are kept.
  //  - NOT_IN_TABLE: own APPENDED_ROW, all cells empty, library-definition orientation (rotation 0, cap 1).
  //  - several source rows with different texts, a block crossing a divider, or an AMBIGUOUS row without V1_2 evidence: own APPENDED_ROW; a cell is kept only
  //    when every source row gives it with certainty and identically. Orientation from the source inserts when they agree, else the library definition.
#if METADATA032
  // TVXD-MEPS-METADATA-032: the source decision below is unchanged (DecideSource); only a non-blocking decision then takes the user-edited cells of its own
  // entry from the overlay (LegendMetadata032.Apply). Blockers are never relaxed.
  public static ExportDecision Decide(UsedSymbol u,LegendMap map,Func<string,LibEntry> entry){var d=DecideSource(u,map,entry);if(!d.blocking)LegendMetadata032.Apply(d,map);return d;}
  public static ExportDecision DecideSource(UsedSymbol u,LegendMap map,Func<string,LibEntry> entry){
#else
  public static ExportDecision Decide(UsedSymbol u,LegendMap map,Func<string,LibEntry> entry){
#endif
   var d=new ExportDecision{name=u.name??u.entryId,entryId=u.entryId};var e=entry(u.entryId);
   if(e==null){d.status=NotInLibrary;d.reason="không có trong thư viện "+Meps05.LibraryDir;return d;}
   d.entry=e;d.name=e.name;var why=Meps05.PlaceIssue(e);if(why!=null){d.status=Meps05.StyleCode(e);d.reason=why;return d;}
   var src=map.sources.FirstOrDefault(x=>Same(x.sourceSha256,e.sourceSha256));
   if(src==null){d.status=NotMapped;d.reason="nguồn chưa được đọc bảng ký hiệu (chưa có bố cục bảng nguồn)";return d;}
   if(src.status!=Table){d.status=src.status==NoTable?NoTable:Ambiguous;d.reason=src.status==NoTable?"nguồn không có bảng ký hiệu (không có bố cục 4 cột)":"bảng nguồn không đọc được chắc chắn: "+string.Join("; ",src.issues.Where(i=>!i.StartsWith("INFO")).Take(2));return d;}
   if(src.columns.Count!=5||src.headers.Count!=4||src.dividers.Count<2){d.status=Ambiguous;d.reason="bảng nguồn thiếu cột/tiêu đề/đường kẻ";return d;}
   var rows=map.rows.Where(r=>Same(r.sourceSha256,e.sourceSha256)&&Same(r.name,e.name)).OrderBy(r=>r.row).ToList();
   var res=Resolve(map,e.sourceSha256,e.name);
   if(res.status==Ok){var pose=Pose(rows.Where(x=>x.row==res.row.row).ToList());if(pose.issue!=null)return Blocked(d,pose.issue+" (hàng nguồn "+(res.row.row+1)+")");
    return Mapped(d,Ok,res.row,pose,new[]{res.row.description,res.row.spec,res.row.note},null);}
   if(rows.Count==0)return Appended(d,NotInTable,LibraryPose(),new LegendCell[3],"khối không có dòng trong bảng nguồn");
   foreach(var r in rows){var t=TransformIssue(r);if(t!=null)return Blocked(d,t+" (hàng nguồn "+(r.row+1)+")");}
   var r0=rows[0];bool one=r0.row>=0&&rows.All(r=>r.row==r0.row&&r.status==r0.status);
   if(one&&r0.status==MissingDescription){var pose=Pose(rows);if(pose.issue!=null)return Blocked(d,pose.issue+" (hàng nguồn "+(r0.row+1)+")");
    return Mapped(d,MissingDescription,r0,pose,new[]{r0.description,r0.spec,r0.note},"ô DIỄN GIẢI của nguồn trống");}
   if(one&&r0.status==Ambiguous&&r0.ambiguity==TextCrossing&&r0.certain!=null&&r0.certain.Count==3){var pose=Pose(rows);if(pose.issue!=null)return Blocked(d,pose.issue+" (hàng nguồn "+(r0.row+1)+")");
    return Mapped(d,Ambiguous,r0,pose,r0.certain.ToArray(),"hàng nguồn có chữ vượt đường kẻ; ô không chắc chắn để trống");}
   var cells=new LegendCell[3];bool dropped=false;
   for(int k=0;k<3;k++){int kk=k;var c=rows.Select(r=>CertainCell(r,kk)).ToList();if(c.All(x=>x!=null)&&c.Select(x=>x.contents).Distinct().Count()==1)cells[k]=c[0];else dropped=true;}
   string note=rows.Select(r=>r.row).Distinct().Count()>1?"khối có trong "+rows.Select(r=>r.row).Distinct().Count()+" hàng nguồn ("+string.Join(", ",rows.Select(r=>(r.row+1).ToString(CultureInfo.InvariantCulture)).Distinct())+")":r0.ambiguity==BlockCrossing?"khối nguồn vượt đường kẻ hàng/cột":r0.status==Ambiguous?"hàng nguồn mơ hồ, chưa có dữ liệu ô chắc chắn (đọc lại bảng bằng "+Parser+")":"hàng nguồn "+r0.status;
   var p=Pose(rows);if(p.issue!=null){note+="; các tham chiếu nguồn khác hướng/tỉ lệ ("+p.issue+"), dùng hướng định nghĩa thư viện";p=LibraryPose();}else p.origin=OriginSource;
   return Appended(d,dropped||rows.Any(r=>r.status!=Ok&&r.status!=MissingDescription)?Ambiguous:MissingDescription,p,cells,note);}
  static LegendPose LibraryPose(){return new LegendPose{rotation=0,cap=1,origin=OriginLibrary};}
  // Cell k of a source row when it is certain: OK / MISSING_DESCRIPTION rows, or a TEXT_CROSSING row's untouched cell; null otherwise.
  static LegendCell CertainCell(LegendRow r,int k){if(r.status==Ok||r.status==MissingDescription)return new[]{r.description,r.spec,r.note}[k];
   if(r.status==Ambiguous&&r.ambiguity==TextCrossing&&r.certain!=null&&r.certain.Count==3)return r.certain[k];return null;}
  static ExportDecision Blocked(ExportDecision d,string issue){d.blocking=true;d.status=UnsupportedTransform;d.reason=issue+"; không đoán hướng đặt.";return d;}
  // Whitespace-only cells count as empty (nothing written); other cells are kept verbatim (plain text, raw contents, style, layer, handles).
  static void Fill(ExportDecision d,LegendCell[] cells){for(int k=0;k<3;k++){var c=cells[k];d.cells[k]=c==null||(c.text??"").Trim().Length==0?new LegendCell():c;if(d.cells[k].contents.Length==0)d.blanks.Add(CellNames[k]);}}
  static ExportDecision Mapped(ExportDecision d,string status,LegendRow row,LegendPose pose,LegendCell[] cells,string note){d.blocking=false;d.status=status;d.layout=SourceRow;d.row=row.row;d.source=row;d.pose=pose;pose.origin=OriginSource;Fill(d,cells);
   string first=d.cells[0].text;int nl=first.IndexOf('\n');d.reason="hàng nguồn "+(row.row+1)+(status==Ok?": "+(nl>=0?first.Substring(0,nl)+" …":first):"; "+note+"; ô để trống: "+(d.blanks.Count==0?"(không)":string.Join(", ",d.blanks))+" (MEPS không tự viết)");return d;}
  static ExportDecision Appended(ExportDecision d,string status,LegendPose pose,LegendCell[] cells,string note){d.blocking=false;d.status=status;d.layout=AppendedRow;d.row=-1;d.pose=pose;Fill(d,cells);
   d.reason=note+"; dòng riêng sau các dòng nguồn, hướng "+(pose.origin==OriginLibrary?"theo định nghĩa thư viện":"theo bảng nguồn")+"; ô để trống: "+(d.blanks.Count==0?"(không)":string.Join(", ",d.blanks))+" (MEPS không tự viết)";return d;}
  // Height of an appended row: the median data-row height of the source table (dividers), else the heading row height.
  public static double TypicalRowHeight(LegendSource s){var gaps=new List<double>();for(int i=0;i+1<s.dividers.Count;i++){double g=s.dividers[i]-s.dividers[i+1];if(g>0&&Core.Finite(g))gaps.Add(g);}
   if(gaps.Count==0)return Math.Max(0,s.headerTop-s.headerBottom);gaps.Sort();int m=gaps.Count/2;return gaps.Count%2==1?gaps[m]:(gaps[m-1]+gaps[m])/2;}
  // The row record written by LegendCad.Build: the decision's cells; a source row's bounds and provenance, or for an appended row the typical height.
  static LegendRow ExportRow(ExportDecision d,LegendSource src){var s=d.source;var r=new LegendRow{name=d.entry.name,blockName=s!=null?s.blockName:d.entry.name,entryId=d.entry.id,status=d.status,sourceSha256=d.entry.sourceSha256,sourceFile=d.entry.sourceFile,row=d.row,description=d.cells[0],spec=d.cells[1],note=d.cells[2]};
   if(s!=null){r.top=s.top;r.bottom=s.bottom;r.insertHandle=s.insertHandle;r.layer=s.layer;r.dynamic=s.dynamic;}else{r.top=TypicalRowHeight(src);r.bottom=0;}r.issues.Add(d.reason);return r;}
  static string CellKey(LegendRow r){return r.description.contents+"\u0001"+r.spec.contents+"\u0001"+r.note.contents;}
  // Export plan: every non-blocked used symbol appears exactly once. Source rows keep the source order and grouping (symbols sharing a row share its texts);
  // appended rows follow the source rows of their source, each with its own key (source|APPENDED_ROW|entry id), ordered by name then ID.
  public static LegendPlan PlanForExport(List<UsedSymbol> used,LegendMap map,Func<string,LibEntry> entry){var p=new LegendPlan{policy=PolicyBlank};var groups=new Dictionary<string,PlanRow>();
   foreach(var u in used){var d=Decide(u,map,entry);if(d.blocking){p.missing.Add(d.name+": "+d.status+" — "+d.reason);continue;}
    if(d.status!=Ok)p.warnings.Add(d.name+": "+d.status+" — "+d.reason);
    var src=map.sources.First(x=>Same(x.sourceSha256,d.entry.sourceSha256));var text=ExportRow(d,src);string sha=d.entry.sourceSha256.ToLowerInvariant();
    string key=d.layout==SourceRow?sha+"|"+d.row.ToString(CultureInfo.InvariantCulture):sha+"|"+AppendedRow+"|"+d.entry.id;PlanRow g;
    // Defensive: a source row whose texts would differ between its symbols is never merged.
    if(groups.TryGetValue(key,out g)&&CellKey(g.text)!=CellKey(text)){key+="|"+d.entry.id;g=null;groups.TryGetValue(key,out g);}
    if(g==null)groups[key]=g=new PlanRow{sourceSha256=d.entry.sourceSha256,row=d.row,key=key,layout=d.layout,status=d.status,text=text};else if(g.status==Ok&&d.status!=Ok)g.status=d.status;
    g.symbols.Add(u);g.entries.Add(d.entry);g.poses.Add(d.pose);p.symbols++;}
   var order=map.sources.Select(x=>x.sourceSha256.ToLowerInvariant()).ToList();
   foreach(var g in groups.Values.OrderBy(x=>order.IndexOf(x.sourceSha256.ToLowerInvariant())).ThenBy(x=>x.layout==SourceRow?0:1).ThenBy(x=>x.layout==SourceRow?x.row:0).ThenBy(x=>x.layout==SourceRow?"":x.entries[0].name,StringComparer.OrdinalIgnoreCase).ThenBy(x=>x.key,StringComparer.Ordinal)){
    var xs=g.symbols.Select((u,i)=>new{u,e=g.entries[i],o=g.poses[i]}).OrderBy(q=>q.o.x).ThenBy(q=>q.e.name,StringComparer.OrdinalIgnoreCase).ToList();
    g.symbols=xs.Select(q=>q.u).ToList();g.entries=xs.Select(q=>q.e).ToList();g.poses=xs.Select(q=>q.o).ToList();p.rows.Add(g);}
   return p;}
  // Axis-aligned box of the block's own extents after rotation about its base point: [minX,minY,maxX,maxY].
  public static double[] RotatedBox(double minX,double minY,double maxX,double maxY,double rotation){double c=Math.Cos(rotation),s=Math.Sin(rotation);var b=new[]{double.MaxValue,double.MaxValue,double.MinValue,double.MinValue};
   foreach(var x in new[]{minX,maxX})foreach(var y in new[]{minY,maxY}){double u=x*c-y*s,v=x*s+y*c;b[0]=Math.Min(b[0],u);b[1]=Math.Min(b[1],v);b[2]=Math.Max(b[2],u);b[3]=Math.Max(b[3],v);}return b;}
  // ---- Legend layout (new DWG) ----
  // Uniform factor so the (rotated) symbol box fits its slot with padding: never larger than max (the source table scale, default 1), never depends on any
  // project insertion scale.
  public static double FitScale(double width,double height,double slotWidth,double slotHeight,double pad){return FitScale(width,height,slotWidth,slotHeight,pad,1);}
  public static double FitScale(double width,double height,double slotWidth,double slotHeight,double pad,double max){if(!Core.Finite(width)||!Core.Finite(height)||width<0||height<0)throw new Exception("Kích thước khối không hợp lệ.");Library.CheckScale(max);
   double w=slotWidth-2*pad,h=slotHeight-2*pad;if(!(w>0)||!(h>0))throw new Exception("Ô ký hiệu quá nhỏ.");double s=max;if(width>1e-12)s=Math.Min(s,w/width);if(height>1e-12)s=Math.Min(s,h/height);return Library.CheckScale(s);}
  // F03: text styles in the legend DWG are namespaced by source hash, so a source style never merges with the new drawing's STANDARD or another source's
  // same-named style.
  public static string StyleName(string sourceSha256,string name){if(sourceSha256==null||!Regex.IsMatch(sourceSha256,"^[0-9a-fA-F]{64}$"))throw new Exception("Hash nguồn không hợp lệ.");Library.ValidName(name);return "MEPS05_"+sourceSha256.Substring(0,8).ToUpperInvariant()+"_"+name;}
  public static double RowHeight(double sourceHeight,double textHeight,double pad){if(!Core.Finite(sourceHeight)||sourceHeight<0||!Core.Finite(textHeight)||textHeight<0)throw new Exception("Chiều cao hàng không hợp lệ.");return Math.Max(sourceHeight,textHeight+2*pad);}
  // Left edges of n equal slots inside [left,right].
  public static double[] Slots(int n,double left,double right){if(n<1)throw new Exception("Không có ký hiệu.");var r=new double[n];double w=(right-left)/n;for(int i=0;i<n;i++)r[i]=left+i*w;return r;}
  // ---- legend.json (inside the library root, written atomically like catalog.json) ----
  static readonly string[] SourceStatus={Table,NoTable,Ambiguous},RowStatus={Ok,Ambiguous,MissingDescription,UnresolvedDynamic};
  static void Txt(string s,int max){if(s!=null&&(s.Length>max||s.Any(ch=>char.IsControl(ch)&&ch!='\r'&&ch!='\n'&&ch!='\t')))throw new Exception("legend.json có chuỗi quá dài hoặc ký tự điều khiển.");}
  static void Num(params double[] v){if(v.Any(x=>!Core.Finite(x)))throw new Exception("legend.json có số không hữu hạn.");}
  static void CellOk(LegendCell c){if(c==null||c.handles==null||c.text==null||c.contents==null)throw new Exception("legend.json có ô thiếu dữ liệu.");Txt(c.text,8000);Txt(c.contents,16000);Txt(c.style,260);Txt(c.layer,260);Num(c.height);foreach(var h in c.handles)Txt(h,40);
   if(c.anchored!=null&&c.anchored.Any(h=>!c.handles.Contains(h)))throw new Exception("legend.json: chữ gán theo điểm neo không thuộc ô.");}
  public static void Validate(LegendMap m){if(m==null||m.schema!=Schema||m.version!=1||m.sources==null||m.rows==null)throw new Exception("legend.json không đúng schema "+Schema+" phiên bản 1.");
   foreach(var s in m.sources){if(s==null||s.sourceSha256==null||!Regex.IsMatch(s.sourceSha256,"^[0-9a-fA-F]{64}$")||!SourceStatus.Contains(s.status)||s.columns==null||s.dividers==null||s.headers==null||s.issues==null||s.layers==null||s.styles==null)throw new Exception("legend.json: nguồn không hợp lệ.");
    Num(s.headerTop,s.headerBottom,s.bottom,s.titleTop);Num(s.columns.ToArray());Num(s.dividers.ToArray());foreach(var c in s.headers)CellOk(c);if(s.title!=null)CellOk(s.title);foreach(var t in new[]{s.sourceFile,s.units,s.gridLayer,s.extractedUtc,s.parser}.Concat(s.issues))Txt(t,4000);
    foreach(var st in s.styles){if(st==null||string.IsNullOrEmpty(st.name))throw new Exception("legend.json: kiểu chữ không hợp lệ.");foreach(var t in new[]{st.name,st.fileName,st.bigFontFileName,st.typeface})Txt(t,260);Num(st.xScale,st.oblique,st.textSize);}
    if(s.status==Table&&(s.columns.Count!=5||s.dividers.Count<2||s.headers.Count!=4))throw new Exception("legend.json: bảng nguồn thiếu cột/đường kẻ.");}
   foreach(var r in m.rows){if(r==null||r.sourceSha256==null||!RowStatus.Contains(r.status)||r.issues==null)throw new Exception("legend.json: hàng không hợp lệ.");CellOk(r.description);CellOk(r.spec);CellOk(r.note);Num(r.top,r.bottom,r.x,r.y,r.z,r.rotation,r.sx,r.sy,r.sz,r.nx,r.ny,r.nz);
    foreach(var t in new[]{r.name,r.blockName,r.entryId,r.layer,r.insertHandle,r.sourceFile}.Concat(r.issues))Txt(t,4000);if(r.entryId!=null&&!Regex.IsMatch(r.entryId,"^[0-9a-f]{12}$"))throw new Exception("legend.json: ID không hợp lệ.");
    if(!m.sources.Any(s=>Same(s.sourceSha256,r.sourceSha256)))throw new Exception("legend.json: hàng không thuộc nguồn nào.");
    if(r.ambiguity!=null&&(r.status!=Ambiguous||(r.ambiguity!=TextCrossing&&r.ambiguity!=BlockCrossing)))throw new Exception("legend.json: lý do mơ hồ không hợp lệ.");
    if(r.certain!=null){if(r.certain.Count!=3||r.ambiguity!=TextCrossing)throw new Exception("legend.json: ô chắc chắn không hợp lệ.");foreach(var c in r.certain)if(c!=null)CellOk(c);}}}
#if METADATA032
  // 032: every fresh read also reads and validates metadata032.json (absent = empty overlay); a malformed sidecar throws here, before any export or save.
  public static LegendMap Load(string root){string p=Library.SafePath(root,File);LegendMap m;if(!System.IO.File.Exists(p))m=new LegendMap();else{m=Library.Json().Deserialize<LegendMap>(System.IO.File.ReadAllText(p,Encoding.UTF8));Validate(m);}
   m.metadata032=LegendMetadata032.Read(root);return m;}
#else
  public static LegendMap Load(string root){string p=Library.SafePath(root,File);if(!System.IO.File.Exists(p))return new LegendMap();var m=Library.Json().Deserialize<LegendMap>(System.IO.File.ReadAllText(p,Encoding.UTF8));Validate(m);return m;}
#endif
  public static void Save(string root,LegendMap m){Validate(m);string p=Library.SafePath(root,File);Directory.CreateDirectory(Path.GetDirectoryName(p));string text=Library.Json().Serialize(m);Validate(Library.Json().Deserialize<LegendMap>(text));Library.WriteAtomic(p,text);}
 }
#if CABLES016
 // ---- TVXD-MEPS-CABLES-016 addendum (user 2026-10-09): symbol sizing of two groups in the CENTRED legend (LegendCad.Stage / BuildCentered only) ----
 //  - SWITCH: the nine source switches (entry ID AND name: E-CT-1..6, E-CT-2C-1..3) keep exactly their source table scale (LegendPose.cap) and rotation;
 //    never fit-shrunk. Their rotated boxes are packed left to right in the plan order (source x) with one gap = pad × GapFactor between adjacent boxes
 //    and centred as a group in the symbol column.
 //  - NHE: E-NHE-RJ45 / E-NHE-TEL (ID AND name) get one uniform scale each so that their rotated box height equals the rotated box height of E-OC-1 of the
 //    SAME source hash at its own source pose (catalog extents × legend.json rotation/cap). Missing/invalid reference → exception (no fallback size).
 //    Only the generated legend copy is scaled; nothing in the library or drawing changes. Their cells stay as decided by Legend.Decide (blank = blank).
 //  The symbol column grows only when a packed group + 2 × pad is wider than it (delta); the other three columns keep their widths and move by delta.
 //  A packed row is at least the tallest packed symbol + 2 × pad. A row mixing packed and other symbols is refused. Every other row keeps Legend.Slots /
 //  Legend.FitScale. All values are raw drawing units of the source table (no unit conversion, no INSUNITS interpretation).
 // scale/rotation/width/height: planned (width/height = scaled rotated box). cx: planned centre X in table coordinates. minX..maxY: final box written by
 // LegendCad.Draw in the legend's own coordinates (0 until drawn).
 public sealed class LegendSymbol016 {public string entryId,name,kind;public double scale,rotation,width,height,cx,minX,minY,maxX,maxY;}
 // index = position in the plan rows of this source; minHeight = tallest packed symbol + 2 × pad (0 for an unpacked row); height = final row height (Draw).
 public sealed class LegendRow016 {public int index;public string key;public bool packed;public double groupWidth,minHeight,height;public List<LegendSymbol016> symbols=new List<LegendSymbol016>();}
 // oldWidth/requiredWidth = symbol column width before/after; delta = requiredWidth − oldWidth (≥ 0). referenceHeight = E-OC-1 height (0 when no NHE row).
 public sealed class LegendLayout016 {public string sourceSha256,referenceId,referenceName;public double pad,gap,oldWidth,requiredWidth,delta,referenceHeight;public List<LegendRow016> rows=new List<LegendRow016>();}
 public static class Layout016 {
  public const string Switch="SWITCH",Nhe="NHE";
  public const double GapFactor=0.25;
  public const string ReferenceId="3a5a91f081ba",ReferenceName="E-OC-1";
  // Same IDs/names as Identity015.OneWay/TwoWay and its codebook (repeated here so Legend.cs compiles without Identity015.cs).
  public static readonly string[][] Switches={new[]{"af06e101199b","E-CT-1"},new[]{"7c3b3f10bf73","E-CT-2"},new[]{"6eb1c9a43e1a","E-CT-3"},new[]{"f86dcd0aacba","E-CT-4"},
   new[]{"0bfd9b2cf670","E-CT-5"},new[]{"acdb9d18b907","E-CT-6"},new[]{"288cc0186435","E-CT-2C-1"},new[]{"649e3503d144","E-CT-2C-2"},new[]{"fba189fcc8c4","E-CT-2C-3"}};
  public static readonly string[][] Nhes={new[]{"458d71810086","E-NHE-RJ45"},new[]{"3bb4ec1dabb2","E-NHE-TEL"}};
  // TVXD-MEPS-FAN-025 (user 2026-10-10): ONLY the ceiling fan entry e665d1f5fbf3 / E-QUAT-TR (source c4356be5…) is packed like a switch: its source table pose
  // (cap, rotation) exactly, never fit-shrunk; the symbol column/row grow through the same packed mechanics. Other E-QUAT entries keep the old behaviour.
  public const string Fan="FAN";
  public const string FanSourceSha256="c4356be550c9436356489fb7fb49d575d53682f6e7e203a3fdd40d90b7c9ac6a";
  public static readonly string[][] Fans={new[]{"e665d1f5fbf3","E-QUAT-TR"}};
  static bool Same(string a,string b){return string.Equals(a,b,StringComparison.OrdinalIgnoreCase);}
  static bool In(string[][] list,LibEntry e){return e!=null&&list.Any(p=>string.Equals(p[0],e.id,StringComparison.Ordinal)&&Same(p[1],e.name));}
  // SWITCH, NHE, FAN or null (other symbol: old behaviour). FAN also needs the exact source hash.
  public static string Kind(LibEntry e){return In(Switches,e)?Switch:In(Nhes,e)?Nhe:In(Fans,e)&&Same(e.sourceSha256,FanSourceSha256)?Fan:null;}
  static double[] Box(LibEntry e,double rotation,string what){if(!new[]{e.minX,e.minY,e.maxX,e.maxY,rotation}.All(Core.Finite)||e.maxX<e.minX||e.maxY<e.minY)throw new Exception("Kích thước khối "+what+" trong catalog không hợp lệ; chưa tạo bảng.");
   return Legend.RotatedBox(e.minX,e.minY,e.maxX,e.maxY,rotation);}
  // Rotated box height of E-OC-1 (catalog entry ReferenceId, same name and source hash) at its source table pose (Legend.Pose of its legend.json rows).
  public static double ReferenceHeight(LegendMap map,string sourceSha256,Func<string,LibEntry> catalog){string what=ReferenceName+" ("+ReferenceId+")";
   var e=catalog==null?null:catalog(ReferenceId);
   if(e==null)throw new Exception("Thiếu ổ cắm tham chiếu "+what+" trong catalog thư viện; không xác định được cỡ ký hiệu E-NHE, chưa tạo bảng.");
   if(!Same(e.name,ReferenceName)||!Same(e.sourceSha256,sourceSha256))throw new Exception("Mục "+ReferenceId+" trong catalog không phải "+ReferenceName+" cùng nguồn với E-NHE; không xác định được cỡ ký hiệu E-NHE, chưa tạo bảng.");
   var rows=map==null?new List<LegendRow>():map.rows.Where(r=>Same(r.sourceSha256,sourceSha256)&&Same(r.name,ReferenceName)).ToList();
   if(rows.Count==0)throw new Exception(what+" không có trong bảng ký hiệu nguồn (legend.json); không xác định được cỡ ký hiệu E-NHE, chưa tạo bảng.");
   var pose=Legend.Pose(rows);if(pose.issue!=null)throw new Exception(what+": hướng/tỉ lệ nguồn không dùng được ("+pose.issue+"); không xác định được cỡ ký hiệu E-NHE, chưa tạo bảng.");
   var b=Box(e,pose.rotation,what);double h=(b[3]-b[1])*pose.cap;if(!Core.Finite(h)||!(h>0))throw new Exception(what+" có chiều cao bằng 0; không xác định được cỡ ký hiệu E-NHE, chưa tạo bảng.");return h;}
  // Plan of one source table: rows = the plan rows of that source in drawing order; [left,right] = its symbol column; pad = cell padding.
  public static LegendLayout016 Plan(IList<PlanRow> rows,LegendMap map,Func<string,LibEntry> catalog,double left,double right,double pad){
   if(rows==null)throw new Exception("Thiếu kế hoạch bảng.");if(!new[]{left,right,pad}.All(Core.Finite)||!(right>left)||!(pad>0))throw new Exception("Cột ký hiệu/khoảng đệm không hợp lệ.");
   var L=new LegendLayout016{pad=pad,gap=pad*GapFactor,oldWidth=right-left};
   for(int i=0;i<rows.Count;i++){var pr=rows[i];var row=new LegendRow016{index=i,key=pr.key};L.rows.Add(row);var kinds=pr.entries.Select(Kind).ToList();
    if(kinds.All(k=>k==null))continue;
    if(kinds.Any(k=>k==null))throw new Exception("Hàng bảng ký hiệu có cả công tắc/E-NHE/quạt trần và ký hiệu khác ("+string.Join(", ",pr.entries.Select(x=>x.name))+"); chưa hỗ trợ, chưa tạo bảng.");
    if(pr.poses.Count!=pr.entries.Count)throw new Exception("Kế hoạch bảng thiếu hướng đặt nguồn; quét lại.");
    row.packed=true;double sum=0,tallest=0;
    for(int j=0;j<pr.entries.Count;j++){var e=pr.entries[j];var pose=pr.poses[j];var b=Box(e,pose.rotation,e.name);double w=b[2]-b[0],h=b[3]-b[1],sc;
     if(kinds[j]==Switch||kinds[j]==Fan)sc=Library.CheckScale(pose.cap);
     else{if(L.referenceId==null){L.referenceHeight=ReferenceHeight(map,e.sourceSha256,catalog);L.referenceId=ReferenceId;L.referenceName=ReferenceName;L.sourceSha256=e.sourceSha256;}
      else if(!Same(L.sourceSha256,e.sourceSha256))throw new Exception("Ký hiệu E-NHE từ nhiều nguồn trong một bảng; chưa tạo bảng.");
      if(!(h>0))throw new Exception(e.name+" có chiều cao bằng 0; không phóng được theo "+ReferenceName+", chưa tạo bảng.");sc=Library.CheckScale(L.referenceHeight/h);}
     var s=new LegendSymbol016{entryId=e.id,name=e.name,kind=kinds[j],scale=sc,rotation=pose.rotation,width=w*sc,height=h*sc};row.symbols.Add(s);sum+=s.width;tallest=Math.Max(tallest,s.height);}
    row.groupWidth=sum+L.gap*(row.symbols.Count-1);row.minHeight=tallest+2*pad;}
   L.requiredWidth=Math.Max(L.oldWidth,L.rows.Where(r=>r.packed).Select(r=>r.groupWidth+2*pad).DefaultIfEmpty(0).Max());L.delta=L.requiredWidth-L.oldWidth;
   foreach(var g in L.rows.Where(q=>q.packed)){double x=left+(L.requiredWidth-g.groupWidth)/2;foreach(var s in g.symbols){s.cx=x+s.width/2;x+=s.width+L.gap;}}
   return L;}
  // ---- placement clarification (user 2026-10-09): a NEW Model placement of E-NHE-RJ45 / E-NHE-TEL (ID AND name) is as tall as E-OC-1 placed at 1× ----
  // Default uniform scale = local unrotated catalog height of E-OC-1 (ReferenceId, same name, same source hash) / local unrotated catalog height of the
  // E-NHE entry. E-OC-1 itself is placed raw 1×, so its catalog height IS its placed height; legend.json is not needed. Every other entry → 1.
  // Missing/foreign/zero-height reference or zero-height entry → exception (no fallback). Raw drawing units; no unit conversion. Only the scale of the
  // new insertion; library, source files and blocks already in drawings are unchanged, and SCALE afterwards is the user's choice.
  public static double PlacementScale(LibEntry entry,Func<string,LibEntry> catalog){if(entry==null)throw new Exception("Thiếu mục thư viện.");
   if(Kind(entry)!=Nhe)return 1;string what=ReferenceName+" ("+ReferenceId+")";
   var r=catalog==null?null:catalog(ReferenceId);
   if(r==null)throw new Exception("Thiếu ổ cắm tham chiếu "+what+" trong catalog thư viện; không xác định được cỡ đặt "+entry.name+", chưa đặt.");
   if(!Same(r.name,ReferenceName)||!Same(r.sourceSha256,entry.sourceSha256))throw new Exception("Mục "+ReferenceId+" trong catalog không phải "+ReferenceName+" cùng nguồn với "+entry.name+"; không xác định được cỡ đặt, chưa đặt.");
   var rb=Box(r,0,what);var eb=Box(entry,0,entry.name);double rh=rb[3]-rb[1],eh=eb[3]-eb[1];
   if(!Core.Finite(rh)||!(rh>0))throw new Exception(what+" có chiều cao bằng 0; không xác định được cỡ đặt "+entry.name+", chưa đặt.");
   if(!Core.Finite(eh)||!(eh>0))throw new Exception(entry.name+" có chiều cao bằng 0; không phóng được theo "+ReferenceName+", chưa đặt.");
   return Library.CheckScale(rh/eh);}
 }
#endif
}
