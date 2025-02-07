using BM2.Domain.Entities.UserProfile;

namespace BM2.Domain.Entities.UserRecords;

public class Record : BaseRecord
{
    public Guid AccountId { get; set; }
    public DateTime RecordDateTime { get; set; }

    public Account? Account { get; set; }
}