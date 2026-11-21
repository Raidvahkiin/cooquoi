namespace Cooquoi.Platform.Authorization;

/// <summary>Authorization policies for <c>[Authorize(Policy = …)]</c> on endpoints.</summary>
public static class CooquoiPolicies
{
    /// <summary>F11: only catalogue curators change Foodpedia.</summary>
    public const string Curator = "curator";
}
