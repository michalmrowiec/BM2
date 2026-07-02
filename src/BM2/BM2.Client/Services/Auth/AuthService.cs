using System.Security.Claims;
using BM2.Client.Models;
using BM2.Client.Services.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace BM2.Client.Services.Auth
{
    public interface IAuthService
    {
        Task Login(LoggedUser logedUser);
        Task Logout();
        Task<bool> CheckLogin();
        Task<bool> EnsureInitializedAsync();
        Task<ClaimsPrincipal> GetAuthenticationStateAsync();
        Task<string> GetJwtToken();
    }

    public class AuthService : IAuthService
    {
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly ILocalStorageService _localStorageService;
        private readonly SemaphoreSlim _initializationLock = new(1, 1);
        private LoggedUser? _cachedLoggedUser;
        private bool _isInitialized;

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
            return await EnsureInitializedAsync();
        }

        public async Task<bool> EnsureInitializedAsync()
        {
            if (_isInitialized)
                return _cachedLoggedUser is not null;

            await _initializationLock.WaitAsync();
            try
            {
                if (_isInitialized)
                    return _cachedLoggedUser is not null;

                LoggedUser? logedUser;
                try
                {
                    logedUser = await _localStorageService.GetItemAsync<LoggedUser>("jwt");
                }
                catch (InvalidOperationException)
                {
                    return false;
                }

                if (logedUser is null || string.IsNullOrWhiteSpace(logedUser.JwtToken))
                {
                    ((CustomAuthStateProvider)_authenticationStateProvider).LogoutUser();
                    _cachedLoggedUser = null;
                    _isInitialized = true;
                    return false;
                }

                if (JwtHelper.IsTokenExpired(logedUser.JwtToken))
                {
                    await _localStorageService.RemoveItemAsync("jwt");
                    ((CustomAuthStateProvider)_authenticationStateProvider).LogoutUser();
                    _cachedLoggedUser = null;
                    _isInitialized = true;
                    return false;
                }

                _cachedLoggedUser = logedUser;
                ((CustomAuthStateProvider)_authenticationStateProvider)
                    .AuthenticateUser(logedUser.EmailAddress);
                _isInitialized = true;

                return true;
            }
            finally
            {
                _initializationLock.Release();
            }
        }

        public async Task Login(LoggedUser logedUser)
        {
            ((CustomAuthStateProvider)_authenticationStateProvider)
                .AuthenticateUser(logedUser.EmailAddress);

            _cachedLoggedUser = logedUser;
            _isInitialized = true;
            await _localStorageService.SetItemAsync("jwt", logedUser);
        }

        public async Task Logout()
        {
            ((CustomAuthStateProvider)_authenticationStateProvider)
                .LogoutUser();

            _cachedLoggedUser = null;
            _isInitialized = true;
            await _localStorageService.RemoveItemAsync("jwt");
        }

        public async Task<string> GetJwtToken()
        {
            await EnsureInitializedAsync();
            return _cachedLoggedUser?.JwtToken ?? string.Empty;
        }
    }
}
