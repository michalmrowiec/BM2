using System.Security.Claims;

namespace BM2.Server.Infrastructure;

public interface ICurrentUser
{
    Guid UserId { get; }
}

/// <summary>The authenticated user of the current request. Use only behind authorization.</summary>
public sealed class HttpContextCurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public Guid UserId
    {
        get
        {
            var value = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id)
                ? id
                : throw new InvalidOperationException("No authenticated user with a valid id in the current request.");
        }
    }
}
