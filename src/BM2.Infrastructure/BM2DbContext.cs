using BM2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

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

    public DbSet<BaseRecord> BaseRecords { get; set; }
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

        ConfigureCurrencies(modelBuilder);
        ConfigureRecordStatuses(modelBuilder);
        ConfigureUsers(modelBuilder);
        ConfigureAuditLogins(modelBuilder);
        ConfigureWallets(modelBuilder);
        ConfigureAccounts(modelBuilder);
        ConfigureCategories(modelBuilder);
        ConfigureTags(modelBuilder);
        ConfigureRecordTagRelations(modelBuilder);
        ConfigureBaseRecords(modelBuilder);
        ConfigureRecords(modelBuilder);
        ConfigureRecordTemplates(modelBuilder);
        ConfigurePeriodicRecordDefinitions(modelBuilder);

        // modelBuilder.ApplyConfigurationsFromAssembly(typeof(BaseDocumentConfiguration).Assembly);
    }

    private static void ConfigureCurrencies(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Currency>(currencyBuilder =>
        {
            currencyBuilder.HasKey(x => x.Id);
            currencyBuilder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);
            currencyBuilder.Property(x => x.Symbol)
                .IsRequired()
                .HasMaxLength(20);
            currencyBuilder.Property(x => x.IsoCode)
                .IsRequired()
                .HasMaxLength(10);
            currencyBuilder.Property(x => x.Country)
                .IsRequired()
                .HasMaxLength(200);
        });
    }

    private static void ConfigureRecordStatuses(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RecordStatus>(recordStatusBuilder =>
        {
            recordStatusBuilder.HasKey(x => x.Id);
            recordStatusBuilder.Property(x => x.SystemCode)
                .IsRequired();
            recordStatusBuilder.Property(x => x.RecordStatusName)
                .IsRequired()
                .HasMaxLength(200);
            recordStatusBuilder.Property(x => x.ForRecords)
                .IsRequired();
            recordStatusBuilder.Property(x => x.ForPeriodicRecord)
                .IsRequired();
        });
    }

    private static void ConfigureUsers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(userBuilder =>
        {
            userBuilder.HasKey(x => x.Id);
            userBuilder.Property(x => x.EmailAddress)
                .IsRequired()
                .HasMaxLength(255);
            userBuilder.Property(x => x.PasswordHash)
                .HasMaxLength(255);
            userBuilder.Property(x => x.IsActive)
                .IsRequired()
                .HasDefaultValue(true);
            userBuilder.Property(x => x.MaxCategories)
                .IsRequired()
                .HasDefaultValue(20);
            userBuilder.Property(x => x.MaxTags)
                .IsRequired()
                .HasDefaultValue(40);
            userBuilder.Property(x => x.MaxRecordTemplates)
                .IsRequired()
                .HasDefaultValue(10);
            userBuilder.Property(x => x.MaxPeriodicRecordDefinitions)
                .IsRequired()
                .HasDefaultValue(5);
            userBuilder.Property(x => x.MaxRecordsPerMonth)
                .IsRequired()
                .HasDefaultValue(50);
            userBuilder.Property(x => x.MaxWallets)
                .IsRequired()
                .HasDefaultValue(2);
            userBuilder.Property(x => x.MaxAccountsPerWallet)
                .IsRequired()
                .HasDefaultValue(3);
            ConfigureEntityAuditProperties(userBuilder);

            // userBuilder.HasMany(x => x.Wallets)
            //     .WithOne(x => x.OwnedByUser)
            //     .HasForeignKey(x => x.OwnedByUserId)
            //     .OnDelete(DeleteBehavior.Restrict);
            // userBuilder.HasMany(x => x.Accounts)
            //     .WithOne(x => x.OwnedByUser)
            //     .HasForeignKey(x => x.OwnedByUserId)
            //     .OnDelete(DeleteBehavior.Restrict);
            // userBuilder.HasMany(x => x.Categories)
            //     .WithOne(x => x.OwnedByUser)
            //     .HasForeignKey(x => x.OwnedByUserId)
            //     .OnDelete(DeleteBehavior.Cascade);
            // userBuilder.HasMany(x => x.Tags)
            //     .WithOne(x => x.OwnedByUser)
            //     .HasForeignKey(x => x.OwnedByUserId)
            //     .OnDelete(DeleteBehavior.Cascade);
            // userBuilder.HasMany(x => x.Records)
            //     .WithOne(x => x.OwnedByUser)
            //     .HasForeignKey(x => x.OwnedByUserId)
            //     .OnDelete(DeleteBehavior.Restrict);
            // userBuilder.HasMany(x => x.RecordTemplates)
            //     .WithOne(x => x.OwnedByUser)
            //     .HasForeignKey(x => x.OwnedByUserId)
            //     .OnDelete(DeleteBehavior.Cascade);
            // userBuilder.HasMany(x => x.RecordTagRelations)
            //     .WithOne(x => x.OwnedByUser)
            //     .HasForeignKey(x => x.OwnedByUserId)
            //     .OnDelete(DeleteBehavior.Restrict);
            // userBuilder.HasMany(x => x.PeriodicRecordDefinitions)
            //     .WithOne(x => x.OwnedByUser)
            //     .HasForeignKey(x => x.OwnedByUserId)
            //     .OnDelete(DeleteBehavior.Restrict);
            // userBuilder.HasMany(x => x.AuditLogins)
            //     .WithOne(x => x.User)
            //     .HasForeignKey(x => x.UserId)
            //     .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureAuditLogins(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLogin>(auditLoginBuilder =>
        {
            auditLoginBuilder.HasKey(x => x.Id);
            auditLoginBuilder.Property(x => x.UserId)
                .IsRequired();
            auditLoginBuilder.Property(x => x.DateTimeOfLogin)
                .IsRequired()
                .ValueGeneratedOnAdd();

            auditLoginBuilder.HasOne(x => x.User)
                .WithMany(x => x.AuditLogins)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureWallets(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Wallet>(walletBuilder =>
        {
            walletBuilder.HasKey(x => x.Id);
            walletBuilder.Property(x => x.WalletName)
                .IsRequired()
                .HasMaxLength(100);
            walletBuilder.Property(x => x.IsActive)
                .IsRequired()
                .HasDefaultValue(true);
            walletBuilder.Property(x => x.DefaultCurrencyId)
                .IsRequired();
            ConfigureOwnedByUserProperty(walletBuilder);
            ConfigureEntityAuditProperties(walletBuilder);

            // walletBuilder.HasMany<Account>(x => x.Accounts)
            //     .WithOne(x => x.Wallet)
            //     .HasForeignKey(x => x.WalletId)
            //     .OnDelete(DeleteBehavior.Restrict);
            // walletBuilder.HasMany(x => x.Categories)
            //     .WithOne(x => x.Wallet)
            //     .HasForeignKey(x => x.WalletId)
            //     .OnDelete(DeleteBehavior.Restrict);
            // walletBuilder.HasMany(x => x.Tags)
            //     .WithOne(x => x.Wallet)
            //     .HasForeignKey(x => x.WalletId)
            //     .OnDelete(DeleteBehavior.Restrict);
            walletBuilder.HasOne(x => x.OwnedByUser)
                .WithMany(x => x.Wallets)
                .HasForeignKey(x => x.OwnedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
            walletBuilder.HasOne(x => x.DefaultCurrency)
                .WithMany()
                .HasForeignKey(x => x.DefaultCurrencyId);
            // walletBuilder.HasMany(x=>x.PeriodicRecordDefinitions)
            //     .WithOne(x => x.Wallet)
            //     .HasForeignKey(x => x.WalletId)
            //     .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureAccounts(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(accountBuilder =>
        {
            accountBuilder.HasKey(x => x.Id);
            accountBuilder.Property(x => x.WalletId)
                .IsRequired();
            accountBuilder.Property(x => x.AccountName)
                .IsRequired()
                .HasMaxLength(100);
            accountBuilder.Property(x => x.IsActive)
                .IsRequired()
                .HasDefaultValue(true);
            accountBuilder.Property(x => x.DefaultCurrencyId)
                .IsRequired();
            ConfigureOwnedByUserProperty(accountBuilder);
            ConfigureEntityAuditProperties(accountBuilder);

            accountBuilder.HasOne(x => x.Wallet)
                .WithMany(x => x.Accounts)
                .HasForeignKey(x => x.WalletId)
                .OnDelete(DeleteBehavior.Restrict);
            accountBuilder.HasOne(x => x.DefaultCurrency)
                .WithMany()
                .HasForeignKey(x => x.DefaultCurrencyId);
            accountBuilder.HasOne(x => x.OwnedByUser)
                .WithMany(x => x.Accounts)
                .HasForeignKey(x => x.OwnedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
            // accountBuilder.HasMany(x => x.Records)
            //     .WithOne(x => x.Account)
            //     .HasForeignKey(x => x.AccountId)
            //     .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureCategories(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(categoryBuilder =>
        {
            categoryBuilder.HasKey(x => x.Id);
            categoryBuilder.Property(x => x.WalletId)
                .IsRequired();
            categoryBuilder.Property(x => x.CategoryName)
                .IsRequired()
                .HasMaxLength(80);
            ConfigureOwnedByUserProperty(categoryBuilder);
            ConfigureEntityAuditProperties(categoryBuilder);

            categoryBuilder.HasOne(x => x.Wallet)
                .WithMany(x => x.Categories)
                .HasForeignKey(x => x.WalletId)
                .OnDelete(DeleteBehavior.Cascade);
            categoryBuilder.HasOne(x => x.OwnedByUser)
                .WithMany(x => x.Categories)
                .HasForeignKey(x => x.OwnedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
            // categoryBuilder.HasMany(x => x.Records)
            //     .WithOne(x => x.Category)
            //     .HasForeignKey(x => x.CategoryId)
            //     .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureTags(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tag>(tagBuilder =>
        {
            tagBuilder.HasKey(x => x.Id);
            tagBuilder.Property(x => x.WalletId)
                .IsRequired();
            tagBuilder.Property(x => x.TagName)
                .IsRequired()
                .HasMaxLength(150);
            ConfigureOwnedByUserProperty(tagBuilder);
            ConfigureEntityAuditProperties(tagBuilder);

            tagBuilder.HasOne(x => x.Wallet)
                .WithMany(x => x.Tags)
                .HasForeignKey(x => x.WalletId)
                .OnDelete(DeleteBehavior.Cascade);
            tagBuilder.HasOne(x => x.OwnedByUser)
                .WithMany(x => x.Tags)
                .HasForeignKey(x => x.OwnedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
            // tagBuilder.HasMany(x => x.Records)
            //     .WithMany(x => x.Tags)
            //     .UsingEntity<RecordTagRelation>();
        });
    }

    private static void ConfigureRecordTagRelations(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RecordTagRelation>(recordTagRelationBuilder =>
        {
            recordTagRelationBuilder.HasKey(x => new { x.RecordId, x.TagId });
            recordTagRelationBuilder.Property(x => x.RecordId)
                .IsRequired();
            recordTagRelationBuilder.Property(x => x.TagId)
                .IsRequired();
            ConfigureOwnedByUserProperty(recordTagRelationBuilder);

            recordTagRelationBuilder.HasOne(x => x.Record)
                .WithMany()
                .HasForeignKey(x => x.RecordId)
                .OnDelete(DeleteBehavior.Cascade);
            recordTagRelationBuilder.HasOne(x => x.Tag)
                .WithMany()
                .HasForeignKey(x => x.TagId)
                .OnDelete(DeleteBehavior.Restrict);
            recordTagRelationBuilder.HasOne(x => x.OwnedByUser)
                .WithMany()
                .HasForeignKey(x => x.OwnedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureBaseRecords(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BaseRecord>(baseRecordBuilder =>
        {
            baseRecordBuilder.UseTpcMappingStrategy();
            baseRecordBuilder.HasKey(x => x.Id);
            baseRecordBuilder.Property(x => x.CategoryId)
                .IsRequired();
            baseRecordBuilder.Property(x => x.StatusId)
                .IsRequired();
            baseRecordBuilder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(150);
            baseRecordBuilder.Property(x => x.Description)
                .HasMaxLength(500);
            baseRecordBuilder.Property(x => x.Amount)
                .IsRequired()
                .HasDefaultValue(0)
                .HasPrecision(18, 2);
            baseRecordBuilder.Property(x => x.PlannedAmount)
                .HasPrecision(18, 2);
            baseRecordBuilder.Property(x => x.CurrencyId)
                .IsRequired();

            ConfigureOwnedByUserProperty(baseRecordBuilder);
            ConfigureEntityAuditProperties(baseRecordBuilder);

            baseRecordBuilder.HasOne<Category>(x => x.Category)
                .WithMany(x => x.Records)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            // baseRecordBuilder.HasMany(x => x.Tags)
            //     .WithMany(x => x.Records)
            //     .UsingEntity<RecordTagRelation>(
            //         r => r.HasOne(x => x.Tag)
            //             .WithMany()
            //             .HasForeignKey(x => x.TagId),
            //         r => r.HasOne(x => x.Record)
            //             .WithMany()
            //             .HasForeignKey(x => x.RecordId),
            //         rtr => { rtr.HasKey(x => x.Id); });
            baseRecordBuilder.HasMany(x => x.Tags)
                .WithMany(x => x.Records)
                .UsingEntity<RecordTagRelation>();
            baseRecordBuilder.HasOne<Currency>(x => x.Currency)
                .WithMany(x => x.Records)
                .HasForeignKey(x => x.CurrencyId)
                .OnDelete(DeleteBehavior.Restrict);
            baseRecordBuilder.HasOne(x=>x.Status)
                .WithMany()
                .HasForeignKey(x=>x.StatusId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureRecords(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Record>(recordBuilder =>
        {
            recordBuilder.UseTpcMappingStrategy();
            recordBuilder.Property(x => x.AccountId)
                .IsRequired();
            recordBuilder.Property(x => x.RecordDateTime)
                .IsRequired();

            recordBuilder.HasOne<Account>(x => x.Account)
                .WithMany(x => x.Records)
                .HasForeignKey(x => x.AccountId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureRecordTemplates(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RecordTemplate>(recordTemplateBuilder =>
        {
            recordTemplateBuilder.UseTpcMappingStrategy();
            recordTemplateBuilder.Property(x => x.WalletId)
                .IsRequired();

            recordTemplateBuilder.HasOne<Wallet>(x => x.Wallet)
                .WithMany(x => x.RecordTemplates)
                .HasForeignKey(x => x.WalletId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigurePeriodicRecordDefinitions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PeriodicRecordDefinition>(periodicRecordDefinitionBuilder =>
        {
            periodicRecordDefinitionBuilder.HasKey(x => x.Id);
            periodicRecordDefinitionBuilder.Property(x => x.RecordTemplateId)
                .IsRequired();
            periodicRecordDefinitionBuilder.Property(x => x.CurrencyId)
                .IsRequired();
            periodicRecordDefinitionBuilder.Property(x => x.PeriodicRecordStatusId)
                .IsRequired();
            periodicRecordDefinitionBuilder.Property(x => x.SetRecordAccountId)
                .IsRequired();
            periodicRecordDefinitionBuilder.Property(x => x.WalletId)
                .IsRequired();
            periodicRecordDefinitionBuilder.Property(x => x.SetRecordAccountId)
                .IsRequired();
            ConfigureOwnedByUserProperty(periodicRecordDefinitionBuilder);
            ConfigureEntityAuditProperties(periodicRecordDefinitionBuilder);

            periodicRecordDefinitionBuilder.HasOne(x => x.OwnedByUser)
                .WithMany(x => x.PeriodicRecordDefinitions)
                .HasForeignKey(x => x.OwnedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
            periodicRecordDefinitionBuilder.HasOne(x => x.Wallet)
                .WithMany(x => x.PeriodicRecordDefinitions)
                .HasForeignKey(x => x.WalletId)
                .OnDelete(DeleteBehavior.ClientCascade);
            periodicRecordDefinitionBuilder.HasOne(x => x.SetRecordAccount)
                .WithMany(x => x.PeriodicRecordDefinitions)
                .HasForeignKey(x => x.SetRecordAccountId)
                .OnDelete(DeleteBehavior.Restrict);
            periodicRecordDefinitionBuilder.HasOne(x => x.PeriodicRecordStatus)
                .WithMany()
                .HasForeignKey(x => x.PeriodicRecordStatusId)
                .OnDelete(DeleteBehavior.Restrict);
            periodicRecordDefinitionBuilder.HasOne(x => x.SetRecordAccount)
                .WithMany()
                .HasForeignKey(x => x.SetRecordAccountId)
                .OnDelete(DeleteBehavior.Restrict);
            periodicRecordDefinitionBuilder.HasOne(x=>x.RecordTemplate)
                .WithMany(x=>x.PeriodicRecordDefinitions)
                .HasForeignKey(x=>x.RecordTemplateId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureEntityAuditProperties<TEntity>(EntityTypeBuilder<TEntity> entityBuilder)
        where TEntity : class, IEntityAudit
    {
        entityBuilder.Property(x => x.CreatedAt)
            .IsRequired();
        entityBuilder.Property(x => x.CreatedBy)
            .IsRequired();
    }

    private static void ConfigureOwnedByUserProperty<TEntity>(EntityTypeBuilder<TEntity> entityBuilder)
        where TEntity : class, IOwnedByUser
    {
        entityBuilder.Property(x => x.OwnedByUserId)
            .IsRequired();
    }
}