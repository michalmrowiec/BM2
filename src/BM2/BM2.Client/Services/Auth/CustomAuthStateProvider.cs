using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace BM2.Client.Services.Auth
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var identity = new ClaimsIdentity();
            var user = new ClaimsPrincipal(identity);

            return Task.FromResult(new AuthenticationState(user));
        }

        public void AuthenticateUser(string userEmail)
        {
            var identity = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Name, userEmail),
                new Claim(ClaimTypes.Email, userEmail),
            ], "Custom Authentication");

            var user = new ClaimsPrincipal(identity);

            NotifyAuthenticationStateChanged(
                Task.FromResult(new AuthenticationState(user)));
        }

        public void LogoutUser()
        {
            var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
            NotifyAuthenticationStateChanged(
                Task.FromResult(new AuthenticationState(anonymousUser)));
        }
    }
}
