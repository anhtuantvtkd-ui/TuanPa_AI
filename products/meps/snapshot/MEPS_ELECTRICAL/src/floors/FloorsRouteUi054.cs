using System;using System.Linq;using System.Drawing;using System.Windows.Forms;
namespace Tvxd.Meps {
 public sealed partial class FloorsUi054 {
  ToolTip routeHelp=new ToolTip{AutoPopDelay=15000,InitialDelay=400,ReshowDelay=150,ShowAlways=true};TextBox routeInput;Label routeCaption,routeApplied;Button routeReset;bool refreshingRoute;string routeFloorId,routeRoomId;
  Control BuildRouteBar(){
   var bar=new FlowLayoutPanel{Dock=DockStyle.Top,Height=34,Padding=new Padding(8,3,0,0),WrapContents=false};
   routeCaption=new Label{Width=174,Height=25,TextAlign=ContentAlignment.MiddleLeft};
   routeInput=new TextBox{Name="routeHeight056",Width=60,TextAlign=HorizontalAlignment.Center,Margin=new Padding(3,4,3,0)};
   routeApplied=new Label{Width=250,Height=25,TextAlign=ContentAlignment.MiddleLeft,ForeColor=Color.Teal};
   routeReset=MakeButton("DÙNG CHUNG TẦNG",delegate{Run(delegate{if(routeRoomId==null)return;data.SetRouteHeight(routeFloorId,routeRoomId,null);RefreshRoute();drawing.Invalidate();});});
   Disposed+=delegate{routeHelp.Dispose();};
   routeInput.TextChanged+=delegate{PreviewRoute();};
   routeInput.Validating+=delegate(object s,System.ComponentModel.CancelEventArgs e){if(!refreshingRoute&&!CommitRoute())e.Cancel=true;};
   routeInput.KeyDown+=delegate(object s,KeyEventArgs e){if(e.KeyCode==Keys.Escape){RefreshRoute();e.SuppressKeyPress=true;}else if(e.KeyCode==Keys.Enter){CommitRoute();e.SuppressKeyPress=true;}};
   bar.Controls.AddRange(new Control[]{routeCaption,routeInput,routeApplied,routeReset});return bar;
  }
  decimal? ParseRoute(){return string.IsNullOrWhiteSpace(routeInput.Text)?(decimal?)null:Floors054.Number(routeInput.Text);}
  bool CommitRoute(){if(refreshingRoute||routeInput==null||routeFloorId==null)return true;try{data.SetRouteHeight(routeFloorId,routeRoomId,ParseRoute());PreviewRoute();drawing.Invalidate();return true;}catch(Exception ex){status.Text=ex.Message;status.ForeColor=Color.Firebrick;return false;}}
  void RefreshRoute(){if(routeInput==null||loading)return;refreshingRoute=true;try{var floor=data.floors[Selected()];routeFloorId=floor.id;routeRoomId=RoomId();var own=routeRoomId==null?floor.routeHeight:floor.rooms.Single(r=>r.id==routeRoomId).routeHeight;
   routeCaption.Text="CAO ĐỘ ĐI ỐNG (M)";routeInput.Text=own.HasValue?own.Value.ToString("0.###"):"";routeReset.Visible=routeRoomId!=null;string help=routeRoomId==null?"CAO ĐỘ LẮP ĐẶT ĐOẠN ỐNG NẰM NGANG, TÍNH TỪ SÀN HOÀN THIỆN; KHÔNG PHẢI CHIỀU CAO TẦNG.\\nVÍ DỤ 3.2 M: CÁC PHÒNG TRONG TẦNG DÙNG 3.2 M, TRỪ PHÒNG CÓ GIÁ TRỊ RIÊNG.\\nNHÀ MỘT TẦNG KHAI BÁO NGAY TẠI TẦNG 1, KHÔNG THÊM TẦNG GIẢ.":"CHỈ ÁP DỤNG CHO PHÒNG ĐANG CHỌN, TÍNH TỪ SÀN HOÀN THIỆN CỦA PHÒNG.\\nVÍ DỤ TẦNG 3.2 M, PHÒNG VỆ SINH 2.6 M: NHẬP 2.6 Ở ĐÂY.\\nCÁC PHÒNG KHÁC VẪN 3.2 M. ĐỂ TRỐNG = DÙNG CHUNG TẦNG.";routeHelp.SetToolTip(routeCaption,help.Replace("\\n","\n"));routeHelp.SetToolTip(routeInput,help.Replace("\\n","\n"));routeHelp.SetToolTip(routeApplied,"CAO ĐỘ THỰC TẾ SẼ DÙNG: ƯU TIÊN GIÁ TRỊ RIÊNG CỦA PHÒNG; ĐỂ TRỐNG THÌ LẤY GIÁ TRỊ CHUNG CỦA TẦNG.");routeHelp.SetToolTip(routeReset,"BỎ GIÁ TRỊ RIÊNG CỦA PHÒNG NÀY VÀ TRỞ VỀ CAO ĐỘ CHUNG CỦA TẦNG. KHÔNG ĐỔI PHÒNG KHÁC.");
  }finally{refreshingRoute=false;}PreviewRoute();}
  void PreviewRoute(){if(refreshingRoute||routeFloorId==null)return;try{var value=ParseRoute();if(value<0)throw new InvalidOperationException();var effective=routeRoomId==null?value:value??data.floors.Single(f=>f.id==routeFloorId).routeHeight;
   routeApplied.Text=effective.HasValue?"ÁP DỤNG: "+effective.Value.ToString("0.###")+" M · "+(routeRoomId!=null&&value.HasValue?"RIÊNG PHÒNG":"CHUNG TẦNG"):"CHƯA KHAI BÁO CAO ĐỘ ĐI ỐNG";routeApplied.ForeColor=effective.HasValue?(routeRoomId!=null&&value.HasValue?Color.DarkOrange:Color.Teal):Color.Firebrick;
  }catch{routeApplied.Text="KIỂM TRA CAO ĐỘ ĐI ỐNG";routeApplied.ForeColor=Color.Firebrick;}}
 }
}
