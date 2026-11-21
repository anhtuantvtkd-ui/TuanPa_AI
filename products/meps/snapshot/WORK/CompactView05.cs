using System;using System.Drawing;using System.Drawing.Drawing2D;using System.Linq;using System.Windows.Forms;using System.Globalization;using System.Collections.Generic;
namespace Tvxd.Meps {
 // 0.5.2 compact palette (TVXD-MEPS-UI-006; default 300 px, tested 280/300/320 × 600/774/900). Same dark theme, 26 px previews, exact names, three placement
 // modes, import and legend as 0.5.1 (CompactLibraryView stays unchanged for 0.4.2), with:
 //  - the tile list gets half the height it had in 0.5.1 (GridHeightFor) and scrolls on its own; the space below it stays blank (reserved for later controls).
 //  - no selected-block details, Place button, settings toggle or bottom status: messages and warnings show in a compact status line at the top (full text in
 //    its tooltip); scale and rotation open in a bounded dialog from the scale chip or ≡.
 //  - one LEFT click on a tile sends one guarded placement request (pick a point on Model, ESC cancels). Right click, arrows, Home/End, search, group and
 //    reload only select; Enter places the selected tile; ESC in the palette drops a request whose point is not picked yet. While a request is armed, further
 //    clicks (incl. the second click of a double click) dispatch nothing until the plugin calls Disarm.
 //  - tiles, buttons and fields are painted with rounded corners.
 public sealed class CompactLibraryView05:UserControl {
  static readonly Color CBack=CompactLibraryView.CBack,CField=CompactLibraryView.CField,CTile=CompactLibraryView.CTile,CLine=CompactLibraryView.CLine,CFore=CompactLibraryView.CFore,CMuted=CompactLibraryView.CMuted,CTeal=CompactLibraryView.CTeal,CTealHot=CompactLibraryView.CTealHot,CWarn=CompactLibraryView.CWarn;
  public static readonly Color CBad=CompactLibraryView.CBad,CArmed=Color.FromArgb(16,56,52),CHover=Color.FromArgb(48,48,52);
  // 0.5.1 reference: the tiles got client height − 294 (fixed rows) − 10 (padding). 0.5.2 gives them half of that (600 → 148, 774 → 235, 900 → 298),
  // never less than two tile rows. Fixed rows: header 30 + status 34 + toolbar 36 + search 30 + group 30 = 160.
  public const int MinWidth=280,DefaultWidth=300,PreviewSize=Meps05.PreviewSize,TileHeight=PreviewSize+10,TileMargin=3,RowPitch=TileHeight+2*TileMargin,GridMin=2*RowPitch,
   OldFixed=294,OldPadding=10,GridRow=5,SettingsHeight=240,NameGap=4,Radius=6;
  public const string AddCaption="+ Thêm block",LegendCaption="Tạo bảng ký hiệu",SettingsCaption="Tỉ lệ & góc xoay",RawMode="Tỉ lệ đều",FitMode="Theo bề rộng",RatioMode="Theo tỷ lệ bản vẽ";
  public static int GridHeightFor(int clientHeight){return Math.Max(GridMin,(clientHeight-OldFixed-OldPadding)/2);}
  internal readonly TableLayoutPanel root=new TableLayoutPanel{ColumnCount=1,RowCount=7,Margin=new Padding(0),Padding=new Padding(6,4,6,6)},settings=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=2,RowCount=6,Margin=new Padding(0)};
  internal readonly Label title=new Label{Text="Ký hiệu điện · MEPS "+Meps05.Version,Dock=DockStyle.Fill,AutoEllipsis=true,TextAlign=ContentAlignment.MiddleLeft},
   units=new Label{Dock=DockStyle.Fill,AutoEllipsis=true,Margin=new Padding(0,2,0,0)},status=new Label{Dock=DockStyle.Fill,AutoEllipsis=true,TextAlign=ContentAlignment.MiddleLeft,Margin=new Padding(2,0,4,0)};
  internal readonly RoundButton add=new RoundButton{Text=AddCaption,Dock=DockStyle.Fill,Margin=new Padding(0,4,4,4),AccessibleName="Thêm block từ file DWG/DXF"},menu=new RoundButton{Text="≡",Dock=DockStyle.Fill,Margin=new Padding(0,2,0,2),AccessibleName="Tùy chọn thư viện"},
   legend=new RoundButton{Text=LegendCaption,Dock=DockStyle.Fill,Margin=new Padding(4,4,0,4),AccessibleName="Tạo bảng ký hiệu các khối đã dùng ra DWG mới"},chip=new RoundButton{Dock=DockStyle.Fill,Margin=new Padding(0,5,0,5),AccessibleName=SettingsCaption},
   close=new RoundButton{Text="Đóng",Anchor=AnchorStyles.Top|AnchorStyles.Right,Size=new Size(96,30),Margin=new Padding(0,4,0,0),DialogResult=DialogResult.OK,AccessibleName="Đóng tỉ lệ và góc xoay"};
  internal readonly TextBox search=new TextBox{AccessibleName="Tìm khối"},scale=new TextBox{Text="1",AccessibleName="Tỉ lệ"},width=new TextBox{Text="",AccessibleName="Bề rộng muốn"},
   refDen=new TextBox{Text="",AccessibleName="Mẫu số tỷ lệ gốc N0"},drawDen=new TextBox{Text="",AccessibleName="Mẫu số tỷ lệ bản vẽ N"},rotation=new TextBox{Text="0",AccessibleName="Góc xoay (độ)"};
  internal readonly ComboBox category=new ComboBox{DropDownStyle=ComboBoxStyle.DropDownList,AccessibleName="Nhóm"},mode=new ComboBox{DropDownStyle=ComboBoxStyle.DropDownList,AccessibleName="Cách đặt"};
  internal readonly FlowLayoutPanel grid=new FlowLayoutPanel{Dock=DockStyle.Fill,AutoScroll=true,WrapContents=true,FlowDirection=FlowDirection.LeftToRight,Margin=new Padding(0,2,0,2),Padding=new Padding(0)};
  // Reserved for later controls: intentionally empty.
  internal readonly System.Windows.Forms.Panel blank=new System.Windows.Forms.Panel{Dock=DockStyle.Fill,Margin=new Padding(0),Name="blank"};
  internal readonly Form dialog=new Form{Text=SettingsCaption,FormBorderStyle=FormBorderStyle.FixedToolWindow,ShowInTaskbar=false,MinimizeBox=false,MaximizeBox=false,StartPosition=FormStartPosition.Manual};
  internal readonly ContextMenuStrip options=new ContextMenuStrip();internal readonly ToolStripMenuItem showHidden=new ToolStripMenuItem("Hiện mục đã ẩn"){CheckOnClick=true};
  internal readonly Dictionary<string,ToolStripMenuItem> items=new Dictionary<string,ToolStripMenuItem>();
  internal readonly ToolTip tips=new ToolTip{ShowAlways=true};
  internal readonly List<Tile> tiles=new List<Tile>();
  readonly Dictionary<string,Bitmap> cache=new Dictionary<string,Bitmap>();
  internal int Renders;
  public string Root;public LibCatalog Catalog;public Action<string,LibPlacement> Request;
  // Shows the scale/rotation dialog modally; the plugin replaces it with AutoCAD's Application.ShowModalDialog.
  public Func<Form,DialogResult> ShowModal=f=>f.ShowDialog();
  string drawingName,drawingUnits,armed,armedName;bool filling;
  // Entry ID of the placement request sent and not yet finished (null = none).
  internal string Armed{get{return armed;}}
  internal LibEntry Selected{get{var t=tiles.FirstOrDefault(x=>x.Selected&&x.Listed);return t==null?null:t.Entry;}}
  internal string[] Shown{get{return tiles.Where(t=>t.Listed).Select(t=>t.Entry.name).ToArray();}}
  public CompactLibraryView05(){Dock=DockStyle.Fill;BackColor=CBack;ForeColor=CFore;Font=new Font("Segoe UI",9f);
   // Rows: header, status, toolbar (add | legend), search, group, tiles (GridHeightFor), blank (rest).
   foreach(int h in new[]{30,34,36,30,30,GridMin,-1})root.RowStyles.Add(h<0?new RowStyle(SizeType.Percent,100):new RowStyle(SizeType.Absolute,h));Controls.Add(root);
   title.Font=new Font("Segoe UI",10f,FontStyle.Bold);var head=Row(new[]{-1,32});head.Controls.Add(title,0,0);head.Controls.Add(menu,1,0);root.Controls.Add(head,0,0);
   status.ForeColor=CMuted;status.Font=new Font("Segoe UI",8f);var line=Row(new[]{-1,96});line.Controls.Add(status,0,0);line.Controls.Add(chip,1,0);root.Controls.Add(line,0,1);
   var bar=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=2,RowCount=1,Margin=new Padding(0)};bar.RowStyles.Add(new RowStyle(SizeType.Percent,100));bar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,46));bar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,54));
   bar.Controls.Add(add,0,0);bar.Controls.Add(legend,1,0);root.Controls.Add(bar,0,2);
   var find=Row(new[]{44,-1});find.Controls.Add(Caption("Tìm"),0,0);find.Controls.Add(new RoundField(search),1,0);root.Controls.Add(find,0,3);
   var group=Row(new[]{44,-1});group.Controls.Add(Caption("Nhóm"),0,0);group.Controls.Add(new RoundField(category),1,0);root.Controls.Add(group,0,4);
   grid.BackColor=CBack;root.Controls.Add(grid,0,GridRow);blank.BackColor=CBack;root.Controls.Add(blank,0,GridRow+1);
   settings.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,78));settings.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));foreach(int h in new[]{30,30,30,30,30,-1})settings.RowStyles.Add(h<0?new RowStyle(SizeType.Percent,100):new RowStyle(SizeType.Absolute,h));
   mode.Items.AddRange(new object[]{RawMode,FitMode,RatioMode});mode.SelectedIndex=0;
   // Drawing ratio on one row: [N0] → [N], i.e. symbol drawn for 1:N0, current drawing 1:N.
   var ratioRow=Row(new[]{-1,22,-1});var arrow=Caption("→");arrow.TextAlign=ContentAlignment.MiddleCenter;ratioRow.Controls.Add(new RoundField(refDen),0,0);ratioRow.Controls.Add(arrow,1,0);ratioRow.Controls.Add(new RoundField(drawDen),2,0);
   var fields=new[]{new KeyValuePair<string,Control>("Cách đặt",new RoundField(mode)),new KeyValuePair<string,Control>("Tỉ lệ",new RoundField(scale)),new KeyValuePair<string,Control>("Bề rộng",new RoundField(width)),new KeyValuePair<string,Control>("1:N0 → 1:N",ratioRow),new KeyValuePair<string,Control>("Góc (độ)",new RoundField(rotation))};
   for(int i=0;i<fields.Length;i++){settings.Controls.Add(Caption(fields[i].Key),0,i);settings.Controls.Add(fields[i].Value,1,i);}
   settings.Controls.Add(units,0,5);settings.SetColumnSpan(units,2);units.ForeColor=CMuted;units.Font=new Font("Segoe UI",8f);units.AutoEllipsis=false;
   // Bounded scale/rotation dialog: fixed size, closed with Đóng / Enter / ESC; the values stay in this view (per drawing for N0/N, see SetDrawing).
   var pane=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=1,RowCount=2,Margin=new Padding(0),Padding=new Padding(8,6,8,6)};pane.RowStyles.Add(new RowStyle(SizeType.Percent,100));pane.RowStyles.Add(new RowStyle(SizeType.Absolute,36));
   pane.Controls.Add(settings,0,0);pane.Controls.Add(close,0,1);dialog.Controls.Add(pane);dialog.BackColor=CBack;dialog.ForeColor=CFore;dialog.Font=Font;dialog.ClientSize=new Size(DefaultWidth,SettingsHeight+48);dialog.AcceptButton=close;dialog.CancelButton=close;
   foreach(var t in new[]{search,scale,width,refDen,drawDen,rotation}){t.BackColor=CField;t.ForeColor=CFore;}
   foreach(var c in new[]{category,mode}){c.BackColor=CField;c.ForeColor=CFore;c.FlatStyle=FlatStyle.Flat;}
   menu.Colors(CBack,CLine,CHover,CFore);add.Colors(CField,CTeal,CHover,CTealHot);add.Font=new Font("Segoe UI",9.5f,FontStyle.Bold);legend.Colors(CField,CTeal,CHover,CTealHot);
   chip.Colors(CField,CLine,CHover,CFore);chip.Font=new Font("Segoe UI",8.25f);close.Colors(CTeal,CTeal,CTealHot,Color.White);close.Font=new Font("Segoe UI",9f,FontStyle.Bold);
   items["settings"]=new ToolStripMenuItem(SettingsCaption+"…");items["settings"].Click+=(s,e)=>OpenSettings();
   foreach(var m in new[]{new[]{"hide","Ẩn mục đang chọn"},new[]{"restore","Khôi phục mục đang chọn"},new[]{"undo","Hoàn tác lần đặt cuối"},new[]{"reload","Tải lại thư viện"}}){string a=m[0];var item=new ToolStripMenuItem(m[1]);item.Click+=(s,e)=>Run(a);items[a]=item;}
   options.Items.Add(items["settings"]);options.Items.Add(new ToolStripSeparator());options.Items.Add(showHidden);options.Items.Add(items["hide"]);options.Items.Add(items["restore"]);options.Items.Add(new ToolStripSeparator());options.Items.Add(items["undo"]);options.Items.Add(items["reload"]);
   menu.Click+=(s,e)=>options.Show(menu,new Point(menu.Width-options.Width,menu.Height));showHidden.CheckedChanged+=(s,e)=>{Categories();Filter();};
   add.Click+=(s,e)=>Run("import");legend.Click+=(s,e)=>Run("legend");chip.Click+=(s,e)=>OpenSettings();
   search.TextChanged+=(s,e)=>Filter();category.SelectedIndexChanged+=(s,e)=>{if(!filling)Filter();};grid.SizeChanged+=(s,e)=>SizeTiles();
   search.KeyDown+=(s,e)=>{if(e.KeyCode==Keys.Down||e.KeyCode==Keys.Enter){FocusTiles();e.SuppressKeyPress=true;}};
   foreach(var t in new[]{scale,width,refDen,drawDen,rotation})t.TextChanged+=(s,e)=>ShowResult();
   mode.SelectedIndexChanged+=(s,e)=>{Enable();ShowResult();};Enable();
   tips.SetToolTip(add,"Thêm block từ file DWG/DXF (chỉ đọc nguồn; mục mới được chép, chuẩn hoá màu/nét và đọc bảng ký hiệu nếu nguồn có).");tips.SetToolTip(menu,"Tỉ lệ & góc xoay, hiện mục đã ẩn, ẩn/khôi phục, hoàn tác, tải lại");
   tips.SetToolTip(grid,"Bấm chuột trái một hình rồi chọn điểm trên Model; ESC để huỷ. Chuột phải chỉ chọn (ẩn/khôi phục).");
   tips.SetToolTip(legend,"Quét Model của bản vẽ hiện tại, lấy các ký hiệu MEPS đã dùng và tạo bảng ký hiệu trong một file DWG MỚI. Bản vẽ hiện tại không bị sửa.");
   tips.SetToolTip(refDen,"N0: mẫu số tỷ lệ mà ký hiệu nguồn được vẽ cho (1:N0). Anh tự nhập; MEPS không tự đoán.");tips.SetToolTip(drawDen,"N: mẫu số tỷ lệ của bản vẽ hiện tại (1:N). Lưu riêng cho bản vẽ này khi đặt.");
   int tab=0;foreach(var c in new Control[]{search,category,grid,add,legend,chip,menu})c.TabIndex=tab++;
   tab=0;foreach(var c in new Control[]{mode,scale,width,refDen,drawDen,rotation,close})c.TabIndex=tab++;
   Message("Bấm một hình rồi chọn điểm trên Model; ESC để huỷ.");ShowResult();Fit();}
  static TableLayoutPanel Row(int[] cols){var t=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=cols.Length,RowCount=1,Margin=new Padding(0)};t.RowStyles.Add(new RowStyle(SizeType.Percent,100));foreach(int w in cols)t.ColumnStyles.Add(w<0?new ColumnStyle(SizeType.Percent,100):new ColumnStyle(SizeType.Absolute,w));return t;}
  static Label Caption(string text){return new Label{Text=text,Dock=DockStyle.Fill,TextAlign=ContentAlignment.MiddleLeft,AutoEllipsis=true,Margin=new Padding(0),ForeColor=CMuted};}
  void Enable(){scale.Enabled=mode.SelectedIndex!=1;width.Enabled=mode.SelectedIndex==1;refDen.Enabled=drawDen.Enabled=mode.SelectedIndex==2;}
  internal int FixedHeight{get{return root.RowStyles.Cast<RowStyle>().Where((r,i)=>i!=GridRow&&r.SizeType==SizeType.Absolute).Sum(r=>(int)r.Height);}}
  internal int MinContentHeight{get{return FixedHeight+root.Padding.Vertical+GridMin;}}
  // Sized on resize from the view's own Size (unlike ClientSize it does not shrink while a scroll bar is shown), never from inside a layout pass, so the
  // list row always matches the stable height. The palette itself scrolls only below MinContentHeight; otherwise AutoScroll is off and root fills it.
  bool fitting;
  protected override void OnSizeChanged(EventArgs e){base.OnSizeChanged(e);Fit();}
  internal void Fit(){if(fitting||root.RowStyles.Count<=GridRow)return;fitting=true;
   try{int w=Width,h=Height;bool scroll=h<MinContentHeight;if(!scroll&&AutoScroll){AutoScrollPosition=Point.Empty;AutoScroll=false;}
    int g=GridHeightFor(h);if((int)root.RowStyles[GridRow].Height!=g)root.RowStyles[GridRow].Height=g;
    if(!scroll){if(root.Bounds!=new Rectangle(0,0,w,h))root.Bounds=new Rectangle(0,0,w,h);}
    else{if(!AutoScroll){root.Location=Point.Empty;AutoScroll=true;}root.Size=new Size(Math.Max(0,w-SystemInformation.VerticalScrollBarWidth),MinContentHeight);}}
   finally{fitting=false;}}
  public void Message(string s){Status(s,false);}
  public void Warn(string s){Status(s,true);}
  void Status(string s,bool warn){status.Text=s;status.ForeColor=warn?CWarn:CMuted;tips.SetToolTip(status,s);}
  internal bool Warning{get{return status.ForeColor==CWarn;}}
  // Drawing switch: N0/N are per drawing. ratio = {N0, N} stored in that drawing (null = none stored): fields are filled or cleared, never carried over.
  public void SetDrawing(string name,string insunits,double[] ratio){drawingName=name;drawingUnits=insunits;
   refDen.Text=ratio!=null&&ratio.Length>=2?Library.F(ratio[0]):"";drawDen.Text=ratio!=null&&ratio.Length>=2?Library.F(ratio[1]):"";ShowResult();}
  public void Reload(){Reload(null);}
  public void Reload(string selectName){string keep=selectName==null&&Selected!=null?Selected.id:null;Catalog=null;
   if(Root!=null)try{Catalog=Library.Load(Root);}catch(Exception ex){Warn("Catalog lỗi, chưa sửa gì: "+ex.Message+" (bản trước: catalog.json.bak)");}else Warn("Chưa có thư mục thư viện.");
   grid.SuspendLayout();try{foreach(var t in tiles){grid.Controls.Remove(t);t.Dispose();}tiles.Clear();if(Catalog!=null)foreach(var e in Library.Search(Catalog,"",Library.All,true)){var t=new Tile(this,e);tips.SetToolTip(t,Tip(e));tiles.Add(t);grid.Controls.Add(t);}}finally{grid.ResumeLayout(false);}
   Categories();Filter();var pick=tiles.FirstOrDefault(t=>t.Listed&&(keep!=null?t.Entry.id==keep:selectName!=null&&string.Equals(t.Entry.name,selectName,StringComparison.OrdinalIgnoreCase)));if(pick!=null)Choose(pick,false);}
  internal static string StyleText(LibEntry e){return e.style==Meps05.StyleFrozen?"nét cố định theo nguồn":e.style==Meps05.StyleAmbiguous?"nét CHƯA cố định (nguồn mơ hồ)":"nét theo layer đích (bản chép cũ)";}
  static string Tip(LibEntry e){return e.name+"\n"+e.category+" · nguồn "+e.sourceFile+"\nGốc "+Library.F(Library.Width(e))+" × "+Library.F(Library.Height(e))+" "+e.units+" · "+StyleText(e)+(e.hidden?"\nĐANG ẨN":"")+(e.status!="OK"?"\nCHƯA HỖ TRỢ: "+string.Join("; ",e.warnings.Where(w=>!w.StartsWith("INFO")).Take(3)):"")+"\nBấm chuột trái để đặt (chọn điểm, ESC huỷ); chuột phải: ẩn/khôi phục.";}
  void Categories(){filling=true;try{string current=category.SelectedItem as string;category.Items.Clear();category.Items.AddRange(Catalog==null?new object[]{Library.All}:Library.Categories(Catalog,showHidden.Checked).Cast<object>().ToArray());int i=current==null?-1:category.Items.IndexOf(current);category.SelectedIndex=i>=0?i:0;}finally{filling=false;}}
  internal void Filter(){var listed=Catalog==null?new HashSet<string>():new HashSet<string>(Library.Search(Catalog,search.Text,category.SelectedItem as string,showHidden.Checked).Select(e=>e.id));
   grid.SuspendLayout();try{foreach(var t in tiles){t.Listed=listed.Contains(t.Entry.id);t.Visible=t.Listed;if(!t.Listed)t.Selected=false;}}finally{grid.ResumeLayout(true);}SizeTiles();ShowResult();}
  internal void SizeTiles(){int w=Math.Max(90,(grid.Width-SystemInformation.VerticalScrollBarWidth-grid.Padding.Horizontal-4*TileMargin)/2);grid.SuspendLayout();try{foreach(var t in tiles)if(t.Width!=w||t.Height!=TileHeight)t.Size=new Size(w,TileHeight);}finally{grid.ResumeLayout(true);}}
  internal void Wheel(int delta){int y=-grid.AutoScrollPosition.Y-delta*RowPitch/120;grid.AutoScrollPosition=new Point(0,Math.Max(0,y));}
  // Same recoloured renderer as 0.4.2, at 26 px. Cached per root, id and library-file hash.
  internal Bitmap Thumb(LibEntry e){string key=Root+"|"+e.id+"|"+e.fileSha256;Bitmap b;if(cache.TryGetValue(key,out b))return b;Renders++;using(var raw=Library.Render(Library.LoadPreview(Root,e),PreviewSize))b=CompactLibraryView.Dark(raw);cache[key]=b;return b;}
  // Up to two lines for the tile name: the whole name if it fits, else a break after the last '-' (or character) that fits; the rest is ellipsized on line 2.
  internal static string[] NameLines(string name,Font font,int width){name=name??"";if(TextRenderer.MeasureText(name,font,Size.Empty,TextFormatFlags.NoPrefix|TextFormatFlags.NoPadding).Width<=width)return new[]{name};
   int cut=0;for(int i=1;i<name.Length;i++)if(TextRenderer.MeasureText(name.Substring(0,i),font,Size.Empty,TextFormatFlags.NoPrefix|TextFormatFlags.NoPadding).Width<=width)cut=i;else break;
   if(cut<=0)return new[]{name};int dash=name.LastIndexOf('-',cut-1);if(dash>=cut/2)cut=dash+1;return new[]{name.Substring(0,cut),name.Substring(cut)};}
  internal static GraphicsPath RoundRect(RectangleF r,float radius){var p=new GraphicsPath();float d=Math.Max(1,Math.Min(radius*2,Math.Min(r.Width,r.Height)));
   p.AddArc(r.X,r.Y,d,d,180,90);p.AddArc(r.Right-d,r.Y,d,d,270,90);p.AddArc(r.Right-d,r.Bottom-d,d,d,0,90);p.AddArc(r.X,r.Bottom-d,d,d,90,90);p.CloseFigure();return p;}
  // Selection only: never dispatches (used by clicks, keys, reload and import).
  internal void Choose(Tile t,bool focus){foreach(var x in tiles)if(x!=t&&x.Selected){x.Selected=false;x.TabStop=false;x.Invalidate();}t.Selected=true;t.TabStop=true;t.Invalidate();grid.ScrollControlIntoView(t);if(focus)t.Focus();ShowResult();}
  void FocusTiles(){var t=tiles.FirstOrDefault(x=>x.Selected&&x.Listed)??tiles.FirstOrDefault(x=>x.Listed);if(t!=null)Choose(t,true);}
  // The single placement entry point of the palette (left click on a tile, Enter on a tile). One request at a time.
  internal void Place(){if(armed!=null){Message("Đang chờ chọn điểm trên Model cho "+armedName+"; ESC để huỷ. Bấm thêm không đặt thêm.");return;}Run("place");}
  void Arm(LibEntry e){armed=e.id;armedName=e.name;Repaint(e.id);}
  // Called by the plugin when the request has finished (placed, ESC, error, document switch).
  public void Disarm(){if(armed==null)return;string id=armed;armed=armedName=null;Repaint(id);}
  // ESC in the palette: drops the request if its point is not picked yet (the plugin ignores it once its command is running). Never adds anything.
  internal bool CancelArm(){if(armed==null)return false;string name=armedName;Disarm();try{if(Request!=null)Request("cancel",null);}catch(Exception){}Message("Đã huỷ yêu cầu đặt "+name+"; chưa thay đổi bản vẽ.");return true;}
  void Repaint(string id){foreach(var t in tiles)if(t.Entry.id==id)t.Invalidate();}
  protected override bool ProcessCmdKey(ref System.Windows.Forms.Message msg,Keys keyData){if(keyData==Keys.Escape&&CancelArm())return true;return base.ProcessCmdKey(ref msg,keyData);}
  internal bool Key(Keys k){if(k==Keys.Escape)return CancelArm();var list=tiles.Where(t=>t.Listed).ToList();if(list.Count==0)return false;int i=list.FindIndex(t=>t.Selected);
   if(k==Keys.Enter){if(i>=0)Place();return true;}int step=k==Keys.Left?-1:k==Keys.Right?1:k==Keys.Up?-2:k==Keys.Down?2:0;int to=k==Keys.Home?0:k==Keys.End?list.Count-1:step==0?-2:i<0?0:Math.Max(0,Math.Min(list.Count-1,i+step));if(to==-2)return false;Choose(list[to],true);return true;}
  // Mouse on a tile: left = select + one placement request; right = select + ≡ menu (hide/restore); a double click adds nothing.
  internal void TileDown(Tile t,MouseButtons b){if(b==MouseButtons.Left||b==MouseButtons.Right)Choose(t,true);}
  internal void TileClick(Tile t,MouseButtons b){if(b==MouseButtons.Left&&t.Selected)Place();}
  internal void TileUp(Tile t,MouseButtons b,Point at){if(b==MouseButtons.Right)options.Show(t,at);}
  internal void OpenSettings(){ShowResult();
   try{var at=chip.IsHandleCreated?chip.PointToScreen(new Point(chip.Width-dialog.Width,chip.Height+2)):Cursor.Position;var area=Screen.FromPoint(at).WorkingArea;dialog.Location=new Point(Math.Max(area.Left,Math.Min(at.X,area.Right-dialog.Width)),Math.Max(area.Top,Math.Min(at.Y,area.Bottom-dialog.Height)));}catch(Exception){}
   try{ShowModal(dialog);}catch(Exception ex){Warn("Chưa mở được "+SettingsCaption+": "+ex.Message);}ShowResult();}
  static bool Number(string text,out double v){return double.TryParse(text,NumberStyles.Float,CultureInfo.CurrentCulture,out v)||double.TryParse(text,NumberStyles.Float,CultureInfo.InvariantCulture,out v);}
  internal double BaseScale(){double v;if(!Number(scale.Text,out v))throw new Exception("Nhập tỉ lệ bằng số dương.");return Library.CheckScale(v);}
  internal double RefDenominator(){double v;if(!Number(refDen.Text,out v))throw new Exception("Nhập mẫu số tỷ lệ gốc N0 (1:N0); không có giá trị mặc định.");return Meps05.CheckDenominator(v,"Mẫu số tỷ lệ gốc N0");}
  internal double DrawingDenominator(){double v;if(!Number(drawDen.Text,out v))throw new Exception("Nhập mẫu số tỷ lệ bản vẽ N (1:N); không có giá trị mặc định.");return Meps05.CheckDenominator(v,"Mẫu số tỷ lệ bản vẽ N");}
  internal double ScaleFor(LibEntry e){double v;if(mode.SelectedIndex==1){if(!Number(width.Text,out v))throw new Exception("Nhập bề rộng mong muốn bằng số (đơn vị bản vẽ).");return Library.FitWidthScale(e,v);}
   if(mode.SelectedIndex==2)return Meps05.RatioScale(BaseScale(),RefDenominator(),DrawingDenominator());return BaseScale();}
  internal double RotationDeg(){double v;if(!Number(rotation.Text,out v))throw new Exception("Nhập góc xoay bằng số độ.");return Library.CheckRotation(v);}
  // Scale chip (top), ≡ menu caption, field colours and the factor/units note of the dialog. Text only; nothing is dispatched or redrawn in a loop.
  void ShowResult(){var e=Selected;string scaleText,chipScale,rotText;bool badScale=false,badRot=false,badBase=false,badN0=false,badN=false;
   try{BaseScale();}catch(Exception){badBase=true;}if(mode.SelectedIndex==2){try{RefDenominator();}catch(Exception){badN0=true;}try{DrawingDenominator();}catch(Exception){badN=true;}}
   try{if(mode.SelectedIndex==1){scaleText=e==null?"theo bề rộng "+width.Text:Library.F(ScaleFor(e))+"× (bề rộng "+width.Text+")";chipScale=e==null?"rộng "+width.Text:Library.F(ScaleFor(e))+"×";}
    else if(mode.SelectedIndex==2){chipScale=Library.F(ScaleFor(null))+"×";scaleText="1:"+refDen.Text+"→1:"+drawDen.Text+" = "+chipScale;}else{chipScale=Library.F(ScaleFor(null))+"×";scaleText=chipScale;}}
   catch(Exception){scaleText="tỉ lệ chưa hợp lệ";chipScale="tỉ lệ ?";badScale=true;}
   try{rotText=Library.F(RotationDeg())+"°";}catch(Exception){rotText="góc ?";badRot=true;}
   scale.BackColor=badBase&&mode.SelectedIndex!=1?CBad:CField;width.BackColor=badScale&&mode.SelectedIndex==1?CBad:CField;refDen.BackColor=badN0?CBad:CField;drawDen.BackColor=badN?CBad:CField;rotation.BackColor=badRot?CBad:CField;
   chip.Text=chipScale+" · "+rotText;chip.ForeColor=badScale||badRot?CWarn:CFore;items["settings"].Text=SettingsCaption+": "+scaleText+" · "+rotText+"…";
   string drawing=drawingUnits==null?"bản vẽ: chưa đọc":"bản vẽ "+drawingUnits;string factor;
   try{factor=mode.SelectedIndex==2?"Hệ số = "+Meps05.RatioText(BaseScale(),RefDenominator(),DrawingDenominator()):mode.SelectedIndex==1?"Hệ số tính theo bề rộng":"Hệ số = "+Library.F(BaseScale());}catch(Exception){factor="Hệ số: chưa đủ giá trị hợp lệ";}
   units.Text="Nguồn "+(e==null?"(chọn khối)":e.units)+" → "+drawing+". "+factor+". KHÔNG tự đổi inch→mm.";
   string note=units.Text+"\nHệ số = đơn vị bản vẽ cho 1 đơn vị nguồn. N0, N do anh nhập, lưu riêng trong bản vẽ khi đặt; MEPS không tự đoán tỷ lệ."+(drawingName!=null?"\n"+drawingName:"");
   tips.SetToolTip(units,note);tips.SetToolTip(chip,SettingsCaption+": "+scaleText+" · "+rotText+(badScale||badRot?" — CHƯA HỢP LỆ, bấm để sửa":" — bấm để chỉnh")+"\n"+note);}
  internal void Run(string action){try{Act(action);}catch(Exception ex){Warn("Chưa thực hiện: "+ex.Message);}}
  internal void Act(string action){
   if(action=="import"){if(Root==null)throw new Exception("Chưa có thư mục thư viện.");if(Request!=null)Request("import",null);return;}
   if(action=="undo"){if(Request!=null)Request("undo",null);return;}
   if(action=="legend"){if(Root==null)throw new Exception("Chưa có thư mục thư viện.");if(Request!=null)Request("legend",null);return;}
   if(action=="reload"){Reload();Message("Đã tải lại thư viện.");return;}
   var e=Selected;if(e==null)throw new Exception("Chọn một hình khối trong danh sách.");
   if(action=="place"){if(armed!=null)throw new Exception("đang chờ chọn điểm cho "+armedName+"; chọn điểm hoặc ESC trước.");if(e.hidden)throw new Exception("Mục đang ẩn; khôi phục trước khi đặt.");var issue=Meps05.PlaceIssue(e);if(issue!=null)throw new Exception(e.name+": "+issue);
    var p=new LibPlacement{id=e.id,rotationDeg=RotationDeg(),fitWidth=mode.SelectedIndex==1,ratio=mode.SelectedIndex==2};p.scale=ScaleFor(e);
    if(p.fitWidth){double w;Number(width.Text,out w);p.width=w;}if(p.ratio){p.baseScale=BaseScale();p.refDenominator=RefDenominator();p.drawingDenominator=DrawingDenominator();}else if(!p.fitWidth)p.baseScale=p.scale;
    // Armed only after the plugin accepted the request (a refusal throws); never armed without a callback.
    if(Request==null)throw new Exception("bảng chưa nối với AutoCAD; chưa đặt gì.");Message("Chọn điểm chèn "+e.name+" trên Model; ESC để huỷ.");Request("place",p);Arm(e);return;}
   if(action=="hide"){if(e.hidden)throw new Exception("Mục đã ẩn.");Library.SetHidden(Root,e.id,true);Reload();Message("Đã ẩn "+e.name+". Không xoá file nguồn, file thư viện hay khối đã đặt; ≡ → Hiện mục đã ẩn để khôi phục.");return;}
   if(action=="restore"){if(!e.hidden)throw new Exception("≡ → Hiện mục đã ẩn, chọn mục [ẩn] rồi khôi phục.");Library.SetHidden(Root,e.id,false);Reload();Message("Đã khôi phục "+e.name+".");return;}
   throw new Exception("Thao tác không hợp lệ: "+action);}
  protected override void Dispose(bool disposing){if(disposing){foreach(var b in cache.Values)b.Dispose();cache.Clear();tips.Dispose();options.Dispose();dialog.Dispose();}base.Dispose(disposing);}
  // Flat button painted as a rounded bar (fill, 1 px border, centred caption); hover and focus only change colours.
  internal sealed class RoundButton:Button {
   Color fill=CField,border=CLine,hover=CHover;bool hot;
   // TVXD-MEPS-UI-031 (optional, off by default so every existing button paints as before): Icon = Glyph draws Text as a filled Segoe UI path of IconPx
   // (IconStyle) centred on its ink bounds; Grid = 16-unit vector legend table scaled to IconPx; Chevron = small down arrow IconPx wide. CornerRadius < 0
   // keeps Radius. Nothing here owns a Font.
   internal enum IconKind{None,Glyph,Grid,Chevron}
   internal IconKind Icon=IconKind.None;internal float IconPx=16f,CornerRadius=-1f;internal FontStyle IconStyle=FontStyle.Bold;
   public RoundButton(){SetStyle(ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.UserPaint|ControlStyles.ResizeRedraw,true);FlatStyle=FlatStyle.Flat;FlatAppearance.BorderSize=0;UseVisualStyleBackColor=false;Cursor=Cursors.Hand;}
   internal void Colors(Color f,Color b,Color h,Color fore){fill=f;border=b;hover=h;ForeColor=fore;BackColor=f;Invalidate();}
   protected override void OnMouseEnter(EventArgs e){hot=true;Invalidate();base.OnMouseEnter(e);}
   protected override void OnMouseLeave(EventArgs e){hot=false;Invalidate();base.OnMouseLeave(e);}
   protected override void OnPaint(PaintEventArgs pe){var g=pe.Graphics;g.Clear(Parent!=null?Parent.BackColor:CBack);g.SmoothingMode=SmoothingMode.AntiAlias;
    using(var path=RoundRect(new RectangleF(.5f,.5f,Width-2f,Height-2f),CornerRadius<0?Radius:CornerRadius))using(var b=new SolidBrush(hot&&Enabled?hover:fill))using(var pen=new Pen(Focused&&ShowFocusCues?CFore:border)){g.FillPath(b,path);g.DrawPath(pen,path);}
    if(Icon!=IconKind.None){PaintIcon(g,Enabled?ForeColor:CMuted);return;}
    TextRenderer.DrawText(g,Text,Font,ClientRectangle,Enabled?ForeColor:CMuted,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.SingleLine|TextFormatFlags.EndEllipsis|TextFormatFlags.NoPrefix);}
   // Icon bounds in client coordinates (UI QA): the ink box of the glyph path, or the IconPx square of the grid/chevron, centred in the button.
   internal RectangleF IconBounds{get{if(Icon!=IconKind.Glyph)return new RectangleF((Width-1-IconPx)/2f,(Height-1-IconPx)/2f,IconPx,IconPx);using(var p=GlyphPath())return p.GetBounds();}}
   GraphicsPath GlyphPath(){var p=new GraphicsPath();using(var fam=new FontFamily("Segoe UI"))using(var sf=(StringFormat)StringFormat.GenericTypographic.Clone())p.AddString(Text??"",fam,(int)IconStyle,IconPx,PointF.Empty,sf);
    var b=p.GetBounds();using(var m=new Matrix()){m.Translate((Width-1)/2f-(b.X+b.Width/2),(Height-1)/2f-(b.Y+b.Height/2));p.Transform(m);}return p;}
   void PaintIcon(Graphics g,Color c){
    if(Icon==IconKind.Glyph){using(var p=GlyphPath())using(var br=new SolidBrush(c))g.FillPath(br,p);return;}
    var r=IconBounds;float k=IconPx/16f;
    if(Icon==IconKind.Grid){using(var pen=new Pen(c,1.5f*k)){using(var box=RoundRect(new RectangleF(r.X+.75f*k,r.Y+.75f*k,14.5f*k,14.5f*k),2.5f*k))g.DrawPath(pen,box);
      g.DrawLine(pen,r.X+6*k,r.Y+k,r.X+6*k,r.Y+15*k);g.DrawLine(pen,r.X+k,r.Y+5.75f*k,r.X+15*k,r.Y+5.75f*k);g.DrawLine(pen,r.X+k,r.Y+10.25f*k,r.X+15*k,r.Y+10.25f*k);}return;}
    using(var pen=new Pen(c,1.5f*k){StartCap=LineCap.Round,EndCap=LineCap.Round,LineJoin=LineJoin.Round})g.DrawLines(pen,new[]{new PointF(r.X+4*k,r.Y+6*k),new PointF(r.X+8*k,r.Y+10*k),new PointF(r.X+12*k,r.Y+6*k)});}
  }
  // Rounded container of one borderless text box or flat combo box; its fill follows the field colour (CBad when invalid), teal border while focused.
  // Fully qualified: Tvxd.Meps.Panel (Panel.cs) is a different, sealed type.
  internal sealed class RoundField:System.Windows.Forms.Panel {
   internal readonly Control Field;
   // TVXD-MEPS-UI-031 (optional): CornerRadius < 0 keeps Radius; Border/FocusBorder default to the existing colours.
   internal float CornerRadius=-1f;internal Color Border=CLine,FocusBorder=CTeal;
   public RoundField(Control field){Field=field;Dock=DockStyle.Fill;Margin=new Padding(0,2,0,2);BackColor=CBack;SetStyle(ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.UserPaint|ControlStyles.ResizeRedraw,true);
    var box=field as TextBox;if(box!=null){box.BorderStyle=BorderStyle.None;Padding=new Padding(7,5,6,2);}else Padding=new Padding(4,1,4,1);
    field.Dock=DockStyle.Fill;field.Margin=new Padding(0);Controls.Add(field);field.BackColorChanged+=(s,e)=>Invalidate();field.EnabledChanged+=(s,e)=>Invalidate();field.GotFocus+=(s,e)=>Invalidate();field.LostFocus+=(s,e)=>Invalidate();}
   protected override void OnPaint(PaintEventArgs pe){var g=pe.Graphics;g.Clear(Parent!=null?Parent.BackColor:CBack);g.SmoothingMode=SmoothingMode.AntiAlias;
    using(var path=RoundRect(new RectangleF(.5f,.5f,Width-2f,Height-2f),CornerRadius<0?Radius:CornerRadius))using(var b=new SolidBrush(Field.BackColor))using(var pen=new Pen(Field.ContainsFocus?FocusBorder:Border)){g.FillPath(b,path);g.DrawPath(pen,path);}}
  }
  // One library entry: rounded tile, 26 px green preview at the left, exact block name (up to two lines) to its right.
  internal sealed class Tile:Control {
   public readonly LibEntry Entry;readonly CompactLibraryView05 owner;public bool Selected,Listed=true;bool hot;
   public Tile(CompactLibraryView05 o,LibEntry e){owner=o;Entry=e;SetStyle(ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.UserPaint|ControlStyles.ResizeRedraw|ControlStyles.Selectable|ControlStyles.StandardClick|ControlStyles.StandardDoubleClick,true);
    Name="tile_"+e.id;Text=e.name;AccessibleName=e.name;AccessibleRole=AccessibleRole.ListItem;TabStop=false;Margin=new Padding(TileMargin);BackColor=CTile;ForeColor=e.hidden?CMuted:Meps05.PlaceIssue(e)!=null?CWarn:CFore;Font=new Font("Segoe UI",8.25f);Cursor=Cursors.Hand;}
   internal bool IsArmed{get{return owner.Armed==Entry.id;}}
   internal Rectangle PreviewRect{get{return new Rectangle(4,(Height-PreviewSize)/2,PreviewSize,PreviewSize);}}
   internal Rectangle NameRect{get{int x=4+PreviewSize+NameGap;return new Rectangle(x,2,Math.Max(1,Width-x-4),Height-4);}}
   internal string[] Lines{get{return NameLines((Entry.hidden?"[ẩn] ":"")+Entry.name,Font,NameRect.Width);}}
   protected override void OnPaint(PaintEventArgs pe){var g=pe.Graphics;g.Clear(Parent!=null?Parent.BackColor:CBack);g.SmoothingMode=SmoothingMode.AntiAlias;float w=Selected?2f:1f;
    using(var path=RoundRect(new RectangleF(w/2,w/2,Width-w-1,Height-w-1),Radius)){using(var b=new SolidBrush(IsArmed?CArmed:CTile))g.FillPath(b,path);
     g.DrawImage(owner.Thumb(Entry),PreviewRect);var r=NameRect;var lines=Lines;var flags=TextFormatFlags.EndEllipsis|TextFormatFlags.Left|TextFormatFlags.SingleLine|TextFormatFlags.NoPrefix|TextFormatFlags.NoPadding;
     if(lines.Length==1)TextRenderer.DrawText(g,lines[0],Font,r,ForeColor,flags|TextFormatFlags.VerticalCenter);
     else{int h=r.Height/2;TextRenderer.DrawText(g,lines[0],Font,new Rectangle(r.X,r.Y,r.Width,h),ForeColor,flags|TextFormatFlags.Bottom);TextRenderer.DrawText(g,lines[1],Font,new Rectangle(r.X,r.Y+h,r.Width,r.Height-h),ForeColor,flags|TextFormatFlags.Top);}
     using(var pen=new Pen(Selected||IsArmed?CTeal:hot?CMuted:CLine,w))g.DrawPath(pen,path);}
    if(Focused&&Selected)using(var dots=new Pen(CMuted){DashStyle=DashStyle.Dot})using(var inner=RoundRect(new RectangleF(3,3,Width-7,Height-7),Radius-2))g.DrawPath(dots,inner);}
   protected override void OnMouseEnter(EventArgs e){hot=true;Invalidate();base.OnMouseEnter(e);}
   protected override void OnMouseLeave(EventArgs e){hot=false;Invalidate();base.OnMouseLeave(e);}
   protected override void OnMouseDown(MouseEventArgs e){owner.TileDown(this,e.Button);base.OnMouseDown(e);}
   protected override void OnMouseClick(MouseEventArgs e){base.OnMouseClick(e);owner.TileClick(this,e.Button);}
   protected override void OnMouseUp(MouseEventArgs e){base.OnMouseUp(e);owner.TileUp(this,e.Button,e.Location);}
   protected override void OnMouseWheel(MouseEventArgs e){owner.Wheel(e.Delta);base.OnMouseWheel(e);}
   protected override bool IsInputKey(Keys k){return k==Keys.Left||k==Keys.Right||k==Keys.Up||k==Keys.Down||k==Keys.Home||k==Keys.End||k==Keys.Enter||base.IsInputKey(k);}
   protected override void OnKeyDown(KeyEventArgs e){if(owner.Key(e.KeyCode)){e.Handled=true;e.SuppressKeyPress=true;}else base.OnKeyDown(e);}
   protected override void OnGotFocus(EventArgs e){Invalidate();base.OnGotFocus(e);}
   protected override void OnLostFocus(EventArgs e){Invalidate();base.OnLostFocus(e);}
  }
 }
}
