# Phân vai theo công việc

## 1. Kiến trúc điều phối: 1 Tổng chỉ huy + 2 Đội nhân sự
- **Tổng chỉ huy / Điều phối viên (Agent chính):** `Gemini 4 Hydrogen (Extra High)` — chỉ làm 4 việc chốt:
  1. Chốt phân tích nghiệp vụ MEPS/CAD và đặc tả kỹ thuật (công thức, điều kiện biên, tiêu chí PASS/FAIL).
  2. Chia việc rõ ràng thành **Task Packet** ($\le 16\text{ KB}$ diff / phạm vi file hẹp), điều phối đúng model theo độ khó.
  3. Kiểm soát cổng chất lượng (chặn build/đóng gói khi thiếu bằng chứng test hoặc chưa có review độc lập).
  4. Chỉ chốt đóng gói thương phẩm vào `D:\TuanPa_AI\OUTPUT\N. TVXD.BG VN` khi đã đạt duyệt.
- **Kỷ luật giữ sạch context:** `Extra High` không tự đọc log dài, không tự viết code boilerplate hay chỉnh hồ sơ lặp lại; đẩy việc thực thi xuống đội `Hydrogen High/Medium/Low` và đội `Codex`.

| Vai | Phụ trách đề xuất | Đầu vào | Đầu ra |
|---|---|---|---|
| Chủ sản phẩm/nghiệm thu | Phùng Anh Tuấn | yêu cầu, nguồn kỹ thuật | ưu tiên, xác nhận giả thiết, duyệt commit/phát hành |
| Tổng chỉ huy / Điều phối | `Gemini 4 Hydrogen (Extra High)` | yêu cầu chủ sở hữu, trạng thái repo | phân tích, đặc tả kỹ thuật, Task Packet, quyết định đóng gói |
| Kỹ sư cao cấp / Ca khó | `Gemini 4 Hydrogen (High)` | đặc tả, lỗi IL/C#/CAD phức tạp | bản vá rủi ro cao, gỡ bug native, review chéo |
| Kỹ sư triển khai & Build/Test | `Gemini 4 Hydrogen (Medium)` | Task Packet đã chốt | code LISP/C#/PS1/TS, `build_v4.cmd`, log `accoreconsole` |
| Thư ký kỹ thuật & Kiểm nhanh | `Gemini 4 Hydrogen (Low)` / `3.8 Flash` | kết quả test, SHA commit | `npm test`, `workflow:check`, lọc log, cập nhật hồ sơ `TASK`/`COMMUNICATION` |
| Kiến trúc & Review rủi ro cao | `Codex GPT-6.2 Astra` | bài toán phức tạp, diff rủi ro cao | thiết kế giải thuật/gỡ lỗi khó, review độc lập rủi ro cao |
| Coder & Review thường (mặc định Codex) | `Codex GPT-6.2 Sol` | Task Packet hoặc `git diff` ($\le 16\text{ KB}$) | lập trình C#/LISP/test hàng ngày, review thường khi bên khác viết |
| Kiểm tra trực quan UI/CAD | `Codex GPT-6.1 Terra` | ảnh chụp Palette UI, PDF/DWG, form WinForms | kiểm tra bố cục trực quan, thao tác giao diện |
| Coder / Reviewer dự phòng | `Codex GPT-6.1 Sol` / `GPT-6 Astra` / `6 Sol` | Task Packet hẹp hoặc thay khi hết quota | viết script, vá lỗi, review dự phòng |
| Tác vụ nhẹ & Tiết kiệm token | `Codex GPT-6 Luna` / `GPT-5.6 Sol` / `5.6 Terra` / `5.6 Luna` | log, JSON, checklist, diff nhỏ | lọc log, chuẩn hóa JSON/tài liệu, rà checklist, soát cú pháp |
| Trợ lý tại editor | Copilot Free | tệp/ngữ cảnh đã chọn | gợi ý code, test, giải thích |
| Hỗ trợ phản biện nhỏ | Llama qua Cloudflare | câu hỏi ngắn, ngữ cảnh không riêng tư | giải thích/điểm cần hỏi lại; không nghiệm thu |
| Kiểm tra xác định | Actions/test runner | commit + fixtures | pass/fail + logs |
| Theo dõi phụ thuộc | Dependabot | manifests/lockfile | cảnh báo/PR, không tự merge |

## 2. Định tuyến theo cấp độ công việc
- **Cấp Nhẹ** (chỉ sửa docs, JSON hồ sơ, lọc log, script phụ không phát hành vào `OUTPUT/`): `Hydrogen Extra High` chốt yêu cầu $\rightarrow$ `Hydrogen Low/Medium` làm $\rightarrow$ (tùy chọn bổ sung `Codex GPT-6 Luna` / `5.6 Sol` soát nhanh) $\rightarrow$ checklist tự kiểm và chủ sở hữu xác nhận $\rightarrow$ `Extra High` chốt.
- **Cấp Vừa** (nâng cấp LISP bóc tách, sửa Palette/UI, script build/deploy phát hành): `Hydrogen Extra High` chốt đặc tả $\rightarrow$ `Hydrogen Medium` hoặc `Codex GPT-6.2 Sol` (1 Writer) triển khai $\rightarrow$ `Hydrogen Medium` chạy `build_v4.cmd` + `accoreconsole` (kèm `Codex GPT-6.1 Terra` kiểm tra trực quan nếu chạm UI/bản vẽ) $\rightarrow$ Reviewer độc lập (`Codex GPT-6.2 Sol` nếu Gemini viết; `Hydrogen High` / `Codex GPT-6.2 Astra` nếu `6.2 Sol` viết) $\rightarrow$ Chủ sở hữu duyệt $\rightarrow$ `Extra High` đóng gói `OUTPUT/`.
- **Cấp Khó / Rủi ro cao** (sửa `ToadoPart`, `TagSeq`, `meps_orig.il`, công thức chuẩn): `Hydrogen Extra High` + `Codex GPT-6.2 Astra` (hoặc `Hydrogen High`) chốt giải thuật $\rightarrow$ 1 Writer (`Hydrogen High` hoặc `Codex GPT-6.2 Sol`) viết $\rightarrow$ `Hydrogen Medium` build + chạy full regression `accoreconsole` $\rightarrow$ Reviewer độc lập khác người thiết kế & viết mã (`Codex GPT-6.2 Astra` nếu Gemini thiết kế/viết; `Hydrogen High` hoặc `Codex GPT-6 Astra` nếu `6.2 Astra` đã thiết kế) $\rightarrow$ Chủ sở hữu duyệt $\rightarrow$ `Extra High` đóng gói `OUTPUT/`.

## 3. Quy tắc vận hành, Fallback & Gọi Codex thực tế
- Không dùng nhiều AI chỉ để đủ vai; chỉ gọi đúng tầng model công việc cần.
- **Single-Writer + Independent Review:** Mọi thay đổi mã nguồn/phát hành đều phải có Reviewer độc lập (khác Writer và Thiết kế) trước khi đóng gói. Chỉ tác vụ cấp Nhẹ không chạm mã phát hành mới được dùng checklist tự kiểm + xác nhận của chủ sở hữu.
- **Fallback khi hết quota:** Chuyển model dự phòng cùng nhóm (ví dụ `GPT-6.2 Astra` $\rightarrow$ `GPT-6 Astra`; `GPT-6.2 Sol` $\rightarrow$ `GPT-6.1 Sol`; `Hydrogen Medium` $\rightarrow$ `3.8 Flash Medium`), ghi model thật vào `work-items/TASK-xxxx.json`; không giả phản hồi hoặc tự bật chi phí.
- **Cách gọi Codex trên máy cục bộ:**
  - Đường dẫn thực thi (tìm động qua gói cài đặt Microsoft Store để tránh lệch phiên bản): `(Get-AppxPackage OpenAI.Codex).InstallLocation + "\app\resources\codex.exe"`.
  - Gọi không tương tác qua `codex.exe exec -m <model> -s <read-only|workspace-write> -C D:\TuanPa_AI -` và **truyền prompt ping UTF-8 ($\le 200$ từ / $\le 1\text{ KB}$) qua `stdin`** để tránh treo pipe; dùng `-s read-only` cho Reviewer/Architect và `-s workspace-write` cho Writer (hoặc giao qua phiên Codex Desktop mở sẵn).
  - **Cơ chế lấy diff ($\le 16\text{ KB}$ UTF-8):** Prompt chỉ ghi `TASK_ID`, `BASE_SHA`/`RESULT_SHA` và phạm vi tệp; Codex tự đọc `git diff` theo SHA nếu có `git`, hoặc đọc trực tiếp tệp patch/Task Packet ($\le 16\text{ KB}$, đã lọc secrets) được Gemini ghi sẵn đường dẫn trong prompt (tránh nhồi cả diff 16 KB vào `stdin` và tránh lỗi khi shell thiếu `git` trong PATH).

## 4. Gói giao việc chuẩn (Task Packet)
`TASK_ID`; `repo`; `branch`; `BASE_SHA`; `RESULT_SHA` (nếu có); `writer`; `reviewer`; `cấp độ việc`; `phạm vi tệp`; `nguồn`; `tiêu chí PASS/FAIL`; `test command + result`; `finding`; `câu hỏi còn thiếu`.
MCP hiện đọc `main`: với nhánh chưa merge, gửi diff/đoạn tệp đã lọc secrets ($\le 16\text{ KB}$) kèm SHA. Không để AI đọc `main` cũ rồi review nhánh mới.
