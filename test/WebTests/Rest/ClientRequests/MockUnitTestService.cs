using InfrastructureTests.Web.Rest.ClientRequests.Server;
using InfrastructureTests.Web.Rest.Setup;
using Microsoft.AspNetCore.Http;
using NetFusion.Rest.Resources.Hal;
using System.Collections.Generic;
using System.Linq;

namespace InfrastructureTests.Web.Rest.ClientRequests
{
    public class MockUnitTestService : IMockedService
    {
        public IEnumerable<object> ServerResources { get; set; }
        public HalResource ServerReceivedResource { get; set; }

        public IEnumerable<T> GetResources<T>()
        {
            return ServerResources.OfType<T>();
        }

        public bool TriggerServerSideException { get; set; } = false;
        public int ReturnsStatusCode { get; set; } = StatusCodes.Status200OK;


        public MockUnitTestService()
        {
            Customers = new CustomerResource[] { };
        }

        public IEnumerable<CustomerResource> Customers { get; set; }
    }
}
