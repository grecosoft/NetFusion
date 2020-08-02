using NetFusion.Bootstrap.Container;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace NetFusion.Test.Container
{
    /// <summary>
    /// Returns object used to act on the created and arranged composite-container.
    /// After the container is acted on, the unit-test uses the Assert property to 
    /// test the state of the container or one of its related objects.
    /// </summary>
    public class ContainerAct
    {
        private readonly ContainerFixture _fixture;
        private readonly CompositeContainer _container;

        private bool _actedOn;
        private IServiceProvider _serviceProvider;
        private Exception _resultingException;

        public ContainerAct(ContainerFixture fixture)
        {
            _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
            _container = fixture.GetOrBuildContainer();
        }
        
        
        //-- Composite Container Actions:
        
        /// <summary>
        /// Bootstraps the composite-container and adds the resulting IContainerApp
        /// service-collection.
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
            }

            return this;
        }

        /// <summary>
        /// Allows an unit-test to act on the composite-application under test.
        /// </summary>
        /// <param name="act">Method passed the instance of the application under test to be
        /// acted on by the unit-test.  The method can invoke an asynchronous method.</param>
        /// <returns>Self Reference.</returns>
        public async Task<ContainerAct> OnApplication(Func<ICompositeApp, Task> act)
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
            }

            return this;
        }
        
        /// <summary>
        /// Allows an unit-test to act on the composite-application under test.
        /// </summary>
        /// <param name="act">Method passed the instance of the application under test to be
        /// acted on by the unit-test.</param>
        /// <returns>Self Reference.</returns>
        public ContainerAct OnApplication(Action<ICompositeApp> act)
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
            }

            return this;
        }
        
        
        //-- Service Provider Assertions:

        /// <summary>
        /// Allows an unit-test to act on the server-provider associated with the
        /// composite-application.
        /// </summary>
        /// <param name="act">Method called to act on the service provider.</param>
        /// <returns>Self Reference.</returns>
        public async Task<ContainerAct> OnServices(Func<IServiceProvider, Task> act)
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
            }

            return this;
        }
        
        /// <summary>
        /// Allows an unit-test to act on the server-provider associated with the
        /// composite-application.
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
            }

            return this;
        }
        
        
        //-- Service Instance Assertions:

        public async Task<ContainerAct> OnService<T>(Func<T, Task> act)
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
            }

            return this;
        }

        public ContainerAct OnService<T>(Action<T> act)
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
            }

            return this;
        }
        
        /// <summary>
        /// After acting on the container under test, the unit-test can call methods on this
        /// property to assert its state.
        /// </summary>
        public ContainerAssert Assert => new ContainerAssert(_fixture, _container, 
            _serviceProvider, 
            _resultingException);
    }
}
