# TRẠNG THÁI DỰ ÁN (PROJECT STATE)
Cập nhật lần cuối: 2026-11-22 18:50 | Phụ trách điều phối: Gemini Argon & Codex

---

## 1. Mục tiêu vừa hoàn thành: Phát hành chính thức phiên bản TVXD.BG V8
Chủ sở hữu (Phùng Anh Tuấn) đã trực tiếp nghiệm thu và chỉ đạo xuất bản phiên bản mới:
> *"Đến đoạn này xuất bản đi đã gần đúng hết ý tôi rồi. Xuất bản thành 1 V mới đi"*

- **3 cải tiến kỹ thuật đột phá trong bản V8**:
  1. **Kích hoạt AutoCAD Dogleg Shoulder (`EnableDogleg = true`, `DoglegLength = s.Height * 1.5`)**: Người dùng tự do kéo các điểm grip của nhãn để né vật cản / tránh đè lên các block hoặc đường nét khác mà **không bị sụp đổ thành một đường thẳng chéo**. Nhãn luôn duy trì vai gấp khúc ngang chuẩn AutoCAD.
  2. **Bỏ hoàn toàn nền che trắng/xám (`BackgroundFill = false`)**: Chữ số tọa độ hiển thị trong suốt hoàn toàn, trả lại nền đen tự nhiên của AutoCAD theo đúng yêu cầu *"TÔI KHÔNG CẦN CHỮ NỀN TRẮNG"*.
  3. **Khóa cố định đường gạch phân cách giữa X và Y**: Sử dụng `AttachmentHorizontal` và `AttachmentBottomOfTopLine` cho cả LeftLeader và RightLeader. Khi xoay hoặc flip qua trục đứng/ngang, đường gạch luôn nằm vững chắc ở giữa dòng X và dòng Y, **không bao giờ bị nhảy đổi sang gạch chân dưới tọa độ Y**.
- **Kiểm thử Native Engine (`accoreconsole.exe`) trên bản vẽ thực tế `Mat bang cap dien trung the.dwg`**:
  + `DetailedMatrixTest.cs`: 16/16 góc xoay 360° tỏa tròn quanh block đều **PASS 100%**, độ trôi vị trí qua các lần làm mới (refresh) bằng 0 tuyệt đối (`detailed_matrix_test_report.txt`).
  + `GripDragTest.cs`: 6/6 trường hợp kéo grip né vật cản (Q1 $\to$ Q2 $\to$ Q3 $\to$ Q4, xiên lệch trục) đều **PASS 100%** (`grip_drag_test_report.txt`).
- **Biên dịch & Đóng gói sản phẩm**:
  + Biên dịch hợp nhất nguyên vẹn 1.295 MEPS types qua `build_v4.cmd`. Mã băm SHA-256 của `TVXD.Meps.dll`: `9d44094137d9d3ddde71eb2c62b6ed99f3cdc6b38b2155a2010b328985ffef47`.
  + Tệp kiểm toán `SHA256SUMS.json`: 293/293 files khớp 100% (0 sai lệch).
  + Thư mục phát hành chính thức: [8. TVXD.BG V8](file:///D:/TuanPa_AI/OUTPUT/8.%20TVXD.BG%20V8).
  + Live runtime đã cập nhật: `%LOCALAPPDATA%\TVXD\MEPS060\TVXD.Meps.dll`.
- **Hồ sơ nghiệm thu**: `work-items/TASK-0016.json` đã hoàn thiện và đạt chuẩn `RELEASED`.

---

## 2. Kế hoạch tiếp theo (Next Steps / Backlog)

1. **TASK-0017 (CAD Tooling Backlog)**:
   - Phát triển lệnh `T-TOADOXUAT` (hoặc `T-TOADOTABLE`): Đọc XData `TVXD_TOADO` từ các nhãn đã gắn trong bản vẽ để xuất tự động ra bảng tọa độ CAD (Table) hoặc tệp Excel/CSV.
2. **Khép kín hồ sơ cũ (`TASK-0011`, `TASK-0014`, `TASK-0015`)**:
   - Hoàn tất evidence và đóng task theo quy trình `WORKFLOW.md`.

---

## 3. Lệnh kiểm tra nhanh
```powershell
Set-Location 'D:\TuanPa_AI'
# 1. Kiểm tra tính hợp lệ toàn bộ hồ sơ workflow:
node workflow-check.mjs

# 2. Chạy toàn bộ 19 test tự động:
node --test services.test.mjs workflow-check.test.mjs

# 3. Biên dịch lại toàn bộ sản phẩm:
cmd.exe /c "cd /d D:\TuanPa_AI\products\meps\tools && build_v4.cmd"
```
