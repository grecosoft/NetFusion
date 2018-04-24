using InfrastructureTests.Web.Rest.Setup;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Rest.Resources;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InfrastructureTests.Web.Rest.LinkGeneration.Server
{
    [Route("api/linked/resource")]
    public class LinkedResourceController : Controller
    {
        private readonly IMockedService _mockedService;

        public LinkedResourceController(IMockedService mockedService)
        {
            _mockedService = mockedService;
        }

        [HttpGet]
        public LinkedResource GetResource()
        {
            var resources = _mockedService.GetResources<LinkedResource>();          
            if (resources.Count() == 0)
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

        [HttpGet("scenario-1/{id}")]
        public LinkedResource GetById(int id)
        {
            return null;
        }

        [HttpGet("scenario-2/{id}/param-one/{p1}")]
        public void GetByIdAndRequiredParam(int id, string p1)
        {

        }

        [HttpGet("scenario-3/{id}/param-one/{p1?}")]
        public void GetByIdWithOneOptionalParam(int id, int? p1 = null)
        {

        }

        [HttpGet("scenario-4/{id}/param-one/{p1?}/{p2?}")]
        public void GetByIdWithMultipleOptionalParams(int id, int? p1 = null, string p2 = null)
        {

        }

        [HttpPost("scenario-5/create")]
        public LinkedResource Create([FromBody]LinkedResource resource)
        {
            return null;
        }

        [HttpPut("scenario-6/{id}/update")]
        public LinkedResource Update(int id, [FromBody]LinkedResource resource)
        {
            return null;
        }

        [HttpGet("view")]
        public LinkedViewResource GetViewResource()
        {
            var resources = _mockedService.GetResources<LinkedResource>();
            if (resources.Count() == 0)
            {
                throw new InvalidOperationException(
                    "Unit test didn't provided mocked server resource.");
            }

            return new LinkedViewResource(resources.First());
        }
    }
}
