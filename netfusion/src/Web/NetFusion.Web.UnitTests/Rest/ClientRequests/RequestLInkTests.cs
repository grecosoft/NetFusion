﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using FluentAssertions;
using NetFusion.Web.Rest.Client;
using NetFusion.Web.Rest.Resources;
using Xunit;

namespace NetFusion.Web.UnitTests.Rest.ClientRequests;

/// <summary>
/// Unit-tests validating the creation and population of links on the client.
/// Mostly, in HAL links are returned with the resources but the client will
/// usually make a request to an entry resource to obtains entry URLs for
/// loading initial resources.  Then URLs are usually template based.
/// </summary>
[SuppressMessage("ReSharper", "PossibleNullReferenceException")]
public class RequestLinkTests
{
    /// <summary>
    /// The client will have to make one or more initial requests to entry point resources.
    /// This provides a starting point for client resource navigation.
    /// </summary>
    [Fact]
    public void ClientCanCreateLink_ToGetEntryResource()
    {
        var request = ApiRequest.Get("api/test/url");

        request.RequestUri.Should().Be("api/test/url");
        request.Method.Should().Be(HttpMethod.Get);
        request.IsTemplate.Should().BeFalse();
    }

    /// <summary>
    /// Resources can return links with HREF templates for given scenarios.  The client can populate
    /// link template tokens with specified values.  The result is a new non-template link.
    /// </summary>
    [Fact]
    public void ClientCanPopulate_LinkTemplate()
    {
        var routeValues = new Dictionary<string, object>
        {
            { "state", "pa" },
            { "city", "new-kensington" }
        };

        var request = ApiRequest.Get("api/test/url/{state}/{city}", config => config.WithRouteValues(routeValues));
        request.RequestUri.Should().Be("api/test/url/pa/new-kensington");
    }

    /// <summary>
    /// Some of the link template tokens can be optional and must not be specified.  After the link
    /// is populated, all optional non-populated tokens are removed from the query string.
    /// </summary>
    [Fact]
    public void ClientMustNotPopulate_OptionalLinkTemplateTokens()
    {
        var routeValues = new Dictionary<string, object>
        {
            { "state", "me" },
            { "city", "kennebunkport" }
        };

        var request = ApiRequest.Get("api/test/url/{state}/{?city}/{?region}", config => config.WithRouteValues(routeValues));
        request.RequestUri.Should().Be("api/test/url/me/kennebunkport");
    }

    /// <summary>
    /// The client when requesting a resource with associated links submit the link
    /// by creating a corresponding ApiRequest object.
    /// </summary>
    [Fact]
    public void ClientCanCreate_RequestFromLink()
    {
        // Arrange:
        // Link as returned from server and associated with resource.
        var link = new Link("GET", "/api/test/url");

        // Act:
        var request = link.ToRequest();

        // Assert:
        request.Should().NotBeNull();
        request.Method.Method.Should().Be("GET");
        request.RequestUri.Should().Be(link.Href);
    }

    /// <summary>
    /// An ApiRequest can't be created from a link containing a template unless
    /// the non-optional route parameters are not specified.
    /// </summary>
    [Fact]
    public void CannotCreateRequest_FromTemplateLink_WithoutValues()
    {
        // Arrange:
        // Link as returned from server and associated with resource.
        var link = new Link("GET", "/api/test/url/{id}")
        {
            Templated = true
        };

        // Act:
        var exception = Record.Exception(() => link.ToRequest());
        exception.Should().BeOfType<InvalidOperationException>();
        exception.Message.Should().Be("Links containing non-populated template URL tokens can't be requested.");
    }

    /// <summary>
    /// When creating a request from a link containing a template based URL, 
    /// a dictionary can be passed with the template route values.
    /// </summary>
    [Fact]
    public void CanSpecifyRouteParameters_WhenCreatedRequest_AsDictionary()
    {
        // Arrange:
        // Link as returned from server and associated with resource.
        var link = new Link("GET", "/api/test/url/{id}")
        {
            Templated = true
        };

        // Act:
        var request = link.ToRequest(new Dictionary<string, object> { { "id", 110 } });
        request.AssertRequest("/api/test/url/110", HttpMethod.Get);
    }

    /// <summary>
    /// When creating a request from a link containing a template based URL, 
    /// a dynamic type can be passed with the template route values.
    /// </summary>
    [Fact]
    public void CanSpecifyRouteParameters_WhenCreatedRequest_AsDynamic()
    {
        // Arrange:
        // Link as returned from server and associated with resource.
        var link = new Link("GET", "/api/test/url/{id}")
        {
            Templated = true
        };

        // Act:
        var request = link.ToRequest(values => values.Id = 110);
        request.AssertRequest("/api/test/url/110", HttpMethod.Get);
    }

    [Fact]
    public void RequestUri_IsRequired()
    {
        var exception = Record.Exception(() => ApiRequest.Create("", HttpMethod.Get));
        exception.Should().BeOfType<InvalidOperationException>();
        exception.Message.Should().Be("Request URI value not specified.");

        exception = Record.Exception(() => ApiRequest.Create(null, HttpMethod.Get));
        exception.Should().BeOfType<InvalidOperationException>();
        exception.Message.Should().Be("Request URI value not specified.");

        exception = Record.Exception(() => ApiRequest.Create("    ", HttpMethod.Get));
        exception.Should().BeOfType<InvalidOperationException>();
        exception.Message.Should().Be("Request URI value not specified.");
    }

    [Fact]
    public void RequestMethod_IsRequired()
    {
        var exception = Record.Exception(() => ApiRequest.Create("api/test", null));
        exception.Should().BeOfType<InvalidOperationException>();
        exception.Message.Should().Be("Request HTTP Method not specified.");
    }

    [Fact]
    public void CanCreateRequest_WithTemplateUrl()
    {
        var request = ApiRequest.Create("/api/test/house/{id}", HttpMethod.Get, 
            config => config.WithRouteValues(values => values.Id = 535));

        request.AssertRequest("/api/test/house/535", HttpMethod.Get);
    }

    [Fact]
    public void OptionalRouteParameters_MustNotBeSpecified()
    {
        // Arrange:
        // Link as returned from server and associated with resource.
        var link = new Link("GET", "/api/test/url/{id}/{?test}")
        {
            Templated = true
        };

        // Act:
        var request = link.ToRequest(values => values.Id = 300);
        request.AssertRequest("/api/test/url/300", HttpMethod.Get);
    }

    [Fact]
    public void CanCheckIf_Resource_HasLink()
    {
        var response = new HalResource
        {
            Links = new Dictionary<string, Link>
            {
                {"current-status", new Link("GET", "/api/test")}
            }
        };

        response.HasLink("current-status").Should().BeTrue();
    }

    [Fact]
    public void CanGetResourceLink()
    {
        var link = new Link("GET", "/api/test");
        var response = new HalResource
        {
            Links = new Dictionary<string, Link>
            {
                {"current-status", link}
            }
        };

        response.GetLink("current-status").Should().NotBeNull();
    }
        
    [Fact]
    public void CanTypeToGetResourceLink()
    {
        var link = new Link("GET", "/api/test");
        var response = new HalResource
        {
            Links = new Dictionary<string, Link>
            {
                {"current-status", link}
            }
        };

        response.TryGetLink("current-status", out Link foundLink).Should().BeTrue();
        foundLink.Should().NotBeNull();
        foundLink.Should().BeSameAs(link);
    }
}