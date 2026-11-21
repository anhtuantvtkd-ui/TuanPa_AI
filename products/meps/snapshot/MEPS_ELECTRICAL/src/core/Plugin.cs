using System;using System.IO;using System.Linq;using System.Collections.Generic;using System.Runtime.InteropServices;using Autodesk.AutoCAD.Runtime;using Autodesk.AutoCAD.ApplicationServices;using Autodesk.AutoCAD.DatabaseServices;using Autodesk.AutoCAD.Geometry;using Autodesk.AutoCAD.EditorInput;using Autodesk.AutoCAD.Windows;using App=Autodesk.AutoCAD.ApplicationServices.Application;using Ex=System.Exception;
[assembly:CommandClass(typeof(Tvxd.Meps.Plugin))]
// The 0.6.0 native QA helper (TESTS/NativeTest008.cs, built as TVXD.Meps.QA060.dll) reads the palette's internal layout fields directly.
[assembly:System.Runtime.CompilerServices.InternalsVisibleTo("TVXD.Meps.QA060")]
namespace Tvxd.Meps {
 public sealed class Plugin {
  const string Key="TVXD_MEPS_V1",RatioKey="TVXD_MEPS05_DRAWING_SCALE";static PaletteSet palette,compactSet;static Panel panel;static CompactView06 compact;static Document bound,undoDoc;static long revision,undoRevision;static Job pending;static bool executing,hooked,persisted;static string paletteError,firstPlacement;static LibPlacement lastPlacement;static LegendResult lastLegend;static LegendPreflight lastPreflight;static List<string> lastNotes=new List<string>();
  static readonly PlaceQueue queue=new PlaceQueue();static PlacementLog placementLog;
  sealed class Job{public string action,handle;public Meta meta;public int rows,cols;public Document doc;public long version;public Matrix3d ucs;public UnitsValue units;public LibEntry target;}
  // Portable: the library folder sits next to the DLL (OUTPUT/LIBRARY_0_6 or the unpacked package). LIBRARY_0_6 is a COPY of LIBRARY_0_5 made before tests;
  // 0.6.0 writes only LIBRARY_0_6 (import, hide/restore, scoped legend remap) and never LIBRARY_0_5 or older libraries.
  public static string LibraryRoot{get{return Path.Combine(Path.GetDirectoryName(typeof(Plugin).Assembly.Location),Meps05.LibraryDir);}}
  public static string LogRoot{get{return Path.Combine(Path.GetDirectoryName(typeof(Plugin).Assembly.Location),Meps06.LogDir);}}
  // Read-only views for the native QA helper.
  public static PaletteSet CompactPalette{get{return compactSet;}}public static CompactView06 CompactView{get{return compact;}}public static bool PersistedLayout{get{return persisted;}}
  public static string LastPaletteError{get{return paletteError;}}public static string FirstPlacement{get{return firstPlacement;}}public static LibPlacement LastPlacement{get{return lastPlacement;}}public static LegendResult LastLegend{get{return lastLegend;}}
  public static LegendPreflight LastPreflight{get{return lastPreflight;}}public static List<string> LastInsertNotes{get{return lastNotes;}}public static string QueueState{get{return queue.State;}}public static bool Executing{get{return executing;}}
  public static string PlacementLogPath{get{return Log().FilePath(DateTime.UtcNow);}}public static string RatioDictionaryKey{get{return RatioKey;}}
  static PlacementLog Log(){if(placementLog==null)placementLog=new PlacementLog(LogRoot);return placementLog;}
  static void Trace(PlaceRequest r,string evt,string detail){if(r==null)return;Log().Write(r.id,evt,r.entryName,(DateTime.UtcNow-r.requested).TotalMilliseconds,detail);}
  // LWDEFAULT of this AutoCAD session, captured when a library copy is made/upgraded and recorded as provenance (never an engineering standard).
  public static LwOptions RuntimeLineweight(){int v=Convert.ToInt32(App.GetSystemVariable("LWDEFAULT"));return new LwOptions{runtimeDefault=v,provenance="LWDEFAULT="+v+" (thiết lập AutoCAD lúc nhập "+DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm",System.Globalization.CultureInfo.InvariantCulture)+" UTC, nguồn không có giá trị)"};}
  // Ratio values N0/N/base are stored only in the drawing they were used in (named-object dictionary), written inside the placement command (same as 0.5).
  public static double[] ReadRatio(Database db){try{using(var tr=db.TransactionManager.StartOpenCloseTransaction()){var nod=(DBDictionary)tr.GetObject(db.NamedObjectsDictionaryId,OpenMode.ForRead);if(!nod.Contains(RatioKey))return null;var x=(Xrecord)tr.GetObject(nod.GetAt(RatioKey),OpenMode.ForRead);
     using(var data=x.Data){var v=data==null?new TypedValue[0]:data.AsArray();if(v.Length<3)return null;var r=new[]{Convert.ToDouble(v[0].Value),Convert.ToDouble(v[1].Value),Convert.ToDouble(v[2].Value)};Meps05.RatioScale(r[2],r[0],r[1]);return r;}}}catch{return null;}}
  public static void WriteRatio(Transaction tr,Database db,LibPlacement p){Meps05.RatioScale(p.baseScale,p.refDenominator,p.drawingDenominator);var nod=(DBDictionary)tr.GetObject(db.NamedObjectsDictionaryId,OpenMode.ForWrite);Xrecord x;if(nod.Contains(RatioKey))x=(Xrecord)tr.GetObject(nod.GetAt(RatioKey),OpenMode.ForWrite);else{x=new Xrecord();nod.SetAt(RatioKey,x);tr.AddNewlyCreatedDBObject(x,true);}
   using(var data=new ResultBuffer(new TypedValue((int)DxfCode.Real,p.refDenominator),new TypedValue((int)DxfCode.Real,p.drawingDenominator),new TypedValue((int)DxfCode.Real,p.baseScale),new TypedValue((int)DxfCode.Text,"explicit-user")))x.Data=data;}
  // Revision counter: kept for the room/device/edit operations of MEPS060ADV (strict). Library placement does not use it: a zoom/pan may modify the view
  // record and must not cancel a pick; placement binds the exact Document, UCS and INSUNITS instead (PlaceContext) and the library file hash (Insert).
  static void Changed(object s,ObjectEventArgs e){revision++;}static void Erased(object s,ObjectErasedEventArgs e){revision++;}
  static void Bind(Document d){if(d==bound)return;if(bound!=null)try{bound.Database.ObjectModified-=Changed;bound.Database.ObjectAppended-=Changed;bound.Database.ObjectErased-=Erased;}catch{}bound=d;revision=0;pending=null;undoDoc=null;if(d!=null){d.Database.ObjectModified+=Changed;d.Database.ObjectAppended+=Changed;d.Database.ObjectErased+=Erased;}}
  // 0.6.0 (TVXD-MEPS-IMPLEMENT-008): MEPS060/MEPS060ADV/TVXDMEPS060DO/UNDO and new palette GUIDs, so it loads beside every earlier build (0.1–0.5.2).
  static void Hook(){if(hooked)return;hooked=true;App.DocumentManager.DocumentActivated+=(s,e)=>{Bind(e.Document);var dropped=queue.DocumentChanged(e.Document);if(dropped!=null){Trace(dropped,"CANCEL","đổi bản vẽ trước khi lệnh bắt đầu");Warn("Đã huỷ yêu cầu đặt "+dropped.entryName+" vì đổi bản vẽ; chưa thay đổi bản vẽ nào.");}
    if(panel!=null)panel.Display("Đã đổi bản vẽ — bấm Nhận / đọc lại",new Snapshot());if(compact!=null&&!queue.Busy)compact.Disarm();ShowDrawing();};}
  static void ShowDrawing(){if(compact==null)return;var d=App.DocumentManager.MdiActiveDocument;if(d==null)compact.SetDrawing(null,null,null,null);else compact.SetDrawing(d.Name,Path.GetFileName(d.Name),d.Database.Insunits.ToString(),ReadRatio(d.Database));}
  static void Say(string s){if(compact!=null)compact.Message(s);if(panel!=null)panel.Message(s);}
  static void Warn(string s){if(compact!=null)compact.Warn(s);if(panel!=null)panel.Message(s);try{var d=App.DocumentManager.MdiActiveDocument;if(d!=null)d.Editor.WriteMessage("\nTVXD MEPS "+Meps06.Version+": "+s);}catch{}}
  [DllImport("user32.dll")]static extern bool GetWindowRect(IntPtr hWnd,out RECT rect);
  [StructLayout(LayoutKind.Sequential)]struct RECT{public int Left,Top,Right,Bottom;}
  // Screen rectangle of the active drawing window (not the monitor, not the main frame); empty when CAD does not expose it.
  public static System.Drawing.Rectangle DrawingRect(){try{var d=App.DocumentManager.MdiActiveDocument;if(d!=null&&d.Window!=null){RECT r;if(GetWindowRect(d.Window.Handle,out r)&&r.Right>r.Left&&r.Bottom>r.Top)return System.Drawing.Rectangle.FromLTRB(r.Left,r.Top,r.Right,r.Bottom);}}catch{}return System.Drawing.Rectangle.Empty;}
  // MEPS060: compact symbol palette. First open (no saved layout of this GUID): floating at the top-left corner of the drawing window, client about
  // 208 × 500 logical px (× DPI). Later opens keep CAD's saved position/size/dock. Resize, dock and auto-hide stay available.
  const string LayoutMarker="TvxdMeps060Layout";
  [CommandMethod("MEPS060",CommandFlags.NoUndoMarker)]public void Open(){Hook();bool created=false;
   if(compactSet==null){created=true;compact=new CompactView06{Root=LibraryRoot,Request=LibraryAction,ShowModal=f=>App.ShowModalDialog(f)};compact.Reload();
    compactSet=new PaletteSet("MEPS "+Meps06.Version,new Guid("6A0E3C71-58D2-4B9F-9E14-0C7F2B860600")){MinimumSize=new System.Drawing.Size(compact.S(CompactView06.MinClientWidth),compact.S(300)),Style=PaletteSetStyles.ShowCloseButton|PaletteSetStyles.ShowAutoHideButton|PaletteSetStyles.ShowPropertiesMenu,DockEnabled=DockSides.Left|DockSides.Right};
    compactSet.Load+=(s,e)=>{try{persisted=Convert.ToBoolean(e.ConfigurationSection.ReadProperty(LayoutMarker,false));}catch{}};compactSet.Save+=(s,e)=>{try{e.ConfigurationSection.WriteProperty(LayoutMarker,true);}catch{}};
    compactSet.Add("Điện",compact);
#if WIRES010
    WirePlugin010.Attach(compact,()=>executing||pending!=null||queue.Busy||FlexBusy());
#endif
#if FLEX022
    // TVXD-MEPS-FLEXDUCT-022: palette bar → FlexDuct022.Queue (bound document/INSUNITS/UCS, TVXDFLEX022RUN). Busy is shared both ways with placement/wire/model.
    compact.RequestFlex=FlexAction;compact.FlexBelow();FlexDuct022.OtherBusy=()=>executing||pending!=null||queue.Busy||WireBusy();
#endif
#if LAYOUT030
    // TVXD-MEPS-LAYOUT-030: "Dàn trang ▾" menu → LayoutAction → Layout030.Queue. Busy shared both ways (FlexBusy covers layout for placement/wire/model).
    compact.RequestLayout=LayoutAction;Layout030.OtherBusy=NonLayoutBusy;Layout030.Say=Say;Layout030.Warn=Warn;
    FlexDuct022.OtherBusy=()=>executing||pending!=null||queue.Busy||WireBusy()||LayoutBusy();
#endif
#if LIBUPDATE023
    compact.RequestUpdate=UpdateAction;
#endif
   }
#if FLEX022
   // Registered at NETLOAD by FlexApp022 already; idempotent here (no database access).
   FlexDuct022.Initialize();
#endif
   Bind(App.DocumentManager.MdiActiveDocument);ShowDrawing();compactSet.Visible=true;
   bool first=created&&!persisted;if(first)FirstOpen();
   compact.Visible=true;compact.PerformLayout();Recover();
#if STANDARDS012
   RequestStandards();
#endif
  }
#if STANDARDS012
  // TVXD-MEPS-LAYERS-012: opening the palette prepares the profile layers of the active Model drawing in a separate normal command (MEPS060 itself is
  // NoUndoMarker and must not write); TVXDMEPS012PREP runs in the document context with its own UNDO step. Document events never write.
  static void RequestStandards(){var d=App.DocumentManager.MdiActiveDocument;if(d==null)return;
   if(!d.Database.TileMode){Warn("Layer chuẩn "+Standards012.Profile+": chưa chuẩn bị vì đang ở Layout; vào Model rồi mở lại bảng (hoặc đặt ký hiệu/nét, MEPS sẽ chuẩn bị lúc đó).");return;}
   prepFor=d;try{d.SendStringToExecute("TVXDMEPS012PREP ",true,false,false);}catch(Ex ex){prepFor=null;Warn("Chưa gửi được lệnh chuẩn bị layer chuẩn: "+ex.Message+". Layer sẽ được chuẩn bị khi đặt ký hiệu/nét.");}}
  public static StdReport012 LastStandards{get{return lastStandards;}}static StdReport012 lastStandards;
  // prepFor: the document whose palette opening queued TVXDMEPS012PREP. If another drawing is active when the queued command runs, nothing is written
  // there (the request belonged to the original drawing); a manual TVXDMEPS012PREP without a queued request acts on the active drawing.
  static Document prepFor;
  [CommandMethod("TVXDMEPS012PREP",CommandFlags.Modal)]public void PrepareStandards(){var d=App.DocumentManager.MdiActiveDocument;var origin=prepFor;prepFor=null;if(d==null)return;
   if(origin!=null&&origin!=d){Warn("Bản vẽ đã đổi trước khi chuẩn bị layer chuẩn: không tạo layer trong bản vẽ hiện tại (yêu cầu thuộc bản vẽ trước). Mở lại bảng MEPS trong bản vẽ cần chuẩn bị, hoặc đặt ký hiệu/nét để MEPS kiểm tra lúc đó.");return;}
   try{if(!d.Database.TileMode)throw new Ex("không ở Model");if((Convert.ToInt32(App.GetSystemVariable("UNDOCTL"))&1)==0||!d.Database.UndoRecording)throw new Ex("UNDO đang tắt; MEPS không tạo layer khi không hoàn tác được");
    var rep=StandardsCad012.Ensure(d.Database,null);lastStandards=rep;string msg="Layer chuẩn ("+Standards012.SourceFile+" + ACI 3 + E-DH): "+rep.Summary()+". CLAYER, đơn vị, kiểu chữ, thiết lập in giữ nguyên.";
    if(rep.Complete)Say(msg);else Warn(msg);d.Editor.WriteMessage("\nTVXD MEPS "+Meps06.Version+": "+msg);
#if IDENTITY015
    // TVXD-MEPS-IDENTITY-015: the drawing's code registry (definitions available, not quantities), own transaction in this command; no symbol is cloned.
    try{var idr=IdentityCad015.EnsureProject(d.Database,Library.Load(LibraryRoot));lastIdentity=idr;string im="Mã ký hiệu/nét: "+idr.Summary()+".";Say(im);d.Editor.WriteMessage("\nTVXD MEPS "+Meps06.Version+": "+im);}
    catch(Ex ex){Warn("Chưa ghi sổ mã ký hiệu/nét: "+ex.Message+". Sổ mã không đổi.");}
#endif
   }
   catch(Ex ex){Warn("Chưa chuẩn bị layer chuẩn: "+ex.Message+". Bản vẽ không đổi; layer sẽ được kiểm tra lại khi đặt ký hiệu/nét.");}}
#if IDENTITY015
  public static IdentityReport015 LastIdentity{get{return lastIdentity;}}static IdentityReport015 lastIdentity;
#endif
#endif
  static System.Drawing.Point TopLeft(List<string> info){var r=DrawingRect();int off=compact.S(8);if(!r.IsEmpty){if(info!=null)info.Add("drawing window "+r);return new System.Drawing.Point(r.Left+off,r.Top+off);}
   var m=App.MainWindow.DeviceIndependentLocation;if(info!=null)info.Add("drawing window unavailable; main window "+m);return new System.Drawing.Point((int)m.X+off,(int)m.Y+off*8);}
  static void FirstOpen(){paletteError=null;var info=new List<string>();
   try{compactSet.Dock=DockSides.None;var target=new System.Drawing.Size(compact.S(CompactView06.ClientWidth),compact.S(CompactView06.ClientHeight));compactSet.Size=target;
    // One correction for the host chrome: palette size minus the hosted view size, measured once.
    int dw=compactSet.Size.Width-compact.Width,dh=compactSet.Size.Height-compact.Height;info.Add("chrome "+dw+"x"+dh);if(dw>=0&&dw<=160&&dh>=0&&dh<=200)compactSet.Size=new System.Drawing.Size(target.Width+dw,target.Height+dh);
    var at=TopLeft(info);compactSet.Location=at;info.Add("location "+at+" size "+compactSet.Size+" view "+compact.Size+" dpi "+compact.Dpi.ToString("0.##",System.Globalization.CultureInfo.InvariantCulture));}
   catch(Ex ex){paletteError="vị trí lần đầu: "+ex.GetType().Name+": "+ex.Message;Warn("Chưa đặt được vị trí bảng lần đầu ("+ex.Message+"); kéo bảng tới chỗ mong muốn.");}
   firstPlacement=string.Join("; ",info);}
  // Position checks use the hosted view's actual screen rectangle, never the PaletteSet.Location getter: native diagnostic R3 read Location and
  // DeviceIndependentLocation 2311,2550 while the view really was at 2336,85 (208×500) inside the drawing window 2303,71 — the first-open setter worked.
  public static System.Drawing.Rectangle CompactScreenRect(){try{if(compact!=null&&compact.IsHandleCreated&&compact.Visible)return compact.RectangleToScreen(compact.ClientRectangle);}catch{}return System.Drawing.Rectangle.Empty;}
  static string placementFix;public static string PlacementCorrection{get{return placementFix;}}
  // One correction on opening, no timer: only a floating view that lies on no screen at all is moved to the drawing's top-left; any position on a
  // screen (first-open or the user's) is kept.
  static void Recover(){placementFix=null;try{var v=CompactScreenRect();if(v.IsEmpty||compactSet.Dock!=DockSides.None||System.Windows.Forms.Screen.AllScreens.Any(s=>s.WorkingArea.IntersectsWith(v)))return;
    var to=TopLeft(null);compactSet.Location=to;placementFix="view "+v+" ngoài mọi màn hình → "+to+", view now "+CompactScreenRect();}
   catch(Ex ex){paletteError="khôi phục vị trí: "+ex.GetType().Name+": "+ex.Message;placementFix="lỗi "+ex.Message;}}
  // MEPS060ADV: the accepted 0.3.1 room/device/library panel, unchanged, in its own palette (library view over LIBRARY_0_6).
  [CommandMethod("MEPS060ADV",CommandFlags.NoUndoMarker)]public void OpenAdvanced(){Hook();if(palette==null){panel=new Panel();panel.Request=Queue;panel.library.Root=LibraryRoot;panel.library.Request=LegacyLibraryAction;panel.library.Reload();palette=new PaletteSet("TVXD MEPS "+Meps06.Version+" • Nâng cao (giao diện 0.3.1)",new Guid("D93B5E20-7C41-4F6A-8B2D-1E5A9C060601")){MinimumSize=new System.Drawing.Size(630,780),Size=new System.Drawing.Size(780,850),Style=PaletteSetStyles.ShowCloseButton|PaletteSetStyles.ShowAutoHideButton|PaletteSetStyles.ShowPropertiesMenu};palette.Add("Điện mặt bằng",panel);}Bind(App.DocumentManager.MdiActiveDocument);palette.Visible=true;panel.Visible=true;foreach(System.Windows.Forms.Control c in panel.Controls){c.Visible=true;c.CreateControl();}panel.PerformLayout();ReadUi();}
  static void Queue(string action,Meta meta,string handle,int rows,int cols){Enqueue(new Job{action=action,meta=meta,handle=handle,rows=rows,cols=cols});}
  static bool WireBusy(){
#if WIRES010
return WirePlugin010.Busy;
#else
return false;
#endif
}
  static bool FlexBusy(){
#if LAYOUT030
   // 030: a pending/running layout command blocks every action that checks FlexBusy (placement, MEPS060ADV/model jobs, wire via Attach, flex).
   if(LayoutBusy())return true;
#endif
#if FLEX022
   return FlexDuct022.Busy;
#else
   return false;
#endif
  }
#if FLEX022
  static void FlexAction(){var d=App.DocumentManager.MdiActiveDocument;if(d==null)throw new Ex("Mở bản vẽ trước.");Bind(d);
   if(WireBusy()||executing||pending!=null||queue.Busy||FlexBusy())throw new Ex("Hoàn tất thao tác MEPS đang chạy trước (hoặc ESC).");if(!d.Database.TileMode)throw new Ex("Chuyển sang Model (TILEMODE=1).");
   FlexDuct022.Queue();FocusDrawing();}
#endif
  static bool LayoutBusy(){
#if LAYOUT030
   return Layout030.Busy;
#else
   return false;
#endif
  }
#if LAYOUT030
  // Everything except the layout module itself (Layout030.OtherBusy).
  static bool NonLayoutBusy(){return executing||pending!=null||queue.Busy||WireBusy()||
#if FLEX022
   FlexDuct022.Busy;
#else
   false;
#endif
  }
  // TVXD-MEPS-LAYOUT-030: one allowed T- command per menu click. No Model requirement (layout commands work in PaperSpace; Layout030.lsp validates each).
  static void LayoutAction(string command){var d=App.DocumentManager.MdiActiveDocument;if(d==null)throw new Ex("Mở bản vẽ trước.");
   if(NonLayoutBusy())throw new Ex("Hoàn tất thao tác MEPS đang chạy trước (hoặc ESC).");Layout030.Queue(command);FocusDrawing();}
#endif
#if LIBUPDATE023
  // TVXD-MEPS-LIBUPDATE-023: the right-clicked entry (exact snapshot, not the current selection) travels in the queued bound-document job.
  static void UpdateAction(LibEntry target){if(target==null)throw new Ex("Chưa có ký hiệu đích.");var j=Library.Json();
   Enqueue(new Job{action=ModelLibraryUI019.Update,target=j.Deserialize<LibEntry>(j.Serialize(target))});FocusDrawing();}
#endif
  static void Enqueue(Job job){var d=App.DocumentManager.MdiActiveDocument;if(d==null)throw new Ex("Mở mặt bằng trước.");Bind(d);if(WireBusy()||FlexBusy()||executing||pending!=null||queue.Busy||!string.IsNullOrEmpty(d.CommandInProgress))throw new Ex("Hoàn tất lệnh CAD / thao tác MEPS đang chạy trước.");if(!d.Database.TileMode)throw new Ex("Chuyển Model.");job.doc=d;job.version=revision;job.ucs=d.Editor.CurrentUserCoordinateSystem;job.units=d.Database.Insunits;pending=job;try{d.SendStringToExecute(job.action=="undo"?"TVXDMEPS060UNDO ":"TVXDMEPS060DO ",true,false,false);}catch{pending=null;throw;}Say(job.action=="legend"?"Đang quét ký hiệu đã dùng trong Model…":"Đang thực hiện: "+job.action+". Theo dõi lời nhắc trên Model; ESC hủy.");}
  static void FocusDrawing(){try{Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView();}catch{}}
  // The advanced (0.3.1) library view still sends "place" with its own scale fields; it goes through the same 0.6 request queue and cursor preview.
  static void LegacyLibraryAction(string action,LibPlacement p){LibraryAction(action,p);}
  // Palette actions. place: one request per click (PlaceQueue); the command string is sent with echo off and the drawing gets focus, so AutoCAD's point
  // input starts without typing. A click while the string is still queued replaces the request; a click during the preview is refused (ESC first); a click
  // while another CAD command runs is refused instead of typing into its prompt. cancel: ESC in the palette before the command started.
  static void LibraryAction(string action,LibPlacement p){
   if(action=="place"){var d=App.DocumentManager.MdiActiveDocument;if(d==null)throw new Ex("Mở bản vẽ trước.");Bind(d);if(WireBusy()||FlexBusy()||executing||pending!=null)throw new Ex("Hoàn tất thao tác MEPS đang chạy trước.");if(!d.Database.TileMode)throw new Ex("Chuyển sang Model (TILEMODE=1).");
    if(p==null)throw new Ex("Yêu cầu đặt không hợp lệ.");var entry=Library.Find(Library.Load(LibraryRoot),p.id);string cmd=d.CommandInProgress;
    var r=new PlaceRequest{doc=d,ucs=d.Editor.CurrentUserCoordinateSystem,units=(int)d.Database.Insunits,placement=p,entryName=entry.name};
    bool send=queue.Request(r,DateTime.UtcNow,string.IsNullOrEmpty(cmd),cmd);Trace(r,"REQUEST",(send?"gửi lệnh":"thay yêu cầu đang chờ")+(p.ratio?" 1:"+Library.F(p.drawingDenominator)+" hệ số "+Library.F(p.scale):" hệ số "+Library.F(p.scale)));
    if(send){try{d.SendStringToExecute("TVXDMEPS060DO ",true,false,false);}catch(Ex ex){queue.Cancel();Trace(r,"FAIL","gửi lệnh: "+ex.Message);throw;}}FocusDrawing();return;}
   if(action=="cancel"){var q=queue.Queued;if(queue.Cancel()){Trace(q,"CANCEL","ESC trong bảng trước khi lệnh bắt đầu");Say("Đã huỷ yêu cầu đặt "+q.entryName+"; chưa thay đổi bản vẽ.");}return;}
   if(action=="undo"){Enqueue(new Job{action="undo"});return;}
   if(action=="legend"){Enqueue(new Job{action="legend"});return;}
#if ROUTE034
   // TVXD-MEPS-WIREQTY-034: the two DÂY DẪN route items run as the queued bound-document job (same busy/queue/Model checks as every palette job).
   if(RouteUi034.Handles(action)){Enqueue(new Job{action=action});FocusDrawing();return;}
#endif
#if TAG033
   // TVXD-MEPS-TAG-033: the TAG button runs as the queued bound-document job (same busy/queue/Model checks as every palette job), before the legacy routes.
   if(TagUi033.Handles(action)){Enqueue(new Job{action=action});FocusDrawing();return;}
#endif
#if CIRCUIT032
   // TVXD-MEPS-METADATA-032: circuit tag menu items run as the queued bound-document job (same busy/queue/Model checks as every palette job).
   if(CircuitUi032.Handles(action)){Enqueue(new Job{action=action});FocusDrawing();return;}
#endif
#if MODELLIB019
   // TVXD-MEPS-MODELLIB-019: '+' menu actions on Model run as the queued bound-document command (same busy/queue checks as every palette job).
   if(ModelLibraryUI019.Handles(action)){Enqueue(new Job{action=action});FocusDrawing();return;}
   // F06: '+ → Thêm từ DWG/DXF' is bound to the document too (queued job, RunImport019); the modeless route below is the legacy (non-019) one.
   if(action=="import"){Enqueue(new Job{action="import"});FocusDrawing();return;}
#endif
   // Import (unchanged from 0.5): copies are frozen (ACI 3, Continuous, explicit effective lineweight from this source) and the source table is read into
   // legend.json. Older unfrozen copies of the SAME source (same hash) are frozen too; other sources are not touched.
   if(action=="import"){if(WireBusy()||executing||pending!=null||queue.Busy)throw new Ex("Hoàn tất thao tác MEPS đang chạy trước.");using(var dialog=new System.Windows.Forms.OpenFileDialog{Filter="Bản vẽ DWG/DXF|*.dwg;*.dxf",Multiselect=false,CheckFileExists=true,Title="Chọn DWG/DXF nguồn (chỉ đọc, không sửa nguồn)"}){if(dialog.ShowDialog()!=System.Windows.Forms.DialogResult.OK)return;Say("Đang đọc "+Path.GetFileName(dialog.FileName)+"…");var lw=RuntimeLineweight();var report=LibraryCad.Import(LibraryRoot,dialog.FileName,lw);
    string frozen;try{var up=LibraryStyle.Upgrade(LibraryRoot,dialog.FileName,lw,true);frozen=up.frozen+up.ambiguous+up.errors.Count>0?" Bản chép cũ cùng nguồn: "+up.Summary()+(up.ambiguousNames.Count>0?" (mơ hồ: "+string.Join(", ",up.ambiguousNames.Take(5))+")":"")+".":"";}catch(Ex ex){frozen=" Chưa cố định được bản chép cũ ("+ex.Message+").";}
    string table;try{table=LegendCad.MapSource(LibraryRoot,dialog.FileName).Summary();}catch(Ex ex){table="chưa đọc được bảng ký hiệu ("+ex.Message+"); mô tả để trống";}
    if(panel!=null)panel.library.Reload();if(compact!=null)compact.Reload(report.added.FirstOrDefault());Say("Đã thêm vào thư viện (bản sao chuẩn hóa, nét cố định): "+report.Summary()+"."+frozen+" Bảng ký hiệu: "+table+". Nguồn giữ nguyên.");}}}
  // Strict check for the MEPS060ADV operations (unchanged): same document and revision, Model, INSUNITS, UCS, UNDO on.
  static void Current(Job j){if(App.DocumentManager.MdiActiveDocument!=j.doc||revision!=j.version||!j.doc.Database.TileMode||j.doc.Database.Insunits!=j.units||!j.doc.Editor.CurrentUserCoordinateSystem.IsEqualTo(j.ucs))throw new Ex("Ngữ cảnh đã đổi; thao tác bị hủy.");if((Convert.ToInt32(App.GetSystemVariable("UNDOCTL"))&1)==0)throw new Ex("UNDO đang tắt; chưa thay đổi hình.");}
  // Placement context: the exact Document of the click is active, Model, INSUNITS and UCS as at the click, UNDO on. Zoom/pan does not invalidate it.
  static void PlaceContext(Document d,PlaceRequest r,string when){if(App.DocumentManager.MdiActiveDocument!=d||!ReferenceEquals(d,r.doc))throw new Ex("Bản vẽ đã đổi "+when+"; không đặt.");if(!d.Database.TileMode)throw new Ex("Không còn ở Model "+when+".");
   if((int)d.Database.Insunits!=r.units)throw new Ex("INSUNITS của bản vẽ đã đổi "+when+"; bấm lại ký hiệu.");if(!(r.ucs is Matrix3d)||!d.Editor.CurrentUserCoordinateSystem.IsEqualTo((Matrix3d)r.ucs))throw new Ex("UCS đã đổi "+when+"; bấm lại ký hiệu.");
   if((Convert.ToInt32(App.GetSystemVariable("UNDOCTL"))&1)==0)throw new Ex("UNDO đang tắt; chưa thay đổi hình.");}
  public static double Meter(Database db){switch(db.Insunits){case UnitsValue.Millimeters:return .001;case UnitsValue.Centimeters:return .01;case UnitsValue.Meters:return 1;default:throw new Ex("V1 cần đơn vị DWG mm, cm hoặc m đã được xác nhận. Không tự đổi INSUNITS.");}}
  public static P[] Boundary(Polyline p){if(!p.Closed||p.NumberOfVertices<3||p.NumberOfVertices>1000)throw new Ex("Chọn polyline kín 3–1000 đỉnh.");var pts=new List<P>();for(int i=0;i<p.NumberOfVertices;i++){if(Math.Abs(p.GetBulgeAt(i))>1e-10)throw new Ex("V1 chưa hỗ trợ cạnh cong của phòng.");var v=p.GetPoint3dAt(i);if(Math.Abs(v.Z)>1e-7)throw new Ex("V1 làm mặt bằng WCS Z=0; cần bản sao phẳng đã xác nhận.");pts.Add(new P(v.X,v.Y));}Core.Polygon(pts.ToArray());return pts.ToArray();}
  public static Meta ReadMeta(Transaction tr,Entity entity){if(entity.ExtensionDictionary.IsNull)return null;var dict=(DBDictionary)tr.GetObject(entity.ExtensionDictionary,OpenMode.ForRead);if(!dict.Contains(Key))return null;var x=(Xrecord)tr.GetObject(dict.GetAt(Key),OpenMode.ForRead);using(var data=x.Data){if(data==null)throw new Ex("Metadata rỗng");string json=string.Concat(data.AsArray().Select(v=>Convert.ToString(v.Value)));var m=Core.Json.Deserialize<Meta>(json);Core.Validate(m);return m;}}
  static void WriteMeta(Transaction tr,Entity e,Meta m){Core.Validate(m);if(e.ExtensionDictionary.IsNull)e.CreateExtensionDictionary();var d=(DBDictionary)tr.GetObject(e.ExtensionDictionary,OpenMode.ForWrite);Xrecord x;if(d.Contains(Key))x=(Xrecord)tr.GetObject(d.GetAt(Key),OpenMode.ForWrite);else{x=new Xrecord();d.SetAt(Key,x);tr.AddNewlyCreatedDBObject(x,true);}string json=Core.Json.Serialize(m);var chunks=new List<TypedValue>();for(int i=0;i<json.Length;i+=240)chunks.Add(new TypedValue((int)DxfCode.Text,json.Substring(i,Math.Min(240,json.Length-i))));using(var buffer=new ResultBuffer(chunks.ToArray()))x.Data=buffer;}
  static ObjectId Layer(Transaction tr,Database db,string name,short color){var table=(LayerTable)tr.GetObject(db.LayerTableId,OpenMode.ForRead);if(table.Has(name)){var l=(LayerTableRecord)tr.GetObject(table[name],OpenMode.ForRead);if(l.IsLocked||l.IsOff||l.IsFrozen)throw new Ex("Layer "+name+" đang khóa/ẩn.");return l.ObjectId;}table.UpgradeOpen();var layer=new LayerTableRecord{Name=name,Color=Autodesk.AutoCAD.Colors.Color.FromColorIndex(Autodesk.AutoCAD.Colors.ColorMethod.ByAci,color)};var id=table.Add(layer);tr.AddNewlyCreatedDBObject(layer,true);return id;}
  static ObjectId HandleId(Database db,string h){long v;if(!long.TryParse(h,System.Globalization.NumberStyles.HexNumber,System.Globalization.CultureInfo.InvariantCulture,out v))throw new Ex("Handle không hợp lệ.");return db.GetObjectId(false,new Handle(v),0);}
  public static Snapshot Scan(Database db){double meter=Meter(db);var s=new Snapshot();using(var tr=db.TransactionManager.StartTransaction()){var space=(BlockTableRecord)tr.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db),OpenMode.ForRead);foreach(ObjectId id in space){var e=tr.GetObject(id,OpenMode.ForRead) as Entity;if(e==null)continue;try{var m=ReadMeta(tr,e);if(m==null)continue;var r=new Row{Handle=e.Handle.ToString(),Id=m.id,Code=m.code,Name=m.name,Level=m.level,Kind=m.kind,Watts=m.watts};if(m.kind=="ROOM"){var p=e as Polyline;if(p==null)throw new Ex("Phòng không còn là polyline");r.polygon=Boundary(p);r.AreaM2=Core.Area(r.polygon)*meter*meter;r.Kind=m.function;s.rooms.Add(r);}else{var b=e as BlockReference;if(b==null)throw new Ex("Thiết bị không còn là block");if(Math.Abs(b.Position.Z)>1e-7)throw new Ex("Thiết bị ngoài mặt phẳng WCS Z=0");r.point=new P(b.Position.X,b.Position.Y);s.devices.Add(r);}}catch(Ex ex){s.issues.Add("Handle "+e.Handle+": "+ex.Message);}}}Core.Reconcile(s);return s;}
  static void Unique(Snapshot s,string code,string except){if(s.rooms.Concat(s.devices).Any(x=>x.Handle!=except&&string.Equals(x.Code,code,StringComparison.OrdinalIgnoreCase)))throw new Ex("Mã đã tồn tại: "+code+". Nhập mã mới.");}
  public static ObjectId AddRoom(Database db,Polyline source,Meta m){Core.Validate(m);if(m.kind!="ROOM")throw new Ex("Sai loại phòng");Boundary(source);Unique(Scan(db),m.code,null);using(var tr=db.TransactionManager.StartTransaction()){var copy=(Polyline)source.Clone();copy.LayerId=Layer(tr,db,"TVXD_MEPS_ROOM",4);var space=(BlockTableRecord)tr.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db),OpenMode.ForWrite);var id=space.AppendEntity(copy);tr.AddNewlyCreatedDBObject(copy,true);WriteMeta(tr,copy,m);tr.Commit();return id;}}
  // Unit blocks use millimetres geometrically; each reference scales explicitly to drawing units.
  // Geometry comes from Symbols.Of, the same shapes the palette previews draw; it is unchanged from 0.1, so the TVXD_MEPS01_* block names stay compatible.
  static ObjectId Symbol(Transaction tr,Database db,string kind){string name="TVXD_MEPS01_"+kind;var bt=(BlockTable)tr.GetObject(db.BlockTableId,OpenMode.ForRead);if(bt.Has(name))return bt[name];bt.UpgradeOpen();var b=new BlockTableRecord{Name=name};var id=bt.Add(b);tr.AddNewlyCreatedDBObject(b,true);var entities=new List<Entity>();foreach(var s in Symbols.Of(kind)){if(s.type=="circle")entities.Add(new Circle(new Point3d(s.x1,s.y1,0),Vector3d.ZAxis,s.r));else if(s.type=="line")entities.Add(new Line(new Point3d(s.x1,s.y1,0),new Point3d(s.x2,s.y2,0)));else entities.Add(new DBText{TextString=s.text,Position=new Point3d(s.x1,s.y1,0),Height=s.h});}foreach(var e in entities){b.AppendEntity(e);tr.AddNewlyCreatedDBObject(e,true);}return id;}
  public static ObjectId[] AddDevices(Database db,P[] points,Meta prototype,string roomHandle){Core.Validate(prototype);if(prototype.kind=="ROOM")throw new Ex("Chọn loại thiết bị");if(points==null||points.Length==0||points.Length>200||points.Any(p=>p==null||!Core.Finite(p.x)||!Core.Finite(p.y)))throw new Ex("Số điểm không hợp lệ.");var scan=Scan(db);if(roomHandle!=null){var room=scan.rooms.SingleOrDefault(r=>r.Handle==roomHandle);if(room==null||points.Any(p=>Core.Locate(room.polygon,p)!=1))throw new Ex("Có điểm nằm ngoài phòng.");}var codes=points.Select((p,i)=>points.Length==1?prototype.code:prototype.code+"-"+(i+1).ToString("000")).ToArray();foreach(string code in codes)Unique(scan,code,null);double scale=.001/Meter(db);var ids=new List<ObjectId>();using(var tr=db.TransactionManager.StartTransaction()){var layer=Layer(tr,db,"TVXD_MEPS_"+prototype.kind,prototype.kind=="LIGHT"?(short)2:(short)3);var symbol=Symbol(tr,db,prototype.kind);var space=(BlockTableRecord)tr.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db),OpenMode.ForWrite);for(int i=0;i<points.Length;i++){var b=new BlockReference(new Point3d(points[i].x,points[i].y,0),symbol){ScaleFactors=new Scale3d(scale),LayerId=layer};ids.Add(space.AppendEntity(b));tr.AddNewlyCreatedDBObject(b,true);WriteMeta(tr,b,new Meta{kind=prototype.kind,code=codes[i],name=prototype.name,watts=prototype.watts});}tr.Commit();}return ids.ToArray();}
  public static void Edit(Database db,string handle,Meta replacement){Core.Validate(replacement);Unique(Scan(db),replacement.code,handle);using(var tr=db.TransactionManager.StartTransaction()){var e=(Entity)tr.GetObject(HandleId(db,handle),OpenMode.ForWrite);var old=ReadMeta(tr,e);if(old==null||old.kind!=replacement.kind)throw new Ex("V1 sửa thuộc tính cùng loại; không đổi loại ký hiệu.");replacement.id=old.id;WriteMeta(tr,e,replacement);tr.Commit();}}
  static void ReadUi(){if(panel==null)return;try{var d=App.DocumentManager.MdiActiveDocument;if(d!=null)panel.Display(Path.GetFileName(d.Name)+" • "+d.Database.Insunits,Scan(d.Database));}catch(Ex ex){panel.Message(ex.Message);}}
  // One command for every palette action. A queued placement request of THIS document runs first (cursor preview); otherwise the pending MEPS060ADV/legend job.
  [CommandMethod("TVXDMEPS060DO",CommandFlags.Modal)]public void Execute(){var d=App.DocumentManager.MdiActiveDocument;string why;var r=queue.Take(d,out why);if(r!=null){RunPlace(d,r);return;}
   if(why!=null){Warn(why);Log().Write(0,"DROP","",-1,why);}
   var j=pending;pending=null;if(j==null){if(!queue.Busy&&compact!=null)compact.Disarm();return;}
   executing=true;bool regen=false;try{if(d!=j.doc||revision!=j.version)throw new Ex("Bản vẽ đổi sau khi bấm nút; đọc lại rồi thử lại.");if(!d.Database.TileMode)throw new Ex("Chuyển Model.");if(j.action!="legend"&&!ModelAction(j.action))Meter(d.Database);bool changed=false;
    if(j.action=="legend"){RunLegend(d,j);return;}
#if ROUTE034
    if(RouteUi034.Handles(j.action)){RunRoute(d,j);return;}
#endif
#if TAG033
    if(TagUi033.Handles(j.action)){RunTag(d,j);return;}
#endif
#if MODELLIB019
    if(ModelLibraryUI019.Handles(j.action)){RunModel(d,j);return;}
    if(j.action=="import"){RunImport019(d,j);return;}
#endif
#if CIRCUIT032
    if(CircuitUi032.Handles(j.action)){RunCircuit(d,j);return;}
#endif
    regen=true;
    if(j.action=="room"||j.action=="trace"){Polyline boundary=null;try{if(j.action=="room"){var opt=new PromptEntityOptions("\nChọn đường bao phòng kín (giữ nguyên nguồn): ");opt.SetRejectMessage("\nChọn polyline.");opt.AddAllowedClass(typeof(Polyline),true);var pick=d.Editor.GetEntity(opt);if(pick.Status!=PromptStatus.OK)return;using(var tr=d.TransactionManager.StartTransaction()){var src=(Polyline)tr.GetObject(pick.ObjectId,OpenMode.ForRead);if(ReadMeta(tr,src)!=null)throw new Ex("Đường bao đã thuộc MEPS; chọn Sửa phòng.");boundary=(Polyline)src.Clone();}}
     else{var pts=new List<Point3d>();while(true){var opt=new PromptPointOptions(pts.Count==0?"\nChọn đỉnh đầu phòng (ESC hủy): ":"\nChọn đỉnh tiếp theo; Enter kết thúc: "){AllowNone=pts.Count>=3};var p=d.Editor.GetPoint(opt);if(p.Status==PromptStatus.None)break;if(p.Status!=PromptStatus.OK)return;pts.Add(p.Value.TransformBy(d.Editor.CurrentUserCoordinateSystem));if(pts.Count>=1000)throw new Ex("Quá nhiều đỉnh.");}boundary=new Polyline();for(int i=0;i<pts.Count;i++){if(Math.Abs(pts[i].Z)>1e-7)throw new Ex("V1 cần WCS Z=0.");boundary.AddVertexAt(i,new Point2d(pts[i].X,pts[i].Y),0,0,0);}boundary.Closed=true;}Current(j);AddRoom(d.Database,boundary,j.meta);changed=true;}finally{if(boundary!=null)boundary.Dispose();}}
    else if(j.action=="place"){var p=d.Editor.GetPoint("\nChọn vị trí thiết bị (ESC hủy): ");if(p.Status!=PromptStatus.OK)return;var w=p.Value.TransformBy(d.Editor.CurrentUserCoordinateSystem);if(Math.Abs(w.Z)>1e-7)throw new Ex("V1 cần WCS Z=0.");Current(j);AddDevices(d.Database,new[]{new P(w.X,w.Y)},j.meta,null);changed=true;}
    else if(j.action=="grid"){if(j.meta.kind!="LIGHT")throw new Ex("Lưới chỉ dùng bố trí đèn sơ bộ.");var room=Scan(d.Database).rooms.Single(x=>x.Handle==j.handle);var pts=Core.Grid(room.polygon,j.rows,j.cols);double marker=.08/Meter(d.Database);foreach(var p in pts){d.Editor.DrawVector(new Point3d(p.x-marker,p.y,0),new Point3d(p.x+marker,p.y,0),2,false);d.Editor.DrawVector(new Point3d(p.x,p.y-marker,0),new Point3d(p.x,p.y+marker,0),2,false);}var opt=new PromptKeywordOptions("\nXem trước "+pts.Length+" đèn. Chỉ bố trí, chưa tính lux. [Apply/Cancel] <Cancel>: ");opt.Keywords.Add("Apply");opt.Keywords.Add("Cancel");opt.AllowNone=true;var pick=d.Editor.GetKeywords(opt);if(pick.Status!=PromptStatus.OK||pick.StringResult!="Apply")return;Current(j);AddDevices(d.Database,pts,j.meta,j.handle);changed=true;}
    else if(j.action=="edit"||j.action=="roomedit"){Current(j);Edit(d.Database,j.handle,j.meta);changed=true;}
    else if(j.action=="findroom"||j.action=="finddevice"){var id=HandleId(d.Database,j.handle);using(var tr=d.TransactionManager.StartTransaction()){var e=(Entity)tr.GetObject(id,OpenMode.ForRead);var bounds=e.GeometricExtents;using(var view=d.Editor.GetCurrentView()){var w2d=Matrix3d.PlaneToWorld(view.ViewDirection);w2d=Matrix3d.Displacement(view.Target-Point3d.Origin)*w2d;w2d=Matrix3d.Rotation(-view.ViewTwist,view.ViewDirection,view.Target)*w2d;bounds.TransformBy(w2d.Inverse());view.CenterPoint=new Point2d((bounds.MinPoint.X+bounds.MaxPoint.X)/2,(bounds.MinPoint.Y+bounds.MaxPoint.Y)/2);view.Height=Math.Max(bounds.MaxPoint.Y-bounds.MinPoint.Y,1/Meter(d.Database))*1.5;view.Width=Math.Max(bounds.MaxPoint.X-bounds.MinPoint.X,view.Height)*1.5;d.Editor.SetCurrentView(view);}d.Editor.SetImpliedSelection(new[]{id});}}
    else if(j.action=="export"){using(var dialog=new System.Windows.Forms.SaveFileDialog{Filter="Excel workbook|*.xlsx",DefaultExt="xlsx",FileName="MEPS_THIET_BI.xlsx",OverwritePrompt=true}){if(dialog.ShowDialog()!=System.Windows.Forms.DialogResult.OK)return;Core.Export(dialog.FileName,Scan(d.Database));Say("Đã xuất "+dialog.FileName);return;}}
    if(changed){undoDoc=d;undoRevision=revision;}ReadUi();if(changed)Say("Đã cập nhật. Bố trí sơ bộ; chưa tự lưu DWG. Lưu DWG để giữ dữ liệu MEPS.");
   }catch(Ex ex){Warn("Chưa thực hiện: "+ex.Message);}finally{executing=false;if(!queue.Busy&&compact!=null)compact.Disarm();if(regen)try{d.Editor.Regen();}catch{}}}
  static bool ModelAction(string action){
#if ROUTE034
   // Routes use the unit the USER declares per route (MM/M); INSUNITS is only compared for a warning, so no Meter requirement.
   if(RouteUi034.Handles(action))return true;
#endif
#if TAG033
   // Labels are unit-free (height in drawing units): no INSUNITS (Meter) requirement.
   if(TagUi033.Handles(action))return true;
#endif
#if CIRCUIT032
   // Circuit tags are unit-free: no INSUNITS (Meter) requirement.
   if(CircuitUi032.Handles(action))return true;
#endif
#if MODELLIB019
   return ModelLibraryUI019.Handles(action)||action=="import";
#else
   return false;
#endif
  }
#if MODELLIB019
  // TVXD-MEPS-MODELLIB-019: capture / editable copy / hide from Model. Context checked before and after every prompt/dialog (ModelContext); errors propagate
  // to Execute's warning. Library writes never happen from the modeless palette.
  // TVXD-MEPS-MODELUI-020 F03: staged local log of the three Model actions in LOGS_0_6 (placement log format; entry column = action). Only stage, status,
  // selected handle and the exception type are written — no drawing path/name, source content or error text. PlacementLog.Write never throws and the sink
  // is wrapped again, so a logging failure cannot turn a result into an error; the log is never a library or drawing write.
  static long modelRun;
  static void RunModel(Document d,Job j){Action<string> guard=when=>ModelContext(d,j,when);long run=++modelRun;var t0=DateTime.UtcNow;
   Action<string,string> stage=(evt,detail)=>{try{Log().Write(run,evt,j.action,(DateTime.UtcNow-t0).TotalMilliseconds,detail);}catch{}};
   stage("MODEL_START","");ModelOutcome019 o;
   try{
#if LIBUPDATE023
    if(j.action==ModelLibraryUI019.Update)o=ModelLibraryUI019.RunUpdate(d,LibraryRoot,j.target,guard,RuntimeLineweight(),stage);else
#endif
    o=ModelLibraryUI019.Run(j.action,d,LibraryRoot,guard,RuntimeLineweight(),stage);}catch(Ex ex){stage("MODEL_FAIL",ex.GetType().Name);throw;}
   stage(o.cancelled?"MODEL_CANCEL":"MODEL_DONE",(o.changed?"changed ":"")+(o.reload?"reload":""));
   if(o.changed){undoDoc=d;undoRevision=revision;}
   if(o.reload){if(panel!=null)panel.library.Reload();if(compact!=null)compact.Reload(o.select);}
   if(o.warn)Warn(o.message);else Say(o.message);try{d.Editor.WriteMessage("\nTVXD MEPS "+Meps06.Version+": "+o.message);}catch{}}
  // F06: '+ → Thêm từ DWG/DXF' as the queued bound-document job (the import reads this document's runtime LWDEFAULT). Same ModelContext guard before and
  // after the file dialog and right before LibraryCad.Import; cancel = nothing written. The user's drawing is only read; steps after Import as in 0.6.
  static void RunImport019(Document d,Job j){Action<string> guard=when=>ModelContext(d,j,when);guard("trước khi chọn file");string path;
   using(var dialog=new System.Windows.Forms.OpenFileDialog{Filter="Bản vẽ DWG/DXF|*.dwg;*.dxf",Multiselect=false,CheckFileExists=true,Title="Chọn DWG/DXF nguồn (chỉ đọc, không sửa nguồn)"}){
    var r=dialog.ShowDialog();guard("sau hộp chọn file");if(r!=System.Windows.Forms.DialogResult.OK){Say("Đã huỷ thêm từ DWG/DXF; thư viện giữ nguyên.");return;}path=dialog.FileName;}
   Say("Đang đọc "+Path.GetFileName(path)+"…");guard("trước khi nhập vào thư viện");var lw=RuntimeLineweight();var report=LibraryCad.Import(LibraryRoot,path,lw);
   string frozen;try{var up=LibraryStyle.Upgrade(LibraryRoot,path,lw,true);frozen=up.frozen+up.ambiguous+up.errors.Count>0?" Bản chép cũ cùng nguồn: "+up.Summary()+(up.ambiguousNames.Count>0?" (mơ hồ: "+string.Join(", ",up.ambiguousNames.Take(5))+")":"")+".":"";}catch(Ex ex){frozen=" Chưa cố định được bản chép cũ ("+ex.Message+").";}
   string table;try{table=LegendCad.MapSource(LibraryRoot,path).Summary();}catch(Ex ex){table="chưa đọc được bảng ký hiệu ("+ex.Message+"); mô tả để trống";}
   if(panel!=null)panel.library.Reload();if(compact!=null)compact.Reload(report.added.FirstOrDefault());Say("Đã thêm vào thư viện (bản sao chuẩn hóa, nét cố định): "+report.Summary()+"."+frozen+" Bảng ký hiệu: "+table+". Nguồn giữ nguyên.");}
  // The exact Document / Model / INSUNITS / UCS of the click, UCS = World (WCS), UNDO on. Zoom/pan does not matter.
  static void ModelContext(Document d,Job j,string when){if(App.DocumentManager.MdiActiveDocument!=d||!ReferenceEquals(d,j.doc))throw new Ex("Bản vẽ đã đổi "+when+"; không thay đổi gì.");
   if(!d.Database.TileMode)throw new Ex("Không còn ở Model "+when+"; không thay đổi gì.");if(d.Database.Insunits!=j.units)throw new Ex("INSUNITS đã đổi "+when+"; không thay đổi gì.");
   if(!d.Editor.CurrentUserCoordinateSystem.IsEqualTo(j.ucs))throw new Ex("UCS đã đổi "+when+"; không thay đổi gì.");if(!j.ucs.IsEqualTo(Matrix3d.Identity))throw new Ex("Cần UCS World (WCS) cho thao tác thư viện từ Model; MEPS không tự đổi UCS.");
   if((Convert.ToInt32(App.GetSystemVariable("UNDOCTL"))&1)==0)throw new Ex("UNDO đang tắt; không thay đổi gì.");}
#endif
#if CIRCUIT032
  // TVXD-MEPS-METADATA-032: circuit tags inside this TVXDMEPS060DO command (one UNDO step). CircuitContext before/after the selection and the dialog and right
  // before Commit; errors propagate to Execute's warning (nothing written). undoDoc only after a real DWG change; staged log lines like the Model actions.
  static long circuitRun;
  static void RunCircuit(Document d,Job j){Action<string> guard=when=>CircuitContext(d,j,when);long run=++circuitRun;var t0=DateTime.UtcNow;
   Action<string,string> stage=(evt,detail)=>{try{Log().Write(run,evt,j.action,(DateTime.UtcNow-t0).TotalMilliseconds,detail);}catch{}};
   stage("CIRCUIT_START","");CircuitOutcome032 o;
   try{o=CircuitUi032.Run(j.action,d,guard,compact!=null?compact.Dpi:1f);}catch(Ex ex){stage("CIRCUIT_FAIL",ex.GetType().Name);throw;}
   stage(o.cancelled?"CIRCUIT_CANCEL":"CIRCUIT_DONE",(o.changed?"changed ":"")+o.count);
   if(o.changed){undoDoc=d;undoRevision=revision;}
   if(o.warn)Warn(o.message);else Say(o.message);try{d.Editor.WriteMessage("\nTVXD MEPS "+Meps06.Version+": "+o.message);}catch{}}
  // Tags are unit- and UCS-free: the exact Document of the click is active, Model, UNDO on.
  static void CircuitContext(Document d,Job j,string when){if(App.DocumentManager.MdiActiveDocument!=d||!ReferenceEquals(d,j.doc))throw new Ex("Bản vẽ đã đổi "+when+"; không thay đổi gì.");
   if(!d.Database.TileMode)throw new Ex("Không còn ở Model "+when+"; không thay đổi gì.");if((Convert.ToInt32(App.GetSystemVariable("UNDOCTL"))&1)==0)throw new Ex("UNDO đang tắt; không thay đổi gì.");}
#endif
#if TAG033
  // TVXD-MEPS-TAG-033: visible labels inside this TVXDMEPS060DO command (one UNDO step). TagContext before/after the selection and the dialog and right before
  // Commit; errors propagate to Execute's warning (nothing written). undoDoc only after a real DWG change; staged log lines like the circuit actions.
  static long tagRun;
  static void RunTag(Document d,Job j){Action<string> guard=when=>TagContext(d,j,when);long run=++tagRun;var t0=DateTime.UtcNow;
   Action<string,string> stage=(evt,detail)=>{try{Log().Write(run,evt,j.action,(DateTime.UtcNow-t0).TotalMilliseconds,detail);}catch{}};
   stage("TAG_START","");TagOutcome033 o;
   try{o=TagUi033.Run(j.action,d,guard,compact!=null?compact.Dpi:1f);}catch(Ex ex){stage("TAG_FAIL",ex.GetType().Name);throw;}
   stage(o.cancelled?"TAG_CANCEL":"TAG_DONE",(o.changed?"changed ":"")+o.count);
   if(o.changed){undoDoc=d;undoRevision=revision;}
   if(o.warn)Warn(o.message);else Say(o.message);try{d.Editor.WriteMessage("\nTVXD MEPS "+Meps06.Version+": "+o.message);}catch{}}
  // Labels are unit- and UCS-free (WCS text, no geometry transform): the exact Document of the click is active, Model, UNDO on.
  static void TagContext(Document d,Job j,string when){if(App.DocumentManager.MdiActiveDocument!=d||!ReferenceEquals(d,j.doc))throw new Ex("Bản vẽ đã đổi "+when+"; không thay đổi gì.");
   if(!d.Database.TileMode)throw new Ex("Không còn ở Model "+when+"; không thay đổi gì.");if((Convert.ToInt32(App.GetSystemVariable("UNDOCTL"))&1)==0)throw new Ex("UNDO đang tắt; không thay đổi gì.");}
#endif
#if ROUTE034
  // TVXD-MEPS-WIREQTY-034: route setup / quantity report inside this TVXDMEPS060DO command. Setup = one transaction (one UNDO step); the report only reads.
  // RouteContext(write) before/after the pick and the dialog and right before Commit; RouteContext(read) around the report scan/re-scan/CSV export.
  // Errors propagate to Execute's warning (nothing written). undoDoc only after a real DWG change; staged log lines like the TAG actions.
  static long routeRun;
  static void RunRoute(Document d,Job j){Action<string> guard=when=>RouteContext(d,j,when,true);Action<string> read=when=>RouteContext(d,j,when,false);long run=++routeRun;var t0=DateTime.UtcNow;
   Action<string,string> stage=(evt,detail)=>{try{Log().Write(run,evt,j.action,(DateTime.UtcNow-t0).TotalMilliseconds,detail);}catch{}};
   stage("ROUTE_START","");RouteOutcome034 o;
#if BDGKL034 && ROOMCAD037
   // S5_REGION_TRACE: room report/options/pick/scan stages go to this run's log (stage never throws); the previous callback is restored in finally.
   var priorRoomStage=RoomUi037.Stage;RoomUi037.Stage=stage;
#endif
   try{o=RouteUi034.Run(j.action,d,guard,read,compact!=null?compact.Dpi:1f);}catch(Ex ex){stage("ROUTE_FAIL",ex.GetType().Name);throw;}
#if BDGKL034 && ROOMCAD037
   finally{RoomUi037.Stage=priorRoomStage;}
#endif
   stage(o.cancelled?"ROUTE_CANCEL":"ROUTE_DONE",o.changed?"changed":"");
   if(o.changed){undoDoc=d;undoRevision=revision;}
   if(o.warn)Warn(o.message);else Say(o.message);try{d.Editor.WriteMessage("\nTVXD MEPS "+Meps06.Version+": "+o.message);}catch{}}
  // The exact Document of the click is active and in Model; UNDO on for a write. No UCS/INSUNITS requirement (the core stamps and compares INSUNITS itself;
  // a stale route signature aborts Apply).
  static void RouteContext(Document d,Job j,string when,bool write){if(App.DocumentManager.MdiActiveDocument!=d||!ReferenceEquals(d,j.doc))throw new Ex("Bản vẽ đã đổi "+when+"; không thay đổi gì.");
   if(!d.Database.TileMode)throw new Ex("Không còn ở Model "+when+"; không thay đổi gì.");
   if(write&&(Convert.ToInt32(App.GetSystemVariable("UNDOCTL"))&1)==0)throw new Ex("UNDO đang tắt; không thay đổi gì.");}
#endif
  // Library placement: cursor preview (PlaceJig06 over transient copies of the library file), then one insertion after the point is accepted.
  // ESC/cancel adds nothing. The palette stays visible. Every step is written to the placement log with the elapsed time since the click.
  static void RunPlace(Document d,PlaceRequest r){executing=true;var p=r.placement;string name=r.entryName;PlacementPreview preview=null;Trace(r,"START","");
   try{PlaceContext(d,r,"trước khi bắt đầu");var entry=Library.Find(Library.Load(LibraryRoot),p.id);name=entry.name;var issue=Meps05.PlaceIssue(entry);if(issue!=null)throw new Ex(entry.name+": "+issue);
#if STANDARDS012
    // Category → profile layer before any preview: an unmapped category is refused here, nothing is guessed.
    string stdLayer=Standards012.LayerFor(entry.category);
#endif
    double scale=p.ratio?Meps05.RatioScale(p.baseScale,p.refDenominator,p.drawingDenominator):p.fitWidth?Library.FitWidthScale(entry,p.width):Library.CheckScale(p.scale);
    if(p.ratio&&Math.Abs(scale-p.scale)>1e-12*Math.Max(1,scale))throw new Ex("Hệ số tỷ lệ không khớp N0/N đã chọn; bấm lại ký hiệu.");Library.CheckRotation(p.rotationDeg);
    var ucs=d.Editor.CurrentUserCoordinateSystem;var z=Vector3d.ZAxis.TransformBy(ucs);if(!z.IsParallelTo(Vector3d.ZAxis)||z.Z<0)throw new Ex("UCS phải song song mặt bằng WCS (Z hướng lên).");var x=Vector3d.XAxis.TransformBy(ucs);double rotation=p.rotationDeg*Math.PI/180+Math.Atan2(x.Y,x.X);
    preview=PlacementPreview.Build(LibraryRoot,entry);Trace(r,"PREVIEW",preview.Summary);
    var jig=new PlaceJig06(preview,scale,rotation,"\nChọn điểm chèn "+entry.name+" (ESC hủy): ");Trace(r,"JIG_START","hệ số "+Library.F(scale));var res=d.Editor.Drag(jig);
    // A preview that failed inside the jig callbacks refuses the insertion, even if CAD returned an accepted point.
    if(jig.Error!=null)throw new Ex("hình xem trước lỗi khi vẽ ("+jig.Error+"); đã dừng, chưa thay đổi bản vẽ. Bấm lại ký hiệu; nếu lặp lại: chuột phải → Tải lại thư viện, rồi gửi nội dung nút ! cho người hỗ trợ.");
    if(res.Status!=PromptStatus.OK){Trace(r,"CANCEL","jig "+res.Status+" frames="+jig.Frames+" samples="+jig.Samples);Say("Đã huỷ đặt "+entry.name+"; chưa thay đổi bản vẽ.");return;}
    var at=jig.Point;Trace(r,"POINT","frames="+jig.Frames+" samples="+jig.Samples);preview.Dispose();preview=null;
    if(Math.Abs(at.Z)>1e-7)throw new Ex("V1 cần điểm ở WCS Z=0.");PlaceContext(d,r,"sau khi chọn điểm");
    // Insert + ratio record in one transaction: a ratio write failure leaves neither the symbol nor the record.
    var notes=new List<string>();
#if STANDARDS012
    // 012 product path: profile layers ensured, private definition on the category layer ByLayer, reference on that layer ByLayer, CLAYER unchanged.
    StdReport012 std;StandardsCad012.InsertAtomic(d.Database,LibraryRoot,entry,at,scale,rotation,notes,p.ratio?(tr,db)=>WriteRatio(tr,db,p):(Action<Transaction,Database>)null,out std);lastStandards=std;
#else
    LibraryCad06.InsertAtomic(d.Database,LibraryRoot,entry,at,scale,rotation,notes,p.ratio?(tr,db)=>WriteRatio(tr,db,p):(Action<Transaction,Database>)null);
#endif
    lastNotes=notes;lastPlacement=new LibPlacement{id=p.id,scale=scale,rotationDeg=p.rotationDeg,width=p.width,baseScale=p.baseScale,refDenominator=p.refDenominator,drawingDenominator=p.drawingDenominator,fitWidth=p.fitWidth,ratio=p.ratio};
    undoDoc=d;undoRevision=revision;Trace(r,"INSERTED",notes.Count(n=>n.StartsWith("CẢNH BÁO"))+" cảnh báo kiểu chữ");
#if STANDARDS012
    if(!std.Complete)Warn("Layer chuẩn: "+std.Summary());
    Say("Đã đặt "+entry.name+" (hệ số "+Library.F(scale)+(p.ratio?", 1:"+Library.F(p.drawingDenominator):"")+") trên layer "+stdLayer+", màu/nét/độ đậm ByLayer trong và ngoài khối; kiểu chữ nguồn giữ nguyên, CLAYER không đổi. Chưa tự lưu DWG; UNDO để bỏ.");}
#else
    Say("Đã đặt "+entry.name+" (hệ số "+Library.F(scale)+(p.ratio?", 1:"+Library.F(p.drawingDenominator):"")+", ACI 3, nét theo nguồn, layer hiện hành, kiểu chữ nguồn giữ nguyên). Chưa tự lưu DWG; UNDO để bỏ.");}
#endif
   catch(Ex ex){Trace(r,"FAIL",ex.GetType().Name+": "+ex.Message);Warn("Chưa đặt "+name+": "+ex.Message);}
   finally{if(preview!=null)preview.Dispose();executing=false;queue.Finish(r);if(compact!=null)compact.Disarm();if(p.ratio)ShowDrawing();}}
  static string SaveReport(LegendPreflight p){try{Directory.CreateDirectory(LogRoot);string path=Path.Combine(LogRoot,"legend_preflight_"+DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff",System.Globalization.CultureInfo.InvariantCulture)+".txt");File.WriteAllText(path,p.Report(),new System.Text.UTF8Encoding(false));return path;}catch(Ex ex){return "(chưa lưu báo cáo: "+ex.Message+")";}}
  // Legend: preflight of this model space (plugin symbols, foreign look-alikes, xrefs) in a dialog; the user can re-read the selected symbol's source table
  // (scoped remap, then automatic rescan). TVXD-MEPS-LEGEND-011: 'Đặt vào Model…' only records the preflight and closes the modal dialog; after it is closed
  // this command asks for the top-left point (Esc = nothing written), re-checks the exact Document / Model / UCS / INSUNITS of the click, rescans (must be
  // identical) and places the centred legend as ONE block in this drawing's Model (LegendPlace011, one transaction inside this command = one UNDO step).
  // No file dialog, no new document; the drawing is not saved.
  static void RunLegend(Document d,Job j){string root=LibraryRoot;
   // TVXD-MEPS-LIBDELETE-021: the legend describes symbols ALREADY in this drawing, so it reads the reference view (active entries + records of entries
   // deleted from the tool: placed refs keep their type identity, the nine switches stay source-backed). The view is only read here, never saved or listed.
   Func<LegendPreflight> scan=()=>{var catalog=Library.WithHistory(Library.Load(root));var map=Legend.Load(root);var used=Legend.Dedupe(LegendCad.ScanUsed(d.Database));var foreign=LibraryCad06.ScanForeign(d.Database,catalog);
#if IDENTITY015
    // 015: the nine source switches always in the legend (reference-only when not placed); first scan, remap rescan and the pre-placement rescan alike.
    return Identity015.Preflight(used,map,catalog,foreign);};
#else
    return Meps06.PreflightForExport(used,map,id=>catalog.entries.FirstOrDefault(e=>e.id==id),foreign);};
#endif
   var first=scan();lastPreflight=first;string report=SaveReport(first);if(!first.CanExport)d.Editor.WriteMessage("\nTVXD MEPS "+Meps06.Version+": chưa đặt được bảng ký hiệu ("+first.Blocking+" mục chặn). Báo cáo: "+report);
   LegendPreflight chosen=null;
   using(var dlg=new LegendPreflightDialog(first)){
    dlg.Remap=item=>{
#if MODELLIB019
     // 019: a generated MODEL_CAPTURE019 table holds the cells the user typed; the capture DWG has no table, so a re-read would erase them. Refused before
     // the file dialog (nothing read or written).
     if(Meps06.ParserOf(Legend.Load(root),item.sourceSha256)==Naming019.Parser)throw new Ex("Nguồn "+item.sourceFile+" là bảng do MEPS tự tạo khi thêm block từ Model ("+Naming019.Parser+"; chữ do người dùng nhập), không phải bảng đọc từ DWG/DXF. Không đọc lại: bản chụp không có bảng, đọc lại sẽ xoá chữ đã nhập. Chưa thay đổi gì.");
#endif
     string path;using(var pick=new System.Windows.Forms.OpenFileDialog{Filter="Bản vẽ DWG/DXF|*.dwg;*.dxf",Multiselect=false,CheckFileExists=true,FileName=item.sourceFile??"",Title="Chọn đúng file nguồn "+item.sourceFile+" (chỉ đọc; hash phải khớp)"}){if(pick.ShowDialog()!=System.Windows.Forms.DialogResult.OK)return null;path=pick.FileName;}
     var res=LibraryCad06.Remap(root,item.sourceSha256,path);Say(res.Summary());if(compact!=null)compact.Reload();var again=scan();lastPreflight=again;SaveReport(again);return again;};
    dlg.Export=pre=>{if(!pre.CanExport)throw new Ex("Còn mục chặn; chưa đặt.");LegendPlace011.CheckUnits(d.Database,pre.map,pre.plan);chosen=pre;return "Chọn góc trên-trái của bảng trên Model.";};
    App.ShowModalDialog(dlg);
    if(dlg.Result==null||chosen==null){if(lastPreflight!=null&&!lastPreflight.CanExport)Warn("Chưa tạo bảng ký hiệu: "+lastPreflight.Blocking+" mục chặn (xem ! hoặc báo cáo "+report+").");return;}}
   LegendContext(d,j,"trước khi chọn điểm");LegendPlace011.CheckUnits(d.Database,chosen.map,chosen.plan);
   var pick2=d.Editor.GetPoint(new PromptPointOptions("\nChọn góc trên-trái của bảng ký hiệu (ESC huỷ): "));
   if(pick2.Status!=PromptStatus.OK){Say("Đã huỷ đặt bảng ký hiệu; bản vẽ không đổi.");return;}
   LegendContext(d,j,"sau khi chọn điểm");var ucs=d.Editor.CurrentUserCoordinateSystem;var z=Vector3d.ZAxis.TransformBy(ucs);if(!z.IsParallelTo(Vector3d.ZAxis)||z.Z<0)throw new Ex("UCS phải song song mặt bằng WCS (Z hướng lên); chưa đặt gì.");
   var x=Vector3d.XAxis.TransformBy(ucs);double rotation=Math.Atan2(x.Y,x.X);var at=pick2.Value.TransformBy(ucs);if(Math.Abs(at.Z)>1e-7)throw new Ex("V1 cần điểm ở WCS Z=0; chưa đặt gì.");
   if(!Meps06.SameScan(chosen,scan()))throw new Ex("Ký hiệu trong Model đã đổi sau khi kiểm tra; bấm tạo bảng lại. Chưa đặt gì.");
   var placed=LegendPlace011.Place(d.Database,root,chosen.map,chosen.plan,at,rotation,null);lastLegend=placed.legend;lastNotes=placed.notes;undoDoc=d;undoRevision=revision;
   foreach(var warning in placed.notes.Where(t=>t.StartsWith("CẢNH BÁO",StringComparison.Ordinal)))Warn(warning);
   string msg="Đã đặt bảng ký hiệu vào Model (1 khối "+placed.blockName+"): "+placed.legend.rows+" dòng, "+placed.legend.symbols+" ký hiệu"+(placed.renamed>0?"; "+placed.renamed+" tên trùng dùng tên riêng":"")+". "+LegendPlace011.TablePolicy+" Chưa tự lưu DWG; UNDO để bỏ.";
   Say(msg);d.Editor.WriteMessage("\nTVXD MEPS "+Meps06.Version+": "+msg);}
  // The exact Document / Model / INSUNITS / UCS of the legend click, UNDO on (zoom/pan does not matter).
  static void LegendContext(Document d,Job j,string when){if(App.DocumentManager.MdiActiveDocument!=d||!ReferenceEquals(d,j.doc))throw new Ex("Bản vẽ đã đổi "+when+"; chưa đặt bảng.");if(!d.Database.TileMode)throw new Ex("Không còn ở Model "+when+"; chưa đặt bảng.");
   if(d.Database.Insunits!=j.units)throw new Ex("INSUNITS đã đổi "+when+"; chưa đặt bảng.");if(!d.Editor.CurrentUserCoordinateSystem.IsEqualTo(j.ucs))throw new Ex("UCS đã đổi "+when+"; chưa đặt bảng.");
   if((Convert.ToInt32(App.GetSystemVariable("UNDOCTL"))&1)==0)throw new Ex("UNDO đang tắt; chưa đặt bảng.");}
  [CommandMethod("TVXDMEPS060UNDO",CommandFlags.Modal|CommandFlags.NoUndoMarker)]public void Undo(){var j=pending;pending=null;var d=App.DocumentManager.MdiActiveDocument;if(j==null||d!=undoDoc||revision!=undoRevision){Say("Không còn là bước MEPS gần nhất; dùng UNDO của CAD.");return;}undoDoc=null;d.Editor.Command("_.U");ReadUi();Say("Đã hoàn tác bước MEPS gần nhất.");}
 }
}
