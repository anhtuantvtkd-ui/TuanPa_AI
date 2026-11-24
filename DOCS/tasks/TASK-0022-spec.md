# SPEC TASK-0022: Sửa lỗi Đánh số liên tiếp đẻ thêm số (Deduplicate & In-place Update cho Tag Sequence)

- **Mục tiêu:** Cập nhật module `products/meps/src/TagSeq/TagSeqPart.cs`.
- **Người thực hiện viết code:** Codex (`gpt-6.2-sol`).
- **Người kiểm thử & đóng gói:** Gemini (`accoreconsole` native AutoCAD + Release build V21).

---

## 1. Bản chất lỗi & Phân tích nguyên nhân gốc (Root Cause)
1. **Hiện tượng người dùng gặp phải:**
   - Khi chọn một dãy block và chạy chức năng "Đánh số liên tiếp" (từ số 60 trở đi), trên mỗi block xuất hiện 2 số cùng lúc:
     - Số cũ nằm ở góc trên bên trái (`B61, B63, B65...`).
     - Số mới sinh ra nằm ở bên phải (`B60, B62, B64...`).
   - Người dùng phản ánh: *"PHẦN TAG ĐÁNH SỐ LIÊN TIẾP TỪ SỐ 60 TRỞ ĐI LẠI ĐẺ THÊM SỐ NỮA"*.

2. **Nguyên nhân gốc trong mã nguồn:**
   - Trong `TagSeqCad073.Apply` hiện tại, hàm chỉ chuyển mảng `texts` và gọi sang `TagCad033.ApplyTexts(db, snap, texts, height, guard)`.
   - Trong `TagCad033.ApplyTexts`:
     - Nếu block có nhãn thuộc channel khác (`TVXD_LABEL_033` vs `TVXD_LABEL_033_1`), hoặc `item.hasTag` bị đánh dấu false, hàm `Create` sẽ tạo thêm một `AttributeReference` mới gắn vào bên phải block (`Anchor`).
     - Thuộc tính cũ không được cập nhật mà vẫn tồn tại song song, dẫn đến mỗi block bị 2 nhãn ("đẻ thêm số").
     - Hơn nữa, nếu có 2 nhãn, các lần quét tiếp theo có thể bốc cả 2 đối tượng khiến số đếm nhảy cóc (nhảy 2 đơn vị mỗi block).

---

## 2. Giải pháp kỹ thuật chuẩn mực trong `TagSeqCad073.Apply`
Cần triển khai logic tự chủ (self-contained) trong `TagSeqCad073.Apply` của `TagSeqPart.cs`:

1. **Duyệt qua từng block trong snapshot:**
   - Mở Transaction trên `Database db`.
   - Với mỗi `item` trong `snap.items`:
     - Mở `BlockReference br` với `OpenMode.ForWrite`.
     - Tìm kiếm tất cả các `AttributeReference` hiện có trong `br.AttributeCollection`.
     - Nhận diện các thuộc tính nhãn MEPS (có Tag là `TVXD_LABEL_033` hoặc `TVXD_LABEL_033_1`).
     
2. **Cập nhật tại chỗ (In-place Update) & Dọn dẹp nhãn trùng:**
   - Nếu block **đã có ít nhất một nhãn MEPS**:
     - Chọn nhãn đầu tiên tìm thấy: nâng quyền `OpenMode.ForWrite`, cập nhật `ar.TextString = text` theo đúng kế hoạch số mới.
     - Cập nhật chiều cao `ar.Height = height` (nếu `height > 0`).
     - Giữ nguyên vị trí hiện hữu của nhãn (`Position` / `AlignmentPoint`), không tự ý nhảy vị trí.
     - Nếu có **nhiều hơn 1 nhãn MEPS** (do các phiên bản trước đẻ thêm nhãn thừa): thực hiện `arExtra.UpgradeOpen(); arExtra.Erase(true);` để dọn dẹp triệt để tất cả nhãn thừa, đảm bảo mỗi block chỉ còn đúng 1 nhãn duy nhất!
   - Nếu block **chưa có nhãn MEPS nào**:
     - Tạo một `AttributeReference` mới với Tag `TVXD_LABEL_033`.
     - Gán `TextString = text`, `Height = height`.
     - Tính vị trí neo bên phải block hoặc điểm chèn block.
     - Thêm vào `br.AttributeCollection.AppendAttribute(at);` và thêm vào transaction `tr.AddNewlyCreatedDBObject(at, true);`.

3. **Bảo toàn kết quả trả về:**
   - Trả về đối tượng `TagApply033` với số lượng `modified`, `created`, `deleted` chính xác.
   - Commit transaction an toàn.

---

## 3. Phạm vi can thiệp (Scope)
- **Duy nhất file:** `products/meps/src/TagSeq/TagSeqPart.cs`.
- Tuyệt đối giữ nguyên các class `TagSeqDir073`, `TagSeqEntry073`, `TagSeqOptions073`, `TagSeqRow073`, `TagSequence073` để không làm vỡ các module khác.
