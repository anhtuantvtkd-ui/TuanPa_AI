using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Script.Serialization;
namespace Tvxd.Meps {
 public sealed class Room054 { public string id=Guid.NewGuid().ToString("N"); public string name; public decimal? routeHeight; public decimal? ceilingHeight; public Dictionary<string,decimal> heights=new Dictionary<string,decimal>(); }
 public sealed class Floor054 { public string id=Guid.NewGuid().ToString("N"); public string name; public decimal rise; public decimal? routeHeight; public decimal? ceilingHeight; public List<Room054> rooms=new List<Room054>(); }
 public sealed partial class Floors054 {
  public int version=4; public decimal datum; public List<Floor054> floors=new List<Floor054>();
  public static Floors054 New(){var d=new Floors054();d.floors.Add(new Floor054{name="TẦNG 1"});d.SeedDevices();return d;}
  public void Validate(){
   if(version!=4||floors==null||floors.Count<1||floors.Count>200)throw new InvalidOperationException("Dữ liệu INPUT không hợp lệ (1–200 tầng, phiên bản 4).");
   var ids=new HashSet<string>();var names=new HashSet<string>(StringComparer.OrdinalIgnoreCase);var roomNames=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
   foreach(var f in floors){Guid g;if(f==null||!Guid.TryParseExact(f.id,"N",out g)||!ids.Add(f.id))throw new InvalidOperationException("Mã tầng không hợp lệ hoặc trùng.");
    if(string.IsNullOrWhiteSpace(f.name)||f.name.Length>60||f.name.Any(char.IsControl)||!names.Add(f.name.Trim().Normalize()))throw new InvalidOperationException("Tên tầng phải khác nhau, dài 1–60 ký tự.");
    if(f.rooms==null||f.rooms.Count>2000)throw new InvalidOperationException("Danh sách phòng không hợp lệ.");
    foreach(var r in f.rooms){if(r==null||!Guid.TryParseExact(r.id,"N",out g)||!ids.Add(r.id))throw new InvalidOperationException("Mã phòng không hợp lệ hoặc trùng.");if(string.IsNullOrWhiteSpace(r.name)||r.name.Length>60||r.name.Any(char.IsControl)||!roomNames.Add(r.name.Trim().Normalize()))throw new InvalidOperationException("Mã/tên phòng phải khác nhau trong bản vẽ, dài 1–60 ký tự.");}
    if(f==floors[0]?f.rise!=0:f.rise<=0)throw new InvalidOperationException("Khoảng cao giữa hai sàn phải lớn hơn 0.");
    if(f.routeHeight<0||f.rooms.Any(r=>r.routeHeight<0))throw new InvalidOperationException("CAO ĐỘ TUYẾN NGANG PHẢI KHÔNG ÂM.");
   } ValidateCeilings(); ValidateHeights(ids); Elevations();
  }
  public decimal[] Elevations(){var a=new decimal[floors.Count];decimal z=datum;for(int i=0;i<a.Length;i++){z=checked(z+floors[i].rise);a[i]=z;}return a;}
  public string Encode(){Validate();return new JavaScriptSerializer().Serialize(this);}
  public static Floors054 Decode(string text){if(text==null||text.Length>1000000)throw new InvalidOperationException("Dữ liệu INPUT quá lớn hoặc rỗng.");var d=new JavaScriptSerializer().Deserialize<Floors054>(text);if(d==null)throw new InvalidOperationException("Dữ liệu INPUT rỗng.");if(d.version==1){d.version=2;d.SeedDevices();foreach(var f in d.floors)foreach(var r in f.rooms)if(r.heights==null)r.heights=new Dictionary<string,decimal>();}if(d.version==2)d.version=3;if(d.version==3)d.version=4;d.Validate();return d;}
  public Floors054 Clone(){return Decode(Encode());}
  public static decimal Number(string s){decimal n;if(string.IsNullOrWhiteSpace(s)||!decimal.TryParse(s.Trim().Replace(',','.'),NumberStyles.AllowLeadingSign|NumberStyles.AllowDecimalPoint,CultureInfo.InvariantCulture,out n))throw new InvalidOperationException("Nhập số M, ví dụ 3,6 hoặc 3.6.");return n;}
  public void Insert(int above,string name,decimal rise){if(above<0||above>=floors.Count)throw new InvalidOperationException("Chọn tầng để chèn phía trên.");var c=Clone();c.floors.Insert(above+1,new Floor054{name=name.Trim(),rise=rise});c.Validate();floors=c.floors;}
  public void Delete(int index,Func<string,bool> linked){if(index<0||index>=floors.Count||floors.Count==1)throw new InvalidOperationException("Cần giữ ít nhất một tầng.");if(floors[index].rooms.Count>0||(linked!=null&&linked(floors[index].id)))throw new InvalidOperationException("Tầng đang có phòng; cần xử lý các phòng trước khi xóa tầng.");var c=Clone();if(index==0){c.datum=checked(c.datum+c.floors[1].rise);c.floors[1].rise=0;}c.floors.RemoveAt(index);c.Validate();datum=c.datum;floors=c.floors;}
  public void AddRoom(int floor,string name){var c=Clone();c.floors[floor].rooms.Add(new Room054{name=name.Trim()});c.Validate();floors=c.floors;}
 }
}
