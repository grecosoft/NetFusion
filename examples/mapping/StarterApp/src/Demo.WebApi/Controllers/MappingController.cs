using WebApiHost.Models;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Mapping;
using Demo.App.Services;
using System.Linq;

namespace WebApiHost.Controllers
{
    [Route("api/[controller]")]
    public class MappingController : Controller
    {
        private readonly SampleEntityService _entityService;
        private readonly IObjectMapper _objectMapper;

        public MappingController(
            SampleEntityService entityService,
            IObjectMapper objectMapper)
        {
            _entityService = entityService;
            _objectMapper = objectMapper;
        }

        [HttpGet("code-mapping")]
        public StudentListingSummary GetStudentListingSummary()
        {
           var courseEntity = _entityService.GetCourse();
           return _objectMapper.Map<StudentListingSummary>(courseEntity);
        }

        [HttpGet("derived-targets")]
        public ContactSummary[] GetContactSummaries()
        {
            var contacts = _entityService.GetContacts();

            return contacts.Select(
                c => _objectMapper.Map<ContactSummary>(c)
            ).ToArray();
        }

        [HttpGet("dependency-mapping")]
        public StudentCalcSummary GetStudentSummary()
        {
            var student = _entityService.GetStudent();
            return _objectMapper.Map<StudentCalcSummary>(student);
        }

        [HttpGet("opensource-mapping")]
        public CarSummary GetStudentCalcSummary() 
        {
            var carEntity = _entityService.GetCar();
            return _objectMapper.Map<CarSummary>(carEntity); 
        }
    }
}
