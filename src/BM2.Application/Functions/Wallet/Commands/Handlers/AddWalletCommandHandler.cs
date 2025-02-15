using AutoMapper;
using BM2.Application.Contracts.Persistence.Base;
using BM2.Application.DTOs;
using BM2.Application.Functions.Wallet.Commands.Requests;
using BM2.Application.Functions.Wallet.Commands.Validators;
using BM2.Application.Responses;
using MediatR;

namespace BM2.Application.Functions.Wallet.Commands.Handlers;

public class AddWalletCommandHandler(IMapper mapper, IUnitOfWork unitOfWork)
    : IRequestHandler<AddWalletCommand, BaseResponse<WalletDTO>>
{
    public async Task<BaseResponse<WalletDTO>> Handle(AddWalletCommand request, CancellationToken cancellationToken)
    {
        var validationResult =
            await new AddWalletCommandValidator(unitOfWork).ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid) return new BaseResponse<WalletDTO>(validationResult);

        var wallet = mapper.Map<AddWalletCommand, Domain.Entities.UserProfile.Wallet>(request);
        wallet.Id = Guid.NewGuid();
        wallet.CreatedAt = DateTime.UtcNow;
        wallet.CreatedBy = request.OwnedByUserId;

        try
        {
            wallet = await unitOfWork.WalletRepository.Add(wallet);
            await unitOfWork.SaveAsync();

            return request.ReturnSuccessWithObject(mapper.Map<Domain.Entities.UserProfile.Wallet, WalletDTO>(wallet));
        }
        catch (Exception e)
        {
            return request.ReturnServerError();
        }
    }
}