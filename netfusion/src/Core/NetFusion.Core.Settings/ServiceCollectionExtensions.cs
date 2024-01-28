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
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(config);

            Type serviceType = typeof(IOptionsChangeTokenSource<>).MakeGenericType(optionType);
            Type implementationType = typeof(ConfigurationChangeTokenSource<>).MakeGenericType(optionType);

            object? instance = Activator.CreateInstance(implementationType, name, config);
            if (instance == null)
            {
                throw new InvalidOperationException(
                    $"Instance of type: {implementationType.FullName} could not be created.");
            }

            services.Add(new ServiceDescriptor(serviceType, instance));

            serviceType = typeof(IConfigureOptions<>).MakeGenericType(optionType);
            implementationType = typeof(NamedConfigureFromConfigurationOptions<>).MakeGenericType(optionType);
            
            instance = Activator.CreateInstance(implementationType, name, config);
            if (instance == null)
            {
                throw new InvalidOperationException(
                    $"Instance of type: {implementationType.FullName} could not be created.");
            }

            services.Add(new ServiceDescriptor(serviceType, instance));
            return services;
        }
    }
}
