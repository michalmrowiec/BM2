using BM2.Contracts.Stats;

namespace BM2.Domain.Stats;

/// <summary>
/// Trend of one monthly series (index 0 = <see cref="First"/>): linear regression plus additive calendar-month
/// seasonality from 12+ months; 6–11 months give a line only, fewer a plain average.
/// </summary>
public sealed class MonthlyTrend
{
    private readonly decimal[] _seasons = new decimal[12];

    private MonthlyTrend(DateOnly first, ForecastModel model)
    {
        First = first;
        Model = model;
    }

    public DateOnly First { get; }

    public ForecastModel Model { get; }

    public decimal Intercept { get; private set; }

    /// <summary>Change per month.</summary>
    public decimal Slope { get; private set; }

    /// <summary>Standard deviation of one month around the fit (0 below 3 months).</summary>
    public decimal Sigma { get; private set; }

    public decimal Season(int month) => _seasons[month - 1];

    /// <summary>Expected value for a month, never below zero.</summary>
    public decimal Predict(DateOnly month)
    {
        if (Model == ForecastModel.None)
        {
            return 0;
        }

        var t = (month.Year - First.Year) * 12 + month.Month - First.Month;
        return Math.Max(0, Intercept + Slope * t + Season(month.Month));
    }

    public static MonthlyTrend Fit(DateOnly first, IReadOnlyList<decimal> values)
    {
        var n = values.Count;
        var model = n switch
        {
            0 => ForecastModel.None,
            < 6 => ForecastModel.Average,
            < 12 => ForecastModel.Linear,
            _ => ForecastModel.Seasonal,
        };
        var trend = new MonthlyTrend(first, model);
        if (n == 0)
        {
            return trend;
        }

        decimal parameters = 1;
        if (model == ForecastModel.Average)
        {
            trend.Intercept = values.Sum() / n;
        }
        else
        {
            trend.Line(values);
            parameters = 2;
            if (model == ForecastModel.Seasonal)
            {
                parameters += trend.FitSeasons(values);
                trend.Line(values); // refit the line on the deseasonalized series
            }
        }

        var squares = Enumerable.Range(0, n).Sum(t =>
        {
            var r = values[t] - trend.Raw(t);
            return r * r;
        });
        var freedom = n - parameters;
        trend.Sigma = n < 3 || freedom <= 0 ? 0 : (decimal)Math.Sqrt((double)(squares / freedom));
        return trend;
    }

    private decimal Raw(int t) => Intercept + Slope * t + Season(First.AddMonths(t).Month);

    private void Line(IReadOnlyList<decimal> values)
    {
        var n = values.Count;
        var y = Enumerable.Range(0, n).Select(t => values[t] - Season(First.AddMonths(t).Month)).ToList();
        var tMean = (n - 1) / 2m;
        var yMean = y.Sum() / n;
        var sxx = Enumerable.Range(0, n).Sum(t => (t - tMean) * (t - tMean));
        Slope = Enumerable.Range(0, n).Sum(t => (t - tMean) * (y[t] - yMean)) / sxx;
        Intercept = yMean - Slope * tMean;
    }

    /// <summary>Mean residual per calendar month, shrunk by c/(c+1) for c samples, centered; returns the parameters used.</summary>
    private decimal FitSeasons(IReadOnlyList<decimal> values)
    {
        var sums = new decimal[12];
        var counts = new int[12];
        for (var t = 0; t < values.Count; t++)
        {
            var m = First.AddMonths(t).Month - 1;
            sums[m] += values[t] - Intercept - Slope * t;
            counts[m]++;
        }

        for (var m = 0; m < 12; m++)
        {
            _seasons[m] = sums[m] / (counts[m] + 1);
        }

        var mean = _seasons.Sum() / 12;
        for (var m = 0; m < 12; m++)
        {
            _seasons[m] -= mean;
        }

        return counts.Sum(c => c / (c + 1m)) - 1;
    }
}
