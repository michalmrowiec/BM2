using System.Text.Json.Serialization;

namespace BM2.Shared.Requests.Commands.Record;

public abstract class AddBaseRecordCommand
{
    public Guid CategoryId { get; set; }
    public Guid StatusId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public decimal? PlannedAmount { get; set; }
    public Guid CurrencyId { get; set; }
    public List<Guid> TagIds { get; set; } = [];
    [JsonIgnore] public Guid OwnedByUserId { get; set; }
}