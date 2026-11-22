# MEPS Electrical Design Tools - TVXD.BG V4

Hệ thống công cụ thiết kế cơ điện (MEPS) chuyên sâu tích hợp trên nền tảng AutoCAD .NET.

---

## 1. Cấu trúc thư mục

```text
products/meps/
├── src/                          # Mã nguồn C#
│   ├── Toado/                    # Module nhãn tọa độ T-TOADO (V4)
│   │   └── ToadoPart.cs          # Mã nguồn nâng cấp: căn giữa chữ, triệt tiêu gạch thừa
│   └── Modules/                  # Các module C# nghiệp vụ điện (Room, Route, Wire, Tag...)
├── tools/                        # Kịch bản và công cụ build
│   ├── base/
│   │   ├── meps_orig.il          # Base IL dịch ngược gốc (bảo toàn 1.295 types & lệnh)
│   │   └── meps_orig.res         # Resource gốc cho PE
│   ├── build_v4.cmd              # Kịch bản 1-click tự động build và đóng gói V4
│   ├── merge_toado.cs            # Kịch bản hợp nhất IL của ToadoPart vào base IL
│   ├── update_v4.cs              # Kịch bản tính và đồng bộ mã SHA256 cho installer
│   ├── build.mjs                 # Script build tự động hóa (Codex)
│   └── compare-v4.mjs            # Script đối soát IL chuẩn hóa
├── test/
│   └── Mat bang cap dien trung the.dwg  # Bản vẽ mẫu phục vụ kiểm thử
└── release/
    └── TVXD.BG_V4/               # Gói cài đặt hoàn chỉnh sẵn sàng sử dụng
        ├── CAI_DAT.cmd           # File chạy cài đặt 1-click
        ├── CAI_DAT.exe           # Trình cài đặt tự động
        ├── MEPS060/              # Thư viện khối, SDNL và TVXD.Meps.dll (V4)
        ├── PHIEN_BAN.json        # Thông tin phiên bản & mã băm
        └── SHA256SUMS.json       # Danh mục SHA256 kiểm tra toàn vẹn
```

---

## 2. Điểm nâng cấp trong phiên bản V4 (`T-TOADO`)

1. **Khắc phục triệt để dấu gạch thừa bên trên:**
   - Đồng bộ `TextAttachmentType.AttachmentBottomOfTopLine` và `TextAttachmentDirection.AttachmentHorizontal` cho toàn bộ 4 hướng (`LeftLeader`, `RightLeader`, `TopLeader`, `BottomLeader`), loại bỏ hoàn toàn nét gập thừa nằm trên đỉnh chữ X khi trỏ hướng lên trên.
2. **Căn chữ ra chính giữa:**
   - Đặt `TextAlignmentType.CenterAlignment` và thu gọn `DoglegLength` vừa vặn tỷ lệ chữ, giúp 2 dòng X và Y cân đối hai đầu đường gạch ngang.

---

## 3. Hướng dẫn phát triển & Build phiên bản mới

1. Chỉnh sửa logic hoặc giao diện trong file `src/Toado/ToadoPart.cs`.
2. Mở Command Prompt / Terminal tại thư mục `tools/` và chạy:
   ```cmd
   build_v4.cmd
   ```
3. Kịch bản sẽ tự động:
   - Biên dịch `ToadoPart.cs` $\to$ dịch ngược `ToadoPart.il`.
   - Ghép an toàn vào `tools/base/meps_orig.il`.
   - Lắp ráp ra file nhị phân `TVXD.Meps.dll` (bảo toàn 100% 1.295 types và lệnh T-MEP).
   - Nạp vào gói cài đặt `release/TVXD.BG_V4/MEPS060/` và đồng bộ lại mã SHA256.

---

## 4. Cài đặt và sử dụng trên AutoCAD 2021

- Chạy `release/TVXD.BG_V4/CAI_DAT.cmd`.
- Khởi động AutoCAD, gõ lệnh `T-TOADO` để gắn nhãn tọa độ điểm hoặc block thiết bị.
