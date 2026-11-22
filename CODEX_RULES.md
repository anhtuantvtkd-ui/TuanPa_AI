# CODEX DEFAULT WORK RULES

Áp dụng cho mọi task trong repo TuanPa_AI.

## Quy tắc nhận việc bắt buộc

Khi COMMUNICATION.md, STATE.md hoặc task record giao việc cho Codex với NEXT_ACTOR: Codex:

1. Codex phải bắt đầu xử lý ngay trong phiên hiện tại.
2. Codex phải đọc handoff, task, diff liên quan và xác định phạm vi.
3. Codex phải chạy preflight: xác minh mọi path/commit được handoff dẫn tới tồn tại, đọc được và có nội dung; kiểm tra branch/ref đúng.
4. Codex phải sửa mã/test/hồ sơ thuộc phạm vi, hoặc ghi rõ lý do kỹ thuật nếu thay đổi đã tồn tại và không cần lặp lại.
5. Codex phải chạy kiểm tra phù hợp, ghi commit/result commit và chuyển handoff cho Gemini review/native test.
6. Codex không được chỉ trả lời 'đã nhận' rồi dừng.
7. Chỉ được dừng khi thiếu input (NEEDS_INPUT), môi trường không có (BLOCKED), hoặc task hoàn tất có bằng chứng.

## Preflight, escalation và tự phục hồi

Nếu path được giao không tồn tại, file rỗng, ref sai, hoặc mã nguồn không truy xuất được, Codex phải trong cùng phiên ghi ngay một entry trên COMMUNICATION.md với trạng thái NEEDS_INPUT/BLOCKED, nêu path/ref/SHA đã kiểm tra và yêu cầu hành động cụ thể từ Gemini.

Nếu blocker chưa được giải tỏa, Codex phải tự gửi follow-up ping trên COMMUNICATION.md ở mỗi lần kiểm tra trạng thái có thay đổi hoặc tối đa sau một chu kỳ poll của runner; ping phải nêu mã blocker, thời điểm kiểm tra gần nhất và hành động Gemini cần làm. Không yêu cầu chủ sở hữu chuyển lời qua chat. Khi Gemini bổ sung commit/path mới, Codex phải tự đọc entry mới nhất, tiếp tục task và ghi kết quả; không lặp lại blocker cũ.

Mỗi handoff phải có tối thiểu: task_id, branch, base/result commit, danh sách path nguồn, tiêu chí nghiệm thu, NEXT_ACTOR và lệnh kiểm tra dự kiến. Handoff thiếu các trường này được ghi NEEDS_INPUT ngay.

## Báo blocker và bàn giao

Mọi thiếu input, lỗi môi trường hoặc điểm không thể tiếp tục phải được ghi ngay trên COMMUNICATION.md hoặc STATE.md, không chờ chủ sở hữu chuyển lời qua chat. Entry bắt buộc có: người gửi, người nhận, task, trạng thái NEEDS_INPUT/BLOCKED, nguyên nhân cụ thể, yêu cầu hành động và NEXT_ACTOR. Khi nhận đủ thông tin, Codex tiếp tục từ entry đó và ghi commit/handoff kết quả trên repo.

## Phân vai mặc định

- Gemini: phân tích, bàn giao, review độc lập, chạy build/native test và báo cáo bằng chứng.
- Codex: writer duy nhất của mã ứng dụng trên task được giao, cập nhật test/hồ sơ và commit.
- Chủ sở hữu: nghiệm thu chuyên môn, cho phép merge/release.

## Luồng chuẩn

Gemini phân tích/bàn giao → Codex viết và kiểm tra → Gemini review/native verification → chủ sở hữu nghiệm thu → merge.

Không tự ghi APPROVED, merge hoặc deploy nếu chưa qua cổng tương ứng.
