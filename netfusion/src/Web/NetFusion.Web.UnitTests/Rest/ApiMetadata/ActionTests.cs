using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NetFusion.Web.UnitTests.Rest.ApiMetadata.Server;
using NetFusion.Web.UnitTests.Rest.ApiMetadata.Setup;
using Xunit;

namespace NetFusion.Web.UnitTests.Rest.ApiMetadata;

/// <summary>
/// ASP.NET Core builds an underlying discovery model that is used to process incoming Web requests and
/// for generating URLs based on a set of controller, action, and route parameters.  The ApiMetadata
/// classes contain a subset of the needed metadata used by the other NetFusion web plugins.
/// </summary>
public class ActionTests
{
    /// <summary>
    /// The root ApiActionMeta contains the action specific information populated
    /// from ASP.NET Core's underlying discovery model.
    /// </summary>
    [Fact]
    public Task ApiAction_Metadata_Populated()
    {
        return TestWebHostSetup.Run(metadata =>
        {
            var actionMeta = metadata.GetActionMeta<MetadataController>("ActionDetails", typeof(int));
            actionMeta.ControllerName.Should().Be("Metadata");
            actionMeta.ActionName.Should().Be("ActionDetails");
            actionMeta.RelativePath.Should().Be("api/documented/actions/metadata/{id}");
            actionMeta.HttpMethod.Should().Be("GET");
            actionMeta.ActionMethodInfo.Should().NotBeNull();

            var actionMethodInfo = typeof(MetadataController).GetMethod("ActionDetails", new[] {typeof(int)});

            actionMeta.ActionMethodInfo.Should().BeSameAs(actionMethodInfo);
        });
    }
       
    /// <summary>
    /// A WebApi method can have the response status and corresponding model specified.
    /// </summary>
    [Fact]
    public Task AssertStatusCodeWithResponse()
    {
        return TestWebHostSetup.Run(metadata =>
        {
            var actionMeta = metadata.GetActionMeta<MetadataController>("ActionWithResponseTypeAndStatus");
            actionMeta.ResponseMeta.AssertResponseMeta(StatusCodes.Status200OK, typeof(ResponseModel));
        });
    }

    /// <summary>
    /// A WebApi method can return more than one status code.
    /// </summary>
    [Fact]
    public Task AssertMultipleStatusCodes()
    {
        return TestWebHostSetup.Run(metadata =>
        {
            var actionMeta = metadata.GetActionMeta<MetadataController>("ActionWithMultipleStatuses");
            actionMeta.ResponseMeta.AssertResponseMeta(StatusCodes.Status200OK);
            actionMeta.ResponseMeta.AssertResponseMeta(StatusCodes.Status404NotFound);
        });
    }

    /// <summary>
    /// Different response status codes can return the same response model.
    /// </summary>
    [Fact]
    public Task AssertActionWithStatusCodesWithSameResponseType()
    {
        return TestWebHostSetup.Run(metadata =>
        {
            var actionMeta = metadata.GetActionMeta<MetadataController>(
                "ActionWithStatusCodesWithSameResponseType");

            actionMeta.ResponseMeta.Should().HaveCount(2);
                    
            actionMeta.ResponseMeta.AssertResponseMeta(StatusCodes.Status200OK, typeof(ResponseModel));
            actionMeta.ResponseMeta.AssertResponseMeta(StatusCodes.Status418ImATeapot, typeof(ResponseModel));
        });
    }
}