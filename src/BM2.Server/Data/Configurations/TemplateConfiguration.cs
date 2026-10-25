using BM2.Contracts.Common;
using BM2.Domain.Tags;
using BM2.Domain.Templates;
using BM2.Domain.Wallets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BM2.Server.Data.Configurations;

// Account and category ids have no foreign keys: missing or archived ones read as empty (see ListTemplates).
internal sealed class TemplateConfiguration : IEntityTypeConfiguration<Template>
{
    public void Configure(EntityTypeBuilder<Template> builder)
    {
        builder.Property(t => t.Currency).HasMaxLength(3).IsUnicode(false).IsFixedLength();
        builder.Property(t => t.Name).HasMaxLength(Limits.RecordName);
        builder.HasOne<Wallet>().WithMany().HasForeignKey(t => t.WalletId).OnDelete(DeleteBehavior.NoAction);
        builder.HasMany(t => t.Tags).WithOne().HasForeignKey(t => t.TemplateId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(t => t.Tags).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class TemplateTagConfiguration : IEntityTypeConfiguration<TemplateTag>
{
    public void Configure(EntityTypeBuilder<TemplateTag> builder)
    {
        builder.HasKey(t => new { t.TemplateId, t.TagId });
        builder.HasOne<Tag>().WithMany().HasForeignKey(t => t.TagId).OnDelete(DeleteBehavior.Cascade);
    }
}
