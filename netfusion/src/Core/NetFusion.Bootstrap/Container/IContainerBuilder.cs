using System;

namespace NetFusion.Bootstrap.Container
{
    /// <summary>
    /// Used to build and instance of an application container.
    /// </summary>
    public interface IContainerBuilder
    {
        /// <summary>
        /// Delegate used to configure the application container being built.
        /// </summary>
        /// <param name="bootstrap">Delegate used to configure container.</param>
        /// <returns>Self reference for method chaining.</returns>
        IContainerBuilder Bootstrap(Action<IAppContainer> bootstrap);

        /// <summary>
        /// Builds the container and returns reference that can be used to
        /// start its execution.
        /// </summary>
        /// <returns>Reference to the build container.</returns>
        IBuiltContainer Build();
    }
}
