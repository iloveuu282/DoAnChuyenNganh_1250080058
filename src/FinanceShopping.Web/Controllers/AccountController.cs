using FinanceShopping.Web.Models;
using FinanceShopping.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
namespace FinanceShopping.Web.Controllers;
public class AccountController(SignInManager<ApplicationUser> signIn,UserManager<ApplicationUser> users) : Controller
{
    [HttpGet,AllowAnonymous] public IActionResult Login()=>User.Identity?.IsAuthenticated==true?RedirectToAction("Index","Home"):View(new LoginForm());
    [HttpPost,AllowAnonymous] public async Task<IActionResult> Login(LoginForm f)
    {
        if(!ModelState.IsValid)return View(f);
        var result=await signIn.PasswordSignInAsync(f.Email.Trim(),f.Password,false,true);
        if(result.Succeeded)return RedirectToAction("Index","Home");
        ModelState.AddModelError("",result.IsLockedOut?"Tài khoản tạm khóa 5 phút vì đăng nhập sai nhiều lần.":"Email hoặc mật khẩu không đúng.");return View(f);
    }
    [HttpGet,AllowAnonymous] public IActionResult Register()=>View(new RegisterForm());
    [HttpPost,AllowAnonymous] public async Task<IActionResult> Register(RegisterForm f)
    {
        if(!ModelState.IsValid)return View(f);
        var user=new ApplicationUser{Email=f.Email.Trim(),UserName=f.Email.Trim()};
        var result=await users.CreateAsync(user,f.Password);
        if(result.Succeeded){await signIn.SignInAsync(user,false);return RedirectToAction("Index","Home");}
        foreach(var e in result.Errors)ModelState.AddModelError("",e.Code.Contains("Duplicate")?"Email đã được đăng ký.":"Mật khẩu cần ít nhất 8 ký tự, gồm chữ hoa, chữ thường, số và ký tự đặc biệt.");
        return View(f);
    }
    [HttpPost,Authorize] public async Task<IActionResult> Logout(){await signIn.SignOutAsync();return RedirectToAction(nameof(Login));}
}
