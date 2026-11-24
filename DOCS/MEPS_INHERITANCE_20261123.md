# TỔNG HỢP KẾ THỪA LỊCH SỬ MEPS (MEPS INHERITANCE SNAPSHOT)
> **Nguồn gốc:** Snapshot tiếp nhận ngày 23/11/2026 (`TVXD-WORKFLOW-20261103-SIMPLE`).  
> **Mục đích:** Lưu trữ cô đọng các bài học kỹ thuật, nguồn gốc baseline và kinh nghiệm thiết kế để tích lũy vào dự án mà không cần duy trì thư mục rác.

---

## 1. NGUỒN GỐC BASELINE VÀ PHIÊN BẢN LỊCH SỬ
- **Baseline gốc:** `BG_R52` (SHA256: `75AC44B1B4852A3EB333F96519082BB278B983352F4387E65A06EED25341815A`).
- **Phụ thuộc trình biên dịch:** `MEP_R12`.
- **Thành quả TAG V2 đã nghiệm thu trước đó:** DLL SHA256 `2499A597F75EA1D6E65789F9E9315ECF2486A5963B99CD0421C68AA5A2B69DFD` (đã kiểm tra trên 34 block với 4 hướng nhìn xoay UCS 0/30/90/-60 độ).
- **Bộ cài sạch (Install Clean):** Đã kiểm tra giữ nguyên `DATA/`, thư viện `HTKT/`, các tùy chọn `INI` của người dùng; có cơ chế rollback hoàn nguyên byte-for-byte và từ chối cài đè khi AutoCAD đang mở.

---

## 2. BÀI HỌC KỸ THUẬT HÌNH HỌC VÀ THUẬT TOÁN (CORE LESSONS)
1. **Đánh số thứ tự Block hiển thị (Tag Sequence):**
   - Điểm chèn (`br.Position`) của block thường xuyên bị trôi hoặc lệch xa do thiết kế thư viện block ban đầu.
   - Khi tính thứ tự màn hình: Cần tính tâm hình học thực tế, loại trừ các thuộc tính (attribute definitions/references) và các thành phần vô hạn hoặc ẩn, chiếu về hệ tọa độ màn hình (DCS/UCS).
   - Bảo toàn chiều cao chữ và thuộc tính gốc của người dùng; hỗ trợ lệnh `U` (Undo) khôi phục nguyên trạng.
2. **Tọa độ Block Khảo sát (T-TOADO):**
   - Phân biệt 3 khái niệm: Điểm đặt mũi tên Leader, Điểm lấy tọa độ khảo sát và Hệ tọa độ hiển thị.
   - Khi điểm chèn nội bộ trôi hàng triệu đơn vị khỏi hình hiển thị, cơ chế fallback về tâm extents giúp mũi tên gán nhãn luôn chỉ đúng vị trí mắt người nhìn thấy trên bản vẽ.
3. **Bảo toàn dữ liệu người dùng khi nâng cấp bộ cài:**
   - Khi cập nhật phần mềm, bộ cài phải luôn quét và bảo toàn các thư mục `DATA/`, các file cấu hình `.ini`, tuyệt đối không ghi đè làm mất thư viện riêng của kỹ sư thiết kế.

---

## 3. QUY TRÌNH TINH GỌN (SIMPLE RULES)
- Một Task $\rightarrow$ Một Checkpoint ngắn $\le 16.000$ byte UTF-8.
- Single-Writer per scope, Independent Reviewer khác model với Executor.
- Mã nguồn và bài học kinh nghiệm luôn chốt và lưu vết trên Git Repo.
