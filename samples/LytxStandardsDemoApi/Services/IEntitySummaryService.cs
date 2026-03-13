using LytxStandardsDemoApi.Infrastructure;
using LytxStandardsDemoApi.Models;

namespace LytxStandardsDemoApi.Services;

public interface IEntitySummaryService
{
    Task<Result<EntitySummary>> GetEntitySummaryAsync(Guid entityId, Guid actorId, Guid rootGroupId, int companyId);
}
