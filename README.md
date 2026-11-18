# FinanceShopping

Đồ án quản lý tài chính cá nhân và hỗ trợ mua sắm của Mai Việt Hoàng, MSSV 1250080058, lớp 12_ĐH_CNPM1. Giảng viên hướng dẫn: ThS. Từ Thanh Trí.

## Kiểm tra sản phẩm

Bản mã nguồn ngày 17/11/2026 đã được build và kiểm thử trên Windows, .NET SDK 10.0.401 và SQL Server 2025 Express. Không thay đổi schema hay yêu cầu tạo lại database ứng dụng.

1. Nếu đã cấu hình và chạy được trước đây, dừng phiên ứng dụng cũ, chạy `02_CHAY_UNG_DUNG.cmd` rồi mở http://localhost:5188 khi xuất hiện thông báo lắng nghe.
2. Đăng nhập tài khoản demo `demo@example.com`, mật khẩu `Demo@2026!`. Chọn tháng 11/2026 để xem dữ liệu mẫu. Dữ liệu đã thao tác sẽ được giữ lại.
3. Nên đăng ký tài khoản riêng để thử sửa, xóa và xác nhận mua mà không thay đổi số liệu demo của báo cáo.
4. Thực hiện các bước trong [Kiểm tra sản phẩm](docs/KIEM_TRA_SAN_PHAM.md), gồm nhiều kế hoạch trong tháng, xác nhận/hủy, cảnh báo và danh mục trùng tên.

Máy mới: cài .NET SDK 10.x và SQL Server Database Engine, kết nối thử bằng SSMS, rồi chạy `01_CAI_DAT.cmd`. Nhập Server name giống SSMS. Xem [hướng dẫn cài đặt](docs/HUONG_DAN_CHAY.md). Không chạy schema.sql vào database đã có bảng.

## Phạm vi chức năng

Ứng dụng dùng ASP.NET Core MVC, Razor Views, Identity, EF Core và SQL Server. Gồm tài khoản, ví, danh mục, thu chi, ngân sách tháng, danh sách/mặt hàng mua sắm, xác nhận/hủy mua và tổng quan.

- Một ngân sách chung mỗi tài khoản mỗi tháng; số dư và ngân sách còn lại tính từ giao dịch hiện có.
- Tổng dự kiến cộng các danh sách chưa mua trong cùng tháng; chưa trừ tiền hoặc ngân sách thực tế.
- Xác nhận toàn bộ danh sách tạo một khoản chi theo ngày mua thực tế. Transaction, Version và chỉ mục duy nhất bảo vệ tính nhất quán và chống trùng.
- Hủy xác nhận xóa khoản chi liên kết và đưa danh sách về chưa mua. Không sửa/xóa riêng khoản chi liên kết khi danh sách còn đã mua.
- Vượt số dư hoặc ngân sách chỉ cảnh báo, không chặn ghi nhận khoản chi đã xảy ra.
- Danh mục trùng tên được phân biệt bằng mã trong lựa chọn và thống kê. Thành tiền từng dòng làm tròn hai chữ số rồi cộng thành tổng dự kiến.

## Kết quả kiểm chứng ngày 17/11/2026

| Chế độ | Kết quả | Phạm vi |
|---|---|---|
| SQLite/HTTP | 34 PASS, 0 FAIL | 33 ca nghiệp vụ SQLite và TC29 HTTP SQLite |
| SQL Server kết hợp HTTP | 39 PASS, 0 FAIL | 33 ca nghiệp vụ SQL Server, 3 ca đồng thời hai kết nối SQL Server, 2 ca HTTP SQL Server và TC29 HTTP SQLite |

Hai ca HTTP SQL Server sử dụng hai phiên cookie đăng nhập độc lập, gửi xác nhận/hủy đồng thời, kiểm tra thông báo xung đột, gửi lặp và biểu mẫu cũ sau khi hủy. Cơ sở dữ liệu được kiểm tra lại về khoản chi liên kết, số dư và ngân sách. Đây là TestServer trong tiến trình, chưa phải hai thiết bị vật lý.

Ứng dụng Kestrel thực tế đã đăng nhập được với database đang cấu hình và đọc thành công sáu màn hình chính. Chế độ LAN đã nhận được Host dạng IP; chưa kiểm tra kết nối từ điện thoại vật lý hoặc toàn bộ bố cục bằng trình duyệt tự động.

Log của lần kiểm chứng này được giữ tại [docs/kiem_thu](docs/kiem_thu): `build.txt`, `test_sqlite.txt`, `test_sqlserver.txt`, `startup_smoke.txt`. Xem [tổng hợp kiểm thử](docs/KIEM_THU.md) để phân biệt môi trường và giới hạn.

## Chạy kiểm thử

- `03_KIEM_THU.cmd`: SQLite/HTTP.
- `06_KIEM_THU_SQL_SERVER.cmd`: SQL Server trên `.\SQLEXPRESS` qua Windows Authentication.
- Instance khác: `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/ReviewChecks.ps1 -SqlServer -Server 'TENMAY\INSTANCE'`.

Log mỗi lần chạy được lưu riêng ở `output/checks`. SQL test tạo database mới với tên ngẫu nhiên kết thúc `_Test`; không xóa database có sẵn và không đặt lại database sản phẩm. Cần quyền tạo database kiểm thử. Các database kiểm thử được giữ lại để đối chiếu.

## Thư mục và công cụ

- `FinanceShopping.sln`, `src/FinanceShopping.Web`: mã nguồn ứng dụng .NET 10.
- `tests/FinanceShopping.Checks`: bộ kiểm thử nghiệp vụ và HTTP.
- `database/schema.sql`: DDL cho database rỗng; `verify.sql`, `van_dap_ssms.sql`, `doi_chieu_mua_sam.sql`: truy vấn đối chiếu.
- `Data/DemoSeeder.cs` trong dự án web: khởi tạo tài khoản và dữ liệu mẫu qua Identity.
- `diagrams`: sơ đồ và tệp nguồn; `docs`: hướng dẫn sử dụng, cài đặt và kiểm thử. Xem [mục lục tài liệu](docs/README.md).
- `04_CHAY_TREN_DIEN_THOAI.cmd`: mở chế độ LAN khi hai thiết bị cùng Wi-Fi; xem `docs/DIEN_THOAI_VA_GIAO_DIEN.md`.

Giữ `appsettings.Local.json` riêng trên máy; không đưa chuỗi kết nối chứa mật khẩu vào GitHub hoặc gói nộp công khai. Bản cập nhật không đổi mô hình dữ liệu nên không cần chạy lại khởi tạo database.

## Giới hạn hiện tại

Chưa có lịch sử hủy xác nhận, nghiệp vụ trả hàng/hoàn tiền, chuyển tiền giữa ví, ngân sách theo danh mục, mua từng phần hoặc kết nối ngân hàng. Hủy xác nhận phục vụ sửa ghi nhận nhầm. Các giới hạn này phải được giải thích đúng khi demo.

Thư mục này là mã nguồn để đưa lên GitHub. Hồ sơ Word/PDF, mẫu của Khoa, nhận xét giảng viên, ảnh gốc và tài liệu chuẩn bị bảo vệ được quản lý riêng bên ngoài kho mã nguồn. Cấu hình máy `appsettings.Local.json`, file build và kết quả chạy mới trong `output` được Git bỏ qua theo `.gitignore`.

## Tài nguyên minh chứng của báo cáo

- [31 ảnh chụp phần mềm và SQL Server](docs/anh_bao_cao/README.md), đánh số khớp các Hình 3.1–3.31 trong Word.
- [10 sơ đồ UML, kiến trúc và dữ liệu](diagrams/README.md), kèm nguồn chỉnh sửa draw.io và ảnh PNG.
