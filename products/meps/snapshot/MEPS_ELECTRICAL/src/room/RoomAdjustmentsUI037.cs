using System;using System.Linq;using System.Globalization;using System.Collections.Generic;using IOE=System.InvalidOperationException;
#if ROOMCAD037 && BDGKL034
namespace Tvxd.Meps {
 // TVXD-MEPS-ROOMQTY-037 S5 session helpers of the room table (Vietnamese text: /codepage:65001). No Forms, no CAD call. Used by RoomReport037 (the
 // grid cell handlers and the BỎ DÒNG / KHÔI PHỤC / XÁC NHẬN LẠI / THÊM DÒNG buttons call exactly these methods) and by the GPT harness.
 //  - Cell edit: ONE field of ONE row input (exact BdgklInput034 field name). Text is parsed first (blank = null = not declared; 0 is a value; comma
 //    decimal 1,5; no sign/exponent; finite), then only that field changes, then RoomUi037.Recalc (project mode re-applies the project heights, so inherited
 //    heights stay dynamic; nothing becomes a device override). Never builds a RoomDeclaration037 (no ApplyDeclaration with unspecified fields).
 //  - Exclusion / supplement: project mode only. The session's adjustments are copied, the RoomAdjustments037 mutator runs on the copy, and only on success
 //    the copy replaces session.adjustments, followed by Recalc. Any refusal = InvalidOperationException and the session is unchanged.
 //  - Exclusions are captured against the CURRENT complete scan signature; XÁC NHẬN LẠI re-captures an existing record of a row key that is in the current
 //    scan (no matching by label, no remapping); a row key not in the current scan can only be restored.
 public static class RoomAdjustmentsUi037 {
  static readonly StringComparer O=StringComparer.Ordinal;
  // Editable TUYẾN cells (exact BdgklInput034 field names).
  public const string Circuit="circuitId",Routing="routingMode",TailCount="tailCount",TailEach="tailEachLength",Factor="factor",Assembly="assemblyCode",Conduit="conduitCode";
  public static readonly string[] CellFields={Circuit,Routing,TailCount,TailEach,Factor,Assembly,Conduit};
  // Editable BỔ SUNG cells (exact RoomSupplement037 field names).
  public const string SKind="kind",SCode="materialCode",SMetres="metres",SCircuit="circuitId",SDescription="description",SReason="reason";
  public static readonly string[] SupplementFields={SKind,SCode,SMetres,SCircuit,SDescription,SReason};
  // Grid labels (index 0 = blank = not declared) and their exact codes.
  public static readonly string[] RoutingLabels={"","TRẦN","SÀN","TƯỜNG"};
  static readonly string[] RoutingCodes={null,Bdgkl034.Ceiling,Bdgkl034.Floor,Bdgkl034.Wall};
  public static readonly string[] KindLabels={"","DÂY","ỐNG CHÌM","ỐNG NỔI"};
  static readonly string[] KindCodes={null,RoomAdjustments037.Wire,RoomAdjustments037.ConduitHidden,RoomAdjustments037.ConduitExposed};
  public static string RoutingLabel(string code){int k=Array.IndexOf(RoutingCodes,code);return k>=0?RoutingLabels[k]:code;}
  public static string KindLabel(string code){int k=Array.IndexOf(KindCodes,code);return k>=0?KindLabels[k]:code;}
  public static string Caption(string field){
   switch(field){case Circuit:return "KÊNH / LỘ";case Routing:return "HƯỚNG ĐI";case TailCount:return "SỐ ĐẦU CHỜ";case TailEach:return "DÀI 1 ĐẦU CHỜ";
    case Factor:return "HỆ SỐ UỐN CONG";case Assembly:return "BỘ DÂY";case Conduit:return "ỐNG";default:return field;}}
  public static string SupplementCaption(string field){
   switch(field){case SKind:return "LOẠI";case SCode:return "VẬT LIỆU (MÃ)";case SMetres:return "KHỐI LƯỢNG (m)";case SCircuit:return "LỘ";
    case SDescription:return "MÔ TẢ";case SReason:return "LÝ DO";default:return field;}}

  // ---- parsing (exception = refused; nothing changed) ----
  static string Choice(string text,string[] labels,string[] codes,string what){
   if(string.IsNullOrEmpty(text))return null;
   int k=Array.IndexOf(labels,text);if(k>0)return codes[k];
   k=Array.IndexOf(codes,text);if(k>0)return codes[k];
   throw new IOE(what+" \""+text+"\" không hợp lệ: chọn "+string.Join(" / ",labels.Skip(1))+" hoặc để trống.");}
  // Code text (circuit / assembly / conduit / material): blank = null; otherwise the exact Bdgkl034 text rule (nothing trimmed).
  static string Code(string text,string what){if(string.IsNullOrEmpty(text))return null;var p=Bdgkl034.TextProblem(text);if(p!=null)throw new IOE(what+" \""+text+"\" "+p+".");return text;}
  static string Note(string text,string what){if(string.IsNullOrEmpty(text))return null;var p=RoomAdjustments037.NoteProblem(text,what);if(p!=null)throw new IOE(p);return text;}
  // One TUYẾN cell text → typed value (string / int? / double? boxed; null = blank).
  public static object ParseCell(string field,string text){
   switch(field){
    case Circuit:case Assembly:case Conduit:return Code(text,Caption(field));
    case Routing:return Choice(text,RoutingLabels,RoutingCodes,"Hướng đi");
    case TailCount:return BdgklUi034.ParseCount(text,"Số đầu chờ");
    case TailEach:return BdgklUi034.ParseDecimal(text,"Dài một đầu chờ");
    case Factor:return BdgklUi034.ParseDecimal(text,"Hệ số uốn cong");
    default:throw new IOE("Cột \""+field+"\" không sửa được trong bảng.");}}
  // null = acceptable text; otherwise the reason (used by the grid's CellValidating).
  public static string CellProblem(string field,string text){try{ParseCell(field,text);return null;}catch(IOE ex){return ex.Message;}}
  public static object CellValue(BdgklInput034 i,string field){
   switch(field){case Circuit:return i.circuitId;case Routing:return i.routingMode;case TailCount:return i.tailCount;case TailEach:return i.tailEachLength;
    case Factor:return i.factor;case Assembly:return i.assemblyCode;case Conduit:return i.conduitCode;default:throw new IOE("Cột \""+field+"\" không sửa được trong bảng.");}}
  // Grid text of a value: blank for null, comma decimal with full round-trip precision (display never changes the stored value).
  public static string CellText(BdgklInput034 i,string field){if(i==null)return "";var v=CellValue(i,field);if(v==null)return "";
   if(field==Routing)return RoutingLabel((string)v);if(v is double)return RoomLook037.Exact((double)v);if(v is int)return ((int)v).ToString(CultureInfo.InvariantCulture);return (string)v;}
  // 045: also used by the atomic bulk apply (RoomFastEntryUi045) after the whole target was parsed and validated.
  public static void SetCell(BdgklInput034 i,string field,object v){
   switch(field){case Circuit:i.circuitId=(string)v;break;case Routing:i.routingMode=(string)v;break;case TailCount:i.tailCount=(int?)v;break;
    case TailEach:i.tailEachLength=(double?)v;break;case Factor:i.factor=(double?)v;break;case Assembly:i.assemblyCode=(string)v;break;case Conduit:i.conduitCode=(string)v;break;
    default:throw new IOE("Cột \""+field+"\" không sửa được trong bảng.");}}

  // ---- TUYẾN cell edit (both modes) ----
  // Parses, then changes ONLY this field of this row, then Recalc. null = same value (nothing changed, session not marked dirty).
  // 045 PATCH1 F02: an accepted typed value EQUAL to the inherited (non-RIÊNG) value of an inheritable field is the user's own value: it becomes
  // RIÊNG (dirty, Recalc, message = one undoable change). Already RIÊNG / legacy mode / a blank over a missing (CHƯA CÓ) cell = unchanged (null).
  // Callers reach this only with an accepted edit (the grid gates on actual typing, see RoomReport037.SameTyped045); focus / leave / Esc never call it.
  public static string ApplyCell(RoomSession037 s,string rowKey,string field,string text){
   if(s==null||s.batch==null)throw new IOE("Chưa có phòng đang mở. Chưa đổi gì.");
   var row=s.batch.rows.FirstOrDefault(q=>O.Equals(q.rowKey,rowKey));
   if(row==null||row.input==null)throw new IOE("Dòng "+rowKey+" không còn trong phòng; bấm 'Cập nhật'. Chưa đổi gì.");
   object v;try{v=ParseCell(field,text);}catch(IOE ex){throw new IOE(ex.Message+" Giá trị cũ giữ nguyên.");}
   if(Equals(CellValue(row.input,field),v)){
    if(v==null||!RoomFastEntryUi045.Inherited(s,rowKey,field))return null;
    RoomFastEntryUi045.MarkExplicit(s,rowKey,field);s.dirty=true;RoomUi037.Recalc(s);
    return "Đã giữ "+Caption(field)+" tuyến "+row.startLabel+" → "+row.endLabel+" = "+CellText(row.input,field)+" làm giá trị RIÊNG (không còn kế thừa mẫu / mặc định). Chưa lưu vào bản vẽ. "+RoomUi037.Summary(s)+".";}
   SetCell(row.input,field,v);
   RoomFastEntryUi045.MarkExplicit(s,rowKey,field);   // 045: a typed value (also a typed blank) is the row's own value (RIÊNG), never re-inherited
   s.dirty=true;RoomUi037.Recalc(s);
   return "Đã sửa "+Caption(field)+" tuyến "+row.startLabel+" → "+row.endLabel+" = "+(v==null?"(trống)":CellText(row.input,field))+". Chưa lưu vào bản vẽ. "+RoomUi037.Summary(s)+".";}

  // ---- BỎ DÒNG / KHÔI PHỤC / XÁC NHẬN LẠI (project mode) ----
  static void Ready(RoomSession037 s,string what){
   if(s==null)throw new ArgumentNullException("s");
   if(!s.projectMode)throw new IOE(what+" chỉ dùng trong chế độ dự án. Chưa đổi gì.");
   if(s.batch==null||s.scan==null)throw new IOE("Chọn vùng CAD (quét phòng) trước. Chưa đổi gì.");}
  static RoomAdjustmentsData037 Working(RoomSession037 s){return RoomAdjustments037.Copy(s.adjustments)??new RoomAdjustmentsData037();}
  static void Swap(RoomSession037 s,RoomAdjustmentsData037 d){s.adjustments=d;s.dirty=true;RoomUi037.Recalc(s);}
  static RoomCompletionRow037 Row(RoomSession037 s,string rowKey){return s.batch.rows.FirstOrDefault(q=>q.rowKey!=null&&O.Equals(q.rowKey,rowKey));}
  static string RouteText(RoomCompletionRow037 r,string rowKey){return r==null?"khoá "+rowKey:r.startLabel+" → "+r.endLabel;}
  public static RoomExclusion037 Exclusion(RoomSession037 s,string rowKey){
   return s==null||s.adjustments==null||s.adjustments.exclusions==null?null:s.adjustments.exclusions.FirstOrDefault(e=>e!=null&&O.Equals(e.rowKey,rowKey));}
  public static RoomExclusionState037 ExclusionState(RoomSession037 s,string rowKey){
   return s==null||s.batch==null||s.batch.exclusionStates==null?null:s.batch.exclusionStates.FirstOrDefault(e=>e!=null&&O.Equals(e.rowKey,rowKey));}
  // BỎ DÒNG: source row of the current complete scan, reason required. Geometry untouched; the row stays visible as ĐÃ BỎ DÒNG with its inputs.
  public static string Exclude(RoomSession037 s,string rowKey,string reason){Ready(s,"BỎ DÒNG");
   if(s.scan.signature==null)throw new IOE("Lần quét chưa hoàn tất (vướng mắc CAD); chỉ bỏ dòng được trên một lần quét hoàn chỉnh. Chưa đổi gì.");
   var r=Row(s,rowKey);if(r==null)throw new IOE("Dòng "+rowKey+" không có trong lần quét hiện tại. Chưa đổi gì.");
   if(string.IsNullOrWhiteSpace(reason))throw new IOE("Nhập LÝ DO BỎ DÒNG (bắt buộc). Chưa đổi gì.");
   var d=Working(s);
   try{RoomAdjustments037.Exclude(d,rowKey,s.scan.signature,reason);}catch(IOE ex){throw new IOE("Chưa bỏ dòng: "+ex.Message);}
   Swap(s,d);
   return "ĐÃ BỎ DÒNG tuyến "+RouteText(r,rowKey)+" (lý do: "+reason+"). Hình CAD không đổi; KHÔI PHỤC để tính lại. Chưa lưu vào bản vẽ. "+RoomUi037.Summary(s)+".";}
  // KHÔI PHỤC: record kept for audit (inactive); also the explicit resolution of a stale / unmatched record.
  public static string Restore(RoomSession037 s,string rowKey){Ready(s,"KHÔI PHỤC");
   var e=Exclusion(s,rowKey);if(e==null)throw new IOE("Dòng "+rowKey+" chưa bị bỏ. Chưa đổi gì.");
   if(!e.active)throw new IOE("Dòng "+rowKey+" đã được khôi phục trước đó. Chưa đổi gì.");
   var d=Working(s);
   try{RoomAdjustments037.Restore(d,rowKey);}catch(IOE ex){throw new IOE("Chưa khôi phục: "+ex.Message);}
   Swap(s,d);
   return "ĐÃ KHÔI PHỤC tuyến "+RouteText(Row(s,rowKey),rowKey)+" (bản ghi bỏ dòng giữ lại để đối chiếu). Chưa lưu vào bản vẽ. "+RoomUi037.Summary(s)+".";}
  // XÁC NHẬN LẠI: an existing record whose signature differs from the current complete scan is captured again on THIS scan (same exact row key; reason =
  // the typed one, else the recorded one). A row key that the current scan does not produce cannot be re-confirmed (only KHÔI PHỤC).
  public static string Reconfirm(RoomSession037 s,string rowKey,string reason){Ready(s,"XÁC NHẬN LẠI");
   if(s.scan.signature==null)throw new IOE("Lần quét chưa hoàn tất (vướng mắc CAD); chưa xác nhận lại được. Chưa đổi gì.");
   var e=Exclusion(s,rowKey);if(e==null)throw new IOE("Dòng "+rowKey+" không có bản ghi bỏ dòng. Chưa đổi gì.");
   var r=Row(s,rowKey);
   if(r==null)throw new IOE("Khoá dòng "+rowKey+" không còn trong lần quét hiện tại; MEPS không ghép theo nhãn. Chỉ KHÔI PHỤC được bản ghi này. Chưa đổi gì.");
   string why=string.IsNullOrWhiteSpace(reason)?e.reason:reason;
   var d=Working(s);
   try{RoomAdjustments037.Exclude(d,rowKey,s.scan.signature,why);}catch(IOE ex){throw new IOE("Chưa xác nhận lại: "+ex.Message);}
   Swap(s,d);
   return "ĐÃ XÁC NHẬN LẠI việc bỏ tuyến "+RouteText(r,rowKey)+" trên lần quét hiện tại (lý do: "+why+"). Chưa lưu vào bản vẽ. "+RoomUi037.Summary(s)+".";}

  // ---- BỔ SUNG (project mode; stable room ID of the open room) ----
  public static RoomSupplement037 Supplement(RoomSession037 s,string id){
   return s==null||s.adjustments==null||s.adjustments.supplements==null?null:s.adjustments.supplements.FirstOrDefault(x=>x!=null&&O.Equals(x.id,id));}
  public static RoomSupplementState037 SupplementState(RoomSession037 s,string id){
   return s==null||s.batch==null||s.batch.supplementStates==null?null:s.batch.supplementStates.FirstOrDefault(x=>x!=null&&O.Equals(x.id,id));}
  // THÊM DÒNG: new blank active record (unit m) of the open room; blank values keep the room MISSING_DATA until entered.
  public static string AddSupplement(RoomSession037 s,Func<Guid> ids,out string id){id=null;Ready(s,"THÊM DÒNG");
   if(ids==null)throw new ArgumentNullException("ids");
   var d=Working(s);RoomSupplement037 n;
   try{n=RoomAdjustments037.AddSupplement(d,s.roomId,ids);}catch(IOE ex){throw new IOE("Chưa thêm dòng: "+ex.Message);}
   Swap(s,d);id=n.id;
   return "Đã THÊM DÒNG bổ sung (đơn vị m): nhập LOẠI, VẬT LIỆU, KHỐI LƯỢNG, MÔ TẢ và LÝ DO. Chưa lưu vào bản vẽ. "+RoomUi037.Summary(s)+".";}
  public static object ParseSupplement(string field,string text){
   switch(field){
    case SKind:return Choice(text,KindLabels,KindCodes,"Loại");
    case SCode:return Code(text,"Vật liệu");
    case SCircuit:return Code(text,"Lộ");
    case SMetres:return BdgklUi034.ParseDecimal(text,"Khối lượng");
    case SDescription:return Note(text,"Mô tả");
    case SReason:return Note(text,"Lý do");
    default:throw new IOE("Cột \""+field+"\" không sửa được.");}}
  public static string SupplementProblem(string field,string text){try{ParseSupplement(field,text);return null;}catch(IOE ex){return ex.Message;}}
  public static string SupplementText(RoomSupplement037 x,string field){if(x==null)return "";
   switch(field){case SKind:return KindLabel(x.kind)??"";case SCode:return x.materialCode??"";case SMetres:return RoomLook037.Exact(x.metres);case SCircuit:return x.circuitId??"";
    case SDescription:return x.description??"";case SReason:return x.reason??"";default:return "";}}
  // One field of one supplement; every other value is passed back unchanged (UpdateSupplement). A changed kind/code clears the overlap confirmation (core).
  public static string EditSupplement(RoomSession037 s,string id,string field,string text){Ready(s,"BỔ SUNG");
   var x=Supplement(s,id);if(x==null)throw new IOE("Dòng bổ sung "+id+" không có. Chưa đổi gì.");
   object v;try{v=ParseSupplement(field,text);}catch(IOE ex){throw new IOE(ex.Message+" Giá trị cũ giữ nguyên.");}
   string kind=x.kind,code=x.materialCode,circuit=x.circuitId,desc=x.description,why=x.reason;double? m=x.metres;
   switch(field){case SKind:kind=(string)v;break;case SCode:code=(string)v;break;case SCircuit:circuit=(string)v;break;case SMetres:m=(double?)v;break;
    case SDescription:desc=(string)v;break;default:why=(string)v;break;}
   if(O.Equals(kind,x.kind)&&O.Equals(code,x.materialCode)&&O.Equals(circuit,x.circuitId)&&Nullable.Equals(m,x.metres)&&O.Equals(desc,x.description)&&O.Equals(why,x.reason))return null;
   bool wasConfirmed=x.overlapConfirmed;
   var d=Working(s);RoomSupplement037 n;
   try{n=RoomAdjustments037.UpdateSupplement(d,s.roomId,id,kind,code,m,circuit,desc,why);}catch(IOE ex){throw new IOE("Chưa sửa dòng bổ sung: "+ex.Message);}
   Swap(s,d);
   return "Đã sửa "+SupplementCaption(field)+" của dòng bổ sung"+(wasConfirmed&&!n.overlapConfirmed?" (đổi loại/vật liệu: xác nhận cộng thêm đã bỏ, xem lại)":"")+". Chưa lưu vào bản vẽ. "+RoomUi037.Summary(s)+".";}
  // XOÁ DÒNG (inactive, reversible) / KHÔI PHỤC DÒNG. Record and values kept.
  public static string SetSupplementActive(RoomSession037 s,string id,bool active){Ready(s,active?"KHÔI PHỤC DÒNG":"XOÁ DÒNG");
   var x=Supplement(s,id);if(x==null)throw new IOE("Dòng bổ sung "+id+" không có. Chưa đổi gì.");
   if(x.active==active)throw new IOE(active?"Dòng bổ sung đang dùng. Chưa đổi gì.":"Dòng bổ sung đã xoá (không tính) trước đó. Chưa đổi gì.");
   var d=Working(s);
   try{RoomAdjustments037.SetSupplementActive(d,s.roomId,id,active);}catch(IOE ex){throw new IOE("Chưa đổi dòng bổ sung: "+ex.Message);}
   Swap(s,d);
   return (active?"Đã KHÔI PHỤC dòng bổ sung.":"Đã XOÁ dòng bổ sung khỏi tổng (bản ghi giữ lại, KHÔI PHỤC DÒNG để tính lại).")+" Chưa lưu vào bản vẽ. "+RoomUi037.Summary(s)+".";}
  // XÁC NHẬN CỘNG THÊM: explicit decision after the user reviewed that the same kind/material is also measured from CAD in this room. Confirming is only
  // possible while the calculated state reports that overlap; removing a confirmation is always possible.
  public static string ConfirmOverlap(RoomSession037 s,string id,bool confirmed){Ready(s,"XÁC NHẬN CỘNG THÊM");
   var x=Supplement(s,id);if(x==null)throw new IOE("Dòng bổ sung "+id+" không có. Chưa đổi gì.");
   if(x.overlapConfirmed==confirmed)throw new IOE(confirmed?"Dòng bổ sung đã được xác nhận cộng thêm. Chưa đổi gì.":"Dòng bổ sung chưa được xác nhận. Chưa đổi gì.");
   var st=SupplementState(s,id);
   if(confirmed&&(st==null||!st.overlap))throw new IOE("Dòng bổ sung này không trùng loại/vật liệu đo từ CAD của phòng; không cần xác nhận. Chưa đổi gì.");
   var d=Working(s);
   try{RoomAdjustments037.ConfirmOverlap(d,s.roomId,id,confirmed);}catch(IOE ex){throw new IOE("Chưa xác nhận: "+ex.Message);}
   Swap(s,d);
   return (confirmed?"Đã XÁC NHẬN khối lượng bổ sung là CỘNG THÊM vào phần đo CAD cùng vật liệu (không tự gộp/trừ).":"Đã bỏ xác nhận cộng thêm; dòng bổ sung cần kiểm tra lại.")+
    " Chưa lưu vào bản vẽ. "+RoomUi037.Summary(s)+".";}

  // ---- session ownership / labels ----
  // Draft value: deep copy; nothing declared = null (legacy-equivalent draft).
  public static RoomAdjustmentsData037 ForDraft(RoomAdjustmentsData037 d){
   if(d==null||(d.exclusions!=null&&d.exclusions.Count==0&&d.supplements!=null&&d.supplements.Count==0&&d.version==RoomAdjustments037.Version))return null;
   return RoomAdjustments037.Copy(d);}
  public static string ExclusionStatusLabel(string st){
   return st==RoomAdjustments037.Applied?"ĐANG BỎ":st==RoomAdjustments037.Inactive?"ĐÃ KHÔI PHỤC":st==RoomAdjustments037.SignatureMismatch?"HÌNH ĐÃ ĐỔI – CẦN XÁC NHẬN LẠI HOẶC KHÔI PHỤC":
    st==RoomAdjustments037.Unmatched?"KHÔNG CÒN TRONG BẢN VẼ – CẦN KHÔI PHỤC":"CHƯA TÍNH (dữ liệu bỏ dòng/bổ sung không hợp lệ)";}
  // Short text appended to the room summary when the room has adjustments (empty otherwise, so earlier summaries are unchanged).
  public static string SummarySuffix(RoomSession037 s){if(s==null||s.batch==null||s.adjustments==null)return "";var b=s.batch;
   int stale=b.exclusionStates.Count(e=>e.status==RoomAdjustments037.SignatureMismatch||e.status==RoomAdjustments037.Unmatched);
   int sup=b.supplementStates.Count(x=>x.status!=RoomAdjustments037.Inactive),supOk=b.supplementStates.Count(x=>x.status==RoomCompletion037.Pass);
   if(b.excluded==0&&stale==0&&sup==0)return "";
   return "; bỏ "+b.excluded+" dòng"+(stale>0?" ("+stale+" bản ghi bỏ dòng cần xác nhận lại/khôi phục)":"")+"; bổ sung "+supOk+"/"+sup+" dòng đủ";}
 }
}
#endif
