﻿using System.Text.Json.Serialization;
using BM2.Shared.DTOs;

namespace BM2.Shared.Requests.Commands.Account;

public class AddUpdateAccountCommand : IBaseRequest<AccountDTO>
{
    public Guid Id { get; set; }
    public Guid WalletId { get; set; }
    public string AccountName { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public Guid DefaultCurrencyId { get; set; }
    [JsonIgnore]
    public Guid OwnedByUserId { get; set; }
}