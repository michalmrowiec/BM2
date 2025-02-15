namespace BM2.Application.Contracts.Persistence.Base;

public interface IUnitOfWork : IDisposable
{
    Task SaveAsync();
    public ICurrencyRepository CurrencyRepository { get; }
    public IRecordStatusRepository RecordStatusRepository { get; }
    public IUserRepository UserRepository { get; }
    public IAuditLoginRepository AuditLoginRepository { get; }
    public IWalletRepository WalletRepository { get; }
    public IAccountRepository AccountRepository { get; }
    public ICategoryRepository CategoryRepository { get; }
    public IWalletCategoryRelationRepository WalletCategoryRelationRepository { get; }
    public ITagRepository TagRepository { get; }
    public IWalletTagRelationRepository WalletTagRelationRepository { get; }
}