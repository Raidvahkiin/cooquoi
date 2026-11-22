using System.Collections;
using System.Reflection;

namespace Cooquoi.Platform.OpenApi;

/// <summary>
/// The closed set of codes a <c>string</c> member carries; the OpenAPI document lists them as its <c>enum</c>,
/// like the contract's TypeSpec enums. Documentation only: validators still enforce the values.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public abstract class ContractValuesAttribute : Attribute
{
    public abstract IEnumerable<string> Values { get; }
}

/// <summary>Codes read from a public static field of <paramref name="source"/> (each item's <c>ToString()</c>, e.g. <c>Unit.All</c>).</summary>
public sealed class ContractCodesAttribute(Type source, string member) : ContractValuesAttribute
{
    private const BindingFlags StaticMembers = BindingFlags.Public | BindingFlags.Static;

    public override IEnumerable<string> Values
    {
        get
        {
            var items = source.GetField(member, StaticMembers)?.GetValue(null) as IEnumerable
                ?? throw new InvalidOperationException($"{source.Name}.{member} is not a public static collection field.");
            return items.Cast<object>().Select(item => item.ToString()!);
        }
    }
}

/// <summary>The lower-cased names of <typeparamref name="TEnum"/>, as endpoints write them.</summary>
public sealed class ContractEnumAttribute<TEnum> : ContractValuesAttribute
    where TEnum : struct, Enum
{
    public override IEnumerable<string> Values => Enum.GetNames<TEnum>().Select(name => name.ToLowerInvariant());
}
