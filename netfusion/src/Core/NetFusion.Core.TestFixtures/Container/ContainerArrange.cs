﻿using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Core.Bootstrap.Container;
using NetFusion.Core.Bootstrap.Plugins;

namespace NetFusion.Core.TestFixtures.Container;

/// <summary>
/// Contains methods for arranging the container under test.
/// </summary>
public class ContainerArrange(ContainerFixture fixture)
{
    private readonly ContainerFixture _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));

    /// <summary>
    /// Configures the configuration-builder with settings required for the container under-test.
    /// </summary>
    /// <param name="arrange">Delegate passed the configuration builder to be initialized.</param>
    /// <returns>Self Reference</returns>
    public ContainerArrange Configuration(Action<IConfigurationBuilder> arrange)
    {
        ArgumentNullException.ThrowIfNull(arrange);

        if (_fixture.IsCompositeContainerBuilt)
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
    public ContainerArrange Container(Action<ICompositeContainer> arrange)
    {
        ArgumentNullException.ThrowIfNull(arrange);

        if (_fixture.IsCompositeContainerBuilt)
        {
            throw new InvalidOperationException(
                "Composite Container cannot be arranged once built.");
        }

        arrange(_fixture.GetOrBuildContainer());
        return this;
    }

    /// <summary>
    /// Allows arranging state contained within the unit-test.
    /// </summary>
    /// <param name="arrange">Method invoked to arrange state.</param>
    /// <returns>Self Reference.</returns>
    public ContainerArrange State(Action arrange)
    {
        ArgumentNullException.ThrowIfNull(arrange);

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
        ArgumentNullException.ThrowIfNull(arrange);

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
        ArgumentNullException.ThrowIfNull(arrange);

        if (_fixture.IsCompositeContainerBuilt)
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