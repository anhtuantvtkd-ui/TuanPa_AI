# Kho kiến thức đã kiểm chứng

Đây là bộ nhớ kỹ thuật dùng chung cho Gemini, Codex và người điều phối. Chỉ ghi bài học có bằng chứng từ test, review, log native hoặc quyết định của chủ sở hữu.

## Cách sử dụng

1. Đọc mục liên quan trước khi sửa.
2. Khi phát hiện lỗi lặp lại, thêm một mục theo mẫu.
3. Mỗi mục phải có bằng chứng và test ngăn tái diễn.
4. Không ghi transcript, phỏng đoán chưa kiểm chứng hoặc secrets.

## Quy tắc đã xác nhận

### MEPS / T-TOADO: giữ landing point qua refresh

- **Phát hiện**: MLeader đứng bị drift khi chạy `T-TOADOREFRESH` nhiều lần.
- **Nguyên nhân**: `GetLastVertex(0)` là `endPt` của leader đứng, không phải landing point ban đầu.
- **Quy tắc**: Không dùng vertex cuối làm landing nếu hình học tạo thêm đoạn trung gian. Lưu landing gốc trong XData hoặc khôi phục bằng phép biến đổi xác định.
- **Bằng chứng**: TASK-0016, test DWG thật và log native do Gemini cung cấp.
- **Regression**: Refresh lặp ở 90° và 270° phải có drift bằng 0.
- **Trạng thái**: Đã áp dụng bản sửa; cần native re-test sau mỗi thay đổi liên quan.

### MEPS / T-TOADO: review hình học phải kiểm tra hành vi, không chỉ tọa độ tĩnh

- **Quy tắc**: Test phải bao phủ Explode, vị trí X/Y, vị trí stem, refresh lặp và các góc thuận/ngược.
- **Bằng chứng**: TASK-0016 đã phát hiện test rotation-only không đủ.
- **Regression**: Không gọi PASS nếu chỉ kiểm tra Rotation 90°.
