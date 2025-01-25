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

        ConfigureUsers(modelBuilder);
        ConfigureRecords(modelBuilder);

        // modelBuilder.ApplyConfigurationsFromAssembly(typeof(BaseDocumentConfiguration).Assembly);
    }

    private static void ConfigureUsers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(user =>
        {
            user.HasKey(x => x.Id);
            user.HasMany(x => x.Wallets)
                .WithOne(x => x.OwnedByUser)
                .HasForeignKey(x => x.OwnedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
            user.HasMany(x => x.Accounts)
                .WithOne(x => x.OwnedByUser)
                .HasForeignKey(x => x.OwnedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
            user.HasMany(x => x.Categories)
                .WithOne(x => x.OwnedByUser)
                .HasForeignKey(x => x.OwnedByUserId)
                .OnDelete(DeleteBehavior.Cascade);
            user.HasMany(x => x.Tags)
                .WithOne(x => x.OwnedByUser)
                .HasForeignKey(x => x.OwnedByUserId)
                .OnDelete(DeleteBehavior.Cascade);
            user.HasMany(x => x.Records)
                .WithOne(x => x.OwnedByUser)
                .HasForeignKey(x => x.OwnedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
            user.HasMany(x => x.RecordTemplates)
                .WithOne(x => x.OwnedByUser)
                .HasForeignKey(x => x.OwnedByUserId)
                .OnDelete(DeleteBehavior.Cascade);
            user.HasMany(x => x.RecordTagRelations)
                .WithOne(x => x.OwnedByUser)
                .HasForeignKey(x => x.OwnedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
            user.HasMany(x => x.PeriodicRecordDefinitions)
                .WithOne(x => x.OwnedByUser)
                .HasForeignKey(x => x.OwnedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
            user.HasMany(x => x.AuditLogins)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureRecords(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Record>(record =>
        {
            record.HasKey(x => x.Id);
            record.HasOne<Category>(x => x.Category)
                .WithMany(x => x.Records)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            record.HasMany<Tag>(x => x.Tags)
                .WithMany(x => x.Records)
                .UsingEntity<RecordTagRelation>(
                    r => r.HasOne(x => x.Tag)
                        .WithMany()
                        .HasForeignKey(x => x.TagId),
                    r => r.HasOne(x => x.Record)
                        .WithMany()
                        .HasForeignKey(x => x.RecordId),
                    rtr => { rtr.HasKey(x => x.Id); });
            record.HasOne<Currency>(x => x.Currency)
                .WithMany(x => x.Records)
                .HasForeignKey(x => x.CurrencyId)
                .OnDelete(DeleteBehavior.Restrict);
            record.HasOne<Account>(x => x.Account)
                .WithMany(x => x.Records)
                .HasForeignKey(x => x.AccountId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}