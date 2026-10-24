using BM2.Contracts.Common;
using BM2.Domain.Wallets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BM2.Server.Data.Configurations;

internal sealed class WalletConfiguration : IEntityTypeConfiguration<Wallet>
{
    public void Configure(EntityTypeBuilder<Wallet> builder)
    {
        builder.Property(w => w.Name).HasMaxLength(Limits.WalletName);
        builder.Property(w => w.BaseCurrency).IsCurrencyCode();

        builder.HasMany(w => w.Members).WithOne().HasForeignKey(m => m.WalletId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(w => w.Members).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class WalletMemberConfiguration : IEntityTypeConfiguration<WalletMember>
{
    public void Configure(EntityTypeBuilder<WalletMember> builder)
    {
        builder.HasKey(m => new { m.WalletId, m.UserId });
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(m => m.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
