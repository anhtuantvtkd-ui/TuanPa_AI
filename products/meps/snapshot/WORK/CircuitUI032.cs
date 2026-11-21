using System;using System.Drawing;using System.Linq;using System.Windows.Forms;
#if CIRCUIT032
using Autodesk.AutoCAD.EditorInput;using Document=Autodesk.AutoCAD.ApplicationServices.Document;using App=Autodesk.AutoCAD.ApplicationServices.Application;
using ObjectId=Autodesk.AutoCAD.DatabaseServices.ObjectId;using TypedValue=Autodesk.AutoCAD.DatabaseServices.TypedValue;using DxfCode=Autodesk.AutoCAD.DatabaseServices.DxfCode;
namespace Tvxd.Meps {
 // TVXD-MEPS-METADATA-032 (circuit tags): owned compact dark dialog "Gán / xem mã lộ" (fixed size, shown through AutoCAD's ShowModalDialog). Shows the selected
 // count, the immutable type codes (read-only), the drawing summary, and PanelId / CircuitId / category. Uniform existing tags prefill; mixed = blank plus a
 // hint (opening never overwrites). Lưu only validates and returns Value; the DWG write happens afterwards in the bound command (CircuitCad032.Apply).
 // Sizes are logical px × dpi with pixel fonts (AutoScaleMode.None), colours as MetadataDialog032. Public members for UI QA.
 public sealed class CircuitDialog032:Form {
  public const string InfoText="Lưu ghi mã vào từng block đã chọn trong bản vẽ: U/UNDO hoàn tác cả bước; chưa tự lưu DWG. Mã lộ là nhãn bạn nhập, không phải kiểm tra thiết kế điện. "+
   "COPY/WBLOCK mang theo mã; EXPLODE làm mất mã. Chuyển block sang lộ khác: chọn lại và gán mã mới.";
  static readonly Color UiBack=Color.FromArgb(0x17,0x21,0x2B),UiField=Color.FromArgb(0x20,0x2D,0x3A),UiLine=Color.FromArgb(0x35,0x46,0x55),UiFore=Color.FromArgb(0xE7,0xEE,0xF5),
   UiMuted=Color.FromArgb(0x8F,0xA0,0xB0),UiTeal=Color.FromArgb(0x46,0xC7,0xB7),UiTealHot=Color.FromArgb(0x6F,0xD8,0xCB),UiHover=Color.FromArgb(0x26,0x40,0x4A),ErrorColor=Color.FromArgb(0xFF,0x8C,0x6E);
  public readonly Label CountLabel=new Label(),SummaryLabel=new Label();public readonly Label[] Captions={new Label(),new Label(),new Label()};
  public readonly TextBox TypesBox=new TextBox(),PanelBox=new TextBox(),CircuitBox=new TextBox(),InfoBox=new TextBox(),StatusBox=new TextBox();
  public readonly ComboBox CategoryBox=new ComboBox();
  public readonly Button SaveBtn=new Button(),CancelBtn=new Button();
  public readonly CircuitSelection032 Selection;public CircuitTag032 Value;
  public bool Mixed{get{return mixed;}}readonly bool mixed;
  readonly float dpi;readonly Font face;
  int S(int px){return Math.Max(1,(int)Math.Round(px*dpi));}
  public CircuitDialog032(CircuitSelection032 sel,float dpi){if(sel==null||sel.items.Count==0)throw new ArgumentNullException("sel");Selection=sel;
   this.dpi=float.IsNaN(dpi)||dpi<=0?1f:Math.Max(1f,Math.Min(3f,dpi));
   Text="MEPS — gán / xem mã lộ";Name="circuit032";FormBorderStyle=FormBorderStyle.FixedDialog;MinimizeBox=false;MaximizeBox=false;ShowInTaskbar=false;StartPosition=FormStartPosition.CenterParent;
   AutoScaleMode=AutoScaleMode.None;face=new Font("Segoe UI",12f*this.dpi,FontStyle.Regular,GraphicsUnit.Pixel);Font=face;ClientSize=new Size(S(380),S(440));BackColor=UiBack;ForeColor=UiFore;
   var pane=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=1,RowCount=12,Margin=new Padding(0),Padding=new Padding(S(10),S(8),S(10),S(8)),BackColor=UiBack,ForeColor=UiFore};pane.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
   // count, types, summary, caption, panel, caption, circuit, caption, category, info+rule, status (rest), buttons
   foreach(int h in new[]{S(22),S(58),S(34),S(20),S(26),S(20),S(26),S(20),S(26),S(76),-100,S(40)})pane.RowStyles.Add(h<0?new RowStyle(SizeType.Percent,-h):new RowStyle(SizeType.Absolute,h));
   CountLabel.Text="Đã chọn "+sel.items.Count+" ký hiệu MEPS. Mã loại (chỉ đọc, không đổi):";CountLabel.Name="circuit032_count";
   SummaryLabel.Text=sel.scan.Summary();SummaryLabel.Name="circuit032_summary";SummaryLabel.ForeColor=UiMuted;
   foreach(var l in new[]{CountLabel,SummaryLabel}){l.Dock=DockStyle.Fill;l.AutoEllipsis=true;l.Margin=new Padding(0);if(l==CountLabel)l.ForeColor=UiFore;}
   var caps=new[]{"Mã tủ (PanelId)","Mã lộ (CircuitId)","Loại lộ"};var names=new[]{"panel","circuit","category"};
   for(int k=0;k<3;k++){var c=Captions[k];c.Text=caps[k];c.Dock=DockStyle.Fill;c.TextAlign=ContentAlignment.BottomLeft;c.Margin=new Padding(0);c.ForeColor=UiMuted;c.Name="circuit032_caption_"+names[k];}
   foreach(var t in new[]{PanelBox,CircuitBox}){t.BackColor=UiField;t.ForeColor=UiFore;t.BorderStyle=BorderStyle.FixedSingle;t.CharacterCasing=CharacterCasing.Upper;t.MaxLength=Circuit032.MaxId;t.Dock=DockStyle.Fill;t.Margin=new Padding(0,S(1),0,S(1));}
   PanelBox.Name="circuit032_panel";PanelBox.AccessibleName="Mã tủ";PanelBox.TabIndex=0;CircuitBox.Name="circuit032_circuit";CircuitBox.AccessibleName="Mã lộ";CircuitBox.TabIndex=1;
   CategoryBox.DropDownStyle=ComboBoxStyle.DropDownList;CategoryBox.FlatStyle=FlatStyle.Flat;CategoryBox.BackColor=UiField;CategoryBox.ForeColor=UiFore;CategoryBox.Dock=DockStyle.Fill;CategoryBox.Margin=new Padding(0,S(1),0,S(1));
   CategoryBox.Name="circuit032_category";CategoryBox.AccessibleName="Loại lộ";CategoryBox.TabIndex=2;CategoryBox.Items.Add("(chưa chọn — chọn rõ loại lộ)");
   foreach(var k in Circuit032.Categories)CategoryBox.Items.Add(Circuit032.CategoryCaption(k)+" ("+k+")");
   foreach(var b in new[]{TypesBox,InfoBox,StatusBox}){b.Multiline=true;b.ReadOnly=true;b.WordWrap=true;b.ScrollBars=ScrollBars.Vertical;b.BorderStyle=BorderStyle.None;b.BackColor=UiBack;b.ForeColor=UiFore;b.TabStop=false;b.Dock=DockStyle.Fill;b.Margin=new Padding(0,S(2),0,0);}
   TypesBox.BackColor=UiField;TypesBox.Text=sel.Types();TypesBox.Name="circuit032_types";TypesBox.AccessibleName="Mã loại";
   InfoBox.Text=Circuit032.Rule+"\r\n"+InfoText;InfoBox.ForeColor=UiMuted;InfoBox.Name="circuit032_info";InfoBox.AccessibleName="Quy tắc nhập";StatusBox.Name="circuit032_status";StatusBox.AccessibleName="Thông báo";
   SaveBtn.Text="Lưu";SaveBtn.Name="circuit032_save";CancelBtn.Text="Hủy";CancelBtn.Name="circuit032_cancel";CancelBtn.DialogResult=DialogResult.Cancel;
   var bar=new FlowLayoutPanel{FlowDirection=System.Windows.Forms.FlowDirection.RightToLeft,Dock=DockStyle.Fill,Margin=new Padding(0),Padding=new Padding(0,S(4),0,0),WrapContents=false,TabIndex=3,BackColor=UiBack};
   foreach(var b in new[]{CancelBtn,SaveBtn}){b.AutoSize=true;b.AutoSizeMode=System.Windows.Forms.AutoSizeMode.GrowAndShrink;b.MinimumSize=new Size(S(72),S(30));b.Margin=new Padding(S(4),0,0,0);b.Font=face;
    bool main=b==SaveBtn;b.FlatStyle=FlatStyle.Flat;b.UseVisualStyleBackColor=false;b.BackColor=main?UiTeal:UiField;b.ForeColor=main?UiBack:UiFore;
    b.FlatAppearance.BorderColor=main?UiTeal:UiLine;b.FlatAppearance.MouseOverBackColor=main?UiTealHot:UiHover;b.FlatAppearance.MouseDownBackColor=main?UiTealHot:UiHover;bar.Controls.Add(b);}
   SaveBtn.TabIndex=0;CancelBtn.TabIndex=1;
   var rows=new Control[]{CountLabel,TypesBox,SummaryLabel,Captions[0],PanelBox,Captions[1],CircuitBox,Captions[2],CategoryBox,InfoBox,StatusBox,bar};for(int r=0;r<rows.Length;r++)pane.Controls.Add(rows[r],0,r);
   Controls.Add(pane);SaveBtn.Click+=(s,e)=>TryAccept();CancelButton=CancelBtn;
   bool mx;var pre=Circuit032.Uniform(sel.items,out mx);mixed=mx;PanelBox.Text=pre.panel;CircuitBox.Text=pre.circuit;
   CategoryBox.SelectedIndex=pre.category==null?0:1+Array.IndexOf(Circuit032.Categories,pre.category);ActiveControl=PanelBox;
   int tagged=sel.items.Count(x=>x.tag!=null);
   Status(mixed?"Các block đã chọn đang có mã lộ khác nhau hoặc có block chưa gán: ô khác nhau để trống. Lưu sẽ gán CÙNG một mã tủ / lộ / loại cho cả "+sel.items.Count+" block; Hủy để giữ nguyên.":
    tagged>0?"Đang hiện mã lộ hiện có của "+tagged+" block. Sửa rồi Lưu để gán lại; Hủy để giữ nguyên.":sel.items.Count+" block chưa gán lộ. Nhập mã tủ, mã lộ, chọn loại lộ rồi Lưu.",false);}
  void Status(string text,bool error){StatusBox.ForeColor=error?ErrorColor:UiFore;StatusBox.Text=text??"";}
  // Validates the input (rule, explicit category, conflict with tagged symbols outside the selection). false = dialog stays open with the reason; nothing written.
  public bool TryAccept(){try{int k=CategoryBox.SelectedIndex;var t=Circuit032.Make(PanelBox.Text,CircuitBox.Text,k>=1&&k<=Circuit032.Categories.Length?Circuit032.Categories[k-1]:null);
    string clash=Circuit032.CategoryConflict(Selection.others,t);if(clash!=null)throw new InvalidOperationException(clash);Value=t;}
   catch(Exception ex){Status(ex.Message,true);return false;}
   DialogResult=DialogResult.OK;return true;}
  protected override void Dispose(bool disposing){base.Dispose(disposing);if(disposing)face.Dispose();}
 }
 public sealed class CircuitOutcome032 {public bool changed,cancelled,warn;public int count;public string message;}
 // Runs INSIDE the bound TVXDMEPS060DO command (Plugin.RunCircuit): selection → validation (all-or-none) → dialog → Apply in one transaction / one UNDO step.
 // guard = the plugin's context check (same document, Model, UNDO on), called before/after every prompt and dialog and right before Commit.
 public static class CircuitUi032 {
  public const string Assign="circuitassign",Unassigned="circuitunassigned",AssignCaption="Gán / xem mã lộ…",UnassignedCaption="Chọn block chưa gán lộ";
  public static bool Handles(string action){return action==Assign||action==Unassigned;}
  // QA hooks for the native harness (no keyboard): Select returns the picked ObjectIds (null = Esc); Show displays the dialog and returns its result.
  // Defaults: Editor.GetSelection with an INSERT filter (locked-layer objects stay in the set so they are reported, not skipped) and Application.ShowModalDialog.
  public static Func<Document,ObjectId[]> Select=DefaultSelect;
  public static Func<CircuitDialog032,DialogResult> Show=f=>App.ShowModalDialog(f);
  public static ObjectId[] DefaultSelect(Document d){var o=new PromptSelectionOptions{MessageForAdding="\nChọn các block MEPS cần gán / xem mã lộ (Enter kết thúc, ESC huỷ): ",RejectObjectsOnLockedLayers=false};
   var r=d.Editor.GetSelection(o,new SelectionFilter(new[]{new TypedValue((int)DxfCode.Start,"INSERT")}));
   if(r.Status==PromptStatus.Cancel)return null;return r.Status==PromptStatus.OK&&r.Value!=null?r.Value.GetObjectIds():new ObjectId[0];}
  public static CircuitOutcome032 Run(string action,Document d,Action<string> guard,float dpi){var db=d.Database;
   if(action==Unassigned){
    // Read-only: selects (pickfirst) the supported Model symbols without a tag; nothing is written.
    guard("trước khi tìm block chưa gán");var ids=CircuitCad032.UnassignedIds(db);string summary=CircuitCad032.Scan(db).Summary();
    if(ids.Count==0)return new CircuitOutcome032{message="Không có ký hiệu MEPS nào chưa gán lộ trong Model. "+summary};
    d.Editor.SetImpliedSelection(ids.ToArray());return new CircuitOutcome032{count=ids.Count,message="Đã chọn "+ids.Count+" ký hiệu chưa gán lộ (chỉ chọn, không sửa). "+summary};}
   if(action!=Assign)throw new InvalidOperationException("Thao tác mã lộ không hợp lệ.");
   guard("trước khi chọn block");var picked=Select(d);guard("sau khi chọn block");
   if(picked==null)return new CircuitOutcome032{cancelled=true,message="Đã huỷ gán mã lộ; bản vẽ không đổi."};
   if(picked.Length==0)return new CircuitOutcome032{cancelled=true,message="Chưa chọn block nào; bản vẽ không đổi."};
   var sel=CircuitCad032.Snapshot(db,picked);CircuitTag032 want;
   using(var dlg=new CircuitDialog032(sel,dpi)){var r=Show(dlg);guard("sau hộp gán mã lộ");
    if(r!=DialogResult.OK||dlg.Value==null)return new CircuitOutcome032{cancelled=true,message="Đã huỷ gán mã lộ; bản vẽ không đổi."};want=dlg.Value;}
   guard("ngay trước khi ghi mã lộ");var res=CircuitCad032.Apply(db,sel,want,guard);
   return new CircuitOutcome032{changed=res.changed,warn=res.warn,count=res.written,message=res.message};}
 }
}
#endif
