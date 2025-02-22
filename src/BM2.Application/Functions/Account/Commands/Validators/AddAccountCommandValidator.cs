using BM2.Application.Contracts.Persistence.Base;
using BM2.Application.Responses;
using BM2.Shared.Models;
using BM2.Shared.Requests.Commands.Account;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BM2.Application.Functions.Account.Commands.Validators;

public class AddAccountCommandValidator : AbstractValidator<AddAccountCommand>
{
    public AddAccountCommandValidator(IUnitOfWork unitOfWork)
    {
        RuleFor(x => x.AccountName)
            .NotEmpty()
            .MaximumLength(ModelsRequirements.AccountNameMaxLength);

        RuleFor(x => x)
            .CustomAsync(async (request, context, cancellationToken) =>
            {
                var user = await unitOfWork.UserRepository.GetByIdAsync(request.OwnedByUserId,
                    q => q.Include(u => u.Wallets).ThenInclude(w => w.Accounts));
                user.ThrowExceptionIfNull();
                var walletForAccount = user!.Wallets.FirstOrDefault(w => w.Id == request.WalletId);
                walletForAccount.ThrowExceptionIfNull();

                var maxAccountsPerWallet = user.MaxAccountsPerWallet;
                
                if (walletForAccount!.Accounts.Count >= maxAccountsPerWallet)
                {
                    context.AddFailure(
                        $"The user has reached the maximum number of accounts ({maxAccountsPerWallet}) for this wallet ({walletForAccount.WalletName}).");
                }
            });
    }
}