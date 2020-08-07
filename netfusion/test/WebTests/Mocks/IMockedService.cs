using System.Collections.Generic;
using NetFusion.Rest.Resources;

namespace WebTests.Mocks
{
    public interface IMockedService
    {
        HalResource ServerReceivedResource { get; set; }

        IEnumerable<T> GetResources<T>();

        bool TriggerServerSideException { get; set; }

        int ReturnsStatusCode { get; set; }
    }
}
