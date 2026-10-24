using BM2.Contracts.Wallets;
using BM2.Domain.Common;

namespace BM2.Domain.Wallets;

public sealed class Wallet
{
    private readonly List<WalletMember> _members = [];

    private Wallet()
    {
        Name = "";
        BaseCurrency = "";
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public string BaseCurrency { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public IReadOnlyCollection<WalletMember> Members => _members;

    public static Wallet Create(string name, string baseCurrency, Guid ownerId, DateTimeOffset now)
    {
        var wallet = new Wallet { CreatedAt = now };
        wallet.Update(name, baseCurrency);
        wallet._members.Add(new WalletMember(ownerId, MemberRole.Owner, now));
        return wallet;
    }

    public void Update(string name, string baseCurrency)
    {
        Name = name.Trim();
        BaseCurrency = baseCurrency;
    }

    /// <summary>Adds the user or raises their role; never lowers it.</summary>
    public void Grant(Guid userId, MemberRole role, DateTimeOffset now)
    {
        var member = Find(userId);
        if (member is null)
        {
            _members.Add(new WalletMember(userId, role, now));
        }
        else if (role > member.Role)
        {
            member.Role = role;
        }
    }

    public Result ChangeMemberRole(Guid userId, MemberRole role)
    {
        var member = Find(userId);
        if (member is null)
        {
            return Error.NotFound();
        }

        if (role != MemberRole.Owner && IsLastOwner(member))
        {
            return LastOwnerError();
        }

        member.Role = role;
        return Result.Success;
    }

    public Result RemoveMember(Guid userId)
    {
        var member = Find(userId);
        if (member is null)
        {
            return Error.NotFound();
        }

        if (IsLastOwner(member))
        {
            return LastOwnerError();
        }

        _members.Remove(member);
        return Result.Success;
    }

    private WalletMember? Find(Guid userId) => _members.Find(m => m.UserId == userId);

    private bool IsLastOwner(WalletMember member) =>
        member.Role == MemberRole.Owner && _members.Count(m => m.Role == MemberRole.Owner) == 1;

    private static Error LastOwnerError() => Error.Conflict("A wallet needs at least one owner.", "Role");
}
