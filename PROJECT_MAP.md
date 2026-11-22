# BẢN ĐỒ DỰ ÁN (PROJECT MAP)
Định vị nhanh các thành phần cốt lõi của TuanPa_AI (Tra cứu trong 0.1s không cần quét ổ đĩa):

## Mã nguồn & Nghiệp vụ (Source Code)
- `products/meps/src/Toado/ToadoPart.cs`: Mã nguồn C# lệnh tọa độ `T-TOADO`, hộp thoại và cấu trúc MLeader.
- `products/meps/src/Modules/`: Chứa hơn 40 tệp C# nghiệp vụ điện MEPS (Room, Route, Wire, Tag, Floors, Layout...).

## Công cụ Build & Biên dịch (Build Tools)
- `products/meps/tools/build_v4.cmd`: Kịch bản 1-click tự động build C#, merge IL và tạo `TVXD.Meps.dll`.
- `products/meps/tools/base/meps_orig.il`: File IL gốc dịch ngược (chứa đầy đủ 1.295 class & 40+ lệnh MEPS).
- `products/meps/tools/base/meps_orig.res`: Resource Win32 gốc bảo đảm tính toàn vẹn của PE assembly.
- `products/meps/tools/merge_toado.cs`: Tool C# tự động ghép module Toado vào IL gốc.
- `products/meps/tools/update_v4.cs`: Tool C# tự động cập nhật mã băm SHA256 cho installer.

## Phát hành & Bàn giao (Release & Output)
- `OUTPUT/`: Thư mục bàn giao các bản thương phẩm (`1. TVXD.BG V1`, `2. TVXD.BG V2`, `3. TVXD.BG V3`, `4. TVXD.BG V4`...).
- `products/meps/release/TVXD.BG_V4/`: Gói cài đặt mẫu gồm `CAI_DAT.cmd`, `CAI_DAT.exe` và thư mục `MEPS060/`.

## Bản vẽ thử nghiệm (Test Fixture)
- `products/meps/test/Mat bang cap dien trung the.dwg`: Bản vẽ mẫu AutoCAD để nạp và kiểm tra tính năng.

## Quy chuẩn & Chỉ dẫn phối hợp (Governance)
- `STATE.md`: Trạng thái dự án siêu ngắn (Đã xong, Đang làm, Bước tiếp theo).
- `WORKFLOW.md`: Quy trình kỹ thuật 7 bước áp dụng riêng cho repo này.
- `AGENTS.md`: Chỉ dẫn vận hành, phân vai và quy tắc bảo mật cho Gemini & Codex.
