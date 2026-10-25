using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace BM2.Client.State;

/// <summary>Base for components that show the current wallet: loads it once interactive and re-renders on changes.</summary>
public abstract class WalletComponentBase : ComponentBase, IDisposable
{
    [Inject]
    protected WalletState WalletState { get; set; } = null!;

    [CascadingParameter]
    private Task<AuthenticationState>? AuthenticationState { get; set; }

    protected override async Task OnInitializedAsync()
    {
        WalletState.Changed += HandleWalletChanged;

        // Anonymous visitors (e.g. the landing page) must not hit the API: a 401 would redirect them to log in.
        var signedIn = AuthenticationState is null
            || (await AuthenticationState).User.Identity?.IsAuthenticated == true;
        if (RendererInfo.IsInteractive && signedIn)
        {
            await WalletState.InitializeAsync();
        }
    }

    /// <summary>Runs after another wallet was selected or the current one was refreshed.</summary>
    protected virtual Task OnWalletChangedAsync() => Task.CompletedTask;

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            WalletState.Changed -= HandleWalletChanged;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void HandleWalletChanged() => _ = InvokeAsync(async () =>
    {
        await OnWalletChangedAsync();
        StateHasChanged();
    });
}
