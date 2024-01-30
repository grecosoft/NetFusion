using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using NetFusion.Web.Rest.Resources;
using NetFusion.Web.UnitTests.Mocks;
using NetFusion.Web.UnitTests.Rest.ClientRequests.Server;

namespace NetFusion.Web.UnitTests.Rest.Setup;

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


    public IEnumerable<CustomerModel> Customers { get; set; } = Array.Empty<CustomerModel>();
}