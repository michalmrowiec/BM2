using System.Globalization;
using System.Text.RegularExpressions;
using BM2.Client.Localization;

namespace BM2.ClientTests.Localization;

public sealed partial class StringsTests : IDisposable
{
    private readonly CultureInfo _culture = CultureInfo.CurrentCulture;
    private readonly CultureInfo _uiCulture = CultureInfo.CurrentUICulture;

    public StringsTests() => Use("pl-PL");

    public void Dispose()
    {
        CultureInfo.CurrentCulture = _culture;
        CultureInfo.CurrentUICulture = _uiCulture;
    }

    private static void Use(string culture) =>
        (CultureInfo.CurrentCulture, CultureInfo.CurrentUICulture) = (new(culture), new(culture));

    [Theory]
    [InlineData(1, "1 operacja")]
    [InlineData(2, "2 operacje")]
    [InlineData(5, "5 operacji")]
    [InlineData(12, "12 operacji")]
    [InlineData(22, "22 operacje")]
    [InlineData(112, "112 operacji")]
    public void Polish_plural_picks_the_form(int n, string expected) =>
        Assert.Equal(expected, Strings.Plural(n, "{0} record", "{0} records"));

    [Fact]
    public void English_plural_and_missing_entries_stay_english()
    {
        Assert.Equal("Not a key", Strings.Tr("Not a key"));
        Use("en-GB");
        Assert.Equal("2 records", Strings.Plural(2, "{0} record", "{0} records"));
        Assert.Equal("Save", Strings.Tr("Save"));
    }

    [Fact]
    public void Unsupported_language_falls_back_to_english()
    {
        Assert.Equal("en-GB", Strings.CultureFor("de"));
        Assert.Equal("pl-PL", Strings.CultureFor("pl-PL"));
        Assert.Equal("en-GB", Strings.CultureFor(null));
    }

    [Fact]
    public void Message_with_filled_in_values_is_translated()
    {
        var key = Strings.Languages[1].Texts!.Keys.First(k => k.Contains("{0}", StringComparison.Ordinal) && !k.Contains("{1}", StringComparison.Ordinal));
        var message = key.Replace("{0}", "42", StringComparison.Ordinal);

        Assert.Equal(Strings.Tr(key, "42"), Strings.TrMessage(message));
    }

    [Fact]
    public void Every_literal_in_the_ui_has_a_polish_entry()
    {
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (!File.Exists(Path.Combine(root.FullName, "BM2.slnx")))
        {
            root = root.Parent!;
        }

        var texts = Strings.Languages[1].Texts!;
        var missing = new[] { "src/BM2.Client", "src/BM2.Server/Components" }
            .SelectMany(dir => Directory.EnumerateFiles(Path.Combine(root.FullName, dir), "*.*", SearchOption.AllDirectories))
            .Where(f => f.EndsWith(".razor", StringComparison.Ordinal) || f.EndsWith(".cs", StringComparison.Ordinal))
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                && !f.Contains($"{Path.DirectorySeparatorChar}Localization{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .SelectMany(f => Literal().Matches(File.ReadAllText(f)).Select(m => m.Groups["text"].Value.Replace("\\\"", "\"", StringComparison.Ordinal)))
            .Where(text => !texts.ContainsKey(text))
            .Distinct()
            .Order()
            .ToList();

        Assert.Empty(missing);
    }

    [GeneratedRegex("""\b(?:Tr\(|Plural\([^,()]+,\s*)"(?<text>(?:[^"\\]|\\.)+)"[,)]""")]
    private static partial Regex Literal();
}
