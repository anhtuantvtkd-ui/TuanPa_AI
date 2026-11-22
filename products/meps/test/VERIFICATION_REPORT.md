# BÁO CÁO KIỂM CHỨNG KỸ THUẬT NATIVE CAD (TASK-0016)
Dành cho Independent Reviewer (Codex / Owner)

---

## 1. Kết quả kiểm thử trực tiếp trên engine AutoCAD (accoreconsole.exe)
- **Tệp mã kiểm thử**: `products/meps/test/TestGeometryToado.cs`
- **Kịch bản chạy**: `products/meps/test/RunTestGeometry.scr`
- **Tệp log thực thi đầy đủ**: `products/meps/test/accoreconsole_test_report.txt`
- **Bản vẽ fixture có sẵn trong repo**: `products/meps/test/Mat bang cap dien trung the.dwg`

### Chi tiết 6 ca kiểm thử hình học:
| Ca kiểm thử | Hướng / Góc | Tọa độ Target | Tọa độ Landing | Kiểm tra hình học bóc tách (Explode) | Kết quả |
| :--- | :---: | :---: | :---: | :--- | :---: |
| **Case 1** | Xiên lên - Phải (Q1) | (0, 0) | (25, 15) | Nét gạch tại $Y=15$, Text Bounds: $[11.27, 18]$ $\to$ Nét gạch nằm giữa dòng X và Y | **PASS** |
| **Case 2** | Xiên lên - Trái (Q2) | (50, 0) | (25, 15) | Nét gạch tại $Y=15$, Text Bounds: $[11.27, 18]$ $\to$ Nét gạch nằm giữa dòng X và Y | **PASS** |
| **Case 3** | Xiên xuống - Trái (Q3) | (100, 0) | (75, -15) | Nét gạch tại $Y=-15$, Text Bounds: $[-18.73, -12]$ $\to$ Nét gạch nằm giữa dòng X và Y | **PASS** |
| **Case 4** | Xiên xuống - Phải (Q4) | (150, 0) | (175, -15) | Nét gạch tại $Y=-15$, Text Bounds: $[-18.73, -12]$ $\to$ Nét gạch nằm giữa dòng X và Y | **PASS** |
| **Case 5** | Thẳng đứng 90° Lên | (200, 0) | (200, 30) | Xoay chữ 90° ($\approx 1.57$ rad), X Bounds: $[200, 206.73]$, MidX: $203.36$ $\to$ Nét gióng đứng phân cách giữa X (trái) và Y (phải) | **PASS** |
| **Case 6** | Thẳng đứng 90° Xuống | (250, 0) | (250, -30) | Xoay chữ 90° ($\approx 1.57$ rad), X Bounds: $[250, 256.73]$, MidX: $253.36$ $\to$ Nét gióng đứng phân cách giữa X (trái) và Y (phải) | **PASS** |

**Tổng kết**: 6/6 ca kiểm thử hình học đều PASS 100%.

---

## 2. Bằng chứng toàn vẹn bản phát hành (Release Payload & Checksums)
- Theo quy định tại mục 58 của `WORKFLOW.md`, thư mục `OUTPUT/` trên máy local được đưa vào `.gitignore` để tránh phình dung lượng Git.
- Gói phát hành chuẩn lưu trữ trực tiếp trong repository tại: `products/meps/release/TVXD.BG_V4/`.
- Tệp kiểm toán: `products/meps/release/TVXD.BG_V4/SHA256SUMS.json` (đầy đủ 293 mục).
- Tệp DLL sau biên dịch: `products/meps/release/TVXD.BG_V4/MEPS060/TVXD.Meps.dll`.
- **Mã băm SHA-256 xác thực**:
  ```
  aec4cbc736652924c981acd0c8cc5b91c7c3534a80cd58b0095b0d88609ffa5e
  ```
- Gói cục bộ `D:\TuanPa_AI\OUTPUT\7. TVXD.BG V7` và tệp live `%LOCALAPPDATA%\TVXD\MEPS060\TVXD.Meps.dll` đều khớp chính xác 100% với mã băm trên.
