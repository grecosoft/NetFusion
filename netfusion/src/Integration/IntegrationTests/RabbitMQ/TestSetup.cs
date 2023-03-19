using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Messaging.Plugin;
using NetFusion.RabbitMQ.Plugin.Configs;
using NetFusion.RabbitMQ.Settings;
using NetFusion.Test.Plugins;

namespace IntegrationTests.RabbitMQ
{
    public static class TestSetup
    {
        public static CompositeContainer WithRabbitMqHost(this CompositeContainer container, 
            params Type[] hostTypes)
        {
            var hostPlugin = new MockHostPlugin();
            hostPlugin.AddModule<HostModule>();
            hostPlugin.AddPluginType(hostTypes);

            var corePlugin = new MockCorePlugin();
            corePlugin.AddConfig<RabbitMqConfig>();
            corePlugin.AddPluginType<BusSettings>();
            corePlugin.AddModule<MockBusModule>();
            corePlugin.AddModule<MockPublisherModule>();
            corePlugin.AddModule<MockSubscriberModule>();
            
            container.RegisterPlugin<MessagingPlugin>();
            container.RegisterPlugins(corePlugin);
            container.RegisterPlugins(hostPlugin);
          
            return container;
        }
        
        public static void AddValidBusConfig(IConfigurationBuilder configBuilder)
        {
            var values = GetValidBusConfig();
            configBuilder.AddInMemoryCollection(values);
        }
        
        public static void AddValidMultipleBusConfig(IConfigurationBuilder configBuilder)
        {
            var firstBusConfig = GetValidBusConfig();
            var secondBusConfig = GetValidBusConfig("TestBus2");
            
            foreach (var secondBusValue in secondBusConfig)
            {
                firstBusConfig.Add(secondBusValue);
            }
            
            configBuilder.AddInMemoryCollection(firstBusConfig);
        }

        public static void AddValidBusConfigWithExchangeSettings(IConfigurationBuilder configBuilder)
        {
            var values = GetValidBusConfig();

            AddExchangeSettingsToFirstConfig(values);
            configBuilder.AddInMemoryCollection(values);
        }

        public static void AddValidBusConfigWithQueueSettings(IConfigurationBuilder configBuilder)
        {
            var values = GetValidBusConfig();

            AddQueueSettingsToFirstConfig(values);
            configBuilder.AddInMemoryCollection(values);
        }


        private static IDictionary<string, string> GetValidBusConfig(string busName = "TestBus1")
        {
            var values = new Dictionary<string, string>
            {
                [$"NetFusion:RabbitMQ:Connections:{busName}:UserName"] = "TestUser",
                [$"NetFusion:RabbitMQ:Connections:{busName}:Password"] = "TestPassword",
                [$"NetFusion:RabbitMQ:Connections:{busName}:Heartbeat"] = "20",
                [$"NetFusion:RabbitMQ:Connections:{busName}:VHostName"] = "TestVHost",
                [$"NetFusion:RabbitMQ:Connections:{busName}:Hosts:0:hostName"] = "TestHost",
                [$"NetFusion:RabbitMQ:Connections:{busName}:Hosts:0:port"] = "2222"
            };


            return values;
        }

        public static void AddExchangeSettingsToFirstConfig(IDictionary<string, string> values)
        {
            values["NetFusion:RabbitMQ:Connections:TestBus1:ExchangeSettings:TestExchangeName:ExchangeName"] = "TestExchangeName";
            values["NetFusion:RabbitMQ:Connections:TestBus1:ExchangeSettings:TestExchangeName:Passive"] = "true";
            values["NetFusion:RabbitMQ:Connections:TestBus1:ExchangeSettings:TestExchangeName:IsNonRoutedSaved"] = "true";
            values["NetFusion:RabbitMQ:Connections:TestBus1:ExchangeSettings:TestExchangeName:ContentType"] = "TestContentType";
            values["NetFusion:RabbitMQ:Connections:TestBus1:ExchangeSettings:TestExchangeName:CancelRpcRequestAfterMs"] = "10000";
        }

        public static void AddQueueSettingsToFirstConfig(IDictionary<string, string> values)
        {
            values["NetFusion:RabbitMQ:Connections:TestBus1:QueueSettings:TestQueueName:QueueName"] = "TestQueueName";
            values["NetFusion:RabbitMQ:Connections:TestBus1:QueueSettings:TestQueueName:Passive"] = "true";
            values["NetFusion:RabbitMQ:Connections:TestBus1:QueueSettings:TestQueueName:PerQueueMessageTtl"] = "20000";
            values["NetFusion:RabbitMQ:Connections:TestBus1:QueueSettings:TestQueueName:MaxPriority"] = "10";
            values["NetFusion:RabbitMQ:Connections:TestBus1:QueueSettings:TestQueueName:PrefetchCount"] = "5";
            values["NetFusion:RabbitMQ:Connections:TestBus1:QueueSettings:TestQueueName:Priority"] = "2";
        }
    }

    public class HostModule : PluginModule
    {
        public override void RegisterServices(IServiceCollection services)
        {

        }
    }
}