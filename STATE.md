# TRẠNG THÁI DỰ ÁN (PROJECT STATE)
Cập nhật lần cuối: 2026-11-22 10:25 | Phụ trách: Gemini & Codex

## 1. Mục tiêu hiện tại
- Hoàn tất phiên bản V5, sửa dứt điểm lỗi hình học `T-TOADO` theo đúng mẫu hình ảnh người dùng.
- Duy trì tính ổn định của hệ thống MEPS (1.295 types, ribbon, SDNL, HTKT).

## 2. Việc đã xong (Done)
- Sửa dứt điểm 2 lỗi hình học `T-TOADO`:
  + Chuyển nét nằm ngang ở đỉnh chữ X vào giữa 2 dòng X và Y (đường gióng xiên kết nối trực tiếp vào nét gạch phân cách giữa X và Y, xóa bỏ hoàn toàn nét thừa ở đỉnh X).
  + Chuẩn hóa `MLeaderStyle` mang tên `TVXD_TOADO` (`EnableDogleg = false`, `AttachmentBottomOfTopLine`, `ExtendLeaderToText = true`).
  + Tính toán tọa độ `TextLocation` chính xác cho cả 2 hướng trỏ trái/phải (`landingPt.X >= targetPt.X` và ngược lại), đảm bảo text và đường gạch chân gắn kết đồng nhất không bị trôi/văng.
- Biên dịch thành công `TVXD.Meps.dll` (SHA256: `5F29C4DCFA3E52671062C35ACEFF8AA2E8D34D8B5E45183C48846074885E638E`).
- Kiểm tra tính toàn vẹn 293 files trong checksums đạt 100%.
- Đóng gói hoàn chỉnh bản phát hành vào: `D:\TuanPa_AI\OUTPUT\5. TVXD.BG V5`.
- Cập nhật trực tiếp DLL mới vào môi trường chạy thật `%LOCALAPPDATA%\TVXD\MEPS060\TVXD.Meps.dll`.

## 3. Việc đang làm (In Progress)
- Bàn giao `5. TVXD.BG V5` cho người dùng nghiệm thu trên bản vẽ AutoCAD.

## 4. Bước tiếp theo (Next Action)
- Người dùng kiểm tra lệnh `T-TOADO` trong AutoCAD: pick điểm gán nhãn, kiểm tra nét gạch nằm xen giữa 2 dòng X/Y, đường gióng nối thẳng vào gạch giữa.
