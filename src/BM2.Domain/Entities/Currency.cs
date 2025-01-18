namespace BM2.Domain.Entities;

public class Currency
{
    public Guid CurrencyId { get; set; }
    public string Name { get; set; }
    public string Symbol { get; set; }
    public string IsoCode { get; set; }
    public string Country { get; set; }
}