using BM2.Contracts.Categories;
using BM2.Contracts.Records;
using BM2.Contracts.Templates;
using BM2.Domain.Access;
using BM2.Domain.Common;
using BM2.Server.Data;
using Microsoft.EntityFrameworkCore;

namespace BM2.Server.Features.Templates;

public static class TemplateEndpoints
{
    public static RouteGroupBuilder MapTemplateEndpoints(this RouteGroupBuilder wallet)
    {
        var templates = wallet.MapGroup("/templates");
        ListTemplates.Map(templates);
        CreateTemplate.Map(templates);
        UpdateTemplate.Map(templates);
        DeleteTemplate.Map(templates);
        return wallet;
    }
}

internal static class TemplateRules
{
    /// <summary>Like records: writing needs edit access to an account; named accounts must be writable.</summary>
    public static bool CanWrite(WalletAccess access) => access.ReadableAccountIds.Any(access.CanWriteAccount);

    /// <summary>Ids from the request must belong to the wallet from the route.</summary>
    public static async Task<Error?> CheckAsync(AppDbContext db, WalletAccess access, SaveTemplateRequest request, CancellationToken ct)
    {
        var isTransfer = request.Type == RecordType.Transfer;
        List<Guid> accountIds = [.. new[] { request.AccountId, isTransfer ? request.TransferAccountId : null }.OfType<Guid>().Distinct()];
        var found = accountIds.Count == 0
            ? 0
            : await db.Accounts.CountAsync(a => accountIds.Contains(a.Id) && a.WalletId == access.WalletId, ct);
        if (found != accountIds.Count)
        {
            return Error.NotFound();
        }

        if (!CanWrite(access) || !accountIds.TrueForAll(access.CanWriteAccount))
        {
            return Error.Forbidden();
        }

        if (!isTransfer && request.CategoryId is { } categoryId)
        {
            // Income may use an expense category (refund).
            var anyType = request.Type == RecordType.Income;
            if (!await db.Categories.AnyAsync(
                    c => c.Id == categoryId && c.WalletId == access.WalletId && (anyType || c.Type == CategoryType.Expense), ct))
            {
                return Error.NotFound();
            }
        }

        var tagIds = request.TagIds;
        var tags = tagIds.Count == 0 ? 0 : await db.Tags.CountAsync(t => tagIds.Contains(t.Id) && t.WalletId == access.WalletId, ct);
        return tags == tagIds.Count ? null : Error.NotFound();
    }
}
