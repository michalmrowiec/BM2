using BM2.Application.Contracts.Persistence;
using BM2.Domain.Entities.UserRecords;
using BM2.Infrastructure.Repositories.Base;
using BM2.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BM2.Infrastructure.Repositories;

public class RecordRepository(
    BM2DbContext context) : GenericRepository<Record>(context), IRecordRepository
{
    public async Task<IReadOnlyList<Record>> GetAllForMonthAsync(Guid userId, int year, int month, Guid? walletId = null) =>
        await GetListByAsync(x =>
                x.OwnedByUserId == userId
                && x.RecordDateTime.Year == year
                && x.RecordDateTime.Month == month
                && (!walletId.HasValue || x.Account!.WalletId == walletId.Value),
            q =>
                q.Include(r => r.Currency)
                    .Include(r => r.Category)
                    .Include(r => r.Tags)
                    .Include(r => r.Status)
                    .Include(x => x.Account));
}