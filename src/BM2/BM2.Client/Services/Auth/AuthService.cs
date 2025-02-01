using System.Security.Claims;
using BM2.Client.Services.LocalStorage;
using BM2.Client.VMs;
using Microsoft.AspNetCore.Components.Authorization;

namespace BM2.Client.Services.Auth
{
    public interface IAuthService
    {
        Task Login(LogedUser logedUser);
        Task Logout();
        Task<bool> CheckLogin();
        Task<ClaimsPrincipal> GetAuthenticationStateAsync();
        Task<string> GetJwtToken();
    }

    public class AuthService : IAuthService
    {
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly ILocalStorageService _localStorageService;

        public AuthService(AuthenticationStateProvider authenticationStateProvider, ILocalStorageService localStorageService)
        {
            _authenticationStateProvider = authenticationStateProvider;
            _localStorageService = localStorageService;
        }

        public async Task<ClaimsPrincipal> GetAuthenticationStateAsync()
        {
            var authState = await ((CustomAuthStateProvider)_authenticationStateProvider)
                .GetAuthenticationStateAsync();
            return authState.User;
        }

        public async Task<bool> CheckLogin()
        {
            var logedUser = await _localStorageService.GetItemAsync<LogedUser>("jwt");
            if (JwtHelper.IsTokenExpired(logedUser.JwtToken))
            {
                await _localStorageService.RemoveItemAsync("jwt");
                return false;
            }

            ((CustomAuthStateProvider)_authenticationStateProvider)
                .AuthenticateUser(logedUser.EmailAddress);

            return true;
        }

        public async Task Login(LogedUser logedUser)
        {
            ((CustomAuthStateProvider)_authenticationStateProvider)
                .AuthenticateUser(logedUser.EmailAddress);

            await _localStorageService.SetItemAsync("jwt", logedUser);
        }

        public async Task Logout()
        {
            ((CustomAuthStateProvider)_authenticationStateProvider)
                .LogoutUser();

            await _localStorageService.RemoveItemAsync("jwt");
        }

        public async Task<string> GetJwtToken()
        {
            var logedData = await _localStorageService.GetItemAsync<LogedUser>("jwt");
            return logedData.JwtToken;
        }
    }
}
