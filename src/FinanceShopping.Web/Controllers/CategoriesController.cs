using FinanceShopping.Web.Data;
using FinanceShopping.Web.Services;
using FinanceShopping.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace FinanceShopping.Web.Controllers;
public class CategoriesController(AppDbContext db,FinanceService finance) : AppController
{
    public async Task<IActionResult> Index()=>View(await db.Categories.Where(x=>x.UserId==UserId).OrderBy(x=>x.Type).ThenBy(x=>x.Name).ToListAsync());
    public async Task<IActionResult> Edit(int id=0)
    {
        if(id==0)return View(new CategoryForm());
        var c=await db.Categories.SingleOrDefaultAsync(x=>x.Id==id&&x.UserId==UserId);
        return c==null?NotFound():View(new CategoryForm{Id=c.Id,Name=c.Name,Type=c.Type});
    }
    [HttpPost] public async Task<IActionResult> Edit(CategoryForm f)
    {
        if(!ModelState.IsValid)return View(f);
        if(await Execute(()=>finance.SaveCategory(UserId,f))){TempData["Success"]="Đã lưu danh mục.";return RedirectToAction(nameof(Index));}
        return View(f);
    }
    [HttpPost] public async Task<IActionResult> Delete(int id){await Execute(()=>finance.DeleteCategory(UserId,id));return RedirectToAction(nameof(Index));}
}
