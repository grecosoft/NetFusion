using Autofac;
using CoreTests.Messaging.Mocks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NetFusion.Bootstrap.Container;
using NetFusion.Domain.Messaging;
using NetFusion.Domain.Roslyn.Core;
using NetFusion.Domain.Roslyn.Testing;
using NetFusion.Domain.Scripting;
using NetFusion.Messaging;
using NetFusion.Test.Container;
using NetFusion.Test.Plugins;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace BootstrapTests.Messaging
{
    /// <summary>
    /// An in-process message handler method can be decorated with the ApplyScriptPredicate attribute.  
    /// This attribute is used to specify a script name associated with the derived message type and 
    /// the name of a property calculated by the script returning a boolean value  indicating if the
    /// message handler applies to the published message.
    /// </summary>
    public class DispatchEvaluationTests
    {
        [Fact(DisplayName = nameof(HandlerCalled_WhenMessagePassesDispathPredicate))]
        public Task HandlerCalled_WhenMessagePassesDispathPredicate()
        {

            return ScriptEventConsumer.Test(
                async c => {
                    c.Build();

                    var mockEvt = new MockDomainEvent { RuleTestValue = 7000 };
                    await c.Services.Resolve<IMessagingService>()
                        .PublishAsync(mockEvt);
                },
                (IAppContainer c) => {
                    var consumer = c.Services.Resolve<IEnumerable<IMessageConsumer>>()
                       .OfType<MockDomainEventConsumer>()
                       .First();

                    consumer.ExecutedHandlers.Should().Contain("OnEventPredicatePases");
                });
        }

        //--------------------------------TEST SPECIFIC SETUP------------------------------------------//

        public static ContainerTest ScriptEventConsumer => ContainerSetup
            .Arrange((TestTypeResolver config) =>
            {
                config.AddPlugin<MockAppHostPlugin>()
                    .AddPluginType<MockDomainEvent>()
                    .AddPluginType<MockDomainEventConsumer>();

                config.AddPlugin<MockCorePlugin>()
                    .UseMessagingPlugin();

            }, c =>
            {
                var scriptingService = CreateEvalService();
                c.WithConfig<AutofacRegistrationConfig>(regConfig =>
                {
                    regConfig.Build = builder =>
                    {
                        builder.RegisterInstance(scriptingService)
                            .As<IEntityScriptingService>();
                    };
                });
            });


        public class MockDomainEvent : DomainEvent
        {
            public int RuleTestValue { get; set; } = -1;
        }

        public class MockDomainEventConsumer : MockConsumer,
            IMessageConsumer
        {

            [InProcessHandler, ApplyScriptPredicate("test-script", "IsImportant")]
            public void OnEventPredicatePases(MockDomainEvent evt)
            {
                AddCalledHandler(nameof(OnEventPredicatePases));
            }
        }

        // Creates a configured evaluation service with a single expression.
        private static IEntityScriptingService CreateEvalService()
        {
            var expressions = new List<EntityExpression>();
            expressions.AddExpression("IsImportant", "Entity.RuleTestValue > 5000");

            var es = new EntityScript(
                Guid.NewGuid().ToString(),
                "test-script",
                typeof(MockDomainEvent).AssemblyQualifiedName,
                new ReadOnlyCollection<EntityExpression>(expressions));

            var loggerFactory = new LoggerFactory();
            var evalSrv = new EntityScriptingService(loggerFactory);

            evalSrv.Load(new EntityScript[] { es });
            return evalSrv;
        }
    }
}
    
