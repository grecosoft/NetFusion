using FluentAssertions;
using NetFusion.Domain.Entity;
using NetFusion.Domain.Entity.Services;
using NetFusion.Tests.Core.Domain.Mocks;
using System.Collections.Generic;
using Xunit;

namespace NetFusion.Tests.Core.Domain
{
    public class EntityEvaluationTests
    {
        private readonly IEntityEvaluationService EvalSrv;

        public EntityEvaluationTests()
        {
            this.EvalSrv = CreateEvaluationService();
        }

        /// <summary>
        /// A new calculated attribute can be calculated and added 
        /// to the set of entity attribute values.
        /// </summary>
        [Fact]
        public void CanAddCalculatedAttribute()
        {
            var expressions = new List<EntityPropertyExpression>();
            var entity = CreateDefaultEntity();

            entity.DeleteAttribute("Value2");
            entity.Attributes.Value1 = 10;
            this.EvalSrv.Evaluate(entity).Wait();

            entity.ContainsAttribute("Value2").Should().BeTrue();
            var result = (int)entity.Attributes.Value2;
            result.Should().Be(110);
        }

        /// <summary>
        /// A new calculated entity attribute value can be updated by a
        /// proceeding expression.
        /// </summary>
        [Fact]
        public void CanUpdateCalculatedAttribute()
        {
            var expressions = new List<EntityPropertyExpression>();
            var entity = CreateDefaultEntity();

            entity.Attributes.Value1 = 0;
            entity.Attributes.Value3 = 105;
            this.EvalSrv.Evaluate(entity).Wait();

            entity.ContainsAttribute("Value4").Should().BeTrue();
            var result = (int)entity.Attributes.Value4;
            result.Should().Be(5);
        }

        /// <summary>
        /// A newly calculated entity attribute value can be based on
        /// an entity static property.
        /// </summary>
        [Fact]
        public void ExpressonCanContainEntityStaticProperty()
        {
            var expressions = new List<EntityPropertyExpression>();
            var entity = CreateDefaultEntity();

            entity.IsActive = true;
            this.EvalSrv.Evaluate(entity).Wait();

            entity.ContainsAttribute("Value5").Should().BeTrue();
            var result = (int)entity.Attributes.Value5;
            result.Should().Be(2000);
        }

        [Fact]
        public void ExpressionCanCombineEntityPropertyAndAttribute()
        {

        }

        [Fact]
        public void ExpressionCanDependOnPriorCalculatedProperty()
        {

        }

        [Fact]
        public void ExpressionCanDependOnPriorCalculatedAttribute()
        {

        }

        // Creates an initial default domain entity with dynamic entity
        // attributes.
        private DynamicEntity CreateDefaultEntity()
        {
            var entity = new DynamicEntity();
            entity.Attributes.Value1 = 0;
            entity.Attributes.Value2 = 0;
            entity.Attributes.Value3 = 0;
            entity.Attributes.Value4 = 0;

            return entity;
        }

        private IEntityEvaluationService CreateEvaluationService()
        {
            // CanAddCalculatedAttribute
            var expressions = new List<EntityPropertyExpression>();
            expressions.AddExpression<DynamicEntity>("Value2", "_.Value1 + 100");

           // CanUpdateCalculatedAttribute
           expressions.AddExpression<DynamicEntity>("Value4", "_.Value3 + 100");
           expressions.AddExpression<DynamicEntity>("Value4", "_.Value4 > 200 ? 5 : 10");

            // ExpressonCanContainEntityStaticProperty
            expressions.AddExpression<DynamicEntity>("Value5", "Entity.IsActive ? 2000 : 1000");
            return expressions.CreateService();
        }
    }

}
