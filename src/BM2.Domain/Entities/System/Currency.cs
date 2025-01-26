namespace BM2.Domain.Entities;

public class Currency : IEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Symbol { get; set; } = null!;
    public string IsoCode { get; set; } = null!;
    public string Country { get; set; } = null!;

    public ICollection<BaseRecord> Records { get; set; } = [];
    public ICollection<PeriodicRecordDefinition> PeriodicRecordDefinitions { get; set; } = [];
    public ICollection<Wallet> Wallets { get; set; } = [];
    public ICollection<Account> Accounts { get; set; } = [];
}