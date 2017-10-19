using NetFusion.Base.Plugins;
using NetFusion.Domain.Entities.Core;

namespace NetFusion.Domain.Entities.Registration
{
    /// <summary>
    /// Application component can specify multiple instances of this interface
    /// to indicate what behaviors are associated with a given domain-entity.
    /// Types implementing this interface are discovered during the application
    /// bootstrap process and used to configure the DomainEntityFactory.
    /// </summary>
    public interface IBehaviorRegistry : IKnownPluginType
    {
        /// <summary>
        /// Called to allow an application component to register domain-entity behaviors.
        /// </summary>
        /// <param name="registry">Reference to a factory used to register behaviors.</param>
        void Register(IFactoryRegistry registry);
    }
}
