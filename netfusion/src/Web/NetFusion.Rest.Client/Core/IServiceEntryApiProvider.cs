﻿using System.Threading.Tasks;
using NetFusion.Rest.Resources;
using NetFusion.Rest.Resources.Hal;

namespace NetFusion.Rest.Client.Core
{
    /// <summary>
    /// Implemented by the RequestClientBuilder and passed to the
    /// built IRequestClient instance to provide a callback used
    /// to load the request client's associated entry resource.
    /// </summary>
    public interface IServiceEntryApiProvider
    {
        /// <summary>
        /// Returns the associated service api entry resource.  The same
        /// instance is returned after the first successful load.
        /// </summary>
        /// <returns>Entry point resource.</returns>
        Task<HalResource<TEntry>> GetEntryPointResource<TEntry>()
            where TEntry : EntryPointModel;
    }
}
