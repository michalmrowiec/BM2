using BM2.Client.Services;
using BM2.Contracts.Wallets;

namespace BM2.Client.State;

/// <summary>The user's wallets and the snapshot of the selected one, shared by all screens.</summary>
public sealed class WalletState(ApiClient api, BrowserStorage storage)
{
    private const string WalletKey = "bm2-wallet";
    private Task? _initialization;

    public event Action? Changed;

    public IReadOnlyList<WalletListItem> Wallets { get; private set; } = [];

    public WalletSnapshot? Current { get; private set; }

    public bool IsLoaded { get; private set; }

    public ApiProblem? Problem { get; private set; }

    /// <summary>Safe to call from every component; loads once.</summary>
    public Task InitializeAsync() => _initialization ??= InitializeCoreAsync();

    public async Task SelectAsync(Guid walletId)
    {
        await storage.SetAsync(WalletKey, walletId.ToString());
        await ReloadAsync(walletId);
    }

    public Task RefreshAsync() => ReloadAsync(Current?.Id);

    public async ValueTask<Guid?> GetLastAccountAsync() =>
        Current is not null && Guid.TryParse(await storage.GetAsync(LastAccountKey(Current.Id)), out var id) ? id : null;

    public async ValueTask SetLastAccountAsync(Guid accountId)
    {
        if (Current is not null)
        {
            await storage.SetAsync(LastAccountKey(Current.Id), accountId.ToString());
        }
    }

    private async Task InitializeCoreAsync()
    {
        var stored = await storage.GetAsync(WalletKey);
        await ReloadAsync(Guid.TryParse(stored, out var id) ? id : null);
    }

    private async Task ReloadAsync(Guid? preferred)
    {
        var list = await api.GetAsync<List<WalletListItem>>("api/wallets");
        if (list.Ok)
        {
            Wallets = list.Value!;
            var selected = Wallets.Any(w => w.Id == preferred) ? preferred : Wallets.Count > 0 ? Wallets[0].Id : null;
            var snapshot = selected is { } walletId
                ? await api.GetAsync<WalletSnapshot>($"api/wallets/{walletId}")
                : new ApiResult<WalletSnapshot>(null, null);
            Current = snapshot.Value;
            Problem = snapshot.Problem;
        }
        else
        {
            Problem = list.Problem;
        }

        IsLoaded = true;
        Changed?.Invoke();
    }

    private static string LastAccountKey(Guid walletId) => $"bm2-last-account:{walletId}";
}
