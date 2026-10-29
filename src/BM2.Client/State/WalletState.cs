using BM2.Client.Services;
using BM2.Contracts.Records;
using BM2.Contracts.Wallets;

namespace BM2.Client.State;

/// <summary>The user's wallets and the snapshot of the selected one, shared by all screens.</summary>
public sealed class WalletState(ApiClient api, BrowserStorage storage)
{
    private const string WalletKey = "bm2-wallet";
    private Task? _initialization;
    private (Guid WalletId, Task<RecordSuggestions?> Load)? _suggestions;

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

    /// <summary>Reloads the current wallet's snapshot; the wallet list (with its totals) stays as it is.</summary>
    public Task RefreshAsync() => Current is { } wallet ? ReloadSnapshotAsync(wallet.Id) : ReloadAsync(null);

    /// <summary>Reloads the wallet list too: after wallets were renamed, deleted or left, or to show fresh totals.</summary>
    public Task RefreshAllAsync() => ReloadAsync(Current?.Id);

    public async ValueTask<Guid?> GetLastAccountAsync() =>
        Current is not null && Guid.TryParse(await storage.GetAsync(LastAccountKey(Current.Id)), out var id) ? id : null;

    public async ValueTask SetLastAccountAsync(Guid accountId)
    {
        if (Current is not null)
        {
            await storage.SetAsync(LastAccountKey(Current.Id), accountId.ToString());
        }
    }

    /// <summary>Name, category and tag suggestions for the record form, loaded once per wallet.</summary>
    public Task<RecordSuggestions?> GetSuggestionsAsync()
    {
        if (Current is not { } wallet)
        {
            return Task.FromResult<RecordSuggestions?>(null);
        }

        if (_suggestions is not { } cached || cached.WalletId != wallet.Id)
        {
            _suggestions = (wallet.Id, LoadSuggestionsAsync(wallet.Id));
        }

        return _suggestions.Value.Load;
    }

    /// <summary>Call after records change, so the next form sees new names.</summary>
    public void ForgetSuggestions() => _suggestions = null;

    private async Task<RecordSuggestions?> LoadSuggestionsAsync(Guid walletId)
    {
        var result = await api.GetAsync<RecordSuggestions>($"api/wallets/{walletId}/records/suggestions");
        if (!result.Ok)
        {
            ForgetSuggestions();
        }

        return result.Value;
    }

    private async Task InitializeCoreAsync()
    {
        var stored = await storage.GetAsync(WalletKey);
        await ReloadAsync(Guid.TryParse(stored, out var id) ? id : null);
        if (Problem is not null)
        {
            // Let the next page try again instead of keeping the failure.
            _initialization = null;
        }
    }

    private async Task ReloadAsync(Guid? preferred)
    {
        var list = await api.GetAsync<List<WalletListItem>>("api/wallets");
        if (list.Ok)
        {
            Wallets = list.Value!;
            var selected = Wallets.Any(w => w.Id == preferred) ? preferred : Wallets.Count > 0 ? Wallets[0].Id : null;
            var snapshot = selected is { } walletId
                ? await LoadSnapshotAsync(walletId)
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

    private async Task ReloadSnapshotAsync(Guid walletId)
    {
        var snapshot = await LoadSnapshotAsync(walletId);
        Current = snapshot.Value;
        Problem = snapshot.Problem;
        Changed?.Invoke();
    }

    private Task<ApiResult<WalletSnapshot>> LoadSnapshotAsync(Guid walletId) => api.GetAsync<WalletSnapshot>(
        $"api/wallets/{walletId}?today={DateTime.Now.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture)}");

    private static string LastAccountKey(Guid walletId) => $"bm2-last-account:{walletId}";
}
