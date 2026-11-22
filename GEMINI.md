# Điểm vào cho Gemini
Đọc AGENTS.md, WORKFLOW.md, ROLES.md và task được giao.
Chọn Argon khi có sẵn; mức suy luận theo độ khó/rủi ro trong WORKFLOW.md. Báo đúng model, không tự tuyên bố đã đổi. Tái dùng tài liệu đã có trong context nếu ref không đổi; chọn tệp theo bản đồ README.md và task, không đọc toàn repo.
Dùng https://gemini.google.com/app?hl=en rồi chọn @TuanPa AI Bridge. Có thể trả lời tiếng Việt.
Cầu nối chỉ có read_tuanpa_repo và ask_meta_llama; không có tạo nhánh/ghi tệp/PR/chạy test.
Vì vậy trả code/patch đúng đường dẫn và hướng dẫn test cho người tích hợp. Nêu rõ “chưa áp dụng/chưa chạy” nếu chưa có công cụ.
Không yêu cầu hoặc nhận khóa truy cập trong chat. Không gửi toàn bộ hồ sơ khách hàng sang Llama.

## Lệnh mẫu để bắt đầu
Đọc AGENTS.md, WORKFLOW.md và ROLES.md từ TuanPa_AI. Với task tôi gửi, phân loại thông tin thiếu, lập thiết kế và ca chuẩn trước. Chỉ triển khai phạm vi đã rõ. Xuất patch theo tệp và test plan. Không tuyên bố đã sửa GitHub hoặc đã chạy test nếu chưa có bằng chứng.
