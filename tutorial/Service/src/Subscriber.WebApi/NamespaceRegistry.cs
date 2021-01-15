using Azure.Messaging.ServiceBus.Administration;
using NetFusion.Azure.ServiceBus.Namespaces;
using Subscriber.WebApi.Commands;

namespace Subscriber.WebApi
{
    public class NamespaceRegistry : NamespaceRegistryBase
    {
        public NamespaceRegistry() : base("ms-integration")
        {
            
        }
        
        protected override void OnDefineNamespace()
        {
            CreateQueue<SendEmail>("send-email", queue =>
            {
                //queue.DeadLetteringOnMessageExpiration = true;
                //queue.DefaultMessageTimeToLive = TimeSpan.FromMinutes(2);
            });
            
            CreateSecondaryQueue("generate-errors");
            
            CreateQueue<GenerateData>("generate-data", queue =>
            {
                queue.ForwardDeadLetteredMessagesTo = "generate-errors";
                
            });

            CreateRpcQueue("AutoCalculations");
        }
        
        protected override void OnConfigureSubscriptions()
        {
            Subscription("health-alerts", "important", sub =>
            {
                sub.AddRule("test-filter", new SqlRuleFilter("AlertName IN ('Low Memory200')") );
            });
        }
    }
}