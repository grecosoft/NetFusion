using System;
using NetFusion.Common.Extensions;
using NetFusion.Messaging;
using NetFusion.RabbitMQ.Subscriber;
using Subscriber.WebApi.Commands;

namespace Subscriber.WebApi.Handlers
{
    public class SampleWorkQueueHandler : IMessageConsumer
    {
        [WorkQueue("testBus", "GeneratedAndSendEmail")]
        public void GenerateEmail(SendEmail email)
        {
            Console.WriteLine(email.ToIndentedJson());
        }
    }
}