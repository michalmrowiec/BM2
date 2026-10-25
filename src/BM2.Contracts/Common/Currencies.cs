namespace BM2.Contracts.Common;

/// <summary>ISO 4217 currencies supported by BM2.</summary>
public static class Currencies
{
    public static IReadOnlyList<string> All { get; } =
        ["PLN", "EUR", "USD", "GBP", "CHF", "CZK", "SEK", "NOK", "DKK", "HUF", "UAH", "JPY", "CAD", "AUD"];

    public static bool IsSupported(string? code) => code is not null && All.Contains(code, StringComparer.Ordinal);
}
