using FinanceShopping.Web.Data;
using FinanceShopping.Web.Services;
using FinanceShopping.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace FinanceShopping.Web.Controllers;
public class BudgetsController(AppDbContext db,FinanceService finance) : AppController
{
    public async Task<IActionResult> Index(DateTime? month)
    {
        if(!ModelState.IsValid || month.HasValue&&(month.Value.Year<1900||month.Value.Year>2100))
        {TempData["Error"]="Tháng không hợp lệ; dùng giai đoạn 1900–2100.";return RedirectToAction(nameof(Index));}
        var m=FinanceService.Month(month??DateTime.Today);
        ViewBag.Remaining=await finance.Remaining(UserId,m);
        return View(new BudgetForm{Month=m,Limit=await db.Budgets.Where(x=>x.UserId==UserId&&x.Month==m).Select(x=>(decimal?)x.Limit).SingleOrDefaultAsync()??0});
    }
    [HttpPost] public async Task<IActionResult> Save(BudgetForm f)
    {
        if(ValidForm()&&await Execute(()=>finance.SaveBudget(UserId,f)))TempData["Success"]="Đã lưu ngân sách tháng.";
        return RedirectToAction(nameof(Index),new{month=f.Month.ToString("yyyy-MM-dd")});
    }
}
