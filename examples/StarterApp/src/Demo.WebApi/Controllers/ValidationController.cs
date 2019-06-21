using Demo.App.Entities;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Base.Validation;
using NetFusion.Bootstrap.Validation;

namespace Demo.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class ValidationController : Controller
    {
        private IValidationService _validationSrv;

        public ValidationController(IValidationService validationSrv)
        {
            _validationSrv = validationSrv;
        }

        [HttpPost]
        public ValidationResultSet ValidateCustomer([FromBody]Customer customer) {
            return _validationSrv.Validate(customer);
        }
    }
}
