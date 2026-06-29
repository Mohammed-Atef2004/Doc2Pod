using Domain.SharedKernel;
using MediatR;
using Microsoft.AspNetCore.Mvc;
namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class ApiController : ControllerBase
{
    private ISender? _sender;
    protected ISender Sender => _sender ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    protected IActionResult HandleFailure(Result result)
    {
        if (result.IsSuccess)
        {
            throw new InvalidOperationException("Success results should not call HandleFailure.");
        }

        int statusCode = result.Error.Code switch
        {
            var code when code.Contains("Page")
               => StatusCodes.Status400BadRequest,
            var code when code.Contains("Unauthorized") || code.Contains("Invalid")
                => StatusCodes.Status401Unauthorized,
            var code when code.EndsWith(".NotFound")
                => StatusCodes.Status404NotFound,
            var code when code.EndsWith(".AlreadyExists") || code.EndsWith("AlreadyExists") || code.EndsWith(".AlreadyRunning")
                => StatusCodes.Status409Conflict,
            "Podcast.NotReady" => StatusCodes.Status400BadRequest,

            _ => StatusCodes.Status400BadRequest
        };

        return result switch
        {
            IValidationResult validationResult =>
                BadRequest(
                    CreateProblemDetails(
                        "Validation Error",
                        StatusCodes.Status400BadRequest,
                        result.Error,
                        validationResult.Errors)),


            _ => StatusCode(statusCode,
                    CreateProblemDetails(
                        GetTitleForStatus(statusCode),
                        statusCode,
                        result.Error))
        };
    }

    private static string GetTitleForStatus(int statusCode) =>
        statusCode switch
        {
            StatusCodes.Status401Unauthorized => "Unauthorized Access",
            StatusCodes.Status404NotFound => "Resource Not Found",
            StatusCodes.Status409Conflict => "Conflict / Duplicate",
            _ => "Bad Request"
        };

    private static ProblemDetails CreateProblemDetails(
        string title,
        int status,
        Error error,
        Error[]? errors = null) =>
        new()
        {
            Title = title,
            Type = error.Code,
            Detail = error.Message,
            Status = status,
            Extensions = { { "errors", errors } }
        };

}