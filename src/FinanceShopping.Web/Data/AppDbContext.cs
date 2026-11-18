using FinanceShopping.Web.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace FinanceShopping.Web.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Budget> Budgets => Set<Budget>();
    public DbSet<ShoppingList> ShoppingLists => Set<ShoppingList>();
    public DbSet<ShoppingItem> ShoppingItems => Set<ShoppingItem>();
    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        foreach (var type in new[] {typeof(Wallet),typeof(Category),typeof(Transaction),typeof(Budget),typeof(ShoppingList),typeof(ShoppingItem)})
        {
            b.Entity(type).HasOne(typeof(ApplicationUser)).WithMany().HasForeignKey("UserId").OnDelete(DeleteBehavior.Restrict);
            b.Entity(type).HasIndex("UserId");
        }
        b.Entity<Wallet>().Property(x=>x.OpeningBalance).HasPrecision(18,2);
        b.Entity<Transaction>().Property(x=>x.Amount).HasPrecision(18,2);
        b.Entity<Budget>().Property(x=>x.Limit).HasPrecision(18,2);
        b.Entity<ShoppingItem>().Property(x=>x.Quantity).HasPrecision(12,2);
        b.Entity<ShoppingItem>().Property(x=>x.EstimatedPrice).HasPrecision(18,2);
        b.Entity<Transaction>().HasOne(x=>x.Wallet).WithMany().HasForeignKey(x=>x.WalletId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Transaction>().HasOne(x=>x.Category).WithMany().HasForeignKey(x=>x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Transaction>().HasOne(x=>x.ShoppingList).WithOne(x=>x.Purchase).HasForeignKey<Transaction>(x=>x.ShoppingListId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Transaction>().HasIndex(x=>x.ShoppingListId).IsUnique().HasFilter("[ShoppingListId] IS NOT NULL");
        b.Entity<Transaction>().HasIndex(x=>new {x.UserId,x.Date});
        b.Entity<Budget>().HasIndex(x=>new {x.UserId,x.Month}).IsUnique();
        b.Entity<ShoppingList>().Property(x=>x.Version).IsConcurrencyToken();
        b.Entity<ShoppingItem>().HasOne(x=>x.ShoppingList).WithMany(x=>x.Items).HasForeignKey(x=>x.ShoppingListId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<Transaction>().ToTable(t=> {t.HasCheckConstraint("CK_Transaction_Amount","[Amount] > 0");t.HasCheckConstraint("CK_Transaction_Type","[Type] IN (1,2)");t.HasCheckConstraint("CK_Purchase_Expense","[ShoppingListId] IS NULL OR [Type] = 2");});
        b.Entity<Category>().ToTable(t=>t.HasCheckConstraint("CK_Category_Type","[Type] IN (1,2)"));
        b.Entity<Budget>().ToTable(t=>t.HasCheckConstraint("CK_Budget_Limit","[Limit] > 0"));
        b.Entity<ShoppingItem>().ToTable(t=> {t.HasCheckConstraint("CK_Item_Quantity","[Quantity] > 0");t.HasCheckConstraint("CK_Item_Price","[EstimatedPrice] >= 0");t.HasCheckConstraint("CK_Item_Priority","[Priority] IN (1,2,3)");});
    }
}
