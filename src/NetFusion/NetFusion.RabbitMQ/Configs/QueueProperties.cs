using NetFusion.Common.Validation;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NetFusion.RabbitMQ.Configs
{
    /// <summary>
    /// Used to specify queue properties that are stored external to
    /// the queue definition defined in code.
    /// </summary>
    public class QueueProperties : IObjectValidation
    {
        public QueueProperties()
        {
            this.RouteKeys = new string[] { };
        }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Queue Name Required")]
        public string QueueName { get; set; }

        public IEnumerable<string> RouteKeys { get; set; }

        public ObjectValidator ValidateObject()
        {
            return new ObjectValidator(this);
        }
    }
}
