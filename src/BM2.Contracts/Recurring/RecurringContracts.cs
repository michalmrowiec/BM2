using BM2.Contracts.Records;
using FluentValidation;

namespace BM2.Contracts.Recurring;

public enum RecurrenceFrequency
{
    Daily = 1,
    Weekly = 2,
    Monthly = 3,
    Yearly = 4,
}

public enum RecurrenceEnd
{
    Never = 1,
    OnDate = 2,
    AfterCount = 3,
}

/// <summary>Auto adds completed records; Confirm adds pending ones to approve.</summary>
public enum RecurringMode
{
    Auto = 1,
    Confirm = 2,
}

/// <summary>
/// Occurrence k is the start date plus k intervals, counted from the start, so a rule on the 31st falls on the last
/// day of shorter months and comes back to the 31st.
/// </summary>
public sealed record RecurrenceSchedule(
    RecurrenceFrequency Frequency,
    int Interval,
    DateOnly StartDate,
    RecurrenceEnd End,
    DateOnly? EndDate,
    int? Count)
{
    private static readonly DateOnly Horizon = new(2100, 12, 31);

    public DateOnly DateOf(int index) => Frequency switch
    {
        RecurrenceFrequency.Daily => StartDate.AddDays(index * Interval),
        RecurrenceFrequency.Weekly => StartDate.AddDays(index * Interval * 7),
        RecurrenceFrequency.Monthly => StartDate.AddMonths(index * Interval),
        _ => StartDate.AddYears(index * Interval),
    };

    /// <summary>The date of occurrence <paramref name="index"/>, or null after the end.</summary>
    public DateOnly? NextDate(int index)
    {
        if (index < 0 || (End == RecurrenceEnd.AfterCount && index >= Count))
        {
            return null;
        }

        var date = DateOf(index);
        return date > Horizon || (End == RecurrenceEnd.OnDate && date > EndDate) ? null : date;
    }

    /// <summary>The first occurrence index on or after <paramref name="date"/> (may be past the end).</summary>
    public int FirstIndexOnOrAfter(DateOnly date)
    {
        var index = 0;
        while (DateOf(index) < date && DateOf(index) < Horizon)
        {
            index++;
        }

        return index;
    }

    /// <summary>" k/N" for installments (rules that end after N times).</summary>
    public string? Suffix(int index) => End == RecurrenceEnd.AfterCount ? $" {index + 1}/{Count}" : null;
}

public sealed record RecurringRuleItem(
    Guid Id,
    SaveRecordRequest Record,
    RecurrenceSchedule Schedule,
    RecurringMode Mode,
    bool IsPaused,
    int NextIndex,
    DateOnly? NextDate);

/// <summary>
/// Creates or replaces a rule. <see cref="Record"/> gives the fields of each record; its date and status are ignored
/// (the schedule start and the mode decide them). <see cref="FromRecordId"/> links an existing record as the first
/// occurrence, so the rule continues after it.
/// </summary>
public sealed record SaveRecurringRuleRequest(
    SaveRecordRequest Record,
    RecurrenceSchedule Schedule,
    RecurringMode Mode,
    bool IsPaused,
    Guid? FromRecordId);

public sealed class SaveRecurringRuleRequestValidator : AbstractValidator<SaveRecurringRuleRequest>
{
    public SaveRecurringRuleRequestValidator()
    {
        RuleFor(x => x.Record).NotNull().SetValidator(new SaveRecordRequestValidator());
        RuleFor(x => x.Mode).IsInEnum();
        RuleFor(x => x.Schedule).NotNull();
        RuleFor(x => x.Schedule.Frequency).IsInEnum().When(x => x.Schedule is not null);
        RuleFor(x => x.Schedule.Interval).InclusiveBetween(1, 366).When(x => x.Schedule is not null)
            .WithMessage("Repeat every 1 to 366.");
        RuleFor(x => x.Schedule.StartDate).Must(d => d.Year is >= 2000 and <= 2100).When(x => x.Schedule is not null)
            .WithMessage("Choose a start date.");
        RuleFor(x => x.Schedule.End).IsInEnum().When(x => x.Schedule is not null);
        RuleFor(x => x.Schedule.EndDate)
            .Must((x, end) => end >= x.Schedule.StartDate && end.Value.Year <= 2100)
            .When(x => x.Schedule is { End: RecurrenceEnd.OnDate })
            .WithMessage("Choose an end date on or after the start.");
        RuleFor(x => x.Schedule.Count).InclusiveBetween(1, 1000)
            .When(x => x.Schedule is { End: RecurrenceEnd.AfterCount })
            .WithMessage("Enter 1 to 1000 times.");
    }
}
