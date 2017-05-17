using NetFusion.Bootstrap.Plugins;

namespace NetFusion.Utilities.Validation.Core
{
    /// <summary>
    /// Interface implemented by plug-in module so its services can be accessed
    /// from other application components.
    /// </summary>
    public interface IValidationModule : IPluginModuleService
    {
        /// <summary>
        /// Returns an object containing the validations for an object.
        /// </summary>
        /// <param name="obj">The object to be validated.</param>
        /// <returns>Instance of an object containing validations.</returns>
        IObjectValidator CreateValidator(object obj);
    }
}
