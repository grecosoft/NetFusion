using InfrastructureTests.Web.Rest.ClientRequests.Server;
using NetFusion.Rest.Resources.Hal;
using System.Collections.Generic;

namespace InfrastructureTests.Web.Rest.Setup
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
