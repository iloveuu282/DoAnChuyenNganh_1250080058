using System.ComponentModel.DataAnnotations;
using FinanceShopping.Web.Models;
namespace FinanceShopping.Web.ViewModels;
public class LoginForm
{
    [Required(ErrorMessage="Vui lòng nhập email."),EmailAddress] public string Email { get; set; } = "";
    [Required(ErrorMessage="Vui lòng nhập mật khẩu."),DataType(DataType.Password)] public string Password { get; set; } = "";
}
public class RegisterForm : LoginForm
{
    [Compare(nameof(Password),ErrorMessage="Mật khẩu nhập lại không khớp."),DataType(DataType.Password)] public string ConfirmPassword { get; set; } = "";
}
public class WalletForm
{
    public int Id {get;set;}
    [Required,MaxLength(100)] public string Name {get;set;} = "";
    [Range(typeof(decimal),"-999999999999","999999999999")] public decimal OpeningBalance {get;set;}
}
public class CategoryForm
{
    public int Id {get;set;}
    [Required,MaxLength(100)] public string Name {get;set;} = "";
    [EnumDataType(typeof(FlowType))] public FlowType Type {get;set;} = FlowType.Expense;
}
public class TransactionForm
{
    public int Id {get;set;}
    [Range(1,int.MaxValue)] public int WalletId {get;set;}
    [Range(1,int.MaxValue)] public int CategoryId {get;set;}
    [EnumDataType(typeof(FlowType))] public FlowType Type {get;set;} = FlowType.Expense;
    [Range(typeof(decimal),"0.01","999999999999")] public decimal Amount {get;set;}
    [DataType(DataType.Date)] public DateTime Date {get;set;} = DateTime.Today;
    [MaxLength(300, ErrorMessage="Ghi chú tối đa 300 ký tự.")] public string? Note {get;set;} = "";
}
public class BudgetForm
{
    public DateTime Month {get;set;} = new(DateTime.Today.Year,DateTime.Today.Month,1);
    [Range(typeof(decimal),"0.01","999999999999")] public decimal Limit {get;set;}
}
public class ListForm
{
    public int Id {get;set;}
    public Guid Version {get;set;}
    [Required,MaxLength(150)] public string Name {get;set;} = "";
    public DateTime PlannedMonth {get;set;} = new(DateTime.Today.Year,DateTime.Today.Month,1);
}
public class ItemForm
{
    public int Id {get;set;}
    public int ShoppingListId {get;set;}
    public Guid Version {get;set;}
    [Required,MaxLength(150)] public string Name {get;set;} = "";
    [Range(typeof(decimal),"0.01","999999")] public decimal Quantity {get;set;} = 1;
    [Range(typeof(decimal),"0","999999999999")] public decimal EstimatedPrice {get;set;}
    [EnumDataType(typeof(Priority))] public Priority Priority {get;set;} = Priority.Normal;
}
public class PurchaseForm
{
    public int Id {get;set;}
    public Guid Version {get;set;}
    [Range(typeof(decimal),"0.01","999999999999")] public decimal Amount {get;set;}
    [DataType(DataType.Date)] public DateTime Date {get;set;} = DateTime.Today;
    [Range(1,int.MaxValue)] public int WalletId {get;set;}
    [Range(1,int.MaxValue)] public int CategoryId {get;set;}
}
public record WalletSummary(Wallet Wallet, decimal Balance);
public record ChartEntry(string Name,decimal Amount);
public class Overview
{
    public DateTime From {get;set;}
    public DateTime To {get;set;}
    public DateTime Month {get;set;}
    public decimal Income {get;set;}
    public decimal Expense {get;set;}
    public decimal? Budget {get;set;}
    public decimal MonthExpense {get;set;}
    public List<WalletSummary> Wallets {get;set;} = [];
    public List<ChartEntry> Chart {get;set;} = [];
}
public class ShoppingDetails
{
    public ShoppingList List {get;set;} = null!;
    public decimal Estimate {get;set;}
    public decimal PlannedTotal {get;set;}
    public decimal? Remaining {get;set;}
    public List<Wallet> Wallets {get;set;} = [];
    public List<Category> Categories {get;set;} = [];
}
