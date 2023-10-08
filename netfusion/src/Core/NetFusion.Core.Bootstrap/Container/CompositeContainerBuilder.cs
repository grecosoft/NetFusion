using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetFusion.Common.Base;
using NetFusion.Common.Base.Logging;
using NetFusion.Common.Base.Scripting;
using NetFusion.Common.Base.Validation;
using NetFusion.Core.Bootstrap.Plugins;

namespace NetFusion.Core.Bootstrap.Container;

/// <summary>
/// Provides an implementation used by the host application to build a composite application
/// from a set of registered plugins.
///
/// https://github.com/grecosoft/NetFusion/wiki/core-bootstrap-overview
/// </summary>
internal class CompositeContainerBuilder : ICompositeContainerBuilder
{
    private readonly IServiceCollection _serviceCollection;
    private readonly ITypeResolver _typeResolver;

    private readonly CompositeContainer _container;
        
    public CompositeContainerBuilder(IServiceCollection serviceCollection,
        IConfiguration configuration,
        ITypeResolver typeResolver,
        IExtendedLogger extendedLogger)
    {
        if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            
        _serviceCollection = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));
        _typeResolver = typeResolver ?? throw new ArgumentNullException(nameof(typeResolver));

        // Set property on static class referenced to write logs before .NET's ILogger can be referenced.
        NfExtensions.Logger = extendedLogger ?? throw new ArgumentNullException(nameof(extendedLogger));

        _container = new CompositeContainer(serviceCollection, configuration);
    }
        
    public ICompositeContainerBuilder AddPlugin<TPlugin>() where TPlugin : IPlugin, new()
    {
        _container.RegisterPlugin<TPlugin>();
        return this;
    }

    // This override is used by unit tests.
    public ICompositeContainerBuilder AddPlugin(params IPlugin[] plugin)
    {
        _container.RegisterPlugins(plugin);
        return this;
    }

    public ICompositeContainerBuilder InitPluginConfig<T>(Action<T> configure) where T : IPluginConfig
    {
        if (configure == null) throw new ArgumentNullException(nameof(configure));

        T config = _container.GetPluginConfig<T>();
        configure(config);

        return this;
    }

    // Populates the IServiceCollection with services registered by all plugin-modules.
    // The end result is a populated service-collection with a registered ICompositeApp
    // instance that can be started and used for the lifetime of the host.  
    public void Compose(Action<IServiceCollection>? services = null)
    {
        try
        {
            RegisterRequiredDefaultServices();

            _container.Compose(_typeResolver);
                
            // Allow the host initialization code to specify any last service overrides.
            services?.Invoke(_serviceCollection);
        }
        catch(Exception ex)
        {
            NfExtensions.Logger.LogError<CompositeContainerBuilder>(ex, "Error building Composite Container");
            throw;
        }
    }
        
    private void RegisterRequiredDefaultServices()
    {
        _serviceCollection.AddSingleton<ILoggerFactory, LoggerFactory>();
            
        // These services can be overridden by the host:
        _serviceCollection.AddSingleton<IValidationService, ValidationService>();
        _serviceCollection.AddSingleton<IEntityScriptingService, NullEntityScriptingService>();
    }
}