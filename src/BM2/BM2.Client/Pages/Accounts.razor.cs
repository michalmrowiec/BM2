using BM2.Client.Components;
using BM2.Client.Services.API;
using BM2.Shared.DTOs;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Newtonsoft.Json;

namespace BM2.Client.Pages;

public partial class Accounts(IApiClient apiClient, IDialogService dialogService) : ComponentBase
{
    [Inject] private IApiClient ApiClient { get; set; } = apiClient;
    [Inject] private IDialogService DialogService { get; set; } = dialogService;
    private IList<AccountDTO> AccountList { get; set; } = new List<AccountDTO>();

    private async Task GetAccounts()
    {
        var response = await ApiClient.Get("api/v1/accounts");
        var r = await response.Content.ReadAsStringAsync();
        AccountList = JsonConvert.DeserializeObject<IList<AccountDTO>>(r) ?? [];
        StateHasChanged();
    }

    protected override async Task OnInitializedAsync()
    {
        await GetAccounts();
    }
    
    private Task OpenAddAccountDialogAsync()
    {
        var options = new DialogOptions
        {
            CloseOnEscapeKey = true,
            CloseButton = true,
            MaxWidth = MaxWidth.Small,
            FullWidth = true
        };
        var parameters = new DialogParameters<AddAccountDialogForm>
        {
            {
                x => x.FuncsOnCreated,
                [EventCallback.Factory.Create(this, GetAccounts)]
            }
        };
        return DialogService.ShowAsync<AddAccountDialogForm>(null, parameters, options);
    }
}