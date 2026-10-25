namespace BM2.Client.Components;

public static class TagPalette
{
    /// <summary>CSS class that sets <c>--tag</c> from the theme token of a palette key; null for a neutral tag.</summary>
    public static string? ClassFor(string? color) => color is null ? null : $"tag-color--{color}";
}
