# Chạy trên điện thoại

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

## Kiểm tra hiển thị

Có thể dùng Device Toolbar trong Chrome hoặc Edge DevTools để kiểm tra chiều rộng 360 px và 390 px. Thử menu, biểu mẫu, bảng dữ liệu và thông báo lỗi. Bảng rộng được cuộn trong khung.

Kiểm tra bằng DevTools không thay thế thử trên điện thoại thật. Bộ kết quả hiện có chưa bao gồm kiểm thử trên thiết bị vật lý.
