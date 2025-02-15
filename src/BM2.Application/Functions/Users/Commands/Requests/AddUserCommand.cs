using BM2.Application.DTOs;
using BM2.Application.Responses;
using MediatR;

namespace BM2.Application.Functions.Users.Commands.Requests;

public class AddUserCommand : IRequest<BaseResponse<UserDTO>>
{
    public string EmailAddress { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string RepeatPassword { get; set; } = null!;
}