# MEPS — bản nhập từ Antigravity

TASK-0011 · [Issue #11](https://github.com/anhtuantvtkd-ui/TuanPa_AI/issues/11)

Đây là bản sao nguyên trạng dữ liệu dự án do chủ sở hữu chỉ định, nhập ngày 22/11/2026 (Asia/Bangkok). Chủ sở hữu đã xác nhận cho phép công khai cả bản vẽ, Excel và gói cài đặt. Không xóa hoặc thay đổi workspace nguồn.

## Các thư mục

- `snapshot/MEPS_ELECTRICAL/`: mã nguồn C# phân module, scripts, tài nguyên, tài liệu và output hiện có.
- `snapshot/WORK/`: cây mã nguồn phẳng và script build trước đó.
- `snapshot/TVXD.BG V1` đến `V4`, `BG_R52`, `MEPS060`: các gói/phiên bản được giữ riêng để đối chiếu.
- Các file ở gốc snapshot: bản vẽ, công cụ hỗ trợ, mã IL, gói nén và metadata từ nguồn.

Không mặc định mã nguồn `MEPS_ELECTRICAL` tái tạo được DLL V4: chưa chứng minh quan hệ nguồn–binary. Chưa chọn bản sản xuất chính thức, chưa chạy AutoCAD hoặc installer. Một số script và tài liệu giữ đường dẫn tuyệt đối đến workspace cũ; cần task riêng trước khi dùng để build trong clone mới. Không chạy installer chỉ để kiểm tra bản nhập.

## Kiểm chứng

`import-manifest.json` ghi kích thước và SHA-256 cho từng tệp; phần `excluded` liệt kê cấu hình agent cục bộ không nhập. `.gitattributes` giữ nguyên byte trong snapshot.

Từ gốc repository chạy:

```text
node products/meps/verify-import.mjs
```

Kiểm tra này chứng minh tính toàn vẹn của bản sao, không nghiệm thu chức năng CAD. Các kiểm tra npm ở root chỉ bảo vệ bridge và hồ sơ workflow.

Codex tích hợp bản sao; Gemini 3.8 Flash (High) đã tư vấn phương án copy + SHA-256. Phiên Gemini được kết nối cho việc này không có lịch sử cuộc chat Antigravity cũ và chưa thực hiện review mã nguồn độc lập.

Việc merge, chọn phiên bản chuẩn và quy trình phối hợp phát triển sẽ do chủ sở hữu quyết định sau khi xem bản nhập.
