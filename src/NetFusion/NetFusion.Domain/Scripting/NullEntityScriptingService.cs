using System.Collections.Generic;
using System.Threading.Tasks;

namespace NetFusion.Domain.Scripting
{
    /// <summary>
    /// If a host uses a plug-in that is dependent on the NetFusion.Domain plug-in's
    /// IEntityScriptingSerivce, but doesn't need to utilize it provided functionality,
    /// an singleton instance of this NULL implementation can be registered.
    /// </summary>
    public class NullEntityScriptingService : IEntityScriptingService
    {
        public void CompileAllScripts()
        {
            
        }

        public Task ExecuteAsync(object entity, string scriptName = "default")
        {
            return Task.FromResult(default(object));
        }

        public void Load(IEnumerable<EntityScript> scripts)
        {
           
        }

        public Task<bool> SatifiesPredicate(object entity, ScriptPredicate predicate) 
        {
            return Task.FromResult(true);
        }
    }
}
