using System.Data.Common;
using System.Net;
using System.Text.RegularExpressions;
using FinanceShopping.Web.Data;
using FinanceShopping.Web.Models;
using FinanceShopping.Web.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

// Runs the real MVC/Identity/antiforgery pipeline against an isolated SQL Server database.
// TestServer is in process: this does not claim a physical two-device browser test.
static class HttpSqlChecks
{
    static void Require(bool value,string message) { if(!value)throw new Exception(message); }
    static string Token(string html)
    {
        var token=WebUtility.HtmlDecode(Regex.Match(html,"name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value);
        Require(token.Length>20,"Missing antiforgery token");return token;
    }
    static async Task Login(HttpClient client,string email)
    {
        var token=Token(await client.GetStringAsync("/Account/Login"));
        using var response=await client.PostAsync("/Account/Login",new FormUrlEncodedContent(new Dictionary<string,string>{
            ["Email"]=email,["Password"]="Demo@2026!",["__RequestVerificationToken"]=token}));
        Require(response.StatusCode==HttpStatusCode.Redirect,"SQL HTTP login failed");
    }
    public static async Task Run()
    {
        using var factory=new SqlHttpFactory();
        using var first=factory.CreateClient(new WebApplicationFactoryClientOptions{AllowAutoRedirect=false});
        using var second=factory.CreateClient(new WebApplicationFactoryClientOptions{AllowAutoRedirect=false});
        using var other=factory.CreateClient(new WebApplicationFactoryClientOptions{AllowAutoRedirect=false});
        using(var scope=factory.Services.CreateScope())await DemoSeeder.Seed(scope.ServiceProvider);
        await Login(first,"demo@example.com");await Login(second,"demo@example.com");await Login(other,"other@example.com");
        int id,wallet,category;Guid originalVersion;decimal originalBalance;decimal? originalRemaining;
        using(var scope=factory.Services.CreateScope())
        {
            var db=scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var user=await db.Users.Where(u=>u.Email=="demo@example.com").Select(u=>u.Id).SingleAsync();
            var list=await db.ShoppingLists.SingleAsync(l=>l.UserId==user);
            id=list.Id;originalVersion=list.Version;
            wallet=await db.Wallets.Where(w=>w.UserId==user&&w.Name=="Tiền mặt").Select(w=>w.Id).SingleAsync();
            category=await db.Categories.Where(c=>c.UserId==user&&c.Name=="Mua sắm").Select(c=>c.Id).SingleAsync();
            var finance=scope.ServiceProvider.GetRequiredService<FinanceService>();
            originalBalance=(await finance.Wallets(user)).Single(w=>w.Wallet.Id==wallet).Balance;
            originalRemaining=await finance.Remaining(user,new DateTime(2026,11,1));
        }
        var detail=$"/Shopping/Details/{id}";
        Dictionary<string,string> Purchase(string token)=>new(){["Id"]=id.ToString(),["Version"]=originalVersion.ToString(),
            ["Amount"]="420000",["Date"]="2026-11-05",["WalletId"]=wallet.ToString(),["CategoryId"]=category.ToString(),["__RequestVerificationToken"]=token};
        var token1=Token(await first.GetStringAsync(detail));var token2=Token(await second.GetStringAsync(detail));
        using(var missing=await first.PostAsync("/Shopping/Confirm",new FormUrlEncodedContent(Purchase(token1).Where(p=>p.Key!="__RequestVerificationToken"))))
            Require(missing.StatusCode==HttpStatusCode.BadRequest,"Purchase accepted without CSRF token");
        // Cross-account POST must also leave the original list and its transactions untouched.
        var otherToken=Token(await other.GetStringAsync("/Shopping"));
        using(var denied=await other.PostAsync("/Shopping/Confirm",new FormUrlEncodedContent(Purchase(otherToken))))
            Require(denied.StatusCode==HttpStatusCode.Redirect,"Unexpected cross-account response");
        using(var deniedPage=await other.GetAsync(detail))Require(deniedPage.StatusCode==HttpStatusCode.NotFound,"Cross-account list exposed");
        await Verify(false,0,originalBalance,originalRemaining);

        factory.Gate.Arm();
        HttpResponseMessage[] responses;
        try
        {
            responses=await Task.WhenAll(first.PostAsync("/Shopping/Confirm",new FormUrlEncodedContent(Purchase(token1))),
                second.PostAsync("/Shopping/Confirm",new FormUrlEncodedContent(Purchase(token2)))).WaitAsync(TimeSpan.FromSeconds(60));
        }
        finally {factory.Gate.Disarm();}
        foreach(var response in responses){using(response)Require(response.StatusCode==HttpStatusCode.Redirect,"Concurrent purchase returned an error page");}
        Require(factory.Gate.Readers==2,"Two HTTP requests did not overlap at the SQL read");
        await Verify(true,1,originalBalance-420000,originalRemaining-420000);
        var pages=await Task.WhenAll(first.GetStringAsync(detail),second.GetStringAsync(detail));
        Require(pages.Select(WebUtility.HtmlDecode).Count(p=>p!.Contains("Đã xác nhận mua và tạo một giao dịch chi tiêu."))==1,"Expected one successful purchase notification");
        Require(pages.Select(WebUtility.HtmlDecode).Any(p=>p!.Contains("Có thao tác đồng thời")||p.Contains("Dữ liệu đã thay đổi")||p.Contains("yêu cầu trùng")),"Conflict did not produce a friendly message");
        // Retrying the original successful/failed form must not create a second expense.
        using(var retry=await first.PostAsync("/Shopping/Confirm",new FormUrlEncodedContent(Purchase(token1))))
            Require(retry.StatusCode==HttpStatusCode.Redirect,"Retry failed");
        await Verify(true,1,originalBalance-420000,originalRemaining-420000);
        Console.WriteLine("PASS SQL-HTTP-1 | Two independent login cookies, CSRF, cross-account POST, simultaneous Confirm, friendly conflict, retry; linked=1; amount=420000; SQL Server TestServer");

        Guid purchasedVersion;
        using(var scope=factory.Services.CreateScope())purchasedVersion=await scope.ServiceProvider.GetRequiredService<AppDbContext>().ShoppingLists.Where(l=>l.Id==id).Select(l=>l.Version).SingleAsync();
        token1=Token(await first.GetStringAsync(detail));token2=Token(await second.GetStringAsync(detail));
        Dictionary<string,string> Cancel(string token)=>new(){["id"]=id.ToString(),["version"]=purchasedVersion.ToString(),["__RequestVerificationToken"]=token};
        factory.Gate.Arm();
        try
        {
            responses=await Task.WhenAll(first.PostAsync("/Shopping/Cancel",new FormUrlEncodedContent(Cancel(token1))),
                second.PostAsync("/Shopping/Cancel",new FormUrlEncodedContent(Cancel(token2)))).WaitAsync(TimeSpan.FromSeconds(60));
        }
        finally {factory.Gate.Disarm();}
        foreach(var response in responses){using(response)Require(response.StatusCode==HttpStatusCode.Redirect,"Concurrent cancel returned an error page");}
        Require(factory.Gate.Readers==2,"Cancel requests did not overlap");
        await Verify(false,0,originalBalance,originalRemaining);
        using(var retry=await first.PostAsync("/Shopping/Cancel",new FormUrlEncodedContent(Cancel(token1))))
            Require(retry.StatusCode==HttpStatusCode.Redirect,"Repeated cancel failed");
        // An old purchase form from before cancellation must be rejected by Version.
        using(var stale=await second.PostAsync("/Shopping/Confirm",new FormUrlEncodedContent(Purchase(token2))))
            Require(stale.StatusCode==HttpStatusCode.Redirect,"Stale purchase response failed");
        await Verify(false,0,originalBalance,originalRemaining);
        var finalPage=WebUtility.HtmlDecode(await second.GetStringAsync(detail));
        Require(finalPage.Contains("Danh sách đã thay đổi"),"Stale purchase form was not explained");
        Console.WriteLine("PASS SQL-HTTP-2 | Simultaneous Cancel, repeated cancel, stale purchase after cancel; linked=0; wallet and monthly budget restored; SQL Server TestServer");

        async Task Verify(bool purchased,int count,decimal balance,decimal? remaining)
        {
            using var scope=factory.Services.CreateScope();var db=scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var list=await db.ShoppingLists.AsNoTracking().SingleAsync(l=>l.Id==id);
            var transactions=await db.Transactions.Where(t=>t.ShoppingListId==id).AsNoTracking().ToListAsync();
            Require(list.IsPurchased==purchased&&transactions.Count==count,"Inconsistent purchase state");
            if(count==1)Require(transactions[0].Amount==420000&&transactions[0].Date==new DateTime(2026,11,5)&&transactions[0].WalletId==wallet&&transactions[0].CategoryId==category,"Incorrect linked transaction");
            var finance=scope.ServiceProvider.GetRequiredService<FinanceService>();
            Require((await finance.Wallets(list.UserId)).Single(w=>w.Wallet.Id==wallet).Balance==balance,"Wallet balance changed unexpectedly");
            Require(await finance.Remaining(list.UserId,new DateTime(2026,11,1))==remaining,"Monthly budget changed unexpectedly");
        }
    }
}

sealed class SqlHttpFactory : WebApplicationFactory<WebEntryPoint>
{
    public HttpPurchaseGate Gate {get;}=new();
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var sql=new SqlConnectionStringBuilder(Environment.GetEnvironmentVariable("FINANCE_TEST_SQLSERVER"));
        if(!sql.InitialCatalog.EndsWith("_Test",StringComparison.Ordinal))throw new Exception("HTTP test DB must end with _Test");
        sql.InitialCatalog="FinanceShopping_Http_"+Guid.NewGuid().ToString("N")[..12]+"_Test";
        Console.WriteLine($"SQL HTTP TEST DATABASE: {sql.InitialCatalog}; no existing database is deleted or reused");
        var root=AppContext.BaseDirectory;
        while(!File.Exists(Path.Combine(root,"FinanceShopping.sln")))root=Directory.GetParent(root)?.FullName??throw new Exception("Solution root missing");
        builder.UseSetting("contentRoot",Path.Combine(root,"src","FinanceShopping.Web"));
        builder.ConfigureLogging(logging=>logging.SetMinimumLevel(LogLevel.Warning));
        builder.ConfigureServices(services=>{
            services.RemoveAll<DbContextOptions<AppDbContext>>();services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
            services.AddDbContext<AppDbContext>(o=>o.UseSqlServer(sql.ConnectionString).AddInterceptors(Gate));
            using var provider=services.BuildServiceProvider();using var scope=provider.CreateScope();
            scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
        });
    }
}

sealed class HttpPurchaseGate : DbCommandInterceptor
{
    TaskCompletionSource ready=new(TaskCreationOptions.RunContinuationsAsynchronously);
    int armed,readers;
    public int Readers=>Volatile.Read(ref readers);
    public void Arm(){readers=0;ready=new(TaskCreationOptions.RunContinuationsAsynchronously);Volatile.Write(ref armed,1);}
    public void Disarm(){Volatile.Write(ref armed,0);ready.TrySetResult();}
    public override async ValueTask<DbDataReader> ReaderExecutedAsync(DbCommand command,CommandExecutedEventData eventData,DbDataReader result,CancellationToken cancellationToken=default)
    {
        if(Volatile.Read(ref armed)==1&&command.Transaction!=null&&command.CommandText.TrimStart().StartsWith("SELECT",StringComparison.OrdinalIgnoreCase)&&command.CommandText.Contains("[ShoppingLists]"))
        {
            if(Interlocked.Increment(ref readers)==2)ready.TrySetResult();
            await ready.Task.WaitAsync(TimeSpan.FromSeconds(20),cancellationToken);
        }
        return result;
    }
}
