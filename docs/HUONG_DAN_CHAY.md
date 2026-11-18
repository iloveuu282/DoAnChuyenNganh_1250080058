# Hướng dẫn cài đặt và chạy trên Windows 11

## 1. Chuẩn bị

Cần .NET SDK 10.x và SQL Server Database Engine. Có thể dùng Visual Studio với workload ASP.NET and web development, hoặc VS Code và dòng lệnh. SSMS là công cụ quản lý SQL Server; kiểm tra kết nối tới Database Engine trước khi cấu hình ứng dụng. Chỉ cài những thành phần máy còn thiếu.

Mở PowerShell, chạy:

```powershell
dotnet --version
dotnet --list-sdks
```

Cần có SDK 10.x. Visual Studio cần workload ASP.NET and web development. Nếu VS không nhận net10.0, cập nhật VS hoặc dùng `dotnet` trong Terminal. Không thay TargetFramework xuống .NET Framework 4.x.

Giải nén vào đường dẫn ngắn, ví dụ `C:\DoAn\FinanceShopping`. Toàn bộ tài liệu dùng thư mục này làm gốc. Không mở riêng một file C# rồi bấm Run Code.

## 2. Chọn đúng SQL Server

Trong SSMS, chọn Database Engine, Windows Authentication, kết nối với máy chủ bạn thường dùng. Sao chép **Server name**. Ví dụ:

- `localhost` hoặc tên máy: instance mặc định.
- `.\SQLEXPRESS` hoặc `TENMAY\SQLEXPRESS`: SQL Server Express.
- `(localdb)\MSSQLLocalDB`: LocalDB, chỉ dùng khi đã cài thành phần này. Cài SQL Server không đồng nghĩa đã có LocalDB.

Nếu chưa kết nối được, mở Services, kiểm tra `SQL Server (MSSQLSERVER)` hoặc instance tương ứng đang Running. LocalDB không phải dịch vụ thường trực trong Services; kiểm tra bằng `sqllocaldb info` nếu có.

## 3. Cài đặt bằng tệp hỗ trợ

Chạy `01_CAI_DAT.cmd`. Nhập Server name đã kiểm tra. Script ghi chuỗi kết nối vào `src/FinanceShopping.Web/appsettings.Local.json`, build Release, tạo bảng bằng `EnsureCreated` và seed tài khoản mẫu bằng Identity.

Script chỉ thêm dữ liệu mẫu khi tài khoản đó chưa có ví. Chạy lại không nhân đôi ví/giao dịch trong trường hợp dữ liệu mẫu còn nguyên. Nếu demo bị sửa nhiều, dùng database mới tên `FinanceShopping_Demo2` bằng cách đổi chuỗi kết nối và chạy seed; không xóa dữ liệu quan trọng để đặt lại demo. Không có chức năng reset ngầm.

Cấu hình mặc định sử dụng Windows Authentication. Tài khoản Windows hiện tại cần quyền tạo database trong lần đầu, hoặc nhờ người có quyền tạo trước database **rỗng** `FinanceShopping` và cấp quyền tạo bảng/đọc/ghi. Không dùng database đang chứa các bảng của đồ án khác. `EnsureCreated` không cập nhật lược đồ đã tồn tại và không phải cơ chế migrations.

Nếu dùng SQL Authentication, sửa `appsettings.Local.json` thay vì chạy lại Configure:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=TENMAY;Database=FinanceShopping;User Id=TAIKHOAN;Password=MATKHAU;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=False"
  }
}
```

Thay các chữ in hoa bằng giá trị của bạn. JSON phải escape dấu `\` thành `\\` khi nhập tên instance. Không đưa file cấu hình chứa mật khẩu vào bài nộp công khai. Tùy chọn TrustServerCertificate dùng cho máy thử localhost, không phải cấu hình triển khai Internet.

## 4. Chạy

Chạy `02_CHAY_UNG_DUNG.cmd`. Chờ dòng `Now listening on: http://localhost:5188`. Mở http://localhost:5188 nếu trình duyệt chưa tự mở. Giữ cửa sổ CMD mở. Dừng bằng Ctrl+C trong cửa sổ đó. Tắt trình duyệt không đồng nghĩa tắt server. Script chạy chế độ Production để trang lỗi không lộ stack trace.

Ứng dụng tự chuyển tới đăng nhập. Hai tài khoản độc lập:

| Email | Mật khẩu | Mục đích |
|---|---|---|
| demo@example.com | Demo@2026! | Trình diễn chính |
| other@example.com | Demo@2026! | Kiểm tra cách ly dữ liệu |

Tài khoản mới đăng ký sẽ rỗng. Trình tự hợp lý: tạo ví → tạo danh mục thu và chi → ghi thu chi → đặt ngân sách → lập mua sắm.

## 5. Chạy bằng Visual Studio

Mở `FinanceShopping.sln`, đặt **FinanceShopping.Web** làm Startup Project. Chạy Configure một lần trước. Bấm Ctrl+F5 với profile FinanceShopping (HTTP). Visual Studio dùng `launchSettings.json`, địa chỉ 5188. Không chạy đồng thời CMD và VS cùng cổng. Khi sửa mã nguồn, build lại. Có thể xem các bảng bằng SQL Server Object Explorer hoặc SSMS.

## 6. Chạy bằng VS Code hoặc Terminal

Mở Terminal tại thư mục gốc, thực hiện:

```powershell
cd src/FinanceShopping.Web
dotnet restore
dotnet build -c Release
dotnet run -c Release --no-build --no-launch-profile -- --seed-demo
$env:ASPNETCORE_URLS='http://localhost:5188'
$env:ASPNETCORE_ENVIRONMENT='Production'
dotnet run -c Release --no-build --no-launch-profile
```

Lệnh seed chạy rồi thoát, lệnh sau mới mở server. Muốn chỉ khởi tạo bảng mà không thêm demo, dùng `--init-db` thay `--seed-demo`.

## 7. SQL thủ công nếu cần

Cách tự động ở mục 3 là lựa chọn chính. Nếu giảng viên muốn thấy SQL:

1. Tạo database rỗng FinanceShopping trong SSMS, chọn database này.
2. Mở `database/schema.sql`, chạy toàn bộ. Đây là DDL đầy đủ, gồm Identity, index duy nhất, check constraint và khóa ngoại.
3. Cấu hình đúng chuỗi kết nối rồi chạy `--seed-demo` để tạo tài khoản với mật khẩu băm chuẩn và dữ liệu mẫu.
4. Mở `database/verify.sql` để đối chiếu sau khi chạy chức năng.

Không chạy DDL vào database đã được khởi tạo tự động. Không cần vừa chạy schema.sql vừa tạo bảng qua EnsureCreated trên hai database khác nhau. Lược đồ bàn giao theo cách tạo mới, chưa có lịch sử migrations; nếu đổi mô hình sau này cần lập migration có kiểm soát thay vì mong EnsureCreated sửa bảng.

## 8. Kiểm thử tự động

Chạy `03_KIEM_THU.cmd` hoặc:

```powershell
dotnet run --project tests/FinanceShopping.Checks -c Release
```

Kết quả của bản hiện tại: `34 PASS, 0 FAIL`. Mặc định dùng SQLite tạm trong bộ nhớ; không sửa database demo. Khi chạy bằng CMD, log mới được lưu riêng theo thời gian trong `output/checks`.

Để chạy 33 ca nghiệp vụ, ba ca đồng thời hai kết nối và hai ca HTTP trên **SQL Server thật**, chạy `06_KIEM_THU_SQL_SERVER.cmd` với instance `.\SQLEXPRESS`, hoặc dùng lệnh dưới đây cho instance khác. Chỉ dùng cấu hình database thử tên kết thúc bằng `_Test`. Chương trình thêm mã ngẫu nhiên để tạo database mới cho mỗi lần chạy, không xóa database có sẵn. Fixture dọn riêng dữ liệu A/B trong database mới sau từng ca. Cần quyền tạo database. Không trỏ vào database sản phẩm hoặc dữ liệu cần giữ:

```powershell
$env:FINANCE_TEST_SQLSERVER='Server=TENMAY;Database=FinanceShopping_Test;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=False'
dotnet run --project tests/FinanceShopping.Checks -c Release
Remove-Item Env:FINANCE_TEST_SQLSERVER
```

Ca TC29 HTTP vẫn dùng SQLite để tách host. Tổng chế độ SQL Server là `39 PASS, 0 FAIL`, trong đó 38 ca dùng SQL Server và một ca HTTP SQLite. SQL-CONCURRENT-1..3 đồng bộ hai kết nối ở tầng nghiệp vụ; SQL-HTTP-1..2 gửi xác nhận/hủy từ hai phiên cookie độc lập qua MVC TestServer với SQL Server. Đây chưa phải thao tác từ hai thiết bị thật. Xem [kiểm thử](KIEM_THU.md) và [các bước kiểm tra sản phẩm](KIEM_TRA_SAN_PHAM.md).

## 9. Kiểm tra giao diện nhỏ

Mở DevTools bằng F12 → bật Device Toolbar → chọn chiều rộng khoảng 390 px. Thử menu, form thu chi, danh sách mặt hàng, thông báo lỗi. Bảng rộng được cuộn ngang, không cắt cột. Đây là kiểm tra bố cục responsive; không đồng nghĩa đã thử trên điện thoại vật lý. Muốn dùng điện thoại thật qua Wi-Fi cần cấu hình bind địa chỉ LAN, AllowedHosts và firewall; không cần cho phạm vi localhost của đồ án, không mở cổng nếu chỉ quay bằng giả lập màn hình nhỏ.

## 10. Publish tùy chọn

Sau khi đã chạy và nghiệm thu bằng mã nguồn:

```powershell
dotnet publish src/FinanceShopping.Web -c Release -r win-x64 --self-contained true -o publish
```

Thư mục publish chứa file thực thi web và runtime; vẫn cần SQL Server, cấu hình kết nối đúng và database đã khởi tạo. Sao chép `appsettings.Local.json` vào publish nếu chưa có. Mở Terminal tại publish rồi chạy `FinanceShopping.Web.exe --urls http://localhost:5188`. Không bật PublishSingleFile vì không cần gom tài nguyên web. Tùy chọn này cần tải runtime win-x64, chưa được dựng hoặc thử trên Windows trong bộ bàn giao. Bản mã nguồn + CMD là cách chạy chính.

## 11. Sự cố thường gặp

| Hiện tượng | Cách kiểm tra và xử lý |
|---|---|
| SQL error 26/40/53 hoặc không tìm thấy server | So lại Server name với SSMS, instance và trạng thái dịch vụ; SSMS phải kết nối thành công trước. |
| Login failed / Cannot open database | Kiểm tra Windows/SQL Authentication, quyền tạo database và tên database. |
| Invalid object name | Đang trỏ sai DB hoặc chưa chạy seed/init. Không tự tạo riêng từng bảng bằng tay. |
| LocalDB không tồn tại | Nhập đúng SQL Server instance bạn đã cài; không mặc định ép dùng LocalDB. |
| Cổng 5188 đang được sử dụng | Đóng phiên CMD/VS cũ. Hoặc đổi ASPNETCORE_URLS sang localhost:5189 rồi truy cập đúng cổng. |
| NuGet không tải được | Kiểm tra Internet/proxy, chạy dotnet restore tại thư mục dự án. Không dùng --no-restore cho lần đầu. |
| Tổng quan trống | Chọn tháng 11/2026 và khoảng ngày tháng 11; kiểm tra đang dùng tài khoản demo. |
| Không đăng nhập được | Kiểm tra seed đã thành công, mật khẩu Demo@2026!, chờ 5 phút nếu bị khóa vì sai 5 lần. |
| Không sửa khoản chi mua sắm | Đây là BR07; vào danh sách mua sắm → Hủy xác nhận → sửa → xác nhận lại. |
| Không xóa ví/danh mục | Kiểm tra giao dịch đang tham chiếu; ràng buộc bảo vệ lịch sử dữ liệu. |
| Danh sách đã thay đổi | Tải lại trang. Version chống thao tác từ tab cũ; kiểm tra trạng thái trước khi thử lại. |
| Trình duyệt báo chưa kết nối lúc mới chạy | Chờ server khởi động xong rồi tải lại. Giữ cửa sổ server mở. |

## 12. Trước khi gửi bài

Thực hiện [các bước kiểm tra sản phẩm](KIEM_TRA_SAN_PHAM.md), đối chiếu kết quả với [minh chứng kiểm thử](KIEM_THU.md). Git bỏ qua bin, obj, output và cấu hình máy theo `.gitignore`; kiểm tra `git status` trước khi đưa mã nguồn lên kho. Giữ dữ liệu giả lập và hướng dẫn seed. Tài khoản demo chỉ dùng cho trình diễn cục bộ. Hồ sơ báo cáo và video được quản lý riêng bên ngoài thư mục mã nguồn.
