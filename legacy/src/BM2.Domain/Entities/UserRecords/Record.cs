using BM2.Domain.Entities.UserProfile;

namespace BM2.Domain.Entities.UserRecords;

public class Record : BaseRecord
{
    public Guid AccountId { get; set; }
    public DateTime RecordDateTime { get; set; }

    public Account? Account { get; set; }
    
    public Guid? AccountRecordTransferId { get; set; } // ewentualnie zamienić na enum - RecordType - to jest tylko info to szybkiego oznaczenia
}