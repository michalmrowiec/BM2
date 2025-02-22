using BM2.Shared.DTOs;

namespace BM2.Shared.Requests.Queries.Category;

public record GetCategoriesForWalletQuery(Guid UserId, Guid WalletId) : IBaseRequestCollection<CategoryDTO>;