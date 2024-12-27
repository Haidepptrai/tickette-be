using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Tickette.API.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class ErrorsController : ControllerBase
{
    private readonly ILogger<ErrorsController> _logger;

    public ErrorsController(ILogger<ErrorsController> logger)
    {
        _logger = logger;
    }

    [Route("/errors")]
    public IActionResult Error()
    {
        var exceptionHandler = HttpContext.Features.Get<IExceptionHandlerFeature>();
        var exception = exceptionHandler?.Error;

        if (exception == null)
        {
            return ProblemResponse(
                type: "https://tickette.example.com/problems/unknown-error",
                title: "An unknown error occurred",
                detail: "No exception information was provided.",
                statusCode: HttpStatusCode.InternalServerError
            );
        }

        // Default response values
        string type = "https://tickette.example.com/problems/server-error";
        string title = "An error occurred";
        string detail = exception.Message;
        HttpStatusCode statusCode = HttpStatusCode.InternalServerError;

        // Handle specific exceptions
        switch (exception)
        {
            case BadHttpRequestException badHttpRequestException:
                type = "https://tickette.example.com/problems/bad-request";
                title = "Bad Request";
                detail = badHttpRequestException.Message;
                statusCode = HttpStatusCode.BadRequest;
                break;

            case UnauthorizedAccessException:
                type = "https://tickette.example.com/problems/unauthorized";
                title = "Unauthorized";
                detail = "You do not have permission to access this resource.";
                statusCode = HttpStatusCode.Unauthorized;
                break;

            case NotImplementedException:
                type = "https://tickette.example.com/problems/not-implemented";
                title = "Not Implemented";
                detail = "This functionality is not implemented yet.";
                statusCode = HttpStatusCode.NotImplemented;
                break;

            default:
                // Log unhandled exceptions
                _logger.LogError(exception, "An unhandled exception occurred.");
                break;
        }

        return ProblemResponse(type, title, detail, statusCode);
    }

    private ObjectResult ProblemResponse(string type, string title, string detail, HttpStatusCode statusCode)
    {
        var problemDetails = new ProblemDetails
        {
            Type = type,
            Title = title,
            Detail = detail,
            Status = (int)statusCode,
            Instance = HttpContext.Request.Path,
        };

        return new ObjectResult(problemDetails)
        {
            StatusCode = (int)statusCode
        };
    }
}
