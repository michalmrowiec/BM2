namespace BM2.Application.Contracts.Persistence.Base;

public interface IUnitOfWork : IDisposable
{
    public IUserRepository UserRepository { get; }
    public IAuditLoginRepository AuditLoginRepository { get; }
    public IWalletRepository WalletRepository { get; }
}