# TASK-0024 SPECIFICATION
## AUTOLISP BÓC TÁCH KHỐI LƯỢNG ĐƯỜNG DÂY 0,4KV SINH HOẠT ĐẾN HỘ DÂN XUẤT CSV

### 1. MỤC TIÊU VÀ BỐI CẢNH
- **Yêu cầu người dùng:** Viết 1 chương trình AutoLISP cho AutoCAD để kỹ sư quét chọn các đối tượng trên bản vẽ (gồm Dim chiều dài, tủ điện, số nhà, ghi chú cáp), nhận diện chiều dài và các thông số, sau đó tự động tính toán và xuất ra file CSV chuẩn khớp 100% với cấu trúc file mẫu `D:\TuanPa_AI\INPUT\BKL CAP DIEN\Book1.xlsx`.
- **Đặc điểm quan trọng:** Chiều dài có thể thay đổi linh hoạt theo số Dim (nhận diện theo `Measurement` thực tế hoặc `TextOverride` của Dim khi quét chọn).

---

### 2. DỮ LIỆU ĐẦU VÀO TRÊN BẢN VẼ (`CAP DIEN SCT.dwg`)
1. **Block Tủ điện (`Tu Cong To`):**
   - Thuộc tính `TEN_TU`: ví dụ `II-TÐ-01`
   - Thuộc tính `TEN_TBA`: ví dụ `TBA DT`
   - Tọa độ tâm/điểm đặt tủ $P_{tu} = (X_{tu}, Y_{tu})$
2. **Các đối tượng Kích thước (Dimension) trên tuyến cáp:**
   - Các Dimension nằm dọc theo tuyến hè/đường đo khoảng cách giữa các lô đất và từ tủ điện đến các lô đất (ví dụ các đoạn 9m, 7m, 8m...).
   - Dimension đo đoạn rẽ nhánh vào đầu lô đất (ví dụ đoạn 1.7m ~ 2m).
   - Chiều dài mỗi đoạn dim: lấy theo `TextOverride` nếu có gõ đè số dim; nếu để mặc định `<>` lấy theo giá trị đo `Measurement` làm tròn số nguyên gần nhất (hoặc giữ nguyên số lẻ nếu có).
3. **Các đối tượng Text/MText Số nhà:**
   - Nội dung dạng `SỐ NHÀ 1`, `SỐ NHÀ 2`, ..., `SỐ NHÀ 12` (hoặc số nhà bất kỳ).
   - Tọa độ vị trí tương ứng với lô đất. Mỗi lô đất thường gồm 1 cặp hộ dân (ví dụ: Nhà 1 & 2; Nhà 3 & 4; Nhà 5 & 6; Nhà 7 & 8; Nhà 9 & 10; Nhà 11 & 12).
4. **Các đối tượng Text/MText Chủng loại cáp nhánh:**
   - Ghi chú cáp tương ứng cho từng hộ:
     - `DSTA 2*10` hoặc `DSTA 2x10`: Cáp 1 pha 2x10mm², công tơ 1F.
     - `DSTA 16` hoặc `DSTA 3*16+1*10`: Cáp 3 pha 3x16+1x10mm², công tơ 3F.

---

### 3. QUY TẮC TÍNH TOÁN THEO BIỂU MẪU `Book1.xlsx`
Cho mỗi dòng hộ dân (tại hàng $r$ trong bảng tính Excel):
1. `STT`: Số thứ tự tủ (dòng tiêu đề tủ ghi 1, 2...; các dòng số nhà để trống).
2. `Tên tủ`: Tên tủ (dòng tiêu đề ghi ví dụ `II-TĐ-01/DT`; dòng số nhà để trống).
3. `Số phụ tải (hộ)`: Dòng tiêu đề ghi tổng số hộ (ví dụ 11 hoặc 12); dòng chi tiết ghi tên số nhà (`Số nhà 1`, `Số nhà 2`...).
4. `Từ tủ điện đến lô đất` (Cột D):
   - Bằng tổng các đoạn Dim từ vị trí tủ điện đến vị trí lô đất tương ứng.
   - Ví dụ bên trái tủ: Lô 4 cạnh tủ = 2m; Lô 3 = 9m; Lô 2 = 9 + 7 = 16m; Lô 1 = 9 + 7 + 7 = 23m.
   - Bên phải tủ: Lô 5 = 9m; Lô 6 = 9 + 8 = 17m.
   - Có thể xuất ra dạng công thức `=9+7+7` hoặc giá trị số `23`.
5. `Từ rãnh cáp đến đầu lô đất` (Cột E): Cố định = `2.0` (m).
6. `Lên xuống rãnh cáp` (Cột F): Cố định = `2.2` (m) (hoặc công thức `=0.7+1.5`).
7. `Lên điểm đấu vào nhà hiện trạng + dự phòng` (Cột G): Cố định = `3.0` (m).
8. `rãnh cáp từ trục chính đến hộ dân (m)` (Cột H):
   - Hộ thứ 1 trong lô (hộ lẻ): `= 2.0` (hoặc `=E{r}`).
   - Hộ thứ 2 trong lô (hộ chẵn): `= 0` (vì đào chung rãnh nhánh vào lô đất).
9. `Số công tơ 1F (2x10mm2)` (Cột I): `= 1` nếu là cáp 2x10, ngược lại để trống hoặc `= 0`.
10. `Số công tơ 3F (3x16+1x10)` (Cột J): `= 1` nếu là cáp 3x16+1x10, ngược lại để trống hoặc `= 0`.
11. `Ống HDPE 40/30 (DSTA 10)` (Cột K):
    - Nếu là 1F: công thức `=D{r}+E{r}+F{r}+G{r}-2`
    - Nếu là 3F: `= 0`
12. `Ống HDPE 50/40 (DSTA 16)` (Cột L):
    - Nếu là 3F: công thức `=M{r}-2` (hoặc `=D{r}+E{r}+F{r}+G{r}-2`)
    - Nếu là 1F: `= 0`
13. `Cáp ngầm 3x16+1x10` (Cột M): Công thức `=(D{r}+E{r}+F{r}+G{r})*J{r}`
14. `Cáp ngầm 2x10` (Cột N): Công thức `=(D{r}+E{r}+F{r}+G{r})*I{r}`

---

### 4. ĐẶC TẢ LỆNH AUTOLISP
- **Tên lệnh:** `BKL_CAP` (hoặc `BKLCAP`)
- **Cách thức hoạt động:**
  1. Người dùng gõ lệnh `BKL_CAP`.
  2. Lisp nhắc: `\nQuet chon khu vuc gom Tu dien, Dim tuyen cap va So nha <Enter de chon tat ca>: `
  3. Lisp tự động lọc và gom nhóm:
     - Tủ điện: lấy tên tủ, tọa độ $X_{tu}$.
     - Tuyến Dim: tự động sắp xếp theo thứ tự tọa độ X, tạo bản đồ tích lũy khoảng cách từ tủ điện sang 2 phía (trái/phải).
     - Số nhà & cáp: ghép cặp số nhà theo lô đất, xác định khoảng cách tương ứng và loại cáp 1 pha hay 3 pha.
  4. Lisp mở hộp thoại lưu file (hoặc xuất ra file CSV tự động cạnh file DWG):
     `D:\TuanPa_AI\INPUT\BKL CAP DIEN\BKL_CAP_DIEN.csv`.
  5. Xuất thông báo hoàn thành kèm tổng số hộ dân và đường dẫn file CSV.
