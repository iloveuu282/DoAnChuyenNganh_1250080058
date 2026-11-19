# Sơ đồ và cách chỉnh sửa

Tất cả 10 sơ đồ hiện hành được lưu trong `diagrams` dưới dạng `.drawio` có thể chỉnh sửa, kèm PNG cùng tên. Xem [danh mục sơ đồ](../diagrams/README.md) để mở từng cặp.

Mở `.drawio` bằng draw.io/diagrams.net hoặc tiện ích draw.io trong trình biên tập. Các khối, chữ, phần thuộc tính và đường nối là đối tượng riêng. Sau khi sửa và lưu, xuất PNG cùng tên để dùng trong Word hoặc slide.

Mô hình dữ liệu thể hiện các bảng nghiệp vụ, khóa và quan hệ với tài khoản. ShoppingListId của Transaction có thể NULL, nhưng phải duy nhất khi có giá trị. Mọi bảng nghiệp vụ có UserId. OwnedEntity là lớp cơ sở C#, không phải bảng riêng; ApplicationUser kế thừa IdentityUser, tương ứng bảng AspNetUsers. Thông tin mua thực tế nằm trong Transaction được tham chiếu qua ShoppingList.Purchase.

Sơ đồ trình tự xác nhận/hủy thể hiện các nhánh alt, transaction, Version và rollback. Luồng hủy phục vụ sửa ghi nhận nhầm, chưa lưu nhật ký hủy và chưa thực hiện trả hàng/hoàn tiền. Sơ đồ kế hoạch bao gồm tổng dự kiến của nhiều danh sách chưa mua trong cùng tháng và không trừ chi thực tế khi mới lập kế hoạch.
