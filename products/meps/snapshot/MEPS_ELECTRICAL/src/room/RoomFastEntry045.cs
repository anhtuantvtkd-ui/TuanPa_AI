using System;using System.Collections.Generic;using System.Globalization;using System.Linq;using System.Text;
namespace Tvxd.Meps {
 // TVXD-MEPS-FAST-ENTRY-045: PURE core of the Excel-like table entry and the reusable presets. No AutoCAD API, no Forms, no NOD, no file IO.
 // ASCII-only source: reason codes are English; RoomFastEntryUi045 maps them to Vietnamese text. C# 5 compatible.
 //  - Persisted data (inside RoomProjectDocument037 v2, field "fast"): named presets of the inheritable route fields, defaults chosen explicitly by the
 //    user (one per room, one per verified device-kind pair) and per-room row state (row preset + explicit-blank fields). Nothing is preset by MEPS:
 //    no preset, no default and no value exists until the user creates/chooses it.
 //  - Inheritable fields = routingMode, tailCount, tailEachLength, factor, assemblyCode, conduitCode. circuitId, endpoints, geometry, floor, heights,
 //    conduitMode and the independent-assembly confirmation are NEVER taken from a preset.
 //  - Precedence per field: RIENG (explicit row value, also an explicit blank) > row preset > room default > device-kind default > missing (null).
 //    A preset that leaves a field empty does not stop the chain for that field. Explicit values are stored in the room draft rows exactly as before
 //    (so every v1 declaration is an explicit value after migration); inherited values are never written into drafts.
 //  - Rectangle paste / fill-down planning works on the VISIBLE grid (display order of rows and columns, stable row keys) and validates the whole
 //    target before anything is changed: one refusal = no target, with the exact row/column of the first problem.
 public sealed class FastPreset045 {public string id,name,routingMode,assemblyCode,conduitCode;public int? tailCount;public double? tailEachLength,factor;}
 // scope ROOM: key = stable room ID (RM_...). scope KIND: key = verified device-type pair "START>END" (FastEntry045.KindKey).
 public sealed class FastDefault045 {public string scope,key,presetId;}
 // One route row of one room: chosen row preset (null = none) and fields explicitly left blank (explicit RIENG with no value).
 public sealed class FastRowState045 {public string rowKey,presetId;public List<string> blanks=new List<string>();}
 public sealed class FastRoomState045 {public string roomId;public List<FastRowState045> rows=new List<FastRowState045>();}
 public sealed class FastEntryData045 {
  public int version=1;
  public List<FastPreset045> presets=new List<FastPreset045>();
  public List<FastDefault045> defaults=new List<FastDefault045>();
  public List<FastRoomState045> rooms=new List<FastRoomState045>();
 }
 // Session (not persisted) state of the open room: persisted row state + the explicit (RIENG) field set of every row + table undo history.
 public sealed class FastSession045 {
  public string roomId;
  public readonly Dictionary<string,FastRowState045> rows=new Dictionary<string,FastRowState045>(StringComparer.Ordinal);
  public readonly Dictionary<string,HashSet<string>> explicitFields=new Dictionary<string,HashSet<string>>(StringComparer.Ordinal);
  public readonly FastHistory045<object> history=new FastHistory045<object>(FastEntry045.MaxHistory);
 }

 public static class FastEntry045 {
  public const int DataVersion=1,MaxPresets=200,MaxName=100,MaxId=64,MaxRooms=2000,MaxRows=5000,MaxHistory=50,MaxPasteCells=20000,MaxPasteChars=1000000;
  public const string Room="ROOM",Kind="KIND";
  // Source labels of an effective value (UI maps them to text).
  public const string SrcExplicit="RIENG",SrcRowPreset="ROW_PRESET",SrcRoom="ROOM_DEFAULT",SrcKind="KIND_DEFAULT",SrcMissing="MISSING";
  public const string Routing="routingMode",TailCount="tailCount",TailEach="tailEachLength",Factor="factor",Assembly="assemblyCode",Conduit="conduitCode";
  public static readonly string[] PresetFields={Routing,TailCount,TailEach,Factor,Assembly,Conduit};
  static readonly StringComparer O=StringComparer.Ordinal;
  static readonly CultureInfo Inv=CultureInfo.InvariantCulture;
  public static bool IsPresetField(string f){return f!=null&&Array.IndexOf(PresetFields,f)>=0;}

  // ---- preset values ----
  public static object Value(FastPreset045 p,string field){if(p==null)return null;
   switch(field){case Routing:return p.routingMode;case TailCount:return p.tailCount;case TailEach:return p.tailEachLength;case Factor:return p.factor;
    case Assembly:return p.assemblyCode;case Conduit:return p.conduitCode;default:throw new ArgumentException("not a preset field: "+field);}}
  public static void SetValue(FastPreset045 p,string field,object v){
   switch(field){case Routing:p.routingMode=(string)v;break;case TailCount:p.tailCount=(int?)v;break;case TailEach:p.tailEachLength=(double?)v;break;
    case Factor:p.factor=(double?)v;break;case Assembly:p.assemblyCode=(string)v;break;case Conduit:p.conduitCode=(string)v;break;
    default:throw new ArgumentException("not a preset field: "+field);}}
  public static bool Empty(FastPreset045 p){return PresetFields.All(f=>Value(p,f)==null);}
  // Verified device-kind key of a route row: both authoritative scan types present; anything else = null (unassigned, never guessed).
  public static string KindKey(string startType,string endType){
   if(string.IsNullOrEmpty(startType)||string.IsNullOrEmpty(endType)||startType.IndexOf('>')>=0||endType.IndexOf('>')>=0)return null;
   return startType+">"+endType;}

  // ---- copies (deep; never share lists with the source) ----
  public static FastPreset045 Copy(FastPreset045 p){return p==null?null:new FastPreset045{id=p.id,name=p.name,routingMode=p.routingMode,assemblyCode=p.assemblyCode,
   conduitCode=p.conduitCode,tailCount=p.tailCount,tailEachLength=p.tailEachLength,factor=p.factor};}
  public static FastRowState045 Copy(FastRowState045 r){return r==null?null:new FastRowState045{rowKey=r.rowKey,presetId=r.presetId,blanks=r.blanks==null?null:new List<string>(r.blanks)};}
  public static FastRoomState045 Copy(FastRoomState045 r){return r==null?null:new FastRoomState045{roomId=r.roomId,rows=r.rows==null?null:r.rows.Select(Copy).ToList()};}
  public static FastEntryData045 Copy(FastEntryData045 d){if(d==null)return null;
   return new FastEntryData045{version=d.version,presets=d.presets==null?null:d.presets.Select(Copy).ToList(),
    defaults=d.defaults==null?null:d.defaults.Select(x=>x==null?null:new FastDefault045{scope=x.scope,key=x.key,presetId=x.presetId}).ToList(),
    rooms=d.rooms==null?null:d.rooms.Select(Copy).ToList()};}
  // PATCH1 F01: session row state + RIENG sets (export copy of the open room); the undo history is NOT copied (the copy starts with an empty one).
  public static FastSession045 Copy(FastSession045 s){if(s==null)return null;var t=new FastSession045{roomId=s.roomId};
   foreach(var kv in s.rows)t.rows[kv.Key]=Copy(kv.Value);
   foreach(var kv in s.explicitFields)t.explicitFields[kv.Key]=new HashSet<string>(kv.Value??new HashSet<string>(O),O);
   return t;}

  // ---- validation (null = valid). valueProblem(field,value): engineering value check of the product parser (null = no extra check). ----
  public static string Problem(FastEntryData045 d,ICollection<string> roomIds,Func<string,object,string> valueProblem){
   if(d==null)return null;
   if(d.version!=DataVersion)return "fast entry data version "+d.version.ToString(Inv)+" is not supported";
   if(d.presets==null||d.defaults==null||d.rooms==null)return "fast entry lists are required";
   if(d.presets.Count>MaxPresets)return "more than "+MaxPresets.ToString(Inv)+" presets";
   var ids=new HashSet<string>(O);var names=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
   foreach(var p in d.presets){if(p==null)return "null preset";
    var e=IdProblem(p.id,"preset id");if(e!=null)return e;
    if(string.IsNullOrEmpty(p.name)||p.name.Length>MaxName||p.name.Trim().Length!=p.name.Length||p.name.Any(char.IsControl))return "preset name of "+p.id+" is empty, too long, padded or has control characters";
    if(!ids.Add(p.id))return "duplicate preset id "+p.id;
    if(!names.Add(p.name))return "duplicate preset name "+p.name;
    if(p.tailCount.HasValue&&p.tailCount.Value<0)return "preset "+p.name+": negative tail count";
    foreach(var v in new[]{p.tailEachLength,p.factor})if(v.HasValue&&(double.IsNaN(v.Value)||double.IsInfinity(v.Value)))return "preset "+p.name+": non-finite number";
    if(valueProblem!=null)foreach(var f in PresetFields){var v=Value(p,f);if(v==null)continue;var q=valueProblem(f,v);if(q!=null)return "preset "+p.name+" field "+f+": "+q;}}
   var seen=new HashSet<string>(O);
   foreach(var x in d.defaults){if(x==null)return "null default";
    if(x.scope!=Room&&x.scope!=Kind)return "default scope "+x.scope+" is not ROOM or KIND";
    if(string.IsNullOrEmpty(x.key)||x.key.Length>400||x.key.Any(char.IsControl))return "default key is empty, too long or has control characters";
    if(x.scope==Room&&roomIds!=null&&!roomIds.Contains(x.key))return "room default for unknown room "+x.key;
    if(x.scope==Kind&&x.key.Split('>').Length!=2)return "kind default key "+x.key+" is not START>END";
    if(x.presetId==null||!ids.Contains(x.presetId))return "default "+x.scope+" "+x.key+" points to a missing preset";
    if(!seen.Add(x.scope+"\n"+x.key))return "two defaults for "+x.scope+" "+x.key;}
   if(d.rooms.Count>MaxRooms)return "too many room states";
   var rooms=new HashSet<string>(O);
   foreach(var r in d.rooms){if(r==null)return "null room state";
    if(r.roomId==null||(roomIds!=null&&!roomIds.Contains(r.roomId)))return "room state for unknown room "+r.roomId;
    if(!rooms.Add(r.roomId))return "two room states for "+r.roomId;
    if(r.rows==null||r.rows.Count>MaxRows)return "room state rows of "+r.roomId+" missing or too many";
    var keys=new HashSet<string>(O);
    foreach(var w in r.rows){if(w==null)return "null row state in "+r.roomId;
     if(string.IsNullOrEmpty(w.rowKey)||w.rowKey.Length>400)return "row state without row key in "+r.roomId;
     if(!keys.Add(w.rowKey))return "two row states for "+w.rowKey+" in "+r.roomId;
     if(w.presetId!=null&&!ids.Contains(w.presetId))return "row "+w.rowKey+" points to a missing preset";
     if(w.blanks==null)return "row "+w.rowKey+" blanks list missing";
     if(w.blanks.Any(f=>!IsPresetField(f))||w.blanks.Distinct(O).Count()!=w.blanks.Count)return "row "+w.rowKey+" has unknown or duplicate blank fields";}}
   return null;}
  static string IdProblem(string id,string what){
   if(string.IsNullOrEmpty(id)||id.Length>MaxId||id.Any(c=>!(c>='0'&&c<='9'||c>='A'&&c<='Z'||c>='a'&&c<='z'||c=='_'||c=='-')))return what+" '"+id+"' is empty, too long or has characters other than [A-Za-z0-9_-]";
   return null;}

  // ---- lookups ----
  public static FastPreset045 Preset(FastEntryData045 d,string id){return d==null||d.presets==null||id==null?null:d.presets.FirstOrDefault(p=>p!=null&&O.Equals(p.id,id));}
  public static string DefaultOf(FastEntryData045 d,string scope,string key){if(d==null||d.defaults==null||key==null)return null;
   var x=d.defaults.FirstOrDefault(q=>q!=null&&q.scope==scope&&O.Equals(q.key,key));return x==null?null:x.presetId;}
  public static FastRoomState045 RoomState(FastEntryData045 d,string roomId){return d==null||d.rooms==null||roomId==null?null:d.rooms.FirstOrDefault(r=>r!=null&&O.Equals(r.roomId,roomId));}

  // ---- precedence ----
  // Effective value + source of one inheritable field of one row. explicitValue is used only when isExplicit.
  public static object Resolve(FastEntryData045 d,string roomId,string kindKey,string rowPresetId,bool isExplicit,object explicitValue,string field,out string source){
   if(!IsPresetField(field))throw new ArgumentException("not a preset field: "+field);
   if(isExplicit){source=SrcExplicit;return explicitValue;}
   object v;
   v=Value(Preset(d,rowPresetId),field);if(v!=null){source=SrcRowPreset;return v;}
   v=Value(Preset(d,DefaultOf(d,Room,roomId)),field);if(v!=null){source=SrcRoom;return v;}
   v=Value(Preset(d,DefaultOf(d,Kind,kindKey)),field);if(v!=null){source=SrcKind;return v;}
   source=SrcMissing;return null;}

  // ---- session state ----
  public static FastRowState045 Row(FastSession045 s,string rowKey,bool create){FastRowState045 r;
   if(s.rows.TryGetValue(rowKey,out r))return r;if(!create)return null;r=new FastRowState045{rowKey=rowKey};s.rows[rowKey]=r;return r;}
  public static HashSet<string> Explicit(FastSession045 s,string rowKey){HashSet<string> h;
   if(!s.explicitFields.TryGetValue(rowKey,out h)){h=new HashSet<string>(O);s.explicitFields[rowKey]=h;}return h;}
  // Persisted room state of the session (rows with a preset or explicit blanks only; sorted, deterministic).
  public static FastRoomState045 ToRoomState(FastSession045 s){var r=new FastRoomState045{roomId=s.roomId};
   foreach(var k in s.rows.Keys.OrderBy(x=>x,O)){var w=s.rows[k];var bl=w.blanks==null?new List<string>():w.blanks.Where(IsPresetField).Distinct(O).OrderBy(x=>x,O).ToList();
    if(w.presetId==null&&bl.Count==0)continue;r.rows.Add(new FastRowState045{rowKey=k,presetId=w.presetId,blanks=bl});}
   return r;}
  // Replaces (or removes when empty) the room state of one room inside d (d is changed; callers pass their own copy).
  public static void PutRoomState(FastEntryData045 d,FastRoomState045 r){if(d==null||r==null)return;if(d.rooms==null)d.rooms=new List<FastRoomState045>();
   d.rooms.RemoveAll(x=>x!=null&&O.Equals(x.roomId,r.roomId));if(r.rows!=null&&r.rows.Count>0)d.rooms.Add(Copy(r));
   d.rooms.Sort((a,b)=>O.Compare(a.roomId,b.roomId));}
  // Defaults: presetId null = remove the default (explicit user action).
  public static void SetDefault(FastEntryData045 d,string scope,string key,string presetId){
   if(scope!=Room&&scope!=Kind)throw new ArgumentException("scope");if(string.IsNullOrEmpty(key))throw new ArgumentException("key");
   d.defaults.RemoveAll(x=>x!=null&&x.scope==scope&&O.Equals(x.key,key));
   if(presetId!=null)d.defaults.Add(new FastDefault045{scope=scope,key=key,presetId=presetId});
   d.defaults.Sort((a,b)=>{int c=O.Compare(a.scope,b.scope);return c!=0?c:O.Compare(a.key,b.key);});}
  // A preset removed from the list also loses every default/row reference to it (they become 'no preset', never another preset).
  public static void PruneReferences(FastEntryData045 d){var ids=new HashSet<string>(d.presets.Where(p=>p!=null).Select(p=>p.id),O);
   d.defaults.RemoveAll(x=>x==null||!ids.Contains(x.presetId));
   foreach(var r in d.rooms)if(r!=null&&r.rows!=null)foreach(var w in r.rows)if(w!=null&&w.presetId!=null&&!ids.Contains(w.presetId))w.presetId=null;
   d.rooms.RemoveAll(r=>r==null||r.rows==null||r.rows.All(w=>w==null||(w.presetId==null&&(w.blanks==null||w.blanks.Count==0))));}

  // Room defaults / room states of rooms that no longer exist are removed (a deleted room never passes its choices to another room).
  public static void PruneRooms(FastEntryData045 d,ICollection<string> roomIds){if(d==null||roomIds==null)return;
   if(d.defaults!=null)d.defaults.RemoveAll(x=>x!=null&&x.scope==Room&&!roomIds.Contains(x.key));
   if(d.rooms!=null)d.rooms.RemoveAll(r=>r!=null&&(r.roomId==null||!roomIds.Contains(r.roomId)));}

  // ---- TSV (Excel clipboard text) ----
  // Rows separated by CRLF / LF / CR, cells by TAB; a quoted cell ("...", "" = ") may contain TAB / line breaks. Exactly ONE final line break is the
  // row terminator Excel appends and is dropped; "" (after that) = one row with one blank cell (Excel copy of one empty cell). Ragged rows = error.
  public static List<List<string>> ParseTsv(string text,out string error){error=null;
   if(text==null){error="CLIPBOARD_EMPTY";return null;}
   if(text.Length>MaxPasteChars){error="CLIPBOARD_TOO_LARGE";return null;}
   var rows=new List<List<string>>();var row=new List<string>();var cell=new StringBuilder();int i=0;bool quotedCell=false;
   while(i<text.Length){char c=text[i];
    if(cell.Length==0&&!quotedCell&&c=='"'){
     int j=i+1;var q=new StringBuilder();bool closed=false;
     while(j<text.Length){if(text[j]=='"'){if(j+1<text.Length&&text[j+1]=='"'){q.Append('"');j+=2;continue;}closed=true;j++;break;}q.Append(text[j]);j++;}
     if(closed&&(j>=text.Length||text[j]=='\t'||text[j]=='\r'||text[j]=='\n')){cell.Append(q.ToString());quotedCell=true;i=j;continue;}
     // not a well-formed quoted cell: the quote is literal text (Excel writes unbalanced quotes unquoted)
     cell.Append(c);i++;continue;}
    if(c=='\t'){row.Add(cell.ToString());cell.Clear();quotedCell=false;i++;continue;}
    if(c=='\r'||c=='\n'){row.Add(cell.ToString());cell.Clear();quotedCell=false;rows.Add(row);row=new List<string>();
     i+=c=='\r'&&i+1<text.Length&&text[i+1]=='\n'?2:1;continue;}
    cell.Append(c);i++;}
   bool endedWithBreak=text.Length>0&&(text[text.Length-1]=='\n'||text[text.Length-1]=='\r');
   if(!endedWithBreak||cell.Length>0||row.Count>0||quotedCell){row.Add(cell.ToString());rows.Add(row);}
   if(rows.Count==0)rows.Add(new List<string>{""});
   int w=rows[0].Count;
   if(rows.Any(r=>r.Count!=w)){error="CLIPBOARD_RAGGED";return null;}
   if((long)w*rows.Count>MaxPasteCells){error="CLIPBOARD_TOO_LARGE";return null;}
   return rows;}
  public static string ToTsv(IList<IList<string>> m){var b=new StringBuilder();
   foreach(var r in m){for(int c=0;c<r.Count;c++){if(c>0)b.Append('\t');b.Append(Quote(r[c]??""));}b.Append("\r\n");}
   return b.ToString();}
  static string Quote(string s){if(s.IndexOf('\t')<0&&s.IndexOf('\r')<0&&s.IndexOf('\n')<0&&!(s.Length>0&&s[0]=='"'))return s;return "\""+s.Replace("\"","\"\"")+"\"";}

  // ---- visible-grid planning ----
  // Selection bounds over the VISIBLE grid (row index = display order of shown rows, column index = display position among visible columns).
  // Refused when empty or not one full rectangle (discontiguous / holes).
  public static bool Rectangle(IList<int[]> selected,out int r0,out int c0,out int r1,out int c1,out string error){r0=c0=r1=c1=-1;error=null;
   if(selected==null||selected.Count==0){error="SELECTION_EMPTY";return false;}
   r0=selected.Min(x=>x[0]);r1=selected.Max(x=>x[0]);c0=selected.Min(x=>x[1]);c1=selected.Max(x=>x[1]);
   var set=new HashSet<long>(selected.Select(x=>(long)x[0]*100000+x[1]));
   if(set.Count!=(long)(r1-r0+1)*(c1-c0+1)){error="SELECTION_NOT_RECTANGLE";return false;}
   return true;}
 }

 // Grid shape handed to the planner (visible rows/columns only). field[c] null = read-only column (CAD measure, result, status, identity).
 public sealed class FastGrid045 {
  public int Rows{get{return rowKeys.Count;}}public int Cols{get{return fields.Count;}}
  public List<string> rowKeys=new List<string>(),rowLabels=new List<string>(),fields=new List<string>(),captions=new List<string>();
  public List<bool> rowEditable=new List<bool>();
  public Func<int,int,string> text;     // current displayed (effective) text of a visible cell; used by fill-down
 }
 public sealed class FastTarget045 {public int row,col;public string rowKey,field,text;public object value;}
 // Result of a plan: targets (all validated) or a refusal with the first offending visible cell (-1 = not cell related) and the rejected text.
 public sealed class FastPlan045 {public List<FastTarget045> targets=new List<FastTarget045>();public string error,detail,badText;public int badRow=-1,badCol=-1;
  public bool Ok{get{return error==null;}}}

 public static class FastPlanner045 {
  // parse(field,text) -> typed value, or throws InvalidOperationException with the reason (product parser: RoomAdjustmentsUi037.ParseCell).
  static FastPlan045 Fail(FastPlan045 p,string error,int r,int c,string text,string detail){p.targets.Clear();p.error=error;p.badRow=r;p.badCol=c;p.badText=text;p.detail=detail;return p;}
  static FastPlan045 Check(FastGrid045 g,FastPlan045 p,int r,int c,string text,Func<string,string,object> parse){
   if(r<0||r>=g.Rows||c<0||c>=g.Cols)return Fail(p,"OUT_OF_BOUNDS",r,c,text,null);
   if(g.fields[c]==null)return Fail(p,"READ_ONLY_COLUMN",r,c,text,null);
   if(!g.rowEditable[r])return Fail(p,"READ_ONLY_ROW",r,c,text,null);
   object v;try{v=parse(g.fields[c],text);}catch(InvalidOperationException ex){return Fail(p,"INVALID_VALUE",r,c,text,ex.Message);}
   p.targets.Add(new FastTarget045{row=r,col=c,rowKey=g.rowKeys[r],field=g.fields[c],text=text,value=v});return null;}
  // Excel paste: clip 1x1 -> every selected cell; selection 1 cell -> clip size from there; equal size -> one to one; anything else refused.
  public static FastPlan045 Paste(FastGrid045 g,IList<int[]> selected,List<List<string>> clip,Func<string,string,object> parse){var p=new FastPlan045();
   int r0,c0,r1,c1;string e;if(!FastEntry045.Rectangle(selected,out r0,out c0,out r1,out c1,out e))return Fail(p,e,-1,-1,null,null);
   if(clip==null||clip.Count==0||clip[0].Count==0)return Fail(p,"CLIPBOARD_EMPTY",-1,-1,null,null);
   int ch=clip.Count,cw=clip[0].Count,sh=r1-r0+1,sw=c1-c0+1;
   if(ch==1&&cw==1){for(int r=r0;r<=r1;r++)for(int c=c0;c<=c1;c++){var f=Check(g,p,r,c,clip[0][0],parse);if(f!=null)return f;}return p;}
   if(sh==1&&sw==1){r1=r0+ch-1;c1=c0+cw-1;}
   else if(sh!=ch||sw!=cw)return Fail(p,"SIZE_MISMATCH",-1,-1,null,ch.ToString(CultureInfo.InvariantCulture)+"x"+cw.ToString(CultureInfo.InvariantCulture)+" vs "+
    sh.ToString(CultureInfo.InvariantCulture)+"x"+sw.ToString(CultureInfo.InvariantCulture));
   for(int r=r0;r<=r1;r++)for(int c=c0;c<=c1;c++){var f=Check(g,p,r,c,clip[r-r0][c-c0],parse);if(f!=null)return f;}
   return p;}
  // Fill down: each selected column copies the text of its TOP selected cell to the other selected cells of that column. One selected row = copy
  // from the visible row above (Excel Ctrl+D). Source cells must be readable; targets are validated like a paste.
  public static FastPlan045 FillDown(FastGrid045 g,IList<int[]> selected,Func<string,string,object> parse,Func<int,int,bool> sourcePending){var p=new FastPlan045();
   int r0,c0,r1,c1;string e;if(!FastEntry045.Rectangle(selected,out r0,out c0,out r1,out c1,out e))return Fail(p,e,-1,-1,null,null);
   int src=r0,first=r0+1;
   if(r0==r1){if(r0==0)return Fail(p,"FILL_NO_SOURCE",r0,c0,null,null);src=r0-1;first=r0;}
   for(int c=c0;c<=c1;c++){
    if(g.fields[c]==null)return Fail(p,"READ_ONLY_COLUMN",src,c,null,null);
    if(sourcePending!=null&&sourcePending(src,c))return Fail(p,"FILL_SOURCE_PENDING",src,c,null,null);
    var t=g.text==null?null:g.text(src,c);if(t==null)return Fail(p,"FILL_NO_SOURCE",src,c,null,null);
    for(int r=first;r<=r1;r++){var f=Check(g,p,r,c,t,parse);if(f!=null)return f;}}
   return p;}
  // Copy: displayed texts of the selected rectangle (any column).
  public static string Copy(FastGrid045 g,IList<int[]> selected,out string error){int r0,c0,r1,c1;
   if(!FastEntry045.Rectangle(selected,out r0,out c0,out r1,out c1,out error))return null;
   var m=new List<IList<string>>();for(int r=r0;r<=r1;r++){var l=new List<string>();for(int c=c0;c<=c1;c++)l.Add(g.text==null?"":g.text(r,c)??"");m.Add(l);}
   return FastEntry045.ToTsv(m);}
 }

 // Bounded undo/redo of whole snapshots, valid only inside one context (room, batch, base revision, ...). A new action clears redo; a context
 // change empties both stacks (stale history is refused, never applied to another room/scan/base).
 public sealed class FastHistory045<T> where T:class {
  readonly int max;readonly List<T> undo=new List<T>(),redo=new List<T>();string context;
  public FastHistory045(int max){this.max=Math.Max(1,max);}
  public int UndoCount{get{return undo.Count;}}public int RedoCount{get{return redo.Count;}}public string Context{get{return context;}}
  // true when the stacks belong to ctx; otherwise both are cleared and false is returned.
  public bool Bind(string ctx){if(string.Equals(context,ctx,StringComparison.Ordinal))return true;undo.Clear();redo.Clear();context=ctx;return false;}
  public void Clear(){undo.Clear();redo.Clear();}
  // State BEFORE a new accepted action.
  public void Record(string ctx,T before){if(before==null)throw new ArgumentNullException("before");Bind(ctx);undo.Add(before);if(undo.Count>max)undo.RemoveAt(0);redo.Clear();}
  // current = state now (goes to the other stack); returns the state to restore or null (nothing / other context).
  public T Undo(string ctx,T current){if(!Bind(ctx)||undo.Count==0)return null;var t=undo[undo.Count-1];undo.RemoveAt(undo.Count-1);redo.Add(current);return t;}
  public T Redo(string ctx,T current){if(!Bind(ctx)||redo.Count==0)return null;var t=redo[redo.Count-1];redo.RemoveAt(redo.Count-1);undo.Add(current);if(undo.Count>max)undo.RemoveAt(0);return t;}
  // Restore failed: put the popped state back (stacks exactly as before the call).
  public void UndoFailed(T popped){if(redo.Count>0)redo.RemoveAt(redo.Count-1);undo.Add(popped);}
  public void RedoFailed(T popped){if(undo.Count>0)undo.RemoveAt(undo.Count-1);redo.Add(popped);}
 }
}
