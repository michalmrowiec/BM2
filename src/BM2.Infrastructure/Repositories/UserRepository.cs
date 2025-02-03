using BM2.Application.Contracts.Persistence;
using BM2.Application.Contracts.Persistence.Base;
using BM2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.Logging;

namespace BM2.Infrastructure.Repositories;

public class UserRepository(
    BM2DbContext context,
    IBaseRepository<User> baseRepository,
    ILogger<UserRepository> logger) : IUserRepository
{
    private readonly IQueryable<User> _users = context.Users.GetUndeleted().AsNoTracking();
    
    public async Task<User?> GetByEmailAddressAsync(string emailAddress) =>
        await baseRepository.GetByAsync(x => x.EmailAddress == emailAddress);

    public async Task<User> AddAsync(User entity)=> await baseRepository.AddAsync(entity);

    public Task DeleteAsync(User entity)
    {
        throw new NotImplementedException();
    }

    public Task<User> UpdateAsync(User entity)
    {
        throw new NotImplementedException();
    }
}