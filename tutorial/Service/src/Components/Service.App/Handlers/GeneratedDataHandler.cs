using System;
using NetFusion.Azure.ServiceBus.Subscriber;
using NetFusion.Common.Extensions;
using NetFusion.Messaging;
using Service.Domain.Commands;

namespace Service.App.Handlers
{
    public class GeneratedDataHandler : IMessageConsumer
    {
        [QueueSubscription("ms-integration", "response-data")]
        public void OnDataReceived(GeneratedDataResponse responseMsg)
        {
            Console.WriteLine(responseMsg.ToIndentedJson());
        }
    }
}