namespace LytxStandardsDemoApi.Data;

public interface IPostgresDbContext
{
    Task<T?> QueryFirst<T>(string sql, ConnectionTarget connectionTarget, object parameters) where T : class;
}
