using NetFusion.Domain.Entity;
using NetFusion.Domain.Scripting;
using NetFusion.Domain.Roslyn.Core;
using NetFusion.Tests.Core.Domain.Mocks;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NetFusion.Tests.Core.Domain
{
    public static class EntityEvaluationTestExtensions
    {
        public static IList<EntityExpression> AddExpression<T>(this IList<EntityExpression> expressions,
            string propertyName,
            string expression) where T: IAttributedEntity
        {
            expressions.Add(new EntityExpression(expression, 0, propertyName));
            return expressions;
        }

        public static IList<EntityExpression> AddExpression<T>(this IList<EntityExpression> expressions,
            string expression) where T : IAttributedEntity
        {
            expressions.Add(new EntityExpression(expression, 0));
            return expressions;
        }

        public static IEntityScriptingService CreateService(this IList<EntityExpression> expressions)
        {
            var es = new EntityScript(
                "",
                "default", 
                typeof(DynamicEntity).AssemblyQualifiedName,
                new ReadOnlyCollection<EntityExpression>(expressions));
                
            var evalSrv = new EntityScriptingService();
            evalSrv.Load(new EntityScript[] { es });
            return evalSrv;
        }


    }
}
