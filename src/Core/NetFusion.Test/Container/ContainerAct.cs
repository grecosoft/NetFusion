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
        private AppContainer _container;
        private bool _actedOn = false;
        private Exception _resultingException;
        private IServiceProvider _services;
        private ContainerFixture _fixture;


        public ContainerAct(ContainerFixture fixture)
        {
            _fixture = fixture;
            _container = fixture.ContainerUnderTest;
        }

        public IServiceProvider TestServiceScope => _services;

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
            _fixture.InitContainer();
            try
            {
                await act(_container);
            }
            catch (Exception ex)
            {
                _resultingException = ex;
            }

            return this;
        }

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
                _services = testScope.ServiceProvider;

                await act(_services);
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
        public ContainerAssert Assert => new ContainerAssert(_container, _services, _resultingException);

    }
}
