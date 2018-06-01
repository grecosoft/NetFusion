using NetFusion.Rest.Common;
using System;

namespace NetFusion.Rest.Client.Settings
{
    /// <summary>
    /// Extension methods used to configure a IRequestSettings instance
    /// with common predefined configurations.
    /// </summary>
    public static class RequestSettingsExtensions
    {
        /// <summary>
        /// Applies the settings commonly used when submitting and consuming
        /// REST/HAL based requests.
        /// </summary>
        /// <param name="requestSettings">The request settings to configure.</param>
        /// <returns>Reference to the settings being configured.</returns>
        public static IRequestSettings UseHalDefaults(this IRequestSettings requestSettings)
        {
            if (requestSettings == null) throw new ArgumentNullException(nameof(requestSettings),
                "Request Settings cannot be null.");

            requestSettings.Headers.AcceptMediaType(InternetMediaTypes.HalJson);
            requestSettings.Headers.AcceptMediaType(InternetMediaTypes.Json);

            // If the settings content-type is not specified assume JSON serialized
            // content will be submitted to the server.
            if (requestSettings.Headers.ContentType == null)
            {
                requestSettings.Headers.ContentMediaType(InternetMediaTypes.Json);
            }
            return requestSettings;
        }
    }
}
