using System.Globalization;
using BM2.ClientTests.Infrastructure;

namespace BM2.ClientTests.Components;

public class MoneyInputTests : BunitContext
{
    [Theory]
    [InlineData("pl-PL", "12,50", "12.50")]
    [InlineData("pl-PL", "12.50", "12.50")]
    [InlineData("pl-PL", "1 234,5", "1234.5")]
    [InlineData("en-US", "12.50", "12.50")]
    [InlineData("en-US", "1,234.50", "1234.50")]
    [InlineData("en-US", "1,234", "1234")]
    [InlineData("en-US", "-20", "-20")]
    public void Parses_amount_in_culture(string culture, string typed, string expected)
    {
        using var scope = new CultureScope(culture);
        var cut = Render<MoneyFormHost>();

        cut.Find("input").Change(typed);

        cut.Instance.Model.Amount.ShouldBe(decimal.Parse(expected, CultureInfo.InvariantCulture));
        cut.FindAll(".field__error").ShouldBeEmpty();
    }

    [Fact]
    public void Rejects_more_than_two_decimals()
    {
        using var scope = new CultureScope("en-US");
        var cut = Render<MoneyFormHost>();

        cut.Find("input").Change("1.005");

        cut.Find(".field__error").TextContent.ShouldNotBeEmpty();
        cut.Find("input").GetAttribute("aria-invalid").ShouldBe("true");
    }

    [Theory]
    [InlineData("pl-PL", "1.500")]
    [InlineData("pl-PL", "1,500")]
    [InlineData("en-US", "2.750")]
    public void Third_decimal_digit_is_rejected_even_when_zero(string culture, string typed)
    {
        using var scope = new CultureScope(culture);
        var cut = Render<MoneyFormHost>();

        cut.Find("input").Change(typed);

        cut.Find(".field__error").TextContent.ShouldNotBeEmpty();
        cut.Instance.Model.Amount.ShouldBeNull();
    }

    [Fact]
    public void Empty_text_clears_the_value()
    {
        var cut = Render<MoneyFormHost>();
        cut.Find("input").Change("5");

        cut.Find("input").Change("");

        cut.Instance.Model.Amount.ShouldBeNull();
    }

    [Fact]
    public void Uses_decimal_keyboard_and_shows_currency()
    {
        var cut = Render<MoneyFormHost>();

        cut.Find("input").GetAttribute("inputmode").ShouldBe("decimal");
        cut.Find(".money-input__currency").TextContent.ShouldBe("zł");
    }
}
