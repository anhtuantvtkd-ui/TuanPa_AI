#if LAYOUT030
using System;using System.IO;using System.Linq;using System.Collections.Generic;using System.Security.Cryptography;using Autodesk.AutoCAD.Runtime;using Autodesk.AutoCAD.ApplicationServices;using Autodesk.AutoCAD.DatabaseServices;using App=Autodesk.AutoCAD.ApplicationServices.Application;using Ex=System.Exception;
[assembly:CommandClass(typeof(Tvxd.Meps.Layout030Commands))]
namespace Tvxd.Meps {
 // TVXD-MEPS-LAYOUT-030: palette "Dàn trang ▾" → Queue(command) → native dispatch of the five T- commands of Layout030.lsp (next to the DLL).
 //  - Only Commands are accepted; the request captures the active Document, a random token and the SHA-256 of Layout030.lsp. No user-supplied code runs.
 //  - First request per Document / source hash: a fixed, guarded Lisp expression (LoadExpression) is queued in THAT document: (load full-path) inside
 //    vl-catch-all-apply, then TVXDLAYOUT030READY token status. Only "OK" for the same token in the same, still active Document marks the request ready and
 //    caches the hash for the Document (a repeat command does not reload the file, so the Lisp configuration is not reset). A failed/cancelled load refuses
 //    execution (stale functions never run) and clears the request.
 //  - Ready → TVXDLAYOUT030DO (CommandFlags.Modal|NoUndoMarker): recheck Document/request/hash, then (035, F035-DISPATCH01) queue the exact whitelisted
 //    command name "<command> " with SendStringToExecute and RETURN. AutoCAD runs it after TVXDLAYOUT030DO ends, as a normal top-level command — the same
 //    path as typing it, so the original Lisp's nested (command ...) calls and its Undo grouping behave as when typed. App.Invoke is no longer used: in
 //    native QA (TESTS/LAYOUT035/GUI1) the same original T-DTL failed "khong sao chep duoc layout mau" through Invoke and succeeded when typed.
 //  - After the send, the request stays pending (Busy) until the command really ends: LispWillStart whose FirstLine names "C:<command>" marks it started;
 //    LispEnded / LispCancelled at the same nesting depth ends it. Before the start is seen, a bounded Idle check drops it (StaleSeconds) only when no
 //    command/Lisp is active (CMDACTIVE = 0); after the start there is no timeout (user dialogs/prompts may take any time). Each request is sent once.
 //  - A request is dropped before start on document switch/close, on a Lisp cancel/end of OUR load without a result, or by the bounded Idle check
 //    (handler attached only while a request is pending — no timer, no daemon). Unknown or late tokens are ignored.
 //  - Busy is shared with placement/wire/flex/model actions through OtherBusy (set by Plugin) and Plugin.FlexBusy. PaperSpace is allowed; the Lisp validates
 //    each command. Queued is not success: each command prints its own result on the command line (the six original commands print their original
 //    messages; only T-STT prints THANH CONG / DA HUY / THAT BAI and owns its Undo group). No generalized eval, no document activation, no retries.
 public static class Layout030 {
  public const string SourceName="Layout030.lsp",DoCommand="TVXDLAYOUT030DO",ReadyFunction="TVXDLAYOUT030READY",ReadyOk="OK";
  public const int StaleSeconds=20;
  // 037 (user 2026-10-11): T-DTV / T-DTA removed from the palette whitelist; five commands remain.
  public static readonly string[] Commands={"T-CK","T-STT","T-DTL","T-DTM","T-TKI"};
  public static bool Allowed(string command){return command!=null&&Commands.Contains(command,StringComparer.Ordinal);}
  // loadStarted: our load expression began. sent: the command name was queued (once). depth: Lisp nesting seen after the send. started: the top-level
  // Lisp evaluation of "C:<command>" began (the command is running).
  sealed class Request{public Document doc;public string command,token,hash,path;public bool ready,loadStarted,sent,started;public int depth;public DateTime created,sentAt;}
  static Request pending;static bool hooked,idle;static Document watched;
  static readonly Dictionary<Document,string> loaded=new Dictionary<Document,string>();
  // Set by Plugin: placement/model queue, wire and flex jobs (never this module itself). Say/Warn: palette messages (Warn also echoes on the command line).
  public static Func<bool> OtherBusy;public static Action<string> Say,Warn;
  // Busy from the click until the command really ends (or the request is dropped before it starts).
  public static bool Busy{get{return pending!=null;}}
  // Read-only views for QA.
  public static string PendingCommand{get{var r=pending;return r==null?null:r.command;}}public static bool PendingReady{get{var r=pending;return r!=null&&r.ready;}}
  public static bool PendingSent{get{var r=pending;return r!=null&&r.sent;}}
  public static bool Running{get{var r=pending;return r!=null&&r.started;}}public static string LastMessage{get{return lastMessage;}}public static string LastError{get{return lastError;}}
  public static int Dispatched{get{return dispatched;}}public static int IgnoredTokens{get{return ignored;}}
  public static int Completed{get{return completed;}}public static int Cancelled{get{return cancelled;}}public static string LastStartLine{get{return lastStartLine;}}
  public static string LoadedHash(Document d){string h;return d!=null&&loaded.TryGetValue(d,out h)?h:null;}
  static string lastMessage,lastError,lastStartLine;static int dispatched,ignored,completed,cancelled;
  // CMDACTIVE of the active document (0 = no command, script, dialog or AutoLISP active); -1 if unreadable.
  static int CmdActive(){try{return Convert.ToInt32(App.GetSystemVariable("CMDACTIVE"));}catch{return -1;}}
  public static string SourcePath{get{return Path.Combine(Path.GetDirectoryName(typeof(Layout030).Assembly.Location),SourceName);}}
  public static string Hash(string path){using(var sha=SHA256.Create())using(var s=File.OpenRead(path))return string.Concat(sha.ComputeHash(s).Select(b=>b.ToString("x2")));}
  // The only Lisp text this module sends besides the command name: fixed form, path with forward slashes, hex token. A path that cannot be written as a
  // plain Lisp string (quote, control character) is refused instead of escaped.
  public static string LoadExpression(string path,string token){
   if(string.IsNullOrEmpty(path)||path.Any(c=>c=='"'||c<' '))throw new Ex("Đường dẫn "+SourceName+" có ký tự không dùng được trong Lisp; chép bộ cài vào thư mục khác. Chưa chạy gì.");
   if(string.IsNullOrEmpty(token)||!token.All(Uri.IsHexDigit))throw new Ex("Mã yêu cầu không hợp lệ.");
   return "((lambda ( / r) (vl-load-com) (setq r (vl-catch-all-apply 'load (list \""+path.Replace('\\','/')+"\"))) ("+ReadyFunction+" \""+token+"\" (if (vl-catch-all-error-p r) (vl-catch-all-error-message r) \""+ReadyOk+"\")) (princ))) ";}
  static void Info(string s){lastMessage=s;var a=Say;if(a!=null)a(s);else Echo(s);}
  static void Bad(string s){lastMessage=s;lastError=s;var a=Warn;if(a!=null)a(s);else Echo(s);}
  static void Echo(string s){try{var d=App.DocumentManager.MdiActiveDocument;if(d!=null)d.Editor.WriteMessage("\nTVXD Dàn trang: "+s);}catch{}}
  static string Name(Request r){return r.command+" ("+Path.GetFileName(r.doc.Name)+")";}
  // Palette entry (Plugin.LayoutAction). Throws with a Vietnamese reason when refused; nothing is queued then.
  public static void Queue(string command){
   if(!Allowed(command))throw new Ex("Lệnh dàn trang không hợp lệ: "+command+".");
   var d=App.DocumentManager.MdiActiveDocument;if(d==null)throw new Ex("Mở bản vẽ trước.");
   if(Busy)throw new Ex("Đang chờ/chạy lệnh dàn trang "+(PendingCommand??"")+"; hoàn tất hoặc Esc trước.");
   if(OtherBusy!=null&&OtherBusy())throw new Ex("Hoàn tất thao tác MEPS đang chạy trước (hoặc ESC).");
   if(!string.IsNullOrEmpty(d.CommandInProgress))throw new Ex("Hoàn tất lệnh CAD đang chạy ("+d.CommandInProgress+") trước.");
   // 035: the command name is sent as command-line input, so also refuse while any command/Lisp/script/dialog is active (it would become its input).
   if(CmdActive()!=0)throw new Ex("Hoàn tất lệnh hoặc Lisp đang chạy trên dòng lệnh trước (Esc), rồi bấm lại.");
   string path=SourcePath;if(!File.Exists(path))throw new Ex("Không thấy "+SourceName+" cạnh DLL ("+path+"); chưa chạy gì.");
   string hash;try{hash=Hash(path);}catch(Ex ex){throw new Ex("Không đọc được "+SourceName+": "+ex.Message+". Chưa chạy gì.");}
   Hook();string have;bool cached=loaded.TryGetValue(d,out have)&&have==hash;
   var r=new Request{doc=d,command=command,token=Guid.NewGuid().ToString("N"),hash=hash,path=path,ready=cached,created=DateTime.UtcNow};
   string text=cached?DoCommand+" ":LoadExpression(path,r.token);
   pending=r;Watch(d);
   try{d.SendStringToExecute(text,false,false,false);}catch(Ex ex){pending=null;Unwatch();throw new Ex("Chưa gửi được lệnh tới AutoCAD: "+ex.Message);}
   Info(cached?"Đang mở "+command+": theo lời nhắc trên dòng lệnh; Esc huỷ.":"Đang nạp "+SourceName+" (lần đầu trong bản vẽ này) rồi mở "+command+"; Esc huỷ.");}
  static void Drop(Request r,string why){if(r==null)return;if(pending==r){pending=null;Unwatch();}Bad(why);}
  static void Hook(){if(hooked)return;hooked=true;
   // A started command belongs to its document; it is not dropped by a switch and ends through its own Lisp events (or document close).
   App.DocumentManager.DocumentActivated+=(s,e)=>{var r=pending;if(r!=null&&r.doc!=e.Document&&!r.started&&r.depth==0)Drop(r,NotStarted(r,"Đổi bản vẽ"));};
   App.DocumentManager.DocumentToBeDestroyed+=(s,e)=>{var r=pending;if(r!=null&&r.doc==e.Document)Drop(r,"Bản vẽ đóng: đã huỷ yêu cầu "+r.command+".");loaded.Remove(e.Document);if(watched==e.Document)Unwatch();};}
  // Lisp events of the request's document and Idle are attached only while a request is pending.
  static void Watch(Document d){Unwatch();watched=d;try{d.LispWillStart+=LispStart;d.LispEnded+=LispEnd;d.LispCancelled+=LispCancel;}catch{}if(!idle){idle=true;App.Idle+=OnIdle;}}
  static void Unwatch(){var d=watched;watched=null;if(d!=null)try{d.LispWillStart-=LispStart;d.LispEnded-=LispEnd;d.LispCancelled-=LispCancel;}catch{}if(idle){idle=false;App.Idle-=OnIdle;}}
  // Message for a request dropped before its command started. After the send the queued name cannot be withdrawn, so say so instead of "nothing ran".
  static string NotStarted(Request r,string why){return why+": "+(r.sent
   ?"đã thôi theo dõi "+Name(r)+" (đã gửi nhưng chưa thấy bắt đầu). Lệnh đã gửi có thể vẫn chạy trong bản vẽ đó như khi gõ tay; kiểm tra dòng lệnh."
   :"đã huỷ yêu cầu "+Name(r)+"; chưa chạy gì.");}
  // True when a Lisp FirstLine is the top-level evaluation of "C:<command>" (case-insensitive, whole name, e.g. "(C:T-DTL)").
  static bool NamesCommand(string line,string command){if(line==null)return false;string k="C:"+command;
   int i=line.IndexOf(k,StringComparison.OrdinalIgnoreCase);if(i<0)return false;int j=i+k.Length;
   return j>=line.Length||!(char.IsLetterOrDigit(line[j])||line[j]=='-'||line[j]=='_');}
  // Before ready: our load expression is recognised by its token in the first line. After the send: every Lisp start in the watched document counts
  // nesting; the first top-level one naming the command marks it started. Other Lisp evaluations do not end the request.
  static void LispStart(object s,LispWillStartEventArgs e){var r=pending;if(r==null)return;string f=e==null?null:e.FirstLine;
   if(!r.ready){if(f!=null&&f.Contains(r.token))r.loadStarted=true;return;}
   if(!r.sent)return;
   r.depth++;if(r.depth==1&&!r.started&&NamesCommand(f,r.command)){r.started=true;lastStartLine=f;}}
  static void LispEnd(object s,EventArgs e){LispStop(false);}
  static void LispCancel(object s,EventArgs e){LispStop(true);}
  static void LispStop(bool cancel){var r=pending;if(r==null)return;
   if(!r.ready){if(r.loadStarted)Drop(r,cancel?"Đã huỷ nạp "+SourceName+"; chưa chạy "+r.command+".":"Nạp "+SourceName+" kết thúc mà không báo kết quả; chưa chạy "+r.command+". Bấm lại để thử.");return;}
   if(!r.sent||r.depth==0)return;
   r.depth--;if(r.depth>0||!r.started)return;
   pending=null;Unwatch();
   if(cancel){cancelled++;Info(r.command+" đã dừng (Esc hoặc lỗi trong lệnh); xem dòng lệnh. Thay đổi đã làm (nếu có) kiểm tra rồi dùng U nếu cần.");}
   else{completed++;Info(r.command+" đã kết thúc; kết quả xem trên dòng lệnh.");}}
  // Bounded pre-start check only. Never times out once the command (or any Lisp after the send) is active.
  static void OnIdle(object s,EventArgs e){var r=pending;if(r==null){Unwatch();return;}if(r.started||r.depth>0)return;
   if(App.DocumentManager.MdiActiveDocument!=r.doc){Drop(r,NotStarted(r,"Bản vẽ đã đổi"));return;}
   if(r.sent){
    if(CmdActive()==0&&string.IsNullOrEmpty(r.doc.CommandInProgress)&&(DateTime.UtcNow-r.sentAt).TotalSeconds>StaleSeconds){
     loaded.Remove(r.doc);Drop(r,"Không thấy "+r.command+" bắt đầu sau "+StaleSeconds+" giây; đã thôi theo dõi. Xem dòng lệnh; lần bấm sau sẽ nạp lại "+SourceName+".");}
    return;}
   if((DateTime.UtcNow-r.created).TotalSeconds>StaleSeconds)Drop(r,"Yêu cầu "+r.command+" không bắt đầu sau "+StaleSeconds+" giây; đã huỷ, chưa chạy gì. Bấm lại.");}
  // TVXDLAYOUT030READY token status — called by the fixed load expression only. Unknown/late token: ignored (counted).
  internal static void Ready(string token,string status){var r=pending;
   if(r==null||r.ready||token==null||!string.Equals(r.token,token,StringComparison.Ordinal)){ignored++;return;}
   if(App.DocumentManager.MdiActiveDocument!=r.doc){Drop(r,"Bản vẽ đã đổi khi nạp "+SourceName+"; chưa chạy "+r.command+".");return;}
   if(!string.Equals(status,ReadyOk,StringComparison.Ordinal)){loaded.Remove(r.doc);Drop(r,"Lỗi khi nạp "+SourceName+": "+(string.IsNullOrEmpty(status)?"(không rõ)":status)+". Chưa chạy "+r.command+".");return;}
   try{if(Hash(r.path)!=r.hash){loaded.Remove(r.doc);Drop(r,SourceName+" đã thay đổi trong lúc nạp; chưa chạy "+r.command+". Bấm lại.");return;}}
   catch(Ex ex){loaded.Remove(r.doc);Drop(r,"Không đọc lại được "+SourceName+": "+ex.Message+". Chưa chạy "+r.command+".");return;}
   loaded[r.doc]=r.hash;r.ready=true;
   try{r.doc.SendStringToExecute(DoCommand+" ",false,false,false);}catch(Ex ex){Drop(r,"Đã nạp "+SourceName+" nhưng chưa gửi được "+r.command+": "+ex.Message+".");}}
  // TVXDLAYOUT030DO body. A manual TVXDLAYOUT030DO without a ready request of THIS document runs nothing; an already sent request is never sent again.
  // On success it only queues "<command> " and returns: the command starts after this .NET command ends. The request stays pending (Busy) until the
  // command's Lisp ends (LispStop) or the bounded pre-start check drops it.
  internal static void Dispatch(){var d=App.DocumentManager.MdiActiveDocument;var r=pending;
   if(r==null){if(d!=null)Echo("Không có yêu cầu dàn trang đang chờ; bấm lại trong bảng MEPS.");return;}
   if(r.sent){Echo(r.command+" đã được gửi; không gửi lại.");return;}
   if(d==null||d!=r.doc){Drop(r,"Bản vẽ đã đổi; không chạy "+r.command+".");return;}
   if(!r.ready||!Allowed(r.command)){Drop(r,SourceName+" chưa nạp xong trong bản vẽ này; không chạy "+r.command+".");return;}
   string have;if(!loaded.TryGetValue(d,out have)||have!=r.hash){Drop(r,SourceName+" chưa nạp (hoặc đã đổi) trong bản vẽ này; không chạy "+r.command+". Bấm lại.");return;}
   try{if(Hash(r.path)!=have){loaded.Remove(d);Drop(r,SourceName+" đã thay đổi sau khi nạp; không chạy bản cũ. Bấm lại để nạp bản mới.");return;}}
   catch(Ex ex){Drop(r,"Không đọc được "+SourceName+": "+ex.Message+"; không chạy "+r.command+".");return;}
   r.sent=true;r.sentAt=DateTime.UtcNow;r.depth=0;lastError=null;dispatched++;
   // Exact whitelisted name + one Enter; echoed like a typed command. Not activated, not wrapped into another document.
   try{d.SendStringToExecute(r.command+" ",false,false,true);}
   catch(Ex ex){Drop(r,"Chưa gửi được "+r.command+" ra dòng lệnh: "+ex.Message+". Chưa chạy gì.");return;}
   Info("Đã gửi "+r.command+" ra dòng lệnh (như gõ lệnh); theo lời nhắc, Esc huỷ. Kết quả hiện trên dòng lệnh.");}
 }
 public sealed class Layout030Commands {
  [CommandMethod(Layout030.DoCommand,CommandFlags.Modal|CommandFlags.NoUndoMarker)]public void Do(){Layout030.Dispatch();}
  // (TVXDLAYOUT030READY "token" "OK"|"error message"): narrow callback of the fixed load expression; anything else is ignored.
  [LispFunction(Layout030.ReadyFunction)]public object Ready(ResultBuffer args){
   var v=args==null?new TypedValue[0]:args.AsArray();
   string token=v.Length>0?v[0].Value as string:null,status=v.Length>1?v[1].Value as string:null;
   Layout030.Ready(token,status);return null;}
 }
}
#endif
