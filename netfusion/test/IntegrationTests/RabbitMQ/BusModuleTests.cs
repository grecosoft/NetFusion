using System;
using System.Linq;
using EasyNetQ;
using NetFusion.Bootstrap.Exceptions;
using NetFusion.RabbitMQ.Plugin.Modules;
using NetFusion.Test.Container;
using Xunit;

namespace IntegrationTests.RabbitMQ
{
    using EasyNetQ.Topology;
    using NetFusion.RabbitMQ.Metadata;

    /// <summary>
    /// The bus module is responsible for storing the connection to the RabbitMq server
    /// for a specified name instance as specified within the applicaiton host's configuration.
    /// </summary>
    public class BusModuleTests
    {
        /// <summary>
        /// Multiple message busses are supported by specifying their settings within
        /// the application host's application settings.  Each bus configuration is 
        /// identified by a string name and used to lookup the bus when needed.
        /// </summary>
        [Fact]
        public void ConfiguredBus_CanBe_Referenced()
        {
            ContainerFixture.Test(fixture => {
                fixture.Arrange
                    .Configuration(TestSetup.AddValidBusConfig)
                    .Container(c =>
                    {
                        c.WithRabbitMqHost();
                    })
                    .Act.OnApplication(ca =>
                    {
                        ca.Start();
                    })
                    .Assert.PluginModule((BusModule m) =>
                    {
                        var bus = m.GetBus("TestBus1");
                        Assert.NotNull(bus);
                    });
            });
        }

        /// <summary>
        /// An exception is raised if a bus is requested that has not been configured.
        /// </summary>
        [Fact]
        public void ExceptionIfRequestedBus_IsNotConfigured()
        {
            ContainerFixture.Test(fixture => {
                fixture.Arrange
                    .Configuration(TestSetup.AddValidBusConfig)
                    .Container(c =>
                    {
                        c.WithRabbitMqHost();
                    })
                    .Assert.PluginModule<BusModule>(m =>
                    {
                        var ex = Assert.Throws<InvalidOperationException>(() => m.GetBus("TestBus99"));
                        Assert.Equal(
                            "The bus named: TestBus99 has not been configured.  Check application configuration.", 
                            ex.Message);
                    });
            });
        }

        /// <summary>
        /// The settings specified within the application's settings is used to populate the EasyNetQ
        /// ConnectionConfiguration object from which the IBus instance is created.
        /// </summary>
        [Fact]
        public void BusConfiguredCorrectly_FromSettings()
        {
            ContainerFixture.Test(fixture => {
                fixture.Arrange
                    .Configuration(TestSetup.AddValidBusConfig)
                    .Container(c =>
                    {
                        c.WithRabbitMqHost();
                    })
                    .Act.OnApplication(ca =>
                    {
                        ca.Start();
                    })
                    .Assert.PluginModule((MockBusModule m) =>
                    {
                        Assert.True(m.ConnConfigs.Count() == 1);
                        var busConfig = m.ConnConfigs.First();

                        Assert.Equal("TestUser", busConfig.UserName);        
                        Assert.Equal("TestPassword", busConfig.Password);
                        Assert.Equal("TestVHost", busConfig.VirtualHost);    
                      //  Assert.Equal(20, busConfig.RequestedHeartbeat);
                        Assert.Equal("TestBus1", busConfig.Name);
                        Assert.True(busConfig.Hosts.Count() == 1); 

                        var hostConfig = busConfig.Hosts.First();
                        Assert.Equal("TestHost", hostConfig.Host);
                        Assert.Equal(2222, hostConfig.Port);
                    });
            });
        }

        /// <summary>
        /// Exchange settings can be specified within the applications configuration.
        /// </summary>
        [Fact]
        public void CanSpecifyExchangeSettings_InConfiguration()
        {
            ContainerFixture.Test(fixture => {
                fixture.Arrange
                    .Configuration(TestSetup.AddValidBusConfigWithExchangeSettings)
                    .Container(c =>
                    {
                        c.WithRabbitMqHost();
                    })
                    .Assert.PluginModule<MockBusModule>(m =>
                    {
                        var exchangeMeta = ExchangeMeta.Define("TestBus1", "TestExchangeName", ExchangeType.Direct);
                        m.ApplyExchangeSettings(exchangeMeta);
                        
                        Assert.True(exchangeMeta.IsNonRoutedSaved);
                        Assert.Equal("TestContentType", exchangeMeta.ContentType);
                        Assert.Equal(10000, exchangeMeta.CancelRpcRequestAfterMs);
                    });
            });
        }

        /// <summary>
        /// Queue settings can be specified within the applications configuration.
        /// </summary>
        [Fact]
        public void CanSpecifyQueueSettings_InConfiguration()
        {
             ContainerFixture.Test(fixture => {
                fixture.Arrange
                    .Configuration(TestSetup.AddValidBusConfigWithQueueSettings)
                    .Container(c =>
                    {
                        c.WithRabbitMqHost();
                    })
                    .Assert.PluginModule<MockBusModule>(m =>
                    {
                        var queueMeta = ExchangeMeta.DefineDefault("TestBus1", "TestQueueName").QueueMeta;
                        m.ApplyQueueSettings(queueMeta);
                        
                        Assert.Equal(20000, queueMeta.PerQueueMessageTtl);
                        Assert.Equal(10, queueMeta.MaxPriority);
                        Assert.Equal(5, queueMeta.PrefetchCount);
                        Assert.Equal(2, queueMeta.Priority);
                    });
            });
        }

        /// <summary>
        /// Additional custom properties are set used to identify the connection client.
        /// These value are listed within the RabbitMQ Web Admin Client.  These values
        /// are obtained from a common set of plugin properties used to identify the 
        /// RabbitMQ plugin and the host application utilizing RabbitMQ. 
        /// </summary>
        [Fact]
        public void AdditionalPropertiesSet_ToIdentifyClient()
        {
            ContainerFixture.Test(fixture => {
                fixture.Arrange
                    .Configuration(TestSetup.AddValidBusConfig)
                    .Container(c =>
                    {
                        c.WithRabbitMqHost();
                    })
                    .Act.OnApplication(ca =>
                    {
                        ca.Start();
                    })
                    .Assert.PluginModule<MockBusModule>(m =>
                    {
                        Assert.True(m.ConnConfigs.Count() == 1);
                        var busConfig = m.ConnConfigs.First();

                        var mockRabbitPlugin = m.Context.Plugin;
                        var mockHostPlugin = m.Context.AppHost;

                        AssertClientProperty(busConfig, "Client Assembly", mockRabbitPlugin.AssemblyName);
                        AssertClientProperty(busConfig, "Client Version", mockRabbitPlugin.AssemblyVersion);
                        AssertClientProperty(busConfig, "AppHost Assembly", mockHostPlugin.AssemblyName);
                        AssertClientProperty(busConfig, "AppHost Version", mockHostPlugin.AssemblyVersion);
                        AssertClientProperty(busConfig, "AppHost Description", mockHostPlugin.Description);
                        AssertClientProperty(busConfig, "Machine Name", Environment.MachineName);
                    });
            });
        }

        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        private static void AssertClientProperty(ConnectionConfiguration busConfig, string propertyName, object expectedValue)
        {
            busConfig.ClientProperties.TryGetValue(propertyName, out object value);
            Assert.Equal(expectedValue, value);
        }
    }

}