using BM2.Application.DTOs;

namespace BM2.Application.Functions.Record.Commands.Requests;

public class AddRecordCommand : AddBaseRecordCommand, IBaseRequest<RecordDTO>
{
    public Guid AccountId { get; set; }
    public DateTime RecordDateTime { get; set; }
}