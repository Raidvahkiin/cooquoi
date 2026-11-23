using Cooquoi.Context.Foodpedia.Features.SearchIngredients;
using Cooquoi.Context.Foodpedia.Tests.Support;
using Cooquoi.Testing;
using Raidvahkiin.Ddd.EntityFrameworkCore.Paging;

namespace Cooquoi.Context.Foodpedia.Tests.Features.SearchIngredients;

/// <summary>Against the seed: salmon, smoked salmon, tofu (Đậu phụ), zucchini (courgette)…</summary>
public sealed class SearchIngredientsEndpointTests(ApiFixture api)
{
    private async Task<Paged<IngredientSummary>> Search(string query, string locale = "en")
    {
        var result = await api.Scenario(s =>
        {
            s.AsUser();
            s.Get.Url($"/v1/ingredients?{query}");
            s.WithRequestHeader("Accept-Language", locale);
        });
        return (await result.ReadAsJsonAsync<Paged<IngredientSummary>>())!;
    }

    [Fact]
    public async Task Vietnamese_without_accents_finds_the_accented_name()
    {
        var page = await Search("q=ca%20hoi", "vi");

        page.Items.Select(item => item.Code).ShouldBe(["salmon", "smoked-salmon"]);
        page.Items[0].Name.ShouldBe("Cá hồi");
    }

    [Fact]
    public async Task The_Vietnamese_letter_d_with_stroke_matches_a_plain_d()
    {
        var page = await Search("q=dau%20hu");

        page.Items.ShouldHaveSingleItem().Code.ShouldBe("tofu");
    }

    [Fact]
    public async Task A_name_from_another_locale_finds_the_ingredient_named_in_the_requested_locale()
    {
        var page = await Search("q=saumon", "en");

        page.Items.Select(item => item.Name).ShouldBe(["Salmon", "Smoked salmon"]);
    }

    [Fact]
    public async Task Synonyms_are_searched()
    {
        var page = await Search("q=courgette", "vi");

        page.Items.ShouldHaveSingleItem().Name.ShouldBe("Bí ngòi");
    }

    [Fact]
    public async Task A_typo_still_finds_the_ingredient()
    {
        var page = await Search("q=corriander");

        page.Items.ShouldContain(item => item.Code == "coriander");
    }

    [Fact]
    public async Task Every_tag_filter_must_match()
    {
        var page = await Search("tags=family:rice&tags=diet:vegan&pageSize=100");

        page.Items.Select(item => item.Code).ShouldBe(["brown-rice", "white-rice"]);
        page.Items[0].CategoryTags.ShouldBe(["category:grains"]);
    }

    [Fact]
    public async Task Pages_report_the_total_count_and_follow_the_requested_sort()
    {
        var page = await Search("tags=family:rice&sort=-code&page=2&pageSize=1");

        page.TotalCount.ShouldBe(2);
        page.Items.ShouldHaveSingleItem().Code.ShouldBe("brown-rice");
    }

    [Fact]
    public async Task A_page_far_past_the_end_is_empty_not_an_error()
    {
        var page = await Search("page=200000000");

        page.Items.ShouldBeEmpty();
        page.Page.ShouldBe(200000000);
    }

    [Fact]
    public async Task A_sort_outside_the_whitelist_is_a_validation_problem()
    {
        var result = await api.Scenario(s =>
        {
            s.AsUser();
            s.Get.Url("/v1/ingredients?sort=createdBy");
            s.StatusCodeShouldBe(400);
        });

        var problem = await result.ShouldBeProblemAsync(400, "common.invalid-sort");
        problem.GetProperty("params").GetProperty("sort").GetString().ShouldBe("createdBy");
    }

    [Fact]
    public async Task A_page_size_over_the_maximum_is_a_field_error()
    {
        var result = await api.Scenario(s =>
        {
            s.AsUser();
            s.Get.Url("/v1/ingredients?pageSize=500");
            s.StatusCodeShouldBe(400);
        });

        var problem = await result.ShouldBeProblemAsync(400, "common.validation");
        var error = problem.GetProperty("errors").EnumerateArray().ShouldHaveSingleItem();
        error.GetProperty("field").GetString().ShouldBe("pageSize");
        error.GetProperty("code").GetString().ShouldBe("validation.range");
    }
}
