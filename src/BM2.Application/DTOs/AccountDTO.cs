namespace BM2.Application.DTOs;

public class AccountDTO
{
    public Guid Id { get; set; }
    public Guid WalletId { get; set; }
    public string AccountName { get; set; } = null!;
    public bool IsActive { get; set; }
    public Guid DefaultCurrencyId { get; set; }
    //public Guid OwnedByUserId { get; set; }
    
    public CurrencyDTO? DefaultCurrency { get; set; }
}