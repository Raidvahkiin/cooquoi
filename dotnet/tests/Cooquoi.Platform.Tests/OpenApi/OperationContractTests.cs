using System.Text.Json;
using Cooquoi.Platform.Tests.Support;

namespace Cooquoi.Platform.Tests.OpenApi;

/// <summary>The OpenAPI document served in development: the same one the build writes to <c>dotnet/generated/</c>.</summary>
public sealed class OperationContractTests(ApiFixture api)
{
    private async Task<JsonElement> DocumentAsync()
    {
        var result = await api.Host.Scenario(s => s.Get.Url("/openapi/v1.json"));
        return JsonDocument.Parse(await result.ReadAsTextAsync()).RootElement;
    }

    private static JsonElement Operation(JsonElement document, string path, string method) =>
        document.GetProperty("paths").GetProperty(path).GetProperty(method);

    [Fact]
    public async Task The_document_lists_every_operation_by_its_contract_id()
    {
        var document = await DocumentAsync();

        var operationIds = document.GetProperty("paths").EnumerateObject()
            .SelectMany(path => path.Value.EnumerateObject())
            .Select(operation => operation.Value.GetProperty("operationId").GetString());

        operationIds.ShouldBe(
            ["getMe", "signIn", "searchIngredients", "createIngredient", "getIngredient", "defineConversion", "setExpiryKind", "createPantry", "renamePantry", "setPantryStorage", "deletePantry", "listPantries", "restock"],
            ignoreOrder: true);
    }

    [Fact]
    public async Task A_replace_action_answers_204_with_ETag_and_its_template_problems()
    {
        var responses = Operation(await DocumentAsync(), "/v1/ingredients/{ingredientId}:define-conversion", "post").GetProperty("responses");

        responses.EnumerateObject().Select(r => r.Name)
            .ShouldBe(["204", "400", "401", "403", "404", "409", "412", "422", "428"], ignoreOrder: true);
        responses.GetProperty("204").GetProperty("headers").TryGetProperty("ETag", out _).ShouldBeTrue();
        responses.GetProperty("412").GetProperty("content").GetProperty("application/problem+json")
            .GetProperty("schema").GetProperty("$ref").GetString().ShouldBe("#/components/schemas/ProblemDetails");
    }
}
