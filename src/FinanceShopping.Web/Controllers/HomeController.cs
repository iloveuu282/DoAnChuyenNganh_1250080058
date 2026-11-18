using FinanceShopping.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace FinanceShopping.Web.Controllers;
public class HomeController(FinanceService finance) : AppController
{
    public async Task<IActionResult> Index(DateTime? from,DateTime? to,DateTime? month)
    {
        if(!ModelState.IsValid || new[]{from,to,month}.Any(x=>x.HasValue&&(x.Value.Year<1900||x.Value.Year>2100)))
        {TempData["Error"]="Ngày hoặc tháng không hợp lệ; dùng giai đoạn 1900–2100.";return RedirectToAction(nameof(Index));}
        var m=FinanceService.Month(month??DateTime.Today);
        try{return View(await finance.Overview(UserId,from??m,to??m.AddMonths(1).AddDays(-1),m));}
        catch(BusinessException ex){TempData["Error"]=ex.Message;return RedirectToAction(nameof(Index));}
    }
    [AllowAnonymous] public IActionResult Error()=>View();
}
