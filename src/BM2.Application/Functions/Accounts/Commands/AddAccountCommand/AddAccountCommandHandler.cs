using AutoMapper;
using BM2.Application.Contracts.Persistence.Base;
using BM2.Application.DTOs;
using BM2.Application.Responses;
using BM2.Domain.Entities.UserProfile;
using MediatR;

namespace BM2.Application.Functions.Accounts.Commands.AddAccountCommand;

public class AddAccountCommandHandler(IMapper mapper, IUnitOfWork unitOfWork)
    : IRequestHandler<AddAccountCommand, BaseResponse<AccountDTO>>
{
    public async Task<BaseResponse<AccountDTO>> Handle(AddAccountCommand request, CancellationToken cancellationToken)
    {
        var validationResult =
            await new AddAccountCommandValidator(unitOfWork).ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid) return new BaseResponse<AccountDTO>(validationResult);

        var entity = mapper.Map<AddAccountCommand, Account>(request);
        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = request.OwnedByUserId;

        try
        {
            entity = await unitOfWork.AccountRepository.Add(entity);
            await unitOfWork.AccountRepository.Save();

            return request.ReturnSuccessWithObject(mapper.Map<Account, AccountDTO>(entity));
        }
        catch (Exception e)
        {
            return request.ReturnServerError();
        }
    }

    // private async Task<BaseResponse<TEntityDTO>> Handle<TCommand, TEntity, TEntityDTO, TValidator>(
    //     TCommand command,
    //     TValidator validator,
    //     CancellationToken cancellationToken)
    //     where TCommand : IRequest<BaseResponse<TEntityDTO>>, IBaseRequest, IOwnedByUser
    //     where TEntity : class, IEntity, IEntityAudit, IOwnedByUser
    //     where TEntityDTO : class
    // where TValidator : AbstractValidator<TCommand>
    // {
    //     var validationResult =
    //         await validator.ValidateAsync(command, cancellationToken);
    //
    //     if (!validationResult.IsValid) return new BaseResponse<TEntityDTO>(validationResult);
    //
    //     var entity = mapper.Map<TCommand, TEntity>(command);
    //     entity.Id = Guid.NewGuid();
    //     entity.CreatedAt = DateTime.UtcNow;
    //     entity.CreatedBy = command.OwnedByUserId;
    //
    //     try
    //     {
    //         entity = await unitOfWork.GenericRepository<TEntity>().Add(entity);
    //         await unitOfWork.GenericRepository<TEntity>().Save();
    //
    //         return command.ReturnSuccessWithObject(mapper.Map<TEntity, TEntityDTO>(entity));
    //     }
    //     catch (Exception e)
    //     {
    //         return command.ReturnServerError();
    //     }
    // }
}