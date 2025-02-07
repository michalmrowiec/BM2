using BM2.Domain.Entities;
using BM2.Domain.Entities.UserProfile;
using FluentValidation;

namespace BM2.Application.Functions.Wallets.Commands.AddWalletCommand;

public class AddWalletCommandValidator : AbstractValidator<AddWalletCommand>
{
    public AddWalletCommandValidator()
    {
        RuleFor(x => x.WalletName)
            .NotEmpty()
            .MaximumLength(Wallet.WalletNameMaxLength);
    }
}