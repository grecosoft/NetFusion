using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Rest.Resources;
using NetFusion.Rest.Resources.Hal;
using WebTests.Mocks;

namespace WebTests.Rest.LinkGeneration.Server
{
    [ApiController, Route("api/linked/resource")]
    public class LinkedResourceController : ControllerBase
    {
        private readonly IMockedService _mockedService;

        public LinkedResourceController(IMockedService mockedService)
        {
            _mockedService = mockedService;
        }

        [HttpGet]
        public HalResource GetResource()
        {
            var resources = _mockedService.GetResources<LinkedResource>().ToArray();          
            if (!resources.Any())
            {
                throw new InvalidOperationException(
                    "Unit test didn't provided mocked server resource.");
            }

            var model = resources.First();
            var resource = model.AsResource();
            // Unit test might make multiple calls after updating the state of the resource
            // to test outcome on link generation - clear any prior added links.
            resource.Links = new Dictionary<string, Link>();
            return resource;           
        }

        [HttpGet("scenario-1/{id}")]
        public HalResource GetById(int id)
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
        public HalResource Create([FromBody]LinkedResource resource)
        {
            return null;
        }

        [HttpPut("scenario-6/{id}/update")]
        public HalResource Update(int id, [FromBody]LinkedResource resource)
        {
            return null;
        }
        
        [HttpPost("scenario-33/{id}/comment")]
        public IActionResult AppendComment(int id, [FromQuery]string comment)
        {
            return null;
        }
    }
}
