using BM2.Application.Contracts.Persistence;
using BM2.Domain.Entities.UserProfile;
using BM2.Domain.Entities.UserRecords;
using BM2.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace BM2.Infrastructure.Repositories;

public class RecordTagRelationRepository(
    BM2DbContext context) : GenericRepository<RecordTagRelation>(context), IRecordTagRelationRepository
{

}