using System;
using Microsoft.AspNetCore.Mvc;
using Service.Domain.Entities;
using Service.WebApi.Models;

namespace Service.WebApi.Controllers.Core
{
    [ApiController, Route("api/base/attributed-entity")]
    public class AttributedEntityController : ControllerBase
    {
        [HttpGet("read")]
        public IActionResult ReadSensorData()
        {
            var sensor = new Sensor(Guid.NewGuid(), "Living Room Temp", 73.04M);
            sensor.Attributes.SetValue("IsFanOn", true);

            return Ok(sensor);
        }

        [HttpPatch("set/attributes")]
        public IActionResult AddAttributes([FromBody]AttributeModel model)
        {
            var sensor = new Sensor(Guid.NewGuid(), "Dinning Room Temp", 73.04M);
            foreach (AttributeValue attribute in model.Attributes)
            {
                sensor.Attributes.SetValue(attribute.Name, attribute.Value);
            }

            return Ok(sensor);
        }

        [HttpGet("get/dynamically")]
        public IActionResult AccessDynamically()
        {
            var sensor = new Sensor(Guid.NewGuid(), "Living Room Temp", 73.04M);
            sensor.Attributes.SetValue("IsFanOn", true);
            sensor.Attributes.SetValue("IsAcOn", false);

            return Ok(
                new
                {
                    FanOn = sensor.Attributes.Values.IsFanOn,
                    AcOn = sensor.Attributes.Values.IsAcOn
                }
            );
        }
    }
}