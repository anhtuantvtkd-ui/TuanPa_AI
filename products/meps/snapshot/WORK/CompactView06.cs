using System;using System.Drawing;using System.Drawing.Drawing2D;using System.Linq;using System.Windows.Forms;using System.Collections.Generic;using System.Runtime.InteropServices;
#if MODELLIB019 && !CABLES016
#error MODELLIB019 palette changes need CABLES016 (the ? guide and header layout).
#endif
#if LAYOUT030 && !FLEX022
#error LAYOUT030 needs FLEX022 (the Dàn trang row sits under the wire/flex row).
#endif
#if UI031 && !(LAYOUT030 && MODELLIB019)
#error UI031 needs LAYOUT030 and MODELLIB019 (it restyles the 030 palette and its action rows).
#endif
#if TAG033 && !UI031
#error TAG033 needs UI031 (the TAG button takes the right slot of the 031 DÀN TRANG row).
#endif
namespace Tvxd.Meps {
 // 0.6.0 compact palette (TVXD-MEPS-IMPLEMENT-008, user-approved mockup): small floating window, client about 208 × 500 logical px (80 % of 250 × 625).
 //  - header: "MEPS / Điện" and compact rounded buttons + (add block), ▦ (legend), ⚙ (ratio setup), ! (all messages and errors in full, with tooltips);
 //  - one wide search row with a small ▼ group filter button (26 px; menu of groups with the current one checked, current group in its tooltip; no category
 //    chip row, no permanent group combo box); a slim "Tỷ lệ ký hiệu" dropdown (Gốc 1×, 1:100, 1:125, 1:150, 1:250, custom);
 //  - two-column list of 32 px tiles with 16 px icons and regular names in the upper half; the lower half stays blank.
 // No selected-block details, footer, status row or Place button. Sizes are logical px × Dpi (system DPI / 96) and fonts are pixel sizes with the same factor,
 // so the layout scales as one piece (AutoScaleMode.None: the host never scales it twice).
 // One left click on a tile = one placement request; the plugin starts AutoCAD's point input with the symbol following the cursor. ESC in the palette
 // drops a request whose CAD command has not started. Thumbnail size never affects the insertion scale.
 public sealed class CompactView06:UserControl {
#if UI031
  // TVXD-MEPS-UI-031 (MEPS-TVBG, user-authorised preview): dark slate theme set explicitly on the view and every container (no inherited light strip).
  // Tiles keep CompactLibraryView.CTile because the cached thumbnails are flattened onto that colour; block icon colours are unchanged.
  internal static readonly Color UiBack=Color.FromArgb(0x17,0x21,0x2B),UiField=Color.FromArgb(0x20,0x2D,0x3A),UiLine=Color.FromArgb(0x35,0x46,0x55),UiFore=Color.FromArgb(0xE7,0xEE,0xF5),
   UiMuted=Color.FromArgb(0x8F,0xA0,0xB0),UiTeal=Color.FromArgb(0x46,0xC7,0xB7),UiTealHot=Color.FromArgb(0x6F,0xD8,0xCB),UiHover=Color.FromArgb(0x26,0x40,0x4A);
  static readonly Color CBack=UiBack,CField=UiField,CTile=CompactLibraryView.CTile,CLine=UiLine,CFore=UiFore,CMuted=UiMuted,CTeal=UiTeal,CTealHot=UiTealHot,CWarn=CompactLibraryView.CWarn;
  static readonly Color CArmed=CompactLibraryView05.CArmed,CHover=UiHover;
  public const int SearchHeight=22,SearchRadius=8,ArrowWidth=22,HeaderButtonGap=2,HeaderRadius=6;public const float GlyphPx=18f,GridIconPx=16f,ChevronPx=12f;
  public const string BrandTitle="MEPS-TVBG";
#else
  static readonly Color CBack=CompactLibraryView.CBack,CField=CompactLibraryView.CField,CTile=CompactLibraryView.CTile,CLine=CompactLibraryView.CLine,CFore=CompactLibraryView.CFore,CMuted=CompactLibraryView.CMuted,CTeal=CompactLibraryView.CTeal,CTealHot=CompactLibraryView.CTealHot,CWarn=CompactLibraryView.CWarn;
  static readonly Color CArmed=CompactLibraryView05.CArmed,CHover=CompactLibraryView05.CHover;
  public const int SearchHeight=28;
#endif
  public const int ClientWidth=208,ClientHeight=500,MinClientWidth=192,HeaderHeight=28,RatioHeight=28,RootPad=4,IconButton=26,RatioLabelWidth=80,Icon=16,TileHeight=32,TileMargin=2,Radius=6;
#if CABLES016
  // TVXD-MEPS-CABLES-016: no "Tỷ lệ ký hiệu" row and no ⚙ button in the palette (their space goes to the grid/lower half). Every placement is raw 1×
  // (p.ratio=false) except new E-NHE-RJ45/E-NHE-TEL placements (default scale = height of E-OC-1 at 1×, see Placement); a ratio stored in a drawing by an
  // earlier build is neither applied nor changed/deleted. Placed blocks can be scaled with SCALE.
  // The ratio controls/dialogs below are kept for the old build configuration only (created but never parented, wired or tab-reachable here).
  // NAMING_HELP016: the header ! button becomes ? (same slot/size) and opens the read-only block naming guide (ShowNamingHelp). The full message/warning
  // log moves to the right-click menu of the list ("Nhật ký / Cảnh báo") and a button of the guide; the title tooltip carries the latest message and the
  // warning count and the title turns the warning colour while warnings exist. The guide text describes the rules already in Library/Identity015/Standards012.
  public const int GridRow=2;
#else
  public const int GridRow=3;
#endif
  public const float TextPx=11f,TitlePx=12f,SymbolPx=13f;
  public const string Title="MEPS / Điện",RatioCaption="Tỷ lệ ký hiệu",AddTip="Thêm block từ file DWG/DXF (chỉ đọc nguồn; mục mới được chép và chuẩn hoá vào thư viện).",
   LegendTip="Tạo bảng ký hiệu: kiểm tra trước (liệt kê ký hiệu thiếu mô tả / không nhận dạng); khi đủ điều kiện bấm 'Đặt vào Model…' rồi chọn góc trên-trái — bảng (cột DIỄN GIẢI căn trái và giữa theo chiều đứng, các chữ khác căn giữa ô) được đặt thành MỘT khối trong Model hiện tại (UNDO để bỏ, không tạo file DWG riêng).",
   SettingsTip="Thiết lập tỷ lệ: nhập và xác nhận mẫu số gốc N0 và hệ số gốc trước khi dùng 1:N. MEPS không tự đặt N0, không tự quy đổi đơn vị.",
   MessagesTip="Thông báo và lỗi (đầy đủ nội dung)";
#if CABLES016
  public const string HelpCaption="?",HelpTip="Hướng dẫn đặt tên block",LogCaption="Nhật ký / Cảnh báo";
#if MODELLIB019
  // TVXD-MEPS-MODELLIB-019: '+' menu (action, caption) in display order; the guide below states the mandatory rule Naming019 applies to NEW entries.
  internal static readonly string[][] AddActions={new[]{"modeladd","Thêm block từ Model"},new[]{"import","Thêm từ DWG/DXF"},new[]{"modelcopy","Tạo bản sao để sửa"},new[]{"modelremove","Xoá khỏi công cụ (chọn trên Model)"}};
  internal readonly ContextMenuStrip addMenu=new ContextMenuStrip();
  public const string AddTip019="Thêm / sửa thư viện: thêm block từ Model, thêm từ DWG/DXF, tạo bản sao để sửa, xoá khỏi công cụ. Tên block mới phải đúng quy tắc (nút ?).";
  // TVXD-MEPS-LIBDELETE-021: right-click a tile → DeleteCaption → owned confirmation (ConfirmDeleteDialog021 through ShowModal) → LibraryDelete021.Delete →
  // reload. No Model prompt. Distinct from "Ẩn mục đang chọn" (hide flag, restorable).
  public const string DeleteCaption="Xóa khỏi công cụ...";
  internal const string NamingHelpText=
   "HƯỚNG DẪN ĐẶT TÊN VÀ THÊM BLOCK (MEPS 019)\r\n"+
   "Tên block quyết định nhóm, layer và mã loại. Tên mới sai quy tắc bị TỪ CHỐI: MEPS báo lý do, không tự đổi tên, không thêm gì vào thư viện.\r\n\r\n"+
   "1. QUY TẮC BẮT BUỘC CHO BLOCK MỚI\r\n"+
   "   E-<NHÓM>-<LOẠI>[-<BIẾN THỂ>…]\r\n"+
   "   • Chỉ chữ IN HOA A–Z, số 0–9 và dấu \"-\"; không dấu tiếng Việt, không khoảng trắng, không \"--\", không kết thúc bằng \"-\"; tối đa 120 ký tự.\r\n"+
   "   • NHÓM: DEN, CT, OC, NHE, DH, QUAT, TU, AT, X, SDNL, DAY.\r\n"+
   "   • Đúng: E-OC-USB-1, E-DEN-DL-18W, E-CT-3C-1.  Sai: e-oc-usb (chữ thường), E-OC USB (khoảng trắng), E-ỔCẮM-1 (có dấu), E-PCCC-1 (nhóm chưa có).\r\n"+
   "   • Tên đã có trong thư viện (kể cả mục đang ẩn và mục đã xoá khỏi công cụ) hoặc trùng 67 tên gốc bị từ chối; MEPS gợi ý tên bản sửa, ví dụ E-OC-1-R02. Bạn tự quyết định tên.\r\n"+
   "   • 67 ký hiệu gốc và các mục đã có giữ nguyên tên và mã; MEPS không đổi tên thư viện hay bản vẽ.\r\n\r\n"+
   "2. NHÓM → LAYER KHI ĐẶT\r\n"+
   "   E-DEN → E-DEN    E-CT → E-CT    E-OC → E-OC    E-NHE → E-NHE\r\n"+
   "   E-DH → E-DH      E-QUAT → E-QUAT    E-TU → E-TU\r\n"+
   "   E-AT → E-TB      E-X → E-TB     E-SDNL → E-SDNL-TB    E-DAY → E-DAY-CS\r\n\r\n"+
   "3. NÚT +\r\n"+
   "   • Thêm block từ Model: chọn đúng MỘT block trong Model, nhập tên và (tuỳ chọn) DIỄN GIẢI / QUY CÁCH / GHI CHÚ. MEPS chụp đúng hình đang thấy (tỉ lệ, góc xoay, thuộc tính, khối lồng) thành MỘT mục; khối lồng không thành mục riêng. Block gốc và bản vẽ không bị đổi tên, sửa hay lưu. Thuộc tính được chụp cố định theo giá trị lúc chụp.\r\n"+
   "   • Thêm từ DWG/DXF: chỉ đọc file nguồn như trước. Block gốc mới có tên sai hoặc đã dùng → không thêm mục nào. Khối phụ chỉ nằm lồng bên trong và có tên sai được bỏ qua, không nhập riêng.\r\n"+
   "   • Tạo bản sao để sửa: chọn một block, nhập tên mới chưa dùng (MEPS gợi ý -R02…), chọn điểm đặt. Bản sao có định nghĩa riêng, không dùng chung khối lồng với bản gốc; bản gốc giữ nguyên. Nhấp đúp bản sao (hoặc BEDIT) để sửa bằng lệnh AutoCAD, BCLOSE để lưu, rồi \"Thêm block từ Model\" và chọn bản sao. Khối động và thuộc tính nhiều dòng chưa tạo được bản sao (MEPS báo lý do).\r\n"+
   "   • Thêm block có tên bất kỳ: chèn (INSERT) block đó vào Model, rồi \"Thêm block từ Model\" và nhập tên chuẩn mới E-<NHÓM>-<LOẠI>. Block nguồn giữ tên cũ, không bị sửa. \"Thêm từ DWG/DXF\" vẫn đòi tên trong file đúng quy tắc. Xref, proxy và khối chứa chúng không thêm được (MEPS báo lý do).\r\n"+
   "   • Xoá khỏi công cụ (chọn trên Model): chọn một ký hiệu đã đặt từ bảng MEPS. MEPS nhận đúng mục theo dấu/mã ghi trên khối (không đoán theo tên), hỏi xác nhận rồi XOÁ mục khỏi công cụ như lệnh chuột phải bên dưới. Khối không mang dấu MEPS hoặc thuộc thư viện khác: từ chối, không đổi gì.\r\n"+
   "   • Xoá khỏi công cụ (chuột phải ký hiệu → Xóa khỏi công cụ...): không cần chọn trên Model. Mục không còn trong danh sách, tìm kiếm, nhóm, \"Hiện mục đã ẩn\" hay khôi phục. Khối đã đặt trong bản vẽ giữ nguyên tên/mã, bảng ký hiệu của bản vẽ đó vẫn nhận ra; file nguồn và file thư viện không bị xoá. Tên đã xoá không dùng lại cho mục mới (MEPS gợi ý -R02…). Muốn có lại đúng mục cũ: '+ → Thêm từ DWG/DXF' với đúng file nguồn cũ.\r\n"+
   "   • Ẩn (khác với xoá): chuột phải → Ẩn mục đang chọn; khôi phục: Hiện mục đã ẩn → chọn mục → Khôi phục mục đang chọn.\r\n\r\n"+
   "4. SỬA HÌNH = MỤC MỚI\r\n"+
   "   Hình đã sửa được thêm thành tên mới (bản sửa, ví dụ E-OC-1-R02). MEPS không ghi đè hình của mục cũ, không gộp ID/mã; ký hiệu đã đặt bằng mục cũ giữ nguyên. E-OC-1 và E-OC-1-R02 là hai mã loại khác nhau; MEPS không tự gộp chúng khi thống kê.\r\n\r\n"+
   "5. SAU KHI THÊM TỪ MODEL\r\n"+
   "   • Mã chuẩn: TVXD_<tên chuẩn hoá>_<ID 12 ký tự hex>; ID do MEPS tính từ bản chụp và tên, bạn không gõ ID.\r\n"+
   "   • Bảng ký hiệu: dòng của mục mới dùng đúng chữ bạn nhập, ô để trống thì để trống (MEPS không tự viết, không lấy chữ từ DXF gốc); bố cục cột theo bảng ký hiệu gốc có sẵn.\r\n"+
   "   • Nguồn của dòng này là bảng MEPS TỰ TẠO (MODEL_CAPTURE019, chữ do bạn nhập), không phải bảng đọc từ DWG/DXF, không phải parser cũ. \"Đọc lại bảng nguồn\" bị từ chối cho nguồn này (bản chụp không có bảng; đọc lại sẽ xoá chữ đã nhập). Muốn đổi chữ: thêm lại thành tên bản sửa mới.\r\n"+
   "   • Đơn vị: bảng dùng dấu bố cục INSUNITS của bảng mẫu; INSUNITS thật của bản vẽ chụp được ghi trong catalog/legend để truy vết, MEPS không quy đổi kích thước.\r\n"+
   "   • Thuộc tính đã chụp là giá trị cố định của mục: không sửa riêng từng lần đặt.\r\n"+
   "   • Block gốc vẫn ở Model và KHÔNG tự chuyển sang mục/mã mới (MEPS không tự xoá, không nhận nó làm mục mới). Block gốc là khối ngoài (chưa có dấu MEPS): vẫn không nhận dạng, bảng ký hiệu sẽ báo \"không nhận dạng\". Block gốc đã là ký hiệu MEPS: giữ mã/mục cũ như trước. Muốn dùng mục mới: đặt ký hiệu từ bảng MEPS thay cho block gốc trước khi tạo bảng ký hiệu.\r\n\r\n"+
   "6. BÓC KHỐI LƯỢNG: chưa có trong bản này.\r\n\r\n"+
   "7. NHẬT KÝ / CẢNH BÁO: chuột phải danh sách ký hiệu → Nhật ký / Cảnh báo, hoặc nút bên dưới.";
#else
  // Shown read-only by ShowNamingHelp. Only rules the code already applies are stated as rules; the rest is marked as a recommendation or a later step.
  internal const string NamingHelpText=
   "HƯỚNG DẪN ĐẶT TÊN BLOCK (MEPS 016)\r\n"+
   "Tên block là căn cứ để MEPS xếp nhóm, chọn layer và gán mã loại. Công cụ bóc khối lượng sau này sẽ dựa vào mã đó, vì vậy cần đặt tên đúng ngay khi tạo block.\r\n\r\n"+
   "1. CẤU TRÚC TÊN (tên bạn đặt cho block trong file DWG/DXF nguồn)\r\n"+
   "   E-<NHOM>-<LOAI>[-<BIEN_THE>]\r\n"+
   "   • Nên dùng chữ IN HOA, không dấu, không khoảng trắng; chỉ A–Z, 0–9 và dấu gạch ngang \"-\". Đây là khuyến nghị: MEPS không bắt buộc, nhưng khi tạo mã, mọi ký tự ngoài A–Z/0–9 (kể cả chữ có dấu, khoảng trắng) đều bị đổi thành \"_\", nên tên dễ trùng hoặc khó đọc.\r\n"+
   "   • MEPS chỉ từ chối tên rỗng, dài hơn 200 ký tự, có ký tự điều khiển hoặc có một trong các ký tự  < > / \\ \" : ; ? * | , = `\r\n"+
   "   • Nhóm = hai phần đầu của tên (E-CT-2C-1 thuộc nhóm E-CT). Tên phải có ít nhất 3 phần; tên chỉ có 2 phần (ví dụ E-DEN) không thuộc nhóm E-DEN.\r\n\r\n"+
   "2. NHÓM ĐANG CÓ — NHÓM QUYẾT ĐỊNH LAYER\r\n"+
   "   E-DEN → layer E-DEN      E-CT → E-CT      E-OC → E-OC\r\n"+
   "   E-NHE → E-NHE            E-DH → E-DH      E-QUAT → E-QUAT\r\n"+
   "   E-TU → E-TU              E-AT → E-TB      E-X → E-TB\r\n"+
   "   (Thư viện gốc còn có E-SDNL → E-SDNL-TB và E-DAY → E-DAY-CS.)\r\n"+
   "   Nhóm chưa có trong bảng ánh xạ: MEPS chặn, báo cần bổ sung ánh xạ, không tự đoán layer.\r\n\r\n"+
   "3. VÍ DỤ TÊN TRONG THƯ VIỆN GỐC\r\n"+
   "   E-DEN-DL-12W     nhóm E-DEN\r\n"+
   "   E-CT-1           công tắc một chiều\r\n"+
   "   E-CT-2C-1        công tắc hai chiều\r\n"+
   "   E-OC-1           ổ cắm điện\r\n"+
   "   E-NHE-RJ45       ổ mạng\r\n"+
   "   E-NHE-TEL        ổ điện thoại\r\n\r\n"+
   "4. QUY TẮC\r\n"+
   "   • Tên chỉ nói LOẠI thiết bị. Không đưa tầng, phòng, vị trí hay số lượng vào tên (không đặt E-OC-1-TANG2, E-DEN-DL-12W-P101).\r\n"+
   "   • Cùng một loại thì dùng đúng một tên ở mọi nơi. Khác loại hoặc khác quy cách thì dùng hậu tố khác (ví dụ E-OC-1 và E-OC-2).\r\n"+
   "   • Mô tả và quy cách để người đọc (công suất, vật liệu…) ghi ở phần diễn giải/quy cách, tách khỏi tên. MEPS không suy ra công suất hay vật liệu từ tên: \"12W\" trong tên chỉ là chữ.\r\n\r\n"+
   "5. HAI LOẠI TÊN\r\n"+
   "   a) Tên thư viện: tên block do bạn đặt, ví dụ E-CT-1.\r\n"+
   "   b) Mã chuẩn: MEPS tự gán và ghi lên khối khi đặt.\r\n"+
   "      • Thư viện gốc có sổ mã cố định: E-CT-1 → TVXD_E_CT_1C_01, E-OC-1 → TVXD_E_OC_1.\r\n"+
   "      • Block nhập thêm (ngoài sổ mã): TVXD_<tên đã chuẩn hoá>_<ID 12 ký tự hex>, ví dụ E-OC-USB → TVXD_E_OC_USB_1A2B3C4D5E6F (ID chỉ để minh hoạ).\r\n"+
   "      • Bạn KHÔNG gõ phần ID. MEPS tự thêm ID thực, tính từ mã băm file nguồn và tên block, để hai block khác nhau không trùng mã.\r\n"+
   "      • Nhập lại cùng file nguồn, cùng tên → cùng ID. File nguồn đã sửa → ID khác, cần rà lại ánh xạ. MEPS không tự gộp các mục cùng tên đến từ các nguồn khác nhau.\r\n"+
   "      • Đổi tên một block đã có không tự cập nhật mã hay các khối đã đặt.\r\n\r\n"+
   "6. BÓC KHỐI LƯỢNG (CHƯA CÓ TRONG BẢN NÀY)\r\n"+
   "   MEPS 016 chưa tự đếm hay bóc khối lượng. Dự kiến cho bước sau: thiết bị được đếm theo mã chuẩn ghi trên khối, không tính các ký hiệu nằm trong bảng ký hiệu; chiều dài cáp lấy từ hình học thực tế của tuyến, không nhân theo số lõi.\r\n\r\n"+
   "7. THÊM / ẨN BLOCK\r\n"+
   "   • Nút + : thêm block từ file DWG/DXF (chỉ đọc file nguồn).\r\n"+
   "   • Chưa có chức năng lấy block trực tiếp từ Model.\r\n"+
   "   • Ẩn: chuột phải → Ẩn mục đang chọn. Ẩn không xoá file; khôi phục bằng chuột phải → Hiện mục đã ẩn → Khôi phục mục đang chọn.\r\n\r\n"+
   "8. NHẬT KÝ / CẢNH BÁO\r\n"+
   "   Chuột phải trong danh sách ký hiệu → Nhật ký / Cảnh báo, hoặc nút bên dưới. Khi có cảnh báo, tiêu đề \"MEPS / Điện\" đổi màu; rê chuột lên tiêu đề để xem thông báo gần nhất.";
#endif
#endif
  public readonly float Dpi;
  public int S(int px){return Math.Max(1,(int)Math.Round(px*Dpi));}
  public static float SystemDpi(){try{using(var g=Graphics.FromHwnd(IntPtr.Zero))return g.DpiX/96f;}catch{return 1f;}}
  internal int RowPitch{get{return S(TileHeight)+2*S(TileMargin);}}
  internal int GridMin{get{return 2*RowPitch;}}
#if CABLES016
  internal int FixedHeight{get{return S(HeaderHeight)+S(SearchHeight);}}
#else
  internal int FixedHeight{get{return S(HeaderHeight)+S(SearchHeight)+S(RatioHeight);}}
#endif
  internal int MinContentHeight{get{return FixedHeight+2*S(RootPad)+GridMin;}}
  // The list gets half of what remains under the fixed rows (never less than two tile rows); the other half stays blank.
  public int GridHeightFor(int clientHeight){return Math.Max(GridMin,(clientHeight-FixedHeight-2*S(RootPad))/2);}
  internal readonly TableLayoutPanel root;internal readonly Label title,ratioLabel;
  internal readonly CompactLibraryView05.RoundButton add,legend,settings,messages,filter;
  internal readonly TextBox search;internal readonly ComboBox ratio;
  // Group filter: menu opened by the ▼ button; one checked item = current group (Library.All = no filter).
  internal readonly ContextMenuStrip groups=new ContextMenuStrip();string group=Library.All;
  internal string Group{get{return group;}}
#if UI031
  // TVXD-MEPS-UI-031: the ▾ segment opens HỆ THỐNG → Electric / Điện (click = all electric blocks, no CAD action) whose submenu lists Tất cả block điện
  // and every existing catalog group (label + code); Plumbing/Nước and HVAC are listed disabled (no library yet). groupItems = the selectable group items in
  // menu order (GroupNames/SelectGroup, QA). Codes stay the catalog's E-X groups; a code without a known label is shown as the code itself.
  public const string SystemHeading="HỆ THỐNG",ElectricCaption="Electric / Điện",AllElectricCaption="Tất cả block điện",FutureTip="Chưa có thư viện cho hệ thống này.";
  internal static readonly string[] FutureSystems={"Plumbing / Nước (chưa có thư viện)","HVAC (chưa có thư viện)"};
  internal static readonly Dictionary<string,string> GroupLabels=new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase){{"E-DEN","Chiếu sáng"},{"E-CT","Công tắc"},{"E-OC","Ổ cắm"},{"E-NHE","Điện nhẹ"},{"E-DH","Điều hòa"},{"E-QUAT","Quạt"},{"E-TU","Tủ điện"}};
  internal readonly List<ToolStripMenuItem> groupItems=new List<ToolStripMenuItem>();internal ToolStripMenuItem electricItem;
  internal static string GroupCaption(string g){string l;return g==Library.All?AllElectricCaption:GroupLabels.TryGetValue(g,out l)?l+" ("+g+")":g;}
  internal string[] GroupNames{get{return groupItems.Select(i=>(string)i.Tag).ToArray();}}
  // PATCH1 F031-02: dark slate renderer for the group menu and the Electric / Điện submenu only (other menus/dialogs keep the system look).
  // Text CFore (disabled/heading CMuted), hover CHover with teal border, separators CLine, image margin = menu background, light check mark drawn here.
  internal static readonly ToolStripProfessionalRenderer MenuRenderer=new DarkMenuRenderer();
  sealed class DarkMenuColors:ProfessionalColorTable {
   public DarkMenuColors(){UseSystemColors=false;}
   public override Color ToolStripDropDownBackground{get{return CBack;}}public override Color MenuBorder{get{return CLine;}}
   public override Color ImageMarginGradientBegin{get{return CBack;}}public override Color ImageMarginGradientMiddle{get{return CBack;}}public override Color ImageMarginGradientEnd{get{return CBack;}}
   public override Color MenuItemSelected{get{return CHover;}}public override Color MenuItemBorder{get{return CTeal;}}
   public override Color MenuItemSelectedGradientBegin{get{return CHover;}}public override Color MenuItemSelectedGradientEnd{get{return CHover;}}
   public override Color MenuItemPressedGradientBegin{get{return CHover;}}public override Color MenuItemPressedGradientMiddle{get{return CHover;}}public override Color MenuItemPressedGradientEnd{get{return CHover;}}
   public override Color SeparatorDark{get{return CLine;}}public override Color SeparatorLight{get{return CBack;}}
   public override Color CheckBackground{get{return CField;}}public override Color CheckSelectedBackground{get{return CHover;}}public override Color CheckPressedBackground{get{return CHover;}}
   public override Color ButtonSelectedBorder{get{return CTeal;}}
  }
  sealed class DarkMenuRenderer:ToolStripProfessionalRenderer {
   public DarkMenuRenderer():base(new DarkMenuColors()){RoundedEdges=false;}
   protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e){e.TextColor=!e.Item.Enabled||e.Item is ToolStripLabel?CMuted:CFore;base.OnRenderItemText(e);}
   protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e){e.ArrowColor=e.Item!=null&&!e.Item.Enabled?CMuted:CFore;base.OnRenderArrow(e);}
   protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e){var r=e.ImageRectangle;if(r.Width<=0||r.Height<=0)return;var g=e.Graphics;
    using(var b=new SolidBrush(e.Item.Selected?CHover:CField))g.FillRectangle(b,r);using(var p=new Pen(CLine))g.DrawRectangle(p,r.X,r.Y,r.Width-1,r.Height-1);
    var m=g.SmoothingMode;g.SmoothingMode=SmoothingMode.AntiAlias;float x=r.X,y=r.Y,w=r.Width,h=r.Height;
    using(var p=new Pen(e.Item.Enabled?CTealHot:CMuted,Math.Max(1.5f,w/8f)){StartCap=LineCap.Round,EndCap=LineCap.Round,LineJoin=LineJoin.Round})
     g.DrawLines(p,new[]{new PointF(x+w*.26f,y+h*.52f),new PointF(x+w*.43f,y+h*.70f),new PointF(x+w*.75f,y+h*.32f)});
    g.SmoothingMode=m;}
  }
#else
  internal string[] GroupNames{get{return groups.Items.OfType<ToolStripMenuItem>().Select(i=>(string)i.Tag).ToArray();}}
#endif
  internal readonly FlowLayoutPanel grid;
  // Lower half: intentionally empty.
  internal readonly System.Windows.Forms.Panel blank;
#if FLEX022
  // TVXD-MEPS-FLEXDUCT-022 integration: one small rounded button in the lower area; the rest of the lower area stays blank. Click = RequestFlex (plugin:
  // FlexDuct022.Queue → one prompt: pick a LINE/PLINE/ARC/SPLINE centerline to create, or an owned curved duct to update, then diameter). No presets, no
  // diameter field in the palette.
  // TVXD-MEPS-FLEXUI-029: the wire chooser and this button share ONE row (flexHost): wire left, flex right, each exactly one tile wide at the tile columns'
  // x positions (TileWidth/TileMargin, same logic as SizeTiles), height FlexBarHeight. Without a wire view the flex button takes the left slot.
  // F029-UI-01: both action captions ("Dây dẫn ▾", "Ống gió mềm") use ActionFont = the largest Segoe UI size (TextPx down in 0.5 px steps, not below
  // ActionMinPx) in which each caption + ActionPad px fits the button width at MinClientWidth for this Dpi. Tile/common fonts and panel width unchanged.
#if UI031
  public const string FlexCaption="ỐNG GIÓ MỀM";
#else
  public const string FlexCaption="Ống gió mềm";
#endif
  public const string FlexTip="Ống gió mềm: bấm rồi chọn đường tâm có sẵn trên Model — LINE, PLINE, ARC hoặc SPLINE — và nhập đường kính theo đơn vị bản vẽ hiện hành (MEPS không quy đổi đơn vị). Thân nét 0.05 mm; đường tâm gốc giữ nguyên hình và handle, thành nét tâm mờ dùng tính chiều dài."
   +"\nLINE: chọn thân ống, kéo grip đầu mút để dài/ngắn theo phương tuyến."
   +"\nPLINE/ARC/SPLINE: sửa đường tâm bằng grip của nó, rồi bấm lại nút này và chọn ống (thân hoặc đường tâm) để CẬP NHẬT; trước khi cập nhật, ống không được cộng khối lượng. Esc huỷ.";
  public const int FlexBarHeight=26,FlexGap=4,ActionPad=8;public const float ActionMinPx=7f;
#if UI031
  // 031: DÀN TRANG shares the action font, so its caption is part of the fit (one font for all three action tiles).
  internal static readonly string[] ActionCaptions={WireView010.CompactBarText,FlexCaption,LayoutCaption};
#else
  internal static readonly string[] ActionCaptions={WireView010.CompactBarText,FlexCaption};
#endif
  internal readonly System.Windows.Forms.Panel flexHost;internal readonly CompactLibraryView05.RoundButton flexBar;internal readonly Font ActionFont;
  public Action RequestFlex;
#endif
#if LAYOUT030
  // TVXD-MEPS-LAYOUT-030 (user-approved compact dropdown): ONE rounded row "Dàn trang ▾" (LayoutBarHeight, width of both tile columns) under the wire/flex
  // row; the rest of the lower area stays blank. Click = OpenLayoutMenu only (popup over the drawing, no action on opening). An item closes every palette menu,
  // then RequestLayout(command): the plugin queues Layout030 (native load of Layout030.lsp, then TVXDLAYOUT030DO). Regular TextFont, existing button colours.
#if UI031
  // 031: one tile wide and FlexBarHeight high, left slot of the row under DÂY DẪN / ỐNG GIÓ MỀM (right slot empty), action font, uppercase caption.
  public const string LayoutCaption="DÀN TRANG";public const int LayoutBarHeight=FlexBarHeight;
#else
  public const string LayoutCaption="Dàn trang ▾";public const int LayoutBarHeight=24;
#endif
  public const string LayoutHelpCaption="Hướng dẫn ngắn",
   LayoutTip="Dàn trang: bấm để mở menu 5 lệnh T- (Layout030.lsp). T-CK, T-DTL, T-DTM, T-TKI là lệnh DAN_TRANG gốc chỉ đổi tên; T-STT đánh lại số tờ. Số tờ trong khung dùng font Arial. Lần đầu trong mỗi bản vẽ MEPS nạp Layout030.lsp cạnh DLL.";
  // 035: (command, caption) in menu order; the caption starts with the exact command it sends. All directly in one menu, then separator, help.
  // 037: T-DTV / T-DTA removed by the user; five commands.
  internal static readonly string[][] LayoutActions={new[]{"T-CK","T-CK · Đặt khung"},new[]{"T-STT","T-STT · Đánh số"},new[]{"T-DTL","T-DTL · Tạo layout"},new[]{"T-DTM","T-DTM · Dàn nhiều tờ"},
   new[]{"T-TKI","T-TKI · Tạo khung"}};
  internal readonly ContextMenuStrip layoutMenu=new ContextMenuStrip();internal readonly Dictionary<string,ToolStripMenuItem> layoutItems=new Dictionary<string,ToolStripMenuItem>();internal ToolStripMenuItem layoutHelpItem;
  internal readonly System.Windows.Forms.Panel layoutHost;internal readonly CompactLibraryView05.RoundButton layoutBar;internal Form layoutHelpDialog;internal TextBox layoutHelpText;
  public Action<string> RequestLayout;
  internal const string LayoutHelpText=
   "DÀN TRANG (Layout030.lsp, 5 lệnh T-)\r\n\r\n"+
   "T-CK, T-DTL, T-DTM, T-TKI là lệnh gốc CK, DTL, DTM, TKI của DAN_TRANG, chỉ đổi tên; cách làm, câu hỏi và thông báo giữ nguyên bản gốc. T-STT là lệnh đánh số riêng. "+
   "Số tờ (SO_TO) trong khung mới đặt dùng kiểu chữ Arial riêng của MEPS; tên tờ, tỷ lệ và các kiểu chữ khác giữ nguyên.\r\n\r\n"+
   "• T-CK · Đặt khung: đặt khung vùng in trên Model. Hộp thoại chọn tỷ lệ (dòng có (m) cho bản vẽ hệ mét) và điểm bắt; bấm liên tục, số tờ tự tăng; Enter kết thúc. Khi đặt: T đổi tỷ lệ, D đổi điểm bắt, G góc nghiêng, N bật/tắt hỏi tên tờ. Thiếu block khung thì tự tạo. Nếu ô khung tên đo được khác bộ block đang có và đã có khung đặt, lệnh hỏi dựng lại bộ block (Có = XOÁ các khung đã đặt).\r\n"+
   "• T-STT · Đánh số: sau khi chép khung bằng COPY (số tờ giữ nguyên), quét chọn các khung, chọn MỘT hướng TraiPhai (trái→phải, mặc định) / PhaiTrai (phải→trái) / TrenDuoi (trên→dưới) / DuoiTren (dưới→trên), nhập số bắt đầu (mặc định 1). MEPS kiểm tra rồi TỰ ÁP DỤNG: bước 1, số 1, 2, 3… (không thêm số 0). Esc trước khi nhập xong số bắt đầu: bản vẽ không đổi. Khung không chọn không bị đổi; trùng số với khung không chọn thì từ chối, không ghi gì. Tên layout không tự đổi. U để hoàn tác.\r\n"+
   "• T-DTL · Tạo layout: mỗi khung thành một layout tên \"<số tờ> - <tên tờ>\", sao chép từ layout mẫu (ưu tiên layout MAU, rồi layout mẫu lần trước, không có thì hỏi chọn). Layout mẫu có một viewport bằng ô vẽ của khung tên. Layout trùng tên: Boqua (mặc định, bỏ qua) hoặc Taolai (xoá layout trùng tên rồi tạo lại).\r\n"+
   "• T-DTM · Dàn nhiều tờ: chạy trên layout ĐANG MỞ (có sẵn một khung tên xref và một viewport), dàn tất cả các tờ lên CHÍNH layout đó, mỗi tờ một khung tên và một viewport; chọn hướng xếp và số khung mỗi hàng. Chạy lại trên cùng layout thì dọn các bộ của lần trước, chỉ giữ một bộ rồi dàn lại. Không tạo layout mới.\r\n"+
   "• T-TKI · Tạo khung: tạo bộ block khung vùng in. Hỏi tạo lại cả block đã có: Không (mặc định) chỉ tạo block còn thiếu; Có tạo lại tất cả và XOÁ các khung đã đặt dùng các block đó.\r\n\r\n"+
   "• Kết quả mỗi lệnh in trên dòng lệnh như bản gốc (ví dụ \">> DA TAO … layout\", \">> Da huy.\"). Bốn lệnh gốc không có cơ chế hoàn tác riêng của MEPS: kiểm tra kết quả, cần thì dùng U/UNDO của AutoCAD. T-STT tự kiểm tra trước và in THANH CONG / DA HUY / THAT BAI.\r\n"+
   "• Sau lần dùng menu đầu tiên trong một bản vẽ, có thể gõ thẳng T-CK, T-DTL… trên dòng lệnh của bản vẽ đó. Bản MEPS không định nghĩa CK/DTL/DTM/TKI và không còn lệnh T-DTV/T-DTA.";
#endif
#if TAG033
  // TVXD-MEPS-TAG-033: ONE button "TAG" in the right slot of the DÀN TRANG row (one tile wide, FlexBarHeight high, action font; no new row). Click =
  // TagClicked → Request(TagUi033.Edit) → the plugin's queued bound-document job (selection prompt, owned dialog, TagCad033.Apply). No menu.
  internal readonly CompactLibraryView05.RoundButton tagBar;
  public const string TagTip="TAG: gắn / sửa nhãn chữ hiển thị cạnh block (công tắc, đèn, quạt, CB…). Bấm rồi chọn block trên Model, Enter; nhập chữ và chiều cao (đơn vị bản vẽ). "+
   "Nhãn đi theo block khi MOVE/COPY; kéo grip của nhãn để dời riêng. Một bước U. Không phải quan hệ lộ/mạch.";
  internal const string TagHelpText="\r\n\r\n10. TAG — NHÃN CHỮ HIỂN THỊ CẠNH BLOCK\r\n"+
   "   • Bấm TAG (cạnh DÀN TRANG) → chọn một hay nhiều block trên Model → Enter → nhập chữ (ví dụ 1,2 / AC-01 / Q1) và chiều cao chữ theo ĐƠN VỊ BẢN VẼ → Lưu. Hủy/Esc: không đổi gì.\r\n"+
   "   • Chọn block đã có nhãn: hộp hiện chữ/cỡ hiện có. Chọn nhiều: để trống chiều cao = giữ cỡ từng nhãn; chỉ khi tích 'Đổi tên cho tất cả' mới đổi chữ.\r\n"+
   "   • Nhãn là thuộc tính của block: MOVE/COPY mang theo; kéo grip của nhãn để dời riêng; sửa được bằng Properties. Định nghĩa block và thư viện không đổi.\r\n"+
   "   • Mỗi lần Lưu là một bước U/UNDO; MEPS không tự lưu DWG. ATTSYNC/BATTMAN Sync hoặc EXPLODE có thể làm mất nhãn. Block động/chú thích/xref/khoá layer bị từ chối, nêu rõ lý do.";
#endif
#if LIBUPDATE023
  // TVXD-MEPS-LIBUPDATE-023: right-click a tile → UpdateCaption → the plugin queues the bound-document job with THAT entry (RequestUpdate), which prompts for
  // the edited Model block and confirms before LibraryUpdate023.Update. No library write from the palette itself.
  public const string UpdateCaption="Cập nhật hình từ Model…";
  public Action<LibEntry> RequestUpdate;
  internal const string UpdateHelpText="\r\n\r\n8. CẬP NHẬT HÌNH CỦA MỤC CÓ SẴN (giữ tên/mã/bảng ký hiệu)\r\n"+
   "   • Sửa hình: nhấp đúp khối trên Model (hoặc lệnh BEDIT), sửa, BCLOSE → Lưu. Việc này chỉ đổi bản vẽ hiện tại.\r\n"+
   "   • Muốn các bản vẽ SAU dùng hình mới: chuột phải đúng ký hiệu trong danh sách → "+UpdateCaption+" → chọn khối đã sửa trên Model → đọc kỹ hộp xác nhận → Có.\r\n"+
   "   • MEPS giữ nguyên ID, tên, mã loại, nhóm và dòng bảng ký hiệu; chỉ thay hình của mục. Bản sao hình cũ và tệp tạm được xoá sau khi cập nhật kiểm tra thành công (nếu lỗi giữa chừng, MEPS khôi phục hình cũ). Ký hiệu đã đặt trong các bản vẽ cũ giữ hình cũ (không sửa hàng loạt); lần đặt mới dùng hình mới.\r\n"+
   "   • Hình được chụp đúng như đang thấy (tỉ lệ, góc xoay, thuộc tính của khối đã chọn; gốc = điểm chèn). Đơn vị của mục lấy theo đơn vị bản vẽ hiện tại (bản vẽ chưa đặt đơn vị bị từ chối); MEPS KHÔNG tự quy đổi tỉ lệ.\r\n"+
   "   • Cập nhật thư viện là ghi file, UNDO của CAD KHÔNG hoàn tác được và hình cũ không được giữ lại; đối tượng đã đặt trên Model không bị ảnh hưởng.";
#endif
#if METADATA032
  // TVXD-MEPS-METADATA-032: right-click a tile → MetadataCaption → MetadataDialog032 (owned, through ShowModal) → LegendMetadata032.Save. Only metadata032.json
  // in the library root is written; catalog, legend.json, block files and drawings are not.
  public const string MetadataCaption="Sửa thông tin bảng ký hiệu…";
  internal const string MetadataHelpText="\r\n\r\n9. SỬA DIỄN GIẢI / QUY CÁCH / GHI CHÚ CỦA MỤC CÓ SẴN (không chụp lại, không đổi tên)\r\n"+
   "   • Chuột phải đúng ký hiệu trong danh sách → "+MetadataCaption+" Hộp thoại hiện chữ đang dùng khi tạo bảng; tên khối chỉ đọc.\r\n"+
   "   • Gõ tiếng Việt, xuống dòng được; ô để trống = để trống có chủ ý. Tối đa 500 ký tự mỗi ô, không ký tự điều khiển. Lưu → ghi; Hủy/Esc → không ghi gì.\r\n"+
   "   • Chỉ bảng ký hiệu TẠO MỚI từ thư viện này dùng chữ mới; bảng đã đặt trong bản vẽ không đổi. Lưu là ghi file thư viện (metadata032.json): UNDO của CAD không hoàn tác.\r\n"+
   "   • 'Dùng lại nội dung gốc' rồi Lưu: bỏ phần sửa tay của riêng ký hiệu này, lấy lại chữ của bảng nguồn. Đọc lại bảng nguồn hay cập nhật hình không xoá phần đã sửa.\r\n"+
   "   • Thêm block: nút + như trước. Bớt block: chuột phải → "+DeleteCaption+" (phần đã sửa vẫn giữ cho ký hiệu đã đặt).";
#endif
  internal readonly ContextMenuStrip options=new ContextMenuStrip();internal readonly ToolStripMenuItem showHidden=new ToolStripMenuItem("Hiện mục đã ẩn"){CheckOnClick=true};
  internal readonly Dictionary<string,ToolStripMenuItem> items=new Dictionary<string,ToolStripMenuItem>();
  internal readonly ToolTip tips=new ToolTip{ShowAlways=true};
  internal readonly List<Tile> tiles=new List<Tile>();
  internal readonly Font TextFont,TitleFont,SymbolFont;
  readonly Dictionary<string,Bitmap> cache=new Dictionary<string,Bitmap>();
  internal int Renders;
  public string Root;public LibCatalog Catalog;public Action<string,LibPlacement> Request;
  // Shows dialogs modally; the plugin replaces it with AutoCAD's Application.ShowModalDialog.
  public Func<Form,DialogResult> ShowModal=f=>f.ShowDialog();
  string armed,armedName;bool filling,fitting;
  internal string Armed{get{return armed;}}
  internal LibEntry Selected{get{var t=tiles.FirstOrDefault(x=>x.Selected&&x.Listed);return t==null?null:t.Entry;}}
  internal string[] Shown{get{return tiles.Where(t=>t.Listed).Select(t=>t.Entry.name).ToArray();}}
  // Messages: every message and warning is kept in full (last 200) and shown by the ! button; the last one is also the title tooltip.
  internal readonly List<string> log=new List<string>();int warnings;internal string Last;internal bool LastWarning;
  internal int Warnings{get{return warnings;}}
  // Ratio state: setup per drawing (in this session, or read from the drawing's stored N0/N/base), current choice (null = raw 1×), custom N.
  RatioSetup setup;double? choice,custom;string drawingKey,drawingName,drawingUnits;int lastIndex;readonly Dictionary<string,RatioSetup> setups=new Dictionary<string,RatioSetup>(StringComparer.OrdinalIgnoreCase);
  internal RatioSetup Setup{get{return setup;}}internal double? Choice{get{return choice;}}internal bool Confirmed{get{return setup!=null&&setup.confirmed;}}
#if CABLES016
  // Naming guide (reused, disposed with the view) and the log entry of the right-click menu. The header button keeps its field name "messages".
  internal Form helpDialog;internal TextBox helpText;internal readonly ToolStripMenuItem logItem=new ToolStripMenuItem(LogCaption);
  internal CompactLibraryView05.RoundButton Help{get{return messages;}}
  // Latest message + warning count (title tooltip).
  internal string StatusText{get{return (Last??"")+"\n"+LogCaption+(warnings>0?": "+warnings+" cảnh báo":"")+" — chuột phải danh sách ký hiệu hoặc nút "+HelpCaption+" để xem đầy đủ.";}}
#endif
  internal Form setupDialog,customDialog,messagesDialog;internal TextBox setupN0,setupBase,customBox,messagesText;internal CheckBox setupConfirm;internal CompactLibraryView05.RoundButton setupOk,customOk;internal Label setupUnits,setupFormula;
  public CompactView06():this(SystemDpi()){}
  public CompactView06(float dpi){Dpi=float.IsNaN(dpi)||dpi<=0?1f:Math.Max(1f,Math.Min(3f,dpi));AutoScaleMode=AutoScaleMode.None;Dock=DockStyle.Fill;BackColor=CBack;ForeColor=CFore;
   TextFont=new Font("Segoe UI",TextPx*Dpi,FontStyle.Regular,GraphicsUnit.Pixel);TitleFont=new Font("Segoe UI",TitlePx*Dpi,FontStyle.Bold,GraphicsUnit.Pixel);SymbolFont=new Font("Segoe UI Symbol",SymbolPx*Dpi,FontStyle.Regular,GraphicsUnit.Pixel);Font=TextFont;
#if FLEX022
   ActionFont=FitActionFont();
#endif
#if CABLES016
   int[] rows={S(HeaderHeight),S(SearchHeight),GridMin,-1};
#else
   int[] rows={S(HeaderHeight),S(SearchHeight),S(RatioHeight),GridMin,-1};
#endif
   root=new TableLayoutPanel{ColumnCount=1,RowCount=rows.Length,Margin=new Padding(0),Padding=new Padding(S(RootPad))};
   // One percent column: without it the column auto-sizes to the rows' preferred width and never shrinks below it (header overflow at 192 px).
   root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
   foreach(int h in rows)root.RowStyles.Add(h<0?new RowStyle(SizeType.Percent,100):new RowStyle(SizeType.Absolute,h));Controls.Add(root);
   title=new Label{Text=Title,Dock=DockStyle.Fill,AutoEllipsis=true,TextAlign=ContentAlignment.MiddleLeft,Font=TitleFont,Margin=new Padding(0),ForeColor=CFore};
#if UI031
   root.BackColor=CBack;title.Text=BrandTitle;title.BackColor=CBack;title.AccessibleName=BrandTitle;
#endif
#if CABLES016
   add=Glyph("+","Thêm block");legend=Glyph("▦","Tạo bảng ký hiệu");settings=Glyph("⚙","Thiết lập tỷ lệ");messages=Glyph(HelpCaption,HelpTip);
#if UI031
   // 031: three 26 px square header buttons on one baseline (gap HeaderButtonGap): + and ? as bold ink-centred glyph paths, legend as a vector grid.
   foreach(var b in new[]{add,legend,messages}){b.Margin=new Padding(S(HeaderButtonGap),S(1),0,S(1));b.CornerRadius=S(HeaderRadius);}
   add.Icon=messages.Icon=CompactLibraryView05.RoundButton.IconKind.Glyph;add.IconPx=messages.IconPx=GlyphPx*Dpi;legend.Icon=CompactLibraryView05.RoundButton.IconKind.Grid;legend.IconPx=GridIconPx*Dpi;
#endif
#else
   add=Glyph("+","Thêm block");legend=Glyph("▦","Tạo bảng ký hiệu");settings=Glyph("⚙","Thiết lập tỷ lệ");messages=Glyph("!","Thông báo và lỗi");
#endif
#if CABLES016
   // Header: title + ! only after + and ▦ (no ⚙ slot). settings stays unparented, disabled and out of the tab order.
   settings.Enabled=false;settings.Visible=false;settings.TabStop=false;
#if UI031
   int hb=S(IconButton)+S(HeaderButtonGap);var head=Row(new[]{-1,hb,hb,hb});
#else
   var head=Row(new[]{-1,S(IconButton),S(IconButton),S(IconButton)});
#endif
   head.Controls.Add(title,0,0);head.Controls.Add(add,1,0);head.Controls.Add(legend,2,0);head.Controls.Add(messages,3,0);root.Controls.Add(head,0,0);
#else
   var head=Row(new[]{-1,S(IconButton),S(IconButton),S(IconButton),S(IconButton)});head.Controls.Add(title,0,0);head.Controls.Add(add,1,0);head.Controls.Add(legend,2,0);head.Controls.Add(settings,3,0);head.Controls.Add(messages,4,0);root.Controls.Add(head,0,0);
#endif
   search=new TextBox{AccessibleName="Tìm ký hiệu",Font=TextFont,BackColor=CField,ForeColor=CFore};
   filter=Glyph("▼","Nhóm");groups.Font=TextFont;
#if UI031
   // 031: one 22 px row: rounded search field (TextBox vertically centred by padding, never shorter than its native height) + 22 px rounded ▾ segment.
   filter.Text="";filter.AccessibleRole=AccessibleRole.ButtonDropDown;filter.Margin=new Padding(S(HeaderButtonGap),0,0,0);filter.CornerRadius=S(SearchRadius);
   filter.Icon=CompactLibraryView05.RoundButton.IconKind.Chevron;filter.IconPx=ChevronPx*Dpi;groups.Renderer=MenuRenderer;groups.BackColor=CBack;groups.ForeColor=CFore;
   var sf=Field(search);sf.Margin=new Padding(0);sf.CornerRadius=S(SearchRadius);sf.Border=CLine;sf.FocusBorder=CTeal;sf.BackColor=CBack;
   sf.Padding=new Padding(S(7),Math.Max(0,(S(SearchHeight)-search.PreferredHeight)/2),S(4),0);
   var find=Row(new[]{-1,S(ArrowWidth)+S(HeaderButtonGap)});find.Controls.Add(sf,0,0);find.Controls.Add(filter,1,0);root.Controls.Add(find,0,1);
#else
   var find=Row(new[]{-1,S(IconButton)});find.Controls.Add(Field(search),0,0);find.Controls.Add(filter,1,0);root.Controls.Add(find,0,1);
#endif
   ratioLabel=new Label{Text=RatioCaption,Dock=DockStyle.Fill,TextAlign=ContentAlignment.MiddleLeft,AutoEllipsis=true,Margin=new Padding(0),ForeColor=CMuted,Font=TextFont};
   ratio=new ComboBox{DropDownStyle=ComboBoxStyle.DropDownList,AccessibleName=RatioCaption,Font=TextFont,BackColor=CField,ForeColor=CFore,FlatStyle=FlatStyle.Flat,DropDownWidth=S(170)};
#if CABLES016
   ratio.Enabled=false;ratio.Visible=false;ratio.TabStop=false;ratioLabel.Visible=false;
#else
   var scaleRow=Row(new[]{S(RatioLabelWidth),-1});scaleRow.Controls.Add(ratioLabel,0,0);scaleRow.Controls.Add(Field(ratio),1,0);root.Controls.Add(scaleRow,0,2);
#endif
   grid=new FlowLayoutPanel{Dock=DockStyle.Fill,AutoScroll=true,WrapContents=true,FlowDirection=FlowDirection.LeftToRight,Margin=new Padding(0,S(2),0,0),Padding=new Padding(0),BackColor=CBack};root.Controls.Add(grid,0,GridRow);
   blank=new System.Windows.Forms.Panel{Dock=DockStyle.Fill,Margin=new Padding(0),Name="blank",BackColor=CBack};root.Controls.Add(blank,0,GridRow+1);
#if FLEX022
   flexHost=new System.Windows.Forms.Panel{Dock=DockStyle.Top,Height=S(FlexBarHeight)+S(FlexGap),Padding=new Padding(0,S(FlexGap),0,0),Margin=new Padding(0),Name="flex",BackColor=CBack};
   flexBar=new CompactLibraryView05.RoundButton{Text=FlexCaption,AccessibleName=FlexCaption,AccessibleRole=AccessibleRole.PushButton,Dock=DockStyle.None,Margin=new Padding(0),Font=ActionFont};
   flexBar.Colors(CField,CLine,CHover,CFore);flexBar.Click+=(s,e)=>FlexClicked();flexHost.Controls.Add(flexBar);blank.Controls.Add(flexHost);tips.SetToolTip(flexBar,FlexTip);
   // 029: a wire view added to the lower area later (WirePlugin010.Attach) joins the row; the row follows the tile columns on every resize.
   blank.ControlAdded+=(s,e)=>{if(e.Control is WireView010)FlexBelow();};flexHost.SizeChanged+=(s,e)=>ArrangeActions();grid.SizeChanged+=(s,e)=>ArrangeActions();
#endif
#if LAYOUT030
   // 030: docked Top under flexHost (in front of it in z-order, so it is docked after it; FlexBelow keeps that order).
   layoutHost=new System.Windows.Forms.Panel{Dock=DockStyle.Top,Height=S(LayoutBarHeight)+S(FlexGap),Padding=new Padding(0,S(FlexGap),0,0),Margin=new Padding(0),Name="layout",BackColor=CBack};
   layoutBar=new CompactLibraryView05.RoundButton{Text=LayoutCaption,AccessibleName="Dàn trang",AccessibleRole=AccessibleRole.ButtonDropDown,Dock=DockStyle.None,Margin=new Padding(0),Font=TextFont};
   layoutBar.Colors(CField,CLine,CHover,CFore);layoutBar.Click+=(s,e)=>OpenLayoutMenu();layoutHost.Controls.Add(layoutBar);blank.Controls.Add(layoutHost);layoutHost.BringToFront();tips.SetToolTip(layoutBar,LayoutTip);
   layoutHost.SizeChanged+=(s,e)=>ArrangeActions();BuildLayoutMenu();
#if UI031
   layoutBar.Font=ActionFont;
#endif
#if TAG033
   // 033: TAG in the empty right slot of the same row (ArrangeActions places it); same colours/font/size as DÀN TRANG.
   tagBar=new CompactLibraryView05.RoundButton{Text=TagUi033.Caption,Name="tag033",AccessibleName="TAG — nhãn hiển thị",AccessibleRole=AccessibleRole.PushButton,Dock=DockStyle.None,Margin=new Padding(0),Font=ActionFont};
   tagBar.Colors(CField,CLine,CHover,CFore);tagBar.Click+=(s,e)=>TagClicked();layoutHost.Controls.Add(tagBar);tips.SetToolTip(tagBar,TagTip);
#endif
#endif
   foreach(var m in new[]{new[]{"hide","Ẩn mục đang chọn"},new[]{"restore","Khôi phục mục đang chọn"},new[]{"reload","Tải lại thư viện"}}){string a=m[0];var item=new ToolStripMenuItem(m[1]);item.Click+=(s,e)=>Run(a);items[a]=item;}
   options.Items.Add(items["hide"]);options.Items.Add(items["restore"]);options.Items.Add(showHidden);options.Items.Add(new ToolStripSeparator());options.Items.Add(items["reload"]);
   showHidden.CheckedChanged+=(s,e)=>{Categories();Filter();};
#if CABLES016
   options.Items.Add(new ToolStripSeparator());options.Items.Add(logItem);logItem.Click+=(s,e)=>ShowMessages();
   add.Click+=(s,e)=>AddClicked();legend.Click+=(s,e)=>Run("legend");messages.Click+=(s,e)=>ShowNamingHelp();filter.Click+=(s,e)=>OpenGroups();
   search.TextChanged+=(s,e)=>Filter();grid.SizeChanged+=(s,e)=>SizeTiles();
   // The same menu on the empty part of the list and the blank lower half (a tile opens it itself, see TileUp).
   // 021: opened outside a tile = no delete target (the delete item is disabled and Act refuses).
   foreach(var c in new Control[]{grid,blank})c.MouseUp+=(s,e)=>{if(e.Button==MouseButtons.Right)ShowOptions((Control)s,e.Location,null);};
#else
   add.Click+=(s,e)=>AddClicked();legend.Click+=(s,e)=>Run("legend");settings.Click+=(s,e)=>OpenSetup();messages.Click+=(s,e)=>ShowMessages();filter.Click+=(s,e)=>OpenGroups();
   search.TextChanged+=(s,e)=>Filter();ratio.SelectedIndexChanged+=(s,e)=>RatioChanged();grid.SizeChanged+=(s,e)=>SizeTiles();
#endif
   search.KeyDown+=(s,e)=>{if(e.KeyCode==Keys.Down||e.KeyCode==Keys.Enter){FocusTiles();e.SuppressKeyPress=true;}};
#if UI031
   search.HandleCreated+=(s,e)=>Cue(search,ScopeCue);
#else
   search.HandleCreated+=(s,e)=>Cue(search,"Tìm ký hiệu…");
#endif
#if CABLES016
   tips.SetToolTip(add,AddTip);tips.SetToolTip(legend,LegendTip);tips.SetToolTip(settings,SettingsTip);tips.SetToolTip(messages,HelpTip);tips.SetToolTip(search,"Tìm theo tên khối, nhóm hoặc file nguồn");
#if MODELLIB019 && METADATA032
   // F032-03: the right-click metadata item is listed in the grid tooltip.
   tips.SetToolTip(grid,"Bấm chuột trái một ký hiệu: hình đi theo con trỏ, chọn điểm trên Model; ESC để huỷ. Chuột phải một ký hiệu: sửa thông tin bảng ký hiệu, xoá khỏi công cụ, ẩn/khôi phục, "+LogCaption+".");
#elif MODELLIB019
   tips.SetToolTip(grid,"Bấm chuột trái một ký hiệu: hình đi theo con trỏ, chọn điểm trên Model; ESC để huỷ. Chuột phải một ký hiệu: xoá khỏi công cụ, ẩn/khôi phục, "+LogCaption+".");
#else
   tips.SetToolTip(grid,"Bấm chuột trái một ký hiệu: hình đi theo con trỏ, chọn điểm trên Model; ESC để huỷ. Chuột phải: ẩn/khôi phục, "+LogCaption+".");
#endif
#else
   tips.SetToolTip(add,AddTip);tips.SetToolTip(legend,LegendTip);tips.SetToolTip(settings,SettingsTip);tips.SetToolTip(messages,MessagesTip);tips.SetToolTip(search,"Tìm theo tên khối, nhóm hoặc file nguồn");
   tips.SetToolTip(grid,"Bấm chuột trái một ký hiệu: hình đi theo con trỏ, chọn điểm trên Model; ESC để huỷ. Chuột phải: ẩn/khôi phục.");
#endif
#if CABLES016
   int tab=0;foreach(var c in new Control[]{search,filter,grid,add,legend,messages})c.TabIndex=tab++;
#if MODELLIB019
   // TVXD-MEPS-MODELLIB-019: '+' opens a compact menu (no new permanent row); the list's right-click menu offers the three Model actions too.
   addMenu.Font=TextFont;foreach(var m in AddActions){string a=m[0];var item=new ToolStripMenuItem(m[1]){Tag=a};item.Click+=(s,e)=>Run(a);addMenu.Items.Add(item);}
   options.Items.Add(new ToolStripSeparator());foreach(var m in AddActions.Where(x=>x[0]!="import")){string a=m[0];var item=new ToolStripMenuItem(m[1]){Tag=a};item.Click+=(s,e)=>Run(a);options.Items.Add(item);}
   // 021: actual delete of the right-clicked tile, after hide/restore (distinct item).
   var del=new ToolStripMenuItem(DeleteCaption){Tag="delete"};del.Click+=(s,e)=>Run("delete");items["delete"]=del;options.Items.Insert(options.Items.IndexOf(items["restore"])+1,del);
#if LIBUPDATE023
   // 023: update the right-clicked tile's geometry from an edited Model block (after the delete item; disabled without a tile target).
   var upd=new ToolStripMenuItem(UpdateCaption){Tag="update"};upd.Click+=(s,e)=>Run("update");items["update"]=upd;options.Items.Insert(options.Items.IndexOf(del)+1,upd);
#endif
#if METADATA032
   // 032: edit the right-clicked tile's legend text (just before the delete item); disabled when the menu was opened outside a tile.
   var meta=new ToolStripMenuItem(MetadataCaption){Tag="metadata",Name="metadata032",AccessibleName=MetadataCaption};meta.Click+=(s,e)=>Run("metadata");items["metadata"]=meta;
   options.Items.Insert(options.Items.IndexOf(del),meta);options.Opening+=(s,e)=>{meta.Enabled=contextTarget!=null;};
#endif
#if CIRCUIT032
   // 032 circuit tags: two items at the end of the existing right-click menu (no permanent control). They act on blocks picked on Model, not on the tile.
   options.Items.Add(new ToolStripSeparator());
   foreach(var m in new[]{new[]{CircuitUi032.Assign,CircuitUi032.AssignCaption},new[]{CircuitUi032.Unassigned,CircuitUi032.UnassignedCaption}}){string a=m[0];
    var item=new ToolStripMenuItem(m[1]){Tag=a,Name=a,AccessibleName=m[1]};item.Click+=(s,e)=>Run(a);items[a]=item;options.Items.Add(item);}
#endif
   tips.SetToolTip(add,AddTip019);
#endif
#else
   int tab=0;foreach(var c in new Control[]{search,filter,ratio,grid,add,legend,settings,messages})c.TabIndex=tab++;
#endif
   Categories();FillRatio();Message("Bấm một ký hiệu, rê chuột và chọn điểm trên Model; ESC để huỷ.");Fit();}
  CompactLibraryView05.RoundButton Glyph(string glyph,string name){var b=new CompactLibraryView05.RoundButton{Text=glyph,Dock=DockStyle.Fill,Margin=new Padding(S(1)),AccessibleName=name,Font=SymbolFont};b.Colors(CField,CLine,CHover,CFore);return b;}
  TableLayoutPanel Row(int[] cols){var t=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=cols.Length,RowCount=1,Margin=new Padding(0)};
#if UI031
   t.BackColor=CBack;
#endif
   t.RowStyles.Add(new RowStyle(SizeType.Percent,100));foreach(int w in cols)t.ColumnStyles.Add(w<0?new ColumnStyle(SizeType.Percent,100):new ColumnStyle(SizeType.Absolute,w));return t;}
  CompactLibraryView05.RoundField Field(Control c){var f=new CompactLibraryView05.RoundField(c);f.Margin=new Padding(0,S(2),c is ComboBox&&c!=ratio?0:S(2),S(2));return f;}
  [DllImport("user32.dll",CharSet=CharSet.Unicode)]static extern IntPtr SendMessage(IntPtr h,int msg,IntPtr w,string l);
  static void Cue(TextBox t,string text){try{SendMessage(t.Handle,0x1501,(IntPtr)1,text);}catch(Exception){}}
  protected override void OnSizeChanged(EventArgs e){base.OnSizeChanged(e);Fit();}
  // Sized from the view's own Size (stable while a scroll bar is shown), never from inside a layout pass. The view scrolls only below MinContentHeight.
  internal void Fit(){if(fitting||root==null||root.RowStyles.Count<=GridRow)return;fitting=true;
   try{int w=Width,h=Height;bool scroll=h<MinContentHeight;if(!scroll&&AutoScroll){AutoScrollPosition=Point.Empty;AutoScroll=false;}
    int g=GridHeightFor(h);if((int)root.RowStyles[GridRow].Height!=g)root.RowStyles[GridRow].Height=g;
    if(!scroll){if(root.Bounds!=new Rectangle(0,0,w,h))root.Bounds=new Rectangle(0,0,w,h);}
    else{if(!AutoScroll){root.Location=Point.Empty;AutoScroll=true;}root.Size=new Size(Math.Max(0,w-SystemInformation.VerticalScrollBarWidth),MinContentHeight);}}
   finally{fitting=false;}}
  // ---- messages ----
  public void Message(string s){Add(s,false);}
  public void Warn(string s){Add(s,true);}
  void Add(string s,bool warn){if(string.IsNullOrEmpty(s))return;log.Add(DateTime.Now.ToString("HH:mm:ss")+(warn?" [!] ":"     ")+s);if(log.Count>200)log.RemoveAt(0);if(warn)warnings++;Last=s;LastWarning=warn;
#if CABLES016
   // The ? button keeps its help tooltip and colours; the status goes to the title.
   Indicate();
#else
   tips.SetToolTip(title,s);tips.SetToolTip(messages,MessagesTip+(warnings>0?" — "+warnings+" cảnh báo":"")+"\nGần nhất: "+s);messages.Colors(CField,warnings>0?CWarn:CLine,CHover,warnings>0?CWarn:CMuted);
#endif
   if(messagesText!=null)messagesText.Text=string.Join("\r\n",log);}
#if CABLES016
  void Indicate(){tips.SetToolTip(title,StatusText);title.ForeColor=warnings>0?CWarn:CFore;}
  void ClearIndicator(){Indicate();}
  // Read-only naming guide; "Nhật ký / Cảnh báo" closes it and opens the log (one modal dialog at a time).
  internal void ShowNamingHelp(){if(helpDialog==null){helpDialog=Dialog(HelpTip,S(400),S(440));var pane=Pane(new[]{-1,S(38)});
    helpText=new TextBox{Multiline=true,ReadOnly=true,ScrollBars=ScrollBars.Vertical,Dock=DockStyle.Fill,BackColor=CField,ForeColor=CFore,Font=TextFont,BorderStyle=BorderStyle.None,WordWrap=true,AccessibleName=HelpTip,Text=NamingHelpText
#if LIBUPDATE023
     +UpdateHelpText
#endif
#if METADATA032
     +MetadataHelpText
#endif
#if TAG033
     +TagHelpText
#endif
    };pane.Controls.Add(helpText,0,0);
    var logs=MakeButton(LogCaption);logs.DialogResult=DialogResult.Retry;var close=MakeButton("Đóng");close.DialogResult=DialogResult.OK;var bar=Row(new[]{-1,-1});bar.Controls.Add(logs,0,0);bar.Controls.Add(close,1,0);pane.Controls.Add(bar,0,1);
    helpDialog.Controls.Add(pane);helpDialog.AcceptButton=close;helpDialog.CancelButton=close;}
   helpText.SelectionStart=0;helpText.SelectionLength=0;helpText.ScrollToCaret();Near(helpDialog);DialogResult r;
   try{r=ShowModal(helpDialog);}catch(Exception ex){Warn("Chưa mở được hướng dẫn đặt tên block: "+ex.Message);return;}
   if(r==DialogResult.Retry)ShowMessages();}
#else
  void ClearIndicator(){messages.Colors(CField,CLine,CHover,CMuted);}
#endif
  internal void ShowMessages(){if(messagesDialog==null){messagesDialog=Dialog(MessagesTip,S(360),S(280));var pane=Pane(new[]{-1,S(38)});
    messagesText=new TextBox{Multiline=true,ReadOnly=true,ScrollBars=ScrollBars.Vertical,Dock=DockStyle.Fill,BackColor=CField,ForeColor=CFore,Font=TextFont,BorderStyle=BorderStyle.None,WordWrap=true};pane.Controls.Add(messagesText,0,0);
    var copy=MakeButton("Sao chép");copy.Click+=(s,e)=>{try{Clipboard.SetText(messagesText.Text.Length>0?messagesText.Text:" ");}catch(Exception){}};var clear=MakeButton("Xoá");clear.Click+=(s,e)=>{log.Clear();warnings=0;messagesText.Text="";ClearIndicator();};
    var close=MakeButton("Đóng");close.DialogResult=DialogResult.OK;var bar=Row(new[]{-1,-1,-1});bar.Controls.Add(copy,0,0);bar.Controls.Add(clear,1,0);bar.Controls.Add(close,2,0);pane.Controls.Add(bar,0,1);messagesDialog.Controls.Add(pane);messagesDialog.AcceptButton=close;messagesDialog.CancelButton=close;}
   messagesText.Text=string.Join("\r\n",log);messagesText.SelectionStart=messagesText.Text.Length;messagesText.ScrollToCaret();Near(messagesDialog);try{ShowModal(messagesDialog);}catch(Exception){}}
  Form Dialog(string text,int w,int h){var f=new Form{Text=text,FormBorderStyle=FormBorderStyle.FixedToolWindow,ShowInTaskbar=false,MinimizeBox=false,MaximizeBox=false,StartPosition=FormStartPosition.Manual,BackColor=CBack,ForeColor=CFore,Font=TextFont,AutoScaleMode=AutoScaleMode.None};f.ClientSize=new Size(w,h);return f;}
  TableLayoutPanel Pane(int[] rows){var p=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=1,RowCount=rows.Length,Margin=new Padding(0),Padding=new Padding(S(8),S(6),S(8),S(6))};foreach(int h in rows)p.RowStyles.Add(h<0?new RowStyle(SizeType.Percent,100):new RowStyle(SizeType.Absolute,h));return p;}
  CompactLibraryView05.RoundButton MakeButton(string text){var b=new CompactLibraryView05.RoundButton{Text=text,Dock=DockStyle.Fill,Margin=new Padding(S(2)),Font=TextFont};b.Colors(CField,CTeal,CHover,CTealHot);return b;}
  Label Caption(string text){return new Label{Text=text,Dock=DockStyle.Fill,TextAlign=ContentAlignment.MiddleLeft,Margin=new Padding(0),ForeColor=CMuted,Font=TextFont};}
  void Near(Form f){try{var at=IsHandleCreated?PointToScreen(new Point(S(8),S(HeaderHeight))):Cursor.Position;var area=Screen.FromPoint(at).WorkingArea;f.Location=new Point(Math.Max(area.Left,Math.Min(at.X,area.Right-f.Width)),Math.Max(area.Top,Math.Min(at.Y,area.Bottom-f.Height)));}catch(Exception){}}
  // ---- ratio ----
  static bool IsPreset(double n){return Meps06.Presets.Any(p=>p==n);}
  internal string SourceUnits(){return Catalog==null||Catalog.entries.Count==0?"(chưa có thư viện)":string.Join(", ",Catalog.entries.Select(e=>e.units??"?").Distinct().OrderBy(x=>x,StringComparer.OrdinalIgnoreCase));}
  void FillRatio(){filling=true;try{ratio.Items.Clear();ratio.Items.Add(Meps06.RawItem);foreach(var n in Meps06.Presets)ratio.Items.Add(Meps06.PresetText(n));
    if(custom.HasValue&&!IsPreset(custom.Value))ratio.Items.Add(Meps06.PresetText(custom.Value)+Meps06.CustomSuffix);ratio.Items.Add(Meps06.CustomItem);ratio.SelectedIndex=IndexOf(choice);lastIndex=ratio.SelectedIndex;}finally{filling=false;}RatioNote();}
  int IndexOf(double? n){if(!n.HasValue)return 0;int i=Array.IndexOf(Meps06.Presets,n.Value);if(i>=0)return i+1;return custom.HasValue&&custom.Value==n.Value?Meps06.Presets.Length+1:0;}
  double? ValueAt(int i){if(i<=0)return null;if(i<=Meps06.Presets.Length)return Meps06.Presets[i-1];if(custom.HasValue&&!IsPreset(custom.Value)&&i==Meps06.Presets.Length+1)return custom;return null;}
  // A preset or custom 1:N needs the confirmed setup first (opened automatically; cancelling keeps the previous choice). Changing the choice only affects
  // later insertions; placed symbols are never rescaled.
  void RatioChanged(){if(filling)return;int i=ratio.SelectedIndex;
   if(i<=0){choice=null;lastIndex=0;RatioNote();Message("Tỷ lệ ký hiệu: "+Meps06.RawItem+" (hình gốc, chưa quy đổi); áp dụng cho lần đặt sau.");return;}
   if(!Confirmed&&!OpenSetup()){Revert();return;}
   if(i==ratio.Items.Count-1){double n;if(!AskDenominator(out n)){Revert();return;}custom=n;choice=n;FillRatio();}
   else{choice=ValueAt(i);lastIndex=i;RatioNote();}
   Message("Tỷ lệ ký hiệu 1:"+Library.F(choice.Value)+" → hệ số "+Library.F(Meps06.Scale(setup,choice))+" ("+Library.F(setup.baseScale)+" × "+Library.F(choice.Value)+" / "+Library.F(setup.refDenominator)+"); chỉ áp dụng cho lần đặt sau, ký hiệu đã đặt giữ nguyên.");}
  void Revert(){filling=true;try{ratio.SelectedIndex=Math.Max(0,Math.Min(lastIndex,ratio.Items.Count-1));}finally{filling=false;}RatioNote();}
  internal string RatioText(){if(!choice.HasValue)return Meps06.RawItem+" (hình gốc, chưa quy đổi)";try{return "1:"+Library.F(choice.Value)+" → hệ số "+Library.F(Meps06.Scale(setup,choice));}catch(Exception ex){return "1:"+Library.F(choice.Value)+" — "+ex.Message;}}
  void RatioNote(){string s=RatioText()+"\n"+(Confirmed?"Thiết lập: N0 = "+Library.F(setup.refDenominator)+", hệ số gốc "+Library.F(setup.baseScale)+" ("+setup.provenance+").":"Chưa thiết lập N0 (bấm ⚙); chỉ dùng được "+Meps06.RawItem+".")
    +"\nĐơn vị nguồn (metadata thư viện): "+SourceUnits()+", chưa xác nhận là mm. Bản vẽ: "+(drawingUnits??"chưa đọc")+". Không tự quy đổi đơn vị. Đổi tỷ lệ chỉ áp dụng cho lần đặt sau.";
   tips.SetToolTip(ratio,s);tips.SetToolTip(ratioLabel,s);}
  void BuildSetup(){if(setupDialog!=null)return;setupDialog=Dialog("Thiết lập tỷ lệ ký hiệu",S(270),S(304));var pane=Pane(new[]{S(78),S(30),S(30),S(40),S(50),-1});
   setupUnits=new Label{Dock=DockStyle.Fill,Margin=new Padding(0),ForeColor=CMuted,Font=TextFont};pane.Controls.Add(setupUnits,0,0);
   setupN0=new TextBox{AccessibleName="Mẫu số gốc N0",Font=TextFont,BackColor=CField,ForeColor=CFore};setupBase=new TextBox{AccessibleName="Hệ số gốc",Font=TextFont,BackColor=CField,ForeColor=CFore};
   var r1=Row(new[]{S(130),-1});r1.Controls.Add(Caption("Ký hiệu vẽ cho 1:"),0,0);r1.Controls.Add(Field(setupN0),1,0);pane.Controls.Add(r1,0,1);
   var r2=Row(new[]{S(130),-1});r2.Controls.Add(Caption("Hệ số gốc"),0,0);r2.Controls.Add(Field(setupBase),1,0);pane.Controls.Add(r2,0,2);
   setupFormula=new Label{Dock=DockStyle.Fill,Margin=new Padding(0),ForeColor=CFore,Font=TextFont};pane.Controls.Add(setupFormula,0,3);
   setupConfirm=new CheckBox{Text="Tôi xác nhận N0 và hệ số gốc ở trên (MEPS không tự đặt, không tự quy đổi đơn vị).",Dock=DockStyle.Fill,Margin=new Padding(0),ForeColor=CFore,Font=TextFont,CheckAlign=ContentAlignment.TopLeft,TextAlign=ContentAlignment.TopLeft};pane.Controls.Add(setupConfirm,0,4);
   setupOk=MakeButton("Xác nhận");setupOk.DialogResult=DialogResult.OK;var cancel=MakeButton("Hủy");cancel.DialogResult=DialogResult.Cancel;var bar=Row(new[]{-1,-1});bar.Controls.Add(setupOk,0,0);bar.Controls.Add(cancel,1,0);pane.Controls.Add(bar,0,5);
   setupDialog.Controls.Add(pane);setupDialog.AcceptButton=setupOk;setupDialog.CancelButton=cancel;
   setupN0.TextChanged+=(s,e)=>SetupState();setupBase.TextChanged+=(s,e)=>SetupState();setupConfirm.CheckedChanged+=(s,e)=>SetupState();
   tips.SetToolTip(setupN0,"N0: mẫu số tỷ lệ mà ký hiệu thư viện được vẽ cho (1:N0). Không có giá trị mặc định.");tips.SetToolTip(setupBase,"Hệ số gốc nhân thêm vào mọi tỷ lệ 1:N (thường là 1 nếu không cần hiệu chỉnh).");}
  internal void SetupState(){if(setupDialog==null)return;string n0=setupN0.Text,b=setupBase.Text;string formula;bool valid=true;
   try{var s=Meps06.Confirm(n0,b,true,null);formula="Hệ số chèn = "+Library.F(s.baseScale)+" × N / "+Library.F(s.refDenominator)+(choice.HasValue?"; 1:"+Library.F(choice.Value)+" → "+Library.F(Meps06.Scale(s,choice)):"; ví dụ 1:"+Library.F(Meps06.Presets[0])+" → "+Library.F(Meps06.Scale(s,Meps06.Presets[0])));}
   catch(Exception ex){valid=false;formula=ex.Message;}
   setupFormula.Text=formula;setupFormula.ForeColor=valid?CFore:CWarn;setupOk.Enabled=valid&&setupConfirm.Checked;}
  internal bool OpenSetup(){BuildSetup();setupUnits.Text="Đơn vị nguồn (metadata thư viện): "+SourceUnits()+" — chưa xác nhận là mm.\nĐơn vị bản vẽ hiện tại: "+(drawingUnits??"chưa đọc")+". MEPS không tự quy đổi đơn vị.";
   setupN0.Text=Confirmed?Library.F(setup.refDenominator):"";setupBase.Text=Confirmed?Library.F(setup.baseScale):"1";setupConfirm.Checked=false;SetupState();Near(setupDialog);
   DialogResult r;try{r=ShowModal(setupDialog);}catch(Exception ex){Warn("Chưa mở được thiết lập tỷ lệ: "+ex.Message);return false;}
   if(r!=DialogResult.OK){Message("Đã huỷ thiết lập tỷ lệ; giữ nguyên lựa chọn trước.");return false;}
   try{var s=Meps06.Confirm(setupN0.Text,setupBase.Text,setupConfirm.Checked,"người dùng xác nhận "+DateTime.Now.ToString("yyyy-MM-dd HH:mm"));setup=s;if(drawingKey!=null)setups[drawingKey]=s;RatioNote();
    Message("Đã xác nhận: N0 = "+Library.F(s.refDenominator)+", hệ số gốc "+Library.F(s.baseScale)+". Hệ số chèn = hệ số gốc × N / N0; không tự quy đổi đơn vị.");return true;}
   catch(Exception ex){Warn("Chưa thiết lập tỷ lệ: "+ex.Message);return false;}}
  internal bool AskDenominator(out double n){n=0;if(customDialog==null){customDialog=Dialog("Tỷ lệ tùy chỉnh",S(240),S(118));var pane=Pane(new[]{S(30),S(30),-1});
    pane.Controls.Add(Caption("Mẫu số tỷ lệ bản vẽ 1:N"),0,0);customBox=new TextBox{AccessibleName="Mẫu số N",Font=TextFont,BackColor=CField,ForeColor=CFore};pane.Controls.Add(Field(customBox),0,1);
    customOk=MakeButton("Chọn");customOk.DialogResult=DialogResult.OK;var cancel=MakeButton("Hủy");cancel.DialogResult=DialogResult.Cancel;var bar=Row(new[]{-1,-1});bar.Controls.Add(customOk,0,0);bar.Controls.Add(cancel,1,0);pane.Controls.Add(bar,0,2);
    customDialog.Controls.Add(pane);customDialog.AcceptButton=customOk;customDialog.CancelButton=cancel;}
   customBox.Text=custom.HasValue?Library.F(custom.Value):"";Near(customDialog);DialogResult r;try{r=ShowModal(customDialog);}catch(Exception ex){Warn("Chưa mở được tỷ lệ tùy chỉnh: "+ex.Message);return false;}
   if(r!=DialogResult.OK)return false;
   try{double v;if(!Meps06.Number(customBox.Text.Trim(),out v))throw new Exception("Nhập mẫu số N bằng số dương.");n=Meps05.CheckDenominator(v,"Mẫu số tỷ lệ bản vẽ N");return true;}catch(Exception ex){Warn("Chưa chọn tỷ lệ tùy chỉnh: "+ex.Message);return false;}}
  // Drawing switch. key = full document name; stored = {N0, N, base} saved in that drawing by an earlier ratio placement (explicit user values), else null.
  // Nothing carries over from another drawing; a setup confirmed in this session for the same drawing is kept.
  public void SetDrawing(string key,string name,string units,double[] stored){drawingKey=key??name;drawingName=name;drawingUnits=units;custom=null;choice=null;setup=null;
#if CABLES016
   // 016: a stored ratio (earlier build) is ignored here and left in the drawing as it is; no session setup is reused. Placement stays raw 1×.
   if(stored!=null)Message("Bản vẽ có tỷ lệ ký hiệu lưu từ phiên bản trước; bản này không dùng (không xoá/không sửa), ký hiệu đặt theo hình gốc 1× — dùng SCALE nếu cần.");
#else
   if(stored!=null&&stored.Length>=3){try{var s=new RatioSetup{refDenominator=stored[0],baseScale=stored[2],confirmed=true,provenance="đã lưu trong bản vẽ ở lần đặt trước"};Meps06.Scale(s,stored[1]);setup=s;choice=stored[1];if(!IsPreset(stored[1]))custom=stored[1];}
    catch(Exception ex){Warn("Tỷ lệ lưu trong bản vẽ không hợp lệ, dùng "+Meps06.RawItem+": "+ex.Message);setup=null;choice=null;custom=null;}}
   RatioSetup known;if(setup==null&&drawingKey!=null&&setups.TryGetValue(drawingKey,out known))setup=known;else if(setup!=null&&drawingKey!=null)setups[drawingKey]=setup;
#endif
   FillRatio();}
  internal LibPlacement Placement(LibEntry e){var p=new LibPlacement{id=e.id,rotationDeg=0};
#if CABLES016
   // 016: never a ratio (whatever the drawing or session state). Raw 1× except E-NHE-RJ45/E-NHE-TEL, whose default is the uniform scale that makes
   // them as tall as E-OC-1 placed at 1× (Layout016.PlacementScale; throws if the reference is missing). RunPlace uses p.scale as is for the cursor
   // preview AND the insertion (no ratio, no fitWidth), so both get this one value; it is not multiplied again anywhere.
   p.scale=Layout016.PlacementScale(e,Lookup);p.baseScale=p.scale;p.ratio=false;
#else
   if(choice.HasValue){p.scale=Meps06.Scale(setup,choice);p.ratio=true;p.baseScale=setup.baseScale;p.refDenominator=setup.refDenominator;p.drawingDenominator=choice.Value;}else{p.scale=1;p.baseScale=1;}
#endif
   return p;}
#if CABLES016
  // Sizing reference ONLY (PlacementScale + its tooltip): an entry deleted from the tool keeps its retained dimensions (Library.Reference), so E-NHE stays
  // placeable after E-OC-1 is deleted (PATCH02 gap found 021). Never used to list, select or insert; Find/Search still exclude deleted entries.
  LibEntry Lookup(string id){return Library.Reference(Catalog,id);}
  // Scale wording of a placement request (message) and of a tile (tooltip): never "1×" for an E-NHE entry.
  internal static string ScaleText(LibPlacement p){return p.scale==1?Meps06.RawItem+" (hình gốc, chưa quy đổi)":"hệ số "+Library.F(p.scale)+" (cao bằng "+Layout016.ReferenceName+" đặt 1×)";}
  string ScaleNote(LibEntry e){if(Layout016.Kind(e)!=Layout016.Nhe)return "";
   try{return "\nĐặt mới: hệ số "+Library.F(Layout016.PlacementScale(e,Lookup))+", cao bằng "+Layout016.ReferenceName+" cùng nguồn đặt 1× (ký hiệu đã đặt không đổi; SCALE để chỉnh).";}
   catch(Exception ex){return "\nCHƯA ĐẶT ĐƯỢC: "+ex.Message;}}
#endif
  // ---- library ----
  public void Reload(){Reload(null);}
  public void Reload(string selectName){string keep=selectName==null&&Selected!=null?Selected.id:null;Catalog=null;
   if(Root!=null)try{Catalog=Library.Load(Root);}catch(Exception ex){Warn("Catalog lỗi, chưa sửa gì: "+ex.Message+" (bản trước: catalog.json.bak)");}else Warn("Chưa có thư mục thư viện.");
   grid.SuspendLayout();try{foreach(var t in tiles){grid.Controls.Remove(t);t.Dispose();}tiles.Clear();if(Catalog!=null)foreach(var e in Library.Search(Catalog,"",Library.All,true)){var t=new Tile(this,e);
#if CABLES016
    tips.SetToolTip(t,Tip(e)+ScaleNote(e));
#else
    tips.SetToolTip(t,Tip(e));
#endif
    tiles.Add(t);grid.Controls.Add(t);}}finally{grid.ResumeLayout(false);}
   Categories();Filter();RatioNote();var pick=tiles.FirstOrDefault(t=>t.Listed&&(keep!=null?t.Entry.id==keep:selectName!=null&&string.Equals(t.Entry.name,selectName,StringComparison.OrdinalIgnoreCase)));if(pick!=null)Choose(pick,false);}
  static string Tip(LibEntry e){return e.name+"\n"+e.category+" · nguồn "+e.sourceFile+" ("+e.units+", chưa quy đổi)\nGốc "+Library.F(Library.Width(e))+" × "+Library.F(Library.Height(e))+" · "+CompactLibraryView05.StyleText(e)+(e.dynamic?" · khối động":"")+(e.hidden?"\nĐANG ẨN":"")
   +(Meps05.PlaceIssue(e)!=null?"\nCHƯA ĐẶT ĐƯỢC: "+Meps05.PlaceIssue(e):"")+"\nBấm chuột trái: hình đi theo con trỏ, chọn điểm; ESC huỷ. Chuột phải: "+
#if MODELLIB019
   "xoá khỏi công cụ, ẩn/khôi phục.";}
#else
   "ẩn/khôi phục.";}
#endif
  // Rebuilds the group menu; the current group is kept while it still exists, otherwise the filter returns to Library.All.
  void Categories(){var names=Catalog==null?new[]{Library.All}:Library.Categories(Catalog,showHidden.Checked);if(!names.Contains(group))group=Library.All;
   var old=groups.Items.Cast<ToolStripItem>().ToList();groups.Items.Clear();foreach(var i in old)i.Dispose();
#if UI031
   // 031: HỆ THỐNG heading, Electric / Điện (submenu: Tất cả block điện, separator, existing groups), then the disabled future systems.
   groupItems.Clear();groups.Items.Add(new ToolStripLabel(SystemHeading){ForeColor=CMuted,AccessibleName=SystemHeading});
   electricItem=new ToolStripMenuItem(ElectricCaption){Checked=true,AccessibleName=ElectricCaption,ToolTipText="Bấm: chọn "+AllElectricCaption+". Rê chuột: chọn nhóm."};electricItem.DropDown.Font=TextFont;
   electricItem.DropDown.Renderer=MenuRenderer;electricItem.DropDown.BackColor=CBack;electricItem.DropDown.ForeColor=CFore;
   electricItem.Click+=(s,e)=>{SelectGroup(Library.All);try{groups.Close(ToolStripDropDownCloseReason.ItemClicked);}catch(Exception){}};
   foreach(var n in names){string name=n;var item=new ToolStripMenuItem(GroupCaption(name)){Tag=name,Checked=name==group,AccessibleName=GroupCaption(name)};item.Click+=(s,e)=>SelectGroup(name);
    groupItems.Add(item);electricItem.DropDownItems.Add(item);if(name==Library.All)electricItem.DropDownItems.Add(new ToolStripSeparator());}
   groups.Items.Add(electricItem);foreach(var f in FutureSystems)groups.Items.Add(new ToolStripMenuItem(f){Enabled=false,ToolTipText=FutureTip,AccessibleName=f});groups.ShowItemToolTips=true;
#else
   foreach(var n in names){string name=n;var item=new ToolStripMenuItem(name){Tag=name,Checked=name==group};item.Click+=(s,e)=>SelectGroup(name);groups.Items.Add(item);}
#endif
   GroupNote();}
#if UI031
  internal void SelectGroup(string name){if(!GroupNames.Contains(name))throw new ArgumentException("Không có nhóm "+name+".");group=name;foreach(var i in groupItems)i.Checked=(string)i.Tag==name;GroupNote();Filter();}
  // Search scope shown as the search box's cue text (Điện · current group).
  internal string ScopeCue{get{return "Tìm: Điện · "+(group==Library.All?"Tất cả":GroupCaption(group));}}
#else
  internal void SelectGroup(string name){if(!GroupNames.Contains(name))throw new ArgumentException("Không có nhóm "+name+".");group=name;foreach(var i in groups.Items.OfType<ToolStripMenuItem>())i.Checked=(string)i.Tag==name;GroupNote();Filter();}
#endif
  internal void OpenGroups(){groups.Show(filter,new Point(0,filter.Height));}
  // Tooltip and accessible description carry the current group; the button border turns teal while a group filter is active.
#if UI031
  void GroupNote(){bool all=group==Library.All;string cap=GroupCaption(group);
   tips.SetToolTip(filter,"Hệ thống: Điện — nhóm đang chọn: "+cap+(all?"":" (bấm ▾ → "+ElectricCaption+" để đổi hoặc chọn "+AllElectricCaption+")"));filter.AccessibleDescription="Hệ thống: Điện; nhóm đang chọn: "+cap;
   filter.Colors(CField,all?CLine:CTeal,CHover,all?CFore:CTealHot);if(search!=null&&search.IsHandleCreated)Cue(search,ScopeCue);}
#else
  void GroupNote(){bool all=group==Library.All;tips.SetToolTip(filter,"Lọc theo nhóm — đang chọn: "+group+(all?"":" (bấm để đổi hoặc chọn "+Library.All+")"));filter.AccessibleDescription="Nhóm đang chọn: "+group;
   filter.Colors(CField,all?CLine:CTeal,CHover,all?CFore:CTealHot);}
#endif
  internal void Filter(){var listed=Catalog==null?new HashSet<string>():new HashSet<string>(Library.Search(Catalog,search.Text,group,showHidden.Checked).Select(e=>e.id));
   grid.SuspendLayout();try{foreach(var t in tiles){t.Listed=listed.Contains(t.Entry.id);t.Visible=t.Listed;if(!t.Listed)t.Selected=false;}}finally{grid.ResumeLayout(true);}SizeTiles();}
  internal int TileWidth{get{return Math.Max(S(60),(grid.Width-SystemInformation.VerticalScrollBarWidth-grid.Padding.Horizontal-4*S(TileMargin))/2);}}
  internal void SizeTiles(){int w=TileWidth,h=S(TileHeight);grid.SuspendLayout();try{foreach(var t in tiles)if(t.Width!=w||t.Height!=h)t.Size=new Size(w,h);}finally{grid.ResumeLayout(true);}}
  internal void Wheel(int delta){int y=-grid.AutoScrollPosition.Y-delta*RowPitch/120;grid.AutoScrollPosition=new Point(0,Math.Max(0,y));}
  // Library preview rendered at twice the icon size (at least 24 px, the renderer's minimum) and drawn into the 16 px icon box. Display only.
  internal Bitmap Thumb(LibEntry e){int size=Math.Max(24,2*S(Icon));string key=Root+"|"+e.id+"|"+e.fileSha256+"|"+size;Bitmap b;if(cache.TryGetValue(key,out b))return b;Renders++;using(var raw=Library.Render(Library.LoadPreview(Root,e),size))b=CompactLibraryView.Dark(raw);cache[key]=b;return b;}
  internal void Choose(Tile t,bool focus){foreach(var x in tiles)if(x!=t&&x.Selected){x.Selected=false;x.TabStop=false;x.Invalidate();}t.Selected=true;t.TabStop=true;t.Invalidate();grid.ScrollControlIntoView(t);if(focus)t.Focus();}
  void FocusTiles(){var t=tiles.FirstOrDefault(x=>x.Selected&&x.Listed)??tiles.FirstOrDefault(x=>x.Listed);if(t!=null)Choose(t,true);}
  // ---- placement requests ----
  // The plugin decides: a request replaces one whose command has not started, and is refused (exception, shown as a warning) while the cursor preview runs.
  internal void Place(Tile t){if(t==null)return;var e=t.Entry;
   try{if(e.hidden)throw new Exception("mục đang ẩn; khôi phục trước khi đặt.");var issue=Meps05.PlaceIssue(e);if(issue!=null)throw new Exception(e.name+": "+issue);if(Request==null)throw new Exception("bảng chưa nối với AutoCAD; chưa đặt gì.");
    var p=Placement(e);Request("place",p);Arm(e);
#if CABLES016
    Message("Đặt "+e.name+" ("+ScaleText(p)+"): rê chuột trên Model, hình đi theo con trỏ; bấm chọn điểm, ESC để huỷ.");}
#else
    Message("Đặt "+e.name+" ("+RatioText()+"): rê chuột trên Model, hình đi theo con trỏ; bấm chọn điểm, ESC để huỷ.");}
#endif
   catch(Exception ex){Warn("Chưa đặt: "+ex.Message);}}
  void Arm(LibEntry e){string old=armed;armed=e.id;armedName=e.name;Repaint(old);Repaint(e.id);}
  // Called by the plugin when the request has finished (placed, ESC, error, document switch).
  public void Disarm(){if(armed==null)return;string id=armed;armed=armedName=null;Repaint(id);}
  internal bool CancelArm(){if(armed==null)return false;string name=armedName;Disarm();try{if(Request!=null)Request("cancel",null);}catch(Exception){}Message("Đã huỷ yêu cầu đặt "+name+" (nếu CAD chưa bắt đầu chọn điểm); chưa thay đổi bản vẽ.");return true;}
  void Repaint(string id){if(id==null)return;foreach(var t in tiles)if(t.Entry.id==id)t.Invalidate();}
  protected override bool ProcessCmdKey(ref System.Windows.Forms.Message msg,Keys keyData){if(keyData==Keys.Escape&&CancelArm())return true;return base.ProcessCmdKey(ref msg,keyData);}
  internal bool Key(Keys k){if(k==Keys.Escape)return CancelArm();var list=tiles.Where(t=>t.Listed).ToList();if(list.Count==0)return false;int i=list.FindIndex(t=>t.Selected);
   if(k==Keys.Enter){if(i>=0)Place(list[i]);return true;}int step=k==Keys.Left?-1:k==Keys.Right?1:k==Keys.Up?-2:k==Keys.Down?2:0;int to=k==Keys.Home?0:k==Keys.End?list.Count-1:step==0?-2:i<0?0:Math.Max(0,Math.Min(list.Count-1,i+step));if(to==-2)return false;Choose(list[to],true);return true;}
  internal void TileDown(Tile t,MouseButtons b){if(b==MouseButtons.Left||b==MouseButtons.Right)Choose(t,true);}
  internal void TileClick(Tile t,MouseButtons b){if(b==MouseButtons.Left)Place(t);}
  internal void TileUp(Tile t,MouseButtons b,Point at){if(b==MouseButtons.Right)ShowOptions(t,at,t.Entry);}
  // 021: the entry of the tile the right-click menu was opened on (null = opened on the empty list/lower half). Only the delete action uses it.
  internal LibEntry ContextTarget{get{return contextTarget;}}LibEntry contextTarget;
  internal void ShowOptions(Control c,Point at,LibEntry target){contextTarget=target;ToolStripMenuItem del;if(items.TryGetValue("delete",out del))del.Enabled=target!=null;if(items.TryGetValue("update",out del))del.Enabled=target!=null;options.Show(c,at);}
  internal void Run(string action){try{Act(action);}catch(Exception ex){Warn("Chưa thực hiện: "+ex.Message);}}
#if CIRCUIT032
  // QA hook: clicks the ACTUAL circuit menu item (same Click handler as the right-click menu → Request → bound queue). Only the two circuit actions.
  public void ClickCircuitMenu(string action){ToolStripMenuItem it;if(!CircuitUi032.Handles(action)||!items.TryGetValue(action,out it))throw new ArgumentException("Không phải mục mã lộ: "+action);it.PerformClick();}
#endif
#if TAG033
  // QA hook: clicks the ACTUAL TAG button (Button.PerformClick: the palette must be shown and the button visible/enabled) → TagClicked → Request → bound queue.
  public void ClickTag(){if(tagBar==null)throw new InvalidOperationException("Không có nút TAG.");tagBar.PerformClick();}
  // One request per click; every palette menu is closed BEFORE the request reaches AutoCAD (selection prompt on the command line).
  internal void TagClicked(){CloseMenus();
#if LAYOUT030
   CloseLayoutMenu();
#endif
   try{if(Request==null)throw new Exception("bảng chưa nối với AutoCAD.");if(armed!=null)throw new Exception("Đang chờ đặt "+armedName+"; ESC trước.");
    Request(TagUi033.Edit,null);Message("TAG: chọn các block trên Model rồi Enter (xem dòng lệnh); hộp nhãn sẽ mở. ESC huỷ, bản vẽ không đổi.");}
   catch(Exception ex){Warn("Chưa thực hiện TAG: "+ex.Message);}}
#endif
  void AddClicked(){
#if MODELLIB019
   OpenAddMenu();
#else
   Run("import");
#endif
  }
#if MODELLIB019
  internal void OpenAddMenu(){addMenu.Show(add,new Point(0,add.Height));}
  // TVXD-MEPS-MODELUI-020 F01 (defensive hardening; the root cause of the live 020 failure is NOT verified — the old 019 menu passed the same UI tests, and the
  // 020 runtime log stopped at AWAIT_SELECT followed by Cancel): a menu item's Click runs while its menu is still shown, so every palette dropdown is closed
  // here BEFORE a Model/import/legend request goes to AutoCAD, on the hypothesis that an open menu could still hold input during the Editor prompt. One shared
  // path (Act) for the '+' menu and the list's right-click menu; Request is still called once.
  internal ContextMenuStrip[] Menus{get{return new[]{addMenu,options,groups};}}
  internal bool AnyMenuOpen{get{return Menus.Any(m=>m.Visible);}}
  internal void CloseMenus(){foreach(var m in Menus)try{if(m.Visible)m.Close(ToolStripDropDownCloseReason.ItemClicked);}catch(Exception){}}
  // Action-specific instruction shown in the palette before the request (and so before AutoCAD's selection prompt starts).
  public static string ModelInstruction(string action){return action=="modeladd"?"Thêm block từ Model: chọn MỘT block trên Model (xem dòng lệnh); Esc để huỷ.":
   action=="modelcopy"?"Tạo bản sao để sửa: chọn MỘT block trên Model (xem dòng lệnh), sau đó nhập tên và chọn điểm đặt; Esc để huỷ.":
   "Xoá khỏi công cụ (chọn trên Model): chọn MỘT ký hiệu đã đặt trên Model (xem dòng lệnh), MEPS hỏi xác nhận trước khi xoá mục khỏi công cụ; Esc để huỷ. Không cần chọn trên Model: chuột phải ký hiệu trong danh sách → "+DeleteCaption;}
  // 021: deletes exactly the right-clicked tile's entry after the owned confirmation (ShowModal = Application.ShowModalDialog in AutoCAD). Refused with nothing
  // written: no tile target, selection changed, entry armed for placement, No/default/Esc/close. LibraryDelete021 re-reads the catalog under the library lock
  // and refuses a changed/missing entry (stale request). No Editor prompt, no document lock: only the library catalog file is written.
  internal void DeleteFromTool(LibEntry target){contextTarget=null;
   if(target==null)throw new Exception("Chuột phải đúng ký hiệu cần xoá trong danh sách (chưa có ký hiệu nào được chọn); chưa thay đổi gì.");
   if(Root==null)throw new Exception("Chưa có thư mục thư viện.");
   var sel=Selected;if(sel==null||sel.id!=target.id)throw new Exception("Ký hiệu đang chọn đã đổi sau khi mở menu; chuột phải lại đúng ký hiệu cần xoá. Chưa thay đổi gì.");
   if(armed==target.id)throw new Exception("Đang chờ đặt "+target.name+"; ESC trước khi xoá. Chưa thay đổi gì.");
   DialogResult r;using(var dlg=new ConfirmDeleteDialog021(target.name,target.id))r=ShowModal(dlg);
   if(r!=DialogResult.Yes){Message("Đã huỷ xoá "+target.name+"; thư viện giữ nguyên.");return;}
   var res=LibraryDelete021.Delete(Root,target);Reload();Message(res.Summary());}
#if LIBUPDATE023
  // 023: dispatches exactly the right-clicked tile's entry (snapshot) to the plugin's queued bound-document job. Same refusals as delete (no target, selection
  // changed, armed for placement); nothing is written here.
  internal void UpdateFromModel(LibEntry target){contextTarget=null;
   if(target==null)throw new Exception("Chuột phải đúng ký hiệu cần cập nhật hình trong danh sách; chưa thay đổi gì.");
   if(Root==null)throw new Exception("Chưa có thư mục thư viện.");if(RequestUpdate==null)throw new Exception("bảng chưa nối với AutoCAD.");
   var sel=Selected;if(sel==null||sel.id!=target.id)throw new Exception("Ký hiệu đang chọn đã đổi sau khi mở menu; chuột phải lại đúng ký hiệu. Chưa thay đổi gì.");
   if(armed!=null)throw new Exception("Đang chờ đặt "+armedName+"; ESC trước. Chưa thay đổi gì.");
   RequestUpdate(target);Message("Cập nhật hình "+target.name+": chọn MỘT khối đã sửa trên Model (xem dòng lệnh), đọc hộp xác nhận; Esc để huỷ. Thư viện chỉ đổi sau khi bạn bấm Có.");}
#endif
#if METADATA032
  // 032: edits exactly the right-clicked tile's entry. Same refusals as delete (no target, selection changed, entry armed for placement). The owned dialog
  // (ShowModal) saves itself through LegendMetadata032.Save under the library lock; Hủy/Esc/close = nothing written. No Editor prompt, no document lock.
  internal void EditMetadata(LibEntry target){contextTarget=null;
   if(target==null)throw new Exception("Chuột phải đúng ký hiệu cần sửa thông tin trong danh sách; chưa thay đổi gì.");
   if(Root==null)throw new Exception("Chưa có thư mục thư viện.");
   var sel=Selected;if(sel==null||sel.id!=target.id)throw new Exception("Ký hiệu đang chọn đã đổi sau khi mở menu; chuột phải lại đúng ký hiệu. Chưa thay đổi gì.");
   if(armed==target.id)throw new Exception("Đang chờ đặt "+target.name+"; ESC trước. Chưa thay đổi gì.");
   var snap=LegendMetadata032.Snapshot(Root,target);MetadataResult032 res;using(var dlg=new MetadataDialog032(snap,Dpi)){ShowModal(dlg);res=dlg.Result;}
   if(res==null){Message("Đã huỷ sửa thông tin bảng ký hiệu của "+target.name+"; chưa ghi gì.");return;}
   Message(res.Summary());}
#endif
#endif
#if FLEX022
  // 022 integration: one request per click through the plugin (busy/document/units/UCS checks there). Usable by UI QA with a stub RequestFlex (no drawing).
  // 029: the same click creates (free LINE/PLINE/ARC/SPLINE) or updates (an owned curved duct, after its centerline was edited); the plugin decides.
  internal void FlexClicked(){
#if MODELLIB019
   CloseMenus();
#endif
   try{if(RequestFlex==null)throw new Exception("bảng chưa nối với AutoCAD.");if(armed!=null)throw new Exception("Đang chờ đặt "+armedName+"; ESC trước.");
    RequestFlex();Message("Ống gió mềm: chọn đường tâm LINE/PLINE/ARC/SPLINE trên Model để tạo, hoặc chọn ống đã tạo để cập nhật sau khi sửa đường tâm (xem dòng lệnh); rồi nhập đường kính theo đơn vị bản vẽ hiện hành. Esc huỷ.");}
   catch(Exception ex){Warn("Chưa thực hiện Ống gió mềm: "+ex.Message);}}
  // Entry point kept from 022 (the plugin calls it after WirePlugin010.Attach): moves the wire view from the lower area into the action row, then lays the
  // row out. Idempotent; works with no wire view (flex alone, left slot).
  internal void FlexBelow(){var wire=blank.Controls.OfType<WireView010>().FirstOrDefault();
   if(wire!=null){wire.Dock=DockStyle.None;wire.Margin=new Padding(0);flexHost.Controls.Add(wire);}
   flexHost.BringToFront();
#if LAYOUT030
   if(layoutHost!=null)layoutHost.BringToFront();
#endif
   ArrangeActions();}
  internal WireView010 ActionWire{get{return flexHost.Controls.OfType<WireView010>().FirstOrDefault();}}
  // Same two-column geometry as the tiles: tile k's left = grid padding + TileMargin + k × (TileWidth + 2 TileMargin). Only child bounds change (never the
  // row's or the grid's size), and a guard stops re-entry.
  bool arranging;
  internal void ArrangeActions(){if(arranging||flexHost==null||flexBar==null)return;arranging=true;
   try{int w=TileWidth,h=S(FlexBarHeight),top=S(FlexGap),left=grid.Padding.Left+S(TileMargin),pitch=w+2*S(TileMargin);var wire=ActionWire;
    if(wire!=null){var r=new Rectangle(left,top,w,h);if(wire.Bounds!=r)wire.Bounds=r;}
    var f=new Rectangle(wire!=null?left+pitch:left,top,w,h);if(flexBar.Bounds!=f)flexBar.Bounds=f;
#if UI031
    // 031: exactly one tile wide in the left column (same size as DÂY DẪN / ỐNG GIÓ MỀM); the right slot stays empty.
    if(layoutBar!=null){var lb=new Rectangle(left,top,w,S(LayoutBarHeight));if(layoutBar.Bounds!=lb)layoutBar.Bounds=lb;}
#if TAG033
    // 033: TAG fills the right slot (tile column 1), same size as DÀN TRANG.
    if(tagBar!=null){var tb=new Rectangle(left+pitch,top,w,S(LayoutBarHeight));if(tagBar.Bounds!=tb)tagBar.Bounds=tb;}
#endif
#elif LAYOUT030
    // 030: left of tile column 0 to right of tile column 1.
    if(layoutBar!=null){var lb=new Rectangle(left,top,pitch+w,S(LayoutBarHeight));if(layoutBar.Bounds!=lb)layoutBar.Bounds=lb;}
#endif
   }
   finally{arranging=false;}}
  // F029-UI-01: action button width at a given view client width (same formula as TileWidth with grid width = client − 2·RootPad).
  internal int ActionWidthAt(int clientWidth){return Math.Max(S(60),(clientWidth-2*S(RootPad)-SystemInformation.VerticalScrollBarWidth-4*S(TileMargin))/2);}
  const TextFormatFlags ActionFlags=TextFormatFlags.SingleLine|TextFormatFlags.NoPrefix;
  // True when every action caption (TextRenderer width, default padding) plus S(ActionPad) fits width w.
  internal bool ActionFits(Font f,int w){foreach(var t in ActionCaptions)if(TextRenderer.MeasureText(t,f,Size.Empty,ActionFlags).Width+S(ActionPad)>w)return false;return true;}
  // Largest size from TextPx down in 0.5 px steps that fits at MinClientWidth; ActionMinPx when even that does not fit (reported by ActionFits = false).
#if UI031
  // 031: same fit, Segoe UI Semibold (Segoe UI Bold when Semibold is not installed) for the three uppercase action captions.
  static Font ActionFace(float px){var f=new Font("Segoe UI Semibold",px,FontStyle.Regular,GraphicsUnit.Pixel);if(f.Name=="Segoe UI Semibold")return f;f.Dispose();return new Font("Segoe UI",px,FontStyle.Bold,GraphicsUnit.Pixel);}
  Font FitActionFont(){int w=ActionWidthAt(S(MinClientWidth));
   for(float px=TextPx;px>ActionMinPx;px-=0.5f){var f=ActionFace(px*Dpi);if(ActionFits(f,w))return f;f.Dispose();}
   return ActionFace(ActionMinPx*Dpi);}
#else
  Font FitActionFont(){int w=ActionWidthAt(S(MinClientWidth));
   for(float px=TextPx;px>ActionMinPx;px-=0.5f){var f=new Font("Segoe UI",px*Dpi,FontStyle.Regular,GraphicsUnit.Pixel);if(ActionFits(f,w))return f;f.Dispose();}
   return new Font("Segoe UI",ActionMinPx*Dpi,FontStyle.Regular,GraphicsUnit.Pixel);}
#endif
#endif
#if LAYOUT030
  // 030/035: menu items carry their command (Tag; the caption already starts with it, so it is shown once); layoutItems[command] for QA. Opening the menu
  // runs nothing. 035: all items directly in the one dropdown (no submenu); 037: five items.
  void BuildLayoutMenu(){layoutMenu.Font=TextFont;
   foreach(var m in LayoutActions){string c=m[0];var item=new ToolStripMenuItem(m[1]){Tag=c,AccessibleName=m[1]};item.Click+=(s,e)=>LayoutClicked(c);layoutItems[c]=item;layoutMenu.Items.Add(item);}
   layoutMenu.Items.Add(new ToolStripSeparator());layoutHelpItem=new ToolStripMenuItem(LayoutHelpCaption);layoutHelpItem.Click+=(s,e)=>{CloseLayoutMenu();ShowLayoutHelp();};layoutMenu.Items.Add(layoutHelpItem);}
  internal void OpenLayoutMenu(){
#if MODELLIB019
   CloseMenus();
#endif
   layoutMenu.Show(layoutBar,new Point(0,layoutBar.Height));}
  internal void CloseLayoutMenu(){try{if(layoutMenu.Visible)layoutMenu.Close(ToolStripDropDownCloseReason.ItemClicked);}catch(Exception){}}
  // One request per item click; every palette menu is closed BEFORE the request reaches AutoCAD (its prompts follow on the command line).
  internal void LayoutClicked(string command){CloseLayoutMenu();
#if MODELLIB019
   CloseMenus();
#endif
   try{var a=LayoutActions.FirstOrDefault(x=>x[0]==command);if(a==null)throw new Exception("lệnh không có trong menu: "+command+".");
    if(RequestLayout==null)throw new Exception("bảng chưa nối với AutoCAD.");if(armed!=null)throw new Exception("Đang chờ đặt "+armedName+"; ESC trước.");
    RequestLayout(command);Message("Dàn trang — "+a[1]+": theo lời nhắc trên dòng lệnh; Esc huỷ. Kết quả hiện trên dòng lệnh.");}
   catch(Exception ex){Warn("Chưa thực hiện Dàn trang: "+ex.Message);}}
  // Read-only short guide (reused, disposed with the view).
  internal void ShowLayoutHelp(){if(layoutHelpDialog==null){layoutHelpDialog=Dialog("Dàn trang — "+LayoutHelpCaption.ToLowerInvariant(),S(360),S(330));var pane=Pane(new[]{-1,S(38)});
    layoutHelpText=new TextBox{Multiline=true,ReadOnly=true,ScrollBars=ScrollBars.Vertical,Dock=DockStyle.Fill,BackColor=CField,ForeColor=CFore,Font=TextFont,BorderStyle=BorderStyle.None,WordWrap=true,AccessibleName="Hướng dẫn Dàn trang",Text=LayoutHelpText};pane.Controls.Add(layoutHelpText,0,0);
    var close=MakeButton("Đóng");close.DialogResult=DialogResult.OK;pane.Controls.Add(close,0,1);layoutHelpDialog.Controls.Add(pane);layoutHelpDialog.AcceptButton=close;layoutHelpDialog.CancelButton=close;}
   layoutHelpText.SelectionStart=0;layoutHelpText.SelectionLength=0;Near(layoutHelpDialog);
   try{ShowModal(layoutHelpDialog);}catch(Exception ex){Warn("Chưa mở được hướng dẫn Dàn trang: "+ex.Message);}}
#endif
  internal void Act(string action){
#if MODELLIB019
   if(action=="modeladd"||action=="modelcopy"||action=="modelremove"){CloseMenus();if(Root==null)throw new Exception("Chưa có thư mục thư viện.");if(Request==null)throw new Exception("bảng chưa nối với AutoCAD.");
    Message(ModelInstruction(action));Request(action,null);return;}
   if(action=="delete"){CloseMenus();DeleteFromTool(contextTarget);return;}
#if LIBUPDATE023
   if(action=="update"){CloseMenus();UpdateFromModel(contextTarget);return;}
#endif
#if METADATA032
   if(action=="metadata"){CloseMenus();EditMetadata(contextTarget);return;}
#endif
#if CIRCUIT032
   if(CircuitUi032.Handles(action)){CloseMenus();if(Request==null)throw new Exception("bảng chưa nối với AutoCAD.");
    Message(action==CircuitUi032.Assign?"Chọn các block MEPS trên Model rồi Enter; hộp gán mã lộ sẽ mở. ESC huỷ, bản vẽ không đổi.":"Đang tìm ký hiệu MEPS chưa gán lộ trong Model (chỉ chọn, không sửa).");
    Request(action,null);return;}
#endif
#endif
   if(action=="import"||action=="legend"){
#if MODELLIB019
    CloseMenus();
#endif
    if(Root==null)throw new Exception("Chưa có thư mục thư viện.");if(Request==null)throw new Exception("bảng chưa nối với AutoCAD.");Request(action,null);return;}
   if(action=="reload"){Reload();Message("Đã tải lại thư viện.");return;}
   var e=Selected;if(e==null)throw new Exception("Chọn một ký hiệu trong danh sách.");
   if(action=="hide"){if(e.hidden)throw new Exception("Mục đã ẩn.");Library.SetHidden(Root,e.id,true);Reload();Message("Đã ẩn "+e.name+". Không xoá file nguồn, file thư viện hay khối đã đặt; chuột phải → Hiện mục đã ẩn để khôi phục.");return;}
   if(action=="restore"){if(!e.hidden)throw new Exception("Chuột phải → Hiện mục đã ẩn, chọn mục [ẩn] rồi khôi phục.");Library.SetHidden(Root,e.id,false);Reload();Message("Đã khôi phục "+e.name+".");return;}
   throw new Exception("Thao tác không hợp lệ: "+action);}
  protected override void Dispose(bool disposing){if(disposing){foreach(var b in cache.Values)b.Dispose();cache.Clear();tips.Dispose();options.Dispose();groups.Dispose();
#if MODELLIB019
    addMenu.Dispose();
#endif
#if LAYOUT030
    layoutMenu.Dispose();if(layoutHelpDialog!=null)layoutHelpDialog.Dispose();
#endif
    foreach(var f in new[]{setupDialog,customDialog,messagesDialog})if(f!=null)f.Dispose();
#if CABLES016
    // Unparented legacy ratio controls (not disposed by the control tree).
    settings.Dispose();ratio.Dispose();ratioLabel.Dispose();if(helpDialog!=null)helpDialog.Dispose();
#endif
   }base.Dispose(disposing);
#if UI031
   // 031: the fitted Semibold action font is owned here; released after the child controls that use it.
   if(disposing&&ActionFont!=null)ActionFont.Dispose();
#endif
  }
  // One library entry: rounded 32 px tile, 16 px icon at the left, exact block name in regular text (up to two lines, full name in the tooltip).
  internal sealed class Tile:Control {
   public readonly LibEntry Entry;readonly CompactView06 owner;public bool Selected,Listed=true;bool hot;
   public Tile(CompactView06 o,LibEntry e){owner=o;Entry=e;SetStyle(ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.UserPaint|ControlStyles.ResizeRedraw|ControlStyles.Selectable|ControlStyles.StandardClick|ControlStyles.StandardDoubleClick,true);
    Name="tile_"+e.id;Text=e.name;AccessibleName=e.name;AccessibleRole=AccessibleRole.ListItem;TabStop=false;Margin=new Padding(o.S(TileMargin));BackColor=CTile;ForeColor=e.hidden?CMuted:Meps05.PlaceIssue(e)!=null?CWarn:CFore;Font=o.TextFont;Cursor=Cursors.Hand;}
   internal bool IsArmed{get{return owner.Armed==Entry.id;}}
   internal Rectangle IconRect{get{int s=owner.S(Icon);return new Rectangle(owner.S(5),(Height-s)/2,s,s);}}
   internal Rectangle NameRect{get{int x=owner.S(5)+owner.S(Icon)+owner.S(5);return new Rectangle(x,owner.S(1),Math.Max(1,Width-x-owner.S(4)),Height-2*owner.S(1));}}
   internal string[] Lines{get{return CompactLibraryView05.NameLines((Entry.hidden?"[ẩn] ":"")+Entry.name,Font,NameRect.Width);}}
   protected override void OnPaint(PaintEventArgs pe){var g=pe.Graphics;g.Clear(Parent!=null?Parent.BackColor:CBack);g.SmoothingMode=SmoothingMode.AntiAlias;float w=Selected||IsArmed?2f:1f;
    using(var path=CompactLibraryView05.RoundRect(new RectangleF(w/2,w/2,Width-w-1,Height-w-1),owner.S(Radius))){using(var b=new SolidBrush(IsArmed?CArmed:CTile))g.FillPath(b,path);
     var mode=g.InterpolationMode;g.InterpolationMode=InterpolationMode.HighQualityBicubic;g.DrawImage(owner.Thumb(Entry),IconRect);g.InterpolationMode=mode;
     var r=NameRect;var lines=Lines;var flags=TextFormatFlags.EndEllipsis|TextFormatFlags.Left|TextFormatFlags.SingleLine|TextFormatFlags.NoPrefix|TextFormatFlags.NoPadding;
     if(lines.Length==1)TextRenderer.DrawText(g,lines[0],Font,r,ForeColor,flags|TextFormatFlags.VerticalCenter);
     else{int h=r.Height/2;TextRenderer.DrawText(g,lines[0],Font,new Rectangle(r.X,r.Y,r.Width,h),ForeColor,flags|TextFormatFlags.Bottom);TextRenderer.DrawText(g,lines[1],Font,new Rectangle(r.X,r.Y+h,r.Width,r.Height-h),ForeColor,flags|TextFormatFlags.Top);}
     using(var pen=new Pen(Selected||IsArmed?CTeal:hot?CMuted:CLine,w))g.DrawPath(pen,path);}
    if(Focused&&Selected)using(var dots=new Pen(CMuted){DashStyle=DashStyle.Dot})using(var inner=CompactLibraryView05.RoundRect(new RectangleF(3,3,Width-7,Height-7),Math.Max(1,owner.S(Radius)-2)))g.DrawPath(dots,inner);}
   protected override void OnMouseEnter(EventArgs e){hot=true;Invalidate();base.OnMouseEnter(e);}
   protected override void OnMouseLeave(EventArgs e){hot=false;Invalidate();base.OnMouseLeave(e);}
   protected override void OnMouseDown(MouseEventArgs e){owner.TileDown(this,e.Button);base.OnMouseDown(e);}
   protected override void OnMouseClick(MouseEventArgs e){base.OnMouseClick(e);owner.TileClick(this,e.Button);}
   protected override void OnMouseUp(MouseEventArgs e){base.OnMouseUp(e);owner.TileUp(this,e.Button,e.Location);}
   protected override void OnMouseWheel(MouseEventArgs e){owner.Wheel(e.Delta);base.OnMouseWheel(e);}
   protected override bool IsInputKey(Keys k){return k==Keys.Left||k==Keys.Right||k==Keys.Up||k==Keys.Down||k==Keys.Home||k==Keys.End||k==Keys.Enter||base.IsInputKey(k);}
   protected override void OnKeyDown(KeyEventArgs e){if(owner.Key(e.KeyCode)){e.Handled=true;e.SuppressKeyPress=true;}else base.OnKeyDown(e);}
   protected override void OnGotFocus(EventArgs e){Invalidate();base.OnGotFocus(e);}
   protected override void OnLostFocus(EventArgs e){Invalidate();base.OnLostFocus(e);}
  }
 }
#if MODELLIB019
 // TVXD-MEPS-LIBDELETE-021: confirmation of "Xóa khỏi công cụ" (palette right-click and the Model route), shown through the owner's ShowModal
 // (Application.ShowModalDialog in AutoCAD). Names the exact entry. Không is the default (focus and Enter) and Esc; closing the window also cancels.
 // Only Có returns Yes. Never confirms itself. Public members for UI QA.
 public sealed class ConfirmDeleteDialog021:Form {
  public readonly Label Info=new Label();public readonly Button Yes=new Button(),No=new Button();
  public static string Text021(string name,string id){return "Xoá "+name+" (mục "+id+") khỏi công cụ MEPS?\r\n\r\nĐây là XOÁ, không phải ẩn: mục không còn trong danh sách, tìm kiếm, nhóm, \"Hiện mục đã ẩn\" hay khôi phục.\r\n"+
   "Khối "+name+" đã đặt trong bản vẽ, file nguồn và file thư viện được GIỮ NGUYÊN; bảng ký hiệu của bản vẽ cũ vẫn nhận ra ký hiệu này.\r\nMuốn có lại đúng mục cũ: '+ → Thêm từ DWG/DXF' với đúng file nguồn cũ.";}
  public ConfirmDeleteDialog021(string name,string id){Text="MEPS — xoá khỏi công cụ";FormBorderStyle=FormBorderStyle.FixedDialog;MinimizeBox=false;MaximizeBox=false;ShowInTaskbar=false;StartPosition=FormStartPosition.CenterParent;
   Font=new System.Drawing.Font("Segoe UI",9f);AutoScaleMode=AutoScaleMode.Font;ClientSize=new Size(440,220);
   Info.Text=Text021(name,id);Info.Dock=DockStyle.Fill;Info.Padding=new Padding(10);Info.AccessibleName="Nội dung xác nhận";
   No.Text="Không";No.DialogResult=DialogResult.No;No.AutoSize=true;No.TabIndex=0;Yes.Text="Có, xoá khỏi công cụ";Yes.DialogResult=DialogResult.Yes;Yes.AutoSize=true;Yes.TabIndex=1;
   var bar=new FlowLayoutPanel{FlowDirection=System.Windows.Forms.FlowDirection.RightToLeft,Dock=DockStyle.Bottom,Height=40,Padding=new Padding(6)};bar.Controls.Add(No);bar.Controls.Add(Yes);
   Controls.Add(Info);Controls.Add(bar);AcceptButton=No;CancelButton=No;ActiveControl=No;Shown+=(s,e)=>No.Focus();}
 }
#endif
}
