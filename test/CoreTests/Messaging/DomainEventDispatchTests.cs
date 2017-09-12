using Autofac;
using CoreTests.Messaging.Mocks;
using FluentAssertions;
using NetFusion.Base.Scripting;
using NetFusion.Bootstrap.Container;
using NetFusion.Common;
using NetFusion.Domain.Messaging;
using NetFusion.Messaging;
using NetFusion.Test.Container;
using NetFusion.Test.Plugins;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace CoreTests.Messaging
{
    /// <summary>
    /// Tests for asserting the basic publishing and handling of derived message types.
    /// </summary>
    public class DomainEventDispatchTests
    {
        /// <summary>
        /// When a domain event is published using the service, the corresponding discovered
        /// consumer event handler methods will be invoked.
        /// </summary>
        [Fact (DisplayName = nameof(DomainEventConsumer_HandlerInvoked))]
        public Task DomainEventConsumer_HandlerInvoked()
        {
            return DefaultSetup.EventConsumer.Test(
                async c => 
                {
                    c.Build();

                    var mockEvt = new MockDomainEvent();
                    await c.Services.Resolve<IMessagingService>()
                        .PublishAsync(mockEvt);
                }, 
                (IAppContainer c) =>
                {
                    var consumer = c.Services.Resolve<IEnumerable<IMessageConsumer>>()
                       .OfType<MockDomainEventConsumer>()
                       .First();

                    consumer.ExecutedHandlers.Should().Contain("OnEventHandlerOne");
                });
        }

        /// <summary>
        /// A consumer event handler for a base domain event type can be marked
        /// with the IncludeDerivedEvents attribute to indicate it should be 
        /// called for any derived domain events.
        /// </summary>
        [Fact(DisplayName = nameof(EventHandlerForBaseType_InvokedIfAppliedAttribute))]
        public Task EventHandlerForBaseType_InvokedIfAppliedAttribute()
        {
            return ConsumerWithDerivedEventHandler.Test(
                async c =>
                {
                    c.Build();

                    var mockEvt = new MockDerivedDomainEvent();
                    await c.Services.Resolve<IMessagingService>()
                        .PublishAsync(mockEvt);
                },
                (IAppContainer c) =>
                {
                    var consumer = c.Services.Resolve<IEnumerable<IMessageConsumer>>()
                        .OfType<MockBaseMessageConsumer>()
                        .First();

                    consumer.ExecutedHandlers.Should().ContainSingle("OnIncludeBaseEventHandler");
                });
        }

        [Fact(DisplayName = nameof(ExceptionsRecorded_ForEachEventHandler))]
        public Task ExceptionsRecorded_ForEachEventHandler()
        {
            return ContainerSetup.Arrange((TestTypeResolver config) => {

                    config.AddPlugin<MockAppHostPlugin>()
                        .AddPluginType<MockDomainEvent>()
                        .AddPluginType<MockErrorMessageConsumer>();

                    config.AddPlugin<MockCorePlugin>()
                        .UseMessagingPlugin();
                })
                .Test(async c =>
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

                    c.Build();

                    var srv = c.Services.Resolve<IMessagingService>();
                    var evt = new MockDomainEvent();
                    await srv.PublishAsync(evt);
                },
                (c, e) =>
                {
                    e.Should().NotBeNull();
                    e.Should().BeOfType<PublisherException>();
                });
        }

        //--------------------------------TEST SPECIFIC SETUP------------------------------------------//

        public class MockBaseDomainEvent : DomainEvent
        {

        }

        public class MockDerivedDomainEvent : MockBaseDomainEvent
        {
        }

        public class MockBaseMessageConsumer : MockConsumer,
        IMessageConsumer
        {

            [InProcessHandler]
            public void OnBaseEventHandler(MockBaseDomainEvent domainEvent)
            {
                Check.NotNull(domainEvent, nameof(domainEvent));
                AddCalledHandler("OnBaseEventHandler");
            }

            [InProcessHandler]
            public void OnIncludeBaseEventHandler([IncludeDerivedMessages]MockBaseDomainEvent domainEvent)
            {
                Check.NotNull(domainEvent, nameof(domainEvent));
                AddCalledHandler("OnIncludeBaseEventHandler");
            }
        }

        public static ContainerTest ConsumerWithDerivedEventHandler => ContainerSetup
            .Arrange((TestTypeResolver config) =>
            {
                config.AddPlugin<MockAppHostPlugin>()
                    .AddPluginType<MockDerivedDomainEvent>()
                    .AddPluginType<MockBaseMessageConsumer>();

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
    }
}
