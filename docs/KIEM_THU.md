# Kiểm thử FinanceShopping

Bản mã nguồn được kiểm chứng ngày 17/11/2026 trên Windows, .NET SDK 10.0.401 và SQL Server 2025 Express. Build Release đạt 0 lỗi và 0 cảnh báo.

| Chế độ | Kết quả | Thành phần |
|---|---|---|
| SQLite/HTTP | 34 PASS, 0 FAIL | 33 ca nghiệp vụ SQLite, TC29 HTTP SQLite |
| SQL Server kết hợp HTTP | 39 PASS, 0 FAIL | 33 ca nghiệp vụ SQL Server, 3 ca đồng thời SQL Server, 2 ca HTTP SQL Server, TC29 HTTP SQLite |

## Các nhóm kiểm chứng

- TC01–28, TC30–34: phép tính ví/ngân sách, quyền sở hữu, dữ liệu không hợp lệ, kế hoạch tháng, danh mục trùng tên, trạng thái mua/hủy, biểu mẫu cũ, ràng buộc duy nhất và độ chính xác tiền tệ.
- TC17–18: chèn lỗi sau khi ghi hoặc xóa khoản chi để xác minh rollback trên cả SQLite và SQL Server.
- SQL-CONCURRENT-1..3: hai kết nối SQL Server cùng đọc trạng thái trước khi xác nhận; cuối cùng chỉ một khoản chi liên kết, gửi lại không tạo trùng.
- SQL-HTTP-1: hai phiên đăng nhập độc lập gửi xác nhận qua MVC/CSRF với SQL Server; kiểm tra quyền sở hữu, thông báo xung đột và gửi lặp.
- SQL-HTTP-2: hủy đồng thời, hủy lặp, khôi phục số dư/ngân sách; biểu mẫu mua cũ sau khi hủy bị từ chối.
- TC29: định tuyến HTTP, Identity, CSRF, màn hình, đăng ký, binding tháng, quyền đọc chéo và ghi chú; luôn dùng SQLite TestServer.

## Minh chứng

- [Build Release](kiem_thu/build.txt)
- [SQLite và HTTP SQLite](kiem_thu/test_sqlite.txt)
- [SQL Server và HTTP SQL Server](kiem_thu/test_sqlserver.txt)
- [Khởi động ứng dụng thực tế](kiem_thu/startup_smoke.txt)

Máy chủ Kestrel Production với database đang cấu hình đã đăng nhập được và đọc thành công sáu màn hình chính. Host dạng IP được chấp nhận trong chế độ LAN. Lỗi SQL 1205 xuất hiện trong log tình huống đồng thời là xung đột được chủ động tạo để kiểm tra xử lý; kết luận dựa trên phản hồi và dữ liệu cuối cùng.

## Chạy lại và giới hạn

Chạy `03_KIEM_THU.cmd` hoặc `06_KIEM_THU_SQL_SERVER.cmd`; xem [hướng dẫn chạy](HUONG_DAN_CHAY.md) nếu SQL Server dùng instance khác. Log mỗi lần chạy lưu trong `output/checks`. SQL test tạo database mới có hậu tố `_Test`, không xóa database có sẵn và không đặt lại dữ liệu sản phẩm.

HTTP SQL Server sử dụng TestServer trong tiến trình với hai phiên cookie độc lập, chưa phải hai thiết bị vật lý. Chưa kiểm tra toàn bộ giao diện bằng trình duyệt tự động, điện thoại thật hoặc tải lớn. Hệ thống chưa lưu nhật ký hủy và chưa xử lý trả hàng/hoàn tiền thực tế.
