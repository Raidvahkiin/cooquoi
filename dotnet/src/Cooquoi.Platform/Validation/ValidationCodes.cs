using FluentValidation.Results;

namespace Cooquoi.Platform.Validation;

/// <summary>The shared <c>validation.*</c> codes clients translate (ADR 0014 §10).</summary>
public static class ValidationCodes
{
    /// <summary>Set by use cases with <c>WithErrorCode</c> for a closed set of values.</summary>
    public const string OneOf = "validation.one-of";

    private static readonly Dictionary<string, string> ByValidator = new(StringComparer.Ordinal)
    {
        ["NotEmptyValidator"] = "validation.required",
        ["NotNullValidator"] = "validation.required",
        ["MaximumLengthValidator"] = "validation.max-length",
        ["MinimumLengthValidator"] = "validation.min-length",
        ["GreaterThanValidator"] = "validation.min",
        ["GreaterThanOrEqualValidator"] = "validation.min",
        ["LessThanOrEqualValidator"] = "validation.max",
        ["InclusiveBetweenValidator"] = "validation.range",
        ["RegularExpressionValidator"] = "validation.pattern",
        ["PredicateValidator"] = "validation.invalid",
    };

    /// <summary>Message parameters worth translating; FluentValidation's own (PropertyName…) are dropped.</summary>
    private static readonly string[] KeptParams = ["MaxLength", "MinLength", "ComparisonValue", "From", "To", "AllowedValues"];

    /// <summary>Every code a 400 response may carry, listed by the OpenAPI document.</summary>
    public static IEnumerable<string> All => ByValidator.Values.Append(OneOf).Distinct();

    internal static string Of(ValidationFailure failure) =>
        failure.ErrorCode.StartsWith("validation.", StringComparison.Ordinal)
            ? failure.ErrorCode
            : ByValidator.GetValueOrDefault(failure.ErrorCode, "validation.invalid");

    internal static IReadOnlyDictionary<string, object?>? ParamsOf(ValidationFailure failure)
    {
        var kept = (failure.FormattedMessagePlaceholderValues ?? [])
            .Where(placeholder => KeptParams.Contains(placeholder.Key))
            .ToDictionary(placeholder => char.ToLowerInvariant(placeholder.Key[0]) + placeholder.Key[1..], placeholder => (object?)placeholder.Value);
        return kept.Count == 0 ? null : kept;
    }
}
