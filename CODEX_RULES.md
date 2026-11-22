# CODEX DEFAULT WORK RULES

Áp dụng cho mọi task trong repo TuanPa_AI.

## Quy tắc nhận việc bắt buộc

Khi COMMUNICATION.md, STATE.md hoặc task record giao việc cho Codex với NEXT_ACTOR: Codex:

1. Codex phải bắt đầu xử lý ngay trong phiên hiện tại.
2. Codex phải đọc handoff, task, diff liên quan và xác định phạm vi.
3. Codex phải sửa mã/test/hồ sơ thuộc phạm vi, hoặc ghi rõ lý do kỹ thuật nếu thay đổi đã tồn tại và không cần lặp lại.
4. Codex phải chạy kiểm tra phù hợp, ghi commit/result commit và chuyển handoff cho Gemini review/native test.
5. Codex không được chỉ trả lời 'đã nhận' rồi dừng.
6. Chỉ được dừng khi thiếu input (NEEDS_INPUT), môi trường không có (BLOCKED), hoặc task hoàn tất có bằng chứng.

## Báo blocker và bàn giao

Mọi thiếu input, lỗi môi trường hoặc điểm không thể tiếp tục phải được ghi ngay trên COMMUNICATION.md hoặc STATE.md, không chờ chủ sở hữu chuyển lời qua chat. Entry bắt buộc có: người gửi, người nhận, task, trạng thái NEEDS_INPUT/BLOCKED, nguyên nhân cụ thể, yêu cầu hành động và NEXT_ACTOR. Khi nhận đủ thông tin, Codex tiếp tục từ entry đó và ghi commit/handoff kết quả trên repo.

## Phân vai mặc định

- Gemini: phân tích, bàn giao, review độc lập, chạy build/native test và báo cáo bằng chứng.
- Codex: writer duy nhất của mã ứng dụng trên task được giao, cập nhật test/hồ sơ và commit.
- Chủ sở hữu: nghiệm thu chuyên môn, cho phép merge/release.

## Luồng chuẩn

Gemini phân tích/bàn giao → Codex viết và kiểm tra → Gemini review/native verification → chủ sở hữu nghiệm thu → merge.

Không tự ghi APPROVED, merge hoặc deploy nếu chưa qua cổng tương ứng.
