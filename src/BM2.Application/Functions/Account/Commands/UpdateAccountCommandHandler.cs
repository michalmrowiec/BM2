using AutoMapper;
using BM2.Application.Contracts.Persistence.Base;
using BM2.Application.Functions.Account.Commands.Validators;
using BM2.Application.Responses;
using BM2.Shared.DTOs;
using BM2.Shared.Requests.Commands.Account;
using MediatR;

namespace BM2.Application.Functions.Account.Commands;

public class AddUpdateAccountCommandHandler(IMapper mapper, IUnitOfWork unitOfWork)
    : IRequestHandler<AddUpdateAccountCommand, BaseResponse<AccountDTO>>
{
    public async Task<BaseResponse<AccountDTO>> Handle(AddUpdateAccountCommand request,
        CancellationToken cancellationToken)
    {
        var validationResult =
            await new AddUpdateAccountCommandValidator(unitOfWork).ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid) return new BaseResponse<AccountDTO>(validationResult);

        var entity = await unitOfWork.AccountRepository.GetByIdAsync(request.WalletId);

        entity.ThrowExceptionIfNull();
        entity!.CheckPermission(request.OwnedByUserId);

        try
        {
            var updatedEntity = mapper.Map(request, entity);

            updatedEntity!.UpdatedAt = DateTime.UtcNow;
            updatedEntity.UpdatedBy = request.OwnedByUserId;

            updatedEntity = await unitOfWork.AccountRepository.Update(updatedEntity);
            await unitOfWork.SaveAsync();

            return request.ReturnSuccessWithObject(
                mapper.Map<Domain.Entities.UserProfile.Account, AccountDTO>(updatedEntity));
        }
        catch (Exception e)
        {
            return request.ReturnServerError();
        }
    }
}