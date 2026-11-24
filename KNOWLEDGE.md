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

### MEPS / Palette: kiểm cả callback và dispatch

- **Bằng chứng** TASK-0025, commit `4864292eb24a69bcf0b2e6763a9c1762db71a263`. WinForms gọi đúng callback cho 10/10 mục, trong khi IL của cả hai DLL cho thấy `T-KLHD` bị bộ lọc dispatch từ chối. [Báo cáo](DOCS/tasks/TASK-0025-codex-verification-report.md)
- **Test hồi quy cần có** Dùng fixture AutoCAD để bấm từng mục và xác nhận lệnh chạy đúng một lần. Đo cả nhãn và vùng vẽ của nút sau khi hook layout chạy.

- **Kiểm chứng bổ sung TASK-0025** Bản vá `5bdd390eb926dfe39b591bdfdf9168da9cc35e16` cho thấy có thể kiểm `Allowed` trên DLL release trong AutoCAD Core Console và bắt `LispWillStart` sau `Dispatch` với request sẵn sàng. Ca hồi quy ở ranh giới này kiểm mã mới, mã cũ, mã bị từ chối và LISP thực sự được kích hoạt. Lượt bấm Palette từ trạng thái chưa nạp LISP cần thử trong host desktop. [Bằng chứng](DOCS/tasks/TASK-0025-codex-verification-report.md#verification-status-resolved)
