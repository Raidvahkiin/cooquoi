using System.Text.Json;
using Cooquoi.Context.Foodpedia.Domain.Ingredients;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Cooquoi.Context.Foodpedia.Persistence.Configurations;

/// <summary>Stores a <see cref="ConversionTable"/> as <c>[{"unit":"fillet","unitAmount":1,"baseAmount":200}]</c>.</summary>
internal static class ConversionTableJson
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    public static readonly ValueConverter<ConversionTable, string> Converter = new(
        table => Serialize(table),
        json => Deserialize(json));

    private static string Serialize(ConversionTable table) =>
        JsonSerializer.Serialize(table.Items.Select(item => new Row(item.Unit.Code, item.UnitAmount, item.BaseAmount)), Options);

    private static ConversionTable Deserialize(string json) =>
        ConversionTable.Restore((JsonSerializer.Deserialize<Row[]>(json, Options) ?? [])
            .Select(row => new Conversion(UnitConversion.FromCode(row.Unit), row.UnitAmount, row.BaseAmount)));

    private sealed record Row(string Unit, decimal UnitAmount, decimal BaseAmount);
}
