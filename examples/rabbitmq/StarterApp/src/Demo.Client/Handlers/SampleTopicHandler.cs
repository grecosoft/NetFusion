using System;
using Demo.Client.DomainEvents;
using NetFusion.Common.Extensions;
using NetFusion.Messaging;
using NetFusion.RabbitMQ.Subscriber;

namespace Demo.Client.Handlers
{
    public class SampleTopicHandler : IMessageConsumer
    {
        [TopicQueue("GermanAutosSales", "CompletedAutoSales",
            "VW.*.2017", "BMW.*.2018", BusName = "testBus")]
        public void GermanAutoSales(AutoSaleCompleted completedSale)
        {
            Console.WriteLine(completedSale.ToIndentedJson());
        }

        [TopicQueue("AmericanAutosSales", "CompletedAutoSales",
            "Chevy.*.2017", "Buick.*.2019", BusName = "testBus")]
        public void AmericanAutoSales(AutoSaleCompleted completedSale)
        {
            Console.WriteLine(completedSale.ToIndentedJson());
        }
    }
}