using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using WebTests.Rest.ApiMetadata.Server;
using Xunit;

namespace WebTests.Rest.ApiMetadata
{
    /// <summary>
    /// Validates the population of action level metadata from the underlying
    /// parameter descriptions discovered by the ASP.NET runtime.
    /// </summary>
    public class ActionTests
    {
        /// <summary>
        /// Validates the action level metadata properties are correctly set.
        /// </summary>
        [Fact]
        public Task ApiAction_Metadata_Populated()
        {
            return TestApiMetadata.Run(metadata =>
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
       
        [Fact]
        public Task AssertStatusCodeWithResponse()
        {
            return TestApiMetadata.Run(metadata =>
            {
                var actionMeta = metadata.GetActionMeta<MetadataController>("ActionWithResponseTypeAndStatus");
                actionMeta.ResponseMeta.AssertResponseMeta(StatusCodes.Status200OK, typeof(ResponseModel));
            });
        }

        [Fact]
        public Task AssertMultipleStatusCodes()
        {
            return TestApiMetadata.Run(metadata =>
            {
                var actionMeta = metadata.GetActionMeta<MetadataController>("ActionWithMultipleStatuses");
                actionMeta.ResponseMeta.AssertResponseMeta(StatusCodes.Status200OK);
                actionMeta.ResponseMeta.AssertResponseMeta(StatusCodes.Status404NotFound);
            });
        }

        [Fact]
        public Task AssertActionWithStatusCodesWithSameResponseType()
        {
            return TestApiMetadata.Run(metadata =>
            {
                var actionMeta = metadata.GetActionMeta<MetadataController>(
                    "ActionWithStatusCodesWithSameResponseType");

                actionMeta.ResponseMeta.Should().HaveCount(2);
                    
                actionMeta.ResponseMeta.AssertResponseMeta(StatusCodes.Status200OK, typeof(ResponseModel));
                actionMeta.ResponseMeta.AssertResponseMeta(StatusCodes.Status418ImATeapot, typeof(ResponseModel));
            });
        }
    }
}