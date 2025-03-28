﻿using BM2.Client.Components;
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
    
    private Task OpenAddUpdateAccountDialogAsync(AccountDTO? account = null)
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
                x => x.FuncsOnSuccess,
                [EventCallback.Factory.Create(this, GetAccounts)]
            },
            {
                x => x.AccountToUpdate, account
            },
            {
                x => x.FormType, DialogFormType.Edit
            }
        };
        return DialogService.ShowAsync<AddAccountDialogForm>(null, parameters, options);
    }
    
    private Task OpenUpdateAccountAssignmentDialogAsync(AccountDTO account)
    {
        var options = new DialogOptions
        {
            CloseOnEscapeKey = true,
            CloseButton = true,
            MaxWidth = MaxWidth.Small,
            FullWidth = true
        };
        var parameters = new DialogParameters<UpdateAccountAssignment>
        {
            {
                x => x.FuncsOnUpdated, [EventCallback.Factory.Create(this, GetAccounts)]
            },
            {
                x => x.OldAccount, account
            }
        };
        return DialogService.ShowAsync<UpdateAccountAssignment>(null, parameters, options);
    }
}