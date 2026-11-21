using System;using System.Drawing;using System.Linq;using System.Text;using System.Globalization;using System.Collections.Generic;using System.Windows.Forms;using IOE=System.InvalidOperationException;
#if ROOMCAD037 && BDGKL034
namespace Tvxd.Meps {
 // TVXD-MEPS-ROOMQTY-037 S3 project UI integration (Vietnamese text: /codepage:65001). Opened from RoomReport037 (owned modals of the bound command
 // RoomUi037.Run, same palette dispatch DÂY DẪN → 'Bảng khối lượng điện…'). No new command, menu, queue or palette.
 //  - RoomProjectDialog037 'DỰ ÁN / TẦNG / PHÒNG': project name, floors (storey / wire ceiling / mounting ceiling), rooms (unique CODE + free description,
 //    optional wire / mounting ceiling overrides) and device-height rules by type with scope CHUNG (project default) or PHÒNG (one room).
 //    Works on a deep copy (RoomProject037.Copy); every add/update/rule change is validated on a further copy and only then replaces the working copy.
 //    Result only on explicit Apply (Áp dụng); Cancel/Esc = no change. No engineering number is ever preset.
 //  - RoomDeviceDialog037 'CAO ĐỘ THIẾT BỊ': per actual device (native handle + authoritative scan type) explicit FIXED / MOUNTING_OFFSET override or
 //    DÙNG LẠI MẶC ĐỊNH (reset to inherit) for the open room; shows effective value + readable source CHUNG / PHÒNG / RIÊNG. Unknown type = never editable.
 //  - RoomProjectUi037: pure helpers (no CAD calls) used by RoomUi037 and the GPT harness: labels, profile application to the open room's rows, draft
 //    capture into a project document, export copy with readable names.
 // Heights: RoomProject037.Resolve is the only authority. Rows receive the VALID effective height as Bdgkl034 FIXED fields (snapshot, recomputed at every
 // Recalc); MISSING/INVALID = empty fields (INVALID also an explicit blocker). Effective values are never copied into rules or drafts.
 public static class RoomProjectUi037 {
  static readonly StringComparer O=StringComparer.Ordinal,OI=StringComparer.OrdinalIgnoreCase;
  // Prefix of every blocker produced by ApplyProfile (report shows them with origin 'Dự án', not 'Quét CAD').
  public const string BlockerPrefix="DỰ ÁN: ";
  public static string TypeLabel(string t){return RoomHeight037.TypeName(t).ToUpperInvariant();}
  public static string SourceLabel(string src){return src==RoomProject037.Chung?"CHUNG":src==RoomProject037.Phong?"PHÒNG":src==RoomProject037.Rieng?"RIÊNG":"—";}
  public static string ModeLabel(string m){return m==RoomProject037.Fixed?"CỐ ĐỊNH TỪ SÀN HOÀN THIỆN":m==RoomProject037.MountingOffset?"HẠ DƯỚI TRẦN LẮP THIẾT BỊ":"—";}
  public static string StatusLabel(string st){return st==RoomProject037.Valid?"HỢP LỆ":st==RoomProject037.Invalid?"KHÔNG HỢP LỆ":"CHƯA KHAI";}
  public static string Exact(double? v){return v.HasValue?Bdgkl034.N(v.Value).Replace('.',','):"";}

  // ---- document / model lookups ----
  // 045: the fast-entry data (presets, chosen defaults, row state) is deep-copied with the document, so room switch, recovery capture, export and
  // save never drop or share it.
  public static RoomProjectDocument037 CopyDocument(RoomProjectDocument037 d){if(d==null)return null;
   return new RoomProjectDocument037{version=d.version,model=RoomProject037.Copy(d.model),
    drafts=d.drafts==null?null:d.drafts.Select(x=>x==null?null:new RoomProjectDraft037{roomId=x.roomId,draftJson=x.draftJson}).ToList(),fast=FastEntry045.Copy(d.fast)};}
  public static RoomProjectData037 Model(RoomSession037 s){return s==null||s.projectDocument==null?null:s.projectDocument.model;}
  public static HierFloor037 FindFloor(RoomProjectData037 m,string floorId){
   if(m==null||m.hierarchy==null||m.hierarchy.floors==null||string.IsNullOrEmpty(floorId))return null;
   return m.hierarchy.floors.FirstOrDefault(f=>f!=null&&O.Equals(f.id,floorId));}
  public static HierRoom037 FindRoom(RoomProjectData037 m,string roomId,out HierFloor037 floor){floor=null;
   if(m==null||m.hierarchy==null||m.hierarchy.floors==null||string.IsNullOrEmpty(roomId))return null;
   foreach(var f in m.hierarchy.floors){if(f==null||f.rooms==null)continue;var r=f.rooms.FirstOrDefault(x=>x!=null&&O.Equals(x.id,roomId));if(r!=null){floor=f;return r;}}
   return null;}
  // Readable room label "code — description" (never the stable RM_ id).
  public static string RoomLabel(HierRoom037 r){if(r==null)return "(phòng không có trong dự án)";return string.IsNullOrEmpty(r.code)?r.name:r.code+" — "+r.name;}
  public static string RoomLabel(RoomSession037 s){if(s==null)return "";if(!s.projectMode)return s.roomId;HierFloor037 f;
   var r=FindRoom(Model(s),s.roomId,out f);return r!=null?RoomLabel(r):s.roomId==null?"(chưa chọn phòng)":"(phòng không có trong dự án)";}
  public static string FloorLabel(RoomSession037 s,string floorId){if(s==null||!s.projectMode)return floorId;var f=FindFloor(Model(s),floorId);
   return f!=null?f.name:floorId==null?"—":"(tầng không có trong dự án)";}
  // Readable active scope of the report.
  public static string ScopeText(RoomSession037 s){var m=Model(s);
   if(s.projectError!=null)return "DỰ ÁN: KHÔNG ĐỌC ĐƯỢC dữ liệu dự án của bản vẽ ("+s.projectError+"). MEPS không ghi đè; 'Dữ liệu cũ' vẫn dùng được.";
   if(m==null)return "CHƯA CÓ DỰ ÁN trong bản vẽ: bấm 'DỰ ÁN / TẦNG / PHÒNG' để tạo dự án, tầng, phòng và cao độ (chỉ ghi vào bản vẽ khi bấm 'Lưu vào bản vẽ').";
   string rid=s.roomId??s.selectedRoomId;HierFloor037 f;var r=FindRoom(m,rid,out f);
   var b=new StringBuilder("DỰ ÁN ").Append(m.hierarchy.name);
   if(r!=null)b.Append(" · TẦNG ").Append(f.name).Append(" · PHÒNG ").Append(RoomLabel(r));else b.Append(" · CHƯA CHỌN PHÒNG");
   if(s.batch==null)b.Append(r!=null?" (chưa quét: bấm 'Chọn vùng CAD')":" (bấm 'Chọn vùng CAD' để chọn phòng của dự án và quét)");
   if(s.projectDirty||s.dirty)b.Append(" · CÓ THAY ĐỔI CHƯA LƯU");
   return b.ToString();}
  // Wiring ceiling of a room = room override ?? floor value (null = not declared). Never the mounting ceiling or storey height.
  public static double? WireCeiling(RoomProjectData037 m,string roomId){HierFloor037 f;var r=FindRoom(m,roomId,out f);
   if(r==null)return null;return r.wireCeilingOverride.HasValue?r.wireCeilingOverride:f.wireCeilingHeight;}

  // ---- devices of the open room (authoritative scan types; nothing inferred) ----
  public static List<RoomDevice037> Devices(RoomSession037 s){var l=new List<RoomDevice037>();
   if(s==null||s.batch==null||s.batch.source==null||s.batch.source.devices==null)return l;var seen=new HashSet<string>(O);
   foreach(var d in s.batch.source.devices)if(d!=null&&!string.IsNullOrEmpty(d.id)&&seen.Add(d.id))l.Add(d);
   return l.OrderBy(d=>d.name??"",O).ThenBy(d=>d.id,O).ToList();}
  public static string DeviceLabel(RoomDevice037 d){return d==null?"?":(string.IsNullOrWhiteSpace(d.name)?"(không tên)":d.name)+" ["+d.id+"]";}
  static RoomHeightRule037 OwnRule(RoomProjectData037 m,string deviceId){if(m==null||m.devices==null)return null;
   var e=m.devices.FirstOrDefault(x=>x!=null&&OI.Equals(x.deviceId,deviceId));return e==null?null:e.rule;}
  public static RoomHeightRule037 RoomRule(RoomProjectData037 m,string roomId,string type){if(m==null||m.rooms==null)return null;
   var e=m.rooms.FirstOrDefault(x=>x!=null&&O.Equals(x.roomId,roomId));return e==null||e.rules==null?null:e.rules.FirstOrDefault(x=>x!=null&&O.Equals(x.deviceType,type));}
  public static RoomHeightRule037 DefaultRule(RoomProjectData037 m,string type){if(m==null||m.defaults==null)return null;
   return m.defaults.FirstOrDefault(x=>x!=null&&O.Equals(x.deviceType,type));}
  // Unique devices of this type in the scanned room that follow a change of the rule at this level:
  // roomLevel=false (CHUNG): no own rule and no room rule of the type; roomLevel=true (PHÒNG): no own rule. Only the currently scanned room is counted.
  public static int Inheriting(RoomProjectData037 m,string roomId,IEnumerable<RoomDevice037> devs,string type,bool roomLevel){if(m==null||devs==null)return 0;
   if(!roomLevel&&RoomRule(m,roomId,type)!=null)return 0;
   return devs.Count(d=>d!=null&&O.Equals(d.type,type)&&OwnRule(m,d.id)==null);}

  // ---- profile → rows of the open room (called by RoomUi037.Recalc in project mode). Returns explicit blockers. ----
  public static List<string> ApplyProfile(RoomSession037 s){var l=new List<string>();if(s==null||s.batch==null)return l;var m=Model(s);
   HierFloor037 f;var room=FindRoom(m,s.roomId,out f);
   if(room==null)l.Add(BlockerPrefix+"Phòng đang mở không có trong dữ liệu dự án; chọn lại phòng bằng 'Chọn vùng CAD'. Không lấy tầng/cao độ.");
   double? ceil=room==null?null:WireCeiling(m,s.roomId);
   var cache=new Dictionary<string,double?>(O);var reported=new HashSet<string>(O);
   foreach(var r in s.batch.rows){var i=r.input;if(i==null)continue;
    i.floorId=room==null?null:f.id;i.ceilingHeight=ceil;
    var hs=room==null?null:Height(m,s.roomId,r.startId,r.startType,r.startName,cache,reported,l);
    var he=room==null?null:Height(m,s.roomId,r.endId,r.endType,r.endName,cache,reported,l);
    i.startHeightMode=hs.HasValue?Bdgkl034.Fixed:null;i.startFixedHeight=hs;i.startCeilingOffset=null;
    i.endHeightMode=he.HasValue?Bdgkl034.Fixed:null;i.endFixedHeight=he;i.endCeilingOffset=null;}
   return l;}
  static double? Height(RoomProjectData037 m,string roomId,string id,string type,string name,Dictionary<string,double?> cache,HashSet<string> reported,List<string> l){
   string k=(id??"")+"\u001f"+(type??"");double? h;if(cache.TryGetValue(k,out h))return h;
   var e=RoomProject037.Resolve(m,roomId,id,type);h=e.status==RoomProject037.Valid?e.height:null;cache[k]=h;
   if(e.status==RoomProject037.Invalid&&reported.Add(k))l.Add(BlockerPrefix+"Cao độ "+TypeLabel(type)+" "+(string.IsNullOrWhiteSpace(name)?"":name+" ")+"["+id+"] KHÔNG HỢP LỆ (nguồn "+SourceLabel(e.source)+"): "+e.message);
   return h;}
  public static RoomEffectiveHeight037 Effective(RoomProjectData037 m,string roomId,RoomDevice037 d){return RoomProject037.Resolve(m,roomId,d==null?null:d.id,d==null?null:d.type);}

  // ---- drafts (stable room ID; declarations only) ----
  public static void PutDraft(RoomProjectDocument037 doc,RoomDraft037 draft){if(doc==null||draft==null)throw new IOE("Thiếu tài liệu dự án hoặc bản nháp.");
   var j=RoomStore037.Encode(draft);if(doc.drafts==null)throw new IOE("Danh sách bản nháp của dự án rỗng (null).");
   int i=doc.drafts.FindIndex(x=>x!=null&&O.Equals(x.roomId,draft.roomId));var n=new RoomProjectDraft037{roomId=draft.roomId,draftJson=j};
   if(i>=0)doc.drafts[i]=n;else doc.drafts.Add(n);}
  public static RoomDraft037 DraftOf(RoomProjectDocument037 doc,string roomId,out string error){error=null;
   if(doc==null||doc.drafts==null||roomId==null)return null;var x=doc.drafts.FirstOrDefault(d=>d!=null&&O.Equals(d.roomId,roomId));if(x==null)return null;
   try{var d=RoomStore037.Decode(x.draftJson);if(!O.Equals(d.roomId,roomId))throw new IOE("bản nháp ghi phòng khác");return d;}catch(IOE ex){error=ex.Message;return null;}}

  // ---- names ----
  // Project/floor/room texts must satisfy BOTH the hierarchy rule and the BDGKL text rule (<= 100 chars, no leading/trailing space) so that the
  // readable names can be written to the Excel copy. null = acceptable.
  public static string NameProblem(string text,string what){var p=Bdgkl034.TextProblem(text);if(p!=null)return what+" "+p+".";
   var q=RoomHierarchy037.TextProblem(text,what);return q==null?null:what+" không hợp lệ ("+q+").";}
  // S4 coded room of a project model: the code must be unique in the whole drawing; the description may repeat on the same floor (distinct codes).
  // Delegates to RoomProject037.AddRoom (single coded-room policy: model + code validated first, then RoomHierarchy037.AddCodedRoom); nothing
  // changes on refusal. Never suffixes a name.
  public static HierRoom037 AddCodedRoom(RoomProjectData037 d,string floorId,string code,string name,Func<Guid> ids){
   return RoomProject037.AddRoom(d,floorId,code,name,ids);}
  // Ceiling/storey value: blank = null (not declared); otherwise finite > 0 (the hierarchy refuses 0 for these references).
  public static double? Positive(string text,string what){var v=BdgklUi034.ParseDecimal(text,what);if(v.HasValue&&!(v.Value>0))throw new IOE(what+" phải lớn hơn 0 (hoặc để trống = chưa khai).");return v;}

  // ---- Excel copy (current room only) ----
  // Separate batch over a copy of the scan input with readable room label (roomId) and floor NAME (floorId); same declarations and effective heights.
  // The session batch, its identities and the scan are not changed. Must reproduce the session results exactly, else refused.
  public static RoomCompletionBatch037 ExportCopy(RoomSession037 s){if(s==null||s.batch==null||s.batch.source==null)throw new IOE("Chưa có phòng đang mở.");
   var m=Model(s);HierFloor037 f;var room=FindRoom(m,s.roomId,out f);if(room==null)throw new IOE("Phòng đang mở không có trong dự án; chưa xuất.");
   var fp=Bdgkl034.TextProblem(f.name);if(fp!=null)throw new IOE("Tên tầng \""+f.name+"\" không ghi được vào Excel ("+fp+"); sửa tên tầng ở 'DỰ ÁN / TẦNG / PHÒNG'. Chưa xuất.");
   string label=RoomLabel(room);if(Bdgkl034.TextProblem(label)!=null)label=room.code;
   var lp=Bdgkl034.TextProblem(label);if(lp!=null)throw new IOE("Mã phòng \""+room.code+"\" không ghi được vào Excel ("+lp+"); sửa mã phòng. Chưa xuất.");
   var src=s.batch.source;
   var inp=new RoomInput037{roomId=label,units=src.units,devices=new List<RoomDevice037>(src.devices),segments=new List<RoomSegment037>(src.segments),nodeDevices=new List<RoomNodeDevice037>(src.nodeDevices)};
   var b=RoomCompletion037.Create(inp);b.conduitMode=s.batch.conduitMode;b.independentAssembliesConfirmed=s.batch.independentAssembliesConfirmed;
   b.externalBlockers=new List<string>(s.batch.externalBlockers);
   var by=s.batch.rows.Where(r=>r.rowKey!=null).ToDictionary(r=>r.rowKey,O);
   foreach(var r in b.rows){RoomCompletionRow037 o;if(r.input==null||r.rowKey==null||!by.TryGetValue(r.rowKey,out o)||o.input==null)continue;var i=r.input;var q=o.input;
    i.floorId=f.name;i.circuitId=q.circuitId;i.routingMode=q.routingMode;i.ceilingHeight=q.ceilingHeight;i.tailCount=q.tailCount;i.tailEachLength=q.tailEachLength;i.factor=q.factor;
    i.assemblyCode=q.assemblyCode;i.conduitCode=q.conduitCode;i.startHeightMode=q.startHeightMode;i.startFixedHeight=q.startFixedHeight;i.startCeilingOffset=q.startCeilingOffset;
    i.endHeightMode=q.endHeightMode;i.endFixedHeight=q.endFixedHeight;i.endCeilingOffset=q.endCeilingOffset;}
   // S5: exact copy of the BỎ DÒNG / BỔ SUNG declarations and the scan signature. This copy's source.roomId is the readable label, so ONLY here the copied
   // supplements of the open room (stable ID) carry that label as roomId; the session, its adjustments and every stable ID stay untouched.
   b.sourceSignature=s.batch.sourceSignature;b.adjustments=CopyForLabel(s.batch.adjustments,src.roomId,label);
   RoomCompletion037.Calculate(b);
   bool same=b.status==s.batch.status&&b.rows.Count==s.batch.rows.Count&&b.excluded==s.batch.excluded&&b.rows.All(r=>{RoomCompletionRow037 o;
    if(r.rowKey==null||!by.TryGetValue(r.rowKey,out o)||r.status!=o.status||r.excluded!=o.excluded)return false;
    if(r.excluded)return r.result==null&&o.result==null&&string.Equals(r.exclusionReason,o.exclusionReason,StringComparison.Ordinal);
    return r.result!=null&&o.result!=null&&Nullable.Equals(r.result.routeM,o.result.routeM)&&Nullable.Equals(r.result.routeAfterFactorM,o.result.routeAfterFactorM)&&
     Nullable.Equals(r.result.conduitHiddenAfterFactorM,o.result.conduitHiddenAfterFactorM)&&Nullable.Equals(r.result.conduitExposedAfterFactorM,o.result.conduitExposedAfterFactorM);})&&
    SameManual(b.manualTotals,s.batch.manualTotals)&&SameCombined(b.combinedTotals,s.batch.combinedTotals)&&
    b.exclusionStates.Select(x=>x.rowKey+"\u001f"+x.status).SequenceEqual(s.batch.exclusionStates.Select(x=>x.rowKey+"\u001f"+x.status),O)&&
    b.supplementStates.Select(x=>x.id+"\u001f"+x.status).SequenceEqual(s.batch.supplementStates.Select(x=>x.id+"\u001f"+x.status),O);
   if(!same)throw new IOE("Bản sao xuất Excel (tên tầng/phòng đọc được) không cho cùng kết quả với bảng đang mở; chưa xuất. Báo lỗi này cho người phát triển.");
   return b;}
  // Deep copy for the export copy only: supplements whose roomId is the stable ID become the label (null stays null).
  static RoomAdjustmentsData037 CopyForLabel(RoomAdjustmentsData037 d,string stableId,string label){var c=RoomAdjustments037.Copy(d);
   if(c!=null&&c.supplements!=null)foreach(var x in c.supplements)if(x!=null&&O.Equals(x.roomId,stableId))x.roomId=label;
   return c;}
  static bool SameManual(List<RoomManualTotal037> a,List<RoomManualTotal037> b){if(a==null||b==null)return a==b;if(a.Count!=b.Count)return false;
   for(int k=0;k<a.Count;k++){var x=a[k];var y=b[k];if(!O.Equals(x.source,y.source)||!O.Equals(x.kind,y.kind)||!O.Equals(x.code,y.code)||!O.Equals(x.unit,y.unit)||x.items!=y.items||!x.metres.Equals(y.metres))return false;}
   return true;}
  static bool SameCombined(List<RoomCombinedTotal037> a,List<RoomCombinedTotal037> b){if(a==null||b==null)return a==b;if(a.Count!=b.Count)return false;
   for(int k=0;k<a.Count;k++){var x=a[k];var y=b[k];if(!O.Equals(x.kind,y.kind)||!O.Equals(x.code,y.code)||!O.Equals(x.unit,y.unit)||x.cadRows!=y.cadRows||x.manualItems!=y.manualItems||
     !x.cadM.Equals(y.cadM)||!x.manualM.Equals(y.manualM)||!x.combinedM.Equals(y.combinedM))return false;}
   return true;}
 }

 // ---- DỰ ÁN / TẦNG / PHÒNG ----
 public sealed class RoomProjectDialog037:Form {
  public readonly TextBox ProjectNameBox=new TextBox(),FloorNameBox=new TextBox(),StoreyBox=new TextBox(),FloorWireBox=new TextBox(),FloorMountBox=new TextBox(),
   RoomCodeBox=new TextBox(),RoomNameBox=new TextBox(),RoomWireBox=new TextBox(),RoomMountBox=new TextBox(),InfoBox=new TextBox(),StatusBox=new TextBox();
  public readonly TreeView ProjectTree=new TreeView();
  public readonly Button AddFloorBtn=new Button(),UpdateFloorBtn=new Button(),AddRoomBtn=new Button(),UpdateRoomBtn=new Button(),RevertBtn=new Button(),OkBtn=new Button(),CancelBtn=new Button();
  public readonly ComboBox ScopeBox=new ComboBox();
  public readonly ComboBox[] RuleModes;public readonly TextBox[] RuleValues;public readonly Button[] RuleResets;public readonly Label[] RuleNotes;
  // Working deep copy (null until the project is created). Result = copy of the opened document (drafts kept) with this model; only set by Apply.
  public RoomProjectData037 Model;public RoomProjectDocument037 Result;public string SelectedRoomId;
  readonly RoomProjectDocument037 original;readonly string drawingId;readonly Func<Guid> ids;readonly List<RoomDevice037> scanned;readonly string scannedRoomId;
  readonly float dpi;readonly Font face,bold;readonly ToolTip tip=new ToolTip();bool loading;string floorSnap="",roomSnap="",ruleScope;string[] ruleSnap;
  static readonly string[] Types=RoomProject037.Types.ToArray();
  int S(int px){return Math.Max(1,(int)Math.Round(px*dpi));}
  // current: the session document (null = no project yet). scannedDevices/scannedRoomId: the room currently scanned (affected counts only).
  public RoomProjectDialog037(RoomProjectDocument037 current,string drawingId,Func<Guid> ids,IList<RoomDevice037> scannedDevices,string scannedRoomId,string selectRoomId,string legacyInfo,float dpi){
   if(ids==null)throw new ArgumentNullException("ids");original=current;this.drawingId=drawingId;this.ids=ids;this.scannedRoomId=scannedRoomId;
   scanned=scannedDevices==null?new List<RoomDevice037>():scannedDevices.Where(d=>d!=null).ToList();this.dpi=BdgklLook034.Dpi(dpi);
   Model=current==null?null:RoomProject037.Copy(current.model);int n=Types.Length;
   RuleModes=new ComboBox[n];RuleValues=new TextBox[n];RuleResets=new Button[n];RuleNotes=new Label[n];ruleSnap=new string[n];
   Text="MEPS — Dự án / tầng / phòng";Name="room037_project_dialog";FormBorderStyle=FormBorderStyle.Sizable;MinimizeBox=false;MaximizeBox=true;ShowInTaskbar=false;
   StartPosition=FormStartPosition.CenterParent;AutoScaleMode=AutoScaleMode.None;face=RoomLook037.Face(this.dpi,false);bold=RoomLook037.Face(this.dpi,true);Font=face;
   BackColor=BdgklLook034.Back;ForeColor=BdgklLook034.Ink;ClientSize=new Size(S(1100),S(760));MinimumSize=new Size(S(900),S(560));
   var root=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=2,RowCount=4,Padding=new Padding(S(8)),BackColor=BdgklLook034.Back};
   root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,S(300)));root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
   foreach(int h in new[]{S(40),-100,S(52),S(44)})root.RowStyles.Add(h<0?new RowStyle(SizeType.Percent,-h):new RowStyle(SizeType.Absolute,h));
   Disposed+=(s,e)=>tip.Dispose();
   // header: project name (box follows the window width; the secondary hint wraps)
   var head=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=3,RowCount=1,Margin=new Padding(0),BackColor=BdgklLook034.Back};
   head.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));head.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,45));head.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,55));
   head.RowStyles.Add(new RowStyle(SizeType.Percent,100));
   head.Controls.Add(new Label{Text="TÊN DỰ ÁN",AutoSize=true,Anchor=AnchorStyles.Left,Font=bold,ForeColor=BdgklLook034.Ink,Margin=new Padding(0,0,S(8),0)},0,0);
   ProjectNameBox.Name="room037_project_name";ProjectNameBox.AccessibleName="Tên dự án";ProjectNameBox.Anchor=AnchorStyles.Left|AnchorStyles.Right;ProjectNameBox.BorderStyle=BorderStyle.FixedSingle;
   ProjectNameBox.MaxLength=Bdgkl034.MaxText;ProjectNameBox.Margin=new Padding(0,0,S(8),0);head.Controls.Add(ProjectNameBox,1,0);
   head.Controls.Add(new Label{Text="Một bản vẽ chứa mọi tầng; mã phòng duy nhất trong bản vẽ.",Dock=DockStyle.Fill,ForeColor=BdgklLook034.Muted,TextAlign=ContentAlignment.MiddleLeft,Margin=new Padding(0)},2,0);
   root.Controls.Add(head,0,0);root.SetColumnSpan(head,2);
   // left: tree
   var left=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=1,RowCount=3,Margin=new Padding(0,0,S(8),0),BackColor=BdgklLook034.Paper};
   left.RowStyles.Add(new RowStyle(SizeType.Absolute,S(30)));left.RowStyles.Add(new RowStyle(SizeType.Percent,100));left.RowStyles.Add(new RowStyle(SizeType.Absolute,S(150)));
   left.Controls.Add(new Label{Text="PHÂN VÙNG DỰ ÁN",Dock=DockStyle.Fill,Font=bold,ForeColor=BdgklLook034.Teal,TextAlign=ContentAlignment.MiddleLeft,Padding=new Padding(S(6),0,0,0)},0,0);
   ProjectTree.Name="room037_project_tree";ProjectTree.AccessibleName="Cây dự án: tầng và phòng";ProjectTree.Dock=DockStyle.Fill;ProjectTree.HideSelection=false;
   ProjectTree.BorderStyle=BorderStyle.None;ProjectTree.BackColor=BdgklLook034.Paper;ProjectTree.ForeColor=BdgklLook034.Ink;ProjectTree.FullRowSelect=true;ProjectTree.ShowLines=true;
   ProjectTree.ItemHeight=S(24);left.Controls.Add(ProjectTree,0,1);
   var note=new Label{Dock=DockStyle.Fill,ForeColor=BdgklLook034.Muted,Padding=new Padding(S(6),S(4),S(4),0),
    Text="Mỗi phòng có MÃ riêng (không trùng trong bản vẽ); mã phân biệt các phòng.\r\nMô tả có thể trùng, kể cả trên cùng một tầng (khác mã); MEPS không tự thêm ký tự vào mô tả."};
   left.Controls.Add(note,0,2);root.Controls.Add(left,0,1);
   // right: editor = vertical stack of section blocks. Minimum content width; below it the panel scrolls horizontally (and vertically), so no
   // control is clipped or unreachable at the minimum window size.
   int minContent=S(720);
   var scroll=new Panel{Dock=DockStyle.Fill,AutoScroll=true,AutoScrollMinSize=new Size(minContent,0),BackColor=BdgklLook034.Back,Margin=new Padding(0)};
   var g=new TableLayoutPanel{ColumnCount=1,AutoSize=true,AutoSizeMode=System.Windows.Forms.AutoSizeMode.GrowAndShrink,Dock=DockStyle.Top,MinimumSize=new Size(minContent,0),
    Margin=new Padding(0),BackColor=BdgklLook034.Back};
   g.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
   int at=0;
   Action<Control,int> block=(c,h)=>{g.RowStyles.Add(new RowStyle(SizeType.Absolute,h));c.Dock=DockStyle.Fill;c.Margin=new Padding(0);g.Controls.Add(c,0,at);at++;};
   // floor / room identity: label (own width) | input | readable hint (wraps). Name/code inputs span input + hint (full width).
   var idg=new TableLayoutPanel{ColumnCount=3,BackColor=BdgklLook034.Back};int ir=0,ih=0;
   idg.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));idg.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,S(130)));idg.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
   Action<int> idRow=h=>{idg.RowStyles.Add(new RowStyle(SizeType.Absolute,h));ih+=h;};
   Action<string> section=t=>{idRow(S(34));
    var l=new Label{Text=t,Dock=DockStyle.Fill,Font=bold,ForeColor=BdgklLook034.Teal,TextAlign=ContentAlignment.BottomLeft,Margin=new Padding(0,0,0,S(2))};idg.Controls.Add(l,0,ir);idg.SetColumnSpan(l,3);ir++;};
   Action<string,TextBox,string,string,bool> field=(cap,box,name,hint,wide)=>{idRow(wide?S(32):S(40));
    idg.Controls.Add(new Label{Text=cap,AutoSize=true,Anchor=AnchorStyles.Left,ForeColor=BdgklLook034.Ink,Margin=new Padding(0,0,S(12),0)},0,ir);
    box.Name=name;box.AccessibleName=cap;box.Anchor=AnchorStyles.Left|AnchorStyles.Right;box.BorderStyle=BorderStyle.FixedSingle;box.MaxLength=Bdgkl034.MaxText;box.Margin=new Padding(0,0,wide?0:S(8),0);
    if(!wide)box.TextAlign=HorizontalAlignment.Right;box.TextChanged+=(s,e)=>Changed();idg.Controls.Add(box,1,ir);
    if(wide)idg.SetColumnSpan(box,2);
    else idg.Controls.Add(new Label{Text=hint??"",Dock=DockStyle.Fill,ForeColor=BdgklLook034.Muted,TextAlign=ContentAlignment.MiddleLeft,Margin=new Padding(0)},2,ir);
    ir++;};
   // short visible text; AccessibleName/tooltip keep the full previous wording
   Action<Button,string,string,string,Button,string,string,string> buttons=(b1,t1,a1,n1,b2,t2,a2,n2)=>{idRow(S(40));
    var p=new FlowLayoutPanel{Dock=DockStyle.Fill,WrapContents=false,Margin=new Padding(0),BackColor=BdgklLook034.Back};
    foreach(var x in new[]{Tuple.Create(b1,t1,a1,n1),Tuple.Create(b2,t2,a2,n2)}){x.Item1.Text=x.Item2;x.Item1.Name=x.Item4;x.Item1.AccessibleName=x.Item3;tip.SetToolTip(x.Item1,x.Item3);
     BdgklLook034.Style(x.Item1,false,face,S(110),S(28));x.Item1.Margin=new Padding(0,S(6),S(8),0);p.Controls.Add(x.Item1);}
    idg.Controls.Add(p,0,ir);idg.SetColumnSpan(p,3);ir++;};
   section("TẦNG ĐANG CHỌN");
   field("TÊN TẦNG",FloorNameBox,"room037_floor_name",null,true);
   field("CHIỀU CAO TẦNG (m)",StoreyBox,"room037_floor_storey","sàn tầng này → sàn tầng trên; chỉ lưu thông tin, không cộng vào chiều dài",false);
   field("CAO ĐỘ TRẦN ĐI DÂY (m)",FloorWireBox,"room037_floor_wire","từ sàn hoàn thiện đến trần đi dây = H sàn → trần của mọi tuyến",false);
   field("CAO ĐỘ TRẦN LẮP THIẾT BỊ (m)",FloorMountBox,"room037_floor_mount","mốc cho cách khai HẠ DƯỚI TRẦN LẮP THIẾT BỊ; KHÁC trần đi dây",false);
   buttons(AddFloorBtn,"THÊM TẦNG","THÊM TẦNG MỚI","room037_floor_add",UpdateFloorBtn,"CẬP NHẬT TẦNG","CẬP NHẬT TẦNG ĐANG CHỌN","room037_floor_update");
   section("PHÒNG ĐANG CHỌN");
   field("MÃ PHÒNG",RoomCodeBox,"room037_room_code",null,true);
   field("MÔ TẢ PHÒNG",RoomNameBox,"room037_room_name",null,true);
   field("TRẦN ĐI DÂY RIÊNG CỦA PHÒNG (m)",RoomWireBox,"room037_room_wire","để trống = theo trần đi dây của tầng",false);
   field("TRẦN LẮP THIẾT BỊ RIÊNG CỦA PHÒNG (m)",RoomMountBox,"room037_room_mount","để trống = theo trần lắp thiết bị của tầng",false);
   buttons(AddRoomBtn,"THÊM PHÒNG","THÊM PHÒNG VÀO TẦNG ĐANG CHỌN","room037_room_add",UpdateRoomBtn,"CẬP NHẬT PHÒNG","CẬP NHẬT PHÒNG ĐANG CHỌN","room037_room_update");
   idg.RowCount=ir;block(idg,ih);
   // height rules by type: explicit widths for type / value / reset (reset column = full button text); mode and status columns share the rest
   var rt=new TableLayoutPanel{ColumnCount=5,BackColor=BdgklLook034.Back};int rr=0,rh=0;
   foreach(var cs in new[]{new ColumnStyle(SizeType.Absolute,S(96)),new ColumnStyle(SizeType.Percent,55),new ColumnStyle(SizeType.Absolute,S(90)),new ColumnStyle(SizeType.Percent,45),
    new ColumnStyle(SizeType.AutoSize)})rt.ColumnStyles.Add(cs);
   Action<int> ruleRow=h=>{rt.RowStyles.Add(new RowStyle(SizeType.Absolute,h));rh+=h;};
   ruleRow(S(34));
   var rs=new Label{Text="CAO ĐỘ THIẾT BỊ THEO LOẠI",Dock=DockStyle.Fill,Font=bold,ForeColor=BdgklLook034.Teal,TextAlign=ContentAlignment.BottomLeft,Margin=new Padding(0,0,0,S(2))};
   rt.Controls.Add(rs,0,rr);rt.SetColumnSpan(rs,5);rr++;
   ruleRow(S(34));
   rt.Controls.Add(new Label{Text="PHẠM VI",Dock=DockStyle.Fill,ForeColor=BdgklLook034.Ink,TextAlign=ContentAlignment.MiddleLeft,Margin=new Padding(0)},0,rr);
   ScopeBox.Name="room037_rule_scope";ScopeBox.AccessibleName="Phạm vi khai cao độ: CHUNG hoặc PHÒNG";ScopeBox.DropDownStyle=ComboBoxStyle.DropDownList;ScopeBox.Dock=DockStyle.Fill;
   ScopeBox.Margin=new Padding(0,S(4),S(6),S(4));rt.Controls.Add(ScopeBox,1,rr);rt.SetColumnSpan(ScopeBox,3);rr++;
   ruleRow(S(30));
   string[] hd={"LOẠI","CÁCH KHAI","GIÁ TRỊ (m)","ĐANG ÁP DỤNG",""};
   for(int c=0;c<hd.Length;c++)rt.Controls.Add(new Label{Text=hd[c],Dock=DockStyle.Fill,Font=bold,ForeColor=BdgklLook034.Ink,BackColor=BdgklLook034.Paper,
    TextAlign=c==2?ContentAlignment.MiddleRight:ContentAlignment.MiddleLeft,Margin=new Padding(0,0,c==4?0:S(2),0)},c,rr);
   rr++;
   for(int i=0;i<n;i++){string t=Types[i];ruleRow(S(34));
    rt.Controls.Add(new Label{Text=RoomProjectUi037.TypeLabel(t),Dock=DockStyle.Fill,TextAlign=ContentAlignment.MiddleLeft,Margin=new Padding(0)},0,rr);
    var mb=RuleModes[i]=new ComboBox{Name="room037_rule_mode_"+t,AccessibleName="Cách khai cao độ "+RoomProjectUi037.TypeLabel(t),Dock=DockStyle.Fill,DropDownStyle=ComboBoxStyle.DropDownList,
     DropDownWidth=S(260),Margin=new Padding(0,S(4),S(6),S(4))};
    mb.Items.Add(new RouteDialog034.Choice(null,"(chưa khai)"));mb.Items.Add(new RouteDialog034.Choice(RoomProject037.Fixed,"CỐ ĐỊNH TỪ SÀN HOÀN THIỆN"));
    mb.Items.Add(new RouteDialog034.Choice(RoomProject037.MountingOffset,"HẠ DƯỚI TRẦN LẮP THIẾT BỊ"));mb.SelectedIndex=0;mb.SelectedIndexChanged+=(s,e)=>Changed();rt.Controls.Add(mb,1,rr);
    var vb=RuleValues[i]=new TextBox{Name="room037_rule_value_"+t,AccessibleName="Giá trị cao độ "+RoomProjectUi037.TypeLabel(t)+" (m)",Dock=DockStyle.Fill,BorderStyle=BorderStyle.FixedSingle,
     MaxLength=Bdgkl034.MaxText,TextAlign=HorizontalAlignment.Right,Margin=new Padding(0,S(4),S(6),S(4))};vb.TextChanged+=(s,e)=>Changed();rt.Controls.Add(vb,2,rr);
    // status text wraps (two lines fit the row) instead of being cut
    var nl=RuleNotes[i]=new Label{Name="room037_rule_note_"+t,Dock=DockStyle.Fill,ForeColor=BdgklLook034.Muted,TextAlign=ContentAlignment.MiddleLeft,Margin=new Padding(0)};
    rt.Controls.Add(nl,3,rr);
    var rb=RuleResets[i]=new Button{Name="room037_rule_reset_"+t};BdgklLook034.Style(rb,false,face,S(120),S(26));rb.Margin=new Padding(S(6),S(3),0,S(3));string tt=t;rb.Click+=(s,e)=>ResetRule(tt);
    rt.Controls.Add(rb,4,rr);ruleSnap[i]=Snap(mb,vb);rr++;}
   rt.RowCount=rr;block(rt,rh);
   var rn=new Label{ForeColor=BdgklLook034.Muted,TextAlign=ContentAlignment.MiddleLeft,
    Text="Ô trống = chưa khai (không phải 0); 0 là giá trị hợp lệ. Ưu tiên: RIÊNG thiết bị > PHÒNG > CHUNG. Đổi CHUNG/PHÒNG chỉ đổi thiết bị đang thừa hưởng; khai riêng giữ nguyên.\r\n"+
     "Khai riêng từng thiết bị: nút 'CAO ĐỘ THIẾT BỊ' ở bảng phòng (sau khi quét)."};
   block(rn,S(64));
   BdgklLook034.ReadOnlyBox(InfoBox);InfoBox.Name="room037_project_info";InfoBox.AccessibleName="Giải thích (chỉ đọc)";
   block(InfoBox,S(110));InfoBox.Margin=new Padding(0,S(4),0,0);
   g.RowCount=at;scroll.Controls.Add(g);root.Controls.Add(scroll,1,1);
   BdgklLook034.ReadOnlyBox(StatusBox);StatusBox.Name="room037_project_status";StatusBox.AccessibleName="Thông báo";StatusBox.BackColor=BdgklLook034.Back;StatusBox.BorderStyle=BorderStyle.None;
   root.Controls.Add(StatusBox,0,2);root.SetColumnSpan(StatusBox,2);
   var bar=new FlowLayoutPanel{FlowDirection=System.Windows.Forms.FlowDirection.RightToLeft,Dock=DockStyle.Fill,WrapContents=false,Margin=new Padding(0),BackColor=BdgklLook034.Back};
   OkBtn.Text="ÁP DỤNG";OkBtn.Name="room037_project_ok";CancelBtn.Text="HỦY";CancelBtn.Name="room037_project_cancel";CancelBtn.DialogResult=DialogResult.Cancel;
   RevertBtn.Text="HOÀN LẠI";RevertBtn.Name="room037_project_revert";
   OkBtn.AccessibleName="Áp dụng";CancelBtn.AccessibleName="Hủy";RevertBtn.AccessibleName="Hoàn lại ô đang sửa";
   foreach(var b in new[]{CancelBtn,OkBtn,RevertBtn}){BdgklLook034.Style(b,b==OkBtn,face,S(90),S(30));b.Margin=new Padding(S(4),S(4),0,0);tip.SetToolTip(b,b.AccessibleName);bar.Controls.Add(b);}
   root.Controls.Add(bar,0,3);root.SetColumnSpan(bar,2);Controls.Add(root);CancelButton=CancelBtn;
   // S5-UI-FONT-001: ambient Font did not reach editable boxes/combos (rendered 9pt). Set the shared scaled face explicitly on every control in the
   // tree (boxes, combos, labels, panels, tree); controls already given `bold` keep it. Reuses the two form fonts, no per-control allocation.
   Action<Control> applyFace=null;applyFace=c=>{foreach(Control x in c.Controls){if(!ReferenceEquals(x.Font,bold))x.Font=face;applyFace(x);}};applyFace(this);
   AddFloorBtn.Click+=(s,e)=>AddFloor();UpdateFloorBtn.Click+=(s,e)=>UpdateFloor();AddRoomBtn.Click+=(s,e)=>AddRoom();UpdateRoomBtn.Click+=(s,e)=>UpdateRoom();
   RevertBtn.Click+=(s,e)=>Revert();OkBtn.Click+=(s,e)=>TryAccept();
   ProjectTree.BeforeSelect+=(s,e)=>{if(loading)return;if(FloorPending()||RoomPending()){e.Cancel=true;
    Status("Ô tầng/phòng đang sửa chưa áp dụng: bấm THÊM/CẬP NHẬT hoặc 'HOÀN LẠI' trước khi chọn mục khác.",true);}};
   ProjectTree.AfterSelect+=(s,e)=>{if(!loading)LoadSelection();};
   ScopeBox.SelectedIndexChanged+=(s,e)=>ScopeChanged();
   InfoBox.Text="Thiết lập này chỉ đổi bảng đang mở; bấm 'Lưu vào bản vẽ' ở bảng phòng để ghi vào bản vẽ (một lần lưu, U hoàn tác). Hủy = không đổi gì.\r\n"+
    "Ba mốc cao độ KHÁC NHAU: chiều cao tầng (chỉ thông tin), trần ĐI DÂY (tính đoạn đứng của tuyến) và trần LẮP THIẾT BỊ (mốc của cách khai HẠ DƯỚI TRẦN). MEPS không lấy mốc này thay cho mốc kia và không tự điền số nào."+
    (scannedRoomId!=null?"\r\nSố thiết bị bị ảnh hưởng khi đổi CHUNG/PHÒNG chỉ đếm cho PHÒNG ĐANG QUÉT; các phòng khác tính lại khi được chọn và quét.":"")+
    (string.IsNullOrEmpty(legacyInfo)?"":"\r\n"+legacyInfo);
   loading=true;
   try{ProjectNameBox.Text=Model==null?"":Model.hierarchy.name;RebuildTree(selectRoomId);ruleScope=null;RebuildScopes();}
   finally{loading=false;}
   LoadSelection();LoadRules();ActiveControl=Model==null?(Control)ProjectNameBox:ProjectTree;}

  // ---- selection ----
  public string SelectedFloorId(){var nd=ProjectTree.SelectedNode;if(nd==null)return null;return nd.Parent==null?nd.Name:nd.Parent.Name;}
  public string SelectedRoom(){var nd=ProjectTree.SelectedNode;return nd==null||nd.Parent==null?null:nd.Name;}
  // QA helper: selects a floor or room node by stable ID; false = not found or blocked by pending edits.
  public bool SelectNode(string id){foreach(TreeNode f in ProjectTree.Nodes){if(f.Name==id){ProjectTree.SelectedNode=f;return ProjectTree.SelectedNode==f;}
    foreach(TreeNode r in f.Nodes)if(r.Name==id){ProjectTree.SelectedNode=r;return ProjectTree.SelectedNode==r;}}return false;}
  void RebuildTree(string selectId){bool was=loading;loading=true;
   try{ProjectTree.BeginUpdate();ProjectTree.Nodes.Clear();TreeNode sel=null;
    if(Model!=null)foreach(var f in Model.hierarchy.floors.OrderBy(x=>x.order).ThenBy(x=>x.id,StringComparer.Ordinal)){
     var fn=new TreeNode("TẦNG "+f.name){Name=f.id};ProjectTree.Nodes.Add(fn);if(f.id==selectId)sel=fn;
     foreach(var r in f.rooms){var rn=new TreeNode(RoomProjectUi037.RoomLabel(r)){Name=r.id};fn.Nodes.Add(rn);if(r.id==selectId)sel=rn;}}
    ProjectTree.ExpandAll();ProjectTree.EndUpdate();
    if(sel==null&&ProjectTree.Nodes.Count>0)sel=ProjectTree.Nodes[0];ProjectTree.SelectedNode=sel;}
   finally{loading=was;}}
  static string Snap(params Control[] c){return string.Join("\u001f",c.Select(x=>{var cb=x as ComboBox;return cb!=null?cb.SelectedIndex.ToString(CultureInfo.InvariantCulture):x.Text;}));}
  public bool FloorPending(){return Snap(FloorNameBox,StoreyBox,FloorWireBox,FloorMountBox)!=floorSnap;}
  public bool RoomPending(){return Snap(RoomCodeBox,RoomNameBox,RoomWireBox,RoomMountBox)!=roomSnap;}
  public bool RulesPending(){for(int i=0;i<Types.Length;i++)if(Snap(RuleModes[i],RuleValues[i])!=ruleSnap[i])return true;return false;}
  void LoadSelection(){bool was=loading;loading=true;
   try{var f=RoomProjectUi037.FindFloor(Model,SelectedFloorId());HierFloor037 rf;var r=RoomProjectUi037.FindRoom(Model,SelectedRoom(),out rf);
    FloorNameBox.Text=f==null?"":f.name;StoreyBox.Text=RoomProjectUi037.Exact(f==null?null:f.storeyHeight);
    FloorWireBox.Text=RoomProjectUi037.Exact(f==null?null:f.wireCeilingHeight);FloorMountBox.Text=RoomProjectUi037.Exact(f==null?null:f.mountingCeilingHeight);
    RoomCodeBox.Text=r==null?"":r.code??"";RoomNameBox.Text=r==null?"":r.name;
    RoomWireBox.Text=RoomProjectUi037.Exact(r==null?null:r.wireCeilingOverride);RoomMountBox.Text=RoomProjectUi037.Exact(r==null?null:r.mountingCeilingOverride);
    floorSnap=Snap(FloorNameBox,StoreyBox,FloorWireBox,FloorMountBox);roomSnap=Snap(RoomCodeBox,RoomNameBox,RoomWireBox,RoomMountBox);
    UpdateFloorBtn.Enabled=f!=null;AddRoomBtn.Enabled=f!=null;UpdateRoomBtn.Enabled=r!=null;}
   finally{loading=was;}
   Changed();}
  public void Revert(){LoadSelection();bool was=loading;loading=true;try{LoadRuleBoxes();}finally{loading=was;}Changed();Status("Đã hoàn lại các ô đang sửa (theo bản đang thiết lập).",false);}

  // ---- project / floor / room edits (validated on a copy, then replace the working copy) ----
  void Status(string t,bool err){StatusBox.ForeColor=err?BdgklLook034.Error:BdgklLook034.Ink;StatusBox.Text=t??"";}
  void CommitProjectName(){var name=ProjectNameBox.Text;
   if(Model==null){if(string.IsNullOrWhiteSpace(name))return;var p=RoomProjectUi037.NameProblem(name,"Tên dự án");if(p!=null)throw new IOE(p);
    if(string.IsNullOrEmpty(drawingId))throw new IOE("Bản vẽ không có mã nhận dạng hợp lệ; không tạo được dự án.");
    Model=RoomProject037.Create(name,drawingId,ids);return;}
   if(name==Model.hierarchy.name)return;var q=RoomProjectUi037.NameProblem(name,"Tên dự án");if(q!=null)throw new IOE(q);
   var tmp=RoomProject037.Copy(Model);tmp.hierarchy.name=name;var e=RoomProject037.Problem(tmp);if(e!=null)throw new IOE(e);Model=tmp;}
  RoomProjectData037 Start(){CommitProjectName();if(Model==null)throw new IOE("Nhập TÊN DỰ ÁN trước (MEPS không tự đặt tên).");return RoomProject037.Copy(Model);}
  static void Done(RoomProjectData037 tmp){var e=RoomProject037.Problem(tmp);if(e!=null)throw new IOE(e+" Không thay đổi gì.");}
  void FloorValues(out string name,out double? st,out double? w,out double? mt){name=FloorNameBox.Text;var p=RoomProjectUi037.NameProblem(name,"Tên tầng");if(p!=null)throw new IOE(p);
   st=RoomProjectUi037.Positive(StoreyBox.Text,"Chiều cao tầng");w=RoomProjectUi037.Positive(FloorWireBox.Text,"Cao độ trần đi dây");mt=RoomProjectUi037.Positive(FloorMountBox.Text,"Cao độ trần lắp thiết bị");}
  void RoomValues(out string code,out string name,out double? w,out double? mt){code=RoomCodeBox.Text;name=RoomNameBox.Text;
   var p=RoomProjectUi037.NameProblem(code,"Mã phòng")??RoomProjectUi037.NameProblem(name,"Mô tả phòng");if(p!=null)throw new IOE(p);
   w=RoomProjectUi037.Positive(RoomWireBox.Text,"Trần đi dây riêng của phòng");mt=RoomProjectUi037.Positive(RoomMountBox.Text,"Trần lắp thiết bị riêng của phòng");}
  bool Edit(Func<string> act){try{var m=act();if(Model!=null&&!RuleModes[0].Enabled)LoadRules();   // project just created: rule grid becomes usable
    Status(m+" Chưa lưu vào bản vẽ (Áp dụng, rồi 'Lưu vào bản vẽ').",false);return true;}catch(Exception ex){Status(ex.Message,true);return false;}}
  public bool AddFloor(){return Edit(()=>{string name;double? st,w,mt;FloorValues(out name,out st,out w,out mt);var tmp=Start();
   var f=RoomProject037.AddFloor(tmp,name,ids);f.storeyHeight=st;f.wireCeilingHeight=w;f.mountingCeilingHeight=mt;Done(tmp);
   Model=tmp;RebuildTree(f.id);RebuildScopes();LoadSelection();return "Đã thêm tầng \""+name+"\".";});}
  public bool UpdateFloor(){return Edit(()=>{var id=SelectedFloorId();if(id==null)throw new IOE("Chọn một tầng trong cây trước.");string name;double? st,w,mt;FloorValues(out name,out st,out w,out mt);
   var tmp=Start();var f=RoomHierarchy037.RenameFloor(tmp.hierarchy,id,name);f.storeyHeight=st;f.wireCeilingHeight=w;f.mountingCeilingHeight=mt;Done(tmp);
   var sel=SelectedRoom()??id;Model=tmp;RebuildTree(sel);RebuildScopes();LoadSelection();return "Đã cập nhật tầng \""+name+"\".";});}
  public bool AddRoom(){return Edit(()=>{var fid=SelectedFloorId();if(fid==null)throw new IOE("Chọn (hoặc thêm) một tầng trước khi thêm phòng.");string code,name;double? w,mt;RoomValues(out code,out name,out w,out mt);
   var tmp=Start();var r=RoomProjectUi037.AddCodedRoom(tmp,fid,code,name,ids);r.wireCeilingOverride=w;r.mountingCeilingOverride=mt;Done(tmp);
   Model=tmp;RebuildTree(r.id);RebuildScopes();LoadSelection();return "Đã thêm phòng "+RoomProjectUi037.RoomLabel(r)+".";});}
  public bool UpdateRoom(){return Edit(()=>{var rid=SelectedRoom();if(rid==null)throw new IOE("Chọn một phòng trong cây trước.");string code,name;double? w,mt;RoomValues(out code,out name,out w,out mt);
   var tmp=Start();
   // coded room: description may repeat on the floor (RoomHierarchy037.RenameRoom S4 rule)
   var r=RoomProject037.RenameRoom(tmp,rid,code,name);r.wireCeilingOverride=w;r.mountingCeilingOverride=mt;Done(tmp);
   Model=tmp;RebuildTree(r.id);RebuildScopes();LoadSelection();return "Đã cập nhật phòng "+RoomProjectUi037.RoomLabel(r)+" (mã nhận dạng nội bộ giữ nguyên).";});}

  // ---- rules ----
  void RebuildScopes(){bool was=loading;loading=true;
   try{ScopeBox.Items.Clear();ScopeBox.Items.Add(new RouteDialog034.Choice(null,"CHUNG — mặc định của dự án"));int sel=0;
    if(Model!=null)foreach(var f in Model.hierarchy.floors.OrderBy(x=>x.order).ThenBy(x=>x.id,StringComparer.Ordinal))foreach(var r in f.rooms){
     ScopeBox.Items.Add(new RouteDialog034.Choice(r.id,"PHÒNG "+RoomProjectUi037.RoomLabel(r)+" · tầng "+f.name));if(r.id==ruleScope)sel=ScopeBox.Items.Count-1;}
    if(sel==0)ruleScope=null;ScopeBox.SelectedIndex=sel;}
   finally{loading=was;}}
  // QA helper: scope by stable room ID (null = CHUNG). false = not found or the pending rule edits are invalid (nothing changed).
  public bool SetScope(string roomId){for(int i=0;i<ScopeBox.Items.Count;i++)if(((RouteDialog034.Choice)ScopeBox.Items[i]).value==roomId){ScopeBox.SelectedIndex=i;return ruleScope==roomId;}return false;}
  void ScopeChanged(){if(loading)return;var c=ScopeBox.SelectedItem as RouteDialog034.Choice;string want=c==null?null:c.value;if(want==ruleScope)return;
   try{CommitRules();}
   catch(Exception ex){bool was=loading;loading=true;try{SetScopeIndex(ruleScope);}finally{loading=was;}Status("Chưa đổi phạm vi: "+ex.Message,true);return;}
   ruleScope=want;LoadRules();}
  void SetScopeIndex(string id){for(int i=0;i<ScopeBox.Items.Count;i++)if(((RouteDialog034.Choice)ScopeBox.Items[i]).value==id){ScopeBox.SelectedIndex=i;return;}ScopeBox.SelectedIndex=0;}
  RoomHeightRule037 Rule(RoomProjectData037 m,string type){return ruleScope==null?RoomProjectUi037.DefaultRule(m,type):RoomProjectUi037.RoomRule(m,ruleScope,type);}
  static void Choose(ComboBox b,string value){for(int i=0;i<b.Items.Count;i++)if(string.Equals(((RouteDialog034.Choice)b.Items[i]).value,value,StringComparison.Ordinal)){b.SelectedIndex=i;return;}b.SelectedIndex=0;}
  static string Value(ComboBox b){var c=b.SelectedItem as RouteDialog034.Choice;return c==null?null:c.value;}
  void LoadRuleBoxes(){for(int i=0;i<Types.Length;i++)LoadRuleRow(i);}
  void LoadRuleRow(int i){var r=Rule(Model,Types[i]);Choose(RuleModes[i],r==null?null:r.mode);RuleValues[i].Text=r==null?"":RoomProjectUi037.Exact(r.value);ruleSnap[i]=Snap(RuleModes[i],RuleValues[i]);}
  void LoadRules(){bool was=loading;loading=true;
   try{LoadRuleBoxes();bool on=Model!=null;
    for(int i=0;i<Types.Length;i++){RuleModes[i].Enabled=on;RuleValues[i].Enabled=on;RuleResets[i].Enabled=on;
     RuleResets[i].Text=ruleScope==null?"XOÁ MẶC ĐỊNH":"DÙNG LẠI MẶC ĐỊNH";RuleResets[i].AccessibleName=RuleResets[i].Text+" "+RoomProjectUi037.TypeLabel(Types[i]);
     tip.SetToolTip(RuleResets[i],ruleScope==null?"Xoá mặc định CHUNG của "+RoomProjectUi037.TypeLabel(Types[i])+" (thiết bị đang thừa hưởng thành CHƯA KHAI)":
      "PHÒNG bỏ khai báo riêng của "+RoomProjectUi037.TypeLabel(Types[i])+", dùng lại mặc định CHUNG");}}
   finally{loading=was;}
   Changed();}
  // Reads the five rows of the current scope. Each row: both blank = absent (an existing rule must be removed with the explicit button), else mode AND value.
  // All rows are validated and applied on a copy; the working copy is replaced only when everything succeeds.
  public void CommitRules(){if(!RulesPending())return;if(Model==null)throw new IOE("Tạo dự án (TÊN DỰ ÁN) trước khi khai cao độ.");
   var tmp=RoomProject037.Copy(Model);
   for(int i=0;i<Types.Length;i++){string t=Types[i];var mode=Value(RuleModes[i]);var v=BdgklUi034.ParseDecimal(RuleValues[i].Text,"Giá trị "+RoomProjectUi037.TypeLabel(t));var cur=Rule(tmp,t);
    if(mode==null&&!v.HasValue){if(cur!=null)throw new IOE(RoomProjectUi037.TypeLabel(t)+": ô đang trống nhưng đã có khai báo ("+RoomProjectUi037.ModeLabel(cur.mode)+" "+RoomProjectUi037.Exact(cur.value)+
      "); bấm '"+RuleResets[i].Text+"' để bỏ, hoặc nhập lại. Không thay đổi gì.");continue;}
    if(mode==null||!v.HasValue)throw new IOE(RoomProjectUi037.TypeLabel(t)+": cần chọn CÁCH KHAI và nhập GIÁ TRỊ (hoặc để trống cả hai). Không thay đổi gì.");
    if(cur!=null&&cur.mode==mode&&cur.value==v.Value)continue;
    if(ruleScope==null)RoomProject037.SetDefault(tmp,t,mode,v.Value);else RoomProject037.SetRoomRule(tmp,ruleScope,t,mode,v.Value);}
   Model=tmp;bool was=loading;loading=true;try{LoadRuleBoxes();}finally{loading=was;}}
  // XOÁ MẶC ĐỊNH (scope CHUNG) / DÙNG LẠI MẶC ĐỊNH (scope PHÒNG) for one type. Only that row is reloaded; other rows keep what is typed.
  public bool ResetRule(string type){int i=Array.IndexOf(Types,type);if(i<0||Model==null)return false;
   try{var tmp=RoomProject037.Copy(Model);bool removed;
    if(ruleScope==null)removed=tmp.defaults.RemoveAll(x=>x!=null&&x.deviceType==type)>0;else removed=RoomProject037.ResetRoomRule(tmp,ruleScope,type);
    Done(tmp);Model=tmp;bool was=loading;loading=true;try{LoadRuleRow(i);}finally{loading=was;}Changed();
    Status(!removed?RoomProjectUi037.TypeLabel(type)+": không có khai báo để bỏ.":ruleScope==null?"Đã xoá mặc định CHUNG của "+RoomProjectUi037.TypeLabel(type)+" (thiết bị thừa hưởng sẽ thành CHƯA KHAI).":
     "Phòng dùng lại mặc định CHUNG cho "+RoomProjectUi037.TypeLabel(type)+".",false);return removed;}
   catch(Exception ex){Status(ex.Message,true);return false;}}
  // Notes + live preview (affected unique devices of the scanned room only). Never mutates.
  void Changed(){if(loading)return;var parts=new List<string>();bool bad=false;
   for(int i=0;i<Types.Length;i++){string t=Types[i];var cur=Model==null?null:Rule(Model,t);string noteText;
    if(ruleScope==null)noteText=cur==null?"chưa khai":RoomProjectUi037.ModeLabel(cur.mode)+" "+RoomProjectUi037.Exact(cur.value);
    else{var d=RoomProjectUi037.DefaultRule(Model,t);noteText=cur!=null?"PHÒNG: "+RoomProjectUi037.ModeLabel(cur.mode)+" "+RoomProjectUi037.Exact(cur.value):
     d==null?"theo CHUNG: chưa khai":"theo CHUNG: "+RoomProjectUi037.ModeLabel(d.mode)+" "+RoomProjectUi037.Exact(d.value);}
    RuleNotes[i].Text=noteText;
    if(Snap(RuleModes[i],RuleValues[i])==ruleSnap[i])continue;
    try{var v=BdgklUi034.ParseDecimal(RuleValues[i].Text,"Giá trị "+RoomProjectUi037.TypeLabel(t));var mode=Value(RuleModes[i]);
     if((mode==null)!=(!v.HasValue)){parts.Add(RoomProjectUi037.TypeLabel(t)+": cần cả cách khai và giá trị");bad=true;continue;}
     if(mode==null&&cur!=null){parts.Add(RoomProjectUi037.TypeLabel(t)+": ô trống — dùng nút '"+RuleResets[i].Text+"' để bỏ khai báo");bad=true;continue;}
     string where=scannedRoomId==null?"":(ruleScope==null||ruleScope==scannedRoomId?" · phòng đang quét: "+RoomProjectUi037.Inheriting(Model,scannedRoomId,scanned,t,ruleScope!=null)+" thiết bị thừa hưởng sẽ đổi":"");
     parts.Add(RoomProjectUi037.TypeLabel(t)+" → "+(mode==null?"chưa khai":RoomProjectUi037.ModeLabel(mode)+" "+RoomProjectUi037.Exact(v))+where);}
    catch(IOE ex){parts.Add(ex.Message);bad=true;}}
   if(FloorPending())parts.Add("ô TẦNG đang sửa chưa áp dụng");if(RoomPending())parts.Add("ô PHÒNG đang sửa chưa áp dụng");
   if(parts.Count>0)Status((bad?"Chưa Áp dụng được: ":"Sẽ áp dụng khi bấm Áp dụng: ")+string.Join("; ",parts)+".",bad);
   else if(Model==null)Status("Chưa có dự án: nhập TÊN DỰ ÁN, thêm tầng và phòng. Không có số nào được điền sẵn.",false);
   else if(StatusBox.ForeColor!=BdgklLook034.Error)Status("",false);}
  public bool TryAccept(){
   try{CommitProjectName();
    if(FloorPending()||RoomPending())throw new IOE("Ô TẦNG/PHÒNG đang sửa chưa áp dụng: bấm THÊM/CẬP NHẬT hoặc 'HOÀN LẠI'. Chưa áp dụng gì.");
    CommitRules();
    if(Model==null)throw new IOE("Chưa có dự án: nhập TÊN DỰ ÁN (và thêm tầng/phòng), hoặc bấm Hủy.");
    var p=RoomProject037.Problem(Model);if(p!=null)throw new IOE("Dữ liệu dự án không hợp lệ: "+p);
    var doc=RoomProjectUi037.CopyDocument(original)??new RoomProjectDocument037();doc.model=RoomProject037.Copy(Model);
    RoomFastEntryUi045.PruneRooms(doc);   // 045: room defaults / row state of a deleted room go with it (never moved to another room)
    Result=doc;SelectedRoomId=SelectedRoom();}
   catch(Exception ex){Result=null;Status(ex.Message,true);return false;}
   DialogResult=DialogResult.OK;return true;}
  protected override void Dispose(bool disposing){base.Dispose(disposing);if(disposing){face.Dispose();bold.Dispose();}}
 }

 // ---- CAO ĐỘ THIẾT BỊ (open room only) ----
 public sealed class RoomDeviceDialog037:Form {
  public readonly DataGridView DeviceGrid;public readonly ComboBox ModeBox=new ComboBox();public readonly TextBox ValueBox=new TextBox(),InfoBox=new TextBox(),StatusBox=new TextBox();
  public readonly Button SetBtn=new Button(),ResetBtn=new Button(),OkBtn=new Button(),CancelBtn=new Button();
  public RoomProjectData037 Model;public RoomProjectData037 Result;
  public readonly string RoomId;public readonly List<RoomDevice037> Devices;
  readonly float dpi;readonly Font face,bold;bool loading;string editSnap="";
  public static readonly string[] Columns={"THIẾT BỊ","LOẠI","CAO ĐỘ HIỆU LỰC (m)","NGUỒN","CÁCH KHAI","GIÁ TRỊ KHAI RIÊNG (m)","GHI CHÚ"};
  int S(int px){return Math.Max(1,(int)Math.Round(px*dpi));}
  public RoomDeviceDialog037(RoomProjectData037 model,string roomId,IList<RoomDevice037> devices,string roomLabel,float dpi){
   if(model==null||string.IsNullOrEmpty(roomId))throw new ArgumentException("Cần dự án và phòng đang mở.");
   Model=RoomProject037.Copy(model);RoomId=roomId;Devices=devices==null?new List<RoomDevice037>():devices.Where(d=>d!=null).ToList();this.dpi=BdgklLook034.Dpi(dpi);
   Text="MEPS — Cao độ thiết bị";Name="room037_device_dialog";FormBorderStyle=FormBorderStyle.Sizable;MinimizeBox=false;MaximizeBox=true;ShowInTaskbar=false;
   StartPosition=FormStartPosition.CenterParent;AutoScaleMode=AutoScaleMode.None;face=RoomLook037.Face(this.dpi,false);bold=RoomLook037.Face(this.dpi,true);Font=face;
   BackColor=BdgklLook034.Back;ForeColor=BdgklLook034.Ink;ClientSize=new Size(S(1000),S(620));MinimumSize=new Size(S(760),S(460));
   var pane=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=1,RowCount=5,Padding=new Padding(S(8)),BackColor=BdgklLook034.Back};pane.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
   foreach(int h in new[]{S(72),-100,S(44),S(48),S(44)})pane.RowStyles.Add(h<0?new RowStyle(SizeType.Percent,-h):new RowStyle(SizeType.Absolute,h));
   BdgklLook034.ReadOnlyBox(InfoBox);InfoBox.Name="room037_device_info";InfoBox.AccessibleName="Phạm vi (chỉ đọc)";
   DeviceGrid=RoomLook037.Grid("room037_devices","Thiết bị của phòng đang mở",Columns,new[]{24,12,12,9,20,12,30},2,2,face,bold,this.dpi);
   DeviceGrid.Columns[5].DefaultCellStyle.Alignment=DataGridViewContentAlignment.MiddleRight;
   foreach(DataGridViewColumn c in DeviceGrid.Columns)if(c.DefaultCellStyle.Alignment!=DataGridViewContentAlignment.MiddleRight)c.DefaultCellStyle.Alignment=DataGridViewContentAlignment.MiddleLeft;
   var edit=new FlowLayoutPanel{Dock=DockStyle.Fill,WrapContents=false,Margin=new Padding(0),BackColor=BdgklLook034.Back};
   edit.Controls.Add(new Label{Text="KHAI RIÊNG THIẾT BỊ ĐANG CHỌN",AutoSize=true,Font=bold,Margin=new Padding(0,S(10),S(8),0)});
   ModeBox.Name="room037_device_mode";ModeBox.AccessibleName="Cách khai riêng";ModeBox.DropDownStyle=ComboBoxStyle.DropDownList;ModeBox.Width=S(250);ModeBox.Margin=new Padding(0,S(7),S(6),0);
   ModeBox.Items.Add(new RouteDialog034.Choice(null,"(chưa khai riêng)"));ModeBox.Items.Add(new RouteDialog034.Choice(RoomProject037.Fixed,"CỐ ĐỊNH TỪ SÀN HOÀN THIỆN"));
   ModeBox.Items.Add(new RouteDialog034.Choice(RoomProject037.MountingOffset,"HẠ DƯỚI TRẦN LẮP THIẾT BỊ"));ModeBox.SelectedIndex=0;edit.Controls.Add(ModeBox);
   ValueBox.Name="room037_device_value";ValueBox.AccessibleName="Giá trị khai riêng (m)";ValueBox.Width=S(100);ValueBox.BorderStyle=BorderStyle.FixedSingle;ValueBox.TextAlign=HorizontalAlignment.Right;
   ValueBox.MaxLength=Bdgkl034.MaxText;ValueBox.Margin=new Padding(0,S(7),S(8),0);edit.Controls.Add(ValueBox);
   SetBtn.Text="KHAI RIÊNG";SetBtn.Name="room037_device_set";ResetBtn.Text="DÙNG LẠI MẶC ĐỊNH";ResetBtn.Name="room037_device_reset";
   foreach(var b in new[]{SetBtn,ResetBtn}){BdgklLook034.Style(b,false,face,S(90),S(28));b.AccessibleName=b.Text;b.Margin=new Padding(0,S(6),S(6),0);edit.Controls.Add(b);}
   BdgklLook034.ReadOnlyBox(StatusBox);StatusBox.Name="room037_device_status";StatusBox.AccessibleName="Thông báo";StatusBox.BackColor=BdgklLook034.Back;StatusBox.BorderStyle=BorderStyle.None;
   var bar=new FlowLayoutPanel{FlowDirection=System.Windows.Forms.FlowDirection.RightToLeft,Dock=DockStyle.Fill,WrapContents=false,Margin=new Padding(0),BackColor=BdgklLook034.Back};
   OkBtn.Text="Áp dụng";OkBtn.Name="room037_device_ok";CancelBtn.Text="Hủy";CancelBtn.Name="room037_device_cancel";CancelBtn.DialogResult=DialogResult.Cancel;
   foreach(var b in new[]{CancelBtn,OkBtn}){BdgklLook034.Style(b,b==OkBtn,face,S(72),S(30));b.Margin=new Padding(S(4),S(4),0,0);b.AccessibleName=b.Text;bar.Controls.Add(b);}
   pane.Controls.Add(InfoBox,0,0);pane.Controls.Add(DeviceGrid,0,1);pane.Controls.Add(edit,0,2);pane.Controls.Add(StatusBox,0,3);pane.Controls.Add(bar,0,4);Controls.Add(pane);
   CancelButton=CancelBtn;SetBtn.Click+=(s,e)=>SetOverride();ResetBtn.Click+=(s,e)=>ResetOverride();OkBtn.Click+=(s,e)=>TryAccept();
   DeviceGrid.SelectionChanged+=(s,e)=>{if(!loading)LoadEdit(true);};
   int detached=Model.devices.Count(v=>v!=null&&string.Equals(v.roomId,roomId,StringComparison.Ordinal)&&!Devices.Any(d=>string.Equals(d.id,v.deviceId,StringComparison.OrdinalIgnoreCase)));
   InfoBox.Text="PHÒNG "+roomLabel+" · "+Devices.Count.ToString(CultureInfo.InvariantCulture)+" thiết bị trong vùng quét (loại lấy từ bản vẽ; MEPS không đoán loại).\r\n"+
    "Khai riêng chỉ cho thiết bị đang chọn; DÙNG LẠI MẶC ĐỊNH bỏ khai riêng để thiết bị theo PHÒNG/CHUNG. Ô trống = chưa khai (không phải 0). Áp dụng chỉ đổi bảng đang mở; 'Lưu vào bản vẽ' để lưu."+
    (detached>0?"\r\n"+detached.ToString(CultureInfo.InvariantCulture)+" khai báo riêng của phòng này không còn thiết bị trong vùng quét: giữ nguyên, không gán sang thiết bị khác.":"");
   Fill(null);}
  public RoomDevice037 Selected(){return DeviceGrid.SelectedRows.Count==1?DeviceGrid.SelectedRows[0].Tag as RoomDevice037:null;}
  // QA helper: selects the device row of this handle; false = not listed.
  public bool SelectDevice(string id){foreach(DataGridViewRow r in DeviceGrid.Rows){var d=r.Tag as RoomDevice037;
    if(d!=null&&string.Equals(d.id,id,StringComparison.Ordinal)){DeviceGrid.ClearSelection();r.Selected=true;return true;}}return false;}
  static RoomHeightRule037 Own(RoomProjectData037 m,string id){var e=m.devices.FirstOrDefault(x=>x!=null&&string.Equals(x.deviceId,id,StringComparison.OrdinalIgnoreCase));return e==null?null:e.rule;}
  void Fill(string keepId){loading=true;
   try{DeviceGrid.Rows.Clear();
    foreach(var d in Devices){var e=RoomProjectUi037.Effective(Model,RoomId,d);var own=Own(Model,d.id);bool known=RoomProject037.Types.Contains(d.type);
     int k=DeviceGrid.Rows.Add(RoomProjectUi037.DeviceLabel(d),known?RoomProjectUi037.TypeLabel(d.type):"CHƯA PHÂN LOẠI",e.status==RoomProject037.Valid?BdgklLook034.M(e.height):"—",
      RoomProjectUi037.SourceLabel(e.source),RoomProjectUi037.ModeLabel(e.mode),own==null?"":RoomProjectUi037.Exact(own.value),
      known?(e.status==RoomProject037.Valid?"":RoomProjectUi037.StatusLabel(e.status)+": "+e.message):"Loại không hỗ trợ khai cao độ; MEPS không đoán loại.");
     var row=DeviceGrid.Rows[k];row.Tag=d;row.Cells[6].ToolTipText=e.message;
     if(e.status==RoomProject037.Invalid){row.DefaultCellStyle.BackColor=BdgklLook034.ExcludedBack;row.DefaultCellStyle.ForeColor=BdgklLook034.Error;}
     else if(e.status!=RoomProject037.Valid){row.DefaultCellStyle.BackColor=RoomLook037.Missing;row.DefaultCellStyle.ForeColor=RoomLook037.MissingInk;}
     if(keepId!=null&&d.id==keepId){DeviceGrid.ClearSelection();row.Selected=true;}}}
   finally{loading=false;}
   LoadEdit(false);}
  static void Choose(ComboBox b,string value){for(int i=0;i<b.Items.Count;i++)if(string.Equals(((RouteDialog034.Choice)b.Items[i]).value,value,StringComparison.Ordinal)){b.SelectedIndex=i;return;}b.SelectedIndex=0;}
  static string Value(ComboBox b){var c=b.SelectedItem as RouteDialog034.Choice;return c==null?null:c.value;}
  string EditState(){return ModeBox.SelectedIndex.ToString(CultureInfo.InvariantCulture)+"\u001f"+ValueBox.Text;}
  public bool EditPending(){return EditState()!=editSnap;}
  void LoadEdit(bool notePending){bool dropped=notePending&&EditPending();var d=Selected();var own=d==null?null:Own(Model,d.id);bool known=d!=null&&RoomProject037.Types.Contains(d.type);
   Choose(ModeBox,own==null?null:own.mode);ValueBox.Text=own==null?"":RoomProjectUi037.Exact(own.value);editSnap=EditState();
   ModeBox.Enabled=ValueBox.Enabled=SetBtn.Enabled=known;ResetBtn.Enabled=own!=null;
   if(dropped)Status("Ô khai riêng đang gõ cho thiết bị trước CHƯA được áp dụng (chưa bấm KHAI RIÊNG) và đã bỏ.",true);
   else if(d!=null&&!known)Status("Thiết bị "+RoomProjectUi037.DeviceLabel(d)+" chưa phân loại: không khai cao độ được; sửa loại thiết bị trong bản vẽ rồi quét lại.",true);
   else Status("",false);}
  void Status(string t,bool err){StatusBox.ForeColor=err?BdgklLook034.Error:BdgklLook034.Ink;StatusBox.Text=t??"";}
  // Explicit override of the selected device (actual handle + authoritative type). Both mode and value required; 0 is a real value.
  public bool SetOverride(){try{var d=Selected();if(d==null)throw new IOE("Chọn một thiết bị trong bảng.");
    if(!RoomProject037.Types.Contains(d.type))throw new IOE("Thiết bị chưa phân loại; MEPS không đoán loại nên không khai cao độ được.");
    var mode=Value(ModeBox);var v=BdgklUi034.ParseDecimal(ValueBox.Text,"Giá trị khai riêng");
    if(mode==null||!v.HasValue)throw new IOE("Chọn CÁCH KHAI và nhập GIÁ TRỊ (0 là giá trị hợp lệ). Muốn bỏ khai riêng: DÙNG LẠI MẶC ĐỊNH. Không thay đổi gì.");
    var tmp=RoomProject037.Copy(Model);RoomProject037.SetDeviceRule(tmp,RoomId,d.id,d.type,mode,v.Value);Model=tmp;Fill(d.id);
    var e=RoomProjectUi037.Effective(Model,RoomId,d);
    Status("Đã khai riêng "+RoomProjectUi037.DeviceLabel(d)+": "+RoomProjectUi037.ModeLabel(mode)+" "+RoomProjectUi037.Exact(v)+" → "+(e.status==RoomProject037.Valid?BdgklLook034.M(e.height)+" m":RoomProjectUi037.StatusLabel(e.status)+" ("+e.message+")")+".",e.status!=RoomProject037.Valid);
    return true;}
   catch(Exception ex){Status(ex.Message,true);return false;}}
  public bool ResetOverride(){try{var d=Selected();if(d==null)throw new IOE("Chọn một thiết bị trong bảng.");
    var tmp=RoomProject037.Copy(Model);bool removed=RoomProject037.ResetDeviceRule(tmp,RoomId,d.id);Model=tmp;Fill(d.id);
    var e=RoomProjectUi037.Effective(Model,RoomId,d);
    Status(removed?"Thiết bị "+RoomProjectUi037.DeviceLabel(d)+" dùng lại mặc định: "+(e.status==RoomProject037.Valid?BdgklLook034.M(e.height)+" m (nguồn "+RoomProjectUi037.SourceLabel(e.source)+")":RoomProjectUi037.StatusLabel(e.status))+".":
     "Thiết bị này không có khai riêng.",false);return removed;}
   catch(Exception ex){Status(ex.Message,true);return false;}}
  public bool TryAccept(){try{if(EditPending())throw new IOE("Ô khai riêng đang gõ chưa áp dụng: bấm KHAI RIÊNG, hoặc chọn lại thiết bị để bỏ. Chưa áp dụng gì.");
    var p=RoomProject037.Problem(Model);if(p!=null)throw new IOE("Dữ liệu dự án không hợp lệ: "+p);Result=RoomProject037.Copy(Model);}
   catch(Exception ex){Result=null;Status(ex.Message,true);return false;}
   DialogResult=DialogResult.OK;return true;}
  protected override void Dispose(bool disposing){base.Dispose(disposing);if(disposing){face.Dispose();bold.Dispose();}}
 }
}
#endif
