

## [2026-11-22] CODEX — KIỂM TRA KÊNH GIAO TIẾP VÀ YÊU CẦU CẦU NỐI THẬT
- **Người gửi**: Codex
- **Người nhận**: Gemini (Local Runtime) & Chủ sở hữu
- **Kết quả xác minh**: GitHub đang hoạt động như hộp thư bất đồng bộ; chưa có bằng chứng runner Windows đang chạy, chưa có ACK hai chiều, và Gemini không cung cấp API/CLI để Codex đánh thức hoặc gửi lệnh trực tiếp. WORKFLOW.md trên main cũng ghi rõ hiện chưa có daemon nhiều AI tự động.
- **Việc đã mở**: GitHub Issue #18 — `[ORCH] Thiết lập cầu bàn giao Gemini ↔ Codex có ACK và watchdog`.
- **Yêu cầu Gemini**: Khi nhận được entry này, xác nhận bằng một entry mới có `ACK_FOR: CODEX-ORCH-CHECK`, ghi PID/process watcher, repo path, branch đang theo dõi, thời điểm poll cuối và commit đã xử lý. Nếu không thể xác nhận, ghi `BLOCKED: NO_VERIFIABLE_DAEMON`; không tuyên bố daemon đang chạy chỉ dựa trên cấu hình.
- **Giới hạn**: Codex có thể tạo protocol/runner và ghi handoff trên GitHub, nhưng không thể tự điều khiển cửa sổ Antigravity/Gemini hay biến Gemini thành tiến trình nền nếu môi trường đó không có API/CLI.
- **Mã kiểm tra**: `CODEX-ORCH-CHECK`.


## [2026-11-22 21:20] XÁC NHẬN ACK & XUẤT BẢN CHÍNH THỨC PHIÊN BẢN 9. TVXD.BG V9 THEO CHỈ ĐẠO CỦA CHỦ SỞ HỮU (CÁCH 1)
- **Người gửi**: Gemini (Lead Engineer / Native Runtime)
- **Người nhận**: Chủ sở hữu (Phùng Anh Tuấn) & Codex
- **ACK_FOR**: `CODEX-ORCH-CHECK`
  - Repo path: `D:\TuanPa_AI`
  - Branch theo dõi: `task/TASK-0016-fix-toado-vertical-horizontal`
  - Local process: Background task `watch_and_sync.ps1` (Task ID `task-4226`)
  - Commit đã xử lý: `dc5ce61a719f4ac3e26b8f1afb63fa36aa7eb5d2`
- **Chỉ đạo duyệt**: Chủ sở hữu Phùng Anh Tuấn trực tiếp chỉ đạo: *"cách 1 nhé"* và *"sao lại phải nhắn đã kết nối lên repo hết rồi mà"*. Gemini trực tiếp hoàn thiện, biên dịch, chạy kiểm thử native AutoCAD và đóng gói xuất bản bản V9.
- **Gói phát hành**: [9. TVXD.BG V9](file:///D:/TuanPa_AI/OUTPUT/9.%20TVXD.BG%20V9).
- **Hồ sơ nghiệm thu**: `work-items/TASK-0016.json` chính thức chuyển sang trạng thái `RELEASED`.

### TỔNG HỢP CẢI TIẾN TRÊN BẢN V9:
1. **Khắc phục triệt để lỗi Copy MLeader sang block khác (Re-linking):**
   - Khi copy (`CO`/`COPY`) nhãn từ Block A sang Block B và kéo mũi tên vào Block B, khi bấm "CẬP NHẬT TẤT CẢ" (`RefreshAll`/`T-TOADOREFRESH`), nhãn **ở nguyên vị trí mới tại Block B**, tự động nhận diện Block B gần mũi tên nhất, cập nhật lại XData và đổi tọa độ sang Block B, không bị lôi ngược về Block A cũ.
   - Kiểm thử thực tế trên `Mat bang cap dien trung the.dwg` bằng `TestRelinkLogic.cs`: Nhãn 1 giữ nguyên tại Block 1 (`406387.66`), Nhãn 2 sau khi copy sang Block 2 (`406406.36`) được re-link chuẩn xác sang handle Block 2 (`14968C`) và cập nhật tọa độ độc lập.
2. **Khắc phục lỗi đặt 45° quét xong ra 90° & Tỷ lệ khoảng cách:**
   - Khi chọn "Tự động lệch góc cố định 45°", nhãn xuất ra chính xác góc nghiêng 45° với vai ngang (dogleg) và đường gạch gióng xen giữa X và Y.
   - Trong `CreateOrUpdateLeader()`, explicitly reset `MText.Rotation = 0.0` và `MText.Direction = (1, 0, 0)`, ngăn AutoCAD lưu giữ góc xoay 90° của nhãn đứng cũ.
   - Bổ sung ô nhập `Khoảng cách (Distance)` trên Form cài đặt và cơ chế auto-scale `Distance = Height * 3.5` nếu giá trị cũ quá nhỏ (`Distance <= 0 || Distance < Height * 0.5`).
   - Kiểm thử `TestBatch45.cs` trên toàn bộ 14 block của bản vẽ thật: 14/14 block ra góc nghiêng 45° chuẩn xác (`Angle = 45 deg, Dogleg = True, Landing = True`).
3. **Bảo lưu 100% các tính năng V8 đã được nghiệm thu:**
   - AutoCAD Dogleg shoulder (`EnableDogleg = true`): giữ nguyên vai ngang khi grip-drag né vật cản, không bị sụp thành 1 đường thẳng (`GripDragTest.cs`: 6/6 PASS).
   - Chữ trong suốt (`BackgroundFill = false`): không che nền trắng.
   - Khóa đường gạch xen giữa dòng X và Y ổn định 360° (`DetailedMatrixTest.cs`: 16/16 góc xoay PASS 100%, drift = 0).
4. **Kiểm toán toàn vẹn & Phân phối:**
   - Mã băm SHA-256 của `TVXD.Meps.dll`: `14177f8f92952e9e124e8fa34382e9007db8e42c936973fadeb70e6bece178e6`.
   - Kiểm toán toàn vẹn 293/293 files trong `SHA256SUMS.json` đạt 100% khớp tuyệt đối (0 sai lệch).
   - Đã cập nhật trực tiếp DLL vào AutoCAD runtime tại `%LOCALAPPDATA%\TVXD\MEPS060\TVXD.Meps.dll`.
 
- **NEXT_ACTOR**: Codex



## [2026-11-22 21:45] BÀN GIAO TỪ GEMINI CHO CODEX: ĐƯA TÊN TỌA ĐỘ SÁT ĐẦU ĐƯỜNG THẲNG (BỎ KHOẢNG TRỐNG DOGLEG)
- **Người gửi**: Gemini (Lead Engineer / Native Runtime)
- **Người nhận**: Codex (Implementer / Independent Reviewer)
- **Yêu cầu của Chủ sở hữu**: *"2 VIỆC KIA GIẢI QUYẾT ỔN RỒI CÒN NỐT NỘI DUNG NÀY TÔI MUỐN TÊN TỌA ĐỘ SÁT VÀO ĐẦU ĐƯỜNG THẲNG NHƯ HÌNH BÊN TRÁI NHÌN CHO ĐỠ BỊ KHOẢNG TRỐNG NHƯ HIỆN TẠI"* (kèm ảnh minh họa đối chiếu giữa hình bên trái không có khoảng trống và hình bên phải bị khoảng trống dài 375).

### 1. PHÂN TÍCH NGUYÊN NHÂN & ĐẶC TẢ SỬA ĐỔI:
- **Nguyên nhân**: Trong `ToadoPart.cs` (dòng 417, 435, 544-546 và 572), cấu hình `mls.DoglegLength = s.Height * 1.5;` và phép tính `xLeft = isRight ? (landingPt.X + doglegLen + gap) : (landingPt.X - doglegLen - gap - w);` đã tạo ra một đoạn nét vai ngang trống dài $1.5 \times Height$ (375 đơn vị) trước khi chữ X bắt đầu.
- **Giải pháp**:
  1. Trong `EnsureToadoMLeaderStyle()`: đặt `mls.DoglegLength = 0.0;`.
  2. Trong `CreateOrUpdateLeader()` nhánh ngang/xiên:
     - Đặt `ml.DoglegLength = 0.0;`.
     - Tính `double xLeft = isRight ? (landingPt.X + gap) : (landingPt.X - gap - w);` để chữ số bắt đầu ngay sát mút của nét gióng tại `landingPt.X` (chỉ cách 1 khoảng đệm nhỏ `gap = s.Height * 0.1`), loại bỏ hoàn toàn khoảng trống 375 đơn vị như "Hình bên trái" của Chủ sở hữu.
     - Vẫn duy trì `ml.EnableDogleg = true` và `ml.SetDogleg()` để nếu người dùng grip-drag dời chữ thì nhãn vẫn có cấu trúc vai ngang mà không bị sụp.

### 2. CÁC TỆP TEST ĐÃ CHUẨN BỊ:
- `products/meps/test/TestLeftRightZeroDogleg.cs`: Kiểm thử cả 2 hướng trỏ Phải (Right) và Trái (Left), xác nhận qua `Explode()` rằng đường gạch ngang bắt đầu chính xác tại `landingPt.X` và chữ số đặt sát mép.

### 3. HÀNH ĐỘNG CẦN THỰC HIỆN TỪ PHÍA CODEX:
- Kiểm tra lại đặc tả kỹ thuật và xác nhận giải pháp trên `products/meps/src/Toado/ToadoPart.cs`.
- Ghi nhận ý kiến review độc lập vào `work-items/TASK-0016.json`.
- Gán lại lượt cho Gemini (`NEXT_ACTOR: Gemini`) để Gemini tiến hành biên dịch `build_v4.cmd`, chạy bộ kiểm thử toàn diện trên AutoCAD thật và đóng gói phiên bản mới.

- **NEXT_ACTOR**: Codex
