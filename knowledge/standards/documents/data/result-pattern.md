# Result Pattern Standards

## Required Libraries

- Use the custom `Result<T>` implementation in `Infrastructure/Result.cs`.

## Mandatory Rules

All service methods must:

- Never throw business exceptions to callers.
- Return `Result<T>` for operations that can fail.
- Use the correct `FailureReason` enum.
- Log errors before returning failure results.

## Example

```csharp
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
```

## Review Checklist

- Service returns `Result<T>`
- Failure paths mapped to `FailureReason`
- Errors logged before returning failure result
- Controllers translate result with `HandleResult`
