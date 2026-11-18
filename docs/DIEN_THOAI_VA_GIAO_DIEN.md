# Chạy trên điện thoại và chỉnh giao diện

## Điện thoại thật qua Wi-Fi

1. Máy tính đã chạy được ứng dụng và SQL Server. Kết nối điện thoại và máy tính vào cùng Wi-Fi gia đình.
2. Dừng bản đang chạy bằng Ctrl+C. Chạy `04_CHAY_TREN_DIEN_THOAI.cmd`.
3. Script hiển thị các IPv4. Chọn IP của card Wi-Fi đang kết nối, bỏ qua VMware, WSL, VPN hoặc các adapter ảo.
4. Trên điện thoại mở `http://IP_WIFI_MAY_TINH:5188`, ví dụ `http://192.168.1.20:5188`. Không nhập `localhost` hoặc `0.0.0.0` trên điện thoại.
5. Giữ máy tính, ứng dụng và SQL Server hoạt động. Người dùng trên điện thoại vẫn dùng database trên máy tính thông qua ứng dụng; điện thoại không kết nối SQL Server trực tiếp.

Nếu Windows hỏi quyền mạng cho ứng dụng, cho phép trên mạng Private đang dùng. Nếu không vào được:

- Trên máy tính mở chính URL theo IPv4 đó để kiểm tra trước.
- Mở Settings → Network & internet → Wi-Fi → thuộc tính mạng gia đình → Network profile type: Private.
- Mở Windows Security → Firewall & network protection → Allow an app through firewall → Change settings; cho phép đúng ứng dụng .NET đang chạy trên Private.
- Nếu vẫn bị chặn, mở Advanced settings → Inbound Rules → New Rule → Port → TCP, cổng 5188 → Allow the connection → chỉ chọn Private → đặt tên FinanceShopping demo. Không tắt toàn bộ firewall. Sau khi không cần dùng mạng LAN có thể tắt quy tắc này.
- Wi-Fi khách có thể chặn các thiết bị truy cập nhau. Đổi sang Wi-Fi gia đình, dừng VPN nếu nó thay đổi đường mạng.

Đây là bản demo trong mạng nội bộ qua HTTP. Không cấu hình chuyển tiếp cổng router. Khi cần sử dụng từ Internet, phải có bước triển khai riêng với HTTPS và cấu hình vận hành phù hợp.

## Nơi chỉnh giao diện trong VS Code

| Muốn đổi | Mở file |
|---|---|
| Font, cỡ chữ chung, màu chính, nền, độ rộng trang | `src/FinanceShopping.Web/wwwroot/css/theme.css` |
| Bố cục, bảng, biểu mẫu, giao diện điện thoại | `src/FinanceShopping.Web/wwwroot/css/site.css` |
| Tên ứng dụng, menu, footer | `src/FinanceShopping.Web/Views/Shared/_Layout.cshtml` |
| Nội dung Tổng quan | `src/FinanceShopping.Web/Views/Home/Index.cshtml` |
| Thu chi | `src/FinanceShopping.Web/Views/Transactions/` |
| Danh sách, mặt hàng và xác nhận/hủy mua | `src/FinanceShopping.Web/Views/Shopping/` |

Ví dụ: đổi `--font-base: 16px` thành `18px` để phóng chữ. Các cỡ `rem` tăng theo cỡ gốc. Đổi `--color-primary` và `--color-primary-hover` để đổi màu nút/liên kết. Các màu trạng thái cảnh báo giữ ý nghĩa riêng.

Sửa CSS rồi Ctrl+F5 trên trình duyệt máy tính. Với điện thoại, tải lại trang; nếu còn cache thì đóng/mở lại tab hoặc xóa dữ liệu trang. Sau khi sửa `.cshtml` hoặc C#, dừng và chạy lại script để build.

## Ba kích thước cần kiểm tra

- Máy tính: 1366×768 hoặc màn hình của bạn.
- Điện thoại: 390×844 và 360×800 trong Edge/Chrome DevTools.
- Trên điện thoại thật: kiểm tra đăng nhập, nhập tiền, chọn ngày và xác nhận/hủy mua.

Bảng dữ liệu có thể cuộn ngang bên trong khung; toàn trang không nên tràn ngang. Kiểm tra thêm bằng tên danh mục dài và số tiền lớn. Phần này là hướng dẫn kiểm tra; chưa phải kết quả nghiệm thu trên điện thoại của bạn.
