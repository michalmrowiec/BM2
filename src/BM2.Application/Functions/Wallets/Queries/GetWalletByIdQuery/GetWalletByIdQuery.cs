using System.Text.Json.Serialization;
using BM2.Application.DTOs;

namespace BM2.Application.Functions.Wallets.Queries.GetWalletByIdQuery;

public record GetWalletByIdQuery(Guid WalletId, Guid UserId) : IBaseRequest<WalletDTO>;