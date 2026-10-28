using System.Globalization;
using System.Text.RegularExpressions;

namespace BM2.Client.Localization;

/// <summary>
/// UI text. The English text is both the key and the default; Polish maps it in the <c>Strings.Pl.*.cs</c> files
/// (one dictionary per area). Use <c>@Tr("...")</c> in components (imported statically).
/// </summary>
public static partial class Strings
{
    public const string English = "en-GB";
    public const string Polish = "pl-PL";
    public static readonly string[] Cultures = [English, Polish];

    private static Dictionary<string, string>? _pl;
    private static List<(Regex Pattern, string Text)>? _plPatterns;

    private static Dictionary<string, string> Pl => _pl ??= Merge(
        PlCommon, PlNav, PlHome, PlRecords, PlRecordForm, PlStats, PlMore, PlAccount, PlManage, PlMessages);

    private static bool IsPolish => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "pl";

    /// <summary>Culture for a language code from the &lt;html lang&gt; attribute or a cookie; English by default.</summary>
    public static string CultureFor(string? language) =>
        language?.StartsWith("pl", StringComparison.OrdinalIgnoreCase) == true ? Polish : English;

    public static void Apply(string culture)
    {
        var info = CultureInfo.GetCultureInfo(culture);
        CultureInfo.DefaultThreadCurrentCulture = info;
        CultureInfo.DefaultThreadCurrentUICulture = info;
        CultureInfo.CurrentCulture = info;
        CultureInfo.CurrentUICulture = info;
    }

    public static string Tr(string en) => IsPolish && Pl.TryGetValue(en, out var text) ? text : en;

    public static string Tr(string en, params object?[] args) =>
        string.Format(CultureInfo.CurrentCulture, Tr(en), args);

    /// <summary>
    /// Count text. <paramref name="one"/> and <paramref name="other"/> are English formats with {0}; the Polish entry
    /// of <paramref name="one"/> holds "one|few|many".
    /// </summary>
    public static string Plural(int n, string one, string other)
    {
        if (!IsPolish || !Pl.TryGetValue(one, out var forms))
        {
            return string.Format(CultureInfo.CurrentCulture, n == 1 ? one : other, n);
        }

        var parts = forms.Split('|');
        var form = n == 1 ? 0 : n % 10 is >= 2 and <= 4 && n % 100 is < 12 or > 14 ? 1 : 2;
        return string.Format(CultureInfo.CurrentCulture, parts[Math.Min(form, parts.Length - 1)], n);
    }

    /// <summary>Translates a message from the API or a validator, including ones with numbers filled in ("{0}" keys).</summary>
    public static string TrMessage(string message)
    {
        if (!IsPolish)
        {
            return message;
        }

        if (Pl.TryGetValue(message, out var exact))
        {
            return exact;
        }

        _plPatterns ??= Pl.Where(e => e.Key.Contains('{', StringComparison.Ordinal))
            .Select(e => (new Regex("^" + Placeholder().Replace(Regex.Escape(e.Key), "(?<a$1>.+?)") + "$"), e.Value))
            .ToList();
        foreach (var (pattern, text) in _plPatterns)
        {
            var match = pattern.Match(message);
            if (match.Success)
            {
                // Values may be English field names (DataAnnotations), so they're translated too.
                var args = new object[10];
                for (var i = 0; i < args.Length; i++)
                {
                    args[i] = match.Groups["a" + i] is { Success: true } g ? Tr(g.Value) : "";
                }

                return string.Format(CultureInfo.CurrentCulture, text, args);
            }
        }

        return message;
    }

    [GeneratedRegex(@"\\\{(\d)}")]
    private static partial Regex Placeholder();

    private static Dictionary<string, string> Merge(params Dictionary<string, string>[] parts)
    {
        var all = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var part in parts)
        {
            foreach (var (key, value) in part)
            {
                all.TryAdd(key, value);
            }
        }

        return all;
    }
}
