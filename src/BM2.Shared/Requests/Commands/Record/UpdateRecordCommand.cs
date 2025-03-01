using BM2.Shared.DTOs;

namespace BM2.Shared.Requests.Commands.Record;

public class UpdateRecordCommand : AddRecordCommand, IBaseRequest<RecordDTO>
{
    public Guid Id { get; set; }
}