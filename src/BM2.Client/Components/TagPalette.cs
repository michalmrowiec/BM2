using static BM2.Client.Localization.Strings;

namespace BM2.Client.Components;

public static class TagPalette
{
    /// <summary>CSS class that sets <c>--tag</c> from the theme token of a palette key; null for a neutral tag.</summary>
    public static string? ClassFor(string? color) => color is null ? null : $"tag-color--{color}";

    public static string LabelFor(string color) => color switch
    {
        "red" => Tr("Red"),
        "orange" => Tr("Orange"),
        "amber" => Tr("Amber"),
        "lime" => Tr("Lime"),
        "green" => Tr("Green"),
        "teal" => Tr("Teal"),
        "cyan" => Tr("Cyan"),
        "blue" => Tr("Blue"),
        "indigo" => Tr("Indigo"),
        "violet" => Tr("Violet"),
        "pink" => Tr("Pink"),
        "brown" => Tr("Brown"),
        "slate" => Tr("Slate"),
        _ => color,
    };
}
