using Microsoft.Extensions.Logging;

namespace LytxStandardsDemoApi.Infrastructure.Logging;

public sealed class TransactionLogger : ITransactionLogger
{
    private readonly ILogger<TransactionLogger> _logger;
    private readonly Dictionary<string, object?> _customParameters = new(StringComparer.OrdinalIgnoreCase);

    public TransactionLogger(ILogger<TransactionLogger> logger)
    {
        _logger = logger;
    }

    public void AddCustomParameter(string key, object? value)
    {
        _customParameters[key] = value;
    }

    public void LogInformation(string message)
    {
        using (_logger.BeginScope(new Dictionary<string, object?>(_customParameters)))
        {
            _logger.LogInformation(message);
        }
    }

    public void LogError(string message, Exception exception)
    {
        using (_logger.BeginScope(new Dictionary<string, object?>(_customParameters)))
        {
            _logger.LogError(exception, message);
        }
    }
}
