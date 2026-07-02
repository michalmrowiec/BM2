using System.Text.Json.Serialization;
using BM2.Application.Responses;
using MediatR;

namespace BM2.Shared.Requests.Commands.Account;

public class DeleteAccountCommand : IRequest<BaseResponse>
{
    public Guid Id { get; set; }

    [JsonIgnore]
    public Guid OwnedByUserId { get; set; }
}
