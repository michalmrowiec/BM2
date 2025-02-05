using BM2.Application.Contracts.Persistence;
using BM2.Application.Contracts.Persistence.Base;

namespace BM2.Infrastructure.Repositories.Base;

public class UnitOfWork(BM2DbContext context) : IUnitOfWork
{
    public IUserRepository UserRepository { get; } = new UserRepository(context);

    public IAuditLoginRepository AuditLoginRepository { get; } = new AuditLoginRepository(context);
    public IWalletRepository WalletRepository { get; } = new WalletRepository(context);

    private bool _disposed = false;

    protected virtual void Dispose(bool disposing)
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