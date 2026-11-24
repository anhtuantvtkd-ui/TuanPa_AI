# KHO TRI THỨC KỸ THUẬT & BÀI HỌC HỒI QUY (LEARNINGS)
> **Mục tiêu:** Lưu vết các bài học thực tế, nguyên nhân gốc và test fixture bảo vệ để hệ thống "càng làm càng tích lũy và thông minh hơn", không bao giờ tái phát lỗi cũ.

---

## BÀI HỌC 01: ĐÁNH SỐ THỨ TỰ BLOCK (TAG SEQUENCE) THEO HÌNH HỌC THỰC TẾ
- **Phạm vi:** Module `TagSeq` (`products/meps/src/TagSeq/TagSeqPart.cs`).
- **Triệu chứng lỗi cũ:** Đánh số thứ tự từ trái qua phải bị lộn xộn (31/34 vị trí sai thứ tự trên bản vẽ khảo sát thực tế).
- **Nguyên nhân gốc:** Điểm chèn (`br.Position`) của block bị lệch xa khỏi hình học hiển thị trên màn hình do thiết kế block ban đầu hoặc offset nội bộ.
- **Quy tắc giải pháp:** 
  1. Sử dụng trọng tâm hình học thực tế `GeometricExtents.Center` thay vì `br.Position` để sắp xếp tọa độ màn hình.
  2. Tra cứu thuộc tính `TAG`, `SO_HIEU`, `TEN` để gán nhãn chính xác.
- **Test Fixture bảo vệ:** `D:\TuanPa_AI\INPUT\Danh so lien tiep.dwg` (32 block `BE2 DAN59`).
- **Tiêu chuẩn nghiệm thu:** Sắp xếp tăng dần đơn điệu $X=19.21 \rightarrow 2290.10$ nhận đúng nhãn $B1 \rightarrow B32$ (PASS 32/32).

---

## BÀI HỌC 02: XỬ LÝ TRÔI GỐC TỌA ĐỘ BLOCK KHẢO SÁT (T-TOADO ANCHOR DRIFT)
- **Phạm vi:** Module `T-TOADO` (`products/meps/src/Toado/ToadoPart.cs`).
- **Triệu chứng lỗi cũ:** Báo hoàn tất tạo nhãn tọa độ nhưng trên màn hình không thấy Leader nào xuất hiện gần block.
- **Nguyên nhân gốc:** Điểm chèn nội bộ `br.Position` của block nằm ở $(-688491.78, 2288802.70)$, trôi cách xa hình học block hiển thị trên màn hình tới $2.389.494$ đơn vị CAD. Leader bị tạo ở ngoài vũ trụ cách xa vùng nhìn người dùng.
- **Quy tắc giải pháp:** 
  1. Tính khoảng cách giữa `br.Position` và `GeometricExtents.Center`.
  2. Nếu độ lệch vượt ngưỡng an toàn ($> 500$ đơn vị hoặc gấp 5 lần đường chéo extents), hàm tự động chuyển điểm đặt mũi tên Leader về `GeometricExtents.Center`.
  3. Leader scan phải đối chiếu cả `tipPt` thực tế và `br.Position` để tránh tạo nhãn trùng lặp.
- **Test Fixture bảo vệ:** Kịch bản `TestToadoFix.cs` trên block `BE2 DAN59`.
- **Tiêu chuẩn nghiệm thu:** MLeader tạo chính xác tại tọa độ hiển thị thực $(651.831, 19.213)$, nội dung chứa đúng định danh block (ví dụ $B1$).

---

## BÀI HỌC 03: BẢO TOÀN DỮ LIỆU NGƯỜI DÙNG KHI CẬP NHẬT BỘ CÀI (INSTALLER SAFETY)
- **Phạm vi:** Trình cài đặt `CAI_DAT.exe` và các bản phát hành `OUTPUT/TVXD.BG`.
- **Nguyên tắc an toàn:**
  1. **Bảo tồn thư viện riêng:** Quá trình cài mới hay nâng cấp tuyệt đối không ghi đè hoặc xóa các thư mục `DATA/`, thư viện `HTKT/`, và các file cấu hình `.ini` đã có của người dùng.
  2. **AutoCAD Guard:** Luôn kiểm tra xem AutoCAD (`acad.exe`, `accoreconsole.exe`) có đang mở và khóa file DLL không; từ chối cài đè khi tiến trình đang chạy để tránh lỗi hỏng file.
  3. **Rollback an toàn:** Lưu vết trạng thái cũ để có thể hoàn nguyên nguyên trạng byte-for-byte khi cài đặt gặp sự cố.

---

## BÀI HỌC 04: GIỮ NGUYÊN VỊ TRÍ MŨI TÊN VÀ LANDING KHI CẬP NHẬT HOẶC COPY MUSHROOM LEADER (STAY-PUT UPDATE)
- **Phạm vi:** Module `T-TOADO` (`products/meps/src/Toado/ToadoPart.cs`).
- **Triệu chứng lỗi:** Khi người dùng copy MLeader tọa độ sang một block khác rồi cập nhật lại, mũi tên tự động nhảy vị trí sang block khác hoặc lệch vị trí mong muốn.
- **Nguyên nhân gốc:** Thuật toán refresh tự động gán lại `targetPt = Tip(closestBlock)` và tính lại `landingPt`. Nếu block gần đó có tâm khác hoặc match nhầm, mũi tên sẽ bị giật nhảy (jump).
- **Quy tắc giải pháp:**
  1. **Tâm mũi tên hình tròn:** `Tip(br, true)` luôn trả về chính xác trọng tâm hình học `GeometricExtents.Center` của block để mũi tên cắm đúng tâm đối tượng.
  2. **Đứng yên khi cập nhật (Stay-Put):** Giữ nguyên `targetPt = currentArrow` và `landingPt = currentLanding` của leader hiện tại.
  3. **Cập nhật giá trị:** Tính toán lại tọa độ hiển thị $X, Y$ theo đúng `targetPt` và cập nhật tên block gần nhất. Mũi tên và đường dẫn giữ nguyên vị trí 100% không bị nhảy.
- **Test Fixture bảo vệ:** Kịch bản native `TestToadoUpdateStayPut.cs` trên `Danh so lien tiep.dwg`.
- **Tiêu chuẩn nghiệm thu:** `Arrow delta = 0`, `Landing delta = 0` (độ lệch bằng 0 tuyệt đối).


