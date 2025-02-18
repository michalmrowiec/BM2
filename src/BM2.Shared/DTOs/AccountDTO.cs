﻿namespace BM2.Shared.DTOs;

public class AccountBasicDTO
{
    public Guid Id { get; set; }
    public Guid WalletId { get; set; }
    public string AccountName { get; set; } = null!;
    public bool IsActive { get; set; }
    public Guid DefaultCurrencyId { get; set; }
    //public Guid OwnedByUserId { get; set; }
    
    public CurrencyDTO? DefaultCurrency { get; set; }
    //public WalletDTO? Wallet { get; set; }
}

public class AccountDTO : AccountBasicDTO
{
    public WalletBasicDTO? Wallet { get; set; }
}