using System;using System.Collections.Generic;using System.IO;using System.Linq;using System.Text;using System.Text.RegularExpressions;using System.Web.Script.Serialization;
namespace Tvxd.Meps {
 public sealed class PipeRule057 {public string wire;public string pipe;}
 public sealed class PipeRules057 {
  public int version=1;public List<PipeRule057> rows=new List<PipeRule057>();
  public static PipeRules057 Examples(){return new PipeRules057{rows=new List<PipeRule057>{new PipeRule057{wire="2x(1x1.5) mm²",pipe="D16"},new PipeRule057{wire="2x(1x2.5)+E2.5 mm²",pipe="D20"}}};}
  public static string Key(string s){return Regex.Replace((s??"").ToUpperInvariant().Replace('×','X').Replace('*','X').Replace(',','.').Replace('²','2'),@"\s+","");}
  public void Validate(){if(version!=1||rows==null||rows.Count>1000)throw new InvalidOperationException("BẢNG CHỌN ỐNG KHÔNG HỢP LỆ.");var keys=new HashSet<string>();foreach(var r in rows){if(r==null||string.IsNullOrWhiteSpace(r.wire)||r.wire.Length>160||r.wire.Any(char.IsControl))throw new InvalidOperationException("NHẬP CẤU HÌNH DÂY (TỐI ĐA 160 KÝ TỰ).");if(!keys.Add(Key(r.wire)))throw new InvalidOperationException("TRÙNG CẤU HÌNH DÂY: "+r.wire);if(string.IsNullOrWhiteSpace(r.pipe)||r.pipe.Length>40||r.pipe.Any(char.IsControl))throw new InvalidOperationException("NHẬP CỠ ỐNG, VÍ DỤ D16 HOẶC D20.");}}
  public string Encode(){Validate();return new JavaScriptSerializer().Serialize(this);}
  public static PipeRules057 Decode(string raw){if(raw==null||raw.Length>500000)throw new InvalidOperationException("FILE CHỌN ỐNG QUÁ LỚN HOẶC RỖNG.");var d=new JavaScriptSerializer().Deserialize<PipeRules057>(raw);if(d==null)throw new InvalidOperationException("FILE CHỌN ỐNG RỖNG.");d.Validate();return d;}
  public static string Read(string path){return File.Exists(path)?File.ReadAllText(path,Encoding.UTF8):null;}
  public static string Save(string path,PipeRules057 data,string expected){string raw=data.Encode();Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));using(var gate=new FileStream(path+".lock",FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.None)){if(Read(path)!=expected)throw new InvalidOperationException("BẢNG ĐÃ ĐỔI Ở CỬA SỔ KHÁC. ĐÓNG VÀ MỞ LẠI INPUT TRƯỚC KHI SỬA.");var temp=path+"."+Guid.NewGuid().ToString("N")+".tmp";try{File.WriteAllText(temp,raw,new UTF8Encoding(false));Decode(File.ReadAllText(temp));if(File.Exists(path))File.Replace(temp,path,path+".bak");else File.Move(temp,path);}finally{if(File.Exists(temp))File.Delete(temp);}}return raw;}
 }
}
