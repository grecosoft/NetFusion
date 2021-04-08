using System;

namespace NetFusion.Kubernetes.Hosting
{
    public class KubeConfigOptions
    {
        public string LocalConfigPath { get; init; }
        public string ContainerConfigPath { get; init; }
        public bool ReloadOnChange { get; init; }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(LocalConfigPath))
            {
                throw new InvalidOperationException("Local Configuration Path not Specified");
            }

            if (string.IsNullOrWhiteSpace(ContainerConfigPath))
            {
                throw new InvalidOperationException("Container Configuration Path not specified");
            }
        }
    }
}