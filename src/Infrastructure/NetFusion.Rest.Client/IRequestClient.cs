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
        /// Sends an API request to the server.
        /// </summary>
        /// <param name="request">The request to send.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Future response for sent request.</returns>
        Task<ApiResponse> Send(ApiRequest request,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        ///  Sends an API request to the server for a response containing content.
        /// </summary>
        /// <typeparam name="TContent">The type of the content of the response.</typeparam>
        /// <param name="request">The request to send.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Future response for sent request containing returned content.</returns>
        Task<ApiResponse<TContent>> Send<TContent>(ApiRequest request,
            CancellationToken cancellationToken = default(CancellationToken))
            where TContent : class;


    }
}
