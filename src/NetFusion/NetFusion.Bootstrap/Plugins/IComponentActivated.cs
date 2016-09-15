namespace NetFusion.Bootstrap.Plugins
{
    /// <summary>
    /// Can be implemented by a component added to the dependency
    /// injection container to be notified when it is activated.
    /// </summary>
    public interface IComponentActivated
    {
        /// <summary>
        /// Called when dependency injection container activates the component.
        /// </summary>
        void OnActivated();
    }
}
