using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Raidvahkiin.Ddd.AspNetCore;

/// <summary>The single shape of every error response: RFC 9457 with <c>kind</c>, <c>code</c>, <c>params</c>, <c>errors</c> and <c>traceId</c>.</summary>
public static class ProblemWriter
{
    public static ValueTask<bool> TryWriteAsync(this IProblemDetailsService problemDetails, HttpContext httpContext, Problem problem)
    {
        httpContext.Response.StatusCode = problem.Status;
        return problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = ToProblemDetails(problem, httpContext.TraceIdentifier),
        });
    }

    /// <summary>For frameworks that write the <see cref="ProblemDetails"/> themselves (Wolverine validation).</summary>
    public static ProblemDetails ToProblemDetails(Problem problem, string? fallbackTraceId = null)
    {
        var details = new ProblemDetails
        {
            Status = problem.Status,
            Type = $"https://cooquoi.com/errors/{problem.Code}",
            Title = ProblemKind.TitleOf(problem.Status),
            Detail = problem.Detail,
        };
        details.Extensions["kind"] = ProblemKind.Of(problem.Status);
        details.Extensions["code"] = problem.Code;
        details.Extensions["params"] = problem.Parameters;
        if (problem.Errors is { Count: > 0 })
        {
            details.Extensions["errors"] = problem.Errors;
        }

        details.Extensions["traceId"] = Activity.Current?.Id ?? fallbackTraceId;
        return details;
    }
}
