namespace BM2.Application.Functions.Record.Commands.Requests;

public abstract class AddBaseRecordCommand
{
    public Guid CategoryId { get; set; }
    public Guid StatusId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public decimal? PlannedAmount { get; set; }
    public Guid CurrencyId { get; set; }
    public Guid OwnedByUserId { get; set; }
}