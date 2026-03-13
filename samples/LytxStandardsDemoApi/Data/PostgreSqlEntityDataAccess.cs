using System.Text.Json;
using LytxStandardsDemoApi.Models;

namespace LytxStandardsDemoApi.Data;

public sealed class PostgreSqlEntityDataAccess : IEntityDataAccess
{
    private readonly IPostgresDbContext _context;

    public PostgreSqlEntityDataAccess(IPostgresDbContext context)
    {
        _context = context;
    }

    public async Task<EntitySummaryDbModel?> GetEntityById(Guid id)
    {
        const string sql = "SELECT * FROM entity.get_entity_summary_by_id(@id)";
        var parameters = new { id };

        return await _context.QueryFirst<EntitySummaryDbModel>(sql, ConnectionTarget.ReadInstance, parameters);
    }

    public async Task<bool> UpdateEntity(EntitySummaryDbModel entity)
    {
        const string sql = "SELECT * FROM entity.update_entity_summary(@entityData)";
        var parameters = new { entityData = JsonSerializer.Serialize(entity) };

        var result = await _context.QueryFirst<EntitySummaryDbModel>(sql, ConnectionTarget.WriteInstance, parameters);
        return result is not null;
    }
}
