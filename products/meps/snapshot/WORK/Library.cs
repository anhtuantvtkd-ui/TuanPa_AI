using System;using System.Collections.Generic;using System.Drawing;using System.Drawing.Drawing2D;using System.Globalization;using System.IO;using System.Linq;using System.Security.Cryptography;using System.Text;using System.Text.RegularExpressions;using System.Web.Script.Serialization;
namespace Tvxd.Meps {
 // 0.3 block library model: catalog JSON, path safety, search/category, scale math and preview rendering.
 // Pure .NET so it is testable without CAD; reading/normalizing/inserting DWG content lives in LibraryCad.cs.
 public sealed class LibAttr {public string tag,value,prompt;public bool constant,invisible;}
 // 0.5 (additive): style = null for 0.3.1/0.4 copies, LibraryStyle.Frozen when every entity carries an explicit effective lineweight, LibraryStyle.Ambiguous when
 // the source did not allow one value (file left unfrozen). styleNotes records the provenance of every frozen value. Older catalogs simply lack both fields.
 // 021 (additive): deletedUtc = null for every active entry; set only on a record of LibCatalog.deleted (time the user deleted it from the tool).
 public sealed class LibEntry {public string id,name,category,file,fileSha256,preview,previewStatus,status,sourceFile,sourceSha256,units,importedUtc,style,deletedUtc;public int insunits;public bool hidden,dynamic;public double minX,minY,maxX,maxY;public List<LibAttr> attributes=new List<LibAttr>();public List<string> nested=new List<string>(),warnings=new List<string>(),styleNotes=new List<string>();}
 // TVXD-MEPS-LIBDELETE-021: deleted = entries the user deleted from the tool (LibraryDelete021), kept ONLY so symbols already placed in drawings keep their
 // legend/identity lookup (Library.Reference / WithHistory). They are not entries: never listed, searched (also with hidden), grouped, restored or placed.
 // version 2 while deleted is non-empty, so a pre-021 binary (accepts version 1 only) refuses the catalog instead of saving it without the history.
 public sealed class LibCatalog {public string schema=Library.Schema;public int version=1;public List<LibEntry> entries=new List<LibEntry>(),deleted=new List<LibEntry>();}
 // k: "L" open polyline, "C" closed outline, "F" solid fill (even-odd rings), "P" pattern hatch (light fill + outline; pattern lines not drawn), "T" text, "D" point.
 public sealed class PreviewPrim {public string k;public List<double[]> rings=new List<double[]>();public string text;public double x,y,h,rot;public int ha,va;}
 // status: OK = every entity drawn; PARTIAL = some entity types not drawn (reason lists them); UNSUPPORTED = nothing drawable.
 public sealed class PreviewData {public string status="OK",reason;public double minX,minY,maxX,maxY;public List<PreviewPrim> prims=new List<PreviewPrim>();}
 // 0.5 (additive): ratio = "Theo tỷ lệ bản vẽ"; scale is then baseScale*drawingDenominator/refDenominator (Meps05.RatioScale), both denominators entered by the user.
 public sealed class LibPlacement {public string id;public double scale=1,rotationDeg,width,baseScale=1,refDenominator,drawingDenominator;public bool fitWidth,ratio;}
 // One typed record per definition/entity for source-vs-copy verification: text holds exact fields (type, layer, names, text, tags, flags, counts),
 // num holds geometric doubles only. Numbers are never parsed out of text, and text is never compared with a tolerance.
 public sealed class ShapeRecord {public List<string> text=new List<string>();public List<double> num=new List<double>();
  public ShapeRecord T(params string[] s){text.AddRange(s);return this;}
  public ShapeRecord N(params double[] v){num.AddRange(v);return this;}
  public override string ToString(){return string.Join("|",text)+" #"+string.Join(",",num.Select(v=>v.ToString("R",CultureInfo.InvariantCulture)));}}
 public static class Library {
  public const string Schema="TVXD_MEPS_LIBRARY_0_3",CatalogFile="catalog.json",BlocksDir="blocks",PreviewsDir="previews",NamePrefix="MEPS03_",All="(Tất cả)",Other="(KHÁC)";
  public const int MaxEntries=5000,MaxPreviewPoints=400000;
  public const double MinScale=1e-6,MaxScale=1e6;
  static readonly Regex IdPattern=new Regex("^[0-9a-f]{12}$"),HashPattern=new Regex("^[0-9a-f]{64}$");
  public static JavaScriptSerializer Json(){return new JavaScriptSerializer{MaxJsonLength=int.MaxValue,RecursionLimit=64};}
  // TVXD-MEPS-FLEXDUCT-022 PATCH01 (F023-01): raw JavaScriptSerializer output of an entry is not a stable snapshot (native evidence: the same E-TU-AT values
  // serialized with deletedUtc in a different position). EntrySnapshot = deterministic text of ALL public instance fields/properties of LibEntry/LibAttr
  // (found by reflection, so a future field cannot escape), names sorted ordinal, list order kept, strings/null/bools/ints exact, doubles as round-trip "R"
  // text (bit-exact, no tolerance, no JSON re-parse). Any other object type throws, so it is never silently dropped. Used for comparisons only; catalog
  // Save/Load/Validate and the stored JSON are unchanged.
  public static string EntrySnapshot(LibEntry e){return Json().Serialize(Stable(e,0));}
  static object Stable(object v,int depth){if(depth>16)throw new Exception("EntrySnapshot: cấu trúc quá sâu.");if(v==null)return null;
   if(v is string||v is bool||v is int||v is long)return v;
   if(v is double)return "d:"+((double)v).ToString("R",CultureInfo.InvariantCulture);if(v is float)return "f:"+((float)v).ToString("R",CultureInfo.InvariantCulture);
   if(v is System.Collections.IEnumerable){var list=new List<object>();foreach(var x in (System.Collections.IEnumerable)v)list.Add(Stable(x,depth+1));return list;}
   var t=v.GetType();if(t!=typeof(LibEntry)&&t!=typeof(LibAttr))throw new Exception("EntrySnapshot: kiểu chưa hỗ trợ "+t.FullName+".");
   var d=new SortedDictionary<string,object>(StringComparer.Ordinal);const System.Reflection.BindingFlags pub=System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.Instance;
   foreach(var f in t.GetFields(pub))d.Add(f.Name,Stable(f.GetValue(v),depth+1));
   foreach(var p in t.GetProperties(pub))if(p.CanRead&&p.GetIndexParameters().Length==0)d.Add(p.Name,Stable(p.GetValue(v,null),depth+1));
   return d;}
  static string Hex(byte[] b){var sb=new StringBuilder(b.Length*2);foreach(var x in b)sb.Append(x.ToString("x2"));return sb.ToString();}
  public static string Sha256File(string path){using(var s=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.Read))using(var h=SHA256.Create())return Hex(h.ComputeHash(s));}
  public static string Sha256Text(string s){using(var h=SHA256.Create())return Hex(h.ComputeHash(Encoding.UTF8.GetBytes(s)));}
  // AutoCAD symbol names are case-insensitive, so the ID uses the upper-case name.
  // Same source content + same block name always gives the same ID: a re-import is detected, never overwritten.
  public static string EntryId(string sourceSha256,string blockName){if(sourceSha256==null||!HashPattern.IsMatch(sourceSha256))throw new Exception("Hash nguồn không hợp lệ.");ValidName(blockName);return Sha256Text(sourceSha256+"|"+blockName.ToUpperInvariant()).Substring(0,12);}
  public static void ValidName(string name){if(string.IsNullOrWhiteSpace(name)||name.Length>200||name.Any(char.IsControl)||name.IndexOfAny("<>/\\\":;?*|,=`".ToCharArray())>=0)throw new Exception("Tên khối không hợp lệ: "+name);}
  static void Text(string s,int max){if(s!=null&&(s.Length>max||s.Any(c=>char.IsControl(c)&&c!='\r'&&c!='\n')))throw new Exception("Chuỗi catalog quá dài hoặc có ký tự điều khiển.");}
  // Category = name prefix before the second '-' (E-DEN-OP-D172 -> E-DEN). Organization only; it implies no device type or load.
  public static string Category(string name){var parts=(name??"").Split('-');if(parts.Length>=3&&parts[0].Length>0&&parts[1].Length>0)return (parts[0]+"-"+parts[1]).ToUpperInvariant();if(parts.Length==2&&parts[0].Length>0)return parts[0].ToUpperInvariant();return Other;}
  // Library files are addressed by portable relative paths; anything rooted, with "..", "." or empty segments, or resolving outside root is rejected.
  // Lexical containment is not enough: the root itself and every existing segment below it must not be a reparse point (junction/symlink/mount),
  // otherwise a crafted library folder could redirect reads/writes outside. Attributes are read without following the link. Checked on every call (not a lock against concurrent changes).
  public static string SafePath(string root,string relative){string full=LexicalPath(root,relative);NoReparse(root,full);return full;}
  static void NoReparse(string root,string full){string cur=Path.GetFullPath(root),baseFull=cur.TrimEnd('\\','/')+Path.DirectorySeparatorChar;var parts=new List<string>{""};parts.AddRange(full.Substring(baseFull.Length).Split(new[]{Path.DirectorySeparatorChar},StringSplitOptions.RemoveEmptyEntries));
   foreach(var part in parts){if(part.Length>0)cur=Path.Combine(cur,part);FileAttributes a;try{a=File.GetAttributes(cur);}catch(FileNotFoundException){return;}catch(DirectoryNotFoundException){return;}if((a&FileAttributes.ReparsePoint)!=0)throw new Exception("Đường dẫn thư viện đi qua junction/symlink, không dùng: "+cur);}}
  static string LexicalPath(string root,string relative){if(string.IsNullOrWhiteSpace(root)||!Path.IsPathRooted(root))throw new Exception("Thư mục thư viện phải là đường dẫn tuyệt đối.");if(string.IsNullOrWhiteSpace(relative)||relative.Length>200||Path.IsPathRooted(relative)||relative.IndexOfAny(Path.GetInvalidPathChars())>=0||relative.IndexOfAny(new[]{':','*','?','"','<','>','|'})>=0)throw new Exception("Đường dẫn thư viện không hợp lệ: "+relative);
   foreach(var part in relative.Split('/','\\'))if(part.Length==0||part=="."||part==".."||part.Trim()!=part||part.EndsWith("."))throw new Exception("Đường dẫn thư viện không hợp lệ: "+relative);
   string baseFull=Path.GetFullPath(root).TrimEnd('\\','/')+Path.DirectorySeparatorChar;string full=Path.GetFullPath(Path.Combine(baseFull,relative.Replace('/',Path.DirectorySeparatorChar)));if(!full.StartsWith(baseFull,StringComparison.OrdinalIgnoreCase))throw new Exception("Đường dẫn ra ngoài thư viện: "+relative);return full;}
  // Colours an entity can carry outside Entity.Color: those are not normalized, so the entry is marked UNSUPPORTED instead of falsely "all ACI 3".
  // MText \C / \c inline codes are detected conservatively (an escaped "\\C" also counts). Table/MLeader/Mline colours live in cell/style data.
  static readonly Regex InlineColorPattern=new Regex(@"\\[Cc]\d");
  static readonly string[] StyleColorTypes={"Table","MLeader","Mline"};
  public static bool HasInlineColor(string mtextContents){return mtextContents!=null&&InlineColorPattern.IsMatch(mtextContents);}
  public static string StyleColorIssue(string typeName){return StyleColorTypes.Contains(typeName)?"UNSUPPORTED "+typeName+": màu nằm trong style/ô bên trong, chưa chuẩn hóa được ACI 3":null;}
  // Copy verification tolerance (0.3.1): two geometric doubles match when |a-b| <= 1e-8 + 1e-12*max(|a|,|b|).
  // This only absorbs representational noise from DXF/DWG round trips (observed ~1e-13); geometry itself is never altered or rounded.
  public const double GeomAbsTol=1e-8,GeomRelTol=1e-12;
  public static bool GeomEqual(double a,double b){if(a==b||(double.IsNaN(a)&&double.IsNaN(b)))return true;if(!Core.Finite(a)||!Core.Finite(b))return false;return Math.Abs(a-b)<=GeomAbsTol+GeomRelTol*Math.Max(Math.Abs(a),Math.Abs(b));}
  // Record-by-record: count, exact text list, number count, then each number within GeomEqual. Returns at most max issues.
  public static List<string> CompareShapes(List<ShapeRecord> a,List<ShapeRecord> b,int max){var issues=new List<string>();if(a.Count!=b.Count)issues.Add("số đối tượng khác: nguồn "+a.Count+", bản sao "+b.Count);
   for(int i=0;i<Math.Min(a.Count,b.Count)&&issues.Count<max;i++){var x=a[i];var y=b[i];
    if(!x.text.SequenceEqual(y.text)){issues.Add("khác ở mục "+i+" (chuỗi): "+x+" ≠ "+y);continue;}
    if(x.num.Count!=y.num.Count){issues.Add("khác ở mục "+i+" (số lượng số "+x.num.Count+"/"+y.num.Count+"): "+x+" ≠ "+y);continue;}
    for(int k=0;k<x.num.Count;k++)if(!GeomEqual(x.num[k],y.num[k])){issues.Add("khác ở mục "+i+" ("+string.Join("|",x.text)+"), số thứ "+k+": "+x.num[k].ToString("R",CultureInfo.InvariantCulture)+" ≠ "+y.num[k].ToString("R",CultureInfo.InvariantCulture)+" (vượt dung sai 1e-8 + 1e-12·|giá trị|)");break;}}
   return issues;}
  // 021: active entries and deleted records share one ID space (an ID is either active or history, never both); a record of deleted carries deletedUtc and is
  // not hidden, an active entry never carries deletedUtc. Both lists get the same per-entry checks.
  public static void Validate(LibCatalog c){if(c==null||c.schema!=Schema||(c.version!=1&&c.version!=2)||c.entries==null)throw new Exception("catalog.json không đúng schema "+Schema+" phiên bản 1/2.");if(c.deleted==null)c.deleted=new List<LibEntry>();
   if(c.entries.Count>MaxEntries||c.deleted.Count>MaxEntries)throw new Exception("Catalog quá nhiều mục.");var ids=new HashSet<string>();
   foreach(var e in c.entries){ValidateEntry(e,ids);if(e.deletedUtc!=null)throw new Exception("Mục "+e.id+" đang dùng nhưng mang dấu đã xoá; catalog không hợp lệ.");}
   foreach(var e in c.deleted){ValidateEntry(e,ids);if(string.IsNullOrWhiteSpace(e.deletedUtc)||e.hidden)throw new Exception("Bản ghi đã xoá "+e.id+" thiếu thời điểm xoá hoặc còn cờ ẩn; catalog không hợp lệ.");Text(e.deletedUtc,64);}}
  static void ValidateEntry(LibEntry e,HashSet<string> ids){if(e==null||e.id==null||!IdPattern.IsMatch(e.id)||!ids.Add(e.id))throw new Exception("ID thư viện không hợp lệ hoặc trùng: "+(e==null?"null":e.id));ValidName(e.name);
    if(e.file!=BlocksDir+"/"+e.id+".dwg"||e.preview!=PreviewsDir+"/"+e.id+".json")throw new Exception("Đường dẫn file của "+e.id+" không đúng quy ước blocks/<id>.dwg, previews/<id>.json.");
    if(e.fileSha256==null||!HashPattern.IsMatch(e.fileSha256)||e.sourceSha256==null||!HashPattern.IsMatch(e.sourceSha256))throw new Exception("Hash của "+e.id+" không hợp lệ.");
    if(e.status!="OK"&&e.status!="UNSUPPORTED")throw new Exception("Trạng thái của "+e.id+" không hợp lệ.");
    foreach(double v in new[]{e.minX,e.minY,e.maxX,e.maxY})if(!Core.Finite(v))throw new Exception("Kích thước của "+e.id+" không hữu hạn.");if(e.status=="OK"&&!(e.maxX>=e.minX&&e.maxY>=e.minY))throw new Exception("Kích thước của "+e.id+" không hợp lệ.");
    if(e.attributes==null||e.nested==null||e.warnings==null||e.attributes.Any(a=>a==null))throw new Exception("Mục "+e.id+" thiếu danh sách.");
    if(e.styleNotes==null)e.styleNotes=new List<string>();
    foreach(var t in new[]{e.category,e.units,e.previewStatus,e.importedUtc,e.style}.Concat(e.warnings).Concat(e.nested).Concat(e.styleNotes).Concat(e.attributes.SelectMany(a=>new[]{a.tag,a.value,a.prompt})))Text(t,2000);
    if(string.IsNullOrWhiteSpace(e.sourceFile)||e.sourceFile.Length>260||e.sourceFile.IndexOfAny(new[]{'/','\\',':'})>=0||e.sourceFile.Any(char.IsControl))throw new Exception("Tên file nguồn của "+e.id+" không hợp lệ.");}
  public static LibCatalog Load(string root){string path=SafePath(root,CatalogFile);if(!File.Exists(path))return new LibCatalog();var c=Json().Deserialize<LibCatalog>(File.ReadAllText(path,Encoding.UTF8));Validate(c);foreach(var e in c.entries.Concat(c.deleted)){SafePath(root,e.file);SafePath(root,e.preview);}return c;}
  // Atomic: write a temp file beside the target, then File.Replace keeps the previous catalog as catalog.json.bak. version follows the deleted list (021).
  public static void Save(string root,LibCatalog c){if(c!=null&&c.deleted==null)c.deleted=new List<LibEntry>();if(c!=null)c.version=c.deleted.Count>0?2:1;Validate(c);string path=SafePath(root,CatalogFile);Directory.CreateDirectory(Path.GetDirectoryName(path));string text=Json().Serialize(c);Validate(Json().Deserialize<LibCatalog>(text));WriteAtomic(path,text);}
  public static void WriteAtomic(string path,string text){string temp=path+"."+Guid.NewGuid().ToString("N")+".tmp";try{File.WriteAllText(temp,text,new UTF8Encoding(false));if(File.Exists(path))File.Replace(temp,path,path+".bak");else File.Move(temp,path);}finally{if(File.Exists(temp))File.Delete(temp);}}
  // Placement, tool actions and every list use ACTIVE entries only (Find, Search, Categories never read deleted).
  public static LibEntry Find(LibCatalog c,string id){var e=c.entries.FirstOrDefault(x=>x.id==id);if(e==null)throw new Exception("Không tìm thấy mục thư viện "+id+".");return e;}
  // 021: reference lookup for symbols ALREADY placed in a drawing (legend/identity paths only): the active entry, else the deleted record; null when neither.
  public static LibEntry Reference(LibCatalog c,string id){if(c==null)return null;return c.entries.FirstOrDefault(x=>x!=null&&x.id==id)??(c.deleted??new List<LibEntry>()).FirstOrDefault(x=>x!=null&&x.id==id);}
  // Read-only reference view (active entries + deleted records as entries) for the legend preflight/scan of an existing drawing. Never saved: its deleted
  // records carry deletedUtc, which Validate refuses for an active entry, so Save(view) throws. Never given to Search/Categories/placement/registry.
  public static LibCatalog WithHistory(LibCatalog c){if(c==null)return null;return new LibCatalog{schema=c.schema,version=c.version,entries=c.entries.Concat(c.deleted??new List<LibEntry>()).ToList(),deleted=new List<LibEntry>()};}
  // Hide/restore only flips a catalog flag (re-read from disk first). Library files, the source and blocks already in drawings are never deleted.
  public static void SetHidden(string root,string id,bool hidden){var c=Load(root);var e=Find(c,id);if(e.hidden==hidden)return;e.hidden=hidden;Save(root,c);}
  static bool Has(string s,string q){return s!=null&&s.IndexOf(q,StringComparison.OrdinalIgnoreCase)>=0;}
  public static LibEntry[] Search(LibCatalog c,string text,string category,bool includeHidden){string q=(text??"").Trim();return c.entries.Where(e=>(includeHidden||!e.hidden)&&(string.IsNullOrEmpty(category)||category==All||e.category==category)&&(q.Length==0||Has(e.name,q)||Has(e.category,q)||Has(e.id,q)||Has(e.sourceFile,q))).OrderBy(e=>e.category,StringComparer.OrdinalIgnoreCase).ThenBy(e=>e.name,StringComparer.OrdinalIgnoreCase).ToArray();}
  public static string[] Categories(LibCatalog c,bool includeHidden){return new[]{All}.Concat(c.entries.Where(e=>includeHidden||!e.hidden).Select(e=>e.category).Distinct().OrderBy(x=>x,StringComparer.OrdinalIgnoreCase)).ToArray();}
  public static double CheckScale(double s){if(!Core.Finite(s)||s<MinScale||s>MaxScale)throw new Exception("Tỉ lệ phải là số dương hữu hạn trong khoảng 0.000001–1000000.");return s;}
  public static double CheckRotation(double deg){if(!Core.Finite(deg)||Math.Abs(deg)>360)throw new Exception("Góc xoay phải trong khoảng -360…360 độ.");return deg;}
  public static double Width(LibEntry e){return e.maxX-e.minX;}
  public static double Height(LibEntry e){return e.maxY-e.minY;}
  // Optional fit: one uniform factor from the block's own width, so the aspect ratio is kept.
  public static double FitWidthScale(LibEntry e,double width){if(!Core.Finite(width)||width<=0)throw new Exception("Bề rộng mong muốn phải là số dương.");double w=Width(e);if(!Core.Finite(w)||w<=1e-12)throw new Exception("Khối không có bề rộng hợp lệ để co theo bề rộng.");return CheckScale(width/w);}
  public static string F(double v){return v.ToString("0.####",CultureInfo.InvariantCulture);}
  public static string SizeText(LibEntry e,double scale){CheckScale(scale);return "Gốc "+F(Width(e))+" × "+F(Height(e))+" ("+e.units+") → đặt "+F(Width(e)*scale)+" × "+F(Height(e)*scale)+" đơn vị bản vẽ, tỉ lệ "+F(scale)+" (chưa xoay).";}
  public static PreviewData LoadPreview(string root,LibEntry e){try{string p=SafePath(root,e.preview);if(!File.Exists(p))return Missing("Thiếu file xem trước "+e.preview);var d=Json().Deserialize<PreviewData>(File.ReadAllText(p,Encoding.UTF8));ValidatePreview(d);return d;}catch(Exception ex){return Missing("File xem trước lỗi: "+ex.Message);}}
  static PreviewData Missing(string reason){return new PreviewData{status="UNSUPPORTED",reason=reason};}
  public static void ValidatePreview(PreviewData d){if(d==null||d.prims==null||!new[]{"OK","PARTIAL","UNSUPPORTED"}.Contains(d.status))throw new Exception("Dữ liệu xem trước sai cấu trúc.");Text(d.reason,4000);int points=0;
   foreach(double v in new[]{d.minX,d.minY,d.maxX,d.maxY})if(!Core.Finite(v))throw new Exception("Khung xem trước không hữu hạn.");
   foreach(var p in d.prims){if(p==null||!new[]{"L","C","F","P","T","D"}.Contains(p.k)||p.rings==null)throw new Exception("Nét xem trước sai loại.");foreach(var r in p.rings){if(r==null||r.Length<2||r.Length%2!=0||r.Any(v=>!Core.Finite(v)))throw new Exception("Toạ độ xem trước sai.");points+=r.Length/2;}
    if(p.k=="T"){Text(p.text,4000);if(!Core.Finite(p.x)||!Core.Finite(p.y)||!Core.Finite(p.h)||!Core.Finite(p.rot)||p.ha<0||p.ha>2||p.va<0||p.va>2)throw new Exception("Chữ xem trước sai.");}}
   if(points>MaxPreviewPoints)throw new Exception("Xem trước quá nhiều điểm.");}
  static readonly Color Ink=Color.FromArgb(0,110,0),Pattern=Color.FromArgb(190,225,190);
  // Draws the stored geometry fitted to the square. Normalized blocks are ACI 3 green; a darker green is used for contrast on white.
  // Text uses a Windows sans-serif font scaled to the CAD height, so glyph widths can differ slightly from CAD fonts.
  // A preview without drawable geometry is shown as an explicit "no preview" tile, never a substitute picture.
  public static Bitmap Render(PreviewData d,int size){if(size<24||size>1024)throw new Exception("Kích thước xem trước không hợp lệ.");var bitmap=new Bitmap(size,size);using(var g=Graphics.FromImage(bitmap)){g.SmoothingMode=SmoothingMode.AntiAlias;g.Clear(Color.White);
   if(d==null||d.status=="UNSUPPORTED"||d.prims==null||d.prims.Count==0){using(var pen=new Pen(Color.FromArgb(200,60,40),2))g.DrawRectangle(pen,2,2,size-5,size-5);using(var font=new Font("Segoe UI",Math.Max(6f,size/14f)))using(var brush=new SolidBrush(Color.FromArgb(160,40,30)))g.DrawString("KHÔNG CÓ HÌNH XEM TRƯỚC"+(d!=null&&d.reason!=null?"\n"+d.reason:""),font,brush,new RectangleF(6,6,size-12,size-12),new StringFormat{Alignment=StringAlignment.Center,LineAlignment=StringAlignment.Center});return bitmap;}
   double w=Math.Max(d.maxX-d.minX,1e-9),h=Math.Max(d.maxY-d.minY,1e-9);float margin=Math.Max(3f,size/32f);double s=Math.Min((size-2*margin)/w,(size-2*margin)/h);if(d.maxX-d.minX<1e-9&&d.maxY-d.minY<1e-9)s=1;double ox=margin+((size-2*margin)-w*s)/2,oy=margin+((size-2*margin)-h*s)/2;
   Func<double,double,PointF> px=(x,y)=>new PointF((float)(ox+(x-d.minX)*s),(float)(size-oy-(y-d.minY)*s));
   using(var pen=new Pen(Ink,Math.Max(1f,size/96f)))using(var brush=new SolidBrush(Ink))using(var light=new SolidBrush(Pattern))foreach(var p in d.prims){
    if(p.k=="T"){if(string.IsNullOrEmpty(p.text)||p.h<=0)continue;using(var path=new GraphicsPath()){var fam=FontFamily.GenericSansSerif;path.AddString(p.text,fam,(int)FontStyle.Regular,100,PointF.Empty,StringFormat.GenericTypographic);var box=path.GetBounds();if(box.Width<=0)continue;float baseline=100f*fam.GetCellAscent(FontStyle.Regular)/fam.GetEmHeight(FontStyle.Regular),cap=72f,k=(float)(p.h*s)/cap;float ax=p.ha==1?box.Left+box.Width/2:p.ha==2?box.Right:box.Left,ay=p.va==1?baseline-cap/2:p.va==2?baseline-cap:baseline;var a=px(p.x,p.y);using(var m=new Matrix()){m.Translate(a.X,a.Y);m.Rotate((float)(-p.rot*180/Math.PI));m.Scale(k,k);m.Translate(-ax,-ay);path.Transform(m);}g.FillPath(brush,path);}continue;}
    if(p.k=="D"){foreach(var r in p.rings){var a=px(r[0],r[1]);g.FillEllipse(brush,a.X-1.5f,a.Y-1.5f,3,3);}continue;}
    if(p.k=="F"||p.k=="P"){using(var path=new GraphicsPath(FillMode.Alternate)){foreach(var r in p.rings)if(r.Length>=6)path.AddPolygon(Points(r,px));if(path.PointCount>0){g.FillPath(p.k=="F"?(Brush)brush:light,path);g.DrawPath(pen,path);}}continue;}
    foreach(var r in p.rings){var pts=Points(r,px);if(pts.Length==1)g.FillRectangle(brush,pts[0].X,pts[0].Y,1,1);else if(p.k=="C"&&pts.Length>=3)g.DrawPolygon(pen,pts);else g.DrawLines(pen,pts);}}
   if(d.status=="PARTIAL")using(var badge=new SolidBrush(Color.FromArgb(230,140,0)))using(var font=new Font("Segoe UI",Math.Max(6f,size/12f),FontStyle.Bold)){g.FillRectangle(badge,size-size/5f,0,size/5f,size/5f);g.DrawString("!",font,Brushes.White,size-size/5f+1,0);}}
   return bitmap;}
  static PointF[] Points(double[] r,Func<double,double,PointF> px){var pts=new PointF[r.Length/2];for(int i=0;i<pts.Length;i++)pts[i]=px(r[2*i],r[2*i+1]);return pts;}
 }
}
