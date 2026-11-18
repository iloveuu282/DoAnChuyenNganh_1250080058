# Kiểm tra sản phẩm FinanceShopping

Tài liệu dành cho lần kiểm tra giao diện sau khi hoàn thiện mã nguồn ngày 17/11/2026. Chạy `02_CHAY_UNG_DUNG.cmd`, chờ thông báo lắng nghe rồi mở http://localhost:5188. Nếu ứng dụng đang chạy từ phiên cũ, dừng bằng Ctrl+C trước khi chạy lại để nạp bản code mới.

Không cần chạy lại cài đặt hoặc tạo lại cơ sở dữ liệu khi cập nhật bản này. Nếu máy chưa từng cấu hình, thực hiện `01_CAI_DAT.cmd` theo `HUONG_DAN_CHAY.md`.

## Chuẩn bị dữ liệu riêng

Đăng ký tài khoản mới để kiểm tra mà không làm thay đổi số liệu demo trong báo cáo. Tạo ví “Ví kiểm tra” với số dư ban đầu 2.000.000 đồng. Tạo hai danh mục Chi cùng tên “Mua sắm”, ghi lại mã riêng của từng danh mục. Chọn tháng 11/2026 cho các bước dưới đây.

## Các tình huống cần kiểm tra

| Bước | Thao tác | Kết quả cần thấy |
|---|---|---|
| 1 | Đặt ngân sách tháng 11 là 1.000.000 đồng | Một hạn mức chung; chưa có khoản chi thì còn đủ 1.000.000 đồng |
| 2 | Tạo danh sách A với mặt hàng 700.000 đồng, danh sách B với mặt hàng 600.000 đồng | Mỗi trang chi tiết ghi tổng kế hoạch 1.300.000 đồng, cảnh báo vượt dự trù 300.000 đồng; ví và ngân sách thực tế chưa giảm |
| 3 | Xác nhận A với số tiền thực tế 650.000 đồng, ngày 05/11/2026 | A đã mua, một khoản chi liên kết; ví còn 1.350.000 đồng, ngân sách còn 350.000 đồng; kế hoạch chưa mua chỉ còn B: 600.000 đồng |
| 4 | Mở Thu chi và thử mở khoản chi liên kết | Được dẫn về danh sách mua; không có luồng sửa/xóa khoản chi liên kết riêng lẻ |
| 5 | Hủy xác nhận A | Khoản chi liên kết biến mất, ví trở lại 2.000.000 đồng, ngân sách còn 1.000.000 đồng; tổng dự kiến trở lại 1.300.000 đồng |
| 6 | Mở A ở hai cửa sổ đăng nhập cùng tài khoản, tải cả hai trang trước rồi xác nhận gần đồng thời | Cuối cùng chỉ có một khoản chi. Phiên còn lại báo đã xác nhận hoặc xung đột dữ liệu và yêu cầu tải lại |
| 7 | Hủy A; thử gửi lại biểu mẫu xác nhận đã mở từ trước khi hủy | Biểu mẫu cũ bị từ chối do phiên bản đã thay đổi; tải trang mới để xác nhận lại |
| 8 | Hủy A nếu còn đã mua; đặt ngân sách tháng 12 là 1.000.000 đồng; xác nhận A ngày 05/12/2026 với 650.000 đồng | Khoản chi thuộc tháng 12; ngân sách tháng 11 không giảm; tháng 12 còn 350.000 đồng |
| 9 | Ghi hai khoản chi 70 và 30 đồng, cùng tháng, chọn lần lượt hai danh mục trùng tên | Biểu đồ có hai nhóm, nhãn phân biệt bằng mã danh mục; tổng chi 100 đồng trong tình huống này |
| 10 | Thử số tiền 0, ngày không hợp lệ; đăng nhập tài khoản khác rồi mở URL của A | Dữ liệu sai bị từ chối; danh sách của tài khoản khác trả trang không tìm thấy |

Các số dư ở bước 1–8 giả định chỉ thực hiện những thao tác được nêu. Bước 9 làm tăng chi thêm 100 đồng. Nếu đã ghi thêm dữ liệu khác, cần cộng/trừ các giao dịch đó khi đối chiếu.

## Kiểm tra riêng cảnh báo

Nên dùng tài khoản thử khác hoặc tháng khác để không ảnh hưởng các số trên:

- **Chỉ vượt ngân sách:** ví 2 triệu, ngân sách 1 triệu, chi 1,2 triệu → ví vẫn dương 800.000 đồng, thông báo chỉ nêu vượt ngân sách 200.000 đồng.
- **Chỉ âm ví:** ví 100.000 đồng, chưa đặt ngân sách, chi 150.000 đồng → thông báo âm ví 50.000 đồng, không tự coi ngân sách chưa đặt là 0.
- **Cả hai:** ví 100.000 đồng, ngân sách 100.000 đồng, chi 150.000 đồng → hai nội dung cảnh báo riêng; giao dịch vẫn được lưu.

Hủy xác nhận dùng để sửa lần ghi nhận mua sai. Chức năng hiện không lưu lịch sử hủy và không mô tả sự kiện trả hàng/hoàn tiền thực tế.

## Chạy bộ kiểm thử

- `03_KIEM_THU.cmd`: nghiệp vụ SQLite và HTTP SQLite.
- `06_KIEM_THU_SQL_SERVER.cmd`: nghiệp vụ và HTTP với SQL Server tại `.\SQLEXPRESS`, dùng Windows Authentication. Cần quyền tạo database kiểm thử.
- Nếu instance khác, mở PowerShell tại thư mục dự án và chạy `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/ReviewChecks.ps1 -SqlServer -Server 'TENMAY\INSTANCE'`.

Mỗi lần chạy ghi log mới trong `output/checks`. Chế độ SQL chỉ tạo database mới có tên ngẫu nhiên kết thúc `_Test`; không xóa database đang có và không khởi tạo lại dữ liệu sản phẩm. Các database kiểm thử được giữ lại; bộ kiểm thử không gọi `EnsureDeleted`.

Hai ca HTTP SQL Server dùng hai phiên cookie đăng nhập độc lập và TestServer trong tiến trình. Chúng kiểm tra toàn bộ tuyến MVC, CSRF, xử lý xung đột và dữ liệu lưu trên SQL Server; không thay thế kiểm tra trực tiếp trên hai điện thoại.

## Kiểm tra điện thoại

Dừng phiên `02_CHAY_UNG_DUNG.cmd`, chạy `04_CHAY_TREN_DIEN_THOAI.cmd`. Máy tính và điện thoại cùng Wi-Fi; truy cập `http://IP_CUA_MAY_TINH:5188`. Chế độ này cho phép truy cập qua hostname/IP trong LAN. Cấu hình chạy localhost thông thường vẫn giới hạn localhost. Nếu điện thoại không kết nối được, xem `DIEN_THOAI_VA_GIAO_DIEN.md` để kiểm tra mạng và tường lửa.

Khi phát hiện lỗi, ghi màn hình/chức năng, thao tác trước lỗi, số liệu nhập, kết quả mong muốn và kết quả thực tế. Đừng xóa dữ liệu trước khi đối chiếu.
