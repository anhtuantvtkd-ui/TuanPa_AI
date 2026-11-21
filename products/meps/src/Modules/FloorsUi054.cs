using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace Tvxd.Meps {
 public sealed partial class FloorsUi054:Form {
  public Floors054 Result;
  Floors054 data;
  TreeView tree;
  TextBox datum;
  Label status;
  System.Windows.Forms.Panel drawing;
  Func<string,bool> linked;
  bool loading;
  public FloorsUi054(Floors054 source,Func<string,bool> isLinked,string pipeDefaultsPath=null){
   data=source.Clone();linked=isLinked;
   Text="MEPS · INPUT";Size=new Size(800,500);MinimumSize=new Size(760,460);
   StartPosition=FormStartPosition.CenterParent;Font=new Font("Segoe UI",7);BackColor=Color.FromArgb(240,245,249);ForeColor=Color.FromArgb(35,55,72);
   var top=new FlowLayoutPanel{Dock=DockStyle.Top,Height=34,Padding=new Padding(8,4,0,0)};
   top.Controls.Add(new Label{Text="CAO ĐỘ SÀN THẤP NHẤT (M)",AutoSize=true,Margin=new Padding(0,5,8,0)});
   datum=new TextBox{Width=65,TextAlign=HorizontalAlignment.Center,Text=data.datum.ToString(CultureInfo.InvariantCulture)};top.Controls.Add(datum);Controls.Add(top);
   top.Controls.Add(MakeButton("LƯU MẪU",SaveTemplate));top.Controls.Add(MakeButton("MỞ MẪU",OpenTemplate));var routeBar=BuildRouteBar();Controls.Add(routeBar);routeBar.BringToFront();var ceilingBar=BuildCeilingBar();Controls.Add(ceilingBar);ceilingBar.BringToFront();
   var bottom=new FlowLayoutPanel{Dock=DockStyle.Bottom,Height=36,Padding=new Padding(8,4,0,0)};Controls.Add(bottom);
   bottom.Controls.AddRange(new Control[]{
    MakeButton("+ TẦNG",delegate{EditFloor(true);}),
    MakeButton("+ PHÒNG",delegate{EditRoom(true);}),
    MakeButton("SỬA",delegate{if(RoomId()!=null)EditRoom(false);else EditFloor(false);}),
    MakeButton("XÓA",Delete),
    MakeButton("LƯU VÀO DWG",delegate{Run(delegate{if(!ValidateChildren()||!heights.EndEdit()||!CommitInputs())return;var c=data.Clone();c.datum=Floors054.Number(datum.Text);c.Validate();Result=c;DialogResult=DialogResult.OK;Close();});})});
   status=new Label{Dock=DockStyle.Bottom,Height=32,Padding=new Padding(9,3,7,0),Text="CHỌN PHÒNG → NHẬP CỘT RIÊNG; ĐỂ TRỐNG = DÙNG CHUNG.\nCAO ĐỘ TÍNH TỪ SÀN HOÀN THIỆN. LƯU VÀO DWG → CTRL+S."};status.TextChanged+=delegate{var upper=status.Text.ToUpperInvariant();if(status.Text!=upper)status.Text=upper;};Controls.Add(status);
   var split=new SplitContainer{Size=new Size(760,350),Dock=DockStyle.Fill,SplitterDistance=190,SplitterWidth=8,BackColor=BackColor};Controls.Add(split);split.BringToFront();
   tree=new TreeView{Dock=DockStyle.Fill,BorderStyle=BorderStyle.None,ItemHeight=23,HideSelection=false,FullRowSelect=true,ShowLines=true,Indent=16,BackColor=Color.White};
   var treeCard=new InputRoundCard055{Dock=DockStyle.Fill,BackColor=Color.White,Padding=new Padding(7)};treeCard.Controls.Add(tree);split.Panel1.Controls.Add(treeCard);
   tree.BeforeSelect+=delegate(object sender,TreeViewCancelEventArgs e){if(loading)return;if(!CommitInputs()||(heights!=null&&heights.IsCurrentCellInEditMode&&!heights.EndEdit()))e.Cancel=true;};
   tree.AfterSelect+=delegate{drawing.Invalidate();RefreshHeights();RefreshRoute();RefreshCeiling();};
   tree.NodeMouseDoubleClick+=delegate{if(RoomId()!=null)EditRoom(false);else EditFloor(false);};
   drawing=new System.Windows.Forms.Panel{Dock=DockStyle.Fill,BackColor=Color.FromArgb(21,35,48),AutoScroll=true};drawing.Paint+=PaintSection;
   drawing.MouseClick+=delegate(object s,MouseEventArgs e){int row=(e.Y-drawing.AutoScrollPosition.Y-50)/140;if(row>=0&&row<data.floors.Count){int i=data.floors.Count-1-row;tree.SelectedNode=tree.Nodes[i];}};
   var tabs=new TabControl{Dock=DockStyle.Fill,DrawMode=TabDrawMode.OwnerDrawFixed,SizeMode=TabSizeMode.Fixed,ItemSize=new Size(110,25)};tabs.DrawItem+=delegate(object s,DrawItemEventArgs e){var active=e.Index==tabs.SelectedIndex;var r=Rectangle.Inflate(e.Bounds,-3,-2);e.Graphics.SmoothingMode=System.Drawing.Drawing2D.SmoothingMode.AntiAlias;using(var path=new System.Drawing.Drawing2D.GraphicsPath()){int d=14;path.AddArc(r.Left,r.Top,d,d,180,90);path.AddArc(r.Right-d,r.Top,d,d,270,90);path.AddArc(r.Right-d,r.Bottom-d,d,d,0,90);path.AddArc(r.Left,r.Bottom-d,d,d,90,90);path.CloseFigure();using(var b=new SolidBrush(active?Color.FromArgb(24,102,119):Color.FromArgb(223,233,241)))e.Graphics.FillPath(b,path);}using(var font=new Font(Font.FontFamily,6.5f,FontStyle.Bold))TextRenderer.DrawText(e.Graphics,tabs.TabPages[e.Index].Text,font,r,active?Color.White:Color.FromArgb(45,70,88),TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter);};var section=new TabPage("MẶT CẮT TẦNG"){Padding=new Padding(5),BackColor=BackColor};section.Controls.Add(drawing);tabs.TabPages.Add(section);tabs.TabPages.Add(BuildHeights());if(pipeDefaultsPath!=null)tabs.TabPages.Add(BuildPipes(pipeDefaultsPath));split.Panel2.Controls.Add(tabs);
   RoundSurface(split.Panel1,12);RoundSurface(drawing,12);RoundSurface(this,14);
   datum.Validated+=delegate{Run(delegate{if(!CommitInputs())return;var c=data.Clone();c.datum=Floors054.Number(datum.Text);c.Validate();data=c;RefreshTree(Selected(),RoomId());});};
   RefreshTree(0,null);
  }
  Button MakeButton(string text,Action action){var b=new InputRoundButton055{Text=text,AutoSize=false,Font=Font,Width=Math.Max(48,TextRenderer.MeasureText(text,Font).Width+16),Height=24,FlatStyle=FlatStyle.Flat,BackColor=text.StartsWith("XÓA")?Color.FromArgb(252,229,229):text.Contains("MẪU")||text=="SỬA"||text=="ĐỔI TÊN"?Color.FromArgb(220,233,242):Color.FromArgb(24,102,119),ForeColor=text.StartsWith("XÓA")?Color.FromArgb(159,47,47):text.Contains("MẪU")||text=="SỬA"||text=="ĐỔI TÊN"?Color.FromArgb(35,67,87):Color.White,Padding=new Padding(7,2,7,2),Margin=new Padding(3)};b.FlatAppearance.BorderSize=0;RoundSurface(b,9);b.Click+=delegate{action();};return b;}
  void Run(Action a){try{a();}catch(Exception ex){status.Text=ex.Message;status.ForeColor=Color.Firebrick;}}
  int Selected(){var node=tree.SelectedNode;if(node==null)return 0;if(node.Parent!=null)node=node.Parent;return node.Index;}
  string RoomId(){var n=tree.SelectedNode;return n!=null&&n.Parent!=null?(string)n.Tag:null;}
  void RefreshTree(int index,string room){
   loading=true;var expanded=new HashSet<string>(tree.Nodes.Cast<TreeNode>().Where(n=>n.IsExpanded).Select(n=>(string)n.Tag));bool first=tree.Nodes.Count==0;
   tree.BeginUpdate();tree.Nodes.Clear();var z=data.Elevations();
   for(int i=0;i<data.floors.Count;i++){
    var f=data.floors[i];var n=new TreeNode(f.name.ToUpperInvariant()+"  ·  "+z[i].ToString("+0.###;-0.###;0.000")+" M"){Tag=f.id};tree.Nodes.Add(n);
    foreach(var r in f.rooms){var child=new TreeNode(r.name.ToUpperInvariant()){Tag=r.id};n.Nodes.Add(child);if(r.id==room)tree.SelectedNode=child;}
    if(i==index&&room==null)tree.SelectedNode=n;
    if(first||expanded.Contains(f.id)||i==index)n.Expand();
   }
   if(tree.SelectedNode==null)tree.SelectedNode=tree.Nodes[Math.Min(index,tree.Nodes.Count-1)];
   tree.EndUpdate();datum.Text=data.datum.ToString(CultureInfo.InvariantCulture);drawing.AutoScrollMinSize=new Size(0,data.floors.Count*140+80);loading=false;drawing.Invalidate();RefreshHeights();RefreshRoute();RefreshCeiling();
  }
  void EditFloor(bool insert){Run(delegate{if(!CommitInputs())return;
   int i=Selected();using(var f=new Form{Text=insert?"CHÈN PHÍA TRÊN "+data.floors[i].name:"SỬA TẦNG",ClientSize=new Size(380,185),StartPosition=FormStartPosition.CenterParent,Font=Font,FormBorderStyle=FormBorderStyle.FixedDialog,MaximizeBox=false,MinimizeBox=false}){
    RoundSurface(f,12);
    var name=new TextBox{Left=155,Top=20,Width=200,Text=insert?"":data.floors[i].name};
    var h=new TextBox{Left=155,Top=65,Width=200,Text=insert?"":data.floors[i].rise.ToString(CultureInfo.InvariantCulture),Enabled=insert||i>0};
    f.Controls.AddRange(new Control[]{new Label{Left=15,Top=24,Text="TÊN TẦNG",Width=135},name,new Label{Left=15,Top=65,Width=135,Height=45,Text="CAO TỪ SÀN DƯỚI (M)"},h});
    var ok=MakeButton("ÁP DỤNG",delegate{});ok.Location=new Point(155,125);ok.Width=115;f.Controls.Add(ok);f.AcceptButton=ok;
    ok.Click+=delegate{try{decimal rise=h.Enabled?Floors054.Number(h.Text):0;var c=data.Clone();if(insert)c.Insert(i,name.Text,rise);else{c.floors[i].name=name.Text.Trim();c.floors[i].rise=rise;c.Validate();}data=c;f.DialogResult=DialogResult.OK;}catch(Exception ex){MessageBox.Show(f,ex.Message,"Kiểm tra dữ liệu");}};
    if(f.ShowDialog(this)==DialogResult.OK)RefreshTree(insert?i+1:i,null);
   }
  });}
  void EditRoom(bool insert){Run(delegate{if(!CommitInputs())return;
   int i=Selected();string rid=RoomId();if(!insert&&rid==null)return;
   using(var f=new Form{Text=insert?"THÊM PHÒNG · "+data.floors[i].name:"SỬA PHÒNG",ClientSize=new Size(380,130),StartPosition=FormStartPosition.CenterParent,Font=Font,FormBorderStyle=FormBorderStyle.FixedDialog,MaximizeBox=false,MinimizeBox=false}){
    RoundSurface(f,12);
    var name=new TextBox{Left=15,Top=40,Width=345,Text=insert?"":data.floors[i].rooms.Single(r=>r.id==rid).name};
    f.Controls.Add(new Label{Text="MÃ / TÊN PHÒNG (KHÔNG TRÙNG)",AutoSize=true,Left=15,Top=12});f.Controls.Add(name);
    var ok=MakeButton("ÁP DỤNG",delegate{});ok.Location=new Point(245,85);ok.Width=115;f.Controls.Add(ok);f.AcceptButton=ok;
    ok.Click+=delegate{try{var c=data.Clone();if(insert){c.AddRoom(i,name.Text);rid=c.floors[i].rooms.Last().id;}else{c.floors[i].rooms.Single(r=>r.id==rid).name=name.Text.Trim();c.Validate();}data=c;f.DialogResult=DialogResult.OK;}catch(Exception ex){MessageBox.Show(f,ex.Message,"Kiểm tra dữ liệu");}};
    if(f.ShowDialog(this)==DialogResult.OK)RefreshTree(i,rid);
   }
  });}
  void Delete(){Run(delegate{
   int i=Selected();string room=RoomId();if(room==null&&data.floors[i].rooms.Count>0)throw new InvalidOperationException("Tầng đang có phòng. Xóa hoặc xử lý từng phòng trước để không mất thông tin.");
   string prompt=room==null?"Xóa tầng đã chọn? Cao độ phía trên sẽ tính lại.\nXóa sàn thấp nhất giữ cao độ các sàn còn lại.":"Xóa phòng đã chọn khỏi sơ đồ?";
   if(!ConfirmDelete(prompt))return;
   if(room==null)data.Delete(i,linked);else data.floors[i].rooms.RemoveAll(r=>r.id==room);
   RefreshTree(Math.Min(i,data.floors.Count-1),null);
  });}
  bool ConfirmDelete(string prompt){using(var f=new Form{Text="XÁC NHẬN XÓA",ClientSize=new Size(450,150),StartPosition=FormStartPosition.CenterParent,Font=Font,FormBorderStyle=FormBorderStyle.FixedDialog,MaximizeBox=false,MinimizeBox=false}){
   f.Controls.Add(new Label{Text=prompt,Left=15,Top=15,Width=420,Height=70});
   RoundSurface(f,12);var yes=MakeButton("XÓA",delegate{});yes.Location=new Point(160,100);yes.Width=150;yes.DialogResult=DialogResult.OK;
   var no=MakeButton("HỦY",delegate{});no.Location=new Point(325,100);no.Width=100;no.DialogResult=DialogResult.Cancel;
   f.Controls.AddRange(new Control[]{yes,no});f.AcceptButton=no;f.CancelButton=no;return f.ShowDialog(this)==DialogResult.OK;
  }}
  void PaintSection(object s,PaintEventArgs e){
   if(loading)return;var g=e.Graphics;g.TranslateTransform(0,drawing.AutoScrollPosition.Y);var z=data.Elevations();
   using(var pen=new Pen(Color.FromArgb(62,197,181),2))using(var text=new SolidBrush(Color.White))using(var small=new Font(Font.FontFamily,7)){
    g.DrawString("MẶT CẮT SƠ ĐỒ · KHÔNG THEO TỶ LỆ",small,text,15,15);
    for(int i=data.floors.Count-1;i>=0;i--){int y=140+(data.floors.Count-1-i)*140;g.DrawLine(pen,25,y,drawing.ClientSize.Width-30,y);
     g.DrawString(data.floors[i].name.ToUpperInvariant(),Font,text,25,y-27);g.DrawString(z[i].ToString("+0.###;-0.###;0.000")+" M",small,text,drawing.ClientSize.Width-115,y+8);
     PaintCeiling(g,i,y,small);var route=data.RouteHeight(data.floors[i].id,i==Selected()?RoomId():null);if(route.HasValue){var ceiling=DisplayCeiling(i);int ry=y-(ceiling.HasValue&&route.Value>ceiling.Value?92:52);using(var rp=new Pen(Color.FromArgb(244,182,75),1)){rp.DashStyle=System.Drawing.Drawing2D.DashStyle.Dash;g.DrawLine(rp,25,ry,drawing.ClientSize.Width-30,ry);}using(var rb=new SolidBrush(Color.FromArgb(244,182,75)))g.DrawString("CAO ĐỘ ĐI ỐNG: "+route.Value.ToString("0.###")+" M TỪ SÀN",small,rb,25,ry-18);}
     if(i>0)g.DrawString("↕ "+data.floors[i].rise.ToString("0.###")+" M",small,text,drawing.ClientSize.Width-115,y+24);
     if(tree.SelectedNode!=null&&Selected()==i)g.DrawEllipse(pen,12,y-5,10,10);
    }
   }
  }
 }
}
