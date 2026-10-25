namespace BM2.Domain.Budgets;

/// <summary>Monthly spending limit of an expense category, in the wallet's base currency; applies every month.</summary>
public sealed class Budget
{
    private Budget()
    {
    }

    public Guid Id { get; private set; }

    public Guid WalletId { get; private set; }

    public Guid CategoryId { get; private set; }

    public decimal Limit { get; private set; }

    public static Budget Create(Guid walletId, Guid categoryId, decimal limit) =>
        new() { WalletId = walletId, CategoryId = categoryId, Limit = limit };

    public void SetLimit(decimal limit) => Limit = limit;
}
