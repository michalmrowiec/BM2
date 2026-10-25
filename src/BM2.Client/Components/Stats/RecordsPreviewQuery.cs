using BM2.Contracts.Records;

namespace BM2.Client.Components.Stats;

/// <summary>Which records <see cref="RecordsPreview"/> lists; <see cref="To"/> is exclusive.</summary>
public sealed record RecordsPreviewQuery(
    string Title,
    DateOnly From,
    DateOnly To,
    RecordType Type,
    string Currency,
    Guid? AccountId = null,
    Guid? CategoryId = null,
    IReadOnlyCollection<Guid>? TagIds = null);
