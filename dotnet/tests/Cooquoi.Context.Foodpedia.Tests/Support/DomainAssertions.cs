using Raidvahkiin.Ddd;

namespace Cooquoi.Context.Foodpedia.Tests.Support;

public static class DomainAssertions
{
    public static DomainException ShouldFailWith(this Action action, DomainError error)
    {
        var exception = Should.Throw<DomainException>(action);
        exception.Error.ShouldBe(error);
        return exception;
    }
}
