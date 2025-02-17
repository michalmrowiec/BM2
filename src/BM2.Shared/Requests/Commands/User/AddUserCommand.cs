using BM2.Shared.DTOs;

namespace BM2.Shared.Requests.Commands.User;

public class AddUserCommand : IBaseRequest<UserDTO>
{
    public string EmailAddress { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string RepeatPassword { get; set; } = null!;
}