# Ma trận truy vết yêu cầu

| Yêu cầu | Use Case / Business Rules | Cài đặt | Dữ liệu | Kiểm thử |
|---|---|---|---|---|
| FR01 | UC01 / BR10 | Identity; AccountController; mọi truy vấn theo UserId | AspNetUsers; UserId trên nghiệp vụ | TC04, TC19, TC26, TC29; SQL-HTTP-1 |
| FR02 | UC02 / BR01,08 | SaveWallet; DeleteWallet; Wallets | Wallets; Transactions | TC01–02, TC06, TC26 |
| FR03 | UC03 / BR08 | SaveCategory; DeleteCategory | Categories | TC05, TC07, TC29 |
| FR04 | UC04 / BR01,04 | SaveTransaction; DeleteTransaction; bộ lọc | Transactions | TC01–07, TC15, TC25 |
| FR05 | UC05 / BR02,03,09 | Remaining; SaveBudget; Warnings | Budgets; Transactions; Wallets | TC08–10, TC20–21, TC28, TC30, TC32, TC34 |
| FR06 | UC06 / BR04,07 | SaveList; SaveItem; DeleteItem | ShoppingLists; ShoppingItems | TC14, TC22–24, TC27 |
| FR07 | UC07 / BR05 | ItemEstimate; Estimate; PlannedTotal; Details | ShoppingLists; ShoppingItems; Budgets; Transactions | TC10–11, TC30, TC33 |
| FR08 | UC08 / BR06,07,10 | Confirm; Cancel; Version; transaction; nhận diện deadlock | ShoppingLists; Transactions | TC12–21, TC24, TC27–28; SQL-CONCURRENT-1..3; SQL-HTTP-1..2 |
| FR09 | UC09 / BR01,02 | Overview nhóm CategoryId và tên; biểu đồ thanh CSS | Transactions; Categories; Wallets; Budgets | TC25, TC29, TC31 |
