using BM2.Contracts.Common;
using FluentValidation;

namespace BM2.Contracts.Wallets;

public sealed record CreateWalletRequest(string Name, string BaseCurrency);

public sealed record UpdateWalletRequest(string Name, string BaseCurrency);

public sealed class CreateWalletRequestValidator : AbstractValidator<CreateWalletRequest>
{
    public CreateWalletRequestValidator()
    {
        RuleFor(x => x.Name).Name(Limits.WalletName);
        RuleFor(x => x.BaseCurrency).Currency();
    }
}

public sealed class UpdateWalletRequestValidator : AbstractValidator<UpdateWalletRequest>
{
    public UpdateWalletRequestValidator()
    {
        RuleFor(x => x.Name).Name(Limits.WalletName);
        RuleFor(x => x.BaseCurrency).Currency();
    }
}
