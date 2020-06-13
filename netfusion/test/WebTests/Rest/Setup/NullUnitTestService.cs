using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using NetFusion.Rest.Resources.Hal;
using WebTests.Mocks;
using WebTests.Rest.ClientRequests.Server;


namespace WebTests.Rest.Setup
{
    public class NullUnitTestService : IMockedService
    {
        public IEnumerable<CustomerModel> Customers { get; set; } = new CustomerModel[] { };

        public HalResource ServerReceivedResource { get; set; }

        public bool TriggerServerSideException { get; set; } = false;
        public int ReturnsStatusCode { get; set; } = StatusCodes.Status200OK;

        public IEnumerable<T> GetResources<T>()
        {
            return new T[] { };
        }
    }
}
