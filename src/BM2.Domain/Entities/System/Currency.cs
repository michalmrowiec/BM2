namespace BM2.Domain.Entities;

public class Currency : IEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Symbol { get; set; }
    public string IsoCode { get; set; }
    public string Country { get; set; }
}