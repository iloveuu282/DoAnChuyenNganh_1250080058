using FinanceShopping.Web.Data;
using FinanceShopping.Web.Services;
using FinanceShopping.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace FinanceShopping.Web.Controllers;
public class WalletsController(AppDbContext db,FinanceService finance) : AppController
{
    public async Task<IActionResult> Index()=>View(await finance.Wallets(UserId));
    public async Task<IActionResult> Edit(int id=0)
    {
        if(id==0)return View(new WalletForm());
        var w=await db.Wallets.SingleOrDefaultAsync(x=>x.Id==id&&x.UserId==UserId);
        return w==null?NotFound():View(new WalletForm{Id=w.Id,Name=w.Name,OpeningBalance=w.OpeningBalance});
    }
    [HttpPost] public async Task<IActionResult> Edit(WalletForm f)
    {
        if(!ModelState.IsValid)return View(f);
        if(await Execute(()=>finance.SaveWallet(UserId,f))){TempData["Success"]="Đã lưu ví.";return RedirectToAction(nameof(Index));}
        return View(f);
    }
    [HttpPost] public async Task<IActionResult> Delete(int id){await Execute(()=>finance.DeleteWallet(UserId,id));return RedirectToAction(nameof(Index));}
}
