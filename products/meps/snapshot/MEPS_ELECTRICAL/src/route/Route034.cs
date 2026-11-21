using System;using System.Linq;using System.Collections.Generic;using System.Globalization;using System.Text;using System.Text.RegularExpressions;using System.Security.Cryptography;using IOE=System.InvalidOperationException;
#if ROUTE034 && !ROUTE034_PURE
using Autodesk.AutoCAD.DatabaseServices;using Autodesk.AutoCAD.Geometry;
#endif
#if ROUTE034 && !ROUTE034_PURE && !(IDENTITY015 && CABLES016)
#error ROUTE034 (native part) needs IDENTITY015 and CABLES016 (binding to the cable type of an existing route).
#endif
#if ROUTE034
namespace Tvxd.Meps {
 // TVXD-MEPS-WIREQTY-034 core (UI: RouteUI034.cs; display/parse helpers and RouteCsv034 are pure): quantity of wire/cable along a drawn route, from values the USER declares for that route.
 //  - Declared per route: area, circuit (exact text, no case folding/trim), Cable016 catalog code, basis WIRE_M (metres of wire) or CABLE_M (metres of cable),
 //    parallel run count, extra vertical metres, reserve percent, declared drawing unit MM (1000 units per metre) or M (1 unit per metre). Every value is
 //    required; 0 extra/reserve only when typed. Nothing is inferred: no ampacity, load, section, topology, conduit, price or engineering allowance.
 //  - PVC catalog entries have UNKNOWN core count (Cable016 cores 0): the user must choose WIRE_M or CABLE_M; no single-core default. XLPE 2/4-core entries
 //    are CABLE_M only. The core count never multiplies anything; the catalog code/type is never changed.
 //  - quantity (m) = (live route length / unit factor + extraVerticalMeters) × parallelCount × (1 + reservePercent/100). Not rounded here. Non-finite
 //    input or overflow is refused. Length and quantity are NEVER stored: every read measures the live geometry.
 // Pure part (compiled with /define:ROUTE034;ROUTE034_PURE together with Cable016.cs/Standards012.cs in their pure mode): DTOs, validation, strict
 // Encode/Decode, Calculate, grouping. AutoCAD part: RouteCad034.
 public sealed class RouteConfig034 {public string area,circuit,code,basis,declaredUnit;public int parallelCount;public double extraVerticalMeters,reservePercent;
  public bool SameAs(RouteConfig034 o){return o!=null&&Route034.EncodeConfig(this).SequenceEqual(Route034.EncodeConfig(o),StringComparer.Ordinal);}
  public RouteConfig034 Copy(){return (RouteConfig034)MemberwiseClone();}}
 // Integrity stamps taken when the user confirmed the route: INSUNITS, the entity's own handle, the drawing FingerprintGuid and a SHA-256 of the full geometry.
 public sealed class RouteStamp034 {public int insunits;public string ownerHandle,fingerprint,geometryHash;}
 public sealed class RouteRecord034 {public RouteConfig034 config;public RouteStamp034 stamp;}
 public sealed class RouteQty034 {public double lengthDrawingUnits,routeMeters,runMeters,quantity;}
 // One route of a scan/read. status: OK (counted), INCOMPLETE (recognized Cable016 route without setup), INVALID, CONFLICT, RECONFIRM_REQUIRED (not counted;
 // reason says why). liveLength (drawing units) / routeMeters are measured now whenever the geometry is supported; quantity only for OK.
 public sealed class RouteRow034 {public string handle,type,layer,status,reason,warning,knownCode,geometryHash;public double? liveLength,routeMeters,quantity;public RouteConfig034 config;
#if !ROUTE034_PURE
  public ObjectId id;
#endif
 }
 // Total per (area, circuit, code, basis) with the original display values; OK rows only.
 public sealed class RouteGroup034 {public string area,circuit,code,basis;public int routes;public double routeMeters,quantity;}
 public sealed class RouteScan034 {public readonly List<RouteRow034> rows=new List<RouteRow034>();public readonly List<string> warnings=new List<string>();
  public int Count(string status){return rows.Count(r=>r.status==status);}
  public List<RouteGroup034> Groups(){return Route034.Groups(rows);}
  public string Summary(){return "Model có "+rows.Count+" tuyến dây/cáp MEPS: "+Count(Route034.Ok)+" tính khối lượng, "+Count(Route034.Incomplete)+" chưa khai báo, "+
   Count(Route034.Reconfirm)+" cần xác nhận lại, "+Count(Route034.Conflict)+" xung đột mã cáp, "+Count(Route034.Invalid)+" không hợp lệ (không tính)"+(warnings.Count>0?"; "+warnings.Count+" cảnh báo":"")+".";}}
 public static class Route034 {
  public const string Key="TVXD_MEPS_ROUTE_034",Schema="TVXD_MEPS_ROUTE",Version="034.1";
  public const string WireM="WIRE_M",CableM="CABLE_M",UnitMm="MM",UnitM="M";
  public const string Ok="OK",Incomplete="INCOMPLETE",Invalid="INVALID",Conflict="CONFLICT",Reconfirm="RECONFIRM_REQUIRED";
  public const int MaxText=100,ConfigFields=8,Fields=14;
  static readonly CultureInfo Inv=CultureInfo.InvariantCulture;
  static readonly Regex HandleRx=new Regex(@"\A[0-9A-Fa-f]{1,16}\z"),HashRx=new Regex(@"\A[0-9A-F]{64}\z");
  public const string Rule="Khu vực và mạch/lộ: 1–100 ký tự trên một dòng, giữ đúng như nhập (tiếng Việt, dấu câu được); không có khoảng trắng đầu/cuối, "+
   "không xuống dòng hay ký tự điều khiển. MEPS không tự viết hoa, không tự cắt.";
  static string Short(string s){return s==null?"null":s.Length>24?s.Substring(0,24)+"…":s;}
  static bool Fin(double v){return !double.IsNaN(v)&&!double.IsInfinity(v);}
  public static string N(double v){return v.ToString("R",Inv);}
  // Returns the text exactly as given, or throws (nothing trimmed or case-folded).
  public static string CheckText(string s,string what){
   if(s==null||s.Length==0)throw new IOE("Chưa nhập "+what+". "+Rule);
   if(s.Length>MaxText)throw new IOE(what+" dài "+s.Length+" ký tự (tối đa "+MaxText+"). "+Rule);
   if(s[0]==' '||s[s.Length-1]==' ')throw new IOE(what+" \""+Short(s)+"\" có khoảng trắng đầu/cuối; MEPS không tự cắt. "+Rule);
   foreach(char c in s){if(c==' ')continue;if(char.IsSurrogate(c))throw new IOE(what+" \""+Short(s)+"\" có ký tự ngoài bảng chữ cơ bản (emoji…). "+Rule);
    switch(char.GetUnicodeCategory(c)){case UnicodeCategory.Control:case UnicodeCategory.Format:case UnicodeCategory.LineSeparator:case UnicodeCategory.ParagraphSeparator:
     case UnicodeCategory.SpaceSeparator:case UnicodeCategory.PrivateUse:case UnicodeCategory.OtherNotAssigned:
      throw new IOE(what+" \""+Short(s)+"\" có ký tự điều khiển/khoảng trắng đặc biệt U+"+((int)c).ToString("X4",Inv)+". "+Rule);}}
   return s;}
  // Drawing units per metre of the DECLARED unit (the user's choice; INSUNITS is only compared for a warning).
  public static double Factor(string unit){if(unit==UnitMm)return 1000;if(unit==UnitM)return 1;
   throw new IOE("Chưa chọn đơn vị bản vẽ khai báo (MM = 1000 đơn vị/m hoặc M = 1 đơn vị/m); nhận \""+Short(unit)+"\". MEPS không tự giả định.");}
  public static string BasisCaption(string b){return b==WireM?"mét dây (WIRE_M)":b==CableM?"mét cáp (CABLE_M)":"(chưa chọn)";}
  static double Amount(double? v,string what){if(!v.HasValue)throw new IOE("Chưa nhập "+what+" (nhập 0 nếu không có); MEPS không tự điền.");double x=v.Value;
   if(!Fin(x)||x<0)throw new IOE(what+" phải là số hữu hạn ≥ 0; nhận "+N(x)+".");return x==0?0.0:x;}
  // A complete, validated configuration from user input; any missing/invalid value = exception (nothing written).
  public static RouteConfig034 Make(string area,string circuit,string code,string basis,int? parallelCount,double? extraVerticalMeters,double? reservePercent,string declaredUnit){
   var r=new RouteConfig034{area=CheckText(area,"khu vực"),circuit=CheckText(circuit,"mạch/lộ")};
   if(string.IsNullOrEmpty(code))throw new IOE("Chưa chọn mã cáp trong danh mục 016.");
   var c=Cable016.ByCode(code);if(c==null)throw new IOE("Mã cáp \""+Short(code)+"\" không thuộc danh mục 016 (so khớp đúng chữ hoa/thường); MEPS không tự sửa.");r.code=c.code;
   if(basis!=WireM&&basis!=CableM)throw new IOE("Chưa chọn cách tính: "+BasisCaption(WireM)+" hoặc "+BasisCaption(CableM)+"; MEPS không mặc định.");
   if(c.CoresKnown&&basis!=CableM)throw new IOE("Cáp "+c.label+" ("+c.cores.ToString(Inv)+" lõi) chỉ tính theo "+BasisCaption(CableM)+"; MEPS không nhân số lõi.");
   r.basis=basis;
   if(!parallelCount.HasValue)throw new IOE("Chưa nhập số tuyến song song.");if(parallelCount.Value<1)throw new IOE("Số tuyến song song phải là số nguyên ≥ 1; nhận "+parallelCount.Value.ToString(Inv)+".");
   r.parallelCount=parallelCount.Value;
   r.extraVerticalMeters=Amount(extraVerticalMeters,"chiều dài lên/xuống thêm (m)");r.reservePercent=Amount(reservePercent,"dự phòng (%)");
   Factor(declaredUnit);r.declaredUnit=declaredUnit;return r;}
  // A stored/handed-in configuration must already be exactly valid; nothing is repaired. Returns a validated copy.
  public static RouteConfig034 Check(RouteConfig034 c){if(c==null)throw new IOE("Thiếu thông số tuyến.");
   var again=Make(c.area,c.circuit,c.code,c.basis,c.parallelCount,c.extraVerticalMeters,c.reservePercent,c.declaredUnit);
   if(!again.SameAs(c))throw new IOE("thông số tuyến chưa ở dạng chuẩn");return again;}
  // (L / factor + extra) × count × (1 + reserve/100), in this order; every step must stay finite (overflow refused), the route length positive.
  public static RouteQty034 Calculate(double lengthDrawingUnits,RouteConfig034 config){var c=Check(config);
   if(!Fin(lengthDrawingUnits)||!(lengthDrawingUnits>0))throw new IOE("Chiều dài tuyến "+N(lengthDrawingUnits)+" không phải số dương hữu hạn; không tính.");
   double m=lengthDrawingUnits/Factor(c.declaredUnit);if(!Fin(m)||!(m>0))throw new IOE("Chiều dài quy đổi ra mét không hợp lệ ("+N(m)+"); không tính.");
   double run=m+c.extraVerticalMeters;if(!Fin(run))throw new IOE("Chiều dài một tuyến vượt giới hạn số; không tính.");
   double q=run*c.parallelCount;if(!Fin(q))throw new IOE("Khối lượng vượt giới hạn số (nhân số tuyến); không tính.");
   double f=1+c.reservePercent/100;if(!Fin(f))throw new IOE("Hệ số dự phòng vượt giới hạn số; không tính.");
   q=q*f;if(!Fin(q))throw new IOE("Khối lượng vượt giới hạn số (nhân dự phòng); không tính.");
   return new RouteQty034{lengthDrawingUnits=lengthDrawingUnits,routeMeters=m,runMeters=run,quantity=q};}
  // Warning only (never refuses): the declared unit does not match INSUNITS mm(4)/m(6), or INSUNITS is another/unset unit.
  public static string UnitWarning(int insunits,string unit){
   if((insunits==4&&unit==UnitMm)||(insunits==6&&unit==UnitM))return null;
   return "Đơn vị khai báo "+unit+" khác INSUNITS = "+insunits.ToString(Inv)+(insunits==4?" (mm)":insunits==6?" (m)":insunits==0?" (chưa đặt đơn vị)":"")+"; kiểm tra lại. MEPS dùng đúng đơn vị đã khai báo, không tự đổi.";}
  public static string Hash(string s){using(var h=SHA256.Create()){var b=h.ComputeHash(Encoding.UTF8.GetBytes(s??""));var sb=new StringBuilder(64);foreach(var x in b)sb.Append(x.ToString("X2",Inv));return sb.ToString();}}
  // ---- stored layout: [Schema, Version, area, circuit, code, basis, parallelCount, extraVerticalMeters (R), reservePercent (R), declaredUnit,
  //      insunits, ownerHandle, fingerprint, geometryHash], text only ----
  public static string[] EncodeConfig(RouteConfig034 c){return new[]{c.area,c.circuit,c.code,c.basis,c.parallelCount.ToString(Inv),N(c.extraVerticalMeters),N(c.reservePercent),c.declaredUnit};}
  public static void CheckStamp(RouteStamp034 s){if(s==null)throw new IOE("Thiếu dấu xác nhận tuyến.");Guid g;
   if(s.insunits<0)throw new IOE("INSUNITS lưu không hợp lệ");
   if(s.ownerHandle==null||!HandleRx.IsMatch(s.ownerHandle))throw new IOE("handle lưu không hợp lệ");
   if(s.fingerprint==null||s.fingerprint.Length>64||!Guid.TryParse(s.fingerprint,out g))throw new IOE("FingerprintGuid lưu không hợp lệ");
   if(s.geometryHash==null||!HashRx.IsMatch(s.geometryHash))throw new IOE("dấu hình học lưu không hợp lệ");}
  public static string[] Encode(RouteRecord034 r){if(r==null)throw new IOE("Thiếu bản ghi tuyến.");var c=Check(r.config);CheckStamp(r.stamp);
   return new[]{Schema,Version}.Concat(EncodeConfig(c)).Concat(new[]{r.stamp.insunits.ToString(Inv),r.stamp.ownerHandle,r.stamp.fingerprint,r.stamp.geometryHash}).ToArray();}
  static IOE Bad(string why){return new IOE("Dữ liệu tuyến "+Key+" không hợp lệ ("+why+"); MEPS không ghi đè.");}
  // Strict read: exact field count, text only, schema/version, every value valid AND in canonical form (re-encoding gives the same strings). Else exception.
  public static RouteRecord034 Decode(string[] v){if(v==null||v.Length!=Fields||v.Any(x=>x==null))throw Bad("số trường hoặc kiểu dữ liệu sai");
   if(v[0]!=Schema)throw Bad("schema "+Short(v[0]));if(v[1]!=Version)throw Bad("phiên bản "+Short(v[1])+" chưa hỗ trợ; bản này đọc phiên bản "+Version);
   int count,iu;double extra,reserve;
   if(!int.TryParse(v[6],NumberStyles.None,Inv,out count))throw Bad("số tuyến "+Short(v[6]));
   if(!double.TryParse(v[7],NumberStyles.Float,Inv,out extra))throw Bad("chiều dài thêm "+Short(v[7]));
   if(!double.TryParse(v[8],NumberStyles.Float,Inv,out reserve))throw Bad("dự phòng "+Short(v[8]));
   if(!int.TryParse(v[10],NumberStyles.None,Inv,out iu))throw Bad("INSUNITS "+Short(v[10]));
   var r=new RouteRecord034{config=new RouteConfig034{area=v[2],circuit=v[3],code=v[4],basis=v[5],parallelCount=count,extraVerticalMeters=extra,reservePercent=reserve,declaredUnit=v[9]},
    stamp=new RouteStamp034{insunits=iu,ownerHandle=v[11],fingerprint=v[12],geometryHash=v[13]}};
   string[] again;try{again=Encode(r);}catch(IOE ex){throw Bad(ex.Message);}
   if(!again.SequenceEqual(v,StringComparer.Ordinal))throw Bad("giá trị không ở dạng chuẩn");return r;}
  // F034-AGG01: a group total is summed in full precision and every partial sum must stay finite; an overflow refuses the WHOLE grouping (exception, never
  // Infinity in the report or CSV). Basis is part of the key: metres of wire and metres of cable are never added together.
  static double Total(IEnumerable<double> values,string what){double t=0;
   foreach(var v in values){if(!Fin(v))throw new IOE("Giá trị "+what+" không hữu hạn ("+N(v)+"); không lập bảng tổng.");t+=v;
    if(!Fin(t))throw new IOE("Tổng "+what+" vượt giới hạn số; không lập bảng tổng (kiểm tra tuyến có số tuyến/dự phòng/chiều dài bất thường).");}
   return t;}
  public static List<RouteGroup034> Groups(IEnumerable<RouteRow034> rows){
   return (rows??Enumerable.Empty<RouteRow034>()).Where(r=>r.status==Ok&&r.config!=null&&r.quantity.HasValue&&r.routeMeters.HasValue)
    .GroupBy(r=>Tuple.Create(r.config.area,r.config.circuit,r.config.code,r.config.basis))
    .Select(g=>new RouteGroup034{area=g.Key.Item1,circuit=g.Key.Item2,code=g.Key.Item3,basis=g.Key.Item4,routes=g.Count(),
     routeMeters=Total(g.Select(r=>r.routeMeters.Value),"chiều dài tuyến (m)"),quantity=Total(g.Select(r=>r.quantity.Value),"khối lượng (m)")})
    .OrderBy(g=>g.area,StringComparer.Ordinal).ThenBy(g=>g.circuit,StringComparer.Ordinal).ThenBy(g=>g.code,StringComparer.Ordinal).ThenBy(g=>g.basis,StringComparer.Ordinal).ToList();}
  // ---- display and input (pure, UI 034) ----
  // Display only: metres with ONE decimal, Vietnamese decimal comma, Math.Round(v,1,MidpointRounding.AwayFromZero), no thousands separator, no paper/plot
  // scale. 1540 mm → "1,5"; 1550 mm → "1,6"; 1560 mm → "1,6". Storage, calculation and sums keep full precision: round the TOTAL, never add rounded values
  // (1.54 + 1.54 → "3,1", not "3,0"). Non-finite = exception.
  static readonly NumberFormatInfo ViNum=new NumberFormatInfo{NumberDecimalSeparator=",",NumberGroupSeparator="",NegativeSign="-"};
  public static string Meters(double meters){if(!Fin(meters))throw new IOE("Giá trị mét "+N(meters)+" không hữu hạn.");
   double r=Math.Round(meters,1,MidpointRounding.AwayFromZero);if(r==0)r=0.0;return r.ToString("0.0",ViNum);}
  // Display only: drawing units with up to 3 decimals and a decimal comma (the full value is used for every calculation).
  public static string Units(double v){if(!Fin(v))throw new IOE("Giá trị "+N(v)+" không hữu hạn.");double r=Math.Round(v,3,MidpointRounding.AwayFromZero);if(r==0)r=0.0;return r.ToString("0.###",ViNum);}
  static readonly Regex DecRx=new Regex(@"\A[0-9]+([.,][0-9]+)?\z"),IntRx=new Regex(@"\A[0-9]+\z");
  // Typed decimal → value; empty/blank = null (Make then says "not entered"; nothing is filled in). Digits with at most ONE decimal mark, '.' or ','
  // (2.5 = 2,5). No sign, thousands separator, exponent, inner space, NaN or Infinity. Surrounding spaces are ignored; the value is not rounded.
  public static double? ParseDecimal(string s,string what){s=(s??"").Trim();if(s.Length==0)return null;
   if(!DecRx.IsMatch(s))throw new IOE(what+" \""+Short(s)+"\" không hợp lệ: chỉ chữ số và tối đa một dấu thập phân (. hoặc ,); không dấu nghìn, số mũ, dấu âm.");
   double v;if(!double.TryParse(s.Replace(',','.'),NumberStyles.AllowDecimalPoint,Inv,out v)||!Fin(v))throw new IOE(what+" \""+Short(s)+"\" vượt giới hạn số.");return v;}
  // Typed whole number → value; empty/blank = null. Digits only (no sign, decimal mark or separator); range/≥ 1 is checked by Make.
  public static int? ParseCount(string s,string what){s=(s??"").Trim();if(s.Length==0)return null;
   if(!IntRx.IsMatch(s))throw new IOE(what+" \""+Short(s)+"\" phải là số nguyên dương (chỉ chữ số).");
   int v;if(!int.TryParse(s,NumberStyles.None,Inv,out v))throw new IOE(what+" \""+Short(s)+"\" quá lớn.");return v;}
  public static string StatusCaption(string status){return status==Ok?"Đã tính":status==Incomplete?"Chưa khai báo":status==Reconfirm?"Cần xác nhận lại":status==Conflict?"Xung đột mã cáp":status==Invalid?"Không hợp lệ":status??"";}
  // Exactly coincident configured routes (same live geometry hash): warning only, nothing is deduplicated.
  public static List<string> Coincident(IEnumerable<RouteRow034> rows){
   return (rows??Enumerable.Empty<RouteRow034>()).Where(r=>r.config!=null&&r.geometryHash!=null).GroupBy(r=>r.geometryHash,StringComparer.Ordinal).Where(g=>g.Count()>1)
    .Select(g=>"Các tuyến "+string.Join(", ",g.Select(r=>r.handle))+" trùng khít hình học; kiểm tra có vẽ chồng không (MEPS không tự loại trùng).").ToList();}
 }
 // TVXD-MEPS-WIREQTY-034 CSV text (pure, RFC 4180; the caller writes it as UTF-8 WITH BOM). Rows end with CRLF, cells are comma separated.
 //  - TEXT cells are always quoted ("" doubles a quote). A text whose first character is = + - @, a control character (tab, CR, LF…) or whitespace gets an
 //    apostrophe prefix so a spreadsheet cannot read it as a formula or hide its start; after the apostrophe the user's text is literal (never trimmed,
 //    never evaluated). Nothing else is changed.
 //  - NUMBER cells are unquoted plain decimals with the INVARIANT point '.' at full precision ("R" round-trip digits written without an exponent, no
 //    thousands separator); non-finite = exception. The preamble says so, because Excel with a Vietnamese decimal comma may read them as text.
 //  - The preamble always states the completeness (excluded routes per status) and the scan time, so an incomplete export is not mistaken for final.
 //  - No drawing path, user name or machine data is written.
 public static class RouteCsv034 {
  static readonly CultureInfo Inv=CultureInfo.InvariantCulture;
  public const string Eol="\r\n";
  public static bool Dangerous(string s){if(string.IsNullOrEmpty(s))return false;char c=s[0];return c=='='||c=='+'||c=='-'||c=='@'||char.IsControl(c)||char.IsWhiteSpace(c);}
  public static string Text(string s){s=s??"";if(Dangerous(s))s="'"+s;return "\""+s.Replace("\"","\"\"")+"\"";}
  public static string Integer(int v){return v.ToString(Inv);}
  public static string Number(double v){if(double.IsNaN(v)||double.IsInfinity(v))throw new InvalidOperationException("Không xuất số không hữu hạn ("+v.ToString("R",Inv)+").");
   return Plain(v.ToString("R",Inv));}
  public static string Number(double? v){return v.HasValue?Number(v.Value):"";}
  // "1.5E-05" → "0.000015", "1E+16" → "10000000000000000" (same digits, decimal point moved; no rounding).
  static string Plain(string r){int e=r.IndexOfAny(new[]{'E','e'});if(e<0)return r;
   string m=r.Substring(0,e);int exp=int.Parse(r.Substring(e+1),NumberStyles.AllowLeadingSign,Inv);bool neg=m.StartsWith("-",StringComparison.Ordinal);if(neg)m=m.Substring(1);
   int dot=m.IndexOf('.');string digits=dot<0?m:m.Remove(dot,1);int whole=(dot<0?m.Length:dot)+exp;string s;
   if(whole<=0)s="0."+new string('0',-whole)+digits;else if(whole>=digits.Length)s=digits+new string('0',whole-digits.Length);else s=digits.Substring(0,whole)+"."+digits.Substring(whole);
   s=s.TrimStart('0');if(s.Length==0||s[0]=='.')s="0"+s;if(s.IndexOf('.')>=0){s=s.TrimEnd('0');if(s.EndsWith(".",StringComparison.Ordinal))s=s.Substring(0,s.Length-1);}
   return (neg?"-":"")+s;}
  public static string Row(params string[] cells){return string.Join(",",cells)+Eol;}
  public static int Excluded(RouteScan034 scan){return scan.rows.Count-scan.Count(Route034.Ok);}
  static void Preamble(StringBuilder sb,RouteScan034 scan,string scannedAt,string kind){int ex=Excluded(scan);
   sb.Append(Row(Text("MEPS-TVBG — bóc khối lượng dây/cáp theo tuyến (034)"),Text(kind)));
   sb.Append(Row(Text("Trạng thái"),Text(ex>0?"CHƯA ĐỦ: "+ex.ToString(Inv)+" tuyến KHÔNG được tính (xem bảng chi tiết); không dùng làm số liệu cuối":
    scan.rows.Count==0?"KHÔNG CÓ TUYẾN: Model chưa có tuyến dây/cáp MEPS nào":"ĐỦ: mọi tuyến dây/cáp nhận diện được đều đã tính")));
   sb.Append(Row(Text("Thời điểm quét (giờ máy)"),Text(scannedAt??"")));
   sb.Append(Row(Text("Tuyến đã tính"),Integer(scan.Count(Route034.Ok))));
   sb.Append(Row(Text("Tuyến bị loại (không tính)"),Integer(ex)));
   foreach(var st in new[]{Route034.Incomplete,Route034.Reconfirm,Route034.Conflict,Route034.Invalid})sb.Append(Row(Text("Trong đó "+Route034.StatusCaption(st)+" ("+st+")"),Integer(scan.Count(st))));
   sb.Append(Row(Text("Cảnh báo"),Integer(scan.warnings.Count)));foreach(var w in scan.warnings)sb.Append(Row(Text("Cảnh báo"),Text(w)));
   sb.Append(Row(Text("Ghi chú"),Text("Số dùng dấu chấm thập phân (.), đủ độ chính xác, không làm tròn (MEPS chỉ làm tròn 1 chữ số khi hiển thị). Đơn vị (m). "+
    "Mét dây (WIRE_M) và mét cáp (CABLE_M) không cộng lẫn. MEPS không tự loại tuyến trùng, không suy số sợi từ TAG.")));
   sb.Append(Eol);}
  // Totals per (area, circuit, code, basis) of the OK routes. Aggregation overflow = exception (F034-AGG01): nothing to write.
  public static string Groups(RouteScan034 scan,string scannedAt){if(scan==null)throw new InvalidOperationException("Chưa có kết quả quét.");var groups=scan.Groups();var sb=new StringBuilder();
   Preamble(sb,scan,scannedAt,"Bảng tổng theo khu vực / mạch / loại cáp / cách tính");
   sb.Append(Row(Text("Khu vực"),Text("Mạch/lộ"),Text("Mã cáp"),Text("Loại cáp"),Text("Cách tính"),Text("Nghĩa"),Text("Số tuyến"),Text("Chiều dài tuyến (m)"),Text("Khối lượng (m)")));
   foreach(var g in groups){var c=Cable016.ByCode(g.code);
    sb.Append(Row(Text(g.area),Text(g.circuit),Text(g.code),Text(c==null?"":c.label),Text(g.basis),Text(Route034.BasisCaption(g.basis)),Integer(g.routes),Number(g.routeMeters),Number(g.quantity)));}
   return sb.ToString();}
  // Every reported route (excluded rows first, then OK rows, scan order kept inside each part) with its status, reason and live values.
  public static string Detail(RouteScan034 scan,string scannedAt){if(scan==null)throw new InvalidOperationException("Chưa có kết quả quét.");var sb=new StringBuilder();
   Preamble(sb,scan,scannedAt,"Bảng chi tiết từng tuyến");
   sb.Append(Row(Text("Handle"),Text("Trạng thái"),Text("Mã trạng thái"),Text("Lý do"),Text("Cảnh báo"),Text("Loại hình"),Text("Layer"),Text("Mã cáp của tuyến vẽ"),
    Text("Khu vực"),Text("Mạch/lộ"),Text("Mã cáp khai báo"),Text("Cách tính"),Text("Số sợi / số cáp song song"),Text("Bổ sung cho đoạn này (m)"),Text("Dự phòng (%)"),
    Text("Đơn vị khai báo"),Text("Chiều dài hiện tại (đơn vị bản vẽ)"),Text("Chiều dài tuyến (m)"),Text("Khối lượng (m)")));
   foreach(var r in scan.rows.OrderBy(x=>x.status==Route034.Ok?1:0)){var c=r.config;
    sb.Append(Row(Text(r.handle),Text(Route034.StatusCaption(r.status)),Text(r.status),Text(r.reason),Text(r.warning),Text(r.type),Text(r.layer),Text(r.knownCode),
     Text(c==null?null:c.area),Text(c==null?null:c.circuit),Text(c==null?null:c.code),Text(c==null?null:c.basis),c==null?"":Integer(c.parallelCount),
     c==null?"":Number(c.extraVerticalMeters),c==null?"":Number(c.reservePercent),Text(c==null?null:c.declaredUnit),Number(r.liveLength),Number(r.routeMeters),Number(r.quantity)));}
   return sb.ToString();}
 }
#if !ROUTE034_PURE
 // Validated selection for one setup step. signature = everything the step depends on (geometry, layer, cable/identity/route records, INSUNITS, fingerprint).
 public sealed class RouteItem034 {public string handle,type,layer,knownCode,geometryHash,signature,existingStatus,existingReason;public double length;public RouteRecord034 existing;public ObjectId id;}
 public sealed class RouteSnapshot034 {public readonly List<RouteItem034> items=new List<RouteItem034>();public int insunits;public string fingerprint;
  // Prefill: the configuration shared by EVERY item; null when some item has none or they differ (UI then starts blank).
  public RouteConfig034 CommonConfig(){if(items.Count==0||items.Any(x=>x.existing==null))return null;var c=items[0].existing.config;return items.All(x=>x.existing.config.SameAs(c))?c.Copy():null;}
  // Single catalog code bound by the route metadata/layer of every item; null when none or not shared.
  public string CommonKnownCode(){var k=items.Select(x=>x.knownCode).Distinct(StringComparer.Ordinal).ToList();return k.Count==1?k[0]:null;}
  public string Summary(){return "Đã chọn "+items.Count+" tuyến: "+items.Count(x=>x.existing==null)+" chưa khai báo, "+items.Count(x=>x.existingStatus==Route034.Reconfirm)+" cần xác nhận lại, "+
   items.Count(x=>x.existingStatus==Route034.Ok)+" đang hợp lệ.";}}
 public sealed class RouteApply034 {public bool changed,warn;public int written,unchanged;public string message;public readonly List<RouteRow034> rows=new List<RouteRow034>();}
 // AutoCAD side. Call inside the caller's command with the document locked. Apply is ONE transaction (one native UNDO step of that command); no Undo marks,
 // no Save, no file operation, no reactor/event subscription. Only Route034.Key Xrecords are written; geometry, layer, colour, Cable016/Identity015 records untouched.
 // Conservative integrity: the stamps make COPY/paste/WBLOCK/other-drawing results, geometry edits (MOVE/STRETCH/PEDIT/BREAK/JOIN/GRIP…) and INSUNITS changes
 // RECONFIRM_REQUIRED (excluded from totals; length still shown). Only an explicit new Apply by the user confirms such a route again. A record with a
 // malformed/unknown schema is never repaired or overwritten. MOVE of a correct route also needs re-confirmation (the hash covers coordinates).
 public static class RouteCad034 {
  public const int MaxSelect=5000;
  // QA failure injection: "after-first-write" throws after the first record is written (the transaction aborts; nothing stays written). null in production.
  public static string FailAt;
  static void Fail(string stage){if(FailAt!=null&&string.Equals(FailAt,stage,StringComparison.Ordinal))throw new IOE("QA: lỗi giả lập tại "+stage+".");}
  static readonly Tolerance Tol=new Tolerance(1e-9,1e-9);
  static string N(double v){return Route034.N(v);}
  static bool Fin(double v){return !double.IsNaN(v)&&!double.IsInfinity(v);}
  static string P(Point3d p){return N(p.X)+","+N(p.Y)+","+N(p.Z);}
  static string V(Vector3d v){return N(v.X)+","+N(v.Y)+","+N(v.Z);}
  static bool SameZ(double a,double b){return Math.Abs(a-b)<=1e-9*Math.Max(1.0,Math.Max(Math.Abs(a),Math.Abs(b)));}
  static string[] Values(Xrecord x){using(var d=x.Data){return d==null?new string[0]:d.AsArray().Select(v=>v.TypeCode==(short)DxfCode.Text?Convert.ToString(v.Value,CultureInfo.InvariantCulture):null).ToArray();}}
  static bool Has(Transaction tr,DBObject o,string key){return !o.ExtensionDictionary.IsNull&&((DBDictionary)tr.GetObject(o.ExtensionDictionary,OpenMode.ForRead)).Contains(key);}
  // Raw values under key; null = no key; a non-Xrecord entry = exception.
  static string[] ReadRaw(Transaction tr,DBObject o,string key){if(!Has(tr,o,key))return null;var d=(DBDictionary)tr.GetObject(o.ExtensionDictionary,OpenMode.ForRead);
   var x=tr.GetObject(d.GetAt(key),OpenMode.ForRead) as Xrecord;if(x==null)throw new IOE("khoá "+key+" của "+o.Handle+" không phải Xrecord; MEPS không ghi đè");return Values(x);}
  static string Join(string[] v){if(v==null)return "-";var s=new StringBuilder();s.Append(v.Length);foreach(var x in v)s.Append('|').Append(x==null?-1:x.Length).Append(':').Append(x);return s.ToString();}
  // ---- geometry: top-level LINE or open LWPOLYLINE, planar XY at one elevation, zero thickness, normal +Z; native Length (bulge arcs included) ----
  sealed class Geo {public string type,hash,reason;public double length;}
  static Geo Why(Geo g,string r){g.reason=r;return g;}
  static Geo Geometry(Entity e){var g=new Geo{type=e.GetType().Name};var s=new StringBuilder();var ln=e as Line;var pl=e as Polyline;
   try{
    if(ln!=null){g.type="LINE";Point3d a=ln.StartPoint,b=ln.EndPoint;
     if(!Fin(a.X)||!Fin(a.Y)||!Fin(a.Z)||!Fin(b.X)||!Fin(b.Y)||!Fin(b.Z))return Why(g,"Line có toạ độ không hữu hạn");
     if(!SameZ(a.Z,b.Z))return Why(g,"Line dốc/3D (Z đầu "+N(a.Z)+" ≠ Z cuối "+N(b.Z)+"); chỉ đo tuyến phẳng XY ở một cao độ");
     if(ln.Thickness!=0)return Why(g,"Line có Thickness "+N(ln.Thickness)+" ≠ 0");
     if(!ln.Normal.IsEqualTo(Vector3d.ZAxis,Tol))return Why(g,"Line không nằm trong mặt XY (pháp tuyến "+V(ln.Normal)+")");
     g.length=ln.Length;s.Append("LINE|").Append(P(a)).Append('|').Append(P(b)).Append('|').Append(V(ln.Normal)).Append('|').Append(N(ln.Thickness));}
    else if(pl!=null){g.type="LWPOLYLINE";int n=pl.NumberOfVertices;
     if(pl.Closed)return Why(g,"Polyline đóng (Closed) chưa hỗ trợ; chỉ tuyến hở");
     if(n<2)return Why(g,"Polyline có "+n+" đỉnh");
     if(pl.Thickness!=0)return Why(g,"Polyline có Thickness "+N(pl.Thickness)+" ≠ 0");
     if(!pl.Normal.IsEqualTo(Vector3d.ZAxis,Tol))return Why(g,"Polyline không nằm trong mặt XY (pháp tuyến "+V(pl.Normal)+")");
     if(!Fin(pl.Elevation))return Why(g,"Polyline có cao độ không hữu hạn");
     s.Append("LWPOLYLINE|").Append(n).Append('|').Append(N(pl.Elevation)).Append('|').Append(V(pl.Normal)).Append('|').Append(N(pl.Thickness));
     for(int i=0;i<n;i++){var p=pl.GetPoint2dAt(i);double bu=pl.GetBulgeAt(i),sw=pl.GetStartWidthAt(i),ew=pl.GetEndWidthAt(i);
      if(!Fin(p.X)||!Fin(p.Y)||!Fin(bu)||!Fin(sw)||!Fin(ew))return Why(g,"đỉnh "+i+" của Polyline có giá trị không hữu hạn");
      s.Append("|V").Append(i).Append(':').Append(N(p.X)).Append(',').Append(N(p.Y)).Append(",b").Append(N(bu)).Append(",w").Append(N(sw)).Append(',').Append(N(ew));}
     g.length=pl.Length;}
    else return Why(g,"không phải Line hoặc Polyline (LWPOLYLINE); Polyline 2D/3D kiểu cũ, Spline, Arc… chưa hỗ trợ");}
   catch(Autodesk.AutoCAD.Runtime.Exception ex){return Why(g,"không đọc được hình học ("+ex.Message+")");}
   if(!Fin(g.length)||!(g.length>0))return Why(g,"chiều dài "+N(g.length)+" không phải số dương hữu hạn");
   g.hash=Route034.Hash(s.ToString());return g;}
  // ---- metadata and cable binding ----
  sealed class Meta {public CableSpec016 cable,layerSpec;public IdentityRecord015 identity;public string[] route,cableRaw,identityRaw;public string known,conflict;
   public bool Recognized{get{return cable!=null||layerSpec!=null||(identity!=null&&identity.origin==Identity015.OriginCable);}}}
  // Malformed Cable016/Identity015/route keys = exception (no fallback). known = the single Cable016 code bound by the cable record, the cable layer alias
  // and a Cable016 identity; conflict = they disagree, or the identity is a non-cable code (legacy preset, sample, block).
  static Meta ReadMeta(Transaction tr,Entity e){var m=new Meta();
   try{m.cable=CableCad016.Read(tr,e);m.cableRaw=ReadRaw(tr,e,Cable016.Key);}catch(IOE ex){throw new IOE("dữ liệu cáp 016 hỏng ("+ex.Message+")");}
   try{m.identity=IdentityCad015.Read(tr,e);m.identityRaw=ReadRaw(tr,e,Identity015.Key);}catch(IOE ex){throw new IOE("mã nhận dạng 015 hỏng ("+ex.Message+")");}
   m.route=ReadRaw(tr,e,Route034.Key);m.layerSpec=Cable016.Find(e.Layer);
   var codes=new List<Tuple<string,string>>();
   if(m.cable!=null)codes.Add(Tuple.Create("dữ liệu cáp 016",m.cable.code));
   if(m.layerSpec!=null)codes.Add(Tuple.Create("layer "+e.Layer,m.layerSpec.code));
   if(m.identity!=null){if(m.identity.origin==Identity015.OriginCable)codes.Add(Tuple.Create("mã nhận dạng 015",m.identity.code));
    else m.conflict="mã nhận dạng 015 "+m.identity.code+" ("+m.identity.kind+"/"+m.identity.origin+") không phải mã cáp 016; MEPS không gán cáp cho tuyến này";}
   if(m.conflict==null&&codes.Select(t=>t.Item2).Distinct(StringComparer.Ordinal).Count()>1)
    m.conflict="mã cáp không thống nhất ("+string.Join(", ",codes.Select(t=>t.Item1+" = "+t.Item2))+"); MEPS không tự chọn";
   if(m.conflict==null&&codes.Count>0)m.known=codes[0].Item2;
   return m;}
  // Why a confirmed record no longer applies to this entity in this drawing (empty = still valid).
  static List<string> StampIssues(RouteStamp034 s,Database db,string handle,string geometryHash){var r=new List<string>();
   if(!string.Equals(s.ownerHandle,handle,StringComparison.OrdinalIgnoreCase))r.Add("handle lúc xác nhận "+s.ownerHandle+" ≠ "+handle+" (bản sao COPY/dán/chèn)");
   if(!string.Equals(s.fingerprint,db.FingerprintGuid,StringComparison.OrdinalIgnoreCase))r.Add("FingerprintGuid khác bản vẽ lúc xác nhận (WBLOCK/chép sang bản vẽ khác)");
   if(s.insunits!=(int)db.Insunits)r.Add("INSUNITS lúc xác nhận "+s.insunits+" ≠ hiện tại "+(int)db.Insunits);
   if(!string.Equals(s.geometryHash,geometryHash,StringComparison.Ordinal))r.Add("hình học tuyến đã đổi sau khi xác nhận (MOVE/STRETCH/PEDIT/BREAK/JOIN/grip…)");
   return r;}
  static void Live(Database db,ObjectId id){if(id.IsNull||id.IsErased||id.Database!=db)throw new IOE("đối tượng đã bị xoá hoặc không thuộc bản vẽ hiện tại");}
  // One selected entity → item for a write step, or exception naming why it cannot be set up.
  static RouteItem034 Inspect(Transaction tr,Database db,ObjectId id){return Inspect(tr,db,id,true);}
  // BN02 (BDGKL pilot): writeRequired false = read-only measurement; identical checks/signature except that a locked layer is not refused (the lock state is
  // not part of the signature, so locking/unlocking never makes a read stale). Every write path keeps writeRequired true.
  static RouteItem034 Inspect(Transaction tr,Database db,ObjectId id,bool writeRequired){Live(db,id);
   var e=tr.GetObject(id,OpenMode.ForRead) as Entity;if(e==null)throw new IOE("đối tượng "+id.Handle+" không phải đối tượng hình");
   string h="đối tượng "+e.Handle+": ";
   if(e.OwnerId!=SymbolUtilityServices.GetBlockModelSpaceId(db))throw new IOE(h+"không nằm trực tiếp trong Model (Layout, bên trong block hoặc xref)");
   var g=Geometry(e);if(g.reason!=null)throw new IOE(h+g.reason);
   var la=(LayerTableRecord)tr.GetObject(e.LayerId,OpenMode.ForRead);if(writeRequired&&la.IsLocked)throw new IOE(h+"nằm trên layer đang khoá "+la.Name+" (MEPS không mở khoá)");
   Meta m;try{m=ReadMeta(tr,e);}catch(IOE ex){throw new IOE(h+ex.Message+"; MEPS không ghi đè");}
   if(m.conflict!=null)throw new IOE(h+m.conflict);
   RouteRecord034 had=null;if(m.route!=null){try{had=Route034.Decode(m.route);}catch(IOE ex){throw new IOE(h+ex.Message);}}
   var it=new RouteItem034{id=id,handle=e.Handle.ToString(),type=g.type,layer=e.Layer,knownCode=m.known,geometryHash=g.hash,length=g.length,existing=had,existingStatus="NONE"};
   if(had!=null){var iss=StampIssues(had.stamp,db,it.handle,g.hash);
    if(m.known!=null&&!string.Equals(m.known,had.config.code,StringComparison.Ordinal))iss.Add("mã cáp đã khai báo "+had.config.code+" khác mã của tuyến "+m.known);
    it.existingStatus=iss.Count==0?Route034.Ok:Route034.Reconfirm;it.existingReason=iss.Count==0?null:string.Join("; ",iss);}
   it.signature=new StringBuilder().Append(it.handle).Append("|T:").Append(g.type).Append("|G:").Append(g.hash).Append("|L:").Append(N(g.length)).Append("|LY:").Append(e.LayerId.Handle)
    .Append(':').Append(e.Layer.Length).Append(':').Append(e.Layer).Append("|U:").Append((int)db.Insunits).Append("|F:").Append(db.FingerprintGuid)
    .Append("|C:").Append(Join(m.cableRaw)).Append("|I:").Append(Join(m.identityRaw)).Append("|R:").Append(Join(m.route)).ToString();
   return it;}
  // All-or-none: every distinct selected object must be a supported route, else exception listing why (nothing is set up, no subset). Duplicates collapse.
  public static RouteSnapshot034 Snapshot(Database db,IEnumerable<ObjectId> ids){return Snapshot(db,ids,true);}
  // BN02: read-only variant for measuring saved routes (BDGKL scan/read). Same validation and signature as Snapshot, but a locked layer is accepted.
  // Never use its result to write: Snapshot/Apply stay strict.
  public static RouteSnapshot034 SnapshotRead(Database db,IEnumerable<ObjectId> ids){return Snapshot(db,ids,false);}
  static RouteSnapshot034 Snapshot(Database db,IEnumerable<ObjectId> ids,bool writeRequired){if(db==null)throw new IOE("Không có bản vẽ.");
   var uniq=(ids??Enumerable.Empty<ObjectId>()).Distinct().ToList();
   if(uniq.Count==0)throw new IOE("Chưa chọn tuyến nào; bản vẽ không đổi.");
   if(uniq.Count>MaxSelect)throw new IOE("Chọn quá "+MaxSelect+" tuyến một lần; chia nhỏ lựa chọn. Bản vẽ không đổi.");
   var snap=new RouteSnapshot034{insunits=(int)db.Insunits,fingerprint=db.FingerprintGuid};var bad=new List<string>();
   using(var tr=db.TransactionManager.StartOpenCloseTransaction()){foreach(var id in uniq){try{snap.items.Add(Inspect(tr,db,id,writeRequired));}catch(IOE ex){bad.Add(ex.Message);}}tr.Commit();}
   if(bad.Count>0)throw new IOE("Không thiết lập tuyến: "+bad.Count+"/"+uniq.Count+" đối tượng đã chọn không hợp lệ (thiết lập tất cả hoặc không gì; bỏ chúng khỏi lựa chọn rồi chọn lại): "+
    string.Join("; ",bad.Take(6))+(bad.Count>6?"; …":"")+". Bản vẽ không đổi.");
   return snap;}
  // Rows the before/after prompt can show for config on the snapshot (live length at snapshot time). Conflict with a bound code or a calculation failure = exception.
  public static List<RouteRow034> Preview(RouteSnapshot034 snap,RouteConfig034 config){var c=Route034.Check(config);
   if(snap==null||snap.items.Count==0)throw new IOE("Lựa chọn không hợp lệ; bản vẽ không đổi.");var list=new List<RouteRow034>();
   foreach(var it in snap.items){
    if(it.knownCode!=null&&!string.Equals(it.knownCode,c.code,StringComparison.Ordinal))throw new IOE("Tuyến "+it.handle+" là cáp "+it.knownCode+"; không khai báo mã "+c.code+" cho tuyến này. Bản vẽ không đổi.");
    RouteQty034 q;try{q=Route034.Calculate(it.length,c);}catch(IOE ex){throw new IOE("Tuyến "+it.handle+": "+ex.Message+" Bản vẽ không đổi.");}
    list.Add(new RouteRow034{id=it.id,handle=it.handle,type=it.type,layer=it.layer,status=Route034.Ok,knownCode=it.knownCode,geometryHash=it.geometryHash,liveLength=it.length,
     routeMeters=q.routeMeters,quantity=q.quantity,config=c.Copy(),warning=Route034.UnitWarning(snap.insunits,c.declaredUnit)});}
   return list;}
  static void Write(Transaction tr,Entity e,string[] v){if(e.ExtensionDictionary.IsNull){if(!e.IsWriteEnabled)e.UpgradeOpen();e.CreateExtensionDictionary();}
   var d=(DBDictionary)tr.GetObject(e.ExtensionDictionary,OpenMode.ForWrite);
   using(var b=new ResultBuffer(v.Select(s=>new TypedValue((int)DxfCode.Text,s)).ToArray())){
    if(d.Contains(Route034.Key)){var x=tr.GetObject(d.GetAt(Route034.Key),OpenMode.ForWrite) as Xrecord;if(x==null)throw new IOE("Khoá "+Route034.Key+" không phải Xrecord; MEPS không ghi đè.");x.Data=b;}
    else{var x=new Xrecord();x.Data=b;d.SetAt(Route034.Key,x);tr.AddNewlyCreatedDBObject(x,true);}}}
  // Revalidates every item against its snapshot signature (stale = everything aborted), checks the bound cable code, computes the quantity from the live
  // length, writes config + fresh stamps in ONE transaction, reads back, calls guard right before Commit. Identical record everywhere = nothing written.
  public static RouteApply034 Apply(Database db,RouteSnapshot034 snap,RouteConfig034 config,Action<string> guard){RouteConfig034 c;
   try{c=Route034.Check(config);}catch(IOE ex){throw new IOE(ex.Message+" Bản vẽ không đổi.");}
   if(db==null||snap==null||snap.items.Count==0||snap.items.Count>MaxSelect||snap.items.Select(x=>x.id).Distinct().Count()!=snap.items.Count)throw new IOE("Lựa chọn không hợp lệ; bản vẽ không đổi.");
   if(snap.insunits!=(int)db.Insunits||!string.Equals(snap.fingerprint,db.FingerprintGuid,StringComparison.Ordinal))throw new IOE("Bản vẽ/INSUNITS đã đổi sau khi chọn; chọn lại. Bản vẽ không đổi.");
   var res=new RouteApply034();
   using(var tr=db.TransactionManager.StartTransaction()){
    var now=new List<RouteItem034>();
    foreach(var was in snap.items){RouteItem034 cur;
     try{cur=Inspect(tr,db,was.id);}catch(IOE ex){throw new IOE("Tuyến đã đổi sau khi chọn ("+ex.Message+"); chọn lại. Bản vẽ không đổi.");}
     if(!string.Equals(cur.signature,was.signature,StringComparison.Ordinal))throw new IOE("Tuyến "+was.handle+" đã đổi (hình học/layer/dữ liệu cáp/dữ liệu tuyến) sau khi chọn; chọn lại. Bản vẽ không đổi.");
     now.Add(cur);}
    var cur2=new RouteSnapshot034{insunits=snap.insunits,fingerprint=snap.fingerprint};cur2.items.AddRange(now);
    res.rows.AddRange(Preview(cur2,c));
    var want=new List<string[]>();
    foreach(var it in now){var v=Route034.Encode(new RouteRecord034{config=c,stamp=new RouteStamp034{insunits=(int)db.Insunits,ownerHandle=it.handle,fingerprint=db.FingerprintGuid,geometryHash=it.geometryHash}});
     want.Add(v);var e=(Entity)tr.GetObject(it.id,OpenMode.ForRead);var had=ReadRaw(tr,e,Route034.Key);
     if(had!=null&&had.SequenceEqual(v,StringComparer.Ordinal)){res.unchanged++;continue;}
     Write(tr,e,v);res.written++;if(res.written==1)Fail("after-first-write");}
    if(res.written==0){res.message="Cả "+res.unchanged+" tuyến đã có đúng thông số này; không ghi gì.";return res;}
    for(int i=0;i<now.Count;i++){var r=ReadRaw(tr,tr.GetObject(now[i].id,OpenMode.ForRead),Route034.Key);
     if(r==null||!r.SequenceEqual(want[i],StringComparer.Ordinal))throw new IOE("Đọc lại dữ liệu tuyến "+now[i].handle+" không khớp; bản vẽ không đổi.");}
    if(guard!=null)guard("ngay trước khi lưu thông số tuyến");
    tr.Commit();}
   res.changed=true;
   // After Commit (same command): fresh read. A mismatch cannot be rolled back here; it is reported and the user's U reverts the whole step.
   int ok=0;try{using(var tr=db.TransactionManager.StartOpenCloseTransaction()){foreach(var it in snap.items){var e=tr.GetObject(it.id,OpenMode.ForRead);var r=ReadRaw(tr,e,Route034.Key);
      if(r!=null){var rec=Route034.Decode(r);if(rec.config.SameAs(c)&&StampIssues(rec.stamp,db,e.Handle.ToString(),it.geometryHash).Count==0)ok++;}}tr.Commit();}}catch(Exception){}
   res.warn=ok!=snap.items.Count;
   res.message=(res.warn?"CẢNH BÁO: đọc lại sau khi lưu chỉ khớp "+ok+"/"+snap.items.Count+" tuyến; dùng U để hoàn tác bước này. ":"")+
    "Đã lưu thông số cho "+res.written+" tuyến"+(res.unchanged>0?" ("+res.unchanged+" tuyến đã đúng, giữ nguyên)":"")+". U/UNDO hoàn tác cả bước; chưa tự lưu DWG. "+
    "Khối lượng luôn tính lại từ chiều dài hiện tại; COPY, sửa hình học hoặc đổi INSUNITS sẽ cần xác nhận lại.";
   return res;}
  // Row of one Model entity (scan/read); null = neither configured nor a recognized Cable016 route (random drafting lines are not reported).
  static RouteRow034 Evaluate(Transaction tr,Database db,Entity e){bool hasRoute=Has(tr,e,Route034.Key);
   var row=new RouteRow034{id=e.ObjectId,handle=e.Handle.ToString(),type=e.GetType().Name,layer=e.Layer};
   if(!(e is Line)&&!(e is Polyline)){if(!hasRoute)return null;row.status=Route034.Invalid;row.reason="có dữ liệu tuyến 034 nhưng không phải Line/Polyline (đối tượng đã bị đổi loại?)";return row;}
   Meta m;try{m=ReadMeta(tr,e);}catch(IOE ex){row.status=Route034.Invalid;row.reason=ex.Message;return row;}
   if(m.route==null&&!m.Recognized)return null;
   var g=Geometry(e);row.type=g.type;row.knownCode=m.known;if(g.reason==null){row.liveLength=g.length;row.geometryHash=g.hash;}
   if(m.route==null){if(m.conflict!=null){row.status=Route034.Conflict;row.reason=m.conflict;}
    else{row.status=Route034.Incomplete;row.reason="tuyến cáp "+m.known+" chưa khai báo khu vực/mạch/cách tính/số tuyến/lên xuống/dự phòng/đơn vị; không tính khối lượng";}return row;}
   RouteRecord034 rec;try{rec=Route034.Decode(m.route);}catch(IOE ex){row.status=Route034.Invalid;row.reason=ex.Message;return row;}
   row.config=rec.config;
   if(g.reason!=null){row.status=Route034.Invalid;row.reason="không đo được chiều dài: "+g.reason;return row;}
   row.routeMeters=g.length/Route034.Factor(rec.config.declaredUnit);row.warning=Route034.UnitWarning((int)db.Insunits,rec.config.declaredUnit);
   if(m.conflict!=null){row.status=Route034.Conflict;row.reason=m.conflict;return row;}
   if(m.known!=null&&!string.Equals(m.known,rec.config.code,StringComparison.Ordinal)){row.status=Route034.Conflict;row.reason="mã cáp đã khai báo "+rec.config.code+" khác mã của tuyến "+m.known+"; MEPS không tự chọn";return row;}
   var iss=StampIssues(rec.stamp,db,row.handle,g.hash);
   if(iss.Count>0){row.status=Route034.Reconfirm;row.reason="cần mở thiết lập tuyến và lưu lại: "+string.Join("; ",iss);return row;}
   try{var q=Route034.Calculate(g.length,rec.config);row.routeMeters=q.routeMeters;row.quantity=q.quantity;row.status=Route034.Ok;}
   catch(IOE ex){row.status=Route034.Invalid;row.reason=ex.Message;}
   return row;}
  // Read helper (QA/UI): current row of one entity; null = no route data and not a recognized cable route. Read only.
  public static RouteRow034 Read(Database db,ObjectId id){Live(db,id);using(var tr=db.TransactionManager.StartOpenCloseTransaction()){
    var e=tr.GetObject(id,OpenMode.ForRead) as Entity;RouteRow034 r;
    if(e==null)r=null;
    else if(e.OwnerId!=SymbolUtilityServices.GetBlockModelSpaceId(db))r=Has(tr,e,Route034.Key)?new RouteRow034{id=id,handle=e.Handle.ToString(),type=e.GetType().Name,layer=e.Layer,status=Route034.Invalid,reason="không nằm trực tiếp trong Model"}:null;
    else r=Evaluate(tr,db,e);
    tr.Commit();return r;}}
  // Every configured Model entity plus recognized Cable016 routes without setup (INCOMPLETE). Read only. Totals: scan.Groups() (OK rows only).
  public static RouteScan034 ScanModel(Database db){if(db==null)throw new IOE("Không có bản vẽ.");var s=new RouteScan034();
   using(var tr=db.TransactionManager.StartOpenCloseTransaction()){
    foreach(ObjectId id in (BlockTableRecord)tr.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db),OpenMode.ForRead)){if(id.IsErased)continue;var e=tr.GetObject(id,OpenMode.ForRead) as Entity;if(e==null)continue;
     RouteRow034 r;try{r=Evaluate(tr,db,e);}catch(Autodesk.AutoCAD.Runtime.Exception ex){r=new RouteRow034{id=id,handle=e.Handle.ToString(),type=e.GetType().Name,layer=e.Layer,status=Route034.Invalid,reason="lỗi đọc ("+ex.Message+")"};}
     if(r!=null)s.rows.Add(r);}
    tr.Commit();}
   s.warnings.AddRange(Route034.Coincident(s.rows));
   foreach(var w in s.rows.Where(r=>r.warning!=null).Select(r=>r.warning).Distinct(StringComparer.Ordinal))s.warnings.Add(w);
   return s;}
 }
#endif
}
#endif
