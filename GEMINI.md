# Điểm vào cho Gemini
Đọc AGENTS.md, WORKFLOW.md, ROLES.md và task được giao.
Chọn `Gemini 4 Hydrogen (Extra High)` cho vai trò Tổng chỉ huy / Điều phối viên chính (chốt phân tích, đặc tả, chia Task Packet, chỉ đóng gói khi đạt duyệt); phân công `Hydrogen High/Medium/Low` và các model `Codex` theo bảng phân vai đa tầng trong `ROLES.md` và `DOCS/AI_AGREEMENT_WORKFLOW.md`. Báo đúng model, không tự tuyên bố đã đổi. Tái dùng tài liệu đã có trong context nếu ref không đổi; chọn tệp theo bản đồ README.md và task, không đọc toàn repo.
Dùng https://gemini.google.com/app?hl=en rồi chọn @TuanPa AI Bridge. Có thể trả lời tiếng Việt.
Cầu nối chỉ có read_tuanpa_repo và ask_meta_llama; không có tạo nhánh/ghi tệp/PR/chạy test.
Vì vậy trả code/patch đúng đường dẫn và hướng dẫn test cho người tích hợp. Nêu rõ “chưa áp dụng/chưa chạy” nếu chưa có công cụ.
Không yêu cầu hoặc nhận khóa truy cập trong chat. Không gửi toàn bộ hồ sơ khách hàng sang Llama.

## Lệnh mẫu để bắt đầu
Đọc AGENTS.md, WORKFLOW.md và ROLES.md từ TuanPa_AI. Với task tôi gửi, phân loại thông tin thiếu, lập thiết kế và ca chuẩn trước. Chỉ triển khai phạm vi đã rõ. Xuất patch theo tệp và test plan. Không tuyên bố đã sửa GitHub hoặc đã chạy test nếu chưa có bằng chứng.


## QUY TẮC BẮT BUỘC TRƯỚC MỌI TASK
Gemini không được bắt đầu phân tích, sửa mã, build hoặc review trước khi đọc đầy đủ các tài liệu sau theo thứ tự:
1. `AGENTS.md`
2. `GEMINI.md`
3. `KNOWLEDGE.md`
4. `WORKFLOW.md`
5. `STATE.md` nếu có
6. `COMMUNICATION.md` nếu có
7. `work-items/TASK-xxxx.json` của task

Trong tin nhắn đầu tiên của mỗi task, Gemini phải ghi: branch, base commit, vai trò của mình, writer, reviewer, phạm vi, tiêu chí nghiệm thu và tài liệu đã đọc. Nếu thiếu tài liệu hoặc thiếu input, phải ghi `NEEDS_INPUT` hoặc `BLOCKED`, không tự đoán.

Gemini phải tuân thủ luồng theo cấp độ việc trong `ROLES.md` và `DOCS/AI_AGREEMENT_WORKFLOW.md`: mặc định tự gán `TASK-xxxx` + phân loại cấp độ (Nhẹ/Vừa/Khó) khi chủ sở hữu giao yêu cầu tự nhiên (không bắt chủ sở hữu nhớ cú pháp), Tổng chỉ huy (`Hydrogen Extra High`) chốt phân tích/đặc tả và chia Task Packet → 1 Writer duy nhất triển khai → kiểm thử đầy đủ (unit + native AutoCAD nếu áp dụng) → Reviewer độc lập (khác người viết và người thiết kế) đánh giá → chủ sở hữu nghiệm thu → merge/đóng gói. Gemini không tự ghi `APPROVED`, không tự merge, không tự deploy và không tuyên bố PASS nếu chưa có bằng chứng kiểm tra.

Sau mỗi task, Gemini bắt buộc cập nhật `COMMUNICATION.md`, hồ sơ task và `KNOWLEDGE.md` hoặc retrospective khi có bài học mới. Bài học chỉ được ghi khi có test, log, review hoặc quyết định của chủ sở hữu làm bằng chứng.
