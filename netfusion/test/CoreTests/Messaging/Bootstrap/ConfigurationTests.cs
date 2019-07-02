﻿using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Messaging;
using NetFusion.Messaging.Internal;
using NetFusion.Messaging.Plugin.Configs;
using NetFusion.Test.Container;
using Xunit;

namespace CoreTests.Messaging.Bootstrap
{
    /// <summary>
    /// Tests that assert that the messaging plug-in was correctly initialized
    /// by the bootstrap process.
    /// </summary>
    public class ConfigurationTests
    {
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
                .Container(c => c.WithHostConsumer())

                .Assert.Configuration((MessageDispatchConfig config) =>
                {
                    config.PublisherTypes.Should().HaveCount(1);
                    config.PublisherTypes.Should().Contain(typeof(InProcessMessagePublisher));
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
                .Arrange.Container(c => c.WithHostConsumer())                
                .Assert.Services(s =>
                {
                    var service = s.GetService<IMessagingService>();
                    service.Should().NotBeNull();
                });
            });
        }
    }
}
