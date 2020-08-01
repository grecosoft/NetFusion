using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using FluentAssertions;
using NetFusion.Rest.Client;
using Xunit;

namespace WebTests.Rest.ClientRequests
{
    [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
    public class ResponseTests
    {
        /// <summary>
        /// The the server returns an error response status code with a body,
        /// an exception is raised.  The inner exception will have the body of
        /// the response.
        /// </summary>
        [Fact]
        public void CanRaiseException_ForErrorResponse_WithContent()
        {
            var responseMsg = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Locked
            };

            var response = new ApiResponse(responseMsg);
            response.SetErrorContent("The resource is locked.");

            var exception = Assert.Throws<HttpRequestException>(() => response.ThrowIfNotSuccessStatusCode());
            exception.Message.Should().StartWith("Response status code does not indicate success");
            exception.InnerException.Should().NotBeNull();
            exception.InnerException.Message.Should().Be("The resource is locked.");
        }

        /// <summary>
        /// If the server returns an error response status code with no body,
        /// and exception is raised with a null inner exception.
        /// </summary>
        [Fact]
        public void CanRaiseException_ForErrorResponse_WithNoContent()
        {
            var responseMsg = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Locked
            };

            var response = new ApiResponse(responseMsg);

            var exception = Assert.Throws<HttpRequestException>(() => response.ThrowIfNotSuccessStatusCode());
            exception.Message.Should().StartWith("Response status code does not indicate success");
            exception.InnerException.Should().BeNull();
        }
        
        
    }
}