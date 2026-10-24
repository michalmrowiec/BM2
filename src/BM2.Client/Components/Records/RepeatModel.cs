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

    /// <summary>Plain sentences saying what saving will do: the record now, past dates added at once, the next ones.</summary>
    public IEnumerable<string> Effect(
        DateOnly start, RecordStatus? firstStatus, bool firstExists, RecurringRuleItem? existing, DateOnly today)
    {
        var schedule = ToSchedule(start);
        var status = Confirm ? "pending (to confirm)" : "completed";
        int first;
        if (existing is not null)
        {
            first = schedule.FirstIndexOnOrAfter(existing.NextDate ?? today);
            yield return existing.RecordCount == 1
                ? "1 record was already added; when saving you choose whether it changes too."
                : $"{existing.RecordCount} records were already added; when saving you choose whether they change too.";
        }
        else if (firstStatus is { } saved)
        {
            first = 1;
            yield return firstExists
                ? $"This record ({Day(start)}) counts as the first one."
                : $"Saves this record now ({Day(start)}, {saved.ToString().ToLowerInvariant()}).";
        }
        else
        {
            first = 0;
        }

        var past = new List<DateOnly>();
        var next = new List<DateOnly>();
        for (var i = first; schedule.NextDate(i) is { } date && next.Count < 3 && past.Count < 1000; i++)
        {
            (date <= today ? past : next).Add(date);
        }

        if (past.Count > 0 && existing is null)
        {
            yield return past.Count == 1
                ? $"Adds 1 past record right away ({Day(past[0])}), as {status}."
                : $"Adds {past.Count} past records right away ({Day(past[0])} – {Day(past[^1])}), as {status}.";
        }

        var total = End == RecurrenceEnd.AfterCount ? $" ({Count} in total)" : End == RecurrenceEnd.Never ? ", with no end" : "";
        yield return next.Count == 0
            ? "No further records: the schedule ends before the next date."
            : $"Then adds automatically on {string.Join(", ", next.Select(Day))}{(next.Count == 3 ? "…" : "")}{total}, as {status}.";
    }

    private static string Day(DateOnly date) => date.ToString("d MMM yyyy", CultureInfo.CurrentCulture);
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
