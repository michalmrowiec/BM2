using BM2.Client.Components;
using BM2.Client.Services.API;
using BM2.Shared.DTOs;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Newtonsoft.Json;

namespace BM2.Client.Pages;

public partial class Categories(IApiClient apiClient, IDialogService dialogService) : ComponentBase
{
    [Inject] private IApiClient ApiClient { get; set; } = apiClient;
    [Inject] private IDialogService DialogService { get; set; } = dialogService;
    private IList<CategoryWithWalletRelationDTO> CategoryWithWalletRelationList { get; set; } = new List<CategoryWithWalletRelationDTO>();
    private IList<WalletDTO> WalletList { get; set; } = new List<WalletDTO>();

    private async Task GetCategories()
    {
        var response2 = await ApiClient.Get("api/v1/categories/wallet-relations");
        var responseString2 = await response2.Content.ReadAsStringAsync();
        CategoryWithWalletRelationList = JsonConvert.DeserializeObject<IList<CategoryWithWalletRelationDTO>>(responseString2) ?? [];

        var response = await ApiClient.Get("api/v1/wallets");
        var r = await response.Content.ReadAsStringAsync();
        WalletList = JsonConvert.DeserializeObject<IList<WalletDTO>>(r) ?? [];

        StateHasChanged();
    }

    protected override async Task OnInitializedAsync()
    {
        await GetCategories();
    }

    private Task OpenAddCategoryDialogAsync()
    {
        var options = new DialogOptions
        {
            CloseOnEscapeKey = true,
            CloseButton = true,
            MaxWidth = MaxWidth.Small,
            FullWidth = true
        };
        var parameters = new DialogParameters<AddCategoryDialogForm>
        {
            {
                x => x.FuncsOnCreated,
                [EventCallback.Factory.Create(this, GetCategories)]
            }
        };
        return DialogService.ShowAsync<AddCategoryDialogForm>(null, parameters, options);
    }
}