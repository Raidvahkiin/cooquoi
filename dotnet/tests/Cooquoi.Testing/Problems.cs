using System.Text.Json;
using Alba;
using Raidvahkiin.Ddd.AspNetCore;

namespace Cooquoi.Testing;

public static class Problems
{
    /// <summary>Checks the response is a contract problem (status, kind, title + code) and returns it for more checks.</summary>
    public static async Task<JsonElement> ShouldBeProblemAsync(this IScenarioResult result, int status, string code)
    {
        result.Context.Response.StatusCode.ShouldBe(status);
        result.Context.Response.ContentType.ShouldNotBeNull().ShouldStartWith("application/problem+json");
        var problem = await result.ReadAsJsonAsync<JsonElement>();
        problem.GetProperty("code").GetString().ShouldBe(code);
        problem.GetProperty("status").GetInt32().ShouldBe(status);
        problem.GetProperty("kind").GetString().ShouldBe(ProblemKind.Of(status));
        problem.GetProperty("title").GetString().ShouldBe(ProblemKind.TitleOf(status));
        problem.TryGetProperty("traceId", out _).ShouldBeTrue();
        return problem;
    }
}
