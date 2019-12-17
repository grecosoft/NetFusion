using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Base.Scripting;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Roslyn.Internal;

namespace NetFusion.Roslyn.Plugin.Modules
{
    /// <summary>
    /// Module that loads meta data containing expressions that can be evaluated
    /// against and entity and its set of related dynamic properties.
    /// </summary>
    public class ExpressionModule : PluginModule
    {
        // The list of scripts that can be applied against entities 
        // of the same matching type.
        private IEnumerable<EntityScript> _scripts;
        
        //------------------------------------------------------
        //--Plugin Initialization
        //------------------------------------------------------

        public override void RegisterServices(IServiceCollection services)
        {
            // If this plug-in is being used, override the Null scripting service
            // that is configured by default by ICompositeContainerBuilder.
            services.AddSingleton<IEntityScriptingService, EntityScriptingService>();
        }

        //------------------------------------------------------
        //--Plugin Execution
        //------------------------------------------------------
        
        protected override async Task OnRunModuleAsync(IServiceProvider services)
        {
            IEntityScriptMetaRepository expressionRep = services.GetService<IEntityScriptMetaRepository>();

            if (expressionRep == null)
            {
                throw new InvalidOperationException(
                    $"An component implementing the interface: {typeof(IEntityScriptMetaRepository)} " +
                     "is not registered.");
            }

            // Read all of the scripts and load them into the scripting service.
            _scripts = await expressionRep.ReadAllAsync();
            var scriptingSrv = services.GetService<IEntityScriptingService>();

            scriptingSrv.Load(_scripts);
        }

        public override void Log(IDictionary<string, object> moduleLog)
        {
            // For each entity type, store its associated list of scripts:
            var scriptsByEntity = _scripts.ToLookup(s => s.EntityType.FullName,
                es => new
                {
                    es.Name,
                    es.InitialAttributes,
                    es.ImportedAssemblies,
                    es.ImportedNamespaces,
                    Expressions = es.Expressions.OrderBy(e => e.Sequence)
                        .Select(e => new { e.AttributeName, e.Expression })
                });

            // Store the log as a dictionary for best serialization.
            moduleLog["EntityScripts"] = scriptsByEntity.ToDictionary(es => es.Key, es => es.Select(s => s));
        }
    }
}
