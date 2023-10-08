using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Core.Bootstrap.Container;
using NetFusion.Core.Bootstrap.Plugins;

namespace NetFusion.Core.TestFixtures.Container;

/// <summary>
/// Returns object used to act on the created and arranged composite-container.
/// After the container is acted on, the unit-test uses the Assert property to 
/// test the state of the container or one of its related objects.
/// </summary>
public class ContainerAct
{
    private readonly ContainerFixture _fixture;
    private readonly ICompositeContainer _container;

    private bool _actedOn;
    private IServiceProvider? _serviceProvider;
    private bool _recordException;
    private Exception? _resultingException;
    private object? _serviceResult;

    public ContainerAct(ContainerFixture fixture)
    {
        _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
        _container = fixture.GetOrBuildContainer();
    }
        
        
    //-- Composite Container Actions:
        
    /// <summary>
    /// Bootstraps the composite-container and adds the resulting IContainerApp service-collection.
    /// </summary>
    /// <returns>Self Reference.</returns>
    public ContainerAct ComposeContainer()
    {
        if (_actedOn)
        {
            throw new InvalidOperationException("The container can only be acted on once.");
        }

        _actedOn = true;
        try
        {
            _fixture.AssureContainerComposed();
        }
        catch (Exception ex)
        {
            _resultingException = ex;
            if (!_recordException)
            {
                throw;
            }
        }

        return this;
    }

    /// <summary>
    /// Allows an unit-test to act on the composite-application under test.
    /// </summary>
    /// <param name="act">Method passed the instance of the application under test to be
    /// acted on by the unit-test.  The method can invoke an asynchronous method.</param>
    /// <returns>Self Reference.</returns>
    public async Task<ContainerAct> OnCompositeAppAsync(Func<ICompositeApp, Task> act)
    {
        if (_actedOn)
        {
            throw new InvalidOperationException("The container can only be acted on once.");
        }

        _actedOn = true;
        try
        {
            await act(_fixture.AppUnderTest).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _resultingException = ex;
            if (!_recordException)
            {
                throw;
            }
        }

        return this;
    }
        
    /// <summary>
    /// Allows an unit-test to act on the composite-application under test.
    /// </summary>
    /// <param name="act">Method passed the instance of the application under test to be
    /// acted on by the unit-test.</param>
    /// <returns>Self Reference.</returns>
    public ContainerAct OnCompositeApp(Action<ICompositeApp> act)
    {
        if (_actedOn)
        {
            throw new InvalidOperationException("The container can only be acted on once.");
        }

        _actedOn = true;
        try
        {
            act(_fixture.AppUnderTest);
        }
        catch (Exception ex)
        {
            _resultingException = ex;
            if (!_recordException)
            {
                throw;
            }
        }

        return this;
    }
        
    /// <summary>
    /// Allows an unit-test to act on composite-container under test.
    /// </summary>
    /// <param name="act">Method passed the instance of the composite-container
    /// under test to be acted on by the unit-test.</param>
    /// <returns>Self Reference</returns>
    public ContainerAct OnCompositeContainer(Action<ICompositeContainer> act)
    {
        if (_actedOn)
        {
            throw new InvalidOperationException("The container can only be acted on once.");
        }

        _actedOn = true;
        try
        {
            act(_container);
        }
        catch (Exception ex)
        {
            _resultingException = ex;
            if (!_recordException)
            {
                throw;
            }
        }

        return this;
    }
        
        
    //-- Service Provider Assertions:

    /// <summary>
    /// Allows an unit-test to act on the server-provider associated with the composite-application.
    /// </summary>
    /// <param name="act">Method called to act on the service provider.</param>
    /// <returns>Self Reference.</returns>
    public async Task<ContainerAct> OnServicesAsync(Func<IServiceProvider, Task> act)
    {
        if (_actedOn)
        {
            throw new InvalidOperationException("The container can only be acted on once.");
        }
            
        _fixture.AssureCompositeAppStarted();
            
        _actedOn = true;
        try
        {
            var testScope = _fixture.AppUnderTest.CreateServiceScope();
            _serviceProvider = testScope.ServiceProvider;

            await act(_serviceProvider).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _resultingException = ex;
            if (!_recordException)
            {
                throw;
            }
        }

        return this;
    }
        
    /// <summary>
    /// Allows an unit-test to act on the server-provider associated with the composite-application.
    /// </summary>
    /// <param name="act">Method called to act on the service provider.</param>
    /// <returns>Self Reference.</returns>
    public ContainerAct OnServices(Action<IServiceProvider> act)
    {
        if (_actedOn)
        {
            throw new InvalidOperationException("The container can only be acted on once.");
        }

        _fixture.AssureCompositeAppStarted();
            
        _actedOn = true;
        try
        {
            var testScope = _fixture.AppUnderTest.CreateServiceScope();
            _serviceProvider = testScope.ServiceProvider;

            act(_serviceProvider);
        }
        catch (Exception ex)
        {
            _resultingException = ex;
            if (!_recordException)
            {
                throw;
            }
        }

        return this;
    }
        
    /// <summary>
    /// Allows a module form which the composite-application is composted to be acted on
    /// to adjust its state for a specific behavior being asserted.
    /// </summary>
    /// <param name="act">Method called to act on a plugin module.</param>
    /// <typeparam name="TModule">The type of the module to be acted on.</typeparam>
    /// <returns>Self Reference.</returns>
    public ContainerAct OnModule<TModule>(Action<TModule> act)
        where TModule : IPluginModule
    {
        _fixture.AssureContainerComposed();
        _actedOn = true;
            
        var module = _container.AppBuilder.AllModules.OfType<TModule>().FirstOrDefault();
        if (module == null)
        {
            throw new InvalidOperationException(
                $"The plugin module of type: {typeof(TModule)} is not registered");
        }
            
        try
        {
            act(module);
        }
        catch (Exception ex)
        {
            _resultingException = ex;
            if (!_recordException)
            {
                throw;
            }
        }

        return this;
    }
        
        
    //-- Service Instance Assertions:

    /// <summary>
    /// If called, any raised exception will be recorded and not rethrown so the state
    /// of the exception can be asserted. 
    /// </summary>
    /// <returns>Self Reference.</returns>
    public ContainerAct RecordException()
    {
        if (_actedOn)
        {
            throw new InvalidOperationException("Exception must be marked for recording before action is taken.");
        }

        _recordException = true;
        return this;
    }

    /// <summary>
    /// Allows a registered service to be acted on so the results can be asserted.
    /// </summary>
    /// <param name="act">Reference to the service so it can be acted on.</param>
    /// <typeparam name="T">The type of the service to be acted on.</typeparam>
    /// <returns>Self Reference.</returns>
    public async Task<ContainerAct> OnServiceAsync<T>(Func<T, Task> act)
        where T : notnull
    {
        if (_actedOn)
        {
            throw new InvalidOperationException("The container can only be acted on once.");
        }
            
        _fixture.AssureCompositeAppStarted();
            
        _actedOn = true;
        try
        {
            var testScope = _fixture.AppUnderTest.CreateServiceScope();
            _serviceProvider = testScope.ServiceProvider;

            await act(_serviceProvider.GetRequiredService<T>()).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _resultingException = ex;
            if (!_recordException)
            {
                throw;
            }
        }

        return this;
    }

    /// <summary>
    /// Allows a registered service to be acted on so the results can be asserted.
    /// </summary>
    /// <param name="act">Reference to the service so it can be acted on.</param>
    /// <typeparam name="T">The type of the service to be acted on.</typeparam>
    /// <returns>Self Reference.</returns>
    public ContainerAct OnService<T>(Action<T> act)
        where T : notnull
    {
        if (_actedOn)
        {
            throw new InvalidOperationException("The container can only be acted on once.");
        }
            
        _fixture.AssureCompositeAppStarted();
            
        _actedOn = true;
        try
        {
            var testScope = _fixture.AppUnderTest.CreateServiceScope();
            _serviceProvider = testScope.ServiceProvider;

            act(_serviceProvider.GetRequiredService<T>());
        }
        catch (Exception ex)
        {
            _resultingException = ex;
            if (!_recordException)
            {
                throw;
            }
        }

        return this;
    }
    
    public ContainerAct OnService<T, TResult>(Func<T, TResult> act)
        where T : notnull
    {
        if (_actedOn)
        {
            throw new InvalidOperationException("The container can only be acted on once.");
        }
            
        _fixture.AssureCompositeAppStarted();
            
        _actedOn = true;
        try
        {
            var testScope = _fixture.AppUnderTest.CreateServiceScope();
            _serviceProvider = testScope.ServiceProvider;

            _serviceResult = act(_serviceProvider.GetRequiredService<T>());
        }
        catch (Exception ex)
        {
            _resultingException = ex;
            if (!_recordException)
            {
                throw;
            }
        }

        return this;
    }
        
    /// <summary>
    /// After acting on the container under test, the unit-test can call methods on this
    /// property to assert its state.
    /// </summary>
    public ContainerAssert Assert => new (_fixture, _container, 
        _serviceProvider, 
        _serviceResult, 
        _resultingException);
}