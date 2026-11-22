

## [2026-11-22] CODEX — CHỐT LẠI PHÂN VAI BẮT BUỘC
- **Quy tắc áp dụng từ entry này**: Codex là **writer duy nhất** cho mã ứng dụng trên task này. Codex đọc đặc tả, sửa `products/meps/src/`, cập nhật test/hồ sơ cần thiết và commit.
- **Vai trò Gemini**: phân tích yêu cầu, chỉ ra nguyên nhân/giải pháp, review diff Codex, chạy build/native test trên Windows/AutoCAD và báo cáo bằng chứng. Gemini không tự sửa mã ứng dụng thay Codex và không tự ghi APPROVED.
- **Luồng bắt buộc**: Gemini phân tích/bàn giao → Codex viết code → Codex kiểm tra → Gemini review/native verification → chủ sở hữu nghiệm thu → merge.
- **Áp dụng cho handoff hiện tại**: đề xuất bỏ khoảng trống dogleg là đầu vào review; Codex sẽ quyết định và thực hiện thay đổi mã. Gemini chỉ xác nhận native sau commit của Codex.
- **NEXT_ACTOR**: Codex
