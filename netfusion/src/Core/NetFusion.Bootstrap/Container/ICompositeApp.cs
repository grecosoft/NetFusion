using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Base.Validation;

namespace NetFusion.Bootstrap.Container
{
    /// <summary>
    /// Represents an application composed from a set of plugins.
    /// </summary>
    public interface ICompositeApp
    {
        /// <summary>
        /// Log containing details of how the application was composed from plugins.
        /// </summary>
        IDictionary<string, object> Log { get; }
        
        /// <summary>
        /// Starts each plugin module.
        /// </summary>
        /// <returns>Task that can be awaited after which all plugin modules will have been started.</returns>
        Task StartAsync();

        /// <summary>
        /// Starts each plugin module.
        /// </summary>
        void Start();

        /// <summary>
        /// Allows for service-location from a component that is not registered in the container.
        /// </summary>
        /// <returns>Service scope from which services can be resolved.  The scope must be
        /// disposed after use.</returns>
        IServiceScope CreateServiceScope();

        /// <summary>
        /// Creates and object-validator used to validate an object based on the
        /// validation implementation specified when bootstrapping the application.
        /// </summary>
        /// <param name="obj">The object for which the validator is created.</param>
        /// <returns>Validate used to validate a specific entity.</returns>
        IObjectValidator CreateValidator(object obj);
        
        /// <summary>
        /// Stops each plugin module.
        /// </summary>
        /// <returns>Task that cna be awaited after which all plugin modules will have been stopped.</returns>
        Task StopAsync();

        /// <summary>
        /// Stops each plugin module.
        /// </summary>
        void Stop();
    }
}