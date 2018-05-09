using FluentAssertions;
using NetFusion.Messaging;
using NetFusion.Messaging.Config;
using NetFusion.Messaging.Core;
using NetFusion.Messaging.Modules;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Test.Container;
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
        /// The messaging plug-in has a configuration class for specifying related settings.
        /// If the host application doesn't specify configuration a default one is used.  
        /// See next unit-test.
        /// </summary>
        [Fact (DisplayName = "Messaging plug-in can reference Configuration.")]
        public void MessagingPlugin_CanReferenceConfiguration()
        {
            ContainerFixture.Test(fixture => { fixture
                .Arrange
                .Resolver(r => r.WithHostConsumer())
                .Container(c => c.WithConfig<MessageDispatchConfig>())

                .Assert.PluginModule<MessageDispatchModule>(m =>
                {
                    var moduleConfig = m.Context.Plugin.GetConfig<MessageDispatchConfig>();
                    moduleConfig.Should().NotBeNull();
                });
            });
        }

        /// <summary>
        /// Unless cleared, the InProcessEventDispatcher will be used by default
        /// to dispatch events to local consumer event handlers.  This is also the
        /// default configuration if the host does not provide one.
        /// </summary>
        [Fact(DisplayName = "In-Process event dispatcher configured by default")]
        public void InProcessEventDispatcher_ConfiguredByDefault()
        {
            ContainerFixture.Test(fixture => { fixture
                .Arrange
                .Resolver(r => r.WithHostConsumer())


                .Assert.Configuration<MessageDispatchConfig>(config =>
                {
                    config.PublisherTypes.Should().HaveCount(1);
                    config.PublisherTypes.Should().Contain(typeof(InProcessMessagePublisher));
                });
            });
        }

        /// <summary>
        /// If the host application doesn't want to have the default InProcessEventDispatcher
        /// invoked, they can clear the list and add the desired dispatcher types.
        /// </summary>
        [Fact(DisplayName = nameof(DefaultDispatchers_CanBeCleared))]
        public void DefaultDispatchers_CanBeCleared()
        {
            ContainerFixture.Test(fixture => { fixture
                .Arrange
                .Resolver(r => r.WithHostConsumer())
                .Container(c => {
                    c.WithConfig<MessageDispatchConfig>(config => config.ClearMessagePublishers());
                })

                .Assert.Configuration<MessageDispatchConfig>(config =>
                {
                    config.PublisherTypes.Should().HaveCount(0);
                });
            });
        }

        /// <summary>
        /// The plug-in registers a service that can be used to publish messages.
        /// </summary>
        [Fact(DisplayName = nameof(RegistersService_ForPublishingEvents))]
        public void RegistersService_ForPublishingEvents()
        {
            ContainerFixture.Test(fixture => { fixture
                .Arrange.Resolver(r => r.WithHostConsumer())                
                .Assert.Services(s =>
                {
                    var service = s.GetService<IMessagingService>();
                    service.Should().NotBeNull();
                });
            });
        }
    }
}
