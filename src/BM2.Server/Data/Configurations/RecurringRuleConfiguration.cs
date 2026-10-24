using BM2.Contracts.Common;
using BM2.Domain.Recurring;
using BM2.Domain.Tags;
using BM2.Domain.Wallets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BM2.Server.Data.Configurations;

// Account and category ids have no foreign keys: occurrences with missing or archived ones are skipped by catch-up.
internal sealed class RecurringRuleConfiguration : IEntityTypeConfiguration<RecurringRule>
{
    public void Configure(EntityTypeBuilder<RecurringRule> builder)
    {
        builder.Property(r => r.Currency).IsCurrencyCode();
        builder.Property(r => r.Name).HasMaxLength(Limits.RecordName);
        builder.Ignore(r => r.Schedule);
        builder.HasOne<Wallet>().WithMany().HasForeignKey(r => r.WalletId).OnDelete(DeleteBehavior.NoAction);
        builder.HasMany(r => r.Tags).WithOne().HasForeignKey(t => t.RecurringRuleId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(r => r.Tags).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasIndex(r => new { r.WalletId, r.NextDate });
    }
}

internal sealed class RecurringRuleTagConfiguration : IEntityTypeConfiguration<RecurringRuleTag>
{
    public void Configure(EntityTypeBuilder<RecurringRuleTag> builder)
    {
        builder.HasKey(t => new { t.RecurringRuleId, t.TagId });
        builder.HasOne<Tag>().WithMany().HasForeignKey(t => t.TagId).OnDelete(DeleteBehavior.Cascade);
    }
}
