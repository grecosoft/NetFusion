using Autofac;
using FluentAssertions;
using NetFusion.Bootstrap.Container;
using NetFusion.Messaging;
using NetFusion.Messaging.Config;
using NetFusion.Messaging.Core;
using NetFusion.Messaging.Modules;
using NetFusion.Test.Container;
using NetFusion.Test.Plugins;
using Xunit;

namespace CoreTests.Messaging
{
    /// <summary>
    /// Tests that assert that the messaging plug-in was correctly initialized
    /// by the bootstrap process.
    /// </summary>
    public class ConfigurationTests
    {
        /// <summary>
        /// The domain event plug-in has a configuration class for specifying related settings.
        /// </summary>
        [Fact]
        public void Configuration_SetOnPlugin()
        {
            DefaultSetup.EventConsumer
                .Test(c =>
                {
                    c.WithConfig<MessagingConfig>();
                    c.Build();
                },
                (MessagingModule m) =>
                {
                    var moduleConfig = m.Context.Plugin.GetConfig<MessagingConfig>();
                    moduleConfig.Should().NotBeNull();
                });
        }

        /// <summary>
        /// Unless cleared, the InProcessEventDispatcher will be used by default
        /// to dispatch events to local consumer event handlers.  
        /// </summary>
        [Fact(DisplayName = nameof(InProcessEventDispatcher_ConfiguredByDefault))]
        public void InProcessEventDispatcher_ConfiguredByDefault()
        {
            ContainerSetup
                .Arrange((TestTypeResolver config) =>
                {
                    config.AddPlugin<MockAppHostPlugin>();
                    config.AddPlugin<MockCorePlugin>()
                        .UseMessagingPlugin();

                })
                .TestConfig(c =>
                {
                    c.WithConfig<MessagingConfig>();
                    c.Build();

                },
                (MessagingConfig ca) =>
                {
                    ca.PublisherTypes.Should().HaveCount(1);
                    ca.PublisherTypes.Should().Contain(typeof(InProcessMessagePublisher));
                });
        }

        /// <summary>
        /// If the host application doesn't want to have the default InProcessEventDispatcher
        /// invoked, they can clear the list and add the desired dispatcher types.
        /// </summary>
        [Fact(DisplayName = nameof(DefaultDispatchers_CanBeCleared))]
        public void DefaultDispatchers_CanBeCleared()
        {
            ContainerSetup
                .Arrange((TestTypeResolver config) =>
                {
                    config.AddPlugin<MockAppHostPlugin>();
                    config.AddPlugin<MockCorePlugin>()
                        .UseMessagingPlugin();
                })
                .TestConfig(c =>
                {
                    c.WithConfig<MessagingConfig>(config => config.ClearMessagePublishers());
                    c.Build();

                }, 
                (MessagingConfig ca) =>
                {
                    ca.PublisherTypes.Should().HaveCount(0);
                });
        }

        /// <summary>
        /// The plug-in registers a service that can be used to publish messages.
        /// </summary>
        [Fact(DisplayName = nameof(RegistersService_ForPublishingEvents))]
        public void RegistersService_ForPublishingEvents()
        {
            DefaultSetup.EventConsumer
                .Test(
                    c => c.Build(),
                    (IAppContainer c) =>
                    {
                        var service = c.Services.Resolve<IMessagingService>();
                        service.Should().NotBeNull();
                    });
        }
    }
}
