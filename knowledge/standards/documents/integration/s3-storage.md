# S3 Storage Standards

## Required Libraries

- Use `AWSSDK.S3` and `AWSSDK.Extensions.NETCore.Setup` for S3 integration.

## Mandatory Rules

All S3 operations must:

- Use `IAmazonS3`.
- Handle compression for large objects.
- Use proper disposal patterns.
- Log S3 operations.

## Example

```csharp
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
```

## Registration

Always register S3 services in `Bootstrapper.cs`:

```csharp
services.AddAWSService<IAmazonS3>();
services.AddSingleton(StorageServiceFactory);
```

## Review Checklist

- `IAmazonS3` used
- Compression path handled
- Streams disposed correctly
- S3 context logged
- Registration present
