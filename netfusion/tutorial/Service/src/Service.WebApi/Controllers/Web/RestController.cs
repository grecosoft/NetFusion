using System;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Rest.Resources.Hal;
using NetFusion.Web.Mvc.Metadata;
using Service.WebApi.Resources;

namespace Service.WebApi.Controllers.Web
{
    [Route("api/schools"),
    GroupMeta(nameof(RestController))]
    public class RestController : Controller
    {
        [HttpGet("{id}")]
        public IActionResult GetSchool(int id)
        {
            var school = new SchoolResource
            {
                Id = id,
                Name = "Not There Elementary School",
                YearEstablished = 1982,
                NumberTeachers = 80
            };
            
            school.Embed(new AddressResource
            {
                Id = Guid.NewGuid(),
                AddressLine1 = "Dead End Road",
                City = "Centralia",
                State = "PA",
                ZipCode = "17920"
            }, "address");

            return Ok(school);
        }
        
        [HttpGet("{id}/students")]
        public IActionResult GetStudents(int id)
        {
            var parentResource = new HalResource();
            parentResource.Embed(new []
            {
                new StudentResource
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Alex",
                    LastName = "Green",
                    Age = 6
                },
                new StudentResource
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Jojo",
                    LastName = "Banana",
                    Age = 4
                }
            }, "results");

            return Ok(parentResource);
        }

        [HttpGet("students/{studentId}/address"),
            ActionMeta(nameof(GetStudentAddress))]
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