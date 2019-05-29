using System;
using NetFusion.Common.Extensions;
using NetFusion.Messaging;
using NetFusion.RabbitMQ.Subscriber;
using Service.Client.Events;

namespace Service.Client.Handlers
{
    public class SampleTopicHandler : IMessageConsumer
    {
        [TopicQueue("testBus", "GermanAutoSales", "CompletedAutoSales",
            "VW.*.2017", "BMW.*.2018")]
        public void GermanAutoSales(AutoSaleCompleted completedSale)
        {
            Console.WriteLine(completedSale.ToIndentedJson());
        }

        [TopicQueue("testBus", "AmericanAutoSales", "CompletedAutoSales",
            "Chevy.*.2017", "Buick.*.2019")]
        public void AmericanAutoSales(AutoSaleCompleted completedSale)
        {
            Console.WriteLine(completedSale.ToIndentedJson());
        }
        
        [TopicQueue("testBus", "SweetishAutoSales", "CompletedAutoSales")]
        public void SweetishAutoSales(AutoSaleCompleted completedSale)
        {
            Console.WriteLine(completedSale.ToIndentedJson());
        }
    }
}