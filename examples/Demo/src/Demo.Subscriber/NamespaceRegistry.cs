using Azure.Messaging.ServiceBus.Administration;
using Demo.Subscriber.Commands;
using NetFusion.Azure.ServiceBus.Namespaces;

namespace Demo.Subscriber
{
    public class NamespaceRegistry : NamespaceRegistryBase
    {
        public NamespaceRegistry() : base("netfusion-demo")
        {
            
        }

        protected override void OnDefineNamespace()
        {
            CreateQueue<IssueMembershipCard>("card-issuance");
            CreateQueue<UpdateCarFaxReport>("car-fax-report");
            CreateRpcQueue("BizCalculations");
        }

        protected override void OnConfigureSubscriptions()
        {
            Subscription("property-listing", "expensive-properties", config =>
            {
                config.AddRule("expensive", new SqlRuleFilter("price > 250000") );
            });
        }
    }
}