using NetFusion.Domain.Entity;
using NetFusion.Domain.Entity.Services;
using NetFusion.Domain.Roslyn.Core;
using System.Collections.Generic;

namespace NetFusion.Tests.Core.Domain
{
    public static class EntityEvaluationTestExtensions
    {
        public static IList<EntityPropertyExpression> AddExpression<T>(this IList<EntityPropertyExpression> expressions,
            string propertyName,
            string expression) where T: IAttributedEntity
        {
            expressions.Add(new EntityPropertyExpression(typeof(T), propertyName, expression));
            return expressions;
        }

        public static IEntityEvaluationService CreateService(this IList<EntityPropertyExpression> expressions)
        {
            var evalSrv = new EntityEvaluationService();
            evalSrv.Load(expressions);
            return evalSrv;
        }


    }
}
