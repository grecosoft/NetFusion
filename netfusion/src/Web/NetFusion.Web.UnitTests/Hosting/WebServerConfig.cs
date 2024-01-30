using System;
using Microsoft.AspNetCore.TestHost;

namespace NetFusion.Web.UnitTests.Hosting;

/// <summary>
/// Allows the configuration of the built TestServer that can be acted on.
/// </summary>
public class WebServerConfig(TestServer testServer, Func<IServiceProvider> providerFactory)
{
    private readonly TestServer _testServer = testServer;
    private readonly Func<IServiceProvider> _providerFactory = providerFactory;

    /// <summary>
    /// Allows any configurations to be made the the TestServer that will be used
    /// to run the unit-test.
    /// </summary>
    /// <param name="config">Delegate passed the TestServer instance to be configured.</param>
    /// <returns>Self Reference.</returns>
    public WebServerConfig ConfigServer(Action<TestServer> config)
    {
        ArgumentNullException.ThrowIfNull(config);

        config(_testServer);
        return this;
    }

    /// <summary>
    /// Returns an object used to act on the create TestServer.
    /// </summary>
    public WebServerAct Act => new WebServerAct(_testServer, _providerFactory());
}