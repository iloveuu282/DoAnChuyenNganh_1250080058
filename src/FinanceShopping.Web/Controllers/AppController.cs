using System.Security.Claims;
using FinanceShopping.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
namespace FinanceShopping.Web.Controllers;
[Authorize]
public abstract class AppController : Controller
{
    protected string UserId=>User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    protected async Task<bool> Execute(Func<Task> action)
    {
        try { await action();return true; }
        catch(BusinessException ex){TempData["Error"]=ex.Message;}
        catch(Exception ex) when(FinanceService.IsSqlDeadlock(ex)){TempData["Error"]="Có thao tác đồng thời trên dữ liệu này. Hãy tải lại trang để xem trạng thái mới trước khi thử lại.";}
        catch(DbUpdateConcurrencyException){TempData["Error"]="Dữ liệu đã thay đổi. Hãy tải lại trang và thử lại.";}
        catch(DbUpdateException){TempData["Error"]="Không thể lưu vì dữ liệu liên kết hoặc yêu cầu trùng. Hãy tải lại trang và kiểm tra.";}
        catch(SqlException){TempData["Error"]="Cơ sở dữ liệu đang bận hoặc mất kết nối. Hãy tải lại trang trước khi thử lại.";}
        return false;
    }
    protected bool ValidForm()
    {
        if(ModelState.IsValid)return true;
        TempData["Error"]="Dữ liệu nhập chưa hợp lệ. Kiểm tra tên, ngày, số tiền và các trường bắt buộc.";return false;
    }
}
