using BM2.Domain.Entities.Interfaces;
using BM2.Domain.Entities.UserProfile;
using BM2.Domain.Entities.UserRecords;

namespace BM2.Domain.Entities.System;

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

    private Currency(string name, string symbol, string isoCode, string country)
    {
        Id = Guid.NewGuid();
        Name = name;
        Symbol = symbol;
        IsoCode = isoCode;
        Country = country;
    }

    public static Currency CreateInstance(string name, string symbol, string isoCode, string country)
    {
        return new Currency(name, symbol, isoCode, country);
    }
}