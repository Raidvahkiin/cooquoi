using Cooquoi.Context.Foodpedia.Domain.Ingredients.Events;
using Alba;
using Cooquoi.Context.Foodpedia.Domain.Ingredients;
using Cooquoi.Context.Foodpedia.Features.CreateIngredient;
using Cooquoi.Context.Foodpedia.Features.SearchIngredients;
using Cooquoi.Context.Foodpedia.Tests.Support;
using Cooquoi.Testing;
using Raidvahkiin.Ddd.EntityFrameworkCore.Paging;
using CreateIngredientRequest = Cooquoi.Context.Foodpedia.Features.CreateIngredient.CreateIngredient;

namespace Cooquoi.Context.Foodpedia.Tests.Features.CreateIngredient;

public sealed class CreateIngredientEndpointTests(ApiFixture api)
{
    private Task<IScenarioResult> Create(CreateIngredientRequest request, int status, Guid? key = null) =>
        api.Scenario(s =>
        {
            s.AsCurator().WithIdempotencyKey(key);
            s.Post.Json(request).ToUrl("/v1/ingredients");
            s.StatusCodeShouldBe(status);
        });

    [Fact]
    public async Task A_curator_creates_an_ingredient_that_search_then_finds_through_the_outbox()
    {
        var request = Requests.NewIngredient();

        var created = await api.TrackedScenario(s =>
        {
            s.AsCurator().WithIdempotencyKey();
            s.Post.Json(request).ToUrl("/v1/ingredients");
            s.StatusCodeShouldBe(201);
        });

        var id = (await created.Result.ReadAsJsonAsync<CreatedIngredient>())!.Id;
        created.Result.Context.Response.Headers.Location.ToString().ShouldBe($"/v1/ingredients/{id}");
        created.Messages.Executed.MessagesOf<IngredientCreatedDomainEvent>().ShouldContain(e => e.IngredientId.Value == id);

        var search = await api.Scenario(s =>
        {
            s.AsUser();
            s.Get.Url($"/v1/ingredients?q={Uri.EscapeDataString("ca vên " + request.Code[5..])}");
            s.WithRequestHeader("Accept-Language", "vi");
        });
        var found = (await search.ReadAsJsonAsync<Paged<IngredientSummary>>())!.Items.ShouldHaveSingleItem();
        found.Id.ShouldBe(id);
        found.Name.ShouldBe(request.Names[1].Primary);
    }

    [Fact]
    public async Task The_creation_is_audited_with_the_curator()
    {
        var request = Requests.NewIngredient();
        await Create(request, 201);

        await using var db = api.CreateDbContext();
        var row = db.Ingredients.Single(i => i.Code == IngredientCode.Create(request.Code));
        var entry = db.Entry(row);
        entry.Property("CreatedBy").CurrentValue.ShouldBe("user:curator-1");
        entry.Property("Version").CurrentValue.ShouldBe(0L);
        entry.Property("UpdatedBy").CurrentValue.ShouldBe("user:curator-1");
        ((DateTimeOffset)entry.Property("CreatedAt").CurrentValue!).ShouldBeGreaterThan(DateTimeOffset.UtcNow.AddMinutes(-5));
    }

    [Fact]
    public void Seeded_ingredients_are_audited_as_the_seed_job()
    {
        using var db = api.CreateDbContext();
        var salmon = db.Ingredients.Single(i => i.Code == IngredientCode.Create("salmon"));

        db.Entry(salmon).Property("CreatedBy").CurrentValue.ShouldBe("system:foodpedia.seed");
    }

    [Fact]
    public async Task A_user_without_the_curator_role_is_forbidden()
    {
        var result = await api.Scenario(s =>
        {
            s.AsUser().WithIdempotencyKey();
            s.Post.Json(Requests.NewIngredient()).ToUrl("/v1/ingredients");
            s.StatusCodeShouldBe(403);
        });

        await result.ShouldBeProblemAsync(403, "common.forbidden");
    }

    [Fact]
    public async Task F1_an_existing_code_is_a_conflict()
    {
        var result = await Create(Requests.NewIngredient() with { Code = "salmon" }, 409);

        var problem = await result.ShouldBeProblemAsync(409, "foodpedia.codeTaken");
        problem.GetProperty("params").GetProperty("code").GetString().ShouldBe("salmon");
    }

    [Fact]
    public async Task F4_a_primary_name_taken_in_the_same_locale_is_a_conflict_ignoring_case()
    {
        var request = Requests.NewIngredient() with { Names = [new("en", "Tilapia"), new("fr", "saumon")] };

        var result = await Create(request, 409);

        var problem = await result.ShouldBeProblemAsync(409, "foodpedia.nameTaken");
        problem.GetProperty("params").GetProperty("locale").GetString().ShouldBe("fr");
    }

    [Fact]
    public async Task F9_an_unknown_tag_breaks_a_business_rule()
    {
        var result = await Create(Requests.NewIngredient() with { Tags = ["category:fish-seafood", "season:monsoon"] }, 422);

        await result.ShouldBeProblemAsync(422, "foodpedia.unknownTags");
    }

    [Fact]
    public async Task F7_a_rule_broken_in_the_domain_stores_nothing()
    {
        var request = Requests.NewIngredient() with { Conversions = [new("kg", 1, 1000)] };

        var result = await Create(request, 422);

        await result.ShouldBeProblemAsync(422, "foodpedia.conversionNotNeeded");
        await using var db = api.CreateDbContext();
        db.Ingredients.Any(i => i.Code == IngredientCode.Create(request.Code)).ShouldBeFalse();
    }

    [Fact]
    public async Task A_malformed_request_lists_every_field_error_as_a_JSON_path()
    {
        var request = Requests.NewIngredient() with { Code = "", Names = [new("de", "Lachs")], BaseUnit = "stone", ExpiryKind = "" };

        var result = await Create(request, 400);

        var problem = await result.ShouldBeProblemAsync(400, "common.validation");
        var errors = problem.GetProperty("errors").EnumerateArray()
            .Select(error => (error.GetProperty("field").GetString(), error.GetProperty("code").GetString()))
            .ToList();
        errors.ShouldBe(
            [("code", "validation.required"), ("names[0].locale", "validation.oneOf"), ("baseUnit", "validation.oneOf"), ("expiryKind", "validation.oneOf")],
            ignoreOrder: true);
    }

    [Fact]
    public async Task The_expiry_kind_is_stored_and_a_kebab_case_kind_is_accepted()
    {
        var request = Requests.NewIngredient() with { ExpiryKind = "longLife" };
        await Create(request, 201);

        await using var db = api.CreateDbContext();
        db.Ingredients.Single(i => i.Code == IngredientCode.Create(request.Code)).ExpiryKind.ShouldBe(ExpiryKind.LongLife);
    }
}
