namespace BM2.Contracts.Common;

public static class MoneyRules
{
    public const decimal MaxAmount = 999_999_999_999.99m;

    public static bool IsValidAmount(decimal value) => value >= 0 && value <= MaxAmount && HasAtMostTwoDecimals(value);

    public static bool HasAtMostTwoDecimals(decimal value) => decimal.Round(value, 2) == value;
}
