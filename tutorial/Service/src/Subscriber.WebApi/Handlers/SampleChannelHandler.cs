using System;
using NetFusion.Common.Extensions;
using NetFusion.Messaging;
using NetFusion.Redis.Subscriber;
using Subscriber.WebApi.Events;

namespace Subscriber.WebApi.Handlers
{
    public class SampleChannelHandler : IMessageConsumer
    {
        [ChannelSubscription("testdb", "Orders.*.CT")]
        public void OnSubmission(OrderSubmitted orderEvt)
        {
            Console.WriteLine(orderEvt.ToIndentedJson());
        }
    }
}