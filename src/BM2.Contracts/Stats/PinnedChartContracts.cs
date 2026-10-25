using BM2.Contracts.Common;
using FluentValidation;

namespace BM2.Contracts.Stats;

public enum PinnedChartKind
{
    Categories = 1,
    Monthly = 2,
    Tags = 3,
    Forecast = 4,
}

/// <param name="Query">Screen filters as a query string; the client turns it into a stats API call.</param>
public sealed record PinnedChartItem(Guid Id, PinnedChartKind Kind, string Title, string Query);

public sealed record PinChartRequest(PinnedChartKind Kind, string Title, string Query);

public sealed class PinChartRequestValidator : AbstractValidator<PinChartRequest>
{
    public const int MaxQuery = 1000;

    public PinChartRequestValidator()
    {
        RuleFor(x => x.Kind).IsInEnum();
        RuleFor(x => x.Title).Name(100);
        RuleFor(x => x.Query).NotNull().MaximumLength(MaxQuery);
    }
}
