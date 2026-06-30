namespace BM2.Shared;

public class TransactionFilter
{
    public string? SearchText { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }

    // Multi-select filtry (puste listy oznaczają "brak filtra / wybierz wszystkie")
    public List<Guid> AccountIds { get; set; } = new();
    public List<Guid> CategoryIds { get; set; } = new();
    public List<Guid> TagIds { get; set; } = new();
}
