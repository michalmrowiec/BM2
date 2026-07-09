using BM2.Application.Mappings;
using BM2.Domain.Entities.UserRecords;
using BM2.Infrastructure;
using BM2.Shared;
using BM2.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BM2.Services;

public class RecordService(BM2DbContext _context) : IRecordService
{
    private IQueryable<Record> GetQuery(Guid walletId, TransactionFilter filter)
    {
        var query = _context.Records
            .Include(r => r.Category)
            .Include(r => r.Currency)
            .Include(r => r.Status)
            .Include(r => r.Tags)
            .Include(r => r.Account)
                .ThenInclude(a => a.DefaultCurrency)
            .Where(r => r.Account!.WalletId == walletId)
            .AsNoTracking()
            .AsQueryable();

        // 1. Global Search
        if (!string.IsNullOrWhiteSpace(filter.SearchText))
        {
            string search = filter.SearchText.ToLower();
            query = query.Where(r => r.Name.ToLower().Contains(search)
                                     || (r.Description != null && r.Description.ToLower().Contains(search)));
        }

        // 2. Filtry dat
        if (filter.DateFrom.HasValue) query = query.Where(r => r.RecordDateTime >= filter.DateFrom.Value);
        if (filter.DateTo.HasValue) query = query.Where(r => r.RecordDateTime <= filter.DateTo.Value);

        // 3. Multi-select Konta (Jeśli lista nie jest pusta, filtruj baze)
        if (filter.AccountIds.Any())
        {
            query = query.Where(r => filter.AccountIds.Contains(r.AccountId));
        }
        
        if (filter.StatusIds.Any())
        {
            query = query.Where(r => filter.StatusIds.Contains(r.StatusId));
        }

        // 4. Multi-select Kategorie
        if (filter.CategoryIds.Any())
        {
            query = query.Where(r => r.CategoryId.HasValue && filter.CategoryIds.Contains(r.CategoryId.Value));
        }

        // 5. Multi-select Tagi (Rekord posiada chociaż jeden z wybranych tagów)
        if (filter.TagIds.Any())
        {
            if (filter.TagOperator == TransactionFilter.LogicalOperator.Or)
                query = query.Where(r => r.Tags.Any(t => filter.TagIds.Contains(t.Id)));
            else
                query = query.Where(r => filter.TagIds.All(id => r.Tags.Any(t => t.Id == id)));
        }

        return query;
    }
    
    public async Task<(List<RecordDTO> Items, int TotalCount)> GetPagedRecordsAsync(
        int page, int pageSize, string? sortBy, bool sortDescending, Guid walletId, TransactionFilter filter)
    {
        var query = GetQuery(walletId, filter);

        // 6. Pobranie sumy i dynamiczne sortowanie (standardowe podejście MudBlazor)
        int totalCount = await query.CountAsync();

        // Prosty switch dla sortowania kolumn
        query = sortBy switch
        {
            "Name" => sortDescending ? query.OrderByDescending(r => r.Name) : query.OrderBy(r => r.Name),
            "Amount" => sortDescending ? query.OrderByDescending(r => r.Amount) : query.OrderBy(r => r.Amount),
            "Account" => sortDescending ? query.OrderByDescending(r => r.Account!.AccountName) : query.OrderBy(r => r.Account!.AccountName),
            "Status" => sortDescending ? query.OrderByDescending(r => r.Status!.RecordStatusName) : query.OrderBy(r => r.Status!.RecordStatusName),
            _ => sortDescending ? query.OrderByDescending(r => r.RecordDateTime) : query.OrderBy(r => r.RecordDateTime)
        };

        // 7. Paginacja i wykonanie w DB
        var dbItems = await query.Skip(page * pageSize).Take(pageSize).ToListAsync();

        // Mapowanie na DTO
        var dtos = dbItems.Select(r => r.ToDto()).ToList();

        return (dtos, totalCount);
    }

    public Task<decimal> GetSum(Guid walletId, TransactionFilter filter)
    {
        var query = GetQuery(walletId, filter);
        return query.SumAsync(r => r.Amount);
    }

    public async Task<List<(AccountDTO, decimal)>> GetSumForAccounts(Guid walletId, TransactionFilter filter)
    {
        var query = GetQuery(walletId, filter);

        var groupedResult = await query
            .GroupBy(r => r.Account)
            .Select(g => new
            {
                Account = g.Key,
                TotalAmount = g.Sum(x => x.AccountAmount)
            })
            .ToListAsync();
        
        return groupedResult
            .Select(r => (r.Account!.ToDto(), r.TotalAmount))
            .ToList();
    }
}
