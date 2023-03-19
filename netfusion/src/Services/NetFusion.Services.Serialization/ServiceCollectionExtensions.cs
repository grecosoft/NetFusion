using System;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Common.Base.Serialization;

namespace NetFusion.Services.Serialization;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds a serialization manager implementation to the dependency-injection service collection. 
    /// </summary>
    /// <param name="services">The service collection to add manager.</param>
    /// <param name="manager">Reference to the serialization manager instance.</param>
    /// <returns>Service Collection.</returns>
    public static IServiceCollection AddSerializationManager(this IServiceCollection services,
        ISerializationManager manager)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));
        if (manager == null) throw new ArgumentNullException(nameof(manager));
        
        return services.AddSingleton(manager);
    }

    /// <summary>
    /// Adds a serialization manager implementation to the dependency-injection service collection.
    /// </summary>
    /// <param name="services">The service collection to add manager.</param>
    /// <param name="manager">Delegate called to allow configuration of the created manager.</param>
    /// <typeparam name="TManager">The type of serialization manager to be created.</typeparam>
    /// <returns>Service Collection.</returns>
    public static IServiceCollection AddSerializationManager<TManager>(this IServiceCollection services,
        Action<TManager>? manager = null)
        where TManager : class, ISerializationManager, new()
    {
        if (services == null) throw new ArgumentNullException(nameof(services));

        var serializationMgr = new TManager();
        manager?.Invoke(serializationMgr);

        return services.AddSingleton<ISerializationManager>(serializationMgr);
    }

    /// <summary>
    /// Adds the default serialization manger implementation to the dependency-injection service collection.
    /// </summary>
    /// <param name="services">The service collection to add default manager.</param>
    /// <param name="manager">Delegate called to allow configuration of the created manager.</param>
    /// <returns>Service Collection.</returns>
    public static IServiceCollection AddDefaultSerializationManager(this IServiceCollection services,
        Action<SerializationManager>? manager = null)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));
        return services.AddSerializationManager(manager);
    }
}