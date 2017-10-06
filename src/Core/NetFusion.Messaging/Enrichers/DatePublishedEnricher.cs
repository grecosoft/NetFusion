using NetFusion.Messaging.Types;
using System;
using System.Threading.Tasks;

namespace NetFusion.Messaging.Enrichers
{
    /// <summary>
    /// Adds the current date and time to the mess if not already present.
    /// </summary>
    public class DatePublishedEnricher : MessageEnricher
    {
        public override Task Enrich(IMessage message)
        {
            AddMessageProperty(message, "DatePublished", DateTime.UtcNow);
            return base.Enrich(message);
        }
    }
}
