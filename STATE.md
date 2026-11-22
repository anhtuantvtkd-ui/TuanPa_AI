# TRẠNG THÁI DỰ ÁN (PROJECT STATE)
Cập nhật lần cuối: 2026-11-22 13:45 | Phụ trách điều phối: Gemini Argon & Codex

---

## 1. Mục tiêu vừa hoàn thành (Gemini đã thực hiện)
- **Đã xử lý triệt để 2 lỗi hình học của lệnh `T-TOADO`**:
  1. Hướng ngang / xiên (Q1, Q2, Q3, Q4): Nét gạch nằm xen giữa dòng X và dòng Y, không gạch xuyên qua chữ X, ổn định tuyệt đối khi xoay 360°.
  2. Hướng thẳng đứng 90°: Text tự động xoay 90° dọc theo đường gióng, dòng X ở bên trái, dòng Y ở bên phải, đường gióng chạy thẳng ở giữa (khớp 100% bản vẽ `Mat bang cap dien trung the.dwg` và ảnh `media_1795321591323_25784349.png`).
- **Đã kiểm thử thực tế trên AutoCAD engine (`accoreconsole.exe`)**: 6/6 trường hợp Explode bóc tách tọa độ đều PASS.
- **Đã biên dịch tự động (`build_v4.cmd`)**: Hợp nhất giữ nguyên vẹn 1.295 blocks/hàm của MEPS ra `TVXD.Meps.Merged.dll`.
- **Đã cập nhật live CAD**: `%LOCALAPPDATA%\TVXD\MEPS060\TVXD.Meps.dll`.
- **Đã đóng gói bản phát hành V7**: `D:\TuanPa_AI\OUTPUT\7. TVXD.BG V7` (293/293 files khớp SHA256 100%).
- **Hồ sơ nghiệm thu**: `work-items/TASK-0016.json` đã hoàn thiện và đạt chuẩn `RELEASED`.

---

## 2. Giao việc cho Codex (Pending Tasks Handed Over to Codex)

Theo phân vai tại `ROLES.md` và quy trình `WORKFLOW.md` (TUANPA-ENG-1), các công việc chưa hoàn thành hoặc cần Codex thực hiện trên repo gồm có 3 gói việc sau:

### Gói việc 1: Review độc lập (Independent Review) cho `TASK-0016` (Bản V7)
- **TASK_ID**: `TASK-0016`
- **Vai trò của Codex**: Reviewer độc lập (Gemini là Writer).
- **Phạm vi kiểm tra**:
  + Tệp đã sửa: `products/meps/src/Toado/ToadoPart.cs` (hàm `CreateOrUpdateLeader`).
  + Bản phát hành: `D:\TuanPa_AI\OUTPUT\7. TVXD.BG V7\MEPS060\TVXD.Meps.dll`.
  + Log kiểm toán: `SHA256SUMS.json` (293 files khớp 100%).
- **Hành động của Codex**:
  + Kiểm tra logic hình học `isVertical` và căn chỉnh `TextLocation` cho MLeader.
  + Xác nhận `verdict: "APPROVED"` và ghi nhận `independent: true` vào hồ sơ review để bảo đảm nguyên tắc 2 bên độc lập.

### Gói việc 2: Xử lý và chuẩn hóa các Task còn treo (`TASK-0011`, `TASK-0014`, `TASK-0015`)
- **TASK-0011** (*Import MEPS workspace snapshot*):
  + Tác giả ghi nhận là Codex (`writer: "Codex (GPT-6)"`), hiện vẫn ở trạng thái `IMPLEMENTING`.
  + Codex cần kiểm tra lại thư mục `products/meps/`, xác nhận snapshot integrity và bổ sung evidence để hoàn tất đóng task.
- **TASK-0014 & TASK-0015**:
  + Là các task trung gian của các đợt phát hành V5 và V6. Codex cần rà soát và đóng đồng bộ để lịch sử `work-items/` được khép kín gọn gàng.

### Gói việc 3: Khởi tạo & Triển khai tính năng tiếp theo trên Backlog (`TASK-0017`)
- **TASK_ID**: `TASK-0017`
- **Module**: `products/meps` (hạng mục 3: CAD Tooling trong `PRODUCT_BACKLOG.md`).
- **Nội dung công việc**:
  + Xây dựng lệnh xuất bảng tọa độ các đối tượng đã đánh nhãn `T-TOADO` ra bảng CAD (Table) hoặc tệp CSV/Excel (`T-TOADOXUAT` hoặc `T-TOADOTABLE`).
  + Tận dụng XData đã được gắn trên MLeader (`TVXD_TOADO`) để trích xuất danh sách gồm: Số hiệu, Tọa độ X, Tọa độ Y, Cao độ Z.
- **Quy chuẩn thực thi**:
  + Tuân thủ `WORKFLOW.md`: Tạo `work-items/TASK-0017.json` từ `TASK.template.json`.
  + Chỉ viết mã ngắn/module mới trong `products/meps/src/Toado/`.
  + Biên dịch qua `build_v4.cmd`, kiểm thử bằng `accoreconsole`, đóng gói kế tiếp vào `D:\TuanPa_AI\OUTPUT\8. TVXD.BG V8`.

---

## 3. Lệnh kiểm tra nhanh cho Codex khi nhận repo
```powershell
Set-Location 'D:\TuanPa_AI'
# 1. Kiểm tra tính hợp lệ toàn bộ hồ sơ workflow:
node workflow-check.mjs

# 2. Chạy toàn bộ 19 test tự động:
node --test services.test.mjs workflow-check.test.mjs

# 3. Biên dịch lại toàn bộ sản phẩm:
cmd.exe /c "cd /d D:\TuanPa_AI\products\meps\tools && build_v4.cmd"
```
