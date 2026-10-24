using BM2.Application.Responses;
using BM2.Infrastructure.Repositories.Base;
using BM2.Shared.Requests.Commands.Account;
using MediatR;

namespace BM2.Application.Functions.Account.Commands;

public class DeleteAccountCommandHandler(UnitOfWork unitOfWork)
    : IRequestHandler<DeleteAccountCommand, BaseResponse>
{
    public async Task<BaseResponse> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        var account = (await unitOfWork.AccountRepository.GetByIdAsync(request.Id)).EnsureFound();

        account.CheckPermission(request.OwnedByUserId);

        var records = await unitOfWork.RecordRepository.GetListByAsync(x => x.AccountId == account.Id);
        if (records.Any())
        {
            return new BaseResponse(
                BaseResponse.ResponseStatus.BadQuery,
                "This account has records. Move its records to another account before deleting it.");
        }

        account.DeletedAt = DateTime.UtcNow;
        account.DeletedBy = request.OwnedByUserId;

        try
        {
            await unitOfWork.AccountRepository.Update(account);
            await unitOfWork.SaveAsync();

            return request.ReturnSuccess();
        }
        catch (Exception)
        {
            return request.ReturnServerError();
        }
    }
}
