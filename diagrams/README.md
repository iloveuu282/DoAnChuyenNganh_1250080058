# Sơ đồ FinanceShopping

Mỗi sơ đồ có một file `.drawio` làm bản chỉnh sửa và một PNG cùng tên để xem hoặc chèn vào báo cáo. Các khối, chữ, ngăn thuộc tính và đường nối trong `.drawio` là đối tượng riêng có thể chỉnh sửa.

| Sơ đồ | Bản chỉnh sửa | PNG |
|---|---|---|
| Use Case | [01_usecase.drawio](01_usecase.drawio) | [Xem hình](01_usecase.png) |
| Kiến trúc | [02_architecture.drawio](02_architecture.drawio) | [Xem hình](02_architecture.png) |
| Dữ liệu quan hệ | [03_data_model.drawio](03_data_model.drawio) | [Xem hình](03_data_model.png) |
| Lớp nghiệp vụ | [04_classes.drawio](04_classes.drawio) | [Xem hình](04_classes.png) |
| Hoạt động xác nhận mua | [05_purchase_activity.drawio](05_purchase_activity.drawio) | [Xem hình](05_purchase_activity.png) |
| Trình tự xác nhận mua | [06_confirm_sequence.drawio](06_confirm_sequence.drawio) | [Xem hình](06_confirm_sequence.png) |
| Trình tự hủy ghi nhận mua | [07_cancel_sequence.drawio](07_cancel_sequence.drawio) | [Xem hình](07_cancel_sequence.png) |
| Hoạt động thu chi | [08_transaction_activity.drawio](08_transaction_activity.drawio) | [Xem hình](08_transaction_activity.png) |
| Hoạt động ngân sách | [09_budget_activity.drawio](09_budget_activity.drawio) | [Xem hình](09_budget_activity.png) |
| Hoạt động kế hoạch tháng | [10_planning_activity.drawio](10_planning_activity.drawio) | [Xem hình](10_planning_activity.png) |

## Cách chỉnh sửa

Mở file `.drawio` bằng draw.io/diagrams.net hoặc tiện ích hỗ trợ draw.io trong trình biên tập. Chọn từng khối để di chuyển, nhấp đúp để sửa chữ; các đường nối có thể điều chỉnh điểm gấp và đầu mũi tên. Với khối lớp/bảng, tiêu đề và phần thuộc tính là hai đối tượng trong cùng khối.

Sau khi sửa, lưu `.drawio` rồi xuất PNG cùng tên để hai bản đồng bộ. PNG hiện tại có nền trắng, độ phân giải gấp đôi tọa độ sơ đồ; được dựng từ cùng nội dung và bố cục dùng cho file draw.io.

## Quy ước nội dung

- Mô hình dữ liệu giữ UserId trên mọi bảng nghiệp vụ. ShoppingList có tối đa một Transaction liên kết; ShoppingListId có thể NULL và phải duy nhất khi có giá trị.
- OwnedEntity là lớp cơ sở C#, không phải bảng riêng. Tam giác rỗng hướng về lớp cha; hình thoi đặc biểu thị hợp thành.
- Sơ đồ trình tự phân biệt nhánh đã/chưa mua, các bước lưu, commit và rollback.
- Sơ đồ kế hoạch phản ánh code hiện hành: làm tròn từng dòng rồi cộng tổng, đồng thời cộng các danh sách chưa mua cùng tài khoản/tháng; chưa ghi chi thực tế.
