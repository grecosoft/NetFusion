using System.Linq;
using CoreTests.Bootstrap.Mocks;
using FluentAssertions;
using NetFusion.Bootstrap.Health;
using NetFusion.Test.Plugins;
using Xunit;

namespace CoreTests.Health
{
    
    public class HealthCheckTests
    {
        /// <summary>
        /// Modules record the health status for any aspects associated with services it manages.
        /// A Plugin's overall health status is based on all the statuses recorded by any of its modules.
        /// The overall health of the composite-application is based on the overall health of each plugin.
        /// </summary>
        [Fact]
        public void CompositeAppHealth_BasedOn_ModuleChecks()
        {
            var compositeAppHealth = CreateEmptyCompositeAppHealthCheck();
            
            compositeAppHealth.PluginHeathChecks.First()
                .ModuleHealthChecks.First().RecordedUnhealthyAspect("conn-failure", "Issue connecting");

            compositeAppHealth.OverallHealth.Should()
                .Be(HealthCheckResultType.Unhealthy);
            
            compositeAppHealth.PluginHeathChecks.First().OverallHealth.Should()
                .Be(HealthCheckResultType.Unhealthy);
            
            compositeAppHealth.PluginHeathChecks.First()
                .ModuleHealthChecks.First().OverallHealth.Should()
                .Be(HealthCheckResultType.Unhealthy);
        }

        /// <summary>
        /// If at any level there are multiple statuses of different types, the overall status will
        /// be that of the most severe.  
        /// </summary>
        [Fact]
        public void MostSevereStatus_Selected_WhenMultipleStatus()
        {
            var compositeAppHealth = CreateEmptyCompositeAppHealthCheck();
            
            var firstModule = compositeAppHealth.PluginHeathChecks.ElementAt(0).ModuleHealthChecks.ElementAt(0);
            firstModule.RecordHealthyAspect("aspect-1", "value-1");
            firstModule.RecordedDegradedAspect("aspect-2", "value-2");
            
            var secondModule = compositeAppHealth.PluginHeathChecks.ElementAt(1).ModuleHealthChecks.ElementAt(0);
            secondModule.RecordHealthyAspect("aspect-3", "value-3");
            secondModule.RecordedDegradedAspect("aspect-4", "value-4");
                
            var thirdModule = compositeAppHealth.PluginHeathChecks.ElementAt(1).ModuleHealthChecks.ElementAt(1);
            thirdModule.RecordHealthyAspect("aspect-3", "value-3");
            thirdModule.RecordHealthyAspect("aspect-4", "value-4");

            compositeAppHealth.PluginHeathChecks.ElementAt(0).OverallHealth.Should()
                .Be(HealthCheckResultType.Degraded);
            
            compositeAppHealth.PluginHeathChecks.ElementAt(1).OverallHealth.Should()
                .Be(HealthCheckResultType.Degraded);

            compositeAppHealth.OverallHealth.Should()
                .Be(HealthCheckResultType.Degraded);
            
        }

        // Creates an entity composite-application health check to which checks can
        // be added for assertion by specific unit tests.
        private static CompositeAppHealthCheck CreateEmptyCompositeAppHealthCheck()
        {
            var hostPluginChecks = new PluginHeathCheck(new MockHostPlugin());
            hostPluginChecks.AddModuleHealthCheck(new ModuleHealthCheck(new MockPluginOneModule()));
            hostPluginChecks.AddModuleHealthCheck(new ModuleHealthCheck(new MockPluginTwoModule()));

            var appPluginChecks = new PluginHeathCheck(new MockAppPlugin());
            appPluginChecks.AddModuleHealthCheck(new ModuleHealthCheck(new MockPluginOneModule()));
            appPluginChecks.AddModuleHealthCheck(new ModuleHealthCheck(new MockPluginTwoModule()));

            var compositeAppChecks = new CompositeAppHealthCheck();
            compositeAppChecks.AddPluginHealthCheck(hostPluginChecks);
            compositeAppChecks.AddPluginHealthCheck(appPluginChecks);

            return compositeAppChecks;
        }
    }
}