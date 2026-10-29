using System.Globalization;
using System.Text.RegularExpressions;

namespace BM2.Client.Localization;

/// <summary>
/// UI text. The English text is both the key and the default; other languages map it in <c>Strings.&lt;Lang&gt;.*.cs</c>
/// (one dictionary per area). Use <c>@Tr("...")</c> in components (imported statically).
/// A plain dictionary is used instead of .resx: no satellite assemblies to download and an O(1) lookup in WebAssembly.
/// </summary>
public static partial class Strings
{
    public const string English = "en-GB";

    /// <summary>Supported languages, English first. Adding one: its dictionaries, a plural rule and a row here.</summary>
    public static readonly IReadOnlyList<Language> Languages =
    [
        new("en", English, "English", null, n => n == 1 ? 0 : 1),
        new("pl", "pl-PL", "Polski", PlTexts,
            n => n == 1 ? 0 : n % 10 is >= 2 and <= 4 && n % 100 is < 12 or > 14 ? 1 : 2),
    ];

    private static Dictionary<string, string> PlTexts() =>
        Merge(PlCommon, PlNav, PlHome, PlRecords, PlRecordForm, PlStats, PlMore, PlAccount, PlManage, PlMessages);

    public static readonly string[] Cultures = [.. Languages.Select(l => l.Culture)];

    /// <summary>The language of the current UI culture; English when it isn't supported.</summary>
    public static Language Current => Find(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);

    /// <summary>Culture for a language code from the &lt;html lang&gt; attribute or a cookie; English by default.</summary>
    public static string CultureFor(string? language) => Find(language?.Split('-')[0]).Culture;

    private static Language Find(string? code)
    {
        foreach (var language in Languages)
        {
            if (string.Equals(language.Code, code, StringComparison.OrdinalIgnoreCase))
            {
                return language;
            }
        }

        return Languages[0];
    }

    public static void Apply(string culture)
    {
        var info = CultureInfo.GetCultureInfo(culture);
        CultureInfo.DefaultThreadCurrentCulture = info;
        CultureInfo.DefaultThreadCurrentUICulture = info;
        CultureInfo.CurrentCulture = info;
        CultureInfo.CurrentUICulture = info;
    }

    public static string Tr(string en) => Current.Texts is { } texts && texts.TryGetValue(en, out var text) ? text : en;

    public static string Tr(string en, params object?[] args) =>
        string.Format(CultureInfo.CurrentCulture, Tr(en), args);

    /// <summary>
    /// Count text. <paramref name="one"/> and <paramref name="other"/> are English formats with {0}; a translation of
    /// <paramref name="one"/> holds all the language's forms separated by "|" (Polish: "one|few|many").
    /// </summary>
    public static string Plural(int n, string one, string other)
    {
        var language = Current;
        if (language.Texts is not { } texts || !texts.TryGetValue(one, out var forms))
        {
            return string.Format(CultureInfo.CurrentCulture, n == 1 ? one : other, n);
        }

        var parts = forms.Split('|');
        return string.Format(CultureInfo.CurrentCulture, parts[Math.Min(language.PluralForm(n), parts.Length - 1)], n);
    }

    /// <summary>Translates a message from the API or a validator, including ones with numbers filled in ("{0}" keys).</summary>
    public static string TrMessage(string message)
    {
        var language = Current;
        if (language.Texts is not { } texts)
        {
            return message;
        }

        if (texts.TryGetValue(message, out var exact))
        {
            return exact;
        }

        foreach (var (pattern, text) in language.Patterns)
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

    /// <summary>A UI language. <see cref="Texts"/> is null for English, the source language.</summary>
    public sealed class Language(
        string code, string culture, string name, Func<Dictionary<string, string>>? texts, Func<int, int> pluralForm)
    {
        private readonly Lazy<Dictionary<string, string>?> _texts = new(() => texts?.Invoke());
        private List<(Regex Pattern, string Text)>? _patterns;

        public string Code => code;
        public string Culture => culture;

        /// <summary>The language's own name, shown untranslated.</summary>
        public string Name => name;

        public IReadOnlyDictionary<string, string>? Texts => _texts.Value;

        /// <summary>Index of the plural form for <paramref name="n"/> in a "|"-separated translation.</summary>
        internal int PluralForm(int n) => pluralForm(n);

        internal List<(Regex Pattern, string Text)> Patterns => _patterns ??= Texts is null ? [] :
        [
            .. Texts.Where(e => e.Key.Contains('{', StringComparison.Ordinal))
                .Select(e => (new Regex("^" + Placeholder().Replace(Regex.Escape(e.Key), "(?<a$1>.+?)") + "$"), e.Value)),
        ];
    }
}
