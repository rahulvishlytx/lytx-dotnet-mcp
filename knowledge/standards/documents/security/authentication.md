# Authentication Standards

## Required Libraries

- Use `Tina.Authentication.NetCore` (`700.*`) for all authentication.

## Mandatory Controller Rules

All controllers must:

- Use `[Authorize]` on the controller class.
- Extract user context with `User.GetUniqueId()`, `User.GetRootGroupId()`, and `User.GetCompany()`.
- Pass user context into service methods.
- Never bypass authentication checks.

## Example

```csharp
[Authorize]
[ApiController]
[Route("entities")]
public class EntitiesController : ControllerBase
{
    [HttpGet("{id:Guid}")]
    public async Task<IActionResult> GetEntity([FromRoute] Guid id)
    {
        var result = await _entityService.GetEntityAsync(
            id,
            User.GetUniqueId(),
            User.GetRootGroupId(),
            User.GetCompany());

        return HandleResult(result);
    }
}
```

## Registration

Always register authentication in `Program.cs`:

```csharp
builder.UseTina();
builder.Services.AddTinaAuth();
```

## Review Checklist

- `[Authorize]` present
- User context extracted from `User`
- User context passed to service layer
- No alternate bypass path introduced
- Auth registration present in startup
