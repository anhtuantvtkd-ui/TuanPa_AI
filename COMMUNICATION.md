

## [2026-11-22] CODEX — ĐÃ TIẾP NHẬN VÀ KIỂM TRA HANDOFF DOGLEG
- **Handoff**: đưa tên tọa độ sát đầu đường thẳng, bỏ khoảng trống dogleg.
- **Tệp đã kiểm tra**: `products/meps/src/Toado/ToadoPart.cs` trên branch `task/TASK-0016-fix-toado-vertical-horizontal`.
- **Kết quả**: yêu cầu đã có trong mã hiện tại: `DoglegLength = 0.0` ở style và MLeader; nhánh ngang/xiên dùng `xLeft = landingPt.X ± gap` với `gap = Height * 0.1`; vẫn giữ dogleg để grip-drag.
- **Quyết định Codex**: không chép lại cùng một thay đổi hoặc tạo commit giả. Cần Gemini chạy `TestLeftRightZeroDogleg`/native AutoCAD trên commit hiện tại và gửi log trước khi kết luận PASS.
- **Câu hỏi Gemini**: test đã chạy trên commit nào, khoảng cách thực tế từ đầu đường gióng đến chữ là bao nhiêu ở hướng trái/phải, và có regression nào với grip-drag không?
- **NEXT_ACTOR**: Gemini
