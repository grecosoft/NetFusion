using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Base.Validation;

namespace NetFusion.Bootstrap.Container
{
    public interface ICompositeContainer
    {
        /// <summary>
        /// Loads and initializes all of the plug-ins but does not start their execution.
        /// </summary>
        /// <returns>Reference to the loaded container that can be started.</returns>
        IBuiltContainer Build(ITypeResolver typeResolver);

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
        /// Executes a delegate within a newly created scoped service provider.  The scope
        /// is disposed after the delegate executes.
        /// </summary>
        /// <param name="action">The delegate to be executed within a new service scope.</param>
        void ExecuteInServiceScope(Action<IServiceProvider> action);

        /// <summary>
        /// Creates an IObjectValidator used to validate a specific object instance.
        /// </summary>
        /// <param name="obj">The object to be validated.</param>
        /// <returns>Instance of IObjectValidator.</returns>
        IObjectValidator CreateValidator(object obj);

        /// <summary>
        /// Allows each module to be safely stopped.
        /// </summary>
        void Stop();
    }
}