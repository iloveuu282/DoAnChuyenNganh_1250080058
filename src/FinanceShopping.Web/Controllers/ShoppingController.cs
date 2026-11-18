using FinanceShopping.Web.Data;
using FinanceShopping.Web.Models;
using FinanceShopping.Web.Services;
using FinanceShopping.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace FinanceShopping.Web.Controllers;
public class ShoppingController(AppDbContext db,FinanceService finance) : AppController
{
    public async Task<IActionResult> Index()=>View(await db.ShoppingLists.AsNoTracking().Include(x=>x.Items).Where(x=>x.UserId==UserId).OrderByDescending(x=>x.PlannedMonth).ThenByDescending(x=>x.Id).ToListAsync());
    public async Task<IActionResult> Edit(int id=0)
    {
        if(id==0)return View(new ListForm());
        var l=await db.ShoppingLists.SingleOrDefaultAsync(x=>x.UserId==UserId&&x.Id==id);
        if(l==null)return NotFound();
        if(l.IsPurchased)return RedirectToAction(nameof(Details),new{id});
        return View(new ListForm{Id=l.Id,Version=l.Version,Name=l.Name,PlannedMonth=l.PlannedMonth});
    }
    [HttpPost] public async Task<IActionResult> Edit(ListForm f)
    {
        int id=f.Id;
        if(ModelState.IsValid&&await Execute(async()=>id=await finance.SaveList(UserId,f)))return RedirectToAction(nameof(Details),new{id});
        return View(f);
    }
    public async Task<IActionResult> Details(int id)
    {
        var l=await db.ShoppingLists.Include(x=>x.Items).Include(x=>x.Purchase).SingleOrDefaultAsync(x=>x.Id==id&&x.UserId==UserId);
        if(l==null)return NotFound();
        return View(new ShoppingDetails{List=l,Estimate=FinanceService.Estimate(l.Items),PlannedTotal=await finance.PlannedTotal(UserId,l.PlannedMonth),Remaining=await finance.Remaining(UserId,l.PlannedMonth),Wallets=await db.Wallets.Where(x=>x.UserId==UserId).ToListAsync(),Categories=await db.Categories.Where(x=>x.UserId==UserId&&x.Type==FlowType.Expense).ToListAsync()});
    }
    [HttpPost] public async Task<IActionResult> SaveItem(ItemForm f)
    {
        if(ValidForm())await Execute(()=>finance.SaveItem(UserId,f));return RedirectToAction(nameof(Details),new{id=f.ShoppingListId});
    }
    [HttpPost] public async Task<IActionResult> DeleteItem(int id,int itemId,Guid version){await Execute(()=>finance.DeleteItem(UserId,id,itemId,version));return RedirectToAction(nameof(Details),new{id});}
    [HttpPost] public async Task<IActionResult> Delete(int id,Guid version){await Execute(()=>finance.DeleteList(UserId,id,version));return RedirectToAction(nameof(Index));}
    [HttpPost] public async Task<IActionResult> Confirm(PurchaseForm f)
    {
        if(ValidForm())await Execute(async()=>{
            var changed=await finance.Confirm(UserId,f);
            TempData["Success"]=changed?"Đã xác nhận mua và tạo một giao dịch chi tiêu.":"Danh sách đã được xác nhận trước đó, không tạo khoản chi mới.";
            if(changed)TempData["Warning"]=await finance.Warnings(UserId,f.WalletId,f.Date);
        });
        return RedirectToAction(nameof(Details),new{id=f.Id});
    }
    [HttpPost] public async Task<IActionResult> Cancel(int id,Guid version)
    {
        if(await Execute(async()=>{await finance.Cancel(UserId,id,version);}))TempData["Success"]="Đã hủy xác nhận; khoản chi liên kết được xóa.";
        return RedirectToAction(nameof(Details),new{id});
    }
}
