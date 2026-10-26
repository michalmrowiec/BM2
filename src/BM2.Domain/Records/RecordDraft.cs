using BM2.Contracts.Categories;
using BM2.Contracts.Records;

namespace BM2.Domain.Records;

/// <summary>What the user wants the record to be; checked against <see cref="RecordContext"/>.</summary>
public sealed record RecordDraft(
    RecordType Type,
    RecordStatus Status,
    DateOnly OccurredOn,
    Guid AccountId,
    decimal Amount,
    string Currency,
    decimal AccountAmount,
    Guid? CategoryId,
    Guid? TransferAccountId,
    decimal? TransferAmount,
    string? Name,
    IReadOnlyList<Guid> TagIds,
    RecordPair? Pair = null);

public sealed record AccountRef(Guid Id, Guid WalletId, string Currency, bool IsArchived);

public sealed record CategoryRef(Guid Id, Guid WalletId, CategoryType Type, bool IsArchived);

public sealed record TagRef(Guid Id, Guid WalletId);

/// <summary>The entities a draft points to, looked up by id; null means not found.</summary>
public sealed record RecordContext(
    Guid WalletId,
    AccountRef? Account,
    AccountRef? TransferAccount,
    CategoryRef? Category,
    IReadOnlyList<TagRef> Tags);
