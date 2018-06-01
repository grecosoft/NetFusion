using System;

namespace NetFusion.Rest.Client.Core
{
    /// <summary>
    /// Class that wraps an instance of a IRequestClient and can be stored withing the
    /// services collection for a specific derived IRequestClientService interface used
    /// to represent a specific service API base address.
    /// </summary> 
    public abstract class RequestClientService : IRequestClientService
    {
        public IRequestClient Client { get; }
        
        /// <summary>
        /// Wraps instance of generic IRequestClient.
        /// </summary>
        /// <param name="requestClient">The request client to wrap.</param>
        protected RequestClientService(IRequestClient requestClient)
        {
            Client = requestClient ?? throw new ArgumentNullException(nameof(requestClient));
        }
    }
}