using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using IOE=System.InvalidOperationException;

namespace Tvxd.Meps {
 // TVXD-MEPS-ROOMQTY-037 S2: PURE project model = hierarchy (RoomHierarchy037) + explicit device-height declarations at three levels.
 // System + collections + Linq only; no CAD, Forms, JSON or IO; C# 5. Vietnamese messages: compile with /codepage:65001 (as RoomStore037).
 // NOT connected to any UI, store or export yet (RoomProjectStore037 persists it natively).
 //  - Rules are explicit declarations only: project default by type (CHUNG) < room override by type (PHONG) < one actual device (RIENG).
 //    Effective heights are resolved live by Resolve; no effective number is stored or copied into lower levels. No engineering value is ever
 //    defaulted: an absent rule = MISSING, never 0. Value 0 is a legitimate declaration.
 //  - Types: SWITCH/SOCKET/LAMP/FAN/PANEL exactly (authoritative type comes from the caller's classification; nothing is guessed from names,
 //    TAG or geometry; OTHER/unknown has no rule and resolves MISSING).
 //  - Modes: FIXED (height = value) or MOUNTING_OFFSET (height = declared DEVICE-MOUNTING ceiling - value). The mounting ceiling is
 //    HierRoom037.mountingCeilingOverride, else HierFloor037.mountingCeilingHeight; storeyHeight / wireCeilingHeight / wireCeilingOverride are
 //    never used in its place. No mounting reference = MISSING; a negative result = INVALID (never clamped).
 //  - Room CODE: required for every room of a project model, unique across the whole project (one DWG) on RoomHierarchy037.NameKey.
 //    Room name = free description (may repeat on different floors, and among coded rooms of one floor, per the hierarchy S4 rule). IDs never change on rename.
 //  - Every mutating call validates the whole model and the whole request first, then mutates; failure = InvalidOperationException and
 //    nothing changed (ArgumentNullException for a null id factory, from RoomHierarchy037).
 public sealed class RoomHeightRule037 {public string deviceType,mode;public double value;}
 public sealed class RoomProjectRoom037 {public string roomId;public List<RoomHeightRule037> rules=new List<RoomHeightRule037>();}
 public sealed class RoomProjectDevice037 {public string roomId,deviceId;public RoomHeightRule037 rule;}
 public sealed class RoomProjectData037 {
  public int version=1;public HierProject037 hierarchy;
  public List<RoomHeightRule037> defaults=new List<RoomHeightRule037>();
  public List<RoomProjectRoom037> rooms=new List<RoomProjectRoom037>();
  public List<RoomProjectDevice037> devices=new List<RoomProjectDevice037>();
 }
 // status VALID/MISSING/INVALID; source CHUNG/PHONG/RIENG/NONE; mode = mode of the winning rule (null when none). height only when VALID.
 public sealed class RoomEffectiveHeight037 {public string status,source,mode,message;public double? height;}

 public static class RoomProject037 {
  public const int Version=1,MaxDevices=20000;
  public const string Switch="SWITCH",Socket="SOCKET",Lamp="LAMP",Fan="FAN",Panel="PANEL";
  public const string Fixed="FIXED",MountingOffset="MOUNTING_OFFSET";
  public const string Valid="VALID",Missing="MISSING",Invalid="INVALID";
  public const string Chung="CHUNG",Phong="PHONG",Rieng="RIENG",None="NONE";
  public static readonly ReadOnlyCollection<string> Types=new ReadOnlyCollection<string>(new[]{Switch,Socket,Lamp,Fan,Panel});
  static readonly StringComparer O=StringComparer.Ordinal,OI=StringComparer.OrdinalIgnoreCase;

  static bool Blank(string s){return string.IsNullOrWhiteSpace(s);}
  static string Show(string s){return s==null?"(null)":"\""+s+"\"";}
  static bool IsType(string t){return t!=null&&Types.Contains(t);}
  static bool Finite(double v){return !double.IsNaN(v)&&!double.IsInfinity(v);}

  // null = acceptable rule; otherwise the reason.
  public static string RuleProblem(RoomHeightRule037 r){
   if(r==null)return "Khai báo cao độ rỗng.";
   if(!IsType(r.deviceType))return "Loại thiết bị "+Show(r.deviceType)+" không thuộc "+string.Join("/",Types)+"; MEPS không đoán loại.";
   if(r.mode!=Fixed&&r.mode!=MountingOffset)return "Cách khai "+Show(r.mode)+" không phải "+Fixed+" hoặc "+MountingOffset+".";
   if(!Finite(r.value)||r.value<0)return "Giá trị cao độ của "+r.deviceType+" phải là số hữu hạn >= 0.";
   return null;}
  static string RulesProblem(List<RoomHeightRule037> rules,string where){
   if(rules==null)return "Danh sách khai báo "+where+" rỗng (null).";
   var seen=new HashSet<string>(O);
   foreach(var r in rules){var t=RuleProblem(r);if(t!=null)return t+" ("+where+")";
    if(!seen.Add(r.deviceType))return "Loại "+r.deviceType+" được khai hai lần ("+where+").";}
   return null;}
  static string DeviceIdProblem(string deviceId){
   var t=RoomHierarchy037.TextProblem(deviceId,"Device id");
   return t==null?null:"Mã thiết bị "+Show(deviceId)+" không hợp lệ: "+t;}
  static string CodeProblem(string code){
   var t=RoomHierarchy037.TextProblem(code,"Room code");
   return t==null?null:"Mã phòng "+Show(code)+" không hợp lệ: "+t;}

  // Room and its floor, or null.
  static HierRoom037 FindRoom(HierProject037 h,string roomId,out HierFloor037 floor){
   floor=null;if(h==null||Blank(roomId))return null;
   foreach(var f in h.floors){var r=f.rooms.FirstOrDefault(x=>O.Equals(x.id,roomId));if(r!=null){floor=f;return r;}}
   return null;}
  static IEnumerable<HierRoom037> AllRooms(HierProject037 h){return h.floors.SelectMany(f=>f.rooms);}

  // Whole-model validation. null = valid. Corrupt lists/records are reported, never hidden or repaired.
  public static string Problem(RoomProjectData037 d){
   if(d==null)return "Dữ liệu dự án rỗng.";
   if(d.version!=Version)return "Dữ liệu dự án phiên bản "+d.version+" khác phiên bản "+Version+" của MEPS.";
   var hp=RoomHierarchy037.Problem(d.hierarchy);if(hp!=null)return "Cây dự án không hợp lệ: "+hp;
   foreach(var f in d.hierarchy.floors)foreach(var r in f.rooms)
    if(r.code==null)return "Phòng "+Show(r.name)+" ("+r.id+") trên tầng "+Show(f.name)+" chưa có mã phòng; dự án mới bắt buộc mã phòng.";
   var t=RulesProblem(d.defaults,"mặc định chung");if(t!=null)return t;
   if(d.rooms==null)return "Danh sách khai báo theo phòng rỗng (null).";
   if(d.devices==null)return "Danh sách khai báo riêng thiết bị rỗng (null).";
   var roomSeen=new HashSet<string>(O);HierFloor037 fl;
   foreach(var e in d.rooms){
    if(e==null)return "Có mục khai báo phòng rỗng (null).";
    var r=FindRoom(d.hierarchy,e.roomId,out fl);
    if(r==null)return "Khai báo phòng trỏ tới phòng "+Show(e.roomId)+" không có trong dự án.";
    if(!roomSeen.Add(e.roomId))return "Phòng "+r.code+" có hai mục khai báo phòng.";
    t=RulesProblem(e.rules,"phòng "+r.code);if(t!=null)return t;}
   if(d.devices.Count>MaxDevices)return "Quá "+MaxDevices+" thiết bị khai riêng.";
   var devSeen=new Dictionary<string,string>(OI);
   foreach(var v in d.devices){
    if(v==null)return "Có mục khai báo thiết bị rỗng (null).";
    var r=FindRoom(d.hierarchy,v.roomId,out fl);
    if(r==null)return "Thiết bị "+Show(v.deviceId)+" trỏ tới phòng "+Show(v.roomId)+" không có trong dự án.";
    t=DeviceIdProblem(v.deviceId);if(t!=null)return t;
    string other;
    if(devSeen.TryGetValue(v.deviceId,out other))
     return O.Equals(other,v.roomId)?"Thiết bị "+v.deviceId+" có hai khai báo riêng trong phòng "+r.code+"."
                                     :"Thiết bị "+v.deviceId+" được gán vào hai phòng khác nhau; MEPS không tự chọn phòng.";
    devSeen[v.deviceId]=v.roomId;
    t=RuleProblem(v.rule);if(t!=null)return t+" (thiết bị "+v.deviceId+")";}
   return null;}
  static void Check(RoomProjectData037 d){var e=Problem(d);if(e!=null)throw new IOE("Dữ liệu dự án không hợp lệ: "+e+" Không thay đổi gì.");}
  static void Fail(string m){throw new IOE(m+" Không thay đổi gì.");}
  static HierRoom037 Room(RoomProjectData037 d,string roomId,out HierFloor037 floor){
   if(Blank(roomId))Fail("Chưa chọn phòng; MEPS không tự chọn phòng.");
   var r=FindRoom(d.hierarchy,roomId,out floor);
   if(r==null)Fail("Phòng "+Show(roomId)+" không có trong dự án.");
   return r;}
  static void CodeFree(RoomProjectData037 d,string code,HierRoom037 self){
   var t=CodeProblem(code);if(t!=null)Fail(t);var k=RoomHierarchy037.NameKey(code);
   var o=AllRooms(d.hierarchy).FirstOrDefault(x=>x!=self&&x.code!=null&&RoomHierarchy037.NameKey(x.code)==k);
   if(o!=null)Fail("Mã phòng "+Show(code)+" đã dùng cho phòng "+Show(o.name)+" ("+o.id+"); mã phòng phải duy nhất trong bản vẽ.");}
  static RoomHeightRule037 NewRule(string type,string mode,double value){
   var r=new RoomHeightRule037{deviceType=type,mode=mode,value=value};var t=RuleProblem(r);if(t!=null)Fail(t);return r;}
  static void Put(List<RoomHeightRule037> rules,RoomHeightRule037 r){
   int i=rules.FindIndex(x=>O.Equals(x.deviceType,r.deviceType));if(i>=0)rules[i]=r;else rules.Add(r);}

  // ---- Hierarchy wrappers (validate first, then mutate) ----
  public static RoomProjectData037 Create(string projectName,string drawingId,Func<Guid> ids){
   var d=new RoomProjectData037{hierarchy=RoomHierarchy037.Create(projectName,drawingId,ids)};
   Check(d);return d;}
  public static HierFloor037 AddFloor(RoomProjectData037 d,string name,Func<Guid> ids){
   Check(d);return RoomHierarchy037.AddFloor(d.hierarchy,name,ids);}
  public static HierRoom037 AddRoom(RoomProjectData037 d,string floorId,string code,string name,Func<Guid> ids){
   Check(d);CodeFree(d,code,null);
   // S4 coded room: the description may repeat among coded rooms of the floor. Throws before mutating on any bad floor/name/code/id.
   return RoomHierarchy037.AddCodedRoom(d.hierarchy,floorId,code,name,ids);}
  // Changes code and name of one room; its id, rules and device declarations stay attached.
  public static HierRoom037 RenameRoom(RoomProjectData037 d,string roomId,string code,string name){
   Check(d);HierFloor037 f;var r=Room(d,roomId,out f);CodeFree(d,code,r);
   RoomHierarchy037.RenameRoom(d.hierarchy,f.id,r.id,name);       // throws before mutating on a bad/duplicate name
   r.code=code;return r;}

  // ---- Explicit declarations ----
  public static RoomHeightRule037 SetDefault(RoomProjectData037 d,string type,string mode,double value){
   Check(d);var r=NewRule(type,mode,value);Put(d.defaults,r);return r;}
  public static RoomHeightRule037 SetRoomRule(RoomProjectData037 d,string roomId,string type,string mode,double value){
   Check(d);HierFloor037 f;Room(d,roomId,out f);var r=NewRule(type,mode,value);
   var e=d.rooms.FirstOrDefault(x=>O.Equals(x.roomId,roomId));
   if(e==null){e=new RoomProjectRoom037{roomId=roomId};d.rooms.Add(e);}
   Put(e.rules,r);return r;}
  // One actual placed device (native handle). A device already declared in another room is refused (no reassignment).
  public static RoomHeightRule037 SetDeviceRule(RoomProjectData037 d,string roomId,string deviceId,string type,string mode,double value){
   Check(d);HierFloor037 f;var room=Room(d,roomId,out f);
   var t=DeviceIdProblem(deviceId);if(t!=null)Fail(t);
   var r=NewRule(type,mode,value);
   var e=d.devices.FirstOrDefault(x=>OI.Equals(x.deviceId,deviceId));
   if(e!=null&&!O.Equals(e.roomId,roomId))Fail("Thiết bị "+deviceId+" đã có khai báo riêng ở phòng khác ("+e.roomId+"), không phải phòng "+room.code+".");
   if(d.devices.Count>=MaxDevices&&e==null)Fail("Đã có "+MaxDevices+" thiết bị khai riêng.");
   if(e==null)d.devices.Add(new RoomProjectDevice037{roomId=roomId,deviceId=deviceId,rule=r});else e.rule=r;
   return r;}
  // DÙNG LẠI MẶC ĐỊNH for one room/type. true = an override was removed; false = there was none. Other overrides untouched.
  public static bool ResetRoomRule(RoomProjectData037 d,string roomId,string type){
   Check(d);HierFloor037 f;Room(d,roomId,out f);
   if(!IsType(type))Fail("Loại thiết bị "+Show(type)+" không thuộc "+string.Join("/",Types)+".");
   var e=d.rooms.FirstOrDefault(x=>O.Equals(x.roomId,roomId));if(e==null)return false;
   int i=e.rules.FindIndex(x=>O.Equals(x.deviceType,type));if(i<0)return false;
   e.rules.RemoveAt(i);if(e.rules.Count==0)d.rooms.Remove(e);return true;}
  // DÙNG LẠI MẶC ĐỊNH for one actual device. Refused when that device is declared in another room.
  public static bool ResetDeviceRule(RoomProjectData037 d,string roomId,string deviceId){
   Check(d);HierFloor037 f;var room=Room(d,roomId,out f);
   var t=DeviceIdProblem(deviceId);if(t!=null)Fail(t);
   var e=d.devices.FirstOrDefault(x=>OI.Equals(x.deviceId,deviceId));if(e==null)return false;
   if(!O.Equals(e.roomId,roomId))Fail("Thiết bị "+deviceId+" có khai báo riêng ở phòng khác ("+e.roomId+"), không phải phòng "+room.code+".");
   d.devices.Remove(e);return true;}

  // ---- Live resolution (never mutates) ----
  static RoomEffectiveHeight037 Out(string status,string source,string mode,string message,double? h){
   return new RoomEffectiveHeight037{status=status,source=source,mode=mode,message=message,height=h};}
  // deviceId may be null/blank for a room/type-level value (room table). deviceType = authoritative actual type from the caller.
  public static RoomEffectiveHeight037 Resolve(RoomProjectData037 d,string roomId,string deviceId,string deviceType){
   var p=Problem(d);if(p!=null)return Out(Invalid,None,null,"Dữ liệu dự án không hợp lệ: "+p,null);
   if(Blank(roomId))return Out(Missing,None,null,"Chưa chọn phòng.",null);
   HierFloor037 f;var room=FindRoom(d.hierarchy,roomId,out f);
   if(room==null)return Out(Invalid,None,null,"Phòng "+Show(roomId)+" không có trong dự án.",null);
   if(Blank(deviceType))return Out(Missing,None,null,"Thiết bị chưa có loại; không có cao độ.",null);
   RoomHeightRule037 rule=null;string src=None;
   if(!Blank(deviceId)){var e=d.devices.FirstOrDefault(x=>OI.Equals(x.deviceId,deviceId));
    if(e!=null){
     if(!O.Equals(e.roomId,roomId))return Out(Invalid,Rieng,e.rule.mode,"Thiết bị "+deviceId+" có khai báo riêng ở phòng khác ("+e.roomId+"); cần xem lại, MEPS không tự chọn.",null);
     if(!O.Equals(e.rule.deviceType,deviceType))return Out(Invalid,Rieng,e.rule.mode,"Khai báo riêng của thiết bị "+deviceId+" là loại "+e.rule.deviceType+
      " nhưng loại thực tế là "+Show(deviceType)+"; MEPS không tự gán lại.",null);
     rule=e.rule;src=Rieng;}}
   if(rule==null){
    if(!IsType(deviceType))return Out(Missing,None,null,"Loại "+Show(deviceType)+" chưa hỗ trợ khai cao độ; MEPS không đoán.",null);
    var re=d.rooms.FirstOrDefault(x=>O.Equals(x.roomId,roomId));
    var rr=re==null?null:re.rules.FirstOrDefault(x=>O.Equals(x.deviceType,deviceType));
    if(rr!=null){rule=rr;src=Phong;}
    else{var dr=d.defaults.FirstOrDefault(x=>O.Equals(x.deviceType,deviceType));if(dr!=null){rule=dr;src=Chung;}}
    if(rule==null)return Out(Missing,None,null,"Chưa khai cao độ cho loại "+deviceType+" (mặc định/phòng/riêng).",null);}
   if(rule.mode==Fixed)return Out(Valid,src,rule.mode,"Cao độ cố định "+N(rule.value)+".",rule.value);
   double? ceil=room.mountingCeilingOverride;string cs="phòng";
   if(!ceil.HasValue){ceil=f.mountingCeilingHeight;cs="tầng";}
   if(!ceil.HasValue)return Out(Missing,src,rule.mode,"Chưa khai cao độ trần lắp đặt của phòng "+room.code+" hoặc tầng "+f.name+"; không lấy trần đi dây/chiều cao tầng thay thế.",null);
   var h=ceil.Value-rule.value;
   if(!Finite(h))return Out(Invalid,src,rule.mode,"Cao độ tính ra không hữu hạn.",null);
   if(h<0)return Out(Invalid,src,rule.mode,"Trần lắp đặt ("+cs+") "+N(ceil.Value)+" - hạ "+N(rule.value)+" = "+N(h)+" < 0; không tự cắt về 0.",null);
   return Out(Valid,src,rule.mode,"Trần lắp đặt ("+cs+") "+N(ceil.Value)+" - hạ "+N(rule.value)+" = "+N(h)+".",h);}
  static string N(double v){return v.ToString("R",System.Globalization.CultureInfo.InvariantCulture);}

  // ---- Deep copy (no aliasing; null lists/entries preserved so Problem still reports them) ----
  static RoomHeightRule037 Copy(RoomHeightRule037 r){return r==null?null:new RoomHeightRule037{deviceType=r.deviceType,mode=r.mode,value=r.value};}
  static List<RoomHeightRule037> Copy(List<RoomHeightRule037> l){return l==null?null:l.Select(x=>Copy(x)).ToList();}
  static HierRoom037 Copy(HierRoom037 r){return r==null?null:new HierRoom037{id=r.id,name=r.name,code=r.code,wireCeilingOverride=r.wireCeilingOverride,
   mountingCeilingOverride=r.mountingCeilingOverride,legacyKey=r.legacyKey,legacyHash=r.legacyHash};}
  static HierFloor037 Copy(HierFloor037 f){return f==null?null:new HierFloor037{id=f.id,name=f.name,order=f.order,storeyHeight=f.storeyHeight,
   wireCeilingHeight=f.wireCeilingHeight,mountingCeilingHeight=f.mountingCeilingHeight,rooms=f.rooms==null?null:f.rooms.Select(x=>Copy(x)).ToList()};}
  public static HierProject037 CopyHierarchy(HierProject037 p){return p==null?null:new HierProject037{id=p.id,name=p.name,drawingId=p.drawingId,
   floors=p.floors==null?null:p.floors.Select(x=>Copy(x)).ToList()};}
  public static RoomProjectData037 Copy(RoomProjectData037 d){
   if(d==null)return null;
   return new RoomProjectData037{version=d.version,hierarchy=CopyHierarchy(d.hierarchy),defaults=Copy(d.defaults),
    rooms=d.rooms==null?null:d.rooms.Select(e=>e==null?null:new RoomProjectRoom037{roomId=e.roomId,rules=Copy(e.rules)}).ToList(),
    devices=d.devices==null?null:d.devices.Select(v=>v==null?null:new RoomProjectDevice037{roomId=v.roomId,deviceId=v.deviceId,rule=Copy(v.rule)}).ToList()};}
 }
}
