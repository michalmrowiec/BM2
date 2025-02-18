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
    private IList<CategoryDTO> CategoryList { get; set; } = new List<CategoryDTO>();

    private async Task GetCategories()
    {
        var response = await ApiClient.Get("api/v1/categories");
        var responseString = await response.Content.ReadAsStringAsync();
        CategoryList = JsonConvert.DeserializeObject<IList<CategoryDTO>>(responseString) ?? [];
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