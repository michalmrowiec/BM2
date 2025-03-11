using AutoMapper;
using BM2.Application.Contracts.Persistence.Base;
using BM2.Application.Functions.Wallet.Commands.Validators;
using BM2.Application.Responses;
using BM2.Shared.DTOs;
using BM2.Shared.Requests.Commands.Wallet;
using MediatR;

namespace BM2.Application.Functions.Wallet.Commands;

public class UpdateWalletCommandHandler(IMapper mapper, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateWalletCommand, BaseResponse<WalletDTO>>
{
    public async Task<BaseResponse<WalletDTO>> Handle(UpdateWalletCommand request, CancellationToken cancellationToken)
    {
        var validationResult =
            await new AddWalletCommandValidator(unitOfWork, false).ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid) return new BaseResponse<WalletDTO>(validationResult);

        var wallet = await unitOfWork.WalletRepository.GetByIdAsync(request.Id);
        
        wallet.ThrowExceptionIfNull();
        wallet!.CheckPermission(request.OwnedByUserId);
        
        mapper.Map(request, wallet);
        wallet!.UpdatedAt = DateTime.UtcNow;
        wallet.UpdatedBy = request.OwnedByUserId;

        try
        {
            wallet = await unitOfWork.WalletRepository.Update(wallet);
            await unitOfWork.SaveAsync();

            return request.ReturnSuccessWithObject(mapper.Map<Domain.Entities.UserProfile.Wallet, WalletDTO>(wallet));
        }
        catch (Exception e)
        {
            return request.ReturnServerError();
        }
    }
}