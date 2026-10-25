using BM2.Contracts.Stats;
using BM2.Domain.Common;
using BM2.Domain.Stats;
using BM2.Server.Data;
using BM2.Server.Features.Wallets;
using BM2.Server.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace BM2.Server.Features.PinnedCharts;

/// <summary>Charts pinned to Home; each member has their own, so reading the wallet is enough.</summary>
public static class PinnedChartEndpoints
{
    private const int MaxPins = 12;

    public static RouteGroupBuilder MapPinnedChartEndpoints(this RouteGroupBuilder wallet)
    {
        var pins = wallet.MapGroup("/pinned-charts");
        pins.MapPost("/", CreateAsync).Validate<PinChartRequest>();
        pins.MapDelete("/{pinId:guid}", DeleteAsync);
        return wallet;
    }

    internal static async Task<List<PinnedChartItem>> LoadAsync(CurrentWallet current, ICurrentUser user, AppDbContext db, CancellationToken ct)
    {
        var walletId = current.Id;
        var userId = user.UserId;
        return await db.PinnedCharts
            .AsNoTracking()
            .Where(p => p.WalletId == walletId && p.UserId == userId)
            .OrderBy(p => p.SortOrder)
            .Select(p => new PinnedChartItem(p.Id, p.Kind, p.Title, p.Query))
            .ToListAsync(ct);
    }

    private static async Task<Results<Created<PinnedChartItem>, ProblemHttpResult>> CreateAsync(
        PinChartRequest request, CurrentWallet current, ICurrentUser user, AppDbContext db, TimeProvider time, CancellationToken ct)
    {
        var walletId = current.Id;
        var userId = user.UserId;
        var orders = await db.PinnedCharts.Where(p => p.WalletId == walletId && p.UserId == userId).Select(p => p.SortOrder).ToListAsync(ct);
        if (orders.Count >= MaxPins)
        {
            return Error.Conflict($"You can pin up to {MaxPins} charts.").ToProblem();
        }

        var pin = PinnedChart.Create(walletId, userId, request, orders.Count == 0 ? 0 : orders.Max() + 1, time.GetUtcNow());
        db.PinnedCharts.Add(pin);
        await db.SaveChangesAsync(ct);
        return TypedResults.Created($"pinned-charts/{pin.Id}", new PinnedChartItem(pin.Id, pin.Kind, pin.Title, pin.Query));
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> DeleteAsync(
        Guid pinId, CurrentWallet current, ICurrentUser user, AppDbContext db, CancellationToken ct)
    {
        var walletId = current.Id;
        var userId = user.UserId;
        var deleted = await db.PinnedCharts.Where(p => p.Id == pinId && p.WalletId == walletId && p.UserId == userId).ExecuteDeleteAsync(ct);
        return deleted == 0 ? Error.NotFound().ToProblem() : TypedResults.NoContent();
    }
}
