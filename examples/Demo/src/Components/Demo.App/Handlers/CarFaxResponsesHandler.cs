using System;
using Demo.Domain.Commands;
using NetFusion.Azure.ServiceBus.Subscriber;
using NetFusion.Common.Extensions;
using NetFusion.Messaging;

namespace Demo.App.Handlers
{
    public class CarFaxResponsesHandler : IMessageConsumer
    {
        [QueueSubscription("netfusion-demo", "car-fax-updates")]
        public void OnCarFaxUpdated(CarFaxUpdateResult result)
        {
            Console.Write(result.ToIndentedJson());
        }
    }
}