using BM2.Shared.DTOs;

namespace BM2.Shared.Requests.Queries.Tag;

public record GetActiveTagsForWalletQuery(Guid UserId, Guid WalletId) : IBaseRequestCollection<TagDTO>;