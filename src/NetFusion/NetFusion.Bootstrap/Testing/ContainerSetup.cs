using NetFusion.Bootstrap.Container;
using NetFusion.Common;
using System;

namespace NetFusion.Bootstrap.Testing
{
    /// <summary>
    /// Allows client to arrange an instance of a container.  This can be used
    /// when unit-testing and for configuring the application container when 
    /// hosted within LINQ Pad or similar host. This class  implements the
    /// common TDD style of arrange, act, and assert.
    /// </summary>
    public static class ContainerSetup
    {
        /// <summary>
        /// Arranges application container by adding plug-in types to the
        /// provided type resolver.
        /// </summary>
        /// <typeparam name="T">The type of resolver.</typeparam>
        /// <param name="config">Configuration delegate that is passed
        /// and instance of the specified type resolver to which plug-in
        /// types can be added.</param>
        /// <returns>Reference to object that can be used to act on the 
        /// configured application container.</returns>
        public static ContainerAct Arrange<T>(Action<T> config)
            where T : ITypeResolver, new()
        {
            Check.NotNull(config, nameof(config), "configuration delegate not specified");

            var typeResolver = new T();
            config(typeResolver);

            return new ContainerAct(new AppContainer(new string[] { }, typeResolver));
        }

        /// <summary>
        /// Arranges application container by adding plug-in types to the
        /// provided type resolver.  This method uses the HostTypeResolver
        /// by default.
        /// </summary>
        /// <param name="config">Configuration delegate that is passed
        /// and instance of the specified type resolver to which plug-in
        /// types can be added.</param>
        /// <returns>Reference to object that can be used to act on the 
        /// configured application container.</returns>
        public static ContainerAct Arrange(Action<HostTypeResolver> config)
        {
            Check.NotNull(config, nameof(config), "configuration delegate not specified");

            var typeResolver = new HostTypeResolver();
            config(typeResolver);
            return new ContainerAct(new AppContainer(new string[] { }, typeResolver));
        }

        /// <summary>
        /// Arranges application container by adding plug-in types to the
        /// provided type resolver.
        /// </summary>
        /// <typeparam name="T">The type of resolver.</typeparam>
        /// <param name="typeResolver">Reference to an instance of a type resolver.</param>
        /// <param name="config">Configuration delegate that is passed and instance of 
        /// the specified type resolver to which plug-in types can be added.</param>
        /// <returns>Reference to object that can be used to act on the 
        /// configured application container.</returns>
        public static ContainerAct Arrange<T>(T typeResolver, Action<T> config)
            where T : class, ITypeResolver
        {
            Check.NotNull(typeResolver, nameof(typeResolver), "type resolver not specified");
            Check.NotNull(config, nameof(config), "configuration delegate not specified");

            config(typeResolver);
            return new ContainerAct(new AppContainer(new string[] { }, typeResolver));
        }

        /// <summary>
        /// Allows a host such as LinqPad to initialize an application container
        /// for use.  This is very similar to the code that would be used by a
        /// production host.  The main difference is that this method configures
        /// the container to use a variation of the base resolver that allows the
        /// client to easily configure plug-ins without being dependent on runtime 
        /// assemblies.
        /// </summary>
        /// /// <typeparam name="T">The type of resolver.</typeparam>
        /// /// <param name="typeResolver">Reference to an instance of a type resolver.</param>
        /// <param name="config">Configuration delegate that is passed and instance of 
        /// the specified type resolver to which plug-in types can be added.</param>
        /// <returns>Configured application container.</returns>
        public static AppContainer Bootstrap<T>(T typeResolver, Action<T> config)
            where T : class, ITypeResolver
        {
            Check.NotNull(typeResolver, nameof(typeResolver), "type resolver not specified");
            Check.NotNull(config, nameof(config), "configuration delegate not specified");

            config(typeResolver);
            return new AppContainer(new string[] { }, typeResolver);
        }
    }
}
