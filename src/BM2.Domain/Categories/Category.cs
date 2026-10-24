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

    public string? Emoji { get; private set; }

    public static Category Create(Guid walletId, string name, CategoryType type, string? emoji = null) =>
        new() { WalletId = walletId, Name = name.Trim(), Type = type, Emoji = Clean(emoji) };

    public void Update(string name, bool isArchived, string? emoji)
    {
        Name = name.Trim();
        IsArchived = isArchived;
        Emoji = Clean(emoji);
    }

    private static string? Clean(string? emoji) => string.IsNullOrWhiteSpace(emoji) ? null : emoji.Trim();
}
