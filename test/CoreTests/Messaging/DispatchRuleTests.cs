using Autofac;
using CoreTests.Messaging.Mocks;
using FluentAssertions;
using NetFusion.Base.Scripting;
using NetFusion.Bootstrap.Container;
using NetFusion.Messaging;
using NetFusion.Messaging.Rules;
using NetFusion.Messaging.Types;
using NetFusion.Messaging.Types.Rules;
using NetFusion.Test.Container;
using NetFusion.Test.Plugins;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace BootstrapTests.Messaging
{
    /// <summary>
    /// One or more rules can be associated with a message handler.  These rules are static
    /// and are evaluated based on the state of the message being published.  If the rule
    /// evaluates to true, the message handler is invoked.
    /// </summary>
    public class DispatchRuleTests
    {
        [Fact(DisplayName = nameof(HandlerCalled_WhenMessagePassesAllDispatchRules))]
        public Task HandlerCalled_WhenMessagePassesAllDispatchRules()
        {
            return TestContainer.Test(
                async c => 
                {
                    c.Build();

                    var mockEvt = new MockDomainEvent { RuleTestValue = 1500 };
                    await c.Services.Resolve<IMessagingService>()
                        .PublishAsync(mockEvt);
                }, 
                (IAppContainer c) =>
                {
                    var consumer = c.Services.Resolve<IEnumerable<IMessageConsumer>>()
                        .OfType<MockDomainEventConsumer>()
                        .First();

                    consumer.ExecutedHandlers.Should().Contain("OnEventAllRulesPass");
                });
        }

        [Fact(DisplayName = nameof(HandlerCalled_WhenMessagePassesAnyDispatchRule))]
        public Task HandlerCalled_WhenMessagePassesAnyDispatchRule()
        {
            return TestContainer.Test(
                async c => 
                {
                    c.Build();

                    var mockEvt = new MockDomainEvent { RuleTestValue = 3000 };
                    await c.Services.Resolve<IMessagingService>()
                        .PublishAsync(mockEvt);
                }, 
                (IAppContainer c) => 
                {
                    var consumer = c.Services.Resolve<IEnumerable<IMessageConsumer>>()
                        .OfType<MockDomainEventConsumer>()
                        .First();

                    consumer.ExecutedHandlers.Should().Contain("OnEventAnyRulePasses");
                });
        }

        //--------------------------------TEST SPECIFIC SETUP------------------------------------------//

        public static ContainerTest TestContainer => ContainerSetup
            .Arrange((TestTypeResolver config) =>
            {
                config.AddPlugin<MockAppHostPlugin>()
                    .AddPluginType<MockDomainEvent>()
                    .AddPluginType<MockDomainEventConsumer>()
                    .AddPluginType<MockRoleMin>()
                    .AddPluginType<MockRoleMax>();

                config.AddPlugin<MockCorePlugin>()
                    .UseMessagingPlugin();

            }, c =>
            {
                c.WithConfig<AutofacRegistrationConfig>(regConfig =>
                {
                    regConfig.Build = builder =>
                    {
                        builder.RegisterType<NullEntityScriptingService>()
                            .As<IEntityScriptingService>()
                            .SingleInstance();
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
            [InProcessHandler, ApplyDispatchRule(typeof(MockRoleMin), typeof(MockRoleMax), 
                RuleApplyType = RuleApplyTypes.All)]
            public void OnEventAllRulesPass(MockDomainEvent evt)
            {
                AddCalledHandler(nameof(OnEventAllRulesPass));
            }

            [InProcessHandler, ApplyDispatchRule(typeof(MockRoleMin), typeof(MockRoleMax), 
                RuleApplyType = RuleApplyTypes.Any)]
            public void OnEventAnyRulePasses(MockDomainEvent evt)
            {
                AddCalledHandler(nameof(OnEventAnyRulePasses));
            }
        }

        public class MockRoleMin : MessageDispatchRule<MockDomainEvent>
        {
            protected override bool IsMatch(MockDomainEvent message)
            {
                if (message.RuleTestValue == -1) return false;
                return 1000 <= message.RuleTestValue;
            }
        }

        public class MockRoleMax : MessageDispatchRule<MockDomainEvent>
        {
            protected override bool IsMatch(MockDomainEvent message)
            {
                if (message.RuleTestValue == -1) return false;
                return message.RuleTestValue <= 2000;
            }
        }
    }
}
