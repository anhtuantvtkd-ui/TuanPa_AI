using System;using System.Linq;using System.Collections.Generic;using System.Drawing;using System.Windows.Forms;using Autodesk.AutoCAD.ApplicationServices;using Autodesk.AutoCAD.DatabaseServices;using Autodesk.AutoCAD.EditorInput;using Autodesk.AutoCAD.Geometry;using App=Autodesk.AutoCAD.ApplicationServices.Application;
#if MODELLIB019
namespace Tvxd.Meps {
 // TVXD-MEPS-MODELLIB-019 command side of the '+' menu. Runs only inside the queued palette command (Plugin.Execute, bound document, one UNDO step); the
 // caller's guard (same document, Model, INSUNITS, UCS = World, UNDO on) is called before and after every Editor prompt and modal dialog. Esc/cancel at
 // any prompt or dialog writes nothing. Library writes go through ModelLibraryCad019 (checks first, lock, backups, rollback).
 public sealed class ModelOutcome019 {public bool changed,reload,warn,cancelled;public string message,select;}
 public static class ModelLibraryUI019 {
  public const string Add="modeladd",Copy="modelcopy",Remove="modelremove";
#if LIBUPDATE023
  // TVXD-MEPS-LIBUPDATE-023: needs the right-clicked entry (Plugin Job.target) → RunUpdate; Run refuses it without a target.
  public const string Update="modelupdate";
  public static bool Handles(string action){return action==Add||action==Copy||action==Remove||action==Update;}
#else
  public static bool Handles(string action){return action==Add||action==Copy||action==Remove;}
#endif
  static ModelOutcome019 Cancelled(string what){return new ModelOutcome019{cancelled=true,message="Đã huỷ "+what+"; thư viện và bản vẽ giữ nguyên."};}
  // TVXD-MEPS-MODELUI-020 F03: stage(evt, detail) records each step (awaiting selection, selection status + handle, dialog open/result); detail never holds a
  // drawing path/name or source content. A failing sink is ignored (Safe), so logging can neither fail nor change an operation.
  static Action<string,string> Safe(Action<string,string> stage){return (evt,detail)=>{if(stage!=null)try{stage(evt,detail);}catch(Exception){}};}
  // TVXD-MEPS-LIBDELETE-021: explicit focus to the drawing window of THIS bound document right before an Editor prompt, after the active-document guard
  // (Document.Window.Focus, public API since AutoCAD 2014). Not StartUserInteraction (that is for prompts from a modal dialog). A focus failure is logged and
  // the prompt still runs; focus is NOT proof that the user's click reaches the prompt (live acceptance pending).
  static void Focus(Document d,Action<string> guard,Action<string,string> stage,string when){guard(when);
   try{d.Window.Focus();stage("FOCUS","ok");}catch(Exception ex){stage("FOCUS","fail "+ex.GetType().Name);}}
  static ObjectId Pick(Document d,string prompt,Action<string> guard,Action<string,string> stage){var opt=new PromptEntityOptions(prompt);opt.SetRejectMessage("\nChỉ chọn một block (BlockReference).");opt.AddAllowedClass(typeof(BlockReference),false);
   Focus(d,guard,stage,"trước khi chọn block");stage("AWAIT_SELECT","");var r=d.Editor.GetEntity(opt);stage("SELECT",r.Status+(r.Status==PromptStatus.OK?" handle="+r.ObjectId.Handle:""));guard("sau khi chọn block");return r.Status==PromptStatus.OK?r.ObjectId:ObjectId.Null;}
  public static ModelOutcome019 Run(string action,Document d,string root,Action<string> guard,LwOptions lw,Action<string,string> stage=null){var s=Safe(stage);guard("trước khi chọn block");
   if(action==Add)return RunAdd(d,root,guard,lw,s);if(action==Copy)return RunCopy(d,root,guard,s);if(action==Remove)return RunRemove(d,root,guard,s);
   throw new InvalidOperationException("Thao tác không hợp lệ: "+action);}
  static ModelOutcome019 RunAdd(Document d,string root,Action<string> guard,LwOptions lw,Action<string,string> stage){
   var id=Pick(d,"\nChọn MỘT block trong Model để thêm vào thư viện (Esc huỷ): ",guard,stage);if(id.IsNull)return Cancelled("thêm block từ Model");
   var info=ModelLibraryCad019.Inspect(d.Database,id);string name;CaptureCells019 cells;
   using(var dlg=new ModelNameDialog019("Thêm block từ Model vào thư viện",ModelLibraryCad019.DefaultAlias(info),true,n=>Naming019.Check(n,Library.Load(root),null))){
    // 021: a block with any (non-standard) name is accepted as source; only the NEW library name must pass Naming019. The source keeps its name.
    dlg.Info.Text="Block đã chọn: "+info.effectiveName+(info.alias!=null&&info.alias!=info.effectiveName?" (ký hiệu MEPS "+info.alias+")":"")+(info.dynamic?" — khối động: chụp trạng thái đang thấy":"")+". Hình được chụp đúng tỉ lệ/góc xoay/thuộc tính hiện tại; bản vẽ không bị sửa."+
     (Naming019.Shape(info.effectiveName)!=null&&info.alias==null?" Tên block nguồn không theo quy tắc: nhập tên chuẩn mới cho mục thư viện; block nguồn giữ tên cũ.":"");
    stage("DIALOG_OPEN","name");var r=App.ShowModalDialog(dlg);stage("DIALOG_RESULT",r.ToString());guard("sau hộp thoại");if(r!=DialogResult.OK)return Cancelled("thêm block từ Model");name=dlg.Accepted;cells=dlg.Cells;}
   d.Editor.WriteMessage("\nTVXD MEPS: đang chụp "+name+"…");
   var res=ModelLibraryCad019.Capture(d.Database,id,name,cells,root,lw,System.IO.Path.GetFileName(d.Name));
   return new ModelOutcome019{reload=true,select=res.entry.name,message=res.Summary()};}
  static ModelOutcome019 RunCopy(Document d,string root,Action<string> guard,Action<string,string> stage){
   var id=Pick(d,"\nChọn MỘT block để tạo bản sao sửa (Esc huỷ): ",guard,stage);if(id.IsNull)return Cancelled("tạo bản sao");
   var info=ModelLibraryCad019.Inspect(d.Database,id);if(info.dynamic)throw new InvalidOperationException("Khối động chưa tạo được bản sao sửa; dùng '+ → Thêm block từ Model' để chụp trạng thái đang thấy. Chưa thay đổi gì.");
   var names=ModelLibraryCad019.BlockNames(d.Database);string alias=ModelLibraryCad019.DefaultAlias(info);string suggestion=Naming019.Suggest(alias,Library.Load(root),names)??"";string name;
   using(var dlg=new ModelNameDialog019("Tạo bản sao để sửa",suggestion,false,n=>Naming019.Check(n,Library.Load(root),ModelLibraryCad019.BlockNames(d.Database)))){
    dlg.Info.Text="Block gốc: "+info.effectiveName+(info.alias!=null&&info.alias!=info.effectiveName?" (ký hiệu MEPS "+info.alias+")":"")+". Bản sao có định nghĩa riêng, khối gốc giữ nguyên. Tên phải chưa dùng trong thư viện và bản vẽ.";
    stage("DIALOG_OPEN","name");var r=App.ShowModalDialog(dlg);stage("DIALOG_RESULT",r.ToString());guard("sau hộp thoại");if(r!=DialogResult.OK)return Cancelled("tạo bản sao");name=dlg.Accepted;}
   Focus(d,guard,stage,"trước khi chọn điểm");stage("AWAIT_POINT","");var pt=d.Editor.GetPoint(new PromptPointOptions("\nChọn điểm đặt bản sao "+name+" (Esc huỷ): "));stage("POINT",pt.Status.ToString());guard("sau khi chọn điểm");if(pt.Status!=PromptStatus.OK)return Cancelled("tạo bản sao");
   var at=pt.Value.TransformBy(d.Editor.CurrentUserCoordinateSystem);
   var res=ModelLibraryCad019.EditableCopy(d.Database,id,name,at,Library.Load(root));
   try{d.Editor.SetImpliedSelection(new[]{res.reference});}catch(Exception){}
   return new ModelOutcome019{changed=true,message="Đã tạo bản sao độc lập "+name+" tại điểm chọn (đang được chọn). Sửa: nhấp đúp bản sao hoặc lệnh BEDIT → "+name+", sửa bằng lệnh AutoCAD thông thường, BCLOSE lưu; rồi '+ → Thêm block từ Model' chọn bản sao. Khối gốc giữ nguyên; UNDO để bỏ bản sao."};}
  static ModelOutcome019 RunRemove(Document d,string root,Action<string> guard,Action<string,string> stage){
   var id=Pick(d,"\nChọn MỘT ký hiệu đã đặt để xoá mục của nó khỏi công cụ (Esc huỷ): ",guard,stage);if(id.IsNull)return Cancelled("xoá khỏi công cụ");
   var notes=new List<string>();var e=ModelLibraryCad019.ResolveForRemove(d.Database,id,Library.Load(root),notes);
   // 021: the same actual delete as the palette right-click (a hidden entry is deleted too). F02 (020): AutoCAD-owned modal confirmation; default and Esc = Không.
   DialogResult ans;using(var dlg=new ConfirmDeleteDialog021(e.name,e.id)){stage("DIALOG_OPEN","confirm");ans=App.ShowModalDialog(dlg);}stage("DIALOG_RESULT",ans.ToString());
   guard("sau hộp thoại");if(ans!=DialogResult.Yes)return Cancelled("xoá khỏi công cụ");
   var res=LibraryDelete021.Delete(root,e);
   return new ModelOutcome019{reload=true,message=res.Summary()+(notes.Count>0?" "+string.Join(" ",notes):"")};}
#if LIBUPDATE023
  // 023: target = exact snapshot of the right-clicked tile. Read-only prechecks (stale entry, Unitless refused) before the prompt; the selected block, its visible transform
  // and its MEPS link are shown in an owned confirmation (default/Esc = Không); guard after every prompt/dialog; then LibraryUpdate023.Update.
  public static ModelOutcome019 RunUpdate(Document d,string root,LibEntry target,Action<string> guard,LwOptions lw,Action<string,string> stage=null){var s=Safe(stage);guard("trước khi chọn block");
   var now=LibraryUpdate023.Precheck(root,target,d.Database);
   var id=Pick(d,"\nChọn MỘT khối ĐÃ SỬA trên Model để cập nhật hình cho "+now.name+" (Esc huỷ): ",guard,s);if(id.IsNull)return Cancelled("cập nhật hình "+now.name);
   var info=ModelLibraryCad019.Inspect(d.Database,id);string link=LibraryUpdate023.LinkIssue(info,now);if(link!=null)throw new InvalidOperationException("Không cập nhật "+now.name+": "+link+". Chưa thay đổi gì.");
   string transform;using(var tr=d.Database.TransactionManager.StartOpenCloseTransaction()){var br=(BlockReference)tr.GetObject(id,OpenMode.ForRead);var sc=br.ScaleFactors;
    bool plain=Math.Abs(sc.X-1)<1e-9&&Math.Abs(sc.Y-1)<1e-9&&Math.Abs(sc.Z-1)<1e-9&&Math.Abs(br.Rotation)<1e-12;
    transform=plain?"tỉ lệ 1, góc 0":"CHÚ Ý tỉ lệ "+sc.X.ToString("0.####")+"/"+sc.Y.ToString("0.####")+", góc "+(br.Rotation*180/Math.PI).ToString("0.##")+"° — hình mới sẽ GỒM tỉ lệ/góc này";}
   string linked=(info.markerId??info.identityId)==now.id?"khối này là ký hiệu của chính mục "+now.name:"khối này KHÔNG mang dấu MEPS của "+now.name+" (khối ngoài/bản sao) — kiểm tra đúng hình cần dùng";
   DialogResult ans;using(var dlg=new ConfirmUpdateDialog023(now.name,now.id,info.effectiveName+(info.dynamic?" (khối động: chụp trạng thái đang thấy)":"")+"; "+transform+"; "+linked+".","mục hiện ghi "+(string.IsNullOrEmpty(now.units)?"INSUNITS "+now.insunits:now.units)+" → sẽ ghi "+d.Database.Insunits+" (đơn vị bản vẽ hiện tại)")){s("DIALOG_OPEN","confirm-update");ans=App.ShowModalDialog(dlg);}
   s("DIALOG_RESULT",ans.ToString());guard("sau hộp thoại");if(ans!=DialogResult.Yes)return Cancelled("cập nhật hình "+now.name);
   d.Editor.WriteMessage("\nTVXD MEPS: đang cập nhật hình "+now.name+"…");
   var res=LibraryUpdate023.Update(d.Database,id,target,root,lw,System.IO.Path.GetFileName(d.Name));
   return new ModelOutcome019{reload=true,select=now.name,warn=!res.clean,message=res.Summary()};}
#endif
 }
#if LIBUPDATE023
 // 023 owned confirmation: names the exact library entry that will change and what is kept. Không = default (focus, Enter) and Esc. Public for UI QA.
 public sealed class ConfirmUpdateDialog023:Form {
  public readonly Label Info=new Label();public readonly Button Yes=new Button(),No=new Button();
  public static string Text023(string name,string id,string selected,string units){return "Cập nhật HÌNH của mục thư viện "+name+" (mục "+id+") bằng khối đã chọn?\r\n\r\nKhối đã chọn: "+selected+"\r\nĐơn vị: "+units+". MEPS chụp đúng kích thước/góc xoay ĐANG THẤY, KHÔNG tự quy đổi tỉ lệ theo đơn vị.\r\n\r\n"+
   "GIỮ NGUYÊN: ID, tên, mã loại, nhóm, dòng bảng ký hiệu. Bản sao hình cũ và tệp tạm của lần cập nhật được XOÁ sau khi cập nhật kiểm tra thành công (thư viện không còn hình cũ của mục này). Ký hiệu đã đặt trong các bản vẽ giữ hình cũ; lần đặt mới dùng hình mới.\r\nKhối trên Model không bị sửa. UNDO của CAD KHÔNG hoàn tác việc cập nhật thư viện.";}
  public ConfirmUpdateDialog023(string name,string id,string selected,string units){Text="MEPS — cập nhật hình từ Model";FormBorderStyle=FormBorderStyle.FixedDialog;MinimizeBox=false;MaximizeBox=false;ShowInTaskbar=false;StartPosition=FormStartPosition.CenterParent;
   Font=new System.Drawing.Font("Segoe UI",9f);AutoScaleMode=AutoScaleMode.Font;ClientSize=new Size(460,340);
   // PATCH03 F026-UI01: the text sits in a bounded, vertically scrollable panel (wrapping AutoSize label, MaximumSize width = viewport), so long block
   // names/unit notes are never cut off or ellipsised; the button bar is docked at the bottom and always visible. Fonts scale with DPI (AutoScaleMode.Font).
   var content=new System.Windows.Forms.Panel{Dock=DockStyle.Fill,AutoScroll=true,Padding=new Padding(10),AccessibleName="Nội dung xác nhận (cuộn)"};
   Info.Text=Text023(name,id,selected,units);Info.AutoSize=true;Info.AutoEllipsis=false;Info.Location=new Point(10,10);Info.Margin=Padding.Empty;Info.AccessibleName="Nội dung xác nhận";content.Controls.Add(Info);
   EventHandler fit=(s,e)=>{int w=content.ClientSize.Width-content.Padding.Horizontal-SystemInformation.VerticalScrollBarWidth;if(w>50&&Info.MaximumSize.Width!=w)Info.MaximumSize=new Size(w,0);};
   content.Resize+=fit;Load+=fit;
   No.Text="Không";No.DialogResult=DialogResult.No;No.AutoSize=true;No.TabIndex=0;Yes.Text="Có, cập nhật hình";Yes.DialogResult=DialogResult.Yes;Yes.AutoSize=true;Yes.TabIndex=1;
   var bar=new FlowLayoutPanel{FlowDirection=System.Windows.Forms.FlowDirection.RightToLeft,Dock=DockStyle.Bottom,AutoSize=true,AutoSizeMode=AutoSizeMode.GrowAndShrink,MinimumSize=new Size(0,40),Padding=new Padding(6)};bar.Controls.Add(No);bar.Controls.Add(Yes);
   // Fill first, Bottom last: the bar is docked first and keeps its height; the scroll panel takes the rest.
   Controls.Add(content);Controls.Add(bar);AcceptButton=No;CancelButton=No;ActiveControl=No;Shown+=(s,e)=>{fit(null,EventArgs.Empty);No.Focus();};}
 }
#endif
 // Small modal dialog: name (+ optional DIỄN GIẢI / QUY CÁCH / GHI CHÚ). OK validates; an invalid name keeps the dialog open with the exact reason and a
 // suggestion (never applied automatically). Public members for UI QA.
 public sealed class ModelNameDialog019:Form {
  // Reason: read-only wrapping, scrollable text box (not an ellipsised label) so the whole reason, suggestion and example stay readable.
  public readonly TextBox NameBox=new TextBox(),Description=new TextBox(),Spec=new TextBox(),Note=new TextBox(),Reason=new TextBox();public readonly Label Info=new Label(),Rules=new Label();public readonly Button Ok=new Button(),CancelButton2=new Button();
  readonly Func<string,NameCheck019> check;readonly bool withCells;public string Accepted;public CaptureCells019 Cells;
  public ModelNameDialog019(string title,string name,bool cells,Func<string,NameCheck019> check){this.check=check;withCells=cells;Text=title;FormBorderStyle=FormBorderStyle.FixedDialog;MinimizeBox=false;MaximizeBox=false;ShowInTaskbar=false;StartPosition=FormStartPosition.CenterParent;
   Font=new System.Drawing.Font("Segoe UI",9f);AutoScaleMode=AutoScaleMode.Font;ClientSize=new Size(420,cells?396:264);
   var t=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=2,Padding=new Padding(10)};t.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,92));t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
   Action<Control,int> span=(c,h)=>{t.RowStyles.Add(new RowStyle(SizeType.Absolute,h));t.Controls.Add(c,0,t.RowStyles.Count-1);t.SetColumnSpan(c,2);c.Dock=DockStyle.Fill;};
   Action<string,Control,int> row=(caption,c,h)=>{t.RowStyles.Add(new RowStyle(SizeType.Absolute,h));t.Controls.Add(new Label{Text=caption,Dock=DockStyle.Fill,TextAlign=ContentAlignment.MiddleLeft},0,t.RowStyles.Count-1);c.Dock=DockStyle.Fill;t.Controls.Add(c,1,t.RowStyles.Count-1);};
   Info.AutoEllipsis=true;span(Info,44);
   NameBox.Text=name??"";NameBox.AccessibleName="Tên block";NameBox.CharacterCasing=CharacterCasing.Normal;row("Tên block",NameBox,28);
   Rules.Text=Naming019.RuleText;Rules.ForeColor=SystemColors.GrayText;span(Rules,48);
   if(cells){foreach(var p in new[]{new KeyValuePair<string,TextBox>("DIỄN GIẢI",Description),new KeyValuePair<string,TextBox>("QUY CÁCH",Spec),new KeyValuePair<string,TextBox>("GHI CHÚ",Note)}){p.Value.AccessibleName=p.Key;p.Value.MaxLength=Naming019.MaxCell;row(p.Key,p.Value,28);}
    var hint=new Label{Text="Ba ô trên có thể để trống: bảng ký hiệu để trống ô đó (MEPS không tự viết).",ForeColor=SystemColors.GrayText};span(hint,36);}
   Reason.ReadOnly=true;Reason.Multiline=true;Reason.WordWrap=true;Reason.ScrollBars=ScrollBars.Vertical;Reason.BorderStyle=BorderStyle.None;Reason.TabStop=false;Reason.AccessibleName="Lý do";
   Reason.BackColor=SystemColors.Control;Reason.ForeColor=Color.FromArgb(180,40,20);span(Reason,72);
   Ok.Text="Đồng ý";CancelButton2.Text="Huỷ";CancelButton2.DialogResult=DialogResult.Cancel;Ok.Click+=(s,e)=>TryAccept();
   var bar=new FlowLayoutPanel{FlowDirection=System.Windows.Forms.FlowDirection.RightToLeft,Dock=DockStyle.Fill};bar.Controls.Add(CancelButton2);bar.Controls.Add(Ok);t.RowStyles.Add(new RowStyle(SizeType.Absolute,36));t.Controls.Add(bar,0,t.RowStyles.Count-1);t.SetColumnSpan(bar,2);
   t.RowStyles.Add(new RowStyle(SizeType.Percent,100));Controls.Add(t);AcceptButton=Ok;CancelButton=CancelButton2;
   NameBox.TextChanged+=(s,e)=>{Reason.Text="";};Shown+=(s,e)=>{if(NameBox.Text.Length>0)try{ShowCheck(check(NameBox.Text));}catch(Exception ex){Reason.Text=ex.Message;}NameBox.SelectAll();NameBox.Focus();};}
  void ShowCheck(NameCheck019 c){Reason.Text=c.ok?"":c.Message();}
  // Validates the name (fresh catalog/drawing state through the callback) and the cells; closes with OK only when everything is valid.
  // The exact NameBox.Text is validated and accepted (no Trim): a leading/trailing space is refused by the shape rule, never silently removed.
  public bool TryAccept(){string n=NameBox.Text;NameCheck019 c;try{c=check(n);}catch(Exception ex){Reason.Text="Chưa kiểm tra được tên: "+ex.Message;return false;}
   if(!c.ok){ShowCheck(c);NameBox.Focus();return false;}
   if(withCells){try{Cells=Naming019.Clean(new CaptureCells019{description=Description.Text,spec=Spec.Text,note=Note.Text});}catch(Exception ex){Reason.Text=ex.Message;return false;}}
   Accepted=n;DialogResult=DialogResult.OK;Close();return true;}
 }
 // TVXD-MEPS-MODELUI-020 F02: old hide confirmation. Not shown by any route since 021 (the Model route uses ConfirmDeleteDialog021 + LibraryDelete021);
 // kept only so existing 020 UI QA code still compiles. Public members for UI QA.
 public sealed class ConfirmRemoveDialog020:Form {
  public readonly Label Info=new Label();public readonly Button Yes=new Button(),No=new Button();
  public static string Text020(string name,string id){return "Gỡ "+name+" (mục "+id+") khỏi thư viện?\r\n\r\nMEPS chỉ ẨN mục này trong bảng. File nguồn, file thư viện và mọi khối "+name+" đã đặt trong Model được GIỮ NGUYÊN.\r\nKhôi phục: chuột phải danh sách → Hiện mục đã ẩn → chọn mục → Khôi phục mục đang chọn.";}
  public ConfirmRemoveDialog020(string name,string id){Text="MEPS — gỡ khỏi thư viện";FormBorderStyle=FormBorderStyle.FixedDialog;MinimizeBox=false;MaximizeBox=false;ShowInTaskbar=false;StartPosition=FormStartPosition.CenterParent;
   Font=new System.Drawing.Font("Segoe UI",9f);AutoScaleMode=AutoScaleMode.Font;ClientSize=new Size(420,190);
   Info.Text=Text020(name,id);Info.Dock=DockStyle.Fill;Info.Padding=new Padding(10);Info.AccessibleName="Nội dung xác nhận";
   No.Text="Không";No.DialogResult=DialogResult.No;No.AutoSize=true;No.TabIndex=0;Yes.Text="Có, gỡ (ẩn)";Yes.DialogResult=DialogResult.Yes;Yes.AutoSize=true;Yes.TabIndex=1;
   var bar=new FlowLayoutPanel{FlowDirection=System.Windows.Forms.FlowDirection.RightToLeft,Dock=DockStyle.Bottom,Height=40,Padding=new Padding(6)};bar.Controls.Add(No);bar.Controls.Add(Yes);
   Controls.Add(Info);Controls.Add(bar);AcceptButton=No;CancelButton=No;ActiveControl=No;Shown+=(s,e)=>No.Focus();}
 }
}
#endif
