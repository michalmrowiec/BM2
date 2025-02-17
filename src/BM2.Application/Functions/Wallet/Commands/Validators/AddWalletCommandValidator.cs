using BM2.Application.Contracts.Persistence.Base;
using BM2.Application.Responses;
using BM2.Shared.Requests.Commands.Wallet;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BM2.Application.Functions.Wallet.Commands.Validators;

public class AddWalletCommandValidator : AbstractValidator<AddWalletCommand>
{
    public AddWalletCommandValidator(IUnitOfWork unitOfWork)
    {
        RuleFor(x => x.WalletName)
            .NotEmpty()
            .MaximumLength(Domain.Entities.UserProfile.Wallet.WalletNameMaxLength);

        RuleFor(x => x)
            .CustomAsync(async (request, context, cancellationToken) =>
            {
                var user = await unitOfWork.UserRepository.GetByIdAsync(request.OwnedByUserId,
                    q => q.Include(u => u.Wallets));
                user.ThrowExceptionIfNull();

                var maxWallets = user!.MaxWallets;

                if (user.Wallets.Count >= maxWallets)
                {
                    context.AddFailure($"The user has reached the maximum number of wallets ({maxWallets}).");
                }
            });
    }
}