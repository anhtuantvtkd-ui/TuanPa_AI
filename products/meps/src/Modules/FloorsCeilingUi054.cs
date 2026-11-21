using System;using System.Linq;using System.Drawing;using System.Windows.Forms;
namespace Tvxd.Meps {
 public sealed partial class FloorsUi054 {
  TextBox ceilingInput;Label ceilingApplied;Button ceilingReset;bool refreshingCeiling;string ceilingFloorId,ceilingRoomId;
  bool CommitInputs(){return CommitRoute()&&CommitCeiling();}
  Control BuildCeilingBar(){
   var bar=new FlowLayoutPanel{Dock=DockStyle.Top,Height=34,Padding=new Padding(8,3,0,0),WrapContents=false};
   var caption=new Label{Text="CAO ĐỘ TRẦN (M)",Width=174,Height=25,TextAlign=ContentAlignment.MiddleLeft};
   ceilingInput=new TextBox{Name="ceilingHeight060",Width=60,TextAlign=HorizontalAlignment.Center,Margin=new Padding(3,4,3,0)};
   ceilingApplied=new Label{Name="ceilingApplied060",Width=250,Height=25,TextAlign=ContentAlignment.MiddleLeft,ForeColor=Color.Teal};
   ceilingReset=MakeButton("DÙNG CHUNG TẦNG",delegate{Run(delegate{if(ceilingRoomId==null)return;data.SetCeilingHeight(ceilingFloorId,ceilingRoomId,null);RefreshCeiling();drawing.Invalidate();});});ceilingReset.Name="ceilingReset060";
   ceilingInput.TextChanged+=delegate{PreviewCeiling();};
   ceilingInput.Validating+=delegate(object s,System.ComponentModel.CancelEventArgs e){if(!refreshingCeiling&&!CommitCeiling())e.Cancel=true;};
   ceilingInput.KeyDown+=delegate(object s,KeyEventArgs e){if(e.KeyCode==Keys.Escape){RefreshCeiling();e.SuppressKeyPress=true;}else if(e.KeyCode==Keys.Enter){CommitCeiling();e.SuppressKeyPress=true;}};
   string help="CAO ĐỘ MẶT DƯỚI TRẦN HOÀN THIỆN TÍNH TỪ SÀN HOÀN THIỆN CỦA TẦNG.\nCHỌN TẦNG ĐỂ NHẬP CHUNG; CHỌN PHÒNG ĐỂ NHẬP RIÊNG. PHÒNG ĐỂ TRỐNG = DÙNG CHUNG TẦNG.\nNHÀ MỘT TẦNG KHAI BÁO TẠI TẦNG 1, KHÔNG CẦN THÊM TẦNG. CAO ĐỘ ĐI ỐNG KHAI BÁO ĐỘC LẬP.";
   routeHelp.SetToolTip(caption,help);routeHelp.SetToolTip(ceilingInput,help);routeHelp.SetToolTip(ceilingReset,"XÓA CAO ĐỘ TRẦN RIÊNG CỦA PHÒNG, LẤY LẠI CAO ĐỘ TRẦN CHUNG TẦNG.");
   bar.Controls.AddRange(new Control[]{caption,ceilingInput,ceilingApplied,ceilingReset});return bar;
  }
  decimal? ParseCeiling(){return string.IsNullOrWhiteSpace(ceilingInput.Text)?(decimal?)null:Floors054.Number(ceilingInput.Text);}
  bool CommitCeiling(){if(refreshingCeiling||ceilingInput==null||ceilingFloorId==null)return true;try{data.SetCeilingHeight(ceilingFloorId,ceilingRoomId,ParseCeiling());PreviewCeiling();drawing.Invalidate();return true;}catch(Exception ex){status.Text=ex.Message;status.ForeColor=Color.Firebrick;return false;}}
  void RefreshCeiling(){if(ceilingInput==null||loading)return;refreshingCeiling=true;try{var f=data.floors[Selected()];ceilingFloorId=f.id;ceilingRoomId=RoomId();var own=ceilingRoomId==null?f.ceilingHeight:f.rooms.Single(x=>x.id==ceilingRoomId).ceilingHeight;ceilingInput.Text=own.HasValue?own.Value.ToString("0.###"):"";ceilingReset.Visible=ceilingRoomId!=null;}finally{refreshingCeiling=false;}PreviewCeiling();}
  void PreviewCeiling(){if(refreshingCeiling||ceilingFloorId==null)return;try{var own=ParseCeiling();if(own<0)throw new InvalidOperationException();var value=ceilingRoomId==null?own:own??data.floors.Single(x=>x.id==ceilingFloorId).ceilingHeight;ceilingApplied.Text=value.HasValue?"ÁP DỤNG: "+value.Value.ToString("0.###")+" M · "+(ceilingRoomId!=null&&own.HasValue?"RIÊNG PHÒNG":"CHUNG TẦNG"):"CHƯA KHAI BÁO CAO ĐỘ TRẦN";ceilingApplied.ForeColor=!value.HasValue?Color.Firebrick:ceilingRoomId!=null&&own.HasValue?Color.DarkOrange:Color.Teal;}catch{ceilingApplied.Text="KIỂM TRA CAO ĐỘ TRẦN";ceilingApplied.ForeColor=Color.Firebrick;}if(drawing!=null)drawing.Invalidate();}
  decimal? DisplayCeiling(int i){var value=data.CeilingHeight(data.floors[i].id,i==Selected()?RoomId():null);if(i==Selected()&&ceilingInput!=null&&ceilingFloorId==data.floors[i].id)try{var pending=ParseCeiling();if(pending<0)return value;return ceilingRoomId==null?pending:pending??data.floors[i].ceilingHeight;}catch{}return value;}
  void PaintCeiling(Graphics g,int i,int y,Font font){var value=DisplayCeiling(i);if(!value.HasValue)return;var route=data.RouteHeight(data.floors[i].id,i==Selected()?RoomId():null);int cy=y-(route.HasValue&&route.Value>value.Value?52:92);using(var pen=new Pen(Color.FromArgb(139,180,255),1)){pen.DashStyle=System.Drawing.Drawing2D.DashStyle.Dot;g.DrawLine(pen,25,cy,drawing.ClientSize.Width-30,cy);}using(var brush=new SolidBrush(Color.FromArgb(139,180,255)))g.DrawString("TRẦN: "+value.Value.ToString("0.###")+" M TỪ SÀN",font,brush,25,cy-19);}
 }
}
