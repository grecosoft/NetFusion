using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Common.Extensions.Collections;
using NetFusion.Core.Bootstrap.Container;
using NetFusion.Core.Bootstrap.Plugins;

// ReSharper disable once NotAccessedField.Local
namespace NetFusion.Core.TestFixtures.Container;

/// <summary>
/// Object containing methods for asserting the test fixture that was acted on.
/// There are methods for asserting the container and any of its associated objects
/// under test.
/// </summary>
public class ContainerAssert 
{
    private readonly ContainerFixture _fixture;
    private readonly CompositeContainer _container;
    private IServiceProvider? _serviceProvider;
        
    private readonly Exception? _resultingException;
        
    // Constructor called to assert a container that has not been acted on,
    public ContainerAssert(ContainerFixture fixture)
    {
        _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
        _container = fixture.GetOrBuildContainer();
    }
        
    // Constructor called to assert an acted on container.
    public ContainerAssert(
        ContainerFixture fixture,
        CompositeContainer container, 
        IServiceProvider? serviceProvider, 
        Exception? resultingException)
    {
        _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
        _container = container ?? throw new ArgumentNullException(nameof(container));
            
        _serviceProvider = serviceProvider;
        _resultingException = resultingException;
    }
        
        
    //-- Composite Application Assertions:
        
    /// <summary>
    /// Allows the unit-test to assert the state of the builder that was used
    /// to construct the ICompositeApp.
    /// </summary>
    /// <param name="assert">Delegate passed an instance of the composite-application
    /// to be asserted.</param>
    /// <returns>Self Reference.</returns>
    public ContainerAssert CompositeAppBuilder(Action<ICompositeAppBuilder> assert)
    {
        if (assert == null) throw new ArgumentNullException(nameof(assert), 
            "Assert method not specified.");

        _fixture.AssureContainerComposed();
            
        assert(_fixture.GetOrBuildContainer().AppBuilder);
        return this;
    }
        
    public ContainerAssert CompositeContainer(Action<CompositeContainer> assert)
    {
        if (assert == null) throw new ArgumentNullException(nameof(assert), 
            "Assert method not specified.");

        _fixture.AssureContainerComposed();
            
        assert(_fixture.GetOrBuildContainer());
        return this;
    }

    /// <summary>
    /// Allows the unit-test to assert the state of the created composite-application.
    /// </summary>
    /// <param name="assert">The method is passed an instance of the composite-application
    /// to be asserted.</param>
    /// <returns>Self Reference.</returns>
    public ContainerAssert CompositeApp(Action<ICompositeApp> assert)
    {
        if (assert == null) throw new ArgumentNullException(nameof(assert), 
            "Assert method not specified.");

        assert(_fixture.AppUnderTest);
        return this;
    }
        
    /// <summary>
    /// Allows the unit-test to assert the state of the created composite-application.
    /// </summary>
    /// <param name="assert">The method is passed an instance of the composite-application
    /// to be asserted.</param>
    /// <returns>Self Reference.</returns>
    public async Task<ContainerAssert> CompositeAppAsync(Func<ICompositeApp, Task> assert)
    {
        if (assert == null) throw new ArgumentNullException(nameof(assert), 
            "Assert method not specified.");

        await assert(_fixture.AppUnderTest);
        return this;
    }

        
    //-- Plugin State Assertions:
        
    /// <summary>
    /// Allows the unit-test to assert the sate of a plug-in contained within the created
    /// application container.  
    /// </summary>
    /// <typeparam name="TPlugin">The type of the plug-in manifest used to find the
    /// underlying plug-in object created.</typeparam>
    /// <param name="assert">The method passed an instance of the plug-in to be asserted.</param>
    /// <returns>Self reference for method chaining.</returns>
    public ContainerAssert Plugin<TPlugin>(Action<TPlugin> assert)
        where TPlugin : IPlugin
    {
        if (assert == null) throw new ArgumentNullException(nameof(assert), 
            "Assert method not specified.");
            
        _fixture.AssureContainerComposed();

        var plugins = _fixture.GetOrBuildContainer().AppBuilder.AllPlugins
            .Where(p => p.GetType() == typeof(TPlugin)).ToArray();

        if (plugins.Length > 1)
        {
            throw new InvalidOperationException(
                $"More than one plug-in of the type: {typeof(TPlugin)} found.");
        }

        var plugin = plugins.FirstOrDefault() ??
                     throw new InvalidOperationException($"Plug-in of type: {typeof(TPlugin)} not found.");

        assert((TPlugin)plugin);
        return this;
    }
        
    /// <summary>
    /// Allows the unit-test to assert the state of a plug-in configuration associated with
    /// the application container.
    /// </summary>
    /// <typeparam name="TConfig">The type of the configuration to search.</typeparam>
    /// <param name="assert">The method passed an instance of the configuration to be asserted.</param>
    /// <returns>Self reference for method chaining.</returns>
    public ContainerAssert Configuration<TConfig>(Action<TConfig> assert)
        where TConfig : IPluginConfig
    {
        if (assert == null) throw new ArgumentNullException(nameof(assert), 
            "Assert method not specified.");

        _fixture.AssureContainerComposed();

        var config = _fixture.GetOrBuildContainer().AppBuilder
            .AllPlugins.SelectMany(p => p.Configs)
            .OfType<TConfig>().FirstOrDefault();

        if (config == null)
        {
            throw new InvalidOperationException(
                $"Plug-in configuration of type: {typeof(TConfig)} was not found to assert.");
        }

        assert(config);
        return this;
    }

    /// <summary>
    /// Allows the unit-test to assert the state of a plug-in module associated with 
    /// the created application container.
    /// </summary>
    /// <typeparam name="TModule">The type of the module to find.</typeparam>
    /// <param name="assert">The method passed an instance of the plug-in module to
    /// be asserted.</param>
    /// <returns>Self reference for method chaining.</returns>
    public ContainerAssert PluginModule<TModule>(Action<TModule> assert)
        where TModule : IPluginModule
    {
        if (assert == null) throw new ArgumentNullException(nameof(assert), 
            "Assert method not specified.");

        _fixture.AssureContainerComposed();
                
        var modules = _fixture.GetOrBuildContainer().AppBuilder.AllModules
            .OfType<TModule>()
            .ToArray();

        if (modules.Empty())
        {
            throw new InvalidOperationException(
                $"Plug-in module of type: {typeof(TModule)} was not found to assert.");
        }
            
        if (modules.Length > 1)
        {
            throw new InvalidOperationException(
                $"Multiple Plug-in modules of type: {typeof(TModule)} were found.");
        }

        assert(modules.First());

        return this;
    }
        
        
    //-- Service Assertions:
        
    /// <summary>
    /// Allows the populated service-collection to be asserted.
    /// </summary>
    /// <param name="assert">Delegate passed the service-collection to be asserted.</param>
    /// <returns></returns>
    public ContainerAssert ServiceCollection(Action<IServiceCollection> assert)
    {
        if (assert == null) throw new ArgumentNullException(nameof(assert), 
            "Assert method not specified.");
            
        _fixture.AssureContainerComposed();

        assert(_fixture.GetOrBuildContainer().AppBuilder.ServiceCollection);
        return this;
    }
        
    /// <summary>
    /// Passed reference to the created service provider to be asserted.
    /// The assert method can create instances of servers to assert their state.
    /// </summary>
    /// <param name="assert">The assert method.</param>
    /// <returns>Self Reference.</returns>
    public ContainerAssert Services(Action<IServiceProvider> assert)
    {
        if (assert == null) throw new ArgumentNullException(nameof(assert));
            
        _fixture.AssureCompositeAppStarted();
        _serviceProvider ??= _fixture.AppUnderTest.CreateServiceScope().ServiceProvider;

        assert(_serviceProvider);
        return this;
    }
        
    public async Task<ContainerAssert> ServicesAsync(Func<IServiceProvider, Task> assert)
    {
        if (assert == null) throw new ArgumentNullException(nameof(assert));
            
        _fixture.AssureCompositeAppStarted();
        _serviceProvider ??= _fixture.AppUnderTest.CreateServiceScope().ServiceProvider;

        await assert(_serviceProvider);
        return this;
    }

    /// <summary>
    /// Creates service instance from the container that can be asserted.
    /// </summary>
    /// <param name="assert">Method passed the service instance to assert.
    /// If the service is not registered, null will be returned.</param>
    /// <typeparam name="T">The service Type.</typeparam>
    /// <returns>Self Reference.</returns>
    public ContainerAssert Service<T>(Action<T> assert)
        where T : notnull
    {
        if (assert == null) throw new ArgumentNullException(nameof(assert));
            
        _fixture.AssureCompositeAppStarted();
        _serviceProvider ??= _fixture.AppUnderTest.CreateServiceScope().ServiceProvider;

        T serviceUnderTest = _serviceProvider.GetRequiredService<T>();
        assert(serviceUnderTest);

        return this;
    }
        
    /// <summary>
    /// Creates service instance from the container that can be asserted.
    /// </summary>
    /// <param name="assert">Method passed the service instance to assert.</param>
    /// <typeparam name="T">The service Type.</typeparam>
    /// <returns>Self referenced within a pending task.</returns>
    public async Task<ContainerAssert> ServiceAsync<T>(Func<T, Task> assert)
        where T : notnull
    {
        if (assert == null) throw new ArgumentNullException(nameof(assert));
            
        _fixture.AssureCompositeAppStarted();
        _serviceProvider ??= _fixture.AppUnderTest.CreateServiceScope().ServiceProvider;

        T serviceUnderTest = _serviceProvider.GetRequiredService<T>();
        await assert(serviceUnderTest);

        return this;
    }
        

    //-- Exception Assertions:
        
    /// <summary>
    /// Allows the unit-test to assert an expected exception that should have been recorded
    /// when acting on the application container under test.  If no exception was recorded 
    /// or the exception is not of the expected type, an exception is thrown.
    /// </summary>
    /// <typeparam name="TEx">The type of the expected exception.</typeparam>
    /// <param name="assert">Method passed an instance of the exception to be further asserted.</param>
    /// <returns>Self reference for method chaining.</returns>
    public ContainerAssert Exception<TEx>(Action<TEx> assert)
        where TEx : Exception
    {
        if (assert == null) throw new ArgumentNullException(nameof(assert), 
            "Assert method not specified.");

        if (_resultingException == null)
        {
            throw new InvalidOperationException(
                "The unit-test Act method didn't result in an exception.");
        }

        if (_resultingException.GetType() != typeof(TEx))
        {
            throw new InvalidOperationException(
                "The unit-test Act method resulted in an exception but of the wrong type." +
                $"Expected: {typeof(TEx).FullName}  Received: {_resultingException.GetType().FullName}");
        }

        assert((TEx)_resultingException);
        return this;
    }

    /// <summary>
    /// Allows the unit-test to assert an expected exception that should have been recorded
    /// when acting on the application container under test.  If no exception was recorded,
    /// an exception is thrown.
    /// </summary>
    /// <param name="assert">Method passed an instance of the exception to be asserted.</param>
    /// <returns>Self reference for method chaining.</returns>
    public ContainerAssert Exception(Action<Exception> assert)
    {
        if (assert == null) throw new ArgumentNullException(nameof(assert), 
            "Assert method not specified.");

        if (_resultingException == null)
        {
            throw new InvalidOperationException(
                "The unit-test Act method didn't result in an exception.");
        }

        assert(_resultingException);
        return this;
    }
        
        
    //--State Assertions:
        
    /// <summary>
    /// Allows the unit-test to run an assert on state captured external to the
    /// test fixture that is under test.
    /// </summary>
    /// <param name="assert">Method to assert state.</param>
    /// <returns>Self reference.</returns>
    public ContainerAssert State(Action assert)
    {            
        if (assert == null) throw new ArgumentNullException(nameof(assert),
            "Assert method not specified.");

        assert();
        return this;
    }
}