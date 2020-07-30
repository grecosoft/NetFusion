using System;
using System.Threading;
using System.Threading.Tasks;

namespace NetFusion.Rest.Client
{
    /// <summary>
    /// Contract for a client used to make REST based Web API requests.
    /// </summary>
    public interface IRestClient
    {
        /// <summary>
        /// Sends an API request to the server and does not attempt to deserialize the response.
        /// This method can be used if the  Api does not return a response body or if the caller
        /// wants to explicitly handler the body of the response.
        /// </summary>
        /// <param name="request">The request to send.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Future response for sent request.</returns>
        Task<ApiResponse> SendAsync(ApiRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends an API request to the server and deserializes the response body to the type specified.
        /// </summary>
        /// <param name="request">The request to send.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <typeparam name="TContent">The type into which the response body is to be deserialized.</typeparam>
        /// <returns>Response containing the typed response content.</returns>
        Task<ApiResponse<TContent>> SendForContentAsync<TContent>(ApiRequest request,
            CancellationToken cancellationToken = default)
            where TContent : class;

        /// <summary>
        /// Sends an API request to the server for a response containing content conforming
        /// to the HAL specification. 
        /// </summary>
        /// <typeparam name="TModel">The type of the content of the response.</typeparam>
        /// <param name="request">The request to send.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Future response for sent request containing returned HAL based content.</returns>
        Task<ApiHalResponse<TModel>> SendForHalAsync<TModel>(ApiRequest request,
            CancellationToken cancellationToken = default)
            where TModel : class;

        /// <summary>
        /// Sends an API request to the server for a response containing content.
        /// The response body is deserialized into the specified content type but
        /// returned as the object type and accessed using the State property. 
        /// </summary>
        /// <param name="request">The request to send.</param>
        /// <param name="contentType">The type of the content of the response.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Future response for sent request containing returned content.</returns>
        Task<ApiResponse> SendAsync(ApiRequest request, Type contentType,
            CancellationToken cancellationToken = default);
    }
}
