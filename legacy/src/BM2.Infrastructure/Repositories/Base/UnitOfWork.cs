using BM2.Domain.Entities.System;
using BM2.Domain.Entities.UserProfile;
using BM2.Domain.Entities.UserRecords;

namespace BM2.Infrastructure.Repositories.Base;

public sealed class UnitOfWork(BM2DbContext context) : IDisposable
{
    public async Task SaveAsync()
    {
        await context.SaveChangesAsync();
    }

    public GenericRepository<Currency> CurrencyRepository { get; } = new(context);
    public RecordStatusRepository RecordStatusRepository { get; } = new(context);
    public UserRepository UserRepository { get; } = new(context);
    public GenericRepository<AuditLogin> AuditLoginRepository { get; } = new(context);
    public GenericRepository<Wallet> WalletRepository { get; } = new(context);
    public AccountRepository AccountRepository { get; } = new(context);
    public CategoryRepository CategoryRepository { get; } = new(context);
    public WalletCategoryRelationRepository WalletCategoryRelationRepository { get; } = new(context);
    public TagRepository TagRepository { get; } = new(context);
    public WalletTagRelationRepository WalletTagRelationRepository { get; } = new(context);
    public RecordRepository RecordRepository { get; } = new(context);
    public GenericRepository<RecordTemplate> RecordTemplateRepository { get; } = new(context);
    public GenericRepository<PeriodicRecordDefinition> PeriodicRecordDefinitionRepository { get; } = new(context);
    public GenericRepository<RecordTagRelation> RecordTagRelationRepository { get; } = new(context);
    public GenericRepository<AccountRecordTransfer> AccountRecordTransferRepository { get; } = new(context);
    
    private bool _disposed = false;

    private void Dispose(bool disposing)
    {
        if (!this._disposed)
        {
            if (disposing)
            {
                context.Dispose();
            }
        }

        this._disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
