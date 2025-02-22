using BM2.Client.Components;
using BM2.Client.Services.API;
using BM2.Shared.DTOs;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Newtonsoft.Json;

namespace BM2.Client.Pages;

public partial class Records(IApiClient apiClient, IDialogService dialogService) : ComponentBase
{
    [Inject] private IApiClient ApiClient { get; set; } = apiClient;
    [Inject] private IDialogService DialogService { get; set; } = dialogService;
    private IList<RecordDTO> RecordList { get; set; } = new List<RecordDTO>();

    private async Task GetAccounts()
    {
        var response = await ApiClient.Get("api/v1/records");
        var r = await response.Content.ReadAsStringAsync();
        RecordList = JsonConvert.DeserializeObject<IList<RecordDTO>>(r) ?? [];
        StateHasChanged();
    }

    protected override async Task OnInitializedAsync()
    {
        await GetAccounts();
    }
    
    private Task OpenAddRecordDialogAsync()
    {
        var options = new DialogOptions
        {
            CloseOnEscapeKey = true,
            CloseButton = true,
            MaxWidth = MaxWidth.Small,
            FullWidth = true
        };
        var parameters = new DialogParameters<AddRecordDialogForm>
        {
            {
                x => x.FuncsOnCreated,
                [EventCallback.Factory.Create(this, GetAccounts)]
            }
        };
        return DialogService.ShowAsync<AddRecordDialogForm>(null, parameters, options);
    }
}