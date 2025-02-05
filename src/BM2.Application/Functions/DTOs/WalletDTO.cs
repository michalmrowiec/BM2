namespace BM2.Application.Functions.DTOs;

public class WalletDTO
{
    public Guid Id { get; set; }
    public string WalletName { get; set; } = null!;
    public bool IsActive { get; set; }
    public Guid DefaultCurrencyId { get; set; }
    public Guid OwnedByUserId { get; set; }

    public CurrencyDTO? DefaultCurrency { get; set; }
    public ICollection<AccountDTO> Accounts { get; set; } = [];
}