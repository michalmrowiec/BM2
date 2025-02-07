using BM2.Domain.Entities;
using BM2.Domain.Entities.UserProfile;

namespace BM2.Application.Contracts.Services;

public interface IJwtTokenService
{
    public string GenerateJwt(User user);
}