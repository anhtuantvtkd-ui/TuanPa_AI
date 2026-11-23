# SPEC TASK-0021: Hoàn thiện tâm mũi tên Block, Chế độ chữ Xuôi/Ngược và Neo di chuyển theo nét mũi tên

- **Mục tiêu:** Cập nhật module `products/meps/src/Toado/ToadoPart.cs`.
- **Người thực hiện viết code:** Codex (`gpt-6.2-sol`).
- **Người kiểm thử & đóng gói:** Gemini (`accoreconsole` native AutoCAD + Release build).

---

## 1. Yêu cầu chi tiết & Giải pháp kỹ thuật

### Yêu cầu 1: Tâm mũi tên hình tròn phải vào chính xác tâm của Block
- **Nguyên nhân lỗi:** Hàm `Tip(BlockReference br, bool center)` hiện tại lấy `(ext.MinPoint + ext.MaxPoint) * 0.5` từ `br.GeometricExtents`. Vì `br.GeometricExtents` bao hàm cả AttributeReference và các nét phụ bất đối xứng (như tay vặn công tắc, dao cách ly, text nhãn B1...), nên tâm bounding box bị lệch khỏi tâm vòng tròn/tâm block thực tế.
- **Giải pháp xử lý:**
  1. Kiểm tra nếu điểm chèn `br.Position` nằm trong phạm vi hiển thị hợp lý của block (khoảng cách tới `ext.MinPoint` và `ext.MaxPoint` không bị trôi ra ngoài hàng trăm nghìn đơn vị), thì ưu tiên dùng `br.Position` vì các block ký hiệu chuẩn MEPS (đèn, công tắc, dao cách ly, máy biến áp) luôn đặt Insertion Point tại đúng tâm hình học của ký hiệu.
  2. Nếu block có thực thể hình học dạng `Circle` bên trong `BlockTableRecord`, tính tâm `circle.Center.TransformBy(br.BlockTransform)`.
  3. Chỉ khi `br.Position` bị trôi xa hàng triệu đơn vị (như block trắc địa `BE2 DAN59`), mới fallback tính tâm từ các entity hình học trong BTR (loại trừ `ATTDEF`).

### Yêu cầu 2: Tùy chọn chiều đọc chữ Xuôi / Ngược (Flip 180°)
- Thêm trường vào `ToadoSettings`:
  ```csharp
  public bool ReverseText = false;
  ```
- Cập nhật Form `Toado100Form`:
  Thêm CheckBox hoặc RadioButton:
  - `chkReverseText = new CheckBox { Text = "Đảo chiều đọc chữ (Ngược 180°)", Checked = S.ReverseText };`
  - Đưa vào `grpLook` hoặc `grpPlace`, lưu/nạp vào `S.ReverseText`.
- Cập nhật `CreateOrUpdateLeader`:
  - Với Leader ngang / xiên:
    - Nếu `!s.ReverseText`: `mt.Rotation = 0.0; mt.Direction = new Vector3d(1.0, 0.0, 0.0);`
    - Nếu `s.ReverseText`: `mt.Rotation = Math.PI; mt.Direction = new Vector3d(-1.0, 0.0, 0.0);`
  - Với Leader thẳng đứng (`isVertical`):
    - Nếu `!s.ReverseText`: `mt.Rotation = Math.PI / 2; mt.Direction = new Vector3d(0.0, 1.0, 0.0);` (đọc từ dưới lên)
    - Nếu `s.ReverseText`: `mt.Rotation = -Math.PI / 2; mt.Direction = new Vector3d(0.0, -1.0, 0.0);` (đọc từ trên xuống)
- Cập nhật `GetStoredPoints`:
  Nhận diện leader đứng khi `Math.Abs(Math.Abs(ml.MText.Rotation) - Math.PI / 2) < 0.1` (hỗ trợ cả góc $+\pi/2$ và $-\pi/2$).

### Yêu cầu 3: Khi di chuyển bám vào nét mũi tên di chuyển chứ không bám vào chữ
- Điểm neo (grip point / base point) của nhãn phải bám chặt vào chân nét mũi tên (`landingPt`), đường gạch dóng của mũi tên là điểm tựa kéo di chuyển.
- Trong `CreateOrUpdateLeader` cho leader đứng:
  - Điểm vertex cuối của leader line phải gắn kết chặt chẽ với tọa độ `landingPt` để khi người dùng chọn grip hoặc di chuyển trong AutoCAD, con trỏ bám vào nét mũi tên thay vì bám lơ lửng vào chữ.

---

## 2. Phạm vi can thiệp (Scope)
- **Duy nhất 1 file:** `products/meps/src/Toado/ToadoPart.cs`.
- Tuyệt đối không thay đổi các file khác.
