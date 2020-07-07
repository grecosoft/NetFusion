using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NetFusion.Rest.Client;
using NetFusion.Rest.Client.Settings;
using NetFusion.Web.Mvc.Metadata;
using WebTests.Hosting;
using WebTests.Rest.ApiMetadata.Server;
using WebTests.Rest.Setup;
using Xunit;

namespace WebTests.Rest.ApiMetadata
{
    /// <summary>
    /// Validates the population of action level metadata from the underlying
    /// parameter descriptions discovered by the ASP.NET runtime.
    /// </summary>
    public class ApiMetadataTests
    {
        /// <summary>
        /// Validates the action level metadata properties are correctly set.
        /// </summary>
        [Fact]
        public Task ApiAction_Metadata_Populated()
        {
            var mockResource = new MetaResource { Id = 10 };
            
            return WebHostFixture.TestAsync<MetadataController>(async host =>
            {
                var webResponse = await host
                    .ArrangeWithDefaults(mockResource)
                    .Act.OnRestClient(async client =>
                    {             
                        var request = ApiRequest.Create("api/documented/actions", HttpMethod.Get);
                        request.UseHalDefaults();
                        return await client.SendAsync(request);
                    });

                webResponse.Assert.Service((IApiMetadataService service) =>
                {
                    var actionMeta = service.GetActionMeta<MetadataController>("ActionDetails", typeof(int));
                    actionMeta.ControllerName.Should().Be("Metadata");
                    actionMeta.ActionName.Should().Be("ActionDetails");
                    actionMeta.RelativePath.Should().Be("api/documented/actions/metadata/{id}");
                    actionMeta.HttpMethod.Should().Be("GET");
                    actionMeta.ActionMethodInfo.Should().NotBeNull();

                    var actionMethodInfo = typeof(MetadataController).GetMethod("ActionDetails", new[] {typeof(int)});

                    actionMeta.ActionMethodInfo.Should().BeSameAs(actionMethodInfo);
                });
            });
        }
        
        /// <summary>
        /// Validates the metadata describing the possible responses of an action are
        /// correctly set.
        /// </summary>
        [Fact]
        public Task ApiResponse_Metadata_Populated()
        {
            var mockResource = new MetaResource { Id = 10 };
            
            return WebHostFixture.TestAsync<MetadataController>(async host =>
            {
                var webResponse = await host
                    .ArrangeWithDefaults(mockResource)
                    .Act.OnRestClient(async client =>
                    {             
                        var request = ApiRequest.Create("api/documented/actions", HttpMethod.Get);
                        request.UseHalDefaults();
                        return await client.SendAsync(request);
                    });

                webResponse.Assert.Service((IApiMetadataService service) =>
                {
                    AssertStatusCodeWithResponse(service);
                    AssertMultipleStatusCodes(service);
                    AssertActionWithStatusCodesWithSameResponseType(service);
                });

                static void AssertStatusCodeWithResponse(IApiMetadataService service)
                {
                    var actionMeta = service.GetActionMeta<MetadataController>("ActionWithResponseTypeAndStatus");
                    actionMeta.ResponseMeta.AssertResponseMeta(StatusCodes.Status200OK, typeof(ResponseModel));
                }

                static void AssertMultipleStatusCodes(IApiMetadataService service)
                {
                    var actionMeta = service.GetActionMeta<MetadataController>("ActionWithMultipleStatuses");
                    actionMeta.ResponseMeta.AssertResponseMeta(StatusCodes.Status200OK);
                    actionMeta.ResponseMeta.AssertResponseMeta(StatusCodes.Status404NotFound);
                }

                static void AssertActionWithStatusCodesWithSameResponseType(IApiMetadataService service)
                {
                    var actionMeta = service.GetActionMeta<MetadataController>(
                        "ActionWithStatusCodesWithSameResponseType");

                    actionMeta.ResponseMeta.Should().HaveCount(2);
                    
                    actionMeta.ResponseMeta.AssertResponseMeta(StatusCodes.Status200OK, typeof(ResponseModel));
                    actionMeta.ResponseMeta.AssertResponseMeta(StatusCodes.Status418ImATeapot, typeof(ResponseModel));
                }
            });
        }
    }
}