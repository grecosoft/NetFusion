using System;
using Demo.Subscriber.Commands;
using NetFusion.Azure.ServiceBus.Subscriber;
using NetFusion.Common.Extensions;
using NetFusion.Messaging;

namespace Demo.Subscriber
{
    public class AutoProcessing : IMessageConsumer
    {
        [QueueSubscription("netfusion-demo", "car-fax-report")]
        public CarFaxUpdateResult OnUpdateReport(UpdateCarFaxReport command)
        {
            Console.WriteLine(command.ToIndentedJson());

            var reportId = Guid.NewGuid().ToString();
            
            return new CarFaxUpdateResult
            {
                ReportId = reportId,
                ReportUrl = $"http://www.carfax.com/reports/{reportId}",
                ResultStatus = "Completed"
            };
        }
    }
}