using BM2.Shared.DTOs;

namespace BM2.Shared.Requests.Commands.User;

public class LoginUserCommand : IBaseRequest<LoggedUserDTO>
{
    public string EmailAddress { get; set; } = null!;
    public string Password { get; set; } = null!;
}