using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Rest.Resources;
using NetFusion.Rest.Server.Resources;
using WebTests.Rest.Setup;

namespace WebTests.Rest.LinkGeneration.Server
{

    [Route("api/convention/links")]
    public class ConventionBasedController : Controller
    {
        private readonly IMockedService _mockedService;

        public ConventionBasedController(IMockedService mockedService)
        {
            _mockedService = mockedService;
        }

        [HttpGet("resource")]
        public LinkedResource GetResource()
        {
            var resources = _mockedService.GetResources<LinkedResource>();
            if (!resources.Any())
            {
                throw new InvalidOperationException(
                    "Unit test didn't provided mocked server resource.");
            }

            var resource = resources.First();

            // Unit test might make multiple calls after updating the state of the resource
            // to test outcome on link generation - clear any prior added links.
            resource.Links = new Dictionary<string, Link> { };
            return resource;
        }

        [HttpGet("resource2")]
        public LinkedResource2 GetResource2()
        {
            var resources = _mockedService.GetResources<LinkedResource2>();
            if (!resources.Any())
            {
                throw new InvalidOperationException(
                    "Unit test didn't provided mocked server resource.");
            }

            var resource = resources.First();

            // Unit test might make multiple calls after updating the state of the resource
            // to test outcome on link generation - clear any prior added links.
            resource.Links = new Dictionary<string, Link> { };
            return resource;
        }

        [HttpGet("self/{id}")]
        public Task<LinkedResource> GetResourceById(int id)
        {
            return null;
        }

        [HttpPost("create")]
        public string CreateResource([FromBody]LinkedResource resource)
        {
            return null;
        }

        [HttpPut("update/{id}")]
        public string UpdatedResource(int id, [FromBody]LinkedResource resource)
        {
            return null;
        }

        [HttpDelete("delete/{id}"), ResourceType(typeof(LinkedResource))]
        public bool DeleteResource(int id)
        {
            return true;
        }

        [ResourceType(typeof(LinkedResource2))]
        [HttpGet("self/return/attribute/{id}")]
        public IActionResult GetResource2Self(int id)
        {
            var resources = _mockedService.GetResources<LinkedResource>();
            if (!resources.Any())
            {
                throw new InvalidOperationException(
                    "Unit test didn't provided mocked server resource.");
            }

            var resource = resources.First();

            // Unit test might make multiple calls after updating the state of the resource
            // to test outcome on link generation - clear any prior added links.
            resource.Links = new Dictionary<string, Link> { };

            return new ObjectResult(resource);
        }

    }
}
