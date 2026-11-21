using System;using System.Drawing;using System.Linq;using System.Text;using System.Globalization;using System.Collections.Generic;using System.Runtime.CompilerServices;using System.Windows.Forms;using IOE=System.InvalidOperationException;
#if ROOMCAD037 && BDGKL034
namespace Tvxd.Meps {
 // TVXD-MEPS-FAST-ENTRY-045 native integration (Vietnamese text: /codepage:65001). Pure core: RoomFastEntry045.cs.
 //  - RoomFastEntryUi045 (session, no Forms): binds the open room's row state (Rebind, called by RoomUi037.Rebuild), resolves inherited route fields
 //    into the inputs of non-RIÊNG fields (Resolve, called by RoomUi037.Recalc before the unchanged profile + calculation), keeps the RIÊNG set
 //    (MarkExplicit from ApplyCell / dialog / paste / fill), writes only RIÊNG values into drafts (ExplicitOnly, from RoomUi037.Capture) and puts the
 //    row state into a project document next to the draft (PutRoom). Bulk paste / fill = parse + validate EVERY target first (FastPlanner045), then one
 //    in-memory change + one Recalc; table undo/redo = whole snapshots of the open room's declarations + provenance + project presets, valid only in the
 //    same room / batch / base raw / adjustments / project model (any change = history refused and cleared). Never CAD geometry, never the NOD.
 //  - RoomReport037 (partial): cell-range selection on TUYẾN, Ctrl+C/V/D/Z/Y (only when no cell is being edited; the editor keeps its own text undo),
 //    buttons for the same operations, presets (QUẢN LÝ MẪU) and explicitly chosen room / device-kind defaults, compact columns (CỘT NHẬP ĐẦY ĐỦ shows
 //    the rest), CẦN XỬ LÝ default filter, collapsed details, pending raw rows shown as CHỜ ÁP DỤNG (never ĐỦ).
 public static class RoomFastEntryUi045 {
  static readonly StringComparer O=StringComparer.Ordinal;
  static readonly ConditionalWeakTable<object,string> Ids=new ConditionalWeakTable<object,string>();
  static string Id(object o){return o==null?"-":Ids.GetValue(o,k=>Guid.NewGuid().ToString("N"));}
  public static bool Active(RoomSession037 s){return s!=null&&s.projectMode&&s.batch!=null&&s.projectDocument!=null;}

  // Engineering value check of a stored preset value through the product cell parser (round trip text -> same typed value). null = valid.
  public static string ValueProblem(string field,object v){if(v==null)return null;
   try{var i=new BdgklInput034();RoomAdjustmentsUi037.SetCell(i,field,v);var text=RoomAdjustmentsUi037.CellText(i,field);
    var back=RoomAdjustmentsUi037.ParseCell(field,text);return Equals(back,v)?null:"giá trị không đọc lại đúng";}
   catch(IOE ex){return ex.Message;}catch(InvalidCastException){return "kiểu giá trị sai";}}
  public static string Text(string field,object v){if(v==null)return "";var i=new BdgklInput034();RoomAdjustmentsUi037.SetCell(i,field,v);return RoomAdjustmentsUi037.CellText(i,field);}

  // ---- RIÊNG tracking ----
  public static void MarkExplicit(RoomSession037 s,string rowKey,string field){
   if(s==null||s.fast==null||rowKey==null||!FastEntry045.IsPresetField(field)||s.batch==null)return;
   var r=s.batch.rows.FirstOrDefault(q=>q.rowKey!=null&&O.Equals(q.rowKey,rowKey));if(r==null||r.input==null)return;
   FastEntry045.Explicit(s.fast,rowKey).Add(field);
   var w=FastEntry045.Row(s.fast,rowKey,true);if(w.blanks==null)w.blanks=new List<string>();
   w.blanks.Remove(field);if(RoomAdjustmentsUi037.CellValue(r.input,field)==null)w.blanks.Add(field);}   // explicit blank kept as a persisted blank
  public static bool ClearExplicit(RoomSession037 s,string rowKey,string field){
   if(s==null||s.fast==null||rowKey==null||!FastEntry045.IsPresetField(field))return false;
   HashSet<string> ex;bool had=s.fast.explicitFields.TryGetValue(rowKey,out ex)&&ex.Remove(field);
   var w=FastEntry045.Row(s.fast,rowKey,false);if(w!=null&&w.blanks!=null)w.blanks.Remove(field);
   return had;}
  public static bool IsExplicit(RoomSession037 s,string rowKey,string field){HashSet<string> ex;
   return s!=null&&s.fast!=null&&rowKey!=null&&s.fast.explicitFields.TryGetValue(rowKey,out ex)&&ex.Contains(field);}
  // PATCH1 F02: inheritable field of a project-mode row that is NOT RIÊNG now (an accepted typed edit of its same value makes it RIÊNG).
  public static bool Inherited(RoomSession037 s,string rowKey,string field){
   return s!=null&&s.projectMode&&s.fast!=null&&rowKey!=null&&FastEntry045.IsPresetField(field)&&!IsExplicit(s,rowKey,field);}
  public static object[] Values(BdgklInput034 i){return FastEntry045.PresetFields.Select(f=>i==null?null:RoomAdjustmentsUi037.CellValue(i,f)).ToArray();}
  // Declaration dialog: preset fields whose value differs from what the row showed before become RIÊNG.
  public static void MarkChanged(RoomSession037 s,string rowKey,object[] before,BdgklInput034 after){if(before==null||after==null)return;
   for(int k=0;k<FastEntry045.PresetFields.Length;k++){var f=FastEntry045.PresetFields[k];if(!Equals(before[k],RoomAdjustmentsUi037.CellValue(after,f)))MarkExplicit(s,rowKey,f);}}
  // Draft capture: inherited (non-RIÊNG) preset fields are written as null; RIÊNG values unchanged. No fast state = unchanged behaviour.
  public static void ExplicitOnly(RoomSession037 s,string rowKey,RoomDraftRow037 d){if(s==null||s.fast==null||!s.projectMode||d==null)return;
   foreach(var f in FastEntry045.PresetFields){if(IsExplicit(s,rowKey,f))continue;
    switch(f){case FastEntry045.Routing:d.routingMode=null;break;case FastEntry045.TailCount:d.tailCount=null;break;case FastEntry045.TailEach:d.tailEachLength=null;break;
     case FastEntry045.Factor:d.factor=null;break;case FastEntry045.Assembly:d.assemblyCode=null;break;case FastEntry045.Conduit:d.conduitCode=null;break;}}}

  // ---- bind / resolve ----
  // After RoomUi037.Rebuild overlaid the declarations: same room = row state kept by row key (Cập nhật); another room / first open = row state from the
  // project document. RIÊNG = every preset field holding a declared value (v1 drafts: all their values) + persisted explicit blanks.
  public static void Rebind(RoomSession037 s){if(s==null)return;
   if(!s.projectMode||s.batch==null){if(s.batch==null||!s.projectMode)s.fast=null;return;}
   var prev=s.fast!=null&&O.Equals(s.fast.roomId,s.roomId)?s.fast:null;
   var fs=new FastSession045{roomId=s.roomId};
   if(prev!=null){foreach(var kv in prev.rows)fs.rows[kv.Key]=FastEntry045.Copy(kv.Value);}
   else{var st=FastEntry045.RoomState(s.projectDocument==null?null:s.projectDocument.fast,s.roomId);
    if(st!=null&&st.rows!=null)foreach(var w in st.rows)if(w!=null&&w.rowKey!=null&&!fs.rows.ContainsKey(w.rowKey))fs.rows[w.rowKey]=FastEntry045.Copy(w);}
   foreach(var r in s.batch.rows){if(r.input==null||r.rowKey==null)continue;var ex=FastEntry045.Explicit(fs,r.rowKey);
    foreach(var f in FastEntry045.PresetFields)if(RoomAdjustmentsUi037.CellValue(r.input,f)!=null)ex.Add(f);
    FastRowState045 w;if(fs.rows.TryGetValue(r.rowKey,out w)&&w.blanks!=null)foreach(var f in w.blanks)if(FastEntry045.IsPresetField(f))ex.Add(f);}
   s.fast=fs;}
  public static string KindKey(RoomCompletionRow037 r){return r==null?null:FastEntry045.KindKey(r.startType,r.endType);}
  public static object Effective(RoomSession037 s,RoomCompletionRow037 r,string field,out string source){
   var w=s.fast==null?null:FastEntry045.Row(s.fast,r.rowKey,false);
   bool ex=IsExplicit(s,r.rowKey,field);
   return FastEntry045.Resolve(s.projectDocument==null?null:s.projectDocument.fast,s.roomId,KindKey(r),w==null?null:w.presetId,ex,ex&&r.input!=null?RoomAdjustmentsUi037.CellValue(r.input,field):null,field,out source);}
  // Called by RoomUi037.Recalc (project mode) before ApplyProfile + Calculate: inherited fields of every row get their current resolved value (or null).
  public static void Resolve(RoomSession037 s){if(!Active(s))return;
   if(s.fast==null||!O.Equals(s.fast.roomId,s.roomId))Rebind(s);
   foreach(var r in s.batch.rows){if(r.input==null||r.rowKey==null)continue;
    foreach(var f in FastEntry045.PresetFields){if(IsExplicit(s,r.rowKey,f))continue;string src;RoomAdjustmentsUi037.SetCell(r.input,f,Effective(s,r,f,out src));}}}
  // Row state of the open room into doc (next to its draft). doc is the caller's copy.
  public static void PutRoom(RoomProjectDocument037 doc,RoomSession037 s){if(doc==null||s==null||s.fast==null||!O.Equals(s.fast.roomId,s.roomId))return;
   var st=FastEntry045.ToRoomState(s.fast);if(st.rows.Count==0&&doc.fast==null)return;
   if(doc.fast==null)doc.fast=new FastEntryData045();FastEntry045.PutRoomState(doc.fast,st);}
  public static void PruneRooms(RoomProjectDocument037 doc){if(doc==null||doc.fast==null||doc.model==null||doc.model.hierarchy==null)return;
   var ids=new HashSet<string>(doc.model.hierarchy.floors.Where(f=>f!=null&&f.rooms!=null).SelectMany(f=>f.rooms).Where(r=>r!=null).Select(r=>r.id),O);
   FastEntry045.PruneRooms(doc.fast,ids);}
  public static string SourceLabel(string src){
   return src==FastEntry045.SrcExplicit?"RIÊNG":src==FastEntry045.SrcRowPreset?"MẪU CỦA DÒNG":src==FastEntry045.SrcRoom?"MẶC ĐỊNH PHÒNG":src==FastEntry045.SrcKind?"MẶC ĐỊNH LOẠI THIẾT BỊ":"CHƯA CÓ";}
  public static string KindLabel(string kindKey){if(kindKey==null)return "(loại thiết bị chưa xác định)";var p=kindKey.Split('>');
   return p.Length==2?RoomProjectUi037.TypeLabel(p[0])+" → "+RoomProjectUi037.TypeLabel(p[1]):kindKey;}

  // ---- snapshots (table undo) ----
  // PATCH1 F03: conduitMode + independentAssembliesConfirmed (changed by the undoable declaration dialog, quantity-affecting) are part of the snapshot.
  public sealed class Snap {public string context,conduitMode;public bool dirty,projectDirty,independent;public FastEntryData045 project;
   public readonly Dictionary<string,object[]> values=new Dictionary<string,object[]>(StringComparer.Ordinal);
   public readonly Dictionary<string,FastRowState045> rows=new Dictionary<string,FastRowState045>(StringComparer.Ordinal);
   public readonly Dictionary<string,string[]> explicitSets=new Dictionary<string,string[]>(StringComparer.Ordinal);}
  // Context of the open room's table: any change of room, batch (rescan / new room), scan signature, base raw (save / U), adjustments object or project
  // model object (project dialog, device heights, save) makes older history unusable.
  public static string Context(RoomSession037 s){if(!Active(s))return null;
   return (s.roomId??"")+"|"+Id(s.batch)+"|"+(s.scan==null?"":s.scan.signature??"")+"|"+Id(s.projectRaw)+"|"+Id(s.adjustments)+"|"+Id(s.projectDocument.model);}
  public static Snap Take(RoomSession037 s){if(!Active(s)||s.fast==null)return null;var t=new Snap{context=Context(s),dirty=s.dirty,projectDirty=s.projectDirty,project=FastEntry045.Copy(s.projectDocument.fast),
    conduitMode=s.batch.conduitMode,independent=s.batch.independentAssembliesConfirmed};
   foreach(var r in s.batch.rows){if(r.input==null||r.rowKey==null||t.values.ContainsKey(r.rowKey))continue;
    t.values[r.rowKey]=RoomAdjustmentsUi037.CellFields.Select(f=>RoomAdjustmentsUi037.CellValue(r.input,f)).ToArray();}
   foreach(var kv in s.fast.rows)t.rows[kv.Key]=FastEntry045.Copy(kv.Value);
   foreach(var kv in s.fast.explicitFields)t.explicitSets[kv.Key]=kv.Value.OrderBy(x=>x,O).ToArray();
   return t;}
  // Restores declarations + provenance + project presets/defaults of the open room, then ONE Recalc. Refused (exception) outside its context.
  public static void Restore(RoomSession037 s,Snap t){if(t==null||!O.Equals(t.context,Context(s)))throw new IOE("Lịch sử hoàn tác không còn khớp phòng / lần quét / dữ liệu dự án hiện tại.");
   foreach(var r in s.batch.rows){object[] v;if(r.input==null||r.rowKey==null||!t.values.TryGetValue(r.rowKey,out v))continue;
    for(int k=0;k<RoomAdjustmentsUi037.CellFields.Length;k++)RoomAdjustmentsUi037.SetCell(r.input,RoomAdjustmentsUi037.CellFields[k],v[k]);}
   s.fast.rows.Clear();foreach(var kv in t.rows)s.fast.rows[kv.Key]=FastEntry045.Copy(kv.Value);
   s.fast.explicitFields.Clear();foreach(var kv in t.explicitSets)s.fast.explicitFields[kv.Key]=new HashSet<string>(kv.Value,O);
   s.batch.conduitMode=t.conduitMode;s.batch.independentAssembliesConfirmed=t.independent;   // F03: before the ONE Recalc
   s.projectDocument.fast=FastEntry045.Copy(t.project);s.dirty=t.dirty;s.projectDirty=t.projectDirty;
   RoomUi037.Recalc(s);}

  // ---- atomic bulk apply (targets fully validated by FastPlanner045 beforehand) ----
  // Every target row/field is re-checked against the live batch first; then values + RIÊNG marks are set and ONE Recalc runs. Any exception after the
  // first change restores the given snapshot (no partial change survives).
  public static void Apply(RoomSession037 s,IList<FastTarget045> targets,Snap undo){
   if(s==null||s.batch==null)throw new IOE("Chưa có phòng đang mở. Chưa đổi gì.");
   var by=new Dictionary<string,RoomCompletionRow037>(O);foreach(var r in s.batch.rows)if(r.rowKey!=null&&r.input!=null&&!by.ContainsKey(r.rowKey))by[r.rowKey]=r;
   foreach(var t in targets)if(t.rowKey==null||!by.ContainsKey(t.rowKey)||Array.IndexOf(RoomAdjustmentsUi037.CellFields,t.field)<0)
    throw new IOE("Dòng "+t.rowKey+" / cột "+t.field+" không còn nhập được; bấm 'Cập nhật'. Chưa đổi gì.");
   // Without a snapshot (legacy single-room mode) the previous values of the targets and the dirty flag are kept for the rollback.
   var old=targets.Select(t=>RoomAdjustmentsUi037.CellValue(by[t.rowKey].input,t.field)).ToList();bool wasDirty=s.dirty;
   try{foreach(var t in targets){RoomAdjustmentsUi037.SetCell(by[t.rowKey].input,t.field,t.value);MarkExplicit(s,t.rowKey,t.field);}
    s.dirty=true;RoomUi037.Recalc(s);}
   catch(Exception){
    if(undo!=null){try{Restore(s,undo);}catch(Exception){}}
    else{for(int k=targets.Count-1;k>=0;k--)RoomAdjustmentsUi037.SetCell(by[targets[k].rowKey].input,targets[k].field,old[k]);s.dirty=wasDirty;try{RoomUi037.Recalc(s);}catch(Exception){}}
    throw;}}
 }

 public sealed partial class RoomReport037 {
  public const string NeedsFilter045="CAN_XU_LY";
  // 045 strip (TUYẾN tab). Preset controls are project-mode only; range / clipboard work in both modes.
  public readonly ComboBox PresetBox=new ComboBox();
  public readonly Button ApplyPresetBtn=new Button(),RoomDefaultBtn=new Button(),KindDefaultBtn=new Button(),PresetsBtn=new Button(),ClearOverrideBtn=new Button(),
   PasteBtn=new Button(),FillBtn=new Button(),UndoBtn=new Button(),RedoBtn=new Button();
  public readonly CheckBox AllColumnsBox=new CheckBox(),DetailToggle=new CheckBox();
  public readonly Label FastHint=new Label();
  // Clipboard access (only from an explicit key / button). Replaceable by QA; defaults use the Windows clipboard with bounded errors.
  public Func<string> ClipboardRead;public Action<string> ClipboardWrite;
  // Invalid paste: shown in an editable correction box (default dialog) so nothing typed in Excel is lost; QA may set false (no dialog).
  public bool PasteFixEnabled=true;public Func<string,string,string> PasteFix;
  public string LastBulkError,LastPasteText;
  TableLayoutPanel layout045;FlowLayoutPanel fastBar045;string presetKey045;
  // Compact columns: STT, TUYẾN, KÊNH / LỘ, TỔNG TUYẾN, BỘ DÂY, TRẠNG THÁI, CẦN BỔ SUNG; the rest with CỘT NHẬP ĐẦY ĐỦ (logical indexes never change).
  static readonly int[] CompactCols={0,1,2,10,11,13,14};

  Control TopStrip045(Control exBar){
   fastBar045=Strip("room045_fast_bar");
   PresetBox.Name="room045_preset";PresetBox.AccessibleName="Mẫu nhập";PresetBox.DropDownStyle=ComboBoxStyle.DropDownList;PresetBox.Width=S(170);PresetBox.Margin=new Padding(S(4),S(5),S(4),0);
   fastBar045.Controls.Add(new Label{Text="MẪU",AutoSize=true,Margin=new Padding(S(4),S(8),0,0),ForeColor=BdgklLook034.Muted});fastBar045.Controls.Add(PresetBox);
   Action<Button,string,string,string> b=(x,text,name,tip)=>{x.Text=text;x.Name=name;x.AccessibleName=text;BdgklLook034.Style(x,false,face,S(64),S(28));x.Margin=new Padding(S(3),S(3),0,S(3));
    tip037.SetToolTip(x,tip);fastBar045.Controls.Add(x);};
   b(ApplyPresetBtn,"ÁP MẪU CHO DÒNG CHỌN","room045_apply_preset","Gán mẫu đang chọn cho các dòng của vùng chọn (giá trị RIÊNG của dòng giữ nguyên). '(không mẫu)' = bỏ mẫu của dòng.");
   b(RoomDefaultBtn,"MẶC ĐỊNH PHÒNG","room045_room_default","Chọn mẫu đang chọn làm mặc định của phòng đang mở: mọi dòng (cả lần quét sau) kế thừa nếu dòng không có giá trị RIÊNG / mẫu riêng. '(không mẫu)' = bỏ.");
   b(KindDefaultBtn,"MẶC ĐỊNH LOẠI THIẾT BỊ","room045_kind_default","Chọn mẫu đang chọn làm mặc định cho loại tuyến (thiết bị đầu → cuối) của dòng hiện tại, trong toàn dự án. Loại chưa xác định không gán được.");
   b(PresetsBtn,"QUẢN LÝ MẪU…","room045_presets","Tạo / sửa / xoá mẫu nhập (hướng đi, đầu chờ, hệ số, bộ dây, ống). MEPS không có mẫu sẵn.");
   b(ClearOverrideBtn,"BỎ GIÁ TRỊ RIÊNG","room045_clear_override","Ô đang chọn trở lại kế thừa mẫu / mặc định (không phải về 0).");
   b(PasteBtn,"DÁN","room045_paste","Dán vùng ô chép từ Excel (Ctrl+V). Kiểm tra toàn bộ trước khi đổi; lỗi = không đổi ô nào.");
   b(FillBtn,"ĐIỀN XUỐNG","room045_fill","Chép ô trên cùng của vùng chọn xuống các ô bên dưới trong cùng cột (Ctrl+D).");
   b(UndoBtn,"HOÀN TÁC","room045_undo","Hoàn tác thay đổi bảng gần nhất (Ctrl+Z khi không sửa ô). Không phải lệnh U của bản vẽ.");
   b(RedoBtn,"LÀM LẠI","room045_redo","Làm lại thay đổi bảng vừa hoàn tác (Ctrl+Y).");
   AllColumnsBox.Text="CỘT NHẬP ĐẦY ĐỦ";AllColumnsBox.Name="room045_all_columns";AllColumnsBox.AutoSize=true;AllColumnsBox.Margin=new Padding(S(8),S(7),0,0);fastBar045.Controls.Add(AllColumnsBox);
   DetailToggle.Text="CHI TIẾT";DetailToggle.Name="room045_detail_toggle";DetailToggle.AutoSize=true;DetailToggle.Margin=new Padding(S(8),S(7),0,0);fastBar045.Controls.Add(DetailToggle);
   FastHint.Name="room045_hint";FastHint.AutoSize=true;FastHint.ForeColor=BdgklLook034.Muted;FastHint.Margin=new Padding(S(8),S(8),0,0);
   FastHint.Text="Kéo chọn vùng ô · Ctrl+C / Ctrl+V như Excel · Ctrl+D điền xuống · Ctrl+Z / Ctrl+Y hoàn tác bảng · chữ xám = giá trị kế thừa";fastBar045.Controls.Add(FastHint);
   foreach(var c in new Control[]{PresetBox,ApplyPresetBtn,RoomDefaultBtn,KindDefaultBtn,PresetsBtn,ClearOverrideBtn,UndoBtn,RedoBtn})c.Visible=Session.projectMode;
   ApplyPresetBtn.Click+=(s,e)=>ApplyPresetToSelection(SelectedPresetId045());
   RoomDefaultBtn.Click+=(s,e)=>SetRoomDefault(SelectedPresetId045());
   KindDefaultBtn.Click+=(s,e)=>{var r=SelectedRow();if(r==null){SetStatus("Chọn MỘT dòng để lấy loại thiết bị (đầu → cuối) cho mặc định.",true);return;}
    SetKindDefault(RoomFastEntryUi045.KindKey(r),SelectedPresetId045());};
   PresetsBtn.Click+=(s,e)=>EditPresets045();ClearOverrideBtn.Click+=(s,e)=>ClearOverrides();
   PasteBtn.Click+=(s,e)=>DoPaste045();FillBtn.Click+=(s,e)=>FillDown();UndoBtn.Click+=(s,e)=>UndoTable();RedoBtn.Click+=(s,e)=>RedoTable();
   AllColumnsBox.CheckedChanged+=(s,e)=>Columns045(AllColumnsBox.Checked);DetailToggle.CheckedChanged+=(s,e)=>Detail045(DetailToggle.Checked);
   var t=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=1,RowCount=2,AutoSize=true,Margin=new Padding(0),Padding=new Padding(0),BackColor=BdgklLook034.Back,Name="room045_top"};
   t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));t.RowStyles.Add(new RowStyle(SizeType.AutoSize));t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
   t.Controls.Add(fastBar045,0,0);t.Controls.Add(exBar,0,1);return t;}

  void Init045(){
   RouteGrid.SelectionMode=DataGridViewSelectionMode.CellSelect;RouteGrid.MultiSelect=true;RouteGrid.ClipboardCopyMode=DataGridViewClipboardCopyMode.Disable;
   // Ctrl shortcuts only when no cell editor is open (an open editor receives the keys itself: its own text undo / copy / paste stay native).
   RouteGrid.KeyDown+=(s,e)=>{if(RouteGrid.IsCurrentCellInEditMode||!e.Control||e.Alt)return;bool h=true;
    switch(e.KeyCode){case Keys.C:CopySelection045();break;case Keys.V:DoPaste045();break;case Keys.D:FillDown();break;case Keys.Z:if(e.Shift)RedoTable();else UndoTable();break;
     case Keys.Y:RedoTable();break;default:h=false;break;}
    if(h){e.Handled=true;e.SuppressKeyPress=true;}};
   Columns045(false);Detail045(false);}
  // Compact / all columns (Visible only; DisplayIndex and logical indexes unchanged).
  public void Columns045(bool all){foreach(DataGridViewColumn c in RouteGrid.Columns)c.Visible=all||Array.IndexOf(CompactCols,c.Index)>=0;
   if(AllColumnsBox.Checked!=all)AllColumnsBox.Checked=all;}
  public void Detail045(bool show){DetailBox.Visible=show;if(layout045!=null&&layout045.RowStyles.Count>5)layout045.RowStyles[5]=show?new RowStyle(SizeType.Percent,40):new RowStyle(SizeType.Absolute,0);
   if(DetailToggle.Checked!=show)DetailToggle.Checked=show;}

  // ---- rows / filter / painting ----
  bool HasRaw045(RoomCompletionRow037 r){if(RawCellText==null||r==null||r.rowKey==null)return false;
   foreach(var f in RouteFields)if(f!=null&&RawCellText(r.rowKey,f)!=null)return true;return false;}
  bool Shown045(RoomCompletionRow037 r,string f){if(f==null)return true;
   if(f==NeedsFilter045)return (r.status!=RoomCompletion037.Pass&&r.status!=RoomCompletion037.Excluded)||HasRaw045(r);
   return r.status==f;}
  void Paint045(DataGridViewRow row,RoomCompletionRow037 r){
   var st=row.Cells[13];
   if(HasRaw045(r)){st.Value="CHỜ ÁP DỤNG Ô NHÁP";st.Style.ForeColor=BdgklLook034.Error;
    st.ToolTipText="Dòng còn ô nhập nháp chưa áp dụng: kết quả của dòng chưa chính thức, chưa xuất Excel được (không hiện ĐỦ).";}
   else{st.Style.ForeColor=Color.Empty;st.ToolTipText="";}   // corrected: row colours / text of PaintRoute again
   if(!RoomFastEntryUi045.Active(Session)||r.input==null||r.rowKey==null)return;
   for(int c=0;c<RouteFields.Length;c++){var f=RouteFields[c];if(!FastEntry045.IsPresetField(f))continue;
    if(RawCellText!=null&&RawCellText(r.rowKey,f)!=null){row.Cells[c].ToolTipText="";continue;}
    string src;var v=RoomFastEntryUi045.Effective(Session,r,f,out src);var cell=row.Cells[c];
    if(src==FastEntry045.SrcExplicit){cell.ToolTipText="RIÊNG: giá trị của chính dòng này (BỎ GIÁ TRỊ RIÊNG để kế thừa lại).";continue;}
    if(v==null){cell.ToolTipText="CHƯA CÓ: dòng không có giá trị riêng, mẫu hay mặc định cho ô này.";continue;}
    cell.Style.ForeColor=BdgklLook034.Muted;cell.ToolTipText="KẾ THỪA từ "+RoomFastEntryUi045.SourceLabel(src)+" ("+PresetName045(Source045Preset(r,src))+"). Gõ / dán vào ô = giá trị RIÊNG.";}}
  string Source045Preset(RoomCompletionRow037 r,string src){var d=Session.projectDocument==null?null:Session.projectDocument.fast;
   if(src==FastEntry045.SrcRowPreset){var w=Session.fast==null?null:FastEntry045.Row(Session.fast,r.rowKey,false);return w==null?null:w.presetId;}
   if(src==FastEntry045.SrcRoom)return FastEntry045.DefaultOf(d,FastEntry045.Room,Session.roomId);
   if(src==FastEntry045.SrcKind)return FastEntry045.DefaultOf(d,FastEntry045.Kind,RoomFastEntryUi045.KindKey(r));return null;}
  string PresetName045(string id){var p=FastEntry045.Preset(Session.projectDocument==null?null:Session.projectDocument.fast,id);return p==null?"—":p.name;}
  string Provenance045(RoomCompletionRow037 r){if(!RoomFastEntryUi045.Active(Session)||r.input==null)return "";
   var w=Session.fast==null?null:FastEntry045.Row(Session.fast,r.rowKey,false);var d=Session.projectDocument.fast;var kind=RoomFastEntryUi045.KindKey(r);
   var b=new StringBuilder("\r\nNGUỒN GIÁ TRỊ (RIÊNG > mẫu của dòng > mặc định phòng > mặc định loại thiết bị): mẫu của dòng ").Append(PresetName045(w==null?null:w.presetId))
    .Append(" · mặc định phòng ").Append(PresetName045(FastEntry045.DefaultOf(d,FastEntry045.Room,Session.roomId)))
    .Append(" · loại ").Append(RoomFastEntryUi045.KindLabel(kind)).Append(" → ").Append(PresetName045(FastEntry045.DefaultOf(d,FastEntry045.Kind,kind))).Append("\r\n");
   foreach(var f in FastEntry045.PresetFields){string src;var v=RoomFastEntryUi045.Effective(Session,r,f,out src);
    b.Append(RoomAdjustmentsUi037.Caption(f)).Append(" = ").Append(v==null?"(trống)":RoomFastEntryUi045.Text(f,v)).Append(" [").Append(RoomFastEntryUi045.SourceLabel(src)).Append("]  ");}
   return b.ToString();}
  // Current row only when the whole selection lies in it (never one row standing for a multi-row range).
  RoomCompletionRow037 SingleRow045(){var cur=RouteGrid.CurrentRow;if(cur==null||cur.Index<0)return null;
   foreach(DataGridViewCell c in RouteGrid.SelectedCells)if(c.RowIndex!=cur.Index)return null;return cur.Tag as RoomCompletionRow037;}
  void FocusRow045(DataGridViewRow row){if(row==null)return;
   var c=row.Cells.Cast<DataGridViewCell>().Where(x=>x.Visible).OrderBy(x=>x.OwningColumn.DisplayIndex).FirstOrDefault();if(c==null)return;
   RouteGrid.ClearSelection();RouteGrid.CurrentCell=c;c.Selected=true;}
  string RangeText045(){var sel=Selected045();if(sel.Count<2)return null;int r0,c0,r1,c1;string e;
   if(!FastEntry045.Rectangle(sel,out r0,out c0,out r1,out c1,out e))return "Vùng chọn KHÔNG LIỀN: dán / điền xuống cần một khối chữ nhật liền như Excel.";
   return "Đã chọn "+(r1-r0+1).ToString(CultureInfo.InvariantCulture)+" dòng × "+(c1-c0+1).ToString(CultureInfo.InvariantCulture)+" cột. Ctrl+C chép · Ctrl+V dán · Ctrl+D điền xuống · ÁP MẪU CHO DÒNG CHỌN · BỎ GIÁ TRỊ RIÊNG.";}
  void Buttons045(){if(fastBar045==null)return;bool proj=RoomFastEntryUi045.Active(Session);RefreshPresets045();
   var h=Session.fast==null?null:Session.fast.history;string ctx=RoomFastEntryUi045.Context(Session);bool same=h!=null&&ctx!=null&&string.Equals(h.Context,ctx,StringComparison.Ordinal);
   UndoBtn.Enabled=proj&&same&&h.UndoCount>0;RedoBtn.Enabled=proj&&same&&h.RedoCount>0;
   ApplyPresetBtn.Enabled=RoomDefaultBtn.Enabled=ClearOverrideBtn.Enabled=proj;KindDefaultBtn.Enabled=proj&&RoomFastEntryUi045.KindKey(SingleRow045())!=null;
   PresetsBtn.Enabled=Session.projectMode&&Session.projectDocument!=null;PasteBtn.Enabled=FillBtn.Enabled=Session.batch!=null;}
  void RefreshPresets045(){var d=Session.projectDocument==null?null:Session.projectDocument.fast;
   var list=d==null||d.presets==null?new List<FastPreset045>():d.presets.Where(p=>p!=null).ToList();
   var key=string.Join("\u001f",list.Select(p=>p.id+"\u001e"+p.name));if(string.Equals(key,presetKey045,StringComparison.Ordinal))return;
   var keep=SelectedPresetId045();presetKey045=key;PresetBox.Items.Clear();PresetBox.Items.Add(new RouteDialog034.Choice(null,"(không mẫu)"));int sel=0;
   foreach(var p in list){PresetBox.Items.Add(new RouteDialog034.Choice(p.id,p.name));if(keep!=null&&string.Equals(keep,p.id,StringComparison.Ordinal))sel=PresetBox.Items.Count-1;}
   PresetBox.SelectedIndex=sel;}
  string SelectedPresetId045(){var c=PresetBox.SelectedItem as RouteDialog034.Choice;return c==null?null:c.value;}
  // QA: choose a preset in the strip by ID (null = '(không mẫu)'); false = not listed.
  public bool ChoosePreset(string id){RefreshPresets045();for(int i=0;i<PresetBox.Items.Count;i++){var c=PresetBox.Items[i] as RouteDialog034.Choice;
    if(c!=null&&string.Equals(c.value,id,StringComparison.Ordinal)){PresetBox.SelectedIndex=i;return true;}}return false;}

  // ---- visible grid model ----
  List<DataGridViewColumn> VisibleCols045(){return RouteGrid.Columns.Cast<DataGridViewColumn>().Where(c=>c.Visible).OrderBy(c=>c.DisplayIndex).ToList();}
  List<int[]> Selected045(){var cols=VisibleCols045();var pos=new Dictionary<int,int>();for(int k=0;k<cols.Count;k++)pos[cols[k].Index]=k;var l=new List<int[]>();
   foreach(DataGridViewCell c in RouteGrid.SelectedCells){int p;if(c.RowIndex>=0&&pos.TryGetValue(c.ColumnIndex,out p))l.Add(new[]{c.RowIndex,p});}
   if(l.Count==0&&RouteGrid.CurrentCell!=null){int p;if(pos.TryGetValue(RouteGrid.CurrentCell.ColumnIndex,out p))l.Add(new[]{RouteGrid.CurrentCell.RowIndex,p});}
   return l;}
  FastGrid045 Grid045(){var g=new FastGrid045();var cols=VisibleCols045();
   foreach(DataGridViewRow row in RouteGrid.Rows){var r=row.Tag as RoomCompletionRow037;g.rowKeys.Add(r==null?null:r.rowKey);g.rowEditable.Add(r!=null&&r.input!=null&&r.rowKey!=null);
    g.rowLabels.Add("STT "+Convert.ToString(row.Cells[0].Value,CultureInfo.InvariantCulture)+(r==null?"":" ("+(r.startLabel??"?")+" → "+(r.endLabel??"?")+")"));}
   foreach(var c in cols){g.fields.Add(RouteFields[c.Index]);g.captions.Add(c.HeaderText);}
   g.text=(r,c)=>{if(r<0||r>=RouteGrid.Rows.Count||c<0||c>=cols.Count)return null;return Convert.ToString(RouteGrid.Rows[r].Cells[cols[c].Index].Value,CultureInfo.InvariantCulture)??"";};
   return g;}
  string Where045(FastGrid045 g,int r,int c){return (r>=0&&r<g.Rows?g.rowLabels[r]:"dòng thứ "+(r+1).ToString(CultureInfo.InvariantCulture)+" (ngoài bảng đang hiện)")+
   ", cột "+(c>=0&&c<g.Cols?g.captions[c]:"thứ "+(c+1).ToString(CultureInfo.InvariantCulture)+" (ngoài bảng đang hiện)");}
  string Explain045(FastGrid045 g,FastPlan045 p){
   switch(p.error){
    case "SELECTION_EMPTY":return "Chưa chọn ô nào trong bảng TUYẾN.";
    case "SELECTION_NOT_RECTANGLE":return "Vùng chọn không liền: chọn một khối ô chữ nhật liền (như Excel).";
    case "CLIPBOARD_EMPTY":return "Clipboard không có chữ để dán.";
    case "CLIPBOARD_RAGGED":return "Dữ liệu dán có số cột khác nhau giữa các dòng; chép lại một vùng chữ nhật từ Excel.";
    case "CLIPBOARD_TOO_LARGE":return "Dữ liệu dán quá lớn.";
    case "SIZE_MISMATCH":return "Kích thước dữ liệu dán khác vùng chọn ("+p.detail+" dòng×cột): chọn MỘT ô đầu, hoặc đúng vùng cùng kích thước.";
    case "OUT_OF_BOUNDS":return "Dữ liệu dán vượt ra ngoài các dòng / cột đang hiện tại "+Where045(g,p.badRow,p.badCol)+". Bật 'Tất cả' / CỘT NHẬP ĐẦY ĐỦ hoặc chọn vùng nhỏ hơn.";
    case "READ_ONLY_COLUMN":return Where045(g,p.badRow,p.badCol)+": cột đo CAD / kết quả / trạng thái / định danh, không nhập được.";
    case "READ_ONLY_ROW":return Where045(g,p.badRow,p.badCol)+": dòng không có dữ liệu nhập (bản vẽ chưa xác định tuyến).";
    case "INVALID_VALUE":return Where045(g,p.badRow,p.badCol)+": \""+p.badText+"\" — "+p.detail;
    case "FILL_NO_SOURCE":return "Không có ô nguồn phía trên để điền xuống ("+Where045(g,p.badRow,p.badCol)+").";
    case "FILL_SOURCE_PENDING":return "Ô nguồn "+Where045(g,p.badRow,p.badCol)+" còn nhập nháp chưa áp dụng; sửa ô đó trước.";
    default:return p.error+(p.detail==null?"":" "+p.detail);}}

  // ---- operations (buttons, keys and QA call exactly these) ----
  bool Bulk045(FastPlan045 p,FastGrid045 g,string what){
   if(!p.Ok){LastBulkError=Explain045(g,p);SetStatus("Chưa "+what+": "+LastBulkError+" Không ô nào thay đổi.",true);Detail();return false;}
   if(p.targets.Count==0){LastBulkError="Không có ô đích.";SetStatus("Chưa "+what+": không có ô đích.",true);return false;}
   var before=Snapshot045();
   var raw=new List<string[]>();if(RawCellText!=null)foreach(var t in p.targets)if(RawCellText(t.rowKey,t.field)!=null)raw.Add(new[]{RawDraftCodec044.Row,t.rowKey,t.field});
   try{RoomFastEntryUi045.Apply(Session,p.targets,before);}
   catch(Exception ex){LastBulkError=ex.Message;Fill();SetStatus("Chưa "+what+": "+ex.Message+" Không ô nào thay đổi.",true);return false;}
   Record045(before);LastBulkError=null;
   int rows=p.targets.Select(t=>t.rowKey).Distinct(StringComparer.Ordinal).Count();
   Fill();if(raw.Count>0&&RawDoneMany!=null)RawDoneMany(raw);
   SetStatus("Đã "+what+" "+p.targets.Count.ToString(CultureInfo.InvariantCulture)+" ô trên "+rows.ToString(CultureInfo.InvariantCulture)+" dòng (giá trị RIÊNG; Ctrl+Z hoàn tác). Chưa lưu vào bản vẽ. "+RoomUi037.Summary(Session)+".",false);
   if(Accepted!=null)Accepted();return true;}
  // Paste TSV text (the Ctrl+V / DÁN path after reading the clipboard). Whole target validated first; refusal = nothing changed.
  public bool PasteText(string tsv){LastPasteText=tsv;if(!CommitEdits())return false;
   if(Session.batch==null){SetStatus("Chưa có phòng đang mở; chưa dán.",true);return false;}
   var g=Grid045();string err;var clip=FastEntry045.ParseTsv(tsv,out err);
   var p=err!=null?new FastPlan045{error=err}:FastPlanner045.Paste(g,Selected045(),clip,(f,t)=>RoomAdjustmentsUi037.ParseCell(f,t));
   return Bulk045(p,g,"dán");}
  public bool FillDown(){if(!CommitEdits())return false;if(Session.batch==null){SetStatus("Chưa có phòng đang mở.",true);return false;}
   var g=Grid045();var cols=VisibleCols045();
   var p=FastPlanner045.FillDown(g,Selected045(),(f,t)=>RoomAdjustmentsUi037.ParseCell(f,t),(r,c)=>RawCellText!=null&&g.rowKeys[r]!=null&&g.fields[c]!=null&&RawCellText(g.rowKeys[r],g.fields[c])!=null);
   return Bulk045(p,g,"điền xuống");}
  // Copied TSV (also written to the clipboard by Ctrl+C / no button needed); null = refused.
  public string CopySelection045(){var g=Grid045();string err;var t=FastPlanner045.Copy(g,Selected045(),out err);
   if(t==null){SetStatus("Chưa chép: "+Explain045(g,new FastPlan045{error=err}),true);return null;}
   try{(ClipboardWrite??DefaultWrite045)(t);SetStatus("Đã chép vùng chọn (dán được vào Excel).",false);}
   catch(Exception ex){SetStatus("Không ghi được clipboard ("+ex.Message+"); chưa chép.",true);return null;}
   return t;}
  void DoPaste045(){string t;
   try{t=(ClipboardRead??DefaultRead045)();}catch(Exception ex){SetStatus("Không đọc được clipboard ("+ex.Message+"); chưa dán.",true);return;}
   if(t==null){SetStatus("Clipboard không có chữ để dán.",true);return;}
   for(int attempt=0;attempt<10;attempt++){if(PasteText(t))return;
    if(!PasteFixEnabled||LastBulkError==null||t==null)return;
    var fix=(PasteFix??DefaultFix045)(LastBulkError,t);if(fix==null){SetStatus("Đã huỷ dán; không ô nào thay đổi. Lỗi: "+LastBulkError,true);return;}t=fix;}}
  static string DefaultRead045(){return Clipboard.ContainsText(TextDataFormat.UnicodeText)?Clipboard.GetText(TextDataFormat.UnicodeText):null;}
  static void DefaultWrite045(string t){Clipboard.SetText(t,TextDataFormat.UnicodeText);}
  string DefaultFix045(string error,string text){
   using(var f=new Form{Text="Sửa dữ liệu dán — chưa ô nào thay đổi",FormBorderStyle=FormBorderStyle.Sizable,MinimizeBox=false,MaximizeBox=false,ShowInTaskbar=false,
    StartPosition=FormStartPosition.CenterParent,ClientSize=new Size(S(640),S(360)),Font=face,BackColor=BdgklLook034.Back,Name="room045_paste_fix"}){
    var msg=new TextBox{Multiline=true,ReadOnly=true,Dock=DockStyle.Top,Height=S(64),Text=error,ForeColor=BdgklLook034.Error,BorderStyle=BorderStyle.None,BackColor=BdgklLook034.Back,Name="room045_paste_fix_error"};
    var box=new TextBox{Multiline=true,AcceptsTab=true,AcceptsReturn=true,ScrollBars=ScrollBars.Both,WordWrap=false,Dock=DockStyle.Fill,Text=text,Name="room045_paste_fix_text"};
    var bar=new FlowLayoutPanel{Dock=DockStyle.Bottom,FlowDirection=System.Windows.Forms.FlowDirection.RightToLeft,Height=S(40)};
    var ok=new Button{Text="DÁN LẠI",DialogResult=DialogResult.OK};var cancel=new Button{Text="HUỶ",DialogResult=DialogResult.Cancel};
    BdgklLook034.Style(ok,true,face,S(80),S(28));BdgklLook034.Style(cancel,false,face,S(80),S(28));bar.Controls.Add(cancel);bar.Controls.Add(ok);
    f.Controls.Add(box);f.Controls.Add(msg);f.Controls.Add(bar);f.AcceptButton=null;f.CancelButton=cancel;
    return f.ShowDialog(this)==DialogResult.OK?box.Text:null;}}

  // ---- PATCH1/PATCH2 F02: actual edit intent of a TUYẾN cell ----
  // Set only for a DIRTY editor (text typed / item chosen; QA StartEdit marks it dirty the same way): CommitEdits before its EndEdit (PATCH2: EndEdit
  // raises no CellValidating), CellParsing (the grid pushes a dirty value: Enter / Tab / click), CellValidating (dirty + same text). Cleared at
  // CellBeginEdit and when EndEdit is refused; consumed (and cleared) by every RouteGrid CellEndEdit. Focus / leave / CancelEdit never set it
  // (not dirty, nothing pushed). RouteEndEditCore uses it only for same text over an INHERITED cell. No scan, no recalculation on a keystroke.
  int typedRow045=-1,typedCol045=-1;
  void SameTyped045(int row,int col){typedRow045=row;typedCol045=col;}
  void ClearTyped045(){typedRow045=typedCol045=-1;}
  bool TakeTyped045(int row,int col){bool hit=row>=0&&row==typedRow045&&col==typedCol045;typedRow045=typedCol045=-1;return hit;}

  // ---- table undo / redo ----
  RoomFastEntryUi045.Snap Snapshot045(){return RoomFastEntryUi045.Take(Session);}
  void Record045(RoomFastEntryUi045.Snap before){if(before==null||Session.fast==null)return;Session.fast.history.Record(before.context,before);}
  public bool UndoTable(){return History045(true);}
  public bool RedoTable(){return History045(false);}
  bool History045(bool undo){if(!CommitEdits())return false;
   if(!RoomFastEntryUi045.Active(Session)||Session.fast==null){SetStatus("Hoàn tác bảng chỉ có khi đang mở phòng của dự án.",true);return false;}
   var ctx=RoomFastEntryUi045.Context(Session);var h=Session.fast.history;var now=Snapshot045();
   var prev=undo?h.Undo(ctx,now) as RoomFastEntryUi045.Snap:h.Redo(ctx,now) as RoomFastEntryUi045.Snap;
   if(prev==null){SetStatus(undo?"Không còn thay đổi bảng để hoàn tác (lịch sử chỉ giữ trong phòng / lần quét / dữ liệu dự án hiện tại).":"Không có thay đổi bảng để làm lại.",true);Buttons045();return false;}
   try{RoomFastEntryUi045.Restore(Session,prev);}
   catch(Exception ex){if(undo)h.UndoFailed(prev);else h.RedoFailed(prev);try{RoomFastEntryUi045.Restore(Session,now);}catch(Exception){}Fill();SetStatus("Chưa "+(undo?"hoàn tác":"làm lại")+": "+ex.Message,true);return false;}
   Fill();SetStatus((undo?"Đã hoàn tác":"Đã làm lại")+" thay đổi bảng (khai báo, nguồn kế thừa, mẫu và mặc định; không đổi hình CAD). Chưa lưu vào bản vẽ. "+RoomUi037.Summary(Session)+".",false);
   if(Accepted!=null)Accepted();return true;}

  // ---- presets / defaults / RIÊNG ----
  bool Fast045(string what,Func<string> change){if(!CommitEdits())return false;
   if(!Session.projectMode||Session.projectDocument==null){SetStatus("Mẫu / mặc định chỉ dùng trong dự án (bấm 'DỰ ÁN / TẦNG / PHÒNG').",true);return false;}
   var before=Snapshot045();var keepDoc=Session.projectDocument.fast;bool keepProject=Session.projectDirty,keepDirty=Session.dirty;
   string m;
   try{m=change();if(m==null)return false;if(Session.batch!=null)RoomUi037.Recalc(Session);}
   catch(Exception ex){if(before!=null){try{RoomFastEntryUi045.Restore(Session,before);}catch(Exception){}}else{Session.projectDocument.fast=keepDoc;Session.projectDirty=keepProject;Session.dirty=keepDirty;}
    Fill();SetStatus("Chưa "+what+": "+ex.Message+" Không đổi gì.",true);return false;}
   Record045(before);Fill();SetStatus(m+" Chưa lưu vào bản vẽ."+(Session.batch!=null?" "+RoomUi037.Summary(Session)+".":""),false);if(Accepted!=null)Accepted();return true;}
  FastEntryData045 WorkingFast045(){return FastEntry045.Copy(Session.projectDocument.fast)??new FastEntryData045();}
  void CommitFast045(FastEntryData045 d){var m=Session.projectDocument.model;
   var ids=new HashSet<string>(m==null||m.hierarchy==null||m.hierarchy.floors==null?Enumerable.Empty<string>():
    m.hierarchy.floors.Where(f=>f!=null&&f.rooms!=null).SelectMany(f=>f.rooms).Where(r=>r!=null).Select(r=>r.id),StringComparer.Ordinal);
   var p=FastEntry045.Problem(d,ids,RoomFastEntryUi045.ValueProblem);if(p!=null)throw new IOE("dữ liệu mẫu không hợp lệ ("+p+")");
   Session.projectDocument.fast=d;Session.projectDirty=true;}
  // Row preset for every row of the selection (visible rows only). null = remove the row preset. RIÊNG values are not touched.
  public bool ApplyPresetToSelection(string presetId){return Fast045("áp mẫu",()=>{
   if(Session.batch==null||Session.fast==null)throw new IOE("chưa có phòng đang mở");
   if(presetId!=null&&FastEntry045.Preset(Session.projectDocument.fast,presetId)==null)throw new IOE("mẫu không còn trong dự án");
   var keys=Selected045().Select(x=>x[0]).Distinct().Where(i=>i>=0&&i<RouteGrid.Rows.Count).Select(i=>RouteGrid.Rows[i].Tag as RoomCompletionRow037)
    .Where(r=>r!=null&&r.rowKey!=null&&r.input!=null).Select(r=>r.rowKey).Distinct(StringComparer.Ordinal).ToList();
   if(keys.Count==0)throw new IOE("chọn ít nhất một ô của dòng cần áp mẫu");
   foreach(var k in keys)FastEntry045.Row(Session.fast,k,true).presetId=presetId;Session.dirty=true;
   return (presetId==null?"Đã bỏ mẫu của ":"Đã áp mẫu "+PresetName045(presetId)+" cho ")+keys.Count.ToString(CultureInfo.InvariantCulture)+" dòng (giá trị RIÊNG giữ nguyên).";});}
  public bool SetRoomDefault(string presetId){return Fast045("chọn mặc định phòng",()=>{
   if(Session.roomId==null||Session.batch==null)throw new IOE("chưa mở phòng (bấm 'Chọn vùng CAD')");
   var d=WorkingFast045();if(presetId!=null&&FastEntry045.Preset(d,presetId)==null)throw new IOE("mẫu không còn trong dự án");
   FastEntry045.SetDefault(d,FastEntry045.Room,Session.roomId,presetId);CommitFast045(d);
   return presetId==null?"Đã bỏ mặc định của phòng "+RoomProjectUi037.RoomLabel(Session)+".":"Mặc định của phòng "+RoomProjectUi037.RoomLabel(Session)+" = mẫu "+PresetName045(presetId)+" (cả các lần quét sau).";});}
  public bool SetKindDefault(string kindKey,string presetId){return Fast045("chọn mặc định loại thiết bị",()=>{
   if(kindKey==null)throw new IOE("loại thiết bị (đầu → cuối) của dòng chưa xác định; MEPS không đoán");
   var d=WorkingFast045();if(presetId!=null&&FastEntry045.Preset(d,presetId)==null)throw new IOE("mẫu không còn trong dự án");
   FastEntry045.SetDefault(d,FastEntry045.Kind,kindKey,presetId);CommitFast045(d);
   return presetId==null?"Đã bỏ mặc định của loại "+RoomFastEntryUi045.KindLabel(kindKey)+".":"Mặc định của loại "+RoomFastEntryUi045.KindLabel(kindKey)+" = mẫu "+PresetName045(presetId)+" (toàn dự án).";});}
  // Selected cells of inheritable columns go back to inheritance (never to 0). Other selected columns are ignored and said so.
  public bool ClearOverrides(){return Fast045("bỏ giá trị riêng",()=>{
   if(Session.batch==null||Session.fast==null)throw new IOE("chưa có phòng đang mở");
   var g=Grid045();int n=0,skipped=0;
   foreach(var x in Selected045()){var k=g.rowKeys[x[0]];var f=g.fields[x[1]];if(k==null||!FastEntry045.IsPresetField(f)){skipped++;continue;}if(RoomFastEntryUi045.ClearExplicit(Session,k,f))n++;}
   if(n==0)throw new IOE("vùng chọn không có ô RIÊNG nào ở các cột kế thừa được (HƯỚNG ĐI, ĐẦU CHỜ, HỆ SỐ, BỘ DÂY, ỐNG)");
   Session.dirty=true;
   return "Đã bỏ "+n.ToString(CultureInfo.InvariantCulture)+" giá trị RIÊNG: các ô kế thừa lại mẫu / mặc định (ô không có nguồn = CHƯA CÓ, không phải 0)."+
    (skipped>0?" "+skipped.ToString(CultureInfo.InvariantCulture)+" ô ở cột khác giữ nguyên.":"");});}
  // Replaces the preset list (ids kept for edited presets; references to removed presets are dropped, never moved to another preset).
  public bool SetPresets(IList<FastPreset045> presets){return Fast045("lưu danh sách mẫu",()=>{
   if(presets==null)throw new IOE("danh sách mẫu rỗng");var d=WorkingFast045();
   d.presets=presets.Select(FastEntry045.Copy).ToList();
   foreach(var p in d.presets)if(p!=null&&string.IsNullOrEmpty(p.id))p.id="P"+Guid.NewGuid().ToString("N");
   FastEntry045.PruneReferences(d);CommitFast045(d);
   if(Session.fast!=null){var ids=new HashSet<string>(d.presets.Select(p=>p.id),StringComparer.Ordinal);foreach(var w in Session.fast.rows.Values)if(w.presetId!=null&&!ids.Contains(w.presetId))w.presetId=null;}
   return "Đã cập nhật "+d.presets.Count.ToString(CultureInfo.InvariantCulture)+" mẫu nhập (dòng / mặc định dùng mẫu đã xoá trở về không mẫu).";});}
  void EditPresets045(){if(!CommitEdits())return;
   if(!Session.projectMode||Session.projectDocument==null){SetStatus("Mẫu chỉ dùng trong dự án (bấm 'DỰ ÁN / TẦNG / PHÒNG').",true);return;}
   var cur=Session.projectDocument.fast==null?new List<FastPreset045>():Session.projectDocument.fast.presets.Select(FastEntry045.Copy).ToList();
   using(var f=new FastPresetDialog045(cur,dpi)){if(f.ShowDialog(this)!=DialogResult.OK||f.Result==null){SetStatus("Đã huỷ sửa mẫu; không đổi gì.",false);return;}SetPresets(f.Result);}}

  // ---- QA helpers (same selection the mouse makes) ----
  // Selects the rectangle of visible rows (by row key) x columns (by field / logical column index); false = a row or column is not shown.
  public bool SelectCells(IList<string> rowKeys,IList<int> columns){if(rowKeys==null||columns==null)return false;Tabs.SelectedTab=RouteTab;
   var rows=new List<DataGridViewRow>();foreach(var k in rowKeys){var row=RouteGrid.Rows.Cast<DataGridViewRow>().FirstOrDefault(x=>{var r=x.Tag as RoomCompletionRow037;return r!=null&&string.Equals(r.rowKey,k,StringComparison.Ordinal);});
    if(row==null)return false;rows.Add(row);}
   foreach(var c in columns)if(c<0||c>=RouteGrid.Columns.Count||!RouteGrid.Columns[c].Visible)return false;
   RouteGrid.ClearSelection();bool first=true;
   foreach(var row in rows)foreach(var c in columns){var cell=row.Cells[c];if(first){RouteGrid.CurrentCell=cell;first=false;}cell.Selected=true;}
   return true;}
 }

 // QUẢN LÝ MẪU: one editable grid of presets (TÊN + inheritable fields). Every value is parsed by the product cell parser on OK; nothing is preset.
 public sealed class FastPresetDialog045:Form {
  public readonly DataGridView Grid;public readonly Button AddBtn=new Button(),RemoveBtn=new Button(),OkBtn=new Button(),CancelBtn=new Button();public readonly TextBox StatusBox=new TextBox();
  public List<FastPreset045> Result;readonly float dpi;readonly Font face,bold;
  int S(int px){return Math.Max(1,(int)Math.Round(px*dpi));}
  static readonly string[] Fields=FastEntry045.PresetFields;
  public FastPresetDialog045(IList<FastPreset045> presets,float dpi){this.dpi=BdgklLook034.Dpi(dpi);face=RoomLook037.Face(this.dpi,false);bold=RoomLook037.Face(this.dpi,true);
   Text="QUẢN LÝ MẪU NHẬP";Name="room045_presets_dialog";FormBorderStyle=FormBorderStyle.Sizable;MinimizeBox=false;ShowInTaskbar=false;StartPosition=FormStartPosition.CenterParent;
   AutoScaleMode=AutoScaleMode.None;Font=face;BackColor=BdgklLook034.Back;ClientSize=new Size(S(860),S(380));MinimumSize=new Size(S(600),S(260));
   var cols=new[]{"TÊN MẪU"}.Concat(Fields.Select(RoomAdjustmentsUi037.Caption)).ToArray();
   Grid=RoomLook037.Grid("room045_presets_grid","Mẫu nhập",cols,new[]{22,12,10,12,12,16,12},-1,-1,face,bold,this.dpi);
   Grid.ReadOnly=false;Grid.EditMode=DataGridViewEditMode.EditOnKeystrokeOrF2;Grid.SelectionMode=DataGridViewSelectionMode.CellSelect;
   var old=Grid.Columns[1];var dd=new DataGridViewComboBoxColumn{HeaderText=old.HeaderText,Name=old.Name,FlatStyle=FlatStyle.Flat,SortMode=DataGridViewColumnSortMode.NotSortable};
   foreach(var it in RoomAdjustmentsUi037.RoutingLabels)dd.Items.Add(it);Grid.Columns.RemoveAt(1);Grid.Columns.Insert(1,dd);
   Grid.DataError+=(s,e)=>{e.ThrowException=false;};
   foreach(var p in presets??new List<FastPreset045>())Add(p);
   var bar=new FlowLayoutPanel{Dock=DockStyle.Bottom,FlowDirection=System.Windows.Forms.FlowDirection.RightToLeft,Height=S(44),BackColor=BdgklLook034.Back};
   foreach(var t in new[]{Tuple.Create(CancelBtn,"HUỶ"),Tuple.Create(OkBtn,"ÁP DỤNG"),Tuple.Create(RemoveBtn,"XOÁ MẪU"),Tuple.Create(AddBtn,"THÊM MẪU")}){
    t.Item1.Text=t.Item2;BdgklLook034.Style(t.Item1,t.Item1==OkBtn,face,S(84),S(28));t.Item1.Margin=new Padding(S(4),S(6),0,0);bar.Controls.Add(t.Item1);}
   BdgklLook034.ReadOnlyBox(StatusBox);StatusBox.Dock=DockStyle.Bottom;StatusBox.Height=S(44);StatusBox.Text="Ô trống = mẫu không quy định giá trị đó (dòng kế thừa tiếp mặc định khác). 0 là một giá trị.";
   Controls.Add(Grid);Controls.Add(StatusBox);Controls.Add(bar);CancelButton=CancelBtn;
   AddBtn.Click+=(s,e)=>{int k=Add(null);Grid.CurrentCell=Grid.Rows[k].Cells[0];};
   RemoveBtn.Click+=(s,e)=>{var r=Grid.CurrentRow;if(r!=null&&r.Index>=0)Grid.Rows.RemoveAt(r.Index);};
   OkBtn.Click+=(s,e)=>{if(TryAccept())DialogResult=DialogResult.OK;};CancelBtn.Click+=(s,e)=>DialogResult=DialogResult.Cancel;}
  int Add(FastPreset045 p){int k=Grid.Rows.Add();var row=Grid.Rows[k];row.Tag=p==null?null:p.id;row.Cells[0].Value=p==null?"":p.name;
   for(int c=0;c<Fields.Length;c++){var v=p==null?null:FastEntry045.Value(p,Fields[c]);row.Cells[c+1].Value=RoomFastEntryUi045.Text(Fields[c],v);}
   return k;}
  public bool TryAccept(){Grid.EndEdit();var l=new List<FastPreset045>();
   try{foreach(DataGridViewRow row in Grid.Rows){var name=Convert.ToString(row.Cells[0].Value,CultureInfo.InvariantCulture)??"";
     var p=new FastPreset045{id=row.Tag as string,name=name};
     var np=RoomProjectUi037.NameProblem(name,"Tên mẫu");if(name.Length==0||np!=null)throw new IOE("Dòng "+(row.Index+1).ToString(CultureInfo.InvariantCulture)+": "+(np??"chưa nhập tên mẫu."));
     for(int c=0;c<Fields.Length;c++){var t=Convert.ToString(row.Cells[c+1].Value,CultureInfo.InvariantCulture)??"";object v;
      try{v=RoomAdjustmentsUi037.ParseCell(Fields[c],t);}catch(IOE ex){throw new IOE("Mẫu \""+name+"\", cột "+RoomAdjustmentsUi037.Caption(Fields[c])+": "+ex.Message);}
      FastEntry045.SetValue(p,Fields[c],v);}
     if(FastEntry045.Empty(p))throw new IOE("Mẫu \""+name+"\" chưa có giá trị nào.");
     if(l.Any(x=>string.Equals(x.name,name,StringComparison.OrdinalIgnoreCase)))throw new IOE("Hai mẫu cùng tên \""+name+"\".");
     l.Add(p);}}
   catch(IOE ex){StatusBox.ForeColor=BdgklLook034.Error;StatusBox.Text=ex.Message+" Chưa áp dụng.";return false;}
   Result=l;return true;}
  protected override void Dispose(bool disposing){base.Dispose(disposing);if(disposing){face.Dispose();bold.Dispose();}}
 }
}
#endif
