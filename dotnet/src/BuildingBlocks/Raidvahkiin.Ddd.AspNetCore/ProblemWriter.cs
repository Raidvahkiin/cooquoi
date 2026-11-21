using System.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace Raidvahkiin.Ddd.AspNetCore;

/// <summary>The single shape of every error response: RFC 9457 with <c>code</c>, <c>params</c> and <c>traceId</c>.</summary>
public static class ProblemWriter
{
    public static ValueTask<bool> TryWriteAsync(this IProblemDetailsService problemDetails, HttpContext httpContext, Problem problem)
    {
        httpContext.Response.StatusCode = problem.Status;
        return problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails =
            {
                Status = problem.Status,
                Type = $"https://cooquoi.com/errors/{problem.Code}",
                Title = problem.Code,
                Detail = problem.Detail,
                Extensions =
                {
                    ["code"] = problem.Code,
                    ["params"] = problem.Parameters,
                    ["traceId"] = Activity.Current?.Id ?? httpContext.TraceIdentifier,
                },
            },
        });
    }
}
