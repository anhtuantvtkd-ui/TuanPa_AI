using System;using System.Linq;using System.Text;using System.Collections.Generic;using System.Globalization;using System.Security.Cryptography;using System.Web.Script.Serialization;using IOE=System.InvalidOperationException;
#if ROOMCAD037
using Autodesk.AutoCAD.DatabaseServices;
namespace Tvxd.Meps {
 // TVXD-MEPS-ROOMQTY-037 room draft storage (Vietnamese messages: /codepage:65001). One owned dictionary NOD["TVXD_MEPS_ROOMQTY037"] = { SCHEMA: [schema, "ROOM_STORE", "1"],
 // R_<32 hex of SHA-256(roomId)>: Xrecord [schema, "ROOM_DRAFT", "1", json chunks <= 240 chars] }. JSON = JavaScriptSerializer of RoomDraft037; the roomId
 // inside must hash to its key. Only editable declarations, options, rectangle and provenance are stored (never computed quantities).
 //  - Read never repairs: a foreign NOD key, wrong schema/kind/version, non-text data, bad JSON or a key/room mismatch = clear error, nothing erased.
 //  - Save = ONE transaction inside the caller's bound command (its UNDO group; U removes only this draft change): the current record must equal the raw
 //    text the caller loaded (expectedRaw; null = no record) else refused; a malformed/foreign existing record is never overwritten; written values are read
 //    back and decoded before guard + Commit. No geometry, block, layer or other dictionary is touched; no DWG save; no file path.
 public sealed class RoomDraftRow037 {public string rowKey,floorId,circuitId,routingMode,assemblyCode,conduitCode;public double? ceilingHeight,tailEachLength,factor;public int? tailCount;}
 // Height declaration of ONE actual device (handle); shared by every row using that device.
 public sealed class RoomDraftDevice037 {public string deviceId,heightMode;public double? fixedHeight,ceilingOffset;}
 public sealed class RoomDraft037 {
  public string schema,kind;public int version;
  public string roomId,units,fingerprint,signature,confirmedSignature,savedAt;public int insunits;
  public double minX,minY,maxX,maxY,tolerance;public bool unitConfirmed,independentAssembliesConfirmed;public string conduitMode;
  public List<string> sourceHandles=new List<string>(),deviceHandles=new List<string>();
  public List<RoomDraftRow037> rows=new List<RoomDraftRow037>();
  public List<RoomDraftDevice037> devices=new List<RoomDraftDevice037>();
  // NF-037-HEIGHT-UI: optional room height setup of THIS room (null = never used). Same schema version 1: drafts saved before this field existed simply
  // deserialize with null and stay readable; a present value is validated like every other field.
  public RoomHeightSettings037 heights;
  // S5: optional per-room adjustments (BỎ DÒNG exclusions + BỔ SUNG manual supplements; declarations only, never computed totals). Same schema version 1:
  // drafts saved before this field existed deserialize with null (no adjustment) and stay readable; a present value must pass RoomAdjustments037.Problem.
  public RoomAdjustmentsData037 adjustments;
 }
 // One stored record: draft when readable, else error (raw kept so a save can prove nothing changed underneath).
 public sealed class RoomStoreEntry037 {public string key,roomId,raw,error;public RoomDraft037 draft;}
 public sealed class RoomStoreSave037 {public bool changed;public string key,raw;}
 public static class RoomStore037 {
  public const string Key="TVXD_MEPS_ROOMQTY037",Schema="TVXD_MEPS_ROOMQTY037",StoreKind="ROOM_STORE",DraftKind="ROOM_DRAFT",SchemaKey="SCHEMA",VersionText="1";
  public const int Version=1,ChunkChars=240,MaxChars=400000,MaxRooms=200,MaxRows=5000,MaxDevices=5000,MaxText=1000;
  static readonly CultureInfo Inv=CultureInfo.InvariantCulture;
  public static string RecordKey(string roomId){if(string.IsNullOrWhiteSpace(roomId))throw new IOE("Tên phòng trống.");
   using(var h=SHA256.Create()){var b=h.ComputeHash(new UTF8Encoding(false,true).GetBytes(roomId));return "R_"+string.Concat(b.Take(16).Select(x=>x.ToString("X2",Inv)));}}
  static JavaScriptSerializer Json(){return new JavaScriptSerializer{MaxJsonLength=MaxChars*2,RecursionLimit=16};}
  static bool Fin(double? v){return !v.HasValue||(!double.IsNaN(v.Value)&&!double.IsInfinity(v.Value));}
  static bool Fin(double v){return !double.IsNaN(v)&&!double.IsInfinity(v);}
  static void Text(string s,string what,bool required){if(s==null){if(required)throw new IOE(what+" trống.");return;}if(s.Length>MaxText)throw new IOE(what+" dài quá "+MaxText+" ký tự.");}
  // Structural validation only (engineering values stay as typed; the model reports missing/invalid values).
  public static void Validate(RoomDraft037 d){if(d==null)throw new IOE("Bản nháp rỗng.");
   if(d.schema!=Schema||d.kind!=DraftKind)throw new IOE("Bản nháp không phải loại "+Schema+"/"+DraftKind+".");
   if(d.version!=Version)throw new IOE("Bản nháp phiên bản "+d.version.ToString(Inv)+" khác phiên bản "+Version.ToString(Inv)+" của MEPS.");
   Text(d.roomId,"Tên phòng",true);if(string.IsNullOrWhiteSpace(d.roomId))throw new IOE("Tên phòng trống.");
   if(d.units!=RoomQty037.MM&&d.units!=RoomQty037.M)throw new IOE("Đơn vị lưu \""+d.units+"\" không phải MM hoặc M.");
   if(!Fin(d.minX)||!Fin(d.minY)||!Fin(d.maxX)||!Fin(d.maxY)||!(d.maxX>d.minX)||!(d.maxY>d.minY))throw new IOE("Vùng chữ nhật đã lưu không hợp lệ.");
   if(!Fin(d.tolerance)||d.tolerance<0)throw new IOE("Dung sai nối đã lưu không hợp lệ.");
   if(d.conduitMode!=null&&d.conduitMode!=RoomCompletion037.Separate&&d.conduitMode!=RoomCompletion037.Shared)throw new IOE("Cách đi ống đã lưu \""+d.conduitMode+"\" không hợp lệ.");
   foreach(var s in new[]{d.fingerprint,d.signature,d.confirmedSignature,d.savedAt})Text(s,"Dấu vết bản vẽ",false);
   if(d.sourceHandles==null||d.deviceHandles==null||d.rows==null||d.devices==null)throw new IOE("Bản nháp thiếu danh sách bắt buộc.");
   if(d.rows.Count>MaxRows||d.devices.Count>MaxDevices||d.sourceHandles.Count>MaxRows*2||d.deviceHandles.Count>MaxDevices)throw new IOE("Bản nháp vượt giới hạn số dòng/thiết bị.");
   var keys=new HashSet<string>(StringComparer.Ordinal);
   foreach(var r in d.rows){if(r==null||string.IsNullOrEmpty(r.rowKey))throw new IOE("Bản nháp có dòng không có khoá.");if(!keys.Add(r.rowKey))throw new IOE("Bản nháp trùng khoá dòng "+r.rowKey+".");
    foreach(var s in new[]{r.rowKey,r.floorId,r.circuitId,r.routingMode,r.assemblyCode,r.conduitCode})Text(s,"Khai báo dòng",false);
    if(!Fin(r.ceilingHeight)||!Fin(r.tailEachLength)||!Fin(r.factor))throw new IOE("Dòng "+r.rowKey+" có số không hữu hạn.");}
   var ids=new HashSet<string>(StringComparer.Ordinal);
   foreach(var v in d.devices){if(v==null||string.IsNullOrEmpty(v.deviceId))throw new IOE("Bản nháp có thiết bị không có mã.");if(!ids.Add(v.deviceId))throw new IOE("Bản nháp trùng thiết bị "+v.deviceId+".");
    Text(v.deviceId,"Mã thiết bị",true);Text(v.heightMode,"Cách khai cao độ",false);if(!Fin(v.fixedHeight)||!Fin(v.ceilingOffset))throw new IOE("Thiết bị "+v.deviceId+" có số không hữu hạn.");}
   if(d.heights!=null){var hp=RoomHeight037.Problem(d.heights);if(hp!=null)throw new IOE("Thiết lập cao độ đã lưu không hợp lệ: "+hp);}
   if(d.adjustments!=null){var ap=RoomAdjustments037.Problem(d.adjustments);if(ap!=null)throw new IOE("Dữ liệu bỏ dòng / bổ sung đã lưu không hợp lệ: "+ap);}}
  public static string Encode(RoomDraft037 d){Validate(d);var j=Json().Serialize(d);if(j.Length>MaxChars)throw new IOE("Bản nháp quá lớn ("+j.Length.ToString(Inv)+" ký tự, tối đa "+MaxChars.ToString(Inv)+").");return j;}
  public static RoomDraft037 Decode(string json){if(json==null||json.Length==0||json.Length>MaxChars)throw new IOE("Nội dung bản nháp rỗng hoặc quá lớn.");RoomDraft037 d;
   try{d=Json().Deserialize<RoomDraft037>(json);}catch(Exception ex){throw new IOE("JSON bản nháp không đọc được ("+ex.Message+").");}
   Validate(d);return d;}
  // Header + chunks of at most ChunkChars UTF-16 units; a surrogate pair is never split.
  public static string[] Pack(string json){var l=new List<string>{Schema,DraftKind,VersionText};
   for(int i=0;i<json.Length;){int n=Math.Min(ChunkChars,json.Length-i);if(n>1&&i+n<json.Length&&char.IsHighSurrogate(json[i+n-1]))n--;l.Add(json.Substring(i,n));i+=n;}
   return l.ToArray();}
  public static string Unpack(string[] v){if(v==null||v.Length<4||v[0]!=Schema||v[1]!=DraftKind)throw new IOE("Dữ liệu không phải bản nháp phòng MEPS.");
   if(v[2]!=VersionText)throw new IOE("Bản nháp phiên bản "+v[2]+" khác "+VersionText+"; MEPS không đọc/ghi đè.");
   if(v.Skip(3).Any(s=>s==null||s.Length>ChunkChars))throw new IOE("Đoạn dữ liệu bản nháp sai kích thước.");return string.Concat(v.Skip(3));}
  static string[] Values(Xrecord x){if(x==null||x.Data==null)return null;var a=x.Data.AsArray();var v=new string[a.Length];
   for(int i=0;i<a.Length;i++){if(a[i].TypeCode!=(short)DxfCode.Text)return null;v[i]=a[i].Value as string;if(v[i]==null)return null;}return v;}
  static string Raw(string[] v){return v==null?null:string.Join("\u001f",v);}
  static DBDictionary Registry(Transaction tr,Database db){var nod=(DBDictionary)tr.GetObject(db.NamedObjectsDictionaryId,OpenMode.ForRead);if(!nod.Contains(Key))return null;
   var reg=tr.GetObject(nod.GetAt(Key),OpenMode.ForRead) as DBDictionary;if(reg==null)throw new IOE("Khoá "+Key+" của bản vẽ không phải từ điển MEPS; MEPS không đọc/ghi đè.");
   var s=reg.Contains(SchemaKey)?tr.GetObject(reg.GetAt(SchemaKey),OpenMode.ForRead) as Xrecord:null;var v=Values(s);
   if(v==null||v.Length!=3||v[0]!=Schema||v[1]!=StoreKind||v[2]!=VersionText)throw new IOE("Từ điển "+Key+" thiếu/khác SCHEMA "+Schema+" "+StoreKind+" "+VersionText+"; MEPS không đọc/ghi đè.");
   return reg;}
  static RoomStoreEntry037 ReadEntry(Transaction tr,string key,ObjectId id){var e=new RoomStoreEntry037{key=key};
   try{var x=tr.GetObject(id,OpenMode.ForRead) as Xrecord;if(x==null)throw new IOE("mục "+key+" không phải Xrecord");var v=Values(x);if(v==null)throw new IOE("mục "+key+" không phải dữ liệu văn bản");
    e.raw=Raw(v);var d=Decode(Unpack(v));if(RecordKey(d.roomId)!=key)throw new IOE("khoá "+key+" không khớp tên phòng \""+d.roomId+"\"");e.draft=d;e.roomId=d.roomId;}
   catch(IOE ex){e.error=ex.Message;}catch(Autodesk.AutoCAD.Runtime.Exception ex){e.error=ex.Message;}
   return e;}
  // Every stored room (readable or not); empty = none. A foreign/unreadable registry = exception (nothing listed, nothing written).
  public static List<RoomStoreEntry037> ReadAll(Database db){if(db==null)throw new ArgumentNullException("db");var list=new List<RoomStoreEntry037>();
   using(var tr=db.TransactionManager.StartOpenCloseTransaction()){var reg=Registry(tr,db);
    if(reg!=null)foreach(DBDictionaryEntry de in reg){if(de.Key==SchemaKey)continue;list.Add(ReadEntry(tr,de.Key,de.Value));if(list.Count>=MaxRooms)break;}
    tr.Commit();}
   return list.OrderBy(e=>e.roomId??"￿"+e.key,StringComparer.Ordinal).ToList();}
  // The record of this room or null.
  public static RoomStoreEntry037 Find(Database db,string roomId){if(db==null)throw new ArgumentNullException("db");var key=RecordKey(roomId);RoomStoreEntry037 e=null;
   using(var tr=db.TransactionManager.StartOpenCloseTransaction()){var reg=Registry(tr,db);if(reg!=null&&reg.Contains(key))e=ReadEntry(tr,key,reg.GetAt(key));tr.Commit();}
   return e;}
  static ResultBuffer Buffer(string[] v){return new ResultBuffer(v.Select(s=>new TypedValue((int)DxfCode.Text,s)).ToArray());}
  public static RoomStoreSave037 Save(Database db,RoomDraft037 d,string expectedRaw,Action<string> guard){if(db==null)throw new ArgumentNullException("db");
   var vals=Pack(Encode(d));var raw=Raw(vals);var key=RecordKey(d.roomId);var res=new RoomStoreSave037{key=key,raw=raw};
   using(var tr=db.TransactionManager.StartTransaction()){
    var reg=Registry(tr,db);string had=null;
    if(reg!=null&&reg.Contains(key)){var x=tr.GetObject(reg.GetAt(key),OpenMode.ForRead) as Xrecord;
     if(x==null)throw new IOE("Mục "+key+" của phòng không phải Xrecord; MEPS không ghi đè. Bản vẽ không đổi.");var v=Values(x);had=Raw(v);
     try{var old=Decode(Unpack(v));if(old.roomId!=d.roomId)throw new IOE("khoá không khớp tên phòng");}
     catch(IOE ex){throw new IOE("Bản nháp đang có của phòng không đọc được ("+ex.Message+"); MEPS không ghi đè. Bản vẽ không đổi.");}}
    if(!string.Equals(had,expectedRaw,StringComparison.Ordinal))throw new IOE("Bản nháp phòng \""+d.roomId+"\" trong bản vẽ đã đổi sau khi mở (lệnh khác/Undo); đóng và mở lại bảng. Bản vẽ không đổi.");
    if(had!=null&&had==raw)return res;
    Xrecord rec;
    if(reg==null){var nod=(DBDictionary)tr.GetObject(db.NamedObjectsDictionaryId,OpenMode.ForWrite);reg=new DBDictionary();nod.SetAt(Key,reg);tr.AddNewlyCreatedDBObject(reg,true);
     var s=new Xrecord();using(var b=Buffer(new[]{Schema,StoreKind,VersionText}))s.Data=b;reg.SetAt(SchemaKey,s);tr.AddNewlyCreatedDBObject(s,true);}
    if(reg.Contains(key)){rec=(Xrecord)tr.GetObject(reg.GetAt(key),OpenMode.ForWrite);using(var b=Buffer(vals))rec.Data=b;}
    else{int n=0;foreach(DBDictionaryEntry de in reg)if(de.Key!=SchemaKey)n++;
     if(n>=MaxRooms)throw new IOE("Bản vẽ đã có "+n.ToString(Inv)+" phòng lưu (tối đa "+MaxRooms.ToString(Inv)+"); bản vẽ không đổi.");
     if(!reg.IsWriteEnabled)reg.UpgradeOpen();rec=new Xrecord();using(var b=Buffer(vals))rec.Data=b;reg.SetAt(key,rec);tr.AddNewlyCreatedDBObject(rec,true);}
    var back=Values(rec);if(Raw(back)!=raw)throw new IOE("Đọc lại bản nháp không khớp; bản vẽ không đổi.");Decode(Unpack(back));
    if(guard!=null)guard("ngay trước khi lưu bản nháp phòng");
    tr.Commit();}
   res.changed=true;return res;}
 }
}
#endif
