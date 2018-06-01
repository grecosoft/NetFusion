﻿//using FluentAssertions;
//using NetFusion.Domain.Scripting;
//using NetFusion.Tests.Core.Domain.Mocks;
//using System.Collections.Generic;
//using Xunit;

//namespace NetFusion.Tests.Core.Domain
//{
//    public class EntityEvaluationTests
//    {
//        private readonly IEntityScriptingService EvalSrv;

//        public EntityEvaluationTests()
//        {
//            this.EvalSrv = CreateEvaluationService();
//        }

//        /// <summary>
//        /// A new calculated attribute can be calculated and added 
//        /// to the set of entity attribute values.
//        /// </summary>
//        [Fact]
//        public void CanAddCalculatedDynamicAttribute()
//        {
//            var expressions = new List<EntityExpression>();
//            var entity = CreateDefaultEntity();

//            entity.Attributes.Delete("Value2");
//            entity.Attributes.Values.Value1 = 10;
//            this.EvalSrv.ExecuteAsync(entity).Wait();

//            entity.Attributes.Contains("Value2").Should().BeTrue();
//            var result = (int)entity.Attributes.Values.Value2;
//            result.Should().Be(110);
//        }

//        /// <summary>
//        /// A new calculated entity attribute value can be updated by a
//        /// proceeding expression.
//        /// </summary>
//        [Fact]
//        public void CanUpdateCalculatedDynamicAttribute()
//        {
//            var expressions = new List<EntityExpression>();
//            var entity = CreateDefaultEntity();

//            entity.Attributes.Values.Value1 = 0;
//            entity.Attributes.Values.Value3 = 105;
//            this.EvalSrv.ExecuteAsync(entity).Wait();

//            entity.Attributes.Contains("Value4").Should().BeTrue();
//            var result = (int)entity.Attributes.Values.Value4;
//            result.Should().Be(5);
//        }

//        /// <summary>
//        /// A newly calculated entity attribute value can be based on
//        /// an entity static property.
//        /// </summary>
//        [Fact]
//        public void ExpressionCanContainEntityStaticProperty()
//        {
//            var expressions = new List<EntityExpression>();
//            var entity = CreateDefaultEntity();

//            entity.IsActive = true;
//            this.EvalSrv.ExecuteAsync(entity).Wait();

//            entity.Attributes.Contains("Value5").Should().BeTrue();
//            var result = (int)entity.Attributes.Values.Value5;
//            result.Should().Be(2000);
//        }

//        [Fact]
//        public void ExpressionCanCombineEntityPropertyAndDynamicAttribute()
//        {
//            var expressions = new List<EntityExpression>();
//            var entity = CreateDefaultEntity();

//            entity.MaxValue = 1000;
//            entity.Attributes.Values.Value6 = 200;
//            this.EvalSrv.ExecuteAsync(entity).Wait();

//            entity.Attributes.Contains("Value7").Should().BeTrue();
//            var result = (int)entity.Attributes.Values.Value7;
//            result.Should().Be(1200);
//        }

//        [Fact]
//        public void ExpressionCanDependOnPriorCalculatedValues()
//        {
//            var expressions = new List<EntityExpression>();
//            var entity = CreateDefaultEntity();

//            entity.MaxValue = 1000;
//            entity.Attributes.Values.Value1 = 200;
//            this.EvalSrv.ExecuteAsync(entity).Wait();

//            entity.MinValue.Should().Be(300);
//        }

//        // Creates an initial default domain entity with dynamic entity
//        // attributes.
//        private DynamicEntity CreateDefaultEntity()
//        {
//            var entity = new DynamicEntity();
//            entity.Attributes.Values.Value1 = 0;
//            entity.Attributes.Values.Value2 = 0;
//            entity.Attributes.Values.Value3 = 0;
//            entity.Attributes.Values.Value4 = 0;
//            entity.Attributes.Values.Value5 = 0;
//            entity.Attributes.Values.Value6 = 0;
//            entity.Attributes.Values.Value7 = 0;
//            entity.Attributes.Values.Value8 = "";

//            return entity;
//        }

//        private IEntityScriptingService CreateEvaluationService()
//        {
//            // CanAddCalculatedDynamicAttribute
//            var expressions = new List<EntityExpression>();
//            expressions.AddExpression<DynamicEntity>("Value2", "_.Value1 + 100");

//           // CanUpdateCalculatedDynamicAttribute
//           expressions.AddExpression<DynamicEntity>("Value4", "_.Value3 + 100");
//           expressions.AddExpression<DynamicEntity>("Value4", "_.Value4 > 200 ? 5 : 10");

//            // ExpressionCanContainEntityStaticProperty
//            expressions.AddExpression<DynamicEntity>("Value5", "Entity.IsActive ? 2000 : 1000");

//            // ExpressionCanCombineEntityPropertyAndAttribute
//            expressions.AddExpression<DynamicEntity>("Value7", "Entity.MaxValue + _.Value6");

//            // ExpressionCanDependOnPriorCalculatedValues
//            expressions.AddExpression<DynamicEntity>("Entity.MaxValue = ++Entity.MaxValue");
//            expressions.AddExpression<DynamicEntity>("Entity.MinValue = System.Math.Min(_.Value2, Entity.MaxValue)");

//            expressions.AddExpression<DynamicEntity>("_Value8", "ObjectExtensions.ToIndentedJson(Entity.MaxValue)");

//            return expressions.CreateService();
           
//        }
//    }

//}
