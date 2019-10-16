using NetFusion.Base.Validation;
using System;
using NetFusion.Bootstrap.Container;

namespace NetFusion.Bootstrap.Validation
{
    /// <summary>
    /// Container configuration used to register the IObjectValidator
    /// implementation that should be used by the built composite-application.
    /// </summary>
    public class ValidationConfig : IContainerConfig
    {
        /// <summary>
        /// The type implementing IObjectValidator to be used.  This defaults to 
        /// an implementation based on Microsoft's Data Annotations.
        /// </summary>
        public Type ValidatorType { get; private set; } = typeof(DataAnnotationsValidator);

        /// <summary>
        /// Used to specify the type implementing IObjectValidator to be used.
        /// </summary>
        /// <typeparam name="T">The type used for validation.</typeparam>
        public void UseValidatorType<T>() where T : IObjectValidator
        {
            ValidatorType = typeof(T);
        }
    }
}
