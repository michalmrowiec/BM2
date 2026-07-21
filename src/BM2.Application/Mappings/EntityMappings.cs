using BM2.Domain.Entities.System;
using BM2.Domain.Entities.UserProfile;
using BM2.Domain.Entities.UserRecords;
using BM2.Shared.DTOs;
using BM2.Shared.Requests.Commands.Account;
using BM2.Shared.Requests.Commands.Category;
using BM2.Shared.Requests.Commands.Record;
using BM2.Shared.Requests.Commands.Tag;
using BM2.Shared.Requests.Commands.User;
using BM2.Shared.Requests.Commands.Wallet;

namespace BM2.Application.Mappings;

public static class EntityMappings
{
    public static CurrencyDTO ToDto(this Currency currency) => new()
    {
        Id = currency.Id,
        Name = currency.Name,
        Symbol = currency.Symbol,
        IsoCode = currency.IsoCode,
        Country = currency.Country
    };

    public static RecordStatusDTO ToDto(this RecordStatus status) => new()
    {
        Id = status.Id,
        SystemCode = status.SystemCode,
        RecordStatusName = status.RecordStatusName,
        ForRecords = status.ForRecords,
        ForPeriodicRecord = status.ForPeriodicRecord
    };

    public static User ToEntity(this AddUserCommand command) => new()
    {
        EmailAddress = command.EmailAddress
    };

    public static UserDTO ToDto(this User user) => new();

    public static Wallet ToEntity(this AddWalletCommand command) => new()
    {
        WalletName = command.WalletName,
        IsActive = command.IsActive,
        DefaultCurrencyId = command.DefaultCurrencyId,
        OwnedByUserId = command.OwnedByUserId
    };

    public static void Apply(this Wallet wallet, UpdateWalletCommand command)
    {
        wallet.WalletName = command.WalletName;
        wallet.IsActive = command.IsActive;
        wallet.DefaultCurrencyId = command.DefaultCurrencyId;
    }

    public static WalletBaseDTO ToBaseDto(this Wallet wallet) => new()
    {
        Id = wallet.Id,
        WalletName = wallet.WalletName,
        IsActive = wallet.IsActive,
        DefaultCurrencyId = wallet.DefaultCurrencyId,
        DefaultCurrency = wallet.DefaultCurrency?.ToDto()
    };

    public static WalletDTO ToDto(this Wallet wallet) => new()
    {
        Id = wallet.Id,
        WalletName = wallet.WalletName,
        IsActive = wallet.IsActive,
        DefaultCurrencyId = wallet.DefaultCurrencyId,
        DefaultCurrency = wallet.DefaultCurrency?.ToDto(),
        Accounts = wallet.Accounts.Select(x => x.ToBasicDto()).ToList()
    };

    public static Account ToEntity(this BaseAccountCommand command) => new()
    {
        AccountName = command.AccountName,
        WalletId = command.WalletId,
        IsActive = command.IsActive,
        DefaultCurrencyId = command.DefaultCurrencyId,
        OwnedByUserId = command.OwnedByUserId
    };

    public static void Apply(this Account account, BaseAccountCommand command)
    {
        account.AccountName = command.AccountName;
        account.WalletId = command.WalletId;
        account.IsActive = command.IsActive;
        //account.DefaultCurrencyId = command.DefaultCurrencyId; brak możliwości edycji
    }

    public static AccountBasicDTO ToBasicDto(this Account account) => new()
    {
        Id = account.Id,
        AccountName = account.AccountName,
        WalletId = account.WalletId,
        IsActive = account.IsActive,
        DefaultCurrencyId = account.DefaultCurrencyId,
        DefaultCurrency = account.DefaultCurrency?.ToDto()
    };

    public static AccountDTO ToDto(this Account account) => new()
    {
        Id = account.Id,
        AccountName = account.AccountName,
        WalletId = account.WalletId,
        IsActive = account.IsActive,
        DefaultCurrencyId = account.DefaultCurrencyId,
        DefaultCurrency = account.DefaultCurrency?.ToDto(),
        Wallet = account.Wallet?.ToBaseDto()
    };

    public static Category ToEntity(this AddCategoryCommand command) => new()
    {
        CategoryName = command.CategoryName,
        OwnedByUserId = command.OwnedByUserId
    };

    public static CategoryDTO ToDto(this Category category) => new()
    {
        Id = category.Id,
        CategoryName = category.CategoryName
    };

    public static Tag ToEntity(this AddTagCommand command) => new()
    {
        TagName = command.TagName,
        OwnedByUserId = command.OwnedByUserId
    };

    public static TagDTO ToDto(this Tag tag) => new()
    {
        Id = tag.Id,
        TagName = tag.TagName
    };

    public static Record ToEntity(this AddRecordCommand command) => new()
    {
        AccountId = command.AccountId,
        RecordDateTime = command.RecordDateTime,
        CategoryId = command.CategoryId,
        StatusId = command.StatusId,
        Name = command.Name,
        Description = command.Description,
        AccountAmount = command.AccountAmount,
        Amount = command.Amount,
        PlannedAmount = command.PlannedAmount,
        CurrencyId = command.CurrencyId,
        OwnedByUserId = command.OwnedByUserId
    };

    public static RecordTemplate ToEntity(this AddRecordTemplateCommand command) => new()
    {
        WalletId = command.WalletId,
        CategoryId = command.CategoryId,
        StatusId = command.StatusId,
        Name = command.Name,
        Description = command.Description,
        Amount = command.Amount,
        PlannedAmount = command.PlannedAmount,
        CurrencyId = command.CurrencyId,
        OwnedByUserId = command.OwnedByUserId
    };

    public static void Apply(this Record record, UpdateRecordCommand command)
    {
        record.AccountId = command.AccountId;
        record.RecordDateTime = command.RecordDateTime;
        ApplyBaseRecord(record, command);
    }

    public static void Apply(this RecordTemplate recordTemplate, UpdateRecordTemplateCommand command)
    {
        recordTemplate.WalletId = command.WalletId;
        ApplyBaseRecord(recordTemplate, command);
    }

    public static RecordDTO ToDto(this Record record)
    {
        var dto = new RecordDTO
        {
            AccountId = record.AccountId,
            RecordDateTime = record.RecordDateTime,
            AccountRecordTransferId = record.AccountRecordTransferId,
        };

        MapBaseRecord(record, dto);
        return dto;
    }

    public static RecordTemplateDTO ToDto(this RecordTemplate recordTemplate)
    {
        var dto = new RecordTemplateDTO
        {
            WalletId = recordTemplate.WalletId,
            Wallet = recordTemplate.Wallet?.ToBaseDto()
        };

        MapBaseRecord(recordTemplate, dto);
        return dto;
    }

    public static PeriodicRecordDefinition ToEntity(this AddPeriodicRecordDefinitionCommand command) => new()
    {
        Id = command.Id,
        RecordTemplateId = command.RecordTemplateId,
        CurrencyId = command.CurrencyId,
        PeriodicRecordStatusId = command.PeriodicRecordStatusId,
        SetRecordStatusId = command.SetRecordStatusId,
        WalletId = command.WalletId,
        SetRecordAccountId = command.SetRecordAccountId,
        OwnedByUserId = command.OwnedByUserId,
        Periodicity = command.Periodicity!.Value,
        StartDate = command.StartDate
    };

    public static void Apply(this PeriodicRecordDefinition entity, UpdatePeriodicRecordDefinitionCommand command)
    {
        entity.RecordTemplateId = command.RecordTemplateId;
        entity.CurrencyId = command.CurrencyId;
        entity.PeriodicRecordStatusId = command.PeriodicRecordStatusId;
        entity.SetRecordStatusId = command.SetRecordStatusId;
        entity.WalletId = command.WalletId;
        entity.SetRecordAccountId = command.SetRecordAccountId;
        entity.Periodicity = command.Periodicity!.Value;
        entity.StartDate = command.StartDate;
    }

    public static PeriodicRecordDefinitionDTO ToDto(this PeriodicRecordDefinition entity) => new()
    {
        Id = entity.Id,
        RecordTemplateId = entity.RecordTemplateId,
        CurrencyId = entity.CurrencyId,
        PeriodicRecordStatusId = entity.PeriodicRecordStatusId,
        SetRecordStatusId = entity.SetRecordStatusId,
        WalletId = entity.WalletId,
        SetRecordAccountId = entity.SetRecordAccountId,
        Periodicity = entity.Periodicity,
        StartDate = entity.StartDate,
        RecordTemplate = entity.RecordTemplate?.ToDto(),
        Currency = entity.Currency?.ToDto(),
        PeriodicRecordStatus = entity.PeriodicRecordStatus?.ToDto(),
        SetRecordStatus = entity.SetRecordStatus?.ToDto(),
        Wallet = entity.Wallet?.ToBaseDto(),
        SetRecordAccount = entity.SetRecordAccount?.ToBasicDto()
    };

    private static void ApplyBaseRecord(BaseRecord record, AddBaseRecordCommand command)
    {
        record.CategoryId = command.CategoryId;
        record.StatusId = command.StatusId;
        record.Name = command.Name;
        record.Description = command.Description;
        record.Amount = command.Amount;
        record.PlannedAmount = command.PlannedAmount;
        record.CurrencyId = command.CurrencyId;
    }

    private static void MapBaseRecord(BaseRecord record, BaseRecordDTO dto)
    {
        dto.Id = record.Id;
        dto.CategoryId = record.CategoryId;
        dto.StatusId = record.StatusId;
        dto.Name = record.Name;
        dto.Description = record.Description;
        dto.Amount = record.Amount;
        dto.PlannedAmount = record.PlannedAmount;
        dto.CurrencyId = record.CurrencyId;
        dto.Currency = record.Currency?.ToDto();
        dto.Category = record.Category?.ToDto();
        dto.Status = record.Status?.ToDto();
        dto.Tags = record.Tags.Select(x => x.ToDto()).ToList();
    }
    
    public static Record ToFromTransferRecordEntity(this AddAccountRecordTransferCommand command, Guid statusId) => new()
    {
        Id = Guid.NewGuid(),
        AccountId = command.FromAccountId,
        RecordDateTime = command.RecordDateTime,
        CategoryId = null,
        StatusId = statusId,
        Name = command.Name,
        Description = command.Description,
        AccountAmount = Math.Abs(command.FromAmount) * -1,
        Amount = Math.Abs(command.FromAmount) * -1,
        PlannedAmount = 0m,
        CurrencyId = command.FromCurrencyId,
        OwnedByUserId = command.OwnedByUserId,
        CreatedAt = DateTime.UtcNow,
        CreatedBy = command.OwnedByUserId,
    };
    
    public static Record ToToTransferRecordEntity(this AddAccountRecordTransferCommand command, Guid statusId) => new()
    {
        Id = Guid.NewGuid(),
        AccountId = command.ToAccountId,
        RecordDateTime = command.RecordDateTime,
        CategoryId = null,
        StatusId = statusId,
        Name = command.Name,
        Description = command.Description,
        AccountAmount = Math.Abs(command.ToAmount),
        Amount = Math.Abs(command.ToAmount),
        PlannedAmount = 0m,
        CurrencyId = command.ToCurrencyId,
        OwnedByUserId = command.OwnedByUserId,
        CreatedAt = DateTime.UtcNow,
        CreatedBy = command.OwnedByUserId,
    };

    public static AccountRecordTransferDTO ToDto(this AccountRecordTransfer entity) => new()
    {
        Id = entity.Id,
        FromAccountId = entity.FromRecord.AccountId,
        FromAccountName = entity.FromRecord.Account?.AccountName ?? string.Empty,
        ToAccountId = entity.ToRecord.AccountId,
        ToAccountName = entity.ToRecord.Account?.AccountName ?? string.Empty,
        FromAmount = entity.FromRecord.AccountAmount,
        FromCurrencyId =  entity.FromRecord.CurrencyId,
        ToAmount = entity.ToRecord.AccountAmount,
        ToCurrencyId = entity.ToRecord.CurrencyId,
        RecordDateTime = entity.ToRecord.RecordDateTime,
        Name = entity.ToRecord.Name,
        Description = entity.ToRecord.Description,
    };
}
