using BM2.Shared.DTOs;

namespace BM2.Shared.Requests.Commands.Record;

public class UpdateRecordCommand : AddRecordCommand, IBaseRequest<RecordDTO>
{
    public Guid Id { get; set; }

    public UpdateRecordCommand()
    {
    }

    public UpdateRecordCommand(RecordDTO recordToUpdate)
    {
        Id = recordToUpdate.Id;
        Name = recordToUpdate.Name;
        Amount = recordToUpdate.Amount;
        PlannedAmount = recordToUpdate.PlannedAmount;
        AccountId = recordToUpdate.AccountId;
        CategoryId = recordToUpdate.CategoryId;
        CurrencyId = recordToUpdate.CurrencyId;
        StatusId = recordToUpdate.StatusId;
        Description = recordToUpdate.Description;
        RecordDateTime = recordToUpdate.RecordDateTime;
        TagIds = recordToUpdate.Tags.Select(t => t.Id).ToList();
    }
}