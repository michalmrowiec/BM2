using BM2.Domain.Accounts;
using BM2.Domain.Categories;
using BM2.Domain.Tags;
using BM2.Domain.Wallets;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BM2.Server.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options), IDataProtectionKeyContext
{
    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

    public DbSet<Wallet> Wallets => Set<Wallet>();

    public DbSet<WalletMember> WalletMembers => Set<WalletMember>();

    public DbSet<Account> Accounts => Set<Account>();

    public DbSet<AccountMember> AccountMembers => Set<AccountMember>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Tag> Tags => Set<Tag>();

    // Pinned so the model does not depend on IdentityOptions from DI (schema v3 adds passkeys).
    protected override Version SchemaVersion => IdentitySchemaVersions.Version3;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<decimal>().HavePrecision(19, 4);
    }
}
