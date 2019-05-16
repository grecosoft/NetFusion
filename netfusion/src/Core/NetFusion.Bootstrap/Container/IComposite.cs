using Microsoft.Extensions.DependencyInjection;

namespace NetFusion.Bootstrap.Container
{
    using System;

    /// <summary>
    /// An explicit interface implemented by the CompositeContainer
    /// allowing the internal composite parts to be accessed.
    /// </summary>
    public interface IComposite : ICompositeContainer
    {
        /// <summary>
        /// Reference to the built composite application associated
        /// with the composite container.
        /// </summary>
        CompositeApp CompositeApp { get; }
        
        /// <summary>
        /// Reference to the underlying populated service collection
        /// from which the service provider implementation was created.
        /// </summary>
        IServiceCollection Services { get; }

        /// <summary>
        /// Allows a delegate to be specified during building of the composite container
        /// to specify a custom dependency-container to be used.
        /// </summary>
        /// <param name="providerFactory">Creates a service provider instance using
        /// the provided service-collection.</param>
        void SetProviderFactory(Func<IServiceCollection, IServiceProvider> providerFactory);

        IComposite CreateServiceProvider();
        
        /// <summary>
        /// Reference to the created service provider.
        /// </summary>
        IServiceProvider ServiceProvider { get; }
    }
}