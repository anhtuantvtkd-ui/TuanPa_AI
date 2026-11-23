# TRẠNG THÁI DỰ ÁN (PROJECT STATE)
Cập nhật lần cuối: 2026-11-22 21:20 | Phụ trách điều phối: Gemini Argon & Codex

---

## 1. Mục tiêu vừa hoàn thành: Phát hành chính thức phiên bản TVXD.BG V9
Chủ sở hữu (Phùng Anh Tuấn) đã trực tiếp chỉ đạo xử lý dứt điểm theo Cách 1:
> *"cách 1 nhé"* & *"sao lại phải nhắn đã kết nối lên repo hết rồi mà"*

- **Các cải tiến kỹ thuật đột phá trong bản V9**:
  1. **Khắc phục triệt để lỗi Copy MLeader sang block khác (Re-linking)**:
     - Khi copy (`CO`/`COPY`) nhãn từ Block A sang Block B và kéo mũi tên vào Block B, khi bấm "CẬP NHẬT TẤT CẢ" (`RefreshAll`/`T-TOADOREFRESH`), nhãn **ở nguyên vị trí mới tại Block B**, tự động nhận diện Block B gần mũi tên nhất, cập nhật lại XData và đổi tọa độ sang Block B, không bị lôi ngược về Block A cũ.
     - Kiểm thử native engine trên `Mat bang cap dien trung the.dwg` bằng `TestRelinkLogic.cs`: Nhãn 1 giữ nguyên tại Block 1 (`406387.66`), Nhãn 2 sau khi copy sang Block 2 (`406406.36`) được re-link chuẩn xác sang handle Block 2 (`14968C`) và cập nhật tọa độ độc lập.
  2. **Khắc phục lỗi đặt 45° quét xong ra 90° & Tỷ lệ khoảng cách**:
     - Khi chọn "Tự động lệch góc cố định 45°", nhãn xuất ra chính xác góc nghiêng 45° với vai ngang (dogleg) và đường gạch gióng xen giữa X và Y.
     - Trong `CreateOrUpdateLeader()`, explicitly reset `MText.Rotation = 0.0` và `MText.Direction = (1, 0, 0)`, ngăn AutoCAD lưu giữ góc xoay 90° của nhãn đứng cũ.
     - Bổ sung ô nhập `Khoảng cách (Distance)` trên Form cài đặt và cơ chế auto-scale `Distance = Height * 3.5` nếu giá trị cũ quá nhỏ.
     - Kiểm thử `TestBatch45.cs` trên toàn bộ 14 block của bản vẽ thật: 14/14 block ra góc nghiêng 45° chuẩn xác (`Angle = 45 deg, Dogleg = True, Landing = True`).
  3. **Bảo lưu 100% các tính năng V8 đã nghiệm thu**:
     - AutoCAD Dogleg shoulder (`EnableDogleg = true`): giữ nguyên vai ngang khi grip-drag né vật cản (`GripDragTest.cs`: 6/6 PASS).
     - Chữ trong suốt (`BackgroundFill = false`): không che nền trắng.
     - Khóa đường gạch xen giữa dòng X và Y ổn định 360° (`DetailedMatrixTest.cs`: 16/16 góc xoay PASS 100%, drift = 0).
- **Biên dịch & Đóng gói sản phẩm**:
  - Biên dịch hợp nhất nguyên vẹn 1.295 MEPS types qua `build_v4.cmd`. Mã băm SHA-256 của `TVXD.Meps.dll`: `14177f8f92952e9e124e8fa34382e9007db8e42c936973fadeb70e6bece178e6`.
  - Tệp kiểm toán `SHA256SUMS.json`: 293/293 files khớp 100% (0 sai lệch).
  - Thư mục phát hành chính thức: [9. TVXD.BG V9](file:///D:/TuanPa_AI/OUTPUT/9.%20TVXD.BG%20V9).
  - Live runtime đã cập nhật: `%LOCALAPPDATA%\TVXD\MEPS060\TVXD.Meps.dll`.
- **Hồ sơ nghiệm thu**: `work-items/TASK-0016.json` đã hoàn thiện và đạt chuẩn `RELEASED`.

---

## 2. Kế hoạch tiếp theo (Next Steps / Backlog)

1. **TASK-0017 (CAD Tooling Backlog)**:
   - Phát triển lệnh `T-TOADOXUAT` (hoặc `T-TOADOTABLE`): Đọc XData `TVXD_TOADO` từ các nhãn đã gắn trong bản vẽ để xuất tự động ra bảng tọa độ CAD (Table) hoặc tệp Excel/CSV.
2. **Khép kín hồ sơ cũ (`TASK-0011`, `TASK-0014`, `TASK-0015`)**:
   - Hoàn tất evidence và đóng task theo quy trình `WORKFLOW.md`.

---

## 3. Lệnh kiểm tra nhanh
```powershell
Set-Location 'D:\TuanPa_AI'
# 1. Kiểm tra tính hợp lệ toàn bộ hồ sơ workflow:
node workflow-check.mjs

# 2. Chạy toàn bộ test tự động:
node --test services.test.mjs workflow-check.test.mjs

# 3. Biên dịch lại toàn bộ sản phẩm:
cmd.exe /c "cd /d D:\TuanPa_AI\products\meps\tools && build_v4.cmd"
```


---

## 4. TRẠNG THÁI TIẾP NHẬN — TASK-0018 BASEPOINT FIX
- **STATE**: `IN_PROGRESS`
- **Task**: TASK-0018
- **Codex**: Đã được cung cấp đầy đủ source tại `products/meps/tools/tagseqcad_orig.il` và `products/meps/src/TagSeq/TagSeqCadDecompiled.cs`.
- **Blocker**: ĐÃ GIẢI TỎA (RESOLVED)
- **NEXT_ACTOR**: Codex
- **Hành động tiếp theo**: Codex sửa `TagSeqPart.cs`, cập nhật `merge_toado.cs`, chạy build và bàn giao cho Gemini kiểm thử native trên CAD.
