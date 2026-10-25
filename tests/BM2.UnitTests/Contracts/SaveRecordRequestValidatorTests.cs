using BM2.Contracts.Records;

namespace BM2.UnitTests.Contracts;

public class SaveRecordRequestValidatorTests
{
    private static readonly Guid Account = Guid.NewGuid(), Other = Guid.NewGuid(), Category = Guid.NewGuid();
    private readonly SaveRecordRequestValidator _validator = new();

    private static SaveRecordRequest Expense() => new(
        RecordType.Expense, RecordStatus.Completed, new DateOnly(2026, 10, 24), Account, 12.5m, "PLN", 12.5m, Category,
        null, null, null, []);

    private static SaveRecordRequest Transfer() => Expense() with
    {
        Type = RecordType.Transfer, CategoryId = null, TransferAccountId = Other,
    };

    private IEnumerable<string> ErrorsOf(SaveRecordRequest request) =>
        _validator.Validate(request).Errors.Select(e => e.PropertyName);

    [Fact]
    public void Valid_expense_and_transfer_pass()
    {
        ErrorsOf(Expense()).ShouldBeEmpty();
        ErrorsOf(Transfer()).ShouldBeEmpty();
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("1.001")]
    public void Amount_must_be_positive_with_two_decimals(string amount) =>
        ErrorsOf(Expense() with { Amount = decimal.Parse(amount, System.Globalization.CultureInfo.InvariantCulture) })
            .ShouldContain("Amount");

    [Fact]
    public void Expense_needs_a_category() => ErrorsOf(Expense() with { CategoryId = null }).ShouldContain("CategoryId");

    [Fact]
    public void Transfer_needs_a_different_target_account()
    {
        ErrorsOf(Transfer() with { TransferAccountId = null }).ShouldContain("TransferAccountId");
        ErrorsOf(Transfer() with { TransferAccountId = Account }).ShouldContain("TransferAccountId");
    }

    [Fact]
    public void Transfer_amount_is_validated_when_present() =>
        ErrorsOf(Transfer() with { TransferAmount = 0m }).ShouldContain("TransferAmount");

    [Fact]
    public void Name_is_limited_to_100_characters() => ErrorsOf(Expense() with { Name = new string('x', 101) }).ShouldContain("Name");

    [Fact]
    public void Tags_cannot_repeat()
    {
        var tag = Guid.NewGuid();

        ErrorsOf(Expense() with { TagIds = [tag, tag] }).ShouldContain("TagIds");
    }

    [Fact]
    public void Currency_must_be_supported() => ErrorsOf(Expense() with { Currency = "ABC" }).ShouldContain("Currency");

    [Fact]
    public void Missing_date_is_invalid() => ErrorsOf(Expense() with { OccurredOn = default }).ShouldContain("OccurredOn");
}
