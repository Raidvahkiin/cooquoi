namespace Cooquoi.Context.Foodventory.Features.RenamePantry;

/// <summary>C2: body of <c>POST /v1/pantries/{pantryId}:rename</c>; requires <c>If-Match</c>.</summary>
public sealed record RenamePantry(string Name);
