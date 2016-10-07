using System;
using NetFusion.Common.Validation;

namespace NetFusion.Settings
{
    /// <summary>
    /// Base class containing a default implementation of the 
    /// IAppSettings interface.
    /// </summary>
    public abstract class AppSettings : IAppSettings,
        IObjectValidation
    {
        /// <summary>
        /// The identity value of the application setting.
        /// </summary>
        public string AppSettingsId { get; set; }

        /// <summary>
        /// The identity value of the host application that is 
        /// specified in the host plug-in manifest.
        /// </summary>
        public string ApplicationId { get; set; }

        /// <summary>
        /// The environment for which the settings should be loaded.
        /// </summary>
        public EnvironmentTypes Environment { get; set; }

        /// <summary>
        /// The machine associated with the configuration.
        /// </summary>
        public string MachineName { get; set; }

        /// <summary>
        /// Determines if a default uninitialized instance of the settings class
        /// can be used if no corresponding initialization strategy is found.
        /// </summary>
        public bool IsInitializationRequired { get; set; } = true;

        public virtual ObjectValidator ValidateObject()
        {
            return new ObjectValidator(this); 
        }
    }
}
