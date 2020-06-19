using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Rest.Resources.Hal;
using Service.WebApi.Resources;

namespace Service.WebApi.Controllers.Web
{
    /// <summary>
    /// Controller comment.
    /// </summary>
    [ApiController, Route("api/schools")]
    public class RestController : ControllerBase
    {
        /// <summary>
        /// Queries a school resource.
        /// </summary>
        /// <param name="id">Value identifying the school.</param>
        /// <param name="criteria">Query value used to specify a specific grade.</param>
        /// <returns>School resource.</returns>
        [HttpGet("{id}"), ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetSchool(int id, [FromQuery]string criteria = "all-grades")
        {
            var school = new SchoolResource
            {
                Id = id,
                Name = "Not There Elementary School",
                YearEstablished = 1982,
                NumberTeachers = 80
            }.AsResource();
            
            school.EmbedResource(new AddressResource
            {
                Id = Guid.NewGuid(),
                AddressLine1 = "Dead End Road",
                City = "Centralia",
                State = "PA",
                ZipCode = "17920"
            }.AsResource(), "address");

            return Ok(school);
        }

        [HttpGet("{id}/students")]
        public IActionResult GetStudents(int id)
        {
            var parentResource = new HalResource().AsResource();
            parentResource.EmbedResources(new []
            {
                new StudentResource
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Alex",
                    LastName = "Green",
                    Age = 6
                }.AsResource(),
                new StudentResource
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Jojo",
                    LastName = "Banana",
                    Age = 4
                }.AsResource()
            }, "results");

            return Ok(parentResource);
        }

        [HttpGet("students/{studentId}/address")]
        public IActionResult GetStudentAddress(Guid studentId)
        {
            return Ok(new AddressResource
            {
                Id  = Guid.NewGuid(),
                AddressLine1 = "West Main Street",
                City = "Buckhannon",
                State = "WV",
                ZipCode = "262401"
            });
        }
        

        [HttpPost("students/{id}")]
        public IActionResult UpdateStudent(Guid id, [FromBody]StudentResource resource)
        {
            return Ok();
        }
    }
}