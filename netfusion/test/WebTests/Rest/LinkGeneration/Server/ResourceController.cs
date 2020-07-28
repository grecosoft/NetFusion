using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using NetFusion.Rest.Resources;
using NetFusion.Rest.Server.Hal;
using WebTests.Mocks;

namespace WebTests.Rest.LinkGeneration.Server
{
    [ApiController, Route("api/linked/resource")]
    public class ResourceController : ControllerBase
    {
        private readonly IMockedService _mockedService;
        private readonly IHalEmbeddedResourceContext _embeddedContext;

        public ResourceController(IMockedService mockedService, IHalEmbeddedResourceContext embeddedContext)
        {
            _mockedService = mockedService;
            _embeddedContext = embeddedContext;
        }

        [HttpGet]
        public HalResource GetResource()
        {
            var models = _mockedService.GetResources<StateModel>().ToArray();          
            if (!models.Any())
            {
                throw new InvalidOperationException("Unit test didn't provided mocked server resource.");
            }

            var model = models.First();
            var resource = model.AsResource();
            
            // Unit test might make multiple calls after updating the state of the resource
            // to test outcome on link generation - clear any prior added links.
            resource.Links = new Dictionary<string, Link>();
            return resource;           
        }

        [HttpGet("embedded")]
        public IActionResult GetEmbeddedQueryParam()
        {
            return Ok(string.Join("|",_embeddedContext.RequestedEmbeddedModels));
        }

        [HttpGet("embedded/child")]
        public HalResource GetResourceWithEmbedded()
        {
            var parentModel = _mockedService.GetResources<StateModel>().FirstOrDefault();      
            if (parentModel == null)
            {
                throw new InvalidOperationException("Unit test didn't provided mocked server resource.");
            }
            
            var childModel = _mockedService.GetResources<StateEmbeddedModel>().FirstOrDefault();
            if (childModel == null)
            {
                throw new InvalidOperationException("Unit test didn't provided mocked server resource.");
            }
            
            return HalResource.New(parentModel, instance => 
                instance.EmbedResource(childModel.AsResource(), "embedded-resource"));
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
        public HalResource Create([FromBody]StateModel resource)
        {
            return null;
        }

        [HttpPut("scenario-6/{id}/update")]
        public HalResource Update(int id, [FromBody]StateModel resource)
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
