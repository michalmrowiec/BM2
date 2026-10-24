using BM2.Contracts.Wallets;

namespace BM2.UnitTests.Contracts;

public class WalletValidatorTests
{
    private readonly CreateWalletRequestValidator _validator = new();

    [Fact]
    public void Name_is_required_after_trim() =>
        _validator.Validate(new CreateWalletRequest("   ", "PLN")).Errors.ShouldContain(e => e.PropertyName == "Name");

    [Fact]
    public void Missing_name_is_invalid() =>
        _validator.Validate(new CreateWalletRequest(null!, "PLN")).Errors.ShouldContain(e => e.PropertyName == "Name");

    [Fact]
    public void Name_is_at_most_50_characters()
    {
        _validator.Validate(new CreateWalletRequest(new string('x', 50), "PLN")).IsValid.ShouldBeTrue();
        _validator.Validate(new CreateWalletRequest(new string('x', 51), "PLN")).IsValid.ShouldBeFalse();
    }

    [Fact]
    public void Currency_must_be_supported() =>
        _validator.Validate(new CreateWalletRequest("Home", "XYZ")).Errors.ShouldContain(e => e.PropertyName == "BaseCurrency");

    [Fact]
    public void Update_uses_the_same_rules() =>
        new UpdateWalletRequestValidator().Validate(new UpdateWalletRequest("", "XYZ")).Errors.Count.ShouldBe(2);
}
