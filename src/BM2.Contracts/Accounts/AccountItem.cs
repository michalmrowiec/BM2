namespace BM2.Contracts.Accounts;

/// <summary>For an investment account Balance is the net amount paid in; Valuation is the latest manual valuation.</summary>
public sealed record AccountItem(
    Guid Id, string Name, string Currency, decimal OpeningBalance, decimal Balance, decimal WithPlanned, bool IsArchived, bool CanWrite,
    AccountKind Kind = AccountKind.Cash, decimal? Valuation = null, DateOnly? ValuedOn = null,
    bool ExcludeFromTotal = false, bool ShowOnDashboard = true);
