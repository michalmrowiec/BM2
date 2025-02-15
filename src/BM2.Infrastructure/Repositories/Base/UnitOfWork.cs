using BM2.Application.Contracts.Persistence;
using BM2.Application.Contracts.Persistence.Base;

namespace BM2.Infrastructure.Repositories.Base;

public sealed class UnitOfWork(BM2DbContext context) : IUnitOfWork
{
    public async Task SaveAsync()
    {
        await context.SaveChangesAsync();
    }

    public ICurrencyRepository CurrencyRepository { get; } = new CurrencyRepository(context);
    public IRecordStatusRepository RecordStatusRepository { get; } = new RecordStatusRepository(context);
    public IUserRepository UserRepository { get; } = new UserRepository(context);
    public IAuditLoginRepository AuditLoginRepository { get; } = new AuditLoginRepository(context);
    public IWalletRepository WalletRepository { get; } = new WalletRepository(context);
    public IAccountRepository AccountRepository { get; } = new AccountRepository(context);
    public ICategoryRepository CategoryRepository { get; } = new CategoryRepository(context);
    public IWalletCategoryRelationRepository WalletCategoryRelationRepository { get; } = new WalletCategoryRelationRepository(context);
    public ITagRepository TagRepository { get; } = new TagRepository(context);
    public IWalletTagRelationRepository WalletTagRelationRepository { get; } = new WalletTagRelationRepository(context);
    public IRecordRepository RecordRepository { get; } = new RecordRepository(context);

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