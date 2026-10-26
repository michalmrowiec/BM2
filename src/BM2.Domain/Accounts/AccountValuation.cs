namespace BM2.Domain.Accounts;

/// <summary>Manual value of an investment account on a day (one per day).</summary>
public sealed class AccountValuation
{
    private AccountValuation()
    {
    }

    public Guid Id { get; private set; }

    public Guid AccountId { get; private set; }

    public DateOnly ValuedOn { get; private set; }

    public decimal Value { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public static AccountValuation Create(Guid accountId, DateOnly valuedOn, decimal value, DateTimeOffset now) =>
        new() { AccountId = accountId, ValuedOn = valuedOn, Value = value, UpdatedAt = now };

    public void Change(decimal value, DateTimeOffset now)
    {
        Value = value;
        UpdatedAt = now;
    }
}
