using BM2.IntegrationTests.Infrastructure;

namespace BM2.IntegrationTests.Features;

public class ShellTests(IntegrationFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Static_pages_apply_theme_before_first_paint()
    {
        using var client = Factory.CreateAnonymousClient();

        var html = await client.GetStringAsync("/Account/Login", TestContext.Current.CancellationToken);

        var head = html[..html.IndexOf("</head>", StringComparison.Ordinal)];
        head.ShouldContain("js/theme");
        head.ShouldContain("name=\"theme-color\"");
        head.ShouldContain("name=\"color-scheme\"");
        html.ShouldContain("data-theme-toggle");
    }
}
