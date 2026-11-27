using Cooquoi.Platform.OpenApi;
using FluentValidation;

namespace Cooquoi.Platform.Validation;

/// <summary>The one rule for a <c>string</c> field carrying an enum's contract code (ADR 0021).</summary>
public static class ContractEnumRules
{
    /// <summary>
    /// The value is one of <typeparamref name="TEnum"/>'s camelCase codes, else <c>validation.oneOf</c>.
    /// Null fails too: guard an optional field with <c>When(… is not null)</c>.
    /// </summary>
    public static IRuleBuilderOptions<T, string?> IsContractEnum<T, TEnum>(this IRuleBuilder<T, string?> rule)
        where TEnum : struct, Enum =>
        rule.Must(code => code is not null && ContractEnumNames.IsCode<TEnum>(code)).WithErrorCode(ValidationCodes.OneOf);
}
