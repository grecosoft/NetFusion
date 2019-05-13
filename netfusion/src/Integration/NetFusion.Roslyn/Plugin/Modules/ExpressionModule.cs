using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Base.Scripting;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Roslyn.Core;

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

        public override void RegisterServices(IServiceCollection services)
        {
            // If this plug-in is being used, override the Null scripting service
            // that is configured by default by the NetFusion.Domain plug-in.
            services.AddSingleton<IEntityScriptingService, EntityScriptingService>();
        }

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
           moduleLog["Entity:Scripts"] = _scripts.ToDictionary(s => s.EntityType, s =>
           {
               return _scripts.Where(e => e.EntityType == s.EntityType)
                .ToDictionary(e => e.Name, es => new {
                    es.InitialAttributes,
                    es.ImportedAssemblies,
                    es.ImportedNamespaces,
                    Expressions = es.Expressions.OrderBy(e => e.Sequence)
                        .Select(e => new { e.AttributeName, e.Expression })
                });
           });
        }
    }
}
