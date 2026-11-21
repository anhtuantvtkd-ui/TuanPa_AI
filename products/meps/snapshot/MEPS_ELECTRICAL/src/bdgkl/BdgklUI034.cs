using System;using System.Drawing;using System.Linq;using System.Text;using System.Globalization;using System.Collections.Generic;using System.Text.RegularExpressions;using System.Windows.Forms;
#if BDGKL034
using Autodesk.AutoCAD.EditorInput;using Document=Autodesk.AutoCAD.ApplicationServices.Document;using App=Autodesk.AutoCAD.ApplicationServices.Application;
using ObjectId=Autodesk.AutoCAD.DatabaseServices.ObjectId;using CadLine=Autodesk.AutoCAD.DatabaseServices.Line;using CadPolyline=Autodesk.AutoCAD.DatabaseServices.Polyline;
using CadBlockRef=Autodesk.AutoCAD.DatabaseServices.BlockReference;
namespace Tvxd.Meps {
 // TVXD-MEPS-WIREQTY-034 BDGKL native pilot UI over BdgklCad034 (structure of the approved TESTS/BDGKL034/PREVIEW.html). One DÂY DẪN item 'Bảng khối lượng điện…'
 // (WireView010; TVXD-MEPS-ROOMQTY-037: the only quantity entry, label only — action key, backend and saved records unchanged) → CompactView06.Request → the plugin's queued bound-document job → RouteUi034.Handles/Run → BdgklUi034.Run. No palette row, no own queue.
 //  - BdgklReport034: separate resizable light window (modal through Application.ShowModalDialog). Filters Tầng / Phòng/vùng / Lộ; 'Thêm tuyến', 'Sửa tuyến',
 //    'Cập nhật'; 'Chọn vùng CAD' and 'Xuất Excel' disabled as 'bước tiếp theo'. Tabs TUYẾN (11 columns) / DÂY / ỐNG, read-only grids from ONE scan; selected
 //    row details and the calculation below; explicit status text per row and excluded count. Numbers keep full precision; screen shows 1 decimal.
 //  - Thêm/Sửa: the report closes, Editor.GetEntity picks the route then the start and end blocks (Esc = nothing written, the report reopens), then
 //    BdgklDialog034 (owned modal) collects the declared values; Lưu = BdgklCad034.Apply (one transaction). The report reopens after every step.
 // Nothing is preset: no engineering value, unit, mode, factor or code is defaulted; only an existing saved declaration prefills the dialog.
 static class BdgklLook034 {
  internal static readonly Color Back=Color.White,Paper=Color.FromArgb(0xF4,0xF6,0xF8),Line=Color.FromArgb(0xD5,0xDC,0xE2),Ink=Color.FromArgb(0x1F,0x2A,0x33),
   Muted=Color.FromArgb(0x5C,0x68,0x73),Teal=Color.FromArgb(0x0F,0x7F,0x76),TealLight=Color.FromArgb(0xDF,0xF2,0xEF),Error=Color.FromArgb(0xB0,0x37,0x1C),
   ExcludedBack=Color.FromArgb(0xFC,0xF0,0xEB),Disabled=Color.FromArgb(0x9A,0xA4,0xAD);
  internal static float Dpi(float dpi){return float.IsNaN(dpi)||dpi<=0?1f:Math.Max(1f,Math.Min(3f,dpi));}
  internal static void Style(Button b,bool main,Font f,int minW,int minH){b.AutoSize=true;b.AutoSizeMode=System.Windows.Forms.AutoSizeMode.GrowAndShrink;b.MinimumSize=new Size(minW,minH);b.Font=f;
   b.FlatStyle=FlatStyle.Flat;b.UseVisualStyleBackColor=false;b.BackColor=main?Teal:Paper;b.ForeColor=main?Color.White:Ink;b.FlatAppearance.BorderColor=main?Teal:Line;
   b.FlatAppearance.MouseOverBackColor=main?Color.FromArgb(0x12,0x96,0x8B):TealLight;}
  internal static void ReadOnlyBox(TextBox b){b.Multiline=true;b.ReadOnly=true;b.WordWrap=true;b.ScrollBars=ScrollBars.Vertical;b.BorderStyle=BorderStyle.FixedSingle;b.BackColor=Paper;b.ForeColor=Ink;b.TabStop=true;b.Dock=DockStyle.Fill;}
  internal static string M(double? v){return v.HasValue?Route034.Meters(v.Value):"—";}
  // BN04: explanation text uses 15 significant digits like the core equation (6.1999999999999993 → 6.2); the doubles themselves stay unrounded and the
  // stored record keeps "R" text. Display only.
  internal static string R(double? v){return v.HasValue?v.Value.ToString("G15",CultureInfo.InvariantCulture):"—";}
  internal static string Mode(string m){return m==Bdgkl034.Ceiling?"Trần":m==Bdgkl034.Floor?"Sàn":m==Bdgkl034.Wall?"Tường":m??"—";}
  internal static string Status(string s){return s==BdgklCad034.Ok?"Đủ dữ liệu — được tính":s==Bdgkl034.MissingData?"Thiếu dữ liệu — KHÔNG tính":s==Bdgkl034.Check?"Cần kiểm tra — KHÔNG tính":
   s==BdgklCad034.Reconfirm?"Cần xác nhận lại — KHÔNG tính":s==BdgklCad034.Invalid?"Bản ghi không đọc được — KHÔNG tính":s??"";}
 }
 // Read-only report of one scan. Chosen = what the user asked for when it closed (Add / Edit of EditRouteId); None = closed (Đóng/Esc).
 public sealed class BdgklReport034:Form {
  public enum Choice {None,Add,Edit}
  public const string AllCaption="(tất cả)";
  public readonly ComboBox FloorBox=new ComboBox(),AreaBox=new ComboBox(),CircuitBox=new ComboBox();
  public readonly Button AddBtn=new Button(),EditBtn=new Button(),RefreshBtn=new Button(),RegionBtn=new Button(),ExportBtn=new Button(),CloseBtn=new Button();
  public readonly Label ScopeLabel=new Label(),ConfigLabel=new Label(),SummaryLabel=new Label();
  public readonly TabControl Tabs=new TabControl();public readonly TabPage RouteTab=new TabPage("TUYẾN"),WireTab=new TabPage("DÂY"),PipeTab=new TabPage("ỐNG");
  public readonly DataGridView RouteGrid,WireGrid,PipeGrid;
  public readonly TextBox DetailBox=new TextBox(),StatusBox=new TextBox();
  public BdgklScan034 Scan;public string ScannedAt,SummaryText;public Choice Chosen=Choice.None;public ObjectId? EditRouteId;
  // Bound by BdgklUi034: guarded fresh BdgklCad034.ScanModel.
  public Func<BdgklScan034> Rescan;
  public static readonly string[] RouteColumns={"STT","Tuyến","Hướng đi","Ngang (m)","Đứng (m)","Chờ (m)","Tổng tuyến (m)","Bộ dây","Ống","Ống chìm / nổi (m)","Trạng thái"};
  readonly float dpi;readonly Font face,bold;bool filling;readonly ToolTip tips=new ToolTip{ShowAlways=true};
  int S(int px){return Math.Max(1,(int)Math.Round(px*dpi));}
  public BdgklReport034(float dpi){this.dpi=BdgklLook034.Dpi(dpi);
   Text="MEPS — "+BdgklUi034.Title+" (pilot một tuyến)";Name="bdgkl034_report";FormBorderStyle=FormBorderStyle.Sizable;MinimizeBox=false;MaximizeBox=true;ShowInTaskbar=false;
   StartPosition=FormStartPosition.CenterParent;AutoScaleMode=AutoScaleMode.None;face=new Font("Segoe UI",12f*this.dpi,FontStyle.Regular,GraphicsUnit.Pixel);
   bold=new Font("Segoe UI",12f*this.dpi,FontStyle.Bold,GraphicsUnit.Pixel);Font=face;BackColor=BdgklLook034.Back;ForeColor=BdgklLook034.Ink;
   ClientSize=new Size(S(1000),S(640));MinimumSize=new Size(S(760),S(500));
   var pane=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=1,RowCount=7,Padding=new Padding(S(8)),BackColor=BdgklLook034.Back};pane.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
   // BN03: filters row, WRAPPING actions row (auto height), wrapping scope/config line (auto height), tabs (60 %), 2-line wrapping summary, details (40 %), status.
   // 0 = AutoSize row. Every button stays inside the window at any width ≥ MinimumSize and any DPI (rows grow instead of clipping).
   foreach(int h in new[]{S(34),0,0,-60,S(42),-40,S(40)})pane.RowStyles.Add(h<0?new RowStyle(SizeType.Percent,-h):h==0?new RowStyle(SizeType.AutoSize):new RowStyle(SizeType.Absolute,h));
   var bar=new FlowLayoutPanel{Dock=DockStyle.Fill,WrapContents=true,Margin=new Padding(0),BackColor=BdgklLook034.Back};
   var actions=new FlowLayoutPanel{Dock=DockStyle.Fill,WrapContents=true,AutoSize=true,AutoSizeMode=System.Windows.Forms.AutoSizeMode.GrowAndShrink,Margin=new Padding(0,0,0,S(4)),BackColor=BdgklLook034.Back,Name="bdgkl034_actions"};
   var filters=new[]{Tuple.Create("Tầng",FloorBox,"bdgkl034_filter_floor"),Tuple.Create("Phòng/vùng",AreaBox,"bdgkl034_filter_area"),Tuple.Create("Lộ",CircuitBox,"bdgkl034_filter_circuit")};
   foreach(var f in filters){bar.Controls.Add(new Label{Text=f.Item1,AutoSize=true,Margin=new Padding(S(4),S(8),S(2),0),ForeColor=BdgklLook034.Muted});
    f.Item2.Name=f.Item3;f.Item2.AccessibleName="Lọc "+f.Item1;f.Item2.DropDownStyle=ComboBoxStyle.DropDownList;f.Item2.Width=S(120);f.Item2.Margin=new Padding(0,S(4),S(6),0);
    f.Item2.SelectedIndexChanged+=(s,e)=>{if(!filling)Fill();};bar.Controls.Add(f.Item2);}
   AddBtn.Text="Thêm tuyến";AddBtn.Name="bdgkl034_add";EditBtn.Text="Sửa tuyến";EditBtn.Name="bdgkl034_edit";RefreshBtn.Text="Cập nhật";RefreshBtn.Name="bdgkl034_refresh";
   RegionBtn.Text="Chọn vùng CAD — bước tiếp theo";RegionBtn.Name="bdgkl034_region";ExportBtn.Text="Xuất Excel — bước tiếp theo";ExportBtn.Name="bdgkl034_export";
   CloseBtn.Text="Đóng";CloseBtn.Name="bdgkl034_close";CloseBtn.DialogResult=DialogResult.Cancel;
   // Đóng sits with the working buttons; the disabled next-step buttons come last, so they are the ones that wrap.
   foreach(var b in new[]{AddBtn,EditBtn,RefreshBtn,CloseBtn,RegionBtn,ExportBtn}){BdgklLook034.Style(b,b==AddBtn,face,S(70),S(28));b.Margin=new Padding(S(4),S(3),0,0);b.AccessibleName=b.Text;actions.Controls.Add(b);}
   // Not-yet-implemented scope: disabled, never a fake success.
   RegionBtn.Enabled=false;ExportBtn.Enabled=false;RegionBtn.ForeColor=BdgklLook034.Disabled;ExportBtn.ForeColor=BdgklLook034.Disabled;
   ScopeLabel.Text=BdgklCad034.Scope;ScopeLabel.Name="bdgkl034_scope";ScopeLabel.Font=bold;ScopeLabel.AutoSize=true;ScopeLabel.Margin=new Padding(0,S(4),S(16),0);
   ConfigLabel.Name="bdgkl034_config";ConfigLabel.AutoSize=true;ConfigLabel.ForeColor=BdgklLook034.Muted;ConfigLabel.Margin=new Padding(0,S(4),0,0);
   var line=new FlowLayoutPanel{Dock=DockStyle.Fill,WrapContents=true,AutoSize=true,AutoSizeMode=System.Windows.Forms.AutoSizeMode.GrowAndShrink,Margin=new Padding(0,0,0,S(4)),BackColor=BdgklLook034.Back};line.Controls.Add(ScopeLabel);line.Controls.Add(ConfigLabel);
   RouteGrid=Grid("bdgkl034_routes","Bảng tuyến",RouteColumns,new[]{5,30,9,9,9,8,11,10,8,16,22},3,6);
   WireGrid=Grid("bdgkl034_wires","Tổng dây theo nhóm",new[]{"Tầng","Phòng/vùng","Lộ","Bộ dây","Ống","Tuyến được tính","Dòng không tính","Mét tuyến thô (m)","Hệ số","Mét tuyến sau hệ số (m)","Trạng thái nhóm"},
    new[]{10,12,8,10,8,9,9,11,7,12,20},5,9);
   PipeGrid=Grid("bdgkl034_pipes","Tổng ống theo nhóm",new[]{"Tầng","Phòng/vùng","Lộ","Ống","Bộ dây","Ống ngang W thô (m)","Ống đứng X thô (m)","Chìm thô (m)","Nổi thô (m)","Hệ số","Chìm sau hệ số (m)","Nổi sau hệ số (m)","Trạng thái nhóm"},
    new[]{9,10,7,7,9,10,10,9,9,6,10,10,16},5,11);
   RouteTab.Name="bdgkl034_tab_route";WireTab.Name="bdgkl034_tab_wire";PipeTab.Name="bdgkl034_tab_pipe";
   RouteTab.Controls.Add(RouteGrid);WireTab.Controls.Add(WireGrid);PipeTab.Controls.Add(PipeGrid);
   Tabs.Name="bdgkl034_tabs";Tabs.AccessibleName="Chế độ xem TUYẾN / DÂY / ỐNG";Tabs.Dock=DockStyle.Fill;Tabs.TabPages.Add(RouteTab);Tabs.TabPages.Add(WireTab);Tabs.TabPages.Add(PipeTab);
   Tabs.SelectedIndexChanged+=(s,e)=>Detail();
   SummaryLabel.Name="bdgkl034_summary";SummaryLabel.Dock=DockStyle.Fill;SummaryLabel.AutoSize=false;SummaryLabel.AutoEllipsis=false;SummaryLabel.TextAlign=ContentAlignment.MiddleLeft;SummaryLabel.Font=bold;
   BdgklLook034.ReadOnlyBox(DetailBox);DetailBox.Name="bdgkl034_detail";DetailBox.AccessibleName="Chi tiết dòng đã chọn và phép tính";
   BdgklLook034.ReadOnlyBox(StatusBox);StatusBox.Name="bdgkl034_status";StatusBox.AccessibleName="Thông báo";StatusBox.BackColor=BdgklLook034.Back;StatusBox.BorderStyle=BorderStyle.None;
   pane.Controls.Add(bar,0,0);pane.Controls.Add(actions,0,1);pane.Controls.Add(line,0,2);pane.Controls.Add(Tabs,0,3);pane.Controls.Add(SummaryLabel,0,4);pane.Controls.Add(DetailBox,0,5);pane.Controls.Add(StatusBox,0,6);
   Controls.Add(pane);CancelButton=CloseBtn;
   AddBtn.Click+=(s,e)=>{Chosen=Choice.Add;EditRouteId=null;DialogResult=DialogResult.OK;};
   EditBtn.Click+=(s,e)=>StartEdit();RefreshBtn.Click+=(s,e)=>DoRefresh();
   foreach(var g in new[]{RouteGrid,WireGrid,PipeGrid})g.SelectionChanged+=(s,e)=>Detail();
   EditBtn.Enabled=false;}
  DataGridView Grid(string name,string acc,string[] cols,int[] weights,int firstNumber,int lastNumber){
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
  public void SetStatus(string text,bool error){StatusBox.ForeColor=error?BdgklLook034.Error:BdgklLook034.Ink;StatusBox.Text=text??"";}
  static string Pick(ComboBox b){var s=b.SelectedItem as string;return s==null||s==AllCaption?null:s;}
  static void Options(ComboBox b,IEnumerable<string> values){string keep=Pick(b);b.Items.Clear();b.Items.Add(AllCaption);
   foreach(var v in values.Where(x=>x!=null).Distinct(StringComparer.Ordinal).OrderBy(x=>x,StringComparer.Ordinal))b.Items.Add(v);
   int i=keep==null?0:b.Items.IndexOf(keep);b.SelectedIndex=i<0?0:i;}
  bool Match(string floor,string area,string circuit){string f=Pick(FloorBox),a=Pick(AreaBox),c=Pick(CircuitBox);
   return (f==null||string.Equals(f,floor,StringComparison.Ordinal))&&(a==null||string.Equals(a,area,StringComparison.Ordinal))&&(c==null||string.Equals(c,circuit,StringComparison.Ordinal));}
  bool Filtered{get{return Pick(FloorBox)!=null||Pick(AreaBox)!=null||Pick(CircuitBox)!=null;}}
  // Replaces everything on screen with this scan (filters kept when their value still exists).
  public void ShowScan(BdgklScan034 scan,string scannedAt){if(scan==null)throw new ArgumentNullException("scan");Scan=scan;ScannedAt=scannedAt;
   filling=true;try{var cfgs=scan.rows.Where(r=>r.record!=null).Select(r=>r.record.config).ToList();
    Options(FloorBox,cfgs.Select(c=>c.floorId));Options(AreaBox,cfgs.Select(c=>c.areaId));Options(CircuitBox,cfgs.Select(c=>c.circuitId));}
   finally{filling=false;}
   Fill();}
  void Fill(){var scan=Scan;if(scan==null)return;
   RouteGrid.Rows.Clear();WireGrid.Rows.Clear();PipeGrid.Rows.Clear();int stt=0,hidden=0;
   foreach(var r in scan.rows){var c=r.record==null?null:r.record.config;
    if(c==null?Filtered:!Match(c.floorId,c.areaId,c.circuitId)){hidden++;continue;}
    var x=r.result;stt++;
    string route=(r.startLabel??"?")+" → "+(r.endLabel??"?");
    string conduit=x==null?"—":"Chìm "+BdgklLook034.M(x.conduitHiddenM)+" / Nổi "+BdgklLook034.M(x.conduitExposedM);
    int i=RouteGrid.Rows.Add(stt.ToString(CultureInfo.InvariantCulture),route,BdgklLook034.Mode(c==null?null:c.routingMode),BdgklLook034.M(x==null?null:x.horizontalM),
     BdgklLook034.M(x==null?null:x.verticalM),BdgklLook034.M(x==null?null:x.tailsM),BdgklLook034.M(x==null?null:x.routeM),c==null?"—":c.assemblyCode,c==null?"—":c.conduitCode,conduit,BdgklLook034.Status(r.status));
    var row=RouteGrid.Rows[i];row.Tag=r;
    row.Cells[1].ToolTipText="Tuyến "+r.routeHandle+(r.record!=null?" · đầu "+r.record.startHandle+" · cuối "+r.record.endHandle:"");
    row.Cells[10].ToolTipText=r.reason??"";if(!r.Countable)row.DefaultCellStyle.BackColor=BdgklLook034.ExcludedBack;}
   if(scan.batch!=null)foreach(var g in scan.batch.groups){if(!Match(g.floorId,g.areaId,g.circuitId))continue;
    string st=g.status==Bdgkl034.Ok?"Đủ":g.status==Bdgkl034.MissingData?"CHƯA ĐỦ (thiếu dữ liệu)":"CHƯA ĐỦ (cần kiểm tra)";
    int i=WireGrid.Rows.Add(g.floorId,g.areaId,g.circuitId,g.assemblyCode,g.conduitCode,g.countedRows.ToString(CultureInfo.InvariantCulture),g.excludedRows.ToString(CultureInfo.InvariantCulture),
     BdgklLook034.M(g.routeM),BdgklLook034.R(g.factor),BdgklLook034.M(g.routeAfterFactorM),st);WireGrid.Rows[i].Tag=g;
    int k=PipeGrid.Rows.Add(g.floorId,g.areaId,g.circuitId,g.conduitCode,g.assemblyCode,BdgklLook034.M(g.horizontalConduitM),BdgklLook034.M(g.verticalConduitM),BdgklLook034.M(g.conduitHiddenM),
     BdgklLook034.M(g.conduitExposedM),BdgklLook034.R(g.factor),BdgklLook034.M(g.conduitHiddenAfterFactorM),BdgklLook034.M(g.conduitExposedAfterFactorM),st);PipeGrid.Rows[k].Tag=g;
    if(g.status!=Bdgkl034.Ok){WireGrid.Rows[i].DefaultCellStyle.BackColor=BdgklLook034.ExcludedBack;PipeGrid.Rows[k].DefaultCellStyle.BackColor=BdgklLook034.ExcludedBack;}}
   var factors=scan.rows.Where(r=>r.Countable&&r.result!=null&&r.result.factor.HasValue).Select(r=>r.result.factor.Value).Distinct().ToList();
   ConfigLabel.Text="Đơn vị: m · Hệ số uốn cong (khai báo): "+(factors.Count==0?"—":factors.Count==1?BdgklLook034.R(factors[0]):"khác nhau theo tuyến (xem từng nhóm)")+
    " · Hiển thị 1 chữ số thập phân, cộng từ số gốc · INSUNITS = "+scan.insunits.ToString(CultureInfo.InvariantCulture);
   SummaryText=scan.Summary()+(hidden>0?" Bộ lọc đang ẩn "+hidden+" dòng.":"")+" Quét lúc "+(ScannedAt??"?")+".";
   SummaryLabel.Text=SummaryText;tips.SetToolTip(SummaryLabel,SummaryText);SummaryLabel.ForeColor=scan.Excluded>0?BdgklLook034.Error:BdgklLook034.Teal;
   if(StatusBox.Text.Length==0)SetStatus(scan.rows.Count==0?"Chưa có tuyến BDGKL. Bấm 'Thêm tuyến': chọn tuyến, điểm đầu, điểm cuối rồi khai báo.":
    "Chọn một dòng để xem chi tiết; 'Sửa tuyến' mở lại khai báo của dòng đó.",false);
   Detail();}
  public BdgklRow034 SelectedRow(){return RouteGrid.SelectedRows.Count==1?RouteGrid.SelectedRows[0].Tag as BdgklRow034:null;}
  // QA helper: selects the TUYẾN row of this route handle; false = not shown.
  public bool SelectRoute(string handle){foreach(DataGridViewRow r in RouteGrid.Rows){var x=r.Tag as BdgklRow034;
    if(x!=null&&string.Equals(x.routeHandle,handle,StringComparison.Ordinal)){Tabs.SelectedTab=RouteTab;RouteGrid.ClearSelection();r.Selected=true;return true;}}return false;}
  void Detail(){var sb=new StringBuilder();BdgklRow034 r=Tabs.SelectedTab==RouteTab?SelectedRow():null;
   EditBtn.Enabled=r!=null&&r.record!=null;
   if(r!=null){var rec=r.record;var x=r.result;
    sb.Append("Tuyến ").Append(r.routeHandle).Append(r.routeGuid!=null?" · mã tuyến "+r.routeGuid:"").Append(" · ").Append(BdgklLook034.Status(r.status)).Append("\r\n");
    if(!string.IsNullOrEmpty(r.reason))sb.Append("Lý do: ").Append(r.reason).Append("\r\n");
    if(!string.IsNullOrEmpty(r.warning))sb.Append("Cảnh báo: ").Append(r.warning).Append("\r\n");
    if(rec!=null){var c=rec.config;
     sb.Append("Điểm đầu: ").Append(rec.startLabel).Append(" (block ").Append(rec.startBlock).Append(", handle ").Append(rec.startHandle).Append(") · Điểm cuối: ").Append(rec.endLabel)
      .Append(" (block ").Append(rec.endBlock).Append(", handle ").Append(rec.endHandle).Append(")\r\n");
     sb.Append("Chiều dài hiện tại (đo lại mỗi lần quét): ").Append(r.liveLength.HasValue?Route034.Units(r.liveLength.Value)+" đơn vị bản vẽ":"không đo được").Append(" · đơn vị khai báo ").Append(c.declaredUnit).Append("\r\n");
     sb.Append("Tầng ").Append(c.floorId).Append(" · Phòng/vùng ").Append(c.areaId).Append(" · Lộ ").Append(c.circuitId).Append(" · Bộ dây ").Append(c.assemblyCode).Append(" · Ống ").Append(c.conduitCode)
      .Append(" · ").Append(c.noSharedConduit?"Không dùng chung ống (U = V = 0)":"trừ trùng ống ngoài phạm vi pilot").Append("\r\n");}
    if(x!=null){sb.Append("Phép tính:\r\n").Append(x.equation.Replace("\n","\r\n")).Append("\r\n");
     sb.Append("Giá trị chi tiết (15 chữ số có nghĩa; MEPS tính và cộng bằng số đầy đủ): F = ").Append(BdgklLook034.R(x.horizontalM)).Append(" · J = ").Append(BdgklLook034.R(x.verticalM)).Append(" · L = ").Append(BdgklLook034.R(x.tailsM))
      .Append(" · M = ").Append(BdgklLook034.R(x.routeM)).Append(" · M sau hệ số = ").Append(BdgklLook034.R(x.routeAfterFactorM)).Append(" · chìm = ").Append(BdgklLook034.R(x.conduitHiddenM))
      .Append(" · nổi = ").Append(BdgklLook034.R(x.conduitExposedM)).Append(" (m)");
     if(!r.Countable)sb.Append("\r\nDòng này KHÔNG được cộng vào tổng; số trên chỉ để đối chiếu.");}}
   else{DataGridView g=Tabs.SelectedTab==WireTab?WireGrid:Tabs.SelectedTab==PipeTab?PipeGrid:null;
    var grp=g!=null&&g.SelectedRows.Count==1?g.SelectedRows[0].Tag as BdgklGroup034:null;
    if(grp!=null){sb.Append("Nhóm ").Append(grp.floorId).Append(" / ").Append(grp.areaId).Append(" / ").Append(grp.circuitId).Append(" / ").Append(grp.assemblyCode).Append(" / ").Append(grp.conduitCode)
      .Append(": ").Append(grp.countedRows).Append(" tuyến được tính, ").Append(grp.excludedRows).Append(" dòng không tính.\r\n");
     foreach(var i in grp.issues)sb.Append("- ").Append(i.message).Append("\r\n");
     sb.Append("Giá trị chi tiết: M thô = ").Append(BdgklLook034.R(grp.routeM)).Append(" · hệ số ").Append(BdgklLook034.R(grp.factor)).Append(" · M sau hệ số = ").Append(BdgklLook034.R(grp.routeAfterFactorM))
      .Append(" · chìm thô = ").Append(BdgklLook034.R(grp.conduitHiddenM)).Append(" · nổi thô = ").Append(BdgklLook034.R(grp.conduitExposedM)).Append(" (m). Hệ số áp một lần cho tổng nhóm.");}
    else sb.Append("M là mét tuyến cho MỘT bộ dây (không nhân số sợi). Ống: chìm/nổi theo cách phân loại của mẫu BDGKL. Dòng không tính được đếm riêng, không cộng vào tổng.");}
   DetailBox.Text=sb.ToString();}
  void StartEdit(){var r=SelectedRow();if(r==null||r.record==null){SetStatus("Chọn một dòng TUYẾN có bản ghi đọc được trước khi 'Sửa tuyến'.",true);return;}
   Chosen=Choice.Edit;EditRouteId=r.routeId;DialogResult=DialogResult.OK;}
  public void DoRefresh(){try{if(Rescan==null)throw new InvalidOperationException("Chưa nối với AutoCAD.");StatusBox.Text="";ShowScan(Rescan(),RouteUi034.Now());SetStatus("Đã cập nhật lúc "+ScannedAt+" (đọc lại hình và block).",false);}
   catch(Exception ex){SetStatus("Chưa cập nhật được: "+ex.Message,true);}}
  protected override void Dispose(bool disposing){base.Dispose(disposing);if(disposing){face.Dispose();bold.Dispose();tips.Dispose();}}
 }
 // Declaration dialog of ONE route on its exact snapshot. Result/UnitConfirmed after Lưu; DialogResult.Retry = user asked to pick the endpoints again
 // (Draft keeps every typed text). Every value is validated by the core (Bdgkl034.Calculate via BdgklCad034.Preview); nothing is defaulted or corrected.
 public sealed class BdgklDialog034:Form {
  public readonly TextBox InfoBox=new TextBox(),FloorBox=new TextBox(),AreaBox=new TextBox(),CircuitBox=new TextBox(),AssemblyBox=new TextBox(),ConduitBox=new TextBox(),
   CeilingBox=new TextBox(),StartValueBox=new TextBox(),EndValueBox=new TextBox(),TailCountBox=new TextBox(),TailEachBox=new TextBox(),FactorBox=new TextBox(),PreviewBox=new TextBox(),StatusBox=new TextBox();
  public readonly ComboBox UnitBox=new ComboBox(),ModeBox=new ComboBox(),StartModeBox=new ComboBox(),EndModeBox=new ComboBox();
  public readonly CheckBox NoSharedBox=new CheckBox(),UnitAckBox=new CheckBox();
  public readonly Button SaveBtn=new Button(),CancelBtn=new Button(),RepickBtn=new Button();
  public readonly BdgklSnapshot034 Selection;
  public BdgklConfig034 Result;public bool UnitConfirmed;public Dictionary<string,string> Draft;public BdgklResult034 LastResult;
  readonly float dpi;readonly Font face;bool loading;string lastWarn;
  int S(int px){return Math.Max(1,(int)Math.Round(px*dpi));}
  static string Exact(double? v){return v.HasValue?Bdgkl034.N(v.Value).Replace('.',','):"";}
  public BdgklDialog034(BdgklSnapshot034 snap,IDictionary<string,string> draft,float dpi){if(snap==null)throw new ArgumentNullException("snap");Selection=snap;this.dpi=BdgklLook034.Dpi(dpi);
   Text="MEPS — Khai báo tuyến BDGKL";Name="bdgkl034_dialog";FormBorderStyle=FormBorderStyle.Sizable;MinimizeBox=false;MaximizeBox=false;ShowInTaskbar=false;StartPosition=FormStartPosition.CenterParent;
   AutoScaleMode=AutoScaleMode.None;face=new Font("Segoe UI",12f*this.dpi,FontStyle.Regular,GraphicsUnit.Pixel);Font=face;BackColor=BdgklLook034.Back;ForeColor=BdgklLook034.Ink;
   ClientSize=new Size(S(600),S(660));MinimumSize=new Size(S(520),S(520));
   var pane=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=1,RowCount=5,Padding=new Padding(S(8)),BackColor=BdgklLook034.Back};pane.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
   foreach(int h in new[]{S(92),-55,-45,S(44),S(40)})pane.RowStyles.Add(h<0?new RowStyle(SizeType.Percent,-h):new RowStyle(SizeType.Absolute,h));
   foreach(var b in new[]{InfoBox,PreviewBox,StatusBox})BdgklLook034.ReadOnlyBox(b);
   InfoBox.Name="bdgkl034_info";InfoBox.AccessibleName="Tuyến và hai điểm đã chọn (chỉ đọc)";PreviewBox.Name="bdgkl034_preview";PreviewBox.AccessibleName="Phép tính trên tuyến thật";
   StatusBox.Name="bdgkl034_dialog_status";StatusBox.AccessibleName="Thông báo";StatusBox.BackColor=BdgklLook034.Back;StatusBox.BorderStyle=BorderStyle.None;
   // Fields scroll inside their own panel (the form never grows taller than a normal screen).
   var scroll=new Panel{Dock=DockStyle.Fill,AutoScroll=true,BackColor=BdgklLook034.Back,Margin=new Padding(0,S(4),0,S(4))};
   var grid=new TableLayoutPanel{ColumnCount=2,AutoSize=true,AutoSizeMode=System.Windows.Forms.AutoSizeMode.GrowAndShrink,Dock=DockStyle.Top,BackColor=BdgklLook034.Back};
   grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,S(230)));grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
   UnitBox.Items.Add(new RouteDialog034.Choice(null,"(chọn đơn vị)"));UnitBox.Items.Add(new RouteDialog034.Choice(Bdgkl034.UnitMm,"MM — 1000 đơn vị = 1 m"));UnitBox.Items.Add(new RouteDialog034.Choice(Bdgkl034.UnitM,"M — 1 đơn vị = 1 m"));
   ModeBox.Items.Add(new RouteDialog034.Choice(null,"(chọn hướng đi)"));ModeBox.Items.Add(new RouteDialog034.Choice(Bdgkl034.Ceiling,"TRẦN"));ModeBox.Items.Add(new RouteDialog034.Choice(Bdgkl034.Floor,"SÀN"));
   ModeBox.Items.Add(new RouteDialog034.Choice(Bdgkl034.Wall,"TƯỜNG"));
   foreach(var m in new[]{StartModeBox,EndModeBox}){m.Items.Add(new RouteDialog034.Choice(null,"(chọn cách khai báo)"));m.Items.Add(new RouteDialog034.Choice(Bdgkl034.Fixed,"Cố định (cao độ từ sàn hoàn thiện)"));
    m.Items.Add(new RouteDialog034.Choice(Bdgkl034.CeilingOffset,"Hạ dưới trần (khoảng hạ)"));}
   var rows=new[]{
    Tuple.Create("floor","Tầng",(Control)FloorBox),Tuple.Create("area","Phòng/vùng",(Control)AreaBox),Tuple.Create("circuit","Lộ",(Control)CircuitBox),
    Tuple.Create("unit","Đơn vị bản vẽ khai báo",(Control)UnitBox),Tuple.Create("assembly","Bộ dây (mã nhập tay, ví dụ 2x1.5)",(Control)AssemblyBox),
    Tuple.Create("conduit","Ống (mã nhập tay, ví dụ D20)",(Control)ConduitBox),Tuple.Create("mode","Hướng đi",(Control)ModeBox),Tuple.Create("ceiling","H sàn → trần (m)",(Control)CeilingBox),
    Tuple.Create("start_mode","Điểm đầu: cách khai báo cao độ",(Control)StartModeBox),Tuple.Create("start_value","Điểm đầu: cao độ / khoảng hạ (m)",(Control)StartValueBox),
    Tuple.Create("end_mode","Điểm cuối: cách khai báo cao độ",(Control)EndModeBox),Tuple.Create("end_value","Điểm cuối: cao độ / khoảng hạ (m)",(Control)EndValueBox),
    Tuple.Create("tail_count","Số đầu chờ",(Control)TailCountBox),Tuple.Create("tail_each","Dài một đầu chờ (m)",(Control)TailEachBox),Tuple.Create("factor","Hệ số uốn cong (≥ 1)",(Control)FactorBox)};
   for(int i=0;i<rows.Length;i++){var c=rows[i].Item3;grid.RowStyles.Add(new RowStyle(SizeType.Absolute,S(30)));
    grid.Controls.Add(new Label{Text=rows[i].Item2,Name="bdgkl034_caption_"+rows[i].Item1,Dock=DockStyle.Fill,AutoEllipsis=true,ForeColor=BdgklLook034.Muted,TextAlign=ContentAlignment.MiddleLeft,Margin=new Padding(0)},0,i);
    c.Name="bdgkl034_"+rows[i].Item1;c.AccessibleName=rows[i].Item2;c.Dock=DockStyle.Fill;c.Margin=new Padding(0,S(3),0,S(3));c.TabIndex=i;grid.Controls.Add(c,1,i);
    var t=c as TextBox;if(t!=null){t.BorderStyle=BorderStyle.FixedSingle;t.MaxLength=Bdgkl034.MaxText;t.TextChanged+=(s,e)=>Changed();}
    var cb=c as ComboBox;if(cb!=null){cb.DropDownStyle=ComboBoxStyle.DropDownList;cb.SelectedIndexChanged+=(s,e)=>Changed();}}
   NoSharedBox.Text="Không dùng chung ống (pilot chưa hỗ trợ trừ trùng ống; tích để xác nhận U = V = 0)";NoSharedBox.Name="bdgkl034_no_shared";NoSharedBox.AccessibleName="Không dùng chung ống";
   UnitAckBox.Name="bdgkl034_unit_ack";UnitAckBox.AccessibleName="Xác nhận đơn vị khác INSUNITS";UnitAckBox.ForeColor=BdgklLook034.Error;
   int at=rows.Length;foreach(var ck in new[]{NoSharedBox,UnitAckBox}){grid.RowStyles.Add(new RowStyle(SizeType.Absolute,S(44)));ck.Dock=DockStyle.Fill;ck.AutoSize=false;ck.Margin=new Padding(0,S(2),0,S(2));
    ck.TabIndex=at;ck.CheckedChanged+=(s,e)=>Changed();grid.Controls.Add(ck,0,at);grid.SetColumnSpan(ck,2);at++;}
   grid.RowCount=at;scroll.Controls.Add(grid);
   SaveBtn.Text="Lưu";SaveBtn.Name="bdgkl034_save";CancelBtn.Text="Hủy";CancelBtn.Name="bdgkl034_cancel";CancelBtn.DialogResult=DialogResult.Cancel;
   RepickBtn.Text="Chọn lại điểm đầu/cuối…";RepickBtn.Name="bdgkl034_repick";
   var bar=new FlowLayoutPanel{FlowDirection=System.Windows.Forms.FlowDirection.RightToLeft,Dock=DockStyle.Fill,WrapContents=false,Margin=new Padding(0),BackColor=BdgklLook034.Back};
   foreach(var b in new[]{CancelBtn,SaveBtn,RepickBtn}){BdgklLook034.Style(b,b==SaveBtn,face,S(72),S(30));b.Margin=new Padding(S(4),S(4),0,0);b.AccessibleName=b.Text;bar.Controls.Add(b);}
   pane.Controls.Add(InfoBox,0,0);pane.Controls.Add(scroll,0,1);pane.Controls.Add(PreviewBox,0,2);pane.Controls.Add(StatusBox,0,3);pane.Controls.Add(bar,0,4);Controls.Add(pane);
   CancelButton=CancelBtn;SaveBtn.Click+=(s,e)=>TryAccept();RepickBtn.Click+=(s,e)=>{Draft=CaptureDraft();DialogResult=DialogResult.Retry;};
   // Read-only facts: actual route, both picked blocks, INSUNITS as a hint, legacy route 034 data (display only, never converted).
   var it=snap.route;var info=new StringBuilder();
   info.Append("Tuyến ").Append(it.type).Append(' ').Append(it.handle).Append(" · layer ").Append(it.layer).Append(" · chiều dài hiện tại ").Append(Route034.Units(it.length))
    .Append(" đơn vị bản vẽ · INSUNITS = ").Append(snap.insunits.ToString(CultureInfo.InvariantCulture)).Append(" (chỉ tham khảo; bạn tự chọn đơn vị)\r\n");
   info.Append("Điểm đầu: ").Append(snap.start.label).Append(" (block ").Append(snap.start.blockName).Append(", handle ").Append(snap.start.handle).Append(") → Điểm cuối: ").Append(snap.end.label)
    .Append(" (block ").Append(snap.end.blockName).Append(", handle ").Append(snap.end.handle).Append(")\r\n");
   if(it.existing!=null)info.Append("Tuyến 034 cũ (chỉ hiển thị, không chuyển đổi): khu vực ").Append(it.existing.config.area).Append(", mạch ").Append(it.existing.config.circuit)
    .Append(", mã cáp ").Append(it.existing.config.code).Append(", ").Append(Route034.BasisCaption(it.existing.config.basis)).Append(".\r\n");
   info.Append(snap.existingError!=null?"Bản ghi BDGKL đang có KHÔNG dùng được: "+snap.existingError:snap.existing==null?"Chưa có bản ghi BDGKL: nhập đủ các ô rồi Lưu.":
    snap.existingStatus==BdgklCad034.Reconfirm?"CẦN XÁC NHẬN LẠI: "+snap.existingReason+". Kiểm tra các giá trị đang hiện rồi Lưu.":"Đã có bản ghi BDGKL hợp lệ: đang hiện giá trị đã lưu.");
   InfoBox.Text=info.ToString();
   loading=true;
   try{var had=snap.existingError==null&&snap.existing!=null?snap.existing.config:null;
    if(had!=null){FloorBox.Text=had.floorId;AreaBox.Text=had.areaId;CircuitBox.Text=had.circuitId;AssemblyBox.Text=had.assemblyCode;ConduitBox.Text=had.conduitCode;
     CeilingBox.Text=Exact(had.ceilingHeight);StartValueBox.Text=Exact(had.startHeightMode==Bdgkl034.CeilingOffset?had.startCeilingOffset:had.startFixedHeight);
     EndValueBox.Text=Exact(had.endHeightMode==Bdgkl034.CeilingOffset?had.endCeilingOffset:had.endFixedHeight);
     TailCountBox.Text=had.tailCount.HasValue?had.tailCount.Value.ToString(CultureInfo.InvariantCulture):"";TailEachBox.Text=Exact(had.tailEachLength);FactorBox.Text=Exact(had.factor);
     Choose(UnitBox,had.declaredUnit);Choose(ModeBox,had.routingMode);Choose(StartModeBox,had.startHeightMode);Choose(EndModeBox,had.endHeightMode);NoSharedBox.Checked=had.noSharedConduit;}
    else foreach(var cb in new[]{UnitBox,ModeBox,StartModeBox,EndModeBox})cb.SelectedIndex=0;
    if(draft!=null)Restore(draft);}
   finally{loading=false;}
   if(snap.existingError!=null){foreach(Control c in grid.Controls)c.Enabled=false;}
   ActiveControl=FloorBox;Changed();}
  static string Value(ComboBox b){var c=b.SelectedItem as RouteDialog034.Choice;return c==null?null:c.value;}
  // Selects the entry with this stored value (null = placeholder). QA helper; same SelectedIndexChanged path as the mouse.
  public void Choose(ComboBox b,string value){for(int i=0;i<b.Items.Count;i++)if(string.Equals(((RouteDialog034.Choice)b.Items[i]).value,value,StringComparison.Ordinal)){b.SelectedIndex=i;return;}b.SelectedIndex=0;}
  // QA helper: sets the text of a field by its control name (e.g. "bdgkl034_ceiling"); false = no such text field. Same TextChanged path as typing.
  public bool SetField(string name,string text){foreach(var t in TextFields())if(t.Name==name){t.Text=text;return true;}return false;}
  IEnumerable<TextBox> TextFields(){return new[]{FloorBox,AreaBox,CircuitBox,AssemblyBox,ConduitBox,CeilingBox,StartValueBox,EndValueBox,TailCountBox,TailEachBox,FactorBox};}
  public Dictionary<string,string> CaptureDraft(){var d=new Dictionary<string,string>(StringComparer.Ordinal);foreach(var t in TextFields())d[t.Name]=t.Text;
   foreach(var c in new[]{UnitBox,ModeBox,StartModeBox,EndModeBox})d[c.Name]=Value(c);d[NoSharedBox.Name]=NoSharedBox.Checked?"1":null;return d;}
  void Restore(IDictionary<string,string> d){string v;foreach(var t in TextFields())if(d.TryGetValue(t.Name,out v))t.Text=v??"";
   foreach(var c in new[]{UnitBox,ModeBox,StartModeBox,EndModeBox})if(d.TryGetValue(c.Name,out v))Choose(c,v);
   if(d.TryGetValue(NoSharedBox.Name,out v))NoSharedBox.Checked=v=="1";}
  void Status(string text,bool error){StatusBox.ForeColor=error?BdgklLook034.Error:BdgklLook034.Ink;StatusBox.Text=text??"";}
  static string Text100(string s){return string.IsNullOrEmpty(s)?null:s;}
  // Current input → declared config (exception = a number that cannot be read; empty = null and the core then reports it as missing).
  public BdgklConfig034 Build(){
   var c=new BdgklConfig034{floorId=Text100(FloorBox.Text),areaId=Text100(AreaBox.Text),circuitId=Text100(CircuitBox.Text),declaredUnit=Value(UnitBox),assemblyCode=Text100(AssemblyBox.Text),
    conduitCode=Text100(ConduitBox.Text),routingMode=Value(ModeBox),ceilingHeight=BdgklUi034.ParseDecimal(CeilingBox.Text,"H sàn → trần"),startHeightMode=Value(StartModeBox),
    endHeightMode=Value(EndModeBox),tailCount=BdgklUi034.ParseCount(TailCountBox.Text,"Số đầu chờ"),tailEachLength=BdgklUi034.ParseDecimal(TailEachBox.Text,"Dài một đầu chờ"),
    factor=BdgklUi034.ParseDecimal(FactorBox.Text,"Hệ số uốn cong"),noSharedConduit=NoSharedBox.Checked};
   double? sv=BdgklUi034.ParseDecimal(StartValueBox.Text,"Cao độ / khoảng hạ điểm đầu"),ev=BdgklUi034.ParseDecimal(EndValueBox.Text,"Cao độ / khoảng hạ điểm cuối");
   if(c.startHeightMode==Bdgkl034.Fixed)c.startFixedHeight=sv;else if(c.startHeightMode==Bdgkl034.CeilingOffset)c.startCeilingOffset=sv;
   if(c.endHeightMode==Bdgkl034.Fixed)c.endFixedHeight=ev;else if(c.endHeightMode==Bdgkl034.CeilingOffset)c.endCeilingOffset=ev;
   return c;}
  public string UnitIssue(){return Selection.UnitWarning(Value(UnitBox));}
  void Changed(){if(loading)return;string warn=UnitIssue();
   if(!string.Equals(warn,lastWarn,StringComparison.Ordinal)){lastWarn=warn;if(UnitAckBox.Checked)UnitAckBox.Checked=false;}
   UnitAckBox.Visible=warn!=null;UnitAckBox.Text=warn==null?"":"Xác nhận dùng đơn vị "+Value(UnitBox)+" dù INSUNITS = "+Selection.insunits.ToString(CultureInfo.InvariantCulture)+"; MEPS không đổi INSUNITS.";
   if(Selection.existingError!=null){SaveBtn.Enabled=false;LastResult=null;PreviewBox.Text="";Status("Không lưu được: bản ghi BDGKL đang có không dùng được; MEPS không ghi đè.",true);return;}
   try{var c=Build();var r=BdgklCad034.Preview(Selection,c);LastResult=r;
    var sb=new StringBuilder(r.equation.Replace("\n","\r\n"));
    if(r.status!=Bdgkl034.Ok){sb.Insert(0,"Chưa lưu được ("+BdgklLook034.Status(r.status)+"):\r\n"+string.Join("\r\n",r.issues.Select(i=>"- "+i.message))+"\r\n\r\n");}
    else sb.Insert(0,"Tổng tuyến M = "+BdgklLook034.M(r.routeM)+" m (chi tiết "+BdgklLook034.R(r.routeM)+"); sau hệ số "+BdgklLook034.M(r.routeAfterFactorM)+" m (chi tiết "+BdgklLook034.R(r.routeAfterFactorM)+
     "). Ống chìm "+BdgklLook034.M(r.conduitHiddenM)+" / nổi "+BdgklLook034.M(r.conduitExposedM)+" m (chưa hệ số).\r\n\r\n");
    if(warn!=null)sb.Append("\r\n").Append(warn);
    PreviewBox.Text=sb.ToString();
    bool ok=r.status==Bdgkl034.Ok&&(warn==null||UnitAckBox.Checked);SaveBtn.Enabled=ok;
    Status(ok?"Sẵn sàng lưu. Màn hình hiển thị 1 chữ số; MEPS lưu đúng giá trị đã nhập và luôn đo lại chiều dài tuyến.":
     r.status!=Bdgkl034.Ok?(c.noSharedConduit?"Nhập đủ và đúng các ô còn thiếu/sai (xem danh sách bên trên).":"Nhập đủ các ô và tích 'Không dùng chung ống' (pilot chưa hỗ trợ trừ trùng ống)."):
     "Đơn vị khai báo khác INSUNITS: đọc cảnh báo và tích ô xác nhận để Lưu.",!ok);}
   catch(InvalidOperationException ex){SaveBtn.Enabled=false;LastResult=null;PreviewBox.Text="Chiều dài hiện tại: "+Route034.Units(Selection.Length)+" đơn vị bản vẽ. Chưa tính (có ô số không đọc được).";Status(ex.Message,true);}}
  // Validates with the core rules; false = the dialog stays open with the reason; nothing written.
  public bool TryAccept(){try{if(Selection.existingError!=null)throw new InvalidOperationException("Bản ghi BDGKL đang có không dùng được; MEPS không ghi đè.");
    var c=Build();var r=BdgklCad034.Preview(Selection,c);
    if(r.status!=Bdgkl034.Ok)throw new InvalidOperationException("Chưa lưu ("+BdgklLook034.Status(r.status)+"): "+string.Join("; ",r.issues.Take(4).Select(i=>i.message)));
    var w=UnitIssue();if(w!=null&&!UnitAckBox.Checked)throw new InvalidOperationException("Chưa lưu: "+w+" Tích ô xác nhận đơn vị trước khi Lưu.");
    Result=c;UnitConfirmed=w!=null&&UnitAckBox.Checked;}
   catch(Exception ex){Result=null;UnitConfirmed=false;Status(ex.Message,true);return false;}
   DialogResult=DialogResult.OK;return true;}
  protected override void Dispose(bool disposing){base.Dispose(disposing);if(disposing)face.Dispose();}
 }
 // Runs INSIDE the bound command (via RouteUi034.Run → Plugin.RunRoute). guard = write context (same document, Model, UNDO on) around every pick/dialog and
 // right before Commit; readGuard = same document + Model for the read-only scans.
 public static class BdgklUi034 {
  // TVXD-MEPS-ROOMQTY-037: user-facing label of the menu entry, report title and read-only outcome; Action stays "bdgkl034-table".
  public const string Title="Bảng khối lượng điện",Caption=Title+"…";
  public const string Action="bdgkl034-table",PickRoute="route",PickStart="start",PickEnd="end";
  public const int MaxRounds=50;
  // QA hooks (synchronous, no keyboard): Pick(doc, kind) returns the picked ObjectId or null (Esc); ShowReport/ShowDialog display the forms and return
  // their results. ResetHooks restores the defaults.
  public static Func<Document,string,ObjectId?> Pick=DefaultPick;
  public static Func<BdgklReport034,DialogResult> ShowReport=DefaultShowReport;
  public static Func<BdgklDialog034,DialogResult> ShowDialog=DefaultShowDialog;
  public static DialogResult DefaultShowReport(BdgklReport034 f){return App.ShowModalDialog(f);}
  public static DialogResult DefaultShowDialog(BdgklDialog034 f){return App.ShowModalDialog(f);}
  public static void ResetHooks(){Pick=DefaultPick;ShowReport=DefaultShowReport;ShowDialog=DefaultShowDialog;}
  public static ObjectId? DefaultPick(Document d,string kind){PromptEntityOptions o;
   if(kind==PickRoute){o=new PromptEntityOptions("\nBDGKL: chọn MỘT tuyến LINE/LWPOLYLINE hở trên Model (Esc huỷ): "){AllowObjectOnLockedLayer=true};
    o.SetRejectMessage("\nChỉ chọn LINE hoặc LWPOLYLINE.");o.AddAllowedClass(typeof(CadLine),true);o.AddAllowedClass(typeof(CadPolyline),true);}
   else{o=new PromptEntityOptions(kind==PickStart?"\nBDGKL: chọn block thiết bị ĐIỂM ĐẦU trên Model (Esc huỷ): ":"\nBDGKL: chọn block thiết bị ĐIỂM CUỐI trên Model (Esc huỷ): "){AllowObjectOnLockedLayer=true};
    o.SetRejectMessage("\nChỉ chọn block (BlockReference).");o.AddAllowedClass(typeof(CadBlockRef),false);}
   var r=d.Editor.GetEntity(o);return r.Status==PromptStatus.OK?(ObjectId?)r.ObjectId:null;}
  static readonly CultureInfo Inv=CultureInfo.InvariantCulture;
  static readonly Regex DecRx=new Regex(@"\A[0-9]+([.,][0-9]+)?\z"),IntRx=new Regex(@"\A[0-9]+\z"),AmbiguousRx=new Regex(@"\A[1-9][0-9]{0,2}[.,][0-9]{3}\z");
  static string Short(string s){return s.Length>24?s.Substring(0,24)+"…":s;}
  // Typed decimal → value; empty/blank = null (the core reports it as missing; nothing is filled in). Digits with at most ONE decimal mark '.' or ','
  // (0.25 = 0,25); no sign, exponent, inner space or thousands separator. "1,500"/"2.250" (1–3 digits, mark, exactly 3 digits) are refused as ambiguous
  // thousands; write 1,5 or 1500. The value is stored invariant and never rounded.
  public static double? ParseDecimal(string s,string what){s=(s??"").Trim();if(s.Length==0)return null;
   if(!DecRx.IsMatch(s))throw new InvalidOperationException(what+" \""+Short(s)+"\" không hợp lệ: chỉ chữ số và tối đa một dấu thập phân (, hoặc .); không dấu nghìn, dấu âm, số mũ.");
   if(AmbiguousRx.IsMatch(s))throw new InvalidOperationException(what+" \""+Short(s)+"\" dễ nhầm với dấu nghìn; nhập dạng không nhầm lẫn (ví dụ 1,5 hoặc 1500).");
   double v;if(!double.TryParse(s.Replace(',','.'),NumberStyles.AllowDecimalPoint,Inv,out v)||double.IsInfinity(v))throw new InvalidOperationException(what+" \""+Short(s)+"\" vượt giới hạn số.");
   return v;}
  public static int? ParseCount(string s,string what){s=(s??"").Trim();if(s.Length==0)return null;
   if(!IntRx.IsMatch(s))throw new InvalidOperationException(what+" \""+Short(s)+"\" phải là số nguyên ≥ 0 (chỉ chữ số).");
   int v;if(!int.TryParse(s,NumberStyles.None,Inv,out v))throw new InvalidOperationException(what+" \""+Short(s)+"\" quá lớn.");return v;}
  static RouteOutcome034 Cancel(string m){return new RouteOutcome034{cancelled=true,message=m};}
  // Report → (Thêm/Sửa → picks → dialog → Apply) → report again, until the user closes it. Refusals (IOE) are shown in the reopened report, nothing written.
  public static RouteOutcome034 Run(Document d,Action<string> guard,Action<string> readGuard,float dpi){var db=d.Database;bool changed=false,warn=false,noteWarn=false;string note=null,state="";
   var saved=new List<string>();
   try{
    for(int round=0;round<MaxRounds;round++){
     readGuard("trước khi quét bảng BDGKL");var scan=BdgklCad034.ScanModel(db);BdgklReport034.Choice choice;ObjectId? edit;
     using(var dlg=new BdgklReport034(dpi)){dlg.Rescan=()=>{readGuard("trước khi cập nhật bảng BDGKL");return BdgklCad034.ScanModel(db);};
      if(note!=null)dlg.SetStatus(note,noteWarn);dlg.ShowScan(scan,RouteUi034.Now());ShowReport(dlg);readGuard("sau bảng BDGKL");choice=dlg.Chosen;edit=dlg.EditRouteId;state=dlg.SummaryText;}
     if(choice==BdgklReport034.Choice.None)break;
     RouteOutcome034 o;
     try{o=choice==BdgklReport034.Choice.Add?AddRoute(d,guard,dpi):EditRoute(d,edit.Value,guard,dpi);}
     catch(InvalidOperationException ex){o=new RouteOutcome034{cancelled=true,warn=true,message="Chưa lưu: "+ex.Message};}
     note=o.message;noteWarn=o.warn;if(o.changed){changed=true;saved.Add(o.message);if(o.warn)warn=true;}}}
   catch(Exception ex){if(!changed)throw;
    return new RouteOutcome034{changed=true,warn=true,message=string.Join(" ",saved)+" Sau đó dừng: "+ex.Message}; }
   if(!changed)return new RouteOutcome034{cancelled=true,message=Title+" (chỉ đọc): "+state+" Bản vẽ không đổi."};
   return new RouteOutcome034{changed=true,warn=warn,message=string.Join(" ",saved)+(saved.Count>1?" (cùng một lệnh: U hoàn tác mọi lần lưu của lần mở bảng này).":"")};}
  static RouteOutcome034 AddRoute(Document d,Action<string> guard,float dpi){
   guard("trước khi chọn tuyến");var r=Pick(d,PickRoute);guard("sau khi chọn tuyến");
   if(!r.HasValue)return Cancel("Đã huỷ thêm tuyến (Esc); bản vẽ không đổi.");
   return PickEnds(d,r.Value,null,guard,dpi);}
  static RouteOutcome034 PickEnds(Document d,ObjectId route,IDictionary<string,string> draft,Action<string> guard,float dpi){
   guard("trước khi chọn điểm đầu");var s=Pick(d,PickStart);guard("sau khi chọn điểm đầu");if(!s.HasValue)return Cancel("Đã huỷ chọn điểm đầu (Esc); bản vẽ không đổi.");
   var e=Pick(d,PickEnd);guard("sau khi chọn điểm cuối");if(!e.HasValue)return Cancel("Đã huỷ chọn điểm cuối (Esc); bản vẽ không đổi.");
   return Declare(d,BdgklCad034.Snapshot(d.Database,route,s.Value,e.Value),draft,guard,dpi);}
  // Sửa tuyến: saved endpoints and saved declaration. A saved endpoint that is gone asks for an explicit new pick; an unreadable record is never edited.
  static RouteOutcome034 EditRoute(Document d,ObjectId route,Action<string> guard,float dpi){
   var row=BdgklCad034.Read(d.Database,route);
   if(row==null||row.record==null)throw new InvalidOperationException("Tuyến "+(row==null?"":row.routeHandle)+" không có bản ghi BDGKL đọc được ("+(row==null?"không có":row.reason)+"); MEPS không ghi đè.");
   BdgklSnapshot034 snap;
   try{snap=BdgklCad034.SnapshotSaved(d.Database,route);}
   catch(InvalidOperationException ex){try{d.Editor.WriteMessage("\nBDGKL: "+ex.Message);}catch(Exception){}return PickEnds(d,route,null,guard,dpi);}
   return Declare(d,snap,null,guard,dpi);}
  static RouteOutcome034 Declare(Document d,BdgklSnapshot034 snap,IDictionary<string,string> draft,Action<string> guard,float dpi){
   if(snap.existingError!=null)throw new InvalidOperationException("Tuyến "+snap.route.handle+" đã có bản ghi BDGKL không dùng được ("+snap.existingError+"); MEPS không ghi đè.");
   for(int i=0;i<MaxRounds;i++){DialogResult r;BdgklConfig034 cfg;bool ack;Dictionary<string,string> next;
    using(var dlg=new BdgklDialog034(snap,draft,dpi)){r=ShowDialog(dlg);guard("sau hộp khai báo BDGKL");cfg=dlg.Result;ack=dlg.UnitConfirmed;next=dlg.Draft;}
    if(r==DialogResult.Retry){
     guard("trước khi chọn lại điểm đầu");var s=Pick(d,PickStart);guard("sau khi chọn lại điểm đầu");if(!s.HasValue)return Cancel("Đã huỷ chọn lại điểm đầu (Esc); bản vẽ không đổi.");
     var e=Pick(d,PickEnd);guard("sau khi chọn lại điểm cuối");if(!e.HasValue)return Cancel("Đã huỷ chọn lại điểm cuối (Esc); bản vẽ không đổi.");
     snap=BdgklCad034.Snapshot(d.Database,snap.routeId,s.Value,e.Value);draft=next;continue;}
    if(r!=DialogResult.OK||cfg==null)return Cancel("Đã huỷ khai báo BDGKL tuyến "+snap.route.handle+"; bản vẽ không đổi.");
    guard("ngay trước khi lưu BDGKL");var res=BdgklCad034.Apply(d.Database,snap,cfg,ack,guard);
    return new RouteOutcome034{changed=res.changed,warn=res.warn,message=res.message};}
   return Cancel("Dừng sau "+MaxRounds+" lần chọn lại; bản vẽ không đổi.");}
 }
}
#endif
