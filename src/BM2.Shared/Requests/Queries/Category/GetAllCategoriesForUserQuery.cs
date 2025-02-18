using BM2.Shared.DTOs;

namespace BM2.Shared.Requests.Queries.Category;

public record GetAllCategoriesForUserQuery(Guid UserId) : IBaseRequestCollection<CategoryDTO>;