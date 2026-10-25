namespace BM2.Contracts.Records;

/// <summary>The category and tags of the latest record with this name and type.</summary>
public sealed record NameSuggestion(string Name, RecordType Type, Guid? CategoryId, IReadOnlyList<Guid> TagIds);

/// <summary>Recent record names plus category and tag ids, most used first; the client filters them locally.</summary>
public sealed record RecordSuggestions(
    IReadOnlyList<NameSuggestion> Names, IReadOnlyList<Guid> CategoryIds, IReadOnlyList<Guid> TagIds);
