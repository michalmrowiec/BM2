namespace BM2.Shared;

public class TransactionFilter
{
    public enum LogicalOperator
    {
        Or = 0,
        And = 1
    }

    public bool HideAccountTransfers { get; set; }
    
    public string? SearchText { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }

    // Multi-select filtry (puste listy oznaczają "brak filtra / wybierz wszystkie")
    public List<Guid> AccountIds { get; set; } = [];
    public List<Guid> StatusIds { get; set; } = [];
    public List<Guid> CategoryIds { get; set; } = [];
    public List<Guid> TagIds { get; set; } = [];
    public LogicalOperator TagOperator { get; set; } = LogicalOperator.Or;

    public bool IsEmpty() =>
        //string.IsNullOrEmpty(SearchText) &&
        HideAccountTransfers == false
        && !DateFrom.HasValue
        && !DateTo.HasValue
        && !AccountIds.Any()
        && !StatusIds.Any()
        && !CategoryIds.Any()
        && !TagIds.Any();
}