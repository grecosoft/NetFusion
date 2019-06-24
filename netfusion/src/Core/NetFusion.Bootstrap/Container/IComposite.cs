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

        IComposite CreateServiceProvider();
        
        /// <summary>
        /// Reference to the created service provider.
        /// </summary>
        IServiceProvider ServiceProvider { get; }
    }
}