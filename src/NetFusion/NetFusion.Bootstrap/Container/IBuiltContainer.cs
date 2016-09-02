namespace NetFusion.Bootstrap.Container
{
    /// <summary>
    /// Methods that can be invoked on a built container.
    /// </summary>
    public interface IBuiltContainer
    {
        /// <summary>
        /// Starts all created application plug-ins.  This is the last step of the
        /// application container creation process.  The application host should 
        /// call this method after it has been completely initialized.
        /// </summary>
        void Start();
    }
}
