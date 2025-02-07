namespace BM2.Application.DTOs;

public class CurrencyDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Symbol { get; set; } = null!;
    public string IsoCode { get; set; } = null!;
    public string Country { get; set; } = null!;
}