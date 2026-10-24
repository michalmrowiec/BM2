using BM2.Client.Components;
using BM2.ClientTests.Infrastructure;

namespace BM2.ClientTests.Components;

public class AmountTests : BunitContext
{
    [Fact]
    public void Negative_value_uses_unicode_minus_and_negative_class()
    {
        using var culture = new CultureScope("en-US");

        var cut = Render<Amount>(p => p.Add(x => x.Value, -12.5m).Add(x => x.Currency, "PLN"));

        cut.Find(".amount").TextContent.ShouldBe("−12.50 PLN");
        cut.Find(".amount").ClassList.ShouldContain("amount--negative");
    }

    [Fact]
    public void Positive_value_has_plus_and_group_separator()
    {
        using var culture = new CultureScope("en-US");

        var cut = Render<Amount>(p => p.Add(x => x.Value, 1000m).Add(x => x.Currency, "EUR"));

        cut.Find(".amount").TextContent.ShouldBe("+1,000.00 EUR");
        cut.Find(".amount").ClassList.ShouldContain("amount--positive");
    }

    [Fact]
    public void Explicit_minus_sign_applies_to_positive_value()
    {
        using var culture = new CultureScope("en-US");

        var cut = Render<Amount>(p => p.Add(x => x.Value, 5m).Add(x => x.Currency, "PLN").Add(x => x.Sign, AmountSign.Minus));

        cut.Find(".amount").TextContent.ShouldBe("−5.00 PLN");
    }

    [Fact]
    public void Zero_has_no_sign()
    {
        using var culture = new CultureScope("en-US");

        var cut = Render<Amount>(p => p.Add(x => x.Value, 0m).Add(x => x.Currency, "PLN"));

        cut.Find(".amount").TextContent.ShouldBe("0.00 PLN");
    }

    [Fact]
    public void Uses_current_culture_format()
    {
        using var culture = new CultureScope("pl-PL");

        var cut = Render<Amount>(p => p.Add(x => x.Value, -1234.5m).Add(x => x.Currency, "PLN"));

        cut.Find(".amount").TextContent.ShouldBe("−1 234,50 PLN");
    }
}
