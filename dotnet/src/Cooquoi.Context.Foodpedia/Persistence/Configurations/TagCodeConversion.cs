using Cooquoi.Context.Foodpedia.Domain.Tags;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Cooquoi.Context.Foodpedia.Persistence.Configurations;

internal static class TagCodeConversion
{
    public static readonly ValueConverter<TagCode, string> Converter = new(code => code.Value, value => TagCode.Parse(value));
}
