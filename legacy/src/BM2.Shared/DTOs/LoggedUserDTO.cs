namespace BM2.Shared.DTOs;

public class LoggedUserDTO(string emailAddress, string jwtToken)
{
    public string EmailAddress { get; set; } = emailAddress;
    public string JwtToken { get; set; } = jwtToken;
}