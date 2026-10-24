using System.Globalization;
using BM2.Contracts.Common;

namespace BM2.UnitTests.Contracts;

public class MoneyRulesTests
{
    [Theory]
    [InlineData("0.01", true)]
    [InlineData("999999999999.99", true)]
    [InlineData("0", true)]
    [InlineData("-5", false)]
    [InlineData("1.005", false)]
    [InlineData("1000000000000", false)]
    public void Validates_amounts(string value, bool valid) =>
        MoneyRules.IsValidAmount(decimal.Parse(value, CultureInfo.InvariantCulture)).ShouldBe(valid);

    [Fact]
    public void Trailing_zeros_do_not_count_as_decimals() => MoneyRules.HasAtMostTwoDecimals(1.5000m).ShouldBeTrue();

    [Fact]
    public void Supported_currencies_start_with_pln()
    {
        Currencies.All[0].ShouldBe("PLN");
        Currencies.IsSupported("EUR").ShouldBeTrue();
        Currencies.IsSupported("XYZ").ShouldBeFalse();
        Currencies.IsSupported(null).ShouldBeFalse();
    }
}
