using System.Net.Http;
using FluentAssertions;
using NetFusion.Web.Rest.Client;
using NetFusion.Web.Rest.Client.Settings;
using NetFusion.Web.UnitTests.Rest.ClientRequests.Client;
using Xunit;

namespace NetFusion.Web.UnitTests.Rest.ClientRequests;

public class RequestTests
{
    [Fact]
    public void RequestHasSettings()
    {
        var request = ApiRequest.Post("api/post");
        var settings = RequestSettings.Create(config => config.QueryString.AddParam("a", "123"));
            
        request.UsingSettings(settings);
        request.Settings.Should().NotBeNull();
        request.Settings.QueryString.Params.ContainsKey("a").Should().BeTrue();
    }
        
    [Fact]
    public void PostRequest_SetsHttpMethod()
    {
        var request = ApiRequest.Post("api/post");
        request.Method.Should().Be(HttpMethod.Post);
    }
        
    [Fact]
    public void PutRequest_SetsHttpMethod()
    {
        var request = ApiRequest.Put("api/put");
        request.Method.Should().Be(HttpMethod.Put);
    }
        
    [Fact]
    public void DeleteRequest_SetsHttpMethod()
    {
        var request = ApiRequest.Delete("api/delete");
        request.Method.Should().Be(HttpMethod.Delete);
    }
        
    [Fact]
    public void PatchRequest_SetsHttpMethod()
    {
        var request = ApiRequest.Patch("api/patch");
        request.Method.Should().Be(HttpMethod.Patch);
    }

    [Fact]
    public void CanSpecify_Embedded_Resources()
    {
        var request = ApiRequest.Get("api/test", config => config.Embed("r1", "r2"));
        request.EmbeddedNames.Should().Be("r1,r2");
    }

    [Fact]
    public void RequestCan_Have_Content()
    {
        var customer = new CustomerModel();

        var request = ApiRequest.Post("api/test", config => config.WithContent(customer));
        request.Content.Should().BeSameAs(customer);
    }
        
        
}