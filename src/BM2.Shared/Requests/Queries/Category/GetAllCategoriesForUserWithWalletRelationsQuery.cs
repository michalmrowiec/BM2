using BM2.Shared.DTOs;

namespace BM2.Shared.Requests.Queries.Category;

public record GetAllCategoriesForUserWithWalletRelationsQuery(Guid UserId) : IBaseRequestCollection<CategoryWithWalletRelationDTO>;