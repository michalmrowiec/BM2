using BM2.Application.Responses;
using MediatR;

namespace BM2.Shared.Requests.Commands.Record;

public class DeleteAccountRecordTransferCommand : IRequest<BaseResponse>
{
    public Guid Id { get; set; }
    public Guid OwnedByUserId { get; set; }
}
