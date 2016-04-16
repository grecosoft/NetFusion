using NetFusion.Settings;

namespace NetFusion.WebApi.Configs
{
    /// <summary>
    /// Settings provided by the host application specific to
    /// configuring JWT token security.
    /// </summary>
    public class JwtTokenSettings : AppSettings
    {
        public string JwtKey { get; set; }
        public string TokenIssuerName { get; set; }
        public string AppliesToAddress { get; set; }
        public int TokenTimeoutMinutes { get; set; }
    }
}
