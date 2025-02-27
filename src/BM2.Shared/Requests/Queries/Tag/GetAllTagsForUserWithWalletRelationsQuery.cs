using BM2.Shared.DTOs;

namespace BM2.Shared.Requests.Queries.Tag;

public record GetAllTagsForUserWithWalletRelationsQuery(Guid UserId) : IBaseRequestCollection<TagWalletRelationDTO>;