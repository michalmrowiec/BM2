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
    private IList<AccountDTO> AccountList { get; set; } = new List<AccountDTO>();

    private int SelectedYear
    {
        get => _selectedYear;
        set
        {
            _selectedYear = value;
            _ = GetRecords();
        }
    }

    private int SelectedMonth
    {
        get => _selectedMonth;
        set
        {
            _selectedMonth = value;
            _ = GetRecords();
        }
    }

    private int _selectedYear = DateTime.Today.Year;
    private int _selectedMonth = DateTime.Today.Month;

    private readonly List<(int Value, string Text)> _months =
    [
        (1, "January"), (2, "February"), (3, "March"), (4, "April"),
        (5, "May"), (6, "June"), (7, "July"), (8, "August"),
        (9, "September"), (10, "October"), (11, "November"), (12, "December")
    ];

    private async Task GetRecords()
    {
        var response = await ApiClient.Get($"api/v1/records?year={_selectedYear}&month={_selectedMonth}");
        var responseString = await response.Content.ReadAsStringAsync();
        var records = JsonConvert.DeserializeObject<IList<RecordDTO>>(responseString) ?? [];
        RecordList = records.OrderBy(x => x.RecordDateTime).ToList();
        StateHasChanged();
    }

    private async Task GetAccounts()
    {
        var response = await ApiClient.Get($"api/v1/accounts");
        var responseString = await response.Content.ReadAsStringAsync();
        var accounts = JsonConvert.DeserializeObject<IList<AccountDTO>>(responseString) ?? [];
        AccountList = accounts.OrderBy(x => x.AccountName).ToList();
        StateHasChanged();
    }
    
    protected override async Task OnInitializedAsync()
    {
        await GetRecords();
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
                [EventCallback.Factory.Create(this, GetRecords)]
            }
        };
        return DialogService.ShowAsync<AddRecordDialogForm>(null, parameters, options);
    }

    private Task OpenUpdateRecordDialogAsync(RecordDTO record)
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
                x => x.FuncsOnCreated, [EventCallback.Factory.Create(this, GetRecords)]
            },
            {
                x => x.FormType, DialogFormType.Edit
            },
            {
                x => x.RecordToUpdate, record
            }
        };
        return DialogService.ShowAsync<AddRecordDialogForm>(null, parameters, options);
    }
}