using BM2.Application.Contracts.Persistence;
using BM2.Domain.Entities.UserProfile;
using BM2.Infrastructure.Repositories.Base;

namespace BM2.Infrastructure.Repositories;

public class CategoryRepository(
    BM2DbContext context) : GenericRepository<Category>(context), ICategoryRepository
{
}