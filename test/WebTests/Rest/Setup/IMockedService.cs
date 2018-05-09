using System.Collections.Generic;
using NetFusion.Rest.Resources.Hal;
using WebTests.Rest.ClientRequests.Server;

namespace WebTests.Rest.Setup
{
    public interface IMockedService
    {
        HalResource ServerReceivedResource { get; set; }

        IEnumerable<CustomerResource> Customers { get; set; }

        IEnumerable<T> GetResources<T>();

        bool TriggerServerSideException { get; set; }

        int ReturnsStatusCode { get; set; }
    }
}
