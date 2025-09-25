using BM2.Shared.DTOs;

namespace BM2.Shared.Requests.Commands.Record;

public class AddRecordTemplateCommand : AddBaseRecordCommand, IBaseRequest<RecordTemplateDTO>
{
    public Guid WalletId { get; set; }
}