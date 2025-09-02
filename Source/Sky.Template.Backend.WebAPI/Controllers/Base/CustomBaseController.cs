using Microsoft.AspNetCore.Mvc;
using Sky.Template.Backend.Core.BaseResponse;
using System.Net;

namespace Sky.Template.Backend.WebAPI.Controllers.Base;

public class CustomBaseController : ControllerBase
{

    protected async Task<IActionResult> HandleServiceResponseAsync<T>(Func<Task<BaseControllerResponse<T>>> serviceMethod)
    {
        if (serviceMethod is null)
        {
            throw new ArgumentNullException(nameof(serviceMethod), "ServiceMethodCannotBeNull");
        }

        var response = await serviceMethod();
        if (response is null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "ResponseWasNull");
        }

        return StatusCodeFromResponse(response.StatusCode, response);
    }

    protected async Task<IActionResult> HandleServiceResponseAsync(Func<Task<BaseControllerResponse>> serviceMethod)
    {
        if (serviceMethod is null)
        {
            throw new ArgumentNullException(nameof(serviceMethod), "ServiceMethodCannotBeNull");
        }

        var response = await serviceMethod();
        if (response is null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "ResponseWasNull");
        }

        return StatusCodeFromResponse(response.StatusCode, response);
    }
    protected IActionResult HandleResponseStatusCode<T>(BaseControllerResponse<T> response)
    {
        if (response is null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "ResponseWasNull");
        }

        return StatusCodeFromResponse(response.StatusCode, response);
    }

    private IActionResult StatusCodeFromResponse<T>(HttpStatusCode statusCode, BaseControllerResponse<T> response)
    {
        return statusCode switch
        {
            HttpStatusCode.OK => Ok(response),
            HttpStatusCode.Created => Created(string.Empty, response),
            HttpStatusCode.NoContent => NoContent(),
            HttpStatusCode.BadRequest => BadRequest(response),
            HttpStatusCode.Unauthorized => Unauthorized(response),
            HttpStatusCode.Forbidden => Forbid(),
            HttpStatusCode.NotFound => NotFound(response),
            HttpStatusCode.Conflict => Conflict(response),
            _ => StatusCode((int)statusCode, response)
        };
    }

    private IActionResult StatusCodeFromResponse(HttpStatusCode statusCode, BaseControllerResponse response)
    {
        return statusCode switch
        {
            HttpStatusCode.OK => Ok(response),
            HttpStatusCode.Created => Created(string.Empty, response),
            HttpStatusCode.NoContent => NoContent(),
            HttpStatusCode.BadRequest => BadRequest(response),
            HttpStatusCode.Unauthorized => Unauthorized(response),
            HttpStatusCode.Forbidden => Forbid(),
            HttpStatusCode.NotFound => NotFound(response),
            HttpStatusCode.Conflict => Conflict(response),
            _ => StatusCode((int)statusCode, response)
        };
    }
}
