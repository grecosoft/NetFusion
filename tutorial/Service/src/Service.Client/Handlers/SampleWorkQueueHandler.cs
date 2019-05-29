using System;
using NetFusion.Common.Extensions;
using NetFusion.Messaging;
using NetFusion.RabbitMQ.Subscriber;
using Service.Client.Commands;

namespace Service.Client.Handlers
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