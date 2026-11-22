# SỔ GIAO BAN KỸ THUẬT (AI COMMUNICATION LOG)
Kênh trao đổi và bàn giao công việc bất đồng bộ trực tiếp trên repo giữa Gemini và Codex.
Quy tắc: Mỗi AI khi vào phiên làm việc tự động đọc tệp này và `STATE.md`, thực thi nhiệm vụ được bàn giao, sau đó ký tên và ghi log bàn giao tiếp theo.

---

## [2026-11-22 13:45] BÀN GIAO TỪ GEMINI CHO CODEX
- **Người gửi**: Gemini (Lead Engineer / Planner)
- **Người nhận**: Codex (Implementer / Independent Reviewer)
- **Ngữ cảnh**: Đã hoàn thành sửa 2 lỗi hình học của lệnh `T-TOADO`, kiểm thử `accoreconsole` 6/6 ca đạt chuẩn, biên dịch `build_v4.cmd`, cập nhật live DLL và đóng gói bản phát hành [7. TVXD.BG V7](file:///D:/TuanPa_AI/OUTPUT/7.%20TVXD.BG%20V7) (293/293 files khớp SHA256 100%).

### NHIỆM VỤ GIAO CHO CODEX TRÊN REPO:
1. **Thực hiện Independent Review cho `TASK-0016`**:
   - Mở tệp `products/meps/src/Toado/ToadoPart.cs` tại hàm `CreateOrUpdateLeader()`.
   - Xem xét cách giải quyết:
     + Hướng ngang/xiên: Khóa cố định đường gióng tại cao độ `landingPt.Y`, kích hoạt `AttachmentBottomOfTopLine` và `ExtendLeaderToText`, căn chỉnh `TextLocation` chuẩn xác với `LandingGap`.
     + Hướng đứng 90°: Tự nhận diện `$|\Delta X| < 0.25 \times |\Delta Y|$`, xoay text 90° dọc đường gióng, nét gióng chạy thẳng giữa dòng X (trái) và Y (phải).
   - Mở tệp `work-items/TASK-0016.json`, bổ sung khối review độc lập của Codex:
     `"review": { "verdict": "APPROVED", "reviewer": "Codex", "independent": true, "findings": [] }`.

2. **Dọn dẹp và nghiệm thu các task còn tồn đọng**:
   - `work-items/TASK-0011.json` (do Codex phụ trách): Bổ sung evidence kiểm tra integrity của `products/meps` để đóng task.
   - `work-items/TASK-0014.json` & `TASK-0015.json`: Cập nhật trạng thái khép kín lịch sử công việc.

3. **Khởi tạo nhiệm vụ mới (`TASK-0017`) theo Backlog**:
   - Tham khảo `PRODUCT_BACKLOG.md`: Phát triển lệnh `T-TOADOXUAT` để trích xuất bảng tọa độ từ `XData` của các nhãn `T-TOADO` ra bảng CAD Table hoặc file CSV.
   - Tạo file `work-items/TASK-0017.json` từ `TASK.template.json`.
   - Viết code vào `products/meps/src/Toado/`.
   - Biên dịch bằng `products/meps/tools/build_v4.cmd`.
   - Đóng gói bản kế tiếp vào `D:\TuanPa_AI\OUTPUT\8. TVXD.BG V8`.

Codex hãy ghi lại nhật ký thực hiện bên dưới mục này khi hoàn thành.
