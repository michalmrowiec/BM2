using BM2.Application.Contracts.Persistence.Base;
using BM2.Domain.Entities;
using BM2.Domain.Entities.UserProfile;
using BM2.Domain.Models;

namespace BM2.Application.Contracts.Persistence;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAddressAsync(string emailAddress);
}