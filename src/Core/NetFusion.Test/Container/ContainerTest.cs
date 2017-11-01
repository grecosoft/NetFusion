using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NetFusion.Test.Container
{

    /// <summary>
    /// Allows client to act on an instance of a container.  
    /// </summary>
    public class ContainerTest
    {
        private readonly IAppContainer _container;

        public ContainerTest(IAppContainer container)
        {
            Check.NotNull(container, nameof(container));
            _container = container;
        }

        /// <summary>
        /// Provides an action that is passed the instance of the application container.
        /// </summary>
        /// <param name="act">Delegate provided by the consumer that can act on the
        /// application container.</param>
        /// <returns>Returns an instance of a class that can be used to assert that state
        /// of the application container after the action has been taken.</returns>
        public void Act(Action<IAppContainer> act)
        {
            Check.NotNull(act, nameof(act), "action delegate not specified");

            act(_container);
        }


        //-------------------------------- ASYNCHRONOUS ACT AND ASSERT METHODS ------------------------------------

        /// <summary>
        /// Invokes an asynchronous function then calls the assert method when the task completes.
        /// </summary>
        /// <param name="act">Function that will act on container by calling an asynchronous action.</param>
        /// <param name="assert">Asserts the container when the action completes.</param>
        /// <returns>Task representing a future result.</returns>
        public async Task Test(Func<IAppContainer, Task> act, Action<IAppContainer> assert)
        {
            Check.NotNull(act, nameof(act), "act delegate not specified");
            Check.NotNull(assert, nameof(assert), "assert delegate not specified");

            await act(_container);
            assert(_container);
        }

        /// <summary>
        /// Invokes an asynchronous function then calls the assert method when the task completes.
        /// </summary>
        /// <param name="act">Function that will act on container by calling an asynchronous action.</param>
        /// <param name="assert">Asserts the created composite application when the action completes.</param>
        /// <returns>Task representing a future result.</returns>
        public async Task Test(Func<IAppContainer, Task> act, Action<IComposite> assert)
        {
            Check.NotNull(act, nameof(act), "act delegate not specified");
            Check.NotNull(assert, nameof(assert), "assert delegate not specified");

            await act(_container);
            assert((IComposite)_container);
        }

        /// <summary>
        /// Invokes an asynchronous function then calls the assert method when the task completes.
        /// </summary>
        /// <param name="act">Function that will act on container by calling an asynchronous action.</param>
        /// <param name="assert">Asserts the created composite application when the action completes.</param>
        /// <returns>Task representing a future result.</returns>
        public async Task Test(Func<IAppContainer, Task> act, Action<CompositeApplication> assert)
        {
            Check.NotNull(act, nameof(act), "act delegate not specified");
            Check.NotNull(assert, nameof(assert), "assert delegate not specified");

            await act(_container);
            var composite = (IComposite)_container;
            assert(composite.Application);
        }

        /// <summary>
        /// Invokes an asynchronous function then calls the assert method when the task completes.
        /// </summary>
        /// <param name="act">Function that will act on container by calling an asynchronous action.</param>
        /// <param name="assert">Asserts the action resulted in an expected exception.</param>
        /// <returns>Task representing a future result.</returns>
        public async Task Test(Func<IAppContainer, Task> act, Action<IAppContainer, Exception> assert)
        {
            Check.NotNull(act, nameof(act), "act delegate not specified");
            Check.NotNull(assert, nameof(assert), "assert delegate not specified");

            try
            {
                await act(_container);
                throw new InvalidOperationException("Test expected excetion.  Exception was not thrown.");
            }
            catch(Exception ex)
            {
                assert(_container, ex);
            }
        }

        /// <summary>
        /// Invokes an asynchronous function then calls the assert method when the task completes.
        /// </summary>
        /// <typeparam name="T">The specific type of plug-in module to assert.</typeparam>
        /// <param name="act">Function that will act on container by calling an asynchronous action.</param>
        /// <param name="assert">Asserts the state of a given plug-in module.</param>
        /// <returns>Task representing a future result.</returns>
        public async Task Test<T>(Func<IAppContainer, Task> act, Action<T> assert)
            where T : IPluginModule
        {
            Check.NotNull(act, nameof(act), "act delegate not specified");
            Check.NotNull(assert, nameof(assert), "assert delegate not specified");

            await act(_container);

            AssertPluginModule(assert);
        }

        /// <summary>
        /// Invokes an asynchronous function then calls the assert method when the task completes.
        /// </summary>
        /// <typeparam name="T">The specific type of container configuration.</typeparam>
        /// <param name="act">Function that will act on container by calling an asynchronous action.</param>
        /// <param name="assert">Asserts the state of a given container configuration.</param>
        /// <returns>Task representing a future result.</returns>
        public async Task TestConfig<T>(Func<IAppContainer, Task> act, Action<T> assert)
            where T : IContainerConfig
        {
            Check.NotNull(act, nameof(act), "act delegate not specified");
            Check.NotNull(assert, nameof(assert), "assert delegate not specified");

            await act(_container);

            AssertConfig<T>(assert);
        }


        //--------------------------------  ACT AND ASSERT METHODS ------------------------------------

        /// <summary>
        /// Invokes function then calls the assert method when the task completes.
        /// </summary>
        /// <param name="act">Function that will act on container by calling an asynchronous action.</param>
        /// <param name="assert">Asserts the container when the action completes.</param>
        public void Test(Action<IAppContainer> act, Action<IAppContainer> assert)
        {
            Check.NotNull(act, nameof(act), "act delegate not specified");
            Check.NotNull(assert, nameof(assert), "assert delegate not specified");

            act(_container);
            assert(_container);
        }

        /// <summary>
        /// Invokes function then calls the assert method when the task completes.
        /// </summary>
        /// <param name="act">Function that will act on container by calling an asynchronous action.</param>
        /// <param name="assert">Asserts the created composite application when the action completes.</param>
        public void Test(Action<IAppContainer> act, Action<IComposite> assert)
        {
            Check.NotNull(act, nameof(act), "act delegate not specified");
            Check.NotNull(assert, nameof(assert), "assert delegate not specified");

            act(_container);
            assert((IComposite)_container);
        }

        /// <summary>
        /// Invokes function then calls the assert method when the task completes.
        /// </summary>
        /// <param name="act">Function that will act on container by calling an asynchronous action.</param>
        /// <param name="assert">Asserts the created composite application when the action completes.</param>
        public void Test(Action<IAppContainer> act, Action<CompositeApplication> assert)
        {
            Check.NotNull(act, nameof(act), "act delegate not specified");
            Check.NotNull(assert, nameof(assert), "assert delegate not specified");

            act(_container);
            var composite = (IComposite)_container;
            assert(composite.Application);
        }

        /// <summary>
        /// Invokes function then calls the assert method when the task completes.
        /// </summary>
        /// <param name="act">Function that will act on container by calling an asynchronous action.</param>
        /// <param name="assert">Asserts the action resulted in an expected exception.</param>
        public void Test(Action<IAppContainer> act, Action<IAppContainer, Exception> assert)
        {
            Check.NotNull(act, nameof(act), "act delegate not specified");
            Check.NotNull(assert, nameof(assert), "assert delegate not specified");

            try
            {
                act(_container);
                throw new InvalidOperationException("Test expected excetion.  Exception was not thrown.");
            }
            catch (Exception ex)
            {
                assert(_container, ex);
            }
        }
        /// <summary>
        /// Invokes function then calls the assert method when the task completes.
        /// </summary>
        /// <typeparam name="T">The specific type of plug-in module to assert.</typeparam>
        /// <param name="act">Function that will act on container by calling an asynchronous action.</param>
        /// <param name="assert">Asserts the state of a given plug-in module.</param>
        public void Test<T>(Action<IAppContainer> act, Action<T> assert)
            where T : IPluginModule
        {
            Check.NotNull(act, nameof(act), "act delegate not specified");
            Check.NotNull(assert, nameof(assert), "assert delegate not specified");

            act(_container);

            AssertPluginModule<T>(assert);
        }

        /// <summary>
        /// Invokes function then calls the assert method when the task completes.
        /// </summary>
        /// <typeparam name="T">The specific type of container configuration.</typeparam>
        /// <param name="act">Function that will act on container by calling an asynchronous action.</param>
        /// <param name="assert">Asserts the state of a given container configuration.</param>
        public void TestConfig<T>(Action<IAppContainer> act, Action<T> assert)
            where T : IContainerConfig
        {
            Check.NotNull(act, nameof(act), "act delegate not specified");
            Check.NotNull(assert, nameof(assert), "assert delegate not specified");

            act(_container);

            AssertConfig(assert);
        }

        //--------------------------------  COMMON ASSERT METHODS ------------------------------------

        private void AssertPluginModule<T>(Action<T> assert)
            where T : IPluginModule
        {
            var composite = (IComposite)_container;
            var module = composite.Application.AllPluginModules
                .OfType<T>().FirstOrDefault();

            if (module == null)
            {
                throw new InvalidOperationException
                    ($"Plug-in module of type: {typeof(T)} was not found to assert.");
            }

            assert(module);
        }

        private void AssertConfig<T>(Action<T> assert)
            where T : IContainerConfig
        {
            var composite = (IComposite)_container;
            if (composite.Application.Plugins == null)
            {
                throw new InvalidOperationException("Container has not been built.");
            }

            var config = composite.Application
                .Plugins.SelectMany(p => p.PluginConfigs)
                .OfType<T>().FirstOrDefault();

            if (config == null)
            {
                throw new InvalidOperationException(
                    $"Plug-in configuration of type: {typeof(T)} was not found to assert.");
            }

            assert(config);
        }
    }
}
