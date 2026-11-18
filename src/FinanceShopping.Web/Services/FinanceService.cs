using System.Data;
using FinanceShopping.Web.Data;
using FinanceShopping.Web.Models;
using FinanceShopping.Web.ViewModels;
using Microsoft.EntityFrameworkCore;
namespace FinanceShopping.Web.Services;

public class BusinessException(string message) : Exception(message);
public class FinanceService(AppDbContext db)
{
    public static bool IsSqlDeadlock(Exception error)
    {
        for(Exception? current=error;current!=null;current=current.InnerException)
            if(current is Microsoft.Data.SqlClient.SqlException {Number:1205})return true;
        return false;
    }
    public static DateTime Month(DateTime date) => new(date.Year,date.Month,1);
    public static decimal ItemEstimate(ShoppingItem item) => decimal.Round(item.Quantity*item.EstimatedPrice,2,MidpointRounding.AwayFromZero);
    public static decimal Estimate(IEnumerable<ShoppingItem> items) => items.Sum(ItemEstimate);
    public static void ValidDate(DateTime date)
    {
        if(date.Year<1900 || date.Year>2100) throw new BusinessException("Ngày phải thuộc giai đoạn 1900–2100.");
    }
    static string Name(string name,int max)
    {
        name=name.Trim();
        if(name.Length==0 || name.Length>max) throw new BusinessException($"Tên phải có từ 1 đến {max} ký tự.");
        return name;
    }
    static void Amount(decimal value,bool zero=false)
    {
        if(value<(zero?0:0.01m) || value>999999999999m || decimal.Round(value,2)!=value)
            throw new BusinessException("Số tiền không hợp lệ; nhập tối đa 2 chữ số thập phân.");
    }
    public async Task<List<WalletSummary>> Wallets(string user)
    {
        var wallets=await db.Wallets.Where(x=>x.UserId==user).AsNoTracking().OrderBy(x=>x.Name).ToListAsync();
        var totals=await db.Transactions.Where(x=>x.UserId==user).GroupBy(x=>x.WalletId)
            .Select(g=>new {Id=g.Key,Total=g.Sum(x=>x.Type==FlowType.Income?x.Amount:-x.Amount)}).ToListAsync();
        return wallets.Select(w=>new WalletSummary(w,w.OpeningBalance+(totals.FirstOrDefault(t=>t.Id==w.Id)?.Total??0))).ToList();
    }
    public async Task SaveWallet(string user,WalletForm f)
    {
        var w=f.Id==0?new Wallet{UserId=user}:await db.Wallets.SingleOrDefaultAsync(x=>x.UserId==user&&x.Id==f.Id)??throw new BusinessException("Không tìm thấy ví.");
        if(Math.Abs(f.OpeningBalance)>999999999999m || decimal.Round(f.OpeningBalance,2)!=f.OpeningBalance) throw new BusinessException("Số dư ban đầu không hợp lệ.");
        w.Name=Name(f.Name,100); w.OpeningBalance=f.OpeningBalance;
        if(f.Id==0) db.Wallets.Add(w);
        await db.SaveChangesAsync();
    }
    public async Task DeleteWallet(string user,int id)
    {
        var w=await db.Wallets.SingleOrDefaultAsync(x=>x.UserId==user&&x.Id==id)??throw new BusinessException("Không tìm thấy ví.");
        if(await db.Transactions.AnyAsync(x=>x.WalletId==id)) throw new BusinessException("Ví đang có giao dịch, không thể xóa.");
        db.Wallets.Remove(w); await db.SaveChangesAsync();
    }
    public async Task SaveCategory(string user,CategoryForm f)
    {
        if(!Enum.IsDefined(f.Type)) throw new BusinessException("Loại danh mục không hợp lệ.");
        var c=f.Id==0?new Category{UserId=user}:await db.Categories.SingleOrDefaultAsync(x=>x.UserId==user&&x.Id==f.Id)??throw new BusinessException("Không tìm thấy danh mục.");
        if(c.Id!=0&&c.Type!=f.Type&&await db.Transactions.AnyAsync(x=>x.CategoryId==c.Id)) throw new BusinessException("Không thể đổi loại danh mục đã được sử dụng.");
        c.Name=Name(f.Name,100); c.Type=f.Type;
        if(f.Id==0) db.Categories.Add(c);
        await db.SaveChangesAsync();
    }
    public async Task DeleteCategory(string user,int id)
    {
        var c=await db.Categories.SingleOrDefaultAsync(x=>x.UserId==user&&x.Id==id)??throw new BusinessException("Không tìm thấy danh mục.");
        if(await db.Transactions.AnyAsync(x=>x.CategoryId==id)) throw new BusinessException("Danh mục đang có giao dịch, không thể xóa.");
        db.Categories.Remove(c); await db.SaveChangesAsync();
    }
    async Task ValidateReferences(string user,int wallet,int category,FlowType type)
    {
        if(!Enum.IsDefined(type)) throw new BusinessException("Loại giao dịch không hợp lệ.");
        if(!await db.Wallets.AnyAsync(x=>x.Id==wallet&&x.UserId==user)) throw new BusinessException("Ví không hợp lệ.");
        if(!await db.Categories.AnyAsync(x=>x.Id==category&&x.UserId==user&&x.Type==type)) throw new BusinessException("Danh mục phải thuộc tài khoản và đúng loại thu/chi.");
    }
    public async Task SaveTransaction(string user,TransactionForm f)
    {
        Amount(f.Amount); ValidDate(f.Date);
        await ValidateReferences(user,f.WalletId,f.CategoryId,f.Type);
        var t=f.Id==0?new Transaction{UserId=user}:await db.Transactions.SingleOrDefaultAsync(x=>x.UserId==user&&x.Id==f.Id)??throw new BusinessException("Không tìm thấy giao dịch.");
        if(t.ShoppingListId!=null) throw new BusinessException("Hãy hủy xác nhận mua trước khi điều chỉnh khoản chi này.");
        t.WalletId=f.WalletId;t.CategoryId=f.CategoryId;t.Type=f.Type;t.Amount=f.Amount;t.Date=f.Date.Date;t.Note=(f.Note??"").Trim();
        if(t.Note.Length>300) throw new BusinessException("Ghi chú tối đa 300 ký tự.");
        if(f.Id==0)db.Transactions.Add(t);
        await db.SaveChangesAsync();
    }
    public async Task DeleteTransaction(string user,int id)
    {
        var t=await db.Transactions.SingleOrDefaultAsync(x=>x.Id==id&&x.UserId==user)??throw new BusinessException("Không tìm thấy giao dịch.");
        if(t.ShoppingListId!=null) throw new BusinessException("Hãy hủy xác nhận mua trước khi xóa khoản chi này.");
        db.Transactions.Remove(t); await db.SaveChangesAsync();
    }
    public async Task<decimal?> Remaining(string user,DateTime month)
    {
        month=Month(month);var end=month.AddMonths(1);
        var budget=await db.Budgets.Where(x=>x.UserId==user&&x.Month==month).Select(x=>(decimal?)x.Limit).SingleOrDefaultAsync();
        var expense=await db.Transactions.Where(x=>x.UserId==user&&x.Type==FlowType.Expense&&x.Date>=month&&x.Date<end).SumAsync(x=>(decimal?)x.Amount)??0;
        return budget-expense;
    }
    public async Task SaveBudget(string user,BudgetForm f)
    {
        Amount(f.Limit); ValidDate(f.Month); var month=Month(f.Month);
        var b=await db.Budgets.SingleOrDefaultAsync(x=>x.UserId==user&&x.Month==month);
        if(b==null){b=new Budget{UserId=user,Month=month};db.Budgets.Add(b);}
        b.Limit=f.Limit;await db.SaveChangesAsync();
    }
    public async Task<decimal> PlannedTotal(string user,DateTime month)
    {
        month=Month(month);
        var lists=await db.ShoppingLists.AsNoTracking().Include(x=>x.Items)
            .Where(x=>x.UserId==user&&!x.IsPurchased&&x.PlannedMonth==month).ToListAsync();
        return lists.Sum(x=>Estimate(x.Items));
    }
    public async Task<ShoppingList> GetList(string user,int id)
        => await db.ShoppingLists.Include(x=>x.Items).Include(x=>x.Purchase).SingleOrDefaultAsync(x=>x.UserId==user&&x.Id==id)??throw new BusinessException("Không tìm thấy danh sách.");
    static void Editable(ShoppingList l,Guid version)
    {
        if(l.IsPurchased)throw new BusinessException("Danh sách đã mua. Hãy hủy xác nhận trước khi chỉnh sửa.");
        if(l.Version!=version)throw new BusinessException("Danh sách đã thay đổi. Hãy tải lại trang.");
    }
    public async Task<int> SaveList(string user,ListForm f)
    {
        ValidDate(f.PlannedMonth);
        var l=f.Id==0?new ShoppingList{UserId=user}:await GetList(user,f.Id);
        if(f.Id!=0)Editable(l,f.Version);
        l.Name=Name(f.Name,150);l.PlannedMonth=Month(f.PlannedMonth);l.Version=Guid.NewGuid();
        if(f.Id==0)db.ShoppingLists.Add(l);
        await db.SaveChangesAsync();return l.Id;
    }
    public async Task DeleteList(string user,int id,Guid version)
    {
        var l=await GetList(user,id);Editable(l,version);
        db.ShoppingLists.Remove(l);await db.SaveChangesAsync();
    }
    public async Task SaveItem(string user,ItemForm f)
    {
        var l=await GetList(user,f.ShoppingListId);Editable(l,f.Version);
        if(f.Quantity<=0||f.Quantity>999999||decimal.Round(f.Quantity,2)!=f.Quantity)throw new BusinessException("Số lượng phải lớn hơn 0, tối đa 2 chữ số thập phân.");
        Amount(f.EstimatedPrice,true);
        if(!Enum.IsDefined(f.Priority))throw new BusinessException("Mức ưu tiên không hợp lệ.");
        var item=f.Id==0?new ShoppingItem{UserId=user,ShoppingListId=l.Id}:l.Items.SingleOrDefault(x=>x.Id==f.Id&&x.UserId==user)??throw new BusinessException("Không tìm thấy mặt hàng.");
        item.Name=Name(f.Name,150);item.Quantity=f.Quantity;item.EstimatedPrice=f.EstimatedPrice;item.Priority=f.Priority;
        if(f.Id==0)db.ShoppingItems.Add(item);
        l.Version=Guid.NewGuid();await db.SaveChangesAsync();
    }
    public async Task DeleteItem(string user,int listId,int itemId,Guid version)
    {
        var l=await GetList(user,listId);Editable(l,version);
        var item=l.Items.SingleOrDefault(x=>x.Id==itemId&&x.UserId==user)??throw new BusinessException("Không tìm thấy mặt hàng.");
        db.ShoppingItems.Remove(item);l.Version=Guid.NewGuid();await db.SaveChangesAsync();
    }
    // fault is injected by integration tests only, never exposed as HTTP input.
    public async Task<bool> Confirm(string user,PurchaseForm f,Func<Task>? fault=null)
    {
        await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var l=await GetList(user,f.Id);
        if(l.IsPurchased){await tx.CommitAsync();return false;}
        Editable(l,f.Version); Amount(f.Amount);ValidDate(f.Date);
        if(l.Items.Count==0)throw new BusinessException("Cần ít nhất một mặt hàng trước khi xác nhận mua.");
        await ValidateReferences(user,f.WalletId,f.CategoryId,FlowType.Expense);
        db.Transactions.Add(new Transaction{UserId=user,WalletId=f.WalletId,CategoryId=f.CategoryId,Type=FlowType.Expense,Amount=f.Amount,Date=f.Date.Date,Note="Mua sắm: "+l.Name,ShoppingListId=l.Id});
        await db.SaveChangesAsync();
        if(fault!=null)await fault();
        l.IsPurchased=true;l.Version=Guid.NewGuid();await db.SaveChangesAsync();
        await tx.CommitAsync();return true;
    }
    public async Task<bool> Cancel(string user,int id,Guid version,Func<Task>? fault=null)
    {
        await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var l=await GetList(user,id);
        if(!l.IsPurchased){await tx.CommitAsync();return false;}
        if(l.Version!=version)throw new BusinessException("Danh sách đã thay đổi. Hãy tải lại trang.");
        if(l.Purchase==null)throw new BusinessException("Thiếu giao dịch liên kết, cần kiểm tra cơ sở dữ liệu.");
        db.Transactions.Remove(l.Purchase);await db.SaveChangesAsync();
        if(fault!=null)await fault();
        l.IsPurchased=false;l.Version=Guid.NewGuid();await db.SaveChangesAsync();
        await tx.CommitAsync();return true;
    }
    public async Task<string?> Warnings(string user,int wallet,DateTime date)
    {
        var balance=(await Wallets(user)).Single(x=>x.Wallet.Id==wallet).Balance;
        var remaining=await Remaining(user,date);
        var messages=new List<string>();
        var vi=System.Globalization.CultureInfo.GetCultureInfo("vi-VN");
        if(balance<0)messages.Add($"Số dư ví âm {(-balance).ToString("#,##0.##",vi)} đ: chi đã ghi vượt số dư ban đầu và thu đã ghi. Hãy kiểm tra dữ liệu của ví.");
        if(remaining<0)messages.Add($"Tổng chi thực tế tháng {date:MM/yyyy} vượt ngân sách {(-remaining.Value).ToString("#,##0.##",vi)} đ trên tất cả ví.");
        return messages.Count==0?null:"Đã ghi nhận khoản chi. "+string.Join(" ",messages);
    }
    public async Task<Overview> Overview(string user,DateTime from,DateTime to,DateTime month)
    {
        ValidDate(from);ValidDate(to);ValidDate(month);
        if(from.Date>to.Date)throw new BusinessException("Ngày bắt đầu không được sau ngày kết thúc.");
        var end=to.Date.AddDays(1);month=Month(month);var monthEnd=month.AddMonths(1);
        var data=await db.Transactions.Where(x=>x.UserId==user&&x.Date>=from.Date&&x.Date<end).GroupBy(x=>x.Type).Select(g=>new {Type=g.Key,Total=g.Sum(x=>x.Amount)}).ToListAsync();
        var groups=await db.Transactions.Where(x=>x.UserId==user&&x.Type==FlowType.Expense&&x.Date>=month&&x.Date<monthEnd)
            .GroupBy(x=>new{x.CategoryId,x.Category.Name}).Select(g=>new{g.Key.CategoryId,g.Key.Name,Amount=g.Sum(x=>x.Amount)}).ToListAsync();
        // IDs identify categories; duplicate names remain separate and receive distinct labels.
        var duplicateNames=groups.GroupBy(x=>x.Name,StringComparer.OrdinalIgnoreCase).Where(g=>g.Count()>1).Select(g=>g.Key).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var chart=groups.Select(x=>new ChartEntry(duplicateNames.Contains(x.Name)?$"{x.Name} (#{x.CategoryId})":x.Name,x.Amount)).ToList();
        return new Overview{From=from.Date,To=to.Date,Month=month,Income=data.FirstOrDefault(x=>x.Type==FlowType.Income)?.Total??0,Expense=data.FirstOrDefault(x=>x.Type==FlowType.Expense)?.Total??0,Wallets=await Wallets(user),Chart=chart.OrderByDescending(x=>x.Amount).ToList(),MonthExpense=chart.Sum(x=>x.Amount),Budget=await db.Budgets.Where(x=>x.UserId==user&&x.Month==month).Select(x=>(decimal?)x.Limit).SingleOrDefaultAsync()};
    }
}
