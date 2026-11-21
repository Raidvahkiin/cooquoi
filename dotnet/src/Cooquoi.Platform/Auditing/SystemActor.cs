using System.Text.RegularExpressions;

namespace Cooquoi.Platform.Auditing;

/// <summary>
/// Who writes when no user does (ADR 0014 §6e): <c>system:&lt;context&gt;.&lt;handler&gt;</c>, set for the
/// async flow of one message handler (<see cref="SystemActorPolicy"/>) or one background job.
/// </summary>
public static partial class SystemActor
{
    private static readonly AsyncLocal<string?> Actor = new();

    public static string? Current => Actor.Value;

    /// <summary>Applies to the calling async flow only; it never leaks back to the caller.</summary>
    public static void Enter(string actor) => Actor.Value = actor;

    /// <summary><c>Cooquoi.Context.Foodpedia….IngredientSearchTermsHandler</c> → <c>system:foodpedia.ingredient-search-terms</c>.</summary>
    public static string For(Type handlerType)
    {
        var context = handlerType.Namespace?.Split('.') is ["Cooquoi", "Context", var name, ..] ? name.ToLowerInvariant() : "platform";
        var handler = handlerType.Name.EndsWith("Handler", StringComparison.Ordinal) ? handlerType.Name[..^"Handler".Length] : handlerType.Name;
        return $"system:{context}.{WordStart().Replace(handler, "-$1").ToLowerInvariant()}";
    }

    [GeneratedRegex("(?<!^)([A-Z])")]
    private static partial Regex WordStart();
}
