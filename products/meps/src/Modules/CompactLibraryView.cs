using System;using System.Drawing;using System.Drawing.Imaging;using System.Linq;using System.Windows.Forms;using System.Globalization;using System.Collections.Generic;using System.Runtime.InteropServices;
namespace Tvxd.Meps {
 // 0.4 compact palette (default 300 px, tested 280/300/320): dark tiles with green previews in two columns, search + category,
 // a permanent "+ Thêm block" DWG/DXF toolbar, collapsible scale/rotation settings and one primary place button.
 // Pure WinForms over the unchanged Library model. Import, placement and undo go to the plugin through Request.
 // Viewing only reads catalog.json and previews/<id>.json; hide/restore (≡ menu) are the only catalog writes, exactly as in 0.3.1.
 public sealed class CompactLibraryView:UserControl {
  public static readonly Color CBack=Color.FromArgb(37,37,38),CField=Color.FromArgb(30,30,31),CTile=Color.FromArgb(26,26,28),CLine=Color.FromArgb(68,68,72),CFore=Color.FromArgb(232,232,232),CMuted=Color.FromArgb(160,160,166),CTeal=Color.FromArgb(0,150,136),CTealHot=Color.FromArgb(0,176,160),CInk=Color.FromArgb(64,232,96),CBad=Color.FromArgb(92,36,36),CWarn=Color.FromArgb(255,140,110);
  public const int MinWidth=280,DefaultWidth=300,PreviewSize=76,TileHeight=PreviewSize+30,GridMin=120,SettingsHeight=150;
  public const string AddCaption="+ Thêm block",PlaceCaption="Đặt lên bản vẽ",RawMode="Tỉ lệ đều",FitMode="Theo bề rộng";
  internal readonly TableLayoutPanel root=new TableLayoutPanel{ColumnCount=1,RowCount=10,Margin=new Padding(0),Padding=new Padding(6,4,6,6)},settings=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=2,RowCount=5,Margin=new Padding(0)};
  internal readonly Label title=new Label{Text="Ký hiệu điện · MEPS 0.4.2",Dock=DockStyle.Fill,AutoEllipsis=true,TextAlign=ContentAlignment.MiddleLeft},addHint=new Label{Text="DWG · DXF",Dock=DockStyle.Fill,TextAlign=ContentAlignment.MiddleCenter},
   selName=new Label{Dock=DockStyle.Fill,AutoEllipsis=true,TextAlign=ContentAlignment.BottomLeft,Margin=new Padding(2,0,2,0)},selSize=new Label{Dock=DockStyle.Fill,AutoEllipsis=true,Margin=new Padding(2,0,2,0)},
   units=new Label{Dock=DockStyle.Fill,AutoEllipsis=true,Margin=new Padding(0,2,0,0)},status=new Label{Dock=DockStyle.Fill,AutoEllipsis=true,Margin=new Padding(2,2,2,0)};
  internal readonly Button add=new Button{Text=AddCaption,Dock=DockStyle.Fill,Margin=new Padding(0,4,4,4),AccessibleName="Thêm block từ file DWG/DXF"},menu=new Button{Text="≡",Dock=DockStyle.Fill,Margin=new Padding(0,2,0,2),AccessibleName="Tùy chọn thư viện"},
   toggle=new Button{Dock=DockStyle.Fill,TextAlign=ContentAlignment.MiddleLeft,Margin=new Padding(0,2,0,2),AccessibleName="Tỉ lệ và góc xoay"},place=new Button{Text=PlaceCaption,Dock=DockStyle.Fill,Margin=new Padding(0,4,0,4),AccessibleName="Đặt khối đang chọn lên bản vẽ"};
  internal readonly TextBox search=new TextBox{Dock=DockStyle.Fill,AccessibleName="Tìm khối"},scale=new TextBox{Text="1",Dock=DockStyle.Fill,AccessibleName="Tỉ lệ"},width=new TextBox{Text="",Dock=DockStyle.Fill,AccessibleName="Bề rộng muốn"},rotation=new TextBox{Text="0",Dock=DockStyle.Fill,AccessibleName="Góc xoay (độ)"};
  internal readonly ComboBox category=new ComboBox{DropDownStyle=ComboBoxStyle.DropDownList,Dock=DockStyle.Fill,AccessibleName="Nhóm"},mode=new ComboBox{DropDownStyle=ComboBoxStyle.DropDownList,Dock=DockStyle.Fill,AccessibleName="Cách đặt"};
  internal readonly FlowLayoutPanel grid=new FlowLayoutPanel{Dock=DockStyle.Fill,AutoScroll=true,WrapContents=true,FlowDirection=FlowDirection.LeftToRight,Margin=new Padding(0,2,0,2),Padding=new Padding(0)};
  internal readonly ContextMenuStrip options=new ContextMenuStrip();internal readonly ToolStripMenuItem showHidden=new ToolStripMenuItem("Hiện mục đã ẩn"){CheckOnClick=true};
  internal readonly Dictionary<string,ToolStripMenuItem> items=new Dictionary<string,ToolStripMenuItem>();
  internal readonly ToolTip tips=new ToolTip{ShowAlways=true};
  internal readonly List<Tile> tiles=new List<Tile>();
  readonly Dictionary<string,Bitmap> cache=new Dictionary<string,Bitmap>();
  internal int Renders;
  bool expanded;internal bool Expanded{get{return expanded;}}
  public string Root;public LibCatalog Catalog;public Action<string,LibPlacement> Request;
  string drawingName,drawingUnits;bool filling;
  internal LibEntry Selected{get{var t=tiles.FirstOrDefault(x=>x.Selected&&x.Listed);return t==null?null:t.Entry;}}
  internal string[] Shown{get{return tiles.Where(t=>t.Listed).Select(t=>t.Entry.name).ToArray();}}
  public CompactLibraryView(){Dock=DockStyle.Fill;AutoScroll=true;BackColor=CBack;ForeColor=CFore;Font=new Font("Segoe UI",9f);
   // Rows: header, add toolbar, search, category, tiles (rest), selection, settings toggle, settings (0 when collapsed), place, status.
   foreach(int h in new[]{30,40,30,30,-1,52,30,0,46,38})root.RowStyles.Add(h<0?new RowStyle(SizeType.Percent,100):new RowStyle(SizeType.Absolute,h));Controls.Add(root);
   title.Font=new Font("Segoe UI",10f,FontStyle.Bold);var head=Row(new[]{-1,32});head.Controls.Add(title,0,0);head.Controls.Add(menu,1,0);root.Controls.Add(head,0,0);
   var bar=Row(new[]{-1,74});bar.Controls.Add(add,0,0);bar.Controls.Add(addHint,1,0);root.Controls.Add(bar,0,1);addHint.ForeColor=CMuted;
   var find=Row(new[]{44,-1});find.Controls.Add(Caption("Tìm"),0,0);find.Controls.Add(search,1,0);root.Controls.Add(find,0,2);
   var group=Row(new[]{44,-1});group.Controls.Add(Caption("Nhóm"),0,0);group.Controls.Add(category,1,0);root.Controls.Add(group,0,3);
   grid.BackColor=CBack;root.Controls.Add(grid,0,4);
   var sel=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=1,RowCount=2,Margin=new Padding(0)};sel.RowStyles.Add(new RowStyle(SizeType.Absolute,20));sel.RowStyles.Add(new RowStyle(SizeType.Percent,100));selName.Font=new Font("Segoe UI",9f,FontStyle.Bold);selSize.ForeColor=CMuted;sel.Controls.Add(selName,0,0);sel.Controls.Add(selSize,0,1);root.Controls.Add(sel,0,5);
   root.Controls.Add(toggle,0,6);
   settings.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,74));settings.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));foreach(int h in new[]{27,27,27,27,-1})settings.RowStyles.Add(h<0?new RowStyle(SizeType.Percent,100):new RowStyle(SizeType.Absolute,h));
   mode.Items.AddRange(new object[]{RawMode,FitMode});mode.SelectedIndex=0;settings.Controls.Add(Caption("Cách đặt"),0,0);settings.Controls.Add(mode,1,0);settings.Controls.Add(Caption("Tỉ lệ"),0,1);settings.Controls.Add(scale,1,1);
   settings.Controls.Add(Caption("Bề rộng"),0,2);settings.Controls.Add(width,1,2);settings.Controls.Add(Caption("Góc (độ)"),0,3);settings.Controls.Add(rotation,1,3);settings.Controls.Add(units,0,4);settings.SetColumnSpan(units,2);units.ForeColor=CMuted;units.Font=new Font("Segoe UI",8f);
   settings.Visible=false;root.Controls.Add(settings,0,7);root.Controls.Add(place,0,8);status.ForeColor=CMuted;status.Font=new Font("Segoe UI",8f);root.Controls.Add(status,0,9);
   foreach(var c in new Control[]{search,scale,width,rotation}){var t=(TextBox)c;t.BackColor=CField;t.ForeColor=CFore;t.BorderStyle=BorderStyle.FixedSingle;t.Margin=new Padding(0,2,0,2);}
   foreach(var c in new[]{category,mode}){c.BackColor=CField;c.ForeColor=CFore;c.FlatStyle=FlatStyle.Flat;c.Margin=new Padding(0,2,0,2);}
   Flat(menu,CBack,CLine,CFore);Flat(toggle,CBack,CBack,CFore);Flat(add,CField,CTeal,CTealHot);add.Font=new Font("Segoe UI",9.5f,FontStyle.Bold);Flat(place,CTeal,CTeal,Color.White);place.Font=new Font("Segoe UI",10f,FontStyle.Bold);place.FlatAppearance.MouseOverBackColor=CTealHot;
   foreach(var m in new[]{new[]{"hide","Ẩn mục đang chọn"},new[]{"restore","Khôi phục mục đang chọn"},new[]{"undo","Hoàn tác lần đặt cuối"},new[]{"reload","Tải lại thư viện"}}){string a=m[0];var item=new ToolStripMenuItem(m[1]);item.Click+=(s,e)=>Run(a);items[a]=item;}
   options.Items.Add(showHidden);options.Items.Add(items["hide"]);options.Items.Add(items["restore"]);options.Items.Add(new ToolStripSeparator());options.Items.Add(items["undo"]);options.Items.Add(items["reload"]);
   menu.Click+=(s,e)=>options.Show(menu,new Point(menu.Width-options.Width,menu.Height));showHidden.CheckedChanged+=(s,e)=>{Categories();Filter();};
   add.Click+=(s,e)=>Run("import");place.Click+=(s,e)=>Run("place");toggle.Click+=(s,e)=>SetExpanded(!Expanded);
   search.TextChanged+=(s,e)=>Filter();category.SelectedIndexChanged+=(s,e)=>{if(!filling)Filter();};grid.SizeChanged+=(s,e)=>SizeTiles();
   search.KeyDown+=(s,e)=>{if(e.KeyCode==Keys.Down||e.KeyCode==Keys.Enter){FocusTiles();e.SuppressKeyPress=true;}};
   foreach(var t in new[]{scale,width,rotation}){t.TextChanged+=(s,e)=>ShowResult();t.KeyDown+=(s,e)=>{if(e.KeyCode==Keys.Enter){Run("place");e.SuppressKeyPress=true;}};}
   mode.SelectedIndexChanged+=(s,e)=>{scale.Enabled=mode.SelectedIndex==0;width.Enabled=mode.SelectedIndex==1;ShowResult();};width.Enabled=false;
   tips.SetToolTip(add,"Thêm block từ file DWG/DXF (chỉ đọc nguồn; mục mới được chép và chuẩn hoá vào thư viện).");tips.SetToolTip(menu,"Hiện mục đã ẩn, ẩn/khôi phục, hoàn tác, tải lại");tips.SetToolTip(place,"Đặt khối đang chọn: chọn điểm trên Model, ESC để huỷ. Bảng tạm ẩn khi chọn điểm, hiện lại sau khi đặt/huỷ.");
   int tab=0;foreach(var c in new Control[]{search,category,grid,toggle,mode,scale,width,rotation,place,add,menu})c.TabIndex=tab++;
   Message("Chọn một hình rồi bấm "+PlaceCaption+".");ShowResult();}
  static TableLayoutPanel Row(int[] cols){var t=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=cols.Length,RowCount=1,Margin=new Padding(0)};t.RowStyles.Add(new RowStyle(SizeType.Percent,100));foreach(int w in cols)t.ColumnStyles.Add(w<0?new ColumnStyle(SizeType.Percent,100):new ColumnStyle(SizeType.Absolute,w));return t;}
  static Label Caption(string text){return new Label{Text=text,Dock=DockStyle.Fill,TextAlign=ContentAlignment.MiddleLeft,AutoEllipsis=true,Margin=new Padding(0),ForeColor=CMuted};}
  static void Flat(Button b,Color back,Color border,Color fore){b.FlatStyle=FlatStyle.Flat;b.BackColor=back;b.ForeColor=fore;b.FlatAppearance.BorderColor=border;b.FlatAppearance.BorderSize=1;b.UseVisualStyleBackColor=false;}
  // The table is never docked: it fills the client area but keeps a minimum height, so a short palette scrolls instead of clipping fields.
  internal int MinContentHeight{get{return root.RowStyles.Cast<RowStyle>().Where(r=>r.SizeType==SizeType.Absolute).Sum(r=>(int)r.Height)+root.Padding.Vertical+GridMin;}}
  protected override void OnLayout(LayoutEventArgs e){base.OnLayout(e);int h=Math.Max(ClientSize.Height,MinContentHeight);if(root.Width!=ClientSize.Width||root.Height!=h)root.Size=new Size(ClientSize.Width,h);if(h==ClientSize.Height&&root.Top!=0)root.Location=Point.Empty;}
  internal void SetExpanded(bool on){SuspendLayout();expanded=on;settings.Visible=on;root.RowStyles[7].Height=on?SettingsHeight:0;ResumeLayout(true);ShowResult();PerformLayout();}
  public void Message(string s){status.Text=s;tips.SetToolTip(status,s);}
  public void SetDrawing(string name,string insunits){drawingName=name;drawingUnits=insunits;ShowResult();}
  // Reload re-reads catalog.json only; tiles are rebuilt but cached previews (keyed by root, id and library-file hash) are reused.
  public void Reload(){Reload(null);}
  public void Reload(string selectName){string keep=selectName==null&&Selected!=null?Selected.id:null;Catalog=null;
   if(Root!=null)try{Catalog=Library.Load(Root);}catch(Exception ex){Message("Catalog lỗi, chưa sửa gì: "+ex.Message+" (bản trước: catalog.json.bak)");}else Message("Chưa có thư mục thư viện.");
   grid.SuspendLayout();try{foreach(var t in tiles){grid.Controls.Remove(t);t.Dispose();}tiles.Clear();if(Catalog!=null)foreach(var e in Library.Search(Catalog,"",Library.All,true)){var t=new Tile(this,e);tips.SetToolTip(t,Tip(e));tiles.Add(t);grid.Controls.Add(t);}}finally{grid.ResumeLayout(false);}
   Categories();Filter();var pick=tiles.FirstOrDefault(t=>t.Listed&&(keep!=null?t.Entry.id==keep:selectName!=null&&string.Equals(t.Entry.name,selectName,StringComparison.OrdinalIgnoreCase)));if(pick!=null)Choose(pick,false);}
  static string Tip(LibEntry e){return e.name+"\n"+e.category+" · nguồn "+e.sourceFile+"\nGốc "+Library.F(Library.Width(e))+" × "+Library.F(Library.Height(e))+" "+e.units+(e.hidden?"\nĐANG ẨN":"")+(e.status!="OK"?"\nCHƯA HỖ TRỢ: "+string.Join("; ",e.warnings.Where(w=>!w.StartsWith("INFO")).Take(3)):"");}
  void Categories(){filling=true;try{string current=category.SelectedItem as string;category.Items.Clear();category.Items.AddRange(Catalog==null?new object[]{Library.All}:Library.Categories(Catalog,showHidden.Checked).Cast<object>().ToArray());int i=current==null?-1:category.Items.IndexOf(current);category.SelectedIndex=i>=0?i:0;}finally{filling=false;}}
  // Filtering only toggles existing tiles: no catalog re-read, no DWG access, no preview re-render.
  internal void Filter(){var listed=Catalog==null?new HashSet<string>():new HashSet<string>(Library.Search(Catalog,search.Text,category.SelectedItem as string,showHidden.Checked).Select(e=>e.id));
   grid.SuspendLayout();try{foreach(var t in tiles){t.Listed=listed.Contains(t.Entry.id);t.Visible=t.Listed;if(!t.Listed)t.Selected=false;}}finally{grid.ResumeLayout(true);}SizeTiles();ShowResult();}
  internal void SizeTiles(){int w=Math.Max(90,(grid.Width-SystemInformation.VerticalScrollBarWidth-grid.Padding.Horizontal-12)/2);grid.SuspendLayout();try{foreach(var t in tiles)if(t.Width!=w||t.Height!=TileHeight)t.Size=new Size(w,TileHeight);}finally{grid.ResumeLayout(true);}}
  internal Bitmap Thumb(LibEntry e){string key=Root+"|"+e.id+"|"+e.fileSha256;Bitmap b;if(cache.TryGetValue(key,out b))return b;Renders++;using(var raw=Library.Render(Library.LoadPreview(Root,e),PreviewSize))b=Dark(raw);cache[key]=b;return b;}
  // Library.Render draws dark green on white (unchanged 0.3.1 renderer). For the dark tile, coverage = 1 - min(R,G,B)/255 of each pixel
  // is re-blended over the tile colour: green/grey ink becomes bright ACI-3-like green; red/orange warnings keep their hue.
  internal static Bitmap Dark(Bitmap src){var b=new Bitmap(src.Width,src.Height,PixelFormat.Format32bppArgb);using(var g=Graphics.FromImage(b))g.DrawImage(src,0,0,src.Width,src.Height);var rect=new Rectangle(0,0,b.Width,b.Height);var data=b.LockBits(rect,ImageLockMode.ReadWrite,PixelFormat.Format32bppArgb);
   try{var px=new byte[data.Stride*b.Height];Marshal.Copy(data.Scan0,px,0,px.Length);for(int i=0;i+3<px.Length;i+=4){int bl=px[i],gr=px[i+1],rd=px[i+2];int min=Math.Min(rd,Math.Min(gr,bl)),max=Math.Max(rd,Math.Max(gr,bl));float a=(255-min)/255f;Color ink;
     if(gr>=rd&&gr>=bl)ink=CInk;else{float k=max==0?0:255f/max;ink=Color.FromArgb(Math.Min(255,(int)(rd*k)),Math.Min(255,(int)(gr*k)),Math.Min(255,(int)(bl*k)));}
     px[i]=(byte)(CTile.B+(ink.B-CTile.B)*a);px[i+1]=(byte)(CTile.G+(ink.G-CTile.G)*a);px[i+2]=(byte)(CTile.R+(ink.R-CTile.R)*a);px[i+3]=255;}Marshal.Copy(px,0,data.Scan0,px.Length);}finally{b.UnlockBits(data);}return b;}
  internal void Choose(Tile t,bool focus){foreach(var x in tiles)if(x!=t&&x.Selected){x.Selected=false;x.TabStop=false;x.Invalidate();}t.Selected=true;t.TabStop=true;t.Invalidate();grid.ScrollControlIntoView(t);if(focus)t.Focus();ShowResult();}
  void FocusTiles(){var t=tiles.FirstOrDefault(x=>x.Selected&&x.Listed)??tiles.FirstOrDefault(x=>x.Listed);if(t!=null)Choose(t,true);}
  // Keyboard on a tile: arrows move within the two-column grid, Home/End jump, Enter places.
  internal bool Key(Keys k){var list=tiles.Where(t=>t.Listed).ToList();if(list.Count==0)return false;int i=list.FindIndex(t=>t.Selected);
   if(k==Keys.Enter){Run("place");return true;}int step=k==Keys.Left?-1:k==Keys.Right?1:k==Keys.Up?-2:k==Keys.Down?2:0;int to=k==Keys.Home?0:k==Keys.End?list.Count-1:step==0?-2:i<0?0:Math.Max(0,Math.Min(list.Count-1,i+step));if(to==-2)return false;Choose(list[to],true);return true;}
  static bool Number(string text,out double v){return double.TryParse(text,NumberStyles.Float,CultureInfo.CurrentCulture,out v)||double.TryParse(text,NumberStyles.Float,CultureInfo.InvariantCulture,out v);}
  internal double ScaleFor(LibEntry e){double v;if(mode.SelectedIndex==1){if(!Number(width.Text,out v))throw new Exception("Nhập bề rộng mong muốn bằng số (đơn vị bản vẽ).");return Library.FitWidthScale(e,v);}if(!Number(scale.Text,out v))throw new Exception("Nhập tỉ lệ bằng số dương.");return Library.CheckScale(v);}
  internal double RotationDeg(){double v;if(!Number(rotation.Text,out v))throw new Exception("Nhập góc xoay bằng số độ.");return Library.CheckRotation(v);}
  // Live summary: toggle caption, selection size and explicit source/drawing units; invalid inputs are flagged in place.
  void ShowResult(){var e=Selected;string scaleText,rotText;bool badScale=false,badRot=false;
   try{scaleText=mode.SelectedIndex==1?(e==null?"theo bề rộng "+width.Text:Library.F(ScaleFor(e))+"× (bề rộng "+width.Text+")"):Library.F(ScaleFor(null))+"×";}catch(Exception){scaleText="tỉ lệ chưa hợp lệ";badScale=true;}
   try{rotText=Library.F(RotationDeg())+"°";}catch(Exception){rotText="góc chưa hợp lệ";badRot=true;}
   scale.BackColor=badScale&&mode.SelectedIndex==0?CBad:CField;width.BackColor=badScale&&mode.SelectedIndex==1?CBad:CField;rotation.BackColor=badRot?CBad:CField;
   toggle.Text=(Expanded?"▾ ":"▸ ")+"Tỉ lệ & góc: "+scaleText+" · "+rotText;toggle.ForeColor=badScale||badRot?CWarn:CFore;
   string drawing=drawingUnits==null?"bản vẽ: chưa đọc":"bản vẽ "+drawingUnits;
   units.Text="Nguồn "+(e==null?"(chọn khối)":e.units)+" → "+drawing+". Tỉ lệ 1 giữ số đo nguồn, KHÔNG tự đổi inch→mm.";tips.SetToolTip(units,units.Text+(drawingName!=null?"\n"+drawingName:""));
   if(e==null){selName.Text=Catalog==null?"":tiles.Count(t=>t.Listed)+" / "+Catalog.entries.Count+" khối";selSize.Text="Chọn một hình để xem kích thước.";tips.SetToolTip(selName,"");tips.SetToolTip(selSize,"");return;}
   selName.Text=(e.hidden?"[ẩn] ":"")+e.name;tips.SetToolTip(selName,Tip(e));
   try{double s=ScaleFor(e);selSize.Text="Gốc "+Library.F(Library.Width(e))+" × "+Library.F(Library.Height(e))+" "+e.units+" → đặt "+Library.F(Library.Width(e)*s)+" × "+Library.F(Library.Height(e)*s)+" đv bản vẽ";tips.SetToolTip(selSize,Library.SizeText(e,s));}
   catch(Exception ex){selSize.Text=ex.Message;tips.SetToolTip(selSize,ex.Message);}
   if(e.status!="OK")selSize.Text="CHƯA HỖ TRỢ, không đặt được. "+selSize.Text;}
  internal void Run(string action){try{Act(action);}catch(Exception ex){Message("Chưa thực hiện: "+ex.Message);}}
  internal void Act(string action){
   if(action=="import"){if(Root==null)throw new Exception("Chưa có thư mục thư viện.");if(Request!=null)Request("import",null);return;}
   if(action=="undo"){if(Request!=null)Request("undo",null);return;}
   if(action=="reload"){Reload();Message("Đã tải lại thư viện.");return;}
   var e=Selected;if(e==null)throw new Exception("Chọn một hình khối trong danh sách.");
   if(action=="place"){if(e.hidden)throw new Exception("Mục đang ẩn; khôi phục trước khi đặt.");if(e.status!="OK")throw new Exception("Mục "+e.name+" chưa được hỗ trợ để đặt.");var p=new LibPlacement{id=e.id,rotationDeg=RotationDeg(),fitWidth=mode.SelectedIndex==1};p.scale=ScaleFor(e);if(p.fitWidth){double w;Number(width.Text,out w);p.width=w;}if(Request!=null)Request("place",p);return;}
   if(action=="hide"){if(e.hidden)throw new Exception("Mục đã ẩn.");Library.SetHidden(Root,e.id,true);Reload();Message("Đã ẩn "+e.name+". Không xoá file nguồn, file thư viện hay khối đã đặt; ≡ → Hiện mục đã ẩn để khôi phục.");return;}
   if(action=="restore"){if(!e.hidden)throw new Exception("≡ → Hiện mục đã ẩn, chọn mục [ẩn] rồi khôi phục.");Library.SetHidden(Root,e.id,false);Reload();Message("Đã khôi phục "+e.name+".");return;}
   throw new Exception("Thao tác không hợp lệ: "+action);}
  protected override void Dispose(bool disposing){if(disposing){foreach(var b in cache.Values)b.Dispose();cache.Clear();tips.Dispose();options.Dispose();}base.Dispose(disposing);}
  // One library entry: green preview on a dark tile plus its exact block name (ellipsis; full name in the tooltip).
  internal sealed class Tile:Control {
   public readonly LibEntry Entry;readonly CompactLibraryView owner;public bool Selected,Listed=true;bool hot;
   public Tile(CompactLibraryView o,LibEntry e){owner=o;Entry=e;SetStyle(ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.UserPaint|ControlStyles.ResizeRedraw|ControlStyles.Selectable,true);
    Name="tile_"+e.id;Text=e.name;AccessibleName=e.name;AccessibleRole=AccessibleRole.ListItem;TabStop=false;Margin=new Padding(3);BackColor=CTile;ForeColor=e.hidden?CMuted:e.status!="OK"?CWarn:CFore;Font=new Font("Segoe UI",8.25f);Cursor=Cursors.Hand;}
   internal Rectangle PreviewRect{get{return new Rectangle((Width-PreviewSize)/2,4,PreviewSize,PreviewSize);}}
   internal Rectangle NameRect{get{return new Rectangle(4,PreviewSize+6,Width-8,Height-PreviewSize-8);}}
   protected override void OnPaint(PaintEventArgs pe){var g=pe.Graphics;g.Clear(CTile);g.DrawImage(owner.Thumb(Entry),PreviewRect);
    TextRenderer.DrawText(g,(Entry.hidden?"[ẩn] ":"")+Entry.name,Font,NameRect,ForeColor,TextFormatFlags.EndEllipsis|TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.SingleLine|TextFormatFlags.NoPrefix);
    using(var pen=new Pen(Selected?CTeal:hot?CMuted:CLine,Selected?2:1)){float o=pen.Width/2;g.DrawRectangle(pen,o,o,Width-pen.Width,Height-pen.Width);}
    if(Focused&&Selected)ControlPaint.DrawFocusRectangle(g,new Rectangle(3,3,Width-6,Height-6),CFore,CTile);}
   protected override void OnMouseEnter(EventArgs e){hot=true;Invalidate();base.OnMouseEnter(e);}
   protected override void OnMouseLeave(EventArgs e){hot=false;Invalidate();base.OnMouseLeave(e);}
   protected override void OnMouseDown(MouseEventArgs e){owner.Choose(this,true);base.OnMouseDown(e);}
   protected override void OnDoubleClick(EventArgs e){owner.Run("place");base.OnDoubleClick(e);}
   protected override bool IsInputKey(Keys k){return k==Keys.Left||k==Keys.Right||k==Keys.Up||k==Keys.Down||k==Keys.Home||k==Keys.End||k==Keys.Enter||base.IsInputKey(k);}
   protected override void OnKeyDown(KeyEventArgs e){if(owner.Key(e.KeyCode)){e.Handled=true;e.SuppressKeyPress=true;}else base.OnKeyDown(e);}
   protected override void OnGotFocus(EventArgs e){Invalidate();base.OnGotFocus(e);}
   protected override void OnLostFocus(EventArgs e){Invalidate();base.OnLostFocus(e);}
  }
 }
}
