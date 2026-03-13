# AWS Kafka Standards

## Required Libraries

- Use `Tina.Kafka` (`700.*`) for all Kafka implementations.

## Mandatory Publisher Rules

All Kafka publishers must:

- Inherit from or inject `IKafkaProducer`.
- Use `Message<T>` for all published messages.
- Include `MessageHeaders` with `CompanyId` and `RootGroupId` where applicable.
- Log `topicKey` using custom parameters.

## Example

```csharp
public interface IEntityPublisher
{
    Task PublishEntityMessage(EntityMessage message, DateTime updatedDateTime, MessageHeaders headers = null);
}

public class EntityPublisher : IEntityPublisher
{
    private const string EntityTopic = "domain.entityv1";
    private readonly IKafkaProducer _messageProducer;
    private readonly ITransactionLogger _logger;

    public async Task PublishEntityMessage(EntityMessage message, DateTime updatedDateTime, MessageHeaders messageHeaders = null)
    {
        var topicKey = $"{message.CurrentEntity.EntityId}";
        messageHeaders ??= new MessageHeaders { CompanyId = (int)message.CompanyId };

        var kafkaMessage = new Message<EntityMessage>(
            EntityTopic,
            topicKey,
            message,
            updatedDateTime,
            messageHeaders);

        _logger.AddCustomParameter(CustomParameters.EntityKafkaKey, topicKey);
        await _messageProducer.Produce(kafkaMessage);
    }
}
```

## Registration

Always register Kafka in `Bootstrapper.cs`:

```csharp
services.AddTinaKafka();
```

## Review Checklist

- `IKafkaProducer` used
- `Message<T>` wrapper used
- Headers populated correctly
- Topic key logged with custom parameter
- `AddTinaKafka()` registration present
