using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Container;
using System;
using NetFusion.Bootstrap.Plugins;

namespace NetFusion.Test.Container
{
    /// <summary>
    /// Contains methods for arranging the container under test.
    /// </summary>
    public class ContainerArrange
    {
        private readonly ContainerFixture _fixture;

        public ContainerArrange(ContainerFixture fixture)
        {
            _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
        }
        
        /// <summary>
        /// Configures the configuration-builder with settings required for the container under-test.
        /// </summary>
        /// <param name="arrange">Delegate passed the configuration builder to be initialized.</param>
        /// <returns>Self Reference</returns>
        public ContainerArrange Configuration(Action<IConfigurationBuilder> arrange)
        {
            if (arrange == null) throw new ArgumentNullException(nameof(arrange));

            if (_fixture.IsCompositeContainerBuild)
            {
                throw new InvalidOperationException(
                    "Configuration cannot be arranged once Composite Container is Built.");
            }

            arrange(_fixture.ConfigBuilder);
            return this;
        }
        
        /// <summary>
        /// Configures the composite-container with a known set of plugins pertaining to the unit-test.
        /// </summary>
        /// <param name="arrange">Method passed the application container under test.</param>
        /// <returns>Self Reference</returns>
        public ContainerArrange Container(Action<CompositeContainer> arrange)
        {
            if (arrange == null) throw new ArgumentNullException(nameof(arrange));
            
            if (_fixture.IsCompositeContainerBuild)
            {
                throw new InvalidOperationException(
                    "Services cannot be arranged once Composite Container built.");
            }

            arrange(_fixture.GetOrBuildContainer());
            return this;
        }

        public ContainerArrange State(Action arrange)
        {
            if (arrange == null) throw new ArgumentNullException(nameof(arrange));
            
            arrange();
            return this;
        }
        
        /// <summary>
        /// Initializes a plugin configuration required for the container objects under test. 
        /// </summary>
        /// <param name="arrange">Delegate passed the plugin configuration.</param>
        /// <typeparam name="TConfig"></typeparam>
        /// <returns>Self Reference</returns>
        public ContainerArrange PluginConfig<TConfig>(Action<TConfig> arrange)
            where  TConfig : IPluginConfig
        {
            if (arrange == null) throw new ArgumentNullException(nameof(arrange));
            
            var config =  _fixture.GetOrBuildContainer().GetPluginConfig<TConfig>();
            arrange(config);
            return this;
        }
        
        /// <summary>
        /// Configures the service-collection with services required for
        /// the container objects under test.
        /// </summary>
        /// <param name="arrange">Delegate passed the service-collection.</param>
        /// <returns>Self Reference</returns>
        public ContainerArrange Services(Action<IServiceCollection> arrange)
        {
            if (arrange == null) throw new ArgumentNullException(nameof(arrange));

            if (_fixture.IsCompositeContainerBuild)
            {
                throw new InvalidOperationException(
                    "Services cannot be arranged once Composite Container built.");
            }

            arrange(_fixture.ServiceOverrides);
            return this;
        }

        /// <summary>
        /// Allows the unit-test to act on the test-fixture under test.
        /// </summary>
        public ContainerAct Act => new(_fixture);
    
        /// <summary>
        /// Allows the unit-test to assert on the state of the acted on test-fixture.
        /// </summary>
        public ContainerAssert Assert => new(_fixture);
    }
}

