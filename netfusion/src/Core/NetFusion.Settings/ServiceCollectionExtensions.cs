using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
// ReSharper disable All

namespace NetFusion.Settings
{
    /// <summary>
    /// Additional configuration service collection extensions.  These extension methods provide non-generic typed
    /// versions of the corresponding methods defined by Microsoft called to automatically add options. 
    /// The generic version can be found here:
    ///
    /// https://github.com/aspnet/Options/blob/master/src/Microsoft.Extensions.Options.ConfigurationExtensions/OptionsConfigurationServiceCollectionExtensions.cs
    /// https://github.com/aspnet/Options/blob/master/src/Microsoft.Extensions.Options/Options.cs
    /// 
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection Configure(this IServiceCollection services, 
            Type optionType, 
            IConfiguration config)
        {
            return Configure(services, optionType, string.Empty, config);
        }

        public static IServiceCollection Configure(this IServiceCollection services, 
            Type optionType, string name, 
            IConfiguration config)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (config == null) throw new ArgumentNullException(nameof(config));
  
            Type serviceType = typeof(IOptionsChangeTokenSource<>).MakeGenericType(optionType);

            object implementationType = Activator.CreateInstance(
                typeof(ConfigurationChangeTokenSource<>).MakeGenericType(optionType),
                name,
                config);

            services.Add(new ServiceDescriptor(serviceType, implementationType));

            serviceType = typeof(IConfigureOptions<>).MakeGenericType(optionType);

            implementationType = Activator.CreateInstance(
                typeof(NamedConfigureFromConfigurationOptions<>).MakeGenericType(optionType),
                name,
                config);

            services.Add(new ServiceDescriptor(serviceType, implementationType));
            return services;
        }
    }
}
