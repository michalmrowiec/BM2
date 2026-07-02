using BM2.Shared.SystemCodes;

namespace BM2.Shared.DTOs;

public class PeriodicRecordDefinitionDTO
{
    public Guid Id { get; set; }
    public Guid RecordTemplateId { get; set; }
    public Guid CurrencyId { get; set; }
    public Guid PeriodicRecordStatusId { get; set; }
    public Guid SetRecordStatusId { get; set; }
    public Guid WalletId { get; set; }
    public Guid SetRecordAccountId { get; set; }
    public Periodicity Periodicity { get; set; }
    public DateTime StartDate { get; set; }

    public RecordTemplateDTO? RecordTemplate { get; set; }
    public CurrencyDTO? Currency { get; set; }
    public RecordStatusDTO? PeriodicRecordStatus { get; set; }
    public RecordStatusDTO? SetRecordStatus { get; set; }
    public WalletBaseDTO? Wallet { get; set; }
    public AccountBasicDTO? SetRecordAccount { get; set; }
}
