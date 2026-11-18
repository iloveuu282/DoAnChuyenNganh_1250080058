USE [FinanceShopping];
SET NOCOUNT ON;

DECLARE @Email nvarchar(256) = N'demo@example.com';
DECLARE @ListId int = 1; -- THAY 0 BẰNG MÃ DANH SÁCH TRÊN URL
DECLARE @Month date = '20261101';

DECLARE @UserId nvarchar(450) = (
    SELECT Id
    FROM dbo.AspNetUsers
    WHERE NormalizedEmail = UPPER(@Email)
);

IF NOT EXISTS (
    SELECT 1 FROM dbo.ShoppingLists
    WHERE Id = @ListId AND UserId = @UserId
)
    THROW 50001, N'Kiểm tra lại email và mã danh sách.', 1;

-- KẾT QUẢ 1: trạng thái và số khoản chi liên kết
SELECT
    l.Id AS ListId,
    l.Name,
    l.IsPurchased,
    COUNT(t.Id) AS LinkedTransactionCount
FROM dbo.ShoppingLists l
LEFT JOIN dbo.Transactions t ON t.ShoppingListId = l.Id
WHERE l.Id = @ListId AND l.UserId = @UserId
GROUP BY l.Id, l.Name, l.IsPurchased;

-- KẾT QUẢ 2: chi tiết khoản chi liên kết
SELECT
    t.Id AS TransactionId,
    t.ShoppingListId,
    t.Amount,
    t.[Date],
    w.Name AS Wallet,
    c.Name AS Category
FROM dbo.Transactions t
JOIN dbo.Wallets w ON w.Id = t.WalletId
JOIN dbo.Categories c ON c.Id = t.CategoryId
WHERE t.ShoppingListId = @ListId AND t.UserId = @UserId;

-- KẾT QUẢ 3: số dư từng ví tính từ giao dịch
SELECT
    w.Id,
    w.Name,
    w.OpeningBalance,
    w.OpeningBalance
      + COALESCE(SUM(
          CASE WHEN t.Type = 1 THEN t.Amount ELSE -t.Amount END
        ), 0) AS CurrentBalance
FROM dbo.Wallets w
LEFT JOIN dbo.Transactions t
    ON t.WalletId = w.Id AND t.UserId = w.UserId
WHERE w.UserId = @UserId
GROUP BY w.Id, w.Name, w.OpeningBalance;

-- KẾT QUẢ 4: ngân sách tháng và tổng chi thực tế
SELECT
    @Month AS BudgetMonth,
    b.[Limit] AS BudgetLimit,
    e.TotalExpense,
    b.[Limit] - e.TotalExpense AS RemainingBudget
FROM (
    SELECT COALESCE(SUM(Amount), 0) AS TotalExpense
    FROM dbo.Transactions
    WHERE UserId = @UserId
      AND Type = 2
      AND [Date] >= @Month
      AND [Date] < DATEADD(month, 1, @Month)
) e
LEFT JOIN dbo.Budgets b
    ON b.UserId = @UserId AND b.[Month] = @Month;