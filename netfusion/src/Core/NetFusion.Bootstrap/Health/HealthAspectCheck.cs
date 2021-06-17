using System;

namespace NetFusion.Bootstrap.Health
{
    /// <summary>
    /// Describes the health of a specific aspect associated with a plug-in module.
    /// </summary>
    public class HealthAspectCheck
    {
        public string AspectName { get; init; }
        public string AspectValue { get; init; }
        public HealthCheckResultType CheckResult { get; init; }

        internal HealthAspectCheck ThrowIfInvalid()
        {
            if (string.IsNullOrWhiteSpace(AspectName))
            {
                throw new InvalidOperationException("Health-Check Aspect Name must be specified");
            }
            
            if (string.IsNullOrWhiteSpace(AspectValue))
            {
                throw new InvalidOperationException("Health-Check Aspect Value must be specified");
            }

            return this;
        }
    }
}