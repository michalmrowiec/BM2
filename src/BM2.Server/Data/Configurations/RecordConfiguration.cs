using BM2.Contracts.Common;
using BM2.Domain.Records;
using BM2.Domain.Recurring;
using BM2.Domain.Tags;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BM2.Server.Data.Configurations;

internal sealed class RecordConfiguration : IEntityTypeConfiguration<Record>
{
    public void Configure(EntityTypeBuilder<Record> builder)
    {
        builder.Property(r => r.Currency).IsCurrencyCode();
        builder.Property(r => r.Name).HasMaxLength(Limits.RecordName);

        // Deletions of accounts and categories are handled in code (multiple cascade paths).
        builder.HasOne(r => r.Account).WithMany().HasForeignKey(r => r.AccountId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(r => r.TransferAccount).WithMany().HasForeignKey(r => r.TransferAccountId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(r => r.Category).WithMany().HasForeignKey(r => r.CategoryId).OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(r => r.Tags).WithOne().HasForeignKey(t => t.RecordId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(r => r.Tags).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasOne<RecurringRule>().WithMany().HasForeignKey(r => r.RecurringRuleId).OnDelete(DeleteBehavior.SetNull);
        builder.HasIndex(r => new { r.RecurringRuleId, r.OccurrenceDate }).IsUnique().HasFilter("[RecurringRuleId] IS NOT NULL");

        builder.HasIndex(r => new { r.AccountId, r.OccurredOn });
        builder.HasIndex(r => r.PairId).HasFilter("[PairId] IS NOT NULL");
        builder.HasIndex(r => new { r.TransferAccountId, r.OccurredOn }).HasFilter("[TransferAccountId] IS NOT NULL");
    }
}

internal sealed class RecordTagConfiguration : IEntityTypeConfiguration<RecordTag>
{
    public void Configure(EntityTypeBuilder<RecordTag> builder)
    {
        builder.HasKey(t => new { t.RecordId, t.TagId });
        builder.HasOne<Tag>().WithMany().HasForeignKey(t => t.TagId).OnDelete(DeleteBehavior.Cascade);
    }
}
