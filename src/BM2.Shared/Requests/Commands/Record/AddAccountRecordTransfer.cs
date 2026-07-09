using System.Text.Json.Serialization;
using BM2.Shared.DTOs;

namespace BM2.Shared.Requests.Commands.Record;

public class AddAccountRecordTransfer : IBaseRequest<AccountRecordTransferDTO>
{
    public DateTime RecordDateTime { get; set; } = DateTime.Now;
    
    public Guid FromAccountId { get; set; }
    public Guid ToAccountId { get; set; }

    public decimal FromAmount { get; set; }
    public Guid FromCurrencyId { get; set; }
    public decimal ToAmount { get; set; }
    public Guid ToCurrencyId { get; set; }
    
    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    [JsonIgnore] public Guid OwnedByUserId { get; set; }
    
    
    public AddAccountRecordTransfer()
    {
    }
}