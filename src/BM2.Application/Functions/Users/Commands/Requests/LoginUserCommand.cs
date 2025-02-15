using BM2.Application.DTOs;
using BM2.Application.Responses;
using MediatR;

namespace BM2.Application.Functions.Users.Commands.Requests;

public class LoginUserCommand : IRequest<BaseResponse<LoggedUserDTO>>
{
    public string EmailAddress { get; set; } = null!;
    public string Password { get; set; } = null!;
}