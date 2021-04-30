using System;
using Serilog.Core;
using Serilog.Events;

namespace NetFusion.Serilog.Enrichers
{
    /// <summary>
    /// Enriches log events with the name and identity of the host microservice.
    /// The value of the HOSTNAME environment variable is also added to the log
    /// event if defined.  
    /// </summary>
    public class HostIdentityEnricher : ILogEventEnricher 
    {
        public const string HostPluginNameProperty = "Microservice";
        public const string HostPluginIdProperty = "MicroserviceId";
        public const string HostNameProperty = "HostName";

        private readonly string _microserviceName;
        private readonly string _microserviceId;

        public HostIdentityEnricher(string microserviceName, string microserviceId)
        {
            if (string.IsNullOrWhiteSpace(microserviceName))
                throw new ArgumentException("Microservice Name not Specified.", nameof(microserviceName));
            
            if (string.IsNullOrWhiteSpace(microserviceId))
                throw new ArgumentException("Microservice Id not Specified", nameof(microserviceId));
            
            _microserviceName = microserviceName;
            _microserviceId = microserviceId;
        }
        
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            logEvent.AddPropertyIfAbsent(new LogEventProperty(HostPluginNameProperty, new ScalarValue(_microserviceName)));
            logEvent.AddPropertyIfAbsent(new LogEventProperty(HostPluginIdProperty, new ScalarValue(_microserviceId)));

            string hostName = Environment.GetEnvironmentVariable("HOSTNAME");
            if (! string.IsNullOrWhiteSpace(hostName))
            {
                logEvent.AddPropertyIfAbsent(new LogEventProperty(HostNameProperty, new ScalarValue(hostName)));
            }
        }
    }
}