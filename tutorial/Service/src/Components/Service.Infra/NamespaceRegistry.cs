using System;
using NetFusion.Azure.ServiceBus.Namespaces;
using NetFusion.Base;
using Service.Domain.Commands;
using Service.Domain.Events;

namespace Service.Infra
{
    public class NamespaceRegistry : NamespaceRegistryBase
    {
        public NamespaceRegistry() : base("ms-integration")
        {
            
        }
        
        protected override void OnDefineNamespace()
        {
            CreateTopic<HealthAlertDetected>("health-alerts", topic =>
            {
                topic.UseContentType(ContentTypes.Json);
                topic.DefaultMessageTimeToLive = TimeSpan.FromMinutes(1);
            
                topic.SetBusMessageProps((ap, m) =>
                {
                    ap.ApplicationProperties["AlertName"] = m.AlertName;
                });
            });

            RouteToQueue<SendEmail>("send-email", queue =>
            {
                queue.UseContentType(ContentTypes.MessagePack);
            });
            
            RouteToQueue<GenerateData>("generate-data", queue =>
            {
                queue.UseContentType(ContentTypes.Json);
                queue.ReplyToQueueName = "response-data";
                queue.SetBusMessageProps((bm, cmd) => {
                   // bm.ScheduledEnqueueTime = new DateTimeOffset(DateTime.UtcNow.AddSeconds(30));
                });
            });
            
            CreateResponseQueue<GeneratedDataResponse>("response-data");
            
            RouteToRpcQueue<CalculateTradeInValue>("AutoCalculations");
        }
    }
}