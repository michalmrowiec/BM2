using BM2.Domain.Entities.UserProfile;

namespace BM2.Domain.Entities.UserRecords;

public class RecordTemplate : BaseRecord
{
    public Guid WalletId { get; set; }

    public Wallet? Wallet { get; set; }
    public ICollection<PeriodicRecordDefinition> PeriodicRecordDefinitions { get; set; } = [];
}