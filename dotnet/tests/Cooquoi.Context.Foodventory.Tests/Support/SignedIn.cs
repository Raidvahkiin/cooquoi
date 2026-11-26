using Cooquoi.Testing;

namespace Cooquoi.Context.Foodventory.Tests.Support;

public static class SignedIn
{
    /// <summary>A caller whose household exists (first sign-in creates it; tracked, so its default pantries exist when this returns).</summary>
    public static async Task<string> NewUserAsync(this ApiFixture api)
    {
        var subject = $"subject-{Guid.NewGuid()}";
        await api.Tracked(() => api.Host.Scenario(s =>
        {
            s.AsUser(subject).WithIdempotencyKey();
            s.Post.Url("/v1/me:sign-in");
            s.StatusCodeShouldBe(204);
        }));
        return subject;
    }
}
