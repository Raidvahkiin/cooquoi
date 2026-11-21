namespace Raidvahkiin.Ddd.EntityFrameworkCore.Interceptors;

/// <summary>Who is changing data right now: a user id, or a system actor for background work.</summary>
public interface IAuditActor
{
    string Current { get; }
}
