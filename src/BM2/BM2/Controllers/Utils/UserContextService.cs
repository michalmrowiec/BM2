using System.Security.Claims;

namespace BM2.Controllers.Utils
{
    public interface IUserContextService
    {
        Guid GetUserId { get; }
        ClaimsPrincipal? User { get; }
    }

    public class UserContextService(IHttpContextAccessor httpContextAccessor) : IUserContextService
    {
        public ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

        private string GetUserIdAsString
        {
            get
            {
                var userId = User?.FindFirstValue(ClaimTypes.NameIdentifier);

                if (userId is null)
                {
                    throw new InvalidOperationException();
                }

                return userId;
            }
        }

        public Guid GetUserId => Guid.Parse(GetUserIdAsString);
    };
}