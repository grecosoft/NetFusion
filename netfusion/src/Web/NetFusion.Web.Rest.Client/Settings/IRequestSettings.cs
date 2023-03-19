using System.Net.Http;

namespace NetFusion.Web.Rest.Client.Settings;

/// <summary>
/// Represents settings to be applied to sent HTTP requests.
/// </summary>
public interface IRequestSettings
{
    /// <summary>
    /// Headers to be added to the request message.
    /// </summary>
    RequestHeaders Headers { get; }

    /// <summary>
    /// Query string values to be added to the request URI.
    /// </summary>
    QueryString QueryString { get; }

    /// <summary>
    /// Applies the settings to the provided request message.
    /// </summary>
    /// <param name="requestMessage">Request message to apply settings.</param>
    void Apply(HttpRequestMessage requestMessage);

    /// <summary>
    /// Returns a new settings instance with all provided request settings
    /// merged into a new instance of the current.
    /// </summary>
    /// <returns>New merged request settings instance.</returns>
    /// <param name="requestSettings">The settings to merge.</param>
    IRequestSettings GetMerged(IRequestSettings requestSettings);
}