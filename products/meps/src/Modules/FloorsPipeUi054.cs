using System;using System.Drawing;using System.Linq;using System.Windows.Forms;
namespace Tvxd.Meps {
 public sealed partial class FloorsUi054 {
  DataGridView pipes;Label pipeNotice;string pipeFile,pipeExpected;bool pipeLoading,pipeDirty,pipeAvailable;
  TabPage BuildPipes(string path){
   pipeFile=path;var tab=new TabPage("CHỌN ỐNG"){Name="pipeRules057",BackColor=BackColor,Padding=new Padding(7)};
   var help=new Label{Dock=DockStyle.Top,Height=40,Text="KHAI BÁO MỘT LẦN · SỬA TRỰC TIẾP TRONG BẢNG\nLƯU MẶC ĐỊNH CHO BỘ THƯ VIỆN MEPS NÀY.",ForeColor=Color.Teal};
   pipes=new DataGridView{Name="pipeGrid057",Dock=DockStyle.Fill,BackgroundColor=Color.White,BorderStyle=BorderStyle.None,AllowUserToAddRows=false,AllowUserToDeleteRows=false,AllowUserToResizeRows=false,RowHeadersVisible=false,AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill,SelectionMode=DataGridViewSelectionMode.CellSelect,MultiSelect=false,EnableHeadersVisualStyles=false};
   pipes.Columns.Add("wire","CẤU HÌNH DÂY (MM²)");pipes.Columns.Add("pipe","CỠ ỐNG");pipes.Columns[0].FillWeight=260;pipes.Columns[1].FillWeight=90;
   foreach(DataGridViewColumn c in pipes.Columns)c.SortMode=DataGridViewColumnSortMode.NotSortable;
   pipes.ColumnHeadersHeight=34;pipes.RowTemplate.Height=23;pipes.ColumnHeadersDefaultCellStyle.BackColor=Color.FromArgb(231,240,244);pipes.ColumnHeadersDefaultCellStyle.Font=new Font(Font,FontStyle.Bold);pipes.ColumnHeadersDefaultCellStyle.Alignment=DataGridViewContentAlignment.MiddleCenter;pipes.DefaultCellStyle.Alignment=DataGridViewContentAlignment.MiddleCenter;pipes.DefaultCellStyle.SelectionBackColor=Color.FromArgb(206,233,239);pipes.DefaultCellStyle.SelectionForeColor=ForeColor;pipes.AlternatingRowsDefaultCellStyle.BackColor=Color.FromArgb(249,251,253);pipes.GridColor=Color.FromArgb(227,234,240);pipes.CellBorderStyle=DataGridViewCellBorderStyle.SingleHorizontal;
   pipes.CellValueChanged+=delegate{if(!pipeLoading){pipeDirty=true;PipeStatus("CHƯA LƯU · BẤM LƯU MẶC ĐỊNH.",false);}};
   pipes.DataError+=delegate(object s,DataGridViewDataErrorEventArgs e){e.ThrowException=false;PipeStatus("KIỂM TRA NỘI DUNG ĐANG NHẬP.",true);};
   var bar=new FlowLayoutPanel{Dock=DockStyle.Bottom,Height=34};
   var add=MakeButton("+ DÒNG",delegate{if(!pipeAvailable)return;pipes.EndEdit();int i=pipes.Rows.Add("","");pipes.CurrentCell=pipes.Rows[i].Cells[0];pipes.BeginEdit(true);pipeDirty=true;});
   var del=MakeButton("XÓA DÒNG",delegate{if(!pipeAvailable||pipes.CurrentRow==null)return;pipes.CancelEdit();pipes.Rows.Remove(pipes.CurrentRow);pipeDirty=true;PipeStatus("CHƯA LƯU · BẤM LƯU MẶC ĐỊNH.",false);});
   var save=MakeButton("LƯU MẶC ĐỊNH",delegate{SavePipes();});bar.Controls.AddRange(new Control[]{add,del,save});
   pipeNotice=new Label{Dock=DockStyle.Bottom,Height=38,ForeColor=Color.Teal};
   var card=new InputRoundCard055{Dock=DockStyle.Fill,BackColor=Color.White,Padding=new Padding(7)};card.Controls.Add(pipes);tab.Controls.Add(card);tab.Controls.Add(bar);tab.Controls.Add(pipeNotice);tab.Controls.Add(help);
   try{pipeExpected=PipeRules057.Read(path);var source=pipeExpected==null?PipeRules057.Examples():PipeRules057.Decode(pipeExpected);pipeLoading=true;foreach(var r in source.rows)pipes.Rows.Add(r.wire,r.pipe);pipeLoading=false;pipeAvailable=true;pipeDirty=false;PipeStatus(pipeExpected==null?"HAI DÒNG VÍ DỤ ANH ĐÃ CHỌN · BẤM LƯU MẶC ĐỊNH ĐỂ GIỮ.":"ĐÃ ĐỌC BẢNG MẶC ĐỊNH · "+source.rows.Count+" DÒNG.",false);}catch(Exception ex){pipeLoading=false;pipes.ReadOnly=true;add.Enabled=del.Enabled=save.Enabled=false;PipeStatus("KHÔNG ĐỌC ĐƯỢC BẢNG · "+ex.Message,true);}
   routeHelp.SetToolTip(save,"LƯU CHO CÁC BẢN VẼ DÙNG CÙNG BỘ THƯ VIỆN. CHƯA TỰ CHỌN ỐNG HOẶC THAY ĐỔI KHỐI LƯỢNG TRONG BẢN VẼ.");
   FormClosing+=delegate(object sender,FormClosingEventArgs e){if(!pipeAvailable)return;pipes.EndEdit();if(!pipeDirty)return;var answer=MessageBox.Show(this,"BẢNG CHỌN ỐNG CHƯA LƯU. LƯU MẶC ĐỊNH TRƯỚC KHI ĐÓNG?","CHỌN ỐNG",MessageBoxButtons.YesNoCancel,MessageBoxIcon.Question);if(answer==DialogResult.Cancel||(answer==DialogResult.Yes&&!SavePipes()))e.Cancel=true;};return tab;
  }
  void PipeStatus(string message,bool error){pipeNotice.Text=message.ToUpperInvariant();pipeNotice.ForeColor=error?Color.Firebrick:Color.Teal;}
  bool SavePipes(){if(!pipeAvailable)return false;try{if(!pipes.EndEdit())throw new InvalidOperationException("KẾT THÚC Ô ĐANG SỬA TRƯỚC KHI LƯU.");var d=new PipeRules057();foreach(DataGridViewRow row in pipes.Rows)d.rows.Add(new PipeRule057{wire=Convert.ToString(row.Cells[0].Value),pipe=Convert.ToString(row.Cells[1].Value)});pipeExpected=PipeRules057.Save(pipeFile,d,pipeExpected);pipeDirty=false;PipeStatus("ĐÃ LƯU MẶC ĐỊNH · "+d.rows.Count+" DÒNG.",false);return true;}catch(Exception ex){PipeStatus(ex.Message,true);return false;}}
 }
}
