using BM2.Application.DTOs;

namespace BM2.Application.Functions.Category.Commands.Requests;

public class AddCategoryCommand : IBaseRequest<CategoryDTO>
{
    public Guid WalletId { get; set; }
    public string CategoryName { get; set; } = null!;
}