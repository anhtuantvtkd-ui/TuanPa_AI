using System;using System.Drawing;using System.Linq;using System.Windows.Forms;
namespace Tvxd.Meps {
 // 0.6.0 legend preflight dialog (TVXD-MEPS-IMPLEMENT-008). Lists every used symbol (OK, or the reason and the concrete action), foreign look-alike block
 // references and xrefs before anything is written. Actions: re-read the source table of the SELECTED symbol's source only (Remap callback), copy the
 // report, and export a NEW DWG, which is enabled only when nothing blocks (all-or-nothing, never a silent partial legend). Pure WinForms; the plugin
 // supplies the callbacks (Test008 drives it offline).
 // TVXD-MEPS-LEGEND-009: with a Meps06.PreflightForExport result, symbols exported with empty cells are listed as "Ô TRỐNG · <status>" (not blocking).
 // TVXD-MEPS-LEGEND-011: the action button is 'Đặt vào Model…'. The plugin's Export callback only records the request (no file dialog, no CAD prompt) and the
 // dialog closes; the point prompt starts after the modal dialog is closed. Field/callback names are unchanged for Test008.
 public sealed class LegendPreflightDialog:Form {
  internal readonly Label summary;internal readonly ListView list;internal readonly TextBox detail;
  internal readonly CompactLibraryView05.RoundButton remap,copy,export,close;
  // Remap: re-read the selected item's source; returns the new preflight, or null when the user cancelled. Export: accepts the 'Đặt vào Model…' request and
  // returns a message (the dialog then closes with OK), or null to stay open.
  public Func<PreflightItem,LegendPreflight> Remap;public Func<LegendPreflight,string> Export;public Action<string> CopyText=t=>Clipboard.SetText(t);
  public LegendPreflight Current;public string Result;
  readonly float dpi;int S(int px){return Math.Max(1,(int)Math.Round(px*dpi));}
  public LegendPreflightDialog(LegendPreflight p):this(p,CompactView06.SystemDpi()){}
  public LegendPreflightDialog(LegendPreflight p,float scale){dpi=float.IsNaN(scale)||scale<=0?1f:Math.Max(1f,Math.Min(3f,scale));AutoScaleMode=AutoScaleMode.None;
   var font=new Font("Segoe UI",CompactView06.TextPx*dpi,FontStyle.Regular,GraphicsUnit.Pixel);Font=font;Text="Bảng ký hiệu — kiểm tra trước khi đặt vào Model (MEPS "+Meps06.Version+")";
   FormBorderStyle=FormBorderStyle.Sizable;MinimizeBox=false;MaximizeBox=false;ShowInTaskbar=false;StartPosition=FormStartPosition.CenterScreen;BackColor=CompactLibraryView.CBack;ForeColor=CompactLibraryView.CFore;
   ClientSize=new Size(S(620),S(420));MinimumSize=new Size(S(460),S(320));
   var pane=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=1,RowCount=4,Padding=new Padding(S(8))};foreach(int h in new[]{S(40),-1,S(80),S(38)})pane.RowStyles.Add(h<0?new RowStyle(SizeType.Percent,100):new RowStyle(SizeType.Absolute,h));
   summary=new Label{Dock=DockStyle.Fill,Margin=new Padding(0),Font=font};pane.Controls.Add(summary,0,0);
   list=new ListView{Dock=DockStyle.Fill,View=View.Details,FullRowSelect=true,MultiSelect=false,HideSelection=false,BackColor=CompactLibraryView.CField,ForeColor=CompactLibraryView.CFore,Font=font,BorderStyle=BorderStyle.FixedSingle};
   list.Columns.Add("Ký hiệu",S(150));list.Columns.Add("SL",S(36));list.Columns.Add("Trạng thái",S(130));list.Columns.Add("Lý do",S(260));pane.Controls.Add(list,0,1);
   detail=new TextBox{Dock=DockStyle.Fill,Multiline=true,ReadOnly=true,ScrollBars=ScrollBars.Vertical,BackColor=CompactLibraryView.CField,ForeColor=CompactLibraryView.CFore,Font=font,BorderStyle=BorderStyle.None,Margin=new Padding(0,S(4),0,S(4))};pane.Controls.Add(detail,0,2);
   var bar=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=4,RowCount=1,Margin=new Padding(0)};bar.RowStyles.Add(new RowStyle(SizeType.Percent,100));foreach(var w in new[]{34,22,30,14})bar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,w));
   remap=Make("Đọc lại bảng nguồn…",font);copy=Make("Sao chép báo cáo",font);export=Make("Đặt vào Model…",font);close=Make("Đóng",font);close.DialogResult=DialogResult.Cancel;
   bar.Controls.Add(remap,0,0);bar.Controls.Add(copy,1,0);bar.Controls.Add(export,2,0);bar.Controls.Add(close,3,0);pane.Controls.Add(bar,0,3);Controls.Add(pane);CancelButton=close;
   list.SelectedIndexChanged+=(s,e)=>ShowSelected();remap.Click+=(s,e)=>DoRemap();copy.Click+=(s,e)=>{try{CopyText(Current.Report());}catch(Exception ex){detail.Text="Chưa sao chép được: "+ex.Message;}};export.Click+=(s,e)=>DoExport();
   SetPreflight(p);}
  CompactLibraryView05.RoundButton Make(string text,Font font){var b=new CompactLibraryView05.RoundButton{Text=text,Dock=DockStyle.Fill,Margin=new Padding(S(2)),Font=font};b.Colors(CompactLibraryView.CField,CompactLibraryView.CTeal,CompactLibraryView05.CHover,CompactLibraryView.CTealHot);return b;}
  internal PreflightItem SelectedItem{get{return list.SelectedItems.Count==1?list.SelectedItems[0].Tag as PreflightItem:null;}}
  public void SetPreflight(LegendPreflight p){Current=p;list.BeginUpdate();try{list.Items.Clear();
    foreach(var i in p.items.OrderByDescending(x=>x.blocking).ThenBy(x=>x.status==Legend.Ok?1:0).ThenBy(x=>x.name,StringComparer.OrdinalIgnoreCase)){var row=new ListViewItem(new[]{i.name??"",i.count>0?i.count.ToString():"",(i.blocking?"CHẶN · ":p.policy==Legend.PolicyBlank&&p.blankText.Contains(i.name)?"Ô TRỐNG · ":"")+i.status,i.reason??""}){Tag=i,ToolTipText=i.reason+(string.IsNullOrEmpty(i.action)?"":"\n→ "+i.action)};
     row.ForeColor=i.blocking?CompactLibraryView.CWarn:i.status==Legend.Ok?CompactLibraryView.CFore:CompactLibraryView.CMuted;list.Items.Add(row);}}finally{list.EndUpdate();}
   list.ShowItemToolTips=true;summary.Text=p.Summary();summary.ForeColor=p.CanExport?CompactLibraryView.CFore:CompactLibraryView.CWarn;export.Enabled=p.CanExport&&Export!=null;
   if(list.Items.Count>0)list.Items[0].Selected=true;ShowSelected();}
  // Callbacks are assigned after construction: buttons are re-evaluated when the dialog is shown.
  protected override void OnShown(EventArgs e){base.OnShown(e);ShowSelected();}
  internal void UpdateButtons(){ShowSelected();}
  void ShowSelected(){var i=SelectedItem;remap.Enabled=Remap!=null&&i!=null&&!string.IsNullOrEmpty(i.sourceSha256)&&i.status!=Meps06.NotInLibrary;export.Enabled=Current!=null&&Current.CanExport&&Export!=null;
   detail.Text=i==null?Current.Summary():i.name+(i.count>0?" ×"+i.count:"")+" — "+i.status+"\r\n"+i.reason+(string.IsNullOrEmpty(i.action)?"":"\r\nCách xử lý: "+i.action)+(i.sourceFile!=null?"\r\nNguồn: "+i.sourceFile+" ("+Meps06.Short(i.sourceSha256)+")":"");}
  internal void DoRemap(){var i=SelectedItem;if(i==null||Remap==null)return;try{var next=Remap(i);if(next!=null){SetPreflight(next);detail.Text="Đã đọc lại bảng của nguồn "+i.sourceFile+".\r\n"+next.Summary()+"\r\n"+detail.Text;}}catch(Exception ex){detail.Text="Chưa đọc lại bảng nguồn: "+ex.Message;}}
  internal void DoExport(){if(Current==null||!Current.CanExport||Export==null){detail.Text="Chưa đặt: còn mục chặn.";return;}try{Result=Export(Current);if(Result!=null){DialogResult=DialogResult.OK;Close();}}catch(Exception ex){detail.Text="Chưa đặt: "+ex.Message;}}
 }
}
