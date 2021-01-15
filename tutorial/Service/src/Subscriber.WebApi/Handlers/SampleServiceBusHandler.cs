using System;
using NetFusion.Azure.ServiceBus.Subscriber;
using NetFusion.Common.Extensions;
using NetFusion.Messaging;
using Subscriber.WebApi.Commands;
using Subscriber.WebApi.Events;

namespace Subscriber.WebApi.Handlers
{
    public class SampleServiceBusHandler : IMessageConsumer
    {
        [TopicSubscription("ms-integration", "health-alerts", "important")]
        public void OnAlert(HealthAlertDetected alert)
        {
            Console.WriteLine(alert.ToIndentedJson());
        }

        [QueueSubscription("ms-integration", "send-email")]
        public void OnSendEmail(SendEmail command)
        {
            Console.WriteLine(command.ToIndentedJson());
        }
        
        [QueueSubscription("ms-integration", "generate-data")]
        public GeneratedDataResponse OnGenerateData(GenerateData command)
        {
            Console.WriteLine(command.ToIndentedJson());
            return new GeneratedDataResponse
            {
                UniqueIdValue = Guid.NewGuid().ToString(),
                FirstName = "Tom",
                LastName = "Green"
            };
        }

        [RpcQueueSubscription("ms-integration", "AutoCalculations")]
        public TradeInResult OnCalculateTradeInValue(CalculateTradeInValue command)
        {
            return new TradeInResult
            {
                MinValue = 5_000m,
                MaxValue = 6_200m
            };
        }
    }
}