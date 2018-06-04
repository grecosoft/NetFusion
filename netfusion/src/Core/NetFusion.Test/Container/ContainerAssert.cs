﻿using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Manifests;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions.Collections;
using System;
using System.Linq;

namespace NetFusion.Test.Container
{
    /// <summary>
    /// Object containing method for asserting the test fixture under test that was
    /// acted on.  There are methods for asserting the container and any of its 
    /// related objects created by the bootstrap process.
    /// </summary>
    public class ContainerAssert 
    {
        private readonly AppContainer _container;
        private readonly Exception _resultingException;
        private readonly IServiceProvider _testServiceScope;

        public ContainerAssert(AppContainer container, IServiceProvider serviceProvider, Exception resultingException)
        {
            _container = container;
            _resultingException = resultingException;
            _testServiceScope = serviceProvider;
        }

        public ContainerAssert(ContainerFixture fixture)
        {
            _container = fixture.ContainerUnderTest;
           fixture.InitContainer();
        }

        /// <summary>
        /// Passed reference to the created service provider to be asserted.  The assert method can
        /// create instances of servers to assert their state.
        /// </summary>
        /// <param name="assert">The assert method.</param>
        /// <returns>Self reference.</returns>
        public ContainerAssert Services(Action<IServiceProvider> assert)
        {
            if (assert == null) throw new ArgumentNullException(nameof(assert));
            
            var services = _testServiceScope ?? _container.CreateServiceScope().ServiceProvider;
            assert(services);
      
            return this;
        }

        /// <summary>
        /// Allows the unit-test to assert the state of the created application container.
        /// </summary>
        /// <param name="assert">The method passed an instance of the application container
        /// to be asserted.</param>
        /// <returns>Self reference.</returns>
        public ContainerAssert Container(Action<IAppContainer> assert)
        {
            if (assert == null) throw new ArgumentNullException(nameof(assert), 
                "Assert method not specified.");

            assert(_container);
            return this;
        }

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

        /// <summary>
        /// Allows the unit-test to assert the state of the composite application associated
        /// with the created application container.
        /// </summary>
        /// <param name="assert">The method passed an instance of the composite application
        /// to be asserted.</param>
        /// <returns>Self reference for method chaining.</returns>
        public ContainerAssert CompositeApp(Action<CompositeApplication> assert)
        {
            if (assert == null) throw new ArgumentNullException(nameof(assert), 
                "Assert method not specified.");

            // dig it out...
            assert(((IComposite)_container).Application);
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

            var composite = (IComposite)_container;
            var modules = composite.Application.AllPluginModules
                .OfType<TModule>();

            if (modules.Empty())
            {
                throw new InvalidOperationException(
                    $"Plug-in module of type: {typeof(TModule)} was not found to assert.");
            }
            
            if (modules.Count() > 1)
            {
                throw new InvalidOperationException(
                    $"Multiple Plug-in modules of type: {typeof(TModule)} were found.");
            }

            assert(modules.First());

            return this;
        }

        /// <summary>
        /// Allows the unit-test to assert the sate of a plug-in contained within the created
        /// application container.  
        /// </summary>
        /// <typeparam name="TPlugin">The type of the plug-in manifest used to find the
        /// underlying plug-in object created.</typeparam>
        /// <param name="assert">The method passed an instance of the plug-in to be asserted.</param>
        /// <returns>Self reference for method chaining.</returns>
        public ContainerAssert Plugin<TPlugin>(Action<Plugin> assert)
            where TPlugin : IPluginManifest
        {
            if (assert == null) throw new ArgumentNullException(nameof(assert), 
                "Assert method not specified.");

            // When unit-testing... a plug-in and the manifest are the same thing.
            var composite = (IComposite)_container;
            var plugins = composite.Application.Plugins.Where(p => p.Manifest.GetType() == typeof(TPlugin));

            if (plugins.Count() > 1)
            {
                throw new InvalidOperationException(
                    $"More than one plug-in of the type: {typeof(TPlugin)} as found.");
            }

            var plugin = plugins.FirstOrDefault() ??
                throw new InvalidOperationException($"Plug-in of type: {typeof(TPlugin)} not found.");

            assert(plugin);
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
            where TConfig : IContainerConfig
        {
            if (assert == null) throw new ArgumentNullException(nameof(assert), 
                "Assert method not specified.");

            var composite = (IComposite)_container;

            var config = composite.Application
                .Plugins.SelectMany(p => p.PluginConfigs)
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
                    $"The unit-test Act method resulted in an exception but of the wrong type." +
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
    }
}