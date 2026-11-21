using Cooquoi.SharedKernel;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Cooquoi.Context.Foodpedia.Persistence.Configurations;

internal static class UnitConversion
{
    public static readonly ValueConverter<Unit, string> Converter = new(unit => unit.Code, code => FromCode(code));

    public static Unit FromCode(string code) =>
        Unit.FromCode(code) ?? throw new InvalidOperationException($"Unknown unit '{code}' in the database");
}
