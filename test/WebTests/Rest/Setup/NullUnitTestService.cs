using InfrastructureTests.Web.Rest.ClientRequests.Server;
using Microsoft.AspNetCore.Http;
using NetFusion.Rest.Resources.Hal;
using System.Collections.Generic;

namespace InfrastructureTests.Web.Rest.Setup
{
    public class NullUnitTestService : IMockedService
    {
        public IEnumerable<CustomerResource> Customers { get; set; } = new CustomerResource[] { };

        public HalResource ServerReceivedResource { get; set; }

        public bool TriggerServerSideException { get; set; } = false;
        public int ReturnsStatusCode { get; set; } = StatusCodes.Status200OK;

        public IEnumerable<T> GetResources<T>()
        {
            return new T[] { };
        }
    }
}
