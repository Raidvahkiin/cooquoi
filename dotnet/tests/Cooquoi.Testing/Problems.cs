using System.Text.Json;
using Alba;

namespace Cooquoi.Testing;

public static class Problems
{
    /// <summary>Checks the response is a contract problem (status + code) and returns it for more checks.</summary>
    public static async Task<JsonElement> ShouldBeProblemAsync(this IScenarioResult result, int status, string code)
    {
        result.Context.Response.StatusCode.ShouldBe(status);
        result.Context.Response.ContentType.ShouldNotBeNull().ShouldStartWith("application/problem+json");
        var problem = await result.ReadAsJsonAsync<JsonElement>();
        problem.GetProperty("code").GetString().ShouldBe(code);
        problem.GetProperty("status").GetInt32().ShouldBe(status);
        problem.TryGetProperty("traceId", out _).ShouldBeTrue();
        return problem;
    }
}
