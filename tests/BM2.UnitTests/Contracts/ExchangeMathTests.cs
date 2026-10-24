using BM2.Contracts.Records;

namespace BM2.UnitTests.Contracts;

public class ExchangeMathTests
{
    [Fact]
    public void Converts_with_rounding_away_from_zero() => ExchangeMath.ToAccountAmount(10.01m, 4.3125m).ShouldBe(43.17m);

    [Fact]
    public void Half_cent_rounds_up() => ExchangeMath.ToAccountAmount(10m, 4.3125m).ShouldBe(43.13m);

    [Fact]
    public void Derives_rate_from_amounts() => ExchangeMath.RateOf(10m, 43.17m).ShouldBe(4.317m);

    [Fact]
    public void Rate_is_rounded_to_six_places() => ExchangeMath.RateOf(3m, 10m).ShouldBe(3.333333m);
}
