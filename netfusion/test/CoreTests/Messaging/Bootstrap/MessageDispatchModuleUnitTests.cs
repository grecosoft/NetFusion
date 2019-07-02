using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.Messaging;
using NetFusion.Messaging.Enrichers;
using NetFusion.Messaging.Internal;
using NetFusion.Messaging.Plugin;
using NetFusion.Messaging.Plugin.Configs;
using NetFusion.Messaging.Plugin.Modules;
using NetFusion.Messaging.Types;
using NetFusion.Test.Container;
using NetFusion.Test.Plugins;
using Xunit;

namespace CoreTests.Messaging.Bootstrap
{
    public class MessageDispatchModuleUnitTests
    {
        [Fact]
        public void AllMessageConsumers_Discovered()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Container(c =>
                    {
                        var hostPlugin = new MockHostPlugin();
                        hostPlugin.AddPluginType<MessageConsumerOne>();
                        hostPlugin.AddPluginType<MessageConsumerTwo>();
                       
                        c.RegisterPlugins(hostPlugin);
                        c.RegisterPlugin<MessagingPlugin>();
                    })
                    .Assert.PluginModule((MessageDispatchModule m) =>
                    {
                        Assert.NotNull(m.AllMessageTypeDispatchers);
                        Assert.True(m.AllMessageTypeDispatchers.Contains(typeof(MockCommand)));
                        Assert.True(m.AllMessageTypeDispatchers.Contains(typeof(MockDomainEvent)));
            
                        Assert.NotNull(m.InProcessDispatchers);
                        Assert.True(m.InProcessDispatchers.Contains(typeof(MockCommand)));
                        Assert.True(m.InProcessDispatchers.Contains(typeof(MockDomainEvent)));
                    });
            });
        }

        [Fact]
        public void InProcessMessageDispatcher_AddedByDefault()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Container(c =>
                    {
                        var hostPlugin = new MockHostPlugin();

                        c.RegisterPlugins(hostPlugin);
                        c.RegisterPlugin<MessagingPlugin>();
                    })
                    .Assert.Plugin((MessagingPlugin p) =>
                    {
                        var config = p.GetConfig<MessageDispatchConfig>();
                        var publisherType = config.PublisherTypes.FirstOrDefault(
                            pt => pt == typeof(InProcessMessagePublisher));
                        
                        publisherType.Should().NotBeNull();
                    });
            });
        }
        
        [Fact]
        public void DefaultEnrichersAdded()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Container(c =>
                    {
                        var hostPlugin = new MockHostPlugin();

                        c.RegisterPlugins(hostPlugin);
                        c.RegisterPlugin<MessagingPlugin>();
                    })
                    .Assert.Plugin((MessagingPlugin p) =>
                    {
                        var config = p.GetConfig<MessageDispatchConfig>();

                        config.EnricherTypes.Should().Contain(typeof(CorrelationEnricher));
                        config.EnricherTypes.Should().Contain(typeof(DateReceivedEnricher));
                    });
            });
        }

        [Fact]
        public void AllMessagePublishes_AddedAsScoped_ToServiceCollection()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Container(c =>
                    {
                        var hostPlugin = new MockHostPlugin();

                        c.RegisterPlugins(hostPlugin);
                        c.RegisterPlugin<MessagingPlugin>();
                    })
                    .Assert.ServiceCollection(s =>
                    {
                        var registeredServices = s.Where(sd => 
                                sd.Lifetime == ServiceLifetime.Scoped &&
                                sd.ServiceType == typeof(IMessagePublisher))
                            .ToArray();

                        registeredServices.Should().HaveCount(1);
                    });
            });         
        }

        [Fact]
        public void AllMessageConsumers_AddedAsScoped_ToServiceCollection()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Container(c =>
                    {
                        var hostPlugin = new MockHostPlugin();
                        hostPlugin.AddPluginType<MessageConsumerOne>();
                        hostPlugin.AddPluginType<MessageConsumerTwo>();

                        c.RegisterPlugins(hostPlugin);
                        c.RegisterPlugin<MessagingPlugin>();
                    })
                    .Assert.ServiceCollection(s =>
                    {
                        var consumers = s.Where(sd => 
                            sd.ServiceType.IsConcreteTypeDerivedFrom<IMessageConsumer>() && 
                            sd.Lifetime == ServiceLifetime.Scoped);
                        consumers.Should().HaveCount(2);
                    });
            });
        }

        public class MessageConsumerOne : IMessageConsumer
        {
            [InProcessHandler]
            public int OnCommand(MockCommand command)
            {
                Assert.NotNull(command);
                return 1000;
            }
        }
        
        public class MessageConsumerTwo : IMessageConsumer
        {
            [InProcessHandler]
            public void OnDomainEvent(MockDomainEvent domainEvt)
            {
                Assert.NotNull(domainEvt);
            }
        }


        public class MockCommand : Command<int>
        {
            
        }

        public class MockDomainEvent : DomainEvent
        {
            
        }
    }
}