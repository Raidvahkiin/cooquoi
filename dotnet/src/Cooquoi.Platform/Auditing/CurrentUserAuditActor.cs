using Microsoft.AspNetCore.Http;
using Raidvahkiin.Ddd.EntityFrameworkCore.Interceptors;

namespace Cooquoi.Platform.Auditing;

/// <summary>
/// The system actor of the running handler or job first (a handler may run in the flow of the
/// request that published its event), else the token subject of the HTTP caller, else <c>system</c>.
/// Spike: the subject, not the Household <c>UserId</c>, which Platform cannot resolve.
/// </summary>
internal sealed class CurrentUserAuditActor(IHttpContextAccessor accessor) : IAuditActor
{
    public const string System = "system";

    public string Current =>
        SystemActor.Current
        ?? (accessor.HttpContext?.User.FindFirst("sub")?.Value is { } subject ? $"user:{subject}" : System);
}
