using System;
using NetFusion.Common.Extensions;
using NetFusion.Messaging;
using NetFusion.Redis.Subscriber;
using Domain.Events;

namespace Demo.Client.Handlers
{
    public class OrderSubmittedHandler : IMessageConsumer
    {
        [ChannelSubscription("testdb", "Orders.*.CT")]
        public void OnSubmission(OrderSubmitted orderEvt)
        {
            Console.WriteLine(orderEvt.ToIndentedJson());
        }
    }
}