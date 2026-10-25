using BM2.Contracts.Categories;

namespace BM2.Domain.Categories;

public sealed class Category
{
    private Category()
    {
        Name = "";
    }

    public Guid Id { get; private set; }

    public Guid WalletId { get; private set; }

    public string Name { get; private set; }

    public CategoryType Type { get; private set; }

    public bool IsArchived { get; private set; }

    /// <summary>Key from <see cref="CategoryIcons"/>; null shows the first letter.</summary>
    public string? Icon { get; private set; }

    public static Category Create(Guid walletId, string name, CategoryType type, string? icon = null) =>
        new() { WalletId = walletId, Name = name.Trim(), Type = type, Icon = icon };

    public void Update(string name, bool isArchived, string? icon)
    {
        Name = name.Trim();
        IsArchived = isArchived;
        Icon = icon;
    }
}
