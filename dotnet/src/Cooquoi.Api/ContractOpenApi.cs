using System.Reflection;
using Cooquoi.Platform.OpenApi;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

/// <summary>
/// The OpenAPI 3.1 document of what the API really serves, shaped to compare cleanly with the TypeSpec
/// contract (ADR 0009): bearer security, server <c>/</c>, every problem as <c>application/problem+json</c>
/// with the contract's <c>ProblemDetails</c>, and the template headers of <see cref="OperationContract"/>.
/// Written to <c>dotnet/generated/openapi.json</c> at build time (GetDocument.Insider).
/// </summary>
internal static class ContractOpenApi
{
    public const string DocumentName = "v1";

    private const string BearerScheme = "BearerAuth";
    private const string ProblemSchema = "ProblemDetails";
    private const string FieldErrorSchema = "FieldError";
    private const string ProblemContentType = "application/problem+json";

    /// <summary>True when the build-time generator runs the host: there is no database or identity provider then.</summary>
    public static bool IsGeneratingDocument => Assembly.GetEntryAssembly()?.GetName().Name == "GetDocument.Insider";

    /// <summary>Placeholders for the settings Aspire gives at runtime; never used to connect.</summary>
    public static IEnumerable<KeyValuePair<string, string?>> GenerationSettings(string connectionName) =>
    [
        new($"ConnectionStrings:{connectionName}", "Host=openapi-generation;Database=none"),
        new("Authentication:Authority", "https://openapi-generation.invalid/realms/cooquoi"),
    ];

    public static IServiceCollection AddContractOpenApi(this IServiceCollection services) =>
        services.AddOpenApi(DocumentName, options =>
        {
            options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1;
            options.AddOperationTransformer(DescribeOperationAsync);
            options.AddDocumentTransformer(DescribeDocumentAsync);
        });

    private static Task DescribeOperationAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        var metadata = context.Description.ActionDescriptor.EndpointMetadata;
        AddRequestHeaders(operation, metadata.OfType<RequestHeaderMetadata>());
        foreach (var (status, response) in operation.Responses ?? [])
        {
            if (response is OpenApiResponse concrete)
            {
                DescribeResponse(concrete, status, metadata.OfType<ResponseHeaderMetadata>(), context.Document);
            }
        }

        return Task.CompletedTask;
    }

    private static void AddRequestHeaders(OpenApiOperation operation, IEnumerable<RequestHeaderMetadata> headers)
    {
        foreach (var header in headers)
        {
            operation.Parameters ??= [];
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = header.Name,
                In = ParameterLocation.Header,
                Required = header.Required,
                Schema = new OpenApiSchema { Type = JsonSchemaType.String },
            });
        }
    }

    private static void DescribeResponse(OpenApiResponse response, string status, IEnumerable<ResponseHeaderMetadata> headers, OpenApiDocument? document)
    {
        if (int.TryParse(status, out var code) && code >= StatusCodes.Status400BadRequest)
        {
            response.Content = new Dictionary<string, OpenApiMediaType>
            {
                [ProblemContentType] = new() { Schema = new OpenApiSchemaReference(ProblemSchema, document) },
            };
        }

        foreach (var header in headers.Where(h => h.StatusCode.ToString() == status))
        {
            response.Headers ??= new Dictionary<string, IOpenApiHeader>();
            response.Headers[header.Name] = new OpenApiHeader { Required = true, Schema = new OpenApiSchema { Type = JsonSchemaType.String } };
        }
    }

    private static Task DescribeDocumentAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Info.Title = "Cooquoi API";
        document.Servers = [new OpenApiServer { Url = "/" }];

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes[BearerScheme] = new OpenApiSecurityScheme { Type = SecuritySchemeType.Http, Scheme = "Bearer" };
        document.Security = [new OpenApiSecurityRequirement { [new OpenApiSecuritySchemeReference(BearerScheme, document)] = [] }];

        document.Components.Schemas ??= new Dictionary<string, IOpenApiSchema>();
        // ASP.NET's own problem types are replaced by the contract's shape (ADR 0014 §10).
        document.Components.Schemas.Remove("HttpValidationProblemDetails");
        document.Components.Schemas[FieldErrorSchema] = FieldError();
        document.Components.Schemas[ProblemSchema] = ProblemDetails(document);
        return Task.CompletedTask;
    }

    private static OpenApiSchema ProblemDetails(OpenApiDocument document) => new()
    {
        Type = JsonSchemaType.Object,
        Required = new HashSet<string> { "type", "title", "status", "code" },
        Properties = new Dictionary<string, IOpenApiSchema>
        {
            ["type"] = new OpenApiSchema { Type = JsonSchemaType.String, Format = "uri" },
            ["title"] = new OpenApiSchema { Type = JsonSchemaType.String },
            ["status"] = new OpenApiSchema { Type = JsonSchemaType.Integer, Format = "int32" },
            ["detail"] = new OpenApiSchema { Type = JsonSchemaType.String },
            ["instance"] = new OpenApiSchema { Type = JsonSchemaType.String },
            ["code"] = new OpenApiSchema { Type = JsonSchemaType.String },
            ["params"] = FreeObject(),
            ["errors"] = new OpenApiSchema { Type = JsonSchemaType.Array, Items = new OpenApiSchemaReference(FieldErrorSchema, document) },
            ["traceId"] = new OpenApiSchema { Type = JsonSchemaType.String },
        },
    };

    private static OpenApiSchema FieldError() => new()
    {
        Type = JsonSchemaType.Object,
        Required = new HashSet<string> { "field", "code" },
        Properties = new Dictionary<string, IOpenApiSchema>
        {
            ["field"] = new OpenApiSchema { Type = JsonSchemaType.String },
            ["code"] = new OpenApiSchema { Type = JsonSchemaType.String },
            ["params"] = FreeObject(),
        },
    };

    private static OpenApiSchema FreeObject() => new() { Type = JsonSchemaType.Object, AdditionalProperties = new OpenApiSchema() };
}
