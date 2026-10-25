using BM2.Contracts.Common;
using BM2.Domain.Accounts;
using BM2.Domain.Wallets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BM2.Server.Data.Configurations;

internal sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.Property(a => a.Name).HasMaxLength(Limits.AccountName);
        builder.Property(a => a.Currency).IsCurrencyCode();
        builder.HasIndex(a => new { a.WalletId, a.Name }).IsUnique();

        // No cascade: wallet deletion removes accounts in code (SQL Server rejects multiple cascade paths).
        builder.HasOne<Wallet>().WithMany().HasForeignKey(a => a.WalletId).OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(a => a.Members).WithOne().HasForeignKey(m => m.AccountId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(a => a.Members).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class AccountMemberConfiguration : IEntityTypeConfiguration<AccountMember>
{
    public void Configure(EntityTypeBuilder<AccountMember> builder)
    {
        builder.HasKey(m => new { m.AccountId, m.UserId });
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(m => m.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
