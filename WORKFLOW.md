# Quy trình sản xuất phần mềm kỹ thuật TuanPa
Revision: TUANPA-ENG-1. Áp dụng riêng anhtuantvtkd-ui/TuanPa_AI.

## 1. Nhận việc
Tạo một GitHub Issue, một work-items/TASK-xxxx.json từ TASK.template.json, ghi cùng task_id.
Nêu sản phẩm cuối, người dùng, phạm vi, phần loại trừ, tiêu chí nhận, input/nguồn, rủi ro và môi trường cần kiểm.
Chưa đủ dữ liệu: NEEDS_INPUT; ghi câu hỏi cụ thể, vẫn có thể làm phần độc lập.

## 2. Đặc tả và thiết kế
Điện: tải, hệ số, sụt áp, bảo vệ, đơn vị, điều kiện lắp đặt.
Cơ/nước: lưu lượng, áp suất, tổn thất, đặc tính thiết bị, đơn vị, phạm vi tính.
CAD: phiên bản host, .NET, lệnh, bản vẽ thử và thao tác hoàn tác.
Tài liệu/OCR: provenance trang/ô, độ tin cậy, đối chiếu số liệu; chưa chắc thì đánh dấu, không đoán.
Chọn đúng bộ tiêu chuẩn/bản/điều khoản và xác minh hiệu lực theo nguồn có thẩm quyền.
Dùng mức suy luận cao nhất model đang hỗ trợ đã xác minh; ghi model thật, không âm thầm đổi khi hết quota.
Đầu ra: đặc tả, hợp đồng dữ liệu/đơn vị, kiến trúc, ca chuẩn và tiêu chí nghiệm thu.

## 3. Triển khai
Tạo nhánh task/TASK-xxxx-mo-ta từ main và ghi base_commit đầy đủ.
Gemini Argon là lựa chọn triển khai chính theo chủ sở hữu; không phải cam kết luôn tốt hơn model khác.
Một writer cho task; chia module có hợp đồng giao tiếp rõ nếu cần.
Viết lõi xác định, test có ý nghĩa, rồi adapter/UI. Không trộn giả định thiết kế vào dữ liệu khách hàng.
Nếu Gemini chỉ có quyền đọc MCP: xuất patch/code + tên tệp + test plan; chủ sở hữu hoặc Codex có phiên ghi được phép áp dụng vào nhánh. Không báo đã push khi chỉ sinh văn bản.
Không mở rộng MCP sang quyền ghi khi chưa thiết kế và xác nhận cấp quyền.

## 4. Kiểm thử
Chạy npm ci --ignore-scripts, npm test, npm run type-check, npm run workflow:check.
CI hiện kiểm tra bridge và hồ sơ task; mỗi module mới phải bổ sung command/job kiểm thử riêng trước khi tuyên bố được CI bảo vệ.
Lõi tính: so ca chuẩn độc lập, unit conversion, biên, input thiếu/sai và sai số cho phép.
CAD/desktop: test trong đúng AutoCAD/Windows trên fixture; log/screenshot trước-sau, không dùng drawing khách hàng để thử.
OCR/Office: so input-output, số/đơn vị/trang; test mở thực tế nếu tiêu chí yêu cầu.
Không có môi trường native: BLOCKED nghiệm thu native, không ghi PASS.

## 5. Review độc lập
Tạo PR có task_id; gửi reviewer diff, base/result commit, đặc tả và bằng chứng tối thiểu.
Reviewer có thể là Codex hoặc phiên/model khác thực sự chạy; task thường có thể dùng checklist chủ sở hữu, task high cần reviewer độc lập và chủ sở hữu nghiệm thu chuyên môn.
Llama chỉ hỗ trợ giải thích/tìm góc thiếu; giới hạn 768 token, không dùng làm reviewer duy nhất cho tính toán an toàn.
Ghi finding, severity, trạng thái giải quyết và reviewed_commit trong REVIEW.template.json.
Code đổi sau review: kiểm delta và review lại phần ảnh hưởng. Không dùng review cũ để chứng nhận bản mới.

## 6. Chốt và phát hành
Trạng thái: READY_FOR_REVIEW -> APPROVED -> RELEASED (hoặc quay IMPLEMENTING/NEEDS_INPUT/BLOCKED).
APPROVED chỉ khi CI, các test chuyên môn/native liên quan đạt; finding chặn đã xử lý; chủ sở hữu chấp thuận đúng commit.
Merge và release là hành động riêng cần chủ sở hữu cho phép. Không auto-merge PR của AI/Dependabot.
Gói cuối: source commit, artifact+SHA256, hướng dẫn dùng/cài, release notes, test report, known issues, backup/rollback.
Thử rollback trên môi trường thử trước cài đè. Chỉ tuyên bố sản phẩm cuối khi checklist RELEASE đạt.
Mã tính toán/CAD không được tự phát hành qua pipeline Cloudflare dành cho bridge.

## 7. Sửa lỗi sau giao
Ghi version, bước tái hiện, expected/actual, input đã ẩn dữ liệu riêng.
Tái hiện -> regression test -> fix trên nhánh -> review -> version mới -> kiểm lại môi trường người dùng.
Không lặp lại cùng bản cài và gọi là bản sửa.

## Tự động hóa đang có
- GitHub Actions CI: unit tests, TypeScript, kiểm cấu trúc task.
- Dependabot: cảnh báo và PR cập nhật npm/Actions hàng tuần; không tự merge.
- Cloudflare: deploy bridge khi main đổi.
- Gemini MCP: đọc tệp public trên main và gọi Llama; không ghi repo/không tự lấy diff nhánh.
- Copilot Free: chat hỗ trợ code đã thử; cloud agent cần gói trả phí, chưa bật.
- Meta AI Facebook: mới nộp hồ sơ, chưa có quyền connector.
Việc chuyển gói giữa AI, review chuyên môn, merge và phát hành hiện do người điều phối thực hiện; không có daemon nhiều AI chạy tự động.

## Cổng kiểm tra
workflow:check chỉ kiểm cấu trúc/những khai báo còn thiếu; không xác minh tính thật của test/review hay tự cấp quyền.
Phê duyệt và khóa nhánh trong GitHub là lớp riêng. Không coi văn bản hướng dẫn là branch protection.
