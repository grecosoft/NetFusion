using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Container;
using NetFusion.Test.Plugins;
using System;

namespace NetFusion.Test.Container
{
    /// <summary>
    /// Contains methods for arranging the type-resolver and application container
    /// under-test before actions are executed and their results asserted.  The 
    /// methods on this class allow the unit-test to initialize the expected state.
    /// </summary>
    public class ContainerArrange
    {
        private ContainerFixture _fixture;

        public ContainerArrange(ContainerFixture fixture)
        {
            _fixture = fixture;
        }

        public ContainerArrange Services(Action<IServiceCollection> arrange)
        {
            if (arrange == null) throw new ArgumentNullException(nameof(arrange));

            arrange(_fixture.Services);
            return this;
        }

        /// <summary>
        /// Called by a unit-test to arrange the type-resolver to an expected state.
        /// </summary>
        /// <param name="arrange">Method passed the type-resolver under test.</param>
        /// <returns>Self reference for method chaining.</returns>
        public ContainerArrange Resolver(Action<TestTypeResolver> arrange)
        {
            if (arrange == null) throw new ArgumentNullException(nameof(arrange));

            arrange(_fixture.Resolver);
            return this;
        }

        public ContainerArrange Configuration(Action<IConfigurationBuilder> arrange)
        {
            if (arrange == null) throw new ArgumentNullException(nameof(arrange));

            arrange(_fixture.ConfigBuilder);
            return this;
        }

        /// <summary>
        /// Called by a unit-test to arrange the application container to an expected state.
        /// </summary>
        /// <param name="arrange">Method passed the application container under test. </param>
        /// <returns>Self reference for method chaining</returns>
        public ContainerArrange Container(Action<IAppContainer> arrange)
        {
            if (arrange == null) throw new ArgumentNullException(nameof(arrange));

            arrange(_fixture.ContainerUnderTest);
            return this;
        }
 
        /// <summary>
        /// Allows the unit-test to act on the arranged container to assert its correct behavior.  
        /// </summary>
        public ContainerAct Act => new ContainerAct(_fixture);
        public ContainerAssert Assert => new ContainerAssert(_fixture);


    }
}

