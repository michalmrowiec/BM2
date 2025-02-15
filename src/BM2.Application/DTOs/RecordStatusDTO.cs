namespace BM2.Application.DTOs;

public class RecordStatusDTO
{
    public Guid Id { get; set; }
    public string RecordStatusName { get; set; } = null!;
    public bool ForRecords { get; set; }
    public bool ForPeriodicRecord  { get; set; }
}