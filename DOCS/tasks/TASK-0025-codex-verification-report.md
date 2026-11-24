# TASK-0025 — Báo cáo kiểm chứng và nghiệm thu của Codex

## Kết luận

**Chưa đạt nghiệm thu toàn diện.** DLL V4 ráp thành công. Cả 10 mục menu thực tế gửi đúng mã đúng một lần đến `RequestRouteStats` trong WinForms. Tuy nhiên, `T-KLHD` bị từ chối ở tầng dispatch từ Palette trong cả bản nền lẫn bản mới. Bố cục vẫn có nhãn bị cắt. Renderer đã có badge và hover, nhưng phạm vi áp dụng và khả năng thích ứng chưa đạt đề xuất thiết kế. Kiểm chứng nghiệp vụ trong AutoCAD 2021 và phê duyệt của chủ sở hữu còn mở.

## Verification Status: RESOLVED

Kiểm chứng bổ sung ngày 24/11/2026. Codex (GPT-6) đối chiếu commit `5bdd390eb926dfe39b591bdfdf9168da9cc35e16` với `8ffaa23`. Trạng thái này áp dụng cho **F1, lỗi whitelist chặn T-KLHD**. Các kết quả trước đây ở trên mô tả DLL cũ.

| Hạng mục | Kết quả |
|---|---|
| IL của `RouteStats076::.cctor` | Build cô lập qua bước ILAsm, merger báo `RouteStats076 (1)`. ILDasm trên DLL vừa ráp và DLL release đều xác nhận `ldc.i4.8` cấp phát `string[8]`. Bảy phần tử cũ giữ index 0–6. Sau khi ghi CDS ở index 6, đoạn `ldloc.0 → ldc.i4.7 → ldstr "T-KLHD" → stelem.ref` ghi index 7 trước `stsfld Commands`. Độ sâu đoạn chèn là 0 → 1 → 2 → 3 → 0, đúng `.maxstack 3`. |
| `Allowed` trong AutoCAD 2021 Core Console R.47.0.0 | Gọi trên bản sao DLL release. `T-KLHD` và cả bảy mục whitelist cũ đều trả `True`. `t-klhd`, mã ngoài danh sách và `null` trả `False`, đúng so sánh ordinal. |
| Dispatch và LISP | IL giữ đường `Plugin.RouteStatsAction → Queue → Ready/Dispatch`. Sau khi request sẵn sàng và hash LISP khớp, `Dispatch` gửi `"T-KLHD "` bằng `SendStringToExecute(..., false, false, true)`. Trong Core Console trên bản vẽ trắng, phép thử nạp LISP release và dựng request sẵn sàng bằng reflection, rồi gọi `Dispatch` thật. Trace ghi `ROUTE-SENT=True`, `LispWillStart` ghi `(C:T-KLHD)`, và AutoCAD hiện lời nhắc chọn đối tượng của `c:BKL_CAP`. Console được dừng sau khi ghi nhận lời nhắc. Phép thử gọi trực tiếp `T-KLHD` riêng cũng vào `c:BKL_CAP` và báo bản vẽ trắng không có đối tượng phù hợp. |
| Chín lệnh gốc | ILDasm trước/sau khớp ở constructor menu, `RouteClicked`, `RouteStatsAction`, `Allowed`, `Queue`, `Ready` và `Dispatch`. Sáu lệnh RouteStats gốc vẫn được phép. Ba nhánh riêng giữ nguyên thân entry point: `STTPHA → NumberPhase076.Queue`, `CDS → CdsTable155.Start` và `T-VEDAYPL → VedayPl155.Start`. CDS cũng giữ index 6 trong whitelist. |

SHA-256 DLL release đã kiểm là `21236A4BE3B44543815399FF29D951DA6DCCC93E250AD356B07B939F90273BB5`, khớp manifest. Build và thử CAD dùng bản sao cô lập. `git diff HEAD -- products/meps/release` rỗng.

`npm.cmd run type-check` đạt. `npm.cmd test` qua 19/19 unit test và trả exit 1 ở workflow checker do các hồ sơ TASK-0018, TASK-0019 và TASK-0020.

Nghiệm thu toàn TASK-0025 vẫn **BLOCKED**. Phép thử dispatch dùng request sẵn sàng, nên lượt bấm Palette với chu trình nạp LISP từ đầu, thực thi nghiệp vụ của mười mục trên fixture, các finding F2–F5 và phê duyệt chủ sở hữu vẫn cần xử lý. Điểm dò IL trong merger vẫn phụ thuộc văn bản IL nền. Khi thay IL nền, kiểm số lần vá, mảng sau khi ráp và kết quả dispatch trên fixture.

## Phạm vi và truy xuất

- Reviewer độc lập trong phiên kiểm chứng là Codex (GPT-6). Commit triển khai `4864292eb24a69bcf0b2e6763a9c1762db71a263`, base triển khai `e89e5b943a2a94593f95c93c208731b9ee33fd99`. Working tree sản phẩm lúc kiểm tại main `d06a5bb475d386d107accb0d8efc224dd80f6392`, đồng nhất với commit triển khai trong `products/meps`. Hồ sơ kiểm chứng được ghi trên nhánh riêng.
- Nguồn đặc tả [TASK-0025-spec.md](TASK-0025-spec.md) và bản [đề xuất thiết kế Codex](TASK-0025-codex-review.md). Đối chiếu [ModernThemeEngine.cs](../../products/meps/src/Palette/ModernThemeEngine.cs), [merge_toado.cs](../../products/meps/tools/merge_toado.cs), [build_v4.cmd](../../products/meps/tools/build_v4.cmd), IL nền và hai DLL trong Git.
- SHA-256 DLL nền `E7C32986376D8172DC9776799D08C1CEEC1F810D966E5B24FDD94DADFA74D6D5`. SHA-256 DLL kiểm `D2C1D4E9ED4CCCBD29151A7B44E1782DCBD146D6E0EC9B2FBC57CE7280454B10`. SHA-256 IL nền `0D026706EF42AC216B59E0BAC913B878973A2B87CF7E94AE066B666DA7D01BDD`.

## 1. Mười lệnh và callback

So sánh IL từ hai DLL bằng ILDasm. Constructor tạo menu, mảng `RouteActions`, closure `<.ctor>b__c3`, `RouteClicked`, `Plugin.RouteStatsAction`, `RouteStats076.Queue`, `Allowed` và bộ khởi tạo danh sách lệnh giữ nguyên từng thân phương thức. Constructor gán mã vào `Tag`, đăng ký `Click` trên từng item và giữ đường đi qua `RouteClicked(string)` tới `RequestRouteStats`. Đọc thêm [IL nền](../../products/meps/tools/base/meps_orig.il#L51316) và [dispatch](../../products/meps/tools/base/meps_orig.il#L414562).

Thử trực tiếp DLL mới trong tiến trình STA WinForms. Khởi tạo `CompactView06`, gán delegate ghi nhận vào `RequestRouteStats`, duyệt `routeMenu.Items` và gọi `PerformClick()` mỗi mục một lần. Cột cuối mô tả đường đi từ IL; thử delegate dừng trước xử lý CAD.

| Mục | Mã nhận và số lần gọi | Đường đi phía CAD |
|---|---|---|
| STTPHA | STTPHA × 1 | `NumberPhase076.Queue` |
| T-THONGSO | T-THONGSO × 1 | `RouteStats076.Queue`, có trong danh sách cho phép |
| T-KIEMTRA | T-KIEMTRA × 1 | `RouteStats076.Queue`, có trong danh sách cho phép |
| T-THCS | T-THCS × 1 | `RouteStats076.Queue`, có trong danh sách cho phép |
| T-THDIEN | T-THDIEN × 1 | `RouteStats076.Queue`, có trong danh sách cho phép |
| T-VEDAYCS | T-VEDAYCS × 1 | `RouteStats076.Queue`, có trong danh sách cho phép |
| T-VEDAYPL | T-VEDAYPL × 1 | `VedayPl155.Start` |
| T-DIMNET | T-DIMNET × 1 | `RouteStats076.Queue`, có trong danh sách cho phép |
| CDS | CDS × 1 | `CdsTable155.Start` |
| T-KLHD | T-KLHD × 1 | `RouteStats076.Queue` từ chối vì thiếu trong danh sách cho phép |

**F1 · Cao · Mở.** `T-KLHD` hiện diện trong cả hai menu, nhưng `RouteStats076.Queue` gọi `Allowed` và ném ngoại lệ trước khi điều phối nếu mã bị từ chối. `RouteClicked` bắt ngoại lệ và gọi `Warn`. File LISP có `c:T-KLHD` gọi `c:BKL_CAP`, nên entry point nhập trực tiếp sau khi nạp LISP và việc bấm mục Palette là hai đường kích hoạt cần thử riêng. Lỗi dispatch đã có ở base. [Queue](../../products/meps/tools/base/meps_orig.il#L653576) · [Danh sách cho phép](../../products/meps/tools/base/meps_orig.il#L655674) · [LISP](../../products/meps/release/TVXD.BG_V4/MEPS060/RouteStats076.lsp#L4987)

**Event thay đổi.** Handler nghiệp vụ `Click` và callback gốc giữ nguyên theo IL. Mã mới thêm `Menu_Opening` vào năm menu bằng cặp gỡ/gắn cùng delegate, để đặt lại kích thước item khi mở. IL của `ArrangeActions` thay đổi để gọi `ApplyTheme` ở cuối nhánh layout chạy qua điểm chèn. Khả năng mở bằng chuột/phím và phục hồi focus trong host cần kiểm riêng.

## 2. Bố cục và chữ bị cắt

**F2 · Cao · Mở.** Route và Chuẩn hóa dùng cả hàng, tám nút giữa vẫn chia hai cột. `ApplyTheme` cấp kích thước theo pixel với lề/khe 4 px, hàng 32 px (Route 36 px) và font Segoe UI Bold 7,2 hoặc 8,2 pt. Hook chạy sau logic bố cục cũ và ghi đè bounds/font cho các nút đó. `RoundButton.OnPaint` vẫn dùng cờ `EndEllipsis` (`0x8825` gồm `0x8000`). Không có đo nhãn để chọn số cột hay tăng chiều cao theo chữ. [Layout mới](../../products/meps/src/Palette/ModernThemeEngine.cs#L241) · [OnPaint](../../products/meps/tools/base/meps_orig.il#L76697)

Đo bằng `TextRenderer.MeasureText(button.Text, button.Font)` trên chính control của DLL ở 96 DPI, sau khi đặt `layoutHost.Width` và gọi `ArrangeActions`. Số liệu là chiều rộng nút / độ rộng đo được của nhãn, tính bằng px, trước khi trừ thêm khoảng trống của vùng vẽ.

| Nút | Host 208 px | Host 176 px |
|---|---:|---:|
| Thống kê tuyến | 200 / 130 | 168 / 130 |
| Danh mục & in | 98 / 93 | **82 / 93** |
| Ghi chú MTEXT | **98 / 99** | **82 / 99** |
| Bố trí phòng | 98 / 93 | **82 / 93** |
| Chuẩn hóa tiêu chuẩn | 200 / 159 | 168 / 159 |
| Vẽ tuyến ống gió mềm | **60 / 142** | **60 / 142** |

`flexBar` được đổi nhãn/font nhưng khối mới không đặt lại bounds. Control thực tế trong phép thử dùng bounds 60×26. Layout mới đặt cao 232 px khi có đủ nút, còn merge nâng hằng số section `0xba` thành `0xf0` (186 → 240). Các cải thiện về màu sắc, nhãn và hai hàng rộng đã hiện diện. Mã hiện tại vẫn thiếu ba card chức năng, bảy hàng toàn chiều rộng và xử lý rail mà bản thiết kế đề xuất. Excel và Input vẫn tách riêng, tương tự Dàn trang với Danh mục và TAG với MTEXT.

## 3. Dropdown hiện đại

- **Đạt một phần.** Renderer làm nền tối, màu hover/selected, badge bo góc và màu theo tiền tố. Trên `routeMenu` thật ở 96 DPI, cả 10 mã được tách từ caption bằng dấu em dash, badge rộng 74 px và phần mô tả được cấp 222 px. Các mô tả đo từ 67,9 đến 201,6 px ở Segoe UI 9 pt đậm, đủ chỗ tại cấu hình đo. Phép đo chưa xác nhận ảnh vẽ khi mở popup trên màn hình.
- **F3 · Trung bình · Mở.** Chỉ gắn renderer theo tên field `routeMenu`, `layoutMenu`, `sdnlMenu`, `tagMenu`, `drawingIndexMenu`. Excel và submenu không có bước cấu hình riêng. Mọi item cấp ngoài bị ép 320×32 px, kể cả separator nếu có. Popup không đo theo font/DPI thực, không tăng chiều cao khi wrap và không chọn hướng mở theo vùng làm việc. Cách tách caption dựa trên dấu phân cách và giới hạn 14 ký tự có thể nhận sai badge. Chiều rộng mô tả còn trừ cả offset tuyệt đối `rect.X`, và renderer lấy `ContentRectangle` thay cho `e.TextRectangle` nên chưa dành riêng chỗ cho arrow/icon. Viền popup vẽ bằng `DrawRectangle`. Badge và nền hover có bo góc, chưa có triển khai bo vùng popup. [Renderer](../../products/meps/src/Palette/ModernThemeEngine.cs#L30) · [Gắn menu](../../products/meps/src/Palette/ModernThemeEngine.cs#L276)
- **F4 · Trung bình · Mở.** Tắt image/check margin và không có phần vẽ thay thế hoặc High Contrast fallback riêng. Màu chữ tự vẽ không xét `Enabled`. Font dùng riêng trong `MeasureString` mỗi lần vẽ không được dispose chủ động. `ApplyTheme` tạo font và renderer mới mỗi lần bố trí. Chưa đo tăng tài nguyên GDI khi resize hoặc mở menu lặp lại. [Vẽ chữ](../../products/meps/src/Palette/ModernThemeEngine.cs#L73)

## 4. Build, bằng chứng và cổng nghiệm thu

| Kiểm tra | Kết quả |
|---|---|
| `git diff --stat e89e5b9 4864292`, `git diff --name-status 4864292 HEAD -- products/meps` | Xác lập phạm vi và đối chiếu working tree sản phẩm với commit triển khai |
| `build_v4.cmd` trên bản sao cô lập dưới `TEST/` với .NET Framework C# 5, SDK ILDasm 4.8 và tham chiếu AutoCAD 2021 | Exit 0. Biên dịch bốn phần, ráp DLL, hook 1 lần, chèn theme 1 lần và đổi height 1 lần. Log ghi 17 lượt thay dòng title và 1 lượt đổi caption. DLL thử SHA-256 `414D5E4AD1A132027E92A6BB199600588E0F3C603C0382B3316D35D900E122F1`. Bản sao chỉ có nguồn/tệp metadata cần cho build, nên checksum generator cảnh báo và bỏ các tệp payload vắng mặt. Hai lần ráp cô lập cho hash khác nhau; hash DLL đã commit được ghi riêng ở đầu báo cáo. |
| ILDasm trên DLL base và DLL commit | Khớp chuỗi handler/dispatcher đã liệt kê. Xác nhận hook theme có trong DLL commit. |
| WinForms STA, `CompactView06` từ DLL commit, `PerformClick`, `TextRenderer.MeasureText`, `Graphics.MeasureString` | 10/10 truyền mã. Có nhãn nút vượt rộng tại 96 DPI. Cả 10 mô tả Route nằm trong vùng rộng đo được. |
| `npm.cmd test` | 19/19 unit test qua. Workflow checker đi kèm trả exit 1 do hồ sơ TASK-0018, TASK-0019 và TASK-0020. |
| `npm.cmd run type-check` | Pass. Tái dùng dependencies có sẵn. |
| Hành vi CAD và hình ảnh native | Chưa nghiệm thu. Phép thử gọi `Allowed` ngoài AutoCAD gặp lỗi khởi tạo module Autodesk tại `AcPlPlotReactor`; phân tích danh sách cho phép dựa trên IL hai DLL. Chưa ghi nhận một lượt thực thi mười lệnh trong AutoCAD trên fixture. |

**F5 · Trung bình · Mở.** `merge_toado.cs` dò điểm chèn bằng chuỗi IL và chỉ in số lần thay/chèn, thiếu điều kiện dừng khi hook/theme/height không khớp. Các đoạn thay caption quét cả file IL. `build_v4.cmd` thiếu kiểm tra ngay sau compile merger, copy DLL, compile/chạy updater. Có thể tới banner thành công khi các bước đó lỗi. Updater cũng bỏ các file thiếu khỏi manifest; đã quan sát trong bản sao tối giản. [Merge](../../products/meps/tools/merge_toado.cs#L131) · [Build](../../products/meps/tools/build_v4.cmd#L68) · [Updater](../../products/meps/tools/update_v4.cs#L59)

## 5. Điều kiện kiểm lại

1. Sửa đường gọi Palette cho `T-KLHD`. Trong AutoCAD 2021 trên bản vẽ tổng hợp, kiểm từng lệnh bằng menu và entry point gốc phù hợp. So đầu ra, điều kiện Model/Paper, chọn và hủy, chuyển focus, Undo và xác nhận mỗi lần bấm chạy đúng một lần.
2. Kiểm bố cục tại bề rộng dock tối thiểu, 176 và 208 px ở 96 DPI, cùng các tỷ lệ 125%, 150% và 200% trên host. So toàn bộ nhãn với vùng vẽ, gồm Flex. Thử dock hai bên, floating và đổi section.
3. Mở từng menu bằng chuột và bàn phím, thử các mép màn hình và màn hình phụ. Xác nhận badge, mô tả, separator, submenu, icon/check, disabled, High Contrast, Esc/Enter và focus. Kiểm vòng đời font/GDI khi lặp mở menu và resize.
4. Chạy build/checksum đầy đủ trên gói cô lập, xử lý các lỗi workflow đang mở và ghi bằng chứng native cùng phê duyệt của chủ sở hữu trên đúng commit dự kiến giao.

## Tổng kết kiểm chứng

Task và commit kết quả sản phẩm là TASK-0025 và `4864292eb24a69bcf0b2e6763a9c1762db71a263`. Codex thực hiện review và ghi hồ sơ. Phép thử WinForms tách được lỗi dispatch có từ trước khỏi thay đổi UI. Bài học đã có bằng chứng là cần kiểm cả hai ranh giới: từ item tới callback và từ callback tới bộ điều phối. Test hồi quy cần kiểm `T-KLHD` từ Palette trên fixture CAD và đo nhãn thật trên control theo các cấu hình được hỗ trợ. Phiên review chỉ ghi tài liệu và hồ sơ; các sửa đổi sản phẩm, fixture CAD mới và nghiệm thu chủ sở hữu còn mở.
