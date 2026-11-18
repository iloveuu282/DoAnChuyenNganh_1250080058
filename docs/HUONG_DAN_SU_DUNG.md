# Hướng dẫn sử dụng

## Tổng quan

Ba số chính: tổng thu và tổng chi theo khoảng ngày đã chọn, tổng số dư **hiện tại** tính toàn bộ lịch sử các ví. Bộ lọc tháng riêng điều khiển ngân sách và biểu đồ; đổi tháng không tự đổi khoảng ngày nếu bạn đã nhập khoảng riêng. Dùng 01/11–30/11/2026 và tháng 11/2026 khi trình diễn mẫu.

## Ví và danh mục

Ví có tên và số dư ban đầu, cho phép số dư ban đầu âm nếu phản ánh thực tế. Danh mục có tên và loại Thu hoặc Chi. Ví/danh mục đã dùng trong giao dịch không thể xóa. Danh mục đã dùng không được đổi loại vì làm sai ngữ nghĩa lịch sử. Danh mục mới có thể sửa/xóa bình thường.

## Ghi thu chi

Chọn loại → ngày → ví → danh mục cùng loại → số tiền → ghi chú → Lưu. Số tiền dương, tối đa 2 chữ số thập phân, không nhập dấu chấm phân cách nghìn. Ví dụ 450000 thay vì 450.000. Giao diện JavaScript lọc lựa chọn danh mục, máy chủ vẫn kiểm tra lại. Giao dịch thông thường sửa/xóa được; giao dịch sinh từ mua sắm đưa về danh sách liên kết.

Bộ lọc hỗ trợ ngày, loại, danh mục và phân trang 20 dòng. Xóa bộ lọc để xem lại toàn bộ. Khi sửa số tiền, đổi ví hoặc xóa, số dư và ngân sách được tính lại từ dữ liệu hiện tại.

## Ngân sách

Chọn tháng, bấm Xem tháng, nhập hạn mức >0, bấm Lưu. Mỗi tài khoản có một ngân sách tháng, áp dụng tổng chi mọi ví. Lưu cùng tháng là cập nhật, không thêm bản thứ hai. Không thiết lập ngân sách thì hiển thị rõ chưa thiết lập; không coi là ngân sách 0. Số tiền còn lại có thể âm.

## Mua sắm

Tạo danh sách, chọn tháng dự kiến. Thêm từng mặt hàng: tên, số lượng, đơn giá dự kiến, ưu tiên. Đơn giá 0 hợp lệ, số lượng phải dương. Ưu tiên chỉ để sắp xếp (Cao → Bình thường → Thấp), không phải thuật toán tự chọn đồ.

Thành tiền từng mặt hàng bằng số lượng × đơn giá, làm tròn tới 2 chữ số thập phân; tổng dự kiến bằng tổng các thành tiền đã làm tròn. Trang chi tiết đối chiếu riêng danh sách và tổng dự kiến của tất cả danh sách chưa mua trong cùng tháng với ngân sách còn lại. Dự kiến không giữ chỗ ngân sách và không cộng thành chi thực tế.

Sau khi thực sự mua, nhập **tổng tiền thực tế**, **ngày mua thực tế**, **một ví**, **một danh mục chi**, xác nhận toàn bộ. Danh sách phải có ít nhất một mặt hàng. Một khoản chi liên kết được tạo, hiển thị chênh lệch thực tế − dự kiến. Số âm nghĩa là thực tế ít hơn dự kiến. Nếu ngày mua khác tháng dự kiến, khoản chi tính theo ngày thực tế.

Danh sách đã mua bị khóa sửa/xóa. Muốn điều chỉnh: Hủy xác nhận → hệ thống xóa khoản chi liên kết → sửa thông tin hoặc mặt hàng → xác nhận lại. Không vào SSMS xóa riêng một bên. Yêu cầu gửi lặp tới danh sách đang ở cùng trạng thái không tạo thêm thay đổi. Nếu người dùng đã hủy và xác nhận lại ở một vòng mới thì đó là một lần mua hợp lệ mới, không phải cơ chế chống phát lại vĩnh viễn.

## Cảnh báo

Vượt ngân sách hoặc âm số dư là cảnh báo; hệ thống vẫn ghi nhận khoản chi đã xảy ra. Ngân sách và số dư là hai chỉ số khác nhau: có thể còn nhiều tiền trong ví nhưng đã vượt hạn mức tự đặt. Chi phí dự kiến không trừ cả hai chỉ số này.

## Tài khoản

Đăng xuất bằng menu email góc trên. Dùng cửa sổ ẩn danh để thử tài khoản thứ hai. Ứng dụng chỉ có vai trò người dùng; không có admin, liên kết ngân hàng, khôi phục mật khẩu email, chuyển ví, mua từng phần, AI hoặc lấy giá sàn thương mại điện tử.
