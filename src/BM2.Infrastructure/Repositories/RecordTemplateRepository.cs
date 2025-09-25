using BM2.Application.Contracts.Persistence;
using BM2.Domain.Entities.UserRecords;
using BM2.Infrastructure.Repositories.Base;
using BM2.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BM2.Infrastructure.Repositories;

public class RecordTemplateRepository(
    BM2DbContext context) : GenericRepository<RecordTemplate>(context), IRecordTemplateRepository
{
}