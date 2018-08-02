using System;
using NetFusion.Common.Extensions;
using NetFusion.Messaging;
using NetFusion.RabbitMQ.Subscriber;

namespace Demo.Client.Handlers
{
    public class TestTopicHandler : IMessageConsumer
    {
     
       
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