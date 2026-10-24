using System.Globalization;

namespace BM2.Client.Components.Forms;

/// <summary>Parses amounts typed on phones, where the keyboard may offer "." or "," regardless of culture.</summary>
public static class AmountParser
{
    public static bool TryParse(string? text, CultureInfo culture, out decimal? value)
    {
        value = null;
        if (string.IsNullOrWhiteSpace(text))
        {
            return true;
        }

        var cleaned = new string(text.Where(c => char.IsAsciiDigit(c) || c is '.' or ',' or '-').ToArray());
        var separator = cleaned.LastIndexOfAny(['.', ',']);
        var normalized = separator < 0 || IsGroupSeparator(cleaned, separator, culture)
            ? cleaned.Replace(".", "", StringComparison.Ordinal).Replace(",", "", StringComparison.Ordinal)
            : cleaned[..separator].Replace(".", "", StringComparison.Ordinal).Replace(",", "", StringComparison.Ordinal)
                + "." + cleaned[(separator + 1)..];

        if (!decimal.TryParse(normalized, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture, out var parsed))
        {
            return false;
        }

        value = parsed;
        return true;
    }

    // "1,234" in en-US is a thousand; "12,50" is still read as 12.50 because it has only two digits after the comma.
    private static bool IsGroupSeparator(string text, int index, CultureInfo culture) =>
        culture.NumberFormat.NumberGroupSeparator == text[index].ToString()
        && culture.NumberFormat.NumberDecimalSeparator != text[index].ToString()
        && text.Length - index - 1 == 3;
}
