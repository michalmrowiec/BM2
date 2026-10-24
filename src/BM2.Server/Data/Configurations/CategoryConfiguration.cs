using BM2.Contracts.Common;
using BM2.Domain.Categories;
using BM2.Domain.Tags;
using BM2.Domain.Wallets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BM2.Server.Data.Configurations;

internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.Property(c => c.Name).HasMaxLength(Limits.CategoryName);
        builder.Property(c => c.Icon).HasMaxLength(Limits.CategoryIcon);
        builder.HasIndex(c => new { c.WalletId, c.Type, c.Name }).IsUnique();
        builder.HasOne<Wallet>().WithMany().HasForeignKey(c => c.WalletId).OnDelete(DeleteBehavior.NoAction);
    }
}

internal sealed class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.Property(t => t.Name).HasMaxLength(Limits.TagName);
        builder.Property(t => t.Color).HasMaxLength(Limits.TagColor);
        builder.HasIndex(t => new { t.WalletId, t.Name }).IsUnique();
        builder.HasOne<Wallet>().WithMany().HasForeignKey(t => t.WalletId).OnDelete(DeleteBehavior.NoAction);
    }
}
