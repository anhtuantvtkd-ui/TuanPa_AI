using System;using System.Linq;using System.Collections.Generic;using System.Globalization;using System.Text.RegularExpressions;
#if CIRCUIT032 && !CIRCUIT032_PURE
using Autodesk.AutoCAD.DatabaseServices;
#endif
#if CIRCUIT032 && !CIRCUIT032_PURE && !(IDENTITY015 && MODELLIB019)
#error CIRCUIT032 needs IDENTITY015 (type identity on placed symbols) and MODELLIB019 (palette Model routes).
#endif
#if CIRCUIT032
namespace Tvxd.Meps {
 // TVXD-MEPS-METADATA-032 (circuit tags): OPTIONAL per-instance panel/circuit labels entered by the user, for a later quantity takeoff.
 //  - Type identity stays the existing, immutable Identity015 record (code/source ID) and library marker; nothing of it, the block definition, geometry, layer,
 //    scale or attributes is changed. The tag is a SEPARATE Xrecord Circuit032.Key in the BlockReference's extension dictionary:
 //    [Schema, Version, PanelId, CircuitId, Category], text only. No instance GUID/handle is stored (COPY/WBLOCK would duplicate it; handles are not device IDs).
 //  - Values are what the user typed (Trim + upper case invariant, A–Z 0–9 _ - ., 1–40) and an explicitly chosen category. Nothing is inferred from block name,
 //    layer, wires or rooms; codes are user labels, not an electrical design check. A symbol without a tag is UNASSIGNED (never invented).
 //  - Saved with the DWG like any Xrecord (readable without the plugin; not a security measure). COPY/WBLOCK carry the tag; EXPLODE/BURST destroy the
 //    reference and the tag with it.
 // Pure part (compiled alone with /define:CIRCUIT032;CIRCUIT032_PURE): values, strict decode, conflict, prefill and grouping. AutoCAD part: CircuitCad032.
 public sealed class CircuitTag032 {public string panel,circuit,category;
  public bool SameAs(CircuitTag032 o){return o!=null&&string.Equals(panel,o.panel,StringComparison.Ordinal)&&string.Equals(circuit,o.circuit,StringComparison.Ordinal)&&string.Equals(category,o.category,StringComparison.Ordinal);}
  public override string ToString(){return "tủ "+panel+" / lộ "+circuit+" / "+Circuit032.CategoryCaption(category);}}
 // One supported Model symbol. status ASSIGNED / UNASSIGNED / INVALID (issue says why; INVALID items have no usable type or tag and are never grouped).
 public sealed class CircuitItem032 {public string handle,typeCode,typeName,entryId,status,issue;public CircuitTag032 tag;}
 // Diagnostic count per structured key (panel, circuit, category, type code); UNASSIGNED rows have empty panel/circuit/category.
 public sealed class CircuitGroup032 {public string panel,circuit,category,typeCode,typeName;public int count;}
 public sealed class CircuitScan032 {public readonly List<CircuitItem032> items=new List<CircuitItem032>();
  public int Assigned{get{return items.Count(x=>x.status==Circuit032.Assigned);}}public int Unassigned{get{return items.Count(x=>x.status==Circuit032.Unassigned);}}
  public int Invalid{get{return items.Count(x=>x.status==Circuit032.Invalid);}}
  public string Summary(){return "Model có "+items.Count+" ký hiệu MEPS: "+Assigned+" đã gán lộ, "+Unassigned+" chưa gán (UNASSIGNED)"+(Invalid>0?", "+Invalid+" không gán được (thiếu/lỗi mã loại hoặc mã lộ)":"")+".";}
  // Not a quantity export: counts of placed top-level symbols only (no legend reference symbols, no wire lengths).
  public List<CircuitGroup032> Groups(){
   return items.Where(x=>x.status!=Circuit032.Invalid).GroupBy(x=>Tuple.Create(x.tag==null?"":x.tag.panel,x.tag==null?"":x.tag.circuit,x.tag==null?"":x.tag.category,x.typeCode??""))
    .Select(g=>new CircuitGroup032{panel=g.Key.Item1,circuit=g.Key.Item2,category=g.Key.Item3,typeCode=g.Key.Item4,typeName=g.First().typeName,count=g.Count()})
    .OrderBy(g=>g.panel.Length==0?1:0).ThenBy(g=>g.panel,StringComparer.Ordinal).ThenBy(g=>g.circuit,StringComparer.Ordinal).ThenBy(g=>g.category,StringComparer.Ordinal).ThenBy(g=>g.typeCode,StringComparer.Ordinal).ToList();}}
 // The validated selection the dialog shows: items (same order as ids), tagged symbols outside it (conflict check) and the drawing scan.
 public sealed class CircuitSelection032 {public readonly List<CircuitItem032> items=new List<CircuitItem032>(),others=new List<CircuitItem032>();public CircuitScan032 scan=new CircuitScan032();
#if !CIRCUIT032_PURE
  public readonly List<ObjectId> ids=new List<ObjectId>();
#endif
  // Immutable type summary, e.g. "TVXD_E_DEN_01 (E-DEN-1) × 3".
  public string Types(){return string.Join("\r\n",items.GroupBy(x=>x.typeCode).OrderBy(g=>g.Key,StringComparer.Ordinal).Select(g=>g.Key+" ("+g.First().typeName+") × "+g.Count()));}}
 public static class Circuit032 {
  public const string Key="TVXD_MEPS_CIRCUIT_032",Schema="TVXD_MEPS_CIRCUIT",Version="1";
  public const string Lighting="CHIEU_SANG",Outlet="O_CAM",Other="KHAC",Assigned="ASSIGNED",Unassigned="UNASSIGNED",Invalid="INVALID";
  public const int MaxId=40,Fields=5;
  public static readonly string[] Categories={Lighting,Outlet,Other};
  public const string Rule="Mã tủ và mã lộ: 1–40 ký tự A–Z, 0–9, gạch dưới _, gạch ngang -, dấu chấm . (MEPS tự viết hoa, bỏ khoảng trắng đầu/cuối). Ví dụ: tủ T01; lộ CS01 hoặc OC01. "+
   "Loại lộ: chọn rõ (MEPS không đoán theo tên block hay layer).";
  static readonly Regex IdRx=new Regex(@"\A[A-Z0-9_.\-]{1,40}\z");
  static string Short(string s){return s==null?"null":s.Length>24?s.Substring(0,24)+"…":s;}
  public static string CategoryCaption(string c){return c==Lighting?"Chiếu sáng":c==Outlet?"Ổ cắm":c==Other?"Khác":"(chưa chọn)";}
  public static string NormalizeId(string s,string what){string v=(s??"").Trim().ToUpperInvariant();
   if(v.Length==0)throw new InvalidOperationException("Chưa nhập "+what+". "+Rule);if(!IdRx.IsMatch(v))throw new InvalidOperationException(what+" \""+Short(v)+"\" không hợp lệ. "+Rule);return v;}
  // A complete tag from user input; any missing/invalid part = exception (nothing written).
  public static CircuitTag032 Make(string panel,string circuit,string category){var t=new CircuitTag032{panel=NormalizeId(panel,"mã tủ"),circuit=NormalizeId(circuit,"mã lộ"),category=category};
   if(category==null||!Categories.Contains(category))throw new InvalidOperationException("Chưa chọn loại lộ (Chiếu sáng / Ổ cắm / Khác); MEPS không đoán loại theo tên block hay layer.");return t;}
  // Stored values must already be normalised; nothing is repaired.
  public static void Check(CircuitTag032 t){if(t==null)throw new InvalidOperationException("Thiếu mã lộ.");var again=Make(t.panel,t.circuit,t.category);
   if(!again.SameAs(t))throw new InvalidOperationException("mã lộ chưa chuẩn hoá ("+Short(t.panel)+" / "+Short(t.circuit)+")");}
  public static string[] Encode(CircuitTag032 t){Check(t);return new[]{Schema,Version,t.panel,t.circuit,t.category};}
  static Exception Bad(string why){return new InvalidOperationException("Dữ liệu mã lộ "+Key+" không hợp lệ ("+why+"); MEPS không ghi đè.");}
  // Strict read: wrong field count/type, schema or version, or a value that is not exactly normalised = invalid; such a record blocks any overwrite.
  public static CircuitTag032 Decode(string[] v){if(v==null||v.Length!=Fields||v.Any(x=>x==null))throw Bad("số trường hoặc kiểu dữ liệu sai");
   if(v[0]!=Schema)throw Bad("schema "+Short(v[0]));if(v[1]!=Version)throw Bad("phiên bản "+Short(v[1])+" chưa hỗ trợ; bản này đọc phiên bản "+Version);
   var t=new CircuitTag032{panel=v[2],circuit=v[3],category=v[4]};try{Check(t);}catch(InvalidOperationException ex){throw Bad(ex.Message);}return t;}
  // Same panel + circuit already tagged with ANOTHER category on a symbol outside the selection → refused (null = no conflict).
  public static string CategoryConflict(IEnumerable<CircuitItem032> others,CircuitTag032 want){
   var c=(others??Enumerable.Empty<CircuitItem032>()).FirstOrDefault(x=>x.status==Assigned&&x.tag!=null&&x.tag.panel==want.panel&&x.tag.circuit==want.circuit&&x.tag.category!=want.category);
   return c==null?null:"Tủ "+want.panel+" / lộ "+want.circuit+" đã dùng loại "+CategoryCaption(c.tag.category)+" ở block "+c.handle+" ("+c.typeName+"); không gán loại "+CategoryCaption(want.category)+" cho cùng lộ. Chọn đúng loại hoặc mã lộ khác.";}
  // Dialog prefill. Every selected item tagged → each field filled only where all values are equal; any untagged item among tagged ones → all blank.
  // mixed = something differs (the dialog says so; nothing is overwritten by opening it). Category null = not selected.
  public static CircuitTag032 Uniform(IList<CircuitItem032> sel,out bool mixed){var tagged=sel.Where(x=>x.tag!=null).Select(x=>x.tag).ToList();var r=new CircuitTag032{panel="",circuit=""};mixed=false;
   if(tagged.Count==0)return r;if(tagged.Count<sel.Count){mixed=true;return r;}
   Func<Func<CircuitTag032,string>,string> one=f=>tagged.Select(f).Distinct(StringComparer.Ordinal).Count()==1?f(tagged[0]):null;
   string p=one(x=>x.panel),c=one(x=>x.circuit),k=one(x=>x.category);mixed=p==null||c==null||k==null;r.panel=p??"";r.circuit=c??"";r.category=k;return r;}
 }
#if !CIRCUIT032_PURE
 public sealed class CircuitApply032 {public bool changed,warn;public int written,unchanged;public string message;}
 // AutoCAD side. Every write is one transaction inside the caller's command (TVXDMEPS060DO: one UNDO step); any failure before Commit aborts everything.
 public static class CircuitCad032 {
  public const int MaxSelect=5000;
  // QA failure injection: "after-first-write" throws after the first tag is written (the transaction aborts; nothing stays written). null in production.
  public static string FailAt;
  static void Fail(string stage){if(FailAt!=null&&string.Equals(FailAt,stage,StringComparison.Ordinal))throw new InvalidOperationException("QA: lỗi giả lập tại "+stage+".");}
  static bool Same(string a,string b){return string.Equals(a??"",b??"",StringComparison.OrdinalIgnoreCase);}
  static bool SameTag(CircuitTag032 a,CircuitTag032 b){return a==null?b==null:a.SameAs(b);}
  static string[] Values(Xrecord x){using(var d=x.Data){return d==null?new string[0]:d.AsArray().Select(v=>v.TypeCode==(short)DxfCode.Text?Convert.ToString(v.Value,CultureInfo.InvariantCulture):null).ToArray();}}
  static bool Has(Transaction tr,DBObject o,string key){return !o.ExtensionDictionary.IsNull&&((DBDictionary)tr.GetObject(o.ExtensionDictionary,OpenMode.ForRead)).Contains(key);}
  static string[] Mark(Transaction tr,BlockReference br,BlockTableRecord b){var m=LibraryCad.ReadMarker(tr,b);
   if(m==null&&br.IsDynamicBlock&&!br.DynamicBlockTableRecord.IsNull)m=LibraryCad.ReadMarker(tr,(BlockTableRecord)tr.GetObject(br.DynamicBlockTableRecord,OpenMode.ForRead));return m;}
  // Tag of an object; null when it has none; malformed / other version → exception.
  public static CircuitTag032 Read(Transaction tr,DBObject o){if(!Has(tr,o,Circuit032.Key))return null;var d=(DBDictionary)tr.GetObject(o.ExtensionDictionary,OpenMode.ForRead);
   var x=tr.GetObject(d.GetAt(Circuit032.Key),OpenMode.ForRead) as Xrecord;if(x==null)throw new InvalidOperationException("Khoá "+Circuit032.Key+" của block "+o.Handle+" không phải Xrecord; MEPS không ghi đè.");
   return Circuit032.Decode(Values(x));}
  // Writes/replaces only Circuit032.Key; other extension-dictionary keys and attributes are untouched.
  static void Write(Transaction tr,DBObject o,CircuitTag032 t){var v=Circuit032.Encode(t);if(!o.IsWriteEnabled)o.UpgradeOpen();if(o.ExtensionDictionary.IsNull)o.CreateExtensionDictionary();
   var d=(DBDictionary)tr.GetObject(o.ExtensionDictionary,OpenMode.ForWrite);
   using(var b=new ResultBuffer(v.Select(s=>new TypedValue((int)DxfCode.Text,s)).ToArray())){
    if(d.Contains(Circuit032.Key)){var x=tr.GetObject(d.GetAt(Circuit032.Key),OpenMode.ForWrite) as Xrecord;if(x==null)throw new InvalidOperationException("Khoá "+Circuit032.Key+" không phải Xrecord; MEPS không ghi đè.");x.Data=b;}
    else{var x=new Xrecord();x.Data=b;d.SetAt(Circuit032.Key,x);tr.AddNewlyCreatedDBObject(x,true);}}}
  // One reference → item, or exception naming why it cannot carry a tag. lockCheck: a locked layer is refused (writing); the read-only scan skips that check.
  static CircuitItem032 Inspect(Transaction tr,Database db,ObjectId id,bool lockCheck){
   if(id.IsNull||id.IsErased||id.Database!=db)throw new InvalidOperationException("đối tượng đã bị xoá hoặc không thuộc bản vẽ hiện tại");
   var br=tr.GetObject(id,OpenMode.ForRead) as BlockReference;if(br==null)throw new InvalidOperationException("đối tượng "+id.Handle+" không phải block");
   string h="block "+br.Handle+": ";
   if(br is MInsertBlock)throw new InvalidOperationException(h+"MINSERT (khối mảng) không hỗ trợ");
   if(br.OwnerId!=SymbolUtilityServices.GetBlockModelSpaceId(db))throw new InvalidOperationException(h+"không nằm trực tiếp trong Model (Layout hoặc bên trong khối khác)");
   var b=(BlockTableRecord)tr.GetObject(br.BlockTableRecord,OpenMode.ForRead);
   if(b.IsLayout||b.IsFromExternalReference||b.IsFromOverlayReference||b.IsDependent)throw new InvalidOperationException(h+"xref/phụ thuộc xref ("+b.Name+")");
   if(LegendPlace011.IsLegend(tr,b))throw new InvalidOperationException(h+"là bảng ký hiệu đã đặt, không phải ký hiệu thiết bị");
   if(lockCheck){var l=(LayerTableRecord)tr.GetObject(br.LayerId,OpenMode.ForRead);if(l.IsLocked)throw new InvalidOperationException(h+"nằm trên layer đang khoá "+l.Name+" (MEPS không mở khoá)");}
   IdentityRecord015 rec;try{rec=IdentityCad015.Read(tr,br);if(rec!=null)Identity015.Check(rec);}catch(InvalidOperationException ex){throw new InvalidOperationException(h+"mã loại MEPS hỏng ("+ex.Message+")");}
   if(rec==null)throw new InvalidOperationException(h+"chưa có mã loại MEPS — chỉ ký hiệu đặt bằng bảng MEPS mới gán lộ được (block mới từ Model: thêm vào thư viện rồi đặt bằng bảng)");
   if(rec.kind!=Identity015.KindBlock)throw new InvalidOperationException(h+"mã "+rec.code+" không phải mã ký hiệu block");
   var m=Mark(tr,br,b);if(m==null||m.Length<4)throw new InvalidOperationException(h+"định nghĩa khối thiếu dấu thư viện MEPS (khối trùng tên/không rõ nguồn)");
   if(!string.Equals(rec.sourceId,m[0],StringComparison.Ordinal)||!Same(rec.alias,m[2])||!Same(rec.sourceSha256,m[3]))throw new InvalidOperationException(h+"mã loại "+rec.code+" không khớp dấu thư viện của khối ("+m[2]+"); MEPS không tự sửa");
   CircuitTag032 tag;try{tag=Read(tr,br);}catch(InvalidOperationException ex){throw new InvalidOperationException(h+ex.Message);}
   return new CircuitItem032{handle=br.Handle.ToString(),typeCode=rec.code,typeName=rec.alias,entryId=rec.sourceId,tag=tag,status=tag==null?Circuit032.Unassigned:Circuit032.Assigned};}
  // Read-only scan of top-level Model references that carry any MEPS mark (identity, library marker or circuit tag); xrefs and placed legends are skipped.
  static void Each(Database db,Action<ObjectId,CircuitItem032> add){
   using(var tr=db.TransactionManager.StartOpenCloseTransaction()){
    foreach(ObjectId id in (BlockTableRecord)tr.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db),OpenMode.ForRead)){if(id.IsErased)continue;var br=tr.GetObject(id,OpenMode.ForRead) as BlockReference;if(br==null)continue;
     var b=(BlockTableRecord)tr.GetObject(br.BlockTableRecord,OpenMode.ForRead);if(b.IsFromExternalReference||b.IsFromOverlayReference||b.IsDependent||LegendPlace011.IsLegend(tr,b))continue;
     if(!Has(tr,br,Identity015.Key)&&!Has(tr,br,Circuit032.Key)&&Mark(tr,br,b)==null)continue;
     CircuitItem032 it;try{it=Inspect(tr,db,id,false);}catch(InvalidOperationException ex){it=new CircuitItem032{handle=br.Handle.ToString(),status=Circuit032.Invalid,issue=ex.Message};}
     add(id,it);}
    tr.Commit();}}
  public static CircuitScan032 Scan(Database db){var s=new CircuitScan032();Each(db,(id,it)=>s.items.Add(it));return s;}
  public static List<ObjectId> UnassignedIds(Database db){var list=new List<ObjectId>();Each(db,(id,it)=>{if(it.status==Circuit032.Unassigned)list.Add(id);});return list;}
  // All-or-none: every distinct selected object must be a supported symbol, else exception listing why (nothing is tagged, no subset).
  public static CircuitSelection032 Snapshot(Database db,ObjectId[] ids){var uniq=(ids??new ObjectId[0]).Distinct().ToList();
   if(uniq.Count==0)throw new InvalidOperationException("Chưa chọn block nào; bản vẽ không đổi.");
   if(uniq.Count>MaxSelect)throw new InvalidOperationException("Chọn quá "+MaxSelect+" block một lần; chia nhỏ lựa chọn. Bản vẽ không đổi.");
   var sel=new CircuitSelection032();var bad=new List<string>();
   using(var tr=db.TransactionManager.StartOpenCloseTransaction()){foreach(var id in uniq){try{sel.items.Add(Inspect(tr,db,id,true));sel.ids.Add(id);}catch(InvalidOperationException ex){bad.Add(ex.Message);}}tr.Commit();}
   if(bad.Count>0)throw new InvalidOperationException("Không gán mã lộ: "+bad.Count+"/"+uniq.Count+" đối tượng đã chọn không hợp lệ (gán tất cả hoặc không gán gì; bỏ chúng khỏi lựa chọn rồi chọn lại): "+
    string.Join("; ",bad.Take(6))+(bad.Count>6?"; …":"")+". Bản vẽ không đổi.");
   sel.scan=Scan(db);var mine=new HashSet<string>(sel.items.Select(x=>x.handle),StringComparer.Ordinal);sel.others.AddRange(sel.scan.items.Where(x=>!mine.Contains(x.handle)));return sel;}
  // Revalidates every selected reference (document objects, type, old tag, layer lock) and the category conflict, then writes the changed tags in ONE
  // transaction, reads them back, calls guard right before Commit. Exactly equal tags everywhere = no change, nothing written.
  public static CircuitApply032 Apply(Database db,CircuitSelection032 sel,CircuitTag032 want,Action<string> guard){Circuit032.Check(want);
   if(sel==null||sel.items.Count==0||sel.items.Count!=sel.ids.Count)throw new InvalidOperationException("Lựa chọn không hợp lệ; bản vẽ không đổi.");
   var mine=new HashSet<string>(sel.items.Select(x=>x.handle),StringComparer.Ordinal);
   string clash=Circuit032.CategoryConflict(Scan(db).items.Where(x=>!mine.Contains(x.handle)),want);if(clash!=null)throw new InvalidOperationException(clash+" Bản vẽ không đổi.");
   var res=new CircuitApply032();
   using(var tr=db.TransactionManager.StartTransaction()){
    for(int i=0;i<sel.ids.Count;i++){var was=sel.items[i];CircuitItem032 now;
     try{now=Inspect(tr,db,sel.ids[i],true);}catch(InvalidOperationException ex){throw new InvalidOperationException("Block đã đổi sau khi chọn ("+ex.Message+"); chọn lại. Bản vẽ không đổi.");}
     if(now.handle!=was.handle||now.typeCode!=was.typeCode||now.entryId!=was.entryId||!SameTag(now.tag,was.tag))throw new InvalidOperationException("Block "+was.handle+" đã đổi mã loại/mã lộ sau khi chọn; chọn lại. Bản vẽ không đổi.");}
    for(int i=0;i<sel.ids.Count;i++){if(want.SameAs(sel.items[i].tag)){res.unchanged++;continue;}Write(tr,tr.GetObject(sel.ids[i],OpenMode.ForWrite),want);res.written++;if(res.written==1)Fail("after-first-write");}
    if(res.written==0){res.message="Cả "+res.unchanged+" block đã có đúng "+want+"; không ghi gì.";return res;}
    foreach(var id in sel.ids)if(!want.SameAs(Read(tr,tr.GetObject(id,OpenMode.ForRead))))throw new InvalidOperationException("Đọc lại mã lộ không khớp; bản vẽ không đổi.");
    if(guard!=null)guard("ngay trước khi lưu mã lộ");
    tr.Commit();}
   res.changed=true;
   // After Commit (same command): fresh read. A mismatch cannot be rolled back here; it is reported and the user's U reverts the whole step.
   int ok=0;try{using(var tr=db.TransactionManager.StartOpenCloseTransaction()){foreach(var id in sel.ids)if(want.SameAs(Read(tr,tr.GetObject(id,OpenMode.ForRead))))ok++;tr.Commit();}}catch(Exception){}
   res.warn=ok!=sel.ids.Count;
   res.message=(res.warn?"CẢNH BÁO: đọc lại sau khi lưu chỉ khớp "+ok+"/"+sel.ids.Count+" block; dùng U để hoàn tác bước này. ":"")+
    "Đã gán "+want+" cho "+res.written+" block"+(res.unchanged>0?" ("+res.unchanged+" block đã có sẵn mã này)":"")+". Ghi trong bản vẽ: U/UNDO hoàn tác cả bước; chưa tự lưu DWG — lưu DWG để giữ. "+
    "Mã lộ là nhãn người dùng nhập, không phải kiểm tra thiết kế điện; COPY/WBLOCK mang theo mã, EXPLODE làm mất.";
   return res;}
 }
#endif
}
#endif
