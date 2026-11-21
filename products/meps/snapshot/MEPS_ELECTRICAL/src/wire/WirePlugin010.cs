using System;using System.IO;using System.Collections.Generic;using Autodesk.AutoCAD.Runtime;using Autodesk.AutoCAD.ApplicationServices;using Autodesk.AutoCAD.EditorInput;using Autodesk.AutoCAD.Geometry;using App=Autodesk.AutoCAD.ApplicationServices.Application;
using DbPolyline=Autodesk.AutoCAD.DatabaseServices.Polyline;using CadColor=Autodesk.AutoCAD.Colors.Color;using CadColorMethod=Autodesk.AutoCAD.Colors.ColorMethod;
using TransientManager=Autodesk.AutoCAD.GraphicsInterface.TransientManager;using TransientDrawingMode=Autodesk.AutoCAD.GraphicsInterface.TransientDrawingMode;
[assembly:CommandClass(typeof(Tvxd.Meps.WirePlugin010))]
namespace Tvxd.Meps {
 // TVXD-MEPS-LEGEND-011 wire fixes: F2 MSLTSCALE/annotation scale captured with the sample and re-read before drawing (mismatch refused); F3 a queued request
 // is dropped when another command starts first and by Esc in the palette (event-driven, no polling); F4 a point off Z=0 is re-prompted; F5 accepted
 // segments are shown as a transient preview (never in the drawing) until Enter, removed on every exit path; F8 explicit recovery of a corrupt library;
 // F12 a focus failure no longer cancels a dispatched request; F13 no full Regen after drawing.
 public sealed partial class WirePlugin010 {
  static WireView010 view;static Func<bool> otherBusy;static Request pending;static bool running,hooked;static Document watched;
  // TVXD-MEPS-LAYERS-012 (STANDARDS012): preset = snapshot of a profile wire/pipe/earth/trunk layer taken at the click (StandardsCad012.Snapshot: layer,
  // units, LTSCALE, MSLTSCALE, annotation scale); drawn ByLayer on that layer by StandardsCad012.DrawPreset. Never written to styles.json; captured samples
  // are unchanged and are not reinterpreted.
  sealed class Request {public Document doc;public Matrix3d ucs;public int units;public WireStyle010 style;public WireSettings010 settings;public LastWire intent;
#if STANDARDS012
   public PresetSnapshot012 preset;
#endif
  }
  static bool IsPreset(Request r){
#if STANDARDS012
   return r.preset!=null;
#else
   return false;
#endif
  }
  public static bool Busy{get{return pending!=null||running;}}
  // TVXD-MEPS-WIREREPEAT-026 (user 2026-10-10): Enter/Space at the Command prompt repeats TVXDMEPSWIRE010 through AutoCAD's own command history (no key hook,
  // no timer). A repeat without a queued request draws a NEW route of the last type successfully drawn IN THAT DOCUMENT: an immutable copy of the sample
  // style, or the preset layer key only (re-snapshotted each time). No points, ObjectIds or old Request are kept. Remembered only after the first committed
  // segment (cancel-before-draw and sample capture never replace it); cleared when the document is destroyed. Each repeat builds a fresh Request with the
  // CURRENT UCS/units/LTSCALE/MSLTSCALE/annotation scale and the same checks as a palette click.
  sealed class LastWire {public WireStyle010 style;public string presetLayer;}
  static readonly Dictionary<Document,LastWire> last=new Dictionary<Document,LastWire>();
  static WireStyle010 Copy(WireStyle010 s){var j=Library.Json();return j.Deserialize<WireStyle010>(j.Serialize(s));}
  public static bool HasRepeat(Document d){return d!=null&&last.ContainsKey(d);}
  static void Remember(Request r){if(r==null||r.intent==null||r.doc==null)return;last[r.doc]=new LastWire{style=r.intent.style==null?null:Copy(r.intent.style),presetLayer=r.intent.presetLayer};}
  // Fresh request for the active document from its last drawn type; null + reason when there is none or it cannot be used now (nothing written).
  static Request Repeat(out string why){why=null;var d=App.DocumentManager.MdiActiveDocument;LastWire lw;
   if(d==null||!last.TryGetValue(d,out lw)){why="Chưa có loại dây đã vẽ trong bản vẽ này: bấm Chọn dây dẫn trong bảng MEPS.";return null;}
   if(otherBusy!=null&&otherBusy()){why="Hoàn tất thao tác MEPS đang chạy trước.";return null;}
   try{var r=new Request{doc=d,ucs=d.Editor.CurrentUserCoordinateSystem,units=(int)d.Database.Insunits,settings=Settings(d),intent=lw};WireModel010.Validate(r.settings);
    if(lw.presetLayer!=null){
#if STANDARDS012
     r.preset=StandardsCad012.Snapshot(d.Database,d.Name,lw.presetLayer,r.settings);
#else
     why="Nét chuẩn không có trong bản này.";return null;
#endif
    }else{r.style=Copy(lw.style);if(r.style.units!=r.units)throw new InvalidOperationException("Đơn vị bản vẽ khác mẫu "+r.style.name+"; không tự đổi đơn vị.");WireModel010.CheckSettings(r.style,r.settings);}
    Context(r);return r;}
   catch(System.Exception ex){why="Không lặp lại được loại dây trước: "+ex.Message+" Chưa vẽ.";return null;}}
  public static string LibraryPath{get{return Path.Combine(Path.GetDirectoryName(typeof(WirePlugin010).Assembly.Location),"WIRE_LIBRARY_010","styles.json");}}
  public static void Attach(CompactView06 owner,Func<bool> occupied){otherBusy=occupied;view=new WireView010(owner);owner.blank.Controls.Add(view);view.CaptureSample=()=>Queue(null);view.Draw=s=>Queue(s);view.CancelRequest=()=>Cancel();view.Recover=RecoverLibrary;view.ManageCatalog=ManageCatalog061;view.DrawChoice=QueueChoice061;
#if STANDARDS012
   view.DrawPreset=p=>QueuePreset(p);view.SetPresets(Standards012.Presets());
#endif
   Reload();
   if(!hooked){hooked=true;App.DocumentManager.DocumentActivated+=(s,e)=>{if(pending!=null&&pending.doc!=e.Document){Drop();Message("Đổi bản vẽ: đã huỷ yêu cầu nét đang chờ.",true);}};App.DocumentManager.DocumentToBeDestroyed+=(s,e)=>{if(pending!=null&&pending.doc==e.Document)Drop();if(watched==e.Document)Unwatch();last.Remove(e.Document);};}}
  static void Message(string s,bool err){if(view!=null)view.Message(s,err);try{var d=App.DocumentManager.MdiActiveDocument;if(d!=null)d.Editor.WriteMessage("\nMEPS Nét dây: "+s);}catch{} }
  static void Reload(){try{ReloadCatalog061(WireModel010.Load(LibraryPath).styles);}catch(System.Exception ex){view.ShowRecovery(ex is InvalidDataException);Message("Không đọc được thư viện: "+ex.Message+(ex is InvalidDataException?" Bấm 'Khôi phục…' (giữ bản hỏng, không ghi đè).":""),true);}}
  // F8: explicit, confirmed recovery; the corrupt file is kept as a new copy and the backup is never overwritten (WireModel010.Recover).
  static void RecoverLibrary(){if(Busy)throw new InvalidOperationException("Hoàn tất thao tác nét đang chạy trước.");
   var answer=System.Windows.Forms.MessageBox.Show("Tệp thư viện nét styles.json không đọc được.\n\nMEPS sẽ GIỮ bản hỏng thành một tệp styles.json.corrupt_… mới, rồi khôi phục từ bản sao lưu styles.json.bak nếu hợp lệ (nếu không: thư viện trống).\n\nTiếp tục?","MEPS Nét dây — khôi phục thư viện",System.Windows.Forms.MessageBoxButtons.YesNo,System.Windows.Forms.MessageBoxIcon.Warning,System.Windows.Forms.MessageBoxDefaultButton.Button2);
   if(answer!=System.Windows.Forms.DialogResult.Yes){Message("Chưa khôi phục; tệp thư viện giữ nguyên.",false);return;}
   string kept;string text=WireModel010.Recover(LibraryPath,out kept);Reload();Message(text,false);}
  // Model linetype display settings of the active drawing (supported API: MSLTSCALE system variable, Database.Cannoscale).
  static WireSettings010 Settings(Document d){var cs=d.Database.Cannoscale;double f=cs!=null&&cs.PaperUnits>0?cs.DrawingUnits/cs.PaperUnits:0;
   return new WireSettings010{msltscale=Convert.ToInt32(App.GetSystemVariable("MSLTSCALE")),annoScale=f,annoName=cs==null?null:cs.Name};}
  static void Context(Request r){if(r.doc!=App.DocumentManager.MdiActiveDocument||!r.doc.Database.TileMode||(int)r.doc.Database.Insunits!=r.units||!r.doc.Editor.CurrentUserCoordinateSystem.IsEqualTo(r.ucs))throw new InvalidOperationException("Bản vẽ/UCS/đơn vị đã đổi; bấm lại mẫu.");
   if(!r.ucs.IsEqualTo(Matrix3d.Identity))throw new InvalidOperationException("Bản đầu cần UCS World để vẽ nét 2D đúng tọa độ; không tự đổi UCS.");if((Convert.ToInt32(App.GetSystemVariable("UNDOCTL"))&1)==0)throw new InvalidOperationException("UNDO đang tắt; chưa sửa bản vẽ.");
   if(!WireModel010.SameSettings(Settings(r.doc),r.settings))throw new InvalidOperationException("MSLTSCALE/tỷ lệ chú thích đã đổi sau khi bấm; bấm lại mẫu. Chưa vẽ.");}
  // F3: a queued request belongs to the next command of its document only.
  static void Watch(Document d){Unwatch();watched=d;d.CommandWillStart+=OnCommand;}
  static void Unwatch(){if(watched==null)return;try{watched.CommandWillStart-=OnCommand;}catch{}watched=null;}
  static void Drop(){pending=null;Unwatch();}
  static void OnCommand(object s,CommandEventArgs e){if(pending==null||running||string.Equals(e.GlobalCommandName,"TVXDMEPSWIRE010",StringComparison.OrdinalIgnoreCase))return;Drop();Message("Lệnh "+e.GlobalCommandName+" chạy trước: đã huỷ yêu cầu nét đang chờ; bản vẽ giữ nguyên.",false);}
#if WIRECOMMIT018
  // TVXD-MEPS-WIRESCROLL-018 (WIRECOMMIT018): each segment is saved as soon as its endpoint is accepted (WireRoute018); Esc/Enter stop and keep it.
  public const string DrawHint="Chọn điểm; mỗi đoạn lưu ngay. Esc dừng và giữ nét; U bỏ đoạn vừa vẽ. Xong tuyến: Enter/Space tại dòng Command vẽ tuyến mới cùng loại.";
  const string CancelText="Đang chọn điểm: nhấn Esc trong cửa sổ bản vẽ để dừng; các đoạn đã vẽ được giữ.";
#else
  public const string DrawHint="Enter hoàn tất; Esc huỷ toàn bộ tuyến đang vẽ. Xong tuyến: Enter/Space tại dòng Command vẽ tuyến mới cùng loại.";
  const string CancelText="Đang chọn điểm: nhấn Esc trong cửa sổ bản vẽ để huỷ tuyến.";
#endif
  public static bool Cancel(){if(running){Message(CancelText,false);return false;}if(pending==null)return false;Drop();Message("Đã huỷ yêu cầu nét đang chờ (Esc trong bảng); bản vẽ giữ nguyên.",false);return true;}
  static Request NewRequest(WireStyle010 style){var d=App.DocumentManager.MdiActiveDocument;if(d==null)throw new InvalidOperationException("Mở bản vẽ trước.");if(Busy||(otherBusy!=null&&otherBusy())||!string.IsNullOrEmpty(d.CommandInProgress))throw new InvalidOperationException("Hoàn tất lệnh đang chạy hoặc nhấn Esc trước.");
   var r=new Request{doc=d,ucs=d.Editor.CurrentUserCoordinateSystem,units=(int)d.Database.Insunits,style=style,settings=Settings(d)};WireModel010.Validate(r.settings);Context(r);return r;}
  static void Queue(WireStyle010 style){var r=NewRequest(style);if(style!=null){if(style.units!=r.units)throw new InvalidOperationException("Đơn vị bản vẽ khác mẫu; không tự đổi đơn vị.");WireModel010.CheckSettings(style,r.settings);r.intent=new LastWire{style=Copy(style)};}
   Start(r,style==null?"Chọn một LINE/LWPOLYLINE trong Model.":"Chọn điểm tuyến nét. "+DrawHint);}
#if STANDARDS012
  // The click binds this document and the preset's intent; a non-mm drawing is refused here for dashed presets (units are never changed).
  static void QueuePreset(WirePreset012 p){var r=NewRequest(null);r.preset=StandardsCad012.Snapshot(r.doc.Database,r.doc.Name,p.layer,r.settings);r.intent=new LastWire{presetLayer=p.layer};
   Start(r,"Nét chuẩn "+p.layer+" ("+p.linetype+", ByLayer): chọn điểm tuyến. "+DrawHint);}
#endif
  static void Start(Request r,string prompt){pending=r;Watch(r.doc);
   try{r.doc.SendStringToExecute("TVXDMEPSWIRE010 ",true,false,false);}catch{Drop();throw;}
   // F12: the request is already dispatched; a focus failure only means the user clicks into the drawing.
   try{Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView();}catch{}
   Message(prompt,false);}
  // F5: accepted segments as one transient polyline (not database-resident), replaced after each point and removed on every exit path.
  sealed class Preview:IDisposable {DbPolyline line;readonly IntegerCollection vps=new IntegerCollection();readonly CadColor color;
   public Preview(WireStyle010 s):this(s.rgb?CadColor.FromRgb((byte)s.red,(byte)s.green,(byte)s.blue):CadColor.FromColorIndex(CadColorMethod.ByAci,(short)s.aci)){}
   public Preview(CadColor c){color=c;}
   public void Show(IList<Point3d> pts){Clear();if(pts.Count<2)return;var p=new DbPolyline();for(int i=0;i<pts.Count;i++)p.AddVertexAt(i,new Point2d(pts[i].X,pts[i].Y),0,0,0);p.Color=color;line=p;
    TransientManager.CurrentTransientManager.AddTransient(line,TransientDrawingMode.DirectShortTerm,128,vps);}
   void Clear(){if(line==null)return;try{TransientManager.CurrentTransientManager.EraseTransient(line,vps);}catch{}line.Dispose();line=null;}
   public void Dispose(){Clear();}}
  [CommandMethod("TVXDMEPSWIRE010",CommandFlags.Modal)]public void Run(){var r=pending;Drop();
   // 026: no queued request = Enter/Space repeat at the Command prompt → new route of this document's last drawn type (fresh context), else nothing.
   if(r==null){string why;r=Repeat(out why);if(r==null){Message(why,false);return;}Message("Lặp lại "+(r.intent.presetLayer!=null?"nét chuẩn "+r.intent.presetLayer:"mẫu "+r.style.name)+": tuyến mới. "+DrawHint,false);}
   running=true;Preview preview=null;
   try{Context(r);if(r.style==null&&!IsPreset(r)){var opt=new PromptEntityOptions("\nChọn nét mẫu LINE/LWPOLYLINE (Esc huỷ): ");opt.SetRejectMessage("\nChỉ chọn LINE/LWPOLYLINE.");opt.AddAllowedClass(typeof(Autodesk.AutoCAD.DatabaseServices.Line),true);opt.AddAllowedClass(typeof(DbPolyline),true);var picked=r.doc.Editor.GetEntity(opt);if(picked.Status!=PromptStatus.OK){Message("Đã huỷ lấy mẫu; bản vẽ giữ nguyên.",false);return;}Context(r);
     var s=WireCad010.Capture(r.doc.Database,picked.ObjectId,r.doc.Name,Convert.ToInt32(App.GetSystemVariable("LWDEFAULT")),r.settings);WireModel010.Add(LibraryPath,s);Reload();Message("Đã lưu "+s.name+". Bấm mẫu để vẽ.",false);return;}
#if WIRECOMMIT018
    Route018(r);
#else
    var points=new List<Point3d>();
#if STANDARDS012
    preview=r.preset!=null?new Preview(CadColor.FromColorIndex(CadColorMethod.ByAci,(short)Standards012.Layer(r.preset.layer).aci)):new Preview(r.style);
#else
    preview=new Preview(r.style);
#endif
    while(true){Context(r);var opt=new PromptPointOptions(points.Count==0?"\nĐiểm đầu (Esc huỷ): ":"\nĐiểm tiếp theo (Enter xong / Esc huỷ): "){AllowNone=points.Count>0};if(points.Count>0){opt.UseBasePoint=true;opt.BasePoint=points[points.Count-1];opt.UseDashedLine=false;}var got=r.doc.Editor.GetPoint(opt);
     if(got.Status==PromptStatus.Cancel){Message("Đã huỷ tuyến; không tạo nét nào.",false);return;}if(got.Status==PromptStatus.None){if(points.Count<2){Message("Cần hai điểm; chưa tạo nét.",false);return;}break;}if(got.Status!=PromptStatus.OK){Message("Đã dừng; chưa tạo nét.",true);return;}
     // F4: re-prompt instead of discarding the route.
     if(Math.Abs(got.Value.Z)>1e-8){Message("Điểm ngoài mặt phẳng XY (Z ≠ 0); chọn lại điểm khác (tuyến đang vẽ giữ nguyên).",true);continue;}
     if(points.Count>0&&got.Value.DistanceTo(points[points.Count-1])<1e-8){Message("Điểm trùng; chọn điểm khác.",false);continue;}points.Add(got.Value);preview.Show(points);}
    preview.Dispose();preview=null;Context(r);
#if STANDARDS012
    if(r.preset!=null){StandardsCad012.DrawPreset(r.doc.Database,r.preset,points,Settings(r.doc));Remember(r);Message("Đã vẽ "+(points.Count-1)+" đoạn nét chuẩn "+r.preset.layer+" (ByLayer, layer có sẵn hoặc vừa chuẩn bị). Undo để hoàn tác tuyến.",false);return;}
#endif
    WireCad010.Draw(r.doc.Database,r.style,points,Settings(r.doc));Remember(r);Message("Đã vẽ "+(points.Count-1)+" đoạn theo mẫu "+r.style.name+". Undo để hoàn tác tuyến.",false);
#endif
   }catch(System.Exception ex){Message(ex.Message,true);}finally{if(preview!=null)preview.Dispose();running=false;}}
#if WIRECOMMIT018
  // One owned polyline per route (WireRoute018): the anchor writes nothing, every further accepted point is saved at once. The rubber band is the
  // GetPoint base line only (no transient copy of the saved route). U removes the last segment inside this route; Esc/Enter stop and keep it.
  // Errors after a save never say "not drawn": the saved segment count is reported (WireRoute018.Retained).
  static void Route018(Request r){WireRoute018 route;string what;
#if STANDARDS012
   if(r.preset!=null){route=new WireRoute018(r.doc.Database,r.preset,r.settings);what="nét chuẩn "+r.preset.layer+" (ByLayer)";}else
#endif
   {route=new WireRoute018(r.doc.Database,r.style,r.settings);what="mẫu "+r.style.name;}
   try{while(true){
     try{Context(r);}catch(System.Exception ex){Message(WireRoute018.Retained(ex.Message,route.SegmentCount),true);return;}
     int n=route.PointCount;var opt=new PromptPointOptions(n==0?"\nĐiểm đầu (Esc/Enter dừng)":n==1?"\nĐiểm tiếp theo, đoạn lưu ngay (U bỏ điểm đầu; Esc/Enter dừng)":"\nĐiểm tiếp theo, đoạn lưu ngay (U bỏ đoạn vừa vẽ; Esc/Enter dừng, giữ nét)"){AllowNone=true};
     if(n>0){opt.Keywords.Add("U");opt.AppendKeywordsToMessage=true;opt.UseBasePoint=true;opt.BasePoint=route.LastPoint.Value;opt.UseDashedLine=false;}
     var got=r.doc.Editor.GetPoint(opt);
     // MODELLIB019 F018-CONTEXT: re-check document/UCS/units/settings as soon as the prompt returns, before any route.Add/route.Undo.
     try{Context(r);}catch(System.Exception ex){Message(WireRoute018.Retained(ex.Message,route.SegmentCount),true);return;}
     if(got.Status==PromptStatus.Cancel||got.Status==PromptStatus.None)break;
     if(got.Status==PromptStatus.Keyword){if(!string.Equals(got.StringResult,"U",StringComparison.OrdinalIgnoreCase))continue;
      try{Message(route.Undo(Settings(r.doc)),false);}catch(System.Exception ex){Message(WireRoute018.Retained(ex.Message,route.SegmentCount),true);return;}continue;}
     if(got.Status!=PromptStatus.OK){Message(WireRoute018.Retained("Đã dừng chọn điểm.",route.SegmentCount),true);return;}
     // F4 and invalid/duplicate/limit points: re-prompt, nothing written.
     string issue=route.PointIssue(got.Value);if(issue!=null){Message(issue,true);continue;}
     try{route.Add(got.Value,Settings(r.doc));if(route.SegmentCount>0)Remember(r);}catch(ArgumentException ex){Message(ex.Message,true);continue;}catch(System.Exception ex){Message(WireRoute018.Retained(ex.Message,route.SegmentCount),true);return;}}
    int saved=route.SegmentCount;
    Message(saved==0?"Đã dừng; không tạo nét nào.":"Đã dừng; giữ "+saved+" đoạn "+what+" (đã lưu từng đoạn). UNDO sau lệnh có thể hoàn tác cả tuyến; U trong lúc vẽ bỏ đoạn vừa vẽ.",false);}
   catch(System.Exception ex){Message(WireRoute018.Retained(ex.Message,route.SegmentCount),true);}finally{route.Finish();}}
#endif
 }
}
