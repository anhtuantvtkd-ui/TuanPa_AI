using System;using System.Text;using System.Globalization;using System.Collections.Generic;using Autodesk.AutoCAD.DatabaseServices;using Autodesk.AutoCAD.Geometry;using Autodesk.AutoCAD.Colors;
namespace Tvxd.Meps {
#if WIRECOMMIT018
 // TVXD-MEPS-WIRESCROLL-018 (WIRECOMMIT018): one continuous route = ONE LWPOLYLINE owned by this object, saved after every accepted endpoint.
 // Database only (AcDb, no Editor/UI): the same calls run in AutoCAD and in CoreConsole QA.
 //  point 1: anchor only, nothing written. point 2: the polyline is created by the existing StandardsCad012.DrawPreset / WireCad010.Draw (metadata in
 //  the same transaction). point 3+: one own transaction appends a vertex to the same polyline. Undo(): one own transaction removes the last vertex, or
 //  erases the owned polyline when it has only one segment (the anchor stays); with only the anchor it resets the anchor. Finish() never erases.
 // Every write first re-checks units / LTSCALE / MSLTSCALE / annotation scale (and the preset snapshot), then that the owned polyline is still exactly
 // what this route wrote (vertices, widths, layer, colour, linetype, lineweight, scale, identity and cable records); any difference refuses the edit and
 // leaves the drawing as it is. The in-memory list changes only after a commit, so a failure keeps every segment already saved.
 // Only the polyline this route created is ever modified or erased. Layers/linetypes created by the first segment stay after an Undo of that segment.
 // Native UNDO after the command may undo the whole route at once; per-segment global UNDO is not promised.
 public sealed class WireRoute018 {
  public const int MaxPoints=2000;public const double Tol=1e-8;
  readonly Database db;readonly WireStyle010 style;readonly WireSettings010 settings;readonly int units;readonly double ltscale,width;
#if STANDARDS012
  readonly PresetSnapshot012 preset;
#endif
  readonly List<Point3d> points=new List<Point3d>();ObjectId route=ObjectId.Null;string signature;bool finished;
  public int PointCount{get{return points.Count;}}
  public int SegmentCount{get{return points.Count<2?0:points.Count-1;}}
  public Point3d? LastPoint{get{return points.Count==0?(Point3d?)null:points[points.Count-1];}}
  public ObjectId ObjectId{get{return route;}}
  public bool Finished{get{return finished;}}
  public IList<Point3d> Points{get{return points.AsReadOnly();}}
  static void Fail(string s){throw new InvalidOperationException(s);}
  static WireSettings010 Copy(WireSettings010 t){return new WireSettings010{msltscale=t.msltscale,annoScale=t.annoScale,annoName=t.annoName};}
  static bool Same(double a,double b){return Math.Abs(a-b)<=1e-12*Math.Max(1,Math.Abs(a));}
  // Captured sample: the same refusals as WireCad010.Draw, checked before the first point.
  public WireRoute018(Database db,WireStyle010 style,WireSettings010 settings){if(db==null||style==null)throw new ArgumentNullException(db==null?"db":"style");
   WireModel010.Validate(style);WireModel010.CheckSettings(style,settings);if((int)db.Insunits!=style.units)Fail("Đơn vị bản vẽ khác mẫu; không tự đổi đơn vị. Chưa vẽ.");
   this.db=db;this.style=style;this.settings=Copy(settings);units=(int)db.Insunits;ltscale=db.Ltscale;width=style.width;}
#if STANDARDS012
  // Profile preset: snapshot taken at the click (StandardsCad012.Snapshot).
  public WireRoute018(Database db,PresetSnapshot012 preset,WireSettings010 settings){if(db==null||preset==null)throw new ArgumentNullException(db==null?"db":"preset");
   WireModel010.Validate(settings);StandardsCad012.CheckSnapshot(db,preset,settings);this.db=db;this.preset=preset;this.settings=Copy(settings);units=(int)db.Insunits;ltscale=db.Ltscale;width=0;}
#endif
  // Message for an error after some segments were saved: no false "not drawn" text, and the saved count stated.
  public static string Retained(string message,int segments){if(segments<=0||message==null)return message;
   string m=message.Replace(" Chưa vẽ.","").Replace("Chưa vẽ.","").Replace("; chưa sửa bản vẽ.",".").Replace(" Chưa thay đổi bản vẽ.","").Replace("Chưa thay đổi bản vẽ.","").TrimEnd();
   return m+" Không vẽ thêm; giữ "+segments+" đoạn đã lưu.";}
  // null when p can be the next vertex; otherwise the reason (the caller re-prompts, nothing is written).
  public string PointIssue(Point3d p){if(!WireModel010.Finite(p.X)||!WireModel010.Finite(p.Y)||!WireModel010.Finite(p.Z))return "Điểm không hợp lệ; chọn lại.";
   if(Math.Abs(p.Z)>Tol)return "Điểm ngoài mặt phẳng XY (Z ≠ 0); chọn lại điểm khác (các đoạn đã lưu giữ nguyên).";
   if(points.Count>0&&p.DistanceTo(points[points.Count-1])<Tol)return "Điểm trùng điểm trước; chọn điểm khác.";
   if(points.Count>=MaxPoints)return "Tuyến đã đủ "+MaxPoints+" điểm; Enter/Esc dừng rồi vẽ tuyến mới.";return null;}
  void Check(WireSettings010 now){if(finished)Fail("Tuyến đã kết thúc; bấm lại nét để vẽ tuyến mới.");
   try{WireModel010.Validate(now);
    if((int)db.Insunits!=units)Fail("INSUNITS đã đổi trong lúc vẽ; MEPS không sửa tuyến.");if(!Same(db.Ltscale,ltscale))Fail("LTSCALE đã đổi trong lúc vẽ; MEPS không sửa tuyến.");
    if(!WireModel010.SameSettings(settings,now))Fail("MSLTSCALE/tỷ lệ chú thích đã đổi trong lúc vẽ; MEPS không sửa tuyến.");
#if STANDARDS012
    if(preset!=null){StandardsCad012.CheckSnapshot(db,preset,now);if(!db.UndoRecording)Fail("Bản vẽ đang tắt ghi UNDO; MEPS không sửa tuyến.");return;}
#endif
    WireModel010.CheckSettings(style,now);}
   catch(System.Exception ex){throw new InvalidOperationException(Retained(ex.Message,SegmentCount),ex);}}
  static string ColorText(Color c){return (int)c.ColorMethod+":"+(c.ColorMethod==ColorMethod.ByColor?c.Red+","+c.Green+","+c.Blue:c.ColorIndex.ToString(CultureInfo.InvariantCulture));}
  // Properties and MEPS records the route must keep between its own edits.
  static string Signature(Transaction tr,Polyline pl){var s=new StringBuilder();
   s.Append(pl.LayerId.Handle).Append('|').Append(ColorText(pl.Color)).Append('|').Append(pl.LinetypeId.Handle).Append('|').Append((int)pl.LineWeight).Append('|')
    .Append(pl.LinetypeScale.ToString("R",CultureInfo.InvariantCulture)).Append('|').Append(pl.Plinegen);
#if IDENTITY015
   var idr=IdentityCad015.Read(tr,pl);s.Append("|id=").Append(idr==null?"":string.Join("\u001f",Identity015.Encode(idr)));
#endif
#if CABLES016
   var cab=CableCad016.Read(tr,pl);s.Append("|cable=").Append(cab==null?"":string.Join("\u001f",Cable016.Encode(cab)));
#endif
   return s.ToString();}
  string Conflict(string why){return "Tuyến MEPS "+route.Handle+" "+why+" ngoài lệnh; MEPS không ghi đè, bản vẽ giữ nguyên trạng thái hiện tại. Dừng vẽ tiếp.";}
  // The owned polyline, open for read, after proving it is unchanged since the last commit of this route.
  Polyline Owned(Transaction tr){if(route.IsNull||signature==null)Fail("Không xác nhận được tuyến vừa lưu; MEPS không sửa tiếp. Giữ "+SegmentCount+" đoạn đã lưu.");
   if(!route.IsValid||route.IsErased||route.Database!=db)Fail(Conflict("đã bị xoá"));
   var pl=tr.GetObject(route,OpenMode.ForRead,false) as Polyline;if(pl==null)Fail(Conflict("không còn là LWPOLYLINE"));
   if(pl.OwnerId!=SymbolUtilityServices.GetBlockModelSpaceId(db))Fail(Conflict("không còn trong Model"));
   if(pl.Closed||pl.NumberOfVertices!=points.Count)Fail(Conflict("đã đổi số đỉnh/đóng kín"));
   if(Math.Abs(pl.Elevation)>Tol||!pl.Normal.IsEqualTo(Vector3d.ZAxis))Fail(Conflict("đã đổi cao độ/mặt phẳng"));
   for(int i=0;i<points.Count;i++){var v=pl.GetPoint2dAt(i);if(Math.Abs(v.X-points[i].X)>Tol||Math.Abs(v.Y-points[i].Y)>Tol)Fail(Conflict("đã dời đỉnh "+(i+1)));
    if(Math.Abs(pl.GetBulgeAt(i))>Tol||Math.Abs(pl.GetStartWidthAt(i)-width)>Tol||Math.Abs(pl.GetEndWidthAt(i)-width)>Tol)Fail(Conflict("đã đổi cung/bề rộng"));}
   if(Signature(tr,pl)!=signature)Fail(Conflict("đã đổi layer/màu/kiểu nét/độ đậm/tỷ lệ hoặc mã nhận dạng/cáp"));
   var layer=(LayerTableRecord)tr.GetObject(pl.LayerId,OpenMode.ForRead);
   if(layer.IsLocked||layer.IsOff||layer.IsFrozen)Fail("Layer "+layer.Name+" đang "+(layer.IsLocked?"khóa":layer.IsFrozen?"đóng băng":"tắt")+"; MEPS không tự mở. Không vẽ thêm; giữ "+SegmentCount+" đoạn đã lưu.");
   return pl;}
  // Accepts p as the next vertex. Invalid point → ArgumentException (nothing written, route unchanged); other failures → InvalidOperationException with
  // the drawing and the route as before the call.
  public void Add(Point3d p,WireSettings010 now){if(finished)Fail("Tuyến đã kết thúc; bấm lại nét để vẽ tuyến mới.");string w=PointIssue(p);if(w!=null)throw new ArgumentException(w);
   if(points.Count==0){points.Add(p);return;}
   Check(now);
   if(points.Count==1){var pair=new[]{points[0],p};ObjectId id;
#if STANDARDS012
    if(preset!=null)id=StandardsCad012.DrawPreset(db,preset,pair,now);else
#endif
    id=WireCad010.Draw(db,style,pair,now);
    route=id;points.Add(p);signature=null;
    // Committed; a failed read-back only blocks further edits (Owned refuses), it never removes the saved segment.
    try{using(var tr=db.TransactionManager.StartTransaction()){var pl=tr.GetObject(id,OpenMode.ForRead) as Polyline;if(pl!=null)signature=Signature(tr,pl);tr.Commit();}}catch(System.Exception){signature=null;}
    return;}
   using(var tr=db.TransactionManager.StartTransaction()){var pl=Owned(tr);pl.UpgradeOpen();pl.AddVertexAt(pl.NumberOfVertices,new Point2d(p.X,p.Y),0,width,width);tr.Commit();}
   points.Add(p);}
  // U during the point prompt. Returns the status text; failures as in Add.
  public string Undo(WireSettings010 now){if(finished)Fail("Tuyến đã kết thúc; bấm lại nét để vẽ tuyến mới.");
   if(points.Count==0)return "Chưa có điểm nào để bỏ.";
   if(points.Count==1){points.Clear();return "Đã bỏ điểm đầu; chọn lại điểm đầu.";}
   Check(now);bool last=points.Count==2;
   using(var tr=db.TransactionManager.StartTransaction()){var pl=Owned(tr);pl.UpgradeOpen();if(last)pl.Erase();else pl.RemoveVertexAt(points.Count-1);tr.Commit();}
   points.RemoveAt(points.Count-1);if(last){route=ObjectId.Null;signature=null;return "Đã bỏ đoạn duy nhất (xoá tuyến vừa vẽ); giữ điểm đầu, chọn điểm tiếp theo.";}
   return "Đã bỏ đoạn vừa vẽ; còn "+SegmentCount+" đoạn đã lưu.";}
  // Esc/Enter: the saved route stays as it is; an anchor without a segment writes nothing. Returns the saved segment count.
  public int Finish(){finished=true;return SegmentCount;}
 }
#endif
}
