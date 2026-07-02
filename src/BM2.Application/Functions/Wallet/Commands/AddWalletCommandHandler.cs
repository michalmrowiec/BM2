using BM2.Application.Mappings;
using BM2.Infrastructure.Repositories.Base;
using BM2.Application.Functions.Wallet.Commands.Validators;
using BM2.Application.Responses;
using BM2.Shared.DTOs;
using BM2.Shared.Requests.Commands.Wallet;
using MediatR;

namespace BM2.Application.Functions.Wallet.Commands;

public class AddWalletCommandHandler(UnitOfWork unitOfWork)
    : IRequestHandler<AddWalletCommand, BaseResponse<WalletDTO>>
{
    public async Task<BaseResponse<WalletDTO>> Handle(AddWalletCommand request, CancellationToken cancellationToken)
    {
        var validationResult =
            await new AddWalletCommandValidator(unitOfWork).ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid) return new BaseResponse<WalletDTO>(validationResult);

        var wallet = request.ToEntity();
        wallet.Id = Guid.NewGuid();
        wallet.CreatedAt = DateTime.UtcNow;
        wallet.CreatedBy = request.OwnedByUserId;

        try
        {
            wallet = await unitOfWork.WalletRepository.Add(wallet);
            await unitOfWork.SaveAsync();

            return request.ReturnSuccessWithObject(wallet.ToDto());
        }
        catch (Exception e)
        {
            return request.ReturnServerError();
        }
    }
}