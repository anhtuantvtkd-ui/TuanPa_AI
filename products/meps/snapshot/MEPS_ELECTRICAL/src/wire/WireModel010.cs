using System;using System.IO;using System.Linq;using System.Threading;using System.Collections.Generic;using System.Web.Script.Serialization;
namespace Tvxd.Meps {
 // A captured drafting style, not an electrical circuit specification. All lengths are drawing units.
 // msltscale/annoScale/annoName (TVXD-MEPS-LEGEND-011, F2): the source drawing's MSLTSCALE (0/1) and current annotation scale factor (drawing units per paper
 // unit, CANNOSCALE) at capture. -1 / 0 = not recorded (styles saved by the 010 prototype): such a style is never drawn (no invented default).
 public sealed class WireStyle010 {
  public string id,name,layer,linetype,sourceHandle,sourceDrawing,annoName; public string catalogPresetLayer;
  public int units,aci=7,red,green,blue,lineweight,layerAci=7,layerRed,layerGreen,layerBlue,layerWeight,msltscale=-1;
  public bool rgb,layerRgb;
  public double scale=1,globalScale=1,width,patternLength,annoScale;
  public double[] dashes=new double[0];
  public bool plinegen;
 }
 // Model-space linetype display settings of a drawing, read by the command adapter (WirePlugin010) and passed explicitly to WireCad010.
 public sealed class WireSettings010 {public int msltscale=-1;public double annoScale;public string annoName;}
 public sealed class WireLibrary010 {public int version=1;public List<WireStyle010> styles=new List<WireStyle010>();}
 public static class WireModel010 {
  static readonly int[] Weights={0,5,9,13,15,18,20,25,30,35,40,50,53,60,70,80,90,100,106,120,140,158,200,211};
  public const int NameMax=160;
  public static bool Finite(double x){return !double.IsNaN(x)&&!double.IsInfinity(x);}
  // F10: display name only; the layer and linetype fields keep the full source names.
  public static string DisplayName(string raw,int max){raw=(raw??"").Trim();if(max<2)throw new ArgumentException("max");return raw.Length<=max?raw:raw.Substring(0,max-1)+"…";}
  public static void Validate(WireStyle010 s){if(s==null)throw new InvalidDataException("Thiếu mẫu nét.");
   Guid g;if(!Guid.TryParse(s.id,out g)||string.IsNullOrWhiteSpace(s.name)||s.name.Length>NameMax||string.IsNullOrWhiteSpace(s.layer)||string.IsNullOrWhiteSpace(s.linetype))throw new InvalidDataException("Mẫu nét thiếu ID/tên/layer/kiểu nét.");
   if(s.units<0||s.units>24||!Weights.Contains(s.lineweight)||!Weights.Contains(s.layerWeight))throw new InvalidDataException("Đơn vị/độ đậm mẫu không hợp lệ.");
   Color(s.rgb,s.aci,s.red,s.green,s.blue);Color(s.layerRgb,s.layerAci,s.layerRed,s.layerGreen,s.layerBlue);
   if(!Finite(s.scale)||s.scale<=0||!Finite(s.globalScale)||s.globalScale<=0||!Finite(s.scale*s.globalScale)||!Finite(s.width)||s.width<0||!Finite(s.patternLength)||s.patternLength<0)throw new InvalidDataException("Tỷ lệ/bề rộng nét không hợp lệ.");
   if(s.dashes==null||s.dashes.Length>256||s.dashes.Any(x=>!Finite(x)))throw new InvalidDataException("Mẫu gạch nét không hợp lệ.");
   double total=s.dashes.Sum(x=>Math.Abs(x));if((s.dashes.Length>0&&total<=0)||Math.Abs(total-s.patternLength)>1e-8*Math.Max(1,total))throw new InvalidDataException("Chiều dài chu kỳ nét không khớp.");
   if(s.msltscale<-1||s.msltscale>1||!Finite(s.annoScale)||s.annoScale<0||(s.msltscale>=0&&!(s.annoScale>0)))throw new InvalidDataException("MSLTSCALE/tỷ lệ chú thích của mẫu không hợp lệ.");
  }
  static void Color(bool rgb,int aci,int r,int g,int b){if(rgb?(r<0||r>255||g<0||g>255||b<0||b>255):(aci<1||aci>255))throw new InvalidDataException("Màu mẫu chưa được xác định.");}
  public static void Validate(WireSettings010 t){if(t==null||t.msltscale<0||t.msltscale>1||!Finite(t.annoScale)||!(t.annoScale>0))throw new InvalidDataException("Chưa đọc được MSLTSCALE/tỷ lệ chú thích của bản vẽ; chưa vẽ.");}
  // F2: with MSLTSCALE=1 model-space dashes are multiplied by the annotation scale factor, so the sample's appearance is reproduced only with the same
  // MSLTSCALE and (when it is 1) the same factor. Anything else is refused before drawing.
  public static void CheckSettings(WireStyle010 s,WireSettings010 target){Validate(s);Validate(target);
   if(s.msltscale<0)throw new InvalidDataException("Mẫu "+s.name+" lấy từ bản cũ, chưa ghi MSLTSCALE/tỷ lệ chú thích; bấm + Lấy mẫu lại. Chưa vẽ.");
   if(s.msltscale!=target.msltscale)throw new InvalidDataException("MSLTSCALE bản vẽ = "+target.msltscale+" khác mẫu ("+s.msltscale+"); nét sẽ hiển thị khác. Không tự đổi; chưa vẽ.");
   if(s.msltscale==1&&Math.Abs(s.annoScale-target.annoScale)>1e-9*Math.Max(1,s.annoScale))throw new InvalidDataException("Tỷ lệ chú thích bản vẽ "+(target.annoName??"")+" ("+target.annoScale+") khác mẫu "+(s.annoName??"")+" ("+s.annoScale+") khi MSLTSCALE=1; nét sẽ hiển thị khác. Chưa vẽ.");}
  public static bool SameSettings(WireSettings010 a,WireSettings010 b){return a!=null&&b!=null&&a.msltscale==b.msltscale&&Math.Abs(a.annoScale-b.annoScale)<=1e-12*Math.Max(1,Math.Abs(a.annoScale));}
  static void Validate(WireLibrary010 l){if(l==null||l.version!=1||l.styles==null||l.styles.Count>1000)throw new InvalidDataException("Thư viện nét không hợp lệ.");foreach(var s in l.styles)Validate(s);if(l.styles.Select(x=>x.id).Distinct(StringComparer.OrdinalIgnoreCase).Count()!=l.styles.Count)throw new InvalidDataException("Trùng ID mẫu nét.");}
  // F8: malformed JSON is reported as InvalidDataException (file left untouched); Recover is the explicit recovery action.
  public static WireLibrary010 Load(string path){if(!File.Exists(path))return new WireLibrary010();string text=File.ReadAllText(path);WireLibrary010 l;
   try{l=new JavaScriptSerializer().Deserialize<WireLibrary010>(text);}catch(Exception ex){if(!(ex is ArgumentException||ex is InvalidOperationException||ex is FormatException||ex is OverflowException))throw;throw new InvalidDataException("Tệp thư viện nét hỏng ("+Path.GetFileName(path)+"): "+ex.Message);}
   Validate(l);return l;}
  // F9: one writer at a time across AutoCAD processes (exclusive .lock), bounded retry, then a clear message. Nothing is written without the lock.
  public const int LockTries=5,LockWaitMs=200;
  static FileStream Guard(string path){for(int i=1;;i++){try{return new FileStream(path+".lock",FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.None);}
    catch(IOException){if(i>=LockTries)throw new IOException("Thư viện nét đang được một AutoCAD khác ghi; thử lại sau vài giây. Mẫu chưa được lưu, thư viện giữ nguyên.");Thread.Sleep(LockWaitMs);}}}
  public static void Add(string path,WireStyle010 s){Validate(s);Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));
   // Hold this across read-modify-replace so two AutoCAD processes cannot lose each other's samples.
   using(var guard=Guard(path)){var l=Load(path);int n=1;string name=s.name;while(l.styles.Any(x=>string.Equals(x.name,s.name,StringComparison.OrdinalIgnoreCase))){string tail=" ("+(++n)+")";s.name=DisplayName(name,NameMax-tail.Length)+tail;}l.styles.Add(s);Save(path,l);}}
  public static void Save(string path,WireLibrary010 l){Validate(l);Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));string temp=path+"."+Guid.NewGuid().ToString("N")+".tmp";
   try{File.WriteAllText(temp,new JavaScriptSerializer().Serialize(l),new System.Text.UTF8Encoding(false));Load(temp);if(File.Exists(path))File.Replace(temp,path,path+".bak");else File.Move(temp,path);}finally{if(File.Exists(temp))File.Delete(temp);}}
  // F8 explicit recovery (user action, under the writer lock): the unreadable styles.json is first COPIED to a new styles.json.corrupt_<time>_<id> (never
  // overwritten), then replaced by the last valid backup styles.json.bak if that loads, otherwise by an empty library. The backup itself is not changed.
  // Returns the preserved copy and what was restored. Refuses when styles.json is valid (nothing to recover).
  public static string Recover(string path,out string preserved){preserved=null;if(!File.Exists(path))throw new InvalidOperationException("Chưa có tệp thư viện nét; không cần khôi phục.");
   using(var guard=Guard(path)){bool ok=true;try{Load(path);}catch(InvalidDataException){ok=false;}if(ok)throw new InvalidOperationException("Thư viện nét đang đọc được; không cần khôi phục.");
    string copy;do{copy=path+".corrupt_"+DateTime.Now.ToString("yyyyMMdd_HHmmss")+"_"+Guid.NewGuid().ToString("N").Substring(0,6);}while(File.Exists(copy));File.Copy(path,copy,false);preserved=copy;
    WireLibrary010 restored=null;string from;string bak=path+".bak";if(File.Exists(bak)){try{restored=Load(bak);}catch(InvalidDataException){restored=null;}}
    from=restored!=null?"bản sao lưu "+Path.GetFileName(bak)+" ("+restored.styles.Count+" mẫu)":"thư viện trống (không có bản sao lưu hợp lệ)";if(restored==null)restored=new WireLibrary010();
    string temp=path+"."+Guid.NewGuid().ToString("N")+".tmp";
    try{File.WriteAllText(temp,new JavaScriptSerializer().Serialize(restored),new System.Text.UTF8Encoding(false));Load(temp);File.Replace(temp,path,null);}finally{if(File.Exists(temp))File.Delete(temp);}
    return "Đã giữ bản hỏng ở "+Path.GetFileName(copy)+"; khôi phục từ "+from+".";}}
  public static double EntityScale(WireStyle010 s,double targetGlobal){Validate(s);if(!Finite(targetGlobal)||targetGlobal<=0)throw new InvalidDataException("LTSCALE đích không hợp lệ.");double x=s.scale*s.globalScale/targetGlobal;if(!Finite(x)||x<=0)throw new InvalidDataException("Tỷ lệ nét vượt giới hạn.");return x;}
 }
}
