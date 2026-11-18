using FinanceShopping.Web.Data;
using FinanceShopping.Web.Models;
using FinanceShopping.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

var builder=WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.Local.json",optional:true,reloadOnChange:false).AddEnvironmentVariables();
builder.Services.AddDbContext<AppDbContext>(o=>o.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddIdentity<ApplicationUser,IdentityRole>(o=>{
    o.User.RequireUniqueEmail=true;
    o.Password.RequiredLength=8;
    o.Lockout.MaxFailedAccessAttempts=5;
    o.Lockout.DefaultLockoutTimeSpan=TimeSpan.FromMinutes(5);
}).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(o=>{o.LoginPath="/Account/Login";o.AccessDeniedPath="/Account/Login";o.ExpireTimeSpan=TimeSpan.FromHours(4);o.SlidingExpiration=true;});
builder.Services.AddControllersWithViews(o=>o.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));
builder.Services.AddScoped<FinanceService>();
var app=builder.Build();
// Deterministic decimal binding: HTML number fields use a dot; UI formatting explicitly uses vi-VN.
CultureInfo.DefaultThreadCurrentCulture=CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture=CultureInfo.GetCultureInfo("vi-VN");
if(args.Contains("--export-sql"))
{
    using var scope=app.Services.CreateScope();
    Console.WriteLine(scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.GenerateCreateScript());return;
}
if(args.Contains("--init-db")||args.Contains("--seed-demo"))
{
    using var scope=app.Services.CreateScope();
    var db=scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
    if(args.Contains("--seed-demo"))await DemoSeeder.Seed(scope.ServiceProvider);
    Console.WriteLine("Đã khởi tạo cơ sở dữ liệu. Dữ liệu hiện có được giữ nguyên.");return;
}
if(!app.Environment.IsDevelopment())app.UseExceptionHandler("/Home/Error");
app.UseStaticFiles();app.UseRouting();app.UseAuthentication();app.UseAuthorization();
app.MapControllerRoute("default","{controller=Home}/{action=Index}/{id?}");
app.Run();
public partial class Program { }

public class WebEntryPoint { }
