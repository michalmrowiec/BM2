using System.Globalization;
using BM2.Contracts.Categories;
using BM2.Contracts.Records;
using BM2.Contracts.Stats;
using BM2.Contracts.Wallets;

namespace BM2.Client.Components.Stats;

/// <summary>ECharts options and formatting shared by the stats screens.</summary>
public static class StatsCharts
{
    public const string NumberFont = "Fraunces, ui-serif, Georgia, serif";
    private const int MaxColored = 8;
    private static readonly string[] Palette = ["vivid", "blue", "green", "amber", "violet", "teal", "pink", "orange", "red"];

    /// <summary>Swatch key (class <c>stats-swatch--key</c>) of the n-th slice; the tail shares a muted color.</summary>
    public static string SwatchOf(int index) => index < MaxColored ? Palette[index % Palette.Length] : "muted";

    public static CategoryItem? CategoryOf(WalletSnapshot wallet, Guid? id) =>
        id is null ? null : wallet.Categories.FirstOrDefault(c => c.Id == id);

    public static string NameOf(WalletSnapshot wallet, Guid? id) =>
        id is null ? "No category" : CategoryOf(wallet, id)?.Name ?? "Deleted category";

    public static decimal Signed(decimal total, RecordType type) => type == RecordType.Expense ? -total : total;

    public static string Share(decimal part, decimal total) =>
        total == 0 ? "0%" : (part / total).ToString("P1", CultureInfo.CurrentCulture);

    public static string Records(int count) => count == 1 ? "1 record" : $"{count} records";

    public static object Donut(IReadOnlyList<(string Id, string Name, decimal Value)> slices, string currency)
    {
        var total = slices.Sum(s => s.Value);
        return new
        {
            tooltip = new { trigger = "item", formatter = "{b}<br/>{c} " + currency + " ({d}%)" },
            title = new
            {
                text = total.ToString("N0", CultureInfo.CurrentCulture),
                subtext = currency,
                left = "center",
                top = "38%",
                textStyle = new { color = "var(--ink)", fontFamily = NumberFont, fontSize = 22, fontWeight = 600 },
                subtextStyle = new { color = "var(--ink-muted)" },
            },
            series = new object[]
            {
                new
                {
                    type = "pie",
                    radius = new[] { "58%", "86%" },
                    label = new { show = false },
                    itemStyle = new { borderColor = "var(--sheet)", borderWidth = 2 },
                    data = slices.Select((s, i) => new
                    {
                        id = s.Id,
                        name = s.Name,
                        value = s.Value,
                        itemStyle = new { color = $"var({TokenOf(SwatchOf(i))})" },
                    }).ToList(),
                },
            },
        };
    }

    /// <summary>Bars per month: with <paramref name="only"/> one series, otherwise income, expenses and a net line.</summary>
    public static object Months(IReadOnlyList<MonthTotal> items, RecordType? only = null)
    {
        var series = new List<object>();
        if (only != RecordType.Expense)
        {
            series.Add(Bars("Income", items.Select(m => m.Income), only is null ? "var(--positive)" : "var(--vivid)"));
        }

        if (only != RecordType.Income)
        {
            series.Add(Bars("Expenses", items.Select(m => m.Expense), only is null ? "var(--negative)" : "var(--vivid)"));
        }

        if (only is null)
        {
            series.Add(new
            {
                name = "Net",
                type = "line",
                data = items.Select(m => m.Income - m.Expense).ToList(),
                symbol = "circle",
                symbolSize = 6,
                lineStyle = new { color = "var(--ink)", width = 2 },
                itemStyle = new { color = "var(--ink)" },
            });
        }

        return new
        {
            grid = new { left = 4, right = 8, top = only is null ? 36 : 12, bottom = 4, containLabel = true },
            legend = new { show = only is null, top = 0, icon = "roundRect", textStyle = new { color = "var(--ink-muted)" } },
            tooltip = new { trigger = "axis" },
            xAxis = new
            {
                type = "category",
                data = items.Select(m => m.Month.ToString("MMM yy", CultureInfo.CurrentCulture)).ToList(),
                axisTick = new { show = false },
                axisLine = new { lineStyle = new { color = "var(--rule)" } },
                axisLabel = new { color = "var(--ink-muted)" },
            },
            yAxis = new
            {
                type = "value",
                splitLine = new { lineStyle = new { color = "var(--rule)" } },
                axisLabel = new { color = "var(--ink-muted)", fontFamily = NumberFont },
            },
            series,
        };
    }

    private static object Bars(string name, IEnumerable<decimal> values, string color) => new
    {
        name,
        type = "bar",
        data = values.ToList(),
        barMaxWidth = 18,
        itemStyle = new { color, borderRadius = new[] { 4, 4, 0, 0 } },
    };

    private static string TokenOf(string swatch) => swatch switch
    {
        "vivid" => "--vivid",
        "muted" => "--ink-muted",
        _ => $"--tag-{swatch}",
    };
}
