using System;
using System.Net.Http;

namespace NetFusion.Rest.Client.Settings
{
    /// <summary>
    /// Used to configure settings to be applied to sent requests.
    /// </summary>
    public class RequestSettings : IRequestSettings
    {
        public RequestHeaders Headers { get; private set; }
        public QueryString QueryString { get; private set; }

        private RequestSettings() {}

        public bool? SuppressStatusCodeHandlers { get; set; }
       
        /// <summary>
        /// Creates a new request settings instance.
        /// </summary>
        /// <returns>New settings instance.</returns>
        /// <param name="config">Configuration factory method used to configure settings.</param>
        public static IRequestSettings Create(Action<IRequestSettings> config = null) 
        {
            var requestSettings = new RequestSettings
            {
                Headers = new RequestHeaders(),
                QueryString = new QueryString()
            };

            config?.Invoke(requestSettings);
            return requestSettings;
        }

        public void Apply(HttpRequestMessage requestMessage)
        {
            if (requestMessage == null) throw new ArgumentNullException(nameof(requestMessage),
                "Request message cannot be null.");

            Headers.ApplyHeaders(requestMessage);
        }

        public IRequestSettings GetMerged(IRequestSettings requestSettings)
        {
            if (requestSettings != null)
            {
                return new RequestSettings
                {
                    Headers = Headers.GetMergedHeaders(requestSettings.Headers),
                    QueryString = QueryString.GetMerged(requestSettings.QueryString),
                    SuppressStatusCodeHandlers = requestSettings.SuppressStatusCodeHandlers ?? SuppressStatusCodeHandlers ?? false
                };
            }

            return new RequestSettings
            {
                Headers = Headers,
                QueryString = QueryString,
                SuppressStatusCodeHandlers = SuppressStatusCodeHandlers ?? false
            };                      
        }
    }
}
