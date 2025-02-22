using BM2.Shared.DTOs;

namespace BM2.Shared.Requests.Commands.Record;

public class AddRecordCommand : AddBaseRecordCommand, IBaseRequest<RecordDTO>
{
    public Guid AccountId { get; set; }
    public DateTime RecordDateTime { get; set; } = DateTime.Now;
}