using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml;

namespace Tvxd.Meps {
 // TVXD-MEPS-ROOMQTY-037 XLSX EXPORTER for RoomCompletionBatch037. Pure framework code: System.IO.Compression (ZipArchive, .NET 4.5),
 // System.Xml (XmlConvert only), System.Core. No Office, no macros, no external links, no network. Contains Vietnamese literals:
 // compile with /codepage:65001 (shared MEPS build).
 // Write(path, batch): recalculates the batch (never trusts old results), refuses anything but status PASS, refuses an existing path, builds the
 // whole package in memory and only then creates the file with FileMode.CreateNew (an existing file is never opened for writing or replaced; a
 // partially written new file is deleted). Refusals: ArgumentNullException / ArgumentException / InvalidOperationException / IOException.
 // Workbook: BDGKL (source A:AH column meanings, headers rows 10/11, data from row 12, no group rows), DM.DIEM (editable declarations, CAD
 // provenance, shared-allocation evidence), THKL (dynamic totals of PASS rows). All text cells are inline strings (literal "=x", "01", "1E5").
 // Formulas only use IF/IFERROR/AND/OR/NOT/EXACT/INDEX/MATCH/ISNUMBER/N/INT/ABS/LEN/LEFT/RIGHT/SUM/SUMIF/SUMPRODUCT/COUNTIF/COUNTA (no XLOOKUP).
 // fullCalcOnLoad is set; cached values are the model results at export.
 // Design of the live sheet (documented choices):
 //  - Heights are keyed per device in DM.DIEM (one declaration per device, as enforced by the model), ceilings per floor; BDGKL H/I/J recompute.
 //  - Blank cells are never read as 0: every reference is guarded (IF(x="","",x) / ISNUMBER); invalid values produce CHECK, blanks MISSING DATA.
 //  - AE = CHECK if column BP (first CHECK reason) is not empty, else MISSING DATA if BQ (first missing item) is not empty, else PASS; any formula
 //    error -> CHECK. O:S, Y:AD and the "counted" helper columns BJ:BO are only filled for PASS rows, so THKL never sums an invalid row.
 //  - Shared conduit (SHARED): U = SUMIF over this row's own evidence lines in DM.DIEM (segment metres from CAD); V is LIVE from heights
 //    (flag AW/AX x current drop), so height/ceiling edits keep deductions consistent. Owners are validated by key (MATCH + EXACT) and must be PASS.
 //    A participating row (AV=1) whose plan length, routing mode, conduit code, floor or factor differs from the CAD-time value, a changed
 //    conduit mode, or edited/deleted evidence -> CHECK "recalculate in CAD"; the old deduction is never silently reused.
 //    Known limits: keys are matched with MATCH (wildcard-sensitive) but every hit is confirmed with EXACT, so a mismatch fails safe (CHECK).
 //    Overwriting a formula cell (e.g. typing PASS into AE) is not protected (no sheet protection was requested).
 //  - W/X tolerance: a negative W or X is CHECK only below -1E-9 m (binary roundoff of a fully shared row), never a design tolerance.
 // TVXD-MEPS-ROOMQTY-037 NF-037-HEIGHT-UI: optional room height setup (pure data, no Forms). It is a convenience PROFILE for the user; the calculation
 // and the workbook formulas always use the actual per-device declarations it filled in, never these values directly.
 //  floorId: floor name typed by the user (null = not entered). storeyHeight: floor-to-next-floor height (m), METADATA ONLY for future inter-floor routing;
 //  the same-floor pilot never uses it in any length. wireCeilingHeight: wiring ceiling above the finished floor (m) = the rows' ceilingHeight when applied.
 //  groups: one entry per authoritative device type SWITCH/SOCKET/LAMP/FAN/PANEL (OTHER is never grouped); apply = the user ticked it for the last Apply.
 //  No value is ever defaulted: null = not entered.
 public sealed class RoomHeightGroup037 {public string deviceType,heightMode;public bool apply;public double? value;}
 public sealed class RoomHeightSettings037 {
  public string floorId;public double? storeyHeight,wireCeilingHeight;public bool onlyFillMissing=true;
  public List<RoomHeightGroup037> groups=new List<RoomHeightGroup037>();
 }
 // S4 (additive): one official THKL total of a single-room package. cell = THKL address (D/F/G + row); value = cached model value (PASS rows only);
 // used = at least one row of the room declares this code (the source codes listed by default are present with used=false and value 0).
 public sealed class RoomXlsxTotal037 {public string kind,code,unit,cell;public double value;public bool used;}
 // S4: bytes of the single-room workbook + where its official totals are. lastRow = last BDGKL data row (rows 12..lastRow).
 public sealed class RoomXlsxPackage037 {public byte[] bytes;public int lastRow;public List<RoomXlsxTotal037> totals=new List<RoomXlsxTotal037>();}
 public static class RoomHeight037 {
  public static readonly string[] Types={RoomQty037.Switch,RoomQty037.Socket,RoomQty037.Lamp,RoomQty037.Fan,RoomQty037.Panel};
  public static string TypeName(string t){return t==RoomQty037.Switch?"Công tắc":t==RoomQty037.Socket?"Ổ cắm":t==RoomQty037.Lamp?"Đèn":t==RoomQty037.Fan?"Quạt":
   t==RoomQty037.Panel?"Tủ điện":t==RoomQty037.Other?"Thiết bị chưa phân loại":t??"(không rõ loại)";}
  // All five groups, nothing applied, no values (the starting state; never a numeric default).
  public static RoomHeightSettings037 Empty(){var s=new RoomHeightSettings037();foreach(var t in Types)s.groups.Add(new RoomHeightGroup037{deviceType=t});return s;}
  public static RoomHeightSettings037 Copy(RoomHeightSettings037 s){if(s==null)return null;
   var c=new RoomHeightSettings037{floorId=s.floorId,storeyHeight=s.storeyHeight,wireCeilingHeight=s.wireCeilingHeight,onlyFillMissing=s.onlyFillMissing};
   if(s.groups!=null)foreach(var g in s.groups)c.groups.Add(g==null?null:new RoomHeightGroup037{deviceType=g.deviceType,heightMode=g.heightMode,apply=g.apply,value=g.value});
   return c;}
  static bool Fin(double v){return !double.IsNaN(v)&&!double.IsInfinity(v);}
  // null = valid; otherwise the first problem (Vietnamese). Structural + range checks only; nothing is corrected.
  public static string Problem(RoomHeightSettings037 s){
   if(s==null)return "Thiếu thiết lập cao độ.";
   if(s.floorId!=null){var p=Bdgkl034.TextProblem(s.floorId);if(p!=null)return "Tên tầng "+p+".";}
   if(s.storeyHeight.HasValue&&(!Fin(s.storeyHeight.Value)||!(s.storeyHeight.Value>0)))return "Chiều cao tầng phải là số > 0 (hoặc để trống).";
   if(s.wireCeilingHeight.HasValue&&(!Fin(s.wireCeilingHeight.Value)||!(s.wireCeilingHeight.Value>0)))return "Cao độ trần đi dây phải là số > 0 (hoặc để trống).";
   if(s.groups==null)return "Thiếu danh sách nhóm thiết bị.";
   if(s.groups.Count>Types.Length)return "Quá nhiều nhóm thiết bị.";
   var seen=new HashSet<string>(StringComparer.Ordinal);
   foreach(var g in s.groups){
    if(g==null)return "Nhóm thiết bị rỗng.";
    if(Array.IndexOf(Types,g.deviceType)<0)return "Loại nhóm \""+g.deviceType+"\" không thuộc Công tắc/Ổ cắm/Đèn/Quạt/Tủ điện.";
    if(!seen.Add(g.deviceType))return "Nhóm "+TypeName(g.deviceType)+" bị lặp.";
    if(g.heightMode!=null&&g.heightMode!=Bdgkl034.Fixed&&g.heightMode!=Bdgkl034.CeilingOffset)return "Cách khai cao độ nhóm "+TypeName(g.deviceType)+" không hợp lệ.";
    if(g.value.HasValue&&(!Fin(g.value.Value)||g.value.Value<0))return "Cao độ nhóm "+TypeName(g.deviceType)+" phải là số ≥ 0.";
    if(g.apply&&g.heightMode==null)return "Nhóm "+TypeName(g.deviceType)+" được chọn áp dụng nhưng chưa chọn cách khai cao độ.";
    if(g.apply&&!g.value.HasValue)return "Nhóm "+TypeName(g.deviceType)+" được chọn áp dụng nhưng chưa nhập giá trị.";}
   return null;}
 }
 public static class RoomXlsx037 {
  public const string SheetMain="BDGKL",SheetInput="DM.DIEM",SheetTotals="THKL";
  const int First=12,MaxCellText=32767,MaxFormula=8192;
  const string D="'DM.DIEM'!";
  static readonly string[] SrcWire={"2x10+E10","2x6+E6","2x4+E4","2x2.5+E2.5","2x1.5"};
  static readonly string[] SrcPipe={"D32","D25","D20"};
  static readonly CultureInfo Inv=CultureInfo.InvariantCulture;
  static readonly StringComparer O=StringComparer.Ordinal;
  const string Ns="http://schemas.openxmlformats.org/spreadsheetml/2006/main",NsR="http://schemas.openxmlformats.org/officeDocument/2006/relationships";
  const int SHead=1,SText=2,SNum=3,SInText=4,SInNum=5,SInt=6,SInInt=7,STitle=8,SHText=9,SHNum=10,SHInt=11,SStatus=12,SNote=13,SCode=14;
  // Visual layout only (NF-037-XLSX-LAYOUT): Arial 11, vertical centre, wrapped text, explicit row heights, merged 2-level headers, hidden helper columns,
  // A3 landscape for BDGKL/DM.DIEM without fit-to-page shrinking. Formula text, cell addresses and cached values are unchanged by this layer.
  const int PaperA4=9,PaperA3=8;

  // S4 (additive; the single-room workbook bytes, formulas and layout are unchanged): DM.DIEM height-section texts, exposed so the grouped project
  // workbook (RoomProjectExport037) can locate and replace ONLY the legacy diagnostic note by exact text and position.
  public const string HeightSectionTitle="THIẾT LẬP CAO ĐỘ TẦNG VÀ THIẾT BỊ (chỉ để truy vết – công thức dùng cao độ THỰC của từng thiết bị ở bảng thiết bị phía trên, cột E:G)";
  public const string LegacyHeightNote="Phòng này chưa dùng hộp 'Cao độ tầng và thiết bị'; cao độ được khai riêng từng thiết bị.";
  // S4 package totals: kinds of the official THKL cells (D = wire after factor, F = hidden conduit after factor, G = exposed conduit after factor).
  public const string KindWire="WIRE",KindHidden="CONDUIT_HIDDEN",KindExposed="CONDUIT_EXPOSED",UnitMetre="m",TotalsStatusCell="B4";

  // S5 adjustments (RoomAdjustments037: BO DONG exclusions / BO SUNG manual supplements):
  //  - Write / BuildBytes / BuildPackage (legacy single-room APIs) REFUSE a batch that carries any exclusion or supplement record (active or not),
  //    because the single-room workbook cannot show the manual quantities and the audit. Use the grouped project export (RoomProjectExport037).
  //    A batch with adjustments == null or empty lists is exported exactly as before.
  //  - BuildMeasuredPackage = MEASURED-ONLY package for the grouped export: same recalculation and PASS guard; the BDGKL / DM.DIEM / THKL sheets
  //    contain only the ACTIVE rows (excluded rows have result == null and are left out; sharing was already allocated by Calculate over the
  //    active rows of the complete source, nothing is recomputed here). Manual supplements are NOT in this package; the caller must show them and
  //    the exclusion audit. Without adjustments its bytes equal BuildBytes.
  public static bool HasAdjustments(RoomCompletionBatch037 b){var a=b==null?null:b.adjustments;
   return a!=null&&((a.exclusions!=null&&a.exclusions.Count>0)||(a.supplements!=null&&a.supplements.Count>0));}
  static void NoAdjustments(RoomCompletionBatch037 b){
   if(HasAdjustments(b))throw new InvalidOperationException("Export refused: this room has BO DONG / BO SUNG records (excluded source rows or manual supplements); "+
    "the single-room workbook cannot show them. Use the project export (CAD / BO SUNG / TONG columns with the exclusion and supplement audit).");}
  // Rows of the measured workbook: every non-excluded row in presentation order (SORT-039: RoomRowOrder037 floor / natural circuit / batch order, a
  // copy of references; b.rows is never reordered). Every sheet, span and row index uses this one list; owners are matched by rowKey (MATCH + EXACT).
  static List<RoomCompletionRow037> Active(RoomCompletionBatch037 b){return RoomRowOrder037.Sorted(b.rows.Where(r=>!r.excluded));}

  public static void Write(string path,RoomCompletionBatch037 batch){Write(path,batch,null);}
  // settings: optional room height profile, exported as an appended DM.DIEM section only (traceability); it never changes any formula or address.
  public static void Write(string path,RoomCompletionBatch037 batch,RoomHeightSettings037 settings){
   if(batch==null)throw new ArgumentNullException("batch");
   settings=Checked(settings);
   PathCheck(path);
   NoAdjustments(batch);
   PassOnly(batch);
   if(File.Exists(path)||Directory.Exists(path))throw new IOException("Export refused: \""+path+"\" already exists; MEPS never overwrites an output file.");
   byte[] bytes=Build(batch,settings);
   WriteNew(path,bytes);}
  static RoomHeightSettings037 Checked(RoomHeightSettings037 settings){if(settings==null)return null;
   var hp=RoomHeight037.Problem(settings);if(hp!=null)throw new ArgumentException("Height settings invalid: "+hp,"settings");return RoomHeight037.Copy(settings);}
  static void PathCheck(string path){
   if(string.IsNullOrWhiteSpace(path))throw new ArgumentException("Output path is required.","path");
   if(!string.Equals(Path.GetExtension(path),".xlsx",StringComparison.OrdinalIgnoreCase))throw new ArgumentException("Output file must end with .xlsx.","path");}
  static void PassOnly(RoomCompletionBatch037 batch){
   RoomCompletion037.Calculate(batch);
   if(batch.status!=RoomCompletion037.Pass)
    throw new InvalidOperationException("Export refused: batch status is "+(batch.status??"(null)")+", not PASS ("+
     string.Join(" | ",(batch.issues??new List<string>()).Concat(batch.missing??new List<string>()).Take(5))+").");}
  // S4: the single-room package in memory (same checks and bytes as Write: recalculated, PASS only). No file IO.
  public static byte[] BuildBytes(RoomCompletionBatch037 batch,RoomHeightSettings037 settings){
   if(batch==null)throw new ArgumentNullException("batch");
   settings=Checked(settings);NoAdjustments(batch);PassOnly(batch);return Build(batch,settings);}
  // S4: BuildBytes + the addresses/cached values of the official THKL totals of that package (same code lists and row order as the THKL sheet).
  public static RoomXlsxPackage037 BuildPackage(RoomCompletionBatch037 batch,RoomHeightSettings037 settings){return Package(batch,BuildBytes(batch,settings));}
  // S5: measured-only package (see the S5 note above). Refusals as BuildBytes, except that adjustments are accepted.
  public static RoomXlsxPackage037 BuildMeasuredPackage(RoomCompletionBatch037 batch,RoomHeightSettings037 settings){
   if(batch==null)throw new ArgumentNullException("batch");
   settings=Checked(settings);PassOnly(batch);return Package(batch,Build(batch,settings));}
  static RoomXlsxPackage037 Package(RoomCompletionBatch037 batch,byte[] bytes){var rows=Active(batch);
   var p=new RoomXlsxPackage037{bytes=bytes,lastRow=First+rows.Count-1};
   List<string> wire,pipe;TotalCodes(batch,out wire,out pipe);int w1=9,c1=w1+wire.Count+3+3;
   for(int k=0;k<wire.Count;k++){string code=wire[k];double aft=0;bool used=false;
    foreach(var r in rows)if(r.input.assemblyCode==code){aft+=r.result.routeAfterFactorM.Value;used=true;}
    p.totals.Add(new RoomXlsxTotal037{kind=KindWire,code=code,unit=UnitMetre,cell="D"+I(w1+k),value=aft,used=used});}
   for(int k=0;k<pipe.Count;k++){string code=pipe[k];double h=0,e=0;bool used=false;
    foreach(var r in rows)if(r.input.conduitCode==code){h+=r.result.conduitHiddenAfterFactorM.Value;e+=r.result.conduitExposedAfterFactorM.Value;used=true;}
    p.totals.Add(new RoomXlsxTotal037{kind=KindHidden,code=code,unit=UnitMetre,cell="F"+I(c1+k),value=h,used=used});
    p.totals.Add(new RoomXlsxTotal037{kind=KindExposed,code=code,unit=UnitMetre,cell="G"+I(c1+k),value=e,used=used});}
   return p;}
  // S4: creates a NEW .xlsx file from bytes built beforehand (FileMode.CreateNew; an existing file is never opened for writing or replaced; a partially
  // written new file is deleted). Shared by Write and the grouped project export.
  public static void WriteNew(string path,byte[] bytes){
   if(bytes==null||bytes.Length==0)throw new ArgumentException("Workbook bytes are required.","bytes");
   PathCheck(path);
   if(File.Exists(path)||Directory.Exists(path))throw new IOException("Export refused: \""+path+"\" already exists; MEPS never overwrites an output file.");
   var f=new FileStream(path,FileMode.CreateNew,FileAccess.Write,FileShare.None);   // throws if the name is taken; the existing file is untouched
   bool done=false;
   try{f.Write(bytes,0,bytes.Length);f.Flush(true);done=true;}
   finally{f.Dispose();if(!done){try{File.Delete(path);}catch(IOException){}catch(UnauthorizedAccessException){}}}}

  // ---- cell writing ----
  sealed class Sheet {public readonly SortedDictionary<int,SortedDictionary<int,string>> cells=new SortedDictionary<int,SortedDictionary<int,string>>();
   public readonly Dictionary<int,double> heights=new Dictionary<int,double>();
   public readonly List<string> merges=new List<string>();
   public void Put(int r,int c,string xml){SortedDictionary<int,string> row;if(!cells.TryGetValue(r,out row))cells[r]=row=new SortedDictionary<int,string>();row[c]=xml;}
   // Merge r1:c1..r2:c2; every covered cell that has no content gets the anchor style so borders/fill stay continuous.
   public void Merge(int r1,int c1,int r2,int c2,int st){merges.Add(Ref(r1,c1)+":"+Ref(r2,c2));
    for(int r=r1;r<=r2;r++)for(int c=c1;c<=c2;c++){if(r==r1&&c==c1)continue;SortedDictionary<int,string> row;if(cells.TryGetValue(r,out row)&&row.ContainsKey(c))continue;Blank(this,r,c,st);}}
   public void Height(int r,double h){double old;if(!heights.TryGetValue(r,out old)||h>old)heights[r]=h;}
   public string MergeXml(){if(merges.Count==0)return "";var b=new StringBuilder("<mergeCells count=\""+I(merges.Count)+"\">");foreach(var m in merges)b.Append("<mergeCell ref=\""+m+"\"/>");return b.Append("</mergeCells>").ToString();}}
  // Estimated row height (pt) for wrapped Arial 11 text in a column of the given width (Excel width units); explicit lines count too.
  static double TextHeight(string t,double width){if(string.IsNullOrEmpty(t))return 18;int lines=0;double per=Math.Max(1,width*0.95);
   foreach(var part in t.Split('\n'))lines+=Math.Max(1,(int)Math.Ceiling(part.Length*1.1/per));return Math.Min(409,Math.Max(18,lines*15.0+4));}
  static string I(int v){return v.ToString(Inv);}
  static int C(string a){int n=0;foreach(char ch in a)n=n*26+(ch-'A'+1);return n;}
  static string Col(int c){var s="";while(c>0){int m=(c-1)%26;s=(char)('A'+m)+s;c=(c-1)/26;}return s;}
  static string Ref(int r,int c){return Col(c)+I(r);}
  static string Num(double v){if(double.IsNaN(v)||double.IsInfinity(v))throw new InvalidOperationException("Export refused: non-finite number.");return v.ToString("R",Inv);}
  static string X(string s){var b=new StringBuilder(s.Length+8);
   for(int i=0;i<s.Length;i++){char c=s[i];
    if(char.IsHighSurrogate(c)&&i+1<s.Length&&char.IsLowSurrogate(s[i+1])){b.Append(c).Append(s[i+1]);i++;continue;}
    if(!XmlConvert.IsXmlChar(c))throw new InvalidOperationException("Export refused: text contains character U+"+((int)c).ToString("X4",Inv)+" that XLSX cannot store.");
    switch(c){case '&':b.Append("&amp;");break;case '<':b.Append("&lt;");break;case '>':b.Append("&gt;");break;case '"':b.Append("&quot;");break;default:b.Append(c);break;}}
   return b.ToString();}
  static void Blank(Sheet s,int r,int c,int st){s.Put(r,c,"<c r=\""+Ref(r,c)+"\" s=\""+I(st)+"\"/>");}
  static void Txt(Sheet s,int r,int c,string t,int st){if(string.IsNullOrEmpty(t)){Blank(s,r,c,st);return;}
   if(t.Length>MaxCellText)throw new InvalidOperationException("Export refused: text in "+Ref(r,c)+" exceeds "+I(MaxCellText)+" characters.");
   s.Put(r,c,"<c r=\""+Ref(r,c)+"\" s=\""+I(st)+"\" t=\"inlineStr\"><is><t xml:space=\"preserve\">"+X(t)+"</t></is></c>");}
  static void Val(Sheet s,int r,int c,double? v,int st){if(!v.HasValue){Blank(s,r,c,st);return;}s.Put(r,c,"<c r=\""+Ref(r,c)+"\" s=\""+I(st)+"\"><v>"+Num(v.Value)+"</v></c>");}
  // Formula cell; cached number when known, else cached text (may be empty).
  static void Fx(Sheet s,int r,int c,string f,int st,double? num,string text){
   if(f.Length>MaxFormula)throw new InvalidOperationException("Export refused: formula in "+Ref(r,c)+" is too long.");
   string head="<c r=\""+Ref(r,c)+"\" s=\""+I(st)+"\"";
   if(num.HasValue)s.Put(r,c,head+"><f>"+X(f)+"</f><v>"+Num(num.Value)+"</v></c>");
   else s.Put(r,c,head+" t=\"str\"><f>"+X(f)+"</f><v>"+X(text??"")+"</v></c>");}
  static string Chain(string final,params string[] pairs){var b=new StringBuilder();
   for(int i=0;i<pairs.Length;i+=2)b.Append("IF(").Append(pairs[i]).Append(",\"").Append(pairs[i+1]).Append("\",");
   b.Append(final).Append(')',pairs.Length/2);return b.ToString();}

  // ---- layout ----
  sealed class Lay {public int n,L,fs,fe,dh,ds,de,rh,rs,re,eh,es,ee,nh;}
  sealed class Dev {public string id,type,label,floor,mode;public double? fixedH,offset;}
  sealed class Ev {public string dep,owner,role,kind,item,side;public double metres;}
  sealed class Span {public int first,count;}
  static string Tk(string f,Lay y,int r,int q,int e1,int e2){
   return f.Replace("{D}",D).Replace("{r}",I(r)).Replace("{q}",I(q)).Replace("{fs}",I(y.fs)).Replace("{fe}",I(y.fe)).Replace("{ds}",I(y.ds))
    .Replace("{de}",I(y.de)).Replace("{L}",I(y.L)).Replace("{e1}",I(e1)).Replace("{e2}",I(e2));}
  static void AddDev(SortedDictionary<string,Dev> m,string id,string type,string label,string floor,string mode,double? h,double? o){
   if(id==null||m.ContainsKey(id))return;m[id]=new Dev{id=id,type=type,label=label,floor=floor,mode=mode,fixedH=h,offset=o};}
  static string DevLabel(string label,string name){return string.IsNullOrWhiteSpace(name)?label:(label??"?")+" – "+name;}

  static byte[] Build(RoomCompletionBatch037 b,RoomHeightSettings037 settings){
   var rows=Active(b);int n=rows.Count;
   var floors=new List<string>();var ceil=new Dictionary<string,double>(O);
   foreach(var r in rows){var f=r.input.floorId;if(!ceil.ContainsKey(f)){ceil[f]=r.result.ceilingHeightM.Value;floors.Add(f);}}
   floors.Sort(string.CompareOrdinal);
   var devs=new SortedDictionary<string,Dev>(O);
   // S6: BDGKL C/D carry the compact row labels; the DM.DIEM device table keeps compact label + CAD block name for traceability (display only).
   foreach(var r in rows){var d=r.input;
    AddDev(devs,r.startId,r.startType,DevLabel(r.startLabel,r.startName),d.floorId,d.startHeightMode,d.startFixedHeight,d.startCeilingOffset);
    AddDev(devs,r.endId,r.endType,DevLabel(r.endLabel,r.endName),d.floorId,d.endHeightMode,d.endFixedHeight,d.endCeilingOffset);}
   var dl=devs.Values.ToList();
   var ev=new List<Ev>();var spans=new Span[n];
   for(int i=0;i<n;i++){var r=rows[i];var sp=new Span{first=ev.Count};
    foreach(var h in r.horizontalShares)if(h.role==RoomCompletion037.RoleDeducted)
     ev.Add(new Ev{dep=r.rowKey,owner=h.ownerRowKey,role="DEDUCT",kind="HSEG",item=h.segmentId,side="",metres=h.lengthMetres.Value});
    foreach(var v in r.verticalShares)if(v.role==RoomCompletion037.RoleDeducted)
     ev.Add(new Ev{dep=r.rowKey,owner=v.ownerRowKey,role="DEDUCT",kind="VDROP",item=v.deviceId,side=v.side,metres=v.dropMetres.Value});
    sp.count=ev.Count-sp.first;spans[i]=sp;}
   foreach(var r in rows){
    foreach(var h in r.horizontalShares)if(h.role==RoomCompletion037.RoleOwner)
     ev.Add(new Ev{dep=r.rowKey,owner=r.rowKey,role="OWNER",kind="HSEG",item=h.segmentId,side="",metres=h.lengthMetres.Value});
    foreach(var v in r.verticalShares)if(v.role==RoomCompletion037.RoleOwner)
     ev.Add(new Ev{dep=r.rowKey,owner=r.rowKey,role="OWNER",kind="VDROP",item=v.deviceId,side=v.side,metres=v.dropMetres.Value});}
   var y=new Lay{n=n,L=First+n-1,fs=9};y.fe=y.fs+floors.Count-1;y.dh=y.fe+2;y.ds=y.dh+1;y.de=y.ds+dl.Count-1;y.rh=y.de+2;y.rs=y.rh+1;y.re=y.rs+n-1;
   y.eh=y.re+2;y.es=y.eh+1;y.ee=y.es+Math.Max(ev.Count,1)-1;y.nh=y.ee+2;
   var main=Main(b,y,spans,dl);var input=Input(b,y,floors,ceil,dl,ev,settings);var tot=Totals(b,y);
   using(var ms=new MemoryStream()){
    using(var z=new ZipArchive(ms,ZipArchiveMode.Create,true)){
     Entry(z,"[Content_Types].xml",ContentTypes());
     Entry(z,"_rels/.rels","<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">"+
      "<Relationship Id=\"rId1\" Type=\""+NsR+"/officeDocument\" Target=\"xl/workbook.xml\"/></Relationships>");
     Entry(z,"xl/workbook.xml",Workbook(y));
     Entry(z,"xl/_rels/workbook.xml.rels","<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">"+
      "<Relationship Id=\"rId1\" Type=\""+NsR+"/worksheet\" Target=\"worksheets/sheet1.xml\"/><Relationship Id=\"rId2\" Type=\""+NsR+"/worksheet\" Target=\"worksheets/sheet2.xml\"/>"+
      "<Relationship Id=\"rId3\" Type=\""+NsR+"/worksheet\" Target=\"worksheets/sheet3.xml\"/><Relationship Id=\"rId4\" Type=\""+NsR+"/styles\" Target=\"styles.xml\"/></Relationships>");
     Entry(z,"xl/styles.xml",Styles());
     Entry(z,"xl/worksheets/sheet1.xml",main);Entry(z,"xl/worksheets/sheet2.xml",input);Entry(z,"xl/worksheets/sheet3.xml",tot);}
    return ms.ToArray();}}
  static void Entry(ZipArchive z,string name,string text){var bytes=new UTF8Encoding(false).GetBytes(text);
   using(var s=z.CreateEntry(name,CompressionLevel.Optimal).Open())s.Write(bytes,0,bytes.Length);}

  // ---- BDGKL ----
  static readonly string[] Head={"STT","Hạng mục / Diễn giải","Điểm đầu","Điểm cuối","Hướng đi","KC mặt bằng (m)","H sàn→trần (m)","Cao độ điểm đầu (m)",
   "Cao độ điểm cuối (m)","Đoạn đứng lên/xuống (m)","Số đầu chờ","Dây chờ (m)","Tổng dài tuyến dây (m)","Loại dây","DÂY DẪN Cu/PVC (m tuyến)","","","","",
   "Cỡ ống","Trùng ống ngang (m)","Trùng ống đứng (m)","Ống đoạn ngang (m)","Ống đoạn đứng (m)","ỐNG ĐI CHÌM – ÂM TƯỜNG (đoạn đứng)","","",
   "ỐNG ĐI NỔI – TRÊN TRẦN/SÀN (đoạn ngang)","","","Trạng thái","Ghi chú / nguồn","Loại điểm đầu \n(tự nhận)","Loại điểm cuối \n(tự nhận)",
   // helper columns AI..BQ (check / provenance, not edited)
   "Khóa dòng (rowKey)","Dòng khai báo DM.DIEM","Mã TB đầu (CAD)","Mã TB cuối (CAD)","Chỉ số TB đầu","Chỉ số TB cuối","KT cao độ đầu","KT cao độ cuối",
   "F lúc tính CAD (m)","Hướng đi lúc tính","Mã ống lúc tính","Tầng lúc tính","Hệ số lúc tính","Tham gia dùng chung (1/0)","Trừ ống đứng phía đầu (1/0)",
   "Trừ ống đứng phía cuối (1/0)","Số dòng trừ lúc tính","Dòng chứng cứ đầu","Dòng chứng cứ cuối","Số dòng trừ hiện có","Dòng sở hữu không PASS","Hệ số uốn cong",
   "M sau hệ số (m)","Ống chìm (m)","Ống nổi (m)","Ống chìm sau hệ số (m)","Ống nổi sau hệ số (m)","Tính: M (chỉ PASS)","Tính: M sau hệ số","Tính: ống chìm",
   "Tính: ống nổi","Tính: ống chìm sau HS","Tính: ống nổi sau HS","Lý do CHECK","Lý do thiếu dữ liệu"};
  static string TextBad(string x){return "AND("+x+"<>\"\",OR(LEN("+x+")>100,LEFT("+x+",1)=\" \",RIGHT("+x+",1)=\" \"))";}
  static string Main(RoomCompletionBatch037 b,Lay y,Span[] spans,List<Dev> dl){var s=new Sheet();var rows=Active(b);
   string room=rows[0].roomId;
   Txt(s,1,1,"BẢNG DIỄN GIẢI KHỐI LƯỢNG DÂY DẪN VÀ ỐNG LUỒN – PHÒNG "+room,STitle);
   Txt(s,2,1,"Căn cứ: chiều dài đo trên CAD (đoạn dây truy vết trong phòng) + khai báo tại sheet DM.DIEM. PASS = đủ dữ liệu, KHÔNG phải phê duyệt kỹ thuật.",SNote);
   Txt(s,3,1,"INPUT: chỉ sửa ô chữ xanh tại DM.DIEM. Cột AI→BQ: kiểm tra/nguồn, không sửa. Hướng đi: CEILING = lên trần rồi xuống; FLOOR = xuống sàn rồi lên; WALL = đoạn đứng |Hđầu − Hcuối|.",SNote);
   Txt(s,4,1,"Chế độ ống lúc tính CAD: "+b.conduitMode+" (SEPARATE = mỗi dòng một ống riêng; SHARED = người dùng khai báo đoạn trùng vật lý và đoạn xuống thiết bị đi chung ống).",SNote);
   Txt(s,5,1,"Dây = m tuyến của bộ dây khai báo cho mỗi dòng (không nhân số lõi, không suy luận dây N/PE dùng chung). Dây chờ không tính ống. Hệ số uốn cong áp một lần.",SNote);
   Txt(s,6,1,"Nhãn TAG trên bản vẽ là nhãn kênh điều khiển cục bộ trong phòng, không phải lộ/tủ suy luận; lộ cấu trúc (tủ/lộ) được ưu tiên khi có.",SNote);
   Txt(s,7,1,"U/V chỉ có khi SHARED; chứng cứ từng đoạn/đầu thiết bị và dòng sở hữu ở DM.DIEM. Sửa chiều dài/hướng đi/mã ống/tầng/hệ số của dòng dùng chung → CHECK, phải tính lại trong CAD.",SNote);
   Txt(s,9,1,"Chữ xanh = INPUT (DM.DIEM) · chữ đen = công thức · Trạng thái: PASS / MISSING DATA / CHECK · lỗi công thức = CHECK",SNote);
   // Title/notes span the printed source columns A:AH (helpers AI:BQ are hidden).
   s.Merge(1,1,1,C("AH"),STitle);s.Height(1,30);
   for(int r=2;r<=9;r++){if(r==8)continue;s.Merge(r,1,r,C("AH"),SNote);s.Height(r,22);}
   // S5: row 8 is written only for a batch with adjustments (measured-only package); without adjustments it stays empty as before.
   if(HasAdjustments(b)){int manual=b.adjustments.supplements==null?0:b.adjustments.supplements.Count(a=>a!=null&&a.active);
    Txt(s,8,1,"CHỈ DÒNG ĐO CAD ĐANG TÍNH ("+I(rows.Count)+" dòng). "+I(b.excluded)+" dòng nguồn đã BỎ (không có khối lượng, không tham gia phân bổ ống dùng chung) và "+
     I(manual)+" mục BỔ SUNG thủ công KHÔNG nằm ở sheet này – xem sheet TONG_CAD_BO_SUNG / BO_SUNG / BO_DONG của tệp dự án.",SNote);
    s.Merge(8,1,8,C("AH"),SNote);s.Height(8,22);}
   for(int c=1;c<=Head.Length;c++)Txt(s,10,c,Head[c-1],SHead);
   s.Height(10,48);s.Height(11,34);
   for(int c=1;c<=Head.Length;c++)Blank(s,11,c,SHead);
   for(int k=0;k<5;k++)Txt(s,11,C("O")+k,SrcWire[k],SHead);
   for(int k=0;k<3;k++){Txt(s,11,C("Y")+k,SrcPipe[k],SHead);Txt(s,11,C("AB")+k,SrcPipe[k],SHead);}
   // 2-level header: groups O:S, Y:AA, AB:AD on row 10 with sub-headers on row 11; every other header spans rows 10:11.
   s.Merge(10,C("O"),10,C("S"),SHead);s.Merge(10,C("Y"),10,C("AA"),SHead);s.Merge(10,C("AB"),10,C("AD"),SHead);
   for(int c=1;c<=Head.Length;c++){bool grouped=(c>=C("O")&&c<=C("S"))||(c>=C("Y")&&c<=C("AD"));if(!grouped)s.Merge(10,c,11,c,SHead);}
   string fm="MATCH({D}$I{q},{D}$A${fs}:$A${fe},0)";
   string fE="IF({D}$L{q}=\"\",\"\",{D}$L{q})",fF="IF({D}$H{q}=\"\",\"\",{D}$H{q})";
   string fG="IF({D}$I{q}=\"\",\"\",IFERROR(IF(EXACT(INDEX({D}$A${fs}:$A${fe},"+fm+"),{D}$I{q}),IF(INDEX({D}$B${fs}:$B${fe},"+fm+")=\"\",\"\",INDEX({D}$B${fs}:$B${fe},"+fm+")),\"NO FLOOR\"),\"NO FLOOR\"))";
   string fJ="IF(AND(ISNUMBER($G{r}),ISNUMBER($H{r}),ISNUMBER($I{r})),IF(EXACT($E{r},\"CEILING\"),($G{r}-$H{r})+($G{r}-$I{r}),IF(EXACT($E{r},\"FLOOR\"),$H{r}+$I{r},IF(EXACT($E{r},\"WALL\"),ABS($H{r}-$I{r}),\"\"))),\"\")";
   string fK="IF({D}$M{q}=\"\",\"\",{D}$M{q})",fL="IF(AND(ISNUMBER($K{r}),ISNUMBER({D}$N{q})),$K{r}*{D}$N{q},\"\")";
   string fM="IF(AND(ISNUMBER($F{r}),ISNUMBER($J{r}),ISNUMBER($L{r})),$F{r}+$J{r}+$L{r},\"\")";
   string fN="IF({D}$P{q}=\"\",\"\",{D}$P{q})",fT="IF({D}$Q{q}=\"\",\"\",{D}$Q{q})";
   string fV="IF(AND($AW{r}=0,$AX{r}=0),0,IF(AND(ISNUMBER($G{r}),ISNUMBER($H{r}),ISNUMBER($I{r})),IF(EXACT($E{r},\"CEILING\"),$AW{r}*($G{r}-$H{r})+$AX{r}*($G{r}-$I{r}),IF(EXACT($E{r},\"FLOOR\"),$AW{r}*$H{r}+$AX{r}*$I{r},\"\")),\"\"))";
   string fW="IF(AND(ISNUMBER($F{r}),ISNUMBER($U{r})),$F{r}-$U{r},\"\")",fX="IF(AND(ISNUMBER($J{r}),ISNUMBER($V{r})),$J{r}-$V{r},\"\")";
   string fAE="IFERROR(IF($BP{r}<>\"\",\"CHECK\",IF($BQ{r}<>\"\",\"MISSING DATA\",\"PASS\")),\"CHECK\")";
   string fBD="IF({D}$O{q}=\"\",\"\",{D}$O{q})",fBE="IF(AND(ISNUMBER($M{r}),ISNUMBER($BD{r})),$M{r}*$BD{r},\"\")";
   string fBF="IF(AND(ISNUMBER($W{r}),ISNUMBER($X{r})),IF(EXACT($E{r},\"WALL\"),$W{r}+$X{r},$X{r}),\"\")",fBG="IF(ISNUMBER($W{r}),IF(EXACT($E{r},\"WALL\"),0,$W{r}),\"\")";
   string fBH="IF(AND(ISNUMBER($BF{r}),ISNUMBER($BD{r})),$BF{r}*$BD{r},\"\")",fBI="IF(AND(ISNUMBER($BG{r}),ISNUMBER($BD{r})),$BG{r}*$BD{r},\"\")";
   string fBP="IFERROR("+Chain("\"\"",
    "AND({D}$B$4<>\"\",NOT(EXACT({D}$B$4,\"YES\")))","CHECK ASSEMBLY_CONFIRMATION_INVALID",
    "AND({D}$B$5<>\"\",NOT(OR(EXACT({D}$B$5,\"SEPARATE\"),EXACT({D}$B$5,\"SHARED\"))))","CHECK CONDUIT_MODE_INVALID",
    "AND({D}$B$5<>\"\",NOT(EXACT({D}$B$5,{D}$C$5)))","CHECK CONDUIT_MODE_CHANGED_RECALCULATE_IN_CAD",
    "{D}$B$6<>0","CHECK CAD_BLOCKERS",
    "AND($F{r}<>\"\",OR(NOT(ISNUMBER($F{r})),N($F{r})<=0))","CHECK PLAN_LENGTH_INVALID",
    "AND($E{r}<>\"\",NOT(OR(EXACT($E{r},\"CEILING\"),EXACT($E{r},\"FLOOR\"),EXACT($E{r},\"WALL\"))))","CHECK ROUTING_MODE_INVALID",
    "$G{r}=\"NO FLOOR\"","CHECK FLOOR_NOT_IN_FLOOR_TABLE",
    "AND($G{r}<>\"\",OR(NOT(ISNUMBER($G{r})),N($G{r})<=0))","CHECK CEILING_HEIGHT_INVALID",
    "$AO{r}=\"CHECK\"","CHECK START_HEIGHT_INVALID",
    "$AP{r}=\"CHECK\"","CHECK END_HEIGHT_INVALID",
    "AND($K{r}<>\"\",OR(NOT(ISNUMBER($K{r})),N($K{r})<0,N($K{r})<>INT(N($K{r}))))","CHECK TAIL_COUNT_INVALID",
    "AND({D}$N{q}<>\"\",OR(NOT(ISNUMBER({D}$N{q})),N({D}$N{q})<0))","CHECK TAIL_LENGTH_INVALID",
    "AND($BD{r}<>\"\",OR(NOT(ISNUMBER($BD{r})),N($BD{r})<1))","CHECK FACTOR_INVALID",
    "OR("+TextBad("{D}$I{q}")+","+TextBad("{D}$K{q}")+","+TextBad("$N{r}")+","+TextBad("$T{r}")+")","CHECK TEXT_INVALID",
    "AND(ISNUMBER($W{r}),N($W{r})<-0.000000001)","CHECK HORIZONTAL_DEDUCTION_EXCEEDS",
    "AND(ISNUMBER($X{r}),N($X{r})<-0.000000001)","CHECK VERTICAL_DEDUCTION_EXCEEDS",
    "AND($AV{r}=1,ABS(N($F{r})-$AQ{r})>0.000000001)","CHECK SHARED_GEOMETRY_CHANGED_RECALCULATE_IN_CAD",
    "AND($AV{r}=1,OR(NOT(EXACT($E{r},$AR{r})),NOT(EXACT($T{r},$AS{r})),NOT(EXACT({D}$I{q},$AT{r}))))","CHECK SHARED_DECLARATION_CHANGED_RECALCULATE_IN_CAD",
    "AND($AV{r}=1,N($BD{r})<>$AU{r})","CHECK SHARED_FACTOR_CHANGED_RECALCULATE_IN_CAD",
    "$BB{r}<>$AY{r}","CHECK SHARED_EVIDENCE_MODIFIED",
    "$BC{r}>0","CHECK SHARED_OWNER_NOT_PASS")+",\"CHECK FORMULA_ERROR\")";
   string fBQ="IFERROR("+Chain("\"\"",
    "{D}$B$4=\"\"","MISSING ASSEMBLY_CONFIRMATION","{D}$B$5=\"\"","MISSING CONDUIT_MODE","$F{r}=\"\"","MISSING PLAN_LENGTH",
    "{D}$I{q}=\"\"","MISSING FLOOR","$G{r}=\"\"","MISSING CEILING_HEIGHT","$E{r}=\"\"","MISSING ROUTING_MODE",
    "$AO{r}=\"MISSING\"","MISSING START_HEIGHT","$AP{r}=\"MISSING\"","MISSING END_HEIGHT","$K{r}=\"\"","MISSING TAIL_COUNT",
    "{D}$N{q}=\"\"","MISSING TAIL_LENGTH","$BD{r}=\"\"","MISSING FACTOR","{D}$J{q}=\"\"","MISSING AREA","{D}$K{q}=\"\"","MISSING CIRCUIT",
    "$N{r}=\"\"","MISSING ASSEMBLY","$T{r}=\"\"","MISSING CONDUIT","NOT(ISNUMBER($M{r}))","MISSING ROUTE_INCOMPLETE",
    "NOT(AND(ISNUMBER($W{r}),ISNUMBER($X{r})))","MISSING CONDUIT_INCOMPLETE",
    "NOT(AND(ISNUMBER($BE{r}),ISNUMBER($BH{r}),ISNUMBER($BI{r})))","MISSING AFTER_FACTOR_INCOMPLETE")+",\"MISSING FORMULA_ERROR\")";
   var devIndex=new Dictionary<string,int>(O);for(int k=0;k<dl.Count;k++)devIndex[dl[k].id]=k+1;
   for(int i=0;i<rows.Count;i++){var w=rows[i];var d=w.input;var x=w.result;int r=First+i,q=y.rs+i;var sp=spans[i];
    int e1=sp.count>0?y.es+sp.first:0,e2=sp.count>0?y.es+sp.first+sp.count-1:0;
    Func<string,string> T=f=>Tk(f,y,r,q,e1,e2);
    bool part=w.horizontalShares.Any(a=>a.role==RoomCompletion037.RoleOwner||a.role==RoomCompletion037.RoleDeducted)||
     w.verticalShares.Any(a=>a.role==RoomCompletion037.RoleOwner||a.role==RoomCompletion037.RoleDeducted);
    bool vs=w.verticalShares.Any(a=>a.role==RoomCompletion037.RoleDeducted&&a.side=="START"),ve=w.verticalShares.Any(a=>a.role==RoomCompletion037.RoleDeducted&&a.side=="END");
    Val(s,r,1,i+1,SCode);
    string desc=(!string.IsNullOrEmpty(w.structuredKey)?"Lộ "+w.structuredKey:"TAG "+(w.channelLabel??"—"))+" | "+w.startLabel+" → "+w.endLabel;
    Txt(s,r,2,desc,SText);
    Txt(s,r,3,w.startLabel,SText);Txt(s,r,4,w.endLabel,SText);
    Fx(s,r,C("E"),T(fE),SCode,null,d.routingMode);Fx(s,r,C("F"),T(fF),SNum,x.horizontalM,null);Fx(s,r,C("G"),T(fG),SNum,x.ceilingHeightM,null);
    foreach(var side in new[]{"H","I"}){string key=side=="H"?"AK":"AL",idx=side=="H"?"AM":"AN",st=side=="H"?"AO":"AP";
     string m="INDEX({D}$E${ds}:$E${de},$"+idx+"{r})",h="INDEX({D}$F${ds}:$F${de},$"+idx+"{r})",o="INDEX({D}$G${ds}:$G${de},$"+idx+"{r})";
     string dm="MATCH($"+key+"{r},{D}$A${ds}:$A${de},0)";
     string fIdx="IFERROR(IF(EXACT(INDEX({D}$A${ds}:$A${de},"+dm+"),$"+key+"{r}),"+dm+",\"\"),\"\")";
     string fH="IFERROR(IF(EXACT("+m+",\"FIXED\"),IF(AND(ISNUMBER("+h+"),"+o+"=\"\"),"+h+",\"\"),IF(EXACT("+m+",\"CEILING_OFFSET\"),IF(AND(ISNUMBER("+o+"),"+h+"=\"\",ISNUMBER($G{r})),$G{r}-"+o+",\"\"),\"\")),\"\")";
     string fixedPart="IF("+o+"<>\"\",\"CHECK\",IF("+h+"=\"\",\"MISSING\",IF(NOT(ISNUMBER("+h+")),\"CHECK\",IF("+h+"<0,\"CHECK\",IF(AND(ISNUMBER($G{r}),"+h+">$G{r}),\"CHECK\",\"OK\")))))";
     string offPart="IF("+h+"<>\"\",\"CHECK\",IF("+o+"=\"\",\"MISSING\",IF(NOT(ISNUMBER("+o+")),\"CHECK\",IF("+o+"<0,\"CHECK\",IF(NOT(ISNUMBER($G{r})),\"MISSING\",IF("+o+">$G{r},\"CHECK\",\"OK\"))))))";
     string fSt="IFERROR(IF(NOT(ISNUMBER($"+idx+"{r})),\"CHECK\",IF("+m+"=\"\",\"MISSING\",IF(EXACT("+m+",\"FIXED\"),"+fixedPart+",IF(EXACT("+m+",\"CEILING_OFFSET\"),"+offPart+",\"CHECK\")))),\"CHECK\")";
     string devId=side=="H"?w.startId:w.endId;
     Fx(s,r,C(side),T(fH),SNum,side=="H"?x.startHeightM:x.endHeightM,null);
     Txt(s,r,C(key),devId,SHText);Fx(s,r,C(idx),T(fIdx),SHInt,devIndex[devId],null);Fx(s,r,C(st),T(fSt),SHText,null,"OK");}
    Fx(s,r,C("J"),T(fJ),SNum,x.verticalM,null);Fx(s,r,C("K"),T(fK),SInt,d.tailCount,null);Fx(s,r,C("L"),T(fL),SNum,x.tailsM,null);
    Fx(s,r,C("M"),T(fM),SNum,x.routeM,null);Fx(s,r,C("N"),T(fN),SCode,null,d.assemblyCode);
    for(int k=0;k<5;k++){string cl=Col(C("O")+k);
     Fx(s,r,C("O")+k,T("IF($AE{r}<>\"PASS\",\"\",IF(EXACT($N{r},"+cl+"$11),$M{r},0))"),SNum,d.assemblyCode==SrcWire[k]?x.routeM.Value:0.0,null);}
    Fx(s,r,C("T"),T(fT),SCode,null,d.conduitCode);
    if(sp.count>0)Fx(s,r,C("U"),T("SUMIF({D}$D${e1}:$D${e2},\"HSEG\",{D}$G${e1}:$G${e2})"),SNum,w.sharedHorizontalM,null);else Val(s,r,C("U"),0.0,SNum);
    Fx(s,r,C("V"),T(fV),SNum,w.sharedVerticalM,null);
    Fx(s,r,C("W"),T(fW),SNum,x.horizontalConduitM,null);Fx(s,r,C("X"),T(fX),SNum,x.verticalConduitM,null);
    for(int k=0;k<3;k++){string ch=Col(C("Y")+k),ce=Col(C("AB")+k);bool hit=d.conduitCode==SrcPipe[k];
     Fx(s,r,C("Y")+k,T("IF($AE{r}<>\"PASS\",\"\",IF(EXACT($T{r},"+ch+"$11),$X{r}+IF(EXACT($E{r},\"WALL\"),$W{r},0),0))"),SNum,hit?x.conduitHiddenM.Value:0.0,null);
     Fx(s,r,C("AB")+k,T("IF($AE{r}<>\"PASS\",\"\",IF(AND(EXACT($T{r},"+ce+"$11),NOT(EXACT($E{r},\"WALL\"))),$W{r},0))"),SNum,hit?x.conduitExposedM.Value:0.0,null);}
    Fx(s,r,C("AE"),T(fAE),SStatus,null,"PASS");
    string note="Nguồn "+w.sourceKind+" "+w.sourceId+"; đoạn CAD "+string.Join(", ",w.segments.Select(g=>g.id+"="+Bdgkl034.N(g.length)))+" "+w.units+
     "; TAG \""+(w.channelLabel??"")+"\" = nhãn kênh điều khiển cục bộ trong phòng (không suy luận lộ/tủ)"+(string.IsNullOrEmpty(w.structuredKey)?"":"; lộ cấu trúc "+w.structuredKey);
    Txt(s,r,C("AF"),note,SText);
    Txt(s,r,C("AG"),w.startType,SCode);Txt(s,r,C("AH"),w.endType,SCode);
    s.Height(r,Math.Max(Math.Max(TextHeight(desc,34),TextHeight(note,48)),Math.Max(TextHeight(w.startLabel,24),TextHeight(w.endLabel,24))));
    Txt(s,r,C("AI"),w.rowKey,SHText);Val(s,r,C("AJ"),q,SHInt);
    Val(s,r,C("AQ"),x.horizontalM,SHNum);Txt(s,r,C("AR"),d.routingMode,SHText);Txt(s,r,C("AS"),d.conduitCode,SHText);Txt(s,r,C("AT"),d.floorId,SHText);Val(s,r,C("AU"),x.factor,SHNum);
    Val(s,r,C("AV"),part?1:0,SHInt);Val(s,r,C("AW"),vs?1:0,SHInt);Val(s,r,C("AX"),ve?1:0,SHInt);Val(s,r,C("AY"),sp.count,SHInt);
    if(sp.count>0){Val(s,r,C("AZ"),e1,SHInt);Val(s,r,C("BA"),e2,SHInt);
     Fx(s,r,C("BB"),T("SUMPRODUCT(--EXACT({D}$A${e1}:$A${e2},$AI{r}),--ISNUMBER({D}$G${e1}:$G${e2}))"),SHInt,sp.count,null);
     Fx(s,r,C("BC"),T("COUNTIF({D}$H${e1}:$H${e2},\"<>PASS\")"),SHInt,0,null);}
    else{Blank(s,r,C("AZ"),SHInt);Blank(s,r,C("BA"),SHInt);Val(s,r,C("BB"),0,SHInt);Val(s,r,C("BC"),0,SHInt);}
    Fx(s,r,C("BD"),T(fBD),SHNum,x.factor,null);Fx(s,r,C("BE"),T(fBE),SHNum,x.routeAfterFactorM,null);
    Fx(s,r,C("BF"),T(fBF),SHNum,x.conduitHiddenM,null);Fx(s,r,C("BG"),T(fBG),SHNum,x.conduitExposedM,null);
    Fx(s,r,C("BH"),T(fBH),SHNum,x.conduitHiddenAfterFactorM,null);Fx(s,r,C("BI"),T(fBI),SHNum,x.conduitExposedAfterFactorM,null);
    string[] src={"M","BE","BF","BG","BH","BI"};double?[] cv={x.routeM,x.routeAfterFactorM,x.conduitHiddenM,x.conduitExposedM,x.conduitHiddenAfterFactorM,x.conduitExposedAfterFactorM};
    for(int k=0;k<6;k++)Fx(s,r,C("BJ")+k,T("IF($AE{r}=\"PASS\",$"+src[k]+"{r},0)"),SHNum,cv[k],null);
    Fx(s,r,C("BP"),T(fBP),SHText,null,"");Fx(s,r,C("BQ"),T(fBQ),SHText,null,"");}
   var widths=new List<double>{6,34,24,24,11};for(int c=C("F");c<=C("M");c++)widths.Add(10);widths.Add(12);for(int k=0;k<5;k++)widths.Add(11);widths.Add(8);
   for(int k=0;k<4;k++)widths.Add(10);for(int k=0;k<6;k++)widths.Add(9);widths.AddRange(new double[]{14,48,11,11,30});
   for(int c=C("AJ");c<=C("BO");c++)widths.Add(11);widths.Add(40);widths.Add(36);
   string tail="<autoFilter ref=\"A11:BQ"+I(y.L)+"\"/>"+s.MergeXml()+
    StatusRule("AE"+I(First)+":AE"+I(y.L),1)+Page(PaperA3,true,false);
   return SheetXml(s,true,Cols(widths,C("AI")),tail,false);}
  static string StatusRule(string sqref,int priority){
   return "<conditionalFormatting sqref=\""+sqref+"\"><cfRule type=\"cellIs\" dxfId=\"0\" priority=\""+I(priority)+"\" operator=\"equal\"><formula>&quot;CHECK&quot;</formula></cfRule>"+
    "<cfRule type=\"cellIs\" dxfId=\"1\" priority=\""+I(priority+1)+"\" operator=\"equal\"><formula>&quot;MISSING DATA&quot;</formula></cfRule></conditionalFormatting>";}
  static string BlankRule(string first,string sqref,int priority){
   return "<conditionalFormatting sqref=\""+sqref+"\"><cfRule type=\"containsBlanks\" dxfId=\"1\" priority=\""+I(priority)+"\"><formula>LEN(TRIM("+first+"))=0</formula></cfRule></conditionalFormatting>";}
  static string ListRule(string sqref,string items){
   return "<dataValidation type=\"list\" errorStyle=\"warning\" allowBlank=\"1\" showErrorMessage=\"1\" sqref=\""+sqref+"\"><formula1>&quot;"+items+"&quot;</formula1></dataValidation>";}
  // fitWidth: shrink to one page wide (used only for the narrow THKL); otherwise 100 % scale, pages across, repeated titles.
  static string Page(int paper,bool landscape,bool fitWidth){return "<pageMargins left=\"0.4\" right=\"0.4\" top=\"0.5\" bottom=\"0.5\" header=\"0.3\" footer=\"0.3\"/><pageSetup paperSize=\""+I(paper)+
   "\" orientation=\""+(landscape?"landscape":"portrait")+"\" pageOrder=\"overThenDown\""+(fitWidth?" fitToWidth=\"1\" fitToHeight=\"0\"":" scale=\"100\"")+"/>";}
  // hiddenFrom: first 1-based column hidden from normal viewing (0 = none); hidden columns keep their formulas (unhide to inspect).
  static string Cols(IList<double> w){return Cols(w,0);}
  static string Cols(IList<double> w,int hiddenFrom){var b=new StringBuilder("<cols>");
   for(int i=0;i<w.Count;i++)b.Append("<col min=\""+I(i+1)+"\" max=\""+I(i+1)+"\" width=\""+Num(w[i])+"\" customWidth=\"1\""+(hiddenFrom>0&&i+1>=hiddenFrom?" hidden=\"1\"":"")+"/>");return b.Append("</cols>").ToString();}
  static string SheetXml(Sheet s,bool main,string cols,string tail,bool fit){var b=new StringBuilder();
   b.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><worksheet xmlns=\""+Ns+"\" xmlns:r=\""+NsR+"\">");
   b.Append((fit?"<sheetPr><pageSetUpPr fitToPage=\"1\"/></sheetPr>":"")+"<sheetViews><sheetView workbookViewId=\"0\""+(main?" tabSelected=\"1\"":"")+">");
   if(main)b.Append("<pane ySplit=\"11\" topLeftCell=\"A12\" activePane=\"bottomLeft\" state=\"frozen\"/><selection pane=\"bottomLeft\" activeCell=\"A12\" sqref=\"A12\"/>");
   b.Append("</sheetView></sheetViews><sheetFormatPr defaultRowHeight=\"15\"/>").Append(cols).Append("<sheetData>");
   foreach(var row in s.cells){double h;b.Append("<row r=\""+I(row.Key)+"\""+(s.heights.TryGetValue(row.Key,out h)?" ht=\""+Num(h)+"\" customHeight=\"1\"":"")+">");
    foreach(var c in row.Value)b.Append(c.Value);b.Append("</row>");}
   b.Append("</sheetData>").Append(tail).Append("</worksheet>");return b.ToString();}

  // ---- DM.DIEM ----
  static string Input(RoomCompletionBatch037 b,Lay y,List<string> floors,Dictionary<string,double> ceil,List<Dev> dl,List<Ev> ev,RoomHeightSettings037 hs){var s=new Sheet();var rows=Active(b);
   const int Wide=19;   // A:S = widest table of this sheet
   Txt(s,1,1,"DM.DIEM – KHAI BÁO ĐẦU VÀO (chỉ sửa ô chữ xanh)",STitle);s.Merge(1,1,1,Wide,STitle);s.Height(1,30);
   Txt(s,2,1,"Số liệu CAD (mã đoạn, chiều dài, đơn vị, mã thiết bị, nhãn) chỉ để truy vết – không sửa. Giá trị mẫu của bảng BDGKL nguồn không phải mặc định dự án. Ô trống = thiếu dữ liệu, không bao giờ hiểu là 0.",SNote);
   s.Merge(2,1,2,Wide,SNote);s.Height(2,22);
   Txt(s,3,1,"Thông số chung",SHead);Txt(s,3,2,"Giá trị (sửa)",SHead);Txt(s,3,3,"Giá trị lúc tính CAD (không sửa)",SHead);
   string conf=b.independentAssembliesConfirmed?"YES":"";
   Txt(s,4,1,"Xác nhận mỗi dòng là MỘT bộ dây khai báo độc lập (YES); không suy luận dây N/PE dùng chung, không nhân số lõi",SText);Txt(s,4,2,conf,SInText);Txt(s,4,3,conf,SHText);
   Txt(s,5,1,"Chế độ ống: SEPARATE = mỗi dòng một ống riêng; SHARED = đoạn trùng vật lý và đoạn xuống thiết bị đi chung ống (người dùng khai báo)",SText);
   Txt(s,5,2,b.conduitMode,SInText);Txt(s,5,3,b.conduitMode,SHText);
   Txt(s,6,1,"Số vướng mắc CAD/native lúc xuất (phải = 0)",SText);Val(s,6,2,0,SHInt);
   s.Height(3,34);s.Height(4,TextHeight("Xác nhận mỗi dòng là MỘT bộ dây khai báo độc lập (YES); không suy luận dây N/PE dùng chung, không nhân số lõi",40));
   s.Height(5,TextHeight("Chế độ ống: SEPARATE = mỗi dòng một ống riêng; SHARED = đoạn trùng vật lý và đoạn xuống thiết bị đi chung ống (người dùng khai báo)",40));s.Height(8,22);
   Txt(s,8,1,"Tầng (floorId)",SHead);Txt(s,8,2,"H sàn→trần (m)",SHead);
   for(int k=0;k<floors.Count;k++){Txt(s,y.fs+k,1,floors[k],SHText);Val(s,y.fs+k,2,ceil[floors[k]],SInNum);}
   string[] dh={"Mã thiết bị (CAD)","Loại (CAD)","Nhãn (CAD)","Tầng lúc tính","Cách khai cao độ: FIXED | CEILING_OFFSET","Cao độ cố định (m)","Hạ dưới trần (m)"};
   for(int c=0;c<dh.Length;c++)Txt(s,y.dh,c+1,dh[c],SHead);s.heights[y.dh]=32;
   for(int k=0;k<dl.Count;k++){var v=dl[k];int r=y.ds+k;
    Txt(s,r,1,v.id,SHText);Txt(s,r,2,v.type,SHText);Txt(s,r,3,v.label,SHText);Txt(s,r,4,v.floor,SHText);
    Txt(s,r,5,v.mode,SInText);Val(s,r,6,v.fixedH,SInNum);Val(s,r,7,v.offset,SInNum);s.Height(r,Math.Max(TextHeight(v.label,24),TextHeight(v.id,40)));}
   string[] rh={"Dòng BDGKL","Khóa dòng (rowKey)","Nguồn (CAD)","Trạng thái nguồn / lý do","Đoạn CAD = chiều dài (đơn vị bản vẽ)","Chiều dài CAD","Đơn vị",
    "KC mặt bằng (m)","Tầng","Phòng/vùng","Lộ / nhãn TAG kênh (sửa)","Hướng đi: CEILING | FLOOR | WALL","Số đầu chờ","Dài 1 đầu chờ (m)","Hệ số uốn cong",
    "Loại dây (mã bộ dây)","Cỡ ống (mã ống)","TB đầu","TB cuối"};
   for(int c=0;c<rh.Length;c++)Txt(s,y.rh,c+1,rh[c],SHead);s.heights[y.rh]=32;
   for(int i=0;i<rows.Count;i++){var w=rows[i];var d=w.input;int q=y.rs+i;
    Val(s,q,1,First+i,SHInt);Txt(s,q,2,w.rowKey,SHText);Txt(s,q,3,w.sourceKind+" "+w.sourceId,SHText);
    Txt(s,q,4,w.sourceStatus+(w.sourceReasons.Count>0?" ("+string.Join("; ",w.sourceReasons)+")":""),SHText);
    Txt(s,q,5,string.Join("; ",w.segments.Select(g=>g.id+"="+Bdgkl034.N(g.length))),SHText);
    Val(s,q,6,w.drawingLength,SHNum);Txt(s,q,7,w.units,SHText);
    Fx(s,q,8,"IF(OR($F"+I(q)+"=\"\",$G"+I(q)+"=\"\"),\"\",IF(AND(ISNUMBER($F"+I(q)+"),EXACT($G"+I(q)+",\"MM\")),$F"+I(q)+"/1000,IF(AND(ISNUMBER($F"+I(q)+"),EXACT($G"+I(q)+",\"M\")),$F"+I(q)+",\"INVALID\")))",SHNum,w.result.horizontalM,null);
    Txt(s,q,9,d.floorId,SInText);Txt(s,q,10,d.areaId,SHText);Txt(s,q,11,d.circuitId,SInText);Txt(s,q,12,d.routingMode,SInText);
    Val(s,q,13,d.tailCount,SInInt);Val(s,q,14,d.tailEachLength,SInNum);Val(s,q,15,d.factor,SInNum);
    Txt(s,q,16,d.assemblyCode,SInText);Txt(s,q,17,d.conduitCode,SInText);Txt(s,q,18,w.startId,SHText);Txt(s,q,19,w.endId,SHText);
    s.Height(q,Math.Max(Math.Max(TextHeight(w.rowKey,30),TextHeight(w.sourceStatus+(w.sourceReasons.Count>0?" ("+string.Join("; ",w.sourceReasons)+")":""),34)),
     TextHeight(string.Join("; ",w.segments.Select(g=>g.id+"="+Bdgkl034.N(g.length))),34)));}
   string[] eh={"Dòng phụ thuộc (rowKey)","Dòng sở hữu (rowKey)","Vai trò: DEDUCT | OWNER","Loại: HSEG = đoạn ngang | VDROP = đoạn xuống TB","Mã đoạn / thiết bị (CAD)","Phía","m lúc tính CAD","Trạng thái dòng sở hữu"};
   for(int c=0;c<eh.Length;c++)Txt(s,y.eh,c+1,eh[c],SHead);s.heights[y.eh]=32;
   string om="MATCH($B{e},BDGKL!$AI$12:$AI${L},0)".Replace("{L}",I(y.L));
   if(ev.Count==0)Txt(s,y.es,1,"Không có phân bổ dùng chung (SEPARATE hoặc không có đoạn/đầu trùng).",SNote);
   for(int k=0;k<ev.Count;k++){var e=ev[k];int r=y.es+k;string m=om.Replace("{e}",I(r));
    Txt(s,r,1,e.dep,SHText);Txt(s,r,2,e.owner,SHText);Txt(s,r,3,e.role,SHText);Txt(s,r,4,e.kind,SHText);Txt(s,r,5,e.item,SHText);Txt(s,r,6,e.side,SHText);
    Val(s,r,7,e.metres,SHNum);
    if(e.role=="DEDUCT")Fx(s,r,8,"IFERROR(IF(EXACT(INDEX(BDGKL!$AI$12:$AI$"+I(y.L)+","+m+"),$B"+I(r)+"),INDEX(BDGKL!$AE$12:$AE$"+I(y.L)+","+m+"),\"MISSING\"),\"MISSING\")",SHText,null,"PASS");
    else Txt(s,r,8,"SELF",SHText);}
   for(int k=0;k<ev.Count;k++)s.Height(y.es+k,Math.Max(TextHeight(ev[k].dep,40),TextHeight(ev[k].owner,30)));
   Txt(s,y.nh,1,"Ghi chú nguồn (không chặn; truy vết chọn ứng viên)",SHead);s.Merge(y.nh,1,y.nh,8,SHead);s.Height(y.nh,22);
   double noteWidth=40+30+24+34+34+14+10+12;   // A:H
   for(int k=0;k<b.notes.Count;k++){int r=y.nh+1+k;Txt(s,r,1,b.notes[k],SNote);s.Merge(r,1,r,8,SNote);s.Height(r,TextHeight(b.notes[k],noteWidth));}
   HeightSection(s,y.nh+1+b.notes.Count+1,hs,dl);
   string tail=s.MergeXml();int pr=1;
   tail+=BlankRule("B4","B4:B5",pr++);
   tail+=BlankRule("B"+I(y.fs),"B"+I(y.fs)+":B"+I(y.fe),pr++);
   tail+=BlankRule("E"+I(y.ds),"E"+I(y.ds)+":E"+I(y.de),pr++);
   tail+=BlankRule("I"+I(y.rs),"I"+I(y.rs)+":I"+I(y.re),pr++);
   tail+=BlankRule("K"+I(y.rs),"K"+I(y.rs)+":Q"+I(y.re),pr++);
   if(ev.Count>0)tail+="<conditionalFormatting sqref=\"H"+I(y.es)+":H"+I(y.ee)+"\"><cfRule type=\"expression\" dxfId=\"0\" priority=\""+I(pr++)+"\"><formula>AND($C"+I(y.es)+"=&quot;DEDUCT&quot;,$H"+I(y.es)+"&lt;&gt;&quot;PASS&quot;)</formula></cfRule></conditionalFormatting>";
   tail+="<dataValidations count=\"4\">"+ListRule("B4","YES")+ListRule("B5","SEPARATE,SHARED")+ListRule("E"+I(y.ds)+":E"+I(y.de),"FIXED,CEILING_OFFSET")+
    ListRule("L"+I(y.rs)+":L"+I(y.re),"CEILING,FLOOR,WALL")+"</dataValidations>"+Page(PaperA3,true,false);
   return SheetXml(s,false,Cols(new double[]{40,30,24,34,34,14,10,12,12,14,18,16,10,12,10,16,12,16,16}),tail,false);}
  // Appended AFTER every existing DM.DIEM table (existing addresses unchanged). Plain grey traceability values, not inputs and not referenced by any formula:
  // the profile the user applied versus the ACTUAL per-device declarations (device table E:G) that the formulas use.
  static void HeightSection(Sheet s,int r0,RoomHeightSettings037 hs,List<Dev> dl){int r=r0;
   Txt(s,r,1,HeightSectionTitle,SHead);
   s.Merge(r,1,r,8,SHead);s.Height(r,34);r++;
   if(hs==null){Txt(s,r,1,LegacyHeightNote,SNote);s.Merge(r,1,r,8,SNote);s.Height(r,22);return;}
   var meta=new[]{
    Tuple.Create("Tầng (nhập trong hộp cao độ)",hs.floorId,(double?)null,false),
    Tuple.Create("Chiều cao tầng (m) – từ sàn tầng này đến sàn tầng trên. CHỈ LÀ THÔNG TIN cho tuyến liên tầng sau này; bảng cùng tầng KHÔNG dùng để tính.",(string)null,hs.storeyHeight,true),
    Tuple.Create("Cao độ trần đi dây (m) – tính từ sàn hoàn thiện; khi áp dụng = H sàn→trần của các dòng phòng này",(string)null,hs.wireCeilingHeight,true),
    Tuple.Create("Chỉ điền thiết bị chưa có cao độ (khi bấm Áp dụng)",hs.onlyFillMissing?"Có":"Không",(double?)null,false)};
   foreach(var m in meta){Txt(s,r,1,m.Item1,SText);s.Merge(r,1,r,3,SText);if(m.Item4)Val(s,r,4,m.Item3,SHNum);else Txt(s,r,4,m.Item2,SHText);
    s.Height(r,TextHeight(m.Item1,40+30+24));r++;}
   r++;
   string[] gh={"Nhóm thiết bị","Loại (CAD)","Chọn ở lần Áp dụng gần nhất","Cách khai: FIXED | CEILING_OFFSET","Giá trị nhóm (m)","Số thiết bị trong phòng","Khớp giá trị nhóm","Khai riêng / khác nhóm / chưa khai"};
   for(int c=0;c<gh.Length;c++)Txt(s,r,c+1,gh[c],SHead);s.Height(r,34);r++;
   foreach(var t in RoomHeight037.Types){var g=hs.groups==null?null:hs.groups.FirstOrDefault(a=>a!=null&&a.deviceType==t);
    var of=dl.Where(d=>d.type==t).ToList();
    int same=g==null||g.heightMode==null||!g.value.HasValue?0:of.Count(d=>d.mode==g.heightMode&&(g.heightMode==Bdgkl034.CeilingOffset?d.offset==g.value&&!d.fixedH.HasValue:d.fixedH==g.value&&!d.offset.HasValue));
    Txt(s,r,1,RoomHeight037.TypeName(t),SText);Txt(s,r,2,t,SHText);Txt(s,r,3,g!=null&&g.apply?"Có":"Không",SHText);Txt(s,r,4,g==null?null:g.heightMode,SHText);
    Val(s,r,5,g==null?null:g.value,SHNum);Val(s,r,6,of.Count,SHInt);Val(s,r,7,same,SHInt);Val(s,r,8,of.Count-same,SHInt);s.Height(r,22);r++;}
   int other=dl.Count(d=>Array.IndexOf(RoomHeight037.Types,d.type)<0);
   Txt(s,r,1,RoomHeight037.TypeName(RoomQty037.Other),SText);Txt(s,r,2,"OTHER / khác",SHText);Txt(s,r,3,"Không thuộc nhóm",SHText);Blank(s,r,4,SHText);Blank(s,r,5,SHNum);
   Val(s,r,6,other,SHInt);Blank(s,r,7,SHInt);Val(s,r,8,other,SHInt);s.Height(r,22);r++;
   Txt(s,r,1,"Thiết bị chưa phân loại không bao giờ được gán vào nhóm; khai cao độ riêng từng thiết bị. Giá trị nhóm chỉ là hồ sơ đã áp dụng; sửa cao độ trong bảng thiết bị (cột E:G) mới thay đổi kết quả.",SNote);
   s.Merge(r,1,r,8,SNote);s.Height(r,TextHeight("Thiết bị chưa phân loại không bao giờ được gán vào nhóm; khai cao độ riêng từng thiết bị. Giá trị nhóm chỉ là hồ sơ đã áp dụng; sửa cao độ trong bảng thiết bị (cột E:G) mới thay đổi kết quả.",216));}

  // ---- THKL ----
  // THKL code lists: source codes first, then every other declared code in ordinal order (shared by Totals and BuildPackage).
  static void TotalCodes(RoomCompletionBatch037 b,out List<string> wire,out List<string> pipe){var rows=Active(b);
   wire=SrcWire.ToList();foreach(var code in rows.Select(a=>a.input.assemblyCode).Distinct(O).OrderBy(a=>a,O))if(!wire.Contains(code))wire.Add(code);
   pipe=SrcPipe.ToList();foreach(var code in rows.Select(a=>a.input.conduitCode).Distinct(O).OrderBy(a=>a,O))if(!pipe.Contains(code))pipe.Add(code);}
  static string Totals(RoomCompletionBatch037 b,Lay y){var s=new Sheet();var rows=Active(b);string L=I(y.L);
   List<string> wire,pipe;TotalCodes(b,out wire,out pipe);
   int w1=9,w2=w1+wire.Count+3-1,wr=w2+1,ch=wr+2,c1=ch+1,c2=c1+pipe.Count+3-1,cr=c2+1;
   Txt(s,1,1,"THKL – TỔNG HỢP KHỐI LƯỢNG (chỉ các dòng PASS)",STitle);
   Txt(s,2,1,"Dây: m tuyến của bộ dây đã khai báo (không phải m lõi). Ống: chìm/nổi theo cách phân loại của mẫu BDGKL. Cột CHÍNH THỨC chỉ hiện khi trạng thái tổng = PASS. Mã mới: nhập vào ô xanh trống.",SNote);
   s.Merge(1,1,1,7,STitle);s.Height(1,30);s.Merge(2,1,2,7,SNote);s.Height(2,34);
   Txt(s,4,1,"Trạng thái tổng",SHead);
   string fs="IFERROR(IF(OR($B$5=0,$C$5<>$B$5,"+D+"$B$6<>0,COUNTIF(BDGKL!$AE$12:$AE$"+L+",\"CHECK\")>0,ABS($B$"+I(wr)+")>0.000000001,ABS($C$"+I(wr)+")>0.000000001,"+
    "ABS($B$"+I(cr)+")>0.000000001,ABS($C$"+I(cr)+")>0.000000001,ABS($D$"+I(cr)+")>0.000000001,ABS($E$"+I(cr)+")>0.000000001),\"CHECK\","+
    "IF(COUNTIF(BDGKL!$AE$12:$AE$"+L+",\"PASS\")<>$B$5,\"MISSING DATA\",\"PASS\")),\"CHECK\")";
   Fx(s,4,2,fs,SStatus,null,"PASS");
   Txt(s,4,3,"PASS chỉ khi mọi dòng PASS, đủ số dòng, không vướng mắc CAD và không còn khối lượng ở mã chưa liệt kê. PASS = đủ dữ liệu, không phải phê duyệt kỹ thuật.",SNote);
   s.Merge(4,3,4,7,SNote);s.Height(4,34);s.Height(5,20);s.Height(6,20);
   Txt(s,5,1,"Số dòng BDGKL: lúc xuất / hiện có",SText);Val(s,5,2,rows.Count,SInt);Fx(s,5,3,"COUNTA(BDGKL!$AI$12:$AI$"+L+")",SInt,rows.Count,null);
   Txt(s,6,1,"Số dòng PASS / MISSING DATA / CHECK",SText);
   Fx(s,6,2,"COUNTIF(BDGKL!$AE$12:$AE$"+L+",\"PASS\")",SInt,rows.Count,null);Fx(s,6,3,"COUNTIF(BDGKL!$AE$12:$AE$"+L+",\"MISSING DATA\")",SInt,0,null);
   Fx(s,6,4,"COUNTIF(BDGKL!$AE$12:$AE$"+L+",\"CHECK\")",SInt,0,null);
   string[] wh={"Loại dây (mã bộ dây, đúng chính xác)","m tuyến (PASS, chưa hệ số)","m tuyến sau hệ số (PASS)","CHÍNH THỨC sau hệ số (m)"};
   for(int c=0;c<wh.Length;c++)Txt(s,8,c+1,wh[c],SHead);s.heights[8]=32;
   for(int k=0;k<wire.Count+3;k++){int t=w1+k;string code=k<wire.Count?wire[k]:null;string T=I(t);
    double raw=0,aft=0;if(code!=null)foreach(var r in rows)if(r.input.assemblyCode==code){raw+=r.result.routeM.Value;aft+=r.result.routeAfterFactorM.Value;}
    if(code!=null)Txt(s,t,1,code,SHText);else Blank(s,t,1,SInText);
    Fx(s,t,2,"IF($A"+T+"=\"\",\"\",SUMPRODUCT(--EXACT(BDGKL!$N$12:$N$"+L+",$A"+T+"),BDGKL!$BJ$12:$BJ$"+L+"))",SNum,code!=null?(double?)raw:null,"");
    Fx(s,t,3,"IF($A"+T+"=\"\",\"\",SUMPRODUCT(--EXACT(BDGKL!$N$12:$N$"+L+",$A"+T+"),BDGKL!$BK$12:$BK$"+L+"))",SNum,code!=null?(double?)aft:null,"");
    Fx(s,t,4,"IF(OR($B$4<>\"PASS\",$A"+T+"=\"\"),\"\",C"+T+")",SNum,code!=null?(double?)aft:null,"");}
   Txt(s,wr,1,"Chưa liệt kê (phải = 0; nếu khác 0 hãy nhập mã vào ô xanh trống)",SText);s.Height(wr,34);
   Fx(s,wr,2,"SUM(BDGKL!$BJ$12:$BJ$"+L+")-SUM(B"+I(w1)+":B"+I(w2)+")",SNum,0,null);Fx(s,wr,3,"SUM(BDGKL!$BK$12:$BK$"+L+")-SUM(C"+I(w1)+":C"+I(w2)+")",SNum,0,null);
   string[] chd={"Cỡ ống (mã ống, đúng chính xác)","Ống chìm (m)","Ống nổi (m)","Ống chìm sau hệ số (m)","Ống nổi sau hệ số (m)","CHÍNH THỨC chìm sau HS (m)","CHÍNH THỨC nổi sau HS (m)"};
   for(int c=0;c<chd.Length;c++)Txt(s,ch,c+1,chd[c],SHead);s.heights[ch]=32;
   string[] hc={"BL","BM","BN","BO"};
   for(int k=0;k<pipe.Count+3;k++){int t=c1+k;string code=k<pipe.Count?pipe[k]:null;string T=I(t);var v=new double[4];
    if(code!=null)foreach(var r in rows)if(r.input.conduitCode==code){var x=r.result;v[0]+=x.conduitHiddenM.Value;v[1]+=x.conduitExposedM.Value;v[2]+=x.conduitHiddenAfterFactorM.Value;v[3]+=x.conduitExposedAfterFactorM.Value;}
    if(code!=null)Txt(s,t,1,code,SHText);else Blank(s,t,1,SInText);
    for(int j=0;j<4;j++)Fx(s,t,2+j,"IF($A"+T+"=\"\",\"\",SUMPRODUCT(--EXACT(BDGKL!$T$12:$T$"+L+",$A"+T+"),BDGKL!$"+hc[j]+"$12:$"+hc[j]+"$"+L+"))",SNum,code!=null?(double?)v[j]:null,"");
    Fx(s,t,6,"IF(OR($B$4<>\"PASS\",$A"+T+"=\"\"),\"\",D"+T+")",SNum,code!=null?(double?)v[2]:null,"");
    Fx(s,t,7,"IF(OR($B$4<>\"PASS\",$A"+T+"=\"\"),\"\",E"+T+")",SNum,code!=null?(double?)v[3]:null,"");}
   Txt(s,cr,1,"Chưa liệt kê (phải = 0; nếu khác 0 hãy nhập mã vào ô xanh trống)",SText);s.Height(cr,34);
   for(int t=w1;t<=w2;t++)s.Height(t,20);for(int t=c1;t<=c2;t++)s.Height(t,20);s.Height(ch,48);s.Height(8,48);
   for(int j=0;j<4;j++){string cl=Col(2+j);Fx(s,cr,2+j,"SUM(BDGKL!$"+hc[j]+"$12:$"+hc[j]+"$"+L+")-SUM("+cl+I(c1)+":"+cl+I(c2)+")",SNum,0,null);}
   string tail=StatusRule("B4",1)+"<conditionalFormatting sqref=\"B"+I(wr)+":C"+I(wr)+" B"+I(cr)+":E"+I(cr)+"\"><cfRule type=\"expression\" dxfId=\"0\" priority=\"3\"><formula>ABS(B"+I(wr)+")&gt;0.000000001</formula></cfRule></conditionalFormatting>"+Page(PaperA4,true,true);
   return SheetXml(s,false,Cols(new double[]{46,22,22,22,22,22,22}),s.MergeXml()+tail,true);}

  // ---- package parts ----
  static string ContentTypes(){const string M="application/vnd.openxmlformats-officedocument.spreadsheetml.";
   var b=new StringBuilder("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">");
   b.Append("<Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/><Default Extension=\"xml\" ContentType=\"application/xml\"/>");
   b.Append("<Override PartName=\"/xl/workbook.xml\" ContentType=\""+M+"sheet.main+xml\"/>");
   for(int i=1;i<=3;i++)b.Append("<Override PartName=\"/xl/worksheets/sheet"+I(i)+".xml\" ContentType=\""+M+"worksheet+xml\"/>");
   b.Append("<Override PartName=\"/xl/styles.xml\" ContentType=\""+M+"styles+xml\"/></Types>");return b.ToString();}
  static string Workbook(Lay y){string L=I(y.L);
   return "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><workbook xmlns=\""+Ns+"\" xmlns:r=\""+NsR+"\"><bookViews><workbookView activeTab=\"0\"/></bookViews><sheets>"+
    "<sheet name=\""+SheetMain+"\" sheetId=\"1\" r:id=\"rId1\"/><sheet name=\""+SheetInput+"\" sheetId=\"2\" r:id=\"rId2\"/><sheet name=\""+SheetTotals+"\" sheetId=\"3\" r:id=\"rId3\"/></sheets>"+
    "<definedNames><definedName name=\"_xlnm._FilterDatabase\" localSheetId=\"0\" hidden=\"1\">BDGKL!$A$11:$BQ$"+L+"</definedName>"+
    "<definedName name=\"_xlnm.Print_Area\" localSheetId=\"0\">BDGKL!$A$1:$AH$"+L+"</definedName>"+
    "<definedName name=\"_xlnm.Print_Titles\" localSheetId=\"0\">BDGKL!$A:$D,BDGKL!$10:$11</definedName></definedNames>"+
    "<calcPr calcId=\"191029\" fullCalcOnLoad=\"1\"/></workbook>";}
  static string Styles(){const string F="<name val=\"Arial\"/><family val=\"2\"/></font>";const string B="borderId=\"1\" xfId=\"0\" applyBorder=\"1\"";
   // Alignment groups: text = left + middle + wrap; numbers = right + middle; codes/status = centre + middle; headers = centre + middle + wrap.
   const string AT=" applyAlignment=\"1\"><alignment horizontal=\"left\" vertical=\"center\" wrapText=\"1\"/>",AN=" applyAlignment=\"1\"><alignment horizontal=\"right\" vertical=\"center\"/>";
   const string AC=" applyAlignment=\"1\"><alignment horizontal=\"center\" vertical=\"center\" wrapText=\"1\"/>";
   const string Unlocked="<protection locked=\"0\"/></xf>";
   return "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><styleSheet xmlns=\""+Ns+"\">"+
    "<numFmts count=\"2\"><numFmt numFmtId=\"164\" formatCode=\"0.0\"/><numFmt numFmtId=\"165\" formatCode=\"0.0##\"/></numFmts>"+
    "<fonts count=\"5\"><font><sz val=\"11\"/>"+F+"<font><b/><sz val=\"11\"/>"+F+"<font><sz val=\"11\"/><color rgb=\"FF0000FF\"/>"+F+
    "<font><b/><sz val=\"14\"/>"+F+"<font><i/><sz val=\"11\"/><color rgb=\"FF595959\"/>"+F+"</fonts>"+
    "<fills count=\"4\"><fill><patternFill patternType=\"none\"/></fill><fill><patternFill patternType=\"gray125\"/></fill>"+
    "<fill><patternFill patternType=\"solid\"><fgColor rgb=\"FFD9E1F2\"/><bgColor indexed=\"64\"/></patternFill></fill>"+
    "<fill><patternFill patternType=\"solid\"><fgColor rgb=\"FFF2F2F2\"/><bgColor indexed=\"64\"/></patternFill></fill></fills>"+
    "<borders count=\"2\"><border><left/><right/><top/><bottom/><diagonal/></border><border><left style=\"thin\"><color auto=\"1\"/></left><right style=\"thin\"><color auto=\"1\"/></right>"+
    "<top style=\"thin\"><color auto=\"1\"/></top><bottom style=\"thin\"><color auto=\"1\"/></bottom><diagonal/></border></borders>"+
    "<cellStyleXfs count=\"1\"><xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" borderId=\"0\"/></cellStyleXfs><cellXfs count=\"15\">"+
    "<xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" borderId=\"0\" xfId=\"0\"/>"+                                                                     // 0 default
    "<xf numFmtId=\"0\" fontId=\"1\" fillId=\"2\" "+B+" applyFont=\"1\" applyFill=\"1\""+AC+"</xf>"+ // 1 header
    "<xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" "+B+AT+"</xf>"+                     // 2 text
    "<xf numFmtId=\"164\" fontId=\"0\" fillId=\"0\" "+B+" applyNumberFormat=\"1\""+AN+"</xf>"+                                                              // 3 metres 0.0
    "<xf numFmtId=\"49\" fontId=\"2\" fillId=\"0\" "+B+" applyNumberFormat=\"1\" applyFont=\"1\" applyProtection=\"1\""+AT+Unlocked+                                       // 4 input text
    "<xf numFmtId=\"165\" fontId=\"2\" fillId=\"0\" "+B+" applyNumberFormat=\"1\" applyFont=\"1\" applyProtection=\"1\""+AN+Unlocked+                                      // 5 input number
    "<xf numFmtId=\"1\" fontId=\"0\" fillId=\"0\" "+B+" applyNumberFormat=\"1\""+AN+"</xf>"+                                                                // 6 integer
    "<xf numFmtId=\"1\" fontId=\"2\" fillId=\"0\" "+B+" applyNumberFormat=\"1\" applyFont=\"1\" applyProtection=\"1\""+AN+Unlocked+                                        // 7 input integer
    "<xf numFmtId=\"0\" fontId=\"3\" fillId=\"0\" borderId=\"0\" xfId=\"0\" applyFont=\"1\""+AC+"</xf>"+                                                     // 8 title
    "<xf numFmtId=\"0\" fontId=\"0\" fillId=\"3\" "+B+" applyFill=\"1\""+AT+"</xf>"+    // 9 helper text
    "<xf numFmtId=\"165\" fontId=\"0\" fillId=\"3\" "+B+" applyNumberFormat=\"1\" applyFill=\"1\""+AN+"</xf>"+                                               // 10 helper number
    "<xf numFmtId=\"1\" fontId=\"0\" fillId=\"3\" "+B+" applyNumberFormat=\"1\" applyFill=\"1\""+AN+"</xf>"+                                                 // 11 helper integer
    "<xf numFmtId=\"0\" fontId=\"1\" fillId=\"0\" "+B+" applyFont=\"1\""+AC+"</xf>"+              // 12 status
    "<xf numFmtId=\"0\" fontId=\"4\" fillId=\"0\" borderId=\"0\" xfId=\"0\" applyFont=\"1\""+AT+"</xf>"+   // 13 note
    "<xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" "+B+AC+"</xf>"+   // 14 short code / STT (centre)
    "</cellXfs><cellStyles count=\"1\"><cellStyle name=\"Normal\" xfId=\"0\" builtinId=\"0\"/></cellStyles>"+
    "<dxfs count=\"2\"><dxf><font><color rgb=\"FF9C0006\"/></font><fill><patternFill><bgColor rgb=\"FFFFC7CE\"/></patternFill></fill></dxf>"+
    "<dxf><font><color rgb=\"FF9C5700\"/></font><fill><patternFill><bgColor rgb=\"FFFFEB9C\"/></patternFill></fill></dxf></dxfs></styleSheet>";}
 }
}
