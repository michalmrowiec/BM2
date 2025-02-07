using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BM2.Application.Contracts.Services;
using BM2.Domain.Entities;
using BM2.Domain.Entities.UserProfile;
using Microsoft.IdentityModel.Tokens;

namespace BM2.Infrastructure.Services;

public class JwtTokenService(AuthenticationSettings authenticationSettings)
    : IJwtTokenService
{
    private readonly AuthenticationSettings _authenticationSettings = authenticationSettings;

    public string GenerateJwt(User employee)
    {
        var claims = new List<Claim>()
        {
            new(ClaimTypes.NameIdentifier, employee.Id.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authenticationSettings.JwtKey));
        var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiress = DateTime.Now.AddDays(_authenticationSettings.JwtExpireMinutes);

        var token = new JwtSecurityToken(_authenticationSettings.JwtIssuer,
            _authenticationSettings.JwtIssuer,
            claims,
            expires: expiress,
            signingCredentials: cred);

        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(token);
    }
}