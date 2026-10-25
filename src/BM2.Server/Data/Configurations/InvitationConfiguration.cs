using BM2.Domain.Accounts;
using BM2.Domain.Invitations;
using BM2.Domain.Wallets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BM2.Server.Data.Configurations;

internal sealed class InvitationConfiguration : IEntityTypeConfiguration<Invitation>
{
    public void Configure(EntityTypeBuilder<Invitation> builder)
    {
        builder.Property(i => i.TokenHash).HasMaxLength(32).IsFixedLength();
        builder.HasIndex(i => i.TokenHash).IsUnique();
        builder.HasOne<Wallet>().WithMany().HasForeignKey(i => i.WalletId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Account>().WithMany().HasForeignKey(i => i.AccountId).OnDelete(DeleteBehavior.Cascade);
    }
}
