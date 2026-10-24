using BM2.Contracts.Categories;
using BM2.Domain.Categories;

namespace BM2.Domain.Wallets;

/// <summary>Starter categories for a new wallet; users rename or archive them freely.</summary>
public static class DefaultCategories
{
    private static readonly (string Name, string Emoji)[] Expenses =
    [
        ("Groceries", "🛒"), ("Eating out", "🍽️"), ("Transport", "🚗"), ("Home", "🏠"), ("Bills", "🧾"), ("Health", "💊"),
        ("Shopping", "🛍️"), ("Entertainment", "🎬"), ("Other", "📦"),
    ];

    private static readonly (string Name, string Emoji)[] Income = [("Salary", "💼"), ("Other income", "💰")];

    public static IEnumerable<Category> For(Guid walletId) =>
        Expenses.Select(c => Category.Create(walletId, c.Name, CategoryType.Expense, c.Emoji))
            .Concat(Income.Select(c => Category.Create(walletId, c.Name, CategoryType.Income, c.Emoji)));
}
