using Microsoft.AspNetCore.Http;

namespace Cooquoi.Platform.Idempotency;

/// <summary>The key this request holds, shared from the middleware to the save interceptor.</summary>
internal sealed class IdempotencyClaim(string subject, Guid key)
{
    public string Subject { get; } = subject;
    public Guid Key { get; } = key;
    /// <summary>Set by the first save of the request; later saves (event handlers) leave the key alone.</summary>
    public bool Committed { get; set; }

    public static IdempotencyClaim? Of(HttpContext? context) =>
        context?.Items[typeof(IdempotencyClaim)] as IdempotencyClaim;

    public void AttachTo(HttpContext context) => context.Items[typeof(IdempotencyClaim)] = this;
}
