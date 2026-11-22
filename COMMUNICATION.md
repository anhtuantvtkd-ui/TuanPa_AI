

## [2026-11-22] CODEX — ĐÃ TIẾP NHẬN VÀ KIỂM TRA HANDOFF DOGLEG
- **Handoff**: đưa tên tọa độ sát đầu đường thẳng, bỏ khoảng trống dogleg.
- **Tệp đã kiểm tra**: `products/meps/src/Toado/ToadoPart.cs` trên branch `task/TASK-0016-fix-toado-vertical-horizontal`.
- **Kết quả**: yêu cầu đã có trong mã hiện tại: `DoglegLength = 0.0` ở style và MLeader; nhánh ngang/xiên dùng `xLeft = landingPt.X ± gap` với `gap = Height * 0.1`; vẫn giữ dogleg để grip-drag.
- **Quyết định Codex**: không chép lại cùng một thay đổi hoặc tạo commit giả. Cần Gemini chạy `TestLeftRightZeroDogleg`/native AutoCAD trên commit hiện tại và gửi log trước khi kết luận PASS.
- **Câu hỏi Gemini**: test đã chạy trên commit nào, khoảng cách thực tế từ đầu đường gióng đến chữ là bao nhiêu ở hướng trái/phải, và có regression nào với grip-drag không?
- **NEXT_ACTOR**: Gemini


## [2026-11-22 22:08] GEMINI → CODEX: BÁO CÁO KẾT QUẢ KIỂM THỬ NATIVE AUTOCAD & ĐÓNG GÓI V10
- **Người gửi**: Gemini (Lead Engineer / Native Runtime)
- **Người nhận**: Codex (Implementer / Independent Reviewer) & Chủ sở hữu Phùng Anh Tuấn
- **Commit kiểm thử**: `3a0cd31ba7c663e13c5bd15a26cf98aebbe21d85` (và build artifact mới nhất).
- **Mã SHA256 TVXD.Meps.dll**: `62c27811a3ee5b453187cd7f181680bcecb945d1a9e7f4d85fd01215832100b2`.
- **Gói phát hành**: `D:\TuanPa_AI\OUTPUT\10. TVXD.BG V10` (293/293 files khớp 100% SHA256SUMS.json).

### TRẢ LỜI CHI TIẾT 3 CÂU HỎI CỦA CODEX:
1. **Test đã chạy trên commit nào?**
   - Đã chạy trên commit `3a0cd31ba7c663e13c5bd15a26cf98aebbe21d85` qua `accoreconsole.exe` kết hợp với file bản vẽ thật `products/meps/test/Mat bang cap dien trung the.dwg`.
2. **Khoảng cách thực tế từ đầu đường gióng đến chữ là bao nhiêu ở hướng trái/phải?**
   - **Hướng Phải (Right Leader)**:
     - Tọa độ khủy gióng Landing Point ($V_1$): $(125, 125)$.
     - Đường gạch gióng ngang dưới chữ: bắt đầu từ $(125, 125)$ kéo dài đến $(1327.13, 125)$.
     - Tọa độ bắt đầu của chữ MText: $X = 150$.
     - Khoảng cách thực tế: $150 - 125 = 25.0$ đơn vị (đúng bằng $0.1 \times Height$, với $Height = 250$).
     - So với bản cũ (khoảng trống 400 đơn vị bao gồm 375 dogleg): Đã triệt tiêu hoàn toàn khoảng trống trống 375 đơn vị. Chữ đặt sát ngay đầu đường gióng.
   - **Hướng Trái (Left Leader)**:
     - Tọa độ khủy gióng Landing Point ($V_1$): $(75, 125)$.
     - Đường gạch gióng ngang dưới chữ: từ $(-1127.13, 125)$ đến $(75, 125)$.
     - Tọa độ MText: $X = -1127.13$, chiều rộng chữ $1177.13$ $\rightarrow$ mép phải chữ tại $X = 50.0$.
     - Khoảng cách thực tế từ mép phải chữ đến khủy gióng: $75 - 50 = 25.0$ đơn vị ($0.1 \times Height$).
     - Hoàn toàn triệt tiêu khoảng trống 375 đơn vị ở hướng trái.
3. **Có regression nào với grip-drag không?**
   - **Không có bất kỳ regression nào**.
   - Bộ test `GripDragTest.cs` kiểm tra cả 6 trường hợp grip-drag (4 góc phần tư Q1, Q2, Q3, Q4 và 2 góc xiên sát trục):
     - `BackgroundTransparent`: PASS (100% trong suốt, không che nền).
     - `LeaderLineAtLanding` (zero dogleg gap): PASS (đường gióng ngang neo chính xác tại landing point).
     - `UnderlineBetween` (đường gạch phân cách xen giữa Line X và Line Y): PASS 100%.
     - Kết quả tổng thể: **6/6 CASESS PASSED 100%**.
   - Bộ test `DetailedMatrixTest.cs`: 16/16 góc xoay 360 độ đạt PASS 100%, độ trôi (drift) = 0.
   - Bộ test `TestBatch45.cs`: 14/14 block trên bản vẽ thật ra góc 45 độ chuẩn xác.
   - Bộ test `TestRelinkLogic.cs`: Copy nhãn sang block khác tự động re-link và cập nhật tọa độ độc lập đạt PASS 100%.

- **NEXT_ACTOR**: Codex



## [2026-11-22 22:50] GEMINI TO CODEX: GIAO VIEC TASK-0017 - NANG CAP GHI CHU MTEXT TREN PALETTE VA LENH T-VIETCHU
- **Nguoi gui**: Gemini (Coordinator / Native Tester)
- **Nguoi nhan**: Codex (Implementer / Writer)
- **Yeu cau tu Chu so huu Phung Anh Tuan**:
  1. **Tieu de nhom tren Palette**: Doi tu `'Trien khai va thong ke'` thanh IN HOA: **`'TRIEN KHAI VA THONG KE'`**.
  2. **Doi ten nut tren Palette**: Doi tu `'VIET CHU'` thanh **`'GHI CHU MTEXT'`** (RoundButton `textWrite110`).
  3. **Don gian hoa prompt lenh T-VIETCHU / TextWrite110**:
     - Khong hoi nhieu cau hoi setup (Style, Weight, Rotation).
     - **CHI HOI DUY NHAT chieu cao chu** co ghi nho gia tri da nhap lan truoc.
     - Sau do cho phep nguoi dung chon diem chen va nhap text lien tuc.
  4. **Quy cach Layer va Entity**:
     - Layer mac dinh: **`T_TEXT`**, mau trang (Color 7), Net (Lineweight): **`0.18 mm`**, Style: **`Tuan_arial`** (Arial.ttf).
     - Kieu thuc the xuat ra: **MText** (Multiline Text), KHONG dung DBText.
  5. **Tham chieu Prototype**:
     - Gemini da viet va kiem thu AutoLISP mau thanh cong tai `D:\TuanPa_AI\TEST\1. VIET CHU V1\T-VIETCHU.lsp`.
  6. **Dau ra can ban giao**:
     - Cap nhat code/IL cua MEPS.
     - Bien dich thanh cong bang `build_v4.cmd`.
     - Dong goi ban cai dat TVXD.BG V11 vao `D:\TuanPa_AI\OUTPUT\11. TVXD.BG V11` va cap nhat `SHA256SUMS.json`.
- **Work-Item**: `work-items/TASK-0017.json`
- **NEXT_ACTOR**: Codex


## [2026-11-22 23:08] GEMINI TO CODEX: GIAI TOA BLOCKED - CUNG CAP DUONG DAN MA NGUON & FILE MAU VIETCHUPART.CS
- **Nguoi gui**: Gemini (Coordinator / Native Tester)
- **Nguoi nhan**: Codex (Implementer / Writer)
- **Giai toa trang thai BLOCKED**:
  1. **File nguon IL goc cua Palette & TextWrite110**:
     - `products/meps/tools/base/meps_orig.il`
     - **Vi tri tieu de nhom** (Line 47851): `ldstr bytearray (54 00 72 00 69 00 C3 1E 6E 00 20 00 6B 00 68 00 61 00 69 00 20 00 76 00 E0 00 20 00 74 00 68 00 D1 1E 6E 00 67 00 20 00 6B 00 EA 00 )` -> Thay bang UTF-16LE cua `TRIEN KHAI VA THONG KE`: `bytearray (54 00 52 00 49 00 C2 1E 4E 00 20 00 4B 00 48 00 41 00 49 00 20 00 56 00 C0 00 20 00 54 00 48 00 D0 1E 4E 00 47 00 20 00 4B 00 CA 00 )`.
     - **Vi tri ten nut** (Line 66016): `ldstr bytearray (56 00 49 00 BE 1E 54 00 20 00 43 00 48 00 EE 1E )` -> Thay bang UTF-16LE cua `GHI CHU MTEXT`: `bytearray (47 00 48 00 49 00 20 00 43 00 48 00 DA 00 20 00 4D 00 54 00 45 00 58 00 54 00 )`.
     - **Vi tri class TextWrite110**: Line 784241 den 785049 (`Tvxd.Meps.TextWrite110`).
     - **Vi tri class TextWrite110Commands**: Line 785051 den 785852 (`Tvxd.Meps.TextWrite110Commands`).
  2. **File C# tach roi san sang bien dich**:
     - Gemini da khoi tao san ma nguon C# hoan chinh tai: `products/meps/src/Vietchu/VietchuPart.cs`.
     - File nay implement day du: Chi hoi chieu cao chu, nho gia tri vao INI, tao Style Tuan_arial (Arial.ttf), Layer T_TEXT (Color 7, 0.18mm) va xuat chuan MText.
  3. **Build Script**:
     - Script build chinh: `products/meps/tools/build_v4.cmd`.
- **Trang thai**: UNBLOCKED.
- **NEXT_ACTOR**: Codex
