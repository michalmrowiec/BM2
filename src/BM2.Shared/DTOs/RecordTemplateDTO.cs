namespace BM2.Shared.DTOs;

public class RecordTemplateDTO : BaseRecordDTO
{
    public Guid WalletId { get; set; }
    public WalletBaseDTO? Wallet { get; set; }

    public override string ToString()
    {
        return Wallet != null
            ? $"{Name} [{Wallet.WalletName}]"
            : Name;
    }
}
