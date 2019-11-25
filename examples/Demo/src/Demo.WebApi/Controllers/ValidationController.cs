using Demo.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Base.Validation;

namespace Demo.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class ValidationController : Controller
    {
        private readonly IValidationService _validationSrv;

        public ValidationController(IValidationService validationSrv)
        {
            _validationSrv = validationSrv;
        }

        [HttpPost]
        public ValidationResultSet ValidateCustomer([FromBody]Contact customer) {
            return _validationSrv.Validate(customer);
        }
    }
}
