using System;
using System.Threading.Tasks;
using NetFusion.Messaging.Types;
using NetFusion.Messaging.Enrichers;

namespace Demo.Infra
{
    public class MachineNameEnricher : MessageEnricher
    {
        public override Task Enrich(IMessage message)
        {
            AddMessageProperty(message, "MachineName", Environment.MachineName);
            return base.Enrich(message);
        }
    }
}
