namespace NetFusion.Core.Bootstrap.Health;

/// <summary>
/// Indicates the health status for specific items used in determining
/// the overall status of the composite-application.
/// </summary>
public enum HealthCheckStatusType
{
    /// <summary>
    /// All provided services are operational.
    /// </summary>
    Healthy = 1,
        
    /// <summary>
    /// Contains services that are not operational but can till process submitted requests.
    /// </summary>
    Degraded = 2,
        
    /// <summary>
    /// Services required to provide the application's core functionality are not operational. 
    /// </summary>
    Unhealthy = 3
}