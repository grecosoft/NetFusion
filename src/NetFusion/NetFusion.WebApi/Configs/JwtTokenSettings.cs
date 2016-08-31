using System;
using NetFusion.Common.Validation;
using NetFusion.Settings;
using System.ComponentModel.DataAnnotations;

namespace NetFusion.WebApi.Configs
{
    /// <summary>
    /// Settings provided by the host application specific to
    /// configuring JWT token security.
    /// </summary>
    public class JwtTokenSettings : AppSettings,
        IObjectValidation
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "JWT Key is Required.")]
        public string JwtKey { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Token Issuer Name is Required.")]
        public string TokenIssuerName { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Applies to Address Required.")]
        public string AppliesToAddress { get; set; }

        public int TokenTimeoutMinutes { get; set; }
    }
}
