namespace BM2.Client.Components;

// How a currency code is shown next to amounts. "Currency symbols" in Settings (bm2Prefs "pln" = "code" turns it off)
// shows the common symbol instead of the ISO code; codes without a well-known symbol stay as they are.
public static class CurrencyLabel
{
    // Set once per browser in Program.cs and by More; the server prerender always uses the default.
    public static bool PlnAsZloty { get; set; } = true;

    private static readonly Dictionary<string, string> Symbols = new()
    {
        ["PLN"] = "zł", ["EUR"] = "€", ["USD"] = "$", ["GBP"] = "£", ["CHF"] = "CHF", ["JPY"] = "¥",
    };

    public static string Of(string? code) =>
        PlnAsZloty && code is not null && Symbols.TryGetValue(code, out var symbol) ? symbol : code ?? "";
}
