using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetFusion.Base.Scripting;
using NetFusion.Domain.Roslyn.Core;
using NetFusion.Domain.Roslyn.Testing;
using NetFusion.Messaging;
using NetFusion.Messaging.Types;
using NetFusion.Test.Plugins;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CoreTests.Messaging.Mocks
{
    public static class TestEvalSetupExtensions
    {

        public static TestTypeResolver WithHostEvalBasedConsumer(this TestTypeResolver resolver)
        {
            resolver.AddPlugin<MockAppHostPlugin>()
                .AddPluginType<MockEvalDomainEvent>()
                .AddPluginType<MockDomainEventEvalBasedConsumer>();

            resolver.AddPlugin<MockCorePlugin>()
                .UseMessagingPlugin();

            return resolver;
        }

        public static IServiceCollection UseMockedEvalService(this IServiceCollection services)
        {
            var scriptingService = CreateEvalService();
            services.AddSingleton(scriptingService);

            return services;
        }

        // Creates a configured evaluation service with a single expression.
        private static IEntityScriptingService CreateEvalService()
        {
            var expressions = new List<EntityExpression>();
            expressions.AddExpression("IsImportant", "Entity.RuleTestValue > 5000");

            var es = new EntityScript(
                Guid.NewGuid().ToString(),
                "test-script",
                typeof(MockEvalDomainEvent).AssemblyQualifiedName,
                new ReadOnlyCollection<EntityExpression>(expressions));

            var loggerFactory = new LoggerFactory();
            var evalSrv = new EntityScriptingService(loggerFactory);

            evalSrv.Load(new[] { es });
            return evalSrv;
        }
    }

    //-------------------------- MOCKED TYPED --------------------------------------
    public class MockEvalDomainEvent : DomainEvent
    {
        public int RuleTestValue { get; set; } = -1;
    }

    public class MockDomainEventEvalBasedConsumer : MockConsumer,
        IMessageConsumer
    {

        [InProcessHandler, ApplyScriptPredicate("test-script", "IsImportant")]
        public void OnEventPredicatePases(MockEvalDomainEvent evt)
        {
            AddCalledHandler(nameof(OnEventPredicatePases));
        }
    }


}
