using Microsoft.Extensions.DependencyInjection;

namespace NetFusion.Bootstrap.Container
{
    /// <summary>
    /// An explicit interface implemented by the CompositeContainer
    /// allowing the internal Composite Application to be accessed.
    /// </summary>
    public interface IComposite
    {
        /// <summary>
        /// Reference to the built composite application associated
        /// with the composite container.
        /// </summary>
        CompositeApp CompositeApp { get; }
        
        IServiceCollection Services { get; }
    }
}