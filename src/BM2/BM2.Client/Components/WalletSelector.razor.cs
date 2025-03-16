using BM2.Client.Services;
using BM2.Client.Services.API;
using BM2.Domain.Entities.UserProfile;
using BM2.Shared.DTOs;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;

namespace BM2.Client.Components;

public partial class WalletSelector(IWalletSelectionState walletSelectionState, IApiClient apiClient) : ComponentBase
{
    [Inject] public IWalletSelectionState WalletSelectionState { get; set; } = walletSelectionState;
    [Inject] private IApiClient ApiClient { get; set; } = apiClient;
    private WalletDTO SelectedWallet
    {
        get => WalletSelectionState.SelectedWallet;
        set => WalletSelectionState.SetWallet(value);
    }

    protected override async Task OnInitializedAsync()
    {
        var response = await ApiClient.Get($"api/v1/wallets");
        var responseString = await response.Content.ReadAsStringAsync();
        var wallets = JsonConvert.DeserializeObject<IList<WalletDTO>>(responseString) ?? [];
        WalletSelectionState.SetWallets(wallets.OrderBy(x => x.WalletName).ToList());
        StateHasChanged();
    }
}