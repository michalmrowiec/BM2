using BM2.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BM2.Infrastructure;

public class BM2DbContext : DbContext
{
    public DbSet<Currency> Currencies { get; set; }
    public DbSet<RecordStatus> RecordStatuses { get; set; }

    public DbSet<User> Users { get; set; }
    public DbSet<AuditLogin> AuditLogins { get; set; }
    public DbSet<Wallet> Wallets { get; set; }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<UserWalletRelation> UserWalletRelations { get; set; }
    public DbSet<WalletCategoryRelation> WalletCategoryRelations { get; set; }
    public DbSet<WalletTagRelation> WalletTagRelations { get; set; }

    public DbSet<Record> Records { get; set; }
    public DbSet<RecordTemplate> RecordTemplates { get; set; }
    public DbSet<RecordTagRelation> RecordTagRelations { get; set; }
    public DbSet<PeriodicRecordDefinition> PeriodicRecordDefinitions { get; set; }

    public BM2DbContext(DbContextOptions<BM2DbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(user =>
        {
            user.HasKey(e => e.Id);
            user.HasMany<Wallet>(x => x.Wallets)
                .WithMany(x => x.Users)
                .UsingEntity<UserWalletRelation>(
                    r => r.HasOne(x => x.Wallet)
                        .WithMany()
                        .HasForeignKey(x => x.WalletId),
                    r => r.HasOne(x => x.User)
                        .WithMany()
                        .HasForeignKey(x => x.UserId),
                    uwr => { uwr.HasKey(x => x.Id); });
        });

        // modelBuilder.ApplyConfigurationsFromAssembly(typeof(BaseDocumentConfiguration).Assembly);
    }
}