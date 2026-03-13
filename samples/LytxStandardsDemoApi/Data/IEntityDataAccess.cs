using LytxStandardsDemoApi.Models;

namespace LytxStandardsDemoApi.Data;

public interface IEntityDataAccess
{
    Task<EntitySummaryDbModel?> GetEntityById(Guid id);
}
