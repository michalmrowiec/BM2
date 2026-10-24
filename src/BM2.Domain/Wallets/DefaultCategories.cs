using BM2.Contracts.Categories;
using BM2.Domain.Categories;

namespace BM2.Domain.Wallets;

/// <summary>Starter categories for a new wallet; users rename or archive them freely.</summary>
public static class DefaultCategories
{
    private static readonly string[] Expenses =
        ["Groceries", "Eating out", "Transport", "Home", "Bills", "Health", "Shopping", "Entertainment", "Other"];

    private static readonly string[] Income = ["Salary", "Other income"];

    public static IEnumerable<Category> For(Guid walletId) =>
        Expenses.Select(name => Category.Create(walletId, name, CategoryType.Expense))
            .Concat(Income.Select(name => Category.Create(walletId, name, CategoryType.Income)));
}
