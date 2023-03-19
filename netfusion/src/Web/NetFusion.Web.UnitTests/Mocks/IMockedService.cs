using System.Collections.Generic;
using NetFusion.Web.Rest.Resources;

namespace NetFusion.Web.UnitTests.Mocks;

public interface IMockedService
{
    HalResource ServerReceivedResource { get; set; }

    IEnumerable<T> GetResources<T>();

    bool TriggerServerSideException { get; set; }

    int ReturnsStatusCode { get; set; }
}