using BM2.Shared.DTOs;
using BM2.Shared.SystemCodes;
using System.ComponentModel.DataAnnotations;

namespace BM2.Shared.Requests.Commands.Record;

public class AddPeriodicRecordDefinitionCommand : IBaseRequest<PeriodicRecordDefinitionDTO>
{
    public Guid Id { get; set; }
    public Guid RecordTemplateId { get; set; }
    public Guid CurrencyId { get; set; }
    public Guid PeriodicRecordStatusId { get; set; } // Status of periodic record
    public Guid SetRecordStatusId { get; set; } // Set status for created record
    public Guid WalletId { get; set; }
    public Guid SetRecordAccountId { get; set; } // Set account for created record
    public Guid OwnedByUserId { get; set; }
    [Required]
    public Periodicity? Periodicity { get; set; }
    public DateTime StartDate { get; set; }

    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
}