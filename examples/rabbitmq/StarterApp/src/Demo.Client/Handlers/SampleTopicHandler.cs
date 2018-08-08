using System;
using Demo.Client.DomainEvents;
using NetFusion.Common.Extensions;
using NetFusion.Messaging;
using NetFusion.RabbitMQ.Subscriber;

namespace Demo.Client.Handlers
{
    public class SampleTopicHandler : IMessageConsumer
    {
        [TopicQueue("testBus", "GermanAutosSales", "CompletedAutoSales",
            "VW.*.2017", "BMW.*.2018")]
        public void GermanAutoSales(AutoSaleCompleted completedSale)
        {
            Console.WriteLine(completedSale.ToIndentedJson());
        }

        [TopicQueue("testBus", "AmericanAutosSales", "CompletedAutoSales",
            "Chevy.*.2017", "Buick.*.2019")]
        public void AmericanAutoSales(AutoSaleCompleted completedSale)
        {
            Console.WriteLine(completedSale.ToIndentedJson());
        }
    }
}