namespace Cooquoi.Platform.Validation;

internal static class JsonPath
{
    /// <summary><c>Names[1].Primary</c> → <c>names[1].primary</c>, matching the JSON request body.</summary>
    public static string FromPropertyPath(string propertyPath) =>
        string.Join('.', propertyPath.Split('.').Select(segment => segment.Length == 0
            ? segment
            : char.ToLowerInvariant(segment[0]) + segment[1..]));
}
