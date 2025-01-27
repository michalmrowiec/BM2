using BM2.Domain.Entities;
using BM2.Domain.Models;

namespace BM2.Application.Contracts.Persistence;

public interface IUserRepository
{
    Task<User> CreateAsync(User user);
    Task<User> GetByEmailAddressAsync(string emailAddress);
}