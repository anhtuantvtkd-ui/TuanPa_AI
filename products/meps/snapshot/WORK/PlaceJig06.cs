using System;using System.IO;using System.Collections.Generic;using Autodesk.AutoCAD.DatabaseServices;using Autodesk.AutoCAD.Geometry;using Autodesk.AutoCAD.EditorInput;using Autodesk.AutoCAD.GraphicsInterface;using Ex=System.Exception;
namespace Tvxd.Meps {
 // 0.6.0 cursor preview (TVXD-MEPS-IMPLEMENT-008). The library file of the entry is read into a side database (never saved); its definition closure is
 // flattened into NON-database-resident copies (nested references expanded with their transforms, visible attribute references, attribute-definition
 // defaults as text, as an inserted reference shows them). The jig draws those copies under one model transform per frame; nothing is added to the
 // target drawing until the point is accepted and LibraryCad06.Insert runs. Dispose releases the copies and the side database (the copies keep their
 // side layer/style ids, so the side database stays open while they are drawn).
 // If the side copy cannot be read the preview falls back to the catalog extents outline (the insertion itself is unaffected and checks the file again).
 public sealed class PlacementPreview:IDisposable {
  Database side;readonly List<Entity> items=new List<Entity>();
  public Point3d Origin;public int Skipped;public bool Outline;public string Error;double minX,minY,maxX,maxY;
  public int Count{get{return items.Count;}}
  public string Summary{get{return Count+" đối tượng xem trước"+(Skipped>0?", bỏ qua "+Skipped:"")+(Outline?", khung thay thế"+(Error!=null?" ("+Error+")":""):"");}}
  public static PlacementPreview Build(string root,LibEntry e){var p=new PlacementPreview{minX=e.minX,minY=e.minY,maxX=e.maxX,maxY=e.maxY};
   string file=Library.SafePath(root,e.file);if(!File.Exists(file))throw new Ex("Thiếu file thư viện "+e.file+".");if(Library.Sha256File(file)!=e.fileSha256)throw new Ex("File thư viện khác hash trong catalog; không đặt.");
   try{p.side=new Database(false,true);p.side.ReadDwgFile(file,FileOpenMode.OpenForReadAndAllShare,true,"");p.side.CloseInput(true);
    using(var tr=p.side.TransactionManager.StartTransaction()){var bt=(BlockTable)tr.GetObject(p.side.BlockTableId,OpenMode.ForRead);if(!bt.Has(e.name))throw new Ex("File thư viện không chứa khối "+e.name+".");
     var def=(BlockTableRecord)tr.GetObject(bt[e.name],OpenMode.ForRead);p.Origin=def.Origin;p.Walk(tr,def.ObjectId,Matrix3d.Identity,0,new List<ObjectId>());tr.Commit();}}
   catch(Ex ex){p.Release();p.Error=ex.Message;}
   if(p.items.Count==0)p.Outline=true;return p;}
  void Walk(Transaction tr,ObjectId btrId,Matrix3d m,int depth,List<ObjectId> stack){if(depth>16||stack.Contains(btrId)){Skipped++;return;}stack.Add(btrId);
   foreach(ObjectId id in (BlockTableRecord)tr.GetObject(btrId,OpenMode.ForRead)){var e=tr.GetObject(id,OpenMode.ForRead) as Entity;if(e==null||!e.Visible)continue;
    try{var br=e as BlockReference;
     if(br!=null){Walk(tr,br.BlockTableRecord,m*br.BlockTransform,depth+1,stack);foreach(ObjectId a in br.AttributeCollection){var ar=tr.GetObject(a,OpenMode.ForRead) as AttributeReference;if(ar!=null&&ar.Visible&&!ar.Invisible)Add(ar,m);}continue;}
     var ad=e as AttributeDefinition;if(ad!=null){if(!ad.Invisible&&(depth==0||ad.Constant))AddDefault(ad,m);continue;}
     Add(e,m);}
    catch(Ex){Skipped++;}}
   stack.RemoveAt(stack.Count-1);}
  void Add(Entity e,Matrix3d m){items.Add(m.IsEqualTo(Matrix3d.Identity)?(Entity)e.Clone():e.GetTransformedCopy(m));}
  // An attribute definition shows its default value in an inserted reference (LibraryCad06.Insert copies ad.TextString), not its tag.
  void AddDefault(AttributeDefinition ad,Matrix3d m){if(ad.IsMTextAttributeDefinition){var mt=ad.MTextAttributeDefinition;try{mt.TransformBy(m);items.Add(mt);}catch{mt.Dispose();throw;}return;}
   var t=new DBText();try{t.SetPropertiesFrom(ad);t.Normal=ad.Normal;t.TextStyleId=ad.TextStyleId;t.Height=ad.Height;t.WidthFactor=ad.WidthFactor;t.Oblique=ad.Oblique;t.Rotation=ad.Rotation;t.IsMirroredInX=ad.IsMirroredInX;t.IsMirroredInY=ad.IsMirroredInY;
    t.TextString=ad.TextString;t.Position=ad.Position;t.HorizontalMode=ad.HorizontalMode;t.VerticalMode=ad.VerticalMode;if(!ad.IsDefaultAlignment)t.AlignmentPoint=ad.AlignmentPoint;t.TransformBy(m);items.Add(t);}catch{t.Dispose();throw;}}
  // Block transform of a reference at 'at' (WCS), uniform scale and rotation about +Z: the same mapping LibraryCad06.Insert's reference will have.
  public static Matrix3d Transform(Point3d at,double scale,double rotation,Point3d origin){return Matrix3d.Displacement(at.GetAsVector())*Matrix3d.Rotation(rotation,Vector3d.ZAxis,Point3d.Origin)*Matrix3d.Scaling(scale,Point3d.Origin)*Matrix3d.Displacement(Point3d.Origin-origin);}
  public void Draw(WorldGeometry g,Matrix3d m){g.PushModelTransform(m);
   try{if(Outline||items.Count==0){var o=Origin.GetAsVector();g.Polygon(new Point3dCollection(new[]{new Point3d(minX,minY,0)+o,new Point3d(maxX,minY,0)+o,new Point3d(maxX,maxY,0)+o,new Point3d(minX,maxY,0)+o}));}
    else foreach(var e in items)g.Draw(e);}
   finally{g.PopModelTransform();}}
  void Release(){foreach(var e in items)try{e.Dispose();}catch{}items.Clear();if(side!=null){try{side.Dispose();}catch{}side=null;}}
  public bool Disposed{get{return side==null&&items.Count==0;}}
  public void Dispose(){Release();}
 }
 // Point input with the symbol following the cursor (OSNAP/ORTHO/typed coordinates as for any point prompt). Frames/Samples count WorldDraw calls and
 // accepted cursor positions: QA evidence that a live drag happened (they cannot prove what the user saw); Frames counts only frames drawn without error.
 // No managed exception leaves the native callbacks: the first one is kept in Error, the jig cancels at the next sample, and the caller must refuse the
 // insertion whenever Error is set (even if a point was accepted), so a broken or invisible preview never ends in a placement reported as done.
 public sealed class PlaceJig06:DrawJig {
  readonly PlacementPreview preview;readonly double scale,rotation;readonly string prompt;Point3d point;bool has;public int Frames,Samples;public string Error;
  public Point3d Point{get{return point;}}
  public PlaceJig06(PlacementPreview preview,double scale,double rotation,string prompt){this.preview=preview;this.scale=scale;this.rotation=rotation;this.prompt=prompt;}
  void Fail(string where,Ex ex){if(Error==null)Error=where+": "+ex.GetType().Name+": "+ex.Message;}
  protected override SamplerStatus Sampler(JigPrompts prompts){if(Error!=null)return SamplerStatus.Cancel;
   try{var o=new JigPromptPointOptions(prompt){UserInputControls=UserInputControls.Accept3dCoordinates};var r=prompts.AcquirePoint(o);
    if(r.Status!=PromptStatus.OK)return SamplerStatus.Cancel;if(Error!=null)return SamplerStatus.Cancel;if(has&&r.Value.IsEqualTo(point))return SamplerStatus.NoChange;point=r.Value;has=true;Samples++;return SamplerStatus.OK;}
   catch(Ex ex){Fail("Sampler",ex);return SamplerStatus.Cancel;}}
  protected override bool WorldDraw(WorldDraw draw){if(!has||Error!=null)return true;
   try{preview.Draw(draw.Geometry,PlacementPreview.Transform(point,scale,rotation,preview.Origin));Frames++;}catch(Ex ex){Fail("WorldDraw",ex);}return true;}
 }
}
