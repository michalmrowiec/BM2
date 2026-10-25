namespace BM2.Contracts.Accounts;

public sealed record AccountItem(
    Guid Id, string Name, string Currency, decimal OpeningBalance, decimal Balance, decimal WithPlanned, bool IsArchived, bool CanWrite);
