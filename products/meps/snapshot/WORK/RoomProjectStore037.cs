using System;using System.Linq;using System.Collections.Generic;using System.Globalization;using System.Web.Script.Serialization;using IOE=System.InvalidOperationException;
#if ROOMCAD037
using Autodesk.AutoCAD.DatabaseServices;
namespace Tvxd.Meps {
 // TVXD-MEPS-ROOMQTY-037 S2 project storage (Vietnamese messages: /codepage:65001). One owned dictionary NOD["TVXD_MEPS_PROJECT037"] =
 //   { SCHEMA: Xrecord [schema, "PROJECT_STORE", "1"], DOC: Xrecord [schema, "PROJECT_DOC", "1", json chunks <= 240 chars] } and nothing else.
 // JSON = JavaScriptSerializer of RoomProjectDocument037 (model = RoomProjectData037 + verbatim room drafts). Pilot = one record, payload <= MaxChars.
 //  - The legacy dictionary NOD["TVXD_MEPS_ROOMQTY037"] (RoomStore037) is never opened for write, changed or migrated here.
 //  - drawingId of the model must equal DrawingId(db) = Guid.Parse(db.FingerprintGuid).ToString("N") (case-insensitive) on Read and Save.
 //  - Read never repairs: foreign NOD key/entries, wrong schema/kind/version, non-text data, oversize, bad JSON, invalid model/draft or drawing
 //    mismatch = exception with the reason; nothing erased. Missing dictionary = document/raw null, no write.
 //  - Save = ONE transaction inside the caller's bound command (its UNDO group; U removes only this change): the persisted raw must equal
 //    expectedRaw (null = no dictionary) else refused; a malformed/foreign existing record is never overwritten; an identical payload is a
 //    no-op; guard before mutation and before Commit; written values are read back and decoded before Commit. No geometry/DWG save/file IO.
 //  - raw = header + chunks joined with U+001F, identical in Read and Save results.
 // TVXD-MEPS-FAST-ENTRY-045 schema v2: document version 2 adds "fast" (FastEntryData045: presets, explicitly chosen defaults, per-room row state).
 //  - BOTH headers change to "2" (SCHEMA [schema, PROJECT_STORE, "2"] and DOC [schema, PROJECT_DOC, "2", ...]), so a v1 writer (its Registry requires
 //    SCHEMA "1") refuses to read or overwrite a v2 store instead of silently dropping the presets.
 //  - Read accepts v1 (headers "1" + document version 1, no "fast") and migrates IN MEMORY only (version 2, fast null); nothing is written on read.
 //    The returned raw is the exact stored v1 text, so it stays the optimistic token until an explicit Save. Save always writes v2 (both headers in
 //    the same transaction, read back before Commit). Header/document version mismatch or an unknown version = refused, nothing repaired.
 public sealed class RoomProjectDraft037 {public string roomId,draftJson;}
 public sealed class RoomProjectDocument037 {public int version=RoomProjectStore037.Version;public RoomProjectData037 model;public List<RoomProjectDraft037> drafts=new List<RoomProjectDraft037>();
  public FastEntryData045 fast;}
 public sealed class RoomProjectRead037 {public RoomProjectDocument037 document;public string raw;}
 public sealed class RoomProjectSave037 {public bool changed;public string raw;}
 public static class RoomProjectStore037 {
  public const string Key="TVXD_MEPS_PROJECT037",Schema="TVXD_MEPS_PROJECT037",StoreKind="PROJECT_STORE",DocKind="PROJECT_DOC",SchemaKey="SCHEMA",DocKey="DOC",VersionText="2",
   V1Text="1";
  public const int Version=2,V1=1,ChunkChars=240,MaxChars=2000000,Recursion=32,MaxDrafts=2000;
  // Header 3 + worst case chunks of ChunkChars-1 (surrogate pair kept whole) + margin.
  public const int MaxValues=3+MaxChars/(ChunkChars-1)+2;
  static readonly CultureInfo Inv=CultureInfo.InvariantCulture;
  static JavaScriptSerializer Json(){return new JavaScriptSerializer{MaxJsonLength=MaxChars,RecursionLimit=Recursion};}

  public static string DrawingId(Database db){if(db==null)throw new ArgumentNullException("db");Guid g;
   if(!Guid.TryParse(db.FingerprintGuid,out g)||g==Guid.Empty)throw new IOE("Bản vẽ không có FingerprintGuid hợp lệ; MEPS không đọc/ghi dữ liệu dự án.");
   return g.ToString("N");}
  static void SameDrawing(RoomProjectDocument037 d,Database db){var id=DrawingId(db);
   if(!string.Equals(d.model.hierarchy.drawingId,id,StringComparison.OrdinalIgnoreCase))
    throw new IOE("Dữ liệu dự án thuộc bản vẽ "+d.model.hierarchy.drawingId+", không phải bản vẽ này ("+id+"); MEPS không đọc/ghi đè.");}

  // Whole-document validation (structure + references). Engineering values are kept exactly as declared.
  public static void Validate(RoomProjectDocument037 d){if(d==null)throw new IOE("Tài liệu dự án rỗng.");
   if(d.version!=Version)throw new IOE("Tài liệu dự án phiên bản "+d.version.ToString(Inv)+" khác phiên bản "+Version.ToString(Inv)+" của MEPS.");
   var p=RoomProject037.Problem(d.model);if(p!=null)throw new IOE("Mô hình dự án không hợp lệ: "+p);
   if(d.drafts==null)throw new IOE("Danh sách bản nháp phòng rỗng (null).");
   if(d.drafts.Count>MaxDrafts)throw new IOE("Quá "+MaxDrafts.ToString(Inv)+" bản nháp phòng.");
   var rooms=new HashSet<string>(d.model.hierarchy.floors.SelectMany(f=>f.rooms).Select(r=>r.id),StringComparer.Ordinal);
   var seen=new HashSet<string>(StringComparer.Ordinal);
   foreach(var x in d.drafts){if(x==null)throw new IOE("Có bản nháp phòng rỗng (null).");
    if(x.roomId==null||!rooms.Contains(x.roomId))throw new IOE("Bản nháp trỏ tới phòng \""+x.roomId+"\" không có trong dự án.");
    if(!seen.Add(x.roomId))throw new IOE("Phòng "+x.roomId+" có hai bản nháp.");
    RoomDraft037 dr;try{dr=RoomStore037.Decode(x.draftJson);}catch(IOE ex){throw new IOE("Bản nháp của phòng "+x.roomId+" không hợp lệ: "+ex.Message);}
    if(!string.Equals(dr.roomId,x.roomId,StringComparison.Ordinal))throw new IOE("Bản nháp của phòng "+x.roomId+" ghi roomId \""+dr.roomId+"\" khác; MEPS không tự gán lại.");}
   // 045: presets / defaults / row state (structure + references; preset values through the product cell parser when it is compiled in).
   var fp=FastEntry045.Problem(d.fast,rooms,FastValueProblem());if(fp!=null)throw new IOE("Mẫu nhập nhanh / mặc định của dự án không hợp lệ: "+fp+".");}
  static Func<string,object,string> FastValueProblem(){
#if BDGKL034
   return RoomFastEntryUi045.ValueProblem;
#else
   return null;
#endif
  }
  public static string Encode(RoomProjectDocument037 d){Validate(d);string j;
   try{j=Json().Serialize(d);}catch(InvalidOperationException ex){throw new IOE("Dữ liệu dự án quá lớn hoặc không ghi được JSON (tối đa "+MaxChars.ToString(Inv)+" ký tự; "+ex.Message+").");}
   if(j.Length>MaxChars)throw new IOE("Dữ liệu dự án quá lớn ("+j.Length.ToString(Inv)+" ký tự, tối đa "+MaxChars.ToString(Inv)+").");return j;}
  // Any supported document text (v1 or v2). v1 = migrated in memory to version 2 (fast stays null); nothing else changes.
  public static RoomProjectDocument037 Decode(string json){return Decode(json,null);}
  // header: version text of the DOC record ("1"/"2") that must match the document version; null = no header context (recovery payload).
  static RoomProjectDocument037 Decode(string json,string header){
   if(json==null||json.Length==0||json.Length>MaxChars)throw new IOE("Nội dung dữ liệu dự án rỗng hoặc quá lớn.");RoomProjectDocument037 d;
   try{d=Json().Deserialize<RoomProjectDocument037>(json);}catch(Exception ex){throw new IOE("JSON dữ liệu dự án không đọc được ("+ex.Message+").");}
   if(d==null)throw new IOE("Tài liệu dự án rỗng.");
   if(d.version!=V1&&d.version!=Version)throw new IOE("Tài liệu dự án phiên bản "+d.version.ToString(Inv)+" MEPS chưa hỗ trợ; MEPS không đọc/ghi đè.");
   if(header!=null&&header!=d.version.ToString(Inv))throw new IOE("Tiêu đề phiên bản "+header+" khác phiên bản tài liệu "+d.version.ToString(Inv)+"; MEPS không đọc/ghi đè.");
   if(d.version==V1){if(d.fast!=null)throw new IOE("Tài liệu phiên bản 1 không thể có mẫu nhập nhanh; MEPS không đọc/ghi đè.");d.version=Version;}   // memory only
   Validate(d);return d;}

  // Header + chunks of at most ChunkChars UTF-16 units; a surrogate pair is never split. Always written as the current version.
  public static string[] Pack(string json){var l=new List<string>{Schema,DocKind,VersionText};
   for(int i=0;i<json.Length;){int n=Math.Min(ChunkChars,json.Length-i);if(n>1&&i+n<json.Length&&char.IsHighSurrogate(json[i+n-1]))n--;l.Add(json.Substring(i,n));i+=n;}
   return l.ToArray();}
  public static string Unpack(string[] v){if(v==null||v.Length<4||v[0]!=Schema||v[1]!=DocKind)throw new IOE("Dữ liệu không phải tài liệu dự án MEPS.");
   if(v[2]!=VersionText&&v[2]!=V1Text)throw new IOE("Tài liệu dự án phiên bản "+v[2]+" MEPS chưa hỗ trợ (đọc "+V1Text+" / "+VersionText+"); MEPS không đọc/ghi đè.");
   if(v.Length>MaxValues)throw new IOE("Tài liệu dự án có quá nhiều đoạn dữ liệu.");
   long total=0;for(int i=3;i<v.Length;i++){if(v[i]==null||v[i].Length==0||v[i].Length>ChunkChars)throw new IOE("Đoạn dữ liệu dự án sai kích thước.");total+=v[i].Length;}
   if(total>MaxChars)throw new IOE("Tài liệu dự án quá lớn ("+total.ToString(Inv)+" ký tự).");
   return string.Concat(v.Skip(3));}
  // Text values of an Xrecord, or null if empty/non-text/too many values.
  static string[] Values(Xrecord x){if(x==null)return null;var l=new List<string>();
   using(var rb=x.Data){if(rb==null)return null;
    foreach(TypedValue tv in rb){if(l.Count>=MaxValues||tv.TypeCode!=(short)DxfCode.Text)return null;var s=tv.Value as string;if(s==null)return null;l.Add(s);}}
   return l.ToArray();}
  static string Raw(string[] v){return v==null?null:string.Join("\u001f",v);}
  static ResultBuffer Buffer(string[] v){return new ResultBuffer(v.Select(s=>new TypedValue((int)DxfCode.Text,s)).ToArray());}

  // Owned dictionary or null when absent. Foreign/malformed = exception. version = SCHEMA header version text ("1" or "2").
  static DBDictionary Registry(Transaction tr,Database db){string version;return Registry(tr,db,out version);}
  static DBDictionary Registry(Transaction tr,Database db,out string version){version=null;
   var nod=(DBDictionary)tr.GetObject(db.NamedObjectsDictionaryId,OpenMode.ForRead);if(!nod.Contains(Key))return null;
   var reg=tr.GetObject(nod.GetAt(Key),OpenMode.ForRead) as DBDictionary;if(reg==null)throw new IOE("Khoá "+Key+" của bản vẽ không phải từ điển MEPS; MEPS không đọc/ghi đè.");
   var s=reg.Contains(SchemaKey)?tr.GetObject(reg.GetAt(SchemaKey),OpenMode.ForRead) as Xrecord:null;var v=Values(s);
   if(v==null||v.Length!=3||v[0]!=Schema||v[1]!=StoreKind||(v[2]!=VersionText&&v[2]!=V1Text))
    throw new IOE("Từ điển "+Key+" thiếu/khác SCHEMA "+Schema+" "+StoreKind+" "+V1Text+"/"+VersionText+"; MEPS không đọc/ghi đè.");
   version=v[2];
   foreach(DBDictionaryEntry de in reg)if(de.Key!=SchemaKey&&de.Key!=DocKey)throw new IOE("Từ điển "+Key+" có mục lạ \""+de.Key+"\"; MEPS không đọc/ghi đè.");
   if(!reg.Contains(DocKey))throw new IOE("Từ điển "+Key+" thiếu mục "+DocKey+"; MEPS không đọc/ghi đè.");
   return reg;}
  // Raw + decoded document of the existing DOC record (registry already checked). DOC header = SCHEMA header = document version. Any defect = exception.
  static RoomProjectRead037 ReadDoc(Transaction tr,DBDictionary reg,Database db,string schemaVersion){
   var x=tr.GetObject(reg.GetAt(DocKey),OpenMode.ForRead) as Xrecord;if(x==null)throw new IOE("Mục "+DocKey+" không phải Xrecord.");
   var v=Values(x);if(v==null)throw new IOE("Mục "+DocKey+" không phải dữ liệu văn bản hoặc quá lớn.");
   var json=Unpack(v);if(v[2]!=schemaVersion)throw new IOE("Tiêu đề "+DocKey+" phiên bản "+v[2]+" khác SCHEMA "+schemaVersion+"; MEPS không đọc/ghi đè.");
   var d=Decode(json,v[2]);SameDrawing(d,db);return new RoomProjectRead037{document=d,raw=Raw(v)};}

  public static RoomProjectRead037 Read(Database db){if(db==null)throw new ArgumentNullException("db");var res=new RoomProjectRead037();
   using(var tr=db.TransactionManager.StartOpenCloseTransaction()){string ver;var reg=Registry(tr,db,out ver);
    if(reg!=null){try{res=ReadDoc(tr,reg,db,ver);}catch(IOE ex){throw new IOE("Dữ liệu dự án trong bản vẽ không đọc được: "+ex.Message+" MEPS không sửa/ghi đè.");}}
    tr.Commit();}
   return res;}

  public static RoomProjectSave037 Save(Database db,RoomProjectDocument037 document,string expectedRaw,Action<string> guard){if(db==null)throw new ArgumentNullException("db");
   var vals=Pack(Encode(document));SameDrawing(document,db);var raw=Raw(vals);var res=new RoomProjectSave037{raw=raw};
   using(var tr=db.TransactionManager.StartTransaction()){
    string ver;var reg=Registry(tr,db,out ver);string had=null;
    if(reg!=null){try{had=ReadDoc(tr,reg,db,ver).raw;}
     catch(IOE ex){throw new IOE("Dữ liệu dự án đang có không đọc được ("+ex.Message+"); MEPS không ghi đè. Bản vẽ không đổi.");}}
    if(!string.Equals(had,expectedRaw,StringComparison.Ordinal))throw new IOE("Dữ liệu dự án trong bản vẽ đã đổi sau khi mở (lệnh khác/Undo); đóng và mở lại bảng. Bản vẽ không đổi.");
    if(had!=null&&had==raw)return res;
    if(guard!=null)guard("trước khi ghi dữ liệu dự án");
    Xrecord rec;
    if(reg==null){var nod=(DBDictionary)tr.GetObject(db.NamedObjectsDictionaryId,OpenMode.ForWrite);reg=new DBDictionary();nod.SetAt(Key,reg);tr.AddNewlyCreatedDBObject(reg,true);
     var s=new Xrecord();using(var b=Buffer(new[]{Schema,StoreKind,VersionText}))s.Data=b;reg.SetAt(SchemaKey,s);tr.AddNewlyCreatedDBObject(s,true);
     rec=new Xrecord();using(var b=Buffer(vals))rec.Data=b;reg.SetAt(DocKey,rec);tr.AddNewlyCreatedDBObject(rec,true);}
    else{rec=(Xrecord)tr.GetObject(reg.GetAt(DocKey),OpenMode.ForWrite);using(var b=Buffer(vals))rec.Data=b;
     // 045: a v1 store is upgraded in THIS transaction (SCHEMA header "2" together with the v2 DOC); U removes both.
     if(ver!=VersionText){var s=(Xrecord)tr.GetObject(reg.GetAt(SchemaKey),OpenMode.ForWrite);using(var b=Buffer(new[]{Schema,StoreKind,VersionText}))s.Data=b;}}
    var back=Values(rec);if(Raw(back)!=raw)throw new IOE("Đọc lại dữ liệu dự án không khớp; bản vẽ không đổi.");
    var hdr=Values(tr.GetObject(reg.GetAt(SchemaKey),OpenMode.ForRead) as Xrecord);
    if(hdr==null||hdr.Length!=3||hdr[0]!=Schema||hdr[1]!=StoreKind||hdr[2]!=VersionText)throw new IOE("Đọc lại SCHEMA dữ liệu dự án không khớp phiên bản "+VersionText+"; bản vẽ không đổi.");
    var chk=Decode(Unpack(back),VersionText);SameDrawing(chk,db);
    if(guard!=null)guard("ngay trước khi lưu dữ liệu dự án");
    tr.Commit();}
   res.changed=true;return res;}
 }
}
#endif
