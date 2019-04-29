using System;
using NetFusion.Bootstrap.Plugins;

namespace NetFusion.Rest.Server.Plugin.Configs
{
    /// <summary>
    /// Plug-in container configuration.
    /// </summary>
    public class RestApiConfig : IPluginConfig
    {
        /// <summary>
        /// The suffix value used for controller classes.
        /// </summary>
        public string ControllerSuffix { get; private set; } = "Controller";

        /// <summary>
        /// Overrides the suffix value used for controller classes.  Default value is "Controller".
        /// </summary>
        /// <param name="controllerSuffix">Controller suffix.</param>
        public void SetControllerSuffix(string controllerSuffix)
        {
            if (string.IsNullOrWhiteSpace(controllerSuffix))
                throw new ArgumentException("Controller suffix not specified.", nameof(controllerSuffix));

            ControllerSuffix = controllerSuffix;
        }
    }
}
