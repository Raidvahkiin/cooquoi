using JasperFx;
using JasperFx.CodeGeneration;
using JasperFx.CodeGeneration.Frames;
using Wolverine.Configuration;
using Wolverine.Runtime.Handlers;

namespace Cooquoi.Platform.Auditing;

/// <summary>Every message handler starts by naming its actor, so its saves are audited as that handler.</summary>
public sealed class SystemActorPolicy : IHandlerPolicy
{
    public void Apply(IReadOnlyList<HandlerChain> chains, GenerationRules rules, IServiceContainer container)
    {
        foreach (var chain in chains)
        {
            chain.Middleware.Insert(0, new EnterSystemActorFrame(SystemActor.For(chain.Handlers[0].HandlerType)));
        }
    }

    private sealed class EnterSystemActorFrame(string actor) : SyncFrame
    {
        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.WriteLine($"{typeof(SystemActor).FullName}.{nameof(SystemActor.Enter)}(\"{actor}\");");
            Next?.GenerateCode(method, writer);
        }
    }
}
