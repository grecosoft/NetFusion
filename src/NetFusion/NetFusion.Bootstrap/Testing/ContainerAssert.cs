using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common;
using System;
using System.Linq;

namespace NetFusion.Bootstrap.Testing
{
    /// <summary>
    /// Returned after an action is taken on the application container.  
    /// Used to assert the state of the container after an action is
    /// taken.  This class implements the common TDD style of arrange, 
    /// act, and assert.
    /// </summary>
    public class ContainerAssert
    {
        private readonly AppContainer _container;
        private readonly Exception _exception;

        internal ContainerAssert(
            AppContainer container,
            Exception exception)
        {
            _container = container;
            _exception = exception;
        }

        /// <summary>
        /// Asserts the state of the application container.
        /// </summary>
        /// <param name="assert">Delegate that asserts the container by calling
        /// assert methods using an unit-test library.</param>
        public void Assert(Action<AppContainer> assert)
        {
            Check.NotNull(assert, nameof(assert), "assert delegate not specified");

            AssertAction(() => assert(_container));
        }

        /// <summary>
        /// Asserts an expected application container exception.
        /// </summary>
        /// <param name="assert">Delegate that asserts the container and/or
        /// exception by calling assert methods using an unit-test library.</param>
        public void Assert(Action<AppContainer, Exception> assert)
        {
            Check.NotNull(assert, nameof(assert), "assert delegate not specified");

            using (_container)
            {
                assert(_container, _exception);
            }
        }

        /// <summary>
        /// Asserts a specific plug-in module.
        /// </summary>
        /// <typeparam name="T">The module type that should be asserted.</typeparam>
        /// <param name="assert">Delegate that asserts a plug-in module 
        /// of a specific type.</param>
        /// <returns>Reference to instance for method chaining.</returns>
        public void Assert<T>(Action<T> assert)
            where T : class, IPluginModule
        {
            Check.NotNull(assert, nameof(assert), "assert delegate not specified");

            var composite = (IComposite)_container;

            var module = composite.Application.AllPluginModules
                .OfType<T>().FirstOrDefault();

            if (module == null)
            {
                _container.Dispose();

                throw new InvalidOperationException
                    ($"Plug-in module of type: {typeof(T)} was not found to assert.");
            }

            AssertAction(() => assert(module));
        }

        /// <summary>
        /// Asserts a specific application container configuration.
        /// </summary>
        /// <typeparam name="T">The container configuration type that
        /// should be asserted.</typeparam>
        /// <param name="assert">Delegate that asserts a configuration
        /// of a specific type.</param>
        public void AssertConfig<T>(Action<T> assert)
            where T : class, IContainerConfig
        {
            Check.NotNull(assert, nameof(assert), "assert delegate not specified");

            var composite = (IComposite)_container;
            if (composite.Application.Plugins == null)
            {
                throw new InvalidOperationException
                    ("Container has not been built.");
            }

            var config = composite.Application
                .Plugins.SelectMany(p => p.PluginConfigs)
                .OfType<T>().FirstOrDefault();

            if (config == null)
            {
                _container.Dispose();

                throw new InvalidOperationException(
                    $"Plug-in configuration of type: {typeof(T)} was not found to assert.");
            }

            AssertAction(() => assert(config));
        }

        /// <summary>
        /// Executes an assert method that is not passed any application
        /// container associated type instance. This method can reference
        /// the AppContainer singleton instance to assert.
        /// </summary>
        /// <param name="assert"></param>
        public void Assert(Action assert)
        {
            Check.NotNull(assert, nameof(assert), "assert delegate not specified");

            AssertAction(() => assert());
        }

        internal void Assert(Action<CompositeApplication> assert)
        {
            Check.NotNull(assert, nameof(assert), "assert delegate not specified");

            var composite = (IComposite)_container;
            AssertAction(() => assert(composite.Application));
        }

        private void AssertAction(Action assert)
        {
            Check.NotNull(assert, nameof(assert), "assert delegate not specified");

            using (_container)
            {
                if (_exception != null) throw _exception;
                assert();
            }
        }
    }
}
