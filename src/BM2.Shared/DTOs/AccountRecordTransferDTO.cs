namespace BM2.Shared.DTOs;

public class AccountRecordTransferDTO
{
    public Guid Id { get; set; }
    
    public Guid FromAccountId { get; set; }
    public string FromAccountName { get; set; }
    public Guid ToAccountId { get; set; }
    public string ToAccountName { get; set; }

    public decimal FromAmount { get; set; }
    public Guid FromCurrencyId { get; set; }
    public decimal ToAmount { get; set; }
    public Guid ToCurrencyId { get; set; }
}