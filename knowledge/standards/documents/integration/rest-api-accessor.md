# REST API Accessor Standards

## Required Libraries

- Use `Tina.Networking` for all external service calls.
- Never use `HttpClient` directly in business logic.

## Mandatory Rules

All external service calls must:

- Use `IRestApiAccessor`.
- Handle exceptions gracefully.
- Log errors through `ITransactionLogger`.
- Avoid exposing raw HTTP responses to business logic.

## Example

```csharp
public class ExternalServiceAccessor : IExternalServiceAccessor
{
    private readonly IRestApiAccessor _restApiAccessor;
    private readonly string _endpointUrl;
    private readonly ITransactionLogger _logger;

    public async Task<ExternalResponse> GetData(Guid id)
    {
        try
        {
            return await _restApiAccessor.Get<ExternalResponse>(
                $"{_endpointUrl}/api/v1/data/{id}");
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to get external data", ex);
            return null;
        }
    }
}
```

## Registration Pattern

Always create factory methods in `Bootstrapper.cs`:

```csharp
private static IExternalServiceAccessor ExternalServiceAccessorFactory(IServiceProvider provider)
{
    var url = provider.GetService<ICloudUrlProvider>()
        .GetInternalUrl("Lytx.External.Service");
    var restApiAccessor = GetRestApiAccessor(provider, url, "External Service");
    return new ExternalServiceAccessor(restApiAccessor, url);
}
```

## Review Checklist

- `IRestApiAccessor` used
- Exceptions caught and logged
- No raw `HttpClient` usage
- Factory registration present
