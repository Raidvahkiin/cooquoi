using Microsoft.AspNetCore.Http;

namespace Raidvahkiin.Ddd.AspNetCore;

/// <summary>
/// The standard category of a problem, one per HTTP status the API answers: <c>kind</c> (<c>ERR_*</c>, easy to spot
/// in logs, ADR 0021) and the RFC 9457 <c>title</c>, a short English summary. The precise error stays <c>code</c>.
/// </summary>
public static class ProblemKind
{
    public const string Unexpected = "ERR_UNEXPECTED";

    private static readonly Dictionary<int, (string Kind, string Title)> ByStatus = new()
    {
        [StatusCodes.Status400BadRequest] = ("ERR_VALIDATION", "Validation failed"),
        [StatusCodes.Status401Unauthorized] = ("ERR_UNAUTHORIZED", "Unauthorized"),
        [StatusCodes.Status403Forbidden] = ("ERR_FORBIDDEN", "Forbidden"),
        [StatusCodes.Status404NotFound] = ("ERR_NOT_FOUND", "Not found"),
        [StatusCodes.Status409Conflict] = ("ERR_CONFLICT", "Conflict"),
        [StatusCodes.Status412PreconditionFailed] = ("ERR_PRECONDITION_FAILED", "Precondition failed"),
        [StatusCodes.Status422UnprocessableEntity] = ("ERR_BUSINESS_RULE", "Business rule violated"),
        [StatusCodes.Status428PreconditionRequired] = ("ERR_PRECONDITION_REQUIRED", "Precondition required"),
    };

    /// <summary>Every kind, for the OpenAPI document and its drift check.</summary>
    public static IReadOnlyList<string> All { get; } = [.. ByStatus.Values.Select(entry => entry.Kind), Unexpected];

    public static string Of(int status) => ByStatus.TryGetValue(status, out var entry) ? entry.Kind : Unexpected;

    public static string TitleOf(int status) => ByStatus.TryGetValue(status, out var entry) ? entry.Title : "Unexpected error";
}
