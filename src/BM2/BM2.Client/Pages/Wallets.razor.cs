using BM2.Client.Components;
using BM2.Client.Services.API;
using BM2.Shared.DTOs;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Newtonsoft.Json;

namespace BM2.Client.Pages;

public partial class Wallets(IApiClient apiClient, IDialogService dialogService) : ComponentBase
{
    [Inject] private IApiClient ApiClient { get; set; } = apiClient;
    [Inject] private IDialogService DialogService { get; set; } = dialogService;
    private IList<WalletDTO> WalletList { get; set; } = new List<WalletDTO>();

    private async Task GetWallets()
    {
        var response = await ApiClient.Get("api/v1/wallets");
        var r = await response.Content.ReadAsStringAsync();
        WalletList = JsonConvert.DeserializeObject<IList<WalletDTO>>(r) ?? [];
        StateHasChanged();
    }

    protected override async Task OnInitializedAsync()
    {
        await GetWallets();
    }

    private Task OpenAddWalletDialogAsync()
    {
        var options = new DialogOptions
        {
            CloseOnEscapeKey = true,
            CloseButton = true,
            MaxWidth = MaxWidth.Small,
            FullWidth = true
        };
        var parameters = new DialogParameters<AddWalletDialogForm>
        {
            {
                x => x.FuncsOnCreated,
                [EventCallback.Factory.Create(this, GetWallets)]
            }
        };
        return DialogService.ShowAsync<AddWalletDialogForm>(null, parameters, options);
    }
    
    private Task OpenUpdateWalletDialogAsync(WalletDTO wallet)
    {
        var options = new DialogOptions
        {
            CloseOnEscapeKey = true,
            CloseButton = true,
            MaxWidth = MaxWidth.Small,
            FullWidth = true
        };
        var parameters = new DialogParameters<AddWalletDialogForm>
        {
            {
                x => x.FuncsOnCreated,
                [EventCallback.Factory.Create(this, GetWallets)]
            },
            {
                x => x.FormType, DialogFormType.Edit
            },
            {
                x => x.WalletToUpdate, wallet
            }
        };
        return DialogService.ShowAsync<AddWalletDialogForm>(null, parameters, options);
    }
}