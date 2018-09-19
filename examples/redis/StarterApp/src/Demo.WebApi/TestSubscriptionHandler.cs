namespace Demo.WebApi
{
    using System;
    using Domain.Events;
    using NetFusion.Common.Extensions;
    using NetFusion.Messaging;
    using NetFusion.Redis.Subscriber;

    public class TestSubscriptionHandler : IMessageConsumer
    {
        [ChannelSubscription("testdb", "auto-purchases.VW.*.2017")]
        public void OnAutoPurchased(AutoPurchasedEvent purchasedEvent)
        {
            Console.WriteLine(purchasedEvent.ToIndentedJson());
        }
        
    }
}