using System;
using NetFusion.Rest.Client.Resources;
using System.Threading;
using System.Threading.Tasks;

namespace NetFusion.Rest.Client
{
    /// <summary>
    /// Contract for a client used to make REST based Web API requests.
    /// </summary>
    public interface IRequestClient
    {
        /// <summary>
        /// Return the service API entry resource associated with the client.
        /// If the client was built without specifying the entry point address,
        /// an exception is thrown.
        /// </summary>
        /// <returns>The entry point response containing the root API of the service.</returns>
        Task<HalEntryPointResource> GetApiEntry();

        /// <summary>
        /// Sends an API request to the server.
        /// </summary>
        /// <param name="request">The request to send.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Future response for sent request.</returns>
        Task<ApiResponse> SendAsync(ApiRequest request,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        ///  Sends an API request to the server for a response containing content.
        /// </summary>
        /// <typeparam name="TContent">The type of the content of the response.</typeparam>
        /// <param name="request">The request to send.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Future response for sent request containing returned content.</returns>
        Task<ApiResponse<TContent>> SendAsync<TContent>(ApiRequest request,
            CancellationToken cancellationToken = default(CancellationToken))
            where TContent : class;

        /// <summary>
        ///  Sends an API request to the server for a response containing content.
        /// </summary>
        /// <param name="request">The request to send.</param>
        /// <param name="contentType">The type of the content of the response.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Future response for sent request containing returned content.</returns>
        Task<ApiResponse> SendAsync(ApiRequest request, Type contentType,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
