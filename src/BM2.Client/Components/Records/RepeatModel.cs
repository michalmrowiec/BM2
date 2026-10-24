using System.Globalization;
using BM2.Contracts.Records;
using BM2.Contracts.Recurring;

namespace BM2.Client.Components.Records;

/// <summary>Editable schedule for <see cref="RepeatFields"/>; monthly from the record date by default.</summary>
public sealed class RepeatModel
{
    public RecurrenceFrequency Frequency { get; set; } = RecurrenceFrequency.Monthly;

    public int Interval { get; set; } = 1;

    public RecurrenceEnd End { get; set; } = RecurrenceEnd.Never;

    public DateOnly? EndDate { get; set; }

    public int? Count { get; set; } = 12;

    public bool Confirm { get; set; }

    public bool IsPaused { get; set; }

    public static RepeatModel From(RecurringRuleItem rule) => new()
    {
        Frequency = rule.Schedule.Frequency,
        Interval = rule.Schedule.Interval,
        End = rule.Schedule.End,
        EndDate = rule.Schedule.EndDate,
        Count = rule.Schedule.Count ?? 12,
        Confirm = rule.Mode == RecurringMode.Confirm,
        IsPaused = rule.IsPaused,
    };

    public RecurrenceSchedule ToSchedule(DateOnly start) => new(
        Frequency,
        Interval,
        start,
        End,
        End == RecurrenceEnd.OnDate ? EndDate ?? start : null,
        End == RecurrenceEnd.AfterCount ? Count : null);

    public SaveRecurringRuleRequest ToRequest(SaveRecordRequest record, Guid? fromRecordId) => new(
        record, ToSchedule(record.OccurredOn), Confirm ? RecurringMode.Confirm : RecurringMode.Auto, IsPaused, fromRecordId);

    public string Describe(DateOnly start) => RecurringText.Describe(ToSchedule(start));
}

public static class RecurringText
{
    public static string Describe(RecurrenceSchedule s)
    {
        var unit = s.Frequency switch
        {
            RecurrenceFrequency.Daily => "day",
            RecurrenceFrequency.Weekly => "week",
            RecurrenceFrequency.Monthly => "month",
            _ => "year",
        };
        var every = s.Interval == 1 ? $"Every {unit}" : $"Every {s.Interval} {unit}s";
        var on = s.Frequency switch
        {
            RecurrenceFrequency.Weekly => $" on {s.StartDate.DayOfWeek}",
            RecurrenceFrequency.Monthly => s.StartDate.Day > 28 ? $" on day {s.StartDate.Day} (or the last day)" : $" on day {s.StartDate.Day}",
            RecurrenceFrequency.Yearly => $" on {s.StartDate.ToString("d MMM", CultureInfo.InvariantCulture)}",
            _ => "",
        };
        var end = s.End switch
        {
            RecurrenceEnd.OnDate => $", until {s.EndDate?.ToString("d MMM yyyy", CultureInfo.InvariantCulture)}",
            RecurrenceEnd.AfterCount => $", {s.Count} times",
            _ => "",
        };
        return every + on + end;
    }

    /// <summary>Occurrences from the rule's next one up to <paramref name="until"/>, with their index.</summary>
    public static IEnumerable<(int Index, DateOnly Date)> Upcoming(RecurringRuleItem rule, DateOnly until)
    {
        if (rule.IsPaused)
        {
            yield break;
        }

        for (var index = rule.NextIndex; rule.Schedule.NextDate(index) is { } date && date <= until; index++)
        {
            yield return (index, date);
        }
    }

    /// <summary>The rule as a record for the record form (account names are not needed there).</summary>
    public static RecordItem ToRecord(RecurringRuleItem rule)
    {
        var r = rule.Record;
        return new RecordItem(
            rule.Id, r.Type, RecordStatus.Completed, r.OccurredOn, r.AccountId, "", r.Amount, r.Currency, r.AccountAmount, r.CategoryId,
            r.TransferAccountId, null, r.TransferAmount, r.Name, r.TagIds);
    }
}
