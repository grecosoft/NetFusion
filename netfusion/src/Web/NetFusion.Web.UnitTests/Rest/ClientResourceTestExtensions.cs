﻿using System.Net.Http;
using NetFusion.Web.Rest.Client;
using NetFusion.Web.Rest.Resources;
using Xunit;

namespace NetFusion.Web.UnitTests.Rest;

public static class ClientResourceTestExtensions
{
    public static void AssertLink(this HalResource resource,
        string relName,
        HttpMethod expectedMethod,
        string expectedValue)
    {
        Assert.True(
            resource.Links is { Count: > 0 },
            "Resource does not have associated links.");

        Assert.True(resource.Links.ContainsKey(relName), $"Resource does not have link with relation name: {relName}");

        var link = resource.Links[relName];
        Assert.Equal(expectedValue, link.Href);
        Assert.NotNull(link.Method);
        Assert.True(link.Method != null, "HTTP method expected.");
        Assert.Equal(expectedMethod.Method, link.Method);
    }

    public static void AssertRequest(this ApiRequest request,
        string expectedRequestUri,
        HttpMethod expectedMethod)
    {
        Assert.Equal(expectedRequestUri, request.RequestUri);
        Assert.Equal(expectedMethod, request.Method);
    }
}