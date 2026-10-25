# Phân vai theo công việc
| Vai | Phụ trách đề xuất | Đầu vào | Đầu ra |
|---|---|---|---|
| Chủ sản phẩm/nghiệm thu | Phùng Anh Tuấn | yêu cầu, nguồn kỹ thuật | ưu tiên, xác nhận giả thiết, duyệt commit/phát hành |
| Thiết kế & triển khai | Gemini Argon khi có sẵn | task, đặc tả, đoạn code đúng commit | phương án, patch, test và giới hạn |
| Điều phối/tích hợp | chủ sở hữu + Codex trong phiên được phép | task, patch, CI | nhánh/PR, giải xung đột, gói bàn giao |
| Review độc lập | Codex hoặc reviewer khác thực sự kết nối | diff + bằng chứng + tiêu chí | finding có vị trí, verdict, reviewed_commit |
| Trợ lý tại editor | Copilot Free | tệp/ngữ cảnh đã chọn | gợi ý code, test, giải thích |
| Hỗ trợ phản biện nhỏ | Llama qua Cloudflare | câu hỏi ngắn, ngữ cảnh không riêng tư | giải thích/điểm cần hỏi lại; không nghiệm thu |
| Kiểm tra xác định | Actions/test runner | commit + fixtures | pass/fail + logs |
| Theo dõi phụ thuộc | Dependabot | manifests/lockfile | cảnh báo/PR, không tự merge |

Không dùng nhiều AI chỉ để đủ vai. Đây là phân công vận hành, không phải chứng minh model nào mạnh nhất.
Người triển khai không được tự ghi mình là reviewer độc lập. Khi Codex viết code, đổi reviewer hoặc ghi rõ chưa có independent review.
Hết quota: checkpoint, báo thiếu, đề xuất thay model; không giả phản hồi hoặc tự bật chi phí.

## Gói giao việc chuẩn
TASK_ID; repo; branch; BASE_SHA; RESULT_SHA nếu có; writer; phạm vi tệp; nguồn; test command+result; finding; câu hỏi còn thiếu.
MCP hiện đọc main: với nhánh chưa merge, gửi diff/đoạn tệp đã lọc secrets kèm SHA. Không để AI đọc main cũ rồi review nhánh mới.
