using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Base.Validation;

namespace NetFusion.Bootstrap.Container
{
    /// <summary>
    /// Interface containing the composite-container public API registered within 
    /// the service-collection.
    /// </summary>
    public interface ICompositeContainer
    {
        /// <summary>
        /// Log of the composite application built by the application container.
        /// </summary>
        /// <returns>Dictionary of key/value pairs that can be serialized to JSON.</returns>
        IDictionary<string, object> Log { get; }
       
        /// <summary>
        /// Creates a new service scope that can be used to instantiate and execute services.
        /// After invoking the instanced services, the created scope should be disposed. 
        /// </summary>
        /// <returns>New service scope.</returns>
        IServiceScope CreateServiceScope();

        /// <summary>
        /// Creates an IObjectValidator used to validate a specific object instance.
        /// </summary>
        /// <param name="obj">The object to be validated.</param>
        /// <returns>Instance of IObjectValidator.</returns>
        IObjectValidator CreateValidator(object obj);

        /// <summary>
        /// Starts each plugin module.
        /// </summary>
        /// <returns>Task that can be awaited after which all plugin modules will have been started.</returns>
        Task StartAsync();
        
        /// <summary>
        /// Stops each plugin module.
        /// </summary>
        /// <returns>Task that cna be awaited after which all plugin modules will have been stopped.</returns>
        Task StopAsync();
        
        /// <summary>
        /// Starts each plugin module.
        /// </summary>
        void Start();
        
        /// <summary>
        /// Stops each plugin module.
        /// </summary>
        void Stop();
    }
}