using System.Globalization;
using System.Text.Json.Nodes;
using Cooquoi.Platform.OpenApi;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

/// <summary>Schemas of <see cref="ContractOpenApi"/> written the way TypeSpec writes them.</summary>
internal static partial class ContractOpenApi
{
    private static readonly string NoMaximum = int.MaxValue.ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// Numbers as the API writes them (the web JSON defaults also read numbers from strings, which ASP.NET adds as
    /// <c>string</c> + <c>pattern</c>), <c>decimal</c> as TypeSpec's <c>format: decimal</c>, and the codes of
    /// <see cref="ContractValuesAttribute"/> as <c>enum</c>.
    /// </summary>
    private static Task DescribeSchemaAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        if (schema.Type is { } type && type.HasFlag(JsonSchemaType.String) && (type.HasFlag(JsonSchemaType.Integer) || type.HasFlag(JsonSchemaType.Number)))
        {
            schema.Type = type & ~JsonSchemaType.String;
            schema.Pattern = null;
        }

        if (schema.Maximum == NoMaximum)
        {
            // [Range(1, int.MaxValue)] is DataAnnotations' way of writing a minimum only.
            schema.Maximum = null;
        }

        if (context.JsonTypeInfo.Type == typeof(decimal) || context.JsonTypeInfo.Type == typeof(decimal?))
        {
            schema.Format = "decimal";
        }

        var values = ContractValues(context);
        if (values is not null)
        {
            schema.Enum = [.. values.Values.Select(value => (JsonNode)JsonValue.Create(value))];
        }

        return Task.CompletedTask;
    }

    private static ContractValuesAttribute? ContractValues(OpenApiSchemaTransformerContext context)
    {
        var attributes = context.JsonPropertyInfo?.AttributeProvider?.GetCustomAttributes(typeof(ContractValuesAttribute), inherit: true)
            ?? (context.ParameterDescription?.ParameterDescriptor as IParameterInfoParameterDescriptor)?.ParameterInfo.GetCustomAttributes(typeof(ContractValuesAttribute), inherit: true);
        return attributes?.OfType<ContractValuesAttribute>().FirstOrDefault();
    }

    /// <summary>
    /// C# nullability as the contract means it: an optional member (<c>T? X = null</c>) is just optional, a required
    /// nullable one is <c>anyOf: [T, null]</c> like TypeSpec's <c>x: T | null</c>.
    /// </summary>
    private static void DescribeNullability(OpenApiSchema schema)
    {
        foreach (var (name, property) in schema.Properties ?? new Dictionary<string, IOpenApiSchema>())
        {
            if (WithoutNull(property) is not { } value)
            {
                continue;
            }

            schema.Properties![name] = schema.Required?.Contains(name) == true
                ? new OpenApiSchema { AnyOf = [value, new OpenApiSchema { Type = JsonSchemaType.Null }] }
                : value;
        }
    }

    /// <summary>The non-null part of a nullable schema (<c>type: [null, T]</c> or <c>oneOf: [null, $ref]</c>), else null.</summary>
    private static IOpenApiSchema? WithoutNull(IOpenApiSchema schema)
    {
        if (schema is OpenApiSchema { Type: { } type } concrete && type.HasFlag(JsonSchemaType.Null))
        {
            concrete.Type = type & ~JsonSchemaType.Null;
            return concrete;
        }

        return schema.OneOf is [{ Type: JsonSchemaType.Null }, var value] ? (value as OpenApiSchemaReference)?.Target ?? value : null;
    }
}
