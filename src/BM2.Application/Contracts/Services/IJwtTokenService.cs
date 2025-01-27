using BM2.Domain.Entities;

namespace BM2.Application.Contracts.Services;

public interface IJwtTokenService
{
    public string GenerateJwt(User user);
}