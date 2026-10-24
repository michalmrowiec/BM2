using System.IdentityModel.Tokens.Jwt;

namespace BM2.Client.Services.Auth
{
    public static class JwtHelper
    {
        public static bool IsTokenExpired(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var expClaim = jwtToken.Claims.FirstOrDefault(claim => claim.Type == "exp")?.Value;

            if (expClaim != null && long.TryParse(expClaim, out var exp))
            {
                var expirationTime = DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime;
                return expirationTime < DateTime.UtcNow;
            }

            return true;
        }
    }
}
