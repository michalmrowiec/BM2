namespace BM2.Application.Functions.Dtos;

public class LoggedUserDto
{
    public string JwtToken { get; set; } = null!;

    public LoggedUserDto(string jwtToken)
    {
        JwtToken = jwtToken;
    }
}