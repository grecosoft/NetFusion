using System;
using NetFusion.Serilog.Enrichers;
using Serilog;
using Serilog.Configuration;

namespace NetFusion.Serilog
{
    public static class EnricherExtensions
    {
        /// <summary>
        /// Enrich log events with a the Plugin Name and Id.  These two values are set as the
        /// Microservice and MicroserviceId log properties.
        /// </summary>
        /// <param name="configuration">The Serilog log enricher collection.</param>
        /// <param name="microserviceId">The unique value used to identify the microservice.</param>
        /// <param name="microserviceName">The name of the microservice.</param>
        /// <returns>Configuration object allowing method chaining.</returns>
        public static LoggerConfiguration WithHostIdentity(this LoggerEnrichmentConfiguration configuration,
            string microserviceId,
            string microserviceName)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            return configuration.With(new HostIdentityEnricher(microserviceName, microserviceId));
        }
    }
}