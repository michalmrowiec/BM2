using BM2.Contracts.Accounts;

namespace BM2.UnitTests.Contracts;

public class AccountValidatorTests
{
    private readonly CreateAccountRequestValidator _create = new();
    private readonly UpdateAccountRequestValidator _update = new();

    [Fact]
    public void Negative_opening_balance_is_allowed() =>
        _create.Validate(new CreateAccountRequest("Card", "PLN", -250.5m)).IsValid.ShouldBeTrue();

    [Fact]
    public void Opening_balance_has_at_most_two_decimals() =>
        _create.Validate(new CreateAccountRequest("Cash", "PLN", 1.005m)).Errors.ShouldContain(e => e.PropertyName == "OpeningBalance");

    [Fact]
    public void Opening_balance_is_limited() =>
        _create.Validate(new CreateAccountRequest("Cash", "PLN", -1_000_000_000_000m)).IsValid.ShouldBeFalse();

    [Fact]
    public void Name_and_currency_are_validated()
    {
        var errors = _update.Validate(new UpdateAccountRequest(new string('x', 51), "XYZ", 0, false)).Errors;

        errors.Select(e => e.PropertyName).ShouldBe(["Name", "Currency"], ignoreOrder: true);
    }
}
