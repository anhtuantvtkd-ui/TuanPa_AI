using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Tvxd.Meps {
 // TVXD-MEPS-WIREQTY-034 BDGKL pilot: PURE calculation core. No CAD reference, no IO, no persistence/migration, no preprocessor symbol; C# 5 / .NET 4.
 // Contains Vietnamese literals: compile with /codepage:65001 like the other MEPS builds.
 // Scope: one route segment between two DIFFERENT devices on the SAME floor (heights measured from that floor's finished floor level).
 // Formulas follow the user's BDGKL template (row 15) as a sample model, not as an engineering standard; no value is defaulted or designed here:
 //  F = nativeLength / units per metre (declared unit MM = 1000, M = 1)
 //  J: CEILING = (Hc-Ha)+(Hc-Hb); FLOOR = Ha+Hb; WALL = |Ha-Hb|.   Ha/Hb: FIXED height, or CEILING_OFFSET = Hc - offset.
 //  L = tailCount × tailEachLength;  M = F + J + L = route metres for ONE wire assembly (never multiplied by a conductor count parsed from assemblyCode).
 //  W = F - U; X = J - V (U/V = declared shared conduit). Tails are not in conduit; deductions never change M.
 //  WALL: hidden = W + X, exposed = 0;  CEILING/FLOOR: hidden = X, exposed = W (template classification).
 //  After factor = raw value × factor, applied exactly once; nothing is rounded anywhere in this file.
 // Every input field is required: null/"" = MISSING_DATA, never 0. Exceptions: floorFinishedElevation (optional, informational only) and sharedOwnerKey
 // (required only when U or V > 0). Status: CHECK if any value is invalid/conflicting, else MISSING_DATA if any value is missing, else OK.
 // Only OK rows are counted. Values on a non-OK row are informational (each is non-null only when its own inputs are valid) and are never summed.
 public sealed class BdgklInput034 {
  // Identity and text: exact ordinal text, nothing trimmed or case-folded. Labels are display text only and never used as identity.
  public string routeKey,floorId,areaId,circuitId,startKey,endKey,startLabel,endLabel,assemblyCode,conduitCode;
  public double? nativeLength;public string declaredUnit;            // drawing units of the live route; "MM" or "M"
  public string routingMode;public double? ceilingHeight;            // "CEILING" | "FLOOR" | "WALL"; finished floor → ceiling (m)
  public double? floorFinishedElevation;                              // optional; same-floor pilot does not use it in any formula
  public string startHeightMode;public double? startFixedHeight,startCeilingOffset;   // "FIXED" (fixed height) | "CEILING_OFFSET" (offset below ceiling)
  public string endHeightMode;public double? endFixedHeight,endCeilingOffset;
  public int? tailCount;public double? tailEachLength;
  public double? sharedHorizontal,sharedVertical;public string sharedOwnerKey;         // U, V (m) and the routeKey of the row that owns the shared conduit
  public double? factor;                                              // bend factor, explicitly supplied, >= 1
  public BdgklInput034 Copy(){return (BdgklInput034)MemberwiseClone();}
 }
 // status: MISSING_DATA or CHECK. field: the BdgklInput034 field (or result/batch field) concerned.
 public sealed class BdgklIssue034 {public string status,code,field,message;
  public override string ToString(){return status+" "+code+" ["+field+"]: "+message;}}
 public sealed class BdgklResult034 {
  public string status,routeKey,routingMode,equation;
  public readonly List<BdgklIssue034> issues=new List<BdgklIssue034>();
  public double? unitsPerMetre,ceilingHeightM,startHeightM,endHeightM;
  public double? horizontalM,verticalM,tailsM,routeM;                                   // F, J, L, M
  public double? horizontalConduitM,verticalConduitM,conduitHiddenM,conduitExposedM;     // W, X, hidden, exposed (before factor)
  public double? factor,routeAfterFactorM,conduitHiddenAfterFactorM,conduitExposedAfterFactorM;
  public bool Countable{get{return status==Bdgkl034.Ok;}}
 }
 public sealed class BdgklBatchRow034 {public int index;public BdgklInput034 input;public BdgklResult034 result;}
 // Totals of the OK rows of one (floor, area, circuit, assembly, conduit). A counted row belongs to exactly one group; groups are not nested subtotals.
 // Contract: every counted row of a group must carry the identical factor; the factor is applied once to each raw group sum (as the template does at its
 // total row). Mixed factors → status CHECK and after-factor totals null (raw totals kept). Any excluded row makes the group MISSING_DATA or CHECK.
 public sealed class BdgklGroup034 {
  public string floorId,areaId,circuitId,assemblyCode,conduitCode,status;
  public int countedRows,excludedRows;
  public readonly List<string> countedRouteKeys=new List<string>();
  public readonly List<BdgklIssue034> issues=new List<BdgklIssue034>();
  public double? horizontalM,verticalM,tailsM,routeM,horizontalConduitM,verticalConduitM,conduitHiddenM,conduitExposedM;
  public double? factor,routeAfterFactorM,conduitHiddenAfterFactorM,conduitExposedAfterFactorM;
 }
 public sealed class BdgklBatch034 {
  public readonly List<BdgklBatchRow034> rows=new List<BdgklBatchRow034>();
  public readonly List<BdgklGroup034> groups=new List<BdgklGroup034>();
  public int total,ok,missingData,check,excludedUngrouped;   // excludedUngrouped: non-OK rows whose group identity itself is missing/invalid
 }
 public static class Bdgkl034 {
  public const string Ok="OK",MissingData="MISSING_DATA",Check="CHECK";
  public const string Ceiling="CEILING",Floor="FLOOR",Wall="WALL";
  public const string Fixed="FIXED",CeilingOffset="CEILING_OFFSET";
  public const string UnitMm="MM",UnitM="M";
  public const int MaxText=100;
  static readonly CultureInfo Inv=CultureInfo.InvariantCulture;
  static bool Fin(double v){return !double.IsNaN(v)&&!double.IsInfinity(v);}
  // Round-trip text of a raw value (for messages/reports).
  public static string N(double v){return v.ToString("R",Inv);}
  // Explanation text only (15 significant digits hides binary noise such as 2.6999999999999997); the numeric result fields stay unrounded.
  static string E(double? v){return v.HasValue?v.Value.ToString("G15",Inv):"—";}
  // Numeric policy (BD034-F01), deduction checks only: a deduction that exceeds its availability by no more than machine roundoff,
  // |ded-avail| <= 8*2.220446049250313e-16*max(|ded|,|avail|) with both finite, is binary noise of the same decimal value (e.g.
  // 3.9-1.5 + 3.9-3.6 = 2.6999999999999997 vs a declared 2.7), not a design tolerance. Then the availability itself is deducted so the remainder
  // is exactly 0; the declared input is kept unchanged. Any larger excess (2.70001 vs 2.7) stays DEDUCTION_EXCEEDS. Raw totals are never rounded.
  const double RoundoffRel=8*2.220446049250313e-16;
  static bool ExceedsByRoundoffOnly(double ded,double avail){
   if(!Fin(ded)||!Fin(avail)||!(ded>avail))return false;
   return ded-avail<=RoundoffRel*Math.Max(Math.Abs(ded),Math.Abs(avail));}
  static string Short(string s){return s==null?"null":s.Length>24?s.Substring(0,24)+"…":s;}
  static void Miss(List<BdgklIssue034> iss,string code,string field,string msg){iss.Add(new BdgklIssue034{status=MissingData,code=code,field=field,message=msg});}
  static void Bad(List<BdgklIssue034> iss,string code,string field,string msg){iss.Add(new BdgklIssue034{status=Check,code=code,field=field,message=msg});}
  public static string StatusOf(IList<BdgklIssue034> iss){
   bool missing=false;foreach(var i in iss){if(i.status==Check)return Check;if(i.status==MissingData)missing=true;}
   return missing?MissingData:Ok;}
  public static double? UnitsPerMetre(string unit){if(unit==UnitMm)return 1000;if(unit==UnitM)return 1;return null;}
  // null = usable text; otherwise why the exact text is refused (no trimming, no case folding).
  public static string TextProblem(string s){
   if(string.IsNullOrEmpty(s))return "trống";
   if(s.Trim().Length==0)return "chỉ có khoảng trắng";
   if(s.Length>MaxText)return "dài "+s.Length.ToString(Inv)+" ký tự (tối đa "+MaxText.ToString(Inv)+")";
   if(char.IsWhiteSpace(s[0])||char.IsWhiteSpace(s[s.Length-1]))return "có khoảng trắng đầu/cuối (MEPS không tự cắt)";
   foreach(char c in s){if(c==' ')continue;
    switch(char.GetUnicodeCategory(c)){case UnicodeCategory.Control:case UnicodeCategory.Format:case UnicodeCategory.LineSeparator:case UnicodeCategory.ParagraphSeparator:
     case UnicodeCategory.SpaceSeparator:case UnicodeCategory.PrivateUse:case UnicodeCategory.OtherNotAssigned:case UnicodeCategory.Surrogate:
      return "có ký tự điều khiển/khoảng trắng đặc biệt U+"+((int)c).ToString("X4",Inv);}}
   return null;}
  static bool UsableText(string s){return TextProblem(s)==null;}
  static bool Text(List<BdgklIssue034> iss,string v,string field,string what){
   if(string.IsNullOrEmpty(v)){Miss(iss,"MISSING_TEXT",field,"Chưa có "+what+".");return false;}
   string p=TextProblem(v);if(p!=null){Bad(iss,"INVALID_TEXT",field,what+" \""+Short(v)+"\" "+p+".");return false;}
   return true;}
  // The value only if present, finite and >= min (> min when strict); otherwise records MISSING_DATA/CHECK and returns null. -0 becomes 0.
  static double? Num(List<BdgklIssue034> iss,double? v,string field,string what,double min,bool strict){
   if(!v.HasValue){Miss(iss,"MISSING_VALUE",field,"Chưa nhập "+what+" (MEPS không tự điền 0).");return null;}
   double x=v.Value;
   if(!Fin(x)){Bad(iss,"NOT_FINITE",field,what+" không phải số hữu hạn ("+N(x)+").");return null;}
   if(strict?x<=min:x<min){Bad(iss,"OUT_OF_RANGE",field,what+" phải "+(strict?"> ":"≥ ")+N(min)+"; nhận "+N(x)+".");return null;}
   return x==0?0.0:x;}
  static double? G(List<BdgklIssue034> iss,double v,string field,string what){
   if(Fin(v))return v==0?0.0:v;
   Bad(iss,"OVERFLOW",field,what+" vượt giới hạn số (kết quả không hữu hạn).");return null;}
  static double? Units(List<BdgklIssue034> iss,string unit){
   if(string.IsNullOrEmpty(unit)){Miss(iss,"MISSING_UNIT","declaredUnit","Chưa khai báo đơn vị bản vẽ (MM = 1000 đơn vị/m hoặc M = 1 đơn vị/m); MEPS không tự giả định.");return null;}
   double? u=UnitsPerMetre(unit);
   if(!u.HasValue)Bad(iss,"UNKNOWN_UNIT","declaredUnit","Đơn vị \""+Short(unit)+"\" không phải MM hoặc M (so khớp đúng chữ hoa).");
   return u;}
  static string Mode(List<BdgklIssue034> iss,string m){
   if(string.IsNullOrEmpty(m)){Miss(iss,"MISSING_MODE","routingMode","Chưa chọn hướng đi (CEILING/FLOOR/WALL).");return null;}
   if(m==Ceiling||m==Floor||m==Wall)return m;
   Bad(iss,"UNKNOWN_MODE","routingMode","Hướng đi \""+Short(m)+"\" không thuộc CEILING/FLOOR/WALL (so khớp đúng chữ hoa); MEPS không tự đoán.");return null;}
  // Height above the finished floor of one end. ceil = validated ceiling height or null.
  static double? Height(List<BdgklIssue034> iss,string side,string what,string mode,double? fixedH,double? offset,double? ceil){
   string fm=side+"HeightMode",ff=side+"FixedHeight",fo=side+"CeilingOffset";
   if(string.IsNullOrEmpty(mode)){Miss(iss,"MISSING_MODE",fm,"Chưa chọn cách khai báo cao độ "+what+" (FIXED hoặc CEILING_OFFSET).");return null;}
   if(mode==Fixed){
    if(offset.HasValue){Bad(iss,"HEIGHT_CONFLICT",fo,"Cao độ "+what+" khai báo FIXED nhưng vẫn có khoảng hạ dưới trần; chỉ nhập một cách.");return null;}
    double? h=Num(iss,fixedH,ff,"cao độ cố định "+what+" (m)",0,false);
    if(h.HasValue&&ceil.HasValue&&h.Value>ceil.Value){Bad(iss,"HEIGHT_ABOVE_CEILING",ff,"Cao độ "+what+" "+N(h.Value)+" m cao hơn H sàn→trần "+N(ceil.Value)+" m (pilot chỉ nhận cùng tầng).");return null;}
    return h;}
   if(mode==CeilingOffset){
    if(fixedH.HasValue){Bad(iss,"HEIGHT_CONFLICT",ff,"Cao độ "+what+" khai báo CEILING_OFFSET nhưng vẫn có cao độ cố định; chỉ nhập một cách.");return null;}
    double? o=Num(iss,offset,fo,"khoảng hạ dưới trần "+what+" (m)",0,false);
    if(!o.HasValue||!ceil.HasValue)return null;
    if(o.Value>ceil.Value){Bad(iss,"OFFSET_ABOVE_CEILING",fo,"Khoảng hạ dưới trần "+what+" "+N(o.Value)+" m lớn hơn H sàn→trần "+N(ceil.Value)+" m.");return null;}
    return G(iss,ceil.Value-o.Value,side+"HeightM","cao độ "+what);}
   Bad(iss,"UNKNOWN_MODE",fm,"Cách khai báo cao độ "+what+" \""+Short(mode)+"\" không thuộc FIXED/CEILING_OFFSET (so khớp đúng chữ hoa).");return null;}
  static string HeightText(string name,string mode,double? h,double? ceil,double? offset){
   if(mode==Fixed)return name+" = "+E(h)+" m (cố định)";
   if(mode==CeilingOffset)return name+" = Htrần − hạ = "+E(ceil)+" − "+E(offset)+" = "+E(h)+" m";
   return name+" = — (chưa có cách khai báo cao độ hợp lệ)";}

  // One route row. Never throws for bad data: everything is reported through status/issues; input null → MISSING_DATA.
  public static BdgklResult034 Calculate(BdgklInput034 d){
   var r=new BdgklResult034();var iss=r.issues;
   if(d==null){Miss(iss,"INPUT_NULL","input","Không có dữ liệu tuyến.");r.status=MissingData;r.equation="(không có dữ liệu)";return r;}
   r.routeKey=d.routeKey;
   bool routeOk=Text(iss,d.routeKey,"routeKey","mã tuyến");
   Text(iss,d.floorId,"floorId","tầng");Text(iss,d.areaId,"areaId","phòng/vùng");Text(iss,d.circuitId,"circuitId","lộ");
   bool sk=Text(iss,d.startKey,"startKey","khóa liên kết điểm đầu"),ek=Text(iss,d.endKey,"endKey","khóa liên kết điểm cuối");
   Text(iss,d.startLabel,"startLabel","nhãn điểm đầu");Text(iss,d.endLabel,"endLabel","nhãn điểm cuối");
   Text(iss,d.assemblyCode,"assemblyCode","mã bộ dây");Text(iss,d.conduitCode,"conduitCode","mã ống");
   if(sk&&ek&&string.Equals(d.startKey,d.endKey,StringComparison.Ordinal))
    Bad(iss,"SAME_ENDPOINT","endKey","Điểm đầu và điểm cuối là cùng một thiết bị (\""+Short(d.startKey)+"\"); pilot chỉ nhận một đoạn giữa hai thiết bị khác nhau.");
   // F
   double? upm=Units(iss,d.declaredUnit);r.unitsPerMetre=upm;
   double? len=Num(iss,d.nativeLength,"nativeLength","chiều dài tuyến trên bản vẽ",0,true);
   if(upm.HasValue&&len.HasValue)r.horizontalM=G(iss,len.Value/upm.Value,"horizontalM","chiều dài ngang F");
   // Mode, ceiling, heights, J
   string mode=Mode(iss,d.routingMode);r.routingMode=mode;
   double? ceil=Num(iss,d.ceilingHeight,"ceilingHeight","H sàn→trần (m)",0,true);r.ceilingHeightM=ceil;
   if(d.floorFinishedElevation.HasValue&&!Fin(d.floorFinishedElevation.Value))
    Bad(iss,"NOT_FINITE","floorFinishedElevation","Cao độ sàn hoàn thiện không phải số hữu hạn ("+N(d.floorFinishedElevation.Value)+").");
   r.startHeightM=Height(iss,"start","điểm đầu",d.startHeightMode,d.startFixedHeight,d.startCeilingOffset,ceil);
   r.endHeightM=Height(iss,"end","điểm cuối",d.endHeightMode,d.endFixedHeight,d.endCeilingOffset,ceil);
   if(mode!=null&&r.startHeightM.HasValue&&r.endHeightM.HasValue){double ha=r.startHeightM.Value,hb=r.endHeightM.Value;
    if(mode==Ceiling){if(ceil.HasValue)r.verticalM=G(iss,(ceil.Value-ha)+(ceil.Value-hb),"verticalM","chiều dài đứng J");}
    else if(mode==Floor)r.verticalM=G(iss,ha+hb,"verticalM","chiều dài đứng J");
    else r.verticalM=G(iss,Math.Abs(ha-hb),"verticalM","chiều dài đứng J");}
   // L, M
   int? cnt=null;
   if(!d.tailCount.HasValue)Miss(iss,"MISSING_VALUE","tailCount","Chưa nhập số đầu chờ (nhập 0 nếu không có; MEPS không tự điền).");
   else if(d.tailCount.Value<0)Bad(iss,"OUT_OF_RANGE","tailCount","Số đầu chờ phải là số nguyên ≥ 0; nhận "+d.tailCount.Value.ToString(Inv)+".");
   else cnt=d.tailCount.Value;
   double? each=Num(iss,d.tailEachLength,"tailEachLength","chiều dài một đầu chờ (m)",0,false);
   if(cnt.HasValue&&each.HasValue)r.tailsM=G(iss,cnt.Value*each.Value,"tailsM","dây chờ L");
   if(r.horizontalM.HasValue&&r.verticalM.HasValue&&r.tailsM.HasValue)r.routeM=G(iss,r.horizontalM.Value+r.verticalM.Value+r.tailsM.Value,"routeM","tổng tuyến M");
   // Declared shared conduit
   double? u=Num(iss,d.sharedHorizontal,"sharedHorizontal","ống ngang dùng chung U (m)",0,false);
   double? v=Num(iss,d.sharedVertical,"sharedVertical","ống đứng dùng chung V (m)",0,false);
   double? uUse=u,vUse=v;bool uRo=false,vRo=false;   // uRo/vRo: excess was machine roundoff only → availability deducted (see ExceedsByRoundoffOnly)
   if(u.HasValue&&r.horizontalM.HasValue&&u.Value>r.horizontalM.Value){
    if(ExceedsByRoundoffOnly(u.Value,r.horizontalM.Value)){uUse=r.horizontalM.Value;uRo=true;}
    else{Bad(iss,"DEDUCTION_EXCEEDS","sharedHorizontal","Ống ngang dùng chung U = "+N(u.Value)+" m lớn hơn chiều dài ngang F = "+N(r.horizontalM.Value)+" m.");uUse=null;}}
   if(v.HasValue&&r.verticalM.HasValue&&v.Value>r.verticalM.Value){
    if(ExceedsByRoundoffOnly(v.Value,r.verticalM.Value)){vUse=r.verticalM.Value;vRo=true;}
    else{Bad(iss,"DEDUCTION_EXCEEDS","sharedVertical","Ống đứng dùng chung V = "+N(v.Value)+" m lớn hơn chiều dài đứng J = "+N(r.verticalM.Value)+" m.");vUse=null;}}
   bool anyDed=(u.HasValue&&u.Value>0)||(v.HasValue&&v.Value>0),ownerGiven=!string.IsNullOrEmpty(d.sharedOwnerKey);
   if(anyDed){
    if(!ownerGiven)Miss(iss,"SHARED_OWNER_REQUIRED","sharedOwnerKey","Có trừ trùng ống nhưng chưa khai báo tuyến sở hữu đoạn ống chung; MEPS không tự suy từ hình.");
    else if(Text(iss,d.sharedOwnerKey,"sharedOwnerKey","tuyến sở hữu ống chung")&&routeOk&&string.Equals(d.sharedOwnerKey,d.routeKey,StringComparison.Ordinal))
     Bad(iss,"SHARED_OWNER_SELF","sharedOwnerKey","Tuyến sở hữu ống chung không được là chính tuyến này (\""+Short(d.routeKey)+"\").");}
   else if(ownerGiven&&u.HasValue&&v.HasValue)
    Bad(iss,"SHARED_OWNER_WITHOUT_DEDUCTION","sharedOwnerKey","Có khai báo tuyến sở hữu ống chung nhưng U = V = 0; xóa tuyến sở hữu hoặc nhập phần trùng.");
   if(r.horizontalM.HasValue&&uUse.HasValue)r.horizontalConduitM=G(iss,r.horizontalM.Value-uUse.Value,"horizontalConduitM","ống ngang W");
   if(r.verticalM.HasValue&&vUse.HasValue)r.verticalConduitM=G(iss,r.verticalM.Value-vUse.Value,"verticalConduitM","ống đứng X");
   if(mode!=null&&r.horizontalConduitM.HasValue&&r.verticalConduitM.HasValue){
    if(mode==Wall){r.conduitHiddenM=G(iss,r.horizontalConduitM.Value+r.verticalConduitM.Value,"conduitHiddenM","ống chìm");r.conduitExposedM=0.0;}
    else{r.conduitHiddenM=r.verticalConduitM;r.conduitExposedM=r.horizontalConduitM;}}
   // Factor, applied once
   double? k=Num(iss,d.factor,"factor","hệ số uốn cong",1,false);r.factor=k;
   if(k.HasValue){
    if(r.routeM.HasValue)r.routeAfterFactorM=G(iss,r.routeM.Value*k.Value,"routeAfterFactorM","tổng tuyến sau hệ số");
    if(r.conduitHiddenM.HasValue)r.conduitHiddenAfterFactorM=G(iss,r.conduitHiddenM.Value*k.Value,"conduitHiddenAfterFactorM","ống chìm sau hệ số");
    if(r.conduitExposedM.HasValue)r.conduitExposedAfterFactorM=G(iss,r.conduitExposedM.Value*k.Value,"conduitExposedAfterFactorM","ống nổi sau hệ số");}
   r.status=StatusOf(iss);
   // Explanation
   var eq=new List<string>();
   eq.Add("F = "+E(len)+" "+(upm.HasValue?d.declaredUnit:"(đơn vị?)")+" ÷ "+E(upm)+" = "+E(r.horizontalM)+" m");
   eq.Add(HeightText("Hđầu",d.startHeightMode,r.startHeightM,ceil,d.startCeilingOffset));
   eq.Add(HeightText("Hcuối",d.endHeightMode,r.endHeightM,ceil,d.endCeilingOffset));
   if(mode==Ceiling)eq.Add("J (TRẦN) = (Htrần − Hđầu) + (Htrần − Hcuối) = ("+E(ceil)+" − "+E(r.startHeightM)+") + ("+E(ceil)+" − "+E(r.endHeightM)+") = "+E(r.verticalM)+" m");
   else if(mode==Floor)eq.Add("J (SÀN) = Hđầu + Hcuối = "+E(r.startHeightM)+" + "+E(r.endHeightM)+" = "+E(r.verticalM)+" m");
   else if(mode==Wall)eq.Add("J (TƯỜNG) = |Hđầu − Hcuối| = |"+E(r.startHeightM)+" − "+E(r.endHeightM)+"| = "+E(r.verticalM)+" m");
   else eq.Add("J = — (chưa có hướng đi hợp lệ)");
   eq.Add("L = số đầu chờ × dài một đầu = "+(cnt.HasValue?cnt.Value.ToString(Inv):"—")+" × "+E(each)+" = "+E(r.tailsM)+" m");
   eq.Add("M = F + J + L = "+E(r.horizontalM)+" + "+E(r.verticalM)+" + "+E(r.tailsM)+" = "+E(r.routeM)+" m tuyến cho một bộ dây "+(d.assemblyCode??"—")+
    " (không nhân số sợi; trừ trùng ống không đổi M)");
   eq.Add("W = F − U = "+E(r.horizontalM)+" − "+E(u)+" = "+E(r.horizontalConduitM)+" m; X = J − V = "+E(r.verticalM)+" − "+E(v)+" = "+E(r.verticalConduitM)+" m (dây chờ không tính ống)");
   if(uRo)eq.Add("U = "+N(u.Value)+" chỉ vượt F = "+N(r.horizontalM.Value)+" do sai số làm tròn số máy (≤ 8ε tương đối, không phải dung sai thiết kế): trừ đúng F, W = 0");
   if(vRo)eq.Add("V = "+N(v.Value)+" chỉ vượt J = "+N(r.verticalM.Value)+" do sai số làm tròn số máy (≤ 8ε tương đối, không phải dung sai thiết kế): trừ đúng J, X = 0");
   if(mode==Wall)eq.Add("Ống chìm = W + X = "+E(r.conduitHiddenM)+" m; ống nổi = "+E(r.conduitExposedM)+" m (TƯỜNG, theo cách phân loại của mẫu)");
   else eq.Add("Ống chìm = X = "+E(r.conduitHiddenM)+" m; ống nổi = W = "+E(r.conduitExposedM)+" m (TRẦN/SÀN, theo cách phân loại của mẫu)");
   eq.Add("Sau hệ số k = "+E(k)+" (áp một lần, không làm tròn): dây = "+E(r.routeAfterFactorM)+" m; ống chìm = "+E(r.conduitHiddenAfterFactorM)+" m; ống nổi = "+E(r.conduitExposedAfterFactorM)+" m");
   r.equation=string.Join("\n",eq.ToArray());
   return r;}

  static string KeyOf(BdgklInput034 d){return d!=null&&UsableText(d.routeKey)?d.routeKey:null;}
  static bool HasFieldIssue(BdgklResult034 r,string field){foreach(var i in r.issues)if(i.field==field)return true;return false;}
  static void BatchBad(BdgklResult034 r,string code,string field,string msg){Bad(r.issues,code,field,msg);r.status=StatusOf(r.issues);}
  static bool GroupKeyUsable(BdgklInput034 d){return d!=null&&UsableText(d.floorId)&&UsableText(d.areaId)&&UsableText(d.circuitId)&&UsableText(d.assemblyCode)&&UsableText(d.conduitCode);}
  static double? Sum(BdgklGroup034 g,List<BdgklResult034> rs,Func<BdgklResult034,double?> f,string field){
   if(rs.Count==0)return null;double s=0;
   foreach(var r in rs){double? v=f(r);if(!v.HasValue){Bad(g.issues,"INTERNAL_NULL",field,"Dòng hợp lệ thiếu giá trị "+field+".");return null;}s+=v.Value;}
   if(!Fin(s)){Bad(g.issues,"SUM_OVERFLOW",field,"Tổng "+field+" vượt giới hạn số.");return null;}
   return s==0?0.0:s;}
  static double? Times(BdgklGroup034 g,double? sum,double? k,string field){
   if(!sum.HasValue||!k.HasValue)return null;double v=sum.Value*k.Value;
   if(!Fin(v)){Bad(g.issues,"SUM_OVERFLOW",field,"Tổng "+field+" sau hệ số vượt giới hạn số.");return null;}
   return v==0?0.0:v;}

  // Batch: per-row Calculate, then batch checks (duplicate routeKey; shared owner must be a unique, counted row of the same batch and not form a cycle),
  // then groups of OK rows. Raw values are summed unrounded; rows that are not OK are only counted in excludedRows / the batch counters.
  public static BdgklBatch034 Aggregate(IList<BdgklInput034> inputs){
   if(inputs==null)throw new ArgumentNullException("inputs");
   var b=new BdgklBatch034();
   for(int i=0;i<inputs.Count;i++)b.rows.Add(new BdgklBatchRow034{index=i,input=inputs[i],result=Calculate(inputs[i])});
   var byKey=new Dictionary<string,List<int>>(StringComparer.Ordinal);
   foreach(var row in b.rows){string k=KeyOf(row.input);if(k==null)continue;List<int> l;if(!byKey.TryGetValue(k,out l))byKey[k]=l=new List<int>();l.Add(row.index);}
   foreach(var kv in byKey)if(kv.Value.Count>1)foreach(int i in kv.Value)
    BatchBad(b.rows[i].result,"DUPLICATE_ROUTE_KEY","routeKey","Mã tuyến \""+Short(kv.Key)+"\" xuất hiện "+kv.Value.Count.ToString(Inv)+" lần trong lô; không tính để tránh đếm lặp.");
   // owner[i] = index of the declared owner row, or -1 (none declared / already refused at row level)
   var owner=new int[b.rows.Count];
   foreach(var row in b.rows){owner[row.index]=-1;var d=row.input;
    if(d==null||string.IsNullOrEmpty(d.sharedOwnerKey)||HasFieldIssue(row.result,"sharedOwnerKey"))continue;
    List<int> l;
    if(!byKey.TryGetValue(d.sharedOwnerKey,out l))
     BatchBad(row.result,"SHARED_OWNER_NOT_IN_BATCH","sharedOwnerKey","Tuyến sở hữu ống chung \""+Short(d.sharedOwnerKey)+"\" không có trong lô; không trừ trùng khi chưa có chủ sở hữu được tính.");
    else if(l.Count>1)BatchBad(row.result,"SHARED_OWNER_AMBIGUOUS","sharedOwnerKey","Tuyến sở hữu ống chung \""+Short(d.sharedOwnerKey)+"\" bị trùng mã trong lô.");
    else owner[row.index]=l[0];}
   foreach(var row in b.rows){int cur=owner[row.index];
    for(int step=0;cur>=0&&step<=b.rows.Count;step++){
     if(cur==row.index){BatchBad(row.result,"SHARED_OWNER_CYCLE","sharedOwnerKey","Chuỗi tuyến sở hữu ống chung quay vòng về chính tuyến này; trừ trùng sẽ bị đếm hai lần.");break;}
     cur=owner[cur];}}
   for(bool changed=true;changed;){changed=false;
    foreach(var row in b.rows){int o=owner[row.index];
     if(o<0||row.result.status==Check||b.rows[o].result.status==Ok)continue;
     BatchBad(row.result,"SHARED_OWNER_NOT_COUNTED","sharedOwnerKey","Tuyến sở hữu ống chung \""+Short(row.input.sharedOwnerKey)+"\" không được tính ("+b.rows[o].result.status+"); không trừ trùng.");
     changed=true;}}
   // Groups
   var groups=new Dictionary<string,BdgklGroup034>(StringComparer.Ordinal);
   var counted=new Dictionary<BdgklGroup034,List<BdgklResult034>>();
   var worstExcluded=new Dictionary<BdgklGroup034,string>();
   foreach(var row in b.rows){var r=row.result;var d=row.input;b.total++;
    if(r.status==Ok)b.ok++;else if(r.status==Check)b.check++;else b.missingData++;
    if(!GroupKeyUsable(d)){b.excludedUngrouped++;continue;}   // an OK row always has a usable group key
    string key=d.floorId+"\u001F"+d.areaId+"\u001F"+d.circuitId+"\u001F"+d.assemblyCode+"\u001F"+d.conduitCode;
    BdgklGroup034 g;
    if(!groups.TryGetValue(key,out g)){g=new BdgklGroup034{floorId=d.floorId,areaId=d.areaId,circuitId=d.circuitId,assemblyCode=d.assemblyCode,conduitCode=d.conduitCode};
     groups[key]=g;b.groups.Add(g);counted[g]=new List<BdgklResult034>();worstExcluded[g]=Ok;}
    if(r.status==Ok){g.countedRows++;g.countedRouteKeys.Add(r.routeKey);counted[g].Add(r);}
    else{g.excludedRows++;if(r.status==Check||worstExcluded[g]==Ok)worstExcluded[g]=r.status;}}
   foreach(var g in b.groups){var rs=counted[g];
    g.horizontalM=Sum(g,rs,x=>x.horizontalM,"horizontalM");g.verticalM=Sum(g,rs,x=>x.verticalM,"verticalM");g.tailsM=Sum(g,rs,x=>x.tailsM,"tailsM");
    g.routeM=Sum(g,rs,x=>x.routeM,"routeM");g.horizontalConduitM=Sum(g,rs,x=>x.horizontalConduitM,"horizontalConduitM");
    g.verticalConduitM=Sum(g,rs,x=>x.verticalConduitM,"verticalConduitM");g.conduitHiddenM=Sum(g,rs,x=>x.conduitHiddenM,"conduitHiddenM");
    g.conduitExposedM=Sum(g,rs,x=>x.conduitExposedM,"conduitExposedM");
    if(rs.Count>0){double? k=rs[0].factor;bool same=k.HasValue;
     foreach(var r in rs)if(!r.factor.HasValue||!k.HasValue||r.factor.Value!=k.Value)same=false;
     if(same){g.factor=k;
      g.routeAfterFactorM=Times(g,g.routeM,k,"routeAfterFactorM");
      g.conduitHiddenAfterFactorM=Times(g,g.conduitHiddenM,k,"conduitHiddenAfterFactorM");
      g.conduitExposedAfterFactorM=Times(g,g.conduitExposedM,k,"conduitExposedAfterFactorM");}
     else Bad(g.issues,"FACTOR_MISMATCH","factor","Các tuyến trong nhóm có hệ số uốn cong khác nhau; không tính tổng sau hệ số.");}
    if(g.excludedRows>0){string w=worstExcluded[g];
     var msg=g.excludedRows.ToString(Inv)+" dòng không tính ("+(w==Check?"có dòng cần kiểm tra":"thiếu dữ liệu")+"); tổng nhóm chỉ gồm "+g.countedRows.ToString(Inv)+" dòng hợp lệ.";
     if(w==Check)Bad(g.issues,"EXCLUDED_ROWS","rows",msg);else Miss(g.issues,"EXCLUDED_ROWS","rows",msg);}
    g.status=StatusOf(g.issues);}
   return b;}
 }
}
