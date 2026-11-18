-- Run in SSMS after selecting the FinanceShopping database. Read-only diagnostics.
SELECT l.Id, l.UserId, l.IsPurchased, COUNT(t.Id) AS LinkedTransactions
FROM ShoppingLists l LEFT JOIN Transactions t ON t.ShoppingListId=l.Id
GROUP BY l.Id,l.UserId,l.IsPurchased
HAVING (l.IsPurchased=1 AND COUNT(t.Id)<>1) OR (l.IsPurchased=0 AND COUNT(t.Id)<>0);
-- Expected: no rows.
SELECT UserId,[Month],COUNT(*) AS Duplicates FROM Budgets GROUP BY UserId,[Month] HAVING COUNT(*)>1;
-- Expected: no rows.
SELECT t.Id FROM Transactions t JOIN Wallets w ON w.Id=t.WalletId JOIN Categories c ON c.Id=t.CategoryId
WHERE t.UserId<>w.UserId OR t.UserId<>c.UserId OR t.Type<>c.Type;
-- Expected: no rows.
SELECT i.Id FROM ShoppingItems i JOIN ShoppingLists l ON l.Id=i.ShoppingListId WHERE i.UserId<>l.UserId;
-- Expected: no rows.
SELECT w.Id,w.Name,w.OpeningBalance+COALESCE(SUM(CASE WHEN t.Type=1 THEN t.Amount ELSE -t.Amount END),0) AS CurrentBalance
FROM Wallets w LEFT JOIN Transactions t ON t.WalletId=w.Id GROUP BY w.Id,w.Name,w.OpeningBalance;
