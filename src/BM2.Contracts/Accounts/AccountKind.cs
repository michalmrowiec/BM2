namespace BM2.Contracts.Accounts;

/// <summary>Investment accounts are valued by manual valuations and kept out of the cash balance and forecast.</summary>
public enum AccountKind
{
    Cash = 1,
    Investment = 2,
}
