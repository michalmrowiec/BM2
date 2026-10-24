using BM2.Contracts.Records;
using BM2.Domain.Access;
using BM2.Domain.Common;
using BM2.Domain.Records;

namespace BM2.Server.Features.Records;

public static class RecordEndpoints
{
    public static RouteGroupBuilder MapRecordEndpoints(this RouteGroupBuilder wallet)
    {
        var records = wallet.MapGroup("/records");
        ListRecords.Map(records);
        GetRecordSummary.Map(records);
        GetRecordSuggestions.Map(records);
        GetRecord.Map(records);
        CreateRecord.Map(records);
        UpdateRecord.Map(records);
        DeleteRecord.Map(records);
        return wallet;
    }
}

internal static class RecordAccess
{
    /// <summary>
    /// Accounts outside the wallet look missing (404); accounts in the wallet the user may not write give 403.
    /// Category and tag checks are left to the domain.
    /// </summary>
    public static Error? CheckWrite(WalletAccess access, RecordDraft draft, RecordContext context)
    {
        var targetId = draft.Type == RecordType.Transfer ? draft.TransferAccountId : null;
        if (!InWallet(context.Account, access) || (targetId is not null && !InWallet(context.TransferAccount, access)))
        {
            return Error.NotFound();
        }

        return access.CanWriteRecord(draft.AccountId, targetId) ? null : Error.Forbidden();
    }

    private static bool InWallet(AccountRef? account, WalletAccess access) => account?.WalletId == access.WalletId;
}
