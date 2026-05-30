using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LineItem.Api.Controllers;

[ApiController]
public class ErrorController : ControllerBase
{
    private readonly ILogger<ErrorController> _logger;

    public ErrorController(ILogger<ErrorController> logger)
    {
        _logger = logger;
    }

    [Route("error")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult HandleError()
    {
        var handler = HttpContext.Features.Get<IExceptionHandlerFeature>()!;

        _logger.LogError(handler.Error, "Encountered an unhandled exception!");

        return Problem("Encountered an unhandled exception!");
    }
}
