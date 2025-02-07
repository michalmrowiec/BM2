using BM2.Application.Contracts.Persistence;
using BM2.Domain.Entities;
using BM2.Domain.Entities.UserProfile;

namespace BM2.Infrastructure.Repositories;

public class WalletRepository(
    BM2DbContext context) : GenericRepository<Wallet>(context), IWalletRepository
{
}