using BM2.Contracts.Common;
using BM2.Contracts.Records;
using BM2.Contracts.Recurring;
using BM2.Domain.Records;

namespace BM2.Domain.Recurring;

/// <summary>
/// Repeats a record on a schedule. Records are added by catch-up up to today, never ahead; <see cref="NextIndex"/>
/// is the next occurrence to add and <see cref="NextDate"/> its date (null once the rule has ended).
/// </summary>
public sealed class RecurringRule
{
    private readonly List<RecurringRuleTag> _tags = [];

    private RecurringRule()
    {
        Currency = "";
    }

    public Guid Id { get; private set; }

    public Guid WalletId { get; private set; }

    public RecordType Type { get; private set; }

    public Guid AccountId { get; private set; }

    public Guid? CategoryId { get; private set; }

    public Guid? TransferAccountId { get; private set; }

    public decimal Amount { get; private set; }

    public string Currency { get; private set; }

    public decimal AccountAmount { get; private set; }

    public decimal? TransferAmount { get; private set; }

    public string? Name { get; private set; }

    public RecurrenceFrequency Frequency { get; private set; }

    public int Interval { get; private set; }

    public DateOnly StartDate { get; private set; }

    public RecurrenceEnd End { get; private set; }

    public DateOnly? EndDate { get; private set; }

    public int? Count { get; private set; }

    public RecurringMode Mode { get; private set; }

    public bool IsPaused { get; private set; }

    public int NextIndex { get; private set; }

    public DateOnly? NextDate { get; private set; }

    public Guid CreatedByUserId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public IReadOnlyCollection<RecurringRuleTag> Tags => _tags;

    public RecurrenceSchedule Schedule => new(Frequency, Interval, StartDate, End, EndDate, Count);

    /// <param name="continueAfter">Date of an existing record taken as the first occurrence; the rule continues
    /// after it (catch-up adds the missed ones up to today).</param>
    public static RecurringRule Create(Guid walletId, SaveRecurringRuleRequest request, Guid userId, DateTimeOffset now, DateOnly? continueAfter)
    {
        var rule = new RecurringRule { WalletId = walletId, CreatedByUserId = userId, CreatedAt = now };
        rule.Apply(request);
        rule.MoveTo(continueAfter is { } date ? Math.Max(1, rule.Schedule.FirstIndexOnOrAfter(date.AddDays(1))) : 0);
        return rule;
    }

    /// <summary>Changes apply to occurrences not added yet; resuming skips what was missed while paused.</summary>
    public void Update(SaveRecurringRuleRequest request, DateOnly today)
    {
        var covered = Schedule.DateOf(NextIndex);
        var resumed = IsPaused && !request.IsPaused;
        Apply(request);
        MoveTo(Schedule.FirstIndexOnOrAfter(resumed ? Max(covered, today) : covered));
    }

    /// <summary>The record for occurrence <see cref="NextIndex"/>.</summary>
    public RecordDraft NextDraft() =>
        DraftFor(NextIndex, NextDate!.Value, Mode == RecurringMode.Confirm ? RecordStatus.Pending : RecordStatus.Completed);

    /// <summary>The record for occurrence <paramref name="index"/>; installments get " k/N" after the name.</summary>
    public RecordDraft DraftFor(int index, DateOnly date, RecordStatus status)
    {
        var suffix = Schedule.Suffix(index);
        var name = Name is null ? suffix?.Trim() : Name[..Math.Min(Name.Length, Limits.RecordName - (suffix?.Length ?? 0))] + suffix;
        return new RecordDraft(
            Type,
            status,
            date,
            AccountId,
            Amount,
            Currency,
            AccountAmount,
            CategoryId,
            TransferAccountId,
            TransferAmount,
            name,
            _tags.Select(t => t.TagId).ToList());
    }

    public void Advance() => MoveTo(NextIndex + 1);

    private void MoveTo(int index)
    {
        NextIndex = index;
        NextDate = Schedule.NextDate(index);
    }

    private static DateOnly Max(DateOnly a, DateOnly b) => a > b ? a : b;

    private void Apply(SaveRecurringRuleRequest request)
    {
        var record = request.Record;
        var isTransfer = record.Type == RecordType.Transfer;
        Type = record.Type;
        AccountId = record.AccountId;
        CategoryId = isTransfer ? null : record.CategoryId;
        TransferAccountId = isTransfer ? record.TransferAccountId : null;
        Amount = record.Amount;
        Currency = record.Currency;
        AccountAmount = record.AccountAmount;
        TransferAmount = isTransfer ? record.TransferAmount : null;
        Name = string.IsNullOrWhiteSpace(record.Name) ? null : record.Name.Trim();

        var schedule = request.Schedule;
        Frequency = schedule.Frequency;
        Interval = schedule.Interval;
        StartDate = schedule.StartDate;
        End = schedule.End;
        EndDate = schedule.End == RecurrenceEnd.OnDate ? schedule.EndDate : null;
        Count = schedule.End == RecurrenceEnd.AfterCount ? schedule.Count : null;
        Mode = request.Mode;
        IsPaused = request.IsPaused;

        var wanted = record.TagIds.ToHashSet();
        _tags.RemoveAll(t => !wanted.Contains(t.TagId));
        foreach (var tagId in wanted.Where(id => _tags.TrueForAll(t => t.TagId != id)))
        {
            _tags.Add(new RecurringRuleTag(tagId));
        }
    }
}

public sealed class RecurringRuleTag
{
    private RecurringRuleTag()
    {
    }

    internal RecurringRuleTag(Guid tagId) => TagId = tagId;

    public Guid RecurringRuleId { get; private set; }

    public Guid TagId { get; private set; }
}
