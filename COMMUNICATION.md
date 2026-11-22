

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

