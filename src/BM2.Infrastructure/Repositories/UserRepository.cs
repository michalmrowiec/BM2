using BM2.Application.Contracts.Persistence;
using BM2.Domain.Entities;
using BM2.Domain.Entities.UserProfile;
using BM2.Infrastructure.Repositories.Base;

namespace BM2.Infrastructure.Repositories;

public class UserRepository(
    BM2DbContext context) : GenericRepository<User>(context), IUserRepository
{
    public async Task<User?> GetByEmailAddressAsync(string emailAddress) =>
        await GetByAsync(x => x.EmailAddress == emailAddress);
}