using BM2.Client.Services.API;
using BM2.Shared.DTOs;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;

namespace BM2.Client.Pages;

public partial class Wallets(IApiOperator apiOperator) : ComponentBase
{
    [Inject] private IApiOperator ApiOperator { get; set; } = apiOperator;
    private IList<WalletDTO> WalletList { get; set; } = new List<WalletDTO>();

    protected override async Task OnInitializedAsync()
    {
        var response = await ApiOperator.Get("api/v1/wallets");
        var r = await response.Content.ReadAsStringAsync();
        WalletList = JsonConvert.DeserializeObject<IList<WalletDTO>>(r)??[];
        StateHasChanged();
    }
}