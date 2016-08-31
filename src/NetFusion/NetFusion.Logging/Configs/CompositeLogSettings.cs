using NetFusion.Common.Validation;
using NetFusion.Settings;
using System.ComponentModel.DataAnnotations;

namespace NetFusion.Logging.Configs
{
    /// <summary>
    /// Settings specifying where a host application should submit its
    /// composite log when bootstrapped.
    /// </summary>
    public class CompositeLogSettings : AppSettings,
        IObjectValidation
    {
        /// <summary>
        /// The WebApi endpoint.
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "Endpoint is Required")]
        public string Endpoint { get; set; }

        /// <summary>
        /// The WebApi route to send log information.
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "LogRoute is Required")]
        public string LogRoute { get; set; }

        /// <summary>
        /// Indicates if the log should be sent.
        /// </summary>
        public bool SendLog { get; set; } = false;
    }
}
