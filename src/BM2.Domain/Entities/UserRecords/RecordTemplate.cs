namespace BM2.Domain.Entities;

public class RecordTemplate : BaseRecord
{
    public Guid WalletId { get; set; }

    public Wallet? Wallet { get; set; }
}