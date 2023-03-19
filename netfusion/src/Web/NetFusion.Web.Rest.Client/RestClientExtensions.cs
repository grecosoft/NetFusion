using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Web.Common;
using NetFusion.Web.Rest.Client.Core;

namespace NetFusion.Web.Rest.Client;

public static class RestClientExtensions
{
    private static readonly JsonSerializerOptions DefaultOptions;
        
    static RestClientExtensions()
    {
        DefaultOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    /// <summary>
    /// Called to register the IRestClientFactory with the service-collection.
    /// </summary>
    /// <param name="services">The service collection being populated.</param>
    /// <returns>Service collection.</returns>
    public static IServiceCollection AddRestClientFactory(this IServiceCollection services)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));
            
        // This service is registered as singleton since Microsoft's underlying IHttpClientFactory
        // to which it delegates is also registered a singleton.
        services.AddSingleton<IRestClientFactory, RestClientFactory>();
        services.AddSingleton<IRestClientService, RestClientService>();
        return services;
    }
        
    /// <summary>
    /// Adds IMediaTypeSerializer instances to the service collection for the most
    /// commonly used media-types.  Added are Json serialization for both the json
    /// and json+hal media-types.
    /// </summary>
    /// <param name="services">The service collection being populated.</param>
    /// <param name="options">The optional options used to configure JSON serializers.</param>
    /// <returns>Service Collection</returns>
    public static IServiceCollection AddDefaultMediaSerializers(this IServiceCollection services,
        JsonSerializerOptions options = null)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));
            
        options ??= DefaultOptions;
            
        services.AddSingleton<IMediaTypeSerializer>(_ => new JsonMediaTypeSerializer(options));
        services.AddSingleton<IMediaTypeSerializer>(_ => new JsonMediaTypeSerializer(InternetMediaTypes.HalJson, options));

        return services;
    }

    /// <summary>
    /// Registers a IMediaTypeSerializer implementation.
    /// </summary>
    /// <param name="services">The service collection being populated.</param>
    /// <typeparam name="TSerializer">The serializer implementation.</typeparam>
    /// <returns>Service Collection.</returns>
    public static IServiceCollection AddMediaSerializer<TSerializer>(this IServiceCollection services)
        where TSerializer : class, IMediaTypeSerializer
    {
        if (services == null) throw new ArgumentNullException(nameof(services));
            
        services.AddSingleton<IMediaTypeSerializer, TSerializer>();
        return services;
    }

    /// <summary>
    /// Registers an instance of an object implementing IMediaTypeSerializer.
    /// </summary>
    /// <param name="services">The service collection being populated.</param>
    /// <param name="serializer">The serializer implementation.</param>
    /// <returns>Service Collection.</returns>
    public static IServiceCollection AddMediaSerializer(this IServiceCollection services,
        IMediaTypeSerializer serializer)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));
        if (serializer == null) throw new ArgumentNullException(nameof(serializer));

        services.AddSingleton(serializer);
        return services;
    }
}