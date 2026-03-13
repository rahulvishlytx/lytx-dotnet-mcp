namespace LytxStandardsDemoApi.Infrastructure.Logging;

public interface ITransactionLogger
{
    void AddCustomParameter(string key, object? value);
    void LogInformation(string message);
    void LogError(string message, Exception exception);
}
