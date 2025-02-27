using System.Net;
using BM2.Client.Components;
using BM2.Client.Services.API;
using BM2.Client.Services.Notification;
using BM2.Shared.DTOs;
using BM2.Shared.Requests.Commands.Tag;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Newtonsoft.Json;

namespace BM2.Client.Pages;

public partial class Tags(IApiClient apiClient, IDialogService dialogService) : ComponentBase
{
    [Inject] private IApiClient ApiClient { get; set; } = apiClient;
    [Inject] private IDialogService DialogService { get; set; } = dialogService;
    [Inject] private ISnackbar Snackbar { get; set; }
    [Inject] private IAlertService AlertService { get; set; }

    private List<TagWalletRelationDTO> TagWithWalletRelationList { get; set; } = [];

    private IList<WalletDTO> WalletList { get; set; } = new List<WalletDTO>();
    private bool BlockedView { get; set; } = true;

    private async Task GetTags()
    {
        var response2 = await ApiClient.Get("api/v1/tags/wallet-relations");
        var responseString2 = await response2.Content.ReadAsStringAsync();
        TagWithWalletRelationList =
            JsonConvert.DeserializeObject<List<TagWalletRelationDTO>>(responseString2) ?? [];

        var response = await ApiClient.Get("api/v1/wallets");
        var r = await response.Content.ReadAsStringAsync();
        WalletList = JsonConvert.DeserializeObject<IList<WalletDTO>>(r) ?? [];

        StateHasChanged();
    }

    protected override async Task OnInitializedAsync()
    {
        await GetTags();
    }

    private Task OpenAddTagDialogAsync()
    {
        var options = new DialogOptions
        {
            CloseOnEscapeKey = true,
            CloseButton = true,
            MaxWidth = MaxWidth.Small,
            FullWidth = true
        };
        var parameters = new DialogParameters<AddTagDialogForm>
        {
            {
                x => x.FuncsOnCreated,
                [EventCallback.Factory.Create(this, GetTags)]
            }
        };
        return DialogService.ShowAsync<AddTagDialogForm>(null, parameters, options);
    }

    private async Task UpdateTagWalletRelationAsync()
    {
        BlockedView = true;
        StateHasChanged();

        var command = new SetWalletTagRelationsCommand()
        {
            TagWalletRelations = new List<TagWalletRelationCommand>()
        };

        foreach (var category in TagWithWalletRelationList)
        {
            foreach (var walletRelation in category.WalletRelations)
            {
                command.TagWalletRelations.Add(new TagWalletRelationCommand()
                {
                    TagId = category.Id,
                    WalletId = walletRelation.WalletId,
                    Status = walletRelation.Status
                });
            }
        }

        var response = await ApiClient.Create(@"api/v1/tags/wallet-relations", command);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            Snackbar.Add(new MarkupString($"Saved changes"), Severity.Success);
        }
        else
        {
            await response.HandleFailure(AlertService);
        }

        await GetTags();
    }
}