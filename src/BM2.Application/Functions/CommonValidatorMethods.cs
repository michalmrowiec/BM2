using BM2.Application.Contracts.Persistence.Base;
using BM2.Application.Responses;
using BM2.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace BM2.Application.Functions;

public static class CommonValidatorMethods
{
    /// <summary>
    /// Checks if the user owns all the provided wallets. 
    /// If at least one wallet is not owned, throws DomainExceptions.UnauthenticatedException.
    /// </summary>
    /// <param name="walletIds">List of wallet IDs that need to be checked.</param>
    /// <param name="userId">ID of the user to validate ownership.</param>
    /// <param name="unitOfWork">Unit of Work instance for database operations.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <exception cref="DomainExceptions.UnauthenticatedException">Thrown when the user does not own all the wallets.</exception>
    public static async Task ValidateAllWalletsBelongToUser(this IEnumerable<Guid> walletIds, Guid userId, IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var user = await unitOfWork.UserRepository.GetByIdAsync(userId,
            q => q.Include(u => u.Wallets));
        user.ThrowExceptionIfNull();

        var userWallets = user!.Wallets.Select(x => x.Id);

        var unauthorizedWallets = walletIds.Where(x => !userWallets.Contains(x)).ToList();

        if (unauthorizedWallets.Any())
        {
            throw new DomainExceptions.UnauthenticatedException(
                $"User {userId} does not have access to the following wallets: {string.Join(", ", unauthorizedWallets)}");
        }
    }
}