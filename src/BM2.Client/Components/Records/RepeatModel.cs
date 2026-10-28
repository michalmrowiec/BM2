using System.Globalization;
using BM2.Contracts.Records;
using BM2.Contracts.Recurring;
using static BM2.Client.Localization.Strings;

namespace BM2.Client.Components.Records;

/// <summary>Editable schedule for <see cref="RepeatFields"/>; monthly from the record date by default.</summary>
public sealed class RepeatModel
{
    public RecurrenceFrequency Frequency { get; set; } = RecurrenceFrequency.Monthly;

    public int Interval { get; set; } = 1;

    public RecurrenceEnd End { get; set; } = RecurrenceEnd.Never;

    public DateOnly? EndDate { get; set; }

    /// <summary><see cref="EndDate"/> for the date field, which needs a value.</summary>
    public DateOnly LastDate
    {
        get => EndDate ?? default;
        set => EndDate = value;
    }

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
        var status = Confirm ? Tr("pending (to confirm)") : Tr("completed");
        int first;
        if (existing is not null)
        {
            first = schedule.FirstIndexOnOrAfter(existing.NextDate ?? today);
            yield return Plural(
                existing.RecordCount,
                "{0} record was already added; when saving you choose whether it changes too.",
                "{0} records were already added; when saving you choose whether they change too.");
        }
        else if (firstStatus is { } saved)
        {
            first = 1;
            yield return firstExists
                ? Tr("This record ({0}) counts as the first one.", Day(start))
                : Tr("Saves this record now ({0}, {1}).", Day(start), StatusWord(saved));
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
                ? Tr("Adds 1 past record right away ({0}), as {1}.", Day(past[0]), status)
                : Tr("Adds {0} past records right away ({1} – {2}), as {3}.", past.Count, Day(past[0]), Day(past[^1]), status);
        }

        if (next.Count == 0)
        {
            yield return Tr("No further records: the schedule ends before the next date.");
            yield break;
        }

        var dates = string.Join(", ", next.Select(Day)) + (next.Count == 3 ? "…" : "");
        yield return End switch
        {
            RecurrenceEnd.AfterCount => Tr("Then adds automatically on {0} ({1} in total), as {2}.", dates, Count, status),
            RecurrenceEnd.Never => Tr("Then adds automatically on {0}, with no end, as {1}.", dates, status),
            _ => Tr("Then adds automatically on {0}, as {1}.", dates, status),
        };
    }

    private static string StatusWord(RecordStatus status) => status switch
    {
        RecordStatus.Pending => Tr("pending"),
        RecordStatus.Planned => Tr("planned"),
        _ => Tr("completed"),
    };

    private static string Day(DateOnly date) => date.ToString("d MMM yyyy", CultureInfo.CurrentCulture);
}

public static class RecurringText
{
    public static string Describe(RecurrenceSchedule s)
    {
        var n = s.Interval;
        var every = s.Frequency switch
        {
            RecurrenceFrequency.Daily => n == 1 ? Tr("Every day") : Plural(n, "Every {0} day", "Every {0} days"),
            RecurrenceFrequency.Weekly => n == 1 ? Tr("Every week") : Plural(n, "Every {0} week", "Every {0} weeks"),
            RecurrenceFrequency.Monthly => n == 1 ? Tr("Every month") : Plural(n, "Every {0} month", "Every {0} months"),
            _ => n == 1 ? Tr("Every year") : Plural(n, "Every {0} year", "Every {0} years"),
        };
        var text = s.Frequency switch
        {
            RecurrenceFrequency.Weekly => Tr("{0} on {1}", every, s.StartDate.ToString("dddd", CultureInfo.CurrentCulture)),
            RecurrenceFrequency.Monthly => s.StartDate.Day > 28
                ? Tr("{0} on day {1} (or the last day)", every, s.StartDate.Day)
                : Tr("{0} on day {1}", every, s.StartDate.Day),
            RecurrenceFrequency.Yearly => Tr("{0} on {1}", every, s.StartDate.ToString("d MMM", CultureInfo.CurrentCulture)),
            _ => every,
        };
        return s.End switch
        {
            RecurrenceEnd.OnDate => Tr("{0}, until {1}", text, s.EndDate?.ToString("d MMM yyyy", CultureInfo.CurrentCulture)),
            RecurrenceEnd.AfterCount => Tr("{0}, {1}", text, Plural(s.Count ?? 0, "{0} time", "{0} times")),
            _ => text,
        };
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
