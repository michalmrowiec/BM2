using BM2.Application.Functions.Account.Commands.Validators;
using BM2.Application.Mappings;
using BM2.Application.Responses;
using BM2.Infrastructure.Repositories.Base;
using BM2.Shared.DTOs;
using BM2.Shared.Requests.Commands.Account;
using MediatR;

namespace BM2.Application.Functions.Account.Commands;

public class AddAccountCommandHandler(UnitOfWork unitOfWork)
    : IRequestHandler<AddAccountCommand, BaseResponse<AccountDTO>>
{
    public async Task<BaseResponse<AccountDTO>> Handle(AddAccountCommand request, CancellationToken cancellationToken)
    {
        var validationResult =
            await new BaseAccountCommandValidator(unitOfWork).ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid) return new BaseResponse<AccountDTO>(validationResult);

        var entity = request.ToEntity();
        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = request.OwnedByUserId;

        try
        {
            entity = await unitOfWork.AccountRepository.Add(entity);
            await unitOfWork.SaveAsync();

            return request.ReturnSuccessWithObject(entity.ToDto());
        }
        catch (Exception)
        {
            return request.ReturnServerError();
        }
    }
}
