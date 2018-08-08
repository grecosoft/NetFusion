//using System;
//using System.Collections.Generic;
//using Microsoft.Extensions.Configuration;
//using NetFusion.Messaging;
//using NetFusion.RabbitMQ.Settings;
//using NetFusion.Test.Plugins;
//
//namespace IntegrationTests.RabbitMQ
//{
//    public static class TestSetup
//    {
//        public static TestTypeResolver WithRabbitMqHost(this TestTypeResolver resolver, 
//            params Type[] hostTypes)
//        {
//            resolver.AddPlugin<MockAppHostPlugin>()
//                .AddPluginType(hostTypes);
//
//            resolver.AddPlugin<MockCorePlugin>()
//                .UseMessagingPlugin()
//                .AddPluginType<MockBusModule>()
//                .AddPluginType<MockPublisherModule>()
//                .AddPluginType<MockSubscriberModule>()
//                .AddPluginType<BusSettings>();
//
//            return resolver;
//        }
//
//        public static void AddValidBusConfig(IConfigurationBuilder configBuilder)
//        {
//            var values = GetValidBusConfig(0);
//            configBuilder.AddInMemoryCollection(values);
//        }
//        
//        public static void AddValidMultipleBusConfig(IConfigurationBuilder configBuilder)
//        {
//            var firstBusConfig = GetValidBusConfig(0);
//            var secondBusConfig = GetValidBusConfig(1, "TestBus2");
//            
//            foreach (var secondBusValue in secondBusConfig)
//            {
//                firstBusConfig.Add(secondBusValue);
//            }
//            
//            configBuilder.AddInMemoryCollection(firstBusConfig);
//        }
//
//        public static void AddDuplicateBusConfig(IConfigurationBuilder configBuilder)
//        {
//            var values = GetValidBusConfig(0);
//            var dupValues = GetValidBusConfig(1);
//
//            foreach (var dupValue in dupValues)
//            {
//                values.Add(dupValue);
//            }
//            
//            configBuilder.AddInMemoryCollection(values);
//        }
//
//        public static void AddValidBusConfigWithExchangeSettings(IConfigurationBuilder configBuilder)
//        {
//            var values = GetValidBusConfig(0);
//
//            AddExchangeSettingsToFirstConfig(values);
//            configBuilder.AddInMemoryCollection(values);
//        }
//
//        public static void AddValidBusConfigWithQueueSettings(IConfigurationBuilder configBuilder)
//        {
//            var values = GetValidBusConfig(0);
//
//            AddQueueSettingsToFirstConfig(values);
//            configBuilder.AddInMemoryCollection(values);
//        }
//
//
//        private static IDictionary<string, string> GetValidBusConfig(int idx, string busName = "TestBus1")
//        {
//            var values = new Dictionary<string, string>();
//
//            values[$"NetFusion:RabbitMQ:Connections:{idx}:BusName"] = busName;
//            values[$"NetFusion:RabbitMQ:Connections:{idx}:UserName"] = "TestUser";
//            values[$"NetFusion:RabbitMQ:Connections:{idx}:Password"] = "TestPassword";
//            values[$"NetFusion:RabbitMQ:Connections:{idx}:Heartbeat"] = "20";
//            values[$"NetFusion:RabbitMQ:Connections:{idx}:VHostName"] = "TestVHost";
//            values[$"NetFusion:RabbitMQ:Connections:{idx}:Hosts:0:hostName"] = "TestHost";
//            values[$"NetFusion:RabbitMQ:Connections:{idx}:Hosts:0:port"] = "2222";
//
//            return values;
//        }
//
//        public static void AddExchangeSettingsToFirstConfig(IDictionary<string, string> values)
//        {
//            values[$"NetFusion:RabbitMQ:Connections:0:ExchangeSettings:0:ExchangeName"] = "TestExchangeName";
//            values[$"NetFusion:RabbitMQ:Connections:0:ExchangeSettings:0:Passive"] = "true";
//        }
//
//        public static void AddQueueSettingsToFirstConfig(IDictionary<string, string> values)
//        {
//            values[$"NetFusion:RabbitMQ:Connections:0:QueueSettings:0:QueueName"] = "TestQueueName";
//            values[$"NetFusion:RabbitMQ:Connections:0:QueueSettings:0:Passive"] = "true";
//        }
//
//    }
//}