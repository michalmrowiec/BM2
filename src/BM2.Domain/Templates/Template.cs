using BM2.Contracts.Records;
using BM2.Contracts.Templates;

namespace BM2.Domain.Templates;

/// <summary>A saved starting point for a record; the label is the name or else the category name.</summary>
public sealed class Template
{
    private readonly List<TemplateTag> _tags = [];

    private Template()
    {
    }

    public Guid Id { get; private set; }

    public Guid WalletId { get; private set; }

    public RecordType Type { get; private set; }

    public Guid? AccountId { get; private set; }

    public Guid? CategoryId { get; private set; }

    public Guid? TransferAccountId { get; private set; }

    public decimal? Amount { get; private set; }

    public string? Currency { get; private set; }

    public string? Name { get; private set; }

    public bool IsPinned { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public IReadOnlyCollection<TemplateTag> Tags => _tags;

    public static Template Create(Guid walletId, SaveTemplateRequest request, DateTimeOffset now)
    {
        var template = new Template { WalletId = walletId, CreatedAt = now };
        template.Update(request);
        return template;
    }

    public void Update(SaveTemplateRequest request)
    {
        var isTransfer = request.Type == RecordType.Transfer;
        Type = request.Type;
        AccountId = request.AccountId;
        CategoryId = isTransfer ? null : request.CategoryId;
        TransferAccountId = isTransfer ? request.TransferAccountId : null;
        Amount = request.Amount;
        Currency = request.Currency;
        Name = string.IsNullOrWhiteSpace(request.Name) ? null : request.Name.Trim();
        IsPinned = request.IsPinned;

        var wanted = request.TagIds.ToHashSet();
        _tags.RemoveAll(t => !wanted.Contains(t.TagId));
        foreach (var tagId in wanted.Where(id => _tags.TrueForAll(t => t.TagId != id)))
        {
            _tags.Add(new TemplateTag(tagId));
        }
    }
}

public sealed class TemplateTag
{
    private TemplateTag()
    {
    }

    internal TemplateTag(Guid tagId) => TagId = tagId;

    public Guid TemplateId { get; private set; }

    public Guid TagId { get; private set; }
}
