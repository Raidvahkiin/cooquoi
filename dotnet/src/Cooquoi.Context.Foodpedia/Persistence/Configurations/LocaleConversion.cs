using Cooquoi.SharedKernel;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Cooquoi.Context.Foodpedia.Persistence.Configurations;

internal static class LocaleConversion
{
    public static readonly ValueConverter<Locale, string> Converter = new(locale => locale.Code, code => Locale.FromTagOrDefault(code));
}
