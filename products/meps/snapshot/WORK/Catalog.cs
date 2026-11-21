using System;using System.Drawing;using System.Drawing.Drawing2D;using System.Linq;
namespace Tvxd.Meps {
 // One source for placeholder symbol geometry (block units = mm). Plugin.Symbol builds CAD blocks and the panel draws previews from the same shapes.
 public sealed class Shape {public string type,text;public double x1,y1,x2,y2,r,h;}
 public sealed class CatalogItem {public string Kind,Label;}
 public static class Symbols {
  public const double Extent=150;
  public static readonly string[] Kinds={"LIGHT","SOCKET","SWITCH","POWER"};
  // Sample library shown as cards; schematic placeholders only, not approved company symbols.
  public static readonly CatalogItem[] Catalog={new CatalogItem{Kind="LIGHT",Label="Đèn / LIGHT"},new CatalogItem{Kind="SOCKET",Label="Ổ cắm / SOCKET"}};
  public static Shape[] Of(string kind){if(!Kinds.Contains(kind))throw new Exception("Loại ký hiệu không hợp lệ: "+kind);var list=new System.Collections.Generic.List<Shape>{new Shape{type="circle",r=150}};if(kind=="LIGHT"){list.Add(new Shape{type="line",x1=-106,y1=-106,x2=106,y2=106});list.Add(new Shape{type="line",x1=-106,y1=106,x2=106,y2=-106});}else list.Add(new Shape{type="text",text=kind=="SOCKET"?"O":kind=="SWITCH"?"S":"P",x1=-55,y1=-55,h=110});return list.ToArray();}
  static float Scale(int size){return (float)((size*.5-3-Math.Max(1.5,size/40.0))/Extent);}
  public static PointF ToPixel(int size,double x,double y){float s=Scale(size);return new PointF(size*.5f+(float)x*s,size*.5f-(float)y*s);}
  // Text uses a Windows font scaled to the CAD box (left x1, baseline y1, cap height h); CAD txt.shx glyph width may differ slightly.
  public static Bitmap Render(string kind,int size){if(size<24||size>1024)throw new Exception("Kích thước xem trước không hợp lệ.");var bitmap=new Bitmap(size,size);float s=Scale(size);using(var g=Graphics.FromImage(bitmap))using(var pen=new Pen(Color.FromArgb(20,20,20),(float)Math.Max(1.5,size/40.0)))using(var brush=new SolidBrush(Color.FromArgb(20,20,20))){g.SmoothingMode=SmoothingMode.AntiAlias;g.Clear(Color.White);foreach(var e in Of(kind)){var a=ToPixel(size,e.x1,e.y1);if(e.type=="circle"){float r=(float)e.r*s;g.DrawEllipse(pen,a.X-r,a.Y-r,2*r,2*r);}else if(e.type=="line"){var b=ToPixel(size,e.x2,e.y2);g.DrawLine(pen,a,b);}else using(var path=new GraphicsPath()){path.AddString(e.text,FontFamily.GenericSansSerif,(int)FontStyle.Regular,100,PointF.Empty,StringFormat.GenericTypographic);var box=path.GetBounds();float k=(float)e.h*s/box.Height;using(var m=new Matrix()){m.Translate(a.X,a.Y);m.Scale(k,k);m.Translate(-box.Left,-box.Bottom);path.Transform(m);}g.FillPath(brush,path);}}}return bitmap;}
 }
}
