# Logging Standards

## Required Libraries

- Use `Tina.Logging` from `Tina.NetCore` for all logging.

## Mandatory Rules

All logging must:

- Use `ITransactionLogger`.
- Add custom parameters for context.
- Define custom parameter constants in `Infrastructure/CustomParameters.cs`.
- Never log sensitive information directly.

## Example

```csharp
public static class CustomParameters
{
    public const string EntityId = "entityId";
    public const string CompanyId = "companyId";
    public const string RootGroupId = "rootGroupId";
    public const string ActorId = "actorId";
    public const string EntityKafkaKey = "entityKafkaKey";
}

private readonly ITransactionLogger _logger;

_logger.AddCustomParameter(CustomParameters.EntityId, entityId.ToString());
_logger.AddCustomParameter(CustomParameters.CompanyId, companyId);
_logger.LogInformation("Processing entity operation");
```

## Review Checklist

- `ITransactionLogger` used consistently
- Context parameters added before log entry
- Sensitive data excluded
- New parameters added to central constants file
