using System;
using NetFusion.Common.Extensions;
using NetFusion.Messaging;
using NetFusion.RabbitMQ.Subscriber;

namespace Demo.Client.Handlers
{
    public class TestTopicHandler : IMessageConsumer
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
      
        
//        [TopicQueue("GermanAutos", "AutoSales",
//             "AUDI.*", "BMW.*", BusName = "testBus")]
//        public void GermanAutoSold(TestDomainEvent evt)
//        {
//            Console.WriteLine(evt.ToIndentedJson());
//        }
//
//        [WorkQueue("GeneratePdf", BusName="testBus")]
//        public void GeneratePdf(TestCommand command)
//        {
//            Console.WriteLine(command.ToIndentedJson());
//        }
//
//        [FanoutQueue("PartOrderNotification", "PartRequested", BusName="testBus")]
//        public void Notification(NotificationDomainEvent evt)
//        {
//            Console.WriteLine(evt.ToIndentedJson());
//        }
//
//        [RpcQueue("Calculations", "RpcRequests", "CalculatePropertyTax", BusName="testBus")]
//        public TaxCalculations OnRpcRequest(GetPropertyTaxCommand command)
//        {
//            return new TaxCalculations { Value = command.InputValue + 350 };
//        }
    }
}