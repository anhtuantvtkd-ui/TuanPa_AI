using System;using System.Linq;using System.Collections.Generic;using System.Globalization;using System.Text;using IOE=System.InvalidOperationException;
#if TAG033 && !TAG033_PURE
using Autodesk.AutoCAD.DatabaseServices;using Autodesk.AutoCAD.Geometry;
#endif
#if TAG033
namespace Tvxd.Meps {
 // TVXD-MEPS-TAG-033 PILOT (core only, no UI yet): an editable VISIBLE label beside a placed block (switch, lamp, fan, breaker, regulator…), e.g. "1,2", "AC-01", "Q1".
 //  - The label is ONE native AttributeReference, tag TVXD_LABEL_033, appended to the BlockReference's AttributeCollection. No ATTDEF is added; the block
 //    definition, name, geometry, layer, scale and every existing attribute stay unchanged. No wrapper block, no reactor/overrule: MOVE/COPY/WBLOCK carry the
 //    label as part of the reference and its own grip moves it (LockPositionInBlock false). All of this must be PROVEN natively (GPT pilot QA).
 //  - Ownership: the attribute's OWN extension dictionary holds Xrecord Key = [Schema, Version], no copy of the text. An attribute named TVXD_LABEL_033
 //    without that marker belongs to the block/user and is never overwritten (collision = refusal). Text, height and position are read from the attribute
 //    every time, so native Properties/grip edits are the source of truth.
 //  - Never touched: Identity015 / Circuit032 records, library marker/catalog, existing text styles. A new label uses the owned Arial style TVXD_TAG033_ARIAL
 //    (created once and marked; a same-named foreign style is skipped, never changed), ACI 7, the block's layer, WCS horizontal, left/baseline, no field.
 //  - Height is in DRAWING UNITS (no printed-mm guarantee). Suggested default ≈ 18 % of the block's bounding-box height (prefill only; Apply needs an explicit value).
 //  - Known native risks (not yet proven either way): ATTSYNC / BATTMAN Sync / redefinition with sync remove attributes that have no ATTDEF; EXPLODE turns the
 //    label into attribute-definition text; REFEDIT may drop it. Anonymous non-dynamic (ARRAY…) and annotative blocks are refused.
 //  - PATCH01 (F033-DYN01): top-level planar DYNAMIC blocks are supported with the same native attribute (no conversion to static, no definition/parameter/
 //    name change). The stale signature includes the stable dynamic definition and every dynamic property value. The label does NOT follow later dynamic
 //    parameter edits (stretch/flip/visibility…) relative to the geometry: the user may have to move it by its grip. No reactor.
 // Pure part (compiled alone with /define:TAG033;TAG033_PURE): text/height rules, height suggestion, marker check. AutoCAD part: TagCad033.
 // blockName: effective (for a dynamic block: stable definition) name, display only. dynamic: reference is a dynamic block (PATCH01).
 public sealed class TagItem033 {public string handle,tagHandle,text,signature,blockName;public bool hasTag,extOk,dynamic;public double height,x,y,z,minX,minY,maxX,maxY;public double? suggested;
#if !TAG033_PURE
  public ObjectId owner,tagId;
#endif
 }
 // Validated selection: one item per distinct owner block, in selection order.
 public sealed class TagSnapshot033 {public readonly List<TagItem033> items=new List<TagItem033>();
  public int Tagged{get{return items.Count(x=>x.hasTag);}}public int Untagged{get{return items.Count(x=>!x.hasTag);}}
  // Prefill for a later dialog: the value shared by EVERY selected block; null when some block has no label or the values differ.
  public string CommonText(){if(items.Count==0||items.Any(x=>!x.hasTag))return null;var t=items.Select(x=>x.text).Distinct(StringComparer.Ordinal).ToList();return t.Count==1?t[0]:null;}
  public double? CommonHeight(){if(items.Count==0||items.Any(x=>!x.hasTag))return null;var h=items.Select(x=>x.height).Distinct().ToList();return h.Count==1?(double?)h[0]:null;}
  public string Summary(){return "Đã chọn "+items.Count+" block: "+Tagged+" đã có nhãn, "+Untagged+" chưa có nhãn.";}}
 public sealed class TagApply033 {public bool changed,warn;public int created,updated,unchanged;public string message;}
 // Read helper result (QA): current state of an owned label; issue set only by ScanModel for a block whose label cannot be used.
 public sealed class TagInfo033 {public string ownerHandle,tagHandle,text,style,layer,justify,grip,issue;public double height,x,y,z,rotation;public int colorIndex;public bool invisible;}
 public static class Tag033 {
  public const string AttTag="TVXD_LABEL_033",Key="TVXD_MEPS_TAG_033",Schema="TVXD_MEPS_TAG",StyleKey="TVXD_MEPS_TAG_033_STYLE",StyleSchema="TVXD_MEPS_TAG_STYLE",Version="1";
  public const string StyleName="TVXD_TAG033_ARIAL",Font="Arial",Units="đơn vị bản vẽ (drawing units)";
  public const int MaxText=40;public const double DefaultRatio=0.18;
  public const string Rule="Nhãn: 1–40 ký tự trên một dòng, giữ đúng như nhập (chữ tiếng Việt, số, dấu phẩy, gạch ngang, chấm…); không có khoảng trắng đầu/cuối, "+
   "không dùng \\ , %% hoặc %< (CAD đổi thành ký tự/field khác), không xuống dòng hay ký tự điều khiển. Ví dụ: 1,2 / AC-01 / Q1.";
  static string Short(string s){return s==null?"null":s.Length>24?s.Substring(0,24)+"…":s;}
  static IOE Bad(string s,string why){return new IOE("Nhãn \""+Short(s)+"\" "+why+". "+Rule);}
  // Returns the text exactly as given, or throws; nothing is trimmed, case-folded or encoded. DBText decodes %%x, \U+xxxx / \M+ and %<…>% fields,
  // so those are refused instead of being displayed differently from what was typed.
  public static string CheckText(string s){
   if(s==null||s.Length==0)throw new IOE("Chưa nhập chữ nhãn. "+Rule);
   if(s.Length>MaxText)throw new IOE("Nhãn dài "+s.Length+" ký tự (tối đa "+MaxText+"). "+Rule);
   if(s[0]==' '||s[s.Length-1]==' ')throw Bad(s,"có khoảng trắng đầu/cuối; MEPS không tự cắt");
   if(s.IndexOf('\\')>=0)throw Bad(s,"có dấu \\ (CAD đọc \\U+…/\\M+… thành ký tự khác)");
   if(s.Contains("%%"))throw Bad(s,"có %% (CAD đổi %%c, %%d, %%p… thành ký hiệu)");
   if(s.Contains("%<"))throw Bad(s,"có %< (cú pháp field của CAD)");
   foreach(char c in s){if(c==' ')continue;if(char.IsSurrogate(c))throw Bad(s,"có ký tự ngoài bảng chữ cơ bản (emoji…)");
    switch(char.GetUnicodeCategory(c)){case UnicodeCategory.Control:case UnicodeCategory.Format:case UnicodeCategory.LineSeparator:case UnicodeCategory.ParagraphSeparator:
     case UnicodeCategory.SpaceSeparator:case UnicodeCategory.PrivateUse:case UnicodeCategory.OtherNotAssigned:
      throw Bad(s,"có ký tự điều khiển/khoảng trắng đặc biệt U+"+((int)c).ToString("X4",CultureInfo.InvariantCulture));}}
   return s;}
  public static double CheckHeight(double h){if(double.IsNaN(h)||double.IsInfinity(h)||!(h>0))
    throw new IOE("Chiều cao chữ phải là số dương hữu hạn, tính bằng "+Units+"; nhận "+h.ToString("R",CultureInfo.InvariantCulture)+".");return h;}
  // Prefill suggestion only (3 significant digits): DefaultRatio × bounding-box height, or × width for a flat symbol; null when the box is unusable.
  public static double? Suggest(double width,double height){double b=height>0&&!double.IsInfinity(height)?height:width;if(double.IsNaN(b)||double.IsInfinity(b)||!(b>0))return null;
   return double.Parse((b*DefaultRatio).ToString("G3",CultureInfo.InvariantCulture),CultureInfo.InvariantCulture);}
  public static string[] Mark(string schema){return new[]{schema,Version};}
  // Strict marker read: exactly [schema, Version] as text; anything else blocks every write.
  public static void CheckMark(string[] v,string schema,string what){
   if(v==null||v.Length!=2||v.Any(x=>x==null))throw new IOE("dấu "+what+" MEPS 033 hỏng (số trường hoặc kiểu dữ liệu sai); MEPS không ghi đè");
   if(v[0]!=schema)throw new IOE("dấu "+what+" MEPS 033 sai schema "+Short(v[0])+"; MEPS không ghi đè");
   if(v[1]!=Version)throw new IOE("dấu "+what+" MEPS 033 phiên bản "+Short(v[1])+" chưa hỗ trợ (bản này đọc phiên bản "+Version+"); MEPS không ghi đè");}
 }
#if !TAG033_PURE
 // AutoCAD side. Call inside the caller's command with the document locked. Apply is ONE transaction (one native UNDO step of that command); no Undo marks,
 // no Save. Any failure before Commit aborts everything (nothing written).
 public static class TagCad033 {
  public const int MaxSelect=5000;
  public const string GripUnlocked="UNLOCKED",GripLocked="LOCKED",GripUnverified="UNVERIFIED_NO_PROPERTY";
  // QA failure injection: "after-first-write" throws after the first label is created/changed (the transaction aborts; nothing stays written). null in production.
  public static string FailAt;
  static void Fail(string stage){if(FailAt!=null&&string.Equals(FailAt,stage,StringComparison.Ordinal))throw new IOE("QA: lỗi giả lập tại "+stage+".");}
  static readonly Autodesk.AutoCAD.Geometry.Tolerance Tol=new Autodesk.AutoCAD.Geometry.Tolerance(1e-9,1e-9);
  // LockPositionInBlock is resolved by reflection: the pilot must show on CAD2021 whether the property exists and is writable (docs differ).
  static readonly System.Reflection.PropertyInfo LockProp=typeof(AttributeReference).GetProperties().FirstOrDefault(p=>p.Name=="LockPositionInBlock"&&p.PropertyType==typeof(bool));
  static string N(double v){return v.ToString("R",CultureInfo.InvariantCulture);}
  static string P(Point3d p){return N(p.X)+","+N(p.Y)+","+N(p.Z);}
  static string[] Values(Xrecord x){using(var d=x.Data){return d==null?new string[0]:d.AsArray().Select(v=>v.TypeCode==(short)DxfCode.Text?Convert.ToString(v.Value,CultureInfo.InvariantCulture):null).ToArray();}}
  // Marker values under key in o's own extension dictionary; null = no such key; a non-Xrecord entry = exception.
  static string[] ReadMark(Transaction tr,DBObject o,string key){if(o.ExtensionDictionary.IsNull)return null;var d=(DBDictionary)tr.GetObject(o.ExtensionDictionary,OpenMode.ForRead);
   if(!d.Contains(key))return null;var x=tr.GetObject(d.GetAt(key),OpenMode.ForRead) as Xrecord;if(x==null)throw new IOE("khoá "+key+" của "+o.Handle+" không phải Xrecord; MEPS không ghi đè");return Values(x);}
  // Only on objects created in this transaction.
  static void WriteMark(Transaction tr,DBObject o,string key,string schema){if(o.ExtensionDictionary.IsNull)o.CreateExtensionDictionary();
   var d=(DBDictionary)tr.GetObject(o.ExtensionDictionary,OpenMode.ForWrite);if(d.Contains(key))throw new IOE("Khoá "+key+" đã có trên đối tượng mới "+o.Handle+"; MEPS không ghi đè.");
   using(var b=new ResultBuffer(Tag033.Mark(schema).Select(s=>new TypedValue((int)DxfCode.Text,s)).ToArray())){var x=new Xrecord();x.Data=b;d.SetAt(key,x);tr.AddNewlyCreatedDBObject(x,true);}}
  // Grip state: UNLOCKED (moves by its own grip), LOCKED, or UNVERIFIED when this AutoCAD API exposes no such property (native QA decides).
  public static string Grip(AttributeReference a){if(LockProp==null||!LockProp.CanRead)return GripUnverified;return (bool)LockProp.GetValue(a,null)?GripLocked:GripUnlocked;}
  // New labels only: clear the lock when the API allows it; a lock that cannot be cleared aborts (no half-working label).
  static void Unlock(AttributeReference a){if(Grip(a)!=GripLocked)return;var set=LockProp.GetSetMethod();
   if(set==null)throw new IOE("Không mở khoá được grip vị trí của nhãn mới (LockPositionInBlock chỉ đọc trong bản AutoCAD này). Bản vẽ không đổi.");
   set.Invoke(a,new object[]{false});if(Grip(a)!=GripUnlocked)throw new IOE("Grip vị trí của nhãn mới vẫn bị khoá sau khi mở. Bản vẽ không đổi.");}
  // G004 (library capture): true = ar is a MEPS label (valid marker, schema/version, tag TVXD_LABEL_033); false = no marker (foreign attribute, kept even when
  // named TVXD_LABEL_033); malformed marker or marker on another tag = exception. Works in any database (also an owned side copy). Read only.
  public static bool IsOwnedAttribute(Transaction tr,AttributeReference ar){if(ar==null)return false;string[] m;
   try{m=ReadMark(tr,ar,Tag033.Key);if(m==null)return false;Tag033.CheckMark(m,Tag033.Schema,"nhãn");}catch(IOE ex){throw new IOE("thuộc tính "+ar.Handle+": "+ex.Message);}
   if(!string.Equals(ar.Tag,Tag033.AttTag,StringComparison.OrdinalIgnoreCase))throw new IOE("thuộc tính "+ar.Tag+" ("+ar.Handle+") mang dấu nhãn MEPS nhưng sai tên; MEPS không sửa");
   return true;}
  static void Live(Database db,ObjectId id){if(id.IsNull||id.IsErased||id.Database!=db)throw new IOE("đối tượng đã bị xoá hoặc không thuộc bản vẽ hiện tại");}
  // Block for a selected block, or the owner block of a selected owned label. Any other attribute is refused.
  static ObjectId Owner(Transaction tr,Database db,ObjectId id){Live(db,id);var o=tr.GetObject(id,OpenMode.ForRead);if(o is BlockReference)return id;
   var ar=o as AttributeReference;if(ar==null)throw new IOE("đối tượng "+id.Handle+" ("+o.GetType().Name+") không phải block");
   if(ReadMark(tr,ar,Tag033.Key)==null)throw new IOE("thuộc tính "+ar.Tag+" ("+ar.Handle+") không phải nhãn MEPS; hãy chọn block");
   Live(db,ar.OwnerId);return ar.OwnerId;}
  // The owned label of br, or null. Refuses: an attribute named TVXD_LABEL_033 without the marker (block/user data), a malformed or other-version marker,
  // the marker on another tag, more than one label, a multi-line/constant label, an unusable height. attrs = handle:tag of every live attribute.
  static AttributeReference FindOwned(Transaction tr,BlockReference br,string h,out List<string> attrs){AttributeReference own=null;attrs=new List<string>();
   foreach(ObjectId a in br.AttributeCollection){if(a.IsErased)continue;var ar=tr.GetObject(a,OpenMode.ForRead) as AttributeReference;if(ar==null)continue;attrs.Add(ar.Handle+":"+ar.Tag);
    bool named=string.Equals(ar.Tag,Tag033.AttTag,StringComparison.OrdinalIgnoreCase);string[] m;
    try{m=ReadMark(tr,ar,Tag033.Key);if(m!=null)Tag033.CheckMark(m,Tag033.Schema,"nhãn");}catch(IOE ex){throw new IOE(h+"thuộc tính "+ar.Handle+": "+ex.Message);}
    if(m==null){if(named)throw new IOE(h+"đã có thuộc tính "+ar.Tag+" ("+ar.Handle+") của khối/người dùng, không có dấu nhãn MEPS; MEPS không ghi đè và không thêm nhãn trùng tên");continue;}
    if(!named)throw new IOE(h+"thuộc tính "+ar.Tag+" ("+ar.Handle+") mang dấu nhãn MEPS nhưng sai tên; MEPS không sửa");
    if(own!=null)throw new IOE(h+"có hơn một nhãn MEPS ("+own.Handle+", "+ar.Handle+"); MEPS không tự chọn");
    if(ar.IsMTextAttribute||ar.IsConstant)throw new IOE(h+"nhãn "+ar.Handle+" là thuộc tính nhiều dòng/hằng; chưa hỗ trợ");
    if(double.IsNaN(ar.Height)||double.IsInfinity(ar.Height)||!(ar.Height>0))throw new IOE(h+"chiều cao nhãn "+ar.Handle+" không hợp lệ");
    own=ar;}
   return own;}
  // PATCH01: deterministic text of a dynamic reference's property state, in collection order: index, name, type code, units, read-only, value (invariant).
  // A property whose value cannot be read or has an unsupported value type refuses the block (never silently left out of the stale check).
  static string DynValue(object v){if(v==null)return "null";
   if(v is double)return "d"+N((double)v);if(v is float)return "f"+N((float)v);
   if(v is short||v is int||v is long||v is byte||v is ushort||v is uint||v is ulong||v is sbyte)return "i"+Convert.ToString(v,CultureInfo.InvariantCulture);
   if(v is bool)return (bool)v?"b1":"b0";
   var s=v as string;if(s!=null)return "s"+s.Length+":"+s;
   if(v is Point3d)return "p"+P((Point3d)v);if(v is Point2d){var p=(Point2d)v;return "q"+N(p.X)+","+N(p.Y);}
   if(v is Vector3d){var w=(Vector3d)v;return "v"+N(w.X)+","+N(w.Y)+","+N(w.Z);}
   if(v is ObjectId)return "o"+((ObjectId)v).Handle;
   return null;}
  static string DynState(BlockReference br,string h){var s=new StringBuilder();int k=0;DynamicBlockReferencePropertyCollection props;
   try{props=br.DynamicBlockReferencePropertyCollection;}catch(Exception ex){throw new IOE(h+"không đọc được tham số khối động ("+ex.Message+"); MEPS không gắn nhãn");}
   foreach(DynamicBlockReferenceProperty p in props){string name=p.PropertyName??"";object v;
    try{v=p.Value;}catch(Exception ex){throw new IOE(h+"không đọc được tham số động \""+name+"\" ("+ex.Message+"); MEPS không gắn nhãn");}
    string t=DynValue(v);if(t==null)throw new IOE(h+"tham số động \""+name+"\" có kiểu giá trị "+v.GetType().Name+" chưa hỗ trợ; MEPS không gắn nhãn");
    s.Append("|P").Append(k++).Append(':').Append(name.Length).Append(':').Append(name).Append(':').Append(p.PropertyTypeCode).Append(':').Append(p.UnitsType).Append(':').Append(p.ReadOnly?1:0).Append(':').Append(t);}
   return s.Append("|PN").Append(k).ToString();}
  // One reference → item, or exception naming why it cannot carry a label. lockCheck: locked layers (block or label) are refused for writing.
  // signature = block handle/definition/layer/BlockTransform + every attribute handle:tag + the label's text/height/position/alignment/rotation/justify/style/layer
  // /visibility/width; Apply refuses when it differs from the snapshot (stale).
  static TagItem033 Inspect(Transaction tr,Database db,ObjectId id,bool lockCheck){Live(db,id);
   var br=tr.GetObject(id,OpenMode.ForRead) as BlockReference;if(br==null)throw new IOE("đối tượng "+id.Handle+" không phải block");
   string h="block "+br.Handle+": ";
   if(br is MInsertBlock)throw new IOE(h+"MINSERT (khối mảng) không hỗ trợ");
   if(br.OwnerId!=SymbolUtilityServices.GetBlockModelSpaceId(db))throw new IOE(h+"không nằm trực tiếp trong Model (Layout hoặc bên trong khối khác)");
   var b=(BlockTableRecord)tr.GetObject(br.BlockTableRecord,OpenMode.ForRead);
   if(b.IsLayout||b.IsFromExternalReference||b.IsFromOverlayReference||b.IsDependent)throw new IOE(h+"xref/phụ thuộc xref ("+b.Name+")");
   if(LegendPlace011.IsLegend(tr,b))throw new IOE(h+"là bảng ký hiệu đã đặt, không phải ký hiệu thiết bị");
   // PATCH01 F033-DYN01: a top-level planar DYNAMIC reference is supported (e.g. the library double switch TVXD_E_CT_1C_02). Its current definition may be
   // the stable dynamic definition itself or one of ITS evaluated anonymous definitions (*U…); both are only read, never converted, renamed or edited.
   // Anonymous definitions that do not belong to a dynamic block (associative ARRAY…) stay refused.
   BlockTableRecord sb=null;string dyn="";
   if(br.IsDynamicBlock){var sid=br.DynamicBlockTableRecord;
    if(sid.IsNull||sid.IsErased||sid.Database!=db)throw new IOE(h+"khối động không đọc được định nghĩa gốc; MEPS không gắn nhãn");
    sb=(BlockTableRecord)tr.GetObject(sid,OpenMode.ForRead);
    if(!sb.IsDynamicBlock||sb.IsAnonymous||sb.IsLayout)throw new IOE(h+"định nghĩa gốc "+sb.Name+" của khối động không hợp lệ; MEPS không gắn nhãn");
    if(sb.IsFromExternalReference||sb.IsFromOverlayReference||sb.IsDependent)throw new IOE(h+"xref/phụ thuộc xref ("+sb.Name+")");
    if(LegendPlace011.IsLegend(tr,sb))throw new IOE(h+"là bảng ký hiệu đã đặt, không phải ký hiệu thiết bị");
    if(b.ObjectId!=sid){bool mine=false;foreach(ObjectId aid in sb.GetAnonymousBlockIds())if(aid==b.ObjectId){mine=true;break;}
     if(!mine)throw new IOE(h+"định nghĩa hiện thời "+b.Name+" không thuộc khối động "+sb.Name+"; MEPS không gắn nhãn");}
    dyn=DynState(br,h);}
   else{if(b.IsDynamicBlock)throw new IOE(h+"tham chiếu tới định nghĩa động "+b.Name+" nhưng CAD không coi là khối động; MEPS không gắn nhãn");
    if(b.IsAnonymous)throw new IOE(h+"khối ẩn danh "+b.Name+" (ARRAY liên kết…) chưa hỗ trợ");}
   if(b.Annotative==AnnotativeStates.True||br.Annotative==AnnotativeStates.True||(sb!=null&&sb.Annotative==AnnotativeStates.True))throw new IOE(h+"khối chú thích (annotative) chưa hỗ trợ");
   if(!br.Normal.IsEqualTo(Vector3d.ZAxis,Tol))throw new IOE(h+"không nằm phẳng trong mặt XY (pháp tuyến "+N(br.Normal.X)+","+N(br.Normal.Y)+","+N(br.Normal.Z)+")");
   // Same-name ATTDEF collision on the current AND (dynamic) the stable definition.
   foreach(var def in sb==null||sb.ObjectId==b.ObjectId?new[]{b}:new[]{b,sb})if(def.HasAttributeDefinitions)foreach(ObjectId eid in def){var ad=tr.GetObject(eid,OpenMode.ForRead) as AttributeDefinition;
    if(ad!=null&&string.Equals(ad.Tag,Tag033.AttTag,StringComparison.OrdinalIgnoreCase))throw new IOE(h+"định nghĩa khối "+def.Name+" đã có ATTDEF "+ad.Tag+" của người dùng; MEPS không dùng trùng tên");}
   if(lockCheck){var lb=(LayerTableRecord)tr.GetObject(br.LayerId,OpenMode.ForRead);if(lb.IsLocked)throw new IOE(h+"nằm trên layer đang khoá "+lb.Name+" (MEPS không mở khoá)");}
   List<string> attrs;var own=FindOwned(tr,br,h,out attrs);
   if(own!=null&&lockCheck){var la=(LayerTableRecord)tr.GetObject(own.LayerId,OpenMode.ForRead);if(la.IsLocked)throw new IOE(h+"nhãn nằm trên layer đang khoá "+la.Name+" (MEPS không mở khoá)");}
   var it=new TagItem033{handle=br.Handle.ToString(),owner=id,hasTag=own!=null,blockName=sb!=null?sb.Name:b.Name,dynamic=sb!=null};
   try{var ext=br.GeometricExtents;double[] v={ext.MinPoint.X,ext.MinPoint.Y,ext.MaxPoint.X,ext.MaxPoint.Y};
    if(v.All(d=>!double.IsNaN(d)&&!double.IsInfinity(d))&&v[2]>=v[0]&&v[3]>=v[1]){it.extOk=true;it.minX=v[0];it.minY=v[1];it.maxX=v[2];it.maxY=v[3];it.suggested=Tag033.Suggest(v[2]-v[0],v[3]-v[1]);}}
   catch(Exception){}
   var s=new StringBuilder();s.Append(br.Handle).Append('|').Append(b.Handle).Append('|').Append(br.LayerId.Handle);
   foreach(double mv in br.BlockTransform.ToArray())s.Append('|').Append(N(mv));
   foreach(var a in attrs)s.Append("|A:").Append(a);
   // PATCH01: dynamic = stable definition handle + every property (name/type/units/read-only/value, invariant). A parameter edit after the snapshot = stale.
   if(sb!=null)s.Append("|DYN:").Append(sb.Handle).Append('|').Append(dyn);
   if(own!=null){it.tagId=own.ObjectId;it.tagHandle=own.Handle.ToString();it.text=own.TextString;it.height=own.Height;it.x=own.Position.X;it.y=own.Position.Y;it.z=own.Position.Z;
    s.Append("|T:").Append(own.TextString.Length).Append(':').Append(own.TextString).Append('|').Append(N(own.Height)).Append('|').Append(P(own.Position)).Append('|').Append(P(own.AlignmentPoint))
     .Append('|').Append(N(own.Rotation)).Append('|').Append(own.Justify).Append('|').Append(own.TextStyleId.Handle).Append('|').Append(own.LayerId.Handle).Append('|').Append(own.Invisible)
     .Append('|').Append(N(own.WidthFactor));}
   it.signature=s.ToString();return it;}
  // All-or-none: every selected object must be a supported block or an owned label (mapped to its block; duplicates collapse), else exception listing why.
  public static TagSnapshot033 Snapshot(Database db,IEnumerable<ObjectId> ids){if(db==null)throw new IOE("Không có bản vẽ.");
   var input=(ids??Enumerable.Empty<ObjectId>()).ToList();if(input.Count==0)throw new IOE("Chưa chọn block nào; bản vẽ không đổi.");
   var snap=new TagSnapshot033();var owners=new List<ObjectId>();var seen=new HashSet<ObjectId>();var bad=new List<string>();
   using(var tr=db.TransactionManager.StartOpenCloseTransaction()){
    foreach(var id in input){ObjectId o;try{o=Owner(tr,db,id);}catch(IOE ex){bad.Add(ex.Message);continue;}if(seen.Add(o))owners.Add(o);}
    if(owners.Count>MaxSelect)throw new IOE("Chọn quá "+MaxSelect+" block một lần; chia nhỏ lựa chọn. Bản vẽ không đổi.");
    foreach(var o in owners){try{snap.items.Add(Inspect(tr,db,o,true));}catch(IOE ex){bad.Add(ex.Message);}}
    tr.Commit();}
   if(bad.Count>0)throw new IOE("Không gắn nhãn: "+bad.Count+" đối tượng đã chọn không hợp lệ (gắn tất cả hoặc không gắn gì; bỏ chúng khỏi lựa chọn rồi chọn lại): "+
    string.Join("; ",bad.Take(6))+(bad.Count>6?"; …":"")+". Bản vẽ không đổi.");
   return snap;}
  // Owned text style for NEW labels: TVXD_TAG033_ARIAL[_2.._9] — first one that is ours and unchanged, else the first free name. Existing styles are never modified.
  static bool OwnedStyle(Transaction tr,TextStyleTableRecord r){string[] m;try{m=ReadMark(tr,r,Tag033.StyleKey);if(m==null)return false;Tag033.CheckMark(m,Tag033.StyleSchema,"kiểu chữ");}catch(IOE){return false;}
   return !r.IsShapeFile&&!r.IsVertical&&r.TextSize==0&&r.XScale==1&&r.ObliquingAngle==0&&string.Equals(r.Font.TypeFace,Tag033.Font,StringComparison.OrdinalIgnoreCase);}
  static ObjectId Style(Transaction tr,Database db){var tst=(TextStyleTable)tr.GetObject(db.TextStyleTableId,OpenMode.ForRead);
   for(int k=1;k<=9;k++){string name=k==1?Tag033.StyleName:Tag033.StyleName+"_"+k;
    if(tst.Has(name)){var sid=tst[name];if(sid.IsNull||sid.IsErased)continue;if(OwnedStyle(tr,(TextStyleTableRecord)tr.GetObject(sid,OpenMode.ForRead)))return sid;continue;}
    if(!tst.IsWriteEnabled)tst.UpgradeOpen();
    var ts=new TextStyleTableRecord();ts.Name=name;ts.Font=new Autodesk.AutoCAD.GraphicsInterface.FontDescriptor(Tag033.Font,false,false,0,34);ts.TextSize=0;ts.XScale=1;ts.ObliquingAngle=0;
    var id=tst.Add(ts);tr.AddNewlyCreatedDBObject(ts,true);WriteMark(tr,ts,Tag033.StyleKey,Tag033.StyleSchema);
    if(!OwnedStyle(tr,ts))throw new IOE("Kiểu chữ mới "+name+" đọc lại không đúng Arial; bản vẽ không đổi.");return id;}
   throw new IOE("Các tên kiểu chữ "+Tag033.StyleName+" … _9 đều đã dùng cho kiểu chữ khác; MEPS không sửa kiểu chữ có sẵn. Bản vẽ không đổi.");}
  // New label: right of the block's bounding box, baseline so the text is vertically centred on it; fallback beside the insertion point.
  static Point3d Anchor(TagItem033 it,BlockReference br,double h){double z=br.Position.Z;
   return it.extOk?new Point3d(it.maxX+0.5*h,(it.minY+it.maxY)/2-0.5*h,z):new Point3d(br.Position.X+0.5*h,br.Position.Y-0.5*h,z);}
  static void Create(Transaction tr,Database db,TagItem033 it,string text,double h,ObjectId style){var br=(BlockReference)tr.GetObject(it.owner,OpenMode.ForWrite);
   var at=new AttributeReference();at.SetDatabaseDefaults(db);
   at.Tag=Tag033.AttTag;at.TextString=text;at.TextStyleId=style;at.Height=h;at.WidthFactor=1;at.Oblique=0;at.Rotation=0;at.Normal=Vector3d.ZAxis;
   at.HorizontalMode=TextHorizontalMode.TextLeft;at.VerticalMode=TextVerticalMode.TextBase;at.Position=Anchor(it,br,h);
   at.LayerId=br.LayerId;at.ColorIndex=7;at.Linetype="ByLayer";at.LineWeight=LineWeight.ByLayer;at.Invisible=false;
   br.AttributeCollection.AppendAttribute(at);tr.AddNewlyCreatedDBObject(at,true);
   WriteMark(tr,at,Tag033.Key,Tag033.Schema);Unlock(at);}
  // textOrNull / height null = keep each block's current value. Blocks without a label need both (a new label is never invented). Revalidates every block
  // against the snapshot signature, writes in ONE transaction, reads back, calls guard right before Commit. Nothing differs = nothing written.
  public static TagApply033 Apply(Database db,TagSnapshot033 snap,string textOrNull,double? height,Action<string> contextGuard){
   if(db==null||snap==null||snap.items.Count==0||snap.items.Count>MaxSelect||snap.items.Select(x=>x.owner).Distinct().Count()!=snap.items.Count)throw new IOE("Lựa chọn không hợp lệ; bản vẽ không đổi.");
   if(textOrNull==null&&!height.HasValue)throw new IOE("Chưa nhập chữ nhãn hoặc chiều cao; bản vẽ không đổi.");
   string text;double? hh;try{text=textOrNull==null?null:Tag033.CheckText(textOrNull);hh=height.HasValue?(double?)Tag033.CheckHeight(height.Value):null;}
   catch(IOE ex){throw new IOE(ex.Message+" Bản vẽ không đổi.");}
   int fresh=snap.items.Count(x=>!x.hasTag);
   if(fresh>0&&(text==null||!hh.HasValue))throw new IOE(fresh+" block chưa có nhãn: nhãn mới cần nhập cả chữ và chiều cao ("+Tag033.Units+"). Bản vẽ không đổi.");
   var res=new TagApply033();
   using(var tr=db.TransactionManager.StartTransaction()){
    var now=new List<TagItem033>();
    foreach(var was in snap.items){TagItem033 cur;
     try{cur=Inspect(tr,db,was.owner,true);}catch(IOE ex){throw new IOE("Block đã đổi sau khi chọn ("+ex.Message+"); chọn lại. Bản vẽ không đổi.");}
     if(!string.Equals(cur.signature,was.signature,StringComparison.Ordinal))throw new IOE("Block "+was.handle+" hoặc nhãn của nó đã đổi (vị trí/xoay/tỉ lệ/chữ/chiều cao/thuộc tính) sau khi chọn; chọn lại. Bản vẽ không đổi.");
     now.Add(cur);}
    var style=fresh>0?Style(tr,db):ObjectId.Null;
    foreach(var it in now){
     if(!it.hasTag){Create(tr,db,it,text,hh.Value,style);res.created++;}
     else{var ar=(AttributeReference)tr.GetObject(it.tagId,OpenMode.ForRead);bool t=text!=null&&!string.Equals(ar.TextString,text,StringComparison.Ordinal),hc=hh.HasValue&&ar.Height!=hh.Value;
      if(!t&&!hc){res.unchanged++;continue;}
      ar.UpgradeOpen();if(t)ar.TextString=text;if(hc)ar.Height=hh.Value;if(ar.Justify!=AttachmentPoint.BaseLeft)ar.AdjustAlignment(db);res.updated++;}
     if(res.created+res.updated==1)Fail("after-first-write");}
    if(res.created+res.updated==0){res.message="Cả "+res.unchanged+" nhãn đã đúng chữ/chiều cao yêu cầu; không ghi gì.";return res;}
    foreach(var it in now){var br=(BlockReference)tr.GetObject(it.owner,OpenMode.ForRead);List<string> attrs;var own=FindOwned(tr,br,"block "+it.handle+": ",out attrs);
     string wt=text??it.text;double wh=hh.HasValue?hh.Value:it.height;
     if(own==null||!string.Equals(own.TextString,wt,StringComparison.Ordinal)||own.Height!=wh||(!it.hasTag&&(own.Invisible||own.LayerId!=br.LayerId||Grip(own)==GripLocked)))
      throw new IOE("Đọc lại nhãn block "+it.handle+" không khớp; bản vẽ không đổi.");}
    if(contextGuard!=null)contextGuard("ngay trước khi lưu nhãn");
    tr.Commit();}
   res.changed=true;
   // After Commit (same command): fresh read. A mismatch cannot be rolled back here; it is reported and the user's U reverts the whole step.
   int ok=0;try{using(var tr=db.TransactionManager.StartOpenCloseTransaction()){foreach(var it in snap.items){var br=(BlockReference)tr.GetObject(it.owner,OpenMode.ForRead);List<string> attrs;
      var own=FindOwned(tr,br,"",out attrs);if(own!=null&&(textOrNull==null||string.Equals(own.TextString,textOrNull,StringComparison.Ordinal))&&(!height.HasValue||own.Height==height.Value))ok++;}tr.Commit();}}catch(Exception){}
   res.warn=ok!=snap.items.Count;
   res.message=(res.warn?"CẢNH BÁO: đọc lại sau khi lưu chỉ khớp "+ok+"/"+snap.items.Count+" block; dùng U để hoàn tác bước này. ":"")+
    "Đã tạo "+res.created+" nhãn, sửa "+res.updated+" nhãn"+(res.unchanged>0?" ("+res.unchanged+" nhãn đã đúng, giữ nguyên)":"")+". Chiều cao tính bằng "+Tag033.Units+". "+
    "U/UNDO hoàn tác cả bước; chưa tự lưu DWG. Nhãn là thuộc tính hiển thị gắn vào block: MOVE/COPY mang theo, kéo grip để dời riêng nhãn; ATTSYNC/BATTMAN Sync hoặc EXPLODE có thể xoá/biến đổi nhãn.";
   return res;}
  static TagInfo033 Info(Transaction tr,BlockReference br,AttributeReference a){var st=a.TextStyleId.IsNull?null:tr.GetObject(a.TextStyleId,OpenMode.ForRead) as TextStyleTableRecord;
   return new TagInfo033{ownerHandle=br.Handle.ToString(),tagHandle=a.Handle.ToString(),text=a.TextString,height=a.Height,x=a.Position.X,y=a.Position.Y,z=a.Position.Z,rotation=a.Rotation,
    style=st==null?"":st.Name,layer=a.Layer,colorIndex=a.ColorIndex,invisible=a.Invisible,justify=a.Justify.ToString(),grip=Grip(a)};}
  // Read helper (QA): current owned label of a block (or of the block owning a selected label); null = no label; malformed/collision/duplicate = exception.
  public static TagInfo033 Read(Database db,ObjectId blockOrLabel){using(var tr=db.TransactionManager.StartOpenCloseTransaction()){
    var br=(BlockReference)tr.GetObject(Owner(tr,db,blockOrLabel),OpenMode.ForRead);List<string> attrs;var own=FindOwned(tr,br,"block "+br.Handle+": ",out attrs);
    var r=own==null?null:Info(tr,br,own);tr.Commit();return r;}}
  // QA/diagnostics: every Model block reference with a label (COPY results included) or an unusable TVXD_LABEL_033 attribute (issue says why). Read only.
  public static List<TagInfo033> ScanModel(Database db){var list=new List<TagInfo033>();
   using(var tr=db.TransactionManager.StartOpenCloseTransaction()){
    foreach(ObjectId id in (BlockTableRecord)tr.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db),OpenMode.ForRead)){if(id.IsErased)continue;var br=tr.GetObject(id,OpenMode.ForRead) as BlockReference;if(br==null)continue;
     List<string> attrs;try{var own=FindOwned(tr,br,"block "+br.Handle+": ",out attrs);if(own!=null)list.Add(Info(tr,br,own));}catch(IOE ex){list.Add(new TagInfo033{ownerHandle=br.Handle.ToString(),issue=ex.Message});}}
    tr.Commit();}
   return list;}
 }
#endif
}
#endif
