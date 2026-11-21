using System;using System.Drawing;using System.Linq;using System.Windows.Forms;
#if METADATA032
namespace Tvxd.Meps {
 // TVXD-MEPS-METADATA-032: dialog of "Sửa thông tin bảng ký hiệu…" (palette right-click on a tile), shown through the palette's ShowModal
 // (Application.ShowModalDialog in AutoCAD; owned, never a standalone MessageBox). Block name/ID read-only; DIỄN GIẢI / QUY CÁCH / GHI CHÚ multiline Unicode,
 // blanks allowed, initial values = the cells exported NOW. Lưu = LegendMetadata032.Save (errors stay in the dialog, which does not close after a failed
 // save); Hủy / Esc / close = nothing written; "Dùng lại nội dung gốc" refills the source cells and Lưu then removes only this entry's record.
 // Sizes are logical px × dpi with pixel fonts (AutoScaleMode.None, like CompactView06), so 100/125/150 % lay out as one piece. Public members for UI QA.
 // F032-03: dark neutral theme of the approved palette preview (same values as CompactView06 Ui* of UI031, repeated here so the dialog does not depend on
 // UI031): dark panel and inputs, light text, flat buttons, teal Lưu. Layout, sizes and behaviour unchanged.
 public sealed class MetadataDialog032:Form {
  public const string InfoText="• Lưu theo thư viện: mọi bản vẽ dùng thư viện này đều nhận.\r\n• Chỉ bảng ký hiệu TẠO MỚI dùng chữ này; bảng đã đặt không đổi.\r\n"+
   "• Ghi file thư viện: UNDO của CAD không hoàn tác.\r\n• Trả lại chữ gốc: 'Dùng lại nội dung gốc' → Lưu. Ô trống = cố ý để trống.";
  static readonly Color UiBack=Color.FromArgb(0x17,0x21,0x2B),UiField=Color.FromArgb(0x20,0x2D,0x3A),UiLine=Color.FromArgb(0x35,0x46,0x55),UiFore=Color.FromArgb(0xE7,0xEE,0xF5),
   UiMuted=Color.FromArgb(0x8F,0xA0,0xB0),UiTeal=Color.FromArgb(0x46,0xC7,0xB7),UiTealHot=Color.FromArgb(0x6F,0xD8,0xCB),UiHover=Color.FromArgb(0x26,0x40,0x4A);
  public readonly Label NameLabel=new Label();public readonly Label[] Captions={new Label(),new Label(),new Label()};
  public readonly TextBox Description=new TextBox(),Spec=new TextBox(),Note=new TextBox(),InfoBox=new TextBox(),StatusBox=new TextBox();
  public readonly Button SaveBtn=new Button(),ResetBtn=new Button(),CancelBtn=new Button();
  public readonly MetadataSnapshot032 Snapshot;public MetadataResult032 Result;
  public bool ResetPending{get{return resetPending;}}bool resetPending;
  public TextBox[] Fields{get{return new[]{Description,Spec,Note};}}
  static readonly Color ErrorColor=Color.FromArgb(0xFF,0x8C,0x6E);
  readonly string[] shown=new string[3];readonly float dpi;readonly Font face;
  int S(int px){return Math.Max(1,(int)Math.Round(px*dpi));}
  // TextBox line breaks are CRLF; the stored cells use LF.
  static string Display(string s){return (s??"").Replace("\r\n","\n").Replace("\n","\r\n");}
  public MetadataDialog032(MetadataSnapshot032 snap,float dpi){if(snap==null||snap.entry==null)throw new ArgumentNullException("snap");Snapshot=snap;
   this.dpi=float.IsNaN(dpi)||dpi<=0?1f:Math.Max(1f,Math.Min(3f,dpi));
   Text="MEPS — sửa thông tin bảng ký hiệu";Name="metadata032";FormBorderStyle=FormBorderStyle.FixedDialog;MinimizeBox=false;MaximizeBox=false;ShowInTaskbar=false;StartPosition=FormStartPosition.CenterParent;
   AutoScaleMode=AutoScaleMode.None;face=new Font("Segoe UI",12f*this.dpi,FontStyle.Regular,GraphicsUnit.Pixel);Font=face;ClientSize=new Size(S(480),S(540));BackColor=UiBack;ForeColor=UiFore;
   var pane=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=1,RowCount=10,Margin=new Padding(0),Padding=new Padding(S(10),S(8),S(10),S(8)),BackColor=UiBack,ForeColor=UiFore};pane.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
   // name, caption, field, caption, field, caption, field, info, status, buttons (negative = percent of the rest)
   foreach(int h in new[]{S(40),S(22),-34,S(22),-33,S(22),-33,S(82),S(44),S(42)})pane.RowStyles.Add(h<0?new RowStyle(SizeType.Percent,-h):new RowStyle(SizeType.Absolute,h));
   NameLabel.Text="Ký hiệu: "+snap.entry.name+"  (mục "+snap.entry.id+")\r\nTên khối chỉ đọc; chỉ sửa chữ của bảng ký hiệu.";NameLabel.Dock=DockStyle.Fill;NameLabel.AutoEllipsis=true;NameLabel.Margin=new Padding(0);NameLabel.Name="metadata032_name";NameLabel.ForeColor=UiFore;
   pane.Controls.Add(NameLabel,0,0);
   var names=new[]{"description","spec","note"};
   for(int k=0;k<3;k++){var c=Captions[k];c.Dock=DockStyle.Fill;c.TextAlign=ContentAlignment.BottomLeft;c.AutoEllipsis=true;c.Margin=new Padding(0);c.Name="metadata032_caption_"+names[k];c.ForeColor=UiMuted;
    var t=Fields[k];t.BackColor=UiField;t.ForeColor=UiFore;t.BorderStyle=BorderStyle.FixedSingle;t.Multiline=true;t.AcceptsReturn=true;t.AcceptsTab=false;t.WordWrap=true;t.ScrollBars=ScrollBars.Vertical;t.Dock=DockStyle.Fill;t.Margin=new Padding(0,S(2),0,S(2));
    t.Name="metadata032_"+names[k];t.AccessibleName=Legend.CellNames[k];t.TabIndex=k;pane.Controls.Add(c,0,1+2*k);pane.Controls.Add(t,0,2+2*k);}
   foreach(var b in new[]{InfoBox,StatusBox}){b.Multiline=true;b.ReadOnly=true;b.WordWrap=true;b.ScrollBars=ScrollBars.Vertical;b.BorderStyle=BorderStyle.None;b.BackColor=UiBack;b.ForeColor=UiFore;b.TabStop=false;b.Dock=DockStyle.Fill;b.Margin=new Padding(0,S(4),0,0);}
   InfoBox.Text=InfoText;InfoBox.ForeColor=UiMuted;InfoBox.Name="metadata032_info";InfoBox.AccessibleName="Lưu ý";StatusBox.Name="metadata032_status";StatusBox.AccessibleName="Thông báo";
   pane.Controls.Add(InfoBox,0,7);pane.Controls.Add(StatusBox,0,8);
   SaveBtn.Text="Lưu";SaveBtn.Name="metadata032_save";ResetBtn.Text="Dùng lại nội dung gốc";ResetBtn.Name="metadata032_reset";CancelBtn.Text="Hủy";CancelBtn.Name="metadata032_cancel";CancelBtn.DialogResult=DialogResult.Cancel;
   var bar=new FlowLayoutPanel{FlowDirection=System.Windows.Forms.FlowDirection.RightToLeft,Dock=DockStyle.Fill,Margin=new Padding(0),Padding=new Padding(0,S(4),0,0),WrapContents=false,TabIndex=3,BackColor=UiBack};
   foreach(var b in new[]{CancelBtn,SaveBtn,ResetBtn}){b.AutoSize=true;b.AutoSizeMode=System.Windows.Forms.AutoSizeMode.GrowAndShrink;b.MinimumSize=new Size(S(72),S(30));b.Margin=new Padding(S(4),0,0,0);b.Font=face;
    bool main=b==SaveBtn;b.FlatStyle=FlatStyle.Flat;b.UseVisualStyleBackColor=false;b.BackColor=main?UiTeal:UiField;b.ForeColor=main?UiBack:UiFore;
    b.FlatAppearance.BorderColor=main?UiTeal:UiLine;b.FlatAppearance.MouseOverBackColor=main?UiTealHot:UiHover;b.FlatAppearance.MouseDownBackColor=main?UiTealHot:UiHover;bar.Controls.Add(b);}
   SaveBtn.TabIndex=0;ResetBtn.TabIndex=1;CancelBtn.TabIndex=2;pane.Controls.Add(bar,0,9);Controls.Add(pane);
   SaveBtn.Click+=(s,e)=>TryAccept();ResetBtn.Click+=(s,e)=>UseSource();CancelButton=CancelBtn;
   for(int k=0;k<3;k++){shown[k]=Display(snap.effective[k]);Fields[k].Text=shown[k];}
   Captioned();ActiveControl=Description;
   if(snap.effectiveStatus==LegendMetadata032.Invalid)Status(snap.effectiveReason,true);else Status("Trạng thái khi tạo bảng hiện nay: "+snap.effectiveStatus+".",false);}
  void Status(string text,bool error){StatusBox.ForeColor=error?ErrorColor:UiFore;StatusBox.Text=text??"";}
  void Captioned(){var cur=Snapshot.current;
   for(int k=0;k<3;k++){bool empty=string.IsNullOrEmpty(Snapshot.source[k]);string origin=resetPending?"sẽ dùng lại nội dung nguồn"+(empty?" (trống)":""):
     Snapshot.overridden[k]?"đã sửa tay"+(cur!=null?" (lần "+cur.revision+")":""):empty?"theo nguồn (nguồn trống hoặc không chắc chắn)":"theo bảng nguồn";
    Captions[k].Text=Legend.CellNames[k]+" — "+origin;}}
  // "Dùng lại nội dung gốc": refills the source cells; nothing is written until Lưu.
  public void UseSource(){resetPending=true;for(int k=0;k<3;k++){shown[k]=Display(Snapshot.source[k]);Fields[k].Text=shown[k];}Captioned();
   Status("Bấm Lưu để bỏ phần sửa tay và dùng lại nội dung gốc cho riêng ký hiệu này; Hủy để giữ nguyên.",false);}
  // A cell the user did not change and that has no record stays "no override" (source text, null). Changed cells, and cells that already had a record, are
  // saved literally (cleaned). After UseSource, only cells changed again become records. Returns false (dialog stays open, error shown) on any failure.
  public bool TryAccept(){var fields=new string[3];
   try{for(int k=0;k<3;k++){string t=Fields[k].Text??"";bool touched=!string.Equals(t,shown[k],StringComparison.Ordinal);
     if(!resetPending&&Snapshot.overridden[k]){fields[k]=LegendMetadata032.Clean(t,Legend.CellNames[k]);continue;}
     if(!touched)continue;string c=LegendMetadata032.Clean(t,Legend.CellNames[k]);fields[k]=string.Equals(c,Snapshot.source[k]??"",StringComparison.Ordinal)?null:c;}
    Result=LegendMetadata032.Save(Snapshot,fields);}
   catch(Exception ex){Status(ex.Message,true);return false;}
   DialogResult=DialogResult.OK;return true;}
  protected override void Dispose(bool disposing){base.Dispose(disposing);if(disposing)face.Dispose();}
 }
}
#endif
