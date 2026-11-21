using Cooquoi.Context.Foodpedia.Features.GetIngredient;
using Cooquoi.Context.Foodpedia.Tests.Support;
using Cooquoi.Testing;
using Microsoft.EntityFrameworkCore;
using Cooquoi.Context.Foodpedia.Domain.Ingredients;

namespace Cooquoi.Context.Foodpedia.Tests.Features.GetIngredient;

public sealed class GetIngredientEndpointTests(ApiFixture api)
{
    private async Task<Guid> IdOf(string code)
    {
        await using var db = api.CreateDbContext();
        var ingredient = await db.Ingredients.AsNoTracking().SingleAsync(i => i.Code == IngredientCode.Create(code), TestContext.Current.CancellationToken);
        return ingredient.Id.Value;
    }

    [Fact]
    public async Task Returns_the_ingredient_in_the_requested_locale_with_its_version_as_ETag()
    {
        var id = await IdOf("tofu");

        var result = await api.Scenario(s =>
        {
            s.AsUser();
            s.Get.Url($"/v1/ingredients/{id}");
            s.WithRequestHeader("Accept-Language", "vi-VN");
        });

        var tofu = (await result.ReadAsJsonAsync<IngredientDetail>())!;
        tofu.Locale.ShouldBe("vi");
        tofu.Name.ShouldBe("Đậu phụ");
        tofu.Synonyms.ShouldBe(["Đậu hũ"]);
        tofu.BaseUnit.ShouldBe("g");
        tofu.Conversions.ShouldBe([new ConversionDetail("pack", 1, 300)]);
        tofu.ShelfLife.ShouldBe(new ShelfLifeDetail(5, "fridge"));
        tofu.Status.ShouldBe("active");
        result.Context.Response.Headers.ETag.ToString().ShouldBe($"\"{tofu.Version}\"");
    }

    [Fact]
    public async Task Falls_back_to_English_for_an_unsupported_locale()
    {
        var id = await IdOf("garlic");

        var result = await api.Scenario(s =>
        {
            s.AsUser();
            s.Get.Url($"/v1/ingredients/{id}");
            s.WithRequestHeader("Accept-Language", "de-DE");
        });

        var garlic = (await result.ReadAsJsonAsync<IngredientDetail>())!;
        garlic.Locale.ShouldBe("en");
        garlic.Name.ShouldBe("Garlic");
    }

    [Fact]
    public async Task An_unknown_id_is_a_not_found_problem()
    {
        var result = await api.Scenario(s =>
        {
            s.AsUser();
            s.Get.Url($"/v1/ingredients/{Guid.CreateVersion7()}");
            s.StatusCodeShouldBe(404);
        });

        await result.ShouldBeProblemAsync(404, "common.not-found");
    }
}
