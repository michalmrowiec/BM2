using System.Net;
using BM2.Client.Components;
using BM2.Client.Services.API;
using BM2.Client.Services.Notification;
using BM2.Shared.DTOs;
using BM2.Shared.Requests.Commands.Category;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Newtonsoft.Json;

namespace BM2.Client.Pages;

public partial class Categories(IApiClient apiClient, IDialogService dialogService) : ComponentBase
{
    [Inject] private IApiClient ApiClient { get; set; } = apiClient;
    [Inject] private IDialogService DialogService { get; set; } = dialogService;
    [Inject] private ISnackbar Snackbar { get; set; }
    [Inject] private IAlertService AlertService { get; set; }
    private IList<CategoryWalletRelationDTO> CategoryWithWalletRelationList { get; set; } =
        new List<CategoryWalletRelationDTO>();

    private IList<WalletDTO> WalletList { get; set; } = new List<WalletDTO>();
    private bool BlockedView { get; set; } = true;

    private async Task GetCategories()
    {
        var response2 = await ApiClient.Get("api/v1/categories/wallet-relations");
        var responseString2 = await response2.Content.ReadAsStringAsync();
        CategoryWithWalletRelationList =
            JsonConvert.DeserializeObject<IList<CategoryWalletRelationDTO>>(responseString2) ?? [];

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

    private async Task UpdateCategoryWalletRelationAsync()
    {
        var command = new SetWalletCategoryRelationsCommand()
        {
            CategoryWalletRelations = new List<CategoryWalletRelationCommand>()
        };

        foreach (var category in CategoryWithWalletRelationList)
        {
            foreach (var walletRelation in category.WalletRelations)
            {
                command.CategoryWalletRelations.Add(new CategoryWalletRelationCommand()
                {
                    CategoryId = category.Id,
                    WalletId = walletRelation.WalletId,
                    Status = walletRelation.Status
                });
            }
        }
        
        var response = await ApiClient.Create(@"api/v1/categories/wallet-relations", command);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            Snackbar.Add(new MarkupString($"Saved changes"), Severity.Success);
        }
        else
        {
            await response.HandleFailure(AlertService);
        }

        await GetCategories();
    }
}