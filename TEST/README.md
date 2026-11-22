# THƯ MỤC TEST LISP (QUY TRÌNH KIỂM THỬ NHANH)

Quy định chuẩn theo chỉ đạo của Chủ sở hữu (Phùng Anh Tuấn):

## 1. Mục đích
- Khi có yêu cầu nghiệp vụ / tính năng mới, trước tiên triển khai dưới dạng **AutoLISP (.lsp)** để test nhanh logic hình học và giao diện với người dùng.
- Giúp Chủ sở hữu có thể dùng lệnh `APPLOAD` test trực quan ngay trên AutoCAD mà không cần chờ đợi chu kỳ build/merge phức tạp của toàn bộ dự án C# MEPS.
- Sau khi chốt logic trên bản Lisp, sẽ tiến hành đưa vào Workflow 2 AI (Cách 2) để viết mã C# chuẩn vào `products/meps/src/`, biên dịch và xuất bản chính thức vào `OUTPUT`.

## 2. Quy ước đặt tên thư mục
Mỗi bài toán hoặc tính năng được lưu theo từng folder có số thứ tự tăng dần:
- `1. <TEN_LISP> V1`
- `2. <TEN_LISP> V2`
- ...
- `N. <TEN_LISP> VN`

Mỗi thư mục chứa:
- Tệp mã nguồn: `<TEN_LISP>.lsp`
- Tệp hướng dẫn & lệnh gọi: `HUONG_DAN.txt` (nêu rõ tên lệnh gõ trong CAD, các tham số, cách load).
- (Tùy chọn) Bản vẽ mẫu hoặc kết quả test.
