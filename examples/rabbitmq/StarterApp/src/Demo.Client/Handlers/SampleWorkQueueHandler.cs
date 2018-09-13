using System;
using NetFusion.Messaging;
using NetFusion.RabbitMQ.Subscriber;
using Demo.Domain.Commands;
using NetFusion.Common.Extensions;

namespace Demo.Client.Handlers
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
