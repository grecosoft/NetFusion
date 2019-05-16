using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Base.Scripting;
using NetFusion.Base.Serialization;
using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Bootstrap.Validation;
using NetFusion.Messaging.Plugin;
using NetFusion.RabbitMQ.Settings;
using NetFusion.Serialization;
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
            var values = GetValidBusConfig(0);
            configBuilder.AddInMemoryCollection(values);
        }
        
        public static void AddValidMultipleBusConfig(IConfigurationBuilder configBuilder)
        {
            var firstBusConfig = GetValidBusConfig(0);
            var secondBusConfig = GetValidBusConfig(1, "TestBus2");
            
            foreach (var secondBusValue in secondBusConfig)
            {
                firstBusConfig.Add(secondBusValue);
            }
            
            configBuilder.AddInMemoryCollection(firstBusConfig);
        }

        public static void AddDuplicateBusConfig(IConfigurationBuilder configBuilder)
        {
            var values = GetValidBusConfig(0);
            var dupValues = GetValidBusConfig(1);

            foreach (var dupValue in dupValues)
            {
                values.Add(dupValue);
            }
            
            configBuilder.AddInMemoryCollection(values);
        }

        public static void AddValidBusConfigWithExchangeSettings(IConfigurationBuilder configBuilder)
        {
            var values = GetValidBusConfig(0);

            AddExchangeSettingsToFirstConfig(values);
            configBuilder.AddInMemoryCollection(values);
        }

        public static void AddValidBusConfigWithQueueSettings(IConfigurationBuilder configBuilder)
        {
            var values = GetValidBusConfig(0);

            AddQueueSettingsToFirstConfig(values);
            configBuilder.AddInMemoryCollection(values);
        }


        private static IDictionary<string, string> GetValidBusConfig(int idx, string busName = "TestBus1")
        {
            var values = new Dictionary<string, string>();

            values[$"NetFusion:RabbitMQ:Connections:{idx}:BusName"] = busName;
            values[$"NetFusion:RabbitMQ:Connections:{idx}:UserName"] = "TestUser";
            values[$"NetFusion:RabbitMQ:Connections:{idx}:Password"] = "TestPassword";
            values[$"NetFusion:RabbitMQ:Connections:{idx}:Heartbeat"] = "20";
            values[$"NetFusion:RabbitMQ:Connections:{idx}:VHostName"] = "TestVHost";
            values[$"NetFusion:RabbitMQ:Connections:{idx}:Hosts:0:hostName"] = "TestHost";
            values[$"NetFusion:RabbitMQ:Connections:{idx}:Hosts:0:port"] = "2222";

            return values;
        }

        public static void AddExchangeSettingsToFirstConfig(IDictionary<string, string> values)
        {
            values["NetFusion:RabbitMQ:Connections:0:ExchangeSettings:0:ExchangeName"] = "TestExchangeName";
            values["NetFusion:RabbitMQ:Connections:0:ExchangeSettings:0:Passive"] = "true";
            values["NetFusion:RabbitMQ:Connections:0:ExchangeSettings:0:AlternateExchange"] = "TestAltExchangeName";
            values["NetFusion:RabbitMQ:Connections:0:ExchangeSettings:0:ContentType"] = "TestContentType";
            values["NetFusion:RabbitMQ:Connections:0:ExchangeSettings:0:CancelRpcRequestAfterMs"] = "10000";
        }

        public static void AddQueueSettingsToFirstConfig(IDictionary<string, string> values)
        {
            values["NetFusion:RabbitMQ:Connections:0:QueueSettings:0:QueueName"] = "TestQueueName";
            values["NetFusion:RabbitMQ:Connections:0:QueueSettings:0:Passive"] = "true";
            values["NetFusion:RabbitMQ:Connections:0:QueueSettings:0:PerQueueMessageTtl"] = "20000";
            values["NetFusion:RabbitMQ:Connections:0:QueueSettings:0:DeadLetterExchange"] = "TestDeadLetterExchange";
            values["NetFusion:RabbitMQ:Connections:0:QueueSettings:0:DeadLetterRoutingKey"] = "TestDeadLetterRoutingKey";
            values["NetFusion:RabbitMQ:Connections:0:QueueSettings:0:MaxPriority"] = "10";
            values["NetFusion:RabbitMQ:Connections:0:QueueSettings:0:PrefetchCount"] = "5";
            values["NetFusion:RabbitMQ:Connections:0:QueueSettings:0:Priority"] = "2";
        }
    }

    public class HostModule : PluginModule
    {
        public override void RegisterServices(IServiceCollection services)
        {

        }
    }
}