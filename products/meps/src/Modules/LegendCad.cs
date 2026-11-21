using System;using System.IO;using System.Linq;using System.Text;using System.Collections.Generic;using System.Globalization;using Ex=System.Exception;
#if !LEGEND_PURE
using Autodesk.AutoCAD.DatabaseServices;using Autodesk.AutoCAD.Geometry;using Autodesk.AutoCAD.Colors;
#endif
namespace Tvxd.Meps {
 // TVXD-MEPS-LEGEND-011 (no CAD types; TESTS/BUILD011.ps1 compiles it alone with /define:LEGEND_PURE). In a centred cell the MText attachment MiddleCenter decides
 // the horizontal position, but a paragraph alignment option inside the raw source contents (\p…q<l|r|c|j|d>…;) overrides it. Center removes ONLY those
 // alignment options (an emptied \p…; code is dropped); every other character, escape (\\ \{ \}), font (\f), height, colour, indent, tab and paragraph break
 // (\P) is kept verbatim. removed = number of \p codes changed. This is an intentional, recorded layout change; the visible text is unchanged.
 public static class LegendText {
  static bool Align(string o){return o.Length==2&&o[0]=='q'&&"lrcjd".IndexOf(o[1])>=0;}
  public static string Center(string contents,out int removed){removed=0;if(string.IsNullOrEmpty(contents))return contents??"";var sb=new StringBuilder(contents.Length);int i=0,n=contents.Length;
   while(i<n){char c=contents[i];
    if(c!='\\'||i+1>=n){sb.Append(c);i++;continue;}
    char d=contents[i+1];if(d!='p'){sb.Append(c).Append(d);i+=2;continue;}
    int end=contents.IndexOf(';',i+2);if(end<0){sb.Append(contents,i,n-i);break;}
    string body=contents.Substring(i+2,end-i-2);bool x=body.StartsWith("x",StringComparison.Ordinal);var opts=(x?body.Substring(1):body).Split(',');var kept=opts.Where(o=>!Align(o)).ToList();
    if(kept.Count==opts.Length){sb.Append(contents,i,end-i+1);}
    else{removed++;kept.RemoveAll(o=>o.Length==0);if(kept.Count>0)sb.Append("\\p").Append(x?"x":"").Append(string.Join(",",kept)).Append(';');}
    i=end+1;}
   return sb.ToString();}
  // 012: DIỄN GIẢI body cell (MiddleLeft attachment). The same removal of paragraph alignment options only, so the attachment's default left alignment
  // applies to every paragraph; everything else stays verbatim.
  public static string Left(string contents,out int removed){return Center(contents,out removed);}
 }
#if !LEGEND_PURE
 public sealed class MapReport {public string source,sourceSha256,status;public int rows,ok,missing,ambiguous,unresolved;public List<string> issues=new List<string>();
  public string Summary(){return source+": bảng "+status+", "+rows+" ký hiệu trong bảng (đủ mô tả "+ok+", thiếu "+missing+", mơ hồ "+ambiguous+", khối ẩn danh chưa rõ "+unresolved+")"+(issues.Count>0?"; "+string.Join("; ",issues.Where(i=>!i.StartsWith("INFO")).Take(3)):"");}}
 // spec/note/status/layout/blanks (TVXD-MEPS-LEGEND-009): the written cell contents, the plan row status and layout, and the names of the empty cells.
 public sealed class LegendOutRow {public double top,bottom;public List<string> names=new List<string>(),handles=new List<string>(),blanks=new List<string>();public string description,spec,note,status,layout;}
 // centered/normalized/height (TVXD-MEPS-LEGEND-011): centred layout, number of cells whose paragraph alignment codes were removed (LegendText.Center) and
 // the total table height below its top at y = 0.
 public sealed class LegendResult {public string path;public int rows,symbols,texts,normalized;public bool centered;public List<LegendOutRow> table=new List<LegendOutRow>();public List<string> notes=new List<string>();public double width,headerTop,headerBottom,height;
#if CABLES016
  // TVXD-MEPS-CABLES-016 addendum: one Layout016 plan per source table of a centred legend (planned scales, column growth, final symbol boxes, row heights).
  public List<LegendLayout016> layouts016=new List<LegendLayout016>();
#endif
 }
 // 0.5 legend: (1) reads the source legend table natively (MText.Text and raw Contents, real extents, dynamic block originals) into Legend.Parse and
 // stores the mapping in LIBRARY_0_5/legend.json; (2) lists MEPS references of the current model space by their library marker; (3) writes a NEW DWG
 // with copies of the used symbols and the exact source texts. The source and the current drawing are only read.
 public static class LegendCad {
  static Extents3d? Ext(Entity e){try{return e.GeometricExtents;}catch{return null;}}
  static void Box(TText t,Extents3d x){t.minX=x.MinPoint.X;t.minY=x.MinPoint.Y;t.maxX=x.MaxPoint.X;t.maxY=x.MaxPoint.Y;}
  static void Anchor(TText t,Point3d p,double rotation,Vector3d normal,int align){t.anchored=true;t.ax=p.X;t.ay=p.Y;t.rotation=rotation;t.nx=normal.X;t.ny=normal.Y;t.nz=normal.Z;t.align=align;}
  // Definition geometry (attributes excluded: far-away or empty attributes must not move a symbol into another row), transformed to WCS.
  static Extents3d? DefBox(Transaction tr,BlockReference br){var b=(BlockTableRecord)tr.GetObject(br.BlockTableRecord,OpenMode.ForRead);Extents3d? all=null;
   foreach(ObjectId id in b){var e=tr.GetObject(id,OpenMode.ForRead) as Entity;if(e==null||e is AttributeDefinition||!e.Visible)continue;var x=Ext(e);if(x==null)continue;if(all==null)all=x;else{var a=all.Value;a.AddExtents(x.Value);all=a;}}
   if(all==null)return null;var m=br.BlockTransform;var lo=all.Value.MinPoint;var hi=all.Value.MaxPoint;Extents3d? r=null;
   foreach(var px in new[]{lo.X,hi.X})foreach(var py in new[]{lo.Y,hi.Y})foreach(var pz in new[]{lo.Z,hi.Z}){var p=new Point3d(px,py,pz).TransformBy(m);if(r==null)r=new Extents3d(p,p);else{var q=r.Value;q.AddPoint(p);r=q;}}
   return r;}
  // Model-space LINEs (and straight segments of lightweight polylines), MTEXT/TEXT and block references of the open source database.
  public static TableInput Extract(Database db,string sha,string fileName){var t=new TableInput{sourceSha256=sha,sourceFile=fileName};
   using(var tr=db.TransactionManager.StartTransaction()){foreach(ObjectId id in (BlockTableRecord)tr.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db),OpenMode.ForRead)){var e=tr.GetObject(id,OpenMode.ForRead) as Entity;if(e==null)continue;string h=e.Handle.ToString();
     var ln=e as Line;if(ln!=null){t.lines.Add(new TLine{handle=h,layer=e.Layer,x1=ln.StartPoint.X,y1=ln.StartPoint.Y,x2=ln.EndPoint.X,y2=ln.EndPoint.Y});continue;}
     var pl=e as Polyline;if(pl!=null){int n=pl.NumberOfVertices,segs=pl.Closed?n:n-1;for(int i=0;i<segs;i++){if(Math.Abs(pl.GetBulgeAt(i))>1e-12)continue;var a=pl.GetPoint3dAt(i);var b=pl.GetPoint3dAt((i+1)%n);t.lines.Add(new TLine{handle=h+"#"+i,layer=e.Layer,x1=a.X,y1=a.Y,x2=b.X,y2=b.Y});}continue;}
     // 0.5.1 (F07): anchor metadata for Legend.AnchorColumn. MText: Location + attachment column; TEXT: Position (default left) or AlignmentPoint.
     var mt=e as MText;if(mt!=null){var x=new TText{handle=h,layer=e.Layer,text=mt.Text,contents=mt.Contents,style=mt.TextStyleName,height=mt.TextHeight};var ex=Ext(mt);if(ex!=null)Box(x,ex.Value);else{x.minX=x.maxX=mt.Location.X;x.minY=x.maxY=mt.Location.Y;}
      int ap=(int)mt.Attachment;Anchor(x,mt.Location,mt.Rotation,mt.Normal,ap>=1&&ap<=9?(ap-1)%3:-1);t.texts.Add(x);continue;}
     var dt=e as DBText;if(dt!=null&&!(e is AttributeDefinition)){var x=new TText{handle=h,layer=e.Layer,text=dt.TextString,contents=dt.TextString,style=dt.TextStyleName,height=dt.Height};var ex=Ext(dt);if(ex!=null)Box(x,ex.Value);else{x.minX=x.maxX=dt.Position.X;x.minY=x.maxY=dt.Position.Y;}
      var hm=dt.HorizontalMode;Anchor(x,dt.IsDefaultAlignment?dt.Position:dt.AlignmentPoint,dt.Rotation,dt.Normal,hm==TextHorizontalMode.TextLeft?0:hm==TextHorizontalMode.TextCenter||hm==TextHorizontalMode.TextMid?1:hm==TextHorizontalMode.TextRight?2:-1);t.texts.Add(x);continue;}
     var br=e as BlockReference;if(br!=null){var b=(BlockTableRecord)tr.GetObject(br.BlockTableRecord,OpenMode.ForRead);var ins=new TInsert{handle=h,layer=e.Layer,blockName=b.Name,name=b.Name,x=br.Position.X,y=br.Position.Y,z=br.Position.Z,rotation=br.Rotation,sx=br.ScaleFactors.X,sy=br.ScaleFactors.Y,sz=br.ScaleFactors.Z,nx=br.Normal.X,ny=br.Normal.Y,nz=br.Normal.Z};
      // Anonymous dynamic-block representation (*U82 in the source): the symbol is the original definition from DynamicBlockTableRecord.
      if(br.IsDynamicBlock&&!br.DynamicBlockTableRecord.IsNull){ins.name=((BlockTableRecord)tr.GetObject(br.DynamicBlockTableRecord,OpenMode.ForRead)).Name;ins.dynamic=true;}
      var box=DefBox(tr,br);if(box!=null){ins.minX=box.Value.MinPoint.X;ins.minY=box.Value.MinPoint.Y;ins.maxX=box.Value.MaxPoint.X;ins.maxY=box.Value.MaxPoint.Y;}else{ins.minX=ins.maxX=ins.x;ins.minY=ins.maxY=ins.y;}t.inserts.Add(ins);}}
    tr.Commit();}
   return t;}
  static LegendLayer LayerInfo(Transaction tr,LayerTableRecord l){var c=l.Color;var r=new LegendLayer{name=l.Name,lineweight=(int)l.LineWeight,color=c.ToString()};
   r.linetype=((LinetypeTableRecord)tr.GetObject(l.LinetypeObjectId,OpenMode.ForRead)).Name;if(c.ColorMethod==ColorMethod.ByColor){r.trueColor=true;r.red=c.Red;r.green=c.Green;r.blue=c.Blue;}else r.aci=c.ColorIndex;return r;}
  // Reads the table of one source into legend.json (only this source's records are replaced). The source must keep its hash while it is read.
  public static MapReport MapSource(string root,string sourcePath){if(!File.Exists(sourcePath))throw new Ex("Không thấy file nguồn.");string sha=Library.Sha256File(sourcePath);var rep=new MapReport{source=Path.GetFileName(sourcePath),sourceSha256=sha};
   LegendSource s;List<LegendRow> rows;var previous=HostApplicationServices.WorkingDatabase;
   using(var db=LibraryCad.OpenSource(sourcePath,Library.SafePath(root,"logs"))){
    try{HostApplicationServices.WorkingDatabase=db;var t=Extract(db,sha,rep.source);s=Legend.Parse(t,out rows);s.insunits=(int)db.Insunits;s.units=db.Insunits.ToString();s.extractedUtc=DateTime.UtcNow.ToString("o",CultureInfo.InvariantCulture);
     if(s.status==Legend.Table){var cells=s.headers.Concat(rows.SelectMany(r=>new[]{r.description,r.spec,r.note})).ToList();if(s.title!=null)cells.Add(s.title);
      var layerNames=new HashSet<string>(cells.Select(c=>c.layer).Concat(rows.Select(r=>r.layer)).Concat(new[]{s.gridLayer}).Where(n=>!string.IsNullOrEmpty(n)),StringComparer.OrdinalIgnoreCase);
      var styleNames=new HashSet<string>(cells.Select(c=>c.style).Where(n=>!string.IsNullOrEmpty(n)),StringComparer.OrdinalIgnoreCase);
      using(var tr=db.TransactionManager.StartTransaction()){var lt=(LayerTable)tr.GetObject(db.LayerTableId,OpenMode.ForRead);foreach(var n in layerNames)if(lt.Has(n))s.layers.Add(LayerInfo(tr,(LayerTableRecord)tr.GetObject(lt[n],OpenMode.ForRead)));
       var st=(TextStyleTable)tr.GetObject(db.TextStyleTableId,OpenMode.ForRead);foreach(var n in styleNames){if(!st.Has(n))continue;var r=(TextStyleTableRecord)tr.GetObject(st[n],OpenMode.ForRead);var f=r.Font;
        s.styles.Add(new LegendStyle{name=r.Name,fileName=r.FileName,bigFontFileName=r.BigFontFileName,typeface=f.TypeFace,bold=f.Bold,italic=f.Italic,charset=f.CharacterSet,pitch=f.PitchAndFamily,xScale=r.XScale,oblique=r.ObliquingAngle,textSize=r.TextSize,flagBits=r.FlagBits,vertical=r.IsVertical});}
       tr.Commit();}}}
    finally{HostApplicationServices.WorkingDatabase=previous;}}
   if(Library.Sha256File(sourcePath)!=sha)throw new Ex("File nguồn đã thay đổi trong lúc đọc bảng; legend.json không được ghi.");
   var map=Legend.Load(root);Legend.Merge(map,s,rows);Legend.Save(root,map);
   rep.status=s.status;rep.rows=rows.Count;rep.ok=rows.Count(r=>r.status==Legend.Ok);rep.missing=rows.Count(r=>r.status==Legend.MissingDescription);rep.ambiguous=rows.Count(r=>r.status==Legend.Ambiguous);rep.unresolved=rows.Count(r=>r.status==Legend.UnresolvedDynamic);rep.issues.AddRange(s.issues);return rep;}
  // Model-space references only (paper space is never read; erased objects are not enumerated). The marker can sit on the reference's record or, for a
  // dynamic block whose properties were changed, on its original definition. Older 0.3.1/0.4 placements are recognized by the same stable entry ID.
  public static List<UsedRef> ScanUsed(Database db){var list=new List<UsedRef>();
   using(var tr=db.TransactionManager.StartOpenCloseTransaction()){foreach(ObjectId id in (BlockTableRecord)tr.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db),OpenMode.ForRead)){if(id.IsErased)continue;var br=tr.GetObject(id,OpenMode.ForRead) as BlockReference;if(br==null)continue;
     var b=(BlockTableRecord)tr.GetObject(br.BlockTableRecord,OpenMode.ForRead);if(b.IsFromExternalReference||b.IsFromOverlayReference||b.IsDependent){list.Add(new UsedRef{handle=br.Handle.ToString(),xref=true});continue;}
     var mark=LibraryCad.ReadMarker(tr,b);if(mark==null&&br.IsDynamicBlock&&!br.DynamicBlockTableRecord.IsNull)mark=LibraryCad.ReadMarker(tr,(BlockTableRecord)tr.GetObject(br.DynamicBlockTableRecord,OpenMode.ForRead));
     if(mark==null||mark.Length<3)continue;list.Add(new UsedRef{handle=br.Handle.ToString(),entryId=mark[0],name=mark[2],sourceSha256=mark.Length>=4?mark[3]:null});}tr.Commit();}
   return list;}
  static LibEntry Copy(LibEntry e){var j=Library.Json();return j.Deserialize<LibEntry>(j.Serialize(e));}
  static ObjectId EnsureLayer(Transaction tr,Database db,LegendSource s,string name,List<string> notes){if(string.IsNullOrEmpty(name))return db.LayerZero;var lt=(LayerTable)tr.GetObject(db.LayerTableId,OpenMode.ForRead);if(lt.Has(name))return lt[name];
   var info=s.layers.FirstOrDefault(l=>string.Equals(l.name,name,StringComparison.OrdinalIgnoreCase));var rec=new LayerTableRecord{Name=name};
   if(info!=null){rec.Color=info.trueColor?Color.FromRgb((byte)info.red,(byte)info.green,(byte)info.blue):Color.FromColorIndex(ColorMethod.ByAci,(short)(info.aci<0?7:info.aci));if(Lineweight.IsValid(info.lineweight)||info.lineweight==Lineweight.Default)rec.LineWeight=(LineWeight)info.lineweight;
    var lts=(LinetypeTable)tr.GetObject(db.LinetypeTableId,OpenMode.ForRead);if(info.linetype!=null&&lts.Has(info.linetype))rec.LinetypeObjectId=lts[info.linetype];else if(info.linetype!=null)notes.Add("Layer "+name+": linetype nguồn "+info.linetype+" chưa có trong DWG mới, dùng Continuous.");}
   else notes.Add("Layer "+name+": không có thông tin nguồn, tạo mặc định.");
   lt.UpgradeOpen();var id=lt.Add(rec);tr.AddNewlyCreatedDBObject(rec,true);return id;}
  // F03: the source style is recreated under Legend.StyleName(source hash, name) with every recorded property, then read back; a name already present in
  // the new DWG (STANDARD, another source) is never reused. A cell whose style was not recorded stops the build instead of falling back to a default font.
  static ObjectId EnsureStyle(Transaction tr,Database db,LegendSource s,string name,List<string> notes){if(string.IsNullOrEmpty(name))throw new Ex("Ô chữ nguồn không ghi kiểu chữ; đọc lại bảng nguồn ('+ Thêm block').");
   var info=s.styles.FirstOrDefault(x=>string.Equals(x.name,name,StringComparison.OrdinalIgnoreCase));if(info==null)throw new Ex("Kiểu chữ nguồn "+name+" chưa có trong legend.json; bấm '+ Thêm block' với file nguồn "+s.sourceFile+" để đọc lại bảng.");
   string target=Legend.StyleName(s.sourceSha256,info.name);SymbolUtilityServices.ValidateSymbolName(target,false);var st=(TextStyleTable)tr.GetObject(db.TextStyleTableId,OpenMode.ForRead);if(st.Has(target))return st[target];
   if(!(info.xScale>0))throw new Ex("Kiểu chữ nguồn "+name+" có hệ số rộng không hợp lệ "+info.xScale+".");
   // Font descriptor last, after file names and table insertion (CAD drops it when FileName is assigned; NATIVE009).
   var rec=new TextStyleTableRecord{Name=target};
   rec.FileName=info.fileName??"";rec.BigFontFileName=info.bigFontFileName??"";rec.XScale=info.xScale;rec.ObliquingAngle=info.oblique;rec.TextSize=info.textSize;rec.IsVertical=info.vertical;rec.FlagBits=(byte)info.flagBits;
   st.UpgradeOpen();var id=st.Add(rec);tr.AddNewlyCreatedDBObject(rec,true);LibraryCad06.RestoreFont(rec,info);
   var f=rec.Font;Func<string,string,bool> same=(a,b)=>string.Equals(a??"",b??"",StringComparison.OrdinalIgnoreCase);
   if(!same(rec.FileName,info.fileName)||!same(rec.BigFontFileName,info.bigFontFileName)||!same(f.TypeFace,info.typeface)||f.Bold!=info.bold||f.Italic!=info.italic||f.CharacterSet!=info.charset||f.PitchAndFamily!=info.pitch||rec.XScale!=info.xScale||rec.ObliquingAngle!=info.oblique||rec.TextSize!=info.textSize||rec.IsVertical!=info.vertical||rec.FlagBits!=info.flagBits)
    throw new Ex("Kiểu chữ "+target+" không giữ được đúng thuộc tính nguồn "+name+" (font "+rec.FileName+"/"+f.TypeFace+"); không tạo bảng với font khác.");
   notes.Add("Kiểu chữ nguồn "+name+" → "+target+" ("+(string.IsNullOrEmpty(info.typeface)?info.fileName:info.typeface+", "+info.fileName)+").");return id;}
  // Padding inside cells: 0.4 × the source heading text height (layout only).
  static double Pad(LegendSource s){double h=s.headers.Select(c=>c.height).Where(v=>v>0).DefaultIfEmpty(0).Max();return h>0?0.4*h:0.02*Math.Max(1,s.headerTop-s.headerBottom);}
  static MText Text(Transaction tr,Database db,BlockTableRecord space,LegendSource s,LegendCell c,Point3d at,double width,AttachmentPoint attach,List<string> notes){return Text(tr,db,space,s,c,c.contents,at,width,attach,notes);}
  static MText Text(Transaction tr,Database db,BlockTableRecord space,LegendSource s,LegendCell c,string contents,Point3d at,double width,AttachmentPoint attach,List<string> notes){
   var m=new MText();m.SetDatabaseDefaults(db);m.Contents=contents;m.TextHeight=c.height>0?c.height:s.headers.Select(x=>x.height).DefaultIfEmpty(0).Max();m.Width=width;m.Attachment=attach;m.Location=at;
   m.TextStyleId=EnsureStyle(tr,db,s,c.style,notes);m.LayerId=EnsureLayer(tr,db,s,c.layer,notes);space.AppendEntity(m);tr.AddNewlyCreatedDBObject(m,true);return m;}
  // Full-width horizontal lines at the table top and every row boundary; outer borders from the table top, inner column borders from the heading top.
  static void Grid(Transaction tr,Database db,BlockTableRecord space,LegendSource s,double[] X,List<double> ys,double tableTop,double headTop,List<string> notes){var layer=EnsureLayer(tr,db,s,s.gridLayer,notes);
   Action<Point3d,Point3d> line=(a,b)=>{var l=new Line(a,b);l.SetDatabaseDefaults(db);l.LayerId=layer;space.AppendEntity(l);tr.AddNewlyCreatedDBObject(l,true);};
   line(new Point3d(X[0],tableTop,0),new Point3d(X[4],tableTop,0));foreach(var y in ys)line(new Point3d(X[0],y,0),new Point3d(X[4],y,0));
   foreach(var x in new[]{X[0],X[4]})line(new Point3d(x,tableTop,0),new Point3d(x,ys.Last(),0));for(int k=1;k<4;k++)line(new Point3d(X[k],headTop,0),new Point3d(X[k],ys.Last(),0));}
  // Writes a NEW DWG (never overwrites: an existing target is rejected, the file is saved to a temporary name and moved without replace).
  // Per source: title and headings from the source table, then one row per used source row: copies of every used symbol of that row, scaled only to fit
  // their slot and the row's DIỄN GIẢI / QUY CÁCH / GHI CHÚ as the exact raw MText contents. Each copy keeps its source table rotation (F04); its rotated box
  // is fitted to the slot (Legend.FitScale, never above the source table scale). Row height = max(source row height, wrapped text height + padding).
  // Column widths, text heights, namespaced styles (F03) and layers come from the source table record in legend.json. Only frozen copies are used (F02).
  // TVXD-MEPS-LEGEND-009: also accepts a Legend.PlanForExport plan, whose rows may have empty cells (written as nothing) and appended rows (own row after the
  // source rows, height Legend.TypicalRowHeight). The title row is centred over the four columns; body texts stay top-left; symbols centred in their slot.
  // TVXD-MEPS-LEGEND-011: Build keeps exactly this layout for historical callers/tests; BuildCentered (new DWG) and Stage (in-memory, for the Model insert)
  // use the same engine (Draw) with centred = true.
  public static LegendResult Build(string root,LegendMap map,LegendPlan plan,string target){return Save(root,map,plan,target,false);}
  // Centred layout: every non-empty title, heading and body text is MiddleCenter in its cell (width = cell width − 2 × padding, contents through
  // LegendText.Center); each text is measured (MText.ActualHeight after wrapping) and the row height = max(source row height, tallest text + 2 × padding)
  // is fixed BEFORE any text of that row is moved to the centre of its final cell. Empty cells stay empty; texts, styles, layers and heights as in Build.
  // TVXD-MEPS-LAYERS-012 user addendum (latest layout, method name kept): the DIỄN GIẢI BODY cell is MiddleLeft at x = left division + padding, same
  // width, contents through LegendText.Left, so it reads left to right, wraps top to bottom and the paragraph block sits at the measured final row centre.
  // Title, headings, symbols, QUY CÁCH and GHI CHÚ stay centred. Legacy Build keeps its historical top-left body texts.
  public static LegendResult BuildCentered(string root,LegendMap map,LegendPlan plan,string target){return Save(root,map,plan,target,true);}
  // Centred legend drawn into the model space of db, an owned side database that is never saved (LegendPlace011 clones it into the user's Model as one
  // block). db must be empty. TVXD-MEPS-LAYERS-012: its INSUNITS is Undefined (unitless staging, LegendPlace011.TablePolicy); the source numeric layout is
  // kept 1:1 and the source INSUNITS marker is not applied as a physical unit. Build/BuildCentered (new DWG files) keep the source INSUNITS as before.
  public static LegendResult Stage(string root,LegendMap map,LegendPlan plan,Database db){var sources=Sources(map,plan);var res=new LegendResult{centered=true};var previous=HostApplicationServices.WorkingDatabase;
   try{HostApplicationServices.WorkingDatabase=db;db.Insunits=UnitsValue.Undefined;Draw(root,map,plan,sources,db,true,res);}finally{HostApplicationServices.WorkingDatabase=previous;}
   res.notes=res.notes.Distinct().ToList();return res;}
  // Plan and source-table checks shared by every output.
  static List<LegendSource> Sources(LegendMap map,LegendPlan plan){
   if(!plan.CanExport)throw new Ex("Chưa tạo bảng: "+(plan.rows.Count==0?"không có ký hiệu":(plan.policy==Legend.PolicyBlank?"còn mục chặn: ":"thiếu mô tả cho ")+string.Join("; ",plan.missing.Take(10))));
   foreach(var pr in plan.rows){if(pr.poses.Count!=pr.entries.Count||pr.poses.Any(o=>o==null||o.issue!=null))throw new Ex("Kế hoạch bảng thiếu hướng đặt nguồn; quét lại.");
    var bad=pr.entries.FirstOrDefault(x=>Meps05.PlaceIssue(x)!=null);if(bad!=null)throw new Ex("Chưa tạo bảng: "+bad.name+": "+Meps05.StyleCode(bad)+" — "+Meps05.PlaceIssue(bad));}
   var sources=plan.rows.Select(r=>r.sourceSha256.ToLowerInvariant()).Distinct().Select(sha=>map.sources.First(s=>string.Equals(s.sourceSha256,sha,StringComparison.OrdinalIgnoreCase))).ToList();
   if(sources.Any(s=>s.status!=Legend.Table||s.columns.Count!=5))throw new Ex("Bảng nguồn không hợp lệ cho việc tạo bảng ký hiệu.");
   if(sources.Select(s=>s.insunits).Distinct().Count()>1)throw new Ex("Ký hiệu đến từ các nguồn khác đơn vị; V1 chưa ghép chung một bảng.");
   return sources;}
  static LegendResult Save(string root,LegendMap map,LegendPlan plan,string target,bool centered){var sources=Sources(map,plan);
   if(string.IsNullOrWhiteSpace(target)||!Path.IsPathRooted(target)||!string.Equals(Path.GetExtension(target),".dwg",StringComparison.OrdinalIgnoreCase))throw new Ex("Chọn đường dẫn file .dwg mới.");
   if(File.Exists(target))throw new Ex("File đã tồn tại, không ghi đè: "+target);
   string dir=Path.GetDirectoryName(target);if(!Directory.Exists(dir))throw new Ex("Thư mục không tồn tại: "+dir);
   string temp=Path.Combine(dir,"~"+Path.GetFileNameWithoutExtension(target)+"."+Guid.NewGuid().ToString("N").Substring(0,8)+".tmp.dwg");var res=new LegendResult{path=target,centered=centered};var previous=HostApplicationServices.WorkingDatabase;
   try{using(var db=new Database(true,true)){HostApplicationServices.WorkingDatabase=db;db.Insunits=(UnitsValue)sources[0].insunits;Draw(root,map,plan,sources,db,centered,res);
     HostApplicationServices.WorkingDatabase=previous;db.SaveAs(temp,DwgVersion.Current);}}
   finally{HostApplicationServices.WorkingDatabase=previous;}
   try{File.Move(temp,target);}finally{if(File.Exists(temp))File.Delete(temp);}
   res.notes=res.notes.Distinct().ToList();return res;}
  // 011: one centred text, appended at the top centre of its cell only to be measured; the caller moves it to the centre of the final cell.
  // 012 (user addendum): left = true for a DIỄN GIẢI body cell: MiddleLeft at left + padding (same width), the caller moves it to (left + pad, row centre).
  static MText Centred(Transaction tr,Database db,BlockTableRecord space,LegendSource s,LegendCell c,double left,double right,double top,double pad,LegendResult res,string cellName,bool leftAligned=false){
   int removed;string contents=leftAligned?LegendText.Left(c.contents,out removed):LegendText.Center(c.contents,out removed);if(removed>0){res.normalized++;res.notes.Add((leftAligned?"Căn trái":"Căn giữa")+": bỏ "+removed+" mã căn lề đoạn (\\p…q…;) trong ô "+cellName+"; chữ, font và cỡ chữ nguồn giữ nguyên.");}
   var m=Text(tr,db,space,s,c,contents,new Point3d(leftAligned?left+pad:(left+right)/2,top,0),right-left-2*pad,leftAligned?AttachmentPoint.MiddleLeft:AttachmentPoint.MiddleCenter,res.notes);res.texts++;
   if(m.ActualWidth>m.Width*(1+1e-9)+1e-9)res.notes.Add("CẢNH BÁO: ô "+cellName+" có từ dài hơn bề rộng cột ("+Library.F(m.ActualWidth)+" > "+Library.F(m.Width)+"); MEPS không sửa chữ nguồn.");
   return m;}
  static double Measured(MText m,string cellName){double h=m.ActualHeight;if(!Core.Finite(h)||h<0)throw new Ex("Không đo được chiều cao chữ của ô "+cellName+"; chưa tạo bảng.");return h;}
  // TVXD-MEPS-CABLES-016 addendum (CABLES016, centred output only): before the title/headings of each source, Layout016.Plan sizes the switch / E-NHE rows;
  // the symbol column grows by its delta and the other borders move by the same delta. The library catalog is read (Library.Load) only when an E-NHE row
  // needs the E-OC-1 reference. Legacy Build and builds without CABLES016 keep the historical layout.
  static void Draw(string root,LegendMap map,LegendPlan plan,List<LegendSource> sources,Database db,bool centered,LegendResult res){double y=0;
#if CABLES016
   // 021: reference lookup (active entry, else the record of an entry deleted from the tool) — the legend describes symbols already placed in the drawing.
   LibCatalog catalog=null;Func<string,LibEntry> lookup=eid=>{if(catalog==null)catalog=Library.Load(root);return Library.Reference(catalog,eid);};
#endif
   foreach(var s in sources){double[] X=s.columns.Select(x=>x-s.columns[0]).ToArray();double pad=Pad(s);
    var srcRows=plan.rows.Where(r=>string.Equals(r.sourceSha256,s.sourceSha256,StringComparison.OrdinalIgnoreCase)).ToList();
#if CABLES016
    LegendLayout016 lay=null;
    if(centered){lay=Layout016.Plan(srcRows,map,lookup,X[0],X[1],pad);for(int k=1;k<5;k++)X[k]+=lay.delta;res.layouts016.Add(lay);
     if(lay.delta>0)res.notes.Add("Cột KÝ HIỆU rộng thêm "+Library.F(lay.delta)+" ("+Library.F(lay.oldWidth)+" → "+Library.F(lay.requiredWidth)+") để công tắc giữ đúng tỉ lệ nguồn, xếp sát nhau; các cột khác giữ bề rộng.");
     if(lay.referenceId!=null)res.notes.Add("E-NHE phóng đều để cao bằng "+lay.referenceName+" ("+Library.F(lay.referenceHeight)+" đơn vị bản vẽ): "+string.Join(", ",lay.rows.SelectMany(r=>r.symbols).Where(q=>q.kind==Layout016.Nhe).Select(q=>q.name+" × "+Library.F(q.scale)))+"; chỉ bản trong bảng ký hiệu, thư viện không đổi.");}
#endif
    res.width=Math.Max(res.width,X[4]);var ys=new List<double>();double tableTop=y,headTop=y;
    using(var tr=db.TransactionManager.StartTransaction()){var space=(BlockTableRecord)tr.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db),OpenMode.ForWrite);
     if(s.title!=null&&s.titleTop>s.headerTop){double th=s.titleTop-s.headerTop;
      if(!centered){Text(tr,db,space,s,s.title,new Point3d((X[0]+X[4])/2,y-th/2,0),X[4]-2*pad,AttachmentPoint.MiddleCenter,res.notes);res.texts++;}
      else if(s.title.contents.Length>0){var m=Centred(tr,db,space,s,s.title,X[0],X[4],y,pad,res,"tiêu đề");th=Legend.RowHeight(th,Measured(m,"tiêu đề"),pad);m.Location=new Point3d((X[0]+X[4])/2,y-th/2,0);}
      y-=th;ys.Add(y);}
     res.headerTop=headTop=y;double hh=s.headerTop-s.headerBottom;
     if(!centered)for(int k=0;k<4;k++){Text(tr,db,space,s,s.headers[k],new Point3d((X[k]+X[k+1])/2,y-hh/2,0),X[k+1]-X[k]-2*pad,AttachmentPoint.MiddleCenter,res.notes);res.texts++;}
     else{var heads=new MText[4];double need=0;
      for(int k=0;k<4;k++){if(s.headers[k].contents.Length==0)continue;string cellName="tiêu đề cột "+Legend.Headers[k];heads[k]=Centred(tr,db,space,s,s.headers[k],X[k],X[k+1],y,pad,res,cellName);need=Math.Max(need,Measured(heads[k],cellName));}
      hh=Legend.RowHeight(hh,need,pad);for(int k=0;k<4;k++)if(heads[k]!=null)heads[k].Location=new Point3d((X[k]+X[k+1])/2,y-hh/2,0);}
     y-=hh;ys.Add(y);res.headerBottom=y;tr.Commit();}
    for(int ri=0;ri<srcRows.Count;ri++){var pr=srcRows[ri];var row=pr.text;double need=0,h;var o=new LegendOutRow{top=y,description=row.description.contents,spec=row.spec.contents,note=row.note.contents,status=pr.status??row.status,layout=pr.layout};
     for(int k=0;k<3;k++)if(new[]{row.description,row.spec,row.note}[k].contents.Length==0)o.blanks.Add(Legend.CellNames[k]);
     using(var tr=db.TransactionManager.StartTransaction()){var space=(BlockTableRecord)tr.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db),OpenMode.ForWrite);var cells=new[]{row.description,row.spec,row.note};var made=new MText[3];
      for(int k=0;k<3;k++){if(cells[k].contents.Length==0)continue;
       if(!centered){var m=Text(tr,db,space,s,cells[k],new Point3d(X[k+1]+pad,y-pad,0),X[k+2]-X[k+1]-2*pad,AttachmentPoint.TopLeft,res.notes);need=Math.Max(need,m.ActualHeight);res.texts++;}
       else{string cellName="hàng "+(res.rows+1)+" "+Legend.CellNames[k];made[k]=Centred(tr,db,space,s,cells[k],X[k+1],X[k+2],y,pad,res,cellName,k==0);need=Math.Max(need,Measured(made[k],cellName));}}
      h=Legend.RowHeight(row.top-row.bottom,need,pad);
#if CABLES016
      // Packed switch / E-NHE row: taller only when its real-size symbols need it (fixed before any text of the row is centred).
      if(lay!=null&&lay.rows[ri].packed)h=Math.Max(h,lay.rows[ri].minHeight);
#endif
      // DIỄN GIẢI (k = 0) from the left division + padding, vertically centred in the final row; QUY CÁCH / GHI CHÚ at the cell centre.
      for(int k=0;k<3;k++)if(made[k]!=null)made[k].Location=new Point3d(k==0?X[1]+pad:(X[k+1]+X[k+2])/2,y-h/2,0);
      tr.Commit();}
     var slots=Legend.Slots(pr.entries.Count,X[0],X[1]);double slotW=(X[1]-X[0])/pr.entries.Count;
     for(int i=0;i<pr.entries.Count;i++){var e=Copy(pr.entries[i]);e.hidden=false;var pose=pr.poses[i];var box=Legend.RotatedBox(e.minX,e.minY,e.maxX,e.maxY,pose.rotation);
#if CABLES016
      // Packed row: the planned fixed scale (source cap / E-OC-1 height), never Legend.FitScale.
      var p16=lay!=null&&lay.rows[ri].packed?lay.rows[ri].symbols[i]:null;
      double sc=p16!=null?p16.scale:Legend.FitScale(box[2]-box[0],box[3]-box[1],slotW,h,pad,pose.cap);
#else
      double sc=Legend.FitScale(box[2]-box[0],box[3]-box[1],slotW,h,pad,pose.cap);
#endif
      ObjectId layer;using(var tr=db.TransactionManager.StartTransaction()){layer=EnsureLayer(tr,db,s,pose.layer,res.notes);tr.Commit();}db.Clayer=layer;
      // 0.6.0: symbol copies keep their source text styles under the MEPS06_ namespace (a new DWG's STANDARD never replaces the source font).
      var at=new Point3d(slots[i]+slotW/2-(box[0]+box[2])/2*sc,y-h/2-(box[1]+box[3])/2*sc,0);
#if CABLES016
      // Packed row: rotated box centred on its planned cx (packed order, one gap) and on the row centre; the final box is recorded for QA.
      if(p16!=null){at=new Point3d(p16.cx-(box[0]+box[2])/2*sc,at.Y,0);p16.minX=p16.cx-p16.width/2;p16.maxX=p16.cx+p16.width/2;p16.minY=y-h/2-p16.height/2;p16.maxY=y-h/2+p16.height/2;}
#endif
      var id=LibraryCad06.Insert(db,root,e,at,sc,pose.rotation,res.notes);o.names.Add(e.name);o.handles.Add(id.Handle.ToString());res.symbols++;}
#if CABLES016
     if(lay!=null)lay.rows[ri].height=h;
#endif
     db.Clayer=db.LayerZero;y-=h;ys.Add(y);o.bottom=y;res.table.Add(o);res.rows++;}
    using(var tr=db.TransactionManager.StartTransaction()){var space=(BlockTableRecord)tr.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db),OpenMode.ForWrite);Grid(tr,db,space,s,X,ys,tableTop,headTop,res.notes);tr.Commit();}
    res.height=Math.Max(res.height,-ys.Last());y-=s.headerTop-s.headerBottom;}}
 }
#endif
}
