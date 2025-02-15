namespace BM2.Application.Contracts.Persistence.Base;

public interface IUnitOfWork : IDisposable
{
    public ICurrencyRepository CurrencyRepository { get; }
    public IRecordStatusRepository RecordStatusRepository { get; }
    public IUserRepository UserRepository { get; }
    public IAuditLoginRepository AuditLoginRepository { get; }
    public IWalletRepository WalletRepository { get; }
    public IAccountRepository AccountRepository { get; }
}