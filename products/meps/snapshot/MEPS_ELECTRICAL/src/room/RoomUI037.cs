using System;using System.IO;using System.Drawing;using System.Linq;using System.Text;using System.Globalization;using System.Collections.Generic;using System.Windows.Forms;using IOE=System.InvalidOperationException;
#if ROOMCAD037 && BDGKL034
using Autodesk.AutoCAD.EditorInput;using Autodesk.AutoCAD.Geometry;using Document=Autodesk.AutoCAD.ApplicationServices.Document;using App=Autodesk.AutoCAD.ApplicationServices.Application;
using Database=Autodesk.AutoCAD.DatabaseServices.Database;
namespace Tvxd.Meps {
 // TVXD-MEPS-ROOMQTY-037 room quantity UI (Vietnamese text: /codepage:65001). Entry: DÂY DẪN → 'Bảng khối lượng điện…' (action bdgkl034-table, unchanged) →
 // RouteUi034.Run (bound document, plugin guards) → RoomUi037.Run. No new menu, queue or palette; every window is a modal of that bound command.
 //  - RoomReport037: same light resizable report as BdgklReport034 (tabs TUYẾN / DÂY / ỐNG + VƯỚNG MẮC, status filter, details, status line).
 //    Actions: Chọn vùng CAD (report closes → options dialog → two corners with the native Editor; Esc keeps the current session) / Thông số (owned
 //    declaration dialog of the selected row; in-memory) / Lưu vào bản vẽ (report closes → one guarded transaction, RoomStore037) / Xuất Excel (fresh check,
 //    SaveFileDialog, RoomXlsx037.Write new file only) / Cập nhật (read-only rescan, declarations kept by row key) / Dữ liệu cũ (report closes →
 //    BdgklUi034.Run, the legacy per-route records, untouched and never merged) / Đóng.
 //  - Nothing engineering is preset: units, tolerance, floor, mode, heights, tails, factor, assembly, conduit, conduit mode and the independent-assembly
 //    confirmation start empty; INSUNITS is only a hint; only values the user saved earlier (draft) are reloaded. Circuit = the model's prefill (structured
 //    panel/circuit, else room-local TAG channel label) and editable.
 //  - Device heights are stored per actual device (handle) and pushed to every row using that device, so one device can never carry two heights here.
 //  - externalBlockers = only native adapter diagnostics of severity BLOCKED/CHECK (UNIT_INSUNITS_MISMATCH skipped when the user confirmed the unit) plus
 //    "no input"; the composite scan/core status is NOT used (the completion model resolves governing paths, NF-037-N2).
 //  - Staleness: Save and Export re-scan (RoomCad037.IsCurrent). A saved draft whose confirmed signature differs from the current scan needs an explicit
 //    review tick + Save before Export. No automatic DWG save; no ShellExecute.
 // S3 PROJECT MODE (RoomUi037.Run always; QA sessions with projectMode=false keep the earlier single-room behaviour):
 //  - The drawing-local project (RoomProjectStore037, NOD TVXD_MEPS_PROJECT037) is read once at start: missing = no project until 'DỰ ÁN / TẦNG / PHÒNG'
 //    (RoomProjectDialog037) is applied; unreadable = projectError, nothing can overwrite it. The legacy RoomStore037 is never written in project mode
 //    (its drafts are only listed read-only in the setup dialog; no implicit migration; 'Dữ liệu cũ' unchanged).
 //  - The scan room ID is the stable RM_ room ID (rename-safe); labels show code/description, never the internal ID. Floor of every row = containing floor
 //    (stable ID); wiring ceiling = room override ?? floor value; endpoint heights = RoomProject037.Resolve (VALID → Bdgkl034 FIXED fields; MISSING/INVALID →
 //    empty, INVALID also an explicit blocker). Recomputed on every Recalc; drafts never store these snapshots or personal device heights.
 //  - Device overrides only through RoomDeviceDialog037 (actual handle + authoritative type). The row dialog leaves floor/ceiling/heights to the project.
 //  - Room switch keeps the pending declarations of the open room as a draft in the in-memory project document (or needs an explicit discard).
 //  - Save = project document (+ current room draft) in ONE RoomProjectStore037.Save with the optimistic raw read at start; also possible before the first
 //    scan. Export (S4) = RoomProjectExportUi037: scope PHÒNG ĐANG MỞ / TẦNG ĐANG CHỌN / TOÀN DỰ ÁN, every selected room rebuilt fresh from CAD with the
 //    current profile, one grouped workbook (RoomProjectExport037); persisted raw and every room scan re-checked after the path chooser.
 // S5: direct cell editing in TUYẾN (both modes) and, in project mode only, BỎ DÒNG / BỔ SUNG (RoomAdjustmentsUi037 on a copy of session.adjustments; saved
 //  in the room draft as declarations; batch.sourceSignature = scan.signature at every Recalc). See RoomReport037.
 public sealed class RoomSession037 {
  public string roomId,units,scannedAt;public double tolerance,minX,minY,maxX,maxY;public bool unitConfirmed,hasRegion;
  public RoomCadScan037 scan;public RoomCompletionBatch037 batch;
  public Dictionary<string,RoomDraftDevice037> devices=new Dictionary<string,RoomDraftDevice037>(StringComparer.Ordinal);
  public List<RoomDraftRow037> unmatched=new List<RoomDraftRow037>();   // saved declarations whose row key is not in the current scan (kept, never erased)
  public RoomDraft037 saved;public string savedRaw,loadError;public bool dirty;
  // NF-037-HEIGHT-UI: room height setup of THIS room (null = not used). Kept on Cập nhật, cleared when another room is chosen, saved with the draft.
  public RoomHeightSettings037 heights;
  // S3 project mode (see header). projectDocument = in-memory working document (authority for identities and heights); projectRaw = persisted raw it was
  // read/saved from (optimistic token); projectError = unreadable store; projectDirty = in-memory project changes not saved. roomId = stable RM_ ID.
  public bool projectMode,projectDirty;public RoomProjectDocument037 projectDocument;public string projectRaw,projectError,selectedFloorId,selectedRoomId;
  // S5: BỎ DÒNG / BỔ SUNG declarations of THIS room (session-owned deep copy; null = none). Loaded when the room is opened, kept on 'Cập nhật', replaced only
  // by RoomAdjustmentsUi037 after a successful core mutation, saved with the draft, cleared when another room is opened. Recalc hands it to the batch.
  public RoomAdjustmentsData037 adjustments;
  // TVXD-MEPS-RAW-DRAFT-044: raw (not accepted) cell texts of the session (RoomRecoveryUi044; null = no recovery). They block export, never Recalc.
  public RawOverlay044 rawOverlay;
  // TVXD-MEPS-FAST-ENTRY-045: open room's preset/explicit (RIÊNG) state + table undo history (project mode; null = not bound yet). See RoomFastEntryUi045.
  public FastSession045 fast;
  public bool NeedsReview{get{return saved!=null&&scan!=null&&!string.Equals(saved.confirmedSignature,scan.signature,StringComparison.Ordinal);}}
 }
 // One declaration-dialog result. all*: user explicitly asked to copy that field to every row of the room.
 public sealed class RoomDeclaration037 {public string floorId,circuitId,routingMode,startMode,endMode,assemblyCode,conduitCode,conduitMode;
  public double? ceilingHeight,startValue,endValue,tailEachLength,factor;public int? tailCount;public bool independent,allFloor,allCeiling,allTails,allFactor,allAssembly,allConduit;}
 // discardPending (project mode): the user explicitly accepted to drop unsaved declarations of the open room that cannot be kept as a draft.
 public sealed class RoomOptions037 {public string roomId,units;public double tolerance;public bool unitConfirmed,useSavedRegion,discardPending;}

 static class RoomLook037 {
  internal static readonly Color Missing=Color.FromArgb(0xFF,0xF3,0xD6),MissingInk=Color.FromArgb(0x8A,0x5A,0x00);
  internal static Font Face(float dpi,bool bold){return new Font("Segoe UI",12f*dpi,bold?FontStyle.Bold:FontStyle.Regular,GraphicsUnit.Pixel);}
  internal static DataGridView Grid(string name,string acc,string[] cols,int[] weights,int firstNumber,int lastNumber,Font face,Font bold,float dpi){
   Func<int,int> S=px=>Math.Max(1,(int)Math.Round(px*dpi));
   var g=new DataGridView{Name=name,AccessibleName=acc,Dock=DockStyle.Fill,ReadOnly=true,AllowUserToAddRows=false,AllowUserToDeleteRows=false,AllowUserToResizeRows=false,
    AllowUserToOrderColumns=false,RowHeadersVisible=false,SelectionMode=DataGridViewSelectionMode.FullRowSelect,MultiSelect=false,EditMode=DataGridViewEditMode.EditProgrammatically,
    AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill,BackgroundColor=BdgklLook034.Back,BorderStyle=BorderStyle.None,GridColor=BdgklLook034.Line,EnableHeadersVisualStyles=false,
    ColumnHeadersHeightSizeMode=DataGridViewColumnHeadersHeightSizeMode.AutoSize,ShowCellToolTips=true,Font=face};
   g.ColumnHeadersDefaultCellStyle.BackColor=BdgklLook034.Paper;g.ColumnHeadersDefaultCellStyle.ForeColor=BdgklLook034.Ink;g.ColumnHeadersDefaultCellStyle.Font=bold;
   g.ColumnHeadersDefaultCellStyle.WrapMode=DataGridViewTriState.True;g.DefaultCellStyle.SelectionBackColor=BdgklLook034.TealLight;g.DefaultCellStyle.SelectionForeColor=BdgklLook034.Ink;
   g.RowTemplate.Height=S(24);
   for(int i=0;i<cols.Length;i++){var c=new DataGridViewTextBoxColumn{HeaderText=cols[i],Name=name+"_c"+i,FillWeight=weights[i],MinimumWidth=S(36),SortMode=DataGridViewColumnSortMode.NotSortable};
    if(i>=firstNumber&&i<=lastNumber)c.DefaultCellStyle.Alignment=DataGridViewContentAlignment.MiddleRight;g.Columns.Add(c);}
   return g;}
  // S5: editable grid cells (pale blue); missing cells use Missing (amber); errors use BdgklLook034.Error.
  internal static readonly Color EditBack=Color.FromArgb(0xE8,0xF1,0xFB),ExcludedRow=Color.FromArgb(0xEE,0xEF,0xF1);
  internal static string Status(string s){return s==RoomCompletion037.Pass?"Đủ dữ liệu":s==RoomCompletion037.MissingData?"Thiếu dữ liệu":s==RoomCompletion037.Check?"Cần kiểm tra":
   s==RoomCompletion037.Excluded?"ĐÃ BỎ DÒNG":s==RoomAdjustments037.Inactive?"ĐÃ XOÁ (không tính)":s??"";}
  static readonly string[][] Codes={
   new[]{"CONDUIT_MODE_REQUIRED","Chưa chọn cách đi ống của phòng (ống riêng hay đi chung)"},new[]{"CONDUIT_MODE_INVALID","Cách đi ống không hợp lệ"},
   new[]{"INDEPENDENT_ASSEMBLIES_NOT_CONFIRMED","Chưa xác nhận mỗi dòng là một bộ dây độc lập"},
   new[]{"SOURCE_NOT_ASSOCIATED","Bản vẽ chưa xác định được tuyến này (TAG/thiết bị); nhập số không sửa được — xem chi tiết"},
   new[]{"DEVICE_HEIGHT_CONFLICT","Một thiết bị đang có hai cao độ khác nhau"},new[]{"DEVICE_FLOOR_CONFLICT","Một thiết bị được khai ở hai tầng"},
   new[]{"FLOOR_CEILING_CONFLICT","Một tầng đang có hai chiều cao trần khác nhau"},new[]{"SHARED_FACTOR_MISMATCH","Các dòng đi chung ống có hệ số uốn cong khác nhau"},
   new[]{"WALL_SHARED_VERTICAL_UNPROVEN","Đi tường chung ống: phần đứng trùng chưa chứng minh được"},new[]{"SHARED_OWNER_NOT_COUNTED","Dòng sở hữu đoạn ống chung chưa đủ dữ liệu"},
   new[]{"SOURCE_BLOCKERS","Bản vẽ còn vướng mắc (tab VƯỚNG MẮC)"},new[]{"EXTERNAL_BLOCKERS","Quét CAD còn vướng mắc (tab VƯỚNG MẮC)"},new[]{"EMPTY_BATCH","Không có tuyến nào trong vùng"},
   new[]{"ROW_LIMIT","Quá nhiều dòng trong một phòng"},new[]{"INTERNAL_ERROR","Lỗi tính toán nội bộ"},
   new[]{"ALL_ROWS_EXCLUDED","Mọi dòng đo CAD đã bị bỏ; phòng không có dòng đo nào không thể đủ dữ liệu"},
   new[]{"EXCLUSION_SIGNATURE_MISMATCH","Hình đã đổi sau khi bỏ dòng: XÁC NHẬN LẠI hoặc KHÔI PHỤC ở tab BỎ DÒNG"},
   new[]{"EXCLUSION_UNMATCHED","Dòng đã bỏ không còn trong bản vẽ: KHÔI PHỤC ở tab BỎ DÒNG"},
   new[]{"EXCLUDED_SOURCE_NOT_ASSOCIATED","Dòng đã bỏ vẫn có lỗi bản vẽ; bỏ dòng không sửa lỗi bản vẽ"},
   new[]{"ADJUSTMENTS_INVALID","Dữ liệu bỏ dòng / bổ sung không hợp lệ; chưa áp dụng"},
   new[]{"SUPPLEMENT_ROOM_MISMATCH","Dòng bổ sung thuộc phòng khác"},new[]{"SUPPLEMENT_KIND_MISSING","Dòng bổ sung chưa chọn LOẠI"},
   new[]{"SUPPLEMENT_CODE_MISSING","Dòng bổ sung chưa nhập VẬT LIỆU"},new[]{"SUPPLEMENT_METRES_MISSING","Dòng bổ sung chưa nhập KHỐI LƯỢNG (0 phải nhập rõ)"},
   new[]{"SUPPLEMENT_DESCRIPTION_MISSING","Dòng bổ sung chưa nhập MÔ TẢ"},new[]{"SUPPLEMENT_REASON_MISSING","Dòng bổ sung chưa nhập LÝ DO"},
   new[]{"SUPPLEMENT_OVERLAP_UNCONFIRMED","Dòng bổ sung trùng loại/vật liệu đo CAD: xem lại rồi XÁC NHẬN CỘNG THÊM"},
   new[]{"TOTAL_OVERFLOW","Tổng không hữu hạn"}};
  // Concise user text of one model/core message (software codes only in the details).
  internal static string Friendly(string t){if(string.IsNullOrEmpty(t))return "";
   foreach(var c in Codes)if(t==c[0]||t.StartsWith(c[0]+":",StringComparison.Ordinal))return c[1];
   int k=t.IndexOf("] ",StringComparison.Ordinal);return k>=0?t.Substring(k+2):t;}
  internal static string Reason(RoomCompletionRow037 r){var l=r.issues.Concat(r.missing).ToList();if(l.Count==0)return "";
   return Friendly(l[0])+(l.Count>1?" (+"+(l.Count-1).ToString(CultureInfo.InvariantCulture)+")":"");}
  internal static string Exact(double? v){return v.HasValue?Bdgkl034.N(v.Value).Replace('.',','):"";}
 }

 // ---- Report ----
 // S5 direct editing: TUYẾN cells KÊNH / LỘ, HƯỚNG ĐI, SỐ ĐẦU CHỜ, DÀI 1 ĐẦU CHỜ, HỆ SỐ UỐN CONG, BỘ DÂY, ỐNG are edited in the grid (pale blue; amber = the
 // core reports that field missing/wrong); CAD endpoints, measured lengths, results and status are read-only. CellValidating only parses (refused text keeps
 // focus + cell error + status, nothing changes); CellEndEdit applies ONE field through RoomAdjustmentsUi037.ApplyCell and the grids are repainted in place
 // (never Rows.Clear inside a cell event). Every outgoing action (Save, Export, Refresh, region, project, devices, heights, Thông số, legacy, filter,
 // adjustment buttons) first calls CommitEdits and stops when the pending cell is refused. Đóng / Esc / window close discard a pending cell edit
 // (CancelEdit) together with the whole unsaved session, as before; nothing invalid is ever written over a valid value.
 // S5 project mode only (legacy projectMode=false shows none of it): BỎ DÒNG / KHÔI PHỤC of the selected TUYẾN row (LÝ DO required), filter 'Đã bỏ dòng',
 // tab BỎ DÒNG (every exclusion record with its state; KHÔI PHỤC / XÁC NHẬN LẠI for stale or unmatched records) and tab BỔ SUNG (manual supplements: THÊM DÒNG,
 // XOÁ DÒNG / KHÔI PHỤC DÒNG, XÁC NHẬN CỘNG THÊM; totals CAD / BỔ SUNG / TỔNG per exact kind + material + unit).
 // TVXD-MEPS-FAST-ENTRY-045: partial; the Excel-like range editing, presets/defaults, table undo/redo, compact columns, CẦN XỬ LÝ filter and the
 // collapsible detail live in RoomFastEntryUI045.cs (same form, same business operations).
 public sealed partial class RoomReport037:Form {
  public enum Choice {None,Region,Save,Legacy}
  public readonly Button RegionBtn=new Button(),HeightBtn=new Button(),DeclareBtn=new Button(),SaveBtn=new Button(),ExportBtn=new Button(),RefreshBtn=new Button(),LegacyBtn=new Button(),CloseBtn=new Button();
  // S3 project mode only (HeightBtn is hidden there): DỰ ÁN / TẦNG / PHÒNG setup and per-device height overrides of the open room.
  public readonly Button ProjectBtn=new Button(),DeviceBtn=new Button();
  // S5 project mode only.
  public readonly Button ExcludeBtn=new Button(),RestoreBtn=new Button(),RestoreRecordBtn=new Button(),ReconfirmBtn=new Button(),
   AddSupplementBtn=new Button(),RemoveSupplementBtn=new Button(),RestoreSupplementBtn=new Button(),OverlapBtn=new Button();
  public readonly TextBox ReasonBox=new TextBox();public readonly Label TotalLabel=new Label();
  public readonly TabPage SupplementTab=new TabPage("BỔ SUNG"),ExclusionTab=new TabPage("BỎ DÒNG");
  public readonly DataGridView SupplementGrid,TotalGrid,ExclusionGrid;
  public readonly ComboBox StatusFilter=new ComboBox();public readonly CheckBox ReviewBox=new CheckBox();
  public readonly Label ScopeLabel=new Label(),SummaryLabel=new Label();
  public readonly TabControl Tabs=new TabControl();
  public readonly TabPage RouteTab=new TabPage("TUYẾN"),WireTab=new TabPage("DÂY"),PipeTab=new TabPage("ỐNG"),IssueTab=new TabPage("VƯỚNG MẮC");
  public readonly DataGridView RouteGrid,WireGrid,PipeGrid,IssueGrid;
  public readonly TextBox DetailBox=new TextBox(),StatusBox=new TextBox();
  public readonly RoomSession037 Session;public Choice Chosen=Choice.None;public bool ReviewConfirmed;public string SummaryText="";
  // Bound by RoomUi037.Run: read-only rescan (returns a message); export (owner → message); declaration of a row key (owner → message, null = cancelled).
  public Func<string> Rescan;public Func<IWin32Window,string> Export;public Func<string,IWin32Window,string> Declare;
  // Room height setup (owner → message, null = cancelled; in-memory until 'Lưu vào bản vẽ').
  public Func<IWin32Window,string> Heights;
  // S3: project setup / device overrides (owner → message, null = cancelled; in-memory until 'Lưu vào bản vẽ').
  public Func<IWin32Window,string> Project,Devices;
  // TVXD-MEPS-RAW-DRAFT-044 hooks (bound by RoomRecoveryUi044.Attach; all null = no recovery, behaviour as before): raw text of an open-room cell by row key /
  // supplement ID + field (null = none); raw text resolved (kind, key, field); flush of the open editor; before close / close cancelled; accepted mutation;
  // explicit discard of every raw cell. RecoveryLabel + DiscardRawBtn = footer line (hidden while empty).
  public Func<string,string,string> RawCellText,RawSupplementText;public Action<string,string,string> RawDone;public Action RawFlush,BeforeClose,CloseCancelled,Accepted,DiscardRaw;
  // 045: raw cells {kind,key,field} resolved by ONE accepted bulk change (paste / fill / undo); one coherent checkpoint (RoomRecoveryUi044.DoneMany).
  public Action<List<string[]>> RawDoneMany;
  public readonly Label RecoveryLabel=new Label();public readonly Button DiscardRawBtn=new Button();readonly FlowLayoutPanel rawLine;
  public static readonly string[] RouteColumns={"STT","TUYẾN (ĐIỂM ĐẦU → ĐIỂM CUỐI)","KÊNH / LỘ","KC MẶT BẰNG (m)","HƯỚNG ĐI","ĐỨNG (m)","SỐ ĐẦU CHỜ","DÀI 1 ĐẦU CHỜ (m)","CHỜ (m)",
   "HỆ SỐ UỐN CONG","TỔNG TUYẾN (m)","BỘ DÂY","ỐNG","TRẠNG THÁI","CẦN BỔ SUNG / LÝ DO"};
  // S5: field edited in each TUYẾN column (exact BdgklInput034 name; null = read-only). Column 4 is a drop-down (RoomAdjustmentsUi037.RoutingLabels).
  public static readonly string[] RouteFields={null,null,RoomAdjustmentsUi037.Circuit,null,RoomAdjustmentsUi037.Routing,null,RoomAdjustmentsUi037.TailCount,
   RoomAdjustmentsUi037.TailEach,null,RoomAdjustmentsUi037.Factor,null,RoomAdjustmentsUi037.Assembly,RoomAdjustmentsUi037.Conduit,null,null};
  public static readonly string[] SupplementColumns={"LOẠI","VẬT LIỆU (MÃ)","ĐƠN VỊ","KHỐI LƯỢNG (m)","LỘ (KHÔNG BẮT BUỘC)","MÔ TẢ","LÝ DO","TRÙNG ĐO CAD","TRẠNG THÁI","CẦN BỔ SUNG / LÝ DO"};
  // Column 0 is a drop-down (RoomAdjustmentsUi037.KindLabels); unit is always m (read-only).
  public static readonly string[] SupplementFields={RoomAdjustmentsUi037.SKind,RoomAdjustmentsUi037.SCode,null,RoomAdjustmentsUi037.SMetres,RoomAdjustmentsUi037.SCircuit,
   RoomAdjustmentsUi037.SDescription,RoomAdjustmentsUi037.SReason,null,null,null};
  static readonly string[] SupplementMissing={"SUPPLEMENT_KIND_MISSING","SUPPLEMENT_CODE_MISSING",null,"SUPPLEMENT_METRES_MISSING",null,"SUPPLEMENT_DESCRIPTION_MISSING","SUPPLEMENT_REASON_MISSING",null,null,null};
  readonly float dpi;readonly Font face,bold;bool filling;int lastFilter;string cellError;
  // SORT-039: a KÊNH / LỘ edit changed the presentation order; inEndEdit = inside RouteGrid.CellEndEdit (no current-cell change allowed there).
  bool reorderPending,inEndEdit;
  int S(int px){return Math.Max(1,(int)Math.Round(px*dpi));}
  public RoomReport037(RoomSession037 session,float dpi){if(session==null)throw new ArgumentNullException("session");Session=session;this.dpi=BdgklLook034.Dpi(dpi);
   Text="MEPS — "+BdgklUi034.Title+" theo phòng · build"+RoomUi037.Build;Name="room037_report";FormBorderStyle=FormBorderStyle.Sizable;MinimizeBox=false;MaximizeBox=true;ShowInTaskbar=false;
   StartPosition=FormStartPosition.CenterParent;AutoScaleMode=AutoScaleMode.None;face=RoomLook037.Face(this.dpi,false);bold=RoomLook037.Face(this.dpi,true);Font=face;
   BackColor=BdgklLook034.Back;ForeColor=BdgklLook034.Ink;ClientSize=new Size(S(1040),S(660));MinimumSize=new Size(S(760),S(500));
   var pane=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=1,RowCount=8,Padding=new Padding(S(8)),BackColor=BdgklLook034.Back};pane.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
   layout045=pane;   // 045: row 5 (detail) collapses / expands
   foreach(int h in new[]{0,0,0,-60,S(42),-40,S(44),0})pane.RowStyles.Add(h<0?new RowStyle(SizeType.Percent,-h):h==0?new RowStyle(SizeType.AutoSize):new RowStyle(SizeType.Absolute,h));
   var actions=new FlowLayoutPanel{Dock=DockStyle.Fill,WrapContents=true,AutoSize=true,AutoSizeMode=System.Windows.Forms.AutoSizeMode.GrowAndShrink,Margin=new Padding(0,0,0,S(4)),BackColor=BdgklLook034.Back,Name="room037_actions"};
   // Wrapping flow (AutoSize row): at the minimum width / any DPI the buttons move to a new line instead of being clipped.
   var names=new[]{Tuple.Create(ProjectBtn,"DỰ ÁN / TẦNG / PHÒNG","room037_project"),Tuple.Create(RegionBtn,"Chọn vùng CAD","room037_region"),
    Tuple.Create(HeightBtn,"Cao độ tầng và thiết bị","room037_heights"),Tuple.Create(DeviceBtn,"CAO ĐỘ THIẾT BỊ","room037_devices_btn"),
    Tuple.Create(DeclareBtn,"Thông số","room037_declare"),Tuple.Create(SaveBtn,"Lưu vào bản vẽ","room037_save"),
    Tuple.Create(ExportBtn,"Xuất Excel","room037_export"),Tuple.Create(RefreshBtn,"Cập nhật","room037_refresh"),Tuple.Create(LegacyBtn,"Dữ liệu cũ","room037_legacy"),Tuple.Create(CloseBtn,"Đóng","room037_close")};
   // S5: visible action labels uppercase; AccessibleName keeps the earlier wording.
   foreach(var t in names){t.Item1.Text=t.Item2.ToUpperInvariant();t.Item1.Name=t.Item3;t.Item1.AccessibleName=t.Item2;BdgklLook034.Style(t.Item1,t.Item1==RegionBtn||t.Item1==HeightBtn||t.Item1==ProjectBtn,face,S(70),S(28));t.Item1.Margin=new Padding(S(4),S(3),0,0);actions.Controls.Add(t.Item1);}
   CloseBtn.DialogResult=DialogResult.Cancel;
   var bar=new FlowLayoutPanel{Dock=DockStyle.Fill,WrapContents=true,AutoSize=true,AutoSizeMode=System.Windows.Forms.AutoSizeMode.GrowAndShrink,Margin=new Padding(0),BackColor=BdgklLook034.Back};
   bar.Controls.Add(new Label{Text="Lọc trạng thái",AutoSize=true,Margin=new Padding(S(4),S(8),S(2),0),ForeColor=BdgklLook034.Muted});
   StatusFilter.Name="room037_filter_status";StatusFilter.AccessibleName="Lọc trạng thái";StatusFilter.DropDownStyle=ComboBoxStyle.DropDownList;StatusFilter.Width=S(160);StatusFilter.Margin=new Padding(0,S(4),S(12),0);
   // 045: CẦN XỬ LÝ (missing, CHECK, conflicts, pending raw cells) is the first item and the default; 'Tất cả' stays one click away.
   StatusFilter.Items.Add(new RouteDialog034.Choice(NeedsFilter045,"CẦN XỬ LÝ"));
   StatusFilter.Items.Add(new RouteDialog034.Choice(null,"Tất cả"));StatusFilter.Items.Add(new RouteDialog034.Choice(RoomCompletion037.Check,"Cần kiểm tra"));
   StatusFilter.Items.Add(new RouteDialog034.Choice(RoomCompletion037.MissingData,"Thiếu dữ liệu"));StatusFilter.Items.Add(new RouteDialog034.Choice(RoomCompletion037.Pass,"Đủ dữ liệu"));
   if(Session.projectMode)StatusFilter.Items.Add(new RouteDialog034.Choice(RoomCompletion037.Excluded,"Đã bỏ dòng"));
   StatusFilter.SelectedIndex=Session.projectMode?0:1;lastFilter=StatusFilter.SelectedIndex;bar.Controls.Add(StatusFilter);   // 045: legacy mode keeps 'Tất cả'
   // S5: a refused pending cell keeps the previous filter.
   StatusFilter.SelectedIndexChanged+=(s,e)=>{if(filling)return;
    if(!CommitEdits()){filling=true;try{StatusFilter.SelectedIndex=lastFilter;}finally{filling=false;}return;}
    lastFilter=StatusFilter.SelectedIndex;Fill();};
   ScopeLabel.Name="room037_scope";ScopeLabel.AutoSize=true;ScopeLabel.MaximumSize=new Size(S(980),0);ScopeLabel.ForeColor=BdgklLook034.Muted;ScopeLabel.Margin=new Padding(0,S(8),S(12),0);bar.Controls.Add(ScopeLabel);
   ReviewBox.Name="room037_review";ReviewBox.AccessibleName="Đã xem lại thay đổi hình";ReviewBox.AutoSize=true;ReviewBox.ForeColor=BdgklLook034.Error;ReviewBox.Margin=new Padding(S(4),S(2),0,S(2));
   ReviewBox.Text="Hình trong phòng đã đổi so với lần xác nhận đã lưu. Tôi đã xem lại các dòng và khai báo (bấm 'Lưu vào bản vẽ' để ghi nhận).";
   var reviewLine=new FlowLayoutPanel{Dock=DockStyle.Fill,AutoSize=true,AutoSizeMode=System.Windows.Forms.AutoSizeMode.GrowAndShrink,Margin=new Padding(0),BackColor=BdgklLook034.Back};reviewLine.Controls.Add(ReviewBox);
   RouteGrid=RoomLook037.Grid("room037_routes","Bảng tuyến",RouteColumns,new[]{4,20,9,7,7,6,6,7,6,7,8,9,7,9,22},-1,-1,face,bold,this.dpi);
   Editable(RouteGrid,RouteFields,new[]{44,230,110,96,90,76,80,100,76,90,100,110,96,120,300},new[]{3,5,6,7,8,9,10},4,RoomAdjustmentsUi037.RoutingLabels);
   // The first two columns (STT, TUYẾN) stay visible while scrolling horizontally.
   RouteGrid.Columns[1].Frozen=true;
   RouteGrid.CellValidating+=(s,e)=>CellCheck(RouteGrid,RouteFields,e);RouteGrid.CellEndEdit+=(s,e)=>RouteEndEdit(e);
   // 045 PATCH2 F02: edit intent lifecycle — cleared when an editor opens; set when a dirty (typed / chosen) editor value is actually pushed (CellParsing:
   // Enter / Tab / click commits; never raised by focus-only or CancelEdit) and before CommitEdits' EndEdit; consumed by every CellEndEdit.
   RouteGrid.CellBeginEdit+=(s,e)=>ClearTyped045();RouteGrid.CellParsing+=(s,e)=>{if(!filling)SameTyped045(e.RowIndex,e.ColumnIndex);};
   SupplementGrid=RoomLook037.Grid("room037_supplements","Dòng bổ sung thủ công",SupplementColumns,new[]{8,10,4,8,9,14,14,10,9,18},-1,-1,face,bold,this.dpi);
   Editable(SupplementGrid,SupplementFields,new[]{110,130,60,110,130,200,200,170,120,280},new[]{3},0,RoomAdjustmentsUi037.KindLabels);
   SupplementGrid.CellValidating+=(s,e)=>CellCheck(SupplementGrid,SupplementFields,e);SupplementGrid.CellEndEdit+=(s,e)=>SupplementEndEdit(e);
   TotalGrid=RoomLook037.Grid("room037_combined","Tổng CAD / BỔ SUNG / TỔNG theo loại và vật liệu",new[]{"LOẠI","VẬT LIỆU","ĐƠN VỊ","CAD (m)","BỔ SUNG (m)","TỔNG (m)"},
    new[]{14,20,8,14,14,14},3,5,face,bold,this.dpi);
   ExclusionGrid=RoomLook037.Grid("room037_exclusions","Bản ghi bỏ dòng",new[]{"KHOÁ DÒNG","TUYẾN","LÝ DO","TRẠNG THÁI","GHI CHÚ"},new[]{18,18,20,16,28},-1,-1,face,bold,this.dpi);
   WireGrid=RoomLook037.Grid("room037_wires","Tổng dây (chỉ dòng đủ dữ liệu)",new[]{"Tầng","Bộ dây","Số dòng","Mét tuyến chưa hệ số (m)","Mét tuyến sau hệ số (m)"},new[]{14,20,10,18,18},2,4,face,bold,this.dpi);
   PipeGrid=RoomLook037.Grid("room037_pipes","Tổng ống (chỉ dòng đủ dữ liệu)",new[]{"Tầng","Ống","Số dòng","Ống ngang (m)","Ống đứng (m)","Chìm (m)","Nổi (m)","Chìm sau hệ số (m)","Nổi sau hệ số (m)"},
    new[]{10,10,7,10,10,9,9,12,12},2,8,face,bold,this.dpi);
   IssueGrid=RoomLook037.Grid("room037_issues","Vướng mắc chặn kết quả",new[]{"Nguồn","Mức","Nội dung"},new[]{12,10,78},-1,-1,face,bold,this.dpi);
   RouteTab.Name="room037_tab_route";WireTab.Name="room037_tab_wire";PipeTab.Name="room037_tab_pipe";IssueTab.Name="room037_tab_issue";
   SupplementTab.Name="room037_tab_supplement";ExclusionTab.Name="room037_tab_exclusion";
   WireTab.Controls.Add(WireGrid);PipeTab.Controls.Add(PipeGrid);IssueTab.Controls.Add(IssueGrid);
   // S5 TUYẾN tab: [LÝ DO BỎ DÒNG | BỎ DÒNG | KHÔI PHỤC] (project mode only) above the grid.
   var exBar=Strip("room037_exclude_bar");
   exBar.Controls.Add(new Label{Text="LÝ DO BỎ DÒNG (bắt buộc)",AutoSize=true,Margin=new Padding(S(4),S(8),S(4),0),ForeColor=BdgklLook034.Muted});
   ReasonBox.Name="room037_exclude_reason";ReasonBox.AccessibleName="Lý do bỏ dòng";ReasonBox.BorderStyle=BorderStyle.FixedSingle;ReasonBox.MaxLength=RoomAdjustments037.MaxNote;
   ReasonBox.Width=S(320);ReasonBox.Margin=new Padding(0,S(5),S(4),0);exBar.Controls.Add(ReasonBox);
   Action<Control,Button,string,string,string> act=(host,b,text,name,tip)=>{b.Text=text;b.Name=name;b.AccessibleName=text;BdgklLook034.Style(b,false,face,S(90),S(28));
    b.Margin=new Padding(S(4),S(3),0,S(3));tip037.SetToolTip(b,tip);host.Controls.Add(b);};
   act(exBar,ExcludeBtn,"BỎ DÒNG","room037_exclude","Không tính dòng nguồn đang chọn (hình CAD giữ nguyên, có thể KHÔI PHỤC); cần nhập lý do.");
   act(exBar,RestoreBtn,"KHÔI PHỤC","room037_restore","Tính lại dòng đã bỏ đang chọn; bản ghi bỏ dòng giữ lại để đối chiếu.");
   exBar.Visible=Session.projectMode;
   RouteTab.Controls.Add(Stack(TopStrip045(exBar),RouteGrid,null,null));   // 045: preset / range / undo strip above the BỎ DÒNG strip
   // S5 BỎ DÒNG tab: every exclusion record (also restored / stale / unmatched) with explicit KHÔI PHỤC / XÁC NHẬN LẠI.
   var rcBar=Strip("room037_exclusion_bar");
   act(rcBar,RestoreRecordBtn,"KHÔI PHỤC","room037_exclusion_restore","Khôi phục bản ghi bỏ dòng đang chọn (kể cả bản ghi không còn khớp bản vẽ).");
   act(rcBar,ReconfirmBtn,"XÁC NHẬN LẠI","room037_exclusion_reconfirm","Hình đã đổi: xác nhận vẫn bỏ đúng khoá dòng này trên lần quét hiện tại (không ghép theo nhãn).");
   rcBar.Controls.Add(new Label{Text="Bản ghi HÌNH ĐÃ ĐỔI / KHÔNG CÒN TRONG BẢN VẼ chặn kết quả cho tới khi XÁC NHẬN LẠI hoặc KHÔI PHỤC.",AutoSize=true,Margin=new Padding(S(8),S(8),0,0),ForeColor=BdgklLook034.Muted});
   ExclusionTab.Controls.Add(Stack(rcBar,ExclusionGrid,null,null));
   // S5 BỔ SUNG tab: manual supplements (editable) + CAD / BỔ SUNG / TỔNG.
   var spBar=Strip("room037_supplement_bar");
   act(spBar,AddSupplementBtn,"THÊM DÒNG","room037_supplement_add","Thêm một dòng khối lượng bổ sung thủ công (đơn vị m) cho phòng đang mở; không tạo tuyến CAD.");
   act(spBar,RemoveSupplementBtn,"XOÁ DÒNG","room037_supplement_remove","Không tính dòng bổ sung đang chọn (bản ghi giữ lại, có thể KHÔI PHỤC DÒNG).");
   act(spBar,RestoreSupplementBtn,"KHÔI PHỤC DÒNG","room037_supplement_restore","Tính lại dòng bổ sung đã xoá.");
   act(spBar,OverlapBtn,"XÁC NHẬN CỘNG THÊM","room037_supplement_overlap","Sau khi xem lại: khối lượng bổ sung là PHẦN THÊM ngoài phần đo CAD cùng loại/vật liệu (không tự gộp/trừ).");
   TotalLabel.Name="room037_combined_label";TotalLabel.AutoSize=true;TotalLabel.Font=bold;TotalLabel.Margin=new Padding(S(2),S(6),0,S(2));
   SupplementTab.Controls.Add(Stack(spBar,SupplementGrid,TotalLabel,TotalGrid));
   Tabs.Name="room037_tabs";Tabs.AccessibleName="TUYẾN / DÂY / ỐNG / VƯỚNG MẮC";Tabs.Dock=DockStyle.Fill;foreach(var p in new[]{RouteTab,WireTab,PipeTab})Tabs.TabPages.Add(p);
   if(Session.projectMode){Tabs.TabPages.Add(SupplementTab);Tabs.TabPages.Add(ExclusionTab);}
   Tabs.TabPages.Add(IssueTab);
   Tabs.SelectedIndexChanged+=(s,e)=>Detail();
   SummaryLabel.Name="room037_summary";SummaryLabel.Dock=DockStyle.Fill;SummaryLabel.AutoSize=false;SummaryLabel.TextAlign=ContentAlignment.MiddleLeft;SummaryLabel.Font=bold;
   BdgklLook034.ReadOnlyBox(DetailBox);DetailBox.Name="room037_detail";DetailBox.AccessibleName="Chi tiết dòng đã chọn";
   BdgklLook034.ReadOnlyBox(StatusBox);StatusBox.Name="room037_status";StatusBox.AccessibleName="Thông báo";StatusBox.BackColor=BdgklLook034.Back;StatusBox.BorderStyle=BorderStyle.None;
   // TVXD-MEPS-RAW-DRAFT-044 footer: recovery status + BỎ Ô NHÁP (hidden until RoomRecoveryUi044 is attached / raw cells exist).
   rawLine=new FlowLayoutPanel{Name="room037_recovery_line",Dock=DockStyle.Fill,WrapContents=true,AutoSize=true,AutoSizeMode=System.Windows.Forms.AutoSizeMode.GrowAndShrink,Margin=new Padding(0),BackColor=BdgklLook034.Back,Visible=false};
   RecoveryLabel.Name="room037_recovery";RecoveryLabel.AccessibleName="Nháp khôi phục";RecoveryLabel.AutoSize=true;RecoveryLabel.MaximumSize=new Size(S(860),0);RecoveryLabel.ForeColor=BdgklLook034.Muted;
   RecoveryLabel.Margin=new Padding(S(4),S(6),S(8),0);
   DiscardRawBtn.Text="BỎ Ô NHÁP";DiscardRawBtn.Name="room037_discard_raw";DiscardRawBtn.AccessibleName="Bỏ các ô nhập nháp chưa áp dụng";BdgklLook034.Style(DiscardRawBtn,false,face,S(90),S(26));
   DiscardRawBtn.Margin=new Padding(S(4),S(2),0,0);DiscardRawBtn.Visible=false;DiscardRawBtn.CausesValidation=false;DiscardRawBtn.Click+=(s,e)=>{if(DiscardRaw!=null)DiscardRaw();};
   rawLine.Controls.Add(RecoveryLabel);rawLine.Controls.Add(DiscardRawBtn);
   pane.Controls.Add(bar,0,0);pane.Controls.Add(actions,0,1);pane.Controls.Add(reviewLine,0,2);pane.Controls.Add(Tabs,0,3);pane.Controls.Add(SummaryLabel,0,4);pane.Controls.Add(DetailBox,0,5);pane.Controls.Add(StatusBox,0,6);
   pane.Controls.Add(rawLine,0,7);
   Controls.Add(pane);CancelButton=CloseBtn;
   Init045();   // 045: cell-range selection, keys, compact columns, collapsed detail
   RegionBtn.Click+=(s,e)=>{if(!CommitEdits())return;Chosen=Choice.Region;DialogResult=DialogResult.OK;};
   LegacyBtn.Click+=(s,e)=>{if(!CommitEdits())return;Chosen=Choice.Legacy;DialogResult=DialogResult.OK;};
   HeightBtn.Click+=(s,e)=>DoHeights();ProjectBtn.Click+=(s,e)=>DoProject();DeviceBtn.Click+=(s,e)=>DoDevices();
   ProjectBtn.Visible=DeviceBtn.Visible=Session.projectMode;HeightBtn.Visible=!Session.projectMode;
   SaveBtn.Click+=(s,e)=>DoSave();DeclareBtn.Click+=(s,e)=>DoDeclare();ExportBtn.Click+=(s,e)=>DoExport();RefreshBtn.Click+=(s,e)=>DoRefresh();
   // Double click opens 'Thông số' only on read-only columns (editable columns start the cell editor).
   RouteGrid.SelectionChanged+=(s,e)=>Detail();
   RouteGrid.CellDoubleClick+=(s,e)=>{if(e.RowIndex<0)return;if(e.ColumnIndex<0||RouteFields[e.ColumnIndex]==null)DoDeclare();else RouteGrid.BeginEdit(true);};
   SupplementGrid.CellDoubleClick+=(s,e)=>{if(e.RowIndex>=0&&e.ColumnIndex>=0&&SupplementFields[e.ColumnIndex]!=null)SupplementGrid.BeginEdit(true);};
   foreach(var g in new[]{WireGrid,PipeGrid,IssueGrid,SupplementGrid,TotalGrid,ExclusionGrid})g.SelectionChanged+=(s,e)=>Detail();
   ExcludeBtn.Click+=(s,e)=>{var r=SelectedRow();if(r==null){SetStatus("Chọn một dòng ở tab TUYẾN trước khi BỎ DÒNG.",true);return;}if(ExcludeRow(r.rowKey,ReasonBox.Text))ReasonBox.Text="";};
   RestoreBtn.Click+=(s,e)=>{var r=SelectedRow();if(r==null){SetStatus("Chọn dòng đã bỏ ở tab TUYẾN trước khi KHÔI PHỤC.",true);return;}RestoreRow(r.rowKey);};
   RestoreRecordBtn.Click+=(s,e)=>{var k=SelectedExclusionKey();if(k==null){SetStatus("Chọn một bản ghi ở tab BỎ DÒNG.",true);return;}RestoreRow(k);};
   ReconfirmBtn.Click+=(s,e)=>{var k=SelectedExclusionKey();if(k==null){SetStatus("Chọn một bản ghi ở tab BỎ DÒNG.",true);return;}ReconfirmRow(k,null);};
   AddSupplementBtn.Click+=(s,e)=>AddSupplementRow();
   RemoveSupplementBtn.Click+=(s,e)=>{var id=SelectedSupplementId();if(id==null){SetStatus("Chọn một dòng ở tab BỔ SUNG.",true);return;}SetSupplementActive(id,false);};
   RestoreSupplementBtn.Click+=(s,e)=>{var id=SelectedSupplementId();if(id==null){SetStatus("Chọn một dòng ở tab BỔ SUNG.",true);return;}SetSupplementActive(id,true);};
   OverlapBtn.Click+=(s,e)=>{var id=SelectedSupplementId();var x=RoomAdjustmentsUi037.Supplement(Session,id);if(x==null){SetStatus("Chọn một dòng ở tab BỔ SUNG.",true);return;}
    ConfirmSupplementOverlap(id,!x.overlapConfirmed);};
   // Close: a pending cell edit is cancelled explicitly (never applied), like every other unsaved change of the closed session.
   CloseBtn.CausesValidation=false;
   // 044: the open editor text is captured into the raw draft BEFORE CancelEdit (BeforeClose detaches the capture first, so the cancel is not recorded).
   FormClosing+=(s,e)=>{if(DialogResult==DialogResult.OK)return;if(BeforeClose!=null)BeforeClose();bool had=false;
    foreach(var g in new[]{RouteGrid,SupplementGrid})if(g.IsCurrentCellInEditMode){g.CancelEdit();g.EndEdit();had=true;}
    if(had)e.Cancel=false;if(e.Cancel&&CloseCancelled!=null)CloseCancelled();};
   Disposed+=(s,e)=>tip037.Dispose();}
  readonly ToolTip tip037=new ToolTip();
  FlowLayoutPanel Strip(string name){return new FlowLayoutPanel{Name=name,Dock=DockStyle.Fill,WrapContents=true,AutoSize=true,AutoSizeMode=System.Windows.Forms.AutoSizeMode.GrowAndShrink,
   Margin=new Padding(0),BackColor=BdgklLook034.Back};}
  // Vertical stack: top strip (auto size) + main grid; optional label (auto size) + lower grid (60 % / 40 %).
  TableLayoutPanel Stack(Control top,Control main,Control label,Control lower){
   var t=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=1,RowCount=lower==null?2:4,Margin=new Padding(0),Padding=new Padding(0),BackColor=BdgklLook034.Back};
   t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));t.RowStyles.Add(new RowStyle(SizeType.AutoSize));t.RowStyles.Add(new RowStyle(SizeType.Percent,lower==null?100:60));
   t.Controls.Add(top,0,0);t.Controls.Add(main,0,1);
   if(lower!=null){t.RowStyles.Add(new RowStyle(SizeType.AutoSize));t.RowStyles.Add(new RowStyle(SizeType.Percent,40));t.Controls.Add(label,0,2);t.Controls.Add(lower,0,3);}
   return t;}
  // Editable grid: fixed readable widths (horizontal scroll instead of crushed columns), pale blue editable columns, one drop-down column.
  void Editable(DataGridView g,string[] fields,int[] widths,int[] numbers,int dropDown,string[] items){
   g.ReadOnly=false;g.EditMode=DataGridViewEditMode.EditOnKeystrokeOrF2;g.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.None;g.ScrollBars=ScrollBars.Both;
   var old=g.Columns[dropDown];
   var dd=new DataGridViewComboBoxColumn{HeaderText=old.HeaderText,Name=old.Name,DisplayStyle=DataGridViewComboBoxDisplayStyle.DropDownButton,DisplayStyleForCurrentCellOnly=true,
    FlatStyle=FlatStyle.Flat,SortMode=DataGridViewColumnSortMode.NotSortable,MinimumWidth=old.MinimumWidth};
   foreach(var it in items)dd.Items.Add(it);
   g.Columns.RemoveAt(dropDown);g.Columns.Insert(dropDown,dd);
   for(int c=0;c<g.Columns.Count;c++){var col=g.Columns[c];col.Width=S(widths[c]);col.ReadOnly=fields[c]==null;
    if(fields[c]!=null)col.DefaultCellStyle.BackColor=RoomLook037.EditBack;
    if(Array.IndexOf(numbers,c)>=0)col.DefaultCellStyle.Alignment=DataGridViewContentAlignment.MiddleRight;}
   // A stored value outside the drop-down list is added to it when painted; any other data error is shown, never thrown.
   g.DataError+=(s,e)=>{e.ThrowException=false;SetStatus("Ô "+(e.ColumnIndex>=0?g.Columns[e.ColumnIndex].HeaderText:"")+": "+(e.Exception==null?"giá trị không hiển thị được":e.Exception.Message),true);};}
  public void SetStatus(string text,bool error){StatusBox.ForeColor=error?BdgklLook034.Error:BdgklLook034.Ink;StatusBox.Text=text??"";}
  // 044 footer: BỎ Ô NHÁP (n) only while raw cells exist and recovery is attached; the line is hidden while it has nothing to show.
  public void RawFooter(){int n=Session.rawOverlay==null?0:Session.rawOverlay.Count;DiscardRawBtn.Visible=n>0&&DiscardRaw!=null;
   DiscardRawBtn.Text="BỎ Ô NHÁP ("+n.ToString(CultureInfo.InvariantCulture)+")";rawLine.Visible=RecoveryLabel.Text.Length>0||DiscardRawBtn.Visible;}
  void RawResolved(string kind,string key,string field){if(RawDone!=null)RawDone(kind,key,field);}
  string Filter(){var c=StatusFilter.SelectedItem as RouteDialog034.Choice;return c==null?null:c.value;}
  public void ShowSession(){Fill();}
  // 045: the TUYẾN grid selects cell ranges. The row of 'Thông số' / BỎ DÒNG / details = the CURRENT row, and only when every selected cell lies in
  // that row (a multi-row range never stands for one record: null).
  public RoomCompletionRow037 SelectedRow(){return SingleRow045();}
  // QA helper: selects the TUYẾN row of this row key; false = not shown.
  public bool SelectRow(string rowKey){foreach(DataGridViewRow r in RouteGrid.Rows){var x=r.Tag as RoomCompletionRow037;
    if(x!=null&&string.Equals(x.rowKey,rowKey,StringComparison.Ordinal)){Tabs.SelectedTab=RouteTab;FocusRow045(r);return true;}}return false;}
  public string SelectedSupplementId(){return SupplementGrid.SelectedRows.Count==1?SupplementGrid.SelectedRows[0].Tag as string:null;}
  public string SelectedExclusionKey(){return ExclusionGrid.SelectedRows.Count==1?ExclusionGrid.SelectedRows[0].Tag as string:null;}
  static void Pick(DataGridView g,string tag){if(tag==null)return;foreach(DataGridViewRow r in g.Rows)if(string.Equals(r.Tag as string,tag,StringComparison.Ordinal)){g.ClearSelection();r.Selected=true;return;}}
  static bool HasField(List<string> l,string field){var t="["+field+"]";return l!=null&&l.Any(x=>x!=null&&x.Contains(t));}
  static string Combo(DataGridView g,int col,string text){var c=g.Columns[col] as DataGridViewComboBoxColumn;if(c!=null&&text!=null&&!c.Items.Contains(text))c.Items.Add(text);return text;}
  // Full rebuild (never called from a cell event; actions commit the pending cell first).
  void Fill(){var s=Session;var b=s.batch;var keep=SelectedRow();string keepKey=keep==null?null:keep.rowKey,keepSup=SelectedSupplementId(),keepEx=SelectedExclusionKey();
   filling=true;
   try{RouteGrid.Rows.Clear();SupplementGrid.Rows.Clear();
    ReviewBox.Visible=s.NeedsReview;if(!s.NeedsReview)ReviewBox.Checked=false;
    if(b==null){WireGrid.Rows.Clear();PipeGrid.Rows.Clear();IssueGrid.Rows.Clear();TotalGrid.Rows.Clear();ExclusionGrid.Rows.Clear();
     ScopeLabel.Text=s.projectMode?RoomProjectUi037.ScopeText(s):
      "Chưa chọn phòng: bấm 'Chọn vùng CAD' (đặt tên phòng, đơn vị, dung sai nối, rồi chọn hai góc phòng), sau đó 'Cao độ tầng và thiết bị'.";
     SummaryText="Chưa có dữ liệu phòng."+(s.projectDirty?" · CÓ THAY ĐỔI DỰ ÁN CHƯA LƯU.":"");
     SummaryLabel.Text=SummaryText;SummaryLabel.ForeColor=BdgklLook034.Muted;TotalLabel.Text="";
     if(StatusBox.Text.Length==0)SetStatus("Phòng cần có dây vẽ bằng lệnh DÂY DẪN và thiết bị có TAG. 'Dữ liệu cũ' mở bảng BDGKL từng tuyến trước đây.",false);
     RawFooter();}
    else{
     ScopeLabel.Text=s.projectMode?RoomProjectUi037.ScopeText(s)+" · đơn vị bản vẽ "+s.units+" · dung sai nối "+RoomLook037.Exact(s.tolerance)+" · quét lúc "+(s.scannedAt??"?"):
      "Phòng "+s.roomId+" · đơn vị bản vẽ "+s.units+" · dung sai nối "+RoomLook037.Exact(s.tolerance)+" đơn vị bản vẽ · quét lúc "+(s.scannedAt??"?")+
      (s.heights==null?" · chưa thiết lập cao độ nhóm":" · đã thiết lập cao độ"+(s.heights.floorId!=null?" tầng "+s.heights.floorId:""));
     string f=Filter();int stt=0;reorderPending=false;
     // SORT-039: presentation order (floor, natural circuit, batch order) on a copy of references; b.rows keeps the calculation order.
     foreach(var r in RoomRowOrder037.Sorted(b.rows)){if(!Shown045(r,f))continue;stt++;   // 045: CẦN XỬ LÝ also shows rows with pending raw cells
      int k=RouteGrid.Rows.Add();var row=RouteGrid.Rows[k];row.Tag=r;PaintRoute(row,r,stt);
      if(keepKey!=null&&r.rowKey==keepKey)FocusRow045(row);}
     if(s.projectMode&&s.adjustments!=null&&s.adjustments.supplements!=null)foreach(var x in s.adjustments.supplements){if(x==null)continue;
      int k=SupplementGrid.Rows.Add();var row=SupplementGrid.Rows[k];row.Tag=x.id;PaintSupplement(row,x);}
     Pick(SupplementGrid,keepSup);
     FillRest(keepEx);}}
   finally{filling=false;}
   Detail();}
  // In-place repaint after a cell edit (rows and their objects are unchanged: Calculate keeps row objects by key; supplements by ID).
  void Repaint(){if(Session.batch==null)return;filling=true;
   try{int stt=0;foreach(DataGridViewRow row in RouteGrid.Rows){var r=row.Tag as RoomCompletionRow037;if(r!=null)PaintRoute(row,r,++stt);}
    foreach(DataGridViewRow row in SupplementGrid.Rows){var x=RoomAdjustmentsUi037.Supplement(Session,row.Tag as string);if(x!=null)PaintSupplement(row,x);}
    FillRest(SelectedExclusionKey());}
   finally{filling=false;}
   Detail();}
  void PaintRoute(DataGridViewRow row,RoomCompletionRow037 r,int stt){var x=r.result;var i=r.input;
   var v=new object[]{stt.ToString(CultureInfo.InvariantCulture),(r.startLabel??"?")+" → "+(r.endLabel??"?"),RoomAdjustmentsUi037.CellText(i,RoomAdjustmentsUi037.Circuit),
    BdgklLook034.M(r.planMetres),Combo(RouteGrid,4,RoomAdjustmentsUi037.CellText(i,RoomAdjustmentsUi037.Routing)),BdgklLook034.M(x==null?null:x.verticalM),
    RoomAdjustmentsUi037.CellText(i,RoomAdjustmentsUi037.TailCount),RoomAdjustmentsUi037.CellText(i,RoomAdjustmentsUi037.TailEach),BdgklLook034.M(x==null?null:x.tailsM),
    RoomAdjustmentsUi037.CellText(i,RoomAdjustmentsUi037.Factor),BdgklLook034.M(x==null?null:x.routeM),RoomAdjustmentsUi037.CellText(i,RoomAdjustmentsUi037.Assembly),
    RoomAdjustmentsUi037.CellText(i,RoomAdjustmentsUi037.Conduit),RoomLook037.Status(r.status),r.excluded?"Bỏ dòng: "+r.exclusionReason:RoomLook037.Reason(r)};
   for(int c=0;c<v.Length;c++){row.Cells[c].Value=v[c];row.Cells[c].ErrorText="";}
   row.Cells[14].ToolTipText=string.Join("\r\n",r.issues.Concat(r.missing).Select(RoomLook037.Friendly));
   // S6: the route cell shows compact labels; the full device identity (type, block name, TAG label, handle) stays available here.
   row.Cells[1].ToolTipText="Đầu: "+RoomUi037.EndText(r.startType,r.startName,r.startLabel,r.startId)+"\r\nCuối: "+RoomUi037.EndText(r.endType,r.endName,r.endLabel,r.endId)+
    (r.sourceKind==RoomCompletion037.KindSection?"\r\nĐoạn đo nối tiếp của tuyến điều khiển "+r.sourceId+" (phần trước thiết bị đầu tính ở dòng trước, không cộng hai lần).":"");
   row.Cells[2].ToolTipText="Nhãn kênh TAG trong phòng: "+(r.channelLabel??"—")+(string.IsNullOrEmpty(r.structuredKey)?"":" · lộ cấu trúc "+r.structuredKey);
   bool check=r.status==RoomCompletion037.Check,pass=r.status==RoomCompletion037.Pass;
   row.DefaultCellStyle.BackColor=r.excluded?RoomLook037.ExcludedRow:check?BdgklLook034.ExcludedBack:pass?Color.Empty:RoomLook037.Missing;
   row.DefaultCellStyle.ForeColor=r.excluded?BdgklLook034.Muted:check?BdgklLook034.Error:pass?Color.Empty:RoomLook037.MissingInk;
   for(int c=0;c<RouteFields.Length;c++){var fd=RouteFields[c];if(fd==null)continue;var cell=row.Cells[c];
    bool err=!r.excluded&&HasField(r.issues,fd),miss=!r.excluded&&HasField(r.missing,fd);cell.ReadOnly=i==null;
    cell.Style.BackColor=i==null?Color.Empty:err||miss?RoomLook037.Missing:RoomLook037.EditBack;
    cell.Style.ForeColor=err?BdgklLook034.Error:miss?RoomLook037.MissingInk:Color.Empty;cell.Style.Padding=Padding.Empty;   // 044 PATCH2: normal padding
    cell.Style.Alignment=DataGridViewContentAlignment.NotSet;   // 045 F04: column alignment again once corrected
    // 044: a raw (not accepted) text of this row key + field is shown exactly, red, with its reason; the accepted value still drives the result.
    var raw=RawCellText==null||r.rowKey==null?null:RawCellText(r.rowKey,fd);
    if(raw!=null){cell.Value=c==4?Combo(RouteGrid,4,raw):raw;var p=RoomAdjustmentsUi037.CellProblem(fd,raw);
     cell.ErrorText="Ô NHÁP CHƯA ÁP DỤNG: "+(p??"bấm vào ô rồi Enter để áp dụng giá trị này")+" (tổng đang dùng giá trị đã áp dụng; chưa xuất Excel được).";
     cell.Style.BackColor=RoomLook037.Missing;cell.Style.ForeColor=BdgklLook034.Error;cell.Style.Padding=RawPad();
     // 045 F04 (R2 showed padding alone still overlapped right-aligned numbers): raw text is LEFT aligned, away from the error icon drawn at the right edge;
     // the row status also says CHỜ ÁP DỤNG (Paint045), so the pending state is never shown as ĐỦ.
     cell.Style.Alignment=DataGridViewContentAlignment.MiddleLeft;}}
   Paint045(row,r);}
  // 044 PATCH2 (R044-VISUAL-04): the grid draws the error icon at the right edge of the cell; a raw cell reserves that space (DPI-scaled) so the
  // complete right-aligned raw text stays visible beside the icon. Every repaint resets other cells to Padding.Empty.
  Padding RawPad(){return new Padding(0,0,S(RawIconRoom),0);}
  const int RawIconRoom=24;
  void PaintSupplement(DataGridViewRow row,RoomSupplement037 x){var st=RoomAdjustmentsUi037.SupplementState(Session,x.id);
   string overlap=st==null||!x.active?"—":st.overlap?(x.overlapConfirmed?"CÓ – ĐÃ XÁC NHẬN CỘNG THÊM":"CÓ – CHƯA XÁC NHẬN"):"KHÔNG";
   string status=!x.active?RoomLook037.Status(RoomAdjustments037.Inactive):st==null?"CHƯA TÍNH":RoomLook037.Status(st.status);
   var notes=st==null?new List<string>():st.issues.Concat(st.missing).ToList();
   var v=new object[]{Combo(SupplementGrid,0,RoomAdjustmentsUi037.SupplementText(x,RoomAdjustmentsUi037.SKind)),x.materialCode??"",x.unit??"",RoomLook037.Exact(x.metres),x.circuitId??"",
    x.description??"",x.reason??"",overlap,status,notes.Count==0?"":RoomLook037.Friendly(notes[0])+(notes.Count>1?" (+"+(notes.Count-1).ToString(CultureInfo.InvariantCulture)+")":"")};
   for(int c=0;c<v.Length;c++){row.Cells[c].Value=v[c];row.Cells[c].ErrorText="";}
   row.Cells[9].ToolTipText=string.Join("\r\n",notes.Select(RoomLook037.Friendly));
   bool check=st!=null&&x.active&&st.status==RoomCompletion037.Check,miss=st!=null&&x.active&&st.status==RoomCompletion037.MissingData;
   row.DefaultCellStyle.BackColor=!x.active?RoomLook037.ExcludedRow:check?BdgklLook034.ExcludedBack:miss?RoomLook037.Missing:Color.Empty;
   row.DefaultCellStyle.ForeColor=!x.active?BdgklLook034.Muted:check?BdgklLook034.Error:miss?RoomLook037.MissingInk:Color.Empty;
   for(int c=0;c<SupplementFields.Length;c++){if(SupplementFields[c]==null)continue;var cell=row.Cells[c];
    bool m=st!=null&&x.active&&SupplementMissing[c]!=null&&st.missing.Any(q=>q.StartsWith(SupplementMissing[c]+":",StringComparison.Ordinal));
    cell.Style.BackColor=m?RoomLook037.Missing:RoomLook037.EditBack;cell.Style.ForeColor=m?RoomLook037.MissingInk:Color.Empty;cell.Style.Padding=Padding.Empty;
    cell.Style.Alignment=DataGridViewContentAlignment.NotSet;   // 045 F04
    var raw=RawSupplementText==null?null:RawSupplementText(x.id,SupplementFields[c]);   // 044 (see PaintRoute)
    if(raw!=null){cell.Value=c==0?Combo(SupplementGrid,0,raw):raw;var p=RoomAdjustmentsUi037.SupplementProblem(SupplementFields[c],raw);
     cell.ErrorText="Ô NHÁP CHƯA ÁP DỤNG: "+(p??"bấm vào ô rồi Enter để áp dụng giá trị này")+" (chưa xuất Excel được).";
     cell.Style.BackColor=RoomLook037.Missing;cell.Style.ForeColor=BdgklLook034.Error;cell.Style.Padding=RawPad();cell.Style.Alignment=DataGridViewContentAlignment.MiddleLeft;}}}
  // Totals, issues, exclusion records, CAD / BỔ SUNG / TỔNG and the summary (read-only grids only).
  void FillRest(string keepEx){var s=Session;var b=s.batch;
   WireGrid.Rows.Clear();PipeGrid.Rows.Clear();IssueGrid.Rows.Clear();TotalGrid.Rows.Clear();ExclusionGrid.Rows.Clear();
   foreach(var t in b.totals){string fl=RoomProjectUi037.FloorLabel(s,t.floorId);   // project mode: floor NAME, never the stable FL_ ID
    if(t.kind=="WIRE")WireGrid.Rows.Add(fl,t.code,t.rows.ToString(CultureInfo.InvariantCulture),BdgklLook034.M(t.rawM),BdgklLook034.M(t.afterFactorM));
    else PipeGrid.Rows.Add(fl,t.code,t.rows.ToString(CultureInfo.InvariantCulture),BdgklLook034.M(t.horizontalM),BdgklLook034.M(t.verticalM),BdgklLook034.M(t.hiddenRawM),
     BdgklLook034.M(t.exposedRawM),BdgklLook034.M(t.hiddenAfterFactorM),BdgklLook034.M(t.exposedAfterFactorM));}
   foreach(var x in b.blockers)IssueGrid.Rows.Add(x.origin=="SELECTION"?"Chọn tuyến":"Sơ đồ dây",x.severity,x.message+(x.segmentIds.Count>0?" (đoạn "+string.Join(", ",x.segmentIds.Take(20))+")":""));
   foreach(var x in b.externalBlockers){bool pj=x!=null&&x.StartsWith(RoomProjectUi037.BlockerPrefix,StringComparison.Ordinal);
    IssueGrid.Rows.Add(pj?"Dự án":"Quét CAD","CHẶN",pj?x.Substring(RoomProjectUi037.BlockerPrefix.Length):x);}
   var raw=RoomRecoveryUi044.BlockText(s);if(raw!=null)IssueGrid.Rows.Add("Ô nhập nháp","CHẶN",raw);   // 044: raw cells block the official result/export
   if(s.loadError!=null)IssueGrid.Rows.Add("Bản nháp","LỖI",s.loadError);
   if(s.unmatched.Count>0)IssueGrid.Rows.Add("Bản nháp","GHI CHÚ",s.unmatched.Count+" khai báo đã lưu không khớp tuyến hiện tại; vẫn được giữ trong bản nháp: "+string.Join(", ",s.unmatched.Select(u=>u.rowKey).Take(10)));
   // S5: adjustment problems are listed here too (they block the result like the core says), with the action that resolves them.
   foreach(var m in b.issues.Concat(b.missing))if(m.StartsWith("ADJUSTMENTS_INVALID",StringComparison.Ordinal)||m.StartsWith("EXCLUSION_",StringComparison.Ordinal)||
    m.StartsWith("EXCLUDED_SOURCE_NOT_ASSOCIATED",StringComparison.Ordinal)||m.StartsWith("ALL_ROWS_EXCLUDED",StringComparison.Ordinal))IssueGrid.Rows.Add("Bỏ dòng / bổ sung","CHẶN",RoomLook037.Friendly(m)+" — "+m);
   IssueTab.Text="VƯỚNG MẮC ("+(b.blockers.Count+b.externalBlockers.Count).ToString(CultureInfo.InvariantCulture)+")";
   int stale=0;
   if(s.projectMode&&s.adjustments!=null&&s.adjustments.exclusions!=null)foreach(var e in s.adjustments.exclusions){if(e==null)continue;
    var st=RoomAdjustmentsUi037.ExclusionState(s,e.rowKey);var r=b.rows.FirstOrDefault(q=>q.rowKey!=null&&string.Equals(q.rowKey,e.rowKey,StringComparison.Ordinal));
    int k=ExclusionGrid.Rows.Add(e.rowKey,r==null?"(không còn trong lần quét)":(r.startLabel??"?")+" → "+(r.endLabel??"?"),e.reason,
     RoomAdjustmentsUi037.ExclusionStatusLabel(st==null?null:st.status),st==null?"":st.message);
    var row=ExclusionGrid.Rows[k];row.Tag=e.rowKey;
    bool bad=st==null||st.status==RoomAdjustments037.SignatureMismatch||st.status==RoomAdjustments037.Unmatched;if(bad)stale++;
    row.DefaultCellStyle.BackColor=bad?BdgklLook034.ExcludedBack:!e.active?RoomLook037.ExcludedRow:Color.Empty;row.DefaultCellStyle.ForeColor=bad?BdgklLook034.Error:!e.active?BdgklLook034.Muted:Color.Empty;}
   Pick(ExclusionGrid,keepEx);
   ExclusionTab.Text="BỎ DÒNG ("+b.excluded.ToString(CultureInfo.InvariantCulture)+(stale>0?" · "+stale.ToString(CultureInfo.InvariantCulture)+" CẦN XỬ LÝ":"")+")";
   int sup=s.adjustments==null||s.adjustments.supplements==null?0:s.adjustments.supplements.Count(x=>x!=null&&x.active);
   SupplementTab.Text="BỔ SUNG ("+sup.ToString(CultureInfo.InvariantCulture)+")";
   foreach(var c in b.combinedTotals)TotalGrid.Rows.Add(RoomAdjustmentsUi037.KindLabel(c.kind),c.code,c.unit,BdgklLook034.M(c.cadM),BdgklLook034.M(c.manualM),BdgklLook034.M(c.combinedM));
   TotalLabel.Text="TỔNG THEO LOẠI / VẬT LIỆU / ĐƠN VỊ (chỉ cộng đúng cùng loại, cùng mã, cùng đơn vị; BỔ SUNG chỉ gồm dòng bổ sung đủ dữ liệu): "+
    (raw!=null?"TẠM — CHƯA CHÍNH THỨC (còn ô nhập nháp chưa áp dụng).":b.totalsOfficial?"CHÍNH THỨC.":"TẠM — CHƯA CHÍNH THỨC (còn dòng thiếu dữ liệu / cần kiểm tra).");
   bool official=b.totalsOfficial&&raw==null;
   TotalLabel.ForeColor=official?BdgklLook034.Teal:BdgklLook034.Error;
   SummaryText=RoomUi037.Summary(s)+(official?" · Tổng CHÍNH THỨC (mọi dòng đủ dữ liệu).":raw!=null?" · Tổng TẠM: còn ô nhập nháp chưa áp dụng, chưa chính thức.":" · Tổng TẠM: chỉ cộng dòng đủ dữ liệu, chưa chính thức.")+
    (s.dirty||s.projectDirty?" · CÓ THAY ĐỔI CHƯA LƯU.":"");
   SummaryLabel.Text=SummaryText;SummaryLabel.ForeColor=b.status==RoomCompletion037.Pass&&raw==null?BdgklLook034.Teal:BdgklLook034.Error;RawFooter();}

  // ---- S5 cell editing ----
  // Commits a pending cell edit of TUYẾN / BỔ SUNG. false = refused or not applied: the cell keeps the error, the status says why, the caller stops.
  public bool CommitEdits(){if(RawFlush!=null)RawFlush();cellError=null;   // 044: editor text captured before validation
   foreach(var g in new[]{RouteGrid,SupplementGrid}){if(!g.IsCurrentCellInEditMode)continue;
    // 045 PATCH2 F02 (actual shown-modal trace): EndEdit() resets the dirty state and raises CellEndEdit WITHOUT CellValidating, so a dirty editor's
    // intent is kept here, BEFORE EndEdit; an unchanged (not dirty) editor sets nothing.
    if(g==RouteGrid&&g.IsCurrentCellDirty&&g.CurrentCell!=null)SameTyped045(g.CurrentCell.RowIndex,g.CurrentCell.ColumnIndex);
    if(!g.EndEdit()){if(g==RouteGrid)ClearTyped045();if(g.CanFocus)g.Focus();if(StatusBox.ForeColor!=BdgklLook034.Error)SetStatus("Ô đang sửa chưa hợp lệ: sửa lại hoặc Esc để huỷ; chưa làm thao tác này.",true);return false;}}
   if(cellError!=null){SetStatus("Ô vừa sửa chưa áp dụng được ("+cellError+"); giá trị cũ giữ nguyên, chưa làm thao tác này.",true);return false;}
   return true;}
  // Parse only (never repaints inside the event). Unchanged text is not re-validated.
  void CellCheck(DataGridView g,string[] fields,DataGridViewCellValidatingEventArgs e){
   if(filling||e.RowIndex<0||e.ColumnIndex<0||e.ColumnIndex>=fields.Length||fields[e.ColumnIndex]==null||!g.IsCurrentCellInEditMode)return;
   var cell=g[e.ColumnIndex,e.RowIndex];string t=Convert.ToString(e.FormattedValue,CultureInfo.InvariantCulture)??"";
   if(string.Equals(t,Convert.ToString(cell.Value,CultureInfo.InvariantCulture)??"",StringComparison.Ordinal)){cell.ErrorText="";
    if(g==RouteGrid&&g.IsCurrentCellDirty)SameTyped045(e.RowIndex,e.ColumnIndex);return;}   // 045 PATCH1 F02: same text actually typed / chosen
   var f=fields[e.ColumnIndex];
   var p=g==RouteGrid?RoomAdjustmentsUi037.CellProblem(f,t):RoomAdjustmentsUi037.SupplementProblem(f,t);
   if(p==null){cell.ErrorText="";return;}
   e.Cancel=true;cell.ErrorText=p;
   SetStatus("Ô "+(g==RouteGrid?RoomAdjustmentsUi037.Caption(f):RoomAdjustmentsUi037.SupplementCaption(f))+" chưa hợp lệ: "+p+" Giá trị cũ giữ nguyên; sửa lại hoặc Esc để huỷ.",true);}
  // SORT-039: the reorder after a KÊNH / LỘ edit runs after this event (BeginInvoke), never inside it.
  void RouteEndEdit(DataGridViewCellEventArgs e){inEndEdit=true;try{RouteEndEditCore(e);}finally{inEndEdit=false;}
   if(reorderPending){if(IsHandleCreated&&!IsDisposed)BeginInvoke((Action)Reorder);else Reorder();}}
  void RouteEndEditCore(DataGridViewCellEventArgs e){bool typed=TakeTyped045(e.RowIndex,e.ColumnIndex);   // 045 PATCH1 F02: consumed at every end of edit
   if(filling||e.RowIndex<0||e.ColumnIndex<0)return;var f=RouteFields[e.ColumnIndex];if(f==null)return;
   var row=RouteGrid.Rows[e.RowIndex];var r=row.Tag as RoomCompletionRow037;if(r==null)return;
   string t=Convert.ToString(row.Cells[e.ColumnIndex].Value,CultureInfo.InvariantCulture)??"";
   if(string.Equals(t,RoomAdjustmentsUi037.CellText(r.input,f),StringComparison.Ordinal)){
    // 045 PATCH1 F02: the same value actually typed over an INHERITED cell = the row's own value (RIÊNG, one undoable change); focus / leave / Esc /
    // an unchanged editor (not dirty) and an already RIÊNG cell keep the old path (nothing changes).
    if(!typed||!RoomFastEntryUi045.Inherited(Session,r.rowKey,f)){RawResolved(RawDraftCodec044.Row,r.rowKey,f);return;}}
   CommitCell(r.rowKey,f,t);}
  void SupplementEndEdit(DataGridViewCellEventArgs e){if(filling||e.RowIndex<0||e.ColumnIndex<0)return;var f=SupplementFields[e.ColumnIndex];if(f==null)return;
   var row=SupplementGrid.Rows[e.RowIndex];var id=row.Tag as string;var x=RoomAdjustmentsUi037.Supplement(Session,id);if(x==null)return;
   string t=Convert.ToString(row.Cells[e.ColumnIndex].Value,CultureInfo.InvariantCulture)??"";
   if(string.Equals(t,RoomAdjustmentsUi037.SupplementText(x,f),StringComparison.Ordinal)){RawResolved(RawDraftCodec044.Supplement,id,f);return;}
   CommitSupplement(id,f,t);}
  // The single apply path of a TUYẾN cell (grid editor and EditCell). Failure: model unchanged, cell repainted with the model value + error.
  bool CommitCell(string rowKey,string field,string text){
   var before=Snapshot045();   // 045: table undo records the state BEFORE an accepted change (nothing recorded when refused or unchanged)
   try{var m=RoomAdjustmentsUi037.ApplyCell(Session,rowKey,field,text);if(m!=null)Record045(before);RawResolved(RawDraftCodec044.Row,rowKey,field);Repaint();
    if(field==RoomAdjustmentsUi037.Circuit){reorderPending=true;if(!inEndEdit)Reorder();}
    SetStatus(m??"Giá trị không đổi.",false);if(m!=null&&Accepted!=null)Accepted();return true;}
   catch(Exception ex){cellError=ex.Message;Repaint();Mark(RouteGrid,Array.IndexOf(RouteFields,field),rowKey,ex.Message);SetStatus(ex.Message,true);return false;}}
  // SORT-039: re-applies the presentation order (RoomRowOrder037) to the TUYẾN rows already shown: same row objects and same filtered set (no Rows.Clear,
  // nothing recalculated), each grid row repainted from its new Tag with sequential STT; selection and current column follow the selected row key.
  // While a cell is in edit mode it stays pending (the next end of edit retries). Rows that are no longer batch rows → full Fill.
  void Reorder(){if(!reorderPending||IsDisposed||filling||Session.batch==null||RouteGrid.IsCurrentCellInEditMode)return;
   reorderPending=false;
   var shown=new HashSet<RoomCompletionRow037>();foreach(DataGridViewRow row in RouteGrid.Rows){var r=row.Tag as RoomCompletionRow037;if(r!=null)shown.Add(r);}
   var order=RoomRowOrder037.Sorted(Session.batch.rows).Where(shown.Contains).ToList();
   if(order.Count!=RouteGrid.Rows.Count){Fill();return;}
   bool same=true;for(int k=0;k<order.Count;k++)if(!ReferenceEquals(RouteGrid.Rows[k].Tag,order[k])){same=false;break;}
   if(same)return;
   var keep=SelectedRow();string keepKey=keep==null?null:keep.rowKey;int col=RouteGrid.CurrentCell==null?-1:RouteGrid.CurrentCell.ColumnIndex;
   filling=true;
   try{for(int k=0;k<order.Count;k++){var row=RouteGrid.Rows[k];row.Tag=order[k];PaintRoute(row,order[k],k+1);}
    RouteGrid.ClearSelection();
    if(keepKey!=null)foreach(DataGridViewRow row in RouteGrid.Rows){var r=row.Tag as RoomCompletionRow037;if(r==null||!string.Equals(r.rowKey,keepKey,StringComparison.Ordinal))continue;
     if(col>=0&&row.Cells[col].Visible)RouteGrid.CurrentCell=row.Cells[col];else FocusRow045(row);break;}}   // 045: cell-range grid
   finally{filling=false;}
   Detail();}
  bool CommitSupplement(string id,string field,string text){
   try{var m=RoomAdjustmentsUi037.EditSupplement(Session,id,field,text);RawResolved(RawDraftCodec044.Supplement,id,field);Repaint();SetStatus(m??"Giá trị không đổi.",false);
    if(m!=null&&Accepted!=null)Accepted();return true;}
   catch(Exception ex){cellError=ex.Message;Repaint();Mark(SupplementGrid,Array.IndexOf(SupplementFields,field),id,ex.Message);SetStatus(ex.Message,true);return false;}}
  static void Mark(DataGridView g,int col,string tag,string msg){if(col<0)return;
   foreach(DataGridViewRow r in g.Rows){var x=r.Tag as RoomCompletionRow037;string k=x!=null?x.rowKey:r.Tag as string;if(string.Equals(k,tag,StringComparison.Ordinal)){r.Cells[col].ErrorText=msg;return;}}}
  // QA / keyboard-free entry (same path as the grid editor): commits any pending cell first, then applies ONE field. false = refused (status + cell error).
  public bool EditCell(string rowKey,string field,string text){if(!CommitEdits())return false;
   if(field==null||Array.IndexOf(RouteFields,field)<0){SetStatus("Cột \""+field+"\" không sửa được trong bảng.",true);return false;}
   return CommitCell(rowKey,field,text);}
  public bool EditSupplement(string id,string field,string text){if(!CommitEdits())return false;
   if(field==null||Array.IndexOf(SupplementFields,field)<0){SetStatus("Cột \""+field+"\" không sửa được.",true);return false;}
   return CommitSupplement(id,field,text);}
  // QA helper: types text into the cell editor WITHOUT committing (pending edit, as if the user had typed it); the next action must commit or refuse it.
  // false = the cell is not shown / not editable / the editor did not open (e.g. form not shown) / the drop-down has no such item.
  public bool StartEdit(string rowKey,string field,string text){return Start(RouteGrid,RouteTab,RouteFields,field,rowKey,text);}
  public bool StartSupplementEdit(string id,string field,string text){return Start(SupplementGrid,SupplementTab,SupplementFields,field,id,text);}
  bool Start(DataGridView g,TabPage tab,string[] fields,string field,string tag,string text){int c=field==null?-1:Array.IndexOf(fields,field);if(c<0)return false;
   foreach(DataGridViewRow row in g.Rows){var x=row.Tag as RoomCompletionRow037;string k=x!=null?x.rowKey:row.Tag as string;if(!string.Equals(k,tag,StringComparison.Ordinal))continue;
    if(row.Cells[c].ReadOnly||!Tabs.TabPages.Contains(tab)||!g.Columns[c].Visible)return false;   // 045: a hidden (compact) column cannot be current
    Tabs.SelectedTab=tab;g.CurrentCell=row.Cells[c];if(!g.BeginEdit(false)||g.EditingControl==null)return false;
    var cb=g.EditingControl as ComboBox;
    if(cb!=null){int i=cb.Items.IndexOf(text??"");if(i<0)return false;cb.SelectedIndex=i;}else g.EditingControl.Text=text??"";
    g.NotifyCurrentCellDirty(true);return true;}
   return false;}

  // ---- S5 BỎ DÒNG / BỔ SUNG actions (project mode; buttons and QA call these) ----
  bool Act(Func<string> f){if(!CommitEdits())return false;
   try{var m=f();Fill();SetStatus(m,false);if(Accepted!=null)Accepted();return true;}catch(Exception ex){Fill();SetStatus(ex.Message,true);return false;}}
  public bool ExcludeRow(string rowKey,string reason){return Act(()=>RoomAdjustmentsUi037.Exclude(Session,rowKey,reason));}
  public bool RestoreRow(string rowKey){return Act(()=>RoomAdjustmentsUi037.Restore(Session,rowKey));}
  // reason null/blank = keep the recorded reason.
  public bool ReconfirmRow(string rowKey,string reason){return Act(()=>RoomAdjustmentsUi037.Reconfirm(Session,rowKey,reason));}
  // New supplement ID (selected in BỔ SUNG), null = refused.
  public string AddSupplementRow(){string id=null;
   if(!Act(()=>{string n;var m=RoomAdjustmentsUi037.AddSupplement(Session,()=>Guid.NewGuid(),out n);id=n;return m;}))return null;
   Tabs.SelectedTab=SupplementTab;Pick(SupplementGrid,id);return id;}
  public bool SetSupplementActive(string id,bool active){return Act(()=>RoomAdjustmentsUi037.SetSupplementActive(Session,id,active));}
  public bool ConfirmSupplementOverlap(string id,bool confirmed){return Act(()=>RoomAdjustmentsUi037.ConfirmOverlap(Session,id,confirmed));}
  void Detail(){var sb=new StringBuilder();var r=Tabs.SelectedTab==RouteTab?SelectedRow():null;DeclareBtn.Enabled=r!=null;Buttons045();
   // S5 button states (project mode only; the strips are hidden otherwise).
   var rec=r==null?null:RoomAdjustmentsUi037.Exclusion(Session,r.rowKey);
   ExcludeBtn.Enabled=r!=null&&(rec==null||!rec.active);RestoreBtn.Enabled=rec!=null&&rec.active;
   var exKey=SelectedExclusionKey();var exRec=RoomAdjustmentsUi037.Exclusion(Session,exKey);var exSt=RoomAdjustmentsUi037.ExclusionState(Session,exKey);
   RestoreRecordBtn.Enabled=exRec!=null&&exRec.active;ReconfirmBtn.Enabled=exRec!=null&&exSt!=null&&exSt.status==RoomAdjustments037.SignatureMismatch;
   var supId=SelectedSupplementId();var sup=RoomAdjustmentsUi037.Supplement(Session,supId);var supSt=RoomAdjustmentsUi037.SupplementState(Session,supId);
   AddSupplementBtn.Enabled=Session.batch!=null;RemoveSupplementBtn.Enabled=sup!=null&&sup.active;RestoreSupplementBtn.Enabled=sup!=null&&!sup.active;
   OverlapBtn.Enabled=sup!=null&&sup.active&&(sup.overlapConfirmed||(supSt!=null&&supSt.overlap));
   OverlapBtn.Text=sup!=null&&sup.overlapConfirmed?"BỎ XÁC NHẬN CỘNG THÊM":"XÁC NHẬN CỘNG THÊM";
   if(Tabs.SelectedTab==SupplementTab){
    if(sup!=null){sb.Append("Dòng bổ sung ").Append(sup.id).Append(" · phòng ").Append(RoomProjectUi037.RoomLabel(Session)).Append(" · ").Append(sup.active?"đang dùng":"đã xoá (không tính)").Append("\r\n");
     if(supSt!=null){foreach(var m in supSt.issues)sb.Append("Cần kiểm tra: ").Append(RoomLook037.Friendly(m)).Append("\r\n");foreach(var m in supSt.missing)sb.Append("Thiếu: ").Append(RoomLook037.Friendly(m)).Append("\r\n");
      if(supSt.overlap)sb.Append("Phòng này cũng có ").Append(sup.kind).Append(" ").Append(sup.materialCode).Append(" đo từ CAD. Xem lại rồi bấm XÁC NHẬN CỘNG THÊM nếu đây là phần thêm (MEPS không tự gộp/trừ).\r\n");}}
    sb.Append("Khối lượng bổ sung là số mét nhập tay (đơn vị m), cộng riêng cột BỔ SUNG; không nhân hệ số, không thêm đầu chờ, không tạo tuyến hay thiết bị CAD. Ô trống = chưa nhập; 0 là giá trị.");
    DetailBox.Text=sb.ToString();return;}
   if(Tabs.SelectedTab==ExclusionTab){
    if(exRec!=null)sb.Append("Khoá dòng ").Append(exRec.rowKey).Append(" · lý do: ").Append(exRec.reason).Append(" · ").Append(RoomAdjustmentsUi037.ExclusionStatusLabel(exSt==null?null:exSt.status)).Append("\r\n")
     .Append(exSt==null?"":exSt.message).Append("\r\n");
    sb.Append("Bỏ dòng chỉ bỏ dòng khỏi phần tính, không xoá hình CAD. Bản ghi gắn với đúng khoá dòng và lần quét; khi hình đổi phải XÁC NHẬN LẠI hoặc KHÔI PHỤC (MEPS không ghép theo nhãn). Lỗi bản vẽ của dòng đã bỏ vẫn chặn kết quả.");
    DetailBox.Text=sb.ToString();return;}
   if(r!=null){var x=r.result;var i=r.input;
    sb.Append("Tuyến: ").Append(r.startLabel).Append(" → ").Append(r.endLabel).Append(" · ").Append(RoomLook037.Status(r.status)).Append("\r\n");
    if(r.excluded)sb.Append("ĐÃ BỎ DÒNG (lý do: ").Append(r.exclusionReason).Append("): không tính khối lượng; hình CAD và thông số giữ nguyên; KHÔI PHỤC để tính lại.\r\n");
    else if(rec!=null&&rec.active){var st=RoomAdjustmentsUi037.ExclusionState(Session,r.rowKey);if(st!=null)sb.Append("Bản ghi bỏ dòng CHƯA áp dụng: ").Append(st.message).Append("\r\n");}
    sb.Append("Kênh điều khiển (nhãn TAG trong phòng, không phải lộ/tủ suy luận): ").Append(r.channelLabel??"—");
    if(!string.IsNullOrEmpty(r.structuredKey))sb.Append(" · lộ cấu trúc ").Append(r.structuredKey);
    sb.Append("\r\nKC mặt bằng đo từ CAD: ").Append(BdgklLook034.R(r.planMetres)).Append(" m (").Append(Route034.Units(r.drawingLength)).Append(" đơn vị bản vẽ, tổng các đoạn dây truy vết)\r\n");
    foreach(var m in r.issues)sb.Append("Cần kiểm tra: ").Append(RoomLook037.Friendly(m)).Append("\r\n");
    foreach(var m in r.missing)sb.Append("Thiếu: ").Append(RoomLook037.Friendly(m)).Append("\r\n");
    foreach(var h in r.horizontalShares.Where(q=>q.role==RoomCompletion037.RoleDeducted))sb.Append("Ống ngang đi chung: đoạn ").Append(h.segmentId).Append(" ").Append(BdgklLook034.R(h.lengthMetres)).Append(" m tính ở dòng ").Append(h.ownerRowKey).Append("\r\n");
    foreach(var v in r.verticalShares.Where(q=>q.role==RoomCompletion037.RoleDeducted))sb.Append("Ống xuống thiết bị đi chung: ").Append(v.deviceId).Append(" ").Append(BdgklLook034.R(v.dropMetres)).Append(" m tính ở dòng ").Append(v.ownerRowKey).Append("\r\n");
    if(x!=null&&x.equation!=null)sb.Append("Phép tính:\r\n").Append(x.equation.Replace("\n","\r\n")).Append("\r\n");
    sb.Append("Chi tiết kỹ thuật: khoá ").Append(r.rowKey).Append(" · nguồn ").Append(r.sourceKind).Append(' ').Append(r.sourceId).Append(" (").Append(r.sourceStatus)
     .Append(r.sourceReasons.Count>0?": "+string.Join("; ",r.sourceReasons):"").Append(") · thiết bị ").Append(RoomUi037.EndText(r.startType,r.startName,r.startLabel,r.startId)).Append(" / ").Append(RoomUi037.EndText(r.endType,r.endName,r.endLabel,r.endId))
     .Append(" · đoạn ").Append(string.Join(", ",r.segments.Select(g=>g.id+"="+Bdgkl034.N(g.length))));
    sb.Append(Provenance045(r));}   // 045: source of every inherited / RIÊNG field
   else if(Tabs.SelectedTab==RouteTab&&RangeText045()!=null)sb.Append(RangeText045());
   else if(Tabs.SelectedTab==IssueTab)sb.Append("Vướng mắc chặn KẾT QUẢ CHÍNH THỨC và xuất Excel. Sửa trong bản vẽ (dây, block, TAG) rồi bấm 'Cập nhật'. Nhập số trong 'Thông số' không xoá được vướng mắc bản vẽ.");
   else sb.Append("Dây = mét tuyến của bộ dây khai báo cho từng dòng (không nhân số lõi). Ống: ngang/đứng, chìm/nổi theo cách phân loại của mẫu BDGKL; dây chờ không tính ống. Chỉ cộng dòng đủ dữ liệu.");
   DetailBox.Text=sb.ToString();}
  void DoDeclare(){if(!CommitEdits())return;var r=SelectedRow();if(r==null){SetStatus("Chọn MỘT dòng ở tab TUYẾN trước khi bấm 'Thông số' (vùng chọn nhiều dòng không dùng cho hộp thông số).",true);return;}
   var before=Snapshot045();   // 045: the dialog change is one undoable table step
   try{if(Declare==null)throw new IOE("Chưa nối với AutoCAD.");var m=Declare(r.rowKey,this);if(m==null){SetStatus("Đã huỷ sửa thông số; không đổi gì.",false);return;}Record045(before);Fill();SetStatus(m,false);if(Accepted!=null)Accepted();}
   catch(Exception ex){SetStatus("Chưa áp dụng thông số: "+ex.Message,true);}}
  void DoSave(){if(!CommitEdits())return;if(Session.batch==null&&!(Session.projectMode&&Session.projectDocument!=null)){SetStatus(Session.projectMode?"Chưa có dự án: bấm 'DỰ ÁN / TẦNG / PHÒNG' trước; chưa có gì để lưu.":"Chưa chọn phòng; chưa có gì để lưu.",true);return;}
   ReviewConfirmed=ReviewBox.Visible&&ReviewBox.Checked;Chosen=Choice.Save;DialogResult=DialogResult.OK;}
  public void DoProject(){if(!CommitEdits())return;try{if(Project==null)throw new IOE("Chưa nối với AutoCAD.");var m=Project(this);if(m==null){SetStatus("Đã huỷ thiết lập dự án; không đổi gì.",false);return;}Fill();SetStatus(m,false);if(Accepted!=null)Accepted();}
   catch(Exception ex){SetStatus("Chưa áp dụng thiết lập dự án: "+ex.Message,true);}}
  public void DoDevices(){if(!CommitEdits())return;if(Session.batch==null){SetStatus("Chọn vùng CAD (quét phòng của dự án) trước; cao độ riêng áp cho thiết bị thực của phòng đang mở.",true);return;}
   try{if(Devices==null)throw new IOE("Chưa nối với AutoCAD.");var m=Devices(this);if(m==null){SetStatus("Đã huỷ sửa cao độ thiết bị; không đổi gì.",false);return;}Fill();SetStatus(m,false);if(Accepted!=null)Accepted();}
   catch(Exception ex){SetStatus("Chưa áp dụng cao độ thiết bị: "+ex.Message,true);}}
  public void DoHeights(){if(!CommitEdits())return;if(Session.batch==null){SetStatus("Chọn vùng CAD trước; hộp cao độ áp dụng cho các thiết bị của phòng đang mở.",true);return;}
   try{if(Heights==null)throw new IOE("Chưa nối với AutoCAD.");var m=Heights(this);if(m==null){SetStatus("Đã huỷ thiết lập cao độ; không đổi gì.",false);return;}Fill();SetStatus(m,false);if(Accepted!=null)Accepted();}
   catch(Exception ex){SetStatus("Chưa áp dụng cao độ: "+ex.Message,true);}}
  // Project mode (S4): no open-room pre-check here; the scope chooser and the per-room checks decide (a floor/project export needs no open room).
  public void DoExport(){if(!CommitEdits())return;try{var why=Session.projectMode?null:RoomUi037.ExportReason(Session);if(why!=null){SetStatus("Chưa xuất được Excel: "+why,true);return;}
    if(Export==null)throw new IOE("Chưa nối với AutoCAD.");var m=Export(this);Fill();SetStatus(m,false);}
   catch(Exception ex){Fill();SetStatus("Chưa xuất Excel: "+ex.Message,true);}}
  public void DoRefresh(){if(!CommitEdits())return;try{if(Rescan==null)throw new IOE("Chưa nối với AutoCAD.");var m=Rescan();Fill();SetStatus(m,false);if(Accepted!=null)Accepted();}catch(Exception ex){SetStatus("Chưa cập nhật: "+ex.Message,true);}}
  protected override void Dispose(bool disposing){base.Dispose(disposing);if(disposing){face.Dispose();bold.Dispose();}}
 }

 // ---- Room options (name, unit, tolerance) ----
 public sealed class RoomOptionsDialog037:Form {
  public readonly ComboBox RoomBox=new ComboBox(),UnitBox=new ComboBox();public readonly TextBox ToleranceBox=new TextBox(),InfoBox=new TextBox(),StatusBox=new TextBox();
  public readonly CheckBox UnitAckBox=new CheckBox(),UseSavedBox=new CheckBox();public readonly Button OkBtn=new Button(),CancelBtn=new Button();
  public readonly List<RoomStoreEntry037> Entries;public readonly int Insunits;public RoomOptions037 Result;
  // S3 project mode: RoomBox lists the project rooms (floor · code — description); RoomIds[i] = stable room ID of item i; Entries[i] = its draft (key = room ID).
  // DiscardBox is shown only when the open room has unsaved declarations that cannot be kept as a draft (scan incomplete); it must be ticked explicitly.
  public readonly CheckBox DiscardBox=new CheckBox();public readonly List<string> RoomIds=new List<string>();public readonly bool ProjectMode,MustConfirmDiscard;
  readonly float dpi;readonly Font face;readonly string baseInfo;bool loading;
  int S(int px){return Math.Max(1,(int)Math.Round(px*dpi));}
  public RoomOptionsDialog037(List<RoomStoreEntry037> entries,string listError,int insunits,RoomSession037 current,float dpi):this(entries,listError,insunits,current,dpi,null,false){}
  public RoomOptionsDialog037(RoomProjectDocument037 project,int insunits,RoomSession037 current,bool confirmDiscard,float dpi):this(ProjectEntries(project),null,insunits,current,dpi,project,confirmDiscard){}
  // One entry per project room in floor order (draft decoded from the given document; error kept, never repaired).
  public static List<RoomStoreEntry037> ProjectEntries(RoomProjectDocument037 project){var l=new List<RoomStoreEntry037>();if(project==null||project.model==null)return l;
   foreach(var f in project.model.hierarchy.floors.OrderBy(x=>x.order).ThenBy(x=>x.id,StringComparer.Ordinal))foreach(var r in f.rooms){string err;var d=RoomProjectUi037.DraftOf(project,r.id,out err);
    l.Add(new RoomStoreEntry037{key=r.id,roomId=r.id,draft=d,error=err,raw=null});}
   return l;}
  RoomOptionsDialog037(List<RoomStoreEntry037> entries,string listError,int insunits,RoomSession037 current,float dpi,RoomProjectDocument037 project,bool confirmDiscard){
   Entries=entries??new List<RoomStoreEntry037>();Insunits=insunits;this.dpi=BdgklLook034.Dpi(dpi);ProjectMode=project!=null;MustConfirmDiscard=ProjectMode&&confirmDiscard;
   Text="MEPS — Chọn phòng";Name="room037_options";FormBorderStyle=FormBorderStyle.Sizable;MinimizeBox=false;MaximizeBox=false;ShowInTaskbar=false;StartPosition=FormStartPosition.CenterParent;
   AutoScaleMode=AutoScaleMode.None;face=RoomLook037.Face(this.dpi,false);Font=face;BackColor=BdgklLook034.Back;ForeColor=BdgklLook034.Ink;ClientSize=new Size(S(680),S(560));MinimumSize=new Size(S(560),S(460));
   var pane=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=2,RowCount=8,Padding=new Padding(S(8)),BackColor=BdgklLook034.Back};
   pane.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,S(220)));pane.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
   foreach(int h in new[]{S(32),S(32),S(32),S(48),S(36),confirmDiscard?S(48):0,-100,S(44)})pane.RowStyles.Add(h<0?new RowStyle(SizeType.Percent,-h):new RowStyle(SizeType.Absolute,h));
   UnitBox.Items.Add(new RouteDialog034.Choice(null,"(chọn đơn vị)"));UnitBox.Items.Add(new RouteDialog034.Choice(RoomQty037.MM,"MM — 1000 đơn vị bản vẽ = 1 m"));UnitBox.Items.Add(new RouteDialog034.Choice(RoomQty037.M,"M — 1 đơn vị bản vẽ = 1 m"));
   if(ProjectMode)foreach(var en in Entries){HierFloor037 fl;var hr=RoomProjectUi037.FindRoom(project.model,en.roomId,out fl);RoomIds.Add(en.roomId);
    RoomBox.Items.Add("TẦNG "+(fl==null?"?":fl.name)+" · "+RoomProjectUi037.RoomLabel(hr));}
   else foreach(var en in Entries)RoomBox.Items.Add(en.roomId??("(bản nháp không đọc được "+en.key+")"));
   var rows=new[]{Tuple.Create(ProjectMode?"Phòng của dự án (mã — mô tả)":"Tên phòng (bắt buộc, dùng làm khoá bản nháp)",(Control)RoomBox,"room037_room"),Tuple.Create("Đơn vị bản vẽ",(Control)UnitBox,"room037_unit"),
    Tuple.Create("Dung sai nối (đơn vị bản vẽ, ≥ 0)",(Control)ToleranceBox,"room037_tolerance")};
   for(int i=0;i<rows.Length;i++){pane.Controls.Add(new Label{Text=rows[i].Item1,Dock=DockStyle.Fill,AutoEllipsis=true,ForeColor=BdgklLook034.Muted,TextAlign=ContentAlignment.MiddleLeft},0,i);
    var c=rows[i].Item2;c.Name=rows[i].Item3;c.AccessibleName=rows[i].Item1;c.Dock=DockStyle.Fill;c.Margin=new Padding(0,S(3),0,S(3));pane.Controls.Add(c,1,i);}
   if(ProjectMode)RoomBox.DropDownStyle=ComboBoxStyle.DropDownList;else{RoomBox.DropDownStyle=ComboBoxStyle.DropDown;RoomBox.MaxLength=Bdgkl034.MaxText;}
   UnitBox.DropDownStyle=ComboBoxStyle.DropDownList;ToleranceBox.BorderStyle=BorderStyle.FixedSingle;
   UnitAckBox.Name="room037_unit_ack";UnitAckBox.AccessibleName="Xác nhận đơn vị khác INSUNITS";UnitAckBox.ForeColor=BdgklLook034.Error;UnitAckBox.Dock=DockStyle.Fill;
   UseSavedBox.Name="room037_use_saved";UseSavedBox.AccessibleName="Dùng lại vùng đã lưu";UseSavedBox.Text="Dùng lại vùng chữ nhật đã lưu của phòng này (không chọn góc lại)";UseSavedBox.Dock=DockStyle.Fill;
   pane.Controls.Add(UnitAckBox,0,3);pane.SetColumnSpan(UnitAckBox,2);pane.Controls.Add(UseSavedBox,0,4);pane.SetColumnSpan(UseSavedBox,2);
   DiscardBox.Name="room037_discard_pending";DiscardBox.AccessibleName="Bỏ thay đổi chưa lưu của phòng đang mở";DiscardBox.ForeColor=BdgklLook034.Error;DiscardBox.Dock=DockStyle.Fill;
   DiscardBox.Visible=confirmDiscard;DiscardBox.Text="Bỏ thay đổi chưa lưu của phòng \""+(current==null?"":RoomProjectUi037.RoomLabel(current))+
    "\" (lần quét chưa hoàn tất nên không giữ được làm bản nháp). Không tích = giữ phiên đang mở.";
   pane.Controls.Add(DiscardBox,0,5);pane.SetColumnSpan(DiscardBox,2);
   BdgklLook034.ReadOnlyBox(InfoBox);InfoBox.Name="room037_options_info";InfoBox.AccessibleName="Giải thích";pane.Controls.Add(InfoBox,0,6);pane.SetColumnSpan(InfoBox,2);
   OkBtn.Text="Tiếp tục";OkBtn.Name="room037_options_ok";CancelBtn.Text="Hủy";CancelBtn.Name="room037_options_cancel";CancelBtn.DialogResult=DialogResult.Cancel;
   var bar=new FlowLayoutPanel{FlowDirection=System.Windows.Forms.FlowDirection.RightToLeft,Dock=DockStyle.Fill,WrapContents=false,Margin=new Padding(0),BackColor=BdgklLook034.Back};
   BdgklLook034.ReadOnlyBox(StatusBox);StatusBox.Name="room037_options_status";StatusBox.BorderStyle=BorderStyle.None;StatusBox.BackColor=BdgklLook034.Back;StatusBox.Dock=DockStyle.None;StatusBox.Width=S(360);StatusBox.Height=S(36);
   foreach(var b in new[]{CancelBtn,OkBtn}){BdgklLook034.Style(b,b==OkBtn,face,S(72),S(30));b.Margin=new Padding(S(4),S(4),0,0);b.AccessibleName=b.Text;bar.Controls.Add(b);}bar.Controls.Add(StatusBox);
   pane.Controls.Add(bar,0,7);pane.SetColumnSpan(bar,2);Controls.Add(pane);CancelButton=CancelBtn;AcceptButton=OkBtn;
   string unitHint=insunits==4?"4 (mm) → gợi ý MM":insunits==6?"6 (m) → gợi ý M":insunits==0?"0 (chưa đặt) → không có gợi ý":insunits.ToString(CultureInfo.InvariantCulture)+" (không phải mm/m)";
   baseInfo="INSUNITS của bản vẽ = "+unitHint+"; chỉ là gợi ý, bạn tự chọn đơn vị. MEPS không đổi INSUNITS.\r\n"+
    "Dung sai nối: khoảng cách lớn nhất (đơn vị bản vẽ) để coi đầu dây và điểm chèn thiết bị là cùng một điểm. Chỉ dùng để nối, không cộng vào chiều dài; không có giá trị mặc định (0 = trùng đúng).\r\n"+
    "Sau 'Tiếp tục' hộp này đóng; chọn HAI GÓC ĐỐI DIỆN của phòng trên Model (UCS phải song song World). Chỉ dây vẽ bằng DÂY DẪN và thiết bị MEPS có TAG trong vùng được đọc; Esc huỷ và giữ phiên đang mở."+
    (listError!=null?"\r\nKHÔNG đọc được kho bản nháp của bản vẽ: "+listError+" (MEPS không ghi đè).":"")+
    (ProjectMode?"\r\nChỉ chọn phòng ĐÃ CÓ trong dự án (tạo ở 'DỰ ÁN / TẦNG / PHÒNG'); MEPS không tự tạo phòng."+
     (current!=null&&current.dirty&&!confirmDiscard?"\r\nThay đổi chưa lưu của phòng \""+RoomProjectUi037.RoomLabel(current)+"\" được GIỮ làm bản nháp trong phiên (chưa ghi vào bản vẽ cho tới khi Lưu).":""):
     (current!=null&&current.dirty?"\r\nLƯU Ý: phòng \""+current.roomId+"\" đang có thay đổi chưa lưu; chọn phòng mới sẽ bỏ các thay đổi đó (bản vẽ không đổi).":""));
   loading=true;
   try{if(ProjectMode){string want=current==null?null:current.roomId??current.selectedRoomId;int k=want==null?-1:RoomIds.IndexOf(want);if(k>=0)RoomBox.SelectedIndex=k;}
    if(current!=null&&current.hasRegion){if(!ProjectMode)RoomBox.Text=current.roomId;Choose(UnitBox,current.units);ToleranceBox.Text=RoomLook037.Exact(current.tolerance);UnitAckBox.Checked=current.unitConfirmed;}
    else{var e0=ProjectMode?Entry():null;   // preselected project room with a draft: ITS saved unit/tolerance (user data, not a default)
     if(e0!=null&&e0.draft!=null){Choose(UnitBox,e0.draft.units);ToleranceBox.Text=RoomLook037.Exact(e0.draft.tolerance);UnitAckBox.Checked=e0.draft.unitConfirmed;UseSavedBox.Checked=true;}
     else UnitBox.SelectedIndex=0;}}
   finally{loading=false;}
   RoomBox.SelectedIndexChanged+=(s,e)=>Picked();RoomBox.TextChanged+=(s,e)=>Changed();UnitBox.SelectedIndexChanged+=(s,e)=>Changed();ToleranceBox.TextChanged+=(s,e)=>Changed();
   OkBtn.Click+=(s,e)=>TryAccept();Changed();}
  public void Choose(ComboBox b,string value){for(int i=0;i<b.Items.Count;i++)if(string.Equals(((RouteDialog034.Choice)b.Items[i]).value,value,StringComparison.Ordinal)){b.SelectedIndex=i;return;}b.SelectedIndex=0;}
  static string Value(ComboBox b){var c=b.SelectedItem as RouteDialog034.Choice;return c==null?null:c.value;}
  RoomStoreEntry037 Entry(){if(ProjectMode){int i=RoomBox.SelectedIndex;return i>=0&&i<Entries.Count?Entries[i]:null;}
   var t=RoomBox.Text;return Entries.FirstOrDefault(e=>e.roomId!=null&&string.Equals(e.roomId,t,StringComparison.Ordinal));}
  // QA helper (project mode): selects the room by stable ID; false = not in the list.
  public bool SelectRoom(string roomId){int k=RoomIds.IndexOf(roomId);if(k<0)return false;RoomBox.SelectedIndex=k;return true;}
  // Picking a saved room from the list reloads ITS saved unit/tolerance (user data, not a default) and offers its saved rectangle.
  void Picked(){if(loading)return;var e=RoomBox.SelectedIndex>=0&&RoomBox.SelectedIndex<Entries.Count?Entries[RoomBox.SelectedIndex]:null;
   if(e!=null&&e.draft!=null){loading=true;try{Choose(UnitBox,e.draft.units);ToleranceBox.Text=RoomLook037.Exact(e.draft.tolerance);UnitAckBox.Checked=e.draft.unitConfirmed;UseSavedBox.Checked=true;}finally{loading=false;}}
   Changed();}
  public string UnitWarning(){var u=Value(UnitBox);if(u==null||Insunits==0)return null;int want=u==RoomQty037.MM?4:6;
   return Insunits==want?null:"Đơn vị "+u+" khác INSUNITS = "+Insunits.ToString(CultureInfo.InvariantCulture)+" của bản vẽ.";}
  void Changed(){if(loading)return;var e=Entry();var w=UnitWarning();
   UnitAckBox.Visible=w!=null;UnitAckBox.Text=w==null?"":"Xác nhận dùng đơn vị "+Value(UnitBox)+" dù "+w+" (MEPS không đổi INSUNITS)";if(w==null)UnitAckBox.Checked=false;
   UseSavedBox.Enabled=e!=null&&e.draft!=null;if(!UseSavedBox.Enabled)UseSavedBox.Checked=false;
   var info=new StringBuilder(baseInfo);
   if(e!=null&&(e.draft!=null||e.error!=null))info.Append("\r\n").Append(e.draft!=null?"Phòng này đã có bản nháp "+(ProjectMode?"(đã lưu hoặc đang giữ trong phiên) ":"lưu ")+"lúc "+e.draft.savedAt+
     " ("+e.draft.rows.Count+" dòng khai báo); tiếp tục sẽ mở bản nháp đó.":"Bản nháp của phòng này KHÔNG đọc được: "+e.error+" (MEPS không ghi đè).");
   else if(e!=null&&ProjectMode)info.Append("\r\nPhòng này chưa có bản nháp; khai báo bắt đầu trống.");
   InfoBox.Text=info.ToString();StatusBox.Text="";}
  public bool TryAccept(){try{string room;
    if(ProjectMode){int k=RoomBox.SelectedIndex;if(k<0||k>=RoomIds.Count)throw new IOE("Chọn một phòng của dự án (tạo phòng ở 'DỰ ÁN / TẦNG / PHÒNG').");room=RoomIds[k];
     if(MustConfirmDiscard&&!DiscardBox.Checked)throw new IOE("Phòng đang mở có thay đổi chưa lưu không giữ được: tích ô 'Bỏ thay đổi…' để tiếp tục, hoặc Hủy rồi Lưu/Cập nhật trước.");}
    else{room=RoomBox.Text;string p=Bdgkl034.TextProblem(room);if(p!=null)throw new IOE("Tên phòng "+p+".");}
    var u=Value(UnitBox);if(u==null)throw new IOE("Chọn đơn vị bản vẽ (MM hoặc M).");
    var tol=BdgklUi034.ParseDecimal(ToleranceBox.Text,"Dung sai nối");if(!tol.HasValue)throw new IOE("Nhập dung sai nối (≥ 0, đơn vị bản vẽ); MEPS không tự chọn.");
    var w=UnitWarning();if(w!=null&&!UnitAckBox.Checked)throw new IOE(w+" Tích ô xác nhận hoặc chọn đơn vị khác.");
    var e=Entry();bool useSaved=UseSavedBox.Checked&&e!=null&&e.draft!=null;
    Result=new RoomOptions037{roomId=room,units=u,tolerance=tol.Value,unitConfirmed=w!=null&&UnitAckBox.Checked,useSavedRegion=useSaved,discardPending=MustConfirmDiscard&&DiscardBox.Checked};}
   catch(Exception ex){Result=null;StatusBox.ForeColor=BdgklLook034.Error;StatusBox.Text=ex.Message;return false;}
   DialogResult=DialogResult.OK;return true;}
  protected override void Dispose(bool disposing){base.Dispose(disposing);if(disposing)face.Dispose();}
 }

 // ---- Declaration of ONE row (+ room-wide choices) ----
 public sealed class RoomDeclareDialog037:Form {
  public readonly TextBox InfoBox=new TextBox(),FloorBox=new TextBox(),CircuitBox=new TextBox(),CeilingBox=new TextBox(),StartValueBox=new TextBox(),EndValueBox=new TextBox(),
   TailCountBox=new TextBox(),TailEachBox=new TextBox(),FactorBox=new TextBox(),AssemblyBox=new TextBox(),ConduitBox=new TextBox(),PreviewBox=new TextBox(),StatusBox=new TextBox();
  public readonly ComboBox ModeBox=new ComboBox(),StartModeBox=new ComboBox(),EndModeBox=new ComboBox(),ConduitModeBox=new ComboBox();
  public readonly CheckBox IndependentBox=new CheckBox(),AllFloorBox=new CheckBox(),AllCeilingBox=new CheckBox(),AllTailsBox=new CheckBox(),AllFactorBox=new CheckBox(),AllAssemblyBox=new CheckBox(),AllConduitBox=new CheckBox();
  public readonly Button OkBtn=new Button(),CancelBtn=new Button();
  public readonly RoomSession037 Session;public readonly RoomCompletionRow037 Row;public RoomDeclaration037 Result;
  readonly float dpi;readonly Font face;bool loading;
  int S(int px){return Math.Max(1,(int)Math.Round(px*dpi));}
  public RoomDeclareDialog037(RoomSession037 session,RoomCompletionRow037 row,float dpi){if(session==null||session.batch==null||row==null||row.input==null)throw new ArgumentException("Cần một dòng của phòng đang mở.");
   Session=session;Row=row;this.dpi=BdgklLook034.Dpi(dpi);
   Text="MEPS — Thông số tuyến trong phòng";Name="room037_declare_dialog";FormBorderStyle=FormBorderStyle.Sizable;MinimizeBox=false;MaximizeBox=false;ShowInTaskbar=false;StartPosition=FormStartPosition.CenterParent;
   AutoScaleMode=AutoScaleMode.None;face=RoomLook037.Face(this.dpi,false);Font=face;BackColor=BdgklLook034.Back;ForeColor=BdgklLook034.Ink;ClientSize=new Size(S(720),S(700));MinimumSize=new Size(S(600),S(500));
   var pane=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=1,RowCount=5,Padding=new Padding(S(8)),BackColor=BdgklLook034.Back};pane.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
   foreach(int h in new[]{S(96),-62,-38,S(44),S(44)})pane.RowStyles.Add(h<0?new RowStyle(SizeType.Percent,-h):new RowStyle(SizeType.Absolute,h));
   foreach(var b in new[]{InfoBox,PreviewBox,StatusBox})BdgklLook034.ReadOnlyBox(b);
   InfoBox.Name="room037_declare_info";InfoBox.AccessibleName="Tuyến đã chọn (chỉ đọc)";PreviewBox.Name="room037_declare_preview";PreviewBox.AccessibleName="Kiểm tra dòng này";
   StatusBox.Name="room037_declare_status";StatusBox.AccessibleName="Thông báo";StatusBox.BackColor=BdgklLook034.Back;StatusBox.BorderStyle=BorderStyle.None;
   var scroll=new Panel{Dock=DockStyle.Fill,AutoScroll=true,BackColor=BdgklLook034.Back,Margin=new Padding(0,S(4),0,S(4))};
   var grid=new TableLayoutPanel{ColumnCount=3,AutoSize=true,AutoSizeMode=System.Windows.Forms.AutoSizeMode.GrowAndShrink,Dock=DockStyle.Top,BackColor=BdgklLook034.Back};
   grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,S(300)));grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,S(130)));
   ModeBox.Items.Add(new RouteDialog034.Choice(null,"(chọn hướng đi)"));ModeBox.Items.Add(new RouteDialog034.Choice(Bdgkl034.Ceiling,"TRẦN — lên trần rồi xuống"));
   ModeBox.Items.Add(new RouteDialog034.Choice(Bdgkl034.Floor,"SÀN — xuống sàn rồi lên"));ModeBox.Items.Add(new RouteDialog034.Choice(Bdgkl034.Wall,"TƯỜNG — đi dọc tường"));
   foreach(var m in new[]{StartModeBox,EndModeBox}){m.Items.Add(new RouteDialog034.Choice(null,"(chọn cách khai báo)"));m.Items.Add(new RouteDialog034.Choice(Bdgkl034.Fixed,"Cố định (cao độ từ sàn hoàn thiện)"));
    m.Items.Add(new RouteDialog034.Choice(Bdgkl034.CeilingOffset,"Hạ dưới trần (khoảng hạ)"));}
   ConduitModeBox.Items.Add(new RouteDialog034.Choice(null,"(chọn cách đi ống của phòng)"));
   ConduitModeBox.Items.Add(new RouteDialog034.Choice(RoomCompletion037.Separate,"Ống riêng cho từng dòng (không trừ phần trùng)"));
   ConduitModeBox.Items.Add(new RouteDialog034.Choice(RoomCompletion037.Shared,"Đi chung ống ở đoạn trùng thực tế và đoạn xuống cùng thiết bị (trừ phần trùng)"));
   IndependentBox.Text="Mỗi dòng là MỘT bộ dây được khai báo độc lập (MEPS không suy luận dây N/PE dùng chung, không nhân số lõi)";IndependentBox.Name="room037_independent";
   int at=0;
   Action<Control,string,string,CheckBox> add=(c,cap,name,all)=>{grid.RowStyles.Add(new RowStyle(SizeType.Absolute,S(30)));
    grid.Controls.Add(new Label{Text=cap,Name="room037_caption_"+name,Dock=DockStyle.Fill,AutoEllipsis=true,ForeColor=BdgklLook034.Muted,TextAlign=ContentAlignment.MiddleLeft,Margin=new Padding(0)},0,at);
    c.Name="room037_"+name;c.AccessibleName=cap;c.Dock=DockStyle.Fill;c.Margin=new Padding(0,S(3),0,S(3));c.TabIndex=at;grid.Controls.Add(c,1,at);
    var t=c as TextBox;if(t!=null){t.BorderStyle=BorderStyle.FixedSingle;t.MaxLength=Bdgkl034.MaxText;t.TextChanged+=(s,e)=>Changed();}
    var cb=c as ComboBox;if(cb!=null){cb.DropDownStyle=ComboBoxStyle.DropDownList;cb.SelectedIndexChanged+=(s,e)=>Changed();}
    if(all!=null){all.Text="cho mọi dòng";all.Name="room037_all_"+name;all.AccessibleName="Áp dụng "+cap+" cho mọi dòng của phòng";all.Dock=DockStyle.Fill;all.Margin=new Padding(S(6),0,0,0);grid.Controls.Add(all,2,at);}
    at++;};
   grid.RowStyles.Add(new RowStyle(SizeType.Absolute,S(48)));IndependentBox.Dock=DockStyle.Fill;IndependentBox.AccessibleName="Xác nhận bộ dây độc lập";IndependentBox.CheckedChanged+=(s,e)=>Changed();
   grid.Controls.Add(IndependentBox,0,at);grid.SetColumnSpan(IndependentBox,3);at++;
   add(ConduitModeBox,"Cách đi ống của phòng","conduit_mode",null);
   add(FloorBox,"Tầng","floor",AllFloorBox);add(CircuitBox,"Lộ / nhãn kênh","circuit",null);add(ModeBox,"Hướng đi","mode",null);
   add(CeilingBox,"H sàn → trần (m)","ceiling",AllCeilingBox);
   // NF-037-HEIGHT-UI: endpoint height fields carry the ACTUAL device type and name (authoritative scan type), not a generic start/end.
   string sDev=RoomUi037.DeviceCaption(row.startType,row.startName),eDev=RoomUi037.DeviceCaption(row.endType,row.endName);
   add(StartModeBox,"Cao độ "+sDev+" (đầu): cách khai","start_mode",null);add(StartValueBox,"Cao độ "+sDev+" (đầu): giá trị (m)","start_value",null);
   add(EndModeBox,"Cao độ "+eDev+" (cuối): cách khai","end_mode",null);add(EndValueBox,"Cao độ "+eDev+" (cuối): giá trị (m)","end_value",null);
   add(TailCountBox,"Số đầu chờ","tail_count",AllTailsBox);add(TailEachBox,"Dài một đầu chờ (m)","tail_each",null);add(FactorBox,"Hệ số uốn cong (≥ 1)","factor",AllFactorBox);
   add(AssemblyBox,"Bộ dây (mã tự nhập)","assembly",AllAssemblyBox);add(ConduitBox,"Ống (mã tự nhập)","conduit",AllConduitBox);
   foreach(var ck in new[]{AllFloorBox,AllCeilingBox,AllTailsBox,AllFactorBox,AllAssemblyBox,AllConduitBox})ck.CheckedChanged+=(s,e)=>Changed();
   grid.RowCount=at;scroll.Controls.Add(grid);
   OkBtn.Text="Áp dụng";OkBtn.Name="room037_declare_ok";CancelBtn.Text="Hủy";CancelBtn.Name="room037_declare_cancel";CancelBtn.DialogResult=DialogResult.Cancel;
   var bar=new FlowLayoutPanel{FlowDirection=System.Windows.Forms.FlowDirection.RightToLeft,Dock=DockStyle.Fill,WrapContents=false,Margin=new Padding(0),BackColor=BdgklLook034.Back};
   foreach(var b in new[]{CancelBtn,OkBtn}){BdgklLook034.Style(b,b==OkBtn,face,S(72),S(30));b.Margin=new Padding(S(4),S(4),0,0);b.AccessibleName=b.Text;bar.Controls.Add(b);}
   pane.Controls.Add(InfoBox,0,0);pane.Controls.Add(scroll,0,1);pane.Controls.Add(PreviewBox,0,2);pane.Controls.Add(StatusBox,0,3);pane.Controls.Add(bar,0,4);Controls.Add(pane);
   CancelButton=CancelBtn;OkBtn.Click+=(s,e)=>TryAccept();
   int useStart=Uses(row.startId),useEnd=Uses(row.endId);
   InfoBox.Text="Tuyến: "+row.startLabel+" → "+row.endLabel+" · kênh "+(row.channelLabel??"—")+" (nhãn TAG trong phòng, không phải lộ/tủ suy luận) · KC mặt bằng đo từ CAD "+BdgklLook034.M(row.planMetres)+" m (chỉ đọc)\r\n"+
    (session.projectMode?"Tầng, trần đi dây và cao độ "+sDev+" / "+eDev+" lấy từ DỰ ÁN (chỉ đọc ở đây): sửa ở 'DỰ ÁN / TẦNG / PHÒNG' (CHUNG/PHÒNG) hoặc 'CAO ĐỘ THIẾT BỊ' (RIÊNG). Áp dụng ở đây không tạo khai riêng thiết bị.\r\n":
     "Cao độ là của THIẾT BỊ: "+sDev+" (đầu) dùng ở "+useStart+" dòng, "+eDev+" (cuối) dùng ở "+useEnd+" dòng; sửa ở đây là sửa cho mọi dòng có thiết bị đó. Khai cả nhóm: nút 'Cao độ tầng và thiết bị'.\r\n")+
    (row.sourceStatus!=RoomQty037.Associated?"BẢN VẼ CHƯA XÁC ĐỊNH ĐƯỢC TUYẾN NÀY ("+string.Join("; ",row.sourceReasons)+"): nhập số không làm dòng này đủ; sửa TAG/thiết bị trong bản vẽ rồi 'Cập nhật'.":
     "Ô trống = chưa khai báo (không phải 0). Thông số dùng chung ống tính lại ở bảng sau khi Áp dụng.");
   loading=true;
   try{var i=row.input;var b=session.batch;
    IndependentBox.Checked=b.independentAssembliesConfirmed;Choose(ConduitModeBox,b.conduitMode);
    FloorBox.Text=session.projectMode?RoomProjectUi037.FloorLabel(session,i.floorId)??"":i.floorId??"";CircuitBox.Text=i.circuitId??"";Choose(ModeBox,i.routingMode);CeilingBox.Text=RoomLook037.Exact(i.ceilingHeight);
    if(session.projectMode){   // project-owned values: the current effective snapshot, read-only (never turned into personal overrides)
     Choose(StartModeBox,i.startHeightMode);StartValueBox.Text=RoomLook037.Exact(i.startFixedHeight);Choose(EndModeBox,i.endHeightMode);EndValueBox.Text=RoomLook037.Exact(i.endFixedHeight);
     foreach(var ctl in new Control[]{FloorBox,CeilingBox,StartModeBox,StartValueBox,EndModeBox,EndValueBox,AllFloorBox,AllCeilingBox})ctl.Enabled=false;}
    else{Device(row.startId,StartModeBox,StartValueBox);Device(row.endId,EndModeBox,EndValueBox);}
    TailCountBox.Text=i.tailCount.HasValue?i.tailCount.Value.ToString(CultureInfo.InvariantCulture):"";TailEachBox.Text=RoomLook037.Exact(i.tailEachLength);FactorBox.Text=RoomLook037.Exact(i.factor);
    AssemblyBox.Text=i.assemblyCode??"";ConduitBox.Text=i.conduitCode??"";}
   finally{loading=false;}
   ActiveControl=session.projectMode?(Control)CircuitBox:FloorBox;Changed();}
  int Uses(string dev){return dev==null?0:Session.batch.rows.Count(r=>r.startId==dev||r.endId==dev);}
  void Device(string dev,ComboBox mode,TextBox value){RoomDraftDevice037 v;
   if(dev!=null&&Session.devices.TryGetValue(dev,out v)){Choose(mode,v.heightMode);value.Text=RoomLook037.Exact(v.heightMode==Bdgkl034.CeilingOffset?v.ceilingOffset:v.fixedHeight);}
   else{mode.SelectedIndex=0;value.Text="";}}
  public void Choose(ComboBox b,string value){for(int i=0;i<b.Items.Count;i++)if(string.Equals(((RouteDialog034.Choice)b.Items[i]).value,value,StringComparison.Ordinal)){b.SelectedIndex=i;return;}b.SelectedIndex=0;}
  static string Value(ComboBox b){var c=b.SelectedItem as RouteDialog034.Choice;return c==null?null:c.value;}
  static string Txt(TextBox t){return string.IsNullOrEmpty(t.Text)?null:t.Text;}
  // QA helper: sets a text field by control name (e.g. "room037_ceiling"); false = no such field. Same TextChanged path as typing.
  public bool SetField(string name,string text){foreach(var t in new[]{FloorBox,CircuitBox,CeilingBox,StartValueBox,EndValueBox,TailCountBox,TailEachBox,FactorBox,AssemblyBox,ConduitBox})
    if(t.Name==name){t.Text=text;return true;}return false;}
  // Current input → declaration (exception = unreadable number; empty = null = not declared).
  public RoomDeclaration037 Build(){return new RoomDeclaration037{floorId=Txt(FloorBox),circuitId=Txt(CircuitBox),routingMode=Value(ModeBox),
   ceilingHeight=BdgklUi034.ParseDecimal(CeilingBox.Text,"H sàn → trần"),startMode=Value(StartModeBox),startValue=BdgklUi034.ParseDecimal(StartValueBox.Text,"Cao độ / khoảng hạ điểm đầu"),
   endMode=Value(EndModeBox),endValue=BdgklUi034.ParseDecimal(EndValueBox.Text,"Cao độ / khoảng hạ điểm cuối"),tailCount=BdgklUi034.ParseCount(TailCountBox.Text,"Số đầu chờ"),
   tailEachLength=BdgklUi034.ParseDecimal(TailEachBox.Text,"Dài một đầu chờ"),factor=BdgklUi034.ParseDecimal(FactorBox.Text,"Hệ số uốn cong"),
   assemblyCode=Txt(AssemblyBox),conduitCode=Txt(ConduitBox),conduitMode=Value(ConduitModeBox),independent=IndependentBox.Checked,
   allFloor=AllFloorBox.Checked,allCeiling=AllCeilingBox.Checked,allTails=AllTailsBox.Checked,allFactor=AllFactorBox.Checked,allAssembly=AllAssemblyBox.Checked,allConduit=AllConduitBox.Checked};}
  void Status(string t,bool err){StatusBox.ForeColor=err?BdgklLook034.Error:BdgklLook034.Ink;StatusBox.Text=t??"";}
  void Changed(){if(loading)return;
   try{var d=Build();var x=Row.input.Copy();RoomUi037.Put(x,d);x.sharedHorizontal=0.0;x.sharedVertical=0.0;x.sharedOwnerKey=null;
    var r=Bdgkl034.Calculate(x);var sb=new StringBuilder();
    if(r.status==Bdgkl034.Ok)sb.Append("Dòng này đủ số liệu (chưa kể dùng chung ống): tổng tuyến M = ").Append(BdgklLook034.M(r.routeM)).Append(" m, sau hệ số ").Append(BdgklLook034.M(r.routeAfterFactorM)).Append(" m.\r\n");
    else sb.Append("Còn thiếu/sai:\r\n").Append(string.Join("\r\n",r.issues.Select(i=>"- "+i.message))).Append("\r\n");
    if(d.conduitMode==null)sb.Append("- Chưa chọn cách đi ống của phòng.\r\n");if(!d.independent)sb.Append("- Chưa xác nhận bộ dây độc lập.\r\n");
    if(Row.sourceStatus!=RoomQty037.Associated)sb.Append("- Bản vẽ chưa xác định được tuyến này; nhập số không sửa được.\r\n");
    if(r.equation!=null)sb.Append("\r\n").Append(r.equation.Replace("\n","\r\n"));
    PreviewBox.Text=sb.ToString();OkBtn.Enabled=true;Status("Có thể Áp dụng cả khi chưa đủ (lưu bản nháp được); tổng chỉ chính thức khi mọi dòng đủ dữ liệu.",false);}
   catch(IOE ex){OkBtn.Enabled=false;PreviewBox.Text="";Status(ex.Message,true);}}
  public bool TryAccept(){try{Result=Build();}catch(Exception ex){Result=null;Status(ex.Message,true);return false;}DialogResult=DialogResult.OK;return true;}
  protected override void Dispose(bool disposing){base.Dispose(disposing);if(disposing)face.Dispose();}
 }

 // ---- Room height setup: floor / wiring ceiling / device groups (NF-037-HEIGHT-UI) ----
 // Owned modal; works on a deep copy (Cancel/Esc changes nothing). Groups = authoritative scan types SWITCH/SOCKET/LAMP/FAN/PANEL only; OTHER is never
 // grouped. Every Apply box starts UNTICKED and 'only fill devices without height' starts TICKED on every opening; no numeric value is ever preset (only the
 // values this room's saved setup already holds are shown). Result is fully parsed and validated before the dialog closes.
 public sealed class RoomHeightDialog037:Form {
  public readonly TextBox InfoBox=new TextBox(),FloorBox=new TextBox(),CeilingBox=new TextBox(),StoreyBox=new TextBox(),StatusBox=new TextBox();
  public readonly CheckBox OnlyMissingBox=new CheckBox();
  public readonly CheckBox[] ApplyBoxes;public readonly ComboBox[] ModeBoxes;public readonly TextBox[] ValueBoxes;public readonly Label[] CountLabels;
  public readonly Button OkBtn=new Button(),CancelBtn=new Button();
  public readonly RoomSession037 Session;public RoomHeightSettings037 Result;
  readonly Dictionary<string,List<string>> byType;readonly float dpi;readonly Font face,bold;bool loading;
  int S(int px){return Math.Max(1,(int)Math.Round(px*dpi));}
  public RoomHeightDialog037(RoomSession037 session,float dpi){if(session==null||session.batch==null)throw new ArgumentException("Cần phòng đang mở.");
   Session=session;this.dpi=BdgklLook034.Dpi(dpi);byType=RoomUi037.DevicesByType(session);int n=RoomHeight037.Types.Length;
   ApplyBoxes=new CheckBox[n];ModeBoxes=new ComboBox[n];ValueBoxes=new TextBox[n];CountLabels=new Label[n];
   Text="MEPS — Cao độ tầng và thiết bị";Name="room037_height_dialog";FormBorderStyle=FormBorderStyle.Sizable;MinimizeBox=false;MaximizeBox=false;ShowInTaskbar=false;
   StartPosition=FormStartPosition.CenterParent;AutoScaleMode=AutoScaleMode.None;face=RoomLook037.Face(this.dpi,false);bold=RoomLook037.Face(this.dpi,true);Font=face;
   BackColor=BdgklLook034.Back;ForeColor=BdgklLook034.Ink;ClientSize=new Size(S(820),S(680));MinimumSize=new Size(S(760),S(500));
   var pane=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=1,RowCount=4,Padding=new Padding(S(8)),BackColor=BdgklLook034.Back};pane.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
   foreach(int h in new[]{S(84),-100,S(48),S(44)})pane.RowStyles.Add(h<0?new RowStyle(SizeType.Percent,-h):new RowStyle(SizeType.Absolute,h));
   BdgklLook034.ReadOnlyBox(InfoBox);InfoBox.Name="room037_height_info";InfoBox.AccessibleName="Phạm vi phòng (chỉ đọc)";
   BdgklLook034.ReadOnlyBox(StatusBox);StatusBox.Name="room037_height_status";StatusBox.AccessibleName="Thông báo";StatusBox.BackColor=BdgklLook034.Back;StatusBox.BorderStyle=BorderStyle.None;
   var scroll=new Panel{Dock=DockStyle.Fill,AutoScroll=true,BackColor=BdgklLook034.Back,Margin=new Padding(0,S(4),0,S(4))};
   var grid=new TableLayoutPanel{ColumnCount=5,AutoSize=true,AutoSizeMode=System.Windows.Forms.AutoSizeMode.GrowAndShrink,Dock=DockStyle.Top,BackColor=BdgklLook034.Back};
   foreach(int w in new[]{S(220),S(90),S(230),S(120)})grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,w));grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
   int at=0;
   Action<string,bool,int> note=(text,head,h)=>{grid.RowStyles.Add(new RowStyle(SizeType.Absolute,S(h)));
    var l=new Label{Text=text,Dock=DockStyle.Fill,AutoSize=false,ForeColor=head?BdgklLook034.Teal:BdgklLook034.Muted,Font=head?bold:face,TextAlign=ContentAlignment.MiddleLeft,Margin=new Padding(0,S(2),0,S(2))};
    grid.Controls.Add(l,0,at);grid.SetColumnSpan(l,5);at++;};
   Action<string,TextBox,string> field=(cap,box,name)=>{grid.RowStyles.Add(new RowStyle(SizeType.Absolute,S(32)));
    grid.Controls.Add(new Label{Text=cap,Dock=DockStyle.Fill,AutoEllipsis=true,ForeColor=BdgklLook034.Ink,TextAlign=ContentAlignment.MiddleLeft,Margin=new Padding(0)},0,at);
    box.Name=name;box.AccessibleName=cap;box.Dock=DockStyle.Fill;box.BorderStyle=BorderStyle.FixedSingle;box.MaxLength=Bdgkl034.MaxText;box.Margin=new Padding(0,S(3),0,S(3));
    box.TextChanged+=(s,e)=>Changed();grid.Controls.Add(box,1,at);grid.SetColumnSpan(box,2);at++;};
   note("Tầng và trần đi dây (khi nhập, áp dụng cho MỌI dòng của phòng này)",true,30);
   field("Tên tầng",FloorBox,"room037_height_floor");
   field("Cao độ trần đi dây (m)",CeilingBox,"room037_height_ceiling");
   note("Cao độ trần đi dây: tính từ sàn hoàn thiện đến trần nơi đi dây; khi nhập sẽ là \"H sàn → trần\" của mọi dòng phòng này. Để trống = không đổi.",false,40);
   field("Chiều cao tầng (m)",StoreyBox,"room037_height_storey");
   note("Chiều cao tầng: từ sàn tầng này đến sàn tầng trên. CHỈ LƯU làm thông tin cho tuyến liên tầng sau này; bảng cùng tầng KHÔNG dùng để tính và không thay cho cao độ trần đi dây. Để trống được.",false,48);
   note("Cao độ thiết bị theo nhóm (loại thiết bị lấy từ bản vẽ)",true,30);
   grid.RowStyles.Add(new RowStyle(SizeType.Absolute,S(40)));OnlyMissingBox.Name="room037_height_only_missing";OnlyMissingBox.AccessibleName="Chỉ điền thiết bị chưa có cao độ";
   OnlyMissingBox.Text="Chỉ điền thiết bị CHƯA có cao độ (bỏ tích = thay cao độ đang có của các nhóm được chọn áp dụng)";OnlyMissingBox.Dock=DockStyle.Fill;OnlyMissingBox.AutoSize=false;
   OnlyMissingBox.CheckedChanged+=(s,e)=>Changed();grid.Controls.Add(OnlyMissingBox,0,at);grid.SetColumnSpan(OnlyMissingBox,5);at++;
   grid.RowStyles.Add(new RowStyle(SizeType.Absolute,S(30)));
   string[] hd={"Nhóm thiết bị","Áp dụng","Cách khai cao độ","Giá trị (m)","Số thiết bị trong phòng"};
   for(int c=0;c<hd.Length;c++)grid.Controls.Add(new Label{Text=hd[c],Dock=DockStyle.Fill,Font=bold,ForeColor=BdgklLook034.Ink,BackColor=BdgklLook034.Paper,TextAlign=ContentAlignment.MiddleLeft,Margin=new Padding(0,0,S(2),0)},c,at);
   at++;
   for(int i=0;i<n;i++){string t=RoomHeight037.Types[i];grid.RowStyles.Add(new RowStyle(SizeType.Absolute,S(32)));int cnt=Count(t);
    grid.Controls.Add(new Label{Text=RoomHeight037.TypeName(t),Dock=DockStyle.Fill,TextAlign=ContentAlignment.MiddleLeft,Margin=new Padding(0)},0,at);
    var ap=ApplyBoxes[i]=new CheckBox{Name="room037_height_apply_"+t,AccessibleName="Áp dụng nhóm "+RoomHeight037.TypeName(t),Dock=DockStyle.Fill,Margin=new Padding(S(4),0,0,0),Enabled=cnt>0};
    ap.CheckedChanged+=(s,e)=>Changed();grid.Controls.Add(ap,1,at);
    var mb=ModeBoxes[i]=new ComboBox{Name="room037_height_mode_"+t,AccessibleName="Cách khai cao độ nhóm "+RoomHeight037.TypeName(t),Dock=DockStyle.Fill,DropDownStyle=ComboBoxStyle.DropDownList,Margin=new Padding(0,S(3),S(4),S(3))};
    mb.Items.Add(new RouteDialog034.Choice(null,"(chọn cách khai)"));mb.Items.Add(new RouteDialog034.Choice(Bdgkl034.Fixed,"Từ sàn hoàn thiện (cố định)"));
    mb.Items.Add(new RouteDialog034.Choice(Bdgkl034.CeilingOffset,"Hạ dưới trần đi dây"));mb.SelectedIndex=0;mb.SelectedIndexChanged+=(s,e)=>Changed();grid.Controls.Add(mb,2,at);
    var vb=ValueBoxes[i]=new TextBox{Name="room037_height_value_"+t,AccessibleName="Giá trị cao độ nhóm "+RoomHeight037.TypeName(t)+" (m)",Dock=DockStyle.Fill,BorderStyle=BorderStyle.FixedSingle,MaxLength=Bdgkl034.MaxText,Margin=new Padding(0,S(3),S(4),S(3))};
    vb.TextChanged+=(s,e)=>Changed();grid.Controls.Add(vb,3,at);
    var cl=CountLabels[i]=new Label{Name="room037_height_count_"+t,Text=cnt.ToString(CultureInfo.InvariantCulture)+" thiết bị",Dock=DockStyle.Fill,TextAlign=ContentAlignment.MiddleLeft,ForeColor=cnt>0?BdgklLook034.Ink:BdgklLook034.Disabled,Margin=new Padding(0)};
    grid.Controls.Add(cl,4,at);at++;}
   int other=byType.Where(kv=>Array.IndexOf(RoomHeight037.Types,kv.Key)<0).Sum(kv=>kv.Value.Count);   // OTHER, invalid or missing type
   note("Thiết bị chưa phân loại trong phòng: "+other.ToString(CultureInfo.InvariantCulture)+" (MEPS không đoán loại; khai cao độ riêng từng thiết bị bằng nút 'Thông số'). "+
    "Sau khi áp dụng nhóm vẫn có thể sửa riêng từng thiết bị trong 'Thông số'.",false,48);
   grid.RowCount=at;scroll.Controls.Add(grid);
   OkBtn.Text="Áp dụng";OkBtn.Name="room037_height_ok";CancelBtn.Text="Hủy";CancelBtn.Name="room037_height_cancel";CancelBtn.DialogResult=DialogResult.Cancel;
   var bar=new FlowLayoutPanel{FlowDirection=System.Windows.Forms.FlowDirection.RightToLeft,Dock=DockStyle.Fill,WrapContents=false,Margin=new Padding(0),BackColor=BdgklLook034.Back};
   foreach(var b in new[]{CancelBtn,OkBtn}){BdgklLook034.Style(b,b==OkBtn,face,S(72),S(30));b.Margin=new Padding(S(4),S(4),0,0);b.AccessibleName=b.Text;bar.Controls.Add(b);}
   pane.Controls.Add(InfoBox,0,0);pane.Controls.Add(scroll,0,1);pane.Controls.Add(StatusBox,0,2);pane.Controls.Add(bar,0,3);Controls.Add(pane);
   CancelButton=CancelBtn;OkBtn.Click+=(s,e)=>TryAccept();
   InfoBox.Text="Phòng "+session.roomId+" · "+session.batch.rows.Count.ToString(CultureInfo.InvariantCulture)+" dòng · thiết bị theo loại trong bản vẽ: "+
    string.Join(", ",RoomHeight037.Types.Select(tp=>RoomHeight037.TypeName(tp)+" "+Count(tp).ToString(CultureInfo.InvariantCulture)))+", chưa phân loại "+other.ToString(CultureInfo.InvariantCulture)+".\r\n"+
    "Ô trống = chưa khai (không phải 0); 0 là giá trị hợp lệ cho cao độ thiết bị. Áp dụng chỉ đổi phiên đang mở; bấm 'Lưu vào bản vẽ' để lưu.";
   loading=true;
   try{var h=RoomHeight037.Copy(session.heights)??RoomHeight037.Empty();
    FloorBox.Text=h.floorId??"";CeilingBox.Text=RoomLook037.Exact(h.wireCeilingHeight);StoreyBox.Text=RoomLook037.Exact(h.storeyHeight);OnlyMissingBox.Checked=true;
    for(int i=0;i<n;i++){var g=h.groups==null?null:h.groups.FirstOrDefault(a=>a!=null&&a.deviceType==RoomHeight037.Types[i]);ApplyBoxes[i].Checked=false;
     if(g!=null){Choose(ModeBoxes[i],g.heightMode);ValueBoxes[i].Text=RoomLook037.Exact(g.value);}}}
   finally{loading=false;}
   ActiveControl=FloorBox;Changed();}
  int Count(string t){List<string> l;return byType.TryGetValue(t,out l)?l.Count:0;}
  public void Choose(ComboBox b,string value){for(int i=0;i<b.Items.Count;i++)if(string.Equals(((RouteDialog034.Choice)b.Items[i]).value,value,StringComparison.Ordinal)){b.SelectedIndex=i;return;}b.SelectedIndex=0;}
  static string Value(ComboBox b){var c=b.SelectedItem as RouteDialog034.Choice;return c==null?null:c.value;}
  // QA helper: index of a group by type (SWITCH/SOCKET/LAMP/FAN/PANEL), -1 = not a group type.
  public int GroupIndex(string type){return Array.IndexOf(RoomHeight037.Types,type);}
  // Current input → validated settings. Every cell is parsed (also unselected ones): any bad cell = exception, nothing applied.
  public RoomHeightSettings037 Build(){
   var r=new RoomHeightSettings037{floorId=string.IsNullOrEmpty(FloorBox.Text)?null:FloorBox.Text,storeyHeight=BdgklUi034.ParseDecimal(StoreyBox.Text,"Chiều cao tầng"),
    wireCeilingHeight=BdgklUi034.ParseDecimal(CeilingBox.Text,"Cao độ trần đi dây"),onlyFillMissing=OnlyMissingBox.Checked};
   for(int i=0;i<RoomHeight037.Types.Length;i++){string t=RoomHeight037.Types[i];
    r.groups.Add(new RoomHeightGroup037{deviceType=t,apply=ApplyBoxes[i].Checked,heightMode=Value(ModeBoxes[i]),value=BdgklUi034.ParseDecimal(ValueBoxes[i].Text,"Cao độ nhóm "+RoomHeight037.TypeName(t))});}
   var p=RoomHeight037.Problem(r);if(p!=null)throw new IOE(p);return r;}
  void Status(string t,bool err){StatusBox.ForeColor=err?BdgklLook034.Error:BdgklLook034.Ink;StatusBox.Text=t??"";}
  void Changed(){if(loading)return;
   try{var r=Build();int skipped;int fill=RoomUi037.PlannedDevices(Session,r,out skipped);
    var parts=new List<string>();
    if(fill>0)parts.Add("sẽ khai cao độ cho "+fill+" thiết bị");if(skipped>0)parts.Add(skipped+" thiết bị đã có cao độ được giữ nguyên");
    if(r.floorId!=null)parts.Add("tầng \""+r.floorId+"\" cho mọi dòng");if(r.wireCeilingHeight.HasValue)parts.Add("trần đi dây "+BdgklLook034.R(r.wireCeilingHeight)+" m cho mọi dòng");
    if(r.storeyHeight.HasValue)parts.Add("lưu chiều cao tầng "+BdgklLook034.R(r.storeyHeight)+" m (chỉ thông tin)");
    OkBtn.Enabled=true;Status(parts.Count==0?"Chưa chọn nhóm nào và chưa nhập tầng/trần: Áp dụng chỉ lưu thiết lập, không đổi dòng nào.":"Áp dụng: "+string.Join("; ",parts)+".",false);}
   catch(IOE ex){OkBtn.Enabled=false;Status(ex.Message,true);}}
  public bool TryAccept(){try{Result=Build();}catch(Exception ex){Result=null;Status(ex.Message,true);return false;}DialogResult=DialogResult.OK;return true;}
  protected override void Dispose(bool disposing){base.Dispose(disposing);if(disposing){face.Dispose();bold.Dispose();}}
 }

 public static class RoomUi037 {
  public const string Title=BdgklUi034.Title;public const int MaxRounds=100;
  // S9: visible build marker in the RoomReport037 title so the user can identify the loaded assembly (display only; not a schema/store/library ID).
  public const string Build="038-R1";
  // QA hooks (synchronous). PickRegion returns two WCS corners or null (Esc); it throws InvalidOperationException for an unsupported UCS. The Show* hooks
  // display the forms and return their results; ChooseExportPath(owner, suggested name) returns a path or null. RunLegacy = the existing BDGKL report.
  // Hooks only replace user interaction: guards, validation, storage and export always run.
  public static Func<Document,Point3d[]> PickRegion=DefaultPickRegion;
  public static Func<RoomReport037,DialogResult> ShowReport=DefaultShowReport;
  public static Func<RoomOptionsDialog037,DialogResult> ShowOptions=DefaultShowOptions;
  public static Func<RoomDeclareDialog037,IWin32Window,DialogResult> ShowDeclaration=DefaultShowDeclaration;
  public static Func<IWin32Window,string,string> ChooseExportPath=DefaultChooseExportPath;
  public static Func<Document,Action<string>,Action<string>,float,RouteOutcome034> RunLegacy=BdgklUi034.Run;
  public static DialogResult DefaultShowReport(RoomReport037 f){return App.ShowModalDialog(f);}
  public static DialogResult DefaultShowOptions(RoomOptionsDialog037 f){return App.ShowModalDialog(f);}
  public static DialogResult DefaultShowDeclaration(RoomDeclareDialog037 f,IWin32Window owner){return f.ShowDialog(owner);}
  public static Func<RoomHeightDialog037,IWin32Window,DialogResult> ShowHeights=DefaultShowHeights;
  public static DialogResult DefaultShowHeights(RoomHeightDialog037 f,IWin32Window owner){return f.ShowDialog(owner);}
  // S3 hooks (synchronous, owned modals of the report): project setup and device overrides. Apply/validation/storage stay real.
  public static Func<RoomProjectDialog037,IWin32Window,DialogResult> ShowProject=DefaultShowProject;
  public static Func<RoomDeviceDialog037,IWin32Window,DialogResult> ShowDevices=DefaultShowDevices;
  public static DialogResult DefaultShowProject(RoomProjectDialog037 f,IWin32Window owner){return f.ShowDialog(owner);}
  public static DialogResult DefaultShowDevices(RoomDeviceDialog037 f,IWin32Window owner){return f.ShowDialog(owner);}
  // S5_REGION_TRACE: optional best-effort stage trace (event, detail). null = off (default; no handler is attached anywhere). Plugin.RunRoute sets it to the
  // run's placement-log stage for the duration of one route command and restores the previous value. Details are counts, enum results, booleans and window
  // bounds only: never document paths/names, room labels, tags, quantities or drawing coordinates. A failing callback is swallowed (Trace) and never changes
  // the command outcome; original exceptions are only observed (type name) and rethrown unchanged.
  public static Action<string,string> Stage;
  public static void ResetHooks(){PickRegion=DefaultPickRegion;ShowReport=DefaultShowReport;ShowOptions=DefaultShowOptions;ShowDeclaration=DefaultShowDeclaration;
   ShowHeights=DefaultShowHeights;ShowProject=DefaultShowProject;ShowDevices=DefaultShowDevices;ChooseExportPath=DefaultChooseExportPath;RunLegacy=BdgklUi034.Run;
   Stage=null;   // S5_REGION_TRACE: QA isolation (a harness that wants the trace sets Stage AFTER ResetHooks)
   RoomProjectExportUi037.ResetHooks();   // S4: export scope chooser hook
   RoomRecoveryUi044.ResetHooks();}       // 044: recovery root override / last controller
  static void Trace(string evt,string detail){var t=Stage;if(t==null)return;try{t(evt,detail??"");}catch{}}
  // Shown of one created modal → bounds, Visible, active form, intersection with the working area of the form's screen and of any screen. Attached once per
  // instance (called once right after construction); only when a trace is active; the handler never throws.
  static void TraceShown(Form f,string kind){if(Stage==null||f==null)return;try{f.Shown+=(s,e)=>Trace(kind+"_SHOWN",WindowInfo(f));}catch{}}
  static string WindowInfo(Form f){try{var b=f.Bounds;var ic=CultureInfo.InvariantCulture;bool current=false,any=false;int screens=0;
    try{current=Screen.FromControl(f).WorkingArea.IntersectsWith(b);}catch{}
    try{var all=Screen.AllScreens;screens=all.Length;any=all.Any(x=>x.WorkingArea.IntersectsWith(b));}catch{}
    return "visible="+f.Visible+" active="+(Form.ActiveForm==f)+" bounds="+b.X.ToString(ic)+","+b.Y.ToString(ic)+","+b.Width.ToString(ic)+","+b.Height.ToString(ic)+
     " state="+f.WindowState+" currentScreen="+current+" anyScreen="+any+" screens="+screens.ToString(ic);}
   catch(Exception ex){return "info_error="+ex.GetType().Name;}}
  static string TraceNum(int n){return n.ToString(CultureInfo.InvariantCulture);}

  // ---- room height setup (pure; public for the independent harness) ----
  // "Công tắc CT1" etc.: authoritative type name + CAD name (no type guessed from the label).
  public static string DeviceCaption(string type,string name){return RoomHeight037.TypeName(type)+(string.IsNullOrWhiteSpace(name)?"":" "+name);}
  // S6: full identity of one row end for tooltips/details (compact label + type + block name + handle); display only.
  public static string EndText(string type,string name,string label,string id){return (label??"?")+" = "+DeviceCaption(type,name)+" · mã "+(id??"?");}
  // Unique ACTUAL device IDs of the current rows by authoritative type (null/invalid type grouped under its own key; never merged into a group type).
  public static Dictionary<string,List<string>> DevicesByType(RoomSession037 s){var m=new Dictionary<string,SortedSet<string>>(StringComparer.Ordinal);
   Action<string,string> add=(type,id)=>{if(string.IsNullOrEmpty(id))return;string k=type??"";SortedSet<string> set;if(!m.TryGetValue(k,out set))m[k]=set=new SortedSet<string>(StringComparer.Ordinal);set.Add(id);};
   if(s!=null&&s.batch!=null)foreach(var r in s.batch.rows){add(r.startType,r.startId);add(r.endType,r.endId);}
   // A device listed under two types (should not happen: one scan type per device) is reported by the model; here it is counted under each, never re-typed.
   return m.ToDictionary(kv=>kv.Key,kv=>kv.Value.ToList(),StringComparer.Ordinal);}
  static bool HasHeight(RoomSession037 s,string dev){RoomDraftDevice037 v;return s.devices.TryGetValue(dev,out v)&&(v.heightMode!=null||v.fixedHeight.HasValue||v.ceilingOffset.HasValue);}
  // Devices the given settings would set (selected groups; with onlyFillMissing, devices that already have a height are skipped and counted).
  public static List<string> PlannedDeviceIds(RoomSession037 s,RoomHeightSettings037 h,out int skipped){skipped=0;var l=new List<string>();if(s==null||h==null||h.groups==null)return l;
   var by=DevicesByType(s);
   foreach(var g in h.groups){if(g==null||!g.apply)continue;List<string> ids;if(!by.TryGetValue(g.deviceType??"",out ids))continue;
    foreach(var id in ids){if(h.onlyFillMissing&&HasHeight(s,id)){skipped++;continue;}if(!l.Contains(id))l.Add(id);}}
   return l;}
  public static int PlannedDevices(RoomSession037 s,RoomHeightSettings037 h,out int skipped){return PlannedDeviceIds(s,h,out skipped).Count;}
  // Applies one validated setup to the open room. Validation happens BEFORE any change (an invalid request changes nothing). Changes: the selected groups'
  // actual devices (every row using a device follows it), floorId / ceilingHeight of every row when entered; everything else (routing, assembly, conduit,
  // tails, factor, unselected groups, individual overrides when 'only fill missing') is kept. storeyHeight is stored only, never used in any length.
  public static string ApplyHeights(RoomSession037 s,RoomHeightSettings037 h){if(s==null||s.batch==null)throw new IOE("Chưa có phòng đang mở.");
   if(s.projectMode)throw new IOE("Trong dự án, cao độ khai ở 'DỰ ÁN / TẦNG / PHÒNG' (CHUNG/PHÒNG) và 'CAO ĐỘ THIẾT BỊ' (RIÊNG); hộp cao độ phòng cũ không dùng. Chưa thay đổi gì.");
   var p=RoomHeight037.Problem(h);if(p!=null)throw new IOE(p+" Chưa thay đổi gì.");
   var copy=RoomHeight037.Copy(h);int skipped;var ids=PlannedDeviceIds(s,copy,out skipped);
   var by=DevicesByType(s);var plan=new List<Tuple<string,RoomHeightGroup037>>();
   foreach(var g in copy.groups){if(!g.apply)continue;List<string> list;if(!by.TryGetValue(g.deviceType,out list))continue;foreach(var id in list)if(ids.Contains(id))plan.Add(Tuple.Create(id,g));}
   // ---- mutation (nothing above has changed the session) ----
   foreach(var x in plan){var g=x.Item2;
    s.devices[x.Item1]=new RoomDraftDevice037{deviceId=x.Item1,heightMode=g.heightMode,fixedHeight=g.heightMode==Bdgkl034.CeilingOffset?null:g.value,ceilingOffset=g.heightMode==Bdgkl034.CeilingOffset?g.value:null};}
   int rows=0;
   if(copy.floorId!=null||copy.wireCeilingHeight.HasValue)foreach(var r in s.batch.rows){if(r.input==null)continue;
    if(copy.floorId!=null)r.input.floorId=copy.floorId;if(copy.wireCeilingHeight.HasValue)r.input.ceilingHeight=copy.wireCeilingHeight;rows++;}
   s.heights=copy;s.dirty=true;Recalc(s);
   var parts=new List<string>();
   if(plan.Count>0)parts.Add("đã khai cao độ cho "+plan.Count+" thiết bị ("+string.Join(", ",copy.groups.Where(gg=>gg.apply).Select(gg=>RoomHeight037.TypeName(gg.deviceType)))+")");
   if(skipped>0)parts.Add(skipped+" thiết bị đã có cao độ được giữ nguyên");
   if(rows>0)parts.Add((copy.floorId!=null?"tầng \""+copy.floorId+"\"":"")+(copy.floorId!=null&&copy.wireCeilingHeight.HasValue?" và ":"")+
    (copy.wireCeilingHeight.HasValue?"trần đi dây "+BdgklLook034.R(copy.wireCeilingHeight)+" m":"")+" cho "+rows+" dòng");
   if(copy.storeyHeight.HasValue)parts.Add("chiều cao tầng "+BdgklLook034.R(copy.storeyHeight)+" m lưu làm thông tin (không dùng để tính)");
   return "Cao độ tầng và thiết bị: "+(parts.Count==0?"chỉ lưu thiết lập, không đổi dòng nào":string.Join("; ",parts))+". Chưa lưu vào bản vẽ. "+Summary(s);}
  static string Heights(RoomSession037 s,IWin32Window owner,float dpi){if(s.batch==null)throw new IOE("Chưa chọn phòng.");
   if(s.projectMode)throw new IOE("Trong dự án dùng 'DỰ ÁN / TẦNG / PHÒNG' và 'CAO ĐỘ THIẾT BỊ'.");
   RoomHeightSettings037 res;using(var f=new RoomHeightDialog037(s,dpi)){var r=ShowHeights(f,owner);if(r!=DialogResult.OK||f.Result==null)return null;res=f.Result;}
   return ApplyHeights(s,res);}
  static readonly Tolerance AxisTol=new Tolerance(1e-9,1e-9);
  // Two corners picked in the current UCS, returned in WCS. Only a UCS whose X/Y axes equal the WCS axes (any origin) is accepted, so the picked rectangle is
  // the axis-aligned WCS rectangle the scan uses; a rotated/tilted UCS is refused (UCS → World).
  // S5_REGION_TRACE: NATIVE_PICK_* stages carry only PromptStatus values (no coordinates).
  public static Point3d[] DefaultPickRegion(Document d){Trace("NATIVE_PICK_ENTRY","");var ed=d.Editor;var ucs=ed.CurrentUserCoordinateSystem;var cs=ucs.CoordinateSystem3d;
   if(!cs.Xaxis.IsEqualTo(Vector3d.XAxis,AxisTol)||!cs.Yaxis.IsEqualTo(Vector3d.YAxis,AxisTol)){Trace("NATIVE_PICK_UCS_REFUSED","");
    throw new IOE("UCS hiện tại bị xoay/nghiêng so với World; vùng phòng phải là hình chữ nhật theo trục World. Gõ UCS → W rồi chọn lại. Phiên đang mở giữ nguyên.");}
   Trace("NATIVE_PICK1_BEFORE","");
   var p1=ed.GetPoint(new PromptPointOptions("\n"+Title+": chọn góc thứ nhất của phòng (Esc huỷ): "));Trace("NATIVE_PICK1","status="+p1.Status);if(p1.Status!=PromptStatus.OK)return null;
   var p2=ed.GetCorner(new PromptCornerOptions("\nChọn góc đối diện của phòng (Esc huỷ): ",p1.Value));Trace("NATIVE_PICK2","status="+p2.Status);if(p2.Status!=PromptStatus.OK)return null;
   Trace("NATIVE_PICK_RETURN","corners=2");
   return new[]{p1.Value.TransformBy(ucs),p2.Value.TransformBy(ucs)};}
  // S5_REGION_TRACE: the PickRegion hook and the region scan with before/after/error stages (corner count and scan counts only; errors rethrown unchanged).
  static Point3d[] TracedPick(Document d){Trace("PICK_BEFORE","");Point3d[] p;
   try{p=PickRegion(d);}catch(Exception ex){Trace("PICK_ERROR","type="+ex.GetType().Name);throw;}
   Trace("PICK_AFTER",p==null?"cancelled=true":"cancelled=false corners="+TraceNum(p.Length));return p;}
  static RoomCadScan037 TracedScan(Database db,Point3d a,Point3d b,string roomId,string units,double tolerance){Trace("SCAN_BEFORE","");RoomCadScan037 sc;
   try{sc=RoomCad037.Scan(db,a,b,roomId,units,tolerance);}catch(Exception ex){Trace("SCAN_ERROR","type="+ex.GetType().Name);throw;}
   Trace("SCAN_AFTER",sc==null?"scan=null":"sources="+TraceNum(sc.sources.Count)+" devices="+TraceNum(sc.devices.Count)+" diagnostics="+TraceNum(sc.diagnostics.Count)+
    " input="+(sc.input!=null)+" signature="+(sc.signature!=null));
   return sc;}
  // New file only: no overwrite prompt; an existing name is refused later (RoomXlsx037.Write never replaces a file).
  public static string DefaultChooseExportPath(IWin32Window owner,string name){
   using(var f=new SaveFileDialog{Title="Xuất Excel (tệp mới)",Filter="Excel (*.xlsx)|*.xlsx",DefaultExt="xlsx",AddExtension=true,OverwritePrompt=false,CheckPathExists=true,RestoreDirectory=true,FileName=name})
    return f.ShowDialog(owner)==DialogResult.OK?f.FileName:null;}

  // ---- pure session helpers (public for the independent harness) ----
  public static List<string> NativeBlockers(RoomCadScan037 sc,bool unitConfirmed){var l=new List<string>();
   if(sc==null){l.Add("Chưa quét vùng phòng.");return l;}
   if(sc.input==null)l.Add("Quét CAD không tạo được dữ liệu phòng (tuỳ chọn không hợp lệ hoặc vượt giới hạn); xem các vướng mắc CAD.");
   foreach(var d in sc.diagnostics){if(d.severity!=RoomQty037.Blocked&&d.severity!=RoomQty037.Check)continue;if(d.code=="UNIT_INSUNITS_MISMATCH"&&unitConfirmed)continue;
    l.Add(d.code+(d.handles.Count>0?" ["+string.Join(",",d.handles.Take(20))+"]":"")+": "+d.message);}
   return l;}
  static RoomDraftDevice037 Copy(RoomDraftDevice037 v){return new RoomDraftDevice037{deviceId=v.deviceId,heightMode=v.heightMode,fixedHeight=v.fixedHeight,ceilingOffset=v.ceilingOffset};}
  static RoomDraftRow037 Copy(RoomDraftRow037 r){return new RoomDraftRow037{rowKey=r.rowKey,floorId=r.floorId,circuitId=r.circuitId,routingMode=r.routingMode,assemblyCode=r.assemblyCode,
   conduitCode=r.conduitCode,ceilingHeight=r.ceilingHeight,tailEachLength=r.tailEachLength,factor=r.factor,tailCount=r.tailCount};}
  // Device heights of the session → every row (an undeclared device = empty heights, never a default).
  static void Push(RoomSession037 s){foreach(var r in s.batch.rows){var i=r.input;if(i==null)continue;RoomDraftDevice037 v;
    if(r.startId!=null&&s.devices.TryGetValue(r.startId,out v)){i.startHeightMode=v.heightMode;i.startFixedHeight=v.fixedHeight;i.startCeilingOffset=v.ceilingOffset;}
    else{i.startHeightMode=null;i.startFixedHeight=null;i.startCeilingOffset=null;}
    if(r.endId!=null&&s.devices.TryGetValue(r.endId,out v)){i.endHeightMode=v.heightMode;i.endFixedHeight=v.fixedHeight;i.endCeilingOffset=v.ceilingOffset;}
    else{i.endHeightMode=null;i.endFixedHeight=null;i.endCeilingOffset=null;}}}
  // Project mode: floor / wiring ceiling / endpoint heights from the project profile (RoomProjectUi037.ApplyProfile; invalid heights = explicit blockers);
  // legacy mode: per-device session heights (Push).
  // S5: the batch always gets the session's adjustments and the ACTUAL signature of the scan that produced it (null = incomplete scan).
  public static void Recalc(RoomSession037 s){if(s.batch==null)return;s.batch.externalBlockers=NativeBlockers(s.scan,s.unitConfirmed);
   s.batch.adjustments=s.adjustments;s.batch.sourceSignature=s.scan==null?null:s.scan.signature;
   // 045: inherited route fields (row preset > room default > device-kind default) are resolved into the inputs of non-RIÊNG fields before the profile
   // and the unchanged calculation; explicit values are never touched.
   if(s.projectMode){RoomFastEntryUi045.Resolve(s);s.batch.externalBlockers.AddRange(RoomProjectUi037.ApplyProfile(s));}else Push(s);RoomCompletion037.Calculate(s.batch);}
  static bool Declared(RoomDraftRow037 r){return r.floorId!=null||r.circuitId!=null||r.routingMode!=null||r.assemblyCode!=null||r.conduitCode!=null||r.ceilingHeight.HasValue||
   r.tailEachLength.HasValue||r.factor.HasValue||r.tailCount.HasValue;}
  // Editable declarations of the current rows (declared ones only) + saved declarations that matched no current row.
  // Project mode: floor and wiring ceiling are project-owned snapshots and are NOT captured (recomputed from the profile at every load/rebuild/export).
  public static List<RoomDraftRow037> Capture(RoomSession037 s){var l=new List<RoomDraftRow037>();
   if(s.batch!=null)foreach(var r in s.batch.rows){var i=r.input;if(i==null||r.rowKey==null)continue;
    var d=new RoomDraftRow037{rowKey=r.rowKey,floorId=s.projectMode?null:i.floorId,circuitId=i.circuitId,routingMode=i.routingMode,assemblyCode=i.assemblyCode,conduitCode=i.conduitCode,
     ceilingHeight=s.projectMode?null:i.ceilingHeight,tailEachLength=i.tailEachLength,factor=i.factor,tailCount=i.tailCount};
    RoomFastEntryUi045.ExplicitOnly(s,r.rowKey,d);   // 045: inherited values are never written into the draft (only RIÊNG values)
    if(Declared(d))l.Add(d);}
   var have=new HashSet<string>(l.Select(x=>x.rowKey),StringComparer.Ordinal);
   foreach(var u in s.unmatched)if(have.Add(u.rowKey))l.Add(Copy(u));
   return l;}
  // New batch from a scan; declarations overlaid by exact row key; unmatched kept. Recalculates.
  public static void Rebuild(RoomSession037 s,RoomCadScan037 scan,IList<RoomDraftRow037> decl,string conduitMode,bool independent){
   s.scan=scan;s.scannedAt=RouteUi034.Now();s.batch=RoomCompletion037.Create(scan==null?null:scan.input);s.batch.conduitMode=conduitMode;s.batch.independentAssembliesConfirmed=independent;
   var by=new Dictionary<string,RoomDraftRow037>(StringComparer.Ordinal);foreach(var d in decl??new List<RoomDraftRow037>())if(d!=null&&d.rowKey!=null&&!by.ContainsKey(d.rowKey))by[d.rowKey]=d;
   var used=new HashSet<string>(StringComparer.Ordinal);
   foreach(var r in s.batch.rows){RoomDraftRow037 dr;if(r.input==null||r.rowKey==null||!by.TryGetValue(r.rowKey,out dr))continue;used.Add(r.rowKey);var i=r.input;
    i.floorId=dr.floorId;i.circuitId=dr.circuitId;i.routingMode=dr.routingMode;i.assemblyCode=dr.assemblyCode;i.conduitCode=dr.conduitCode;i.ceilingHeight=dr.ceilingHeight;
    i.tailEachLength=dr.tailEachLength;i.factor=dr.factor;i.tailCount=dr.tailCount;}
   s.unmatched=by.Values.Where(q=>!used.Contains(q.rowKey)).Select(Copy).OrderBy(q=>q.rowKey,StringComparer.Ordinal).ToList();
   RoomFastEntryUi045.Rebind(s);   // 045: row state of THIS room (same room = kept by row key; other room = from the project document); RIÊNG = declared values
   Recalc(s);}
  // Writes the dialog values into one input (used by the preview and by ApplyDeclaration; device heights only into this copy).
  public static void Put(BdgklInput034 i,RoomDeclaration037 d){i.floorId=d.floorId;i.circuitId=d.circuitId;i.routingMode=d.routingMode;i.ceilingHeight=d.ceilingHeight;
   i.tailCount=d.tailCount;i.tailEachLength=d.tailEachLength;i.factor=d.factor;i.assemblyCode=d.assemblyCode;i.conduitCode=d.conduitCode;
   i.startHeightMode=d.startMode;i.startFixedHeight=d.startMode==Bdgkl034.CeilingOffset?null:d.startValue;i.startCeilingOffset=d.startMode==Bdgkl034.CeilingOffset?d.startValue:null;
   i.endHeightMode=d.endMode;i.endFixedHeight=d.endMode==Bdgkl034.CeilingOffset?null:d.endValue;i.endCeilingOffset=d.endMode==Bdgkl034.CeilingOffset?d.endValue:null;}
  static void SetDevice(RoomSession037 s,string dev,string mode,double? value){if(dev==null)return;
   if(mode==null&&!value.HasValue){s.devices.Remove(dev);return;}
   s.devices[dev]=new RoomDraftDevice037{deviceId=dev,heightMode=mode,fixedHeight=mode==Bdgkl034.CeilingOffset?null:value,ceilingOffset=mode==Bdgkl034.CeilingOffset?value:null};}
  // Applies one dialog result: this row, the two actual devices (all their rows), room globals and only the fields the user ticked for every row.
  public static string ApplyDeclaration(RoomSession037 s,string rowKey,RoomDeclaration037 d){if(s==null||s.batch==null||d==null)throw new IOE("Chưa có phòng đang mở.");
   var row=s.batch.rows.FirstOrDefault(q=>string.Equals(q.rowKey,rowKey,StringComparison.Ordinal));if(row==null||row.input==null)throw new IOE("Dòng "+rowKey+" không còn trong phòng; bấm 'Cập nhật'.");
   // 045: provenance — dialog fields the user CHANGED (vs the shown effective value) and fields copied with 'cho mọi dòng' become RIÊNG; untouched
   // inherited fields stay inherited.
   var before=RoomFastEntryUi045.Values(row.input);
   Put(row.input,d);
   RoomFastEntryUi045.MarkChanged(s,rowKey,before,row.input);
   var copiedFields=new List<string>();if(d.allTails){copiedFields.Add(FastEntry045.TailCount);copiedFields.Add(FastEntry045.TailEach);}
   if(d.allFactor)copiedFields.Add(FastEntry045.Factor);if(d.allAssembly)copiedFields.Add(FastEntry045.Assembly);if(d.allConduit)copiedFields.Add(FastEntry045.Conduit);
   foreach(var q in s.batch.rows)if(q!=row&&q.input!=null&&q.rowKey!=null)foreach(var f in copiedFields)RoomFastEntryUi045.MarkExplicit(s,q.rowKey,f);
   // Project mode: endpoint heights, floor and ceiling belong to the project profile; Recalc below restores them, nothing becomes a personal override.
   if(!s.projectMode){SetDevice(s,row.startId,d.startMode,d.startValue);SetDevice(s,row.endId,d.endMode,d.endValue);}
   s.batch.independentAssembliesConfirmed=d.independent;s.batch.conduitMode=d.conduitMode;int copied=0;
   foreach(var r in s.batch.rows){if(r==row||r.input==null)continue;var i=r.input;bool any=false;
    if(d.allFloor){i.floorId=d.floorId;any=true;}if(d.allCeiling){i.ceilingHeight=d.ceilingHeight;any=true;}
    if(d.allTails){i.tailCount=d.tailCount;i.tailEachLength=d.tailEachLength;any=true;}if(d.allFactor){i.factor=d.factor;any=true;}
    if(d.allAssembly){i.assemblyCode=d.assemblyCode;any=true;}if(d.allConduit){i.conduitCode=d.conduitCode;any=true;}if(any)copied++;}
   s.dirty=true;Recalc(s);
   int devRows=s.batch.rows.Count(q=>q!=row&&(q.startId==row.startId||q.endId==row.startId||q.startId==row.endId||q.endId==row.endId));
   return "Đã áp dụng thông số cho tuyến "+row.startLabel+" → "+row.endLabel+(copied>0?"; đã chép các ô được chọn sang "+copied+" dòng khác":"")+
    (devRows>0&&!s.projectMode?"; cao độ thiết bị đồng bộ cho "+devRows+" dòng khác dùng cùng thiết bị":"")+". Chưa lưu vào bản vẽ. "+Summary(s);}
  public static string Summary(RoomSession037 s){var b=s.batch;if(b==null)return "Chưa có dữ liệu phòng.";
   return "Phòng "+(s.projectMode?RoomProjectUi037.RoomLabel(s):s.roomId)+": "+b.rows.Count+" dòng — "+b.pass+" đủ, "+b.missingData+" thiếu, "+b.check+" cần kiểm tra; "+(b.blockers.Count+b.externalBlockers.Count)+" vướng mắc bản vẽ"+
    RoomAdjustmentsUi037.SummarySuffix(s);}
  // null = export allowed now; otherwise what is missing (shown on the button press).
  public static string ExportReason(RoomSession037 s){if(s==null||s.batch==null||s.scan==null)return "chưa chọn vùng phòng (bấm 'Chọn vùng CAD').";
   if(s.NeedsReview)return "hình trong phòng đã đổi so với bản nháp đã xác nhận: xem lại các dòng, tích ô 'đã xem lại' và bấm 'Lưu vào bản vẽ' trước khi xuất.";
   var b=s.batch;if(b.status==RoomCompletion037.Pass)return null;var parts=new List<string>();
   if(b.missingData>0)parts.Add(b.missingData+" dòng thiếu dữ liệu");if(b.check>0)parts.Add(b.check+" dòng cần kiểm tra");
   if(b.blockers.Count+b.externalBlockers.Count>0)parts.Add((b.blockers.Count+b.externalBlockers.Count)+" vướng mắc bản vẽ (tab VƯỚNG MẮC)");
   foreach(var m in b.missing.Concat(b.issues))parts.Add(RoomLook037.Friendly(m));
   return string.Join("; ",parts.Distinct())+". Xuất Excel chỉ khi mọi dòng đủ dữ liệu.";}
  public static RoomDraft037 ToDraft(RoomSession037 s,bool reviewConfirmed){var sc=s.scan;var b=s.batch;
   var d=new RoomDraft037{schema=RoomStore037.Schema,kind=RoomStore037.DraftKind,version=RoomStore037.Version,roomId=s.roomId,units=s.units,fingerprint=sc.fingerprint,signature=sc.signature,
    insunits=sc.insunits,minX=s.minX,minY=s.minY,maxX=s.maxX,maxY=s.maxY,tolerance=s.tolerance,unitConfirmed=s.unitConfirmed,independentAssembliesConfirmed=b.independentAssembliesConfirmed,
    conduitMode=b.conduitMode,savedAt=RouteUi034.Now()};
   d.confirmedSignature=s.saved==null||reviewConfirmed||string.Equals(s.saved.confirmedSignature,sc.signature,StringComparison.Ordinal)?sc.signature:s.saved.confirmedSignature;
   d.sourceHandles=sc.sources.Select(x=>x.handle).Where(h=>h!=null).Distinct(StringComparer.Ordinal).OrderBy(h=>h,StringComparer.Ordinal).ToList();
   d.deviceHandles=sc.devices.Keys.OrderBy(h=>h,StringComparer.Ordinal).ToList();
   d.rows=Capture(s);
   // Project mode: the profile document is the only height authority; no personal device heights or room height profile in the draft.
   d.devices=s.projectMode?new List<RoomDraftDevice037>():s.devices.Values.OrderBy(v=>v.deviceId,StringComparer.Ordinal).Select(Copy).ToList();
   d.heights=s.projectMode?null:RoomHeight037.Copy(s.heights);
   // S5: declarations only (deep copy); nothing declared = null.
   d.adjustments=RoomAdjustmentsUi037.ForDraft(s.adjustments);
   return d;}

  // ---- bound-command flow ----
  public static RouteOutcome034 Run(Document d,Action<string> guard,Action<string> readGuard,float dpi){if(d==null)throw new ArgumentNullException("d");
   var db=d.Database;var s=new RoomSession037{projectMode=true};bool changed=false,warn=false,noteWarn=false;string note=null,state="";var done=new List<string>();
   // S3: the saved project is read once at start (missing = none until setup; unreadable = projectError, never replaced). projectRaw = optimistic token.
   readGuard("trước khi đọc dữ liệu dự án");
   try{var rd=RoomProjectStore037.Read(db);s.projectDocument=rd.document;s.projectRaw=rd.raw;}catch(IOE ex){s.projectError=ex.Message;note=ex.Message;noteWarn=true;}
   // TVXD-MEPS-RAW-DRAFT-044: recovery bound after the fresh project read (base = this raw; exact-match restore only; no NOD write, no QSAVE).
   var rec=RoomRecoveryUi044.Start(d,s,readGuard);
   if(rec.StartNote!=null){note=note==null?rec.StartNote:note+" "+rec.StartNote;noteWarn=noteWarn||rec.StartWarn;}
   try{
    for(int round=0;round<MaxRounds;round++){
     readGuard("trước khi mở "+Title);RoomReport037.Choice choice;bool review;string r1=TraceNum(round+1);
     using(var f=new RoomReport037(s,dpi)){
      f.Rescan=()=>Refresh(db,s,readGuard);f.Export=owner=>Export(db,s,readGuard,owner,dpi);f.Declare=(key,owner)=>Declare(s,key,owner,dpi);
      f.Heights=owner=>Heights(s,owner,dpi);f.Project=owner=>Project(db,s,readGuard,owner,dpi);f.Devices=owner=>DeviceHeights(s,owner,dpi);
      rec.Attach(f);
      f.ShowSession();if(note!=null)f.SetStatus(note,noteWarn);
      TraceShown(f,"REPORT");Trace("REPORT_BEFORE","round="+r1+" project="+s.projectMode+" scanned="+(s.batch!=null));DialogResult shown;
      try{shown=ShowReport(f);}catch(Exception ex){Trace("REPORT_ERROR","round="+r1+" type="+ex.GetType().Name);throw;}
      Trace("REPORT_AFTER","round="+r1+" result="+shown+" choice="+f.Chosen);
      readGuard("sau "+Title);choice=f.Chosen;review=f.ReviewConfirmed;state=f.SummaryText;}
     if(choice==RoomReport037.Choice.None){Trace("RUN_CLOSE","rounds="+r1);break;}
     note=null;noteWarn=false;
     try{
      if(choice==RoomReport037.Choice.Region)note=s.projectMode?RegionProject(d,s,readGuard,dpi):Region(d,s,readGuard,dpi);
      else if(choice==RoomReport037.Choice.Save){var o=s.projectMode?SaveProject(db,s,review,guard,readGuard):Save(db,s,review,guard,readGuard);note=o.message;noteWarn=o.warn;if(o.changed){changed=true;done.Add(o.message);}
       if(s.projectMode)rec.AfterSave(readGuard);}   // 044: base advances only after the NOD readback equals the saved raw
      else{var o=RunLegacy(d,guard,readGuard,dpi);note="Dữ liệu cũ (bảng BDGKL từng tuyến, không gộp vào phòng): "+o.message;noteWarn=o.warn;
       if(o.changed){changed=true;if(o.warn)warn=true;done.Add(o.message);}}
      Trace("ACTION_DONE","round="+r1+" choice="+choice);}
     catch(IOE ex){Trace("ACTION_REFUSED","round="+r1+" choice="+choice);note=ex.Message;noteWarn=true;}
     catch(Exception ex){Trace("ACTION_ERROR","round="+r1+" choice="+choice+" type="+ex.GetType().Name);throw;}
     rec.Changed();}}   // 044: checkpoint after every action round (accepted part recomputed; no CAD scan)
   catch(Exception ex){rec.End();   // 044 PATCH2: source-change subscription released on failure too (Close releases it on the normal path)
    if(!changed)throw;return new RouteOutcome034{changed=true,warn=true,message=string.Join(" ",done)+" Sau đó dừng: "+ex.Message};}
   string unsaved=s.dirty||s.projectDirty?" Thay đổi chưa lưu (khai báo phòng"+(s.projectDirty?" / thiết lập dự án, cao độ, bản nháp phòng giữ trong phiên":"")+") đã bỏ.":"";
   // 044: 'đã bỏ' is replaced only when the recovery file holds exactly the current unsaved state (written + read back); never claims a DWG save.
   var kept=rec.Close();if(kept!=null)unsaved=kept;else if(unsaved.Length>0)unsaved+=rec.NotKeptSuffix();
   if(!changed)return new RouteOutcome034{cancelled=true,message=Title+": "+state+unsaved+" Bản vẽ không đổi."};
   return new RouteOutcome034{changed=true,warn=warn||s.dirty||s.projectDirty,message=string.Join(" ",done)+unsaved+(done.Count>1?" (cùng một lệnh: U hoàn tác mọi lần lưu của lần mở bảng này).":"")};}
  static string Declare(RoomSession037 s,string key,IWin32Window owner,float dpi){
   var row=s.batch==null?null:s.batch.rows.FirstOrDefault(r=>string.Equals(r.rowKey,key,StringComparison.Ordinal));if(row==null)throw new IOE("Dòng không còn trong phòng; bấm 'Cập nhật'.");
   RoomDeclaration037 res;using(var f=new RoomDeclareDialog037(s,row,dpi)){var r=ShowDeclaration(f,owner);if(r!=DialogResult.OK||f.Result==null)return null;res=f.Result;}
   return ApplyDeclaration(s,key,res);}
  static string Region(Document d,RoomSession037 s,Action<string> readGuard,float dpi){var db=d.Database;
   readGuard("trước khi chọn phòng");List<RoomStoreEntry037> entries;string listError=null;
   try{entries=RoomStore037.ReadAll(db);}catch(IOE ex){entries=new List<RoomStoreEntry037>();listError=ex.Message;}
   RoomOptions037 opt;
   using(var f=new RoomOptionsDialog037(entries,listError,(int)db.Insunits,s,dpi)){
    TraceShown(f,"OPTIONS");Trace("OPTIONS_BEFORE","project=false entries="+TraceNum(entries.Count)+" listError="+(listError!=null));DialogResult r;
    try{r=ShowOptions(f);}catch(Exception ex){Trace("OPTIONS_ERROR","type="+ex.GetType().Name);throw;}
    Trace("OPTIONS_AFTER","result="+r+" accepted="+(f.Result!=null)+" useSavedRegion="+(f.Result!=null&&f.Result.useSavedRegion));
    readGuard("sau hộp chọn phòng");
    if(r!=DialogResult.OK||f.Result==null)return "Đã huỷ chọn phòng; phiên đang mở giữ nguyên, bản vẽ không đổi.";opt=f.Result;}
   string key=RoomStore037.RecordKey(opt.roomId);var entry=entries.FirstOrDefault(e=>e.key==key);
   Point3d a,b;
   if(opt.useSavedRegion){Trace("REGION_SAVED","draft="+(entry!=null&&entry.draft!=null));
    if(entry==null||entry.draft==null)throw new IOE("Phòng \""+opt.roomId+"\" chưa có vùng đã lưu đọc được; chọn vùng mới.");
    a=new Point3d(entry.draft.minX,entry.draft.minY,0);b=new Point3d(entry.draft.maxX,entry.draft.maxY,0);}
   else{readGuard("trước khi chọn góc phòng");var p=TracedPick(d);readGuard("sau khi chọn góc phòng");
    if(p==null)return "Đã huỷ chọn vùng (Esc); phiên đang mở giữ nguyên, bản vẽ không đổi.";
    if(p.Length!=2)throw new IOE("Cần đúng hai góc đối diện của phòng; phiên đang mở giữ nguyên.");a=p[0];b=p[1];}
   readGuard("trước khi quét phòng");var scan=TracedScan(db,a,b,opt.roomId,opt.units,opt.tolerance);
   // Only now (corners picked, scan done) does the new room replace the session.
   bool wasDirty=s.dirty;string oldRoom=s.roomId;
   s.roomId=opt.roomId;s.units=opt.units;s.tolerance=opt.tolerance;s.unitConfirmed=opt.unitConfirmed;s.minX=scan.minX;s.minY=scan.minY;s.maxX=scan.maxX;s.maxY=scan.maxY;s.hasRegion=true;
   s.saved=null;s.savedRaw=null;s.loadError=listError;s.devices.Clear();s.unmatched=new List<RoomDraftRow037>();s.dirty=false;s.heights=null;   // no leakage from the previous room
   s.adjustments=null;
   IList<RoomDraftRow037> decl=null;string mode=null;bool indep=false;
   if(entry!=null){s.savedRaw=entry.raw;
    if(entry.error!=null)s.loadError="Bản nháp đã lưu của phòng không đọc được: "+entry.error+" (MEPS không ghi đè).";
    else{s.saved=entry.draft;decl=entry.draft.rows;mode=entry.draft.conduitMode;indep=entry.draft.independentAssembliesConfirmed;foreach(var v in entry.draft.devices)s.devices[v.deviceId]=Copy(v);
     // restored as saved; never auto-applied to newly found devices
     s.heights=RoomHeight037.Copy(entry.draft.heights);
     // S5: kept as saved (no UI in this mode); never dropped silently
     s.adjustments=RoomAdjustments037.Copy(entry.draft.adjustments);}}
   Rebuild(s,scan,decl,mode,indep);
   var m=new StringBuilder("Đã quét. "+Summary(s)+".");
   if(wasDirty&&oldRoom!=null)m.Insert(0,"Thay đổi chưa lưu của phòng \""+oldRoom+"\" đã bỏ. ");
   if(s.saved!=null)m.Append(" Đã mở bản nháp lưu lúc "+s.saved.savedAt+".");
   if(s.loadError!=null)m.Append(" "+s.loadError);
   if(s.NeedsReview)m.Append(" HÌNH ĐÃ ĐỔI so với lần xác nhận trước: xem lại từng dòng, tích 'đã xem lại' và Lưu trước khi xuất.");
   if(s.unmatched.Count>0)m.Append(" "+s.unmatched.Count+" khai báo đã lưu không còn khớp tuyến hiện tại (vẫn giữ trong bản nháp).");
   return m.ToString();}
  static string Refresh(Database db,RoomSession037 s,Action<string> readGuard){if(!s.hasRegion)throw new IOE("Chưa chọn phòng; bấm 'Chọn vùng CAD'.");
   readGuard("trước khi cập nhật phòng");
   var scan=RoomCad037.Scan(db,new Point3d(s.minX,s.minY,0),new Point3d(s.maxX,s.maxY,0),s.roomId,s.units,s.tolerance);
   bool same=s.scan!=null&&scan.signature!=null&&string.Equals(scan.signature,s.scan.signature,StringComparison.Ordinal);
   Rebuild(s,scan,Capture(s),s.batch==null?null:s.batch.conduitMode,s.batch!=null&&s.batch.independentAssembliesConfirmed);
   return "Đã quét lại lúc "+s.scannedAt+(same?" (hình không đổi). ":" — HÌNH ĐÃ ĐỔI so với lần quét trước; kiểm tra lại các dòng. ")+Summary(s)+".";}
  static RouteOutcome034 Save(Database db,RoomSession037 s,bool review,Action<string> guard,Action<string> readGuard){
   if(s.batch==null||s.scan==null)throw new IOE("Chưa chọn phòng; chưa có gì để lưu. Bản vẽ không đổi.");
   if(s.loadError!=null)throw new IOE(s.loadError+" Bản vẽ không đổi.");
   if(s.scan.signature==null)throw new IOE("Lần quét chưa hoàn tất (vướng mắc CAD chặn việc đọc phòng); không lưu bản nháp gắn với hình chưa đọc được. Bản vẽ không đổi.");
   readGuard("trước khi kiểm tra hình để lưu");
   if(!RoomCad037.IsCurrent(db,s.scan))throw new IOE("Hình trong phòng đã đổi sau lần quét; bấm 'Cập nhật', xem lại rồi Lưu. Bản vẽ không đổi.");
   var draft=ToDraft(s,review);
   guard("ngay trước khi lưu bản nháp phòng");var res=RoomStore037.Save(db,draft,s.savedRaw,guard);
   s.saved=draft;s.savedRaw=res.raw;s.dirty=false;
   if(!res.changed)return new RouteOutcome034{cancelled=true,message="Bản nháp phòng \""+s.roomId+"\" trong bản vẽ đã đúng như vậy; không ghi gì."};
   return new RouteOutcome034{changed=true,warn=s.NeedsReview,message="Đã lưu bản nháp phòng \""+s.roomId+"\" vào bản vẽ ("+Summary(s)+"). U/UNDO hoàn tác; MEPS không tự lưu tệp DWG."+
    (s.NeedsReview?" Chưa xác nhận đã xem lại thay đổi hình: xuất Excel vẫn bị chặn.":"")};}
  static string SafeName(string s){var bad=Path.GetInvalidFileNameChars();var b=new StringBuilder();foreach(char c in s??"")b.Append(Array.IndexOf(bad,c)>=0||char.IsWhiteSpace(c)?'_':c);
   var t=b.ToString();return t.Length>40?t.Substring(0,40):t;}
  static string Export(Database db,RoomSession037 s,Action<string> readGuard,IWin32Window owner,float dpi){
   // S4 project mode: scope chooser (PHÒNG ĐANG MỞ / TẦNG ĐANG CHỌN / TOÀN DỰ ÁN) → every selected room rebuilt fresh from CAD → one grouped workbook
   // (RoomProjectExportUi037; raw + scans re-checked after the path chooser; new file only). The session is not changed.
   // 044: raw (not accepted) cell texts block every export scope until corrected or explicitly discarded.
   var rawBlock=RoomRecoveryUi044.BlockText(s);if(rawBlock!=null)throw new IOE(rawBlock);
   if(s.projectMode)return RoomProjectExportUi037.Export(db,s,readGuard,owner,dpi);
   if(s.batch==null||s.scan==null)throw new IOE("Chưa chọn phòng.");
   readGuard("trước khi xuất Excel");
   if(!RoomCad037.IsCurrent(db,s.scan))throw new IOE("Hình trong phòng đã đổi sau lần quét; bấm 'Cập nhật' rồi xem lại. Chưa xuất.");
   Recalc(s);var why=ExportReason(s);if(why!=null)throw new IOE(why);
   string label=s.roomId;
   var path=ChooseExportPath(owner,"MEPS_BDGKL_"+SafeName(label)+"_"+DateTime.Now.ToString("yyyyMMdd_HHmm",CultureInfo.InvariantCulture)+".xlsx");
   if(string.IsNullOrEmpty(path))return "Đã huỷ xuất Excel; chưa ghi tệp nào.";
   if(File.Exists(path)||Directory.Exists(path))throw new IOE("Tệp \""+path+"\" đã tồn tại; MEPS không ghi đè. Chọn tên khác.");
   readGuard("ngay trước khi xuất Excel");
   if(!RoomCad037.IsCurrent(db,s.scan))throw new IOE("Hình trong phòng đã đổi trong lúc chọn tệp; bấm 'Cập nhật' rồi xem lại. Chưa xuất.");
   Recalc(s);why=ExportReason(s);if(why!=null)throw new IOE(why);
   RoomXlsx037.Write(path,s.batch,s.heights);
   return "Đã xuất Excel: "+path+" ("+s.batch.rows.Count+" dòng, quét lúc "+s.scannedAt+", đơn vị "+s.units+", phòng "+s.roomId+"). MEPS không tự mở tệp; bản vẽ không đổi.";}

  // ---- S3 project mode ----
  // The persisted project raw must still be the one this session read/saved (another command or U may have changed it): otherwise stop, no write/export.
  static void CheckProjectRaw(Database db,RoomSession037 s,string when){RoomProjectRead037 r;
   try{r=RoomProjectStore037.Read(db);}catch(IOE ex){throw new IOE("Dữ liệu dự án trong bản vẽ "+when+" không đọc được: "+ex.Message+" Chưa ghi/xuất gì.");}
   if(!string.Equals(r.raw,s.projectRaw,StringComparison.Ordinal))
    throw new IOE("Dữ liệu dự án trong bản vẽ đã đổi sau khi mở bảng (lệnh khác hoặc U) — phát hiện "+when+"; đóng bảng và mở lại. MEPS không ghi đè, chưa ghi/xuất gì.");}
  // DỰ ÁN / TẦNG / PHÒNG. Possible before the first scan. Only Apply changes the session (in memory, projectDirty); Cancel = nothing.
  static string Project(Database db,RoomSession037 s,Action<string> readGuard,IWin32Window owner,float dpi){
   if(s.projectError!=null)throw new IOE("Dữ liệu dự án trong bản vẽ không đọc được ("+s.projectError+"); MEPS không mở thiết lập để tránh ghi đè. 'Dữ liệu cũ' vẫn dùng được.");
   readGuard("trước khi mở thiết lập dự án");var drawingId=RoomProjectStore037.DrawingId(db);
   string legacy;   // explicit read-only listing of the legacy single-room drafts (never migrated or written here)
   try{var l=RoomStore037.ReadAll(db);legacy=l.Count==0?null:"Bản nháp phòng KIỂU CŨ trong bản vẽ (chỉ đọc, không tự chuyển vào dự án): "+
     string.Join("; ",l.Take(20).Select(e=>e.draft!=null?"\""+e.roomId+"\" lưu lúc "+e.draft.savedAt+" ("+e.draft.rows.Count+" dòng)":"mục "+e.key+" không đọc được"))+(l.Count>20?"; …":"")+".";}
   catch(IOE ex){legacy="Kho bản nháp kiểu cũ không đọc được: "+ex.Message+" (chỉ đọc; MEPS không ghi đè).";}
   RoomProjectDocument037 res;string sel;
   using(var f=new RoomProjectDialog037(s.projectDocument,drawingId,()=>Guid.NewGuid(),RoomProjectUi037.Devices(s),s.batch==null?null:s.roomId,s.roomId??s.selectedRoomId,legacy,dpi)){
    var r=ShowProject(f,owner);if(r!=DialogResult.OK||f.Result==null)return null;res=f.Result;sel=f.SelectedRoomId;}
   readGuard("sau thiết lập dự án");
   s.projectDocument=res;s.projectDirty=true;
   if(sel!=null&&s.batch==null){HierFloor037 fl;if(RoomProjectUi037.FindRoom(res.model,sel,out fl)!=null){s.selectedRoomId=sel;s.selectedFloorId=fl.id;}}
   if(s.batch!=null)Recalc(s);
   return "Đã áp dụng thiết lập dự án (chưa lưu vào bản vẽ: bấm 'Lưu vào bản vẽ'). "+(s.batch!=null?"Bảng phòng đang mở đã tính lại theo cao độ hiện hành. "+Summary(s)+".":
    "Bấm 'Chọn vùng CAD' để chọn phòng của dự án và quét.");}
  // CAO ĐỘ THIẾT BỊ of the open room (actual handles + authoritative types of the current scan).
  static string DeviceHeights(RoomSession037 s,IWin32Window owner,float dpi){
   if(s.projectDocument==null)throw new IOE("Chưa có dự án.");if(s.batch==null)throw new IOE("Chọn vùng CAD (quét phòng) trước.");
   HierFloor037 fl;if(RoomProjectUi037.FindRoom(s.projectDocument.model,s.roomId,out fl)==null)throw new IOE("Phòng đang mở không có trong dự án.");
   RoomProjectData037 res;
   using(var f=new RoomDeviceDialog037(s.projectDocument.model,s.roomId,RoomProjectUi037.Devices(s),RoomProjectUi037.RoomLabel(s),dpi)){
    var r=ShowDevices(f,owner);if(r!=DialogResult.OK||f.Result==null)return null;res=f.Result;}
   var doc=RoomProjectUi037.CopyDocument(s.projectDocument);doc.model=res;s.projectDocument=doc;s.projectDirty=true;Recalc(s);
   return "Đã áp dụng cao độ riêng thiết bị (chưa lưu vào bản vẽ). "+Summary(s)+".";}
  // Region in project mode: choose an EXISTING project room (stable ID), units/tolerance explicit, optional saved rectangle, same native PickRegion.
  // The open room's pending declarations are kept as a draft in a COPY of the project document; the session changes only after a successful scan.
  static string RegionProject(Document d,RoomSession037 s,Action<string> readGuard,float dpi){var db=d.Database;
   if(s.projectError!=null)throw new IOE("Dữ liệu dự án trong bản vẽ không đọc được; MEPS không chọn phòng/ghi đè. Phiên đang mở giữ nguyên.");
   var m=s.projectDocument==null?null:s.projectDocument.model;
   if(m==null||!m.hierarchy.floors.Any(f=>f.rooms.Count>0))
    throw new IOE("Chưa có phòng trong dự án: bấm 'DỰ ÁN / TẦNG / PHÒNG' để tạo dự án, tầng và phòng trước (MEPS không tự tạo phòng). Phiên đang mở giữ nguyên.");
   bool pending=s.batch!=null&&s.dirty;bool canKeep=!pending||(s.scan!=null&&s.scan.signature!=null&&s.loadError==null);
   // Capture first (pure; discarded if anything below is cancelled or fails).
   var doc=s.projectDocument;string kept=null;
   if(pending&&canKeep){doc=RoomProjectUi037.CopyDocument(doc);RoomProjectUi037.PutDraft(doc,ToDraft(s,false));RoomFastEntryUi045.PutRoom(doc,s);kept=RoomProjectUi037.RoomLabel(s);}
   readGuard("trước khi chọn phòng");RoomOptions037 opt;
   using(var f=new RoomOptionsDialog037(doc,(int)db.Insunits,s,!canKeep,dpi)){
    TraceShown(f,"OPTIONS");Trace("OPTIONS_BEFORE","project=true pending="+pending+" canKeep="+canKeep);DialogResult r;
    try{r=ShowOptions(f);}catch(Exception ex){Trace("OPTIONS_ERROR","type="+ex.GetType().Name);throw;}
    Trace("OPTIONS_AFTER","result="+r+" accepted="+(f.Result!=null)+" useSavedRegion="+(f.Result!=null&&f.Result.useSavedRegion));
    readGuard("sau hộp chọn phòng");
    if(r!=DialogResult.OK||f.Result==null)return "Đã huỷ chọn phòng; phiên đang mở giữ nguyên, bản vẽ không đổi.";opt=f.Result;}
   if(!canKeep&&!opt.discardPending)throw new IOE("Chưa xác nhận bỏ thay đổi chưa lưu của phòng đang mở; phiên giữ nguyên.");
   HierFloor037 fl;var room=RoomProjectUi037.FindRoom(doc.model,opt.roomId,out fl);if(room==null)throw new IOE("Phòng đã chọn không có trong dự án; phiên giữ nguyên.");
   string err;var draft=RoomProjectUi037.DraftOf(doc,opt.roomId,out err);
   Point3d a,b;
   if(opt.useSavedRegion){Trace("REGION_SAVED","draft="+(draft!=null));
    if(draft==null)throw new IOE("Phòng "+RoomProjectUi037.RoomLabel(room)+" chưa có vùng đã lưu đọc được; chọn vùng mới.");
    a=new Point3d(draft.minX,draft.minY,0);b=new Point3d(draft.maxX,draft.maxY,0);}
   else{readGuard("trước khi chọn góc phòng");var p=TracedPick(d);readGuard("sau khi chọn góc phòng");
    if(p==null)return "Đã huỷ chọn vùng (Esc); phiên đang mở giữ nguyên, bản vẽ không đổi.";
    if(p.Length!=2)throw new IOE("Cần đúng hai góc đối diện của phòng; phiên đang mở giữ nguyên.");a=p[0];b=p[1];}
   readGuard("trước khi quét phòng");var scan=TracedScan(db,a,b,opt.roomId,opt.units,opt.tolerance);   // scan room ID = stable RM_ ID
   // ---- only now does the session change ----
   bool dropped=pending&&!canKeep;string oldLabel=RoomProjectUi037.RoomLabel(s);
   s.projectDocument=doc;if(kept!=null)s.projectDirty=true;
   s.roomId=opt.roomId;s.selectedRoomId=opt.roomId;s.selectedFloorId=fl.id;s.units=opt.units;s.tolerance=opt.tolerance;s.unitConfirmed=opt.unitConfirmed;
   s.minX=scan.minX;s.minY=scan.minY;s.maxX=scan.maxX;s.maxY=scan.maxY;s.hasRegion=true;
   s.saved=null;s.savedRaw=null;s.loadError=null;s.devices.Clear();s.unmatched=new List<RoomDraftRow037>();s.dirty=false;s.heights=null;
   // S5: no spill of the previous room's BỎ DÒNG / BỔ SUNG (they went into its draft above when kept)
   s.adjustments=null;
   IList<RoomDraftRow037> decl=null;string mode=null;bool indep=false;int legacyHeights=0;
   if(err!=null)s.loadError="Bản nháp của phòng không đọc được: "+err+" (MEPS không ghi đè).";
   else if(draft!=null){s.saved=draft;decl=draft.rows;mode=draft.conduitMode;indep=draft.independentAssembliesConfirmed;legacyHeights=draft.devices.Count+(draft.heights!=null?1:0);
    s.adjustments=RoomAdjustments037.Copy(draft.adjustments);}
   Rebuild(s,scan,decl,mode,indep);
   var msg=new StringBuilder("Đã quét. "+Summary(s)+".");
   if(kept!=null)msg.Insert(0,"Khai báo chưa lưu của phòng "+kept+" được giữ làm bản nháp trong phiên (bấm 'Lưu vào bản vẽ' để ghi). ");
   if(dropped)msg.Insert(0,"Thay đổi chưa lưu của phòng "+oldLabel+" đã bỏ theo xác nhận. ");
   if(s.saved!=null)msg.Append(" Đã mở bản nháp lúc "+s.saved.savedAt+".");
   if(legacyHeights>0)msg.Append(" Bản nháp có cao độ thiết bị/hộp cao độ kiểu cũ: KHÔNG dùng (dự án là nguồn cao độ).");
   if(s.loadError!=null)msg.Append(" "+s.loadError);
   if(s.NeedsReview)msg.Append(" HÌNH ĐÃ ĐỔI so với lần xác nhận trước: xem lại từng dòng, tích 'đã xem lại' và Lưu trước khi xuất.");
   if(s.unmatched.Count>0)msg.Append(" "+s.unmatched.Count+" khai báo đã lưu không còn khớp tuyến hiện tại (vẫn giữ trong bản nháp).");
   return msg.ToString();}
  // Main save in project mode: project document (+ the open room's draft when a complete scan exists) in ONE RoomProjectStore037.Save (bound document guards,
  // optimistic projectRaw). Session state advances only after success. Legacy RoomStore037 is never written here.
  static RouteOutcome034 SaveProject(Database db,RoomSession037 s,bool review,Action<string> guard,Action<string> readGuard){
   if(s.projectError!=null)throw new IOE("Dữ liệu dự án trong bản vẽ không đọc được ("+s.projectError+"); MEPS không ghi đè. Bản vẽ không đổi.");
   if(s.projectDocument==null)throw new IOE("Chưa có dự án: bấm 'DỰ ÁN / TẦNG / PHÒNG' trước. Bản vẽ không đổi.");
   var doc=RoomProjectUi037.CopyDocument(s.projectDocument);RoomDraft037 draft=null;string skipped=null;
   if(s.batch!=null&&s.scan!=null){
    if(s.loadError!=null)throw new IOE(s.loadError+" Bản vẽ không đổi.");
    if(s.scan.signature==null)skipped="Lần quét chưa hoàn tất nên khai báo của phòng đang mở CHƯA được lưu (chỉ lưu thiết lập dự án).";
    else{readGuard("trước khi kiểm tra hình để lưu");
     if(!RoomCad037.IsCurrent(db,s.scan))throw new IOE("Hình trong phòng đã đổi sau lần quét; bấm 'Cập nhật', xem lại rồi Lưu. Bản vẽ không đổi.");
     draft=ToDraft(s,review);RoomProjectUi037.PutDraft(doc,draft);RoomFastEntryUi045.PutRoom(doc,s);}}   // 045: row presets / explicit blanks with the draft
   readGuard("trước khi lưu dữ liệu dự án");CheckProjectRaw(db,s,"trước khi lưu");
   guard("ngay trước khi lưu dữ liệu dự án");var res=RoomProjectStore037.Save(db,doc,s.projectRaw,guard);
   // ---- success: replace the session model/raw ----
   s.projectDocument=doc;s.projectRaw=res.raw;s.projectDirty=false;if(draft!=null){s.saved=draft;s.dirty=false;}
   string what=draft!=null?"dữ liệu dự án và bản nháp phòng "+RoomProjectUi037.RoomLabel(s):"dữ liệu dự án";
   if(!res.changed)return new RouteOutcome034{cancelled=true,warn=skipped!=null,message="Dữ liệu dự án trong bản vẽ đã đúng như vậy; không ghi gì."+(skipped!=null?" "+skipped:"")};
   return new RouteOutcome034{changed=true,warn=s.NeedsReview||skipped!=null,message="Đã lưu "+what+" vào bản vẽ ("+(s.batch!=null?Summary(s):RoomProjectUi037.ScopeText(s))+
    "). Một lần ghi; U/UNDO hoàn tác; MEPS không tự lưu tệp DWG."+(skipped!=null?" "+skipped:"")+(s.NeedsReview&&draft!=null?" Chưa xác nhận đã xem lại thay đổi hình: xuất Excel vẫn bị chặn.":"")};}
 }
}
#endif
