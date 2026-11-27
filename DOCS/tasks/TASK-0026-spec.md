# Đặc tả Kỹ thuật TASK-0026: Lệnh `T-THTTLL` Bóc tách Khối lượng Hệ thống Thông tin Liên lạc (TTLL)

- **Task ID:** `TASK-0026`
- **Nhánh:** `task/TASK-0026-lisp-t-thttll` (base: `main` @ `609806d5cc2b6b366aace248a9bb55b8ace2d89f`)
- **Cấp độ:** Khó (`ROLES.md`)
- **Chủ sở hữu:** Phùng Anh Tuấn (chốt Phương án A + file chuẩn `MB TTLL.dwg` + tên lệnh `T-THTTLL`)
- **Tổng chỉ huy / Thiết kế:** `Gemini 4 Hydrogen (Extra High)`
- **Writer:** `Gemini 4 Hydrogen (Medium)` (1 Writer duy nhất)
- **Reviewer độc lập:** `Codex GPT-6.2 Astra` (qua `codex.exe exec`)

## 1. Mục tiêu & Phạm vi
Xây dựng công cụ AutoLISP độc lập cho AutoCAD 2021+ (`LISPSYS=1`) tại `products/meps/src/bkl_ttll.lsp`:
1. Cung cấp lệnh **`T-THTTLL`** (và alias `BKL_TTLL`, `TTLL`) cho phép người dùng quét chọn vùng bản vẽ TTLL, thu thập toàn bộ nút (`BE2 DAN59`, `ganivo`, `tupptt`, `Cot Dien`), polyline tuyến (`TC vien thong`, `KT`), nhãn cột (`MTEXT`/`TEXT` `TÊN CỘT:`) và toàn bộ `DIMENSION` (`S-DIM`, `RSA -DIM`, ...).
2. Bóc tách **trung thực 100% theo đối tượng và nhãn trên bản vẽ CAD** (Phương án A — không hardcode ngoại lệ riêng của bất kỳ bản vẽ nào). Mọi sai khác trên bản vẽ (dim cụt không có polyline/nút, dim `D110` nằm cô lập giữa nhịp `2D110`, nút thiếu `DIMENSION` nối tới, `QĐHT > KC`, v.v.) phải được liệt kê rõ ràng trong **Cảnh báo đối soát CAD**.
3. Xuất ra tệp Excel `.xlsx` gồm đúng **3 sheet** giống cấu trúc và toàn bộ công thức động của mẫu `04._BANG_THKL_THONG_TIN_LIEN_LAC.xlsx`:
   - `THKL` (`A1:E18` với đầy đủ 16 hạng mục liên kết động sang `DG TTLL`, gồm cả `D7 =D3*2*3`, `D8 =D3*2`, `D17 =D16`, `D18 =ROUNDUP(D12/15,0)` + khu vực đối soát cảnh báo từ dòng 21 trở xuống nếu có cảnh báo).
   - `DG TTLL` (43 cột `A..AQ`, 2 hàng tiêu đề gộp ô `A2:A3`, `B2:C2`, `D2:D3`, `E2:H2`, `I2:M2`, `N2:R2`, `S2:AK2`, `AL2:AO2`, `AP2:AP3`, `AQ2:AQ3`, đầy đủ công thức cột `A, I..AK` cho từng dòng và hàng `Tổng cộng` dùng `=SUM(...)`).
   - `DM LOAI ONG` (8 loại tuyến: `1D110`, `2D110`, `3D110`, `4D110`, `D60`, `32/25`, `110/90`, `65/50` + hệ số ống để `INDEX/MATCH` ở cột `M` hoạt động).
4. Không phụ thuộc hàm COM dễ lỗi trong `accoreconsole.exe` (`layoutlist`, `vla-get-Layouts`); thu thập CAD hoàn toàn bằng DXF (`entget`, `tblsearch`, `entnext`, `trans`), và cung cấp hàm chạy không hộp thoại `(bkl-ttll:run-with-path sel outPath)` cho test tự động.

## 2. Yêu cầu Thuật toán Chi tiết (`products/meps/src/bkl_ttll.lsp`)

### 2.1. Thu thập thực thể (DXF-only)
1. **Lọc ký hiệu (`KÝ HIỆU`):** Bỏ qua `BE2 DAN59`, `ganivo`, `tupptt` có giá trị thuộc tính nhãn rỗng (`""`). Với `LWPOLYLINE` và `DIMENSION`, bỏ qua những đối tượng thuộc cụm ký hiệu (nằm cô lập không gần nút mạng có nhãn nào trong bán kính `15 m`).
2. **Tính tọa độ nút thực địa trên WCS:**
   - **Quan trọng (`KNOWLEDGE.md`):** `BE2 DAN59` có điểm chèn DXF `10` bị trôi xa. Với mọi `INSERT` (`BE2 DAN59`, `ganivo`, `tupptt`), biến đổi toàn bộ đỉnh `LWPOLYLINE`, `LINE`, `CIRCLE` bên trong block definition (`tblsearch "BLOCK"`) từ OCS block sang WCS (ma trận scale `(41,42,43)`, rotation `50`, WCS `10` của insert qua `(trans ... entity 0)`). **Bỏ qua `WIPEOUT`** (DXF `11` của `WIPEOUT` là vector kích thước). Lấy tâm bounding box WCS nếu có đỉnh hình học; fallback về DXF `10` nếu không có.
   - Đọc nhãn `ATTRIB`: tìm tag `TVXD_LABEL_033` hoặc `ATTRIB` đầu tiên khác rỗng (`strtrim`).
   - Chuẩn hóa tên `ganivo`: nếu bắt đầu bằng `B` theo sau là số (`B1..B151`), chuẩn hóa hiển thị thành `G<số>` (`G1..G151`) để đồng bộ `DG TTLL` (giữ nguyên nếu đã là `G...` hoặc `D...`).
   - **`Cot Dien` (`cht` / `cct`):** lấy tâm WCS (ưu tiên DXF `10` -> WCS); tìm nhãn cột từ `MTEXT`/`TEXT` chứa `TÊN CỘT:` (strip format MTEXT kiểu `bklcap:plain`, lấy phần sau `TÊN CỘT:` và trước `- LOẠI CỘT` / `-`). Nếu có `LEADER` trỏ từ hộp chữ vào gần cột thì ưu tiên tọa độ đầu mũi tên `LEADER` (`<= 8 m`), ngược lại gắn với `POLE` gần nhất trong `<= 30 m`. Nếu cột không có text, đặt tên `CHT` / `CCT`.
3. **Đọc `DIMENSION`:**
   - Hỗ trợ mọi `DIMENSION` (layer `S-DIM`, `RSA -DIM`, v.v.).
   - Lấy text từ block ẩn `(cdr (assoc 2 ed))` (`*Dxxx` -> duyệt `entnext` tìm `MTEXT`/`TEXT`), fallback `(cdr (assoc 1 ed))`. Giải mã `\U+0110` -> `Đ` / `\U+0111` -> `đ`, strip mã MTEXT.
   - Tách loại nhãn & giá trị chiều dài trong ngoặc `(...)`:
     - `4D110`, `3D110`, `2D110`, `1D110` (`D110`), `D60` (`2D60`/`D60`), `32/25`, `110/90`, `65/50`, `QD` (`QĐ`).
     - Điểm đo `p13` = `(13)`, `p14` = `(14)` theo WCS XY; `meas` = `(distance p13 p14)`. Giá trị = số trong ngoặc `(XXm)`, fallback làm tròn `meas` nếu không có số.

### 2.2. Xây dựng Đồ thị Nhịp cho từng Mạng ống
Chạy tách biệt từng mạng (`4D110`, `3D110`, `2D110`, `1D110`, `D60`, `32/25`, `110/90`, `65/50`):
- **Định nghĩa Terminal cứng theo mạng:**
  - `4D110`, `3D110`, `2D110`: terminal cứng = `BE` (cho phép `POLE` ở đầu mút cụt `deg == 1`).
  - `1D110`: terminal cứng = `BE`, `TU` (cho phép `POLE` ở đầu mút cụt `deg == 1` khi có polyline `TC vien thong` hoặc nhãn cột hỗ trợ).
  - `D60`: terminal cứng = `TU`, `GA`.
  - `32/25`, `110/90`, `65/50`: terminal cứng = `BE`, `TU`, `GA`.
- **Snap thông minh 2 pha (tránh nuốt cạnh ngắn `4 m` & `1 m`):**
  - Bán kính snap ban đầu: `2.6 m` với `BE`/`TU`, `2.2 m` với `GA`.
  - **Chống sập cạnh `1 m` (`B32 <-> TT-07`):** nếu 2 đầu `p13`, `p14` snap cùng vào 1 terminal `F`, xét mọi cặp terminal khác nhau `(N1, N2)` đều nằm trong `<= 3.0 m` của `(p13, p14)` và chọn cặp có `d(p13, N1) + d(p14, N2)` nhỏ nhất.
  - Đầu mút chưa phải terminal được gộp đỉnh theo `vertexTol = 2.2 m`.
  - **Khớp đầu cụt (`deg == 1`) sau khi dựng cạnh:** với đỉnh phi-terminal có đúng `1` cạnh nối tới:
    1. Tìm terminal cứng của mạng trong `<= 5.5 m` (khác với terminal đầu kia của chính cạnh đó — giải quyết sạch `B9` `3.7 m`, `B35` `3.8 m`).
    2. Nếu không thấy và mạng cho phép `POLE`, tìm `POLE` trong `<= 7.5 m`: với `1D110`, kiểm tra điều kiện có polyline hỗ trợ tại vùng cột/dim và ưu tiên `POLE` có nhãn `TÊN CỘT:` hoặc không nằm ngay trên nhịp `2D110` đi thẳng qua (tránh gắn nhầm `D110(5m)` ở `B55->B54` vào cột lề đường).
- **Thu gọn đỉnh trung gian & xử lý đỉnh giao bậc 4 (`B56` gần `G100`):**
  - Đỉnh phi-terminal `deg == 2` tự động nối thẳng 2 cạnh qua nó.
  - Đỉnh phi-terminal `deg == 4`: xét 3 cặp ghép đối đỉnh `((0-1, 2-3), (0-2, 1-3), (0-3, 1-2))`, chọn cặp có tổng cos góc hướng nhỏ nhất (thẳng hướng nhất).
- **Chuỗi dim -> 1 dòng `DG TTLL`:**
  - Nếu có >= 2 dim nối tiếp trên 1 nhịp, lưu danh sách giá trị để xuất công thức `=a+b+...` vào cột `E`, và ghi chi tiết `a+b+...` vào cột `Ghi chú` (`AQ`).
  - Chuỗi không đóng được ở 2 đầu terminal hoặc cạnh cô lập -> đưa vào danh sách `WARNINGS`.

### 2.3. Ánh xạ `QĐ(...)` & Kiểm tra Duy nhất Nút
- Mỗi dim `QĐ`: tìm nhịp (`4D110` / `3D110` / `2D110` / `1D110`) mà cả `p13` và `p14` đều cách đoạn gấp khúc của nhịp `<= 4.5 m` (ưu tiên `max(d1, d2)` nhỏ nhất; nếu bằng nhau ưu tiên trục chính `4D110/2D110`). Cộng dồn vào cột `G` (`=q1+q2` nếu có từ 2 dim `QĐ` trở lên). Nếu không gắn được vào nhịp nào -> ghi `WARNINGS`.
- **Định hướng & sắp xếp:**
  - Định hướng tuyến `2D110`: `Cột -> B_i`, `B_i -> B_{i+1}` (nhịp nhỏ trước lớn sau), `B_cuối -> Cột`; `1D110` tủ: `BE -> TT-xx` (theo số `TT`); `1D110` cột: `BE -> Cột ...` (theo số `B`); `D60`: chạy BFS từ các `TT-xx` để định hướng nhánh `TT-xx -> G...` và `G -> G` theo chiều xa dần khỏi tủ, sắp xếp theo `G`.
- **Đếm duy nhất mỗi nút (`1` lần cho mỗi nút thực địa trong vùng chọn):**
  - Mỗi `BE` đếm `1` ở cột `AL` (ưu tiên tại dòng `2D110` có `BE` là điểm cuối `To` khi đi xuôi `B1..B66`, phần còn lại đảm bảo vẫn đếm đủ `1`).
  - Mỗi `GA` đếm `1` ở cột `AN` (ưu tiên tại dòng `D60` có `GA` là điểm cuối `To`, đảm bảo mỗi `GA` thực địa xuất hiện đều đếm đúng `1`).
  - Mỗi `TU` đếm `1` ở cột `AO` tại dòng cấp nguồn `1D110` (`BE -> TT-xx`).
  - Cảnh báo các nút `BE`, `GA`, `TU` có trong vùng chọn nhưng không nối được vào nhịp nào (ví dụ `TT-03` thiếu dim `1D110` tới `B4`).

## 3. Tạo file Excel `.xlsx` & CSV
- `products/meps/src/bkl_ttll.lsp` nhúng script PowerShell tạo tệp `.xlsx` OpenXML (`System.IO.Compression.ZipFile`) chuẩn 3 sheet (`THKL`, `DG TTLL`, `DM LOAI ONG`) với **cả thẻ `<f>` công thức và `<v>` giá trị đã tính sẵn**, định dạng `Times New Roman`, căn lề, gộp ô, độ rộng cột và kẻ khung giống `04._BANG_THKL_THONG_TIN_LIEN_LAC.xlsx`.
- Đồng thời xuất file CSV (UTF-8 BOM, phân tách `;` hoặc `,`) nếu người dùng chọn lưu `.csv` hoặc dùng cho test tự động.

## 4. Tiêu chí Nghiệm thu & Kế hoạch Kiểm thử
1. **Unit & Integration Test tổng hợp (công khai, được phép commit vào `products/meps/test/`):**
   - Tạo script AutoLISP dựng một bản vẽ tổng hợp (trình tự từ `BE` có basepoint trôi xa kiểu `BE2 DAN59`, `ganivo`, `tupptt`, `Cot Dien` + `MTEXT` + `LEADER`, `TC vien thong`, `KT`, đủ cả `4D110`, `2D110`, `1D110`, `2D60`, `QĐ`, cạnh ghép nhiều dim, nút giao bậc 4, cạnh ngắn `1m` và 1 dim dead-end), chạy `T-THTTLL` trên `accoreconsole.exe` và kiểm tra tự động bằng Node `test/bkl_ttll.test.mjs` khớp 100% giá trị từng dòng + tổng cộng + cảnh báo.
2. **Native Verification trên `MB TTLL.dwg` (chạy trong `scratch/`, KHÔNG commit DWG khách hàng):**
   - Chạy `bkl_ttll.lsp` qua `accoreconsole.exe` trên bản sao `scratch/mb_ttll.dwg`, tạo cả `.csv` và `.xlsx`, kiểm chứng Excel mở sạch bằng Excel COM (nếu khả dụng) và đối chiếu 100% số lượng từng hạng mục, tổng chiều dài từng loại nhịp, `31/31` dim `QĐ = 397m`, `66/66 BE`, `151/151 GA`, `13/14 TU` + cảnh báo chính xác `TT-03`, `2D110(38m)`, `D110(40m/19m/26m)`, `4D110(17M)` `B15-B51`, `B22-B23` (`84m`).
3. **Cổng repo & Review độc lập:**
   - Chạy `node --test services.test.mjs workflow-check.test.mjs`, `node workflow-check.mjs`.
   - Đẩy diff + bằng chứng test sang `Codex GPT-6.2 Astra` via `codex.exe exec` đánh giá `ĐẠT` trước khi trình Chủ sở hữu.
