using System;
using Demo.Subscriber.Commands;
using NetFusion.Common.Extensions;
using NetFusion.Messaging;
using NetFusion.RabbitMQ.Subscriber;

namespace Demo.Subscriber
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
