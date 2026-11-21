using System.Reflection;

namespace Cooquoi.Architecture.Tests;

/// <summary>
/// Wolverine discovers message handlers by name only (<c>…Handler</c>, <c>…Consumer</c>). A
/// <c>Handle</c> method on another class is silently ignored: its events are published "with no
/// subscribers" (found in spike phase 2).
/// </summary>
public sealed class HandlerRulesTests
{
    private static readonly string[] HandlerMethods = ["Handle", "HandleAsync", "Consume", "ConsumeAsync"];

    [Fact]
    public void Classes_with_handler_methods_are_named_Handler_or_Consumer()
    {
        var misnamed = CooquoiArchitecture.OwnAssemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsClass && !type.Name.StartsWith('<'))
            .Where(type => type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Any(method => HandlerMethods.Contains(method.Name)))
            .Where(type => !type.Name.EndsWith("Handler", StringComparison.Ordinal) && !type.Name.EndsWith("Consumer", StringComparison.Ordinal))
            .Select(type => type.FullName);

        misnamed.ShouldBeEmpty();
    }
}
