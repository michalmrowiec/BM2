using BM2.Domain.Entities.Interfaces;
using BM2.Domain.Entities.UserProfile;

namespace BM2.Domain.Entities.UserRecords;

public class RecordTagRelation : IEntity, IOwnedByUser
{
    public Guid Id { get; set; }
    public Guid RecordId { get; set; }
    public Guid TagId { get; set; }
    public Guid OwnedByUserId { get; set; }

    public BaseRecord? Record { get; set; }
    public Tag? Tag { get; set; }
    public User? OwnedByUser { get; set; }
    
    private RecordTagRelation(Guid recordId, Guid tagId, Guid ownedByUserId)
    {
        Id = Guid.NewGuid();
        RecordId = recordId;
        TagId = tagId;
        OwnedByUserId = ownedByUserId;
    }

    public static RecordTagRelation CreateInstance(Guid recordId, Guid tagId, Guid ownedByUserId)
    {
        return new RecordTagRelation(recordId, tagId, ownedByUserId);
    }
}