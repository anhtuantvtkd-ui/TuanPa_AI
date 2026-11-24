# TASK-0025: Hiện đại hóa giao diện thanh triển khai TVXD.BG (MEPS Palette Redesign)

## 1. Mục tiêu
Thiết kế lại toàn bộ giao diện thanh công cụ TVXD.BG (MEPS Palette / `CompactView06` / `VerticalBoard103`) với các yêu cầu:
- **Thẩm mỹ hiện đại & màu sắc sinh động:** Giao diện Dark theme sang trọng (Navy Slate / Cyber Dark), đổ bóng tinh tế (subtle elevation), màu nhấn gradient hài hòa (Teal / Cyan / Amber / Indigo).
- **Gộp nút khoa học & không bị tràn chữ:** Tái cấu trúc các nút bấm thành các nhóm tính năng (Cards / Groups) rõ ràng, giải quyết triệt để vấn đề nút quá hẹp làm cắt chữ (`THỐNG KÊ T...`, `CONECT EXC...`).
- **Menu sổ xuống (Dropdown) chuyên nghiệp:** Thiết kế lại `ContextMenuStrip` với Command Badge (nhãn phím tắt nổi bật), biểu tượng (icon) trực quan, viền bo cong hiện đại và hiệu ứng hover sáng rõ.
- **Xem trước demo:** Tạo bản mockup hình ảnh demo trực quan trước khi cập nhật mã nguồn nhị phân.

## 2. Phân tích hiện trạng
- `VerticalBoard103`: Thanh điều hướng dọc gồm 8 tab màu sắc chưa đồng bộ.
- Mục `TRIỂN KHAI VÀ THỐNG KÊ`: 9 nút dạng lưới 2 cột, chiều rộng ~75px khiến hầu hết các nhãn dài bị cắt xén (`CONECT EXC...`, `CHUẨN HÓA ...`, `GHI CHÚ MT...`).
- Menu ngữ cảnh (`ContextMenuStrip`): Nền xám đơn điệu, khoảng cách dòng hẹp, thiếu phân cách trực quan giữa tên lệnh và chức năng.

## 3. Đề xuất kiến trúc & Giao diện mới
### Nhóm nút chức năng đề xuất:
1. **Khối Thống kê & Bóc tách (Takeoff & Statistics):**
   - Nút lớn: `THỐNG KÊ TUYẾN ▾` (Tích hợp T-KLHD, CDS, Cáp điện, Chiếu sáng).
   - Nút phụ: `XUẤT EXCEL / DỮ LIỆU` (Gộp Conect Excel & Input).
2. **Khối Dàn trang & Hồ sơ (Sheet Layout & Annotation):**
   - Nút: `DÀN TRANG & KHUNG ▾` (T-CK, T-DTL, T-DTM, In PDF).
   - Nút: `NHÃN & CHÚ THÍCH ▾` (Gộp TAG và Ghi chú mặt bằng).
3. **Khối Thiết kế & Kỹ thuật:**
   - Nút: `SƠ ĐỒ NGUYÊN LÝ ▾` (SDNL).
   - Nút: `BỐ TRÍ PHÒNG & THIẾT BỊ` (Gộp kiến trúc phòng).
   - Nút: `CHUẨN HÓA & TIÊU CHUẨN` (Font, Dim, Text).

### Menu sổ xuống (Custom Dropdown):
- Mỗi dòng gồm 2 phần:
  - Badge lệnh CAD bên trái: nền tối tương phản cao, bo góc (ví dụ: `[T-KLHD]`, `[CDS]`, `[T-CK]`).
  - Tên mô tả tiếng Việt đầy đủ bên phải: font rõ nét, padding thoáng mát, có separator phân nhóm.
