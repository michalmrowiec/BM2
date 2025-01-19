namespace BM2.Domain.Entities;

public class RecordStatus : IEntity
{
    public Guid Id { get; set; }
    public int StatusCode { get; set; }
    public string RecordStatusName { get; set; }
    public bool ForRecords { get; set; }
    public bool ForPeriodicRecord  { get; set; }
}