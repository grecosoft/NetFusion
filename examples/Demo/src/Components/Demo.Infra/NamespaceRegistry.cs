using Demo.Domain.Commands;
using Demo.Domain.Events;
using NetFusion.Azure.ServiceBus.Namespaces;

namespace Demo.Infra
{
    public class NamespaceRegistry : NamespaceRegistryBase
    {
        public NamespaceRegistry() : base("netfusion-demo") { }

        protected override void OnDefineNamespace()
        {
            RouteToQueue<IssueMembershipCard>("card-issuance");
            
            RouteToQueue<UpdateCarFaxReport>("car-fax-report", queue =>
            {
                queue.ReplyToQueueName = "car-fax-updates";
            });
            
            CreateResponseQueue<CarFaxUpdateResult>("car-fax-updates");
            
            RouteToRpcQueue<CalculateRange>("BizCalculations");
            
            CreateTopic<NewListing>("property-listing", config =>
            {
                config.SetBusMessageProps((bm, m) =>
                {
                    bm.ApplicationProperties["price"] = m.ListedPrice;
                });
            });
            
            CreateTopic<NewSubmission>("submissions");
        }
    }
}