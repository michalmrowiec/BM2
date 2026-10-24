using BM2.Contracts.Records;

namespace BM2.UnitTests.Contracts;

public class RecordFilterTests
{
    [Fact]
    public void Query_string_contains_only_set_fields() =>
        new RecordFilter { From = new DateOnly(2026, 10, 1), Search = "a&b" }.ToQueryString().ShouldBe("?from=2026-10-01&search=a%26b");

    [Fact]
    public void Empty_filter_has_empty_query_string() => new RecordFilter().ToQueryString().ShouldBe("");

    [Fact]
    public void Enums_and_ids_are_written_by_name_and_value()
    {
        var tag = Guid.NewGuid();

        var query = new RecordFilter { TagId = tag, Type = RecordType.Expense, Status = RecordStatus.Planned }.ToQueryString();

        query.ShouldBe($"?tagId={tag}&type=Expense&status=Planned");
    }

    [Fact]
    public void Limit_is_written_last() =>
        new RecordFilter { Search = "x", Limit = 5 }.ToQueryString().ShouldBe("?search=x&limit=5");

    [Fact]
    public void Blank_search_is_left_out() => new RecordFilter { Search = "  " }.ToQueryString().ShouldBe("");
}
