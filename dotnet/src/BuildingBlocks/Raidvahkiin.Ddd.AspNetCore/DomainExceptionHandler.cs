using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Raidvahkiin.Ddd.AspNetCore;

/// <summary>
/// Turns exceptions into problems: a <see cref="DomainException"/> keeps its code and kind;
/// anything else returns 500 without internal details.
/// </summary>
public sealed partial class DomainExceptionHandler(IProblemDetailsService problemDetails, ILogger<DomainExceptionHandler> logger)
    : IExceptionHandler
{
    public const string UnexpectedCode = "common.unexpected";

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var problem = exception is DomainException domain
            ? new Problem(domain.Error.Kind.ToStatusCode(), domain.Error.Code, domain.Message, domain.Parameters)
            : new Problem(StatusCodes.Status500InternalServerError, UnexpectedCode);

        LogFailure(exception, problem);
        return await problemDetails.TryWriteAsync(httpContext, problem);
    }

    private void LogFailure(Exception exception, Problem problem)
    {
        var kind = ProblemKind.Of(problem.Status);
        if (exception is DomainException)
        {
            LogExpected(kind, problem.Code, problem.Status);
        }
        else
        {
            LogUnexpected(exception, kind, problem.Code);
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Request failed: {Kind} {Code} ({Status})")]
    private partial void LogExpected(string kind, string code, int status);

    [LoggerMessage(Level = LogLevel.Error, Message = "Request failed: {Kind} {Code} (500)")]
    private partial void LogUnexpected(Exception exception, string kind, string code);
}
