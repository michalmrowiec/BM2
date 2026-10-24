using FluentValidation;

namespace BM2.Contracts.Common;

public static class RuleExtensions
{
    /// <summary>Required name that fits <paramref name="maxLength"/> after trimming.</summary>
    public static IRuleBuilderOptions<T, string> Name<T>(this IRuleBuilderInitial<T, string> rule, int maxLength) =>
        rule.Cascade(CascadeMode.Stop)
            .Must(name => !string.IsNullOrWhiteSpace(name)).WithMessage("Enter a name.")
            .Must(name => name.Trim().Length <= maxLength).WithMessage($"Use at most {maxLength} characters.");

    public static IRuleBuilderOptions<T, string> Currency<T>(this IRuleBuilder<T, string> rule) =>
        rule.Must(Currencies.IsSupported).WithMessage("Choose a supported currency.");

    /// <summary>Signed amount (e.g. an opening balance) within the money limits.</summary>
    public static IRuleBuilderOptions<T, decimal> Balance<T>(this IRuleBuilder<T, decimal> rule) =>
        rule.Must(value => Math.Abs(value) <= MoneyRules.MaxAmount && MoneyRules.HasAtMostTwoDecimals(value))
            .WithMessage("Enter an amount with at most 2 decimal places.");
}
