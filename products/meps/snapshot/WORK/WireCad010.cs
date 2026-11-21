using System;using System.Linq;using System.Collections.Generic;using Autodesk.AutoCAD.DatabaseServices;using Autodesk.AutoCAD.Geometry;using Autodesk.AutoCAD.Colors;
namespace Tvxd.Meps {
 // No UI dependencies: the same operations run in AutoCAD and CoreConsole QA. No source object is opened for write.
 // TVXD-MEPS-LEGEND-011: model linetype settings (MSLTSCALE, annotation scale) are explicit WireSettings010 snapshots read by the command adapter; symbol
 // table lookups skip erased records (an UNDO of an earlier draw erases the layer/linetype it created, F1).
 public static class WireCad010 {
  static void Fail(string s){throw new InvalidOperationException(s);}
  // F11: ByBlock has no source to inherit from in a model-space sample.
  static int Weight(LineWeight w,int defaultWeight){if(w==LineWeight.ByBlock)Fail("Độ đậm nét ByBlock chưa có nguồn kế thừa trong Model; chọn nét có độ đậm cụ thể hoặc ByLayer.");return w==LineWeight.ByLineWeightDefault?defaultWeight:(int)w;}
  static void ColorTo(Color c,WireStyle010 s,bool layer){bool rgb=c.ColorMethod==ColorMethod.ByColor;if(!rgb&&c.ColorMethod!=ColorMethod.ByAci)Fail("Không xác định được màu mẫu (ByBlock/thuộc tính đặc biệt).");
   int rr=c.Red,gg=c.Green,bb=c.Blue;if(!rgb){int packed=EntityColor.LookUpRgb((byte)c.ColorIndex);rr=(packed>>16)&255;gg=(packed>>8)&255;bb=packed&255;}
   if(layer){s.layerRgb=rgb;s.layerAci=c.ColorIndex;s.layerRed=rr;s.layerGreen=gg;s.layerBlue=bb;}else{s.rgb=rgb;s.aci=c.ColorIndex;s.red=rr;s.green=gg;s.blue=bb;}}
  static Color ColorFrom(WireStyle010 s,bool layer){return layer?(s.layerRgb?Color.FromRgb((byte)s.layerRed,(byte)s.layerGreen,(byte)s.layerBlue):Color.FromColorIndex(ColorMethod.ByAci,(short)s.layerAci)):(s.rgb?Color.FromRgb((byte)s.red,(byte)s.green,(byte)s.blue):Color.FromColorIndex(ColorMethod.ByAci,(short)s.aci));}
  static bool Simple(LinetypeTableRecord lt){if(lt.IsScaledToFit)return false;for(int i=0;i<lt.NumDashes;i++)if(!lt.ShapeStyleAt(i).IsNull||lt.ShapeNumberAt(i)!=0)return false;return true;}
  // F1: the non-erased record of that name, or Null (SymbolTable.Has/indexer may still see a record erased by UNDO).
  public static ObjectId Live(Transaction tr,SymbolTable t,string name){foreach(ObjectId id in t){if(id.IsErased)continue;var r=(SymbolTableRecord)tr.GetObject(id,OpenMode.ForRead);if(string.Equals(r.Name,name,StringComparison.OrdinalIgnoreCase))return id;}return ObjectId.Null;}
  public static WireStyle010 Capture(Database db,ObjectId id,string source,int defaultWeight,WireSettings010 settings){WireModel010.Validate(settings);using(var tr=db.TransactionManager.StartTransaction()){
   var e=tr.GetObject(id,OpenMode.ForRead) as Entity;if(e==null||e.OwnerId!=SymbolUtilityServices.GetBlockModelSpaceId(db))Fail("Chỉ lấy nét trực tiếp trong Model, không lấy nét trong block/xref.");
   var line=e as Line;var poly=e as Polyline;if(line==null&&poly==null)Fail("Chọn LINE hoặc LWPOLYLINE làm mẫu.");
   if(line!=null&&(Math.Abs(line.StartPoint.Z)>1e-8||Math.Abs(line.EndPoint.Z)>1e-8||Math.Abs(line.Thickness)>1e-8))Fail("Bản đầu hỗ trợ nét 2D trên mặt phẳng XY, cao độ 0.");
   double width=0;bool gen=false;
   if(poly!=null){if(!poly.Normal.IsEqualTo(Vector3d.ZAxis)||Math.Abs(poly.Elevation)>1e-8||Math.Abs(poly.Thickness)>1e-8)Fail("Polyline phải nằm trên XY, cao độ 0.");
    if(poly.NumberOfVertices<2)Fail("Polyline mẫu thiếu đỉnh.");width=poly.GetStartWidthAt(0);gen=poly.Plinegen;
    for(int i=0;i<poly.NumberOfVertices;i++)if(Math.Abs(poly.GetStartWidthAt(i)-width)>1e-8||Math.Abs(poly.GetEndWidthAt(i)-width)>1e-8)Fail("Polyline có bề rộng thay đổi; chọn mẫu bề rộng đồng nhất.");}
   var layer=(LayerTableRecord)tr.GetObject(e.LayerId,OpenMode.ForRead);if(layer.IsDependent)Fail("Không lấy layer phụ thuộc xref.");
   var s=new WireStyle010{id=Guid.NewGuid().ToString("N"),layer=layer.Name,units=(int)db.Insunits,scale=e.LinetypeScale,globalScale=db.Ltscale,width=width,plinegen=gen,sourceHandle=e.Handle.ToString(),sourceDrawing=System.IO.Path.GetFileName(source??""),msltscale=settings.msltscale,annoScale=settings.annoScale,annoName=settings.annoName};
   ColorTo(layer.Color,s,true);ColorTo(e.Color.ColorMethod==ColorMethod.ByLayer?layer.Color:e.Color,s,false);
   s.layerWeight=Weight(layer.LineWeight,defaultWeight);s.lineweight=Weight(e.LineWeight==LineWeight.ByLayer?layer.LineWeight:e.LineWeight,defaultWeight);
   if(e.LinetypeId==db.ByBlockLinetype)Fail("Nét ByBlock chưa có nguồn kế thừa; chọn nét độc lập.");var lt=(LinetypeTableRecord)tr.GetObject(e.LinetypeId==db.ByLayerLinetype?layer.LinetypeObjectId:e.LinetypeId,OpenMode.ForRead);
   if(!Simple(lt))Fail("Nét có chữ/shape hoặc chế độ Fit chưa được hỗ trợ; không thay bằng nét liền.");s.linetype=lt.Name;s.patternLength=lt.PatternLength;s.dashes=Enumerable.Range(0,lt.NumDashes).Select(i=>lt.DashLengthAt(i)).ToArray();s.name=WireModel010.DisplayName(layer.Name+" / "+lt.Name,WireModel010.NameMax);
   WireModel010.Validate(s);tr.Commit();return s;}}
  static bool Match(LinetypeTableRecord lt,WireStyle010 s){if(!Simple(lt)||lt.NumDashes!=s.dashes.Length||Math.Abs(lt.PatternLength-s.patternLength)>1e-8)return false;for(int i=0;i<lt.NumDashes;i++)if(Math.Abs(lt.DashLengthAt(i)-s.dashes[i])>1e-8)return false;return true;}
  static ObjectId EnsureType(Database db,Transaction tr,WireStyle010 s){var table=(LinetypeTable)tr.GetObject(db.LinetypeTableId,OpenMode.ForRead);string name=s.linetype;
   if(string.Equals(name,"ByLayer",StringComparison.OrdinalIgnoreCase)||string.Equals(name,"ByBlock",StringComparison.OrdinalIgnoreCase))Fail("Mẫu kiểu nét chưa được xác định.");
   var found=Live(tr,table,name);if(!found.IsNull){if(Match((LinetypeTableRecord)tr.GetObject(found,OpenMode.ForRead),s))return found;name="TVXD_W_"+s.id;}
   found=Live(tr,table,name);if(!found.IsNull){if(Match((LinetypeTableRecord)tr.GetObject(found,OpenMode.ForRead),s))return found;Fail("Mẫu nét đã thay đổi: trùng tên riêng. Lấy mẫu mới.");}
   table.UpgradeOpen();var lt=new LinetypeTableRecord{Name=name,AsciiDescription="MEPS captured sample",NumDashes=s.dashes.Length,PatternLength=s.patternLength};for(int i=0;i<s.dashes.Length;i++)lt.SetDashLengthAt(i,s.dashes[i]);var id=table.Add(lt);tr.AddNewlyCreatedDBObject(lt,true);return id;}
  // settings: the target drawing's MSLTSCALE / annotation scale, read by the caller right before drawing (WireModel010.CheckSettings refuses a mismatch).
  public static ObjectId Draw(Database db,WireStyle010 s,IList<Point3d> points,WireSettings010 settings){WireModel010.Validate(s);WireModel010.CheckSettings(s,settings);if((int)db.Insunits!=s.units)Fail("Đơn vị bản vẽ khác mẫu; không tự đổi đơn vị.");if(points==null||points.Count<2)Fail("Cần ít nhất hai điểm.");
   for(int i=0;i<points.Count;i++){var p=points[i];if(!WireModel010.Finite(p.X)||!WireModel010.Finite(p.Y)||!WireModel010.Finite(p.Z)||Math.Abs(p.Z)>1e-8)Fail("Chỉ hỗ trợ điểm trên XY, cao độ 0.");if(i>0&&p.DistanceTo(points[i-1])<1e-8)Fail("Hai điểm liên tiếp trùng nhau.");}
   double scale=WireModel010.EntityScale(s,db.Ltscale);
   using(var tr=db.TransactionManager.StartTransaction()){var layers=(LayerTable)tr.GetObject(db.LayerTableId,OpenMode.ForRead);ObjectId layerId=Live(tr,layers,s.layer);
    if(!layerId.IsNull){var l=(LayerTableRecord)tr.GetObject(layerId,OpenMode.ForRead);if(l.IsLocked||l.IsOff||l.IsFrozen||l.IsDependent)Fail("Layer đích đang khóa/tắt/đóng băng/phụ thuộc: "+s.layer);}
    var lt=EnsureType(db,tr,s);if(layerId.IsNull){layers.UpgradeOpen();var l=new LayerTableRecord{Name=s.layer,Color=ColorFrom(s,true),LineWeight=(LineWeight)s.layerWeight,LinetypeObjectId=lt};layerId=layers.Add(l);tr.AddNewlyCreatedDBObject(l,true);}
    var ms=(BlockTableRecord)tr.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db),OpenMode.ForWrite);using(var p=new Polyline()){
     p.SetDatabaseDefaults(db);p.LayerId=layerId;p.Color=ColorFrom(s,false);p.LineWeight=(LineWeight)s.lineweight;p.LinetypeId=lt;p.LinetypeScale=scale;p.Plinegen=s.plinegen;
     for(int i=0;i<points.Count;i++)p.AddVertexAt(i,new Point2d(points[i].X,points[i].Y),0,s.width,s.width);
     var id=ms.AppendEntity(p);tr.AddNewlyCreatedDBObject(p,true);
#if IDENTITY015
     // TVXD-MEPS-IDENTITY-015: sample code TVXD_SAMPLE_<id>, UNCLASSIFIED (drawing style only, no cable/conduit specification); same transaction.
     if(string.IsNullOrEmpty(s.catalogPresetLayer))IdentityCad015.MarkSample(tr,p,s);else { if(Cable016.Find(s.catalogPresetLayer)==null)throw new InvalidOperationException("MÃ CÁP GỐC KHÔNG HỢP LỆ.");IdentityCad015.MarkPreset(tr,p,new PresetSnapshot012{layer=s.catalogPresetLayer});CableCad016.MarkPreset(tr,p,s.catalogPresetLayer); }
#endif
     tr.Commit();return id;}}}
 }
}
