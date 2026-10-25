# TuanPa AI Bridge
Gemini có thể gọi Meta Llama qua MCP và đọc repo anhtuantvtkd-ui/TuanPa_AI.

## Phạm vi
- Chỉ GitHub user ID 342535200 được cấp token và dùng công cụ.
- read_tuanpa_repo: liệt kê/đọc file UTF-8 <=64KB trên main, không ghi repo.
- ask_meta_llama: gọi @cf/meta/llama-3.3-70b-instruct-fp8-fast, tối đa 768 token đầu ra.
- Đây là Llama qua Workers AI, không phải tài khoản/chat Meta AI Facebook.
- Chưa tuyên bố hoạt động cho đến khi kiểm thử OAuth và gọi tool từ Gemini thành công.

## Cài đặt
1. Giữ Cloudflare Workers Free; không nâng cấp hoặc thêm thanh toán.
2. Tạo KV namespace OAUTH_KV, thay ID trong wrangler.jsonc.
3. Tạo GitHub OAuth App với homepage URL của Worker và callback /callback.
4. Đặt GITHUB_CLIENT_ID, GITHUB_CLIENT_SECRET, COOKIE_ENCRYPTION_KEY (ngẫu nhiên >=32 byte) bằng Cloudflare Secrets. Không commit secrets.
5. pnpm install; pnpm test; pnpm type-check; pnpm deploy.
6. Trong Gemini Custom Apps, kết nối https://<worker>/mcp và đăng nhập GitHub đúng chủ sở hữu.

OAuth chỉ xin read:user để xác thực. Repo hiện công khai nên công cụ đọc không cần token repo. Không có công cụ ghi/xóa, không gửi dữ liệu riêng hoặc secrets vào Llama.
Quota Workers AI miễn phí là 10.000 neurons/ngày theo tài liệu Cloudflare tại lúc chuẩn bị; hết quota phải chờ reset. Không tự chuyển sang nhà cung cấp trả phí.

## Nguồn
Dựa trên https://github.com/cloudflare/ai/tree/main/demos/remote-mcp-github-oauth (Apache-2.0).
Đã thay tools demo, giới hạn owner ID và loại access token/email khỏi MCP props.
