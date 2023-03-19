using NetFusion.Core.Bootstrap.Health;
using NetFusion.Core.Bootstrap.Plugins;

namespace NetFusion.Core.UnitTests.Health.Mocks;

public class HealthCheckModuleOne : PluginModule,
    IModuleHealthCheckProvider
{
    public HealthCheckStatusType HealthCheckStatus { get; set; } = HealthCheckStatusType.Healthy;
        
    public Task CheckModuleAspectsAsync(ModuleHealthCheck healthCheck)
    {
        healthCheck.RecordAspect(HealthAspectCheck.For("ModuleOneAspect", "ModuleOneValue", HealthCheckStatus));
        return Task.CompletedTask;
    }
}