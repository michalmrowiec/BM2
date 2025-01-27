using BM2.Application.Functions.Dtos;
using BM2.Application.Responses;
using MediatR;

namespace BM2.Application.Functions.Commands;

public class LoginUserCommand : IRequest<BaseResponse<LoggedUserDto>>
{
    public string EmailAddress { get; set; } = null!;
    public string Password { get; set; } = null!;
}