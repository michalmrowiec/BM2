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
        
        ConfigureRecords(modelBuilder);

        // modelBuilder.ApplyConfigurationsFromAssembly(typeof(BaseDocumentConfiguration).Assembly);
    }

    private static void ConfigureRecords(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Record>(tag =>
        {
            tag.HasKey(x => x.Id);
            tag.HasMany(x => x.Tags)
                .WithMany(x => x.Records)
                .UsingEntity<RecordTagRelation>(
                    r => r.HasOne(x => x.Tag)
                        .WithMany()
                        .HasForeignKey(x => x.TagId),
                    r => r.HasOne(x => x.Record)
                        .WithMany()
                        .HasForeignKey(x => x.RecordId),
                    rtr => { rtr.HasKey(x => x.Id); });
        });
    }
}