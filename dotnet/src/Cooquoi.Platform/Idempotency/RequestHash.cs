using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace Cooquoi.Platform.Idempotency;

internal static class RequestHash
{
    /// <summary>SHA-256 of method, path, query and body; the body stays readable for the endpoint.</summary>
    public static async Task<string> ComputeAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        request.EnableBuffering();
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        hash.AppendData(Encoding.UTF8.GetBytes($"{request.Method} {request.Path}{request.QueryString}\n"));
        await using (var body = new MemoryStream())
        {
            await request.Body.CopyToAsync(body, cancellationToken);
            hash.AppendData(body.ToArray());
        }

        request.Body.Position = 0;
        return Convert.ToHexString(hash.GetHashAndReset());
    }
}
