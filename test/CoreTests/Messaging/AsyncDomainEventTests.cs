using Autofac;
using CoreTests.Messaging.Mocks;
using FluentAssertions;
using NetFusion.Base.Scripting;
using NetFusion.Bootstrap.Container;
using NetFusion.Messaging;
using NetFusion.Messaging.Config;
using NetFusion.Test.Container;
using NetFusion.Test.Plugins;
using System.Threading.Tasks;
using Xunit;

namespace BootstrapTests.Messaging
{
    /// <summary>
    /// Unit tests for asynchronous handling of messages.  All messages published are assumed to be 
    /// handled asynchronously.  The message handler can be either synchronous or asynchronous and 
    /// is normalized by using a TaskCompletionSource.  This easily allows the message handler to 
    /// be changed without having to change any of the calling code.
    /// </summary>
    public class AsyncDomainEventTests
    {
        /// <summary>
        /// When a domain-event is published, the event handlers can be asynchronous.  This test 
        /// publishes an event that will be handled by two asynchronous handlers and one synchronous.
        /// </summary>
        [Fact(DisplayName = nameof(DomainEventAsyncHandlers_CanBeInvoked))]
        public Task DomainEventAsyncHandlers_CanBeInvoked()
        {
            return AsyncMessageConsumer.Test(
                async c =>
                {
                    c.WithConfig<MessagingConfig>();
                    c.Build();

                    var domainEventSrv = c.Services.Resolve<IMessagingService>();
                    var evt = new MockDomainEvent();
                    await domainEventSrv.PublishAsync(evt);
                },
                (IAppContainer c) =>
                {
                    var consumer = c.Services.Resolve<MockAsyncMessageConsumer>();
                    consumer.ExecutedHandlers.Should().HaveCount(3);
                    consumer.ExecutedHandlers.Should().Contain("OnEvent1Async", "OnEvent2Async", "OnEvent3");
                }
            );
        }

        //--------------------------------TEST SPECIFIC SETUP------------------------------------------//

        public class MockAsyncMessageConsumer : MockConsumer,
            IMessageConsumer
        {
            [InProcessHandler]
            public Task OnEvent1Async(MockDomainEvent evt)
            {
                AddCalledHandler(nameof(OnEvent1Async));
                return Task.Run(() =>
                {

                });
            }

            [InProcessHandler]
            public Task OnEvent2Async(MockDomainEvent evt)
            {
                AddCalledHandler(nameof(OnEvent2Async));
                return Task.Run(() =>
                {

                });
            }

            [InProcessHandler]
            public void OnEvent3(MockDomainEvent evt)
            {
                AddCalledHandler(nameof(OnEvent3));
            }
        }

        public static ContainerTest AsyncMessageConsumer => ContainerSetup
            .Arrange((TestTypeResolver config) =>
            {
                config.AddPlugin<MockAppHostPlugin>()
                    .AddPluginType<MockDomainEvent>()
                    .AddPluginType<MockAsyncMessageConsumer>();

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
