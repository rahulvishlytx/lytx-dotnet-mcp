using LytxStandardsDemoApi.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace LytxStandardsDemoApi.Controllers;

[ApiController]
public abstract class LytxControllerBase : ControllerBase
{
    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return result.FailureReason switch
        {
            FailureReason.NotFound => NotFound(result.ErrorMessage),
            FailureReason.AccessDenied => StatusCode(StatusCodes.Status403Forbidden, result.ErrorMessage),
            FailureReason.BadRequest => BadRequest(result.ErrorMessage),
            FailureReason.FeatureDisabled => StatusCode(StatusCodes.Status409Conflict, result.ErrorMessage),
            _ => StatusCode(StatusCodes.Status500InternalServerError, result.ErrorMessage)
        };
    }
}
