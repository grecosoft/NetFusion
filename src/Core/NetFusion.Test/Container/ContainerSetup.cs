using NetFusion.Bootstrap.Container;
using NetFusion.Test.Plugins;
using System;

namespace NetFusion.Test.Container
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
        /// Creates an instance an AppContainer using an instance of the 
        /// TestTypeResolver class.
        /// </summary>
        /// <param name="resolver">Delegate passed an instance of the created 
        /// resolver.  The consumer can add mock plug-ins and associated types
        /// to the resolver for unit-testing.</param>
        /// <param name="container">Delegate passed an instance of the created
        /// AppContainer for additional consumer configuration.</param>
        /// <returns>Reference to object that can be used to act on the configured 
        /// application container.</returns>
        public static ContainerTest Arrange(Action<TestTypeResolver> resolver,
            Action<IAppContainer> container = null)
        {
            var typeResolver = new TestTypeResolver();
            var appcontainer = new AppContainer(typeResolver, setGlobalReference: false);

            resolver(typeResolver);
            container?.Invoke(appcontainer);

            return new ContainerTest(appcontainer);
        }

        /// <summary>
        /// Creates an instance of an AppContainer using an instance of the specified
        /// type resolver.
        /// </summary>
        /// <param name="typeResolver">Reference to an instance of a type resolver.</param>
        /// <param name="container">Delegate passed an instance of the created AppContainer 
        /// for additional consumer configuration.</param>
        /// <returns>Reference to object that can be used to act on the configured application
        /// container.</returns>
        public static ContainerTest Arrange(ITypeResolver typeResolver, Action<IAppContainer> container = null)
        {
            var appcontainer = new AppContainer(typeResolver, setGlobalReference: false);
            container?.Invoke(appcontainer);

            return new ContainerTest(appcontainer);
        }

        /// <summary>
        /// Allows a host such as LinqPad to initialize an application container for use.  
        /// </summary>
        /// <param name="typeResolver">Reference to an instance of a type resolver.</param>
        /// <returns>Configured application container.</returns>
        public static AppContainer Bootstrap(ITypeResolver typeResolver)
        {
            return new AppContainer(typeResolver, setGlobalReference: false);
        }
    }
}
