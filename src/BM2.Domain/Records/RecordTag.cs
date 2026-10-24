namespace BM2.Domain.Records;

public sealed class RecordTag
{
    private RecordTag()
    {
    }

    internal RecordTag(Guid tagId) => TagId = tagId;

    public Guid RecordId { get; private set; }

    public Guid TagId { get; private set; }
}
