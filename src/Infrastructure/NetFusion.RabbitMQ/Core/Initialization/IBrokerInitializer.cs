using System.Collections.Generic;

namespace NetFusion.RabbitMQ.Core.Initialization
{
    public interface IBrokerInitializer
    {
        void LogDetails(IDictionary<string, object> log);
    }
}
