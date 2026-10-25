using BM2.Contracts.Stats;
using BM2.Domain.Stats;
using BM2.Domain.Wallets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BM2.Server.Data.Configurations;

internal sealed class PinnedChartConfiguration : IEntityTypeConfiguration<PinnedChart>
{
    public void Configure(EntityTypeBuilder<PinnedChart> builder)
    {
        builder.Property(p => p.Title).HasMaxLength(100);
        builder.Property(p => p.Query).HasMaxLength(PinChartRequestValidator.MaxQuery);
        builder.HasIndex(p => new { p.WalletId, p.UserId, p.SortOrder });
        builder.HasOne<Wallet>().WithMany().HasForeignKey(p => p.WalletId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
