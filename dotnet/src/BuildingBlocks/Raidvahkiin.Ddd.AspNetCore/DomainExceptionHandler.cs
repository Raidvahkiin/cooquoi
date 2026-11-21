using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Raidvahkiin.Ddd.AspNetCore;

/// <summary>
/// The single place where errors become ProblemDetails (code, params, traceId).
/// Unknown exceptions return 500 without internal details.
/// </summary>
public sealed partial class DomainExceptionHandler(IProblemDetailsService problemDetails, ILogger<DomainExceptionHandler> logger)
    : IExceptionHandler
{
    public const string UnexpectedCode = "common.unexpected";

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, code, detail, parameters) = exception is DomainException domain
            ? (domain.Error.Kind.ToStatusCode(), domain.Error.Code, domain.Message, domain.Parameters)
            : (StatusCodes.Status500InternalServerError, UnexpectedCode, null, null);

        LogFailure(exception, code, status);
        httpContext.Response.StatusCode = status;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails =
            {
                Status = status,
                Type = $"https://cooquoi.com/errors/{code}",
                Title = code,
                Detail = detail,
                Extensions =
                {
                    ["code"] = code,
                    ["params"] = parameters,
                    ["traceId"] = Activity.Current?.Id ?? httpContext.TraceIdentifier,
                },
            },
        });
    }

    private void LogFailure(Exception exception, string code, int status)
    {
        if (exception is DomainException)
        {
            LogExpected(code, status);
        }
        else
        {
            LogUnexpected(exception);
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Request failed with {Code} ({Status})")]
    private partial void LogExpected(string code, int status);

    [LoggerMessage(Level = LogLevel.Error, Message = "Unexpected error")]
    private partial void LogUnexpected(Exception exception);
}
