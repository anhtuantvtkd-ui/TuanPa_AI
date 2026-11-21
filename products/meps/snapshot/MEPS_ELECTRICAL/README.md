# MEPS AutoCAD Tools (TUVANXD.BG)

Hệ thống công cụ tự động hóa thiết kế Cơ Điện (MEPS) trên nền tảng **AutoCAD 2021** (.NET Framework 4.8 / C# & AutoLISP).

---

## 📁 Cấu trúc Thư mục Dự án

```text
MEPS_ELECTRICAL/
├── src/                          # Mã nguồn C# (78 files, đã phân nhóm module)
│   ├── core/                     # Khởi tạo plugin, hình học cơ bản, JSON serialization
│   ├── library/                  # Quản lý thư viện block, bảng chọn ký hiệu, tỉ lệ vẽ
│   ├── legend/                   # Tạo bảng ký hiệu tự động (BANG KY HIEU)
│   ├── wire/                     # Vẽ và quản lý dây dẫn, cáp điện (ACI color, ByLayer)
│   ├── room/                     # Bóc tách phòng, bố trí thiết bị, tải W, xuất Excel (.xlsx)
│   ├── floors/                   # Quản lý tầng, cao độ, ống đứng (riser)
│   ├── route/                    # Quản lý lộ, mạch, tuyến cáp động lực
│   ├── flex/                     # Vẽ ống gió mềm (Flex Duct)
│   ├── layout/                   # Bố cục bản vẽ, viewport, khung tên
│   ├── tag/                      # Đánh nhãn thiết bị (TAG), số thứ tự pha
│   ├── bdgkl/                    # Bảng đánh giá khối lượng chiếu sáng & 0.4kV
│   └── panel/                    # Quản lý tủ điện & Jig định vị
│
├── resources/                    # Tài nguyên AutoCAD
│   ├── HTKT_LIBRARY/             # Thư viện block hạ tầng kỹ thuật (DWG + preview JSON)
│   ├── LIBRARY_0_6/              # Thư viện thiết bị điện 0.6.0
│   ├── SDNL/                     # Engine sơ đồ nguyên lý (LISP + Excel mẫu)
│   └── *.lsp                     # Các file AutoLISP tích hợp (Layout, RouteStats, NumberPhase...)
│
├── scripts/                      # Kịch bản Build & Triển khai
│   ├── Build.ps1                 # Script build TVXD.Meps.dll một chạm bằng csc.exe
│   └── BUILD.cmd                 # Script build batch truyền thống
│
├── docs/                         # Tài liệu kiến trúc & Quy chuẩn cho AI
│   ├── ARCHITECTURE.md           # Chi tiết 12 module và luồng dữ liệu
│   └── AI_DEVELOPMENT_RULES.md   # Quy tắc bắt buộc cho AI để không đi sai hướng
│
└── output/                       # Thư mục đích chứa DLL sau khi build
    └── TVXD.Meps.dll             # DLL hoàn chỉnh sẵn sàng NETLOAD vào AutoCAD
```

---

## 🚀 Hướng dẫn Build (Một chạm)

Chạy PowerShell tại thư mục gốc:

```powershell
.\scripts\Build.ps1
```

* Script sẽ tự động gom 78 file C#, áp dụng đầy đủ 17 Feature Flags (`WIRES010`, `STANDARDS012`, `BDGKL034`...) và biên dịch ra `output\TVXD.Meps.dll`.
* Không cần mở Visual Studio nặng nề, biên dịch trực tiếp qua `csc.exe` trong ~3 giây.

---

## ⚡ Cách nạp vào AutoCAD 2021

1. Mở **AutoCAD 2021**.
2. Gõ lệnh: `NETLOAD`
3. Chọn file: `C:\Users\ADMIN\.gemini\antigravity\scratch\MEPS_ELECTRICAL\output\TVXD.Meps.dll`
4. Khởi chạy bảng điều khiển:
   * Gõ `MEPS060`: Mở bảng chọn ký hiệu và công cụ điện
   * Gõ `T-THCS` hoặc `T-THDIEN`: Mở bảng thống kê tuyến cáp & chiếu sáng
   * Gõ `T-VEDAYPL`: Vẽ Pline nối block theo cây đường ngắn nhất
