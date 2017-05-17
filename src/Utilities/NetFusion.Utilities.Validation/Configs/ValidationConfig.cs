using NetFusion.Base.Plugins;
using NetFusion.Bootstrap.Container;
using NetFusion.Utilities.Validation.Core;
using System;

namespace NetFusion.Utilities.Validation.Configs
{
    public class ValidationConfig : IContainerConfig, IKnownPluginType
    {
        /// <summary>
        /// The type implementing IObjectValidator to be used.  This defaults to 
        /// an implementation based on Microsoft's component validation attributes.
        /// </summary>
        public Type ValidatorType { get; private set; } = typeof(ObjectValidator);

        /// <summary>
        /// Used to specify the type implementing IObjectValidator to be used.
        /// </summary>
        /// <typeparam name="T">The type used for validation.</typeparam>
        public void UseValidatorType<T>() where T : IObjectValidator
        {
            this.ValidatorType = typeof(T);
        }
    }
}
