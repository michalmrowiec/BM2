using BM2.Shared.DTOs;

namespace BM2.Shared.Requests.Commands.Record;

public class UpdateRecordTemplateCommand : AddRecordTemplateCommand, IBaseRequest<RecordTemplateDTO>
{
    public Guid Id { get; set; }
}