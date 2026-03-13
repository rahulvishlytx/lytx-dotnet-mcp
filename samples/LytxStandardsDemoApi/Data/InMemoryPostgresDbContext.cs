using LytxStandardsDemoApi.Models;

namespace LytxStandardsDemoApi.Data;

public sealed class InMemoryPostgresDbContext : IPostgresDbContext
{
    private readonly Dictionary<Guid, EntitySummaryDbModel> _entities = new()
    {
        [Guid.Parse("44444444-4444-4444-4444-444444444444")] = new EntitySummaryDbModel
        {
            EntityId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
            Name = "Northwest Fleet Vehicle",
            Status = "Active",
            UpdatedAtUtc = DateTimeOffset.UtcNow.AddMinutes(-15)
        },
        [Guid.Parse("55555555-5555-5555-5555-555555555555")] = new EntitySummaryDbModel
        {
            EntityId = Guid.Parse("55555555-5555-5555-5555-555555555555"),
            Name = "Regional Trailer Unit",
            Status = "Maintenance",
            UpdatedAtUtc = DateTimeOffset.UtcNow.AddHours(-2)
        }
    };

    public Task<T?> QueryFirst<T>(string sql, ConnectionTarget connectionTarget, object parameters) where T : class
    {
        if (typeof(T) == typeof(EntitySummaryDbModel))
        {
            var entityId = (Guid)parameters.GetType().GetProperty("id")!.GetValue(parameters)!;
            _entities.TryGetValue(entityId, out var value);
            return Task.FromResult(value as T);
        }

        return Task.FromResult<T?>(null);
    }
}
