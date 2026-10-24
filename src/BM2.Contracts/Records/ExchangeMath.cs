namespace BM2.Contracts.Records;

/// <summary>Conversion between the transaction currency and the account currency.</summary>
public static class ExchangeMath
{
    public static decimal ToAccountAmount(decimal amount, decimal rate) =>
        Math.Round(amount * rate, 2, MidpointRounding.AwayFromZero);

    public static decimal RateOf(decimal amount, decimal accountAmount) =>
        Math.Round(accountAmount / amount, 6, MidpointRounding.AwayFromZero);
}
