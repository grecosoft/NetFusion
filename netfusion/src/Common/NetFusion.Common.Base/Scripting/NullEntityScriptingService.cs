using System.Collections.Generic;
using System.Threading.Tasks;

namespace NetFusion.Common.Base.Scripting;

/// <summary>
/// By default, this NULL implementation is registered by the composite-container.
/// However, if the host application wants to utilize dynamic evaluated expresses, they
/// must register an implementation. Such as provided by the NetFusion.Services.Roslyn plugin.
/// </summary>
public class NullEntityScriptingService : IEntityScriptingService
{
    public void CompileAllScripts()
    {
            
    }

    public Task ExecuteAsync(object entity, string scriptName = "default")
    {
        return Task.CompletedTask;
    }

    public void Load(IEnumerable<EntityScript> scripts)
    {
           
    }

    public Task<bool> SatisfiesPredicateAsync(object entity, ScriptPredicate predicate) 
    {
        return Task.FromResult(true);
    }
}