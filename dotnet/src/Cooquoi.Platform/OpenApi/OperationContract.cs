using Cooquoi.Platform.Idempotency;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Wolverine.Http;

namespace Cooquoi.Platform.OpenApi;

/// <summary>A header the endpoint reads; the OpenAPI document lists it as a parameter.</summary>
public sealed record RequestHeaderMetadata(string Name, bool Required);

/// <summary>A header the endpoint answers with this status; the OpenAPI document lists it on that response.</summary>
public sealed record ResponseHeaderMetadata(int StatusCode, string Name);

/// <summary>
/// Response metadata of the contract's operation templates (<c>contracts/common/operations.tsp</c>), so the
/// emitted OpenAPI document matches TypeSpec. An endpoint class picks its template from Wolverine's
/// <c>public static void Configure(HttpChain chain)</c> hook.
/// </summary>
public static class OperationContract
{
    /// <summary><c>ListOperation</c>: 200 page, 400, 401.</summary>
    public static void List(HttpChain chain)
    {
        NeverNotFound(chain);
        Problems(chain, 400, 401);
    }

    /// <summary>A resource of the signed-in caller (<c>GET /v1/me</c>): 200, 401; never missing.</summary>
    public static void OwnResource(HttpChain chain)
    {
        NeverNotFound(chain);
        Problems(chain, 401);
    }

    /// <summary><c>ReadOperation</c>: 200 + <c>ETag</c>, 401, 404.</summary>
    public static void Read(HttpChain chain)
    {
        chain.Metadata.WithMetadata(new ResponseHeaderMetadata(StatusCodes.Status200OK, HeaderNames.ETag));
        Problems(chain, 401, 404);
    }

    /// <summary><c>CreateOperation</c>: 201 + <c>Location</c> (body from the <c>Created&lt;T&gt;</c> return type).</summary>
    public static void Create(HttpChain chain)
    {
        Idempotent(chain);
        chain.Metadata.WithMetadata(new ResponseHeaderMetadata(StatusCodes.Status201Created, HeaderNames.Location));
        Problems(chain, 400, 401, 403, 409, 422);
    }

    /// <summary><c>ActionOperation</c>: 204 + <c>ETag</c>.</summary>
    public static void Action(HttpChain chain)
    {
        NoContentWithETag(chain);
        Problems(chain, 400, 401, 403, 404, 409, 422);
    }

    /// <summary><c>ReplaceActionOperation</c>: <see cref="Action"/> that requires <c>If-Match</c> (412, 428).</summary>
    public static void ReplaceAction(HttpChain chain)
    {
        NoContentWithETag(chain);
        chain.Metadata.WithMetadata(new RequestHeaderMetadata(HeaderNames.IfMatch, Required: true));
        Problems(chain, 400, 401, 403, 404, 409, 412, 422, 428);
    }

    /// <summary>Problem responses (<c>application/problem+json</c>).</summary>
    private static void Problems(HttpChain chain, params int[] statusCodes)
    {
        foreach (var statusCode in statusCodes)
        {
            chain.Metadata.ProducesProblem(statusCode);
        }
    }

    private static void NoContentWithETag(HttpChain chain)
    {
        Idempotent(chain);
        chain.Metadata.Produces(StatusCodes.Status204NoContent);
        chain.Metadata.Finally(endpoint => endpoint.RemoveStatusCodeResponse(StatusCodes.Status200OK));
        chain.Metadata.WithMetadata(new ResponseHeaderMetadata(StatusCodes.Status204NoContent, HeaderNames.ETag));
    }

    /// <summary>Wolverine assumes a GET may answer 404 (null resource); these never do.</summary>
    private static void NeverNotFound(HttpChain chain) =>
        chain.Metadata.Finally(endpoint => endpoint.RemoveStatusCodeResponse(StatusCodes.Status404NotFound));

    private static void Idempotent(HttpChain chain) =>
        chain.Metadata.WithMetadata(new RequestHeaderMetadata(IdempotencyMiddleware.Header, Required: true));
}
