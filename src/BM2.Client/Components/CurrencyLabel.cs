namespace BM2.Client.Components;

// How a currency code is shown next to amounts. "Show PLN as zł" in More (bm2Prefs "pln" = "code" turns it off).
public static class CurrencyLabel
{
    // Set once per browser in Program.cs and by More; the server prerender always uses the default.
    public static bool PlnAsZloty { get; set; } = true;

    public static string Of(string? code) => PlnAsZloty && code == "PLN" ? "zł" : code ?? "";
}
