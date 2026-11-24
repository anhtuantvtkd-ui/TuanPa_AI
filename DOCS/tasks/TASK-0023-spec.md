# TASK-0023 SPECIFICATION
## BÓC TÁCH KHỐI LƯỢNG ĐƯỜNG DÂY HẠ THẾ 0,4KV - CÔNG TRÌNH ĐƯỜNG THÂN KHUÊ, PHƯỜNG ĐA MAI, TỈNH BẮC NINH

### 1. THÔNG TIN DỰ ÁN VÀ CƠ SỞ THIẾT KẾ
- **Tên dự án:** Cải tạo, nâng cấp đường Thân Khuê, phường Đa Mai, TP Bắc Ninh.
- **Hạng mục:** Cấp điện sinh hoạt 0.4kV (Bản vẽ MBSH-01 đến MBSH-22, gồm 23 trang).
- **Chủ đầu tư:** Ban quản lý dự án đầu tư xây dựng phường Đa Mai.
- **Đơn vị tư vấn thiết kế:** Công ty CP Kiến trúc Sáng tạo T&D.
- **Tiêu chuẩn bảng kê:** Bảng kê khối lượng đường dây hạ thế 0,4kV theo biểu mẫu chuẩn của kỹ sư thiết kế (gồm các cột: Tên lộ, Từ tủ, Đến tủ/cột, Khoảng cách, Hao hụt trùng cáp 1%, Lên cột/tủ, Xuống cột/tủ, Dự phòng lèo, Tổng cộng chiều dài cáp, Tiếp địa T4C-1.5, Các chủng loại cáp ngầm Cu/XLPE/PVC/DSTA/PVC, Rãnh cáp hè/đường/hiện trạng, Trùng rãnh, Ống luồn HDPE 110/90, 105/80, 85/65, Ống qua đường UPVC D140, D110, Tủ công tơ, Tủ bù, Đầu cáp co nhiệt, Xà giá, Phụ kiện...).

---

### 2. QUY CÁCH KỸ THUẬT VÀ QUY TẮC TÍNH TOÁN
1. **Chủng loại cáp & Ống luồn HDPE gân xoắn:**
   - Cáp `Cu/XLPE/PVC/DSTA/PVC 3x150+1x120mm2` (DSTA 150): Luồn trong ống nhựa gân xoắn HDPE `110/90`.
   - Cáp `Cu/XLPE/PVC/DSTA/PVC 3x120+1x95mm2` (DSTA 120): Luồn trong ống nhựa gân xoắn HDPE `105/80`.
   - Cáp `Cu/XLPE/PVC/DSTA/PVC 3x95+1x70mm2` (DSTA 95): Luồn trong ống nhựa gân xoắn HDPE `105/80`.
   - Cáp `Cu/XLPE/PVC/DSTA/PVC 3x70+1x50mm2` (DSTA 70): Luồn trong ống nhựa gân xoắn HDPE `85/65`.
   - Cáp `Cu/XLPE/PVC/DSTA/PVC 3x50+1x35mm2` (DSTA 50): Luồn trong ống nhựa gân xoắn HDPE `85/65`.

2. **Ống luồn bảo vệ qua đường (UPVC):**
   - Đoạn cáp cắt qua đường `QĐ(...M)` bổ sung ống UPVC:
     - Ống UPVC `D140`: Dùng cho cáp có tiết diện từ DSTA 95 đến DSTA 150.
     - Ống UPVC `D110x5mm`: Dùng cho cáp có tiết diện DSTA 50 và DSTA 70.
     - Với các đoạn ghi `2QĐ`: Bố trí 2 đường ống qua đường song song.

3. **Quy tắc dự phòng, hao hụt & tính chiều dài cáp:**
   - `Hao hụt, trùng cáp (1%)` = `Khoảng cách * 0.01`
   - `Lên cột, tủ (m)`: 2.5m đối với tủ điện (TBA, Tủ phân phối, Tủ công tơ, Tủ bù); 7m - 8m đối với nhánh lên cột điện.
   - `Xuống cột, tủ (m)`: 2.5m đối với tủ điện; 1m - 2.5m tùy cấu hình đấu nối.
   - `Dự phòng, lèo (m)`: 6m tiêu chuẩn cho mỗi tuyến / đoạn cáp có đấu nối.
   - `Tổng cộng (m)` = `Khoảng cách + Hao hụt (1%) + Lên cột/tủ + Xuống cột/tủ + Dự phòng/lèo`.

4. **Đầu cáp co nhiệt & Phụ kiện:**
   - Mỗi đoạn tuyến cáp có 2 đầu cáp co nhiệt hạ thế tương ứng với tiết diện cáp (1 đầu tại nguồn đi, 1 đầu tại điểm đến).
   - Mỗi vị trí tủ điện / cột hạ thế trang bị 1 bộ tiếp địa lặp lại T4C-1.5.
   - Các điểm đấu nối chuyển tiếp lên cột hiện trạng bổ sung xà CSV, chống sét van hạ thế (CSV), giá ôm cáp và ống nối đồng nhôm.

---

### 3. TỔNG HỢP TUYẾN DÂY THEO TỪNG TRẠM BIẾN ÁP (TBA)

#### A. TRẠM BIẾN ÁP DỊCH TỄ (TBA DT) - 1x560kVA (MBSH-01 -> MBSH-11)
* **Lộ 1: Tuyến cáp trục đi về hướng đầu đường Thân Khuê & Chợ Hải An:**
  - Tuyến cáp chính từ TBA DT đi dọc hè:
    - TBA DT -> I-TĐ-01 TBA DT: DSTA 150 (L=46m), luồn HDPE 110/90.
    - TBA DT -> I-TĐ-01 TBA DT (tuyến 2): DSTA 120 (L=46m), luồn HDPE 105/80.
    - I-TĐ-01 -> I-TB-01: DSTA 150 (L=66m) + DSTA 120 (L=66m).
    - Qua đường tại MBSH-04: 2QĐ (L=19m) -> DSTA 150 (L=36m) + DSTA 120 (L=36m), ống qua đường UPVC D140 (L=19m x 2 = 38m).
    - I-TB-01 -> I-TĐ-02: DSTA 150 (L=12m).
    - I-TĐ-02 -> I-TĐ-03: DSTA 150 (L=33m).
    - I-TĐ-03 -> I-TĐ-04: DSTA 150 (L=21m).
    - I-TĐ-04 -> I-TĐ-05: DSTA 120 (L=44m).
    - I-TĐ-05 -> I-TĐ-06: DSTA 95 (L=23m).
  - Tuyến nhánh rẽ qua đường & lên cột:
    - Nhánh rẽ qua đường sang cột (1.10)/1.2/DT: DSTA 50 (L=34m), QĐ=31m, UPVC D110=31m.
    - Nhánh rẽ qua đường lên chợ Hải An đến cột (1.8)/1.2/DT: DSTA 120 (L=36m), QĐ=36m, UPVC D140=36m.
    - Nhánh qua đường đối diện MBSH-02: DSTA 120 (L=5m), QĐ=17m, UPVC D140=17m.
  - Tuyến đối diện bên hè trái (nhánh II):
    - Cáp từ qua đường đến II-TĐ-01: DSTA 120 (L=33m).
    - II-TĐ-01 -> II-TĐ-02: DSTA 120 (L=16m).
    - II-TĐ-02 -> II-TB-01: DSTA 120 (L=12m).
    - II-TB-01 -> II-TĐ-03: DSTA 120 (L=46m).
    - II-TĐ-03 -> II-TĐ-04: DSTA 120 (L=16m).
    - II-TĐ-04 -> II-TĐ-05: DSTA 120 (L=27m).
    - II-TĐ-05 rẽ nhánh: DSTA 120 (L=28m).
    - Qua đường đầu tuyến: DSTA 120 (L=12m), QĐ=12m, UPVC D140=12m.
    - Lên cột 17/477 E7.36: DSTA 120 (L=27m).
    - Rẽ xuống cột 16/477 E7.36: 2 sợi song song DSTA 120 (L=15m) + DSTA 120 (L=15m).
  - Nhánh cấp điện cột (1.2).2/1.1/DT tại MBSH-05:
    - Qua đường: DSTA 120 (L=11m), QĐ=11m, UPVC D140=11m.
    - Lên cột: DSTA 95 (L=35m), QĐ=33m, UPVC D140=33m.

* **Lộ 2: Tuyến cáp đi về hướng Bệnh viện Tâm thần:**
  - Từ TBA DT -> III-TĐ-01: DSTA 120 (L=12m).
  - III-TĐ-01 -> III-TB-01: DSTA 120 (L=27m).
  - III-TB-01 -> sang MBSH-06: DSTA 120 (L=10m).
  - Tiếp tục dọc hè đến IV-TB-01: DSTA 120 (L=51m).
  - Tuyến hè đối diện: III-TB-01 -> III-TĐ-03: DSTA 95 (L=62m), QĐ=12m.
  - Nhánh ngõ bê tông lên cột (3.4).3/1.1/DT:
    - Cắt qua đường: QĐ=5m, QĐ=8m.
    - Đoạn trong ngõ 1: DSTA 120 (L=20m), QĐ=20m, UPVC D140=20m.
    - Đoạn trong ngõ 2: DSTA 120 (L=9m), QĐ=9m, UPVC D140=9m.
    - Đoạn lên cột: DSTA 120 (L=11m), QĐ=11m, UPVC D140=11m.
  - Tuyến tiếp theo (MBSH-07, 08, 09, 10):
    - IV-TB-01 -> IV-TĐ-01: DSTA 120 (L=30m).
    - IV-TĐ-01 -> IV-TĐ-02: DSTA 120 (L=41m).
    - IV-TĐ-02 -> IV-TĐ-03: DSTA 120 (L=69m).
    - IV-TĐ-03 -> IV-TĐ-04: DSTA 120 (L=49m).
    - IV-TĐ-04 -> IV-TĐ-05: DSTA 120 (L=58m).
    - IV-TĐ-05 đi tiếp: DSTA 120 (L=81m).
    - Hè đối diện: III-TĐ-03 -> III-TĐ-04: DSTA 95 (L=74m).
    - Nhánh rẽ ngõ lên cột (3.4)/1.1/DT: DSTA 95 (L=24m) + DSTA 95 (L=10m) + DSTA 95 (L=22m).
    - III-TĐ-04 -> III-TĐ-05: DSTA 70 (L=63m), QĐ=5m, UPVC D110=5m.
    - Cáp đối diện MBSH-10: DSTA 95 (L=66m).
    - Nhánh ngõ lên cột (3.4)/1.2/PH: DSTA 95 (L=21m) + DSTA 95 (L=29m).

---

#### B. TRẠM BIẾN ÁP V69 (TBA V69) - 1x250kVA (MBSH-11 -> MBSH-16)
* **Lộ 1 (MBSH-12, 13, 14):**
  - TBA V69 -> I-TĐ-01 TBA V69: DSTA 150 (L=39m), qua đường QĐ=18m, UPVC D140=18m.
  - Tuyến qua đường trước trạm: QĐ=8m, UPVC D140=8m.
  - Nhánh cấp tủ và ngõ: DSTA 120 (L=6m) x 2 tuyến, QĐ=6m x 2.
  - Nhánh lên cột (2.1)/1.1/V69: DSTA 120 (L=19m), QĐ=19m + DSTA 120 (L=6m), QĐ=6m.
  - Nhánh lên cột 1.2/V69: 2 sợi DSTA 120 (L=14m) song song, cắt qua đường 2QĐ (L=11m).
  - Tuyến cáp chính dọc hè: DSTA 120 (L=108m) tại MBSH-13.
  - Cáp dọc hè tiếp tục: DSTA 120 (L=55m), DSTA 95 (L=36m).
* **Lộ 2 (MBSH-15, 16):**
  - Tuyến hè từ I-TĐ-01/TB -> II-TĐ-01 TBA V69 -> II-TB-01: DSTA 120 (L=42m).
  - Nhánh lên cột (2.5)/1.1/V69: DSTA 95 (L=34m), QĐ=33m, UPVC D140=33m.
  - II-TB-01 -> II-TĐ-02 TBA V69: DSTA 120 (L=43m).
  - II-TĐ-02 -> II-TĐ-03 TBA V69: DSTA 120 (L=31m).
  - II-TĐ-03 đi tiếp: DSTA 120 (L=31m).
  - Điểm tiếp giáp MBSH-11/12: DSTA 120 (L=34m), DSTA 120 (L=22m), DSTA 95 (L=36m), DSTA 95 (L=36m).

---

#### C. TRẠM BIẾN ÁP UBND SONG MAI (TBA UBSM) - 1x560kVA (MBSH-17 -> MBSH-19)
* **Lộ 1: Xuất tuyến trạm TBA UBND SM (MBSH-18):**
  - Xuất trạm nhánh 1: 2 sợi DSTA 150 (L=9m) + 2 sợi DSTA 120 (L=9m).
  - Tiếp đoạn 2: 2 sợi DSTA 150 (L=11m) + 1 sợi DSTA 120 (L=11m) + 1 sợi DSTA 95 (L=11m).
  - Cắt qua đường sang I-TB-01 TBA UBSM: DSTA 150 (L=11m), QĐ=11m, UPVC D140=11m.
  - Cấp V-TĐ-01 TBA UBSM.
  - Nhánh lên cột (1.2).1/UBSM: DSTA 50 (L=10m), UPVC D110=10m.
* **Lộ 2: Tuyến dọc hè đường Thân Khuê (MBSH-17, 18):**
  - I-TB-01 -> I-TĐ-01 TBA UBSM: DSTA 150 (L=19m).
  - I-TĐ-01 -> I-TĐ-02 TBA UBSM: DSTA 150 (L=14m).
  - Cáp dọc hè trên: DSTA 120 (L=5m), DSTA 120 (L=36m), DSTA 120 (L=39m).
  - Cáp hè đối diện: DSTA 120 (L=36m), DSTA 95 (L=36m), DSTA 95 (L=39m).
  - Cắt qua đường: QĐ=7m, QĐ=5m.
  - Nhánh ngõ lên cột (2.4)/1.2/UBSM: DSTA 120 (L=23m).
  - Nhánh qua đường lên cột 18-TS/477 E7.36: DSTA 95 (L=40m), QĐ=34m, UPVC D140=34m.
  - Đoạn tiếp sang MBSH-16:
    - I-TĐ-02 -> I-TĐ-03: DSTA 95 (L=7m), DSTA 95 (L=38m), QĐ=11m.
    - I-TĐ-03 -> I-TĐ-04: DSTA 70 (L=47m).
    - Hè đối diện: II-TĐ-01 -> II-TĐ-02 -> II-TĐ-03: DSTA 120 (L=44m), DSTA 95 (L=40m).
    - Nhánh lên cột 19-TS/477 E7.36: DSTA 95 (L=32m), QĐ=29m, UPVC D140=29m.

---

#### D. TRẠM BIẾN ÁP PHÚC ĐỨC 2 (TBA PĐ2) (MBSH-19 -> MBSH-22)
* **Lộ 1 (MBSH-19):**
  - Cấp tủ I-TĐ-01 TBA PĐ2: DSTA 70 (L=12m) + DSTA 70 (L=10m).
  - Nhánh lên cột 1.3/PĐ2: DSTA 70 (L=20m), UPVC D110=20m.
* **Lộ 2 (MBSH-20, 21):**
  - Cấp tủ II-TĐ-01 TBA PĐ2: DSTA 120 (L=25m).
  - Cắt qua ngõ đường nhựa: DSTA 120 (L=8m) + DSTA 120 (L=6m), QĐ=5m, UPVC D140=5m.
  - Nhánh lên cột 2.6/PĐ2: DSTA 120 (L=28m).
* **Lộ 3 (MBSH-21, 22):**
  - Cấp tủ II-TĐ-02 TBA PĐ2: DSTA 70 (L=52m).
  - Tuyến dọc hè đến cuối tuyến: DSTA 70 (L=101m).
  - Cột cuối tuyến 2.10/PĐ2 (2LT8.5 mới).

---

### 4. ĐẦU RA YÊU CẦU
Tạo file Excel chuẩn `.xlsx`:
- Đường dẫn: `D:\TuanPa_AI\OUTPUT\BANG_BOC_TACH_KHOI_LUONG_DUONG_DAY_0.4KV_THAN_KHUE.xlsx` và bản sao tại `D:\OneDrive\TuanPa_AI\OUTPUT\...`
- Cấu trúc:
  - Header chuẩn: font Segoe UI/Arial, chữ đậm, nền xám/xanh nhạt công trình, kẻ bảng đầy đủ (thin border).
  - Công thức tính tự động cho các cột:
    - Hao hụt: `=ROUND(D... * 0.01, 2)`
    - Tổng cộng cáp: `=SUM(D...:H...)`
    - Phân bổ cột cáp: công thức điều kiện `=IF(...)` hoặc giá trị chính xác theo quy cách.
    - Ống xoắn HDPE tương ứng: `=IF(Cáp 150 > 0, Tổng, 0)...`
    - Tổng từng lộ, tổng từng trạm biến áp, và DÒNG TỔNG CỘNG TOÀN DỰ ÁN.
