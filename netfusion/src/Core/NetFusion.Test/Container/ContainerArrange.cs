using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Container;
using NetFusion.Test.Plugins;
using System;
using NetFusion.Bootstrap.Plugins;

namespace NetFusion.Test.Container
{
    /// <summary>
    /// Contains methods for arranging the type-resolver and application container
    /// under-test before actions are executed and their results asserted.  The 
    /// methods on this class allow the unit-test to initialize the expected state.
    /// </summary>
    public class ContainerArrange
    {
        private readonly ContainerFixture _fixture;

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
        public ContainerArrange Container(Action<CompositeContainer> arrange)
        {
            if (arrange == null) throw new ArgumentNullException(nameof(arrange));

            arrange(_fixture.ContainerUnderTest);
            return this;
        }

        public ContainerArrange PluginConfig<TConfig>(Action<TConfig> arrange) where  TConfig : IPluginConfig
        {
            var config =  _fixture.ContainerUnderTest.GetPluginConfig<TConfig>();
            arrange(config);
            return this;
        }
    
        /// <summary>
        /// Allows the unit-test to act on the test-fixture under test.
        /// </summary>
        public ContainerAct Act => new ContainerAct(_fixture);
    
        /// <summary>
        /// Allows the unit-test to assert on the state of the acted on test-fixture.
        /// </summary>
        public ContainerAssert Assert => new ContainerAssert(_fixture);
    }
}

