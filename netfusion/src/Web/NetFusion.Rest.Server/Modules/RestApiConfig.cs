using NetFusion.Bootstrap.Container;
using System;

namespace NetFusion.Rest.Server.Modules
{
    /// <summary>
    /// Plug-in container configuration.
    /// </summary>
    public class RestApiConfig : IContainerConfig
    {
        /// <summary>
        /// The suffix value used for controller classes.
        /// </summary>
        public string ControllerSuffix { get; private set; } = "Controller";

        /// <summary>
        /// The root path to the location of the controller XML documentation files.
        /// </summary>
        public string ControllerDocDirectoryName { get; private set; } = "ClientDocs";

        /// <summary>
        /// The root path to the location of the type-script client resource definitions.
        /// </summary>
        public string TypeScriptDirectoryName { get; private set; } = "ClientResources";

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

        /// <summary>
        /// Sets the name of the directory containing the XML controller documentation files.
        /// </summary>
        /// <param name="directoryName">The name of the directory located within the ContentRootPath.</param>
        public void SetControllerDocDirectoryName(string directoryName)
        {
            if (string.IsNullOrWhiteSpace(directoryName))
                throw new ArgumentException("Directory Name not specified.", nameof(directoryName));

            ControllerDocDirectoryName = directoryName;
        }

        /// <summary>
        /// Sets the name of the directory containing the type-script definition files.
        /// </summary>
        /// <param name="directoryName">The name of the directory located within the ContentRootPath.</param>
        public void SetTypeScriptDirectoryName(string directoryName)
        {
            if (string.IsNullOrWhiteSpace(directoryName))
                throw new ArgumentException("Directory Name not specified.", nameof(directoryName));

            TypeScriptDirectoryName = directoryName;
        }        
    }
}
