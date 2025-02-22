using BM2.Shared.DTOs.Interfaces;

namespace BM2.Shared.DTOs;

public class RecordStatusDTO : IEntityDTO
{
    public Guid Id { get; set; }
    public string RecordStatusName { get; set; } = null!;
    public bool ForRecords { get; set; }
    public bool ForPeriodicRecord  { get; set; }
}