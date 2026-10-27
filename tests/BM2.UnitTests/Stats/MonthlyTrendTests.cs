using BM2.Contracts.Stats;
using BM2.Domain.Stats;

namespace BM2.UnitTests.Stats;

public class MonthlyTrendTests
{
    private static readonly DateOnly Jan2024 = new(2024, 1, 1);

    [Fact]
    public void Fits_line_and_december_season_from_two_years()
    {
        // 1000 + 10 per month, every December +240.
        var values = Enumerable.Range(0, 24).Select(t => 1000m + 10 * t + (t % 12 == 11 ? 240 : 0)).ToList();

        var trend = MonthlyTrend.Fit(Jan2024, values);

        trend.Model.ShouldBe(ForecastModel.Seasonal);
        trend.Slope.ShouldBe(10m, 1.5m); // shrunk seasons leave a little of the December spike in the line
        trend.Season(12).ShouldBeGreaterThan(100m);
        trend.Predict(new DateOnly(2026, 12, 1)).ShouldBeGreaterThan(trend.Predict(new DateOnly(2026, 11, 1)) + 100);
    }

    [Fact]
    public void Short_history_is_a_plain_average() =>
        MonthlyTrend.Fit(Jan2024, [100m, 300m]).Predict(new DateOnly(2025, 1, 1)).ShouldBe(200m);

    [Fact]
    public void Prediction_never_goes_below_zero() =>
        MonthlyTrend.Fit(Jan2024, [600m, 500m, 400m, 300m, 200m, 100m]).Predict(new DateOnly(2025, 1, 1)).ShouldBe(0m);
}
