using BM2.Domain.Budgets;
using BM2.Domain.Categories;
using BM2.Domain.Wallets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BM2.Server.Data.Configurations;

internal sealed class BudgetConfiguration : IEntityTypeConfiguration<Budget>
{
    public void Configure(EntityTypeBuilder<Budget> builder)
    {
        builder.HasIndex(b => new { b.WalletId, b.CategoryId }).IsUnique();
        builder.HasOne<Wallet>().WithMany().HasForeignKey(b => b.WalletId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne<Category>().WithMany().HasForeignKey(b => b.CategoryId).OnDelete(DeleteBehavior.Cascade);
    }
}
