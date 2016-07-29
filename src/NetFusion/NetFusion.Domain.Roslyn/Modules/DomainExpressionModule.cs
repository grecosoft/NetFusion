using Autofac;
using NetFusion.Bootstrap.Exceptions;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Domain.Scripting;
using NetFusion.Domain.Roslyn.Core;
using System.Collections.Generic;

namespace NetFusion.Domain.Roslyn.Modules
{
    public class DomainExpressionModule : PluginModule
    {
        private IEnumerable<EntityScript> _expressions;

        public override void RegisterComponents(ContainerBuilder builder)
        {
            builder.RegisterType<EntityScriptingService>()
                .As<IEntityScriptingService>()
                .SingleInstance();
        }

        public override void RunModule(ILifetimeScope scope)
        {
            IEntityScriptRepository expressionRep = null;
            if (!scope.TryResolve(out expressionRep))
            {
                throw new ContainerException(
                    $"An component implementing the interface: {typeof(IEntityScriptRepository)} " +
                    $"is not registered.");
            }

            _expressions = expressionRep.ReadAll().Result;
            var evaluationSrv = scope.Resolve<IEntityScriptingService>();

            evaluationSrv.Load(_expressions);
        }

        public override void Log(IDictionary<string, object> moduleLog)
        {
            //moduleLog["Expressions"] = _expressions.GroupBy(
            //    e => e.EntityType,
            //    (et, es) => new
            //    {
            //        EntityType = et,
            //        Expressions = es.Select(e => new
            //        {
            //            e.Id,
            //            e.PropertyName,
            //            e.Expression
            //        })
            //    }).ToDictionary(e => e.EntityType);
        }
    }
}
