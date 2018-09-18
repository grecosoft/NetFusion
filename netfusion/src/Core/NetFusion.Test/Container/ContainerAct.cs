using NetFusion.Bootstrap.Container;
using System;
using System.Threading.Tasks;

namespace NetFusion.Test.Container
{
    /// <summary>
    /// Returns object used to act on the created and arranged application container.
    /// After the container is acted on, the unit-test uses the Assert property to 
    /// test the state of the container or one of its related objects.
    /// </summary>
    public class ContainerAct
    {
        private readonly AppContainer _container;
        private readonly ContainerFixture _fixture;
        private bool _actedOn;
        private Exception _resultingException;

        public ContainerAct(ContainerFixture fixture)
        {
            _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
            _container = fixture.ContainerUnderTest;
        }

        public IServiceProvider TestServiceScope { get; private set; }

        /// <summary>
        /// Allows an unit-test to act on the application container under test.
        /// </summary>
        /// <param name="act">Method passed the instance of the container under test to be
        /// acted on by the unit-test.</param>
        /// <returns>Self reference for method chaining.</returns>
        public ContainerAct OnContainer(Action<IAppContainer> act)
        {
            if (_actedOn)
            {
                throw new InvalidOperationException("The container can only be acted on once.");
            }

            _actedOn = true;
            try
            {
                _fixture.InitContainer();
                act(_container);
            }
            catch (Exception ex)
            {
                _resultingException = ex;
            }

            return this;
        }

        /// <summary>
        /// Builds and starts the container.  This can be used when testing an expected exception
        /// thrown when the container is built and/or started.
        /// </summary>
        /// <returns>Self reference.</returns>
        public ContainerAct BuildAndStartContainer()
        {
            if (_actedOn)
            {
                throw new InvalidOperationException("The container can only be acted on once.");
            }

            _actedOn = true;
            try
            {
                _fixture.InitContainer();
            }
            catch (Exception ex)
            {
                _resultingException = ex;
            }

            return this;
        }

        /// <summary>
        /// Allows an application container that has not been built or started to be acted on.
        /// </summary>
        /// <param name="act">The method passed to act on the created container.</param>
        /// <returns>Self reference.</returns>
        public ContainerAct OnNonInitContainer(Action<IAppContainer> act)
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
            }

            return this;
        }

        /// <summary>
        /// Allows an unit-test to act on the application container under test.
        /// </summary>
        /// <param name="act">Method passed the instance of the container under test to be
        /// acted on by the unit-test.  The method can invoke an asynchronous method.</param>
        /// <returns>Self reference for method chaining.</returns>
        public async Task<ContainerAct> OnContainer(Func<IAppContainer, Task> act)
        {
            if (_actedOn)
            {
                throw new InvalidOperationException("The container can only be acted on once.");
            }

            _actedOn = true;
            try
            {
                _fixture.InitContainer();
                await act(_container).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _resultingException = ex;
            }

            return this;
        }

        /// <summary>
        /// Allows an unit-test to act on the server provider created from the bootstrapped
        /// application container.
        /// </summary>
        /// <param name="act">Method called to act on the service provider.</param>
        /// <returns>Self reference for method chaining.</returns>
        public async Task<ContainerAct> OnServices(Func<IServiceProvider, Task> act)
        {
            if (_actedOn)
            {
                throw new InvalidOperationException("The container can only be acted on once.");
            }

            _actedOn = true;
            try
            {
                _fixture.InitContainer();

                var testScope = _container.CreateServiceScope();
                TestServiceScope = testScope.ServiceProvider;

                await act(TestServiceScope).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _resultingException = ex;
            }

            return this;
        }
        
        public ContainerAct OnServices(Action<IServiceProvider> act)
        {
            if (_actedOn)
            {
                throw new InvalidOperationException("The container can only be acted on once.");
            }

            _actedOn = true;
            try
            {
                _fixture.InitContainer();

                var testScope = _container.CreateServiceScope();
                TestServiceScope = testScope.ServiceProvider;

                act(TestServiceScope);
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
        public ContainerAssert Assert => new ContainerAssert(_container, TestServiceScope, _resultingException);
    }
}
