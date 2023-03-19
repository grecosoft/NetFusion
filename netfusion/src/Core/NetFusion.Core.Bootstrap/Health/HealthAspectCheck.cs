using System;

namespace NetFusion.Core.Bootstrap.Health;

/// <summary>
/// Describes the health of a specific aspect associated with a plug-in module.
/// </summary>
public class HealthAspectCheck
{
    public string AspectName { get; }
    public string AspectValue { get; }
    public HealthCheckStatusType HealthCheckStatus { get; }

    private HealthAspectCheck(string aspectName, string aspectValue, HealthCheckStatusType healthCheckStatus)
    {
        AspectName = aspectName;
        AspectValue = aspectValue;
        HealthCheckStatus = healthCheckStatus;
    }

    /// <summary>
    /// Records the health for a specific aspect of a module.
    /// </summary>
    /// <param name="name">The name identifying the aspect.</param>
    /// <param name="value">The value on which the heath check is based.</param>
    /// <param name="status">The current health of the module's aspect.</param>
    /// <returns>Status a associated with an aspect of a module.</returns>
    public static HealthAspectCheck For(string name, string value, HealthCheckStatusType status)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Health-Check Aspect Name must be specified", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Health-Check Aspect Value must be specified", nameof(value));
        }

        return new HealthAspectCheck(name, value, status);
    }

    /// <summary>
    /// Records a healthy aspect of a module.
    /// </summary>
    /// <param name="name">The name identifying the aspect.</param>
    /// <param name="value">The value on which the heath check is based.</param>
    /// <returns>Status a associated with an aspect of a module.</returns>
    public static HealthAspectCheck ForHealthy(string name, string value) =>
        For(name, value, HealthCheckStatusType.Healthy);

    /// <summary>
    /// Records a degraded aspect of a module.
    /// </summary>
    /// <param name="name">The name identifying the aspect.</param>
    /// <param name="value">The value on which the heath check is based.</param>
    /// <returns>Status a associated with an aspect of a module.</returns>
    public static HealthAspectCheck ForDegraded(string name, string value) =>
        For(name, value, HealthCheckStatusType.Degraded);

    /// <summary>
    /// Records an unhealthy aspect of a module.
    /// </summary>
    /// <param name="name">The name identifying the aspect.</param>
    /// <param name="value">The value on which the heath check is based.</param>
    /// <returns>Status a associated with an aspect of a module.</returns>
    public static HealthAspectCheck ForUnhealthy(string name, string value) =>
        For(name, value, HealthCheckStatusType.Unhealthy);

}