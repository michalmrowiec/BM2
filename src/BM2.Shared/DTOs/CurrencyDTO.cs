namespace BM2.Shared.DTOs;

public class CurrencyDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Symbol { get; set; } = null!;
    public string IsoCode { get; set; } = null!;
    public string Country { get; set; } = null!;

    public override string ToString()
    {
        return string.Concat($"[{this.IsoCode}] {this.Name}");
    }
}