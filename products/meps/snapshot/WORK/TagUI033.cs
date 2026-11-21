using System;using System.Drawing;using System.Linq;using System.Globalization;using System.Collections.Generic;using System.Windows.Forms;
#if TAG033
using Autodesk.AutoCAD.EditorInput;using Document=Autodesk.AutoCAD.ApplicationServices.Document;using App=Autodesk.AutoCAD.ApplicationServices.Application;
using ObjectId=Autodesk.AutoCAD.DatabaseServices.ObjectId;using TypedValue=Autodesk.AutoCAD.DatabaseServices.TypedValue;using DxfCode=Autodesk.AutoCAD.DatabaseServices.DxfCode;
using Database=Autodesk.AutoCAD.DatabaseServices.Database;using OpenMode=Autodesk.AutoCAD.DatabaseServices.OpenMode;using BlockReference=Autodesk.AutoCAD.DatabaseServices.BlockReference;
namespace Tvxd.Meps {
 // TVXD-MEPS-TAG-033 UI: owned compact dark dialog "TAG — nhãn hiển thị" (fixed 380 × 330 logical px, shown through AutoCAD's ShowModalDialog) over the
 // native-proven core (TagCad033). Shows the selected count and block names (read-only), the label text and the text height in DRAWING UNITS.
 //  - Single block: prefilled with its actual label text/height, or (no label yet) empty text and the core's suggested height.
 //  - Several blocks: "Đổi tên cho tất cả" (default unchecked = every label keeps its own text; the text box is disabled). If any selected block has no label
 //    yet, a text is required for it and the core applies ONE text to every selected block, so the box is forced checked and the dialog says so.
 //    Height: one value for all; empty = keep each label's height (only when every block already has a label).
 // Lưu only validates (same Tag033.CheckText / CheckHeight as the core) and returns Text / Height; the DWG write happens afterwards in the bound command
 // (TagCad033.Apply: one transaction, one U). Hủy/Esc = nothing written. Label is a display text, not a circuit relation (Circuit032 is separate).
 public sealed class TagDialog033:Form {
  public const string InfoText="Nhãn là thuộc tính hiển thị gắn vào block (không phải quan hệ lộ/mạch). Dời riêng nhãn: chọn block rồi kéo grip của nhãn (hoặc Ctrl+click nhãn rồi MOVE). "+
   "MOVE/COPY block mang theo nhãn; sửa chữ/cỡ bằng Properties cũng được. Khối động (ví dụ công tắc đôi): đổi tham số động (kéo dãn, lật, hiển thị…) nhãn KHÔNG tự dời theo, kéo grip nhãn nếu cần. "+
   "Lưu: một bước U/UNDO; chưa tự lưu DWG — lưu DWG để giữ. ATTSYNC/BATTMAN Sync hoặc EXPLODE có thể làm mất nhãn.";
  public const string AllCaption="Đổi tên cho tất cả",HeightCaption="Chiều cao chữ (đơn vị bản vẽ)",TextCaption="Chữ nhãn";
  static readonly Color UiBack=Color.FromArgb(0x17,0x21,0x2B),UiField=Color.FromArgb(0x20,0x2D,0x3A),UiLine=Color.FromArgb(0x35,0x46,0x55),UiFore=Color.FromArgb(0xE7,0xEE,0xF5),
   UiMuted=Color.FromArgb(0x8F,0xA0,0xB0),UiTeal=Color.FromArgb(0x46,0xC7,0xB7),UiTealHot=Color.FromArgb(0x6F,0xD8,0xCB),UiHover=Color.FromArgb(0x26,0x40,0x4A),ErrorColor=Color.FromArgb(0xFF,0x8C,0x6E);
  public readonly Label CountLabel=new Label(),TextLabel=new Label(),HeightLabel=new Label();
  public readonly TextBox NamesBox=new TextBox(),LabelBox=new TextBox(),HeightBox=new TextBox(),InfoBox=new TextBox(),StatusBox=new TextBox();
  public readonly CheckBox AllBox=new CheckBox();
  public readonly Button SaveBtn=new Button(),CancelBtn=new Button();
  public readonly TagSnapshot033 Selection;
  // Result after Lưu: Text null = keep each label's text; Height null = keep each height.
  public string Text033;public double? Height033;
  readonly float dpi;readonly Font face;
  int S(int px){return Math.Max(1,(int)Math.Round(px*dpi));}
  static string F(double v){return v.ToString("0.###",CultureInfo.InvariantCulture);}
  public TagDialog033(TagSnapshot033 sel,string names,float dpi){if(sel==null||sel.items.Count==0)throw new ArgumentNullException("sel");Selection=sel;
   this.dpi=float.IsNaN(dpi)||dpi<=0?1f:Math.Max(1f,Math.Min(3f,dpi));
   Text="MEPS — TAG (nhãn hiển thị)";Name="tag033";FormBorderStyle=FormBorderStyle.FixedDialog;MinimizeBox=false;MaximizeBox=false;ShowInTaskbar=false;StartPosition=FormStartPosition.CenterParent;
   AutoScaleMode=AutoScaleMode.None;face=new Font("Segoe UI",12f*this.dpi,FontStyle.Regular,GraphicsUnit.Pixel);Font=face;ClientSize=new Size(S(380),S(330));BackColor=UiBack;ForeColor=UiFore;
   var pane=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=1,RowCount=10,Margin=new Padding(0),Padding=new Padding(S(10),S(8),S(10),S(8)),BackColor=UiBack,ForeColor=UiFore};pane.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
   // count, names, text caption, text, all-checkbox, height caption, height, info, status (rest), buttons
   foreach(int h in new[]{S(20),S(40),S(20),S(26),S(22),S(20),S(26),S(58),-100,S(40)})pane.RowStyles.Add(h<0?new RowStyle(SizeType.Percent,-h):new RowStyle(SizeType.Absolute,h));
   int n=sel.items.Count,tagged=sel.Tagged,fresh=sel.Untagged;
   CountLabel.Text="Đã chọn "+n+" block ("+tagged+" đã có nhãn, "+fresh+" chưa có):";CountLabel.Name="tag033_count";
   TextLabel.Text=TextCaption;TextLabel.Name="tag033_caption_text";HeightLabel.Text=HeightCaption;HeightLabel.Name="tag033_caption_height";
   foreach(var l in new[]{CountLabel,TextLabel,HeightLabel}){l.Dock=DockStyle.Fill;l.AutoEllipsis=true;l.Margin=new Padding(0);l.ForeColor=l==CountLabel?UiFore:UiMuted;if(l!=CountLabel)l.TextAlign=ContentAlignment.BottomLeft;}
   foreach(var t in new[]{LabelBox,HeightBox}){t.BackColor=UiField;t.ForeColor=UiFore;t.BorderStyle=BorderStyle.FixedSingle;t.Dock=DockStyle.Fill;t.Margin=new Padding(0,S(1),0,S(1));}
   LabelBox.MaxLength=Tag033.MaxText;LabelBox.Name="tag033_text";LabelBox.AccessibleName=TextCaption;LabelBox.TabIndex=0;
   AllBox.Text=AllCaption;AllBox.Name="tag033_all";AllBox.AccessibleName=AllCaption;AllBox.Dock=DockStyle.Fill;AllBox.Margin=new Padding(0);AllBox.ForeColor=UiFore;AllBox.TabIndex=1;
   HeightBox.Name="tag033_height";HeightBox.AccessibleName=HeightCaption;HeightBox.TabIndex=2;
   foreach(var b in new[]{NamesBox,InfoBox,StatusBox}){b.Multiline=true;b.ReadOnly=true;b.WordWrap=true;b.ScrollBars=ScrollBars.Vertical;b.BorderStyle=BorderStyle.None;b.BackColor=UiBack;b.ForeColor=UiFore;b.TabStop=false;b.Dock=DockStyle.Fill;b.Margin=new Padding(0,S(2),0,0);}
   NamesBox.BackColor=UiField;NamesBox.Text=names??"";NamesBox.Name="tag033_names";NamesBox.AccessibleName="Tên block (chỉ đọc)";
   InfoBox.Text=InfoText+"\r\n"+Tag033.Rule;InfoBox.ForeColor=UiMuted;InfoBox.Name="tag033_info";InfoBox.AccessibleName="Hướng dẫn";StatusBox.Name="tag033_status";StatusBox.AccessibleName="Thông báo";
   SaveBtn.Text="Lưu";SaveBtn.Name="tag033_save";CancelBtn.Text="Hủy";CancelBtn.Name="tag033_cancel";CancelBtn.DialogResult=DialogResult.Cancel;
   var bar=new FlowLayoutPanel{FlowDirection=System.Windows.Forms.FlowDirection.RightToLeft,Dock=DockStyle.Fill,Margin=new Padding(0),Padding=new Padding(0,S(4),0,0),WrapContents=false,TabIndex=3,BackColor=UiBack};
   foreach(var b in new[]{CancelBtn,SaveBtn}){b.AutoSize=true;b.AutoSizeMode=System.Windows.Forms.AutoSizeMode.GrowAndShrink;b.MinimumSize=new Size(S(72),S(30));b.Margin=new Padding(S(4),0,0,0);b.Font=face;
    bool main=b==SaveBtn;b.FlatStyle=FlatStyle.Flat;b.UseVisualStyleBackColor=false;b.BackColor=main?UiTeal:UiField;b.ForeColor=main?UiBack:UiFore;
    b.FlatAppearance.BorderColor=main?UiTeal:UiLine;b.FlatAppearance.MouseOverBackColor=main?UiTealHot:UiHover;b.FlatAppearance.MouseDownBackColor=main?UiTealHot:UiHover;bar.Controls.Add(b);}
   SaveBtn.TabIndex=0;CancelBtn.TabIndex=1;
   var rows=new Control[]{CountLabel,NamesBox,TextLabel,LabelBox,AllBox,HeightLabel,HeightBox,InfoBox,StatusBox,bar};for(int r=0;r<rows.Length;r++)pane.Controls.Add(rows[r],0,r);
   Controls.Add(pane);SaveBtn.Click+=(s,e)=>TryAccept();CancelButton=CancelBtn;AllBox.CheckedChanged+=(s,e)=>{LabelBox.Enabled=AllBox.Checked;};
   // Prefill from the actual labels (never written by opening the dialog).
   string common=sel.CommonText();double? ch=sel.CommonHeight();
   if(n==1){AllBox.Checked=true;AllBox.Visible=false;}else if(fresh>0){AllBox.Checked=true;AllBox.Enabled=false;}else AllBox.Checked=false;
   LabelBox.Enabled=AllBox.Checked;LabelBox.Text=common??"";
   double? sug=fresh>0?sel.items.Where(x=>!x.hasTag).Select(x=>x.suggested).FirstOrDefault(x=>x.HasValue):null;
   HeightBox.Text=ch.HasValue?F(ch.Value):sug.HasValue?F(sug.Value):"";ActiveControl=LabelBox.Enabled?(Control)LabelBox:HeightBox;
   Status(n==1?(tagged==1?"Đang hiện chữ và chiều cao hiện có của nhãn. Sửa rồi Lưu; Hủy để giữ nguyên.":"Block chưa có nhãn. Nhập chữ và chiều cao (đã gợi ý theo kích thước block) rồi Lưu."):
    fresh>0?fresh+" block chưa có nhãn: chữ nhập vào sẽ dùng cho CẢ "+n+" block (kể cả block đã có nhãn). Chiều cao: một giá trị cho tất cả.":
    "Chiều cao: nhập một giá trị cho tất cả, để trống = giữ cỡ từng nhãn. Chữ: tích '"+AllCaption+"' mới đổi; không tích = giữ chữ riêng của từng nhãn.",false);}
  void Status(string text,bool error){StatusBox.ForeColor=error?ErrorColor:UiFore;StatusBox.Text=text??"";}
  // Height text → value: empty = null; "2.5" and "2,5" accepted (one decimal mark, no thousands separator); anything else = exception.
  public static double? ParseHeight(string s){s=(s??"").Trim();if(s.Length==0)return null;if(s.Count(c=>c==','||c=='.')>1)throw new InvalidOperationException("Chiều cao \""+s+"\" không hợp lệ (chỉ một dấu thập phân).");
   double v;if(!double.TryParse(s.Replace(',','.'),NumberStyles.AllowLeadingSign|NumberStyles.AllowDecimalPoint,CultureInfo.InvariantCulture,out v))throw new InvalidOperationException("Chiều cao \""+s+"\" không phải số.");
   return Tag033.CheckHeight(v);}
  // Validates with the core rules; false = dialog stays open with the reason; nothing written.
  public bool TryAccept(){try{string t=AllBox.Checked?Tag033.CheckText(LabelBox.Text):null;double? h=ParseHeight(HeightBox.Text);
    if(Selection.Untagged>0&&(t==null||!h.HasValue))throw new InvalidOperationException("Block chưa có nhãn cần đủ chữ và chiều cao.");
    if(t==null&&!h.HasValue)throw new InvalidOperationException("Chưa có gì để đổi: tích '"+AllCaption+"' để đổi chữ hoặc nhập chiều cao.");Text033=t;Height033=h;}
   catch(Exception ex){Status(ex.Message,true);return false;}
   DialogResult=DialogResult.OK;return true;}
  protected override void Dispose(bool disposing){base.Dispose(disposing);if(disposing)face.Dispose();}
 }
 public sealed class TagOutcome033 {public bool changed,cancelled,warn;public int count;public string message;}
 // Runs INSIDE the bound TVXDMEPS060DO command (Plugin.RunTag): selection → TagCad033.Snapshot (all-or-none) → dialog → TagCad033.Apply (one transaction /
 // one UNDO step). guard = the plugin's context check (same document, Model, UNDO on), before/after every prompt and dialog and right before Commit.
 public static class TagUi033 {
  public const string Edit="tag033-edit",Caption="TAG";
  public static bool Handles(string action){return action==Edit;}
  // QA hooks for the native harness (synchronous, no keyboard): Select returns the picked ObjectIds (null = Esc); Show displays the dialog and returns its result.
  // Defaults: Editor.GetSelection with an INSERT filter (locked-layer objects stay in the set so they are reported, not skipped) and Application.ShowModalDialog.
  // ResetHooks restores both defaults after QA.
  public static Func<Document,ObjectId[]> Select=DefaultSelect;
  public static Func<TagDialog033,DialogResult> Show=DefaultShow;
  public static DialogResult DefaultShow(TagDialog033 f){return App.ShowModalDialog(f);}
  public static void ResetHooks(){Select=DefaultSelect;Show=DefaultShow;}
  public static ObjectId[] DefaultSelect(Document d){var o=new PromptSelectionOptions{MessageForAdding="\nChọn các block cần gắn / sửa nhãn TAG (Enter kết thúc, ESC huỷ): ",RejectObjectsOnLockedLayers=false};
   var r=d.Editor.GetSelection(o,new SelectionFilter(new[]{new TypedValue((int)DxfCode.Start,"INSERT")}));
   if(r.Status==PromptStatus.Cancel)return null;return r.Status==PromptStatus.OK&&r.Value!=null?r.Value.GetObjectIds():new ObjectId[0];}
  // Read-only block names for the dialog, e.g. "TVXD_E_DEN_01 × 3".
  public static string Names(Database db,TagSnapshot033 sel){var names=new List<string>();
   using(var tr=db.TransactionManager.StartOpenCloseTransaction()){foreach(var it in sel.items){var br=tr.GetObject(it.owner,OpenMode.ForRead) as BlockReference;names.Add(br==null?it.handle:br.Name);}tr.Commit();}
   return string.Join("\r\n",names.GroupBy(x=>x,StringComparer.OrdinalIgnoreCase).OrderBy(g=>g.Key,StringComparer.OrdinalIgnoreCase).Select(g=>g.Key+" × "+g.Count()));}
  public static TagOutcome033 Run(string action,Document d,Action<string> guard,float dpi){if(!Handles(action))throw new InvalidOperationException("Thao tác TAG không hợp lệ.");var db=d.Database;
   guard("trước khi chọn block");var picked=Select(d);guard("sau khi chọn block");
   if(picked==null)return new TagOutcome033{cancelled=true,message="Đã huỷ TAG; bản vẽ không đổi."};
   if(picked.Length==0)return new TagOutcome033{cancelled=true,message="Chưa chọn block nào; bản vẽ không đổi."};
   var sel=TagCad033.Snapshot(db,picked);string text;double? height;
   using(var dlg=new TagDialog033(sel,Names(db,sel),dpi)){var r=Show(dlg);guard("sau hộp TAG");
    if(r!=DialogResult.OK)return new TagOutcome033{cancelled=true,message="Đã huỷ TAG; bản vẽ không đổi."};text=dlg.Text033;height=dlg.Height033;}
   guard("ngay trước khi ghi nhãn");var res=TagCad033.Apply(db,sel,text,height,guard);
   return new TagOutcome033{changed=res.changed,warn=res.warn,count=res.created+res.updated,message=res.message};}
 }
}
#endif
