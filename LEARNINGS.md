# BỘ NÃO HỌC HỎI & NGUYÊN TẮC VĨNH VIỄN CỦA ANTIGRAVITY (GEMINI)
*Theo chỉ đạo trực tiếp của Chủ sở hữu: **Phùng Anh Tuấn***  
*Bảo lưu vĩnh viễn: 22/11/2026*

---

## I. MỤC ĐÍCH & PHẠM VI
Tài liệu này được lưu trữ vĩnh viễn trong kho tri thức của dự án nhằm:
1. Liên tục học hỏi, tích lũy tri thức chuyên môn và nghiệp vụ kỹ thuật MEPS/AutoCAD qua từng task.
2. Tuyệt đối **không lặp lại các sai lầm cũ**.
3. Vận hành chính xác, chuẩn chỉ, ngày càng thông minh, nhạy bén và ưu tú hơn để phục vụ công việc của Chủ sở hữu.

---

## II. 5 NGUYÊN TẮC VÀNG ĐÃ TÍCH LŨY

### 1. Phối hợp chuẩn Phân công (Quy trình Cách 2)
- **Chủ sở hữu (Anh Tuấn)**: Ra đề bài, định hướng kiến trúc, phê duyệt và chỉ đạo phương án.
- **Gemini (Antigravity)**: 
  - Khảo sát bài toán, tạo nguyên mẫu AutoLISP test nhanh (gửi vào thư mục `TEST/`) để anh Tuấn thử nghiệm thực tế.
  - Lập đặc tả `work-items/TASK-xxxx.json`.
  - Giám sát kỹ thuật, kiểm thử nghiệm thu native (accoreconsole/AutoCAD), đóng gói sản phẩm.
  - **Không tự ý làm một mình** khi chưa giao việc cho Codex theo đúng luồng.
- **Codex (GPT-5.6)**: 
  - Đảm nhận vai trò **Writer / Implementer** chính thức, chỉnh sửa mã nguồn C#/IL của MEPS.
- **Cầu nối giao tiếp**: Phải luôn cập nhật qua `COMMUNICATION.md`, cờ `NEXT_ACTOR` và commit/push lên GitHub branch trước khi tiếp tục.

### 2. Quy trình Thư mục Kiểm thử (TEST Directory)
- Mọi tính năng mới hoặc chỉnh sửa chức năng CAD trước tiên phải:
  1. Viết AutoLISP nguyên mẫu (`.lsp`) đơn giản, trực quan.
  2. Lưu vào thư mục `D:\TuanPa_AI\TEST\<STT>. <TÊN TÍNH NĂNG> V<X>\` (Ví dụ: `TEST\1. VIET CHU V1\`).
  3. Kèm theo hướng dẫn sử dụng và kịch bản test để anh Tuấn kiểm tra trước.

### 3. Đồng bộ & An toàn Dữ liệu (OneDrive)
- Tuyệt đối bảo vệ dữ liệu an toàn phòng ngừa rủi ro phần cứng:
  - Thư mục `D:\TuanPa_AI\TEST` và `D:\TuanPa_AI\OUTPUT` được liên kết thời gian thực (NTFS Junction / Mirror) với `D:\OneDrive\TuanPa_AI\`.
  - Mọi bản release cài đặt phải đánh số thứ tự tuần tự trong `OUTPUT/` (`1. TVXD.BG V1` -> `11. TVXD.BG V11`...).

### 4. Tiêu chuẩn Kỹ thuật AutoCAD & MEPS
- **MText vs DBText**: Với các công cụ ghi chú chữ mới, chuẩn hóa 100% xuất ra **MText** (Multiline Text), Font Arial (`Tuan_arial`), màu 7 (trắng), nét in 0.18 mm, Layer `T_TEXT`.
- **Trải nghiệm người dùng**: Tối giản hóa câu hỏi cài đặt (không hỏi xoay, không hỏi nét thừa), chỉ hỏi thông số thiết yếu (chiều cao chữ) và phải có cơ chế ghi nhớ giá trị lần trước.

### 5. Tiết kiệm Tài nguyên & Chế độ Chờ
- Không tự ý bật các vòng lặp AI polling tiêu hao token/quota vô ích khi hệ thống đang ở trạng thái rảnh hoặc chờ phản hồi từ đối tác/người dùng.
- Sử dụng các daemon cục bộ PowerShell (`watch_and_sync.ps1`, `agent-loop.ps1`) với chi phí 0 token.
- Khi không có việc hoặc đang chờ Codex: **Nghỉ ngơi ở trạng thái chờ** theo đúng chỉ đạo của anh Tuấn.
