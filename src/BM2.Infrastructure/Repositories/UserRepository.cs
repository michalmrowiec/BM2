using BM2.Application.Contracts.Persistence;
using BM2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.Logging;

namespace BM2.Infrastructure.Repositories;

public class UserRepository(
    BM2DbContext context,
    ILogger<UserRepository> logger) : IUserRepository
{
    private readonly IQueryable<User> _users = context.Users.GetUndeleted().AsNoTracking();

    public async Task<User> CreateAsync(User user)
    {
        try
        {
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();
            return user;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating entity");
            throw;
        }
    }

    public async Task<User> GetByEmailAddressAsync(string emailAddress)
    {
        try
        {
            var result = await _users.FirstOrDefaultAsync(x => x.EmailAddress == emailAddress);
            return result ?? throw new KeyNotFoundException("The object with the given id was not found.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving entity with Id: {EntityId}", emailAddress);
            throw;
        }
    }
}