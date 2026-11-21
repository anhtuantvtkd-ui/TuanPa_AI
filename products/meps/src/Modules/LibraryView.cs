using System;using System.Drawing;using System.Linq;using System.Windows.Forms;using System.Globalization;using System.Collections.Generic;
namespace Tvxd.Meps {
 // Tab "3 · Thư viện khối": searchable thumbnail browser over the normalized library (OUTPUT/LIBRARY_0_3_1), placement options and add/hide/restore.
 // Hide/restore only edit catalog.json; import and placement are sent to the plugin through Request.
 public sealed class LibraryView:UserControl {
  public const int Thumb=96;
  internal readonly TextBox search=new TextBox{Width=150};
  internal readonly ComboBox category=new ComboBox{DropDownStyle=ComboBoxStyle.DropDownList,Width=120};
  internal readonly CheckBox showHidden=new CheckBox{Text="Hiện mục đã ẩn",AutoSize=true,Margin=new Padding(8,4,3,0)};
  internal readonly ListView browser=new ListView{Dock=DockStyle.Fill,View=View.LargeIcon,MultiSelect=false,HideSelection=false,ShowItemToolTips=true,BackColor=Color.White};
  internal readonly ImageList images=new ImageList{ImageSize=new Size(Thumb,Thumb),ColorDepth=ColorDepth.Depth24Bit};
  internal readonly Label info=new Label{Dock=DockStyle.Fill,AutoSize=false,AutoEllipsis=true,ForeColor=Color.FromArgb(13,69,86)},result=new Label{Dock=DockStyle.Fill,AutoSize=false,AutoEllipsis=true};
  internal readonly ComboBox mode=new ComboBox{DropDownStyle=ComboBoxStyle.DropDownList};
  internal readonly TextBox scale=new TextBox{Text="1"},rotation=new TextBox{Text="0"},width=new TextBox{Text=""};
  internal readonly Dictionary<string,Button> actions=new Dictionary<string,Button>();
  public string Root;public LibCatalog Catalog;public Action<string,LibPlacement> Request;public Action<string> Message=s=>{};
  public const string RawMode="Tỉ lệ (1 = thô 1×)",FitMode="Co theo bề rộng";
  // Top row 36 px: the flow panel keeps 30 px after its 3 px cell margins, enough for the 23 px combo and the 22 px check box with its top margin.
  // Button row 42 px: 3 px cell margins leave 36 px for the 28 px buttons plus their 3 px top/bottom margins, with 2 px spare.
  public LibraryView(){Dock=DockStyle.Fill;var root=new TableLayoutPanel{Dock=DockStyle.Fill,RowCount=6,ColumnCount=1,Margin=new Padding(0)};foreach(int h in new[]{36,0,50,60,42,64})root.RowStyles.Add(h==0?new RowStyle(SizeType.Percent,100):new RowStyle(SizeType.Absolute,h));Controls.Add(root);
   var top=new FlowLayoutPanel{Dock=DockStyle.Fill,WrapContents=false};top.Controls.Add(new Label{Text="Tìm",AutoSize=true,Margin=new Padding(3,7,3,0)});top.Controls.Add(search);top.Controls.Add(category);top.Controls.Add(showHidden);root.Controls.Add(top,0,0);
   browser.LargeImageList=images;root.Controls.Add(browser,0,1);root.Controls.Add(info,0,2);
   mode.Items.AddRange(new object[]{RawMode,FitMode});mode.SelectedIndex=0;var fields=new FlowLayoutPanel{Dock=DockStyle.Fill,WrapContents=false};Panel.Field(fields,"Cách đặt",mode,170);Panel.Field(fields,"Tỉ lệ",scale,70);Panel.Field(fields,"Góc (độ)",rotation,60);Panel.Field(fields,"Bề rộng muốn",width,90);root.Controls.Add(fields,0,3);
   var buttons=new FlowLayoutPanel{Dock=DockStyle.Fill,WrapContents=false};buttons.Controls.Add(B("Đặt khối lên Model","libplace"));buttons.Controls.Add(B("Thêm DWG/DXF…","libimport"));buttons.Controls.Add(B("Ẩn mục chọn","libhide"));buttons.Controls.Add(B("Khôi phục","librestore"));root.Controls.Add(buttons,0,4);root.Controls.Add(result,0,5);
   search.TextChanged+=(s,e)=>Fill();category.SelectedIndexChanged+=(s,e)=>Fill();showHidden.CheckedChanged+=(s,e)=>{Categories();Fill();};browser.SelectedIndexChanged+=(s,e)=>ShowSelected();
   foreach(var c in new Control[]{scale,rotation,width})c.TextChanged+=(s,e)=>ShowResult();mode.SelectedIndexChanged+=(s,e)=>{scale.Enabled=mode.SelectedIndex==0;width.Enabled=mode.SelectedIndex==1;ShowResult();};scale.Enabled=true;width.Enabled=false;
   Reload();}
  public void Reload(){string keep=Selected==null?null:Selected.id;Catalog=null;if(Root==null){info.Text="Chưa có thư mục thư viện.";Fill();return;}try{Catalog=Library.Load(Root);}catch(Exception ex){info.Text="Catalog lỗi, chưa sửa gì: "+ex.Message+" (bản trước: catalog.json.bak)";}Categories();Fill();if(keep!=null&&browser.Items.ContainsKey(keep)){browser.Items[keep].Selected=true;browser.Items[keep].EnsureVisible();}}
  void Categories(){string current=category.SelectedItem as string;category.Items.Clear();category.Items.AddRange(Catalog==null?new object[]{Library.All}:Library.Categories(Catalog,showHidden.Checked).Cast<object>().ToArray());int i=current==null?-1:category.Items.IndexOf(current);category.SelectedIndex=i>=0?i:0;}
  internal LibEntry[] Listed{get{return Catalog==null?new LibEntry[0]:Library.Search(Catalog,search.Text,category.SelectedItem as string,showHidden.Checked);}}
  // Thumbnails are rendered once per entry from its stored preview geometry; a missing or broken preview renders the explicit "no preview" tile.
  // Clearing the list raises SelectedIndexChanged; the filling flag stops that event from re-entering Fill.
  bool filling;
  void Fill(){filling=true;browser.BeginUpdate();try{browser.Items.Clear();foreach(var e in Listed){if(!images.Images.ContainsKey(e.id))images.Images.Add(e.id,Library.Render(Library.LoadPreview(Root,e),Thumb));
     var item=new ListViewItem((e.hidden?"[ẩn] ":"")+e.name,e.id){Name=e.id,Tag=e,ToolTipText=e.name+"\n"+e.category+" • nguồn "+e.sourceFile+"\nXem trước: "+e.previewStatus,ForeColor=e.hidden?Color.Gray:e.status!="OK"?Color.FromArgb(160,40,30):Color.Black};browser.Items.Add(item);}}finally{browser.EndUpdate();filling=false;}
   Summary();ShowResult();}
  void Summary(){if(Catalog==null)return;info.Text=browser.Items.Count+" / "+Catalog.entries.Count+" khối ("+Catalog.entries.Count(x=>x.hidden)+" đang ẩn). Chọn một hình để xem kích thước và đặt.";}
  internal LibEntry Selected{get{return browser.SelectedItems.Count==1?browser.SelectedItems[0].Tag as LibEntry:null;}}
  void ShowSelected(){if(filling)return;var e=Selected;if(e==null){Summary();ShowResult();return;}var warn=e.warnings.Where(w=>!w.StartsWith("INFO")).ToArray();info.Text=e.name+"  •  nhóm "+e.category+"  •  nguồn "+e.sourceFile+(e.hidden?"  •  ĐANG ẨN":"")+"\r\nKhung gốc "+Library.F(Library.Width(e))+" × "+Library.F(Library.Height(e))+" "+e.units+" (INSUNITS "+e.insunits+")  •  thuộc tính "+e.attributes.Count+"  •  xem trước "+e.previewStatus+(e.status!="OK"?"  •  CHƯA HỖ TRỢ: "+string.Join("; ",warn):warn.Length>0?"  •  "+string.Join("; ",warn):"");ShowResult();}
  static bool Number(string text,out double v){return double.TryParse(text,NumberStyles.Float,CultureInfo.CurrentCulture,out v)||double.TryParse(text,NumberStyles.Float,CultureInfo.InvariantCulture,out v);}
  internal double ScaleFor(LibEntry e){double v;if(mode.SelectedIndex==1){if(!Number(width.Text,out v))throw new Exception("Nhập bề rộng mong muốn bằng số (đơn vị bản vẽ).");return Library.FitWidthScale(e,v);}if(!Number(scale.Text,out v))throw new Exception("Nhập tỉ lệ bằng số dương.");return Library.CheckScale(v);}
  internal double RotationDeg(){double v;if(!Number(rotation.Text,out v))throw new Exception("Nhập góc xoay bằng số độ.");return Library.CheckRotation(v);}
  void ShowResult(){string note="Tỉ lệ 1 = giữ số đo nguồn, KHÔNG đổi inch→mm. Tách khỏi bảng tải; không suy W/loại từ tên.";var e=Selected;if(e==null){result.Text=note;return;}try{result.Text=Library.SizeText(e,ScaleFor(e))+"\r\n"+note;}catch(Exception ex){result.Text=ex.Message+"\r\n"+note;}}
  Button B(string caption,string action){var b=new Button{Text=caption,AutoSize=true,Height=28,FlatStyle=FlatStyle.Flat,BackColor=Color.White};actions[action]=b;b.Click+=(s,e)=>{try{Act(action);}catch(Exception ex){Message("Chưa thực hiện: "+ex.Message);}};return b;}
  void Act(string action){if(action=="libimport"){if(Root==null)throw new Exception("Chưa có thư mục thư viện.");if(Request!=null)Request("import",null);return;}var e=Selected;if(e==null)throw new Exception("Chọn một hình khối trong thư viện.");
   if(action=="libplace"){if(e.hidden)throw new Exception("Mục đang ẩn; bấm Khôi phục trước khi đặt.");if(e.status!="OK")throw new Exception("Mục "+e.name+" chưa được hỗ trợ để đặt.");var p=new LibPlacement{id=e.id,rotationDeg=RotationDeg(),fitWidth=mode.SelectedIndex==1};p.scale=ScaleFor(e);if(p.fitWidth){double w;Number(width.Text,out w);p.width=w;}if(Request!=null)Request("place",p);return;}
   if(action=="libhide"){if(e.hidden)throw new Exception("Mục đã ẩn.");Library.SetHidden(Root,e.id,true);Reload();Message("Đã ẩn "+e.name+" khỏi thư viện. Không xoá file nguồn, file thư viện hay khối đã đặt; bật Hiện mục đã ẩn để khôi phục.");return;}
   if(action=="librestore"){if(!e.hidden)throw new Exception("Bật Hiện mục đã ẩn rồi chọn mục [ẩn] cần khôi phục.");Library.SetHidden(Root,e.id,false);Reload();Message("Đã khôi phục "+e.name+".");}}
 }
}
