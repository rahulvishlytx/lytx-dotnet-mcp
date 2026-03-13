
AWS SQS Standards
Required Libraries

Use Tina.Sqs (800.*) for all SQS implementations

Add AWSSDK.SQS for AWS integration

Handler Rules

All SQS handlers must:

Always Inherit from BaseJsonSqsMessageHandler<TRequest>. Do not create handler without this base class.

Always Override HandleMessage(SqsHandlerMessage<TRequest> message) with SqsHandlerMessage<TRequest> parameter. 

Read payload using message.Body

Handle errors inside HandleMessage

public class YourReportHandler : BaseJsonSqsMessageHandler<YourReportRequest>
{
    public override async Task HandleMessage(SqsHandlerMessage<YourReportRequest> message)
    {
        var request = message.Body;
        // Business logic
    }
}

All above handler rules must be followed without exception.

Consumer Registration

Always register consumers using .AddSqsConsumer<THandler, TMessage>() with the following configuration:

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

Consumer configuration must follow the above template in Bootstrapper.cs.

AWS Kafka Standards
Required Libraries

Use Tina.Kafka (700.*) for all Kafka implementations

Handler Rules

All Kafka publishers must:

Always inherit from or inject IKafkaProducer interface

Always use Message<T> wrapper for all published messages

Always include MessageHeaders with CompanyId, RootGroupId

Always log topicKey using custom parameters

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

All above publisher rules must be followed without exception.

Producer Registration

Always register Kafka using .AddTinaKafka() in Bootstrapper.cs:

services.AddTinaKafka();

Authentication Standards
Required Libraries

Use Tina.Authentication.NetCore (700.*) for all authentication

Handler Rules

All controllers must:

Always use [Authorize] attribute on controller class

Always extract user info using User.GetUniqueId(), User.GetRootGroupId(), User.GetCompany()

Always pass user context to service methods

Never bypass authentication checks

[Authorize]
[ApiController]
[Route("entities")]
public class EntitiesController : ControllerBase
{
    [HttpGet("{id:Guid}")]
    public async Task<IActionResult> GetEntity([FromRoute] Guid id)
    {
        var result = await _entityService.GetEntityAsync(id, User.GetUniqueId(), 
            User.GetRootGroupId(), User.GetCompany());
        return HandleResult(result);
    }
}

All above authentication rules must be followed without exception.

Authentication Registration

Always register authentication using .AddTinaAuth() in Program.cs:

builder.UseTina();
builder.Services.AddTinaAuth();

Feature Toggle Standards
Required Libraries

Use Tina.FeatureToggle.NetCore (700.*) for all feature toggles

Handler Rules

All new features must:

Always be behind a feature toggle

Always define toggle keys in Infrastructure/FeatureToggleKeys.cs

Always check feature toggle before executing new functionality

Always provide fallback to existing behavior

public static class FeatureToggleKeys
{
    public const string EnableNewFeature = "enable-new-feature";
}

private readonly IFeatureToggleCollection _featureToggleCollection;

private bool IsFeatureEnabled(string featureKey, Guid? groupId = null)
{
    return groupId.HasValue
        ? _featureToggleCollection.IsFeatureEnabled(featureKey, groupId.Value)
        : _featureToggleCollection.IsFeatureEnabled(featureKey);
}

if (IsFeatureEnabled(FeatureToggleKeys.EnableNewFeature, rootGroupId))
{
    // New feature implementation
}
else
{
    // Legacy implementation
}

All above feature toggle rules must be followed without exception.

Feature Toggle Registration

Always register feature toggles in Bootstrapper.cs and Program.cs:

services.AddSingleton(GetFeatureToggleCollection);
builder.Configuration.AddJsonFile("apptoggles.json", optional: false, reloadOnChange: true);
builder.Services.AddFeatureManagement();

REST API Accessor Standards
Required Libraries

Use Tina.Networking for all external service calls

Never use HttpClient directly

Handler Rules

All external service calls must:

Always use IRestApiAccessor interface

Always handle exceptions gracefully

Always log errors using ITransactionLogger

Never expose raw HTTP responses to business logic

public class ExternalServiceAccessor : IExternalServiceAccessor
{
    private readonly IRestApiAccessor _restApiAccessor;
    private readonly string _endpointUrl;
    private readonly ITransactionLogger _logger;

    public async Task<ExternalServiceAccessor> GetData(Guid id)
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

All above REST API accessor rules must be followed without exception.

Accessor Registration

Always create factory methods in Bootstrapper.cs:

private static IExternalServiceAccessor ExternalServiceAccessorFactory(IServiceProvider provider)
{
    var url = provider.GetService<ICloudUrlProvider>()
        .GetInternalUrl("Lytx.External.Service");
    var restApiAccessor = GetRestApiAccessor(provider, url, "External Service");
    return new ExternalServiceAccessor(restApiAccessor, url);
}

S3 Storage Standards
Required Libraries

Use AWSSDK.S3 and AWSSDK.Extensions.NETCore.Setup for S3 integration

Handler Rules

All S3 operations must:

Always use IAmazonS3 interface

Always handle compression for large objects

Always use proper disposal patterns

Always log S3 operations

public interface IStorageService
{
    Task<T> GetObjectFromFile<T>(string key, bool isCompressed = false);
    Task UploadFileFromStream(string key, Stream stream, bool compress = false);
}

public class S3StorageService : IStorageService
{
    private readonly string _bucketName;
    private readonly IAmazonS3 _s3Client;
    private readonly ITransactionLogger _logger;

    public async Task<T> GetObjectFromFile<T>(string key, bool isCompressed = false)
    {
        _logger.AddCustomParameter(CustomParameters.S3Key, key);
        
        var getObjectRequest = new GetObjectRequest
        {
            BucketName = _bucketName,
            Key = key
        };

        using var response = await _s3Client.GetObjectAsync(getObjectRequest);
        using var responseStream = response.ResponseStream;
        
        Stream streamToRead = isCompressed 
            ? new GZipStream(responseStream, CompressionMode.Decompress)
            : responseStream;

        using var reader = new StreamReader(streamToRead);
        var content = await reader.ReadToEndAsync();
        return JsonSerializer.Deserialize<T>(content);
    }
}

All above S3 storage rules must be followed without exception.

S3 Registration

Always register S3 services in Bootstrapper.cs:

services.AddAWSService<IAmazonS3>();
services.AddSingleton(StorageServiceFactory);

Logging Standards
Required Libraries

Use Tina.Logging (included in Tina.NetCore) for all logging

Handler Rules

All logging must:

Always use ITransactionLogger interface

Always add custom parameters for context

Always define parameters in Infrastructure/CustomParameters.cs

Never log sensitive information directly

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

All above logging rules must be followed without exception.

Database Access Standards
Required Libraries

Use Tina.Data.PostgreSql for PostgreSQL operations

Use Tina.Data.MongoDb for MongoDB operations (if applicable)

Handler Rules

All database operations must:

Always use appropriate connection targets (ReadInstance vs WriteInstance)

Always use parameterized queries

Always handle database exceptions

Always use async/await patterns

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

All above database access rules must be followed without exception.

Database Registration

Always register database contexts in Bootstrapper.cs:

services.AddPostgreSqlDapper("PostgreSql");

Result Pattern Standards
Required Libraries

Use custom Result<T> implementation in Infrastructure/Result.cs

Handler Rules

All service methods must:

Never throw exceptions from business logic

Always return Result<T> for operations that can fail

Always use appropriate FailureReason enums

Always log errors before returning failure results

public async Task<Result<Entity>> GetEntityAsync(Guid entityId)
{
    try
    {
        var entity = await _dataAccess.GetEntityById(entityId);
        return entity == null 
            ? Result<Entity>.FailWith(FailureReason.NotFound)
            : Result<Entity>.SuccessWith(entity);
    }
    catch (Exception ex)
    {
        _logger.LogError("Error retrieving entity", ex);
        return Result<Entity>.FailWith(FailureReason.InternalServerError);
    }
}

protected IActionResult HandleResult<T>(Result<T> result)
{
    if (result.IsSuccess)
        return Ok(result);
        
    return result.FailureReason switch
    {
        FailureReason.NotFound => NotFound(result),
        FailureReason.AccessDenied => StatusCode(StatusCodes.Status403Forbidden, result),
        FailureReason.BadRequest => BadRequest(result),
        _ => StatusCode(StatusCodes.Status500InternalServerError, result)
    };
}

All above result pattern rules must be followed without exception.
