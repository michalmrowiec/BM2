using BM2.Contracts.Categories;
using BM2.Domain.Categories;

namespace BM2.Domain.Wallets;

/// <summary>Starter categories for a new wallet; users rename or archive them freely.</summary>
public static class DefaultCategories
{
    private static readonly (string Name, string Icon)[] Expenses =
    [
        ("Groceries", "basket"), ("Eating out", "food"), ("Transport", "car"), ("Home", "home"), ("Bills", "receipt"),
        ("Health", "pill"), ("Shopping", "cart"), ("Entertainment", "film"), ("Other", "dots"),
    ];

    private static readonly (string Name, string Icon)[] Income = [("Salary", "briefcase"), ("Other income", "piggy")];

    public static IEnumerable<Category> For(Guid walletId) =>
        Expenses.Select(c => Category.Create(walletId, c.Name, CategoryType.Expense, c.Icon))
            .Concat(Income.Select(c => Category.Create(walletId, c.Name, CategoryType.Income, c.Icon)));
}
