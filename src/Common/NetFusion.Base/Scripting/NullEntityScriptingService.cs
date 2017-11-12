using System.Collections.Generic;
using System.Threading.Tasks;

namespace NetFusion.Base.Scripting
{
    /// <summary>
    /// If a host uses a plug-in that is dependent on the IEntityScriptingSerivce, but doesn't 
    /// need to utilize its provided functionality, an singleton instance of this NULL implementation 
    /// can be registered.
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
}
