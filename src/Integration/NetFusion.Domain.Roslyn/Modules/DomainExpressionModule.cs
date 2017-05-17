using Autofac;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Domain.Roslyn.Core;
using NetFusion.Domain.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.Domain.Roslyn.Modules
{
    /// <summary>
    /// Module that loads meta data containing expressions that can be evaluated
    /// against and entity and its set of related dynamic properties.
    /// </summary>
    public class DomainExpressionModule : PluginModule
    {
        // The list of scripts that can be applied against entities 
        // of the same matching type.
        private IEnumerable<EntityScript> _scripts;

        public override void RegisterDefaultComponents(ContainerBuilder builder)
        {
            // If this plug-in is being used, override the Null scripting service
            // that is configured by default by the NetFusion.Domain plug-in.
            builder.RegisterType<EntityScriptingService>()
                .As<IEntityScriptingService>()
                .SingleInstance();
        }

        public override void RunModule(ILifetimeScope scope)
        {
            IEntityScriptMetaRepository expressionRep = null;
            if (!scope.TryResolve(out expressionRep))
            {
                throw new InvalidOperationException(
                    $"An component implementing the interface: {typeof(IEntityScriptMetaRepository)} " +
                    $"is not registered.");
            }

            // Read all of the scripts and load them into the scripting service.
            _scripts = expressionRep.ReadAll().Result;
            var scriptingSrv = scope.Resolve<IEntityScriptingService>();

            scriptingSrv.Load(_scripts);
        }

        public override void Log(IDictionary<string, object> moduleLog)
        {
           moduleLog["EntityScripts"] = _scripts.ToDictionary(s => s.EntityType, s =>
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
