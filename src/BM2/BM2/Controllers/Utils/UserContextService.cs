using System.Security.Claims;

namespace BM2.Controllers.Utils
{
    public interface IUserContextService
    {
        string? GetUserId { get; }
        ClaimsPrincipal? User { get; }
    }

    public class UserContextService : IUserContextService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserContextService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

        public string? GetUserId => User?.FindFirstValue(ClaimTypes.NameIdentifier) is null
            ? null
            : User!.FindFirstValue(ClaimTypes.NameIdentifier!);
    };
}