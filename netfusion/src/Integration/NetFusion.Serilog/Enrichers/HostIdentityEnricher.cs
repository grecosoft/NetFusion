using System;
using Serilog.Core;
using Serilog.Events;

namespace NetFusion.Serilog.Enrichers
{
    /// <summary>
    /// Enriches log events with the name and identity 
    /// of the host microservice.
    /// </summary>
    public class HostIdentityEnricher : ILogEventEnricher 
    {
        public const string HostPluginNameProperty = "Microservice";
        public const string HostPluginIdProperty = "MicroserviceId";

        private readonly string _hostName;
        private readonly string _hostId;

        public HostIdentityEnricher(string microserviceName, string microserviceId)
        {
            if (string.IsNullOrWhiteSpace(microserviceName))
                throw new ArgumentException("Microservice Name not Specified.", nameof(microserviceName));
            
            if (string.IsNullOrWhiteSpace(microserviceId))
                throw new ArgumentException("Microservice Id not Specified", nameof(microserviceId));
            
            _hostName = microserviceName;
            _hostId = microserviceId;
        }
        
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var name = new LogEventProperty(HostPluginNameProperty, new ScalarValue(_hostName));
            var id = new LogEventProperty(HostPluginIdProperty, new ScalarValue(_hostId));
        
            logEvent.AddPropertyIfAbsent(name);
            logEvent.AddPropertyIfAbsent(id);
        }
    }
}