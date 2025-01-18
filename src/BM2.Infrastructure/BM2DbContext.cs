using BM2.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BM2.Infrastructure;

public class BM2DbContext : DbContext
{
    public DbSet<RecordStatus> RecordStatuses { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Wallet> Wallets { get; set; }
    
    public BM2DbContext(DbContextOptions<BM2DbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // modelBuilder.ApplyConfigurationsFromAssembly(typeof(BaseDocumentConfiguration).Assembly);
    }
}