using System.Text;
using System.Text.Json;
using Cooquoi.Platform.Tests.Support;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Wolverine.Runtime;

namespace Cooquoi.Platform.Tests.Serialization;

/// <summary>
/// ADR 0022: the serializers the host uses write camelCase property names — HTTP bodies (Wolverine.HTTP reads
/// ASP.NET's <see cref="JsonOptions"/>) and Wolverine envelope bodies (outbox, inbox, dead letters).
/// </summary>
public sealed class PropertyNameTests(ApiFixture api)
{
    private sealed record Probe(string DisplayName, ProbeItem[] LineItems);

    private sealed record ProbeItem(int UnitAmount);

    private static readonly Probe Sample = new("x", [new ProbeItem(1)]);

    private const string CamelCaseJson = """{"displayName":"x","lineItems":[{"unitAmount":1}]}""";

    [Fact]
    public void Http_bodies_have_camel_case_properties()
    {
        var options = api.Host.Services.GetRequiredService<IOptions<JsonOptions>>().Value.SerializerOptions;

        JsonSerializer.Serialize(Sample, options).ShouldBe(CamelCaseJson);
    }

    [Fact]
    public void Wolverine_messages_have_camel_case_properties()
    {
        var serializer = api.Host.Services.GetRequiredService<IWolverineRuntime>().Options.DefaultSerializer;

        Encoding.UTF8.GetString(serializer.WriteMessage(Sample)).ShouldBe(CamelCaseJson);
    }
}
