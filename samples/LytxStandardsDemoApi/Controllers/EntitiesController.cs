using LytxStandardsDemoApi.Infrastructure.Security;
using LytxStandardsDemoApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LytxStandardsDemoApi.Controllers;

[Authorize]
[Route("entities")]
public sealed class EntitiesController : LytxControllerBase
{
    private readonly IEntitySummaryService _entitySummaryService;

    public EntitiesController(IEntitySummaryService entitySummaryService)
    {
        _entitySummaryService = entitySummaryService;
    }

    [HttpGet("{id:guid}/summary")]
    public async Task<IActionResult> GetEntitySummary([FromRoute] Guid id)
    {
        var result = await _entitySummaryService.GetEntitySummaryAsync(
            id,
            User.GetUniqueId(),
            User.GetRootGroupId(),
            User.GetCompany());

        return HandleResult(result);
    }
}
