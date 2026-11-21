using System;using System.Collections.Generic;using System.Globalization;using System.IO;using System.Linq;using System.Text;
namespace Tvxd.Meps {
 // 0.6.0 pure model (TVXD-MEPS-IMPLEMENT-008): symbol-ratio presets behind an explicit setup, the placement request queue, the placement lifecycle log,
 // namespaced text-style names used at insertion, and the legend preflight / source-remap diagnostics. No CAD types: the plugin passes Document and
 // UCS as opaque objects, so every rule here is testable offline (TESTS/Test008.cs).
 // Ratio setup: N0 (denominator the library symbols were drawn for) and the base factor are typed and confirmed by the user. Nothing is inferred from the
 // source INSUNITS (metadata "Inches", not confirmed as mm), DIMSCALE or CANNOSCALE, and N0 has no default.
 public sealed class RatioSetup {public double refDenominator,baseScale=1;public bool confirmed;public string provenance;}
 // One placement request from the palette. doc/ucs are the AutoCAD Document and Matrix3d captured at the click; units = INSUNITS at the click.
 public sealed class PlaceRequest {public long id;public object doc,ucs;public int units;public LibPlacement placement;public string entryName;public DateTime requested,sent;}
 // Model-space block references that look like library symbols but carry no MEPS marker (kind NAME_MATCH / MEPS_NAME_NO_MARKER), and xrefs (kind XREF).
 public sealed class ForeignRef {public string name,kind;public int count;}
 public sealed class PreflightItem {public string name,entryId,sourceSha256,sourceFile,status,reason,action;public int count;public bool blocking;}
 // Result of the legend check before export. map is the exact mapping the plan was made from (export uses the same instance).
 // policy: Legend.PolicyStrict (Preflight, historical) or Legend.PolicyBlank (PreflightForExport). blankText = names exported with empty/uncertain cells.
 public sealed class LegendPreflight {public List<PreflightItem> items=new List<PreflightItem>();public LegendPlan plan;public LegendMap map;public int used,placements;public string policy=Legend.PolicyStrict;public List<string> staleSources=new List<string>(),blankText=new List<string>();
  public int Blocking{get{return items.Count(i=>i.blocking);}}
  public bool CanExport{get{return plan!=null&&plan.CanExport&&Blocking==0;}}
  public string Summary(){return "Đã quét "+used+" loại ký hiệu MEPS ("+placements+" lần đặt). "+(CanExport?"Đủ điều kiện xuất bảng.":"CHƯA xuất được: "+Blocking+" mục chặn.")
   +(blankText.Count>0?" "+blankText.Count+" ký hiệu xuất với ô để trống (MEPS không tự viết mô tả): "+string.Join(", ",blankText.Take(10))+(blankText.Count>10?" …":"")+".":"")+(staleSources.Count>0?" Nguồn đọc bằng parser cũ: "+string.Join(", ",staleSources)+".":"");}
  public string Report(){var sb=new StringBuilder();sb.AppendLine("TVXD MEPS "+Meps06.Version+" — kiểm tra bảng ký hiệu trước khi xuất");if(policy==Legend.PolicyBlank)sb.AppendLine("Quy tắc xuất: ô thiếu/không chắc chắn để trống, bố cục 4 cột theo bảng nguồn; chỉ chặn khi thiếu bảng nguồn, khối không hợp lệ hoặc hướng đặt không tương thích.");sb.AppendLine(Summary());
   foreach(var i in items.OrderByDescending(x=>x.blocking).ThenBy(x=>x.name,StringComparer.OrdinalIgnoreCase))sb.AppendLine((i.blocking?"[CHẶN] ":i.status==Legend.Ok?"[OK] ":"[CHÚ Ý] ")+i.name+(i.count>0?" ×"+i.count:"")+" — "+i.status+" — "+i.reason+(string.IsNullOrEmpty(i.action)?"":" → "+i.action));
   return sb.ToString();}}
 // Request queue of the palette (one placement at a time). A click while the command string is queued but not started replaces the queued request (the
 // last click wins, no second command string unless the first one looks lost); a click while the cursor preview runs is refused (ESC first).
 public sealed class PlaceQueue {
  public const double ResendSeconds=4;
  PlaceRequest queued,running;long next;
  public PlaceRequest Queued{get{return queued;}}
  public PlaceRequest Running{get{return running;}}
  public bool Busy{get{return queued!=null||running!=null;}}
  public string State{get{return running!=null?"RUNNING #"+running.id+" "+running.entryName:queued!=null?"QUEUED #"+queued.id+" "+queued.entryName:"IDLE";}}
  // Returns true when the caller must (re)send the command string. canSend=false (CAD runs another command) refuses a request that would need a send,
  // so text is never typed into somebody else's prompt.
  public bool Request(PlaceRequest r,DateTime now,bool canSend,string busyCommand){if(r==null||r.doc==null)throw new Exception("Yêu cầu đặt không hợp lệ.");
   if(running!=null)throw new Exception("Đang chọn điểm cho "+running.entryName+" trên Model; ESC trong bản vẽ để huỷ trước khi chọn ký hiệu khác.");
   bool send=queued==null||!ReferenceEquals(queued.doc,r.doc)||(now-queued.sent).TotalSeconds>=ResendSeconds;
   if(send&&!canSend)throw new Exception("CAD đang chạy lệnh "+(busyCommand??"khác")+"; hoàn tất hoặc ESC lệnh đó rồi bấm lại ký hiệu.");
   r.id=++next;r.requested=now;r.sent=send?now:queued.sent;queued=r;return send;}
  // ESC in the palette: drops a request whose command has not started. Once the preview runs, ESC belongs to CAD.
  public bool Cancel(){if(queued==null)return false;queued=null;return true;}
  // Called by the command. Returns the request for this document (now running), or null with the reason (other document / nothing queued).
  public PlaceRequest Take(object doc,out string why){why=null;if(running!=null){why="Lệnh đặt "+running.entryName+" vẫn đang chạy.";return null;}if(queued==null)return null;
   var q=queued;queued=null;if(!ReferenceEquals(q.doc,doc)){why="Yêu cầu đặt "+q.entryName+" thuộc bản vẽ khác; đã huỷ, chưa thay đổi bản vẽ. Bấm lại ký hiệu.";return null;}running=q;return q;}
  public void Finish(PlaceRequest r){if(r!=null&&ReferenceEquals(running,r))running=null;}
  // Document switch: a queued request of another document is dropped (returned for the log); a running one belongs to its command.
  public PlaceRequest DocumentChanged(object doc){if(queued!=null&&!ReferenceEquals(queued.doc,doc)){var q=queued;queued=null;return q;}return null;}}
 // Placement lifecycle log in the owned output folder (OUTPUT/LOGS_0_6). One tab-separated line per event: UTC time, version, request id, event, library
 // symbol name, elapsed ms since the click, short detail. No drawing names, paths, coordinates or other drawing contents. Never throws into placement.
 public sealed class PlacementLog {
  public readonly string Dir;public string LastError;
  public PlacementLog(string dir){if(string.IsNullOrWhiteSpace(dir)||!Path.IsPathRooted(dir))throw new Exception("Thư mục log phải là đường dẫn tuyệt đối.");Dir=dir;}
  public string FilePath(DateTime utc){return Path.Combine(Dir,"placement_"+Meps06.Version.Replace('.','_')+"_"+utc.ToString("yyyyMMdd",CultureInfo.InvariantCulture)+".log");}
  public static string Clean(string s,int max){if(string.IsNullOrEmpty(s))return "-";var b=new StringBuilder(s.Length);foreach(char c in s)b.Append(char.IsControl(c)?' ':c);string t=b.ToString().Trim();if(t.Length==0)return "-";return t.Length>max?t.Substring(0,max)+"…":t;}
  public static string Line(DateTime utc,long id,string evt,string entry,double ms,string detail){var ci=CultureInfo.InvariantCulture;
   return string.Join("\t",new[]{utc.ToString("yyyy-MM-ddTHH:mm:ss.fffZ",ci),"v"+Meps06.Version,"req="+id.ToString(ci),Clean(evt,24),Clean(entry,200),"ms="+(Core.Finite(ms)&&ms>=0?ms.ToString("0.0",ci):"-"),Clean(detail,300)});}
  public void Write(long id,string evt,string entry,double ms,string detail){try{var now=DateTime.UtcNow;Directory.CreateDirectory(Dir);File.AppendAllText(FilePath(now),Line(now,id,evt,entry,ms,detail)+"\r\n",new UTF8Encoding(false));}catch(Exception ex){LastError=ex.Message;}}
  // Parses one line back (QA); null when it is not a log line of this format.
  public static string[] Parse(string line){if(line==null)return null;var p=line.Split('\t');return p.Length==7&&p[1].StartsWith("v")&&p[2].StartsWith("req=")&&p[5].StartsWith("ms=")?p:null;}}
 public static class Meps06 {
  public const string Version=Meps05.Version,LogDir="LOGS_0_6";
  // ---- Symbol ratio ----
  public static readonly double[] Presets={100,125,150,250};
  public const string RawItem="Gốc 1×",CustomItem="Tùy chỉnh…",CustomSuffix=" (tùy chỉnh)";
  public static string PresetText(double n){return "1:"+Library.F(n);}
  public static bool Number(string text,out double v){return double.TryParse(text,NumberStyles.Float,CultureInfo.CurrentCulture,out v)||double.TryParse(text,NumberStyles.Float,CultureInfo.InvariantCulture,out v);}
  public static void CheckSetup(RatioSetup s){if(s==null||!s.confirmed)throw new Exception("Chưa thiết lập tỷ lệ: bấm ⚙, nhập và xác nhận mẫu số gốc N0 cùng hệ số gốc (MEPS không tự đặt N0).");Library.CheckScale(s.baseScale);Meps05.CheckDenominator(s.refDenominator,"Mẫu số tỷ lệ gốc N0");}
  // Factor of one insertion: raw (no denominator) = 1, the source geometry unchanged; preset/custom 1:N = baseScale × N / N0 after the explicit setup.
  public static double Scale(RatioSetup s,double? denominator){if(!denominator.HasValue)return 1;CheckSetup(s);return Meps05.RatioScale(s.baseScale,s.refDenominator,Meps05.CheckDenominator(denominator.Value,"Mẫu số tỷ lệ bản vẽ N"));}
  // Builds a confirmed setup from the dialog texts. Empty N0, invalid numbers or a missing confirmation are refused with the reason; nothing is defaulted.
  public static RatioSetup Confirm(string n0Text,string baseText,bool confirmed,string provenance){double n0,b;
   if(!Number((n0Text??"").Trim(),out n0))throw new Exception("Nhập mẫu số gốc N0 (ký hiệu vẽ cho 1:N0); không có giá trị mặc định.");Meps05.CheckDenominator(n0,"Mẫu số tỷ lệ gốc N0");
   if(!Number((baseText??"").Trim(),out b))throw new Exception("Nhập hệ số gốc bằng số dương.");Library.CheckScale(b);
   if(!confirmed)throw new Exception("Đánh dấu ô xác nhận N0 và hệ số gốc trước khi dùng tỷ lệ.");
   return new RatioSetup{refDenominator=n0,baseScale=b,confirmed=true,provenance=provenance};}
  // ---- Text-style namespace used at insertion (LibraryCad06): MEPS06_<first 8 of source hash>_<source style name>[_k] ----
  public static string StyleName(string sourceSha256,string name,int k){if(sourceSha256==null||sourceSha256.Length!=64||!sourceSha256.All(Uri.IsHexDigit))throw new Exception("Hash nguồn không hợp lệ.");Library.ValidName(name);if(k<1||k>999)throw new Exception("Hậu tố kiểu chữ không hợp lệ.");
   string n="MEPS06_"+sourceSha256.Substring(0,8).ToUpperInvariant()+"_"+name+(k>1?"_"+k.ToString(CultureInfo.InvariantCulture):"");if(n.Length>255)throw new Exception("Tên kiểu chữ quá dài: "+name);return n;}
  public static bool SameStyle(LegendStyle a,LegendStyle b){if(a==null||b==null)return false;Func<string,string,bool> s=(x,y)=>string.Equals(x??"",y??"",StringComparison.OrdinalIgnoreCase);
   return s(a.fileName,b.fileName)&&s(a.bigFontFileName,b.bigFontFileName)&&s(a.typeface,b.typeface)&&a.bold==b.bold&&a.italic==b.italic&&a.charset==b.charset&&a.pitch==b.pitch&&a.flagBits==b.flagBits&&a.vertical==b.vertical
    &&Library.GeomEqual(a.xScale,b.xScale)&&Library.GeomEqual(a.oblique,b.oblique)&&Library.GeomEqual(a.textSize,b.textSize);}
  // ---- Legend preflight ----
  public const string NotInLibrary="NOT_IN_LIBRARY",Unrecognized="UNRECOGNIZED",XrefNotScanned="XREF_NOT_SCANNED",Empty="EMPTY",Internal="INTERNAL",KindName="NAME_MATCH",KindMepsName="MEPS_NAME_NO_MARKER",KindXref="XREF";
  static bool Same(string a,string b){return string.Equals(a,b,StringComparison.OrdinalIgnoreCase);}
  public static string ParserOf(LegendMap m,string sha){var s=m==null?null:m.sources.FirstOrDefault(x=>Same(x.sourceSha256,sha));return s==null?null:s.parser;}
  static string Action(string status,LibEntry e,LegendMap map){string parser=ParserOf(map,e.sourceSha256);string stale=parser!=null&&parser!=Legend.Parser?"Nguồn đang đọc bằng "+parser+": thử 'Đọc lại bảng nguồn' (parser "+Legend.Parser+") với đúng file "+e.sourceFile+". ":"";
   switch(status){
    case Legend.NotMapped:return "Bấm 'Đọc lại bảng nguồn' và chọn đúng file nguồn "+e.sourceFile+" (chỉ đọc, không sửa nguồn).";
    case Legend.Ambiguous:return "MEPS không đoán mô tả. "+stale+"Nếu vẫn mơ hồ: sửa bảng trong BẢN SAO của file nguồn (chữ/khối không vượt đường kẻ), thêm bản sao bằng '+', hoặc bỏ ký hiệu này khỏi Model trước khi xuất.";
    case Legend.MissingDescription:return "Ô DIỄN GIẢI của nguồn trống; MEPS không tự viết mô tả. Bổ sung trong BẢN SAO file nguồn rồi đọc lại, hoặc bỏ ký hiệu khỏi Model.";
    case Legend.NotInTable:return "Khối không có dòng trong bảng nguồn; MEPS không tự viết mô tả. "+stale+"Bổ sung dòng trong BẢN SAO file nguồn rồi đọc lại, hoặc bỏ ký hiệu khỏi Model.";
    case Legend.NoTable:return "File nguồn không có bảng ký hiệu; không có mô tả để dùng. Bỏ ký hiệu khỏi Model hoặc dùng nguồn có bảng.";
    case Legend.UnresolvedDynamic:return "Khối động trong bảng nguồn chưa xác định tên gốc. "+stale+"Nếu vẫn chưa rõ: chèn khối gốc vào bảng trong BẢN SAO file nguồn.";
    case Legend.UnsupportedTransform:return "Khối trong bảng nguồn bị lật/tỉ lệ không đều/nghiêng; sửa trong BẢN SAO file nguồn rồi đọc lại. MEPS không đoán hướng.";
    default:return stale+"Xem lý do; không có cách xử lý tự động.";}}
  // Same checks, in the same order, as Legend.Plan for one used symbol, as a structured item with a concrete action.
  public static PreflightItem Classify(UsedSymbol u,LegendMap map,Func<string,LibEntry> entry){var i=new PreflightItem{name=u.name??u.entryId,entryId=u.entryId,sourceSha256=u.sourceSha256,count=u.count,blocking=true};var e=entry(u.entryId);
   if(e==null){i.status=NotInLibrary;i.reason="không có trong thư viện "+Meps05.LibraryDir;i.action="Khối mang dấu MEPS nhưng mục không còn trong thư viện này: thêm lại nguồn bằng '+', hoặc thay bằng ký hiệu có trong bảng.";return i;}
   i.name=e.name;i.sourceSha256=e.sourceSha256;i.sourceFile=e.sourceFile;
   var why=Meps05.PlaceIssue(e);if(why!=null){i.status=Meps05.StyleCode(e);i.reason=why;i.action="Làm theo cách xử lý ghi trong lý do.";return i;}
   var r=Legend.Resolve(map,e.sourceSha256,e.name);if(r.status!=Legend.Ok){i.status=r.status;i.reason=r.reason;i.action=Action(r.status,e,map);return i;}
   var pose=Legend.Pose(map.rows.Where(x=>Same(x.sourceSha256,r.row.sourceSha256)&&x.row==r.row.row&&Same(x.name,e.name)).ToList());
   if(pose.issue!=null){i.status=Legend.UnsupportedTransform;i.reason=pose.issue+" (hàng nguồn "+(r.row.row+1)+")";i.action=Action(Legend.UnsupportedTransform,e,map);return i;}
   string d=r.row.description.text??"";int nl=d.IndexOf('\n');i.status=Legend.Ok;i.blocking=false;i.reason="hàng nguồn "+(r.row.row+1)+": "+(nl>=0?d.Substring(0,nl)+" …":d);i.action="";return i;}
  public static PreflightItem Foreign(ForeignRef f){var i=new PreflightItem{name=f.name,count=f.count};
   if(f.kind==KindXref){i.status=XrefNotScanned;i.blocking=false;i.reason="xref "+f.name+" ("+f.count+" lần) không được quét";i.action="Ký hiệu nằm trong xref không vào bảng; mở bản vẽ gốc của xref để tạo bảng riêng nếu cần.";return i;}
   i.status=Unrecognized;i.blocking=true;i.reason=(f.kind==KindMepsName?"khối tên dạng "+Library.NamePrefix+"… nhưng mất dấu MEPS (có thể do copy/đổi tên/explode)":"khối cùng tên ký hiệu thư viện nhưng không có dấu MEPS (chèn ngoài plugin)")+", "+f.count+" lần";
   i.action="MEPS không nhận ký hiệu theo tên (hình/nguồn có thể khác). Thay bằng ký hiệu đặt từ bảng MEPS "+Version+", hoặc xoá khỏi Model trước khi xuất.";return i;}
  // All-or-nothing export with actionable feedback: every used symbol is listed (OK or the reason and action), foreign look-alike references block the
  // export (never adopted by name), xrefs are reported. Sources read by an older parser are listed. Consistency with Legend.Plan is checked.
  public static LegendPreflight Preflight(List<UsedSymbol> used,LegendMap map,Func<string,LibEntry> entry,List<ForeignRef> foreign){used=used??new List<UsedSymbol>();
   var p=new LegendPreflight{map=map,plan=Legend.Plan(used,map,entry),used=used.Count,placements=used.Sum(u=>u.count)};
   foreach(var u in used)p.items.Add(Classify(u,map,entry));int fromUsed=p.items.Count(i=>i.blocking);
   foreach(var f in foreign??new List<ForeignRef>())p.items.Add(Foreign(f));
   foreach(var s in map.sources.Where(s=>s.parser!=Legend.Parser))p.staleSources.Add(s.sourceFile+" ("+(s.parser??"không rõ parser")+")");
   if(used.Count==0)p.items.Add(new PreflightItem{name="(Model)",status=Empty,blocking=true,reason="Model chưa có ký hiệu MEPS nào (đã bỏ qua xref, paper space, khối đã xoá)",action="Đặt ký hiệu từ bảng MEPS rồi thử lại."});
   if(fromUsed!=p.plan.missing.Count)p.items.Add(new PreflightItem{name="(kiểm tra)",status=Internal,blocking=true,reason="kiểm tra trước ("+fromUsed+") không khớp kế hoạch ("+p.plan.missing.Count+")",action="Không xuất; báo lỗi cho người phát triển."});
   return p;}
  // ---- 0.6.0 export policy (TVXD-MEPS-LEGEND-009; user rule 2026-10-09) ----
  // Production legend export: missing/uncertain source text is a non-blocking warning (cell left empty, never written by MEPS); blockers are only those of
  // Legend.Decide (library/frozen, source table read and complete, compatible transform), foreign look-alikes and an empty model. Preflight above keeps the
  // historical all-or-nothing rule. Consistency: blockers from the used symbols = plan.missing, and every non-blocked symbol is in the plan exactly once.
  public static PreflightItem ClassifyForExport(UsedSymbol u,LegendMap map,Func<string,LibEntry> entry){var d=Legend.Decide(u,map,entry);var e=d.entry;
   var i=new PreflightItem{name=d.name,entryId=u.entryId,sourceSha256=u.sourceSha256,count=u.count,status=d.status,reason=d.reason,blocking=d.blocking,action=""};
   if(e==null){i.status=NotInLibrary;i.action="Khối mang dấu MEPS nhưng mục không còn trong thư viện này: thêm lại nguồn bằng '+', hoặc thay bằng ký hiệu có trong bảng.";return i;}
   i.sourceSha256=e.sourceSha256;i.sourceFile=e.sourceFile;
   if(d.blocking)i.action=Meps05.PlaceIssue(e)!=null?"Làm theo cách xử lý ghi trong lý do.":Action(d.status,e,map);
   else if(d.status!=Legend.Ok){string parser=ParserOf(map,e.sourceSha256);
#if METADATA032
    // F032-02: with the per-library metadata overlay the user fills missing text from the palette (metadata032.json); the source file stays unchanged.
    i.action="Vẫn xuất được: ô "+string.Join(", ",d.blanks)+" để trống (để trống được phép). Muốn có chữ: chuột phải ký hiệu trong danh sách → Sửa thông tin bảng ký hiệu… (lưu trong thư viện, dùng cho mọi bản vẽ; không sửa file nguồn).";
#else
#if MODELLIB019
    // 019 (F08): a generated MODEL_CAPTURE019 table has no source table to re-read (the re-read is refused); its text is changed only by a new revision.
    if(parser=="MODEL_CAPTURE019"){i.action="Vẫn xuất được: ô "+string.Join(", ",d.blanks)+" để trống và sẽ GIỮ trống (bảng do MEPS tạo khi thêm từ Model, chữ do người dùng nhập; 'Đọc lại bảng nguồn' bị từ chối cho nguồn này). Muốn có chữ: thêm lại từ Model thành tên bản sửa mới (hậu tố -R02, -R03…; MEPS gợi ý tên) và nhập chữ khi thêm.";return i;}
#endif
    i.action="Vẫn xuất được: ô "+string.Join(", ",d.blanks)+" để trống. Muốn có chữ: bổ sung trong BẢN SAO file nguồn "+e.sourceFile+" rồi 'Đọc lại bảng nguồn'"+(parser!=null&&parser!=Legend.Parser?" (nguồn đang đọc bằng "+parser+"; đọc lại bằng "+Legend.Parser+" để giữ các ô chắc chắn của hàng mơ hồ)":"")+".";
#endif
   }
   return i;}
  public static LegendPreflight PreflightForExport(List<UsedSymbol> used,LegendMap map,Func<string,LibEntry> entry,List<ForeignRef> foreign){used=used??new List<UsedSymbol>();
   var p=new LegendPreflight{map=map,plan=Legend.PlanForExport(used,map,entry),used=used.Count,placements=used.Sum(u=>u.count),policy=Legend.PolicyBlank};
   foreach(var u in used)p.items.Add(ClassifyForExport(u,map,entry));var fromUsed=p.items.Where(i=>i.blocking).ToList();
   foreach(var i in p.items.Where(i=>!i.blocking&&i.status!=Legend.Ok))p.blankText.Add(i.name);
   foreach(var f in foreign??new List<ForeignRef>())p.items.Add(Foreign(f));
#if MODELLIB019
   // 019: MODEL_CAPTURE019 (= Naming019.Parser; literal so builds without ModelLibrary019.cs compile) is the current generated (self-authored) table of a
   // Model capture, not a table read by an older parser.
   foreach(var s in map.sources.Where(s=>s.parser!=Legend.Parser&&s.parser!="MODEL_CAPTURE019"))p.staleSources.Add(s.sourceFile+" ("+(s.parser??"không rõ parser")+")");
#else
   foreach(var s in map.sources.Where(s=>s.parser!=Legend.Parser))p.staleSources.Add(s.sourceFile+" ("+(s.parser??"không rõ parser")+")");
#endif
   if(used.Count==0)p.items.Add(new PreflightItem{name="(Model)",status=Empty,blocking=true,reason="Model chưa có ký hiệu MEPS nào (đã bỏ qua xref, paper space, khối đã xoá)",action="Đặt ký hiệu từ bảng MEPS rồi thử lại."});
   if(fromUsed.Count!=p.plan.missing.Count)p.items.Add(new PreflightItem{name="(kiểm tra)",status=Internal,blocking=true,reason="mục chặn ("+fromUsed.Count+") không khớp kế hoạch ("+p.plan.missing.Count+")",action="Không xuất; báo lỗi cho người phát triển."});
   var blocked=new HashSet<string>(fromUsed.Select(i=>i.entryId));var want=used.Where(u=>!blocked.Contains(u.entryId)).Select(u=>u.entryId).OrderBy(x=>x,StringComparer.Ordinal).ToList();
   var got=p.plan.rows.SelectMany(r=>r.symbols).Select(s=>s.entryId).OrderBy(x=>x,StringComparer.Ordinal).ToList();
   if(!want.SequenceEqual(got))p.items.Add(new PreflightItem{name="(kiểm tra)",status=Internal,blocking=true,reason="ký hiệu hợp lệ ("+want.Count+") không khớp ký hiệu trong kế hoạch ("+got.Count+"); không bỏ sót ký hiệu nào",action="Không xuất; báo lỗi cho người phát triển."});
   return p;}
  // Same model content for export: every item (status, name, count) and every planned symbol handle unchanged since the preflight.
  public static bool SameScan(LegendPreflight a,LegendPreflight b){if(a==null||b==null)return false;Func<LegendPreflight,string> key=p=>string.Join("\n",p.items.Select(i=>i.status+"|"+i.name+"|"+i.count).OrderBy(x=>x,StringComparer.Ordinal))+"\n#"+string.Join(",",p.plan.rows.SelectMany(r=>r.symbols).SelectMany(s=>s.handles).OrderBy(x=>x,StringComparer.Ordinal));return key(a)==key(b);}
  // ---- Source remap (LegendCad06.Remap) ----
  public static Dictionary<string,int> Counts(LegendMap m,string sha){var d=new Dictionary<string,int>();var rows=m.rows.Where(r=>Same(r.sourceSha256,sha)).ToList();d["rows"]=rows.Count;foreach(var g in rows.GroupBy(r=>r.status))d[g.Key]=g.Count();return d;}
  public static string CountText(Dictionary<string,int> d){return d==null?"-":string.Join(", ",d.OrderBy(x=>x.Key=="rows"?"":x.Key,StringComparer.Ordinal).Select(x=>x.Key+"="+x.Value));}
  // Remap is scoped to the selected symbol's source: the picked file must be a source of this library and have exactly that hash.
  public static void CheckRemap(LibCatalog c,string selectedSha,string pickedSha){if(string.IsNullOrEmpty(selectedSha)||!c.entries.Any(e=>Same(e.sourceSha256,selectedSha)))throw new Exception("Mục đã chọn không thuộc nguồn nào của thư viện "+Meps05.LibraryDir+".");
   if(!Same(selectedSha,pickedSha))throw new Exception("File đã chọn (hash "+Short(pickedSha)+") khác nguồn "+Short(selectedSha)+" của ký hiệu; không đọc. Chỉ đọc lại đúng nguồn đã chọn.");}
  public static string Short(string sha){return string.IsNullOrEmpty(sha)?"?":sha.Substring(0,Math.Min(8,sha.Length));}
  // Serialized records of every OTHER source (must be identical before and after a scoped remap).
  public static string OthersSignature(LegendMap m,string sha){var j=Library.Json();return j.Serialize(m.sources.Where(s=>!Same(s.sourceSha256,sha)).ToList())+"\n"+j.Serialize(m.rows.Where(r=>!Same(r.sourceSha256,sha)).ToList());}
  public static string RemapLine(DateTime utc,string sha,string sourceFile,string parserBefore,string parserAfter,Dictionary<string,int> before,Dictionary<string,int> after){
   return string.Join("\t",new[]{utc.ToString("yyyy-MM-ddTHH:mm:ss.fffZ",CultureInfo.InvariantCulture),"v"+Version,"source="+sha,PlacementLog.Clean(sourceFile,260),"parser "+(parserBefore??"-")+" -> "+(parserAfter??"-"),"before "+CountText(before),"after "+CountText(after)});}
 }
}
