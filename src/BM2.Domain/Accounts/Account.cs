using BM2.Contracts.Accounts;
using BM2.Contracts.Wallets;
using BM2.Domain.Common;

namespace BM2.Domain.Accounts;

public sealed class Account
{
    private readonly List<AccountMember> _members = [];

    private Account()
    {
        Name = "";
        Currency = "";
    }

    public Guid Id { get; private set; }

    public Guid WalletId { get; private set; }

    public string Name { get; private set; }

    public string Currency { get; private set; }

    public decimal OpeningBalance { get; private set; }

    public bool IsArchived { get; private set; }

    public AccountKind Kind { get; private set; } = AccountKind.Cash;

    public DateTimeOffset CreatedAt { get; private set; }

    public IReadOnlyCollection<AccountMember> Members => _members;

    public static Account Create(
        Guid walletId, string name, string currency, decimal openingBalance, DateTimeOffset now, AccountKind kind = AccountKind.Cash) =>
        new()
        {
            WalletId = walletId,
            Name = name.Trim(),
            Currency = currency,
            OpeningBalance = openingBalance,
            Kind = kind,
            CreatedAt = now,
        };

    public Result Update(
        string name, string currency, decimal openingBalance, bool isArchived, bool hasRecords, AccountKind kind = AccountKind.Cash)
    {
        if (currency != Currency && hasRecords)
        {
            return Error.Validation(nameof(Currency), "The currency can't change once the account has records.");
        }

        Name = name.Trim();
        Currency = currency;
        OpeningBalance = openingBalance;
        IsArchived = isArchived;
        Kind = kind;
        return Result.Success;
    }

    /// <summary>Shares the account with the user or raises their role; never lowers it.</summary>
    public Result Grant(Guid userId, MemberRole role, DateTimeOffset now)
    {
        if (role == MemberRole.Owner)
        {
            return OwnerShareError();
        }

        var member = Find(userId);
        if (member is null)
        {
            _members.Add(new AccountMember(userId, role, now));
        }
        else if (role > member.Role)
        {
            member.Role = role;
        }

        return Result.Success;
    }

    public Result ChangeShareRole(Guid userId, MemberRole role)
    {
        if (role == MemberRole.Owner)
        {
            return OwnerShareError();
        }

        var member = Find(userId);
        if (member is null)
        {
            return Error.NotFound();
        }

        member.Role = role;
        return Result.Success;
    }

    public Result RemoveShare(Guid userId)
    {
        var member = Find(userId);
        if (member is null)
        {
            return Error.NotFound();
        }

        _members.Remove(member);
        return Result.Success;
    }

    private AccountMember? Find(Guid userId) => _members.Find(m => m.UserId == userId);

    private static Error OwnerShareError() =>
        Error.Validation("Role", "An account can be shared only with the editor or viewer role.");
}
