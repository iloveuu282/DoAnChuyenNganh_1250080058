/* MỞ TRONG SSMS, KẾT NỐI .\SQLEXPRESS.
   Chỉ SELECT và khai báo biến; không sửa/xóa dữ liệu.
   Mỗi lần thao tác trên giao diện xong, chạy lại toàn bộ bằng F5.
   Sửa email/tháng/ID danh sách bên dưới nếu cần.
   Khoản chi = Type 2; khoản thu = Type 1.
*/
USE [FinanceShopping];
SET NOCOUNT ON;

DECLARE @Email nvarchar(256) = N'demo@example.com';
DECLARE @Month date = '20261101';
DECLARE @ListId int = NULL; -- NULL: tự chọn danh sách có ID lớn nhất của tài khoản.
DECLARE @UserId nvarchar(450) = (
    SELECT Id FROM dbo.AspNetUsers WHERE NormalizedEmail = UPPER(@Email)
);
IF @UserId IS NULL
    THROW 50001, N'Không tìm thấy email. Sửa @Email cho đúng tài khoản đang demo.', 1;
SET @Month = DATEFROMPARTS(YEAR(@Month), MONTH(@Month), 1);
IF @ListId IS NULL
    SELECT @ListId = MAX(Id) FROM dbo.ShoppingLists WHERE UserId = @UserId;

-- 1. Tài khoản đang đối chiếu: không đọc PasswordHash hoặc token.
SELECT Id AS UserId, Email FROM dbo.AspNetUsers WHERE Id = @UserId;

-- 2. Số dư được TÍNH từ giao dịch. Không có cột CurrentBalance để UPDATE.
SELECT w.Id, w.Name, w.OpeningBalance,
       COALESCE(SUM(CASE WHEN t.Type = 1 THEN t.Amount ELSE 0 END), 0) AS TotalIncome,
       COALESCE(SUM(CASE WHEN t.Type = 2 THEN t.Amount ELSE 0 END), 0) AS TotalExpense,
       w.OpeningBalance + COALESCE(SUM(CASE WHEN t.Type = 1 THEN t.Amount ELSE -t.Amount END), 0) AS CurrentBalance
FROM dbo.Wallets w
LEFT JOIN dbo.Transactions t ON t.WalletId = w.Id AND t.UserId = w.UserId
WHERE w.UserId = @UserId
GROUP BY w.Id, w.Name, w.OpeningBalance;

-- 3. Ngân sách tháng tính trên TẤT CẢ ví. Ngày mua thực tế quyết định tháng chi.
-- Chưa có ngân sách: BudgetLimit và RemainingBudget là NULL, không giả thành 0.
SELECT @Month AS BudgetMonth, b.[Limit] AS BudgetLimit, e.TotalExpense,
       b.[Limit] - e.TotalExpense AS RemainingBudget
FROM (SELECT COALESCE(SUM(Amount), 0) AS TotalExpense
      FROM dbo.Transactions
      WHERE UserId = @UserId AND Type = 2
        AND [Date] >= @Month AND [Date] < DATEADD(month, 1, @Month)) e
LEFT JOIN dbo.Budgets b ON b.UserId = @UserId AND b.[Month] = @Month;

-- 4. Trạng thái danh sách và giao dịch liên kết: trước mua 0/NULL, sau mua 1/ID.
SELECT l.Id AS ShoppingListId, l.Name, l.PlannedMonth, l.IsPurchased, l.Version,
       t.Id AS LinkedTransactionId, t.Amount AS ActualAmount, t.[Date] AS PurchaseDate,
       w.Name AS PaymentWallet, c.Name AS ExpenseCategory
FROM dbo.ShoppingLists l
LEFT JOIN dbo.Transactions t ON t.ShoppingListId = l.Id
LEFT JOIN dbo.Wallets w ON w.Id = t.WalletId
LEFT JOIN dbo.Categories c ON c.Id = t.CategoryId
WHERE l.UserId = @UserId AND l.Id = @ListId;

-- 5. Mặt hàng và tổng dự kiến chỉ phục vụ kế hoạch, không ghi vào Transactions.
SELECT Id, Name, Quantity, EstimatedPrice, Priority,
       ROUND(Quantity * EstimatedPrice, 2) AS EstimatedLineTotal
FROM dbo.ShoppingItems
WHERE UserId = @UserId AND ShoppingListId = @ListId
ORDER BY Priority DESC, Id;
SELECT ROUND(COALESCE(SUM(Quantity * EstimatedPrice), 0), 2) AS EstimatedListTotal
FROM dbo.ShoppingItems WHERE UserId = @UserId AND ShoppingListId = @ListId;

-- 6. Đúng một khoản chi cho danh sách đã mua; hủy mua xong phải là 0.
SELECT @ListId AS ShoppingListId, COUNT(*) AS LinkedTransactionCount
FROM dbo.Transactions WHERE ShoppingListId = @ListId;

-- 7. Giao dịch gần nhất: so sánh ID, Amount, WalletId, CategoryId, ShoppingListId.
SELECT TOP (15) t.Id, t.[Date], t.Type, t.Amount, w.Name AS Wallet,
       c.Name AS Category, t.Note, t.ShoppingListId
FROM dbo.Transactions t
JOIN dbo.Wallets w ON w.Id = t.WalletId
JOIN dbo.Categories c ON c.Id = t.CategoryId
WHERE t.UserId = @UserId ORDER BY t.Id DESC;

-- 8. Kiểm tra nhất quán. Kết quả mong đợi: KHÔNG CÓ DÒNG.
SELECT l.Id, l.IsPurchased, COUNT(t.Id) AS LinkedCount
FROM dbo.ShoppingLists l
LEFT JOIN dbo.Transactions t ON t.ShoppingListId = l.Id
WHERE l.UserId = @UserId
GROUP BY l.Id, l.IsPurchased
HAVING (l.IsPurchased = 1 AND COUNT(t.Id) <> 1)
    OR (l.IsPurchased = 0 AND COUNT(t.Id) <> 0);
SELECT t.Id AS InvalidTransactionId
FROM dbo.Transactions t
JOIN dbo.Wallets w ON w.Id = t.WalletId
JOIN dbo.Categories c ON c.Id = t.CategoryId
LEFT JOIN dbo.ShoppingLists l ON l.Id = t.ShoppingListId
WHERE t.UserId = @UserId AND
      (t.UserId <> w.UserId OR t.UserId <> c.UserId OR t.Type <> c.Type
       OR (t.ShoppingListId IS NOT NULL AND t.UserId <> l.UserId));

-- 9. Chỉ mục duy nhất và khóa ngoại là các cơ chế DB thật có thể giải thích.
SELECT OBJECT_NAME(i.object_id) AS TableName, i.name AS IndexName,
       i.is_unique, i.filter_definition
FROM sys.indexes i
WHERE i.object_id IN (OBJECT_ID('dbo.Budgets'), OBJECT_ID('dbo.Transactions'))
      AND i.is_unique = 1;
SELECT name AS ForeignKeyName, OBJECT_NAME(parent_object_id) AS ChildTable,
       OBJECT_NAME(referenced_object_id) AS ParentTable, delete_referential_action_desc
FROM sys.foreign_keys
WHERE parent_object_id IN (OBJECT_ID('dbo.Transactions'), OBJECT_ID('dbo.ShoppingItems'));
