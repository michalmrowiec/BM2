namespace BM2.Application.Functions.DTOs;

public class LoggedUserDto(string emailAddress, string jwtToken)
{
    public string EmailAddress { get; set; } = emailAddress;
    public string JwtToken { get; set; } = jwtToken;
}