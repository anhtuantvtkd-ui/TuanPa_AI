using System;using System.Linq;using System.Collections.Generic;using Autodesk.AutoCAD.DatabaseServices;using Autodesk.AutoCAD.Runtime;
#if !FLOORS_CORE
using Autodesk.AutoCAD.ApplicationServices;using App=Autodesk.AutoCAD.ApplicationServices.Application;
[assembly:CommandClass(typeof(Tvxd.Meps.FloorsCad054))]
#endif
namespace Tvxd.Meps {
 public class FloorsCad054 {
  public const string Key="TVXD_MEPS_FLOORS054";
#if !FLOORS_CORE
static Document queued;
  static void Cancelled(object sender,CommandEventArgs e){ClearQueue();}
  static void ClearQueue(){var old=queued;queued=null;if(old!=null){try{old.CommandCancelled-=Cancelled;old.CommandFailed-=Cancelled;}catch{}}}
  public static void Queue(){var d=App.DocumentManager.MdiActiveDocument;if(d==null)return;if(Convert.ToInt32(App.GetSystemVariable("CMDACTIVE"))!=0){d.Editor.WriteMessage("\nKết thúc lệnh đang chạy trước khi mở MẶT CẮT TẦNG.");return;}if(queued!=null)return;queued=d;d.CommandCancelled+=Cancelled;d.CommandFailed+=Cancelled;try{d.SendStringToExecute("T-MCT ",true,false,false);}catch{ClearQueue();throw;}}
#endif
  public static string Read(Database db,Transaction tr){var nod=(DBDictionary)tr.GetObject(db.NamedObjectsDictionaryId,OpenMode.ForRead);if(!nod.Contains(Key))return null;var xr=tr.GetObject(nod.GetAt(Key),OpenMode.ForRead) as Xrecord;if(xr==null)throw new InvalidOperationException("Khóa mặt cắt không phải Xrecord; không ghi đè.");using(var rb=xr.Data){if(rb==null)throw new InvalidOperationException("Mặt cắt rỗng.");var values=rb.AsArray();if(values.Any(v=>v.TypeCode!=(int)DxfCode.Text))throw new InvalidOperationException("Mặt cắt sai định dạng.");var raw=string.Concat(values.Select(v=>(string)v.Value));Floors054.Decode(raw);return raw;}}
  public static void Save(Database db,Floors054 data,string expected){string raw=data.Encode();using(var tr=db.TransactionManager.StartTransaction()){if(Read(db,tr)!=expected)throw new InvalidOperationException("Mặt cắt đã thay đổi; mở lại trước khi lưu.");if(raw==expected)return;var nod=(DBDictionary)tr.GetObject(db.NamedObjectsDictionaryId,OpenMode.ForRead);Xrecord xr;if(nod.Contains(Key))xr=(Xrecord)tr.GetObject(nod.GetAt(Key),OpenMode.ForWrite);else{nod.UpgradeOpen();xr=new Xrecord();nod.SetAt(Key,xr);tr.AddNewlyCreatedDBObject(xr,true);}var vs=new List<TypedValue>();for(int i=0;i<raw.Length;){int n=Math.Min(240,raw.Length-i);if(i+n<raw.Length&&char.IsHighSurrogate(raw[i+n-1]))n--;vs.Add(new TypedValue((int)DxfCode.Text,raw.Substring(i,n)));i+=n;}using(var rb=new ResultBuffer(vs.ToArray()))xr.Data=rb;if(Read(db,tr)!=raw)throw new InvalidOperationException("Đọc lại mặt cắt không khớp.");tr.Commit();}}
#if !FLOORS_CORE
  [CommandMethod("T-MCT",CommandFlags.Modal|CommandFlags.NoUndoMarker)]public void Open(){var d=App.DocumentManager.MdiActiveDocument;var requested=queued;ClearQueue();if(d==null)return;if(requested!=null&&requested!=d){d.Editor.WriteMessage("\nMẶT CẮT: Bản vẽ đã đổi. Bấm lại nút.");return;}try{string raw;using(var tr=d.Database.TransactionManager.StartOpenCloseTransaction()){raw=Read(d.Database,tr);}using(var f=new FloorsUi054(raw==null?Floors054.New():Floors054.Decode(raw),null,System.IO.Path.Combine(Plugin.LibraryRoot,"pipe-rules.json"))){if(App.ShowModalDialog(f)!=System.Windows.Forms.DialogResult.OK)return;if(App.DocumentManager.MdiActiveDocument!=d)throw new InvalidOperationException("Bản vẽ đã đổi; chưa lưu.");if(f.Result.Encode()!=raw){d.Editor.Command("_.UNDO","_BEGIN");try{Save(d.Database,f.Result,raw);}finally{d.Editor.Command("_.UNDO","_END");}}d.Editor.WriteMessage("\nĐã lưu MẶT CẮT TẦNG trong bản vẽ. Ctrl+S để lưu DWG ra đĩa.");}}catch(System.Exception ex){d.Editor.WriteMessage("\nMẶT CẮT: "+ex.Message);}}
#endif
 }
}
