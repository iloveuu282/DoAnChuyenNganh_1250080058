using FinanceShopping.Web.Models;
using FinanceShopping.Web.Services;
using FinanceShopping.Web.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace FinanceShopping.Web.Data;
public static class DemoSeeder
{
    public static async Task Seed(IServiceProvider services)
    {
        var manager=services.GetRequiredService<UserManager<ApplicationUser>>();
        var db=services.GetRequiredService<AppDbContext>();
        var service=services.GetRequiredService<FinanceService>();
        foreach(var email in new[]{"demo@example.com","other@example.com"})
        {
            var user=await manager.FindByEmailAsync(email);
            if(user==null)
            {
                user=new ApplicationUser{UserName=email,Email=email,EmailConfirmed=true};
                var result=await manager.CreateAsync(user,"Demo@2026!");
                if(!result.Succeeded)throw new InvalidOperationException(string.Join("; ",result.Errors.Select(x=>x.Description)));
            }
            if(await db.Wallets.AnyAsync(x=>x.UserId==user.Id))continue;
            await using var tx=await db.Database.BeginTransactionAsync();
            var w1=new Wallet{UserId=user.Id,Name="Tiền mặt",OpeningBalance=2000000};
            var w2=new Wallet{UserId=user.Id,Name="Tài khoản ngân hàng",OpeningBalance=5000000};
            var income=new Category{UserId=user.Id,Name="Lương và trợ cấp",Type=FlowType.Income};
            var food=new Category{UserId=user.Id,Name="Ăn uống",Type=FlowType.Expense};
            var buy=new Category{UserId=user.Id,Name="Mua sắm",Type=FlowType.Expense};
            db.AddRange(w1,w2,income,food,buy);await db.SaveChangesAsync();
            var month=new DateTime(2026,11,1);
            db.Budgets.Add(new Budget{UserId=user.Id,Month=month,Limit=3000000});
            db.Transactions.AddRange(new Transaction{UserId=user.Id,WalletId=w2.Id,CategoryId=income.Id,Type=FlowType.Income,Amount=8000000,Date=month.AddDays(1),Note="Dữ liệu giả lập"},new Transaction{UserId=user.Id,WalletId=w1.Id,CategoryId=food.Id,Type=FlowType.Expense,Amount=450000,Date=month.AddDays(2),Note="Ăn uống trong tuần"});
            var list=new ShoppingList{UserId=user.Id,Name="Đồ dùng học tập tháng 11",PlannedMonth=month};
            list.Items.Add(new ShoppingItem{UserId=user.Id,Name="Sổ ghi chép",Quantity=3,EstimatedPrice=30000,Priority=Priority.High});
            list.Items.Add(new ShoppingItem{UserId=user.Id,Name="Balo",Quantity=1,EstimatedPrice=350000,Priority=Priority.Normal});
            db.ShoppingLists.Add(list);
            await db.SaveChangesAsync();await tx.CommitAsync();
        }
    }
}
