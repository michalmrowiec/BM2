using System.Text.Json.Serialization;
using BM2.Application.DTOs;

namespace BM2.Application.Functions.Accounts.Commands.AddAccountCommand;

public class AddAccountCommand : IBaseRequest<AccountDTO>
{
    public Guid WalletId { get; set; }
    public string AccountName { get; set; } = null!;
    public bool IsActive { get; set; }
    public Guid DefaultCurrencyId { get; set; }
    [JsonIgnore]
    public Guid OwnedByUserId { get; set; }
}