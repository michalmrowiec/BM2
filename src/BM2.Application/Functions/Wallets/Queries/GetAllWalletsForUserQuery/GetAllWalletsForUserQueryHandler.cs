﻿using AutoMapper;
using BM2.Application.Contracts.Persistence.Base;
using BM2.Application.DTOs;
using BM2.Application.Responses;
using MediatR;

namespace BM2.Application.Functions.Wallets.Queries.GetAllWalletsForUserQuery;

public class GetAllWalletsForUserQueryHandler(IMapper mapper, IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllWalletsForUserQuery, BaseResponse<IEnumerable<WalletDTO>>>
{
    public async Task<BaseResponse<IEnumerable<WalletDTO>>> Handle(GetAllWalletsForUserQuery request,
        CancellationToken cancellationToken)
    {
        var wallet = await unitOfWork.WalletRepository.GetAllForUserAsync(request.UserId);

        wallet.ThrowExceptionIfNull();
        wallet!.CheckPermission(request.UserId);

        return request.ReturnSuccessWithObject(mapper.Map<IEnumerable<WalletDTO>>(wallet));
    }
}