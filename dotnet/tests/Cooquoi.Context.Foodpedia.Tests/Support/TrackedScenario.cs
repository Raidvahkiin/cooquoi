using Alba;
using Wolverine.Tracking;

namespace Cooquoi.Context.Foodpedia.Tests.Support;

public sealed record TrackedScenario(IScenarioResult Result, ITrackedSession Messages);
