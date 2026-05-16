using BM2.Application.Functions.Account.Commands.Validators;
using BM2.Application.Mappings;
using BM2.Application.Responses;
using BM2.Infrastructure.Repositories.Base;
using BM2.Shared.DTOs;
using BM2.Shared.Requests.Commands.Account;
using MediatR;

namespace BM2.Application.Functions.Account.Commands;

public class UpdateAccountCommandHandler(UnitOfWork unitOfWork)
    : IRequestHandler<UpdateAccountCommand, BaseResponse<AccountDTO>>
{
    public async Task<BaseResponse<AccountDTO>> Handle(UpdateAccountCommand request,
        CancellationToken cancellationToken)
    {
        var validationResult =
            await new BaseAccountCommandValidator(unitOfWork).ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid) return new BaseResponse<AccountDTO>(validationResult);

        var entity = (await unitOfWork.AccountRepository.GetByIdAsync(request.Id)).EnsureFound();

        entity.CheckPermission(request.OwnedByUserId);

        try
        {
            entity.Apply(request);
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = request.OwnedByUserId;

            await unitOfWork.AccountRepository.Update(entity);
            await unitOfWork.SaveAsync();

            return request.ReturnSuccessWithObject(
                entity.ToDto());
        }
        catch (Exception e)
        {
            return request.ReturnServerError();
        }
    }
}
