using BM2.Application.Contracts.Persistence;
using BM2.Domain.Entities;
using BM2.Domain.Models;

namespace BM2.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    public Task<User> CreateAsync(User user)
    {
        throw new NotImplementedException();
    }

    public Task<User> GetByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }
}