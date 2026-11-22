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

/// <summary>The camelCase codes of <typeparamref name="TEnum"/> (<c>LongLife</c> is <c>longLife</c>), as the contract and the database write them (ADR 0021).</summary>
public sealed class ContractEnumAttribute<TEnum> : ContractValuesAttribute
    where TEnum : struct, Enum
{
    public override IEnumerable<string> Values => Enum.GetNames<TEnum>().Select(ContractEnumNames.ToCode);
}

/// <summary>Converts enum members to and from the camelCase codes of the contract (<see href="../../../docs/architecture/adr/0021-enum-values-camel-case.md">ADR 0021</see>).</summary>
public static class ContractEnumNames
{
    public static string ToCode<TEnum>(TEnum value) where TEnum : struct, Enum => ToCode(value.ToString());

    public static string ToCode(string name) => char.ToLowerInvariant(name[0]) + name[1..];

    /// <summary>Exact and case-sensitive: the code is one of <see cref="ContractEnumAttribute{TEnum}"/>'s values.</summary>
    public static TEnum Parse<TEnum>(string code) where TEnum : struct, Enum =>
        Enum.GetValues<TEnum>().Cast<TEnum?>().FirstOrDefault(value => ToCode(value!.Value) == code)
        ?? throw new ArgumentException($"'{code}' is not a {typeof(TEnum).Name} code.", nameof(code));
}
