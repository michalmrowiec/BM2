using BM2.Application.Mappings;
using BM2.Infrastructure.Repositories.Base;
using BM2.Application.Functions.Category.Commands.Validators;
using BM2.Application.Responses;
using BM2.Domain.Entities.UserProfile;
using BM2.Shared.DTOs;
using BM2.Shared.Requests.Commands.Category;
using MediatR;

namespace BM2.Application.Functions.Category.Commands;

public class AddCategoryCommandHandler(UnitOfWork unitOfWork)
    : IRequestHandler<AddCategoryCommand, BaseResponse<CategoryDTO>>
{
    public async Task<BaseResponse<CategoryDTO>> Handle(AddCategoryCommand request, CancellationToken cancellationToken)
    {
        var validationResult =
            await new AddCategoryCommandValidator(unitOfWork).ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid) return new BaseResponse<CategoryDTO>(validationResult);

        var category = request.ToEntity();
        category.Id = Guid.NewGuid();
        category.CreatedAt = DateTime.UtcNow;
        category.CreatedBy = request.OwnedByUserId;

        List<WalletCategoryRelation> walletCategoryRelations = new List<WalletCategoryRelation>();

        foreach (var walletId in request.WalletIds.Distinct())
        {
            walletCategoryRelations.Add(WalletCategoryRelation.CreateInstance(walletId, category.Id, request.OwnedByUserId));
        }
        
        try
        {
            category = await unitOfWork.CategoryRepository.Add(category);
            await unitOfWork.WalletCategoryRelationRepository.AddRange(walletCategoryRelations);
            await unitOfWork.SaveAsync();

            return request.ReturnSuccessWithObject(category.ToDto());
        }
        catch (Exception e)
        {
            return request.ReturnServerError();
        }
    }
}