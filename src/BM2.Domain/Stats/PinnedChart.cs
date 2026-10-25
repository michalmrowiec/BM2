using BM2.Contracts.Stats;

namespace BM2.Domain.Stats;

/// <summary>A stats view one user pinned to Home in a wallet; <see cref="Query"/> is plain text, read by the client.</summary>
public sealed class PinnedChart
{
    private PinnedChart()
    {
        Title = "";
        Query = "";
    }

    public Guid Id { get; private set; }

    public Guid WalletId { get; private set; }

    public Guid UserId { get; private set; }

    public PinnedChartKind Kind { get; private set; }

    public string Title { get; private set; }

    public string Query { get; private set; }

    public int SortOrder { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public static PinnedChart Create(Guid walletId, Guid userId, PinChartRequest request, int sortOrder, DateTimeOffset now) => new()
    {
        WalletId = walletId,
        UserId = userId,
        Kind = request.Kind,
        Title = request.Title.Trim(),
        Query = request.Query,
        SortOrder = sortOrder,
        CreatedAt = now,
    };
}
