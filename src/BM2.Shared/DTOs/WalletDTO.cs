namespace BM2.Shared.DTOs;

public class WalletBaseDTO
{
    public Guid Id { get; set; }
    public string WalletName { get; set; } = null!;
    public bool IsActive { get; set; }
    public Guid DefaultCurrencyId { get; set; }
    //public Guid OwnedByUserId { get; set; }

    public CurrencyDTO? DefaultCurrency { get; set; }
    //public ICollection<AccountDTO> Accounts { get; set; } = [];

    public override string ToString()
    {
        return this.DefaultCurrency != null
            ? string.Concat(this.IsActive ? "" : "(Off) ", this.WalletName, " ", $"[{this.DefaultCurrency?.IsoCode}]")
            : this.WalletName;
    }
}

public class WalletDTO : WalletBaseDTO
{
    public ICollection<AccountBasicDTO> Accounts { get; set; } = [];
}