using BM2.Contracts.Categories;
using BM2.Contracts.Records;
using BM2.Contracts.Templates;
using BM2.Contracts.Wallets;
using static BM2.Client.Localization.Strings;

namespace BM2.Client.Components.Records;

public static class TemplateText
{
    public static CategoryItem? CategoryOf(TemplateItem template, WalletSnapshot wallet) =>
        wallet.Categories.FirstOrDefault(c => c.Id == template.CategoryId);

    /// <summary>Templates have no label of their own: the record name, else the category name.</summary>
    public static string Label(TemplateItem template, WalletSnapshot wallet) =>
        template.Name ?? CategoryOf(template, wallet)?.Name ?? TypeLabel(template.Type);

    public static string TypeLabel(RecordType type) => type switch
    {
        RecordType.Income => Tr("Income"),
        RecordType.Transfer => Tr("Transfer"),
        _ => Tr("Expense"),
    };

    public static string StatusLabel(RecordStatus status) => status switch
    {
        RecordStatus.Pending => Tr("Pending"),
        RecordStatus.Planned => Tr("Planned"),
        _ => Tr("Completed"),
    };

    public static string Currency(TemplateItem template, WalletSnapshot wallet) =>
        template.Currency
        ?? wallet.Accounts.FirstOrDefault(a => a.Id == template.AccountId)?.Currency
        ?? wallet.BaseCurrency;

    public static AmountSign Sign(RecordType type) => type switch
    {
        RecordType.Income => AmountSign.Plus,
        RecordType.Expense => AmountSign.Minus,
        _ => AmountSign.None,
    };

    public static SaveTemplateRequest FromRecord(RecordItem record) => new(
        record.Type, record.AccountId, record.CategoryId, record.TransferAccountId, record.Amount, record.Currency, record.Name,
        record.TagIds, IsPinned: true);
}
