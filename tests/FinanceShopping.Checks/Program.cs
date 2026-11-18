using FinanceShopping.Web.Data;
using FinanceShopping.Web.Models;
using FinanceShopping.Web.Services;
using FinanceShopping.Web.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Text.RegularExpressions;
using System.Net;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;

// Each SQL run creates a new database and never drops an existing database.
var sqlSetting=Environment.GetEnvironmentVariable("FINANCE_TEST_SQLSERVER");
if(!string.IsNullOrWhiteSpace(sqlSetting))
{
    var builder=new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(sqlSetting);
    if(!builder.InitialCatalog.EndsWith("_Test",StringComparison.Ordinal))throw new Exception("Test DB name must end with _Test");
    builder.InitialCatalog=builder.InitialCatalog[..^5]+"_"+Guid.NewGuid().ToString("N")[..12]+"_Test";
    Environment.SetEnvironmentVariable("FINANCE_TEST_SQLSERVER",builder.ConnectionString);
    Console.WriteLine($"SQL TEST DATABASE: {builder.InitialCatalog}; independent disposable fixture data; no database deletion");
}

int passed=0,failed=0;
void Assert(bool condition,string message="Kết quả không đúng") { if(!condition)throw new Exception(message); }
async Task Reject(Func<Task> work) {try{await work();}catch(BusinessException){return;}throw new Exception("Đáng lẽ phải từ chối thao tác");}
async Task Check(string id,string name,Func<Fixture,Task> test)
{
    await using var f=await Fixture.Create();
    try {await test(f);passed++;Console.WriteLine($"PASS {id} | {name}");}
    catch(Exception ex){failed++;Console.WriteLine($"FAIL {id} | {name} | {ex.GetType().Name}: {ex}");}
}
await Check("TC01","Thêm thu, số dư tính lại",async f=>{await f.S.SaveTransaction("A",f.Tx(FlowType.Income,1000));Assert((await f.S.Wallets("A"))[0].Balance==101000);});
await Check("TC02","Sửa và xóa giao dịch cập nhật số dư",async f=>{await f.S.SaveTransaction("A",f.Tx(FlowType.Expense,10000));var t=await f.Db.Transactions.SingleAsync();var edit=f.Tx(FlowType.Expense,20000);edit.Id=t.Id;await f.S.SaveTransaction("A",edit);Assert((await f.S.Wallets("A"))[0].Balance==80000);await f.S.DeleteTransaction("A",t.Id);Assert((await f.S.Wallets("A"))[0].Balance==100000);});
await Check("TC03","Số tiền 0 bị từ chối",f=>Reject(()=>f.S.SaveTransaction("A",f.Tx(FlowType.Expense,0))));
await Check("TC04","Ví tài khoản khác bị từ chối",async f=>{var t=f.Tx(FlowType.Expense,10);t.WalletId=f.OtherWallet;await Reject(()=>f.S.SaveTransaction("A",t));});
await Check("TC05","Danh mục sai loại bị từ chối",async f=>{var t=f.Tx(FlowType.Income,10);t.CategoryId=f.Expense;await Reject(()=>f.S.SaveTransaction("A",t));});
await Check("TC06","Không xóa ví được sử dụng",async f=>{await f.S.SaveTransaction("A",f.Tx(FlowType.Expense,10));await Reject(()=>f.S.DeleteWallet("A",f.Wallet));});
await Check("TC07","Không xóa/đổi loại danh mục được sử dụng",async f=>{await f.S.SaveCategory("A",new CategoryForm{Name="Tạm",Type=FlowType.Expense});var fresh=await f.Db.Categories.SingleAsync(x=>x.Name=="Tạm");await f.S.SaveCategory("A",new CategoryForm{Id=fresh.Id,Name="Tạm sửa",Type=FlowType.Income});await f.S.DeleteCategory("A",fresh.Id);await f.S.SaveTransaction("A",f.Tx(FlowType.Expense,10));await Reject(()=>f.S.DeleteCategory("A",f.Expense));await Reject(()=>f.S.SaveCategory("A",new CategoryForm{Id=f.Expense,Name="X",Type=FlowType.Income}));});
await Check("TC08","Ngân sách tổng chi nhiều ví, ngày cuối tháng",async f=>{await f.S.SaveBudget("A",new BudgetForm{Month=f.Month,Limit=50000});await f.S.SaveTransaction("A",f.Tx(FlowType.Expense,10000));await f.S.SaveWallet("A",new WalletForm{Name="Ví 2"});var w=await f.Db.Wallets.SingleAsync(x=>x.Name=="Ví 2");var t=f.Tx(FlowType.Expense,15000);t.WalletId=w.Id;t.Date=f.Month.AddMonths(1).AddDays(-1);await f.S.SaveTransaction("A",t);Assert(await f.S.Remaining("A",f.Month)==25000);});
await Check("TC09","Một ngân sách duy nhất và cập nhật hạn mức",async f=>{await f.S.SaveBudget("A",new BudgetForm{Month=f.Month,Limit=100});await f.S.SaveBudget("A",new BudgetForm{Month=f.Month.AddDays(15),Limit=200});Assert(await f.Db.Budgets.CountAsync()==1);Assert(await f.S.Remaining("A",f.Month)==200);});
await Check("TC10","Chưa đặt ngân sách trả null",async f=>Assert(await f.S.Remaining("A",f.Month)==null));
await Check("TC11","Chi phí dự kiến không trừ số dư",async f=>{Assert(FinanceService.Estimate((await f.S.GetList("A",f.List)).Items)==440000);Assert((await f.S.Wallets("A"))[0].Balance==100000);Assert(await f.Db.Transactions.CountAsync()==0);});
await Check("TC12","Xác nhận tạo đúng một giao dịch",async f=>{await f.S.Confirm("A",await f.Purchase());var l=await f.S.GetList("A",f.List);Assert(l.IsPurchased&&l.Purchase?.Amount==420000);Assert(await f.Db.Transactions.CountAsync()==1);});
await Check("TC13","Gửi xác nhận lặp không tạo khoản chi mới",async f=>{var p=await f.Purchase();Assert(await f.S.Confirm("A",p));Assert(!await f.S.Confirm("A",p));Assert(await f.Db.Transactions.CountAsync()==1);});
await Check("TC14","Không sửa danh sách đã mua",async f=>{await f.S.Confirm("A",await f.Purchase());var l=await f.S.GetList("A",f.List);await Reject(()=>f.S.SaveList("A",new ListForm{Id=l.Id,Version=l.Version,Name="Đổi",PlannedMonth=f.Month}));});
await Check("TC15","Không sửa/xóa giao dịch liên kết riêng",async f=>{await f.S.Confirm("A",await f.Purchase());var t=await f.Db.Transactions.SingleAsync();var form=f.Tx(FlowType.Expense,100);form.Id=t.Id;await Reject(()=>f.S.SaveTransaction("A",form));await Reject(()=>f.S.DeleteTransaction("A",t.Id));});
await Check("TC16","Hủy xác nhận khôi phục số dư và gửi lặp",async f=>{await f.S.Confirm("A",await f.Purchase());var l=await f.S.GetList("A",f.List);var v=l.Version;Assert(await f.S.Cancel("A",l.Id,v));Assert(!await f.S.Cancel("A",l.Id,v));Assert(await f.Db.Transactions.CountAsync()==0);Assert((await f.S.Wallets("A"))[0].Balance==100000);Assert(!(await f.S.GetList("A",l.Id)).IsPurchased);});
await Check("TC17","Lỗi sau khi lưu khoản chi phải rollback",async f=>{try{await f.S.Confirm("A",await f.Purchase(),()=>throw new InvalidOperationException("Injected"));}catch(InvalidOperationException){}f.Db.ChangeTracker.Clear();Assert(await f.Db.Transactions.CountAsync()==0);Assert(!(await f.S.GetList("A",f.List)).IsPurchased);});
await Check("TC18","Lỗi sau khi xóa khoản chi phải rollback",async f=>{await f.S.Confirm("A",await f.Purchase());var l=await f.S.GetList("A",f.List);try{await f.S.Cancel("A",l.Id,l.Version,()=>throw new InvalidOperationException("Injected"));}catch(InvalidOperationException){}f.Db.ChangeTracker.Clear();Assert(await f.Db.Transactions.CountAsync()==1);Assert((await f.S.GetList("A",f.List)).IsPurchased);});
await Check("TC19","Không đọc hoặc mua danh sách người khác",async f=>{await Reject(async()=>await f.S.GetList("B",f.List));var p=await f.Purchase();await Reject(()=>f.S.Confirm("B",p));});
await Check("TC20","Cảnh báo âm không chặn khoản chi",async f=>{await f.S.SaveBudget("A",new BudgetForm{Month=f.Month,Limit=100});await f.S.Confirm("A",await f.Purchase());Assert((await f.S.Wallets("A"))[0].Balance==-320000);Assert(await f.S.Remaining("A",f.Month)==-419900);Assert(await f.S.Warnings("A",f.Wallet,f.Month)!=null);});
await Check("TC21","Ngày mua thực tế quyết định tháng chi",async f=>{await f.S.SaveBudget("A",new BudgetForm{Month=f.Month,Limit=1000000});await f.S.SaveBudget("A",new BudgetForm{Month=f.Month.AddMonths(1),Limit=1000000});var p=await f.Purchase();p.Date=f.Month.AddMonths(1);await f.S.Confirm("A",p);Assert(await f.S.Remaining("A",f.Month)==1000000);Assert(await f.S.Remaining("A",f.Month.AddMonths(1))==580000);});
await Check("TC22","Phiên bản cũ không sửa được danh sách",async f=>{var old=(await f.S.GetList("A",f.List)).Version;await f.S.SaveList("A",new ListForm{Id=f.List,Version=old,Name="Mới",PlannedMonth=f.Month});await Reject(()=>f.S.SaveList("A",new ListForm{Id=f.List,Version=old,Name="Cũ",PlannedMonth=f.Month}));});
await Check("TC23","Số lượng 0 bị từ chối, đơn giá 0 hợp lệ",async f=>{var v=(await f.S.GetList("A",f.List)).Version;await Reject(()=>f.S.SaveItem("A",new ItemForm{ShoppingListId=f.List,Version=v,Name="X",Quantity=0}));await f.S.SaveItem("A",new ItemForm{ShoppingListId=f.List,Version=v,Name="Tặng",Quantity=1,EstimatedPrice=0});});
await Check("TC24","Không xác nhận danh sách rỗng",async f=>{var id=await f.S.SaveList("A",new ListForm{Name="Rỗng",PlannedMonth=f.Month});var p=await f.Purchase();p.Id=id;p.Version=(await f.S.GetList("A",id)).Version;await Reject(()=>f.S.Confirm("A",p));});
await Check("TC25","Báo cáo đúng khoảng ngày và cơ cấu chi",async f=>{await f.S.SaveTransaction("A",f.Tx(FlowType.Income,100));await f.S.SaveTransaction("A",f.Tx(FlowType.Expense,70));var o=await f.S.Overview("A",f.Month,f.Month.AddMonths(1).AddDays(-1),f.Month);Assert(o.Income==100&&o.Expense==70&&o.Chart.Single().Amount==70);});
await Check("TC26","Không sửa/xóa ví hoặc giao dịch người khác",async f=>{await Reject(()=>f.S.SaveWallet("B",new WalletForm{Id=f.Wallet,Name="X"}));await Reject(()=>f.S.DeleteWallet("B",f.Wallet));await f.S.SaveTransaction("A",f.Tx(FlowType.Expense,10));await Reject(()=>f.S.DeleteTransaction("B",f.Db.Transactions.Single().Id));});
await Check("TC27","Hủy, sửa mặt hàng rồi xác nhận lại",async f=>{await f.S.Confirm("A",await f.Purchase());var l=await f.S.GetList("A",f.List);await f.S.Cancel("A",l.Id,l.Version);l=await f.S.GetList("A",f.List);await f.S.SaveItem("A",new ItemForm{ShoppingListId=l.Id,Version=l.Version,Name="Bút",Quantity=1,EstimatedPrice=10000});await f.S.Confirm("A",await f.Purchase());Assert(await f.Db.Transactions.CountAsync()==1);Assert(FinanceService.Estimate((await f.S.GetList("A",l.Id)).Items)==450000);});
await Check("TC28","DB từ chối ngân sách trùng và liên kết mua trùng",async f=>{await f.S.SaveBudget("A",new BudgetForm{Month=f.Month,Limit=10});f.Db.Budgets.Add(new Budget{UserId="A",Month=f.Month,Limit=20});try{await f.Db.SaveChangesAsync();throw new Exception("Không từ chối trùng");}catch(DbUpdateException){}f.Db.ChangeTracker.Clear();await f.S.Confirm("A",await f.Purchase());f.Db.ChangeTracker.Clear();f.Db.Transactions.Add(new Transaction{UserId="A",WalletId=f.Wallet,CategoryId=f.Expense,Type=FlowType.Expense,Date=f.Month,Amount=1,ShoppingListId=f.List});try{await f.Db.SaveChangesAsync();throw new Exception("Không từ chối trùng");}catch(DbUpdateException){} });
await Check("TC30","Hai danh sách 700k và 600k vượt kế hoạch 300k, không trừ ngân sách",async f=>{
    await f.S.SaveBudget("A",new BudgetForm{Month=f.Month,Limit=1000000});
    var first=await f.S.GetList("A",f.List);foreach(var item in first.Items.ToList())await f.S.DeleteItem("A",first.Id,item.Id,(await f.S.GetList("A",first.Id)).Version);
    await f.S.SaveItem("A",new ItemForm{ShoppingListId=f.List,Version=(await f.S.GetList("A",f.List)).Version,Name="A",Quantity=1,EstimatedPrice=700000});
    var second=await f.S.SaveList("A",new ListForm{Name="B",PlannedMonth=f.Month});
    await f.S.SaveItem("A",new ItemForm{ShoppingListId=second,Version=(await f.S.GetList("A",second)).Version,Name="B",Quantity=1,EstimatedPrice=600000});
    Assert(await f.S.PlannedTotal("A",f.Month)==1300000);Assert(await f.S.Remaining("A",f.Month)==1000000);Assert((await f.S.Wallets("A"))[0].Balance==100000);
    await f.S.Confirm("A",await f.Purchase());Assert(await f.S.PlannedTotal("A",f.Month)==600000);Assert(await f.S.Remaining("A",f.Month)==580000);
    var list=await f.S.GetList("A",f.List);await f.S.Cancel("A",list.Id,list.Version);Assert(await f.S.PlannedTotal("A",f.Month)==1300000);
    Assert(await f.S.PlannedTotal("B",f.Month)==0&&await f.S.PlannedTotal("A",f.Month.AddMonths(1))==0);
});
await Check("TC31","Danh mục trùng tên tách nhóm theo ID và nhãn khác nhau",async f=>{
    await f.S.SaveCategory("A",new CategoryForm{Name="Chi",Type=FlowType.Expense});var id=await f.Db.Categories.Where(c=>c.UserId=="A"&&c.Type==FlowType.Expense&&c.Id!=f.Expense).Select(c=>c.Id).SingleAsync();
    await f.S.SaveTransaction("A",f.Tx(FlowType.Expense,70));var t=f.Tx(FlowType.Expense,30);t.CategoryId=id;await f.S.SaveTransaction("A",t);
    var o=await f.S.Overview("A",f.Month,f.Month.AddDays(20),f.Month);Assert(o.Chart.Count==2&&o.Chart.Sum(c=>c.Amount)==100&&o.Chart.Select(c=>c.Name).Distinct().Count()==2);
});
await Check("TC32","Cảnh báo chỉ âm ví, chỉ vượt ngân sách và cả hai",async f=>{
    await f.S.SaveTransaction("A",f.Tx(FlowType.Expense,110000));var w=await f.S.Warnings("A",f.Wallet,f.Month);Assert(w!=null&&w.Contains("Số dư ví âm")&&!w.Contains("vượt ngân sách"));
    await f.S.SaveTransaction("A",f.Tx(FlowType.Income,200000));await f.S.SaveBudget("A",new BudgetForm{Month=f.Month,Limit=100000});w=await f.S.Warnings("A",f.Wallet,f.Month);Assert(w!=null&&!w.Contains("Số dư ví âm")&&w.Contains("vượt ngân sách"));
    await f.S.SaveTransaction("A",f.Tx(FlowType.Expense,200000));w=await f.S.Warnings("A",f.Wallet,f.Month);Assert(w!=null&&w.Contains("Số dư ví âm")&&w.Contains("vượt ngân sách"));
});
await Check("TC33","Tổng dự kiến bằng tổng thành tiền đã làm tròn của từng mặt hàng",async f=>{
    var list=await f.S.GetList("A",f.List);
    foreach(var item in list.Items.ToList())await f.S.DeleteItem("A",list.Id,item.Id,(await f.S.GetList("A",list.Id)).Version);
    foreach(var name in new[]{"A","B"})await f.S.SaveItem("A",new ItemForm{ShoppingListId=list.Id,Version=(await f.S.GetList("A",list.Id)).Version,Name=name,Quantity=0.5m,EstimatedPrice=0.01m});
    list=await f.S.GetList("A",list.Id);
    Assert(list.Items.All(i=>FinanceService.ItemEstimate(i)==0.01m));
    Assert(FinanceService.Estimate(list.Items)==0.02m&&await f.S.PlannedTotal("A",f.Month)==0.02m);
});
await Check("TC34","Cảnh báo giữ phần thập phân, không hiển thị âm hoặc vượt 0 đồng",async f=>{
    await f.S.SaveBudget("A",new BudgetForm{Month=f.Month,Limit=100000});
    await f.S.SaveTransaction("A",f.Tx(FlowType.Expense,100000.01m));
    var message=await f.S.Warnings("A",f.Wallet,f.Month);
    Assert(message!=null&&message.Contains("Số dư ví âm 0,01 đ")&&message.Contains("vượt ngân sách 0,01 đ"));
});
if(!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("FINANCE_TEST_SQLSERVER")))
    for(var attempt=1;attempt<=3;attempt++)await Check($"SQL-CONCURRENT-{attempt}","Hai kết nối đọc cùng trạng thái trước khi cùng xác nhận",async f=>{
        var p=await f.Purchase();var gate=new PurchaseReadGate();
        async Task<string> Buy(){await using var d=f.Sibling(gate);try{return await new FinanceService(d).Confirm("A",p)?"CREATED":"ALREADY_CONFIRMED";}catch(Exception ex)when(FinanceService.IsSqlDeadlock(ex)){return "DEADLOCK_1205";}catch(DbUpdateConcurrencyException){return "VERSION_CONFLICT";}catch(DbUpdateException ex)when(ex.InnerException is Microsoft.Data.SqlClient.SqlException s&&s.Number is 2601 or 2627){return "UNIQUE_CONFLICT";}}
        var outcomes=await Task.WhenAll(Buy(),Buy()).WaitAsync(TimeSpan.FromSeconds(45));
        f.Db.ChangeTracker.Clear();var list=await f.S.GetList("A",f.List);var count=await f.Db.Transactions.CountAsync(t=>t.ShoppingListId==f.List);
        Assert(gate.Readers==2&&outcomes.Count(x=>x=="CREATED")==1&&count==1&&list.IsPurchased&&list.Purchase?.Amount==420000);
        Assert(!await f.S.Confirm("A",p));Assert(await f.Db.Transactions.CountAsync(t=>t.ShoppingListId==f.List)==1);
        Console.WriteLine($"EVIDENCE two independent connections; reads={gate.Readers}; results={string.Join(",",outcomes)}; linked={count}; purchased={list.IsPurchased}; amount={list.Purchase?.Amount}; retry=no duplicate");
    });
else Console.WriteLine("SKIP SQL-CONCURRENT-1..3 | requires FINANCE_TEST_SQLSERVER; not a SQLite concurrency claim");
// Exercise real routing, Identity cookies, CSRF and ownership with an HTTP test host.
try
{
    using var factory=new TestFactory();using var client=factory.CreateClient(new WebApplicationFactoryClientOptions{AllowAutoRedirect=false});
    using(var scope=factory.Services.CreateScope()) {await DemoSeeder.Seed(scope.ServiceProvider);}
    Assert((await client.GetAsync("/")).StatusCode==HttpStatusCode.Redirect);
    var login=await client.GetStringAsync("/Account/Login");
    var token=WebUtility.HtmlDecode(Regex.Match(login,"name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value);
    Assert(token.Length>20,"Thiếu CSRF token");
    var bad=await client.PostAsync("/Account/Login",new FormUrlEncodedContent(new Dictionary<string,string>{{"Email","demo@example.com"},{"Password","Demo@2026!"}}));Assert(bad.StatusCode==HttpStatusCode.BadRequest);
    var ok=await client.PostAsync("/Account/Login",new FormUrlEncodedContent(new Dictionary<string,string>{{"Email","demo@example.com"},{"Password","Demo@2026!"},{"__RequestVerificationToken",token}}));Assert(ok.StatusCode==HttpStatusCode.Redirect);
    foreach(var path in new[]{"/?month=2026-11-01","/Wallets","/Categories","/Transactions","/Budgets?month=2026-11-01","/Shopping","/Transactions/Edit","/Shopping/Edit"})Assert((await client.GetAsync(path)).StatusCode==HttpStatusCode.OK,path);
    int own,other;using(var scope=factory.Services.CreateScope()){var d=scope.ServiceProvider.GetRequiredService<AppDbContext>();own=await d.ShoppingLists.Where(x=>x.UserId==d.Users.Where(u=>u.Email=="demo@example.com").Select(u=>u.Id).First()).Select(x=>x.Id).FirstAsync();other=await d.ShoppingLists.Where(x=>x.UserId==d.Users.Where(u=>u.Email=="other@example.com").Select(u=>u.Id).First()).Select(x=>x.Id).FirstAsync();}
    Assert((await client.GetAsync($"/Shopping/Details/{own}")).StatusCode==HttpStatusCode.OK);
    Assert((await client.GetAsync($"/Shopping/Details/{other}")).StatusCode==HttpStatusCode.NotFound);
    using var signup=factory.CreateClient(new WebApplicationFactoryClientOptions{AllowAutoRedirect=false});
    var register=await signup.GetStringAsync("/Account/Register");
    var registerToken=WebUtility.HtmlDecode(Regex.Match(register,"name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value);
    var registration=await signup.PostAsync("/Account/Register",new FormUrlEncodedContent(new Dictionary<string,string>{{"Email","new@example.com"},{"Password","New@2026!"},{"ConfirmPassword","New@2026!"},{"__RequestVerificationToken",registerToken}}));
    Assert(registration.StatusCode==HttpStatusCode.Redirect,"Đăng ký thất bại");Assert((await signup.GetAsync("/")).StatusCode==HttpStatusCode.OK,"Không tự đăng nhập sau đăng ký");
    var blankList=await signup.GetStringAsync("/Shopping/Edit");
    var listToken=WebUtility.HtmlDecode(Regex.Match(blankList,"name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value);
    var created=await signup.PostAsync("/Shopping/Edit",new FormUrlEncodedContent(new Dictionary<string,string>{{"Name","Kế hoạch mới"},{"PlannedMonth","2026-11"},{"__RequestVerificationToken",listToken}}));Assert(created.StatusCode==HttpStatusCode.Redirect,"Binding tháng thất bại");
    Assert((await signup.GetAsync(created.Headers.Location!)).StatusCode==HttpStatusCode.OK);
    using(var scope=factory.Services.CreateScope()){var d=scope.ServiceProvider.GetRequiredService<AppDbContext>();var uid=await d.Users.Where(u=>u.Email=="new@example.com").Select(u=>u.Id).SingleAsync();var list=await d.ShoppingLists.SingleAsync(l=>l.UserId==uid);Assert(list.Name=="Kế hoạch mới"&&list.PlannedMonth==new DateTime(2026,11,1)&&!list.IsPurchased);}
    var editTransaction=await client.GetStringAsync("/Transactions/Edit");
    Assert(!Regex.IsMatch(editTransaction,"<input[^>]*data-val-required[^>]*name=\"Note\""),"Ghi chú không được bắt buộc");
    var transactionToken=WebUtility.HtmlDecode(Regex.Match(editTransaction,"name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value);
    int walletId,categoryId;string demoUser;
    using(var scope=factory.Services.CreateScope()){var d=scope.ServiceProvider.GetRequiredService<AppDbContext>();demoUser=await d.Users.Where(u=>u.Email=="demo@example.com").Select(u=>u.Id).SingleAsync();walletId=await d.Wallets.Where(w=>w.UserId==demoUser).Select(w=>w.Id).FirstAsync();categoryId=await d.Categories.Where(c=>c.UserId==demoUser&&c.Type==FlowType.Expense).Select(c=>c.Id).FirstAsync();}
    var transactionData=new Dictionary<string,string>{{"WalletId",walletId.ToString()},{"CategoryId",categoryId.ToString()},{"Type","Expense"},{"Amount","123"},{"Date","2026-11-04"},{"Note",""},{"__RequestVerificationToken",transactionToken}};
    Assert((await client.PostAsync("/Transactions/Edit",new FormUrlEncodedContent(transactionData))).StatusCode==HttpStatusCode.Redirect,"Ghi chú trống bị từ chối");
    using(var scope=factory.Services.CreateScope()){var d=scope.ServiceProvider.GetRequiredService<AppDbContext>();Assert(await d.Transactions.AnyAsync(t=>t.UserId==demoUser&&t.Amount==123&&t.Note==""),"Không lưu được ghi chú trống");}
    transactionData["Note"]=new string('x',301);
    var invalidNote=await client.PostAsync("/Transactions/Edit",new FormUrlEncodedContent(transactionData));
    Assert(invalidNote.StatusCode==HttpStatusCode.OK&&(await invalidNote.Content.ReadAsStringAsync()).Contains("300"),"Thiếu validation độ dài ghi chú");
    passed++;Console.WriteLine("PASS TC29 | HTTP Identity, CSRF, màn hình, tháng, quyền đọc chéo; ghi chú trống và quá 300 ký tự");
}
catch(Exception ex){failed++;Console.WriteLine($"FAIL TC29 | HTTP | {ex}");}
if(!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("FINANCE_TEST_SQLSERVER")))
{
    try {await HttpSqlChecks.Run();passed+=2;}
    catch(Exception ex){failed++;Console.WriteLine($"FAIL SQL-HTTP | {ex}");}
}
else Console.WriteLine("SKIP SQL-HTTP-1..2 | requires FINANCE_TEST_SQLSERVER");
var providerLabel=string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("FINANCE_TEST_SQLSERVER"))?"TC01–28, TC30–34 SQLite relational":"TC01–28, TC30–34 and SQL-CONCURRENT-1..3 SQL Server dedicated _Test";
Console.WriteLine($"RESULT: {passed} PASS, {failed} FAIL | {providerLabel}; TC29 HTTP SQLite TestServer; SQL mode adds SQL-HTTP-1..2 HTTP SQL Server TestServer");
return failed==0?0:1;

class Fixture : IAsyncDisposable
{
    SqliteConnection connection=null!;
    public AppDbContext Db=null!;public FinanceService S=null!;
    public int Wallet,OtherWallet,Expense,Income,List;
    public DateTime Month=new(2026,11,1);
    public static async Task<Fixture> Create()
    {
        var f=new Fixture();f.connection=new SqliteConnection("Data Source=:memory:");await f.connection.OpenAsync();
        var sql=Environment.GetEnvironmentVariable("FINANCE_TEST_SQLSERVER");
        var options=new DbContextOptionsBuilder<AppDbContext>();
        if(string.IsNullOrWhiteSpace(sql))options.UseSqlite(f.connection);else options.UseSqlServer(sql);
        f.Db=new AppDbContext(options.Options);
        // SQL mode requires a dedicated empty test DB named FinanceShopping_Test. Never point at real data.
        if(!string.IsNullOrWhiteSpace(sql)&&!new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(sql).InitialCatalog.EndsWith("_Test",StringComparison.Ordinal))throw new Exception("Test DB name must end with _Test");
        await f.Db.Database.EnsureCreatedAsync();f.S=new FinanceService(f.Db);
        f.Db.Users.AddRange(new ApplicationUser{Id="A",UserName="A",NormalizedUserName="A"},new ApplicationUser{Id="B",UserName="B",NormalizedUserName="B"});await f.Db.SaveChangesAsync();
        var w=new Wallet{UserId="A",Name="Ví A",OpeningBalance=100000};var other=new Wallet{UserId="B",Name="Ví B"};var e=new Category{UserId="A",Name="Chi",Type=FlowType.Expense};var i=new Category{UserId="A",Name="Thu",Type=FlowType.Income};
        var l=new ShoppingList{UserId="A",Name="Kế hoạch",PlannedMonth=f.Month,Items=[new(){UserId="A",Name="Sổ",Quantity=3,EstimatedPrice=30000},new(){UserId="A",Name="Balo",Quantity=1,EstimatedPrice=350000}]};
        f.Db.AddRange(w,other,e,i,l);await f.Db.SaveChangesAsync();f.Wallet=w.Id;f.OtherWallet=other.Id;f.Expense=e.Id;f.Income=i.Id;f.List=l.Id;return f;
    }
    public TransactionForm Tx(FlowType type,decimal amount)=>new(){WalletId=Wallet,CategoryId=type==FlowType.Income?Income:Expense,Type=type,Amount=amount,Date=Month.AddDays(2)};
    public async Task<PurchaseForm> Purchase()=>new(){Id=List,Version=(await S.GetList("A",List)).Version,Amount=420000,Date=Month.AddDays(4),WalletId=Wallet,CategoryId=Expense};
    public AppDbContext Sibling(DbCommandInterceptor interceptor)=>new(new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(Environment.GetEnvironmentVariable("FINANCE_TEST_SQLSERVER")).AddInterceptors(interceptor).Options);
    public async ValueTask DisposeAsync(){
        if(!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("FINANCE_TEST_SQLSERVER"))){
            Db.ChangeTracker.Clear();
            await Db.Transactions.Where(x=>x.UserId=="A"||x.UserId=="B").ExecuteDeleteAsync();
            await Db.ShoppingItems.Where(x=>x.UserId=="A"||x.UserId=="B").ExecuteDeleteAsync();
            await Db.ShoppingLists.Where(x=>x.UserId=="A"||x.UserId=="B").ExecuteDeleteAsync();
            await Db.Budgets.Where(x=>x.UserId=="A"||x.UserId=="B").ExecuteDeleteAsync();
            await Db.Categories.Where(x=>x.UserId=="A"||x.UserId=="B").ExecuteDeleteAsync();
            await Db.Wallets.Where(x=>x.UserId=="A"||x.UserId=="B").ExecuteDeleteAsync();
            await Db.Users.Where(x=>x.Id=="A"||x.Id=="B").ExecuteDeleteAsync();
        }
        await Db.DisposeAsync();await connection.DisposeAsync();
    }
}
class PurchaseReadGate : DbCommandInterceptor
{
    readonly TaskCompletionSource ready=new(TaskCreationOptions.RunContinuationsAsynchronously);
    int readers;
    public int Readers=>readers;
    public override async ValueTask<DbDataReader> ReaderExecutedAsync(DbCommand command,CommandExecutedEventData eventData,DbDataReader result,CancellationToken cancellationToken=default)
    {
        if(command.CommandText.TrimStart().StartsWith("SELECT",StringComparison.OrdinalIgnoreCase)&&command.CommandText.Contains("[ShoppingLists]")){
            if(Interlocked.Increment(ref readers)==2)ready.TrySetResult();
            await ready.Task.WaitAsync(TimeSpan.FromSeconds(15),cancellationToken);
        }
        return result;
    }
}
class TestFactory : WebApplicationFactory<WebEntryPoint>
{
    readonly SqliteConnection connection=new("Data Source=:memory:");
    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        var root=AppContext.BaseDirectory;while(!File.Exists(Path.Combine(root,"FinanceShopping.sln"))){root=Directory.GetParent(root)?.FullName??throw new Exception("Solution root missing");}
        builder.UseSetting("contentRoot",Path.Combine(root,"src","FinanceShopping.Web"));
        builder.ConfigureServices(services=>{
            connection.Open();services.RemoveAll<DbContextOptions<AppDbContext>>();services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
            services.AddDbContext<AppDbContext>(o=>o.UseSqlite(connection));
            using var provider=services.BuildServiceProvider();using var scope=provider.CreateScope();scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
        });
    }
    protected override void Dispose(bool disposing){base.Dispose(disposing);if(disposing)connection.Dispose();}
}
