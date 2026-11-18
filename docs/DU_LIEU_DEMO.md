# Dữ liệu mẫu và phép tính đối chiếu

Cả hai tài khoản được seed dữ liệu giả lập tương tự nhưng có UserId và các mã bản ghi riêng. Không dùng số ID trong tài liệu như một hằng số nếu database đã có dữ liệu.

| Nội dung | Giá trị |
|---|---:|
| Số dư ban đầu Tiền mặt | 2.000.000 |
| Số dư ban đầu Tài khoản ngân hàng | 5.000.000 |
| Thu nhập vào ngân hàng 02/11/2026 | 8.000.000 |
| Chi ăn uống tiền mặt 03/11/2026 | 450.000 |
| Ngân sách tháng 11/2026 | 3.000.000 |
| 3 sổ × 30.000 | 90.000 |
| 1 balo × 350.000 | 350.000 |
| Tổng dự kiến | 440.000 |

Trước mua: tiền mặt 1.550.000, ngân hàng 13.000.000, tổng số dư 14.550.000, thu tháng 8.000.000, chi tháng 450.000, ngân sách còn 2.550.000.

Xác nhận mẫu: mua ngày **05/11/2026**, thực tế **420000**, ví **Tiền mặt**, danh mục **Mua sắm**. Kết quả: tiền mặt 1.130.000, ngân hàng 13.000.000, tổng số dư 14.130.000, chi tháng 870.000, ngân sách còn 2.130.000, chênh lệch −20.000.

Hủy xác nhận: khoản chi 420.000 bị xóa, các số trở về trước mua. Không xóa khoản chi 450.000 ăn uống. Xác nhận lại hợp lệ vẫn có đúng một khoản chi liên kết tại thời điểm đã mua.

Nếu đặt ngân sách tạm 800.000 sau khi mua, còn lại −70.000. Đổi lại 3.000.000 khi kết thúc trình diễn. Không dùng ngày hiện tại tự điền nếu muốn đối chiếu các con số tháng 11.
