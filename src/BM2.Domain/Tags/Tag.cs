namespace BM2.Domain.Tags;

/// <summary>Free label across categories; also used for projects such as a house build or one car.</summary>
public sealed class Tag
{
    private Tag()
    {
        Name = "";
    }

    public Guid Id { get; private set; }

    public Guid WalletId { get; private set; }

    public string Name { get; private set; }

    public bool IsArchived { get; private set; }

    public static Tag Create(Guid walletId, string name) => new() { WalletId = walletId, Name = name.Trim() };

    public void Update(string name, bool isArchived)
    {
        Name = name.Trim();
        IsArchived = isArchived;
    }
}
