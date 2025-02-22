using BM2.Shared.DTOs.Interfaces;

namespace BM2.Shared.DTOs;

public class AccountBasicDTO : IEntityDTO
{
    public Guid Id { get; set; }
    public Guid WalletId { get; set; }
    public string AccountName { get; set; } = null!;
    public bool IsActive { get; set; }
    public Guid DefaultCurrencyId { get; set; }
    //public Guid OwnedByUserId { get; set; }
    
    public CurrencyDTO? DefaultCurrency { get; set; }
    //public WalletDTO? Wallet { get; set; }

    public override string ToString()
    {
        return this.DefaultCurrency != null
            ? string.Concat(this.IsActive ? "" : "(Off) ", this.AccountName, " ", $"[{this.DefaultCurrency?.IsoCode}]")
            : this.AccountName;
    }
}

public class AccountDTO : AccountBasicDTO
{
    public WalletBaseDTO? Wallet { get; set; }
}