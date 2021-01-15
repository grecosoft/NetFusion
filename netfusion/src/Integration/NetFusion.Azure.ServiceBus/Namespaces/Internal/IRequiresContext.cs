namespace NetFusion.Azure.ServiceBus.Namespaces.Internal
{
    /// <summary>
    /// Allows a strategy to indicate a dependency on the Namespace context.
    /// </summary>
    public interface IRequiresContext
    {
        /// <summary>
        /// Reference to the namespace context containing references to common
        /// modules, services, and utility methods used by strategies.
        /// </summary>
        NamespaceContext Context { get; set; }
    }
}