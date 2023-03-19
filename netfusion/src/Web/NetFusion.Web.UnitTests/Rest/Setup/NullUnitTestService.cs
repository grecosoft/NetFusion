using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using NetFusion.Web.Rest.Resources;
using NetFusion.Web.UnitTests.Mocks;
using NetFusion.Web.UnitTests.Rest.ClientRequests.Server;

namespace NetFusion.Web.UnitTests.Rest.Setup;

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