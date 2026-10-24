namespace BM2.Server.Features.Members;

public static class MemberEndpoints
{
    public static RouteGroupBuilder MapMemberEndpoints(this RouteGroupBuilder wallet)
    {
        ListMembers.Map(wallet);
        UpdateMemberRole.Map(wallet);
        RemoveMember.Map(wallet);
        UpdateAccountShare.Map(wallet);
        RemoveAccountShare.Map(wallet);
        return wallet;
    }
}
