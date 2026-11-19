# FinanceShopping

Ứng dụng quản lý tài chính cá nhân và hỗ trợ mua sắm.

- Sinh viên: Mai Việt Hoàng — MSSV 1250080058, lớp 12_ĐH_CNPM1.
- Giảng viên hướng dẫn: ThS. Từ Thanh Trí.
- Công nghệ: C#, ASP.NET Core MVC, Razor Views, Identity, EF Core và SQL Server.

## Tải và chạy

1. Chọn **Code → Download ZIP**, giải nén; hoặc clone kho:

   ```powershell
   git clone https://github.com/iloveuu282/DoAnChuyenNganh_1250080058.git
   cd DoAnChuyenNganh_1250080058
   ```

2. Cài .NET SDK 10.x và SQL Server Database Engine. SSMS dùng để kiểm tra kết nối tới SQL Server.
3. Chạy `01_CAI_DAT.cmd`, nhập đúng **Server name** đang kết nối được trong SSMS. Ví dụ `.\SQLEXPRESS` cho SQL Server Express. Nhấn Enter chỉ khi máy đã có LocalDB `(localdb)\MSSQLLocalDB`.
4. Chạy `02_CHAY_UNG_DUNG.cmd`, chờ server khởi động rồi mở http://localhost:5188.
5. Đăng nhập `demo@example.com` / `Demo@2026!`, chọn tháng **11/2026** để xem dữ liệu mẫu. Tài khoản `other@example.com` có cùng mật khẩu, dùng để kiểm tra dữ liệu tách biệt giữa các tài khoản.

Lần cài đầu cần Internet để tải thư viện và quyền tạo database, hoặc database rỗng đã được cấp quyền. Cấu hình kết nối được lưu riêng trong `appsettings.Local.json`, không đưa lên GitHub. Nếu đã cài đặt thành công, các lần sau chỉ cần chạy bước 4.

Xem [hướng dẫn cài đặt](docs/HUONG_DAN_CHAY.md) khi cần cấu hình khác hoặc xử lý lỗi.

## Chức năng

- Đăng ký, đăng nhập; mỗi tài khoản quản lý dữ liệu riêng.
- Quản lý ví, danh mục và giao dịch thu chi.
- Đặt ngân sách chung theo tháng, theo dõi số tiền còn lại.
- Lập danh sách mua sắm, cộng dự kiến của các danh sách chưa mua trong tháng.
- Xác nhận mua để tạo đúng một khoản chi liên kết; hủy xác nhận để sửa ghi nhận nhầm.
- Xem tổng thu, tổng chi, số dư và biểu đồ chi theo danh mục.

Chi phí dự kiến chưa làm giảm số dư hay ngân sách. Khoản mua được tính theo ngày mua thực tế. Âm ví và vượt ngân sách có cảnh báo riêng, vẫn cho lưu khoản chi đã phát sinh. Danh sách đã mua và khoản chi liên kết chỉ được chỉnh sửa sau khi hủy xác nhận.

## Thành phần bàn giao

| Thành phần | Vị trí |
|---|---|
| Solution và mã nguồn | `FinanceShopping.sln`, `src/FinanceShopping.Web` |
| Thư viện và phiên bản | Hai file `.csproj` trong `src` và `tests` |
| Cấu hình kết nối mẫu | `src/FinanceShopping.Web/appsettings.json` |
| Tạo database và dữ liệu mẫu | `01_CAI_DAT.cmd`, `Data/DemoSeeder.cs` trong dự án web |
| SQL tạo bảng và đối chiếu | `database/` |
| Hướng dẫn cài đặt, sử dụng | [Mục lục tài liệu](docs/README.md) |
| Bộ kiểm thử và kết quả | `tests/FinanceShopping.Checks`, [log kiểm thử](docs/KIEM_THU.md) |
| 10 sơ đồ và nguồn chỉnh sửa | [diagrams](diagrams/README.md) |
| 31 ảnh minh chứng trong báo cáo | [Danh mục ảnh](docs/anh_bao_cao/README.md) |

Database được khởi tạo bằng script và dữ liệu mẫu, không cần file backup. Không chạy `schema.sql` vào database đã có bảng. Báo cáo Word/PDF, giải trình và video demo được nộp riêng cùng hồ sơ đồ án.

## Kiểm thử

Kết quả lưu ngày 17/11/2026, trên Windows với .NET SDK 10.0.401 và SQL Server 2025 Express:

| Chế độ | Kết quả |
|---|---|
| SQLite và HTTP SQLite | 34 PASS, 0 FAIL |
| SQL Server kết hợp HTTP | 39 PASS, 0 FAIL |

Chạy `03_KIEM_THU.cmd` để kiểm thử SQLite. Chạy `06_KIEM_THU_SQL_SERVER.cmd` cho SQL Server Express tại `.\SQLEXPRESS`; cách chọn instance khác nằm trong [hướng dẫn chạy](docs/HUONG_DAN_CHAY.md).

Kiểm thử SQL tạo database riêng có hậu tố `_Test`, cần quyền tạo database và không đặt lại database ứng dụng. Xem [phạm vi và log kiểm thử](docs/KIEM_THU.md) và [các bước kiểm tra trên giao diện](docs/KIEM_TRA_SAN_PHAM.md).

## Phạm vi và giới hạn

Ứng dụng chạy trên Windows, sử dụng SQL Server. Có thể truy cập bằng điện thoại cùng Wi-Fi qua `04_CHAY_TREN_DIEN_THOAI.cmd`; xem [hướng dẫn LAN](docs/DIEN_THOAI_VA_GIAO_DIEN.md).

Chưa kiểm thử trên điện thoại vật lý hoặc tải lớn. Kiểm thử HTTP đồng thời sử dụng hai phiên đăng nhập trong TestServer. Chưa có lịch sử hủy, trả hàng/hoàn tiền, chuyển tiền giữa ví, ngân sách theo danh mục, mua từng phần hoặc liên kết ngân hàng.
