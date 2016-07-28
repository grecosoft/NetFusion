using Autofac;
using NetFusion.Bootstrap.Exceptions;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Domain.Entity;
using NetFusion.Domain.Entity.Services;
using NetFusion.Domain.Roslyn.Core;
using NetFusion.Domain.Services;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.Domain.Roslyn.Modules
{
    public class DomainExpressionModule : PluginModule
    {
        private IEnumerable<EntityExpressionSet> _expressions;

        public override void RegisterComponents(ContainerBuilder builder)
        {
            builder.RegisterType<EntityEvaluationService>()
                .As<IEntityEvaluationService>()
                .SingleInstance();
        }

        public override void RunModule(ILifetimeScope scope)
        {
            IExpressionMetadataRepository expressionRep = null;
            if (!scope.TryResolve(out expressionRep))
            {
                throw new ContainerException(
                    $"An component implementing the interface: {typeof(IExpressionMetadataRepository)} " +
                    $"is not registered.");
            }

            _expressions = expressionRep.ReadAll().Result;
            var evaluationSrv = scope.Resolve<IEntityEvaluationService>();

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
