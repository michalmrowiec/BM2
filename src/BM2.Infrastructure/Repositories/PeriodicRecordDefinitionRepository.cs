using BM2.Application.Contracts.Persistence;
using BM2.Domain.Entities.UserRecords;
using BM2.Infrastructure.Repositories.Base;

namespace BM2.Infrastructure.Repositories;

public class PeriodicRecordDefinitionRepository(
    BM2DbContext context) : GenericRepository<PeriodicRecordDefinition>(context), IPeriodicRecordDefinitionRepository
{
}
