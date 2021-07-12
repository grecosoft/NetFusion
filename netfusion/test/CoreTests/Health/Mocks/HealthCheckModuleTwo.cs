using System.Threading.Tasks;
using NetFusion.Bootstrap.Health;
using NetFusion.Bootstrap.Plugins;

namespace CoreTests.Health.Mocks
{
    public class HealthCheckModuleTwo : PluginModule,
        IModuleHealthCheck
    {
        public HealthCheckStatusType HealthCheckStatus { get; set; } = HealthCheckStatusType.Healthy;
        
        public Task CheckModuleAspectsAsync(ModuleHealthCheck healthCheck)
        {
            healthCheck.RecordAspect(HealthCheckStatus, "ModuleTwoAspect", "ModuleTwoValue");
            return Task.CompletedTask;
        }
    }
}