namespace BM2.Contracts.Records;

/// <remarks>
/// Account names are included because a user with one shared account cannot see the account on the other side of a
/// transfer in the wallet snapshot.
/// </remarks>
public sealed record RecordItem(
    Guid Id,
    RecordType Type,
    RecordStatus Status,
    DateOnly OccurredOn,
    Guid AccountId,
    string AccountName,
    decimal Amount,
    string Currency,
    decimal AccountAmount,
    Guid? CategoryId,
    Guid? TransferAccountId,
    string? TransferAccountName,
    decimal? TransferAmount,
    string? Name,
    IReadOnlyList<Guid> TagIds,
    Guid? RecurringRuleId = null);
