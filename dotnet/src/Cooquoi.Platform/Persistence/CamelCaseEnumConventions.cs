using Cooquoi.Platform.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Cooquoi.Platform.Persistence;

/// <summary>Stores every enum property as its camelCase code (<c>LongLife</c> is <c>longLife</c>, <see href="../../../docs/architecture/adr/0021-enum-values-camel-case.md">ADR 0021</see>).</summary>
public static class CamelCaseEnumConventions
{
    /// <summary>Call from <c>DbContext.ConfigureConventions</c> of every context.</summary>
    public static ModelConfigurationBuilder UseCamelCaseEnums(this ModelConfigurationBuilder configuration)
    {
        configuration.Conventions.Add(_ => new CamelCaseEnumConvention());
        return configuration;
    }

    private sealed class CamelCaseEnumConvention : IModelFinalizingConvention
    {
        public void ProcessModelFinalizing(IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
        {
            foreach (var property in modelBuilder.Metadata.GetEntityTypes().SelectMany(type => type.GetDeclaredProperties()))
            {
                var enumType = Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType;
                if (!enumType.IsEnum || property.GetValueConverter() is not null) continue;
                var converter = (ValueConverter)Activator.CreateInstance(typeof(CamelCaseEnumConverter<>).MakeGenericType(enumType))!;
                property.Builder.HasConversion(converter);
            }
        }
    }

    private sealed class CamelCaseEnumConverter<TEnum>()
        : ValueConverter<TEnum, string>(value => ContractEnumNames.ToCode(value), code => ContractEnumNames.Parse<TEnum>(code))
        where TEnum : struct, Enum;
}
