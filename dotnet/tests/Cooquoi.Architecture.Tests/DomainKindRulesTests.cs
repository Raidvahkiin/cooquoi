using System.Reflection;
using Raidvahkiin.Ddd;

namespace Cooquoi.Architecture.Tests;

/// <summary>ADR 0014 §7: the kind of a domain type is visible in its name, and values are immutable.</summary>
public sealed class DomainKindRulesTests
{
    [Fact]
    public void Aggregate_roots_and_only_they_are_named_Aggregate()
    {
        var mismatches = CooquoiArchitecture.DomainTypes
            .Where(type => type.IsClass && DerivesFrom(type, typeof(AggregateRoot<>)) != type.Name.EndsWith("Aggregate", StringComparison.Ordinal))
            .Select(type => type.FullName);

        mismatches.ShouldBeEmpty();
    }

    [Fact]
    public void Entities_and_only_they_are_named_Entity()
    {
        var mismatches = CooquoiArchitecture.DomainTypes
            .Where(type => type.IsClass && DerivesFrom(type, typeof(Entity<>)) != type.Name.EndsWith("Entity", StringComparison.Ordinal))
            .Select(type => type.FullName);

        mismatches.ShouldBeEmpty();
    }

    [Fact]
    public void Domain_events_and_only_they_are_named_DomainEvent_and_live_in_an_Events_namespace()
    {
        var mismatches = CooquoiArchitecture.DomainTypes
            .Where(type => type.IsClass || type.IsValueType && !type.IsEnum)
            .Where(type => typeof(IDomainEvent).IsAssignableFrom(type) != type.Name.EndsWith("DomainEvent", StringComparison.Ordinal)
                || typeof(IDomainEvent).IsAssignableFrom(type) != IsInEventsNamespace(type))
            .Select(type => type.FullName);

        mismatches.ShouldBeEmpty();
    }

    [Fact]
    public void Integration_events_are_named_Event_but_not_DomainEvent_and_only_they_live_in_PublicApi_Events()
    {
        var integrationEvents = CooquoiArchitecture.OwnAssemblies.SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.Namespace?.StartsWith("Cooquoi.Context.", StringComparison.Ordinal) == true && !type.Name.StartsWith('<'))
            .Where(type => type.Namespace!.EndsWith(".PublicApi.Events", StringComparison.Ordinal))
            .ToList();

        integrationEvents.ShouldNotBeEmpty();
        var mismatches = integrationEvents
            .Where(type => !type.Name.EndsWith("Event", StringComparison.Ordinal) || type.Name.EndsWith("DomainEvent", StringComparison.Ordinal) || typeof(IDomainEvent).IsAssignableFrom(type))
            .Select(type => type.FullName);

        mismatches.ShouldBeEmpty();
    }

    [Fact]
    public void Every_other_domain_type_is_immutable()
    {
        var mutable = CooquoiArchitecture.DomainTypes
            .Where(type => !type.IsEnum && !DerivesFrom(type, typeof(AggregateRoot<>)) && !DerivesFrom(type, typeof(Entity<>)))
            .SelectMany(type => MutableMembers(type).Select(member => $"{type.Name}.{member}"));

        mutable.ShouldBeEmpty();
    }

    private static bool IsInEventsNamespace(Type type) => type.Namespace?.EndsWith(".Events", StringComparison.Ordinal) == true;

    private static IEnumerable<string> MutableMembers(Type type)
    {
        const BindingFlags Instance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
        var settable = type.GetProperties(Instance)
            .Where(property => property.SetMethod is { } setter && !IsInitOnly(setter))
            .Select(property => property.Name);
        var writableFields = type.GetFields(Instance)
            .Where(field => !field.IsInitOnly && !field.Name.Contains("k__BackingField", StringComparison.Ordinal))
            .Select(field => field.Name);
        return settable.Concat(writableFields);
    }

    private static bool IsInitOnly(MethodInfo setter) =>
        setter.ReturnParameter.GetRequiredCustomModifiers().Any(modifier => modifier.FullName == "System.Runtime.CompilerServices.IsExternalInit");

    private static bool DerivesFrom(Type type, Type genericBase)
    {
        for (var current = type.BaseType; current is not null; current = current.BaseType)
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition() == genericBase)
            {
                return true;
            }
        }

        return false;
    }
}
