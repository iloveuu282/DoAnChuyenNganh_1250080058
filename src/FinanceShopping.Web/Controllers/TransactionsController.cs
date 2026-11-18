using FinanceShopping.Web.Data;
using FinanceShopping.Web.Models;
using FinanceShopping.Web.Services;
using FinanceShopping.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace FinanceShopping.Web.Controllers;
public class TransactionsController(AppDbContext db,FinanceService finance) : AppController
{
    async Task Choices()
    {
        ViewBag.Wallets=await db.Wallets.Where(x=>x.UserId==UserId).OrderBy(x=>x.Name).ToListAsync();
        ViewBag.Categories=await db.Categories.Where(x=>x.UserId==UserId).OrderBy(x=>x.Name).ToListAsync();
    }
    public async Task<IActionResult> Index(DateTime? from,DateTime? to,FlowType? type,int? category,int page=1)
    {
        if(!ModelState.IsValid || new[]{from,to}.Any(x=>x.HasValue&&(x.Value.Year<1900||x.Value.Year>2100)) || from.HasValue&&to.HasValue&&from>to || type.HasValue&&!Enum.IsDefined(type.Value))
        {TempData["Error"]="Bộ lọc không hợp lệ. Kiểm tra ngày và thứ tự khoảng thời gian.";return RedirectToAction(nameof(Index));}
        var q=db.Transactions.AsNoTracking().Include(x=>x.Wallet).Include(x=>x.Category).Where(x=>x.UserId==UserId);
        if(from.HasValue)q=q.Where(x=>x.Date>=from.Value.Date);
        if(to.HasValue){var end=to.Value.Date.AddDays(1);q=q.Where(x=>x.Date<end);}
        if(type.HasValue)q=q.Where(x=>x.Type==type.Value);
        if(category.HasValue)q=q.Where(x=>x.CategoryId==category);
        var count=await q.CountAsync();page=Math.Clamp(page,1,Math.Max(1,(count+19)/20));
        ViewBag.Page=page;ViewBag.Pages=Math.Max(1,(count+19)/20);ViewBag.Count=count;
        ViewBag.From=from;ViewBag.To=to;ViewBag.Type=type;ViewBag.Category=category;
        await Choices();return View(await q.OrderByDescending(x=>x.Date).ThenByDescending(x=>x.Id).Skip((page-1)*20).Take(20).ToListAsync());
    }
    public async Task<IActionResult> Edit(int id=0)
    {
        await Choices();if(id==0)return View(new TransactionForm());
        var t=await db.Transactions.SingleOrDefaultAsync(x=>x.Id==id&&x.UserId==UserId);
        if(t==null)return NotFound();
        if(t.ShoppingListId!=null)return RedirectToAction("Details","Shopping",new{id=t.ShoppingListId});
        return View(new TransactionForm{Id=t.Id,WalletId=t.WalletId,CategoryId=t.CategoryId,Amount=t.Amount,Type=t.Type,Date=t.Date,Note=t.Note});
    }
    [HttpPost] public async Task<IActionResult> Edit(TransactionForm f)
    {
        if(ModelState.IsValid&&await Execute(()=>finance.SaveTransaction(UserId,f)))
        {
            TempData["Success"]="Đã lưu giao dịch.";
            if(f.Type==FlowType.Expense)TempData["Warning"]=await finance.Warnings(UserId,f.WalletId,f.Date);
            return RedirectToAction(nameof(Index));
        }
        await Choices();return View(f);
    }
    [HttpPost] public async Task<IActionResult> Delete(int id){await Execute(()=>finance.DeleteTransaction(UserId,id));return RedirectToAction(nameof(Index));}
}
