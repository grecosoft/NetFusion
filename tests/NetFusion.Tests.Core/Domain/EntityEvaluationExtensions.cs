using NetFusion.Domain.Entity;
using NetFusion.Domain.Entity.Services;
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
            expressions.Add(new EntityExpression(expression, 0, true, propertyName));
            return expressions;
        }

        public static IList<EntityExpression> AddExpression<T>(this IList<EntityExpression> expressions,
            string expression) where T : IAttributedEntity
        {
            expressions.Add(new EntityExpression(expression, 0, true));
            return expressions;
        }

        public static IEntityEvaluationService CreateService(this IList<EntityExpression> expressions)
        {
            var es = new EntityExpressionSet("", typeof(DynamicEntity).AssemblyQualifiedName, "", new ReadOnlyCollection<EntityExpression>(expressions));
                
            var evalSrv = new EntityEvaluationService();
            evalSrv.Load(new EntityExpressionSet[] { es });
            return evalSrv;
        }


    }
}
