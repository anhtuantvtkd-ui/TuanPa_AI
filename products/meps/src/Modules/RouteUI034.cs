using System;using System.IO;using System.Drawing;using System.Linq;using System.Text;using System.Globalization;using System.Collections.Generic;using System.Windows.Forms;
#if ROUTE034 && !ROUTE034_PURE
using Autodesk.AutoCAD.EditorInput;using Document=Autodesk.AutoCAD.ApplicationServices.Document;using App=Autodesk.AutoCAD.ApplicationServices.Application;
using ObjectId=Autodesk.AutoCAD.DatabaseServices.ObjectId;using Database=Autodesk.AutoCAD.DatabaseServices.Database;
using CadLine=Autodesk.AutoCAD.DatabaseServices.Line;using CadPolyline=Autodesk.AutoCAD.DatabaseServices.Polyline;
namespace Tvxd.Meps {
 // TVXD-MEPS-WIREQTY-034 UI over the native core (RouteCad034). Two items at the top of the DÂY DẪN popup (WireView010) → CompactView06.Request → the
 // plugin's queued bound-document job TVXDMEPS060DO → RouteUi034.Run. No permanent palette row, no second queue.
 //  - Thiết lập tuyến…: Editor.GetEntity picks ONE Line / LWPOLYLINE in Model (Esc = nothing written) → RouteCad034.Snapshot (all-or-none, refuses invalid) →
 //    RouteDialog034 (owned compact dark dialog, temporary modal through Application.ShowModalDialog) → RouteCad034.Apply (one transaction = one U).
 //  - Bóc khối lượng…: RouteCad034.ScanModel (read-only) → RouteReport034 (modal, resizable): groups + details + excluded count, re-scan, CSV export
 //    (RouteCsv034, UTF-8 BOM, SaveFileDialog with the standard overwrite prompt), or "set up the selected row" (closes the report, then the setup above on
 //    that entity in the same command; nothing is written by the report itself). No CAD table is generated.
 // Nothing is inferred: no default cable type, basis, count, extra length, reserve or unit (INSUNITS is only shown as a hint); no UCS/INSUNITS change; no Save.
 public sealed class RouteDialog034:Form {
  public const string AreaCaption="Khu vực",CircuitCaption="Mạch/lộ",CableCaption="Loại cáp (danh mục 016)",BasisCaption="Cách tính",CountCaption="Số sợi / số cáp song song",
   ExtraCaption="Bổ sung cho đoạn này (m)",ReserveCaption="Dự phòng (%)",UnitCaption="Đơn vị bản vẽ khai báo";
  public const string ReconfirmNote="Sau khi lưu: COPY, MOVE, sửa hình (STRETCH/PEDIT/BREAK/JOIN/grip…) hoặc đổi INSUNITS → tuyến chuyển sang CẦN XÁC NHẬN LẠI và không được tính cho tới khi mở lại hộp này và Lưu.";
  internal static readonly Color UiBack=Color.FromArgb(0x17,0x21,0x2B),UiField=Color.FromArgb(0x20,0x2D,0x3A),UiLine=Color.FromArgb(0x35,0x46,0x55),UiFore=Color.FromArgb(0xE7,0xEE,0xF5),
   UiMuted=Color.FromArgb(0x8F,0xA0,0xB0),UiTeal=Color.FromArgb(0x46,0xC7,0xB7),UiTealHot=Color.FromArgb(0x6F,0xD8,0xCB),UiHover=Color.FromArgb(0x26,0x40,0x4A),ErrorColor=Color.FromArgb(0xFF,0x8C,0x6E);
  public readonly TextBox InfoBox=new TextBox(),AreaBox=new TextBox(),CircuitBox=new TextBox(),CountBox=new TextBox(),ExtraBox=new TextBox(),ReserveBox=new TextBox(),
   PreviewBox=new TextBox(),HelpBox=new TextBox(),StatusBox=new TextBox();
  public readonly ComboBox CableBox=new ComboBox(),BasisBox=new ComboBox(),UnitBox=new ComboBox();
  public readonly CheckBox AckBox=new CheckBox();
  public readonly Button SaveBtn=new Button(),CancelBtn=new Button();
  public readonly RouteSnapshot034 Selection;public readonly RouteItem034 Item;
  // Result after Lưu (validated with the core rules); null after Hủy/Esc.
  public RouteConfig034 Result;
  // Display entry of a combo box; value null = the "(chọn …)" placeholder (never a default).
  public sealed class Choice {public readonly string value,text;public Choice(string value,string text){this.value=value;this.text=text;}public override string ToString(){return text;}}
  readonly float dpi;readonly Font face;bool loading;string lastWarn;
  int S(int px){return Math.Max(1,(int)Math.Round(px*dpi));}
  static string Exact(double v){return RouteCsv034.Number(v).Replace('.',',');}
  static string UnitName(int insunits){return insunits==4?"mm":insunits==6?"m":insunits==0?"chưa đặt":"mã "+insunits.ToString(CultureInfo.InvariantCulture);}
  public RouteDialog034(RouteSnapshot034 sel,float dpi){if(sel==null||sel.items.Count!=1)throw new ArgumentException("Thiết lập tuyến cần đúng MỘT tuyến.","sel");Selection=sel;Item=sel.items[0];
   this.dpi=float.IsNaN(dpi)||dpi<=0?1f:Math.Max(1f,Math.Min(3f,dpi));
   Text="MEPS — Thiết lập tuyến dây/cáp";Name="route034";FormBorderStyle=FormBorderStyle.FixedDialog;MinimizeBox=false;MaximizeBox=false;ShowInTaskbar=false;StartPosition=FormStartPosition.CenterParent;
   AutoScaleMode=AutoScaleMode.None;face=new Font("Segoe UI",12f*this.dpi,FontStyle.Regular,GraphicsUnit.Pixel);Font=face;ClientSize=new Size(S(460),S(640));BackColor=UiBack;ForeColor=UiFore;
   var pane=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=2,RowCount=14,Margin=new Padding(0),Padding=new Padding(S(10),S(8),S(10),S(8)),BackColor=UiBack,ForeColor=UiFore};
   pane.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,S(190)));pane.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
   // info, 8 field rows, acknowledgement, preview, help (rest), status, buttons
   foreach(int h in new[]{S(64),S(28),S(28),S(28),S(28),S(28),S(28),S(28),S(28),S(40),S(64),-100,S(44),S(40)})pane.RowStyles.Add(h<0?new RowStyle(SizeType.Percent,-h):new RowStyle(SizeType.Absolute,h));
   foreach(var b in new[]{InfoBox,PreviewBox,HelpBox,StatusBox}){b.Multiline=true;b.ReadOnly=true;b.WordWrap=true;b.ScrollBars=ScrollBars.Vertical;b.BorderStyle=BorderStyle.None;b.BackColor=UiBack;b.ForeColor=UiFore;b.TabStop=false;b.Dock=DockStyle.Fill;b.Margin=new Padding(0,S(2),0,0);}
   InfoBox.BackColor=UiField;InfoBox.Name="route034_info";InfoBox.AccessibleName="Tuyến đã chọn (chỉ đọc)";PreviewBox.Name="route034_preview";PreviewBox.AccessibleName="Xem trước khối lượng";
   HelpBox.Name="route034_help";HelpBox.AccessibleName="Hướng dẫn";HelpBox.ForeColor=UiMuted;StatusBox.Name="route034_status";StatusBox.AccessibleName="Thông báo";
   foreach(var t in new[]{AreaBox,CircuitBox,CountBox,ExtraBox,ReserveBox}){t.BackColor=UiField;t.ForeColor=UiFore;t.BorderStyle=BorderStyle.FixedSingle;t.Dock=DockStyle.Fill;t.Margin=new Padding(0,S(2),0,S(2));t.TextChanged+=(s,e)=>Changed();}
   AreaBox.MaxLength=Route034.MaxText;CircuitBox.MaxLength=Route034.MaxText;CountBox.MaxLength=12;ExtraBox.MaxLength=40;ReserveBox.MaxLength=40;
   foreach(var c in new[]{CableBox,BasisBox,UnitBox}){c.DropDownStyle=ComboBoxStyle.DropDownList;c.FlatStyle=FlatStyle.Flat;c.BackColor=UiField;c.ForeColor=UiFore;c.Dock=DockStyle.Fill;c.Margin=new Padding(0,S(2),0,S(2));c.SelectedIndexChanged+=(s,e)=>Changed();}
   CableBox.DropDownWidth=S(340);CableBox.MaxDropDownItems=14;
   CableBox.Items.Add(new Choice(null,"(chọn loại cáp)"));foreach(var c in Cable016.Entries)CableBox.Items.Add(new Choice(c.code,c.label+"  ·  "+(c.CoresKnown?"chỉ mét cáp":"mét dây hoặc mét cáp")));
   BasisBox.Items.Add(new Choice(null,"(chọn cách tính)"));BasisBox.Items.Add(new Choice(Route034.WireM,Route034.BasisCaption(Route034.WireM)));BasisBox.Items.Add(new Choice(Route034.CableM,Route034.BasisCaption(Route034.CableM)));
   UnitBox.Items.Add(new Choice(null,"(chọn đơn vị)"));UnitBox.Items.Add(new Choice(Route034.UnitMm,"MM — 1000 đơn vị = 1 m"));UnitBox.Items.Add(new Choice(Route034.UnitM,"M — 1 đơn vị = 1 m"));
   string[] names={"area","circuit","cable","basis","count","extra","reserve","unit"};string[] caps={AreaCaption,CircuitCaption,CableCaption,BasisCaption,CountCaption,ExtraCaption,ReserveCaption,UnitCaption};
   Control[] fields={AreaBox,CircuitBox,CableBox,BasisBox,CountBox,ExtraBox,ReserveBox,UnitBox};
   for(int i=0;i<fields.Length;i++){var l=new Label{Text=caps[i],Name="route034_caption_"+names[i],Dock=DockStyle.Fill,AutoEllipsis=true,Margin=new Padding(0),ForeColor=UiMuted,TextAlign=ContentAlignment.MiddleLeft};
    fields[i].Name="route034_"+names[i];fields[i].AccessibleName=caps[i];fields[i].TabIndex=i;pane.Controls.Add(l,0,i+1);pane.Controls.Add(fields[i],1,i+1);}
   AckBox.Name="route034_unit_ack";AckBox.Dock=DockStyle.Fill;AckBox.Margin=new Padding(0);AckBox.ForeColor=ErrorColor;AckBox.AutoSize=false;AckBox.TabIndex=fields.Length;AckBox.CheckedChanged+=(s,e)=>Changed();
   SaveBtn.Text="Lưu";SaveBtn.Name="route034_save";CancelBtn.Text="Hủy";CancelBtn.Name="route034_cancel";CancelBtn.DialogResult=DialogResult.Cancel;
   var bar=new FlowLayoutPanel{FlowDirection=System.Windows.Forms.FlowDirection.RightToLeft,Dock=DockStyle.Fill,Margin=new Padding(0),Padding=new Padding(0,S(4),0,0),WrapContents=false,TabIndex=fields.Length+1,BackColor=UiBack};
   foreach(var b in new[]{CancelBtn,SaveBtn}){b.AutoSize=true;b.AutoSizeMode=System.Windows.Forms.AutoSizeMode.GrowAndShrink;b.MinimumSize=new Size(S(72),S(30));b.Margin=new Padding(S(4),0,0,0);b.Font=face;
    bool main=b==SaveBtn;b.FlatStyle=FlatStyle.Flat;b.UseVisualStyleBackColor=false;b.BackColor=main?UiTeal:UiField;b.ForeColor=main?UiBack:UiFore;
    b.FlatAppearance.BorderColor=main?UiTeal:UiLine;b.FlatAppearance.MouseOverBackColor=main?UiTealHot:UiHover;b.FlatAppearance.MouseDownBackColor=main?UiTealHot:UiHover;bar.Controls.Add(b);}
   SaveBtn.TabIndex=0;CancelBtn.TabIndex=1;
   pane.Controls.Add(InfoBox,0,0);pane.SetColumnSpan(InfoBox,2);
   int row=fields.Length+1;foreach(var c in new Control[]{AckBox,PreviewBox,HelpBox,StatusBox,bar}){pane.Controls.Add(c,0,row);pane.SetColumnSpan(c,2);row++;}
   Controls.Add(pane);SaveBtn.Click+=(s,e)=>TryAccept();CancelButton=CancelBtn;
   // Read-only facts of the picked route; INSUNITS is a hint only (the user chooses the unit).
   var it=Item;var known=it.knownCode==null?null:Cable016.ByCode(it.knownCode);
   string state=it.existing==null?"Chưa khai báo: nhập đủ các ô rồi Lưu.":it.existingStatus==Route034.Reconfirm?"CẦN XÁC NHẬN LẠI: "+it.existingReason+". Kiểm tra các giá trị đang hiện rồi Lưu.":
    "Đã khai báo và còn hợp lệ: đang hiện các giá trị đã lưu.";
   InfoBox.Text="Tuyến "+it.type+" "+it.handle+" · layer "+it.layer+"\r\nChiều dài hiện tại: "+Route034.Units(it.length)+" đơn vị bản vẽ · INSUNITS = "+sel.insunits.ToString(CultureInfo.InvariantCulture)+
    " ("+UnitName(sel.insunits)+"; chỉ để tham khảo, bạn tự chọn đơn vị khai báo)"+(known!=null?"\r\nLoại cáp theo tuyến đã vẽ: "+known.label+" ("+known.code+"), khoá.":"")+"\r\n"+state;
   HelpBox.Text=RouteUi034.QuickHelp+"\r\n"+ReconfirmNote+"\r\n"+RouteUi034.MoreHelp+"\r\n"+Route034.Rule;
   // Prefill only from the route's actual saved record and its bound cable type; otherwise every value starts empty.
   loading=true;
   try{var had=sel.CommonConfig();
    Pick(CableBox,known!=null?known.code:had!=null?had.code:null);CableBox.Enabled=known==null;
    if(had!=null){AreaBox.Text=had.area;CircuitBox.Text=had.circuit;Pick(BasisBox,had.basis);CountBox.Text=had.parallelCount.ToString(CultureInfo.InvariantCulture);
     ExtraBox.Text=Exact(had.extraVerticalMeters);ReserveBox.Text=Exact(had.reservePercent);Pick(UnitBox,had.declaredUnit);}
    else{Pick(BasisBox,null);Pick(UnitBox,null);}}
   finally{loading=false;}
   ActiveControl=AreaBox;Changed();}
  static void Pick(ComboBox box,string value){for(int i=0;i<box.Items.Count;i++){var c=(Choice)box.Items[i];if(string.Equals(c.value,value,StringComparison.Ordinal)){box.SelectedIndex=i;return;}}box.SelectedIndex=0;}
  static string Value(ComboBox box){var c=box.SelectedItem as Choice;return c==null?null:c.value;}
  // Selects the combo entry with this stored value (null = placeholder). QA helper; same SelectedIndexChanged path as the mouse.
  public void Choose(ComboBox box,string value){Pick(box,value);}
  void Status(string text,bool error){StatusBox.ForeColor=error?ErrorColor:UiFore;StatusBox.Text=text??"";}
  // Warning when the chosen unit does not match INSUNITS mm(4)/m(6) or INSUNITS is unset/other; null = no acknowledgement needed.
  public string UnitIssue(){var u=Value(UnitBox);return u==null?null:Route034.UnitWarning(Selection.insunits,u);}
  // Current input → validated configuration (exception = reason): Route034.Make with the strict parsers, then RouteCad034.Preview (bound cable code and
  // calculation on the picked route). Nothing is defaulted or corrected.
  public RouteConfig034 Build(){int? count=Route034.ParseCount(CountBox.Text,CountCaption);double? extra=Route034.ParseDecimal(ExtraBox.Text,ExtraCaption),reserve=Route034.ParseDecimal(ReserveBox.Text,ReserveCaption);
   var c=Route034.Make(AreaBox.Text,CircuitBox.Text,Value(CableBox),Value(BasisBox),count,extra,reserve,Value(UnitBox));RouteCad034.Preview(Selection,c);return c;}
  // Live preview of the actual route metres and the calculated quantity; Lưu is enabled only for a valid, acknowledged input.
  void Changed(){if(loading)return;string warn=UnitIssue();
   if(!string.Equals(warn,lastWarn,StringComparison.Ordinal)){lastWarn=warn;if(AckBox.Checked)AckBox.Checked=false;}
   AckBox.Visible=warn!=null;AckBox.Text=warn==null?"":"Xác nhận dùng đơn vị "+Value(UnitBox)+" dù INSUNITS = "+Selection.insunits.ToString(CultureInfo.InvariantCulture)+" ("+UnitName(Selection.insunits)+"); MEPS không đổi INSUNITS.";
   string unitText=Value(UnitBox);
   try{var c=Build();var q=Route034.Calculate(Item.length,c);
    PreviewBox.Text="Chiều dài tuyến: "+Route034.Meters(q.routeMeters)+" m ("+Route034.Units(Item.length)+" đơn vị ÷ "+(c.declaredUnit==Route034.UnitMm?"1000":"1")+")\r\n"+
     "Khối lượng: ("+Route034.Meters(q.routeMeters)+" + "+Exact(c.extraVerticalMeters)+") × "+c.parallelCount.ToString(CultureInfo.InvariantCulture)+" × (1 + "+Exact(c.reservePercent)+"%) = "+
     Route034.Meters(q.quantity)+" "+Route034.BasisCaption(c.basis)+(warn!=null?"\r\n"+warn:"");
    bool ok=warn==null||AckBox.Checked;SaveBtn.Enabled=ok;
    Status(ok?"Sẵn sàng lưu. Số hiển thị làm tròn 1 chữ số; MEPS lưu và cộng đúng giá trị đã nhập.":"Đơn vị khai báo khác INSUNITS: đọc cảnh báo và tích ô xác nhận để Lưu.",!ok);}
   catch(InvalidOperationException ex){SaveBtn.Enabled=false;
    string len=unitText==null?"Chiều dài hiện tại: "+Route034.Units(Item.length)+" đơn vị bản vẽ; chọn đơn vị để đổi ra mét.":
     "Chiều dài tuyến: "+Route034.Meters(Item.length/Route034.Factor(unitText))+" m ("+Route034.Units(Item.length)+" đơn vị ÷ "+(unitText==Route034.UnitMm?"1000":"1")+")";
    PreviewBox.Text=len+"\r\nKhối lượng: chưa tính (thiếu hoặc sai thông số)."+(warn!=null?"\r\n"+warn:"");Status(ex.Message,true);}}
  // Validates with the core rules; false = the dialog stays open with the reason; nothing written.
  public bool TryAccept(){try{var c=Build();var w=UnitIssue();if(w!=null&&!AckBox.Checked)throw new InvalidOperationException("Chưa lưu: "+w+" Tích ô xác nhận đơn vị trước khi Lưu.");Result=c;}
   catch(Exception ex){Result=null;Status(ex.Message,true);return false;}
   DialogResult=DialogResult.OK;return true;}
  protected override void Dispose(bool disposing){base.Dispose(disposing);if(disposing)face.Dispose();}
 }
 // Read-only quantity report of the active Model (one scan = one snapshot, dated). Groups and details are display rows of RouteScan034; numbers keep full
 // precision in the scan and are rounded to one decimal only on screen. Buttons: Quét lại, Thiết lập dòng đã chọn…, Xuất CSV nhóm…, Xuất CSV chi tiết…, Đóng.
 public sealed class RouteReport034:Form {
  static readonly Color UiBack=RouteDialog034.UiBack,UiField=RouteDialog034.UiField,UiLine=RouteDialog034.UiLine,UiFore=RouteDialog034.UiFore,UiMuted=RouteDialog034.UiMuted,
   UiTeal=RouteDialog034.UiTeal,UiTealHot=RouteDialog034.UiTealHot,UiHover=RouteDialog034.UiHover,ErrorColor=RouteDialog034.ErrorColor;
  public const string OmittedNote="Line/Polyline không có dữ liệu tuyến 034 và không phải tuyến cáp 016 (layer/dữ liệu cáp) là nét vẽ thường: không liệt kê, không tính.";
  public readonly Label SummaryLabel=new Label(),StateLabel=new Label(),GroupCaption=new Label(),DetailCaption=new Label();
  public readonly ListView GroupList=new ListView(),DetailList=new ListView();
  public readonly TextBox NotesBox=new TextBox(),StatusBox=new TextBox();
  public readonly Button RescanBtn=new Button(),SetupBtn=new Button(),ExportGroupsBtn=new Button(),ExportDetailBtn=new Button(),CloseBtn=new Button();
  // Shown scan, its local time stamp, its groups (null when aggregation failed: GroupError says why) and the state line.
  public RouteScan034 Scan;public string ScannedAt,GroupError,StateText;public List<RouteGroup034> GroupsShown;
  // True when the shown report is not complete (excluded routes or an aggregation error).
  public bool Incomplete;
  // Set by "Thiết lập dòng đã chọn…" (dialog closes with OK); RouteUi034 then runs the setup on that entity.
  public ObjectId? SetupId;
  // Bound by RouteUi034: Rescan = guarded fresh ScanModel; Export(detail) = guarded fresh scan + CSV + file, returns the message.
  public Func<RouteScan034> Rescan;public Func<bool,string> Export;
  readonly float dpi;readonly Font face,bold;
  int S(int px){return Math.Max(1,(int)Math.Round(px*dpi));}
  public RouteReport034(float dpi){this.dpi=float.IsNaN(dpi)||dpi<=0?1f:Math.Max(1f,Math.Min(3f,dpi));
   Text="MEPS — Bóc khối lượng dây/cáp (chỉ đọc)";Name="route034_report";FormBorderStyle=FormBorderStyle.Sizable;MinimizeBox=false;MaximizeBox=true;ShowInTaskbar=false;StartPosition=FormStartPosition.CenterParent;
   AutoScaleMode=AutoScaleMode.None;face=new Font("Segoe UI",12f*this.dpi,FontStyle.Regular,GraphicsUnit.Pixel);bold=new Font("Segoe UI",12f*this.dpi,FontStyle.Bold,GraphicsUnit.Pixel);Font=face;
   ClientSize=new Size(S(820),S(600));MinimumSize=new Size(S(700),S(480));BackColor=UiBack;ForeColor=UiFore;
   var pane=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=1,RowCount=9,Margin=new Padding(0),Padding=new Padding(S(10),S(8),S(10),S(8)),BackColor=UiBack,ForeColor=UiFore};pane.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
   // summary, state, groups caption, groups (40 %), details caption, details (60 %), notes, status, buttons
   foreach(int h in new[]{S(40),S(40),S(22),-40,S(22),-60,S(72),S(40),S(40)})pane.RowStyles.Add(h<0?new RowStyle(SizeType.Percent,-h):new RowStyle(SizeType.Absolute,h));
   foreach(var l in new[]{SummaryLabel,StateLabel,GroupCaption,DetailCaption}){l.Dock=DockStyle.Fill;l.AutoEllipsis=true;l.Margin=new Padding(0);l.ForeColor=UiFore;l.TextAlign=ContentAlignment.MiddleLeft;}
   SummaryLabel.Name="route034_summary";StateLabel.Name="route034_state";StateLabel.Font=bold;GroupCaption.Text="Tổng theo khu vực / mạch / loại cáp / cách tính (chỉ tuyến đã tính)";GroupCaption.ForeColor=UiMuted;
   DetailCaption.Text="Chi tiết từng tuyến (tuyến KHÔNG được tính ở đầu bảng)";DetailCaption.ForeColor=UiMuted;
   foreach(var v in new[]{GroupList,DetailList}){v.View=View.Details;v.FullRowSelect=true;v.MultiSelect=false;v.HideSelection=false;v.GridLines=false;v.BorderStyle=BorderStyle.None;
    v.BackColor=UiField;v.ForeColor=UiFore;v.Dock=DockStyle.Fill;v.Margin=new Padding(0,S(2),0,S(2));v.Font=face;}
   GroupList.Name="route034_groups";GroupList.AccessibleName="Bảng tổng";DetailList.Name="route034_details";DetailList.AccessibleName="Bảng chi tiết";
   var gcols=new[]{Tuple.Create("Khu vực",110),Tuple.Create("Mạch/lộ",90),Tuple.Create("Loại cáp",130),Tuple.Create("Cách tính",120),Tuple.Create("Số tuyến",70),Tuple.Create("Chiều dài tuyến (m)",120),Tuple.Create("Khối lượng (m)",110)};
   for(int i=0;i<gcols.Length;i++)GroupList.Columns.Add(gcols[i].Item1,S(gcols[i].Item2),i>=4?HorizontalAlignment.Right:HorizontalAlignment.Left);
   DetailList.ShowItemToolTips=true;
   foreach(var c in new[]{Tuple.Create("Trạng thái",110),Tuple.Create("Handle",60),Tuple.Create("Khu vực",90),Tuple.Create("Mạch/lộ",80),Tuple.Create("Loại cáp",110),Tuple.Create("Cách tính",80),
    Tuple.Create("Chiều dài hiện tại",110),Tuple.Create("Khối lượng (m)",100),Tuple.Create("Lý do / cảnh báo",360)})DetailList.Columns.Add(c.Item1,S(c.Item2));
   foreach(var b in new[]{NotesBox,StatusBox}){b.Multiline=true;b.ReadOnly=true;b.WordWrap=true;b.ScrollBars=ScrollBars.Vertical;b.BorderStyle=BorderStyle.None;b.BackColor=UiBack;b.ForeColor=UiFore;b.TabStop=false;b.Dock=DockStyle.Fill;b.Margin=new Padding(0,S(2),0,0);}
   NotesBox.ForeColor=UiMuted;NotesBox.Name="route034_notes";NotesBox.AccessibleName="Cảnh báo và ghi chú";StatusBox.Name="route034_report_status";StatusBox.AccessibleName="Thông báo";
   RescanBtn.Text="Quét lại";RescanBtn.Name="route034_rescan";SetupBtn.Text="Thiết lập dòng đã chọn…";SetupBtn.Name="route034_setup_selected";
   ExportGroupsBtn.Text="Xuất CSV nhóm…";ExportGroupsBtn.Name="route034_export_groups";ExportDetailBtn.Text="Xuất CSV chi tiết…";ExportDetailBtn.Name="route034_export_detail";
   CloseBtn.Text="Đóng";CloseBtn.Name="route034_close";CloseBtn.DialogResult=DialogResult.Cancel;
   var bar=new FlowLayoutPanel{FlowDirection=System.Windows.Forms.FlowDirection.RightToLeft,Dock=DockStyle.Fill,Margin=new Padding(0),Padding=new Padding(0,S(4),0,0),WrapContents=false,BackColor=UiBack};
   foreach(var b in new[]{CloseBtn,ExportDetailBtn,ExportGroupsBtn,SetupBtn,RescanBtn}){b.AutoSize=true;b.AutoSizeMode=System.Windows.Forms.AutoSizeMode.GrowAndShrink;b.MinimumSize=new Size(S(72),S(30));b.Margin=new Padding(S(4),0,0,0);b.Font=face;
    bool main=b==ExportGroupsBtn;b.FlatStyle=FlatStyle.Flat;b.UseVisualStyleBackColor=false;b.BackColor=main?UiTeal:UiField;b.ForeColor=main?UiBack:UiFore;
    b.FlatAppearance.BorderColor=main?UiTeal:UiLine;b.FlatAppearance.MouseOverBackColor=main?UiTealHot:UiHover;b.FlatAppearance.MouseDownBackColor=main?UiTealHot:UiHover;bar.Controls.Add(b);}
   var rows=new Control[]{SummaryLabel,StateLabel,GroupCaption,GroupList,DetailCaption,DetailList,NotesBox,StatusBox,bar};for(int r=0;r<rows.Length;r++)pane.Controls.Add(rows[r],0,r);
   Controls.Add(pane);CancelButton=CloseBtn;
   RescanBtn.Click+=(s,e)=>DoRescan();SetupBtn.Click+=(s,e)=>SetupSelected();ExportGroupsBtn.Click+=(s,e)=>DoExport(false);ExportDetailBtn.Click+=(s,e)=>DoExport(true);
   DetailList.SelectedIndexChanged+=(s,e)=>{SetupBtn.Enabled=SelectedRow()!=null;};SetupBtn.Enabled=false;}
  void Status(string text,bool error){StatusBox.ForeColor=error?ErrorColor:UiFore;StatusBox.Text=text??"";}
  static string Q(double? v){return v.HasValue?Route034.Meters(v.Value):"";}
  // Shows a scan (replaces everything on screen). A grouping failure (F034-AGG01) is shown as an error; the details stay visible.
  public void ShowScan(RouteScan034 scan,string scannedAt){if(scan==null)throw new ArgumentNullException("scan");Scan=scan;ScannedAt=scannedAt;GroupError=null;GroupsShown=null;
   try{GroupsShown=scan.Groups();}catch(InvalidOperationException ex){GroupError=ex.Message;}
   GroupList.BeginUpdate();try{GroupList.Items.Clear();
    if(GroupsShown!=null)foreach(var g in GroupsShown){var c=Cable016.ByCode(g.code);
     GroupList.Items.Add(new ListViewItem(new[]{g.area,g.circuit,c==null?g.code:c.label,Route034.BasisCaption(g.basis),g.routes.ToString(CultureInfo.InvariantCulture),Route034.Meters(g.routeMeters),Route034.Meters(g.quantity)}){Tag=g});}}
   finally{GroupList.EndUpdate();}
   DetailList.BeginUpdate();try{DetailList.Items.Clear();
    foreach(var r in scan.rows.OrderBy(x=>x.status==Route034.Ok?1:0)){var cfg=r.config;var cs=cfg==null?null:Cable016.ByCode(cfg.code);
     string len=r.routeMeters.HasValue?Route034.Meters(r.routeMeters.Value)+" m":r.liveLength.HasValue?Route034.Units(r.liveLength.Value)+" đv bản vẽ":"";
     string why=string.Join(" · ",new[]{r.reason,r.warning}.Where(x=>!string.IsNullOrEmpty(x)));
     var item=new ListViewItem(new[]{Route034.StatusCaption(r.status),r.handle,cfg==null?"":cfg.area,cfg==null?"":cfg.circuit,cs!=null?cs.label:cfg!=null?cfg.code:r.knownCode??"",
      cfg==null?"":Route034.BasisCaption(cfg.basis),len,Q(r.quantity),why}){Tag=r,ToolTipText=why};
     if(r.status!=Route034.Ok)item.ForeColor=ErrorColor;DetailList.Items.Add(item);}}
   finally{DetailList.EndUpdate();}
   int ex2=RouteCsv034.Excluded(scan);Incomplete=ex2>0||GroupError!=null;
   SummaryLabel.Text=scan.Summary()+" Quét lúc "+(scannedAt??"?")+" (giờ máy).";
   StateText=GroupError!=null?"LỖI TỔNG HỢP: "+GroupError:ex2>0?"CHƯA ĐỦ: "+ex2+" tuyến KHÔNG được tính (dòng đỏ ở đầu bảng chi tiết). Không dùng làm số liệu cuối.":
    scan.rows.Count==0?"Model chưa có tuyến dây/cáp MEPS nào.":"ĐỦ: mọi tuyến nhận diện được đều đã tính"+(scan.warnings.Count>0?" — có "+scan.warnings.Count+" cảnh báo bên dưới.":".");
   StateLabel.Text=StateText;StateLabel.ForeColor=Incomplete?ErrorColor:scan.rows.Count==0?UiMuted:UiTeal;
   var notes=new List<string>(scan.warnings.Select(w=>"CẢNH BÁO: "+w));notes.Add(OmittedNote);notes.Add(RouteUi034.CsvHelp);notes.Add(RouteUi034.QuickHelp);notes.Add(RouteUi034.MoreHelp);
   NotesBox.Text=string.Join("\r\n",notes);ExportGroupsBtn.Enabled=GroupError==null;SetupBtn.Enabled=SelectedRow()!=null;
   Status(GroupError!=null?"Không lập được bảng tổng: "+GroupError:"Chọn một dòng rồi 'Thiết lập dòng đã chọn…' để khai báo / xác nhận lại tuyến đó (hộp này đóng).",GroupError!=null);}
  public RouteRow034 SelectedRow(){return DetailList.SelectedItems.Count==1?DetailList.SelectedItems[0].Tag as RouteRow034:null;}
  // QA helper: selects the detail row of this handle (exact text); false = not listed.
  public bool SelectHandle(string handle){foreach(ListViewItem i in DetailList.Items){var r=i.Tag as RouteRow034;if(r!=null&&string.Equals(r.handle,handle,StringComparison.Ordinal)){i.Selected=true;i.Focused=true;return true;}}return false;}
  public void DoRescan(){try{if(Rescan==null)throw new InvalidOperationException("Chưa nối với AutoCAD.");ShowScan(Rescan(),RouteUi034.Now());StatusBox.Text="Đã quét lại lúc "+ScannedAt+". "+StatusBox.Text;}
   catch(Exception ex){Status("Chưa quét lại được: "+ex.Message,true);}}
  public void DoExport(bool detail){try{if(Export==null)throw new InvalidOperationException("Chưa nối với AutoCAD.");Status(Export(detail),false);}catch(Exception ex){Status("Chưa xuất CSV: "+ex.Message,true);}}
  public void SetupSelected(){var r=SelectedRow();if(r==null){Status("Chọn một dòng trong bảng chi tiết trước.",true);return;}SetupId=r.id;DialogResult=DialogResult.OK;}
  protected override void Dispose(bool disposing){base.Dispose(disposing);if(disposing){face.Dispose();bold.Dispose();}}
 }
 public sealed class RouteOutcome034 {public bool changed,cancelled,warn;public string message;}
 // Runs INSIDE the bound TVXDMEPS060DO command (Plugin.RunRoute). guard = the plugin's write context (same document, Model, UNDO on) before/after the pick
 // and every dialog and right before Commit; readGuard = same document + Model, for the read-only report and the CSV export.
 public static class RouteUi034 {
  public const string Setup="route034-setup",Report="route034-report",SetupCaption="Thiết lập tuyến…",ReportCaption="Bóc khối lượng…";
#if BDGKL034 && ROOMCAD037
  // TVXD-MEPS-ROOMQTY-037 S9: in the room build the retained entry opens the room quantity table (RoomUi037.Run); help follows its actual buttons.
  public const string QuickHelp="Bốn bước: 1. Vẽ dây (DÂY DẪN → loại cáp) và đặt block thiết bị có TAG. 2. DÂY DẪN → "+BdgklUi034.Caption+" → DỰ ÁN / TẦNG / PHÒNG → tạo dự án, tầng, phòng, cao độ. "+
   "3. CHỌN VÙNG CAD → chọn phòng, đơn vị MM hoặc M (khác đơn vị bản vẽ thì phải tích xác nhận; MEPS không đổi INSUNITS), dung sai nối, hai góc phòng. "+
   "4. Xem lại các dòng tuyến thực tế, bổ sung dữ liệu còn thiếu (THÔNG SỐ, cao độ) → LƯU VÀO BẢN VẼ → XUẤT EXCEL khi mọi dòng đủ dữ liệu. "+
   "Chiều dài lấy từ hình học dây CAD; MEPS không tạo DIM hay bảng hiển thị trong bản vẽ. Đường đi chung (ống) khác khối lượng dây: số bộ dây/ruột phải khai báo, MEPS không tự suy.";
#elif BDGKL034
  // TVXD-MEPS-ROOMQTY-037: the BDGKL034 DÂY DẪN menu no longer lists 'Thiết lập tuyến…' / 'Bóc khối lượng…'; help points to the one retained entry.
  public const string QuickHelp="Hai bước: 1. Vẽ tuyến (DÂY DẪN → loại cáp). 2. DÂY DẪN → "+BdgklUi034.Caption+" → Thêm tuyến/Sửa tuyến → chọn tuyến, điểm đầu, điểm cuối → nhập → Lưu.";
#else
  public const string QuickHelp="Ba bước: 1. Vẽ tuyến (DÂY DẪN → loại cáp). 2. Khai báo: DÂY DẪN → Thiết lập tuyến… → chọn MỘT tuyến → nhập → Lưu. 3. Bóc: DÂY DẪN → Bóc khối lượng….";
#endif
  public const string MoreHelp="Mỗi tuyến (một Line/Polyline) mang MỘT khu vực, MỘT mạch/lộ, MỘT loại cáp. Nhánh rẽ: vẽ thành các đoạn riêng cùng mạch; 'Bổ sung cho đoạn này' cộng RIÊNG cho từng đoạn, "+
   "đừng nhập lặp ở nhiều đoạn. Nhãn TAG chỉ để hiển thị, không phải sơ đồ đi dây: MEPS không suy số sợi hay mạch từ TAG. COPY/MOVE/sửa hình/đổi INSUNITS → cần xác nhận lại. "+
   "Chỉ đo tuyến trong Model; đơn vị khai báo MM hoặc M. MEPS không tự thiết kế (không chọn cỡ cáp, không tính tải, không tự thêm dự phòng).";
  // NR034-UI-01/02: CSV is plain text; a double-click open in Excel may change cell types. Shown in the report notes and the guide.
  public const string CsvHelp="CSV: mở trong Excel bằng Data → From Text/CSV, chọn UTF-8, dấu phân cách dấu phẩy (,), dấu thập phân dấu chấm (.), và đặt các cột mã "+
   "(Handle, Khu vực, Mạch/lộ, Mã cáp) là Text. Mở trực tiếp bằng double-click có thể làm Excel đổi kiểu ô (ví dụ 01 → 1, 1,2 → số/ngày, 1E5 → 100000); tệp CSV vẫn giữ đúng chữ.";
#if BDGKL034
  // TVXD-MEPS-WIREQTY-034 BDGKL pilot: 'Bảng khối lượng điện…' (BdgklUi034.Caption, 037) uses this same queued bound-document path (Plugin.RunRoute guards); BdgklUi034.Run owns the flow.
  public static bool Handles(string action){return action==Setup||action==Report||action==BdgklUi034.Action;}
#else
  public static bool Handles(string action){return action==Setup||action==Report;}
#endif
  // QA hooks for the native harness (synchronous, no keyboard): Select returns the picked ObjectIds (null = Esc; exactly one is required); Show / ShowReport
  // display the dialogs and return their results; PickCsvPath(owner, suggested name) returns the confirmed file path (null = cancelled; the default
  // SaveFileDialog asks the standard overwrite question, so a returned existing path means the user confirmed). ResetHooks restores all defaults.
  public static Func<Document,ObjectId[]> Select=DefaultSelect;
  public static Func<RouteDialog034,DialogResult> Show=DefaultShow;
  public static Func<RouteReport034,DialogResult> ShowReport=DefaultShowReport;
  public static Func<IWin32Window,string,string> PickCsvPath=DefaultPickCsvPath;
  public static DialogResult DefaultShow(RouteDialog034 f){return App.ShowModalDialog(f);}
  public static DialogResult DefaultShowReport(RouteReport034 f){return App.ShowModalDialog(f);}
  public static void ResetHooks(){Select=DefaultSelect;Show=DefaultShow;ShowReport=DefaultShowReport;PickCsvPath=DefaultPickCsvPath;}
  // ONE entity: Line or LWPOLYLINE (exact classes; old 2D/3D polylines are rejected by the prompt). Locked-layer objects can be picked so the core reports why.
  public static ObjectId[] DefaultSelect(Document d){var o=new PromptEntityOptions("\nChọn MỘT tuyến LINE/LWPOLYLINE hở trên Model (Esc huỷ): "){AllowObjectOnLockedLayer=true};
   o.SetRejectMessage("\nChỉ chọn LINE hoặc LWPOLYLINE.");o.AddAllowedClass(typeof(CadLine),true);o.AddAllowedClass(typeof(CadPolyline),true);
   var r=d.Editor.GetEntity(o);return r.Status==PromptStatus.OK?new[]{r.ObjectId}:null;}
  public static string DefaultPickCsvPath(IWin32Window owner,string name){
   using(var f=new SaveFileDialog{Title="Xuất CSV (UTF-8)",Filter="CSV UTF-8 (*.csv)|*.csv",DefaultExt="csv",AddExtension=true,OverwritePrompt=true,CheckPathExists=true,RestoreDirectory=true,FileName=name})
    return f.ShowDialog(owner)==DialogResult.OK?f.FileName:null;}
  public static string Now(){return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss",CultureInfo.InvariantCulture);}
  public static RouteOutcome034 Run(string action,Document d,Action<string> guard,Action<string> readGuard,float dpi){if(!Handles(action))throw new InvalidOperationException("Thao tác tuyến không hợp lệ.");
   if(action==Report)return RunReport(d,guard,readGuard,dpi);
#if BDGKL034 && ROOMCAD037
   // TVXD-MEPS-ROOMQTY-037: the same entry opens the room quantity table; the legacy per-route BDGKL report stays reachable there via 'Dữ liệu cũ'.
   if(action==BdgklUi034.Action)return RoomUi037.Run(d,guard,readGuard,dpi);
#elif BDGKL034
   if(action==BdgklUi034.Action)return BdgklUi034.Run(d,guard,readGuard,dpi);
#endif
   guard("trước khi chọn tuyến");var picked=Select(d);guard("sau khi chọn tuyến");
   if(picked==null)return new RouteOutcome034{cancelled=true,message="Đã huỷ thiết lập tuyến; bản vẽ không đổi."};
   if(picked.Length==0)return new RouteOutcome034{cancelled=true,message="Chưa chọn tuyến nào; bản vẽ không đổi."};
   if(picked.Length!=1)throw new InvalidOperationException("Thiết lập tuyến chọn đúng MỘT tuyến mỗi lần (đã chọn "+picked.Length+"); bản vẽ không đổi.");
   return SetupOne(d,picked[0],guard,dpi);}
  // Snapshot (refuses an unsupported/invalid/conflicting route) → dialog → Apply (stale signature = abort, one transaction, guard before Commit).
  static RouteOutcome034 SetupOne(Document d,ObjectId id,Action<string> guard,float dpi){var db=d.Database;var snap=RouteCad034.Snapshot(db,new[]{id});RouteConfig034 cfg;
   using(var dlg=new RouteDialog034(snap,dpi)){var r=Show(dlg);guard("sau hộp thiết lập tuyến");
    if(r!=DialogResult.OK||dlg.Result==null)return new RouteOutcome034{cancelled=true,message="Đã huỷ thiết lập tuyến "+snap.items[0].handle+"; bản vẽ không đổi."};cfg=dlg.Result;}
   guard("ngay trước khi lưu thông số tuyến");var res=RouteCad034.Apply(db,snap,cfg,guard);
   string qty="";if(res.rows.Count==1&&res.rows[0].routeMeters.HasValue&&res.rows[0].quantity.HasValue)
    qty=" Tuyến "+res.rows[0].handle+": "+Route034.Meters(res.rows[0].routeMeters.Value)+" m tuyến → "+Route034.Meters(res.rows[0].quantity.Value)+" "+Route034.BasisCaption(cfg.basis)+".";
   return new RouteOutcome034{changed=res.changed,warn=res.warn,message=res.message+qty};}
  // Read-only report; the drawing is never written here. A chosen "set up this row" runs SetupOne afterwards in the same command (own guards).
  static RouteOutcome034 RunReport(Document d,Action<string> guard,Action<string> readGuard,float dpi){var db=d.Database;readGuard("trước khi quét Model");
   var scan=RouteCad034.ScanModel(db);ObjectId? setup;string state;bool incomplete;
   using(var dlg=new RouteReport034(dpi)){
    dlg.Rescan=()=>{readGuard("trước khi quét lại");return RouteCad034.ScanModel(db);};
    dlg.Export=detail=>ExportCsv(dlg,db,readGuard,detail);
    dlg.ShowScan(scan,Now());ShowReport(dlg);readGuard("sau hộp bóc khối lượng");setup=dlg.SetupId;state=dlg.StateText;incomplete=dlg.Incomplete;}
   if(!setup.HasValue)return new RouteOutcome034{cancelled=true,warn=incomplete,message="Bóc khối lượng (chỉ đọc): "+state+" Bản vẽ không đổi."};
   guard("trước khi thiết lập tuyến đã chọn");return SetupOne(d,setup.Value,guard,dpi);}
  // Export = a FRESH scan (never a stale screen): the dialog is reloaded with it first, so the screen shows exactly what is written. Aggregation failure =
  // exception before any file dialog. UTF-8 with BOM; nothing written on cancel. F034-EXPORT01: only the path the user picked is written, atomically
  // (RouteFile034.WriteCsv); an existing file is replaced with a kept backup, never truncated in place.
  static string ExportCsv(RouteReport034 dlg,Database db,Action<string> readGuard,bool detail){readGuard("trước khi xuất CSV");
   var fresh=RouteCad034.ScanModel(db);string at=Now();dlg.ShowScan(fresh,at);
   string text=detail?RouteCsv034.Detail(fresh,at):RouteCsv034.Groups(fresh,at);
   string name="MEPS_BOC_DAY_CAP_"+(detail?"CHI_TIET_":"NHOM_")+DateTime.Now.ToString("yyyyMMdd_HHmm",CultureInfo.InvariantCulture)+".csv";
   var path=PickCsvPath(dlg,name);if(string.IsNullOrEmpty(path))return "Đã huỷ xuất CSV; chưa ghi tệp nào.";
   readGuard("sau hộp lưu CSV");var w=RouteFile034.WriteCsv(path,text);
   return "Đã xuất "+Path.GetFileName(w.path)+" (quét lúc "+at+"; "+(detail?fresh.rows.Count+" dòng chi tiết":dlg.GroupsShown.Count+" dòng tổng")+"; đã đọc lại khớp)."+
    (w.backup!=null?" Bản cũ giữ tại "+w.backup+".":"")+" "+dlg.StateText;}
 }
}
#endif
#if ROUTE034
namespace Tvxd.Meps {
 // Result of one successful RouteFile034.WriteCsv: full target path, kept backup of the replaced file (null for a new file), bytes written (BOM included).
 public sealed class RouteWrite034 {public string path,backup,message;public bool replaced;public int bytes;}
 // TVXD-MEPS-WIREQTY-034 F034-EXPORT01: atomic write of ONE CSV file the user explicitly chose (no other file is exported). No AutoCAD type: compiles with
 // /define:ROUTE034;ROUTE034_PURE (System only) for the file harness. Steps, in order:
 //  1. Check (before any write): rooted path, not a device path (\\?\ \\.\), has a file name, the target is not a directory, the parent directory exists
 //     (never created), and neither the target nor any existing ancestor below the volume root is a reparse point (junction/symlink/OneDrive placeholder).
 //  2. UTF-8 WITH BOM bytes (strict encoder: unpaired surrogate = refusal) to a NEW uniquely named temp file in the same directory (FileMode.CreateNew,
 //     FileShare.None, flushed to disk, closed), then read back and compared byte-for-byte. QA: FailAt = "after-temp" fails here.
 //  3. Existing target: File.Replace(temp, target, unique backup, false) — the old bytes stay in the backup, whose path is reported. New target:
 //     File.Move(temp, target), which never overwrites (a file that appears meanwhile is left alone). No fallback to a direct overwrite or delete.
 //  4. The target is read back; only an exact match is reported as written.
 // Any failure before step 3 leaves an existing target byte-for-byte unchanged; only the temp file THIS call created is deleted, else its path is reported.
 // A failure inside step 3 reports where the old file, the new data and any temp file are. Nothing recursive, no settings, no attribute changes.
 public static class RouteFile034 {
  public const string AfterTemp="after-temp";
  // QA failure injection (GPT harness only; null in production).
  public static string FailAt;
  static readonly CultureInfo Inv=CultureInfo.InvariantCulture;
  static void Fail(string stage){if(FailAt!=null&&string.Equals(FailAt,stage,StringComparison.Ordinal))throw new IOException("QA: lỗi giả lập tại "+stage+".");}
  static InvalidOperationException Refuse(string why){return new InvalidOperationException(why+" Chưa ghi gì; tệp đang có (nếu có) giữ nguyên.");}
  // Reparse point at the path or any existing ancestor below the volume root = refusal (attributes are only read).
  static void NoReparse(string full){
   for(string p=full;!string.IsNullOrEmpty(p);p=Path.GetDirectoryName(p)){if(string.Equals(Path.GetPathRoot(p),p,StringComparison.OrdinalIgnoreCase))break;
    if(!File.Exists(p)&&!Directory.Exists(p))continue;
    if((File.GetAttributes(p)&FileAttributes.ReparsePoint)!=0)throw Refuse("Đường dẫn "+p+" là liên kết (junction/symlink/tệp OneDrive chưa tải); MEPS không ghi CSV qua liên kết.");}}
  // Validated full path of the chosen file (step 1). Public for the harness.
  public static string Check(string path){
   if(path==null||path.Trim().Length==0)throw Refuse("Chưa có đường dẫn tệp CSV.");
   if(path.StartsWith(@"\\?\",StringComparison.Ordinal)||path.StartsWith(@"\\.\",StringComparison.Ordinal))throw Refuse("Đường dẫn thiết bị "+path+" không được dùng.");
   string full;try{full=Path.GetFullPath(path);}catch(Exception ex){throw Refuse("Đường dẫn CSV không hợp lệ ("+ex.Message+").");}
   if(!Path.IsPathRooted(full))throw Refuse("Đường dẫn CSV phải là đường dẫn đầy đủ.");
   string name=Path.GetFileName(full),dir=Path.GetDirectoryName(full);
   if(string.IsNullOrEmpty(name)||string.IsNullOrEmpty(dir))throw Refuse("Đường dẫn "+full+" không phải một tệp.");
   NoReparse(full);
   if(Directory.Exists(full))throw Refuse(full+" là thư mục, không phải tệp CSV.");
   if(!Directory.Exists(dir))throw Refuse("Không thấy thư mục "+dir+"; MEPS không tự tạo thư mục.");
   return full;}
  // Exact bytes written: UTF-8 preamble EF BB BF + strict UTF-8 of text. Public for the harness.
  public static byte[] Bytes(string text){var enc=new UTF8Encoding(true,true);byte[] pre=enc.GetPreamble(),body;
   try{body=enc.GetBytes(text??"");}catch(ArgumentException ex){throw Refuse("Nội dung CSV có ký tự không mã hoá được UTF-8 ("+ex.Message+").");}
   var all=new byte[pre.Length+body.Length];Buffer.BlockCopy(pre,0,all,0,pre.Length);Buffer.BlockCopy(body,0,all,pre.Length,body.Length);return all;}
  static bool Same(byte[] a,byte[] b){if(a==null||b==null||a.Length!=b.Length)return false;for(int i=0;i<a.Length;i++)if(a[i]!=b[i])return false;return true;}
  // Deletes only the temp file this call created; returns a note naming it when it could not be removed.
  static string Clean(string temp,bool created){if(!created)return "";
   try{if(File.Exists(temp))File.Delete(temp);}catch(Exception){}
   return File.Exists(temp)?" Còn tệp tạm "+temp+" (có thể xoá).":"";}
  public static RouteWrite034 WriteCsv(string path,string text){string full=Check(path);byte[] data=Bytes(text);
   string dir=Path.GetDirectoryName(full),name=Path.GetFileName(full),tag=DateTime.Now.ToString("yyyyMMdd_HHmmss",Inv)+"_"+Guid.NewGuid().ToString("N").Substring(0,12);
   string temp=Path.Combine(dir,name+".tmp_"+tag),backup=null;bool created=false,existed=false;
   // Step 2 (pre-commit): any failure leaves the target untouched.
   try{
    using(var fs=new FileStream(temp,FileMode.CreateNew,FileAccess.Write,FileShare.None)){created=true;fs.Write(data,0,data.Length);fs.Flush(true);}
    if(!Same(File.ReadAllBytes(temp),data))throw new IOException("bản tạm đọc lại không khớp");
    Fail(AfterTemp);
    NoReparse(full);if(Directory.Exists(full))throw new IOException(full+" vừa trở thành thư mục");
    existed=File.Exists(full);}
   catch(InvalidOperationException ex){throw new InvalidOperationException(ex.Message+Clean(temp,created));}
   catch(Exception ex){throw new InvalidOperationException("Chưa ghi CSV ("+ex.Message+"). Tệp "+name+" đang có (nếu có) giữ nguyên."+Clean(temp,created));}
   // Step 3 (commit): replace with a kept backup, or move into a free name (never overwrite).
   try{if(existed){backup=Path.Combine(dir,name+".bak_"+tag);File.Replace(temp,full,backup,false);}else File.Move(temp,full);}
   catch(Exception ex){bool target=File.Exists(full),old=backup!=null&&File.Exists(backup),fresh=File.Exists(temp);
    if(existed&&!target)throw new InvalidOperationException("Chưa thay xong CSV ("+ex.Message+"): tệp "+name+" hiện không có ở thư mục đích"+
     (old?"; bản cũ đã chuyển sang "+backup:"")+(fresh?"; bản mới còn ở "+temp:"")+". Kiểm tra/đổi tên thủ công; MEPS không xoá tệp nào.");
    if(existed&&old&&target)throw new InvalidOperationException("Chưa xác nhận thay CSV ("+ex.Message+"): tệp "+full+" và bản cũ "+backup+" cùng tồn tại"+(fresh?"; bản mới còn ở "+temp:"")+
     ". Kiểm tra thủ công; MEPS không xoá tệp nào.");
    throw new InvalidOperationException("Chưa ghi CSV ("+ex.Message+(existed?"; nếu tệp đang mở trong Excel/chương trình khác hãy đóng rồi xuất lại; ổ mạng/USB có thể không hỗ trợ thay thế an toàn — MEPS không ghi đè trực tiếp":
     "; có thể tệp cùng tên vừa xuất hiện — MEPS không ghi đè")+"). Tệp "+name+(existed?" đang có giữ nguyên.":" không bị ghi.")+Clean(temp,created&&fresh));}
   // Step 4: only an exact readback counts as written.
   string kept=backup!=null?" Bản cũ giữ tại "+backup+".":"";byte[] back;
   try{back=File.ReadAllBytes(full);}catch(Exception ex){throw new InvalidOperationException("Đã chuyển CSV vào "+full+" nhưng chưa đọc lại được ("+ex.Message+"); chưa xác nhận đã xuất."+kept);}
   if(!Same(back,data))throw new InvalidOperationException("Đã chuyển CSV vào "+full+" nhưng đọc lại KHÔNG khớp; chưa xác nhận đã xuất."+kept);
   return new RouteWrite034{path=full,backup=backup,replaced=existed,bytes=data.Length,
    message="Đã ghi "+name+" (UTF-8 có BOM, "+data.Length.ToString(Inv)+" byte, đọc lại khớp)."+kept};}
 }
}
#endif
