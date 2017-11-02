using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Test.Plugins;
using System;
using System.Linq;

namespace NetFusion.Test.Container
{
    public class ContainerAssert : IDisposable
    {
        private AppContainer _container;
        private Exception _resultingException;

        public ContainerAssert(AppContainer container, Exception resultingException)
        {
            _container = container;
            _resultingException = resultingException;
        }

        public ContainerAssert Container(Action<IAppContainer> assert)
        {
            assert(_container);
            return this;
        }

        public ContainerAssert CompositeApp(Action<CompositeApplication> assert)
        {
            assert(_container.Application);
            return this;
        }

        public ContainerAssert PluginModule<TModule>(Action<TModule> assert)
            where TModule : IPluginModule
        {
            var composite = (IComposite)_container;
            var module = composite.Application.AllPluginModules
                .OfType<TModule>().FirstOrDefault();

            if (module == null)
            {
                throw new InvalidOperationException
                    ($"Plug-in module of type: {typeof(TModule)} was not found to assert.");
            }

            assert(module);

            return this;
        }

        public ContainerAssert Plugin<TPlugin>(Action<Plugin> assert)
            where TPlugin : MockPlugin
        {
            var composite = (IComposite)_container;
            var plugins = composite.Plugins.Where(p => p.Manifest.GetType() == typeof(TPlugin));

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

        public ContainerAssert Configuration<TConfig>(Action<TConfig> assert)
            where TConfig : IContainerConfig
        {
            var composite = (IComposite)_container;
            if (composite.Application.Plugins == null)
            {
                throw new InvalidOperationException("Container has not been built.");
            }

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

        public ContainerAssert Exception<TEx>(Action<TEx> assert)
            where TEx : Exception
        {
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

        public ContainerAssert Exception<TEx>(Action<Exception> assert)
        {
            if (_resultingException == null)
            {
                throw new InvalidOperationException(
                    "The unit-test Act method didn't result in an exception.");
            }

            assert(_resultingException);
            return this;
        }

        public void Dispose()
        {
            _container?.Dispose();
        }
    }
}
