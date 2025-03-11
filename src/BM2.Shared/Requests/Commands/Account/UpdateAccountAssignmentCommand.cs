using System.Text.Json.Serialization;
using BM2.Application.Responses;
using MediatR;

namespace BM2.Shared.Requests.Commands.Account;

public class UpdateAccountAssignmentCommand : IRequest<BaseResponse>
{
    public Guid OldAccountId { get; set; }
    public Guid NewAccountId { get; set; }
    [JsonIgnore]
    public Guid OwnedByUserId { get; set; }
}