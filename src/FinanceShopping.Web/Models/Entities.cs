using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
namespace FinanceShopping.Web.Models;

public class ApplicationUser : IdentityUser { }
public enum FlowType { Income = 1, Expense = 2 }
public enum Priority { Low = 1, Normal = 2, High = 3 }
public abstract class OwnedEntity
{
    public int Id { get; set; }
    [MaxLength(450)] public string UserId { get; set; } = "";
}
public class Wallet : OwnedEntity
{
    [MaxLength(100)] public string Name { get; set; } = "";
    public decimal OpeningBalance { get; set; }
}
public class Category : OwnedEntity
{
    [MaxLength(100)] public string Name { get; set; } = "";
    public FlowType Type { get; set; }
}
public class Transaction : OwnedEntity
{
    public int WalletId { get; set; }
    public Wallet Wallet { get; set; } = null!;
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public FlowType Type { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    [MaxLength(300)] public string Note { get; set; } = "";
    public int? ShoppingListId { get; set; }
    public ShoppingList? ShoppingList { get; set; }
}
public class Budget : OwnedEntity
{
    public DateTime Month { get; set; }
    public decimal Limit { get; set; }
}
public class ShoppingList : OwnedEntity
{
    [MaxLength(150)] public string Name { get; set; } = "";
    public DateTime PlannedMonth { get; set; }
    public bool IsPurchased { get; set; }
    // New token on every list/item mutation; protects concurrent purchase and edits.
    public Guid Version { get; set; } = Guid.NewGuid();
    public List<ShoppingItem> Items { get; set; } = [];
    public Transaction? Purchase { get; set; }
}
public class ShoppingItem : OwnedEntity
{
    public int ShoppingListId { get; set; }
    public ShoppingList ShoppingList { get; set; } = null!;
    [MaxLength(150)] public string Name { get; set; } = "";
    public decimal Quantity { get; set; }
    public decimal EstimatedPrice { get; set; }
    public Priority Priority { get; set; } = Priority.Normal;
}
