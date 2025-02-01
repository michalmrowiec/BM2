using BM2.Application.Functions.Dtos;
using BM2.Application.Responses;
using MediatR;

namespace BM2.Application.Functions.Requests.Command;

public class CreateUserCommand : IRequest<BaseResponse<UserDto>>
{
    public string EmailAddress { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string RepeatPassword { get; set; } = null!;
}