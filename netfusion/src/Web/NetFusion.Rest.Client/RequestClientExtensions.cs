using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Rest.Client.Core;
using NetFusion.Rest.Client.Settings;
using NetFusion.Rest.Common;

namespace NetFusion.Rest.Client
{
    public static class RequestClientExtensions
    {
        private static JsonSerializerOptions _defaultOptions;
        
        static RequestClientExtensions()
        {
            _defaultOptions = new JsonSerializerOptions
            {
                IgnoreNullValues = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public static IServiceCollection AddRequestFactory(this IServiceCollection services)
        {
            services.AddSingleton<IRequestClientFactory, RequestClientFactory>();
            return services;
        }
        
        public static IServiceCollection AddDefaultMediaSerializers(this IServiceCollection services,
            JsonSerializerOptions options = null)
        {
            options ??= _defaultOptions;
            
            services.AddSingleton<IMediaTypeSerializer>(_ => new JsonMediaTypeSerializer(options));
            services.AddSingleton<IMediaTypeSerializer>(_ => new JsonMediaTypeSerializer(InternetMediaTypes.HalJson, options));

            return services;
        }
    }
}