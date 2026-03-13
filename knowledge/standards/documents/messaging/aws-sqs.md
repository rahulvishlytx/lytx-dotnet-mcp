# AWS SQS Standards

## Required Libraries

- Use `Tina.Sqs` (`800.*`) for all SQS implementations.
- Add `AWSSDK.SQS` for AWS integration.

## Mandatory Handler Rules

All SQS handlers must:

- Inherit from `BaseJsonSqsMessageHandler<TRequest>`.
- Override `HandleMessage(SqsHandlerMessage<TRequest> message)`.
- Read payload using `message.Body`.
- Handle errors inside `HandleMessage`.
- Avoid creating handlers that bypass the base handler abstraction.

## Example

```csharp
public class YourReportHandler : BaseJsonSqsMessageHandler<YourReportRequest>
{
    public override async Task HandleMessage(SqsHandlerMessage<YourReportRequest> message)
    {
        var request = message.Body;
        // Business logic
    }
}
```

## Consumer Registration

Always register consumers with `AddSqsConsumer<THandler, TMessage>()` in `Bootstrapper.cs`.

```csharp
services.AddSqsConsumer<YourReportHandler, YourReportRequest>(
    new SqsConsumerConfiguration<YourReportRequest>
    {
        QueueName = "servicename-featurename",
        MaxNumberOfMessages = 10,
        RegionEndpoint = RegionEndpoint.USWest2,
        WaitTimeSeconds = 20,
        MaxAttempts = 100,
        DeadLetterQueueName = "servicename-featurename-deadletter"
    });
```

## Review Checklist

- Correct base handler used
- `message.Body` used for payload access
- Errors handled in handler
- Queue and dead-letter queue configured
- Registration added in `Bootstrapper.cs`
