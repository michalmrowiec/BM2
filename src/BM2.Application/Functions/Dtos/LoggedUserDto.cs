namespace BM2.Application.Functions.Dtos;

public class LoggedUserDto(string jwtToken)
{
    public string JwtToken { get; set; } = jwtToken;
}