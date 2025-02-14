using BM2.Application.Contracts.Persistence.Base;
using BM2.Application.Responses;
using BM2.Domain.Entities.UserProfile;
using FluentValidation;

namespace BM2.Application.Functions.Wallets.Commands.AddWalletCommand;

public class AddWalletCommandValidator : AbstractValidator<AddWalletCommand>
{
    public AddWalletCommandValidator(IUnitOfWork unitOfWork)
    {
        RuleFor(x => x.WalletName)
            .NotEmpty()
            .MaximumLength(Wallet.WalletNameMaxLength);

        RuleFor(x => x)
            .CustomAsync(async (request, context, cancellationToken) =>
            {
                var user = await unitOfWork.UserRepository.GetByIdAsync(request.OwnedByUserId, x => x.Wallets);
                user.ThrowExceptionIfNull();
                
                var maxWallets = user!.MaxWallets;

                if (user.Wallets.Count >= user.MaxWallets)
                {
                    context.AddFailure($"The user has reached the maximum number of wallets ({maxWallets}).");
                }
            });
    }
}