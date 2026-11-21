using System;using System.Drawing;using System.Drawing.Drawing2D;using System.Windows.Forms;using System.Collections.Generic;
namespace Tvxd.Meps {
 // TVXD-MEPS-LEGEND-011: Esc in the palette cancels a queued request (CancelRequest, F3); 'Khôi phục…' appears only when the library file is unreadable
 // (Recover, F8); long names are ellipsized on screen only (F10); dark sample colours get a light underlay so they stay visible (F13).
 // TVXD-MEPS-WIREMENU-013: the section is ONE rounded full-width bar "Chọn dây dẫn ▾" (label/height per WIRELABEL-014) (no permanent list or status label; the rest of the lower half stays free).
 // The bar opens a bounded, scrollable menu prepared in advance: presets, captured samples, 'Khôi phục…' only when the library is unreadable, '+ Lấy mẫu'
 // last. Opening, refilling, closing and Esc send nothing; choosing an item closes the menu first, then sends exactly one callback. Messages go to the
 // parent status and the bar tooltip. Public API unchanged for WirePlugin010.
 // TVXD-MEPS-WIRELABEL-014: the bar now reads "Chọn dây dẫn ▾" and is 26 logical px high (was 30); width, fonts, rounding, menu and callbacks unchanged.
 // TVXD-MEPS-WIRESCROLL-018 (WIRESCROLL018 only): the bar opens a dark popup holding a native owner-drawn ListBox with an always-visible draggable
 // scrollbar and mouse wheel; the ContextMenuStrip stays the hidden row model and never opens. Without the flag the 016 menu is unchanged.
 // TVXD-MEPS-FLEXUI-029 F029-UI-01 (FLEX022 only): the bar is one tile wide beside "Ống gió mềm", so its VISIBLE caption is CompactBarText ("Dây dẫn ▾")
 // in the palette's shared action font (CompactView06.ActionFont). Accessible name and tooltip keep the full "Chọn dây dẫn…"; popup, scroll, callbacks unchanged.
 // TVXD-MEPS-WIREQTY-034 (ROUTE034 only): 'Thiết lập tuyến…' and 'Bóc khối lượng…' at the top of the same popup, then a separator, then the unchanged list;
 // they forward to CompactView06.Request (RouteRequest). QA: ClickRoute(action). Without the flag the popup is unchanged.
 // TVXD-MEPS-ROOMQTY-037 (BDGKL034): only 'Bảng khối lượng điện…' is listed at the top; the two legacy entries are not created (non-BDGKL034 builds keep them).
 // S9 PATCH02 (ROOMCAD037 only): that entry's tooltip and click status describe the room table it opens; other builds keep their text.
 public sealed class WireView010:UserControl {
  public const int BarHeight=26,MenuWidth=300,MenuHeight=360,NameWidth=220,PreviewWidth=36,PreviewHeight=14;
#if WIRECOMMIT018
  // TVXD-MEPS-WIRESCROLL-018 (WIRECOMMIT018): every segment is saved when its endpoint is chosen; Esc stops and keeps it; U removes the last segment.
  public const string BarText="Chọn dây dẫn  ▾",BarTip="Chọn dây dẫn: bấm để chọn nét chuẩn, mẫu đã lấy hoặc + Lấy mẫu. Chọn điểm; mỗi đoạn lưu ngay. Esc dừng và giữ nét; U bỏ đoạn vừa vẽ. Xong tuyến: Enter/Space tại dòng Command vẽ tuyến mới cùng loại.";
#else
  public const string BarText="Chọn dây dẫn  ▾",BarTip="Chọn dây dẫn: bấm để chọn nét chuẩn, mẫu đã lấy hoặc + Lấy mẫu; chọn điểm trên Model, Enter xong · Esc huỷ. Xong tuyến: Enter/Space tại dòng Command vẽ tuyến mới cùng loại.";
#endif
  // 029: visible caption of the half-width bar in the action row (FLEX022); BarText stays the full caption of the full-width bar of other builds.
#if UI031
  // TVXD-MEPS-UI-031: uppercase action caption (no ▾; the bar keeps AccessibleRole ButtonDropDown and its tooltip).
  public const string CompactBarText="DÂY DẪN",AccessibleBarName="Chọn dây dẫn: mở danh sách nét";
  static readonly Color BarBack=CompactView06.UiBack,BarFill=CompactView06.UiField,BarLine=CompactView06.UiLine,BarHover=CompactView06.UiHover,BarFore=CompactView06.UiFore;
#else
  static readonly Color BarBack=CompactLibraryView.CBack,BarFill=CompactLibraryView.CField,BarLine=CompactLibraryView.CLine,BarHover=CompactLibraryView05.CHover,BarFore=CompactLibraryView.CFore;
  public const string CompactBarText="Dây dẫn ▾",AccessibleBarName="Chọn dây dẫn: mở danh sách nét";
#endif
  readonly CompactView06 parent;internal readonly CompactLibraryView05.RoundButton bar;internal readonly ContextMenuStrip menu=new ContextMenuStrip();readonly ToolTip tips=new ToolTip{ShowAlways=true};
  readonly List<Image> images=new List<Image>();bool recovery;
  public Action CaptureSample,CancelRequest,Recover,ManageCatalog;public Action<WireChoice061> DrawChoice;WireCatalog061 userCatalog;public void SetCatalog(WireCatalog061 catalog){userCatalog=catalog;Fill();}public Action<WireStyle010> Draw;
  IList<WireStyle010> samples=new List<WireStyle010>();
#if WIRESCROLL018
  // Rows map 1:1 to menu items (same order); headings, separators and the empty note never act. Up/Down/Home/End/PgUp/PgDn and the wheel only move the
  // selection or scroll; Enter, or press+release on the same row, closes the popup first, then sends exactly one callback. Esc/outside click send nothing.
  public const int RowHeight=24,PopupMinWidth=240;
  internal readonly ScrollPopup018 scrollPopup=new ScrollPopup018();internal readonly ListBox scrollList;readonly ToolStripControlHost scrollHost;readonly ToolTip listTips=new ToolTip{ShowAlways=true};
  readonly Dictionary<ToolStripItem,Row018> rowOf=new Dictionary<ToolStripItem,Row018>();internal readonly List<Row018> rows=new List<Row018>();
  bool filling,closedOverBar;int hot=-1,pressed=-1,lastSel=-1,closedTick;string lastPick;
  const TextFormatFlags RowFlags=TextFormatFlags.NoPrefix|TextFormatFlags.SingleLine|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis|TextFormatFlags.Left;
  internal sealed class Row018{public ToolStripItem item;public string text,tip;public Action act;public Image image;public bool heading,separator;
   public bool Selectable{get{return act!=null&&!separator&&item.Enabled;}}public override string ToString(){return separator?"":item.AccessibleName??text;}}
  // Keys reach the popup's ProcessCmdKey whether the list or the drop-down holds the keyboard, so navigation is handled once, here.
  internal sealed class ScrollPopup018:ToolStripDropDown{public Func<Keys,bool> Key;
   protected override bool ProcessCmdKey(ref System.Windows.Forms.Message msg,Keys keyData){if(Key!=null&&Key(keyData))return true;return base.ProcessCmdKey(ref msg,keyData);}}
  sealed class Dark018:ToolStripRenderer{protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e){using(var b=new SolidBrush(CompactLibraryView.CField))e.Graphics.FillRectangle(b,e.AffectedBounds);}
   protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e){using(var p=new Pen(CompactLibraryView.CLine))e.Graphics.DrawRectangle(p,0,0,e.ToolStrip.Width-1,e.ToolStrip.Height-1);}}
  // Dark scrollbar where the OS theme exists (Windows 10 1809+); otherwise the standard scrollbar stays.
  sealed class List018:ListBox{[System.Runtime.InteropServices.DllImport("uxtheme.dll",CharSet=System.Runtime.InteropServices.CharSet.Unicode)]static extern int SetWindowTheme(IntPtr h,string app,string id);
   protected override void OnHandleCreated(EventArgs e){base.OnHandleCreated(e);try{SetWindowTheme(Handle,"DarkMode_Explorer",null);}catch(Exception){}}}
#endif
#if STANDARDS012
  // TVXD-MEPS-LAYERS-012: profile presets first in the menu; choosing one only sends the preset to DrawPreset.
  public Action<WirePreset012> DrawPreset;IList<WirePreset012> presets=new List<WirePreset012>();
  public void SetPresets(IList<WirePreset012> list){presets=list??new List<WirePreset012>();Fill();}
#if CABLES016
  Font headingFont;
#endif
  // Display-only swatch (never saved): the layer's ACI; dashed presets show an illustrative dash, the real pattern comes from acadiso.lin when drawing.
  static WireStyle010 Swatch(WirePreset012 p){int packed=Autodesk.AutoCAD.Colors.EntityColor.LookUpRgb((byte)p.aci);var dashes=p.metric?new[]{2.0,-1.0}:new double[0];
   return new WireStyle010{name=p.name,layer=p.layer,linetype=p.linetype,aci=p.aci,red=(packed>>16)&255,green=(packed>>8)&255,blue=packed&255,lineweight=p.weight,dashes=dashes,patternLength=p.metric?3.0:0};}
#endif
  int S(int n){return parent.S(n);}void Safe(Action a){try{if(a!=null)a();}catch(Exception ex){Message(ex.Message,true);}}
  public WireView010(CompactView06 owner){parent=owner;AutoScaleMode=AutoScaleMode.None;BackColor=BarBack;ForeColor=BarFore;Font=owner.TextFont;Dock=DockStyle.Top;Height=S(BarHeight);
#if FLEX022
   // TVXD-MEPS-FLEXUI-029: one tile wide beside "Ống gió mềm" (CompactView06.FlexBelow/ArrangeActions): short visible caption, shared action font sized so
   // both action captions fit at the minimum palette width (F029-UI-01). Height, popup size/scroll, tooltip, accessible name and callbacks unchanged.
   string barText=CompactBarText;var barFont=owner.ActionFont;
#else
   string barText=BarText;var barFont=owner.TitleFont;
#endif
   bar=new CompactLibraryView05.RoundButton{Text=barText,AccessibleName=AccessibleBarName,AccessibleRole=AccessibleRole.ButtonDropDown,Dock=DockStyle.Fill,Margin=new Padding(0),Font=barFont};
   bar.Colors(BarFill,BarLine,BarHover,ForeColor);bar.Click+=(s,e)=>Open();
   menu.Font=owner.TextFont;menu.ShowItemToolTips=true;menu.ShowImageMargin=true;menu.ImageScalingSize=new Size(S(PreviewWidth),S(PreviewHeight));menu.MaximumSize=new Size(S(MenuWidth),S(MenuHeight));
#if WIRESCROLL018
   scrollList=new List018{DrawMode=DrawMode.OwnerDrawFixed,ItemHeight=S(RowHeight),IntegralHeight=false,ScrollAlwaysVisible=true,SelectionMode=SelectionMode.One,BorderStyle=BorderStyle.None,
    BackColor=CompactLibraryView.CField,ForeColor=CompactLibraryView.CFore,Font=owner.TextFont,AccessibleName="Chọn dây dẫn: danh sách nét",AccessibleRole=AccessibleRole.List};
   scrollList.DrawItem+=DrawRow;scrollList.SelectedIndexChanged+=(s,e)=>Selected();
   scrollList.MouseDown+=(s,e)=>{pressed=e.Button==MouseButtons.Left?RowAt(e.Location):-1;};
   scrollList.MouseUp+=(s,e)=>{int i=e.Button==MouseButtons.Left?RowAt(e.Location):-1,p=pressed;pressed=-1;if(i>=0&&i==p)ActivateRow(i);};
   scrollList.MouseMove+=(s,e)=>Hot(RowAt(e.Location));scrollList.MouseLeave+=(s,e)=>Hot(-1);
   scrollHost=new ToolStripControlHost(scrollList){AutoSize=false,Margin=Padding.Empty,Padding=Padding.Empty,AccessibleName="Chọn dây dẫn"};
   scrollPopup.Renderer=new Dark018();scrollPopup.Padding=new Padding(1);scrollPopup.BackColor=CompactLibraryView.CField;scrollPopup.Items.Add(scrollHost);scrollPopup.Key=PopupKey;
   scrollPopup.Closed+=(s,e)=>{Hot(-1);pressed=-1;closedOverBar=e.CloseReason==ToolStripDropDownCloseReason.AppClicked&&bar.RectangleToScreen(bar.ClientRectangle).Contains(MousePosition);closedTick=Environment.TickCount;};
#endif
   Controls.Add(bar);tips.SetToolTip(bar,BarTip);SetStyles(new List<WireStyle010>());}
  // Only shows the prepared menu (no callback, no CAD call); a second click closes it.
#if WIRESCROLL018
  // The bar click that just closed the popup (outside click on the bar) does not reopen it. Width >= bar when possible, bounded MenuWidth x MenuHeight.
  internal void Open(){if(scrollPopup.Visible){CloseScroll();return;}bool toggled=closedOverBar&&unchecked(Environment.TickCount-closedTick)<400;closedOverBar=false;if(toggled||filling)return;
   int w=Math.Min(S(MenuWidth),Math.Max(PopupBasis,S(PopupMinWidth))),h=Math.Min(S(MenuHeight)-2,Math.Max(3*scrollList.ItemHeight,rows.Count*scrollList.ItemHeight));
   scrollHost.Size=new Size(w-2,h);scrollList.Size=scrollHost.Size;Hot(-1);pressed=-1;
   int keep=lastPick==null?-1:rows.FindIndex(r=>r.Selectable&&r.ToString()==lastPick);scrollList.SelectedIndex=keep;if(keep<0&&rows.Count>0)scrollList.TopIndex=0;
   scrollPopup.Show(bar,new Point(0,bar.Height));scrollList.Focus();}
#if FLEX022
  // 029: the popup keeps the width it had under the former full-width bar (the action row's width), not the half-width button's.
  int PopupBasis{get{return Parent!=null?Math.Max(bar.Width,Parent.ClientSize.Width):bar.Width;}}
#else
  int PopupBasis{get{return bar.Width;}}
#endif
  internal void CloseScroll(){if(scrollPopup.Visible)scrollPopup.Close(ToolStripDropDownCloseReason.CloseCalled);}
  // Single activation path (mouse, Enter, GPT QA): only a selectable row of the current model acts; headings, separators, the empty note, stale
  // indexes and calls during a refill do nothing. The popup closes BEFORE the one callback.
  internal bool ActivateRow(int index){if(filling||index<0||index>=rows.Count||index>=scrollList.Items.Count)return false;var r=rows[index];
   if(!ReferenceEquals(scrollList.Items[index],r)||!r.Selectable||r.item.IsDisposed||r.item.Owner!=menu)return false;lastPick=r.ToString();Run(r.act);return true;}
  void Run(Action act){CloseScroll();if(menu.Visible)menu.Close(ToolStripDropDownCloseReason.ItemClicked);Safe(act);}
  bool PopupKey(Keys k){if(!scrollPopup.Visible)return false;int sel=scrollList.SelectedIndex,n=rows.Count,page=Math.Max(1,scrollList.ClientSize.Height/Math.Max(1,scrollList.ItemHeight)-1);
   switch(k){case Keys.Escape:CloseScroll();return true;case Keys.Enter:ActivateRow(sel);return true;
    case Keys.Up:Step(sel<0?n-1:sel-1,-1);return true;case Keys.Down:Step(sel+1,1);return true;case Keys.Home:Step(0,1);return true;case Keys.End:Step(n-1,-1);return true;
    case Keys.PageUp:Step(sel<0?0:sel-page,-1);return true;case Keys.PageDown:Step(sel<0?page:sel+page,1);return true;default:return false;}}
  // Moves the selection only (never acts): nearest selectable row from 'from' in 'dir', else the other way; nothing selectable keeps the selection.
  void Step(int from,int dir){int n=rows.Count;if(n==0)return;from=Math.Max(0,Math.Min(n-1,from));
   for(int d=dir,pass=0;pass<2;pass++,d=-d)for(int i=from;i>=0&&i<n;i+=d)if(rows[i].Selectable){scrollList.SelectedIndex=i;return;}}
  // A native click/Shift-key on a heading, separator or the empty note puts the selection back on the last selectable row.
  void Selected(){int i=scrollList.SelectedIndex;if(filling||i<0||i>=rows.Count)return;if(rows[i].Selectable){lastSel=i;return;}scrollList.SelectedIndex=lastSel<rows.Count&&lastSel>=0&&rows[lastSel].Selectable?lastSel:-1;}
  int RowAt(Point pt){int i=scrollList.IndexFromPoint(pt);return i>=0&&i<rows.Count&&scrollList.GetItemRectangle(i).Contains(pt)?i:-1;}
  // Hover highlight + full-name tooltip (names can be ellipsized on screen).
  void Hot(int i){if(i==hot)return;int old=hot;hot=i;if(old>=0&&old<scrollList.Items.Count)scrollList.Invalidate(scrollList.GetItemRectangle(old));if(i>=0)scrollList.Invalidate(scrollList.GetItemRectangle(i));
   listTips.SetToolTip(scrollList,i>=0&&!rows[i].separator?rows[i].tip:null);}
  // Rebuilds the list from the hidden menu model in the same order; Clear already closed the popup and emptied the old rows.
  void Rows(){scrollList.BeginUpdate();try{scrollList.Items.Clear();rows.Clear();hot=-1;pressed=-1;lastSel=-1;
   foreach(ToolStripItem i in menu.Items){Row018 r;bool sep=i is ToolStripSeparator;if(!rowOf.TryGetValue(i,out r))r=new Row018{item=i,separator=sep,heading=!sep&&i.Font.Bold,text=sep?"":i.Text.Replace("&&","&"),tip=sep?null:i.AccessibleName??i.Text};rows.Add(r);}
   scrollList.Items.AddRange(rows.ToArray());}finally{scrollList.EndUpdate();}}
  void DrawRow(object sender,DrawItemEventArgs e){var g=e.Graphics;var b=e.Bounds;bool any=e.Index>=0&&e.Index<rows.Count;var r=any?rows[e.Index]:null;
   using(var bg=new SolidBrush(r!=null&&r.heading?CompactLibraryView.CBack:CompactLibraryView.CField))g.FillRectangle(bg,b);if(r==null)return;
   if(r.separator){using(var p=new Pen(CompactLibraryView.CLine))g.DrawLine(p,b.Left+S(6),b.Top+b.Height/2,b.Right-S(6),b.Top+b.Height/2);return;}
   int x=b.Left+S(6);
   if(r.heading){TextRenderer.DrawText(g,r.text,r.item.Font,new Rectangle(x,b.Top,Math.Max(1,b.Right-x-S(4)),b.Height),CompactLibraryView.CTealHot,RowFlags);
    using(var p=new Pen(CompactLibraryView.CLine))g.DrawLine(p,b.Left,b.Bottom-1,b.Right,b.Bottom-1);return;}
   bool sel=r.Selectable&&(e.State&DrawItemState.Selected)!=0,hov=r.Selectable&&e.Index==hot;
   if(sel||hov)using(var bg=new SolidBrush(sel?CompactLibraryView05.CArmed:CompactLibraryView05.CHover))g.FillRectangle(bg,b);
   if(sel)using(var a=new SolidBrush(CompactLibraryView.CTeal))g.FillRectangle(a,b.Left,b.Top,S(3),b.Height);
   if(r.image!=null)g.DrawImage(r.image,x,b.Top+(b.Height-r.image.Height)/2,r.image.Width,r.image.Height);
   int tx=x+S(PreviewWidth)+S(6);TextRenderer.DrawText(g,r.text,scrollList.Font,new Rectangle(tx,b.Top,Math.Max(1,b.Right-tx-S(4)),b.Height),r.Selectable?CompactLibraryView.CFore:CompactLibraryView.CMuted,RowFlags);
   if(sel&&(e.State&DrawItemState.Focus)!=0)ControlPaint.DrawFocusRectangle(g,Rectangle.Inflate(b,-1,-1),CompactLibraryView.CFore,CompactLibraryView05.CArmed);}
#else
  internal void Open(){if(menu.Visible){menu.Close();return;}menu.MinimumSize=new Size(Math.Min(bar.Width,S(MenuWidth)),0);menu.Show(bar,new Point(0,bar.Height));}
#endif
  // F3: Esc while the palette has focus cancels a request that has not started yet; with the menu open, Esc only closes the menu.
  protected override bool ProcessCmdKey(ref System.Windows.Forms.Message msg,Keys keyData){if(keyData==Keys.Escape&&menu.Visible){menu.Close();return true;}
#if WIRESCROLL018
   if(keyData==Keys.Escape&&scrollPopup.Visible){CloseScroll();return true;}
#endif
   if(keyData==Keys.Escape&&CancelRequest!=null){Safe(CancelRequest);return true;}return base.ProcessCmdKey(ref msg,keyData);}
#if ROUTE034
  // TVXD-MEPS-WIREQTY-034: the current 'Thiết lập tuyến…' / 'Bóc khối lượng…' entries (rebuilt with the menu; always null under BDGKL034, 037).
  internal ToolStripMenuItem routeSetup,routeReport;
#if BDGKL034
  internal ToolStripMenuItem routeBdgkl;
#endif
  // Forwards to the owning palette's Request (Plugin.LibraryAction → Enqueue → TVXDMEPS060DO); no queue of its own. Refusals surface through Safe → Message.
  void RouteRequest(string action){if(parent.Request==null)throw new InvalidOperationException("bảng chưa nối với AutoCAD.");
   if(parent.Armed!=null)throw new InvalidOperationException("Đang chờ đặt ký hiệu; ESC trước.");parent.Request(action,null);
   Message(action==RouteUi034.Setup?"Thiết lập tuyến: chọn MỘT tuyến LINE/LWPOLYLINE trên Model (xem dòng lệnh); hộp khai báo sẽ mở. ESC huỷ, bản vẽ không đổi.":
#if BDGKL034 && ROOMCAD037
    // S9 PATCH02: RoomUi037.Run opens the table first; Model is read only after CHỌN VÙNG CAD (no scan at this click).
    action==BdgklUi034.Action?BdgklUi034.Title+": đang mở bảng khối lượng (chưa quét Model; quét khi bấm CHỌN VÙNG CAD).":
#elif BDGKL034
    action==BdgklUi034.Action?BdgklUi034.Title+": đang quét Model (chỉ đọc); bảng sẽ mở.":
#endif
    "Bóc khối lượng: đang quét Model (chỉ đọc)…",false);}
  // QA hook: activates the ACTUAL menu entry of this action through the same path as the mouse/Enter (WIRESCROLL018: ActivateRow on its row, which also
  // checks the row is current and selectable; otherwise ToolStripItem.PerformClick). false = the entry is not active now (refill, disabled, disposed).
  public bool ClickRoute(string action){var it=action==RouteUi034.Setup?routeSetup:action==RouteUi034.Report?routeReport:
#if BDGKL034
   action==BdgklUi034.Action?routeBdgkl:
#endif
   null;
   if(!RouteUi034.Handles(action))throw new ArgumentException("Không phải mục tuyến: "+action);if(it==null||it.IsDisposed)return false;
#if WIRESCROLL018
   return ActivateRow(rows.FindIndex(r=>ReferenceEquals(r.item,it)));
#else
   if(!it.Enabled||it.Owner!=menu)return false;it.PerformClick();return true;
#endif
  }
#endif
  public void ShowRecovery(bool on){recovery=on;Fill();}
  public void SetStyles(IList<WireStyle010> styles){recovery=false;samples=styles??new List<WireStyle010>();Fill();}
  void Clear(){if(menu.Visible)menu.Close();
#if WIRESCROLL018
   // Close and empty the popup before items/images are disposed, so nothing stale can paint or act.
   CloseScroll();scrollList.Items.Clear();rows.Clear();rowOf.Clear();hot=-1;pressed=-1;lastSel=-1;listTips.SetToolTip(scrollList,null);
#endif
   var old=new List<ToolStripItem>();foreach(ToolStripItem i in menu.Items)old.Add(i);menu.Items.Clear();foreach(var i in old)i.Dispose();foreach(var b in images)b.Dispose();images.Clear();}
#if WIRESCROLL018
  // While the model is rebuilt no row can act (ActivateRow/Open check 'filling'); the list is rebuilt even if a fill step throws.
  void Fill(){filling=true;try{FillMenu();}finally{try{Rows();}finally{filling=false;}}}
  void FillMenu(){Clear();
#else
  void Fill(){Clear();
#endif
#if ROUTE034
   // TVXD-MEPS-WIREQTY-034: route items at the TOP, then a separator, then the unchanged type list. Same Item/row/activation path as every entry (the popup
   // closes first, then ONE callback); the callback only forwards to the owning CompactView06.Request (the plugin's queued bound-document job).
#if BDGKL034
   // TVXD-MEPS-ROOMQTY-037: ONE quantity entry 'Bảng khối lượng điện…' (action bdgkl034-table, same Request path). The legacy 'Thiết lập tuyến…' /
   // 'Bóc khối lượng…' entries are not created and their references are cleared on every rebuild, so ClickRoute(Setup/Report) returns false.
   // Backends, saved route records and RouteUi034.Run dispatch are unchanged.
   routeSetup=null;routeReport=null;
#if ROOMCAD037
   // S9 PATCH02: tooltip follows the room table (RoomUi037) actually opened by this entry; caption, action and callback unchanged.
   routeBdgkl=Item(null,BdgklUi034.Caption,BdgklUi034.Title,"Mở "+BdgklUi034.Title+" theo phòng: DỰ ÁN / TẦNG / PHÒNG → tạo dự án, tầng, phòng; CHỌN VÙNG CAD → chọn phòng đã tạo, "+
    "đơn vị MM hoặc M (khác đơn vị bản vẽ phải tích xác nhận), dung sai nối, chọn hai góc phòng; bổ sung thông số còn thiếu; LƯU VÀO BẢN VẼ giữ bản nháp; tổng chính thức và XUẤT EXCEL chỉ khi mọi dòng đủ dữ liệu.",
    ()=>RouteRequest(BdgklUi034.Action));menu.Items.Add(routeBdgkl);
#else
   routeBdgkl=Item(null,BdgklUi034.Caption,BdgklUi034.Title,"Mở "+BdgklUi034.Title+" (pilot một tuyến): xem bản ghi đã lưu; Thêm tuyến/Sửa tuyến: chọn tuyến, block điểm đầu và điểm cuối, "+
    "khai báo cao độ/hướng đi/bộ dây/ống → Lưu.",()=>RouteRequest(BdgklUi034.Action));menu.Items.Add(routeBdgkl);
#endif
#else
   routeSetup=Item(null,RouteUi034.SetupCaption,"Thiết lập tuyến dây/cáp","Chọn MỘT tuyến LINE/LWPOLYLINE trên Model, khai báo khu vực, mạch/lộ, loại cáp, cách tính, số sợi, bổ sung, dự phòng, đơn vị → Lưu.",
    ()=>RouteRequest(RouteUi034.Setup));menu.Items.Add(routeSetup);
   routeReport=Item(null,RouteUi034.ReportCaption,"Bóc khối lượng dây/cáp","Quét Model (chỉ đọc): tổng theo khu vực/mạch/loại cáp, chi tiết từng tuyến, tuyến không được tính; xuất CSV.",
    ()=>RouteRequest(RouteUi034.Report));menu.Items.Add(routeReport);
#endif
   menu.Items.Add(new ToolStripSeparator());
#endif
menu.Items.Add(Item(null,"QUẢN LÝ DÂY DẪN…","Quản lý dây dẫn","Thêm/xóa nhóm và dây, sửa màu, kiểu nét, độ dày; lưu mặc định.",()=>{if(ManageCatalog!=null)ManageCatalog();}));
if(userCatalog!=null){foreach(var group in userCatalog.groups){var list=userCatalog.wires.FindAll(w=>w.group==group);if(list.Count==0)continue;if(headingFont==null)headingFont=new Font(menu.Font,FontStyle.Bold);menu.Items.Add(new ToolStripMenuItem(group){Enabled=false,Font=headingFont});foreach(var choice in list){var pick=choice;menu.Items.Add(Item(userCatalog.Style(choice,4,1,new WireSettings010{msltscale=0,annoScale=1}),choice.name,"Vẽ "+choice.name,choice.color+" · "+choice.pattern+" · "+choice.weight/100.0+" mm",()=>{if(DrawChoice!=null)DrawChoice(pick);}));}}}else {
#if STANDARDS012 && CABLES016
   // TVXD-MEPS-CABLES-016: the 19 user cables, each group under a small disabled heading (PVC / XLPE 2 lõi / XLPE 4 lõi); same bounded menu, one
   // callback per item after the menu closes. Tooltip: layer, code, section, cores (PVC: unspecified). Not a T_CHUAN classification.
   string heading=null;if(headingFont==null)headingFont=new Font(menu.Font.FontFamily,menu.Font.Size*0.9f,FontStyle.Bold,menu.Font.Unit);
   foreach(var p in presets){var preset=p;var c=Cable016.Find(p.layer);
    if(p.kind!=heading){heading=p.kind;menu.Items.Add(new ToolStripMenuItem(heading){Enabled=false,Font=headingFont,AccessibleName="Nhóm "+heading});}
    string tip=c==null?p.name+"\nLayer "+p.layer:c.label+"\nLayer "+c.layer+" · mã "+c.code+"\nCách điện "+c.insulation+" · tiết diện "+Cable016.SectionText(c.section)+" mm² · số lõi: "+c.CoresText
     +"\nDanh mục cáp người dùng ("+Cable016.Provenance+"), không lấy từ "+Standards012.SourceFile+"; nét như "+Cable016.AliasOf+": ACI "+p.aci+" · "+p.linetype+" · LW "+p.weight/100.0+" mm, ByLayer."
     +"\nGhi mã loại cáp lên tuyến; không tính chiều dài, số lượng hay chọn cỡ cáp.";
    menu.Items.Add(Item(Swatch(p),p.name,"Vẽ cáp "+p.name,tip,()=>{if(DrawPreset!=null)DrawPreset(preset);}));}
   if(presets.Count>0)menu.Items.Add(new ToolStripSeparator());
#elif STANDARDS012
   foreach(var p in presets){var preset=p;menu.Items.Add(Item(Swatch(p),p.name,"Vẽ nét chuẩn "+p.name,p.name+"\nNét chuẩn "+Standards012.SourceFile+": layer "+p.layer+" · "+p.linetype+" · LW "+p.weight/100.0+" mm · ACI "+p.aci
     +"\nMàu/nét/độ đậm ByLayer; lấy theo bản vẽ hiện hành lúc bấm."+(p.metric?"\nNét gạch từ acadiso.lin: chỉ bản vẽ mm.":"")+"\nKhông lưu vào thư viện mẫu.",()=>{if(DrawPreset!=null)DrawPreset(preset);}));}
   if(presets.Count>0)menu.Items.Add(new ToolStripSeparator());
#endif
   }
   if(userCatalog==null&&samples.Count==0)menu.Items.Add(new ToolStripMenuItem("Chưa có mẫu. Bấm + Lấy mẫu."){Enabled=false});
   if(userCatalog==null)foreach(var s in samples){var sample=s;menu.Items.Add(Item(sample,s.name,"Vẽ nét "+s.name,s.name+"\nLayer: "+s.layer+" · "+s.linetype+" · LW "+s.lineweight/100.0+" mm"+(s.msltscale<0?"\nMẫu cũ: chưa ghi MSLTSCALE — lấy mẫu lại để vẽ.":"\nMSLTSCALE "+s.msltscale+(s.msltscale==1?" · tỷ lệ chú thích "+(s.annoName??""):""))
     +"\nTỷ lệ theo mẫu; độc lập tỷ lệ ký hiệu.",()=>{if(Draw!=null)Draw(sample);}));}
   menu.Items.Add(new ToolStripSeparator());
   if(recovery)menu.Items.Add(Item(null,"Khôi phục…","Khôi phục thư viện nét hỏng","Giữ bản hỏng thành tệp mới, khôi phục từ bản sao lưu .bak (nếu hợp lệ). Có hỏi xác nhận.",()=>{if(Recover!=null)Recover();}));
   menu.Items.Add(Item(null,"+ Lấy mẫu","Lấy mẫu nét từ bản vẽ","Chọn LINE/LWPOLYLINE 2D trong Model. Chỉ đọc nét nguồn.",()=>{if(CaptureSample!=null)CaptureSample();}));}
  // One menu entry: on-screen name ellipsized (full name in the tooltip); the click closes the menu BEFORE its single callback.
  ToolStripMenuItem Item(WireStyle010 preview,string name,string accessible,string tip,Action act){Image image=preview==null?null:Preview(preview);if(image!=null)images.Add(image);
   var i=new ToolStripMenuItem(Fit(name)){Image=image,ImageScaling=ToolStripItemImageScaling.None,AccessibleName=accessible,ToolTipText=tip,AutoToolTip=false};
#if WIRESCROLL018
   name=name??"";rowOf[i]=new Row018{item=i,text=name,tip=tip==null?name:tip.StartsWith(name)?tip:name+"\n"+tip,act=act,image=image};
   i.Click+=(o,e)=>Run(act);return i;}
#else
   i.Click+=(o,e)=>{menu.Close(ToolStripDropDownCloseReason.ItemClicked);Safe(act);};return i;}
#endif
  string Fit(string text){text=text??"";int max=S(NameWidth);var flags=TextFormatFlags.NoPrefix|TextFormatFlags.SingleLine;
   if(TextRenderer.MeasureText(text,menu.Font,Size.Empty,flags).Width>max){int n=text.Length;while(n>1&&TextRenderer.MeasureText(text.Substring(0,n)+"…",menu.Font,Size.Empty,flags).Width>max)n--;text=text.Substring(0,n)+"…";}
   return text.Replace("&","&&");}
  // Stroke preview on a small dark tile (display only).
  Bitmap Preview(WireStyle010 s){int w=S(PreviewWidth),h=S(PreviewHeight);var b=new Bitmap(w,h);
   using(var g=Graphics.FromImage(b)){g.SmoothingMode=SmoothingMode.AntiAlias;g.Clear(Color.Transparent);using(var path=CompactLibraryView05.RoundRect(new RectangleF(0,0,w-1,h-1),3*parent.Dpi))using(var bg=new SolidBrush(CompactLibraryView.CTile))g.FillPath(bg,path);
    Stroke(g,s,new RectangleF(S(4),0,w-S(8),h),parent.Dpi);}return b;}
  public void Message(string text,bool error){bar.Colors(BarFill,error?CompactLibraryView.CWarn:BarLine,BarHover,ForeColor);tips.SetToolTip(bar,BarTip+(string.IsNullOrEmpty(text)?"":"\n"+text));
   if(error)parent.Warn(text);else parent.Message(text);}
  protected override void Dispose(bool disposing){if(disposing){Clear();menu.Dispose();tips.Dispose();
#if WIRESCROLL018
    scrollPopup.Dispose();scrollList.Dispose();listTips.Dispose();
#endif
#if STANDARDS012 && CABLES016
    if(headingFont!=null)headingFont.Dispose();
#endif
   }base.Dispose(disposing);}
  // Relative luminance below this on the dark tile: draw a light underlay first (the sample colour itself is not changed).
  const double DarkLimit=0.25;
  static double Luminance(Color c){Func<int,double> ch=v=>{double x=v/255.0;return x<=0.03928?x/12.92:Math.Pow((x+0.055)/1.055,2.4);};return 0.2126*ch(c.R)+0.7152*ch(c.G)+0.0722*ch(c.B);}
  static void Dashes(Graphics g,Pen p,WireStyle010 s,float left,float right,float y,float dpi){if(s.dashes.Length==0){g.DrawLine(p,left,y,right,y);return;}float x=left;int i=0;
   while(x<right&&i<100){double d=s.dashes[i%s.dashes.Length];float len=(float)Math.Max(dpi,Math.Abs(d)/s.patternLength*18*dpi);if(d>=0)g.DrawLine(p,x,y,Math.Min(right,x+(d==0?dpi:len)),y);x+=len;i++;}}
  internal static void Stroke(Graphics g,WireStyle010 s,RectangleF r,float dpi){
   // Captured RGB components also describe indexed colors; ACI7 remains white on this dark tile.
   Color c=!s.rgb&&s.aci==7?Color.White:Color.FromArgb(s.red,s.green,s.blue);float y=r.Top+r.Height/2;
   if(Luminance(c)<DarkLimit)using(var under=new Pen(Color.FromArgb(200,CompactLibraryView.CFore),Math.Max(3,3*dpi))){under.StartCap=under.EndCap=LineCap.Round;Dashes(g,under,s,r.Left,r.Right,y,dpi);}
   using(var p=new Pen(c,Math.Max(2,2*dpi)))Dashes(g,p,s,r.Left,r.Right,y,dpi);}
 }
}
