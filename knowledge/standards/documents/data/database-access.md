# Database Access Standards

## Required Libraries

- Use `Tina.Data.PostgreSql` for PostgreSQL operations.
- Use `Tina.Data.MongoDb` for MongoDB operations when applicable.

## Mandatory Rules

All database operations must:

- Use the correct connection target (`ReadInstance` vs `WriteInstance`).
- Use parameterized queries.
- Handle database exceptions.
- Use async APIs.

## Example

```csharp
public class PostgreSqlEntityDataAccess : IEntityDataAccess
{
    private readonly IPostgresDbContext _context;

    public async Task<EntityDbModel> GetEntityById(Guid id)
    {
        const string sql = "SELECT * FROM schema.get_entity_by_id(@id)";
        var parameters = new { id };

        return await _context.QueryFirst<EntityDbModel>(sql, ConnectionTarget.ReadInstance, parameters);
    }

    public async Task<bool> UpdateEntity(EntityDbModel entity)
    {
        const string sql = "SELECT * FROM schema.update_entity(@entityData)";
        var parameters = new { entityData = JsonSerializer.Serialize(entity) };

        return await _context.QueryFirst<bool>(sql, ConnectionTarget.WriteInstance, parameters);
    }
}
```

## Registration

Register database contexts in `Bootstrapper.cs`:

```csharp
services.AddPostgreSqlDapper("PostgreSql");
```

## Review Checklist

- Connection target matches operation intent
- Query is parameterized
- Async data access used
- Registration present in startup
